using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The corporate content screens on the opened application and a real database (T534; 07 UC-96, UC-98, UC-158 to UC-161, UC-163; 04 9.13, 9.14): the list with its seven groups and no paging, the form that
/// is 404 outside the closed type list, the save that makes a draft, the gate's refusal, the absolute status move, the deletion's confirmation state and the deletion, the move in the manual order with its
/// stale-screen refusal, and the brand screen's read and save.
/// </summary>
[Collection(ProductListPageTestsDefinition.Name)]
public sealed class ContentPanelPageTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Base = new(2026, 11, 14, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _database;

    private string _adminKey = string.Empty;

    private int _counter;

    public ContentPanelPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ListSeed.ClearAsync(_database.Dsn);
        await ExecuteAsync("TRUNCATE about_pages, offerings, portfolio_items, faq_items, branches, custom_pages, announcements, images CASCADE");
        _adminKey = (await TestSessions.AdminAsync(_database.Dsn)).Key;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task TheList_ShowsTheSevenGroupsInOrder_WithoutAPager_AndDrawsNoInlineStyle()
    {
        await SeedOfferingAsync("Montaj", 1);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, PanelFramePaths.CorporateContent);

        Assert.Equal(HttpStatusCode.OK, status);
        int[] at = [.. Enum.GetValues<PanelContentType>().Select(t => html.IndexOf(ContentRoutes.GroupNameOf(t), StringComparison.Ordinal))];
        Assert.All(at, position => Assert.True(position >= 0));
        Assert.Equal(at.Order(), at);
        Assert.Contains("Montaj", html, StringComparison.Ordinal);
        Assert.DoesNotContain("style=", html, StringComparison.Ordinal);
        Assert.DoesNotContain("pager", html, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("/panel/kurumsal-icerik/urun/yeni")]
    [InlineData("/panel/kurumsal-icerik/HIZMET/yeni")]
    [InlineData("/panel/kurumsal-icerik/hizmet/00000000-0000-0000-0000-000000000001")]
    [InlineData("/panel/kurumsal-icerik/sss/00000000-0000-0000-0000-000000000001")]
    public async Task AFormOutsideTheClosedTypeList_OrOfAMissingRecord_Is404(string path)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, _) = await GetAsync(client, path);

        Assert.Equal(HttpStatusCode.NotFound, status);
    }

    [Fact]
    public async Task TheNewForm_Opens_AndASave_MakesADraft_AtTheEndOfItsGroup_AndRedirectsToTheRecord()
    {
        await SeedOfferingAsync("Eski hizmet", 1);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode opened, _) = await GetAsync(client, ContentRoutes.NewPath(PanelContentType.Offering));
        using HttpResponseMessage saved = await PostAsync(client, ContentRoutes.NewPath(PanelContentType.Offering), "SaveContent", ("Input.Name", "Yeni hizmet"));

        Assert.Equal(HttpStatusCode.OK, opened);
        Assert.Equal(HttpStatusCode.SeeOther, saved.StatusCode);
        Assert.Equal("Draft", await ScalarAsync<string>("SELECT publish_status FROM offerings WHERE name = 'Yeni hizmet'"));
        Assert.Equal(2, await ScalarAsync<int>("SELECT sort_order FROM offerings WHERE name = 'Yeni hizmet'"));
        Assert.StartsWith(ContentRoutes.Root + "/hizmet/", saved.Headers.Location!.OriginalString, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TwoAdministratorsOnTheSameContentRecord_TheSecondSaveIsWarned_AndTheFirstOnesValueStaysUntilTheOverwrite_VALA371_VALA372_D1167()
    {
        Guid offering = await SeedOfferingAsync("Montaj", 1);
        int opened = await ScalarAsync<int>($"SELECT version FROM offerings WHERE id = '{offering}'");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        string path = ContentRoutes.FormPath(PanelContentType.Offering, offering);

        // Both administrators opened the form at the same version; the first one saves through the screen.
        using HttpResponseMessage first = await PostAsync(client, path, "SaveContent", ("Input.Name", "Birinci yönetici"), ("Input.Version", opened.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        using HttpResponseMessage second = await PostAsync(client, path, "SaveContent", ("Input.Name", "İkinci yönetici"), ("Input.Version", opened.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        string html = WebUtility.HtmlDecode(await second.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Contains(Shopfolio.Infrastructure.Forms.ConflictControls.RecordMessage, html, StringComparison.Ordinal);
        Assert.Contains(Shopfolio.Infrastructure.Forms.ConflictControls.OverwriteLabel, html, StringComparison.Ordinal);
        Assert.Contains("İkinci yönetici", html, StringComparison.Ordinal);
        Assert.Equal("Birinci yönetici", await ScalarAsync<string>($"SELECT name FROM offerings WHERE id = '{offering}'"));

        using HttpResponseMessage overwrite = await PostAsync(client, path, "SaveContent", ("Input.Name", "İkinci yönetici"), ("Input.Version", (opened + 1).ToString(System.Globalization.CultureInfo.InvariantCulture)));

        Assert.Equal(HttpStatusCode.SeeOther, overwrite.StatusCode);
        Assert.Equal("İkinci yönetici", await ScalarAsync<string>($"SELECT name FROM offerings WHERE id = '{offering}'"));
    }

    [Fact]
    public async Task ASave_WithoutTheRequiredName_IsRefused422_AndWritesNothing()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage refused = await PostAsync(client, ContentRoutes.NewPath(PanelContentType.Offering), "SaveContent", ("Input.Name", "  "));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM offerings"));
    }

    [Fact]
    public async Task APublishMove_OfAnIncompleteRecord_Is422_AndOfACompleteOne_RedirectsAndPublishes()
    {
        Guid incomplete = await SeedFaqAsync("Boş cevap?", null, 1);
        Guid complete = await SeedFaqAsync("İade var mı?", "<p>Evet.</p>", 2);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage refused = await PostAsync(client, ContentRoutes.FormPath(PanelContentType.FaqItem, incomplete), "ChangePublishStatus", ("Input.PublishStatus", "Published"));
        using HttpResponseMessage moved = await PostAsync(client, ContentRoutes.FormPath(PanelContentType.FaqItem, complete), "ChangePublishStatus", ("Input.PublishStatus", "Published"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
        Assert.Equal(HttpStatusCode.SeeOther, moved.StatusCode);
        Assert.Equal("Draft", await ScalarAsync<string>($"SELECT publish_status FROM faq_items WHERE id = '{incomplete}'"));
        Assert.Equal("Published", await ScalarAsync<string>($"SELECT publish_status FROM faq_items WHERE id = '{complete}'"));
    }

    [Fact]
    public async Task ADeletion_WithoutTheMark_ShowsTheConfirmation_AndWritesNothing_ThenWithIt_DeletesAndGoesToTheList()
    {
        Guid faq = await SeedFaqAsync("İade var mı?", "<p>Evet.</p>", 1);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        string path = ContentRoutes.FormPath(PanelContentType.FaqItem, faq);

        using HttpResponseMessage asked = await PostAsync(client, path, "DeleteContent");
        long afterAsk = await ScalarAsync<long>("SELECT count(*) FROM faq_items");
        using HttpResponseMessage done = await PostAsync(client, path, "DeleteContent", ("Input.Confirmed", "true"));

        Assert.Equal(HttpStatusCode.OK, asked.StatusCode);
        Assert.Equal(1L, afterAsk);
        Assert.Equal(HttpStatusCode.SeeOther, done.StatusCode);
        Assert.Equal(PanelFramePaths.CorporateContent, done.Headers.Location!.OriginalString);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM faq_items"));
    }

    [Fact]
    public async Task AMove_WithTheShownOrders_SwapsTheRecords_AndAStaleScreen_Is409_WithNothingWritten()
    {
        await SeedOfferingAsync("Birinci", 1);
        Guid second = await SeedOfferingAsync("İkinci", 2);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage stale = await PostAsync(
            client, PanelFramePaths.CorporateContent, "ReorderContent",
            ("Input.ContentKind", "hizmet"), ("Input.ContentId", second.ToString("D")), ("Input.Direction", "Up"), ("Input.OriginalSortOrder", "2"), ("Input.OriginalAdjacentSortOrder", "9"));
        int afterStale = await ScalarAsync<int>($"SELECT sort_order FROM offerings WHERE id = '{second}'");
        using HttpResponseMessage moved = await PostAsync(
            client, PanelFramePaths.CorporateContent, "ReorderContent",
            ("Input.ContentKind", "hizmet"), ("Input.ContentId", second.ToString("D")), ("Input.Direction", "Up"), ("Input.OriginalSortOrder", "2"), ("Input.OriginalAdjacentSortOrder", "1"));

        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal(2, afterStale);
        Assert.Equal(HttpStatusCode.SeeOther, moved.StatusCode);
        Assert.Equal(1, await ScalarAsync<int>($"SELECT sort_order FROM offerings WHERE id = '{second}'"));
    }

    [Fact]
    public async Task TheBrandScreen_Opens_AnAnInvalidColor_Is422_AStaleVersion_Is409_AndAGoodSave_RedirectsAndShowsTheNewName()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode opened, string html) = await GetAsync(client, PanelFramePaths.Brand);
        int version = int.Parse(System.Text.RegularExpressions.Regex.Match(System.Text.RegularExpressions.Regex.Match(html, @"<input[^>]*Input\.Version[^>]*>").Value, @"value=""(\d+)""").Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
        using HttpResponseMessage badColor = await PostAsync(client, PanelFramePaths.Brand, "SaveBranding", ("Input.BrandName", "Deneme"), ("Input.BrandColor", "ZZZ"), ("Input.Version", version.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        using HttpResponseMessage stale = await PostAsync(client, PanelFramePaths.Brand, "SaveBranding", ("Input.BrandName", "Deneme"), ("Input.Version", "999"));
        using HttpResponseMessage saved = await PostAsync(client, PanelFramePaths.Brand, "SaveBranding", ("Input.BrandName", "Deneme Markası"), ("Input.Version", version.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        (_, string after) = await GetAsync(client, PanelFramePaths.Brand);

        Assert.Equal(HttpStatusCode.OK, opened);
        Assert.Contains("Marka", html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, badColor.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal(HttpStatusCode.SeeOther, saved.StatusCode);
        Assert.Contains("Deneme Markası", after, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheProductSearch_WritesNothing_AndTheTickedResult_JoinsTheLinkedProducts_OfTheSameForm()
    {
        Guid product = await SeedProductAsync("Aranan masa");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        string path = ContentRoutes.NewPath(PanelContentType.Offering);

        using HttpResponseMessage searched = await PostAsync(client, path, "SearchProducts", ("Input.Name", "Yeni hizmet"), ("Input.Search", "Aranan"));
        string searchedHtml = WebUtility.HtmlDecode(await searched.Content.ReadAsStringAsync(Ct));
        using HttpResponseMessage chosen = await PostAsync(client, path, "SearchProducts", ("Input.Name", "Yeni hizmet"), ("Input.AddProducts", product.ToString("D")));
        string chosenHtml = WebUtility.HtmlDecode(await chosen.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.OK, searched.StatusCode);
        Assert.Contains("Aranan masa", searchedHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, chosen.StatusCode);
        Assert.Contains($"Input.LinkedProducts[0].ProductId\" value=\"{product:D}\"", chosenHtml, StringComparison.Ordinal);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM offerings"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM offering_products"));
    }

    [Fact]
    public async Task AFilledBrandName_ThatIsEmptied_Is422_AndTheSavedNameStays()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        (_, string html) = await GetAsync(client, PanelFramePaths.Brand);
        string version = System.Text.RegularExpressions.Regex.Match(System.Text.RegularExpressions.Regex.Match(html, @"<input[^>]*Input\.Version[^>]*>").Value, @"value=""(\d+)""").Groups[1].Value;
        using HttpResponseMessage first = await PostAsync(client, PanelFramePaths.Brand, "SaveBranding", ("Input.BrandName", "Kalıcı ad"), ("Input.Version", version));
        (_, string again) = await GetAsync(client, PanelFramePaths.Brand);
        string next = System.Text.RegularExpressions.Regex.Match(System.Text.RegularExpressions.Regex.Match(again, @"<input[^>]*Input\.Version[^>]*>").Value, @"value=""(\d+)""").Groups[1].Value;

        using HttpResponseMessage emptied = await PostAsync(client, PanelFramePaths.Brand, "SaveBranding", ("Input.BrandName", "  "), ("Input.Version", next));
        (_, string after) = await GetAsync(client, PanelFramePaths.Brand);

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, emptied.StatusCode);
        Assert.Contains("Kalıcı ad", after, StringComparison.Ordinal);
    }

    // ---- D-1547: the behaviours T534's validation proved only by a temporary check ----

    [Fact]
    public async Task TheNewAddress_OfASingleRecord_WithTheRecordThere_Is303ToIt_ForTheAboutPageAndTheAnnouncement()
    {
        AboutPage about = AboutPage.Create(Base);
        Announcement announcement = Announcement.Create(Base);
        await SaveAsync(c =>
        {
            c.AboutPages.Add(about);
            c.Announcements.Add(announcement);
        });
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage aboutMove = await client.GetAsync(new Uri(ContentRoutes.NewPath(PanelContentType.AboutPage), UriKind.Relative), Ct);
        using HttpResponseMessage announcementMove = await client.GetAsync(new Uri(ContentRoutes.NewPath(PanelContentType.Announcement), UriKind.Relative), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, aboutMove.StatusCode);
        Assert.Equal(ContentRoutes.FormPath(PanelContentType.AboutPage, about.Id), aboutMove.Headers.Location!.OriginalString);
        Assert.Equal(HttpStatusCode.SeeOther, announcementMove.StatusCode);
        Assert.Equal(ContentRoutes.FormPath(PanelContentType.Announcement, announcement.Id), announcementMove.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task ASecondAnnouncement_FromTheNewForm_Is409_WritesNothing_AndTheFormBindsToTheRecordThatIsThere()
    {
        Announcement existing = Announcement.Create(Base);
        existing.Text = "Bayram tatili";
        await SaveAsync(c => c.Announcements.Add(existing));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage second = await PostAsync(client, ContentRoutes.NewPath(PanelContentType.Announcement), "SaveContent", ("Input.Text", "Başka duyuru"));
        string html = WebUtility.HtmlDecode(await second.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Contains(ContentRoutes.FormPath(PanelContentType.Announcement, existing.Id) + "?handler=SaveContent", html, StringComparison.Ordinal);
        Assert.DoesNotContain(ContentRoutes.NewPath(PanelContentType.Announcement) + "?handler=SaveContent", html, StringComparison.Ordinal);
        Assert.Contains(Shopfolio.Infrastructure.Forms.ConflictControls.OverwriteLabel, html, StringComparison.Ordinal);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM announcements"));
        Assert.Equal("Bayram tatili", await ScalarAsync<string>($"SELECT text FROM announcements WHERE id = '{existing.Id}'"));
    }

    [Theory]
    [InlineData(150, HttpStatusCode.SeeOther, 1L)]
    [InlineData(151, HttpStatusCode.UnprocessableEntity, 0L)]
    public async Task TheNameCeiling_P27_IsOneHundredFiftyCharacters_At151_ThePageIs422_AndWritesNothing(int length, HttpStatusCode expected, long rows)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await PostAsync(client, ContentRoutes.NewPath(PanelContentType.Offering), "SaveContent", ("Input.Name", new string('a', length)));

        Assert.Equal(expected, response.StatusCode);
        Assert.Equal(rows, await ScalarAsync<long>("SELECT count(*) FROM offerings"));
    }

    [Theory]
    [InlineData(120, HttpStatusCode.SeeOther, 1L)]
    [InlineData(121, HttpStatusCode.UnprocessableEntity, 0L)]
    public async Task TheAnnouncementTextCeiling_IsOneHundredTwentyCharacters_At121_ThePageIs422_AndWritesNothing(int length, HttpStatusCode expected, long rows)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await PostAsync(client, ContentRoutes.NewPath(PanelContentType.Announcement), "SaveContent", ("Input.Text", new string('d', length)));

        Assert.Equal(expected, response.StatusCode);
        Assert.Equal(rows, await ScalarAsync<long>("SELECT count(*) FROM announcements"));
    }

    [Theory]
    [InlineData("https://maps.example.com/sube", HttpStatusCode.SeeOther, 1L)]
    [InlineData("maps.example.com/sube", HttpStatusCode.UnprocessableEntity, 0L)]
    [InlineData("javascript:alert(1)", HttpStatusCode.UnprocessableEntity, 0L)]
    [InlineData("https://maps.example.com/bir sube", HttpStatusCode.UnprocessableEntity, 0L)]
    public async Task TheBranchMapLink_NeedsTheFormOfALink_OtherwiseThePageIs422_AndWritesNothing(string link, HttpStatusCode expected, long rows)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await PostAsync(client, ContentRoutes.NewPath(PanelContentType.Branch), "SaveContent", ("Input.Name", "Merkez şube"), ("Input.MapUrl", link));

        Assert.Equal(expected, response.StatusCode);
        Assert.Equal(rows, await ScalarAsync<long>("SELECT count(*) FROM branches"));
    }

    [Fact]
    public async Task TheAboutPageDeletion_IsRefused422_OnThePage_EvenWithTheMark_AndTheRecordStays()
    {
        AboutPage about = AboutPage.Create(Base);
        await SaveAsync(c => c.AboutPages.Add(about));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        string path = ContentRoutes.FormPath(PanelContentType.AboutPage, about.Id);

        using HttpResponseMessage refused = await PostAsync(client, path, "DeleteContent", ("Input.Confirmed", "true"));
        string html = WebUtility.HtmlDecode(await refused.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
        Assert.Contains(ContentFormModel.AboutNotDeletableText, html, StringComparison.Ordinal);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM about_pages"));
    }

    [Fact]
    public async Task TheBrandForm_WithADeletedLogoOrIcon_Is422_AtTheImage_AndWritesNothing_ABoundImage_Saves()
    {
        Guid image = await SeedImageAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        string version = await BrandVersionAsync(client);
        string gone = Guid.CreateVersion7().ToString("D");

        using HttpResponseMessage deletedLogo = await PostAsync(client, PanelFramePaths.Brand, "SaveBranding", ("Input.BrandName", "Deneme"), ("Input.LogoId", gone), ("Input.Version", version));
        string logoHtml = WebUtility.HtmlDecode(await deletedLogo.Content.ReadAsStringAsync(Ct));
        using HttpResponseMessage deletedIcon = await PostAsync(client, PanelFramePaths.Brand, "SaveBranding", ("Input.BrandName", "Deneme"), ("Input.FaviconId", gone), ("Input.Version", version));
        string iconHtml = WebUtility.HtmlDecode(await deletedIcon.Content.ReadAsStringAsync(Ct));
        string afterRefusals = await BrandVersionAsync(client);
        using HttpResponseMessage bound = await PostAsync(client, PanelFramePaths.Brand, "SaveBranding", ("Input.BrandName", "Deneme"), ("Input.LogoId", image.ToString("D")), ("Input.Version", version));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, deletedLogo.StatusCode);
        Assert.Contains(BrandModel.ExpiredImageText, logoHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, deletedIcon.StatusCode);
        Assert.Contains(BrandModel.ExpiredImageText, iconHtml, StringComparison.Ordinal);
        Assert.Equal(version, afterRefusals);
        Assert.Equal(HttpStatusCode.SeeOther, bound.StatusCode);
    }

    [Fact]
    public async Task TheBrandForm_ShowsTheFallbackTexts_UntilTheNameAndTheLogoAreSet()
    {
        Guid image = await SeedImageAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string bare) = await GetAsync(client, PanelFramePaths.Brand);
        string version = await BrandVersionAsync(client);
        using HttpResponseMessage saved = await PostAsync(client, PanelFramePaths.Brand, "SaveBranding", ("Input.BrandName", "Marka Ltd"), ("Input.LogoId", image.ToString("D")), ("Input.Version", version));
        (_, string set) = await GetAsync(client, PanelFramePaths.Brand);

        Assert.Contains(BrandModel.NameFallbackPrefix, bare, StringComparison.Ordinal);
        Assert.Contains(BrandModel.LogoFallbackText, bare, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, saved.StatusCode);
        Assert.DoesNotContain(BrandModel.NameFallbackPrefix, set, StringComparison.Ordinal);
        Assert.DoesNotContain(BrandModel.LogoFallbackText, set, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheBranchImage_IsWritten_OnANewRecordAndOnAnEdit_AndADeletedImage_Is422_AtTheImage()
    {
        Guid first = await SeedImageAsync();
        Guid second = await SeedImageAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage created = await PostAsync(client, ContentRoutes.NewPath(PanelContentType.Branch), "SaveContent", ("Input.Name", "Merkez"), ("Input.ImageId", first.ToString("D")), ("Input.ImageAltText", "Giriş"));
        Guid branch = await ScalarAsync<Guid>("SELECT id FROM branches WHERE name = 'Merkez'");
        Assert.Equal(first, await ScalarAsync<Guid>($"SELECT image_id FROM branches WHERE id = '{branch}'"));
        Assert.Equal("Giriş", await ScalarAsync<string>($"SELECT image_alt_text FROM branches WHERE id = '{branch}'"));
        string path = ContentRoutes.FormPath(PanelContentType.Branch, branch);
        string version = (await ScalarAsync<int>($"SELECT version FROM branches WHERE id = '{branch}'")).ToString(System.Globalization.CultureInfo.InvariantCulture);

        using HttpResponseMessage deleted = await PostAsync(client, path, "SaveContent", ("Input.Name", "Merkez"), ("Input.ImageId", Guid.CreateVersion7().ToString("D")), ("Input.Version", version));
        string deletedHtml = WebUtility.HtmlDecode(await deleted.Content.ReadAsStringAsync(Ct));
        Guid afterRefusal = await ScalarAsync<Guid>($"SELECT image_id FROM branches WHERE id = '{branch}'");
        using HttpResponseMessage edited = await PostAsync(client, path, "SaveContent", ("Input.Name", "Merkez"), ("Input.ImageId", second.ToString("D")), ("Input.Version", version));

        Assert.Equal(HttpStatusCode.SeeOther, created.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, deleted.StatusCode);
        Assert.Contains(ContentFormModel.ExpiredImageText, deletedHtml, StringComparison.Ordinal);
        Assert.Equal(first, afterRefusal);
        Assert.Equal(HttpStatusCode.SeeOther, edited.StatusCode);
        Assert.Equal(second, await ScalarAsync<Guid>($"SELECT image_id FROM branches WHERE id = '{branch}'"));
    }

    [Fact]
    public async Task TheOpenForm_AfterAPublishMove_IsAStaleForm_AndItsSaveIs409_WithTheMovedRecordKept()
    {
        Guid faq = await SeedFaqAsync("İade var mı?", "<p>Evet.</p>", 1);
        int opened = await ScalarAsync<int>($"SELECT version FROM faq_items WHERE id = '{faq}'");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        string path = ContentRoutes.FormPath(PanelContentType.FaqItem, faq);

        using HttpResponseMessage moved = await PostAsync(client, path, "ChangePublishStatus", ("Input.PublishStatus", "Published"));
        using HttpResponseMessage stale = await PostAsync(client, path, "SaveContent", ("Input.Question", "Değişen soru?"), ("Input.Answer", "<p>Hayır.</p>"), ("Input.Version", opened.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        Assert.Equal(HttpStatusCode.SeeOther, moved.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal("Published", await ScalarAsync<string>($"SELECT publish_status FROM faq_items WHERE id = '{faq}'"));
        Assert.Equal("İade var mı?", await ScalarAsync<string>($"SELECT question FROM faq_items WHERE id = '{faq}'"));
    }

    // ---- helpers ----

    private static async Task<string> BrandVersionAsync(HttpClient client)
    {
        (_, string html) = await GetAsync(client, PanelFramePaths.Brand);
        return System.Text.RegularExpressions.Regex.Match(System.Text.RegularExpressions.Regex.Match(html, @"<input[^>]*Input\.Version[^>]*>").Value, @"value=""(\d+)""").Groups[1].Value;
    }

    private async Task<Guid> SeedImageAsync()
    {
        int n = Interlocked.Increment(ref _counter);
        Image image = Image.Create($"gorsel-{n:D4}", ImageFormat.Jpeg, 800, 600, Base);
        await SaveAsync(c => c.Images.Add(image));
        return image.Id;
    }

    private async Task<Guid> SeedProductAsync(string name)
    {
        int n = Interlocked.Increment(ref _counter);
        Category category = Category.Create("Mobilya", $"kategori-{n:D3}", null, 1, Base.AddMinutes(n));
        Product product = Product.Create(name, $"urun-{n:D4}", ProductType.Physical, 20m, Base.AddMinutes(n)).WithSeededOrigin();
        product.PublishStatus = PublishStatus.Published;
        await SaveAsync(c =>
        {
            c.Categories.Add(category);
            c.Products.Add(product);
            c.ProductCategories.Add(ProductCategory.Create(product.Id, category.Id, true, Base));
        });
        return product.Id;
    }

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(ForgeryTokenSampleModel).Assembly)));

    private HttpClient ClientOf(WebApplicationFactory<Program> factory)
    {
        HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        client.DefaultRequestHeaders.Add(HeaderNames.Cookie, $"{CookieNames.AdminSession}={_adminKey}");
        return client;
    }

    private static async Task<(HttpStatusCode Status, string Html)> GetAsync(HttpClient client, string path)
    {
        using HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative), Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, string path, string handler, params (string Name, string Value)[] fields) =>
        client.PostAsync($"{path}?handler={handler}", new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value))), Ct);

    private async Task<Guid> SeedOfferingAsync(string name, int order)
    {
        int n = Interlocked.Increment(ref _counter);
        Offering offering = Offering.Create(name, $"hizmet-{n:D3}", order, Base.AddMinutes(n));
        await SaveAsync(c => c.Offerings.Add(offering));
        return offering.Id;
    }

    private async Task<Guid> SeedFaqAsync(string question, string? answer, int order)
    {
        int n = Interlocked.Increment(ref _counter);
        FaqItem faq = FaqItem.Create(question, order, Base.AddMinutes(n));
        faq.SetAnswer(answer);
        await SaveAsync(c => c.FaqItems.Add(faq));
        return faq.Id;
    }

    private async Task SaveAsync(Action<AppDbContext> change)
    {
        await using AppDbContext db = ListSeed.Open(_database.Dsn);
        change(db);
        await db.SaveChangesAsync(Ct);
    }

    private async Task<T> ScalarAsync<T>(string sql)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (T)(await command.ExecuteScalarAsync(Ct))!;  // non-null: every query here returns one row
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }
}
