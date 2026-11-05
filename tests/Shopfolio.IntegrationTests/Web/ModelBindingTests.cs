using System.Net;
using System.Reflection;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Binding;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The model binding pattern on the opened application and a test-only sample page (T211 criteria 2, 3, 5, 7, 8; KK2, KK3, KK5, KK7, KK8; 07 5.1.3-5.1.5):
/// two handlers of one form bind the same model, a field the model does not have is ignored, a value the server writes is not read from the request,
/// two rows of one item are a 422 and none is processed, and a request that was changed against its HTML is rejected by the server with 422 and the
/// field message. The sample page lives in the test host only.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed class ModelBindingTests
{
    private const string PagePath = "/panel/model-baglama";

    private static readonly Guid _itemA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid _itemB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private readonly CollectionDatabaseFixture _database;

    public ModelBindingTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private WebApplicationFactory<Program> Open(out BindingRecordStore store)
    {
        BindingRecordStore records = new();
        store = records;
        ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        return factory.WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            s.AddSingleton(records);
            s.AddRazorPages().AddApplicationPart(typeof(BindingFormModel).Assembly);
        }));
    }

    private static HttpClient ClientOf(WebApplicationFactory<Program> factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = false, BaseAddress = new Uri("https://localhost") });

    private static FormUrlEncodedContent Form(params (string Name, string Value)[] fields) =>
        new(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value)));

    private static FormUrlEncodedContent Rows(params (Guid Item, string Quantity)[] rows) =>
        new(rows.SelectMany((r, i) => new[]
        {
            new KeyValuePair<string, string>($"Items[{i}].OrderItemId", r.Item.ToString()),
            new KeyValuePair<string, string>($"Items[{i}].Quantity", r.Quantity),
        }));

    // --- KK2: two handlers of one form bind one input type ---

    [Fact]
    public void SaveNoteAndSearchNotes_BindTheSameInputType()
    {
        static Type? Bound(string handler) => typeof(BindingFormModel).GetMethod(handler)!.GetParameters().Single().ParameterType;

        Assert.Equal(typeof(SaveNoteInput), Bound(nameof(BindingFormModel.OnPostSaveNoteAsync)));
        Assert.Equal(typeof(SaveNoteInput), Bound(nameof(BindingFormModel.OnPostSearchNotesAsync)));
    }

    [Fact]
    public async Task BothHandlersOfTheForm_ReadTheSameFields()
    {
        await using WebApplicationFactory<Program> factory = Open(out BindingRecordStore store);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage saved = await client.PostAsync(PagePath + "?handler=SaveNote", Form(("Name", "masa"), ("Reason", "Damaged")), Ct);
        using HttpResponseMessage searched = await client.PostAsync(PagePath + "?handler=SearchNotes", Form(("Name", "masa"), ("Reason", "Damaged")), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, saved.StatusCode);
        Assert.Equal(HttpStatusCode.OK, searched.StatusCode);
        Assert.Contains("<p class=\"search-result\">masa</p>", await searched.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
        Assert.Equal(["masa"], store.Notes().Select(n => n.Name));
    }

    // --- KK3, KK5: only the model's fields are bound; the server's value is not read ---

    [Fact]
    public void Model_HasOnlyItsOwnFields_AndNoFieldForTheServersValues()
    {
        string[] fields = [.. typeof(SaveNoteInput).GetProperties(BindingFlags.Instance | BindingFlags.Public).Select(p => p.Name).Order(StringComparer.Ordinal)];

        Assert.Equal(["Name", "Reason"], fields);
    }

    [Fact]
    public async Task Post_WithFieldsTheModelDoesNotHave_IgnoresThem_AndTheServersPriceStaysTheServers()
    {
        await using WebApplicationFactory<Program> factory = Open(out BindingRecordStore store);
        using HttpClient client = ClientOf(factory);
        FormUrlEncodedContent form = Form(
            ("Name", "masa"),
            ("Reason", "Damaged"),
            ("Price", "0,01"),
            ("Total", "999"),
            ("Id", Guid.NewGuid().ToString()),
            ("Status", "Paid"));

        using HttpResponseMessage response = await client.PostAsync(PagePath + "?handler=SaveNote", form, Ct);

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        NoteRecord note = Assert.Single(store.Notes());
        Assert.Equal(new NoteRecord("masa", "Damaged", BindingRecordStore.ServerPrice), note);
    }

    [Fact]
    public async Task Post_WithTheModelsFieldsUnderItsParameterPrefix_StillIgnoresTheExtraField()
    {
        await using WebApplicationFactory<Program> factory = Open(out BindingRecordStore store);
        using HttpClient client = ClientOf(factory);
        FormUrlEncodedContent form = Form(("input.Name", "masa"), ("input.Reason", "Wrong"), ("input.Price", "0,01"), ("input.Total", "999"));

        using HttpResponseMessage response = await client.PostAsync(PagePath + "?handler=SaveNote", form, Ct);

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal(new NoteRecord("masa", "Wrong", BindingRecordStore.ServerPrice), Assert.Single(store.Notes()));
    }

    // --- KK3: one row per item ---

    [Theory]
    [InlineData("1", "2")]
    [InlineData("3", "3")]
    public async Task Post_WithTwoRowsOfTheSameItem_Is422_AddsNothingUp_AndProcessesNoRow(string firstQuantity, string secondQuantity)
    {
        await using WebApplicationFactory<Program> factory = Open(out BindingRecordStore store);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync(PagePath + "?handler=ReturnItems", Rows((_itemA, firstQuantity), (_itemA, secondQuantity)), Ct);
        string body = await response.Content.ReadAsStringAsync(Ct);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Null(response.Headers.Location);
        Assert.Contains("data-field=\"Items\">Kalemler için geçerli bir seçim yapılmalıdır.</span>", WebUtility.HtmlDecode(body), StringComparison.Ordinal);
        Assert.DoesNotContain("InvalidListValue", body, StringComparison.Ordinal);
        Assert.Empty(store.Returns());
    }

    [Fact]
    public async Task Post_WithTheRepeatedItemApartAndAnotherBetween_IsStill422AndProcessesNoRow()
    {
        await using WebApplicationFactory<Program> factory = Open(out BindingRecordStore store);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync(PagePath + "?handler=ReturnItems", Rows((_itemA, "1"), (_itemB, "1"), (_itemA, "1")), Ct);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Empty(store.Returns());
    }

    [Fact]
    public async Task Post_WithOneRowPerItem_Is303_AndProcessesEveryRowAsSent()
    {
        await using WebApplicationFactory<Program> factory = Open(out BindingRecordStore store);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync(PagePath + "?handler=ReturnItems", Rows((_itemA, "1"), (_itemB, "2")), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal([new ReturnRecord(_itemA, 1), new ReturnRecord(_itemB, 2)], store.Returns());
    }

    // --- KK7, KK8: the server is the authority; the HTML attributes are an improvement ---

    [Fact]
    public async Task TheFormCarriesTheHtmlAttributes_AsAnImprovementOnly()
    {
        await using WebApplicationFactory<Program> factory = Open(out _);
        using HttpClient client = ClientOf(factory);

        string body = await client.GetStringAsync(PagePath, Ct);

        Assert.Contains("name=\"Name\" required maxlength=\"20\"", body, StringComparison.Ordinal);
        Assert.Contains("name=\"Reason\" value=\"Damaged\"", body, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("", "Damaged", "Name")]
    [InlineData("   ", "Damaged", "Name")]
    [InlineData("bu-ad-yirmi-karakterden-uzundur", "Damaged", "Name")]
    [InlineData("masa", "Hacked", "Reason")]
    [InlineData("masa", "", "Reason")]
    public async Task Post_ChangedAgainstItsHtml_IsRejectedByTheServer_With422AndTheFieldMessage(string name, string reason, string field)
    {
        await using WebApplicationFactory<Program> factory = Open(out BindingRecordStore store);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync(PagePath + "?handler=SaveNote", Form(("Name", name), ("Reason", reason)), Ct);
        string body = await response.Content.ReadAsStringAsync(Ct);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Null(response.Headers.Location);
        Assert.Contains($"data-field=\"{field}\"", body, StringComparison.Ordinal);
        Assert.Empty(store.Notes());
    }

    [Fact]
    public async Task Post_WithoutTheFieldsAtAll_Is422WithBothFieldMessages_AndWritesNothing()
    {
        await using WebApplicationFactory<Program> factory = Open(out BindingRecordStore store);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync(PagePath + "?handler=SaveNote", Form(), Ct);
        string body = await response.Content.ReadAsStringAsync(Ct);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains("data-field=\"Name\"", body, StringComparison.Ordinal);
        Assert.Contains("data-field=\"Reason\"", body, StringComparison.Ordinal);
        Assert.Empty(store.Notes());
    }
}
