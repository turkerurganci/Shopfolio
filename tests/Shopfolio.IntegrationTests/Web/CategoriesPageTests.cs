using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the category tree screen's tests (T526): one database copied from the template; every test empties the tree.</summary>
[CollectionDefinition(CategoriesPageTestsDefinition.Name)]
public sealed class CategoriesPageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "CategoriesPages";
}

/// <summary>
/// E-34, the category tree, and its four handlers on the opened application and a real database (T526 criteria 1-7, KK1-KK7; 07 UC-89, UC-118...UC-121; K-799, K-805,
/// K-1089, K-1191): the siblings in Turkish alphabetical order, no "Alt kategori ekle" on the third level, the server's counts and menu line, the empty row; the address made
/// from the name and numbered on a collision; each refusal in place with its 422, and 404 for a missing node.
/// </summary>
[Collection(CategoriesPageTestsDefinition.Name)]
public sealed class CategoriesPageTests : IAsyncLifetime
{
    private const string Path = "/panel/kategoriler";

    private readonly CollectionDatabaseFixture _database;

    private string _key = string.Empty;

    public CategoriesPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE product_categories, categories, variants, option_values, options, products, admins, sessions CASCADE");
        (_key, _) = await TestSessions.AdminAsync(_database.Dsn);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1, KK2: the order, the level limit, the counts and the menu line ----

    [Fact]
    public async Task TheTree_IsAlphabeticalOnEachLevel_TheThirdLevelHasNoAddChild_AndTheCountsAndMenuLineAreTheServers_T526_KK1_KK2()
    {
        Guid zeytin = await CreateAsync("Zeytin", null);
        Guid ayakkabi = await CreateAsync("Ayakkabı", null);
        Guid canta = await CreateAsync("Çanta", null);
        Guid spor = await CreateAsync("Spor", ayakkabi);
        await CreateAsync("Bot", ayakkabi);
        Guid kosu = await CreateAsync("Koşu", spor);
        await LinkProductAsync(kosu, PublishStatus.Published);
        await LinkProductAsync(canta, PublishStatus.Draft);
        using HttpClient client = ClientOf(Open());

        (HttpStatusCode status, string html) = await GetAsync(client, Path);

        Assert.Equal(HttpStatusCode.OK, status);
        AssertInOrder(html, "Ayakkabı", "Bot", "Spor", "Koşu", "Çanta", "Zeytin");
        Assert.Equal(5, Count(html, CategoriesModel.AddChildLabel));
        Assert.Equal(3, Count(html, CategoriesModel.MenuHiddenText));
        Assert.Contains("0 ürün · 2 alt kategori", html, StringComparison.Ordinal);
        Assert.Equal(2, Count(html, "1 ürün · 0 alt kategori"));
        Assert.DoesNotContain(CategoriesModel.EmptySentence, html, StringComparison.Ordinal);
        Assert.NotEqual(Guid.Empty, zeytin);
    }

    [Fact]
    public async Task AnEmptyTree_IsTheEmptyRow_WithTheNewCategoryForm_T526_KK1()
    {
        using HttpClient client = ClientOf(Open());

        (HttpStatusCode status, string html) = await GetAsync(client, Path);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains(CategoriesModel.EmptySentence, html, StringComparison.Ordinal);
        Assert.Contains("id=\"" + CategoriesModel.NewCategoryFormId + "\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain(CategoriesModel.AddChildLabel, html, StringComparison.Ordinal);
    }

    // ---- KK3, KK4: create, the address and its collision; two refusals and 404 ----

    [Fact]
    public async Task ACategory_GetsItsAddressFromItsName_ASecondSendOpensASecondOne_WithANumberedAddress_T526_KK3_KK4()
    {
        using HttpClient client = ClientOf(Open());

        using HttpResponseMessage first = await PostAsync(client, "CreateCategory", ("Input.Name", "Elbise"));
        using HttpResponseMessage second = await PostAsync(client, "CreateCategory", ("Input.Name", "Elbise"));

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Contains("notice=Saved", first.Headers.Location?.OriginalString, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, second.StatusCode);
        List<string> slugs = await SlugsAsync("Elbise");
        Assert.Equal(2, slugs.Count);
        Assert.Contains("elbise", slugs);
        Assert.Matches("^elbise-[0-9]+$", slugs.Single(s => s != "elbise"));
    }

    [Fact]
    public async Task ABlankName_Is422InPlace_AndWritesNothing_T526_KK4()
    {
        using HttpClient client = ClientOf(Open());

        using HttpResponseMessage response = await PostAsync(client, "CreateCategory", ("Input.Name", "  "));
        string html = await ReadAsync(response);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains("Ad boş olamaz.", html, StringComparison.Ordinal);
        Assert.Equal(0L, await CategoryCountAsync());
    }

    [Fact]
    public async Task AChildUnderTheThirdLevel_Is422_AndAMissingParentIs404_T526_KK4()
    {
        Guid first = await CreateAsync("Birinci", null);
        Guid second = await CreateAsync("İkinci", first);
        Guid third = await CreateAsync("Üçüncü", second);
        using HttpClient client = ClientOf(Open());

        using HttpResponseMessage tooDeep = await PostAsync(client, "CreateCategory", ("Input.Name", "Dördüncü"), ("Input.ParentCategoryId", third.ToString("D")));
        string html = await ReadAsync(tooDeep);
        using HttpResponseMessage noParent = await PostAsync(client, "CreateCategory", ("Input.Name", "Yetim"), ("Input.ParentCategoryId", Guid.NewGuid().ToString("D")));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, tooDeep.StatusCode);
        Assert.Contains(CategoriesModel.ThirdLevelText, html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.NotFound, noParent.StatusCode);
        Assert.Equal(3L, await CategoryCountAsync());
    }

    // ---- KK5: rename ----

    [Fact]
    public async Task ARename_ChangesTheName_TheAddressStays_ABlankNameIs422_AndAMissingNodeIs404_T526_KK5()
    {
        Guid id = await CreateAsync("Eski Ad", null);
        using HttpClient client = ClientOf(Open());

        using HttpResponseMessage renamed = await PostAsync(client, "RenameCategory", ("Input.CategoryId", id.ToString("D")), ("Input.Name", "Yeni Ad"));
        using HttpResponseMessage blank = await PostAsync(client, "RenameCategory", ("Input.CategoryId", id.ToString("D")), ("Input.Name", string.Empty));
        string html = await ReadAsync(blank);
        using HttpResponseMessage missing = await PostAsync(client, "RenameCategory", ("Input.CategoryId", Guid.NewGuid().ToString("D")), ("Input.Name", "Ad"));

        Assert.Equal(HttpStatusCode.SeeOther, renamed.StatusCode);
        Assert.Equal("Yeni Ad|eski-ad", await ScalarAsync<string>($"SELECT name || '|' || slug FROM categories WHERE id = '{id}'"));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, blank.StatusCode);
        Assert.Contains("Ad boş olamaz.", html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    // ---- KK6: move ----

    [Fact]
    public async Task AMovePastThreeLevels_AndAMoveIntoItsOwnSubtree_Are422_AMissingNodeIs404_AndAValidMoveIs303_T526_KK6()
    {
        Guid a = await CreateAsync("Ana", null);
        Guid b = await CreateAsync("Bir", a);
        Guid c = await CreateAsync("Can", b);
        Guid d = await CreateAsync("Dal", null);
        await CreateAsync("Eki", d);
        using HttpClient client = ClientOf(Open());

        using HttpResponseMessage tooDeep = await PostAsync(client, "MoveCategory", ("Input.CategoryId", d.ToString("D")), ("Input.ParentCategoryId", b.ToString("D")));
        string deepHtml = await ReadAsync(tooDeep);
        using HttpResponseMessage ownSubtree = await PostAsync(client, "MoveCategory", ("Input.CategoryId", a.ToString("D")), ("Input.ParentCategoryId", c.ToString("D")));
        string ownHtml = await ReadAsync(ownSubtree);
        using HttpResponseMessage missing = await PostAsync(client, "MoveCategory", ("Input.CategoryId", Guid.NewGuid().ToString("D")), ("Input.ParentCategoryId", a.ToString("D")));
        using HttpResponseMessage valid = await PostAsync(client, "MoveCategory", ("Input.CategoryId", d.ToString("D")), ("Input.ParentCategoryId", a.ToString("D")));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, tooDeep.StatusCode);
        Assert.Contains(CategoriesModel.DepthExceededText, deepHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, ownSubtree.StatusCode);
        Assert.Contains(CategoriesModel.IntoOwnSubtreeText, ownHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal(HttpStatusCode.SeeOther, valid.StatusCode);
        Assert.Equal(a, await ScalarAsync<Guid>($"SELECT parent_category_id FROM categories WHERE id = '{d}'"));
        Assert.Null(await ScalarAsync<Guid?>($"SELECT parent_category_id FROM categories WHERE id = '{a}'"));
    }

    // ---- KK7: delete ----

    [Fact]
    public async Task AFullCategory_IsNotDeleted_And422NamesTheBlockingCounts_AnEmptyOneIsDeleted_AMissingOneIs404_T526_KK7()
    {
        Guid parent = await CreateAsync("Dolu Üst", null);
        await CreateAsync("Alt", parent);
        Guid withProduct = await CreateAsync("Ürünlü", null);
        await LinkProductAsync(withProduct, PublishStatus.Draft);
        Guid empty = await CreateAsync("Boş", null);
        using HttpClient client = ClientOf(Open());

        using HttpResponseMessage hasChild = await PostAsync(client, "DeleteCategory", ("Input.CategoryId", parent.ToString("D")));
        string childHtml = await ReadAsync(hasChild);
        using HttpResponseMessage hasProduct = await PostAsync(client, "DeleteCategory", ("Input.CategoryId", withProduct.ToString("D")));
        string productHtml = await ReadAsync(hasProduct);
        using HttpResponseMessage deleted = await PostAsync(client, "DeleteCategory", ("Input.CategoryId", empty.ToString("D")));
        using HttpResponseMessage missing = await PostAsync(client, "DeleteCategory", ("Input.CategoryId", empty.ToString("D")));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, hasChild.StatusCode);
        Assert.Contains(CategoriesModel.NotEmptyText(0, 1), childHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, hasProduct.StatusCode);
        Assert.Contains(CategoriesModel.NotEmptyText(1, 0), productHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, deleted.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal(3L, await CategoryCountAsync());
    }

    // ---- helpers ----

    private static void AssertInOrder(string html, params string[] names)
    {
        int[] positions = [.. names.Select(name => html.IndexOf("category-tree__name\">" + name + "<", StringComparison.Ordinal))];
        Assert.All(positions, p => Assert.True(p > 0));
        Assert.Equal(positions.Order(), positions);
    }

    private static int Count(string html, string needle) => (html.Length - html.Replace(needle, string.Empty, StringComparison.Ordinal).Length) / needle.Length;

    private static async Task<string> ReadAsync(HttpResponseMessage response) => WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

    /// <summary>Opens a category through the screen's own handler and reads its key back by its name.</summary>
    private async Task<Guid> CreateAsync(string name, Guid? parent)
    {
        using HttpClient client = ClientOf(Open());
        using HttpResponseMessage response = parent is { } p
            ? await PostAsync(client, "CreateCategory", ("Input.Name", name), ("Input.ParentCategoryId", p.ToString("D")))
            : await PostAsync(client, "CreateCategory", ("Input.Name", name));
        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        return await ScalarAsync<Guid>($"SELECT id FROM categories WHERE name = '{name}' ORDER BY created_at DESC LIMIT 1");
    }

    private async Task LinkProductAsync(Guid categoryId, PublishStatus status)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Product product = Product.Create("Ürün", "urun-" + Guid.NewGuid().ToString("N")[..8], ProductType.Physical, 20m, now).WithSeededOrigin();
        product.PublishStatus = status;
        await using AppDbContext db = NewContext();
        db.Products.Add(product);
        db.ProductCategories.Add(ProductCategory.Create(product.Id, categoryId, true, now));
        await db.SaveChangesAsync(Ct);
    }

    private async Task<List<string>> SlugsAsync(string name)
    {
        await using AppDbContext db = NewContext();
        return await db.Categories.Where(c => c.Name == name).Select(c => c.Slug).ToListAsync(Ct);
    }

    private Task<long> CategoryCountAsync() => ScalarAsync<long>("SELECT count(*) FROM categories");

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(ForgeryTokenSampleModel).Assembly)));

    private HttpClient ClientOf(WebApplicationFactory<Program> factory)
    {
        HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        client.DefaultRequestHeaders.Add(HeaderNames.Cookie, $"{CookieNames.AdminSession}={_key}");
        return client;
    }

    private static async Task<(HttpStatusCode Status, string Html)> GetAsync(HttpClient client, string path)
    {
        using HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative), Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, string handler, params (string Name, string Value)[] fields) =>
        client.PostAsync($"{Path}?handler={handler}", new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value))), Ct);

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<T> ScalarAsync<T>(string sql)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        object? value = await command.ExecuteScalarAsync(Ct);
        return value is null or DBNull ? default! : (T)value;
    }
}
