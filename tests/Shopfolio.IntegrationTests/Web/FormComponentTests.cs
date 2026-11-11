using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The form components on the opened application (T543; 04 OB-12 to OB-15; 09 3.4, 16.1): the real shared parts drawn by a test-only sample page, read from the response. The rules of the small models are
/// in the unit tests; here the markup - the label with its requirement, the section forms, the select, the trimmed value, the address fields, the IBAN places, the reason - is read.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed class FormComponentTests
{
    private static readonly TimeSpan PatternTimeout = TimeSpan.FromSeconds(5);

    private readonly CollectionDatabaseFixture _database;

    public FormComponentTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static string ProvinceValue(Guid id) => id.ToString("D");

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(FormSampleModel).Assembly)));

    private static async Task<(HttpStatusCode Status, string Html)> GetAsync(HttpClient client, string path)
    {
        using HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative), Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static async Task<(HttpStatusCode Status, string Html)> PostAsync(HttpClient client, string handler, params (string Name, string Value)[] fields)
    {
        using FormUrlEncodedContent content = new(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value)));
        using HttpResponseMessage response = await client.PostAsync(new Uri(FormSampleModel.Path + "?handler=" + handler, UriKind.Relative), content, Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static string Region(string html, string id)
    {
        int start = html.IndexOf($"<section id=\"{id}\"", StringComparison.Ordinal);
        Assert.True(start >= 0, $"No region '{id}' in the response.");
        int end = html.IndexOf("</section>", start, StringComparison.Ordinal);
        return html[start..end];
    }

    // The markup of the field with this id: from its wrapping div to the next one.
    private static string FieldOf(string html, string id)
    {
        int label = html.IndexOf($"for=\"{id}\"", StringComparison.Ordinal);
        Assert.True(label >= 0, $"No field '{id}' in the response.");
        int start = html.LastIndexOf("<div class=\"field", label, StringComparison.Ordinal);
        int next = html.IndexOf("<div class=\"field", label, StringComparison.Ordinal);
        return next < 0 ? html[start..] : html[start..next];
    }

    // ---- KK1: label, requirement text, message place ----

    [Fact]
    public async Task EveryField_HasAVisibleLabelThatSaysInWordsIfItIsRequired_KK1()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, FormSampleModel.Path);
        string fields = Region(html, "fields");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("<label class=\"field__label\" for=\"Input_Name\">Ad <span class=\"field__requirement\">(zorunlu)</span></label>", fields, StringComparison.Ordinal);
        Assert.Contains("<label class=\"field__label\" for=\"Input_Note\">Not <span class=\"field__requirement\">(isteğe bağlı)</span></label>", fields, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"<(input|select|textarea)[^>]*\srequired(=|\s|>)", RegexOptions.CultureInvariant, PatternTimeout), fields);
        Assert.Contains("aria-required=\"true\"", FieldOf(fields, "Input_Name"), StringComparison.Ordinal);
        Assert.DoesNotContain("aria-required", FieldOf(fields, "Input_Note"), StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheMessageOfAWrongField_IsUnderTheField_AndTiedToItByAriaDescribedBy_KK1()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string html) = await PostAsync(client, "Save", ("Input.Name", string.Empty), ("Input.Topic", "Order"));
        string field = FieldOf(Region(html, "fields"), "Input_Name");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.Contains("aria-describedby=\"Input_Name-message\"", field, StringComparison.Ordinal);
        Assert.Contains("aria-invalid=\"true\"", field, StringComparison.Ordinal);
        Assert.True(field.IndexOf("<input", StringComparison.Ordinal) < field.IndexOf("id=\"Input_Name-message\"", StringComparison.Ordinal));
    }

    // ---- KK2: sections, each its own form and button ----

    [Fact]
    public async Task ASectionedForm_GivesEverySectionItsOwnFormAndItsOwnButton_AndSavesOnlyThatSection_KK2()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string html) = await GetAsync(client, FormSampleModel.Path);
        int sectionsStart = html.IndexOf("<section id=\"sections\"", StringComparison.Ordinal);
        string sections = html[sectionsStart..html.IndexOf("<section id=\"address\"", sectionsStart, StringComparison.Ordinal)];

        Assert.Equal(2, Regex.Count(sections, "<form ", RegexOptions.CultureInvariant, PatternTimeout));
        Assert.Contains("handler=SaveBrand", sections, StringComparison.Ordinal);
        Assert.Contains("handler=SaveSocial", sections, StringComparison.Ordinal);
        string brandForm = sections[..sections.IndexOf("</form>", StringComparison.Ordinal)];
        Assert.Contains("name=\"Input.Brand\"", brandForm, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"Input.Social\"", brandForm, StringComparison.Ordinal);

        (_, string saved) = await PostAsync(client, "SaveBrand", ("Input.Brand", "Yerli"));
        Assert.Contains("Kaydedilen bolum: Brand; marka [Yerli] sosyal []", saved, StringComparison.Ordinal);
    }

    // ---- KK3: a closed list is a select; free text is refused ----

    [Fact]
    public async Task AClosedList_IsASelectWithNoTextBox_AndATypedValueIsRefused_KK3()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string html) = await GetAsync(client, FormSampleModel.Path);
        string topic = FieldOf(Region(html, "fields"), "Input_Topic");
        Assert.Contains("<select", topic, StringComparison.Ordinal);
        Assert.DoesNotContain("<input", topic, StringComparison.Ordinal);

        (HttpStatusCode status, string refused) = await PostAsync(client, "Save", ("Input.Name", "Ali"), ("Input.Topic", "Serbest yazı"));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.Contains("id=\"Input_Topic-message\"", refused, StringComparison.Ordinal);
        Assert.DoesNotContain("Kaydedildi:", refused, StringComparison.Ordinal);
    }

    // ---- KK4: white space around a value is ignored; white space only is empty ----

    [Fact]
    public async Task ASpaceAroundAValue_IsIgnored_AndAValueOfSpacesOnly_IsEmpty_KK4()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode trimmedStatus, string trimmed) = await PostAsync(client, "Save", ("Input.Name", "  Ali  "), ("Input.Topic", "Order"));
        Assert.Equal(HttpStatusCode.OK, trimmedStatus);
        Assert.Contains("Kaydedildi: [Ali] [Order]", trimmed, StringComparison.Ordinal);

        (HttpStatusCode emptyStatus, string empty) = await PostAsync(client, "Save", ("Input.Name", "   "), ("Input.Topic", "Order"));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, emptyStatus);
        Assert.Contains("id=\"Input_Name-message\"", empty, StringComparison.Ordinal);
    }

    // ---- KK5: the address form's fields; no postal code, no identity number ----

    [Fact]
    public async Task TheAddressForm_HasRecipientProvinceDistrictOpenAddressAndPhone_AndNoPostalCodeOrIdentityNumber_KK5()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string html) = await GetAsync(client, FormSampleModel.Path);
        string address = Region(html, "address");

        Assert.Contains("name=\"Input.RecipientName\"", address, StringComparison.Ordinal);
        Assert.Matches(new Regex("<select[^>]*name=\"Input.ProvinceId\"", RegexOptions.CultureInvariant, PatternTimeout), address);
        Assert.Contains("name=\"Input.District\"", address, StringComparison.Ordinal);
        Assert.Matches(new Regex("<textarea[^>]*name=\"Input.AddressLine\"", RegexOptions.CultureInvariant, PatternTimeout), address);
        Assert.Contains("name=\"Input.Phone\"", address, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex("postal|posta kodu|zip|tckn|kimlik", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, PatternTimeout), address);
    }

    // ---- KK6: a province the company does not deliver to is refused in the delivery variant ----

    [Fact]
    public async Task TheDeliveryAddress_RefusesAProvinceTheCompanyDoesNotDeliverTo_WithTheReasonAtTheField_AndKeepsTheOtherValues_KK6()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string html) = await PostAsync(
            client,
            "Address",
            ("Input.RecipientName", "Ali Veli"),
            ("Input.ProvinceId", ProvinceValue(FormSampleModel.ClosedProvince)),
            ("Input.District", "Çankaya"),
            ("Input.AddressLine", "Sokak 1"),
            ("Input.Phone", "5551112233"));
        string province = FieldOf(Region(html, "address"), "Input_ProvinceId");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.Contains("Bu firma Ankara iline teslimat yapmıyor.", province, StringComparison.Ordinal);
        Assert.Contains("aria-describedby=\"Input_ProvinceId-message\"", province, StringComparison.Ordinal);
        Assert.Contains("value=\"Ali Veli\"", html, StringComparison.Ordinal);
        Assert.Contains("value=\"Çankaya\"", html, StringComparison.Ordinal);

        (HttpStatusCode served, _) = await PostAsync(
            client,
            "Address",
            ("Input.RecipientName", "Ali Veli"),
            ("Input.ProvinceId", ProvinceValue(FormSampleModel.ServedProvince)),
            ("Input.District", "Kadıköy"),
            ("Input.AddressLine", "Sokak 1"),
            ("Input.Phone", "5551112233"));
        Assert.Equal(HttpStatusCode.SeeOther, served);
    }

    // ---- KK7: the IBAN field's places ----

    [Theory]
    [InlineData("Card", "Standard", "OrderPage", false)]
    [InlineData("Card", "Standard", "Declaration", false)]
    [InlineData("Card", "LatePayment", "Declaration", false)]
    [InlineData("BankTransfer", "Standard", "OrderPage", true)]
    [InlineData("BankTransfer", "Standard", "Declaration", true)]
    [InlineData("Card", "CardRefundNotRealized", "OrderPage", true)]
    [InlineData("Card", "LatePayment", "OrderPage", true)]
    public async Task TheIbanField_IsOnlyOnTheTransferLine_AndOnTheTwoExceptionsOnTheOrderPage_KK7(string method, string opening, string place, bool shown)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string html) = await GetAsync(client, $"{FormSampleModel.Path}?method={method}&opening={opening}&place={place}");

        Assert.Equal(shown, Region(html, "iban").Contains("name=\"Input.Iban\"", StringComparison.Ordinal));
    }

    [Fact]
    public async Task TheIbanFieldOfAnException_IsOptional_ARequiredOneIsRequired_AndAProcessedOneIsReadOnly_KK7()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string exception) = await GetAsync(client, $"{FormSampleModel.Path}?method=Card&opening=CardRefundNotRealized&place=OrderPage");
        Assert.Contains("(isteğe bağlı)", Region(exception, "iban"), StringComparison.Ordinal);
        Assert.Contains(FramePaths.PrivacyNotice, Region(exception, "iban"), StringComparison.Ordinal);

        (_, string required) = await GetAsync(client, $"{FormSampleModel.Path}?method=BankTransfer&opening=Standard&place=OrderPage");
        Assert.Contains("(zorunlu)", Region(required, "iban"), StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex("readonly", RegexOptions.CultureInvariant, PatternTimeout), Region(required, "iban"));

        (_, string processed) = await GetAsync(client, $"{FormSampleModel.Path}?method=BankTransfer&opening=Standard&place=OrderPage&processed=true");
        Assert.Contains("readonly", Region(processed, "iban"), StringComparison.Ordinal);
        Assert.Contains(IbanFields.ProcessedHint, Region(processed, "iban"), StringComparison.Ordinal);

        (_, string panel) = await GetAsync(client, $"{FormSampleModel.Path}?method=BankTransfer&opening=Standard&place=Panel");
        Assert.Contains("readonly", Region(panel, "iban"), StringComparison.Ordinal);
        Assert.DoesNotContain(FramePaths.PrivacyNotice, Region(panel, "iban"), StringComparison.Ordinal);
    }

    // ---- KK8: no reason, no confirmation ----

    [Fact]
    public async Task AFirmCancellationWithoutAReason_OpensNoConfirmation_AndOtherNeedsTheExplanation_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode noneStatus, string none) = await PostAsync(client, "Cancel", ("Input.Reason", string.Empty), ("Input.Explanation", string.Empty));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, noneStatus);
        Assert.DoesNotContain("confirmation-open", none, StringComparison.Ordinal);
        Assert.Contains("id=\"Input_Reason-message\"", none, StringComparison.Ordinal);

        (HttpStatusCode otherStatus, string other) = await PostAsync(client, "Cancel", ("Input.Reason", "Other"), ("Input.Explanation", "   "));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, otherStatus);
        Assert.DoesNotContain("confirmation-open", other, StringComparison.Ordinal);
        Assert.Contains("id=\"Input_Explanation-message\"", other, StringComparison.Ordinal);

        (HttpStatusCode freeStatus, string free) = await PostAsync(client, "Cancel", ("Input.Reason", "Canim istedi"), ("Input.Explanation", "x"));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, freeStatus);
        Assert.DoesNotContain("confirmation-open", free, StringComparison.Ordinal);

        (HttpStatusCode okStatus, string ok) = await PostAsync(client, "Cancel", ("Input.Reason", "OutOfStock"), ("Input.Explanation", string.Empty));
        Assert.Equal(HttpStatusCode.OK, okStatus);
        Assert.Contains("confirmation-open", ok, StringComparison.Ordinal);
    }

    // ---- KK9: a limit blocks only its own action; the sentence is neutral ----

    [Fact]
    public async Task TheLimit_BlocksOnlyItsOwnAction_WithTheNeutralSentenceBesideTheButton_AndKeepsTheValues_KK9()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string html) = await PostAsync(client, "Limit", ("Input.Name", "kalan deger"));
        int form = html.IndexOf("id=\"limit-form\"", StringComparison.Ordinal);
        string limitForm = html[form..html.IndexOf("</form>", form, StringComparison.Ordinal)];

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.True(limitForm.IndexOf("Giris yap</button>", StringComparison.Ordinal) < limitForm.IndexOf(NeutralMessages.LimitExceeded, StringComparison.Ordinal));
        Assert.Contains("value=\"kalan deger\"", limitForm, StringComparison.Ordinal);
        Assert.DoesNotContain(NeutralMessages.LimitExceeded, Region(html, "fields"), StringComparison.Ordinal);
        Assert.DoesNotContain(NeutralMessages.LimitExceeded, Region(html, "address"), StringComparison.Ordinal);

        // Another action of the same page still works.
        (HttpStatusCode other, string saved) = await PostAsync(client, "Save", ("Input.Name", "Ali"), ("Input.Topic", "Order"));
        Assert.Equal(HttpStatusCode.OK, other);
        Assert.Contains("Kaydedildi: [Ali] [Order]", saved, StringComparison.Ordinal);
    }

    // ---- KK10: the limit is the product constant, in the markup and in the message ----

    [Fact]
    public async Task TheCharacterLimit_IsTheProductConstant_InTheMaxLengthAndInTheMessage_KK10()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string limit = Shopfolio.Infrastructure.ProductConstants.ProductNameMaxLength.ToString(System.Globalization.CultureInfo.InvariantCulture);

        (_, string html) = await GetAsync(client, FormSampleModel.Path);
        Assert.Contains($"maxlength=\"{limit}\"", FieldOf(Region(html, "fields"), "Input_Name"), StringComparison.Ordinal);

        (HttpStatusCode status, string tooLong) = await PostAsync(client, "Save", ("Input.Name", new string('a', Shopfolio.Infrastructure.ProductConstants.ProductNameMaxLength + 1)), ("Input.Topic", "Order"));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.Contains($"{limit} karakter", FieldOf(Region(tooLong, "fields"), "Input_Name"), StringComparison.Ordinal);
    }
}
