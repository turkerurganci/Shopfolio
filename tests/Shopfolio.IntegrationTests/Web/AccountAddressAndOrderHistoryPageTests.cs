using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the address book and order history pages' tests (T518): one database copied from the template; every test sets its rows up again.</summary>
[CollectionDefinition(AccountAddressAndOrderHistoryPageTestsDefinition.Name)]
public sealed class AccountAddressAndOrderHistoryPageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "AccountAddressAndOrderHistoryPages";
}

/// <summary>
/// The member's address book (E-26; 07 UC-70 to UC-73) and order history (E-27; UC-74) on the opened application and a real database (T518 KK1-KK10): what each address draws, what each handler answers
/// (303, 404, 422) and writes, and which orders the history lists. Accounts, sessions, entries, orders and records are real rows of the collection database.
/// </summary>
[Collection(AccountAddressAndOrderHistoryPageTestsDefinition.Name)]
public sealed partial class AccountAddressAndOrderHistoryPageTests : IAsyncLifetime
{
    private const string BookPath = "/hesap/adresler";
    private const string HistoryPath = "/hesap/siparisler";

    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    private readonly CollectionDatabaseFixture _database;
    private Guid _ankara;
    private Guid _izmir;

    public AccountAddressAndOrderHistoryPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [GeneratedRegex(@"class=""order-history__row""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex HistoryRow();

    [GeneratedRegex(@"href=""(/siparis/[^""]+)""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex OrderLink();

    [GeneratedRegex(@"<li class=""address-card"" id=""address-([0-9a-f-]{36})""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Card();

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE customers, admins, orders, audit_logs, dispatch_records, disposal_records, link_records, carts, address_books, sessions, provinces, companies, company_settings, legal_text_versions CASCADE");
        Province ankara = Province.Create("06", "Ankara", Now);
        Province izmir = Province.Create("35", "İzmir", Now);
        CompanySettings settings = CompanySettings.Create(Now);
        settings.BankTransferPaymentWindow = 1;
        settings.DownloadAllowance = 1;
        settings.HomepageLayout = Modules.CorporateContent.Contracts.HomepageLayout.StoreFirst;
        settings.MenuNameProducts = "Ürünler";
        settings.MenuNameOfferings = "Hizmetler";
        settings.MenuNamePortfolioItems = "Çalışmalar";
        settings.MenuNameAboutPage = "Hakkımızda";
        settings.MenuNameFaqItems = "Sıkça Sorulanlar";
        settings.MenuNameContact = "İletişim";
        Company company = Company.Create(Now);
        company.CompanyType = CompanyType.Tradesman;
        company.LegalName = "Yılmaz Ticaret";
        company.TaxIdNumber = "1234567890";
        company.ProvinceId = ankara.Id;
        company.District = "Çankaya";
        company.AddressLine = "Atatürk Bulvarı 1";
        company.Phone = "3121234567";
        company.Email = "iletisim@firma.com.tr";
        company.KepAddress = "firma@hs01.kep.tr";
        company.ChamberName = "İstanbul Esnaf ve Sanatkarlar Odası";
        company.ProfessionalConductRules = "https://www.tobb.org.tr/etik-kurallar";
        await using (AppDbContext db = NewContext())
        {
            db.Provinces.AddRange(ankara, izmir);
            db.Companies.Add(company);
            db.CompanySettings.Add(settings);

            // The company delivers to İzmir only: an entry for Ankara is one the company does not deliver to (02 3.20.1).
            db.CompanyDeliveryProvinces.Add(CompanyDeliveryProvince.Create(izmir.Id, Now));
            await db.SaveChangesAsync(Ct);
        }

        _ankara = ankara.Id;
        _izmir = izmir.Id;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the full and the empty book ----

    [Fact]
    public async Task TheBook_ListsTheOwnEntriesOldestFirst_WithTheirValues_AndTheAddButtonFirst_KK1()
    {
        (string key, Guid member) = await TestSessions.CustomerAsync(_database.Dsn);
        (_, Guid other) = await TestSessions.CustomerAsync(_database.Dsn);
        await SeedEntryAsync(member, "Ev", "Ali Veli", "Çankaya", "Atatürk Bulvarı 1", "5321234567", Now.AddDays(-2));
        await SeedEntryAsync(member, "İş", "Ayşe Kaya", "Konak", "Kızılay Sokak 2", null, Now.AddDays(-1), _izmir);
        await SeedEntryAsync(other, "Yabancı", "Veli Can", "Beşiktaş", "Yabancı Cadde 9", "5329998877", Now.AddDays(-3));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string html, _) = await GetAsync(client, BookPath, key);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.True(html.IndexOf("Ev</h2>", StringComparison.Ordinal) < html.IndexOf("İş</h2>", StringComparison.Ordinal));
        Assert.Contains("Ali Veli", html, StringComparison.Ordinal);
        Assert.Contains("Çankaya, Ankara", html, StringComparison.Ordinal);
        Assert.Contains("Atatürk Bulvarı 1", html, StringComparison.Ordinal);
        Assert.Contains(DisplayFormat.Phone("5321234567"), html, StringComparison.Ordinal);
        Assert.Contains("Konak, İzmir", html, StringComparison.Ordinal);
        Assert.Equal(1, Regex.Count(html, "address-card__phone", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(2)));
        Assert.DoesNotContain("Yabancı", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Yabancı Cadde", html, StringComparison.Ordinal);
        Assert.Equal(2, Card().Count(html));
        Assert.Contains($"id=\"address-add\" class=\"button button--primary\" href=\"{BookPath}?create=true\"", html, StringComparison.Ordinal);
        Assert.Contains("address-card__actions", html, StringComparison.Ordinal);
        Assert.Contains("handler=DeleteAddressBook", html, StringComparison.Ordinal);
        Assert.DoesNotContain(AddressBookTexts.EmptySentence, html, StringComparison.Ordinal);
        Assert.DoesNotContain("address-form-section", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnEmptyBook_SaysThereIsNoSavedAddress_AndItsWayOutIsTheAddButton_KK1()
    {
        (string key, _) = await TestSessions.CustomerAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string html, _) = await GetAsync(client, BookPath, key);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains(AddressBookTexts.EmptySentence, html, StringComparison.Ordinal);
        Assert.Contains($"href=\"{BookPath}?create=true\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("address-list", html, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"address-add\"", html, StringComparison.Ordinal);
    }

    // ---- KK2: the two forms open inside the screen; a stranger's entry and a text that is no id are 404 ----

    [Fact]
    public async Task TheAddForm_OpensFromCreateTrue_AndTheEditFormFromTheEntrysId_BothInsideTheScreen_KK2()
    {
        (string key, Guid member) = await TestSessions.CustomerAsync(_database.Dsn);
        Guid entry = await SeedEntryAsync(member, "Ev", "Ali Veli", "Çankaya", "Atatürk Bulvarı 1", "5321234567", Now.AddDays(-2));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode addStatus, string add, _) = await GetAsync(client, $"{BookPath}?create=true", key);
        (HttpStatusCode editStatus, string edit, _) = await GetAsync(client, $"{BookPath}?addressBookId={entry}", key);
        (_, string closed, _) = await GetAsync(client, $"{BookPath}?create=maybe", key);

        Assert.Equal(HttpStatusCode.OK, addStatus);
        Assert.Contains("handler=CreateAddressBook", add, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"Input.AddressBookId\"", add, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.Name\"", add, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.RecipientName\"", add, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.ProvinceId\"", add, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.District\"", add, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.AddressLine\"", add, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.Phone\"", add, StringComparison.Ordinal);
        Assert.Contains(AddressBookTexts.NewHeading, add, StringComparison.Ordinal);
        Assert.Contains(AddressBookTexts.OrdersAreFrozen, add, StringComparison.Ordinal);
        Assert.Contains("Ankara", add, StringComparison.Ordinal);
        Assert.Contains("İzmir", add, StringComparison.Ordinal);

        Assert.Equal(HttpStatusCode.OK, editStatus);
        Assert.Contains("handler=UpdateAddressBook", edit, StringComparison.Ordinal);
        Assert.Contains($"name=\"Input.AddressBookId\" value=\"{entry}\"", edit, StringComparison.Ordinal);
        Assert.Contains("value=\"Ev\"", edit, StringComparison.Ordinal);
        Assert.Contains("value=\"Ali Veli\"", edit, StringComparison.Ordinal);
        Assert.Contains("Atatürk Bulvarı 1", edit, StringComparison.Ordinal);
        Assert.Contains("5321234567", edit, StringComparison.Ordinal);
        Assert.Contains($"<option value=\"{_ankara}\" selected", edit, StringComparison.Ordinal);
        Assert.Contains(AddressBookTexts.EditHeading, edit, StringComparison.Ordinal);
        Assert.Contains(AddressBookTexts.OrdersAreFrozen, edit, StringComparison.Ordinal);

        Assert.DoesNotContain("address-form-section", closed, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnotherAccountsEntry_AGoneOne_AndATextThatIsNoId_Are404_OnTheEditForm_KK2()
    {
        (string key, Guid member) = await TestSessions.CustomerAsync(_database.Dsn);
        (_, Guid other) = await TestSessions.CustomerAsync(_database.Dsn);
        Guid foreign = await SeedEntryAsync(other, "Yabancı", "Veli Can", "Beşiktaş", "Yabancı Cadde 9", null, Now);
        Guid mine = await SeedEntryAsync(member, "Ev", "Ali Veli", "Çankaya", "Atatürk Bulvarı 1", null, Now);
        await ExecuteAsync($"DELETE FROM address_books WHERE id = '{mine}'");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        foreach (string value in new[] { foreign.ToString(), mine.ToString(), Guid.NewGuid().ToString(), "abc", "00000000-0000-0000-0000-000000000000" })
        {
            (HttpStatusCode status, string html, _) = await GetAsync(client, $"{BookPath}?addressBookId={value}", key);
            Assert.Equal(HttpStatusCode.NotFound, status);
            Assert.DoesNotContain("Yabancı", html, StringComparison.Ordinal);
            Assert.DoesNotContain("Veli Can", html, StringComparison.Ordinal);
        }
    }

    // ---- KK3: the add writes, 303 with the saved notice; an entry in a province the company does not deliver to is kept ----

    [Fact]
    public async Task AnAdd_WritesTheEntry_Is303ToTheBookWithTheSavedNotice_AndAnUndeliveredProvinceIsKept_KK3()
    {
        (string key, Guid member) = await TestSessions.CustomerAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        // Ankara is the province the company does not deliver to; the telephone is written with a leading zero and separators.
        using HttpResponseMessage added = await PostAsync(client, BookPath, "CreateAddressBook", Fields("Ev", "Ali Veli", _ankara.ToString(), "Çankaya", "Atatürk Bulvarı 1", "0 (532) 123 45 67"), key);
        (_, string after, _) = await GetAsync(client, added.Headers.Location!.OriginalString, key);

        Assert.Equal(HttpStatusCode.SeeOther, added.StatusCode);
        Assert.Equal(NoticeParameter.Address(FramePaths.AccountAddresses, NoticeCode.Saved), added.Headers.Location!.OriginalString);
        List<(string Name, string Recipient, Guid Province, string District, string Line, string? Phone)> rows = await EntriesAsync(member);
        Assert.Equal([("Ev", "Ali Veli", _ankara, "Çankaya", "Atatürk Bulvarı 1", "5321234567")], rows);
        Assert.Contains(NoticeTexts.Saved, after, StringComparison.Ordinal);
        Assert.Contains("message--short-lived", after, StringComparison.Ordinal);
        Assert.Contains("Çankaya, Ankara", after, StringComparison.Ordinal);
    }

    // ---- KK4: three 422 reasons on both handlers; the form stays open with the values ----

    [Fact]
    public async Task TheCreate_RefusesAnEmptyRequiredField_AnUnlistedProvince_AndABadPhone_With422_FormOpenAndValuesKept_KK4()
    {
        (string key, Guid member) = await TestSessions.CustomerAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode empty, string emptyHtml) = await PostBodyAsync(client, BookPath, "CreateAddressBook", Fields("", "Ali Veli", _izmir.ToString(), "", "", ""), key);
        (HttpStatusCode unlisted, string unlistedHtml) = await PostBodyAsync(client, BookPath, "CreateAddressBook", Fields("Ev", "Ali Veli", Guid.NewGuid().ToString(), "Konak", "Sokak 1", ""), key);
        (HttpStatusCode garbage, string garbageHtml) = await PostBodyAsync(client, BookPath, "CreateAddressBook", Fields("Ev", "Ali Veli", "not-an-id", "Konak", "Sokak 1", ""), key);
        (HttpStatusCode phone, string phoneHtml) = await PostBodyAsync(client, BookPath, "CreateAddressBook", Fields("Ev", "Ali Veli", _izmir.ToString(), "Konak", "Sokak 1", "12345"), key);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, empty);
        Assert.Contains(FieldMessageCatalog.Build("FieldRequired", AddressForms.Labels.Name, null), emptyHtml, StringComparison.Ordinal);
        Assert.Contains(FieldMessageCatalog.Build("FieldRequired", AddressForms.Labels.District, null), emptyHtml, StringComparison.Ordinal);
        Assert.Contains(FieldMessageCatalog.Build("FieldRequired", AddressForms.Labels.AddressLine, null), emptyHtml, StringComparison.Ordinal);
        Assert.Contains("address-form-section", emptyHtml, StringComparison.Ordinal);
        Assert.Contains("value=\"Ali Veli\"", emptyHtml, StringComparison.Ordinal);
        Assert.Contains($"<option value=\"{_izmir}\" selected", emptyHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, unlisted);
        Assert.Contains(FieldMessageCatalog.Build("InvalidProvince", AddressForms.Labels.Province, null), unlistedHtml, StringComparison.Ordinal);
        Assert.Contains("value=\"Konak\"", unlistedHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, garbage);
        Assert.Contains(FieldMessageCatalog.Build("InvalidProvince", AddressForms.Labels.Province, null), garbageHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, phone);
        Assert.Contains(FieldMessageCatalog.Build("InvalidPhone", AddressForms.Labels.Phone, null), phoneHtml, StringComparison.Ordinal);
        Assert.Contains("value=\"12345\"", phoneHtml, StringComparison.Ordinal);
        foreach (string html in new[] { emptyHtml, unlistedHtml, garbageHtml, phoneHtml })
        {
            Assert.DoesNotContain("FieldRequired", html, StringComparison.Ordinal);
            Assert.DoesNotContain("InvalidProvince", html, StringComparison.Ordinal);
            Assert.DoesNotContain("InvalidPhone", html, StringComparison.Ordinal);
        }

        Assert.Empty(await EntriesAsync(member));
    }

    [Fact]
    public async Task TheUpdate_RefusesTheSameThreeReasons_With422_TheEditFormOpenAndTheEntryUnchanged_KK4()
    {
        (string key, Guid member) = await TestSessions.CustomerAsync(_database.Dsn);
        Guid entry = await SeedEntryAsync(member, "Ev", "Ali Veli", "Çankaya", "Atatürk Bulvarı 1", "5321234567", Now);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode empty, string emptyHtml) = await PostBodyAsync(client, BookPath, "UpdateAddressBook", UpdateFields(entry, "", "Ali Veli", _izmir.ToString(), "Konak", "", ""), key);
        (HttpStatusCode unlisted, string unlistedHtml) = await PostBodyAsync(client, BookPath, "UpdateAddressBook", UpdateFields(entry, "Ev", "Ali Veli", Guid.NewGuid().ToString(), "Konak", "Sokak 1", ""), key);
        (HttpStatusCode phone, string phoneHtml) = await PostBodyAsync(client, BookPath, "UpdateAddressBook", UpdateFields(entry, "Ev", "Ali Veli", _izmir.ToString(), "Konak", "Sokak 1", "12345"), key);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, empty);
        Assert.Contains(FieldMessageCatalog.Build("FieldRequired", AddressForms.Labels.Name, null), emptyHtml, StringComparison.Ordinal);
        Assert.Contains(FieldMessageCatalog.Build("FieldRequired", AddressForms.Labels.AddressLine, null), emptyHtml, StringComparison.Ordinal);
        Assert.Contains($"name=\"Input.AddressBookId\" value=\"{entry}\"", emptyHtml, StringComparison.Ordinal);
        Assert.Contains("handler=UpdateAddressBook", emptyHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, unlisted);
        Assert.Contains(FieldMessageCatalog.Build("InvalidProvince", AddressForms.Labels.Province, null), unlistedHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, phone);
        Assert.Contains(FieldMessageCatalog.Build("InvalidPhone", AddressForms.Labels.Phone, null), phoneHtml, StringComparison.Ordinal);
        Assert.Equal([("Ev", "Ali Veli", _ankara, "Çankaya", "Atatürk Bulvarı 1", "5321234567")], await EntriesAsync(member));
    }

    // ---- KK5: the update writes only to its owner; the orders keep their addresses ----

    [Fact]
    public async Task TheUpdate_WritesToTheOwnersEntryOnly_AForeignOrGoneOrNoIdEntryIs404_AndNoOrderIsTouched_KK5()
    {
        (string key, Guid member) = await TestSessions.CustomerAsync(_database.Dsn);
        (_, Guid other) = await TestSessions.CustomerAsync(_database.Dsn);
        Guid mine = await SeedEntryAsync(member, "Ev", "Ali Veli", "Çankaya", "Atatürk Bulvarı 1", "5321234567", Now);
        Guid foreign = await SeedEntryAsync(other, "Yabancı", "Veli Can", "Beşiktaş", "Yabancı Cadde 9", null, Now);
        await SeedOrderAsync(member, Now, "Lamba");
        string ordersBefore = await OrdersFingerprintAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage strangers = await PostAsync(client, BookPath, "UpdateAddressBook", UpdateFields(foreign, "Ele geçti", "Ali Veli", _izmir.ToString(), "Konak", "Sokak 1", ""), key);
        using HttpResponseMessage gone = await PostAsync(client, BookPath, "UpdateAddressBook", UpdateFields(Guid.NewGuid(), "Ev", "Ali Veli", _izmir.ToString(), "Konak", "Sokak 1", ""), key);
        using HttpResponseMessage noId = await PostAsync(client, BookPath, "UpdateAddressBook", UpdateFields(mine, "Ev", "Ali Veli", _izmir.ToString(), "Konak", "Sokak 1", "").Select(f => f.Name == "Input.AddressBookId" ? (f.Name, "abc") : f).ToList(), key);
        // Ownership comes first: a stranger's entry with broken fields is still the 404 and not the field refusal (07 2.3.3).
        using HttpResponseMessage strangersBroken = await PostAsync(client, BookPath, "UpdateAddressBook", UpdateFields(foreign, "", "", "", "", "", ""), key);

        Assert.Equal(HttpStatusCode.NotFound, strangers.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, gone.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, noId.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, strangersBroken.StatusCode);
        Assert.Equal([("Yabancı", "Veli Can", _ankara, "Beşiktaş", "Yabancı Cadde 9", null)], await EntriesAsync(other));
        Assert.Equal([("Ev", "Ali Veli", _ankara, "Çankaya", "Atatürk Bulvarı 1", "5321234567")], await EntriesAsync(member));

        using HttpResponseMessage written = await PostAsync(client, BookPath, "UpdateAddressBook", UpdateFields(mine, "İş", "Ayşe Kaya", _izmir.ToString(), "Konak", "Kızılay Sokak 2", ""), key);

        Assert.Equal(HttpStatusCode.SeeOther, written.StatusCode);
        Assert.Equal(NoticeParameter.Address(FramePaths.AccountAddresses, NoticeCode.Saved), written.Headers.Location!.OriginalString);
        Assert.Equal([("İş", "Ayşe Kaya", _izmir, "Konak", "Kızılay Sokak 2", null)], await EntriesAsync(member));
        Assert.Equal(ordersBefore, await OrdersFingerprintAsync());
    }

    // ---- KK6: the delete asks nothing, writes the 303 with no notice; a second one is 404 and writes nothing ----

    [Fact]
    public async Task TheDelete_RemovesTheEntry_Is303WithNoNotice_TheSecondIs404_AForeignOneIs404_AndNoOrderIsTouched_KK6()
    {
        (string key, Guid member) = await TestSessions.CustomerAsync(_database.Dsn);
        (_, Guid other) = await TestSessions.CustomerAsync(_database.Dsn);
        Guid mine = await SeedEntryAsync(member, "Ev", "Ali Veli", "Çankaya", "Atatürk Bulvarı 1", null, Now);
        Guid keep = await SeedEntryAsync(member, "İş", "Ayşe Kaya", "Konak", "Kızılay Sokak 2", null, Now.AddMinutes(1));
        Guid foreign = await SeedEntryAsync(other, "Yabancı", "Veli Can", "Beşiktaş", "Yabancı Cadde 9", null, Now);
        await SeedOrderAsync(member, Now, "Lamba");
        string ordersBefore = await OrdersFingerprintAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage first = await PostAsync(client, BookPath, "DeleteAddressBook", [("Input.AddressBookId", mine.ToString())], key);
        string afterFirst = await EntriesFingerprintAsync();
        using HttpResponseMessage second = await PostAsync(client, BookPath, "DeleteAddressBook", [("Input.AddressBookId", mine.ToString())], key);
        using HttpResponseMessage strangers = await PostAsync(client, BookPath, "DeleteAddressBook", [("Input.AddressBookId", foreign.ToString())], key);
        using HttpResponseMessage noId = await PostAsync(client, BookPath, "DeleteAddressBook", [("Input.AddressBookId", "abc")], key);

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Equal(FramePaths.AccountAddresses, first.Headers.Location!.OriginalString);
        Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, strangers.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, noId.StatusCode);
        Assert.Equal(afterFirst, await EntriesFingerprintAsync());
        Assert.Equal([keep], await EntryIdsAsync(member));
        Assert.Equal([foreign], await EntryIdsAsync(other));
        Assert.Equal(ordersBefore, await OrdersFingerprintAsync());
    }

    // ---- KK7: a repeat of the create is a second entry; the form's button locks itself ----

    [Fact]
    public async Task ARepeatOfTheCreate_IsASecondEntry_AndEveryFormLocksItsButton_KK7()
    {
        (string key, Guid member) = await TestSessions.CustomerAsync(_database.Dsn);
        Guid existing = await SeedEntryAsync(member, "Ev", "Ali Veli", "Çankaya", "Atatürk Bulvarı 1", null, Now.AddMinutes(-5));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string form, _) = await GetAsync(client, $"{BookPath}?create=true", key);
        (_, string list, _) = await GetAsync(client, BookPath, key);
        (_, string edit, _) = await GetAsync(client, $"{BookPath}?addressBookId={existing}", key);
        using HttpResponseMessage first = await PostAsync(client, BookPath, "CreateAddressBook", Fields("İş", "Ali Veli", _izmir.ToString(), "Konak", "Sokak 1", ""), key);
        using HttpResponseMessage again = await PostAsync(client, BookPath, "CreateAddressBook", Fields("İş", "Ali Veli", _izmir.ToString(), "Konak", "Sokak 1", ""), key);

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Equal(HttpStatusCode.SeeOther, again.StatusCode);
        Assert.Equal(3, (await EntriesAsync(member)).Count);
        Assert.Equal(2, (await EntriesAsync(member)).Count(e => e.Name == "İş"));
        foreach (string html in new[] { form, list, edit })
        {
            Assert.Matches("<form[^>]*data-js=\"submit-once\"", html);
            Assert.Contains("data-busy-label=", html, StringComparison.Ordinal);
        }
    }

    // ---- both screens: closed, personal, with the inner navigation ----

    [Fact]
    public async Task BothScreens_AreClosedAndNotStored_NameThemselvesInTheTabTitle_AndCarryTheInnerNavigation()
    {
        (string key, _) = await TestSessions.CustomerAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode bookStatus, string book, IReadOnlyList<string> bookHeaders) = await GetAsync(client, BookPath, key);
        (HttpStatusCode historyStatus, string history, IReadOnlyList<string> historyHeaders) = await GetAsync(client, HistoryPath, key);

        Assert.Equal(HttpStatusCode.OK, bookStatus);
        Assert.Equal(HttpStatusCode.OK, historyStatus);
        Assert.Contains("no-store", string.Join(';', bookHeaders), StringComparison.Ordinal);
        Assert.Contains("no-store", string.Join(';', historyHeaders), StringComparison.Ordinal);
        Assert.Contains($"<title>{AddressBookTexts.ScreenName}", book, StringComparison.Ordinal);
        Assert.Contains($"<title>{OrderHistoryTexts.ScreenName}", history, StringComparison.Ordinal);
        Assert.Contains("noindex", book, StringComparison.Ordinal);
        Assert.Contains("noindex", history, StringComparison.Ordinal);
        Assert.Contains($"<a class=\"account-nav__link account-nav__link--current\" href=\"{BookPath}\" aria-current=\"page\">{AccountTexts.NavAddresses}</a>", book, StringComparison.Ordinal);
        Assert.Contains($"<a class=\"account-nav__link account-nav__link--current\" href=\"{HistoryPath}\" aria-current=\"page\">{AccountTexts.NavOrders}</a>", history, StringComparison.Ordinal);
        Assert.Contains($"href=\"{FramePaths.Account}\"", book, StringComparison.Ordinal);
    }

    // ---- KK8: the history, newest first, 25 to a page, no search and no filter ----

    [Fact]
    public async Task TheHistory_IsNewestFirst_25APage_ThePageBoundaryHoldsTies_AndShowsNoSearchOrFilter_KK8()
    {
        (string key, Guid member) = await TestSessions.CustomerAsync(_database.Dsn);
        (_, Guid other) = await TestSessions.CustomerAsync(_database.Dsn);
        List<Guid> orders = [];
        for (int i = 0; i < 30; i++)
        {
            orders.Add((await TestSessions.OrderAsync(_database.Dsn, member)).OrderId);
        }

        string foreignNumber = (await TestSessions.OrderAsync(_database.Dsn, other)).Number;

        // Three newest orders, then 27 orders created at one and the same moment: the page boundary (25 | 5) falls inside the tie.
        await ExecuteAsync($"UPDATE orders SET created_at = '{Now.AddDays(-5):O}' WHERE customer_id = '{member}'");
        for (int i = 0; i < 3; i++)
        {
            await ExecuteAsync($"UPDATE orders SET created_at = '{Now.AddDays(-1).AddMinutes(i):O}' WHERE id = '{orders[i]}'");
        }

        List<string> expected = await ColumnAsync($"SELECT order_number FROM orders WHERE customer_id = '{member}' ORDER BY created_at DESC, id DESC");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string first, _) = await GetAsync(client, HistoryPath, key);
        (_, string second, _) = await GetAsync(client, $"{HistoryPath}?page=2", key);
        (_, string past, _) = await GetAsync(client, $"{HistoryPath}?page=99999999999999", key);
        (_, string wrong, _) = await GetAsync(client, $"{HistoryPath}?page=abc&search=x&sort=total", key);

        Assert.Equal(25, HistoryRow().Count(first));
        Assert.Equal(5, HistoryRow().Count(second));
        Assert.Equal(5, HistoryRow().Count(past));
        Assert.Equal(25, HistoryRow().Count(wrong));
        Assert.Equal(30, expected.Count);
        List<string> shown = [.. NumbersOf(first), .. NumbersOf(second)];
        Assert.Equal(expected, shown);
        Assert.Equal(expected.Count, shown.Distinct(StringComparer.Ordinal).Count());
        Assert.DoesNotContain(foreignNumber, first + second, StringComparison.Ordinal);
        Assert.Contains("rel=\"next\"", first, StringComparison.Ordinal);
        Assert.Contains("rel=\"prev\"", second, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"search\"", first, StringComparison.Ordinal);
        Assert.DoesNotContain("<select", first, StringComparison.Ordinal);
        Assert.DoesNotContain("<form", first, StringComparison.Ordinal);
        Assert.DoesNotContain("<button", first, StringComparison.Ordinal);
        Assert.Contains("badge-pair", first, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheHistoryRow_NamesTheFirstItemAndTheCountOfTheOthers_TheDayTheTotalAndTheTwoBadges_AndOpensTheMemberRoot_KK8_KK10()
    {
        (string key, Guid member) = await TestSessions.CustomerAsync(_database.Dsn);
        (Guid one, string oneNumber) = await SeedOrderAsync(member, Now.AddDays(-2), "Tek ürün");
        (Guid three, string threeNumber) = await SeedOrderAsync(member, Now.AddDays(-1), "Birinci lamba", "İkinci masa", "Üçüncü sandalye");
        (Guid none, string noneNumber) = await SeedOrderAsync(member, Now, "Yalnız ürün", "Yanındaki");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string html, _) = await GetAsync(client, HistoryPath, key);
        (HttpStatusCode opened, string order, _) = await GetAsync(client, $"/siparis/{threeNumber}", key);

        Assert.Equal([noneNumber, threeNumber, oneNumber], NumbersOf(html));
        Assert.Contains(OrderHistoryTexts.ItemsLine("Tek ürün", 0), html, StringComparison.Ordinal);
        Assert.Contains(OrderHistoryTexts.ItemsLine("Birinci lamba", 2), html, StringComparison.Ordinal);
        Assert.Contains(OrderHistoryTexts.ItemsLine("Yalnız ürün", 1), html, StringComparison.Ordinal);
        Assert.DoesNotContain("İkinci masa", html, StringComparison.Ordinal);
        Assert.Contains(DisplayFormat.Date(Now.AddDays(-1)), html, StringComparison.Ordinal);
        Assert.Contains(DisplayFormat.Amount(100m), html, StringComparison.Ordinal);
        StatusBadgesView(html, one, three, none);
        Assert.Equal(HttpStatusCode.OK, opened);
        Assert.Contains(threeNumber, order, StringComparison.Ordinal);
    }

    // ---- KK9: which orders the history lists ----

    [Fact]
    public async Task TheHistory_ListsTheGuestOrderThatFellToTheAccount_TheCustomerOrFirmCancelledOne_AndThePaidThenCancelledOne_NotTheNeverPaidSystemCancelledOnes_KK9()
    {
        (string key, Guid member) = await TestSessions.CustomerAsync(_database.Dsn);
        (_, Guid other) = await TestSessions.CustomerAsync(_database.Dsn);
        (string _, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);

        // A guest order that fell to the account later (02 3.13.2): bound by the account link, with no record at all.
        (Guid guestId, string guestNumber, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        await ExecuteAsync($"UPDATE orders SET customer_id = '{member}' WHERE id = '{guestId}'");
        (Guid byCustomer, string byCustomerNumber) = await SeedOrderAsync(member, Now.AddMinutes(-9), "Müşteri iptali");
        (Guid byFirm, string byFirmNumber) = await SeedOrderAsync(member, Now.AddMinutes(-8), "Firma iptali");
        (Guid paidThenCancelled, string paidNumber) = await SeedOrderAsync(member, Now.AddMinutes(-7), "Ödenip iptal");
        (Guid refunded, string refundedNumber) = await SeedOrderAsync(member, Now.AddMinutes(-6), "Geç ödeme iadesi");
        (Guid expired, string expiredNumber) = await SeedOrderAsync(member, Now.AddMinutes(-5), "Süresi dolan");
        (Guid cardFailed, string failedNumber) = await SeedOrderAsync(member, Now.AddMinutes(-4), "Kart başarısız");
        (Guid notOpened, string notOpenedNumber) = await SeedOrderAsync(member, Now.AddMinutes(-3), "Sayfası açılmayan");
        (Guid reordered, string reorderedNumber) = await SeedOrderAsync(member, Now.AddMinutes(-2), "Aynı sepet");
        (_, string foreignNumber) = await SeedOrderAsync(other, Now, "Yabancı sipariş");

        await SeedRecordAsync(byCustomer, TransitionCode.S3, ActorKind.Buyer, null, adminId);
        await SeedRecordAsync(byFirm, TransitionCode.S3, ActorKind.Admin, null, adminId);
        await SeedRecordAsync(paidThenCancelled, TransitionCode.O1, ActorKind.System, null, adminId);
        await SeedRecordAsync(paidThenCancelled, TransitionCode.O2, ActorKind.Buyer, null, adminId);
        await SeedRecordAsync(refunded, TransitionCode.O1, ActorKind.System, null, adminId);
        await SeedRecordAsync(refunded, TransitionCode.S3, ActorKind.System, TransitionReason.PaymentWindowExpired, adminId);
        await SeedRecordAsync(expired, TransitionCode.S3, ActorKind.System, TransitionReason.PaymentWindowExpired, adminId);
        await SeedRecordAsync(cardFailed, TransitionCode.S3, ActorKind.System, TransitionReason.CardPaymentFailed, adminId);
        await SeedRecordAsync(notOpened, TransitionCode.S3, ActorKind.System, TransitionReason.PaymentPageNotOpened, adminId);
        await SeedRecordAsync(reordered, TransitionCode.O2, ActorKind.System, TransitionReason.SameCartReorder, adminId);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string html, _) = await GetAsync(client, HistoryPath, key);
        List<string> shown = NumbersOf(html);

        Assert.Equal(
            new[] { guestNumber, byCustomerNumber, byFirmNumber, paidNumber, refundedNumber }.Order(StringComparer.Ordinal),
            shown.Order(StringComparer.Ordinal));
        foreach (string hidden in new[] { expiredNumber, failedNumber, notOpenedNumber, reorderedNumber, foreignNumber })
        {
            Assert.DoesNotContain(hidden, html, StringComparison.Ordinal);
        }
    }

    // ---- KK10: the empty history; the row's link is the member root ----

    [Fact]
    public async Task AMemberWithoutAnOrder_SeesTheEmptyLine_WithTheWayBackToTheProducts_AndNoList_KK10()
    {
        (string key, Guid member) = await TestSessions.CustomerAsync(_database.Dsn);
        (_, Guid other) = await TestSessions.CustomerAsync(_database.Dsn);
        await SeedOrderAsync(other, Now, "Yabancı sipariş");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string html, _) = await GetAsync(client, HistoryPath, key);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains(OrderHistoryTexts.EmptySentence, html, StringComparison.Ordinal);
        Assert.Contains($"href=\"/\">{OrderHistoryTexts.EmptyExitLabel}</a>", html, StringComparison.Ordinal);
        Assert.DoesNotContain("order-history__row", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Yabancı sipariş", html, StringComparison.Ordinal);
        Assert.Equal(0, await ScalarAsync($"SELECT count(*) FROM orders WHERE customer_id = '{member}'"));
    }

    // ---- no query in a loop: the commands of the history do not depend on the number of orders it shows (09 20 item 4, 25) ----

    [Fact]
    public async Task TheHistory_SendsTheSameNumberOfCommands_WhateverTheNumberOfOrdersAndItems_KK8()
    {
        (string fewKey, Guid few) = await TestSessions.CustomerAsync(_database.Dsn);
        (string manyKey, Guid many) = await TestSessions.CustomerAsync(_database.Dsn);
        await SeedOrderAsync(few, Now, "Tek");
        for (int i = 0; i < 20; i++)
        {
            await SeedOrderAsync(many, Now.AddMinutes(-i), "Ürün " + i, "Yan ürün " + i);
        }

        StorefrontSearchAndProductTests.CommandCounter counter = new();
        await using WebApplicationFactory<Program> factory = Open(counter);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        await GetAsync(client, HistoryPath, fewKey);
        int beforeFew = counter.Count;
        (_, string fewHtml, _) = await GetAsync(client, HistoryPath, fewKey);
        int fewCommands = counter.Count - beforeFew;
        int beforeMany = counter.Count;
        (_, string manyHtml, _) = await GetAsync(client, HistoryPath, manyKey);
        int manyCommands = counter.Count - beforeMany;

        Assert.Equal(1, HistoryRow().Count(fewHtml));
        Assert.Equal(20, HistoryRow().Count(manyHtml));
        Assert.True(fewCommands > 0);
        Assert.Equal(fewCommands, manyCommands);
    }

    // ---- access: the account folder's session is required, an administrator's session is not a customer's ----

    [Fact]
    public async Task BothScreens_NeedAMembersSession_AndTheirPostsWriteNothingWithoutOne()
    {
        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode bookAnonymous, _, _) = await GetAsync(client, BookPath, null);
        (HttpStatusCode historyAnonymous, _, _) = await GetAsync(client, HistoryPath, null);
        (HttpStatusCode bookAdmin, _, _) = await GetAsync(client, BookPath, null, $"{CookieNames.AdminSession}={adminKey}");
        using HttpResponseMessage createAnonymous = await PostAsync(client, BookPath, "CreateAddressBook", Fields("Ev", "Ali Veli", _izmir.ToString(), "Konak", "Sokak 1", ""), null);

        Assert.Equal(HttpStatusCode.Found, bookAnonymous);
        Assert.Equal(HttpStatusCode.Found, historyAnonymous);
        Assert.Equal(HttpStatusCode.Found, bookAdmin);
        Assert.Equal(HttpStatusCode.SeeOther, createAnonymous.StatusCode);
        Assert.StartsWith("/giris?", createAnonymous.Headers.Location!.OriginalString, StringComparison.Ordinal);
        Assert.Equal(0, await ScalarAsync("SELECT count(*) FROM address_books"));
    }

    // ---------------------------------------------------------------- helpers

    private WebApplicationFactory<Program> Open(StorefrontSearchAndProductTests.CommandCounter? counter = null) =>
        new ShopfolioWebApplicationFactory(_database.Dsn, withJobRunner: counter is null).WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            s.AddRazorPages().AddApplicationPart(typeof(ForgeryTokenSampleModel).Assembly);
            if (counter is not null)
            {
                s.ConfigureDbContext<AppDbContext>(options => options.AddInterceptors(counter));
            }
        }));

    private static List<(string Name, string Value)> Fields(string name, string recipient, string province, string district, string line, string phone) =>
    [
        ("Input.Name", name),
        ("Input.RecipientName", recipient),
        ("Input.ProvinceId", province),
        ("Input.District", district),
        ("Input.AddressLine", line),
        ("Input.Phone", phone),
    ];

    private static List<(string Name, string Value)> UpdateFields(Guid entry, string name, string recipient, string province, string district, string line, string phone) =>
        [("Input.AddressBookId", entry.ToString()), .. Fields(name, recipient, province, district, line, phone)];

    private static List<string> NumbersOf(string html) =>
        [.. OrderLink().Matches(html).Select(m => m.Groups[1].Value["/siparis/".Length..])];

    // Every row of the list carries the two badges side by side, whatever the order's axes are (04 5.27.4).
    private static void StatusBadgesView(string html, params Guid[] orders)
    {
        Assert.Equal(orders.Length, Regex.Count(html, "<span class=\"badge-pair\">", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(2)));
    }

    private async Task<Guid> SeedEntryAsync(Guid customerId, string name, string recipient, string district, string line, string? phone, DateTimeOffset at, Guid? province = null)
    {
        AddressBook entry = AddressBook.Create(customerId, name, recipient, province ?? _ankara, district, line, phone, at);
        await using AppDbContext db = NewContext();
        db.AddressBooks.Add(entry);
        await db.SaveChangesAsync(Ct);
        return entry.Id;
    }

    private async Task<(Guid OrderId, string Number)> SeedOrderAsync(Guid customerId, DateTimeOffset createdAt, params string[] itemNames)
    {
        (Guid id, string number, _) = await TestSessions.OrderAsync(_database.Dsn, customerId);
        await ExecuteAsync($"UPDATE orders SET created_at = '{createdAt:O}' WHERE id = '{id}'");
        await using AppDbContext db = NewContext();
        for (int i = 0; i < itemNames.Length; i++)
        {
            db.OrderItems.Add(OrderItem.Create(id, null, null, ProductType.Digital, itemNames[i], 1, 100m, 20m, 16.67m, createdAt.AddSeconds(i)));
        }

        await db.SaveChangesAsync(Ct);
        return (id, number);
    }

    private async Task SeedRecordAsync(Guid orderId, TransitionCode code, ActorKind actor, TransitionReason? reason, Guid adminId)
    {
        await using AppDbContext db = NewContext();
        db.TransitionRecords.Add(TransitionRecord.Create(
            orderId,
            code,
            actor,
            actor == ActorKind.Admin ? adminId : null,
            actor == ActorKind.Admin ? "Test yönetici" : null,
            reason,
            null,
            null,
            null,
            null,
            Now));
        await db.SaveChangesAsync(Ct);
    }

    private async Task<List<(string Name, string Recipient, Guid Province, string District, string Line, string? Phone)>> EntriesAsync(Guid customerId)
    {
        await using AppDbContext db = NewContext();
        List<AddressBook> rows = await db.AddressBooks.AsNoTracking().Where(a => a.CustomerId == customerId).OrderBy(a => a.CreatedAt).ThenBy(a => a.Id).ToListAsync(Ct);
        return [.. rows.Select(a => (a.Name, a.RecipientName, a.ProvinceId, a.District, a.AddressLine, a.Phone))];
    }

    private async Task<List<Guid>> EntryIdsAsync(Guid customerId)
    {
        await using AppDbContext db = NewContext();
        return await db.AddressBooks.AsNoTracking().Where(a => a.CustomerId == customerId).Select(a => a.Id).ToListAsync(Ct);
    }

    private Task<string> EntriesFingerprintAsync() => ScalarTextAsync("SELECT coalesce(md5(string_agg(a::text, '|' ORDER BY id)), '') FROM address_books a");

    private Task<string> OrdersFingerprintAsync() => ScalarTextAsync("SELECT coalesce(md5(string_agg(o::text, '|' ORDER BY id)), '') FROM orders o");

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

    private async Task<long> ScalarAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return Convert.ToInt64(await command.ExecuteScalarAsync(Ct), System.Globalization.CultureInfo.InvariantCulture);
    }

    private async Task<string> ScalarTextAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (string)(await command.ExecuteScalarAsync(Ct) ?? string.Empty);
    }

    private async Task<List<string>> ColumnAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct);
        List<string> values = [];
        while (await reader.ReadAsync(Ct))
        {
            values.Add(reader.GetString(0));
        }

        return values;
    }

    private static async Task<(HttpStatusCode Status, string Html, IReadOnlyList<string> CacheControl)> GetAsync(HttpClient client, string path, string? sessionKey, string? rawCookie = null)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, path);
        string? cookie = rawCookie ?? (sessionKey is null ? null : $"{CookieNames.Session}={sessionKey}");
        if (cookie is not null)
        {
            request.Headers.TryAddWithoutValidation("Cookie", cookie);
        }

        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        IReadOnlyList<string> cache = response.Headers.TryGetValues("Cache-Control", out IEnumerable<string>? values) ? [.. values] : [];
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)), cache);
    }

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string path, string handler, IEnumerable<(string Name, string Value)> fields, string? sessionKey)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, $"{path}?handler={handler}");
        if (sessionKey is not null)
        {
            request.Headers.TryAddWithoutValidation("Cookie", $"{CookieNames.Session}={sessionKey}");
        }

        request.Content = new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value)));
        return await client.SendAsync(request, Ct);
    }

    private static async Task<(HttpStatusCode Status, string Html)> PostBodyAsync(HttpClient client, string path, string handler, IEnumerable<(string Name, string Value)> fields, string? sessionKey)
    {
        using HttpResponseMessage response = await PostAsync(client, path, handler, fields, sessionKey);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }
}
