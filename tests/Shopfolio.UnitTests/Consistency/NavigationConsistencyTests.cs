using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Security;
using Shopfolio.Pages.Frame;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Equality check of the navigation map (T546; 04 3.1-3.7; 09 19.2 item 3, 21.4 item 1): the rows of the tables and the paragraphs of 3.6 equal the entries of <see cref="NavigationInventory"/> - each with the
/// screens it leaves and reaches - and every entry is carried by the links, redirects and pages of the code; every link and redirect of the web project is a row's evidence or a listed exemption, so a
/// transition that is not in the table does not enter the code. The panel's menu equals the menu table of 3.2 entry by entry, and the return rules of 3.6 are read from the functions that decide them. The
/// checks that must fail read a changed copy of the document or of the sources, never the files (09 19.2 item 3). Reads repository files only; runs in the Lint step.
/// </summary>
[Trait("Consistency", "true")]
public sealed class NavigationConsistencyTests
{
    private static readonly Lazy<string> Document = new(NavigationDocument.Load);

    private static readonly Lazy<IReadOnlyDictionary<string, string>> Sources = new(() => NavigationSites.Load(RepositoryRoot.Find()));

    private static readonly string[] Routes =
        [.. EndpointBindingCheck.PagesOf(typeof(Program).Assembly).Select(p => "/" + p.Template.TrimStart('/')).Distinct(StringComparer.Ordinal)];

    private static IReadOnlyList<string> Problems(string? document = null, IReadOnlyDictionary<string, string>? sources = null, IReadOnlyList<string>? routes = null)
        => NavigationCheck.Problems(document ?? Document.Value, sources ?? Sources.Value, routes ?? Routes);

    private static Dictionary<string, string> Changed(string file, string from, string to)
    {
        Dictionary<string, string> copy = new(Sources.Value, StringComparer.Ordinal);
        string text = copy[file];
        Assert.Contains(from, text, StringComparison.Ordinal);
        copy[file] = text.Replace(from, to, StringComparison.Ordinal);
        return copy;
    }

    private static string WithoutRow(string id)
    {
        string changed = Regex.Replace(Document.Value, "^\\| " + Regex.Escape(id) + " \\|.*\\n", string.Empty, RegexOptions.Multiline | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));
        Assert.NotEqual(Document.Value, changed);
        return changed;
    }

    [Fact]
    public void NavigationMap_EqualsTheCode_InBothDirections()
    {
        IReadOnlyList<string> problems = Problems();

        Assert.True(problems.Count == 0, string.Join('\n', problems));
    }

    [Fact]
    public void Document_HasTheRowsOfTheSixTables_AndTheFiveReturnParagraphs()
    {
        IReadOnlyList<NavigationRow> rows = NavigationDocument.Rows(Document.Value);
        IReadOnlyDictionary<string, string> returns = NavigationDocument.Returns(Document.Value);

        // The count is not written here: the entries equal the rows of the document (the row count is read from the table), each table keeps its numbering without a gap.
        Assert.NotEmpty(rows);
        Assert.Equal(["3.6.1", "3.6.2", "3.6.3", "3.6.4", "3.6.5"], returns.Keys.Order(StringComparer.Ordinal));
        Assert.Equal(rows.Count + returns.Count, NavigationInventory.Entries.Count);
        foreach (IGrouping<string, NavigationRow> table in rows.GroupBy(r => string.Join('.', r.Id.Split('.')[..2]), StringComparer.Ordinal))
        {
            Assert.Equal(Enumerable.Range(1, table.Count()).Select(n => $"{table.Key}.{n}"), table.Select(r => r.Id));
        }

        Assert.Equal(NavigationInventory.Entries.Count, NavigationInventory.Entries.Select(e => e.Id).Distinct(StringComparer.Ordinal).Count());
    }

    // KK1: a row of the customer side deleted from the table, or its link gone from the code.
    [Fact]
    public void CustomerTransition_DeletedFromTheTable_IsReported()
    {
        Assert.Contains(Problems(document: WithoutRow("3.3.7")), p => p.Contains("RowOnlyInCode: 3.3.7", StringComparison.Ordinal));
    }

    [Fact]
    public void CustomerTransition_WhoseLinkLeavesTheCode_IsReported()
    {
        IReadOnlyDictionary<string, string> sources = Changed("Pages/Storefront/Cart.cshtml", "href=\"@FramePaths.Checkout\"", "href=\"#\"");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("EvidenceMissing:3.3.14", StringComparison.Ordinal));
    }

    [Fact]
    public void CustomerTransition_WhoseTargetChangesInTheTable_IsReported()
    {
        string changed = Document.Value.Replace("| 3.3.14 | E-12 | Ödeme adımına geçiş | E-13 |", "| 3.3.14 | E-12 | Ödeme adımına geçiş | E-14 |", StringComparison.Ordinal);
        Assert.NotEqual(Document.Value, changed);

        Assert.Contains(Problems(document: changed), p => p.StartsWith("ToDiffers:3.3.14", StringComparison.Ordinal));
    }

    // KK2: the panel's transitions.
    [Fact]
    public void PanelTransition_DeletedFromTheTable_IsReported()
    {
        Assert.Contains(Problems(document: WithoutRow("3.4.14")), p => p.Contains("RowOnlyInCode: 3.4.14", StringComparison.Ordinal));
    }

    [Fact]
    public void PanelTransition_WhoseLinkLeavesTheCode_IsReported()
    {
        IReadOnlyDictionary<string, string> sources = Changed("Pages/Panel/Members.cshtml", "href=\"@order.Path\"", "href=\"#\"");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("EvidenceMissing:3.4.14", StringComparison.Ordinal));
    }

    // KK3: the entries from outside the site.
    [Fact]
    public void OutsideEntry_DeletedFromTheTable_IsReported()
    {
        Assert.Contains(Problems(document: WithoutRow("3.5.10")), p => p.Contains("RowOnlyInCode: 3.5.10", StringComparison.Ordinal));
    }

    [Fact]
    public void OutsideEntry_WhosePageLeavesTheCode_IsReported()
    {
        string[] routes = [.. Routes.Where(r => !r.StartsWith("/panel/davet", StringComparison.Ordinal))];

        Assert.Contains(Problems(routes: routes), p => p.StartsWith("EvidenceMissing:3.5.10", StringComparison.Ordinal));
    }

    // KK4: the returns.
    [Fact]
    public void ReturnTarget_ChangedInTheDocument_IsReported()
    {
        string changed = Document.Value.Replace("Çıkış yapan yönetici panel girişine iner", "Çıkış yapan yönetici ana sayfaya iner", StringComparison.Ordinal);
        Assert.NotEqual(Document.Value, changed);

        Assert.Contains(Problems(document: changed), p => p.StartsWith("ToDiffers:3.6.4", StringComparison.Ordinal));
    }

    [Fact]
    public void ReturnRedirect_LeavingTheCode_IsReported()
    {
        IReadOnlyDictionary<string, string> sources = Changed("Pages/Panel/SignOut.cshtml.cs", "SessionSchemes.AdminSignInPath", "\"/panel\"");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("EvidenceMissing:3.6.4", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("/urun/koltuk", "/urun/koltuk")]
    [InlineData("/hesap/siparisler", "/hesap/siparisler")]
    [InlineData("/odeme", "/odeme")]
    [InlineData("/giris", "/")]
    [InlineData("/kayit", "/")]
    [InlineData("/sifre-sifirlama/abc", "/")]
    [InlineData("/dogrulama/abc", "/")]
    [InlineData("https://evil.example/x", "/")]
    [InlineData("//evil.example", "/")]
    [InlineData(null, "/")]
    public void SignInReturn_GoesToTheAskedPage_ExceptTheEntryScreens(string? requested, string expected)
    {
        // 04 3.6.1 and 3.6.3: the page the user asked for or came from; an entry screen is the home page.
        Assert.Equal(expected, ReturnPath.AfterSignIn(requested));
    }

    [Theory]
    [InlineData("/panel/siparisler", "/panel/siparisler")]
    [InlineData(null, "/panel")]
    [InlineData("//evil.example", "/panel")]
    public void PanelSignInReturn_GoesToTheAskedScreen_ElseThePanelHome(string? requested, string expected)
    {
        // 04 3.6.1: panel ekranlarında panel girişi; doğrudan açan yönetici E-31'e (the panel home) iner.
        Assert.Equal(expected, ReturnPath.Resolve(requested, ReturnArea.Panel));
    }

    [Theory]
    [InlineData("/urun/koltuk", "/urun/koltuk")]
    [InlineData("/sepet", "/sepet")]
    [InlineData("/hesap/siparisler", "/")]
    [InlineData("/hesap", "/")]
    [InlineData("/panel/siparisler", "/")]
    [InlineData("/siparis/2026-ABC", "/")]
    [InlineData("/siparis/erisim/anahtar", "/siparis/erisim/anahtar")]
    [InlineData(null, "/")]
    public void CustomerSignOut_StaysOnAPageThatNeedsNoSession_ElseGoesHome(string? requested, string expected)
    {
        // 04 3.6.4: oturum gerektirmeyen sayfada kalır, gerektiriyorsa ana sayfaya iner.
        Assert.Equal(expected, ReturnPath.AfterSignOut(requested));
    }

    [Fact]
    public void TheTwoGatesAreSeparate_EachWithItsOwnSignIn()
    {
        // 04 3.6.1: iki kapı ayrıdır; hesap ekranı vitrin girişine, panel ekranı panel girişine götürür.
        Assert.Equal("/giris", SessionSchemes.CustomerSignInPath);
        Assert.Equal("/panel/giris", SessionSchemes.AdminSignInPath);
        Assert.NotEqual(SessionSchemes.CustomerSignInPath, SessionSchemes.AdminSignInPath);
    }

    [Fact]
    public void CheckoutSignIn_ReturnsToThePaymentStep()
    {
        // 04 3.6.2: ödeme adımından girişe giden girişten sonra ödeme adımına döner.
        Assert.Equal("/giris?returnUrl=/odeme", Shopfolio.Pages.Storefront.CheckoutModel.SignInPath);
        Assert.Equal("/odeme", ReturnPath.AfterSignIn("/odeme"));
    }

    // KK5: the five rows between the panel and the storefront, with the return to editing and the draft address of 3.7.
    [Fact]
    public void PanelStorefrontRow_DeletedFromTheTable_IsReported()
    {
        Assert.Contains(Problems(document: WithoutRow("3.7.3")), p => p.Contains("RowOnlyInCode: 3.7.3", StringComparison.Ordinal));
        Assert.Contains(Problems(document: WithoutRow("3.7.5")), p => p.Contains("RowOnlyInCode: 3.7.5", StringComparison.Ordinal));
    }

    [Fact]
    public void BackToEditingLink_LeavingTheCode_IsReported()
    {
        IReadOnlyDictionary<string, string> sources = Changed("Pages/Shared/_DraftBand.cshtml", "href=\"@Model.EditPath\"", "href=\"#\"");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("EvidenceMissing:3.7.4", StringComparison.Ordinal));
    }

    [Fact]
    public void BackToThePanelLink_LeavingTheCode_IsReported()
    {
        IReadOnlyDictionary<string, string> sources = Changed("Pages/Shared/_AdminStrip.cshtml", "href=\"@Model.PanelPath\"", "href=\"#\"");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("EvidenceMissing:3.7.3", StringComparison.Ordinal));
    }

    // KK6: the skeletons.
    [Fact]
    public void StorefrontSkeletonLink_LeavingTheCode_IsReported()
    {
        IReadOnlyDictionary<string, string> sources = Changed("Pages/Shared/_StorefrontFooter.cshtml", "href=\"@footer.ContactPath\"", "href=\"#\"");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("EvidenceMissing:3.1.21", StringComparison.Ordinal));
    }

    [Fact]
    public void PanelSkeletonLink_LeavingTheCode_IsReported()
    {
        IReadOnlyDictionary<string, string> sources = Changed("Pages/Shared/_PanelLayout.cshtml", "href=\"@frame.Admin.AccountPath\"", "href=\"#\"");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("EvidenceMissing:3.2.21", StringComparison.Ordinal));
    }

    [Fact]
    public void PanelMenu_EqualsTheMenuTableOfTheDocument_EntryByEntry()
    {
        Assert.Empty(MenuProblems(Document.Value));
    }

    [Fact]
    public void PanelMenu_WithARenamedEntryInTheDocument_IsReported()
    {
        string changed = Document.Value.Replace("| 3.2.5 | Katalog | Kategoriler | E-34 |", "| 3.2.5 | Katalog | Sınıflar | E-34 |", StringComparison.Ordinal);
        Assert.NotEqual(Document.Value, changed);

        Assert.NotEmpty(MenuProblems(changed));
    }

    [Fact]
    public void PanelMenu_WithAnEntryPointingToAnotherScreen_IsReported()
    {
        string changed = Document.Value.Replace("| 3.2.10 | Üyeler | — | E-40 |", "| 3.2.10 | Üyeler | — | E-36 |", StringComparison.Ordinal);
        Assert.NotEqual(Document.Value, changed);

        Assert.NotEmpty(MenuProblems(changed));
    }

    // KK7: a transition that is not in the table does not enter the code.
    [Fact]
    public void LinkAddedToTheCode_WithoutARow_IsReported()
    {
        IReadOnlyDictionary<string, string> sources = Changed("Pages/Index.cshtml", "@page \"/\"", "@page \"/\"\n<a href=\"/baska-yer\">Başka yer</a>");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("UndocumentedTransition", StringComparison.Ordinal) && p.Contains("/baska-yer", StringComparison.Ordinal));
    }

    [Fact]
    public void RedirectAddedToTheCode_WithoutARow_IsReported()
    {
        IReadOnlyDictionary<string, string> sources = Changed("Pages/Storefront/Cart.cshtml.cs", "FormResults.SeeOther(FramePaths.Cart);", "FormResults.SeeOther(\"/baska-yer\");");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("UndocumentedTransition", StringComparison.Ordinal) && p.Contains("/baska-yer", StringComparison.Ordinal));
    }

    [Fact]
    public void PanelLinkAddedToTheCode_WithoutARow_IsReported()
    {
        IReadOnlyDictionary<string, string> sources = Changed("Pages/Panel/Members.cshtml", "href=\"@order.Path\"", "href=\"@order.Path\"></a><a href=\"@order.OtherPath\"");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("UndocumentedTransition", StringComparison.Ordinal) && p.Contains("@order.OtherPath", StringComparison.Ordinal));
    }

    [Fact]
    public void ExemptSite_LeavingTheCode_IsReported()
    {
        IReadOnlyDictionary<string, string> sources = Changed("Pages/Shared/_Pager.cshtml", "href=\"@Model.NextPath\"", "href=\"#\"");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("DeadExemption", StringComparison.Ordinal));
    }

    // D-1626: the target of a form (the action of a form, a button's formaction), a handler of another screen and the 303 written on a response are sites like a link; the forms that post to the
    // handler of their own screen are none, and the ones the inventory lists stay on their screen.
    [Fact]
    public void FormActionChanged_ToAnotherScreen_IsReported()
    {
        IReadOnlyDictionary<string, string> sources = Changed("Pages/Panel/Orders.cshtml", "action=\"@Model.ListRoot\"", "action=\"/baska-yer\"");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("UndocumentedTransition: FormAction '/baska-yer'", StringComparison.Ordinal));
    }

    [Fact]
    public void ButtonFormAction_ChangedToAnotherScreen_IsReported()
    {
        IReadOnlyDictionary<string, string> sources = Changed("Pages/Panel/Brand.cshtml", "formaction=\"@logoUpload\"", "formaction=\"/baska-yer\"");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("UndocumentedTransition: FormAction '/baska-yer'", StringComparison.Ordinal));
    }

    [Fact]
    public void FormPageTagHelper_ToAnotherScreen_IsReported()
    {
        IReadOnlyDictionary<string, string> sources = Changed("Pages/Storefront/Contact.cshtml", "asp-page-handler=\"SendContactRequest\"", "asp-page=\"/Baska\" asp-page-handler=\"SendContactRequest\"");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("UndocumentedTransition: FormAction '/Baska'", StringComparison.Ordinal));
    }

    [Fact]
    public void HandlerOfAnotherScreen_ByALiteralPath_IsReported()
    {
        IReadOnlyDictionary<string, string> sources = Changed("Pages/Account/Profile.cshtml", "action=\"@(FramePaths.Account)?handler=ChangeName\"", "action=\"/baska-yer?handler=ChangeName\"");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("UndocumentedTransition: FormAction '/baska-yer?handler=ChangeName'", StringComparison.Ordinal));
    }

    [Fact]
    public void HandlerOfAnotherScreen_ThroughALocal_IsReported()
    {
        IReadOnlyDictionary<string, string> sources = Changed("Pages/Panel/Brand.cshtml", "string logoUpload = PanelFramePaths.Brand + \"?handler=UploadImage\";", "string logoUpload = \"/baska-yer?handler=UploadImage\";");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("UndocumentedTransition: FormAction '@logoUpload'", StringComparison.Ordinal));
    }

    [Fact]
    public void FormsPostingToTheirOwnHandler_AreNoSite_ThoseOfTheListsAreExemptSites()
    {
        IReadOnlyList<NavigationSite> forms = [.. NavigationSites.Scan(Sources.Value).Where(s => s.Kind == NavigationSiteKind.FormAction)];

        Assert.DoesNotContain(forms, s => s.Key.Contains("?handler=", StringComparison.Ordinal) || s.File.EndsWith("Account/Profile.cshtml", StringComparison.Ordinal) || s.File.EndsWith("Storefront/Checkout.cshtml", StringComparison.Ordinal));
        Assert.Contains(forms, s => s.File.EndsWith("Panel/Orders.cshtml", StringComparison.Ordinal) && s.Key == "@Model.ListRoot");
        Assert.Contains(forms, s => s.File.EndsWith("Shared/_StorefrontHeader.cshtml", StringComparison.Ordinal) && s.Key == "@search.Path");
    }

    [Fact]
    public void ResponseRedirect_WrittenStraightOnTheResponse_IsASite_AndItsAddressIsTheSecondArgument()
    {
        IReadOnlyDictionary<string, string> sources = Changed("Infrastructure/Errors/UnexpectedErrorLayer.cs", "FormResults.WriteSeeOther(response, resolution.Target ?? \"/\");", "FormResults.WriteSeeOther(response, \"/baska-yer\");");

        Assert.Contains(NavigationSites.Scan(Sources.Value), s => s.Kind == NavigationSiteKind.Redirect && s.File == "Infrastructure/Errors/UnexpectedErrorLayer.cs" && s.Key == "resolution.Target ?? \"/\"");
        IReadOnlyList<string> problems = Problems(sources: sources);
        Assert.Contains(problems, p => p.StartsWith("UndocumentedTransition: Redirect '\"/baska-yer\"'", StringComparison.Ordinal));
        Assert.Contains(problems, p => p.StartsWith("DeadExemption", StringComparison.Ordinal));
    }

    // D-1636: the screens an uncertain outcome goes to are pinned one by one; the recovery redirects that follow them are exempt because they are the error-recovery way (04 2.7.3.3), not because they
    // "return to the screen they came from".
    [Fact]
    public void OutcomeScreens_OfTheCode_AreTheOnesTheInventoryPins()
    {
        Assert.Equal(
            [.. NavigationInventory.OutcomeScreens.Select(o => (o.File, o.Body)).Order()],
            [.. NavigationSites.OutcomeScreens(Sources.Value).Select(o => (o.File, o.Body)).Order()]);
        Assert.All(NavigationInventory.OutcomeScreens, o => Assert.False(string.IsNullOrWhiteSpace(o.Reason)));
    }

    [Theory]
    [InlineData("Pages/Storefront/Checkout.cshtml.cs", "FramePaths.OrderTracking : FramePaths.AccountOrders;", "FramePaths.Cart : FramePaths.Cart;")]
    [InlineData("Pages/Account/Profile.cshtml.cs", "public string OutcomeScreen() => FramePaths.Account;", "public string OutcomeScreen() => FramePaths.Cart;")]
    [InlineData("Pages/Panel/Members.cshtml.cs", "public string OutcomeScreen() => _outcomeAddress;", "public string OutcomeScreen() => PanelFramePaths.Orders;")]
    [InlineData("Pages/Panel/ContentForm.cshtml.cs", "public string OutcomeScreen() => _outcomeScreen;", "public string OutcomeScreen() => FramePaths.Cart;")]
    public void OutcomeScreen_ChangedToAnotherScreen_IsReported(string file, string from, string to)
    {
        IReadOnlyList<string> problems = Problems(sources: Changed(file, from, to));

        Assert.Contains(problems, p => p.StartsWith("OutcomeScreenMissing", StringComparison.Ordinal));
        Assert.Contains(problems, p => p.StartsWith("OutcomeScreenUndocumented", StringComparison.Ordinal));
    }

    [Fact]
    public void OutcomeScreen_OfANewOperation_WithoutAPin_IsReported()
    {
        IReadOnlyDictionary<string, string> sources = Changed("Pages/Panel/Members.cshtml.cs", "public string OutcomeScreen() => _outcomeAddress;", "public string OutcomeScreen() => _outcomeAddress;\n    public string OutcomeScreen() => FramePaths.Cart;");

        IReadOnlyList<string> problems = Problems(sources: sources);

        Assert.Contains(problems, p => p.StartsWith("OutcomeScreenUndocumented", StringComparison.Ordinal) && p.Contains("FramePaths.Cart", StringComparison.Ordinal));
        Assert.Contains(problems, p => p.StartsWith("OutcomeScreenCountDiffers", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("Pages/Panel/ContentForm.cshtml.cs", "_outcomeScreen = PanelFramePaths.CorporateContent;", "_outcomeScreen = PanelFramePaths.Members;")]
    [InlineData("Pages/Panel/Members.cshtml.cs", "private string _outcomeAddress = PanelFramePaths.Members;", "private string _outcomeAddress = PanelFramePaths.Orders;")]
    [InlineData("Infrastructure/Errors/UnexpectedErrorLayer.cs", "string own = context.Request.Path.ToUriComponent();", "string own = \"/baska-yer\";")]
    public void TheLineThatGivesAnOutcomeScreenItsValue_Changed_IsReported(string file, string from, string to)
    {
        Assert.Contains(Problems(sources: Changed(file, from, to)), p => p.StartsWith("EvidenceMissing:Exemption(", StringComparison.Ordinal) || p.StartsWith("EvidenceMissing:OutcomeScreen(", StringComparison.Ordinal));
    }

    // T546 correction 1 (K-1617): a row added to the table that the code does not carry, and a link added to the code that the table does not name, each break the check on their own.
    [Fact]
    public void RowAddedToTheTable_WithoutCode_IsReported()
    {
        string anchor = "| 3.3.39 |";
        int at = Document.Value.IndexOf(anchor, StringComparison.Ordinal);
        Assert.True(at >= 0);
        int lineEnd = Document.Value.IndexOf('\n', at);
        string changed = Document.Value.Insert(lineEnd + 1, "| 3.3.99 | E-01 | Örnek bağlantı | E-02 | — | örnek |\n");

        IReadOnlyList<string> problems = Problems(document: changed);

        Assert.Contains(problems, p => p.Contains("RowOnlyInDocument", StringComparison.Ordinal) && p.Contains("3.3.99", StringComparison.Ordinal));
    }

    [Fact]
    public void RowWithTooFewCells_IsNotPassedOver_ItBreaksTheReader()
    {
        int at = Document.Value.IndexOf("| 3.3.39 |", StringComparison.Ordinal);
        Assert.True(at >= 0);
        string changed = Document.Value.Insert(Document.Value.IndexOf('\n', at) + 1, "| 3.3.99 | E-01 |\n");

        Assert.Throws<InvalidOperationException>(() => NavigationDocument.Rows(changed));
    }

    [Fact]
    public void LinkAddedToAScreenThatHadExemptLinks_WithoutARow_IsReported()
    {
        IReadOnlyDictionary<string, string> sources = Changed("Pages/Account/Deleted.cshtml", "href=\"@FramePaths.Home\"", "href=\"@FramePaths.Home\"></a><a href=\"/baska-yer\"");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("UndocumentedTransition", StringComparison.Ordinal) && p.Contains("/baska-yer", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("3.3.13")]
    [InlineData("3.3.40")]
    [InlineData("3.3.41")]
    [InlineData("3.3.42")]
    [InlineData("3.3.43")]
    [InlineData("3.3.44")]
    [InlineData("3.3.45")]
    [InlineData("3.4.24")]
    [InlineData("3.4.25")]
    public void TheRowsOfTheScreenSpecificationLinks_DeletedFromTheTable_AreReported(string id)
    {
        Assert.Contains(Problems(document: WithoutRow(id)), p => p.Contains("RowOnlyInCode: " + id, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("3.3.13", "Pages/Storefront/Cart.cshtml", "href=\"/@AddressRules.ProductPrefix/@line.ProductSlug\"")]
    [InlineData("3.3.40", "Pages/Account/Deleted.cshtml", "href=\"@FramePaths.OrderTracking\"")]
    [InlineData("3.3.41", "Pages/Orders/Tracking.cshtml", "href=\"@(HttpContext.GetStorefrontFrame()?.Account.SignInPath ?? FramePaths.SignIn)\"")]
    [InlineData("3.3.42", "Pages/Orders/_OrderPage.cshtml", "href=\"@tracking.Href\"")]
    [InlineData("3.3.43", "Pages/Storefront/Register.cshtml", "href=\"@FramePaths.SignIn\"")]
    [InlineData("3.3.44", "Pages/Shared/_IbanField.cshtml", "href=\"@Model.NoticeHref\"")]
    [InlineData("3.3.44", "Pages/Orders/_DefectClaimScreen.cshtml", "href=\"@FramePaths.PrivacyNotice\"")]
    [InlineData("3.3.45", "Pages/Storefront/Contact.cshtml", "href=\"@mapUrl\"")]
    [InlineData("3.4.24", "Pages/Panel/PaymentMethods.cshtml", "href=\"@Shopfolio.Pages.Frame.PanelFramePaths.ShippingAndDefaults\"")]
    [InlineData("3.4.25", "Pages/Panel/OrderDetail.cshtml", "href=\"@link.Href\"")]
    public void TheLinksOfTheScreenSpecificationRows_LeavingTheCode_AreReported(string id, string file, string link)
    {
        IReadOnlyDictionary<string, string> sources = Changed(file, link, "href=\"#\"");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("EvidenceMissing:" + id, StringComparison.Ordinal));
    }

    [Fact]
    public void PrivacyNoticeRow_WithoutTheDefectClaimScreenInItsSources_IsReported()
    {
        // T546 correction 2 (K-1617): 02 3.33.5 and 04 5.19.3 (4) count the defect claim screen (E-19); a row that drops it from "Nereden" while the code draws the link is a difference.
        string changed = Document.Value.Replace("E-18 · E-19 · E-20", "E-18 · E-20", StringComparison.Ordinal);
        Assert.NotEqual(Document.Value, changed);

        Assert.Contains(Problems(document: changed), p => p.StartsWith("FromDiffers:3.3.44", StringComparison.Ordinal));
    }

    // T546 correction 3 (B1; K-1617): the way out of an empty list is proved by the call that gives it to the shared component, never by the component's own link - a change of one screen's exit, or an exit
    // that no row names, breaks the check on that screen.
    [Theory]
    [InlineData("3.4.26", "Pages/Panel/Orders.cshtml.cs", "EmptyStates.WithExit(NoOrdersText, HomeLabel, PanelFramePaths.Home)")]
    [InlineData("3.4.26", "Pages/Panel/Requests.cshtml.cs", "EmptyStates.WithExit(NoRequestsText, HomeLabel, PanelFramePaths.Home)")]
    [InlineData("3.4.26", "Pages/Panel/AuditTrail.cshtml.cs", "EmptyStates.WithExit(NoRowsText, HomeLabel, PanelFramePaths.Home)")]
    public void PanelEmptyListExit_ChangedToAnotherScreen_IsReported(string id, string file, string call)
    {
        // E-36, E-38 and E-52 draw "Ana sayfaya dön" to E-31 (04 3.4.26); the exit goes to the members list instead: the row loses its evidence and the new exit is in no row.
        IReadOnlyList<string> problems = Problems(sources: Changed(file, call, call.Replace("PanelFramePaths.Home", "PanelFramePaths.Members", StringComparison.Ordinal)));

        Assert.Contains(problems, p => p.StartsWith("EvidenceMissing:" + id, StringComparison.Ordinal));
        Assert.Contains(problems, p => p.StartsWith("UndocumentedTransition", StringComparison.Ordinal) && p.Contains("PanelFramePaths.Members", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("Pages/Account/OrderHistory.cshtml.cs", "OrderHistoryTexts.EmptyExitLabel, \"/\"", "3.3.12")]
    [InlineData("Pages/Storefront/Cart.cshtml", "CartTexts.EmptyExitLabel, \"/\"", "3.3.12")]
    [InlineData("Pages/Storefront/FaqPage.cs", "\"Ana sayfaya dön\", FramePaths.Home", "3.3.12")]
    [InlineData("Pages/Storefront/CategoryPage.cs", "\"Ana sayfaya dön\", \"/\"", "3.3.12")]
    [InlineData("Pages/Storefront/SearchPage.cs", "\"Ana sayfaya dön\", \"/\"", "3.3.12")]
    [InlineData("Pages/Content/ContentListPages.cs", "\"Ana sayfaya dön\", \"/\"", "3.3.12")]
    [InlineData("Pages/Panel/Products.cshtml.cs", "NewProductLabel, NewProductPath", "3.4.10")]
    public void EmptyListExit_ChangedToAnotherScreen_IsReported_OnTheScreenThatChangedIt(string file, string tail, string id)
    {
        string to = tail[..tail.LastIndexOf(", ", StringComparison.Ordinal)] + ", \"/baska-yer\"";
        IReadOnlyList<string> problems = Problems(sources: Changed(file, tail, to));

        Assert.Contains(problems, p => p.StartsWith("EvidenceMissing:" + id, StringComparison.Ordinal));
        Assert.Contains(problems, p => p.StartsWith("UndocumentedTransition", StringComparison.Ordinal) && p.Contains("/baska-yer", StringComparison.Ordinal));
    }

    [Fact]
    public void EmptyListExit_AddedToAScreen_WithoutARow_IsReported()
    {
        // A second exit that goes to another screen, built beside the documented one.
        const string call = "EmptyStates.WithExit(NoOrdersText, HomeLabel, PanelFramePaths.Home);";
        IReadOnlyDictionary<string, string> sources = Changed("Pages/Panel/Orders.cshtml.cs", call, call + " _ = EmptyStates.WithExit(NoOrdersText, HomeLabel, PanelFramePaths.Members);");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("UndocumentedTransition", StringComparison.Ordinal) && p.Contains("PanelFramePaths.Members", StringComparison.Ordinal));
    }

    [Fact]
    public void EmptyListExit_OfAnotherKindOfCall_IsAlsoASite()
    {
        // The component's second constructor and the list model's own constructor are sites as well, not only the exit call.
        const string firstRecord = "EmptyStates.WithFirstRecordButton(EmptySentence, NewCouponLabel, PanelFramePaths.Coupons + \"/\" + NewKey)";
        IReadOnlyDictionary<string, string> coupons = Changed("Pages/Panel/Coupons.cshtml.cs", firstRecord, firstRecord.Replace("PanelFramePaths.Coupons + \"/\" + NewKey", "PanelFramePaths.Members", StringComparison.Ordinal));
        IReadOnlyDictionary<string, string> faq = Changed("Pages/Storefront/FaqPage.cs", "\"Ana sayfaya dön\", FramePaths.Home", "\"Ana sayfaya dön\", FramePaths.Cart");

        Assert.Contains(Problems(sources: coupons), p => p.StartsWith("UndocumentedTransition", StringComparison.Ordinal) && p.Contains("PanelFramePaths.Members", StringComparison.Ordinal));
        Assert.Contains(Problems(sources: faq), p => p.StartsWith("UndocumentedTransition", StringComparison.Ordinal) && p.Contains("FramePaths.Cart", StringComparison.Ordinal));
    }

    [Fact]
    public void PanelEmptyListExitRow_DeletedFromTheTable_OrChangedInIt_IsReported()
    {
        Assert.Contains(Problems(document: WithoutRow("3.4.26")), p => p.Contains("RowOnlyInCode: 3.4.26", StringComparison.Ordinal));

        string changed = Document.Value.Replace("| 3.4.26 | E-36 · E-38 · E-52", "| 3.4.26 | E-36 · E-38", StringComparison.Ordinal);
        Assert.NotEqual(Document.Value, changed);
        Assert.Contains(Problems(document: changed), p => p.StartsWith("FromDiffers:3.4.26", StringComparison.Ordinal));
    }

    [Fact]
    public void TheSharedEmptyComponents_AreNoEvidenceOfAnyRow()
    {
        // The component draws what each screen gave it: a row is proved by the call of the screen, so no entry may name the component's file (the weakness of 3.3.12 found in round 2).
        Assert.DoesNotContain(NavigationInventory.Entries.SelectMany(e => e.Evidence), v => v.File.EndsWith("_EmptyRow.cshtml", StringComparison.Ordinal) || v.File.EndsWith("_EmptyState.cshtml", StringComparison.Ordinal));
    }

    [Fact]
    public void TheInventory_HasNoOpenItemAndNoCitedExemption()
    {
        // Equality has no list of known differences: every kind of evidence is a place of the code, and an exemption is a place that stays on its screen.
        Assert.All(NavigationInventory.Entries, e => Assert.NotEmpty(e.Evidence));
        Assert.All(NavigationInventory.Exemptions, x => Assert.False(string.IsNullOrWhiteSpace(x.Reason)));
        Assert.Equal(["Link", "Redirect", "EmptyExit", "FormAction", "Text", "Page"], Enum.GetNames<NavigationEvidenceKind>());
    }

    // T546 correction 4 (B1; K-1617): a link whose address comes from a model, a list or a helper is the same expression whatever it leads to; the row must also carry the line that gives the address its
    // target (a producer, a text that holds the target) - otherwise a changed or deleted target passes.
    [Fact]
    public void ARowWhoseLinkDrawsItsAddressFromAModel_CarriesTheProducerOfTheTarget()
    {
        List<string> without = [];
        // T546 correction 5 (round 4 B1): every row of 04 3 - 3.1, 3.2, 3.3, 3.4, 3.5, 3.6 and 3.7 - not only 3.3 and 3.4.
        foreach (NavigationEntry entry in NavigationInventory.Entries)
        {
            bool fed = entry.Evidence.Any(NavigationInventory.AddressComesFromAModel);
            bool hasTarget = entry.Evidence.Any(v => v.Kind == NavigationEvidenceKind.Text);
            if (fed && !hasTarget)
            {
                without.Add(entry.Id);
            }
        }

        Assert.True(without.Count == 0, "Rows whose link address comes from a model and that carry no target text: " + string.Join(", ", without));
    }

    // T546 correction 4 (B1): one change of the target in the real code for every row of 3.3 and 3.4 - at the producer of the address where the link draws it from a model, else at the link, the redirect or the
    // text that carries the row. Each is reported on the row, or on the site that changed (the new address is in no row; the count of a repeated site differs).
    [Theory]
    [InlineData("3.3.1", "Pages/Lists/ProductCard.cs", "internal const string ProductPathPrefix = \"/urun/\";", "internal const string ProductPathPrefix = \"/baska-yer\";")]
    [InlineData("3.3.2", "Pages/Storefront/HomePage.cs", "new HomeCategoryLink(c.Name, AddressRules.RecordAddress(AddressRules.CategoryPrefix, c.Slug))", "new HomeCategoryLink(c.Name, AddressRules.OrderArea(AddressRules.CategoryPrefix, c.Slug))")]
    [InlineData("3.3.3", "Modules/CorporateContent/HomepageReader.cs", "new HomepageIntro(about.ShortIntroduction, \"/\" + AddressRules.AboutPage,", "new HomepageIntro(about.ShortIntroduction, \"/\" + AddressRules.OrderArea,")]
    [InlineData("3.3.4", "Modules/CorporateContent/HomepageReader.cs", "section.MenuNames.Offerings, \"/\" + AddressRules.ServiceListPage", "section.MenuNames.Offerings, \"/\" + AddressRules.OrderArea")]
    [InlineData("3.3.5", "Pages/Lists/ProductCard.cs", "internal const string ProductPathPrefix = \"/urun/\";", "internal const string ProductPathPrefix = \"/baska-yer\";")]
    [InlineData("3.3.6", "Pages/Storefront/CategoryPage.cs", "new CategoryCrumb(current.Name, CategoryPaths.Of(current.Slug))", "Zew CategoryCrumb(current.Name, CategoryPaths.Of(current.Slug))")]
    [InlineData("3.3.7", "Pages/Product.cshtml.cs", "(VariantSelection.PathFor(productPath, variant)", "(ZariantSelection.PathFor(productPath, variant)")]
    [InlineData("3.3.8", "Modules/CorporateContent/ContentReaders.cs", "AddressRules.RecordAddress(AddressRules.ServicePrefix, o.Slug)", "AddressRules.OrderArea(AddressRules.ServicePrefix, o.Slug)")]
    [InlineData("3.3.9", "Pages/Content/ContentRecordPages.cs", "Kind == ContentPageKind.Offering ? FramePaths.Contact : null", "Kind == ContentPageKind.Offering ? FramePaths.Cart : null")]
    [InlineData("3.3.10", "Pages/Shared/_ContentRecordBody.cshtml", "<partial name=\"_ProductGrid\" model=\"Model.LinkedProducts\" />", "<partial name=\"/baska-yer\" model=\"Model.LinkedProducts\" />")]
    [InlineData("3.3.11", "Pages/Storefront/Contact.cshtml.cs", "(FramePaths.Contact", "(FramePaths.Cart")]
    [InlineData("3.3.12", "Pages/Frame/FramedNotFoundPageRenderer.cs", "new(FramePaths.Home, productLinks)", "new(FramePaths.Cart, productLinks)")]
    [InlineData("3.3.13", "Pages/Storefront/Cart.cshtml", "href=\"/@AddressRules.ProductPrefix/@line.ProductSlug\"", "href=\"/baska-yer\"")]
    [InlineData("3.3.14", "Pages/Storefront/Cart.cshtml", "href=\"@FramePaths.Checkout\"", "href=\"/baska-yer\"")]
    [InlineData("3.3.15", "Pages/Frame/StorefrontFrameBuilder.cs", "new FrameCart(FramePaths.Cart, badge.ItemCount)", "new FrameCart(FramePaths.Contact, badge.ItemCount)")]
    [InlineData("3.3.16", "Pages/Storefront/Checkout.cshtml", "CheckoutModel.LoginTarget", "ZheckoutModel.LoginTarget")]
    [InlineData("3.3.17", "Pages/Storefront/Checkout.cshtml.cs", ": ConfirmationKeys.MemberConfirmationPath(result.OrderNumber!)", ": FramePaths.Cart")]
    [InlineData("3.3.18", "Pages/Orders/ConfirmationScreenModels.cs", "PaymentFrameAddress = FrameAddressOf(answer.FrameAddress);", "ZaymentFrameAddress = FrameAddressOf(answer.FrameAddress);")]
    [InlineData("3.3.19", "Pages/Orders/ConfirmationScreenModels.cs", "public string MemberOrderPath => OrderRoutes.MemberPath(Facts.OrderNumber);", "Zublic string MemberOrderPath => OrderRoutes.MemberPath(Facts.OrderNumber);")]
    [InlineData("3.3.20", "Pages/Account/OrderHistory.cshtml.cs", "OrderRoutes.MemberPath(o.OrderNumber),", "ZrderRoutes.MemberPath(o.OrderNumber),")]
    [InlineData("3.3.21", "Infrastructure/Routing/OrderRoutes.cs", "return \"/\" + AddressRules.OrderArea + \"/\" + AccessSegment + \"/\" + accessKey;", "return \"/\" + AddressRules.AboutPage + \"/\" + AccessSegment + \"/\" + accessKey;")]
    [InlineData("3.3.22", "Pages/Orders/OrderPageModels.cs", "OrderRoutes.ScreenPath(rootPath, \"iptal\")", "OrderRoutes.ScreenPath(rootPath, \"/baska-yer\")")]
    [InlineData("3.3.23", "Pages/Orders/OrderPageModels.cs", "OrderRoutes.ScreenPath(rootPath, \"cayma\")", "OrderRoutes.ScreenPath(rootPath, \"/baska-yer\")")]
    [InlineData("3.3.24", "Pages/Orders/OrderPageModels.cs", "OrderRoutes.ScreenPath(rootPath, \"ayip-talebi\")", "OrderRoutes.ScreenPath(rootPath, \"/baska-yer\")")]
    [InlineData("3.3.25", "Pages/Orders/AfterSalesScreenModels.cs", "public string RootPath => OrderRoots.RootOf(ScreenPath, Screen);", "Zublic string RootPath => OrderRoots.RootOf(ScreenPath, Screen);")]
    [InlineData("3.3.26", "Pages/Storefront/SignIn.cshtml", "href=\"@FramePaths.Register\"", "href=\"/baska-yer\"")]
    [InlineData("3.3.27", "Pages/Storefront/SignIn.cshtml.cs", "(_flow.Start(HttpContext, GoogleFlowKind.SignInOrRegistration, null, Input.ReturnUrl)", "(Zflow.Start(HttpContext, GoogleFlowKind.SignInOrRegistration, null, Input.ReturnUrl)")]
    [InlineData("3.3.28", "Pages/Storefront/Register.cshtml.cs", "(FramePaths.Register + \"?\" + PendingParameter + \"=\" + Uri.EscapeDataString(value)", "(FramePaths.Cart + \"?\" + PendingParameter + \"=\" + Uri.EscapeDataString(value)")]
    [InlineData("3.3.29", "Pages/Components/RegistrationTexts.cs", "SignInNext, FramePaths.SignIn),", "SignInNext, FramePaths.Cart),")]
    [InlineData("3.3.30", "Pages/Storefront/ReauthenticationAddresses.cs", "FramePaths.Reauthentication + \"?\" + OperationParameter + \"=\" + Uri.EscapeDataString(operation.ToString())", "FramePaths.Cart + \"?\" + OperationParameter + \"=\" + Uri.EscapeDataString(operation.ToString())")]
    [InlineData("3.3.31", "Pages/Account/Profile.cshtml.cs", "(FramePaths.AccountDeleted", "(FramePaths.Cart")]
    [InlineData("3.3.32", "Pages/Shared/_AccountNav.cshtml", "(AccountTexts.NavAccount, FramePaths.Account),", "(AccountTexts.NavAccount, FramePaths.Cart),")]
    [InlineData("3.3.33", "Pages/Storefront/EmailRevert.cshtml.cs", "(FramePaths.EmailRevert + \"/\" + Uri.EscapeDataString(key)", "(FramePaths.Cart + \"/\" + Uri.EscapeDataString(key)")]
    [InlineData("3.3.34", "Pages/Components/RegistrationTexts.cs", "\"Kayıt ol\", FramePaths.Register);", "\"Kayıt ol\", FramePaths.Cart);")]
    [InlineData("3.3.35", "Pages/Components/RegistrationTexts.cs", "string accountPath = isMember ? FramePaths.Account : FramePaths.SignIn;", "string accountPath = isMember ? FramePaths.Cart : FramePaths.SignIn;")]
    [InlineData("3.3.36", "Pages/Account/Profile.cshtml", "href=\"@FramePaths.PasswordReset\"", "href=\"/baska-yer\"")]
    [InlineData("3.3.37", "Pages/Storefront/ReauthenticationAddresses.cs", "FramePaths.Reauthentication + \"?\" + OperationParameter + \"=\" + Uri.EscapeDataString(operation.ToString())", "FramePaths.Cart + \"?\" + OperationParameter + \"=\" + Uri.EscapeDataString(operation.ToString())")]
    [InlineData("3.3.38", "Pages/Orders/ConfirmationScreenModels.cs", "(FramePaths.Cart", "(FramePaths.Contact")]
    [InlineData("3.3.39", "Pages/Orders/ConfirmationScreenModels.cs", "IsMemberRoot ? MemberOrderPath : FramePaths.OrderTracking;", "IsMemberRoot ? MemberOrderPath : FramePaths.Cart;")]
    [InlineData("3.3.40", "Pages/Account/Deleted.cshtml", "href=\"@FramePaths.OrderTracking\"", "href=\"/baska-yer\"")]
    [InlineData("3.3.41", "Pages/Orders/Tracking.cshtml", "href=\"@(HttpContext.GetStorefrontFrame()?.Account.SignInPath ?? FramePaths.SignIn)\"", "href=\"/baska-yer\"")]
    [InlineData("3.3.42", "Pages/Orders/_OrderPage.cshtml", "shipping.Link is { } tracking", "Zhipping.Link is { } tracking")]
    [InlineData("3.3.43", "Pages/Storefront/Register.cshtml", "href=\"@FramePaths.SignIn\"", "href=\"/baska-yer\"")]
    [InlineData("3.3.44", "Pages/Components/IbanModels.cs", "customerSide ? FramePaths.PrivacyNotice : null,", "customerSide ? FramePaths.Cart : null,")]
    [InlineData("3.3.45", "Pages/Storefront/Contact.cshtml", "branch.MapUrl is { } mapUrl", "Zranch.MapUrl is { } mapUrl")]
    [InlineData("3.4.1", "Pages/Panel/SignIn.cshtml.cs", "(ReturnPath.Resolve(input.ReturnUrl, ReturnArea.Panel)", "(ZeturnPath.Resolve(input.ReturnUrl, ReturnArea.Panel)")]
    [InlineData("3.4.2", "Pages/Components/PendingCounters.cs", "PanelListTarget.OrdersAwaitingPaymentApproval => OrdersWith(\"AwaitingPayment\"),", "PanelListTarget.OrdersAwaitingPaymentApproval => OrdersWith(\"/baska-yer\"),")]
    [InlineData("3.4.3", "Pages/Panel/Index.cshtml.cs", "missing.Length > 0 ? WarningPathOf(gate) : PanelFramePaths.SalesStateSection", "missing.Length > 0 ? WarningPathOf(gate) : PanelFramePaths.Brand")]
    [InlineData("3.4.4", "Pages/Panel/Index.cshtml.cs", "SetupChecklistItemKind.CompanyIdentity => PanelFramePaths.FirmIdentityAndSales,", "SetupChecklistItemKind.CompanyIdentity => PanelFramePaths.Brand,")]
    [InlineData("3.4.5", "Pages/Panel/Index.cshtml.cs", "PanelFramePaths.Orders + \"/\" + row.OrderId.ToString(\"D\"),", "PanelFramePaths.Brand + \"/\" + row.OrderId.ToString(\"D\"),")]
    [InlineData("3.4.6", "Pages/Panel/Orders.cshtml.cs", "ListReturn.LinkTo($\"{PanelFramePaths.Orders}/{order.OrderId}\", listAddress)", "ListReturn.LinkTo($\"{PanelFramePaths.Brand}/{order.OrderId}\", listAddress)")]
    [InlineData("3.4.7", "Pages/Panel/OrderDetail.cshtml.cs", "$\"{PanelFramePaths.Orders}/{id:D}\"", "$\"{PanelFramePaths.Members}/{id:D}\"")]
    [InlineData("3.4.8", "Pages/Panel/Requests.cshtml.cs", "ListReturn.LinkTo($\"{PanelFramePaths.Requests}/{row.Id}\", listAddress)", "ListReturn.LinkTo($\"{PanelFramePaths.Brand}/{row.Id}\", listAddress)")]
    [InlineData("3.4.9", "Pages/Panel/OrderDetail.cshtml.cs", "BackPath = ListReturn.Target(HttpContext.Request.Query[ReturnPath.ParameterName], PanelFramePaths.Orders);", "BackPath = ListReturn.Target(HttpContext.Request.Query[ReturnPath.ParameterName], PanelFramePaths.Brand);")]
    [InlineData("3.4.10", "Pages/Panel/Products.cshtml.cs", "NewProductPath = ListReturn.LinkTo(PanelFramePaths.Products + \"/yeni\", listAddress);", "NewProductPath = ListReturn.LinkTo(PanelFramePaths.Brand + \"/yeni\", listAddress);")]
    [InlineData("3.4.11", "Pages/Panel/_ProductFormBody.cshtml", "href=\"/@AddressRules.ProductPrefix/@record.Slug\"", "href=\"/baska-yer\"")]
    [InlineData("3.4.12", "Pages/Panel/ContentRoutes.cs", "{Root}/{SegmentOf(type)}/{id:D}", "{Root}/{id:D}")]
    [InlineData("3.4.13", "Modules/CorporateContent/PanelContentEditing.cs", "pagePath = \"/\" + AddressRules.AboutPage;", "pagePath = \"/\" + AddressRules.OrderArea;")]
    [InlineData("3.4.14", "Pages/Panel/Members.cshtml.cs", "ListReturn.LinkTo(PanelFramePaths.Orders + \"/\" + o.OrderId.ToString(), listAddress)", "ListReturn.LinkTo(PanelFramePaths.Brand + \"/\" + o.OrderId.ToString(), listAddress)")]
    [InlineData("3.4.15", "Pages/Panel/FirmIdentity.cshtml.cs", "IdentityMissing(), \"#\" + IdentitySectionId)", "IdentityMissing(), \"/baska-yer\" + IdentitySectionId)")]
    [InlineData("3.4.16", "Pages/Panel/MyAccount.cshtml", "string reauthForEmail = PanelFramePaths.Reauthentication + \"?\" + ReauthModel.OperationParameter + \"=EmailChange\";", "string reauthForEmail = PanelFramePaths.Brand + \"?\" + ReauthModel.OperationParameter + \"=EmailChange\";")]
    [InlineData("3.4.17", "Pages/Panel/AdminAccounts.cshtml.cs", "public const string ListRoot = PanelFramePaths.AdminAccounts;", "public const string ListRoot = PanelFramePaths.Brand;")]
    [InlineData("3.4.18", "Pages/Panel/Invite.cshtml.cs", "(PanelFramePaths.SignIn + \"?\" + SignInModel.AcceptedInvitationParameter + \"=\" + Uri.EscapeDataString(protectedAddress)", "(PanelFramePaths.Brand + \"?\" + SignInModel.AcceptedInvitationParameter + \"=\" + Uri.EscapeDataString(protectedAddress)")]
    [InlineData("3.4.19", "Pages/Panel/ProductForm.cshtml.cs", "$\"{PanelFramePaths.Orders}/{orderId:D}\"", "$\"{PanelFramePaths.Members}/{orderId:D}\"")]
    [InlineData("3.4.20", "Pages/Panel/ProductForm.cshtml.cs", "ReturnPath = ListReturn.Target(returnUrl, PanelFramePaths.Products);", "ReturnPath = ListReturn.Target(returnUrl, PanelFramePaths.Brand);")]
    [InlineData("3.4.21", "Pages/Panel/RequestDetail.cshtml.cs", "SearchOrdersPath = $\"{PanelFramePaths.Orders}?search={encodedEmail}\";", "SearchOrdersPath = $\"{PanelFramePaths.Brand}?search={encodedEmail}\";")]
    [InlineData("3.4.22", "Pages/Panel/LegalTexts.cshtml.cs", "IdentityMissingPrefix + LabelOf(field), PanelFramePaths.FirmIdentityAndSales", "IdentityMissingPrefix + LabelOf(field), PanelFramePaths.Brand")]
    [InlineData("3.4.23", "Pages/Panel/LandingNextStep.cs", "session.Succeeded ? (PanelFramePaths.MyAccount, PanelAccessTexts.ToMyAccountLink) : (PanelFramePaths.SignIn, PanelAccessTexts.ToSignInLink)", "session.Succeeded ? (PanelFramePaths.Brand, PanelAccessTexts.ToMyAccountLink) : (PanelFramePaths.SignIn, PanelAccessTexts.ToSignInLink)")]
    [InlineData("3.4.24", "Pages/Panel/PaymentMethods.cshtml", "href=\"@Shopfolio.Pages.Frame.PanelFramePaths.ShippingAndDefaults\"", "href=\"/baska-yer\"")]
    [InlineData("3.4.25", "Pages/Panel/OrderDetail.cshtml", "display.Link is { } link", "Zisplay.Link is { } link")]
    [InlineData("3.4.26", "Pages/Panel/Orders.cshtml.cs", "PanelFramePaths.Home", "PanelFramePaths.Brand")]
    public void TheTargetOfEveryRow_ChangedInTheCode_IsReported(string id, string file, string from, string to)
    {
        IReadOnlyList<string> problems = Problems(sources: Changed(file, from, to));

        Assert.Contains(problems, p => p.Contains(":" + id + ":", StringComparison.Ordinal) || p.Contains(file, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("Pages/Panel/Index.cshtml.cs", "SetupChecklistItemKind.ReturnAddress => PanelFramePaths.ShippingAndDefaults,", "SetupChecklistItemKind.ReturnAddress => PanelFramePaths.Brand,", "EvidenceMissing:3.4.4")]
    [InlineData("Pages/Panel/Index.cshtml.cs", "SetupChecklistItemKind.PaymentMethod => PanelFramePaths.PaymentMethods,", "SetupChecklistItemKind.PaymentMethod => PanelFramePaths.Members,", "EvidenceMissing:3.4.4")]
    [InlineData("Pages/Panel/Index.cshtml.cs", "\"?pendingWork=EmailUndeliveredMark\"", "\"?pendingWork=Other\"", "EvidenceMissing:3.4.3")]
    [InlineData("Pages/Panel/Index.cshtml.cs", "string.Empty), PanelFramePaths.FirmIdentityAndSales", "string.Empty), PanelFramePaths.Members", "EvidenceMissing:3.4.3")]
    [InlineData("Pages/Shared/_AccountNav.cshtml", "(AccountTexts.NavAddresses, FramePaths.AccountAddresses),", "", "EvidenceMissing:3.3.32")]
    [InlineData("Pages/Shared/_AccountNav.cshtml", "(AccountTexts.NavOrders, FramePaths.AccountOrders),", "(AccountTexts.NavOrders, FramePaths.Account),", "EvidenceMissing:3.3.32")]
    [InlineData("Pages/Components/PendingCounters.cs", "OrdersWith(\"AwaitingShipment\")", "OrdersWith(\"AwaitingDeliveryMark\")", "EvidenceMissing:3.4.2")]
    [InlineData("Pages/Components/PendingCounters.cs", "PanelFramePaths.Requests + \"?requestStatus=Open\"", "PanelFramePaths.Orders + \"?requestStatus=Open\"", "EvidenceMissing:3.4.2")]
    [InlineData("Pages/Panel/FirmIdentity.cshtml.cs", "\"Eksik: açık ödeme yöntemi yok\", PanelFramePaths.PaymentMethods)", "\"Eksik: açık ödeme yöntemi yok\", PanelFramePaths.Members)", "EvidenceMissing:3.4.15")]
    [InlineData("Pages/Components/StateModels.cs", "SalesGateCondition.ReturnAddress => PanelFramePaths.ShippingAndDefaults,", "SalesGateCondition.ReturnAddress => PanelFramePaths.Brand,", "EvidenceMissing:3.4.15")]
    [InlineData("Pages/Panel/LegalTexts.cshtml.cs", "IdentityMissingPrefix + LabelOf(field), PanelFramePaths.FirmIdentityAndSales", "IdentityMissingPrefix + LabelOf(field), PanelFramePaths.Brand", "EvidenceMissing:3.4.22")]
    [InlineData("Pages/Orders/OrderPageModels.cs", "OrderRoutes.ScreenPath(rootPath, \"cayma\")", "OrderRoutes.ScreenPath(rootPath, \"iptal\")", "EvidenceMissing:3.3.23")]
    public void ModelFedAddress_ChangedAtItsProducer_IsReportedOnItsRow(string file, string from, string to, string expected)
    {
        Assert.Contains(Problems(sources: Changed(file, from, to)), p => p.StartsWith(expected, StringComparison.Ordinal));
    }

    // T546 correction 5 (round 4 B1, K1 = D-1641): the producer rule and the target change reach 3.1, 3.2, 3.5, 3.6 and 3.7 as well - the frame builders, the menu reader, the draft calls, the session handler. The
    // assertion is the row itself (EvidenceMissing:<row>) or the changed site (UndocumentedTransition and SiteCountDiffers name the file), never a problem of another place.
    [Theory]
    [InlineData("3.1.1", "Pages/Shared/_StorefrontLayout.cshtml", "href=\"@announcement.LinkUrl\"", "href=\"/baska-yer\"")]
    [InlineData("3.1.2", "Pages/Shared/_StorefrontHeader.cshtml", "<a href=\"/\">", "<a href=\"/baska-yer\">")]
    [InlineData("3.1.3", "Pages/Frame/StorefrontFrameBuilder.cs", "new FrameSearch(FramePaths.Search,", "new FrameSearch(FramePaths.Cart,")]
    [InlineData("3.1.4", "Pages/Frame/StorefrontFrameBuilder.cs", "$\"{FramePaths.SignIn}?{ReturnPath.ParameterName}", "$\"{FramePaths.Cart}?{ReturnPath.ParameterName}")]
    [InlineData("3.1.5", "Pages/Frame/StorefrontFrameBuilder.cs", "FramePaths.OrderTracking,", "FramePaths.Account,")]
    [InlineData("3.1.6", "Pages/Frame/StorefrontFrameBuilder.cs", "FramePaths.Account,", "FramePaths.Cart,")]
    [InlineData("3.1.7", "Pages/Frame/StorefrontFrameBuilder.cs", "FramePaths.AccountOrders,", "FramePaths.Cart,")]
    [InlineData("3.1.8", "Pages/Frame/StorefrontFrameBuilder.cs", "new FrameSignOutForm(FramePaths.SignOut,", "new FrameSignOutForm(FramePaths.Cart,")]
    [InlineData("3.1.9", "Pages/Frame/StorefrontFrameBuilder.cs", "new FrameCart(FramePaths.Cart, badge.ItemCount)", "new FrameCart(FramePaths.Contact, badge.ItemCount)")]
    [InlineData("3.1.10", "Pages/Shared/_StorefrontHeader.cshtml", "<a class=\"site-header__back\" href=\"@Model.Cart.Path\">", "<a class=\"site-header__back\" href=\"/baska-yer\">")]
    [InlineData("3.1.11", "Pages/Frame/FrameMenuTree.cs", "AddressRules.RecordAddress(AddressRules.CategoryPrefix, c.Slug)", "AddressRules.RecordAddress(AddressRules.GeneralPagePrefix, c.Slug)")]
    [InlineData("3.1.12", "Modules/CorporateContent/StorefrontMenuReader.cs", "\"/\" + AddressRules.ServiceListPage", "\"/\" + AddressRules.FaqPage")]
    [InlineData("3.1.13", "Modules/CorporateContent/StorefrontMenuReader.cs", "\"/\" + AddressRules.AboutPage, structure.HasAboutPage", "\"/\" + AddressRules.FaqPage, structure.HasAboutPage")]
    [InlineData("3.1.14", "Modules/CorporateContent/StorefrontMenuReader.cs", "\"/\" + AddressRules.FaqPage, structure.HasFaqItems", "\"/\" + AddressRules.AboutPage, structure.HasFaqItems")]
    [InlineData("3.1.15", "Modules/CorporateContent/StorefrontMenuReader.cs", "AddressRules.RecordAddress(AddressRules.GeneralPagePrefix, p.Slug)", "AddressRules.RecordAddress(AddressRules.CategoryPrefix, p.Slug)")]
    [InlineData("3.1.16", "Modules/CorporateContent/StorefrontMenuReader.cs", "\"/\" + AddressRules.ContactPage", "\"/\" + AddressRules.FaqPage")]
    [InlineData("3.1.17", "Pages/Shared/_StorefrontHeader.cshtml", "<li><a href=\"@link.Url\"", "<li><a href=\"/baska-yer\"")]
    [InlineData("3.1.18", "Pages/Frame/StorefrontFrameBuilder.cs", "etbis, menu.FooterPages, links,", "etbis, [], links,")]
    [InlineData("3.1.19", "Pages/Frame/StorefrontFrameBuilder.cs", "new FrameLegalLink(FramePaths.PrivacyNoticeTitle, FramePaths.PrivacyNotice)", "new FrameLegalLink(FramePaths.PrivacyNoticeTitle, FramePaths.Cart)")]
    [InlineData("3.1.20", "Pages/Shared/_StorefrontFooter.cshtml", "href=\"@etbis\"", "href=\"/baska-yer\"")]
    [InlineData("3.1.21", "Pages/Frame/StorefrontFrameBuilder.cs", "return new FrameFooter(FramePaths.Contact,", "return new FrameFooter(FramePaths.Cart,")]
    [InlineData("3.2.1", "Pages/Frame/PanelFrameBuilder.cs", "Screen(\"Ana sayfa\", PanelFramePaths.Home,", "Screen(\"Ana sayfa\", PanelFramePaths.Brand,")]
    [InlineData("3.2.2", "Pages/Frame/PanelFrameBuilder.cs", "Screen(\"Siparişler\", PanelFramePaths.Orders,", "Screen(\"Siparişler\", PanelFramePaths.Brand,")]
    [InlineData("3.2.3", "Pages/Frame/PanelFrameBuilder.cs", "Screen(\"Talepler\", PanelFramePaths.Requests,", "Screen(\"Talepler\", PanelFramePaths.Brand,")]
    [InlineData("3.2.4", "Pages/Frame/PanelFrameBuilder.cs", "(\"Ürünler\", PanelFramePaths.Products)", "(\"Ürünler\", PanelFramePaths.Brand)")]
    [InlineData("3.2.5", "Pages/Frame/PanelFrameBuilder.cs", "(\"Kategoriler\", PanelFramePaths.Categories)", "(\"Kategoriler\", PanelFramePaths.Brand)")]
    [InlineData("3.2.6", "Pages/Frame/PanelFrameBuilder.cs", "(\"Kuponlar\", PanelFramePaths.Coupons)", "(\"Kuponlar\", PanelFramePaths.Brand)")]
    [InlineData("3.2.7", "Pages/Frame/PanelFrameBuilder.cs", "(\"Kurumsal içerik\", PanelFramePaths.CorporateContent)", "(\"Kurumsal içerik\", PanelFramePaths.Brand)")]
    [InlineData("3.2.8", "Pages/Frame/PanelFrameBuilder.cs", "(\"Ana sayfa ve menü\", PanelFramePaths.HomeAndMenu)", "(\"Ana sayfa ve menü\", PanelFramePaths.Brand)")]
    [InlineData("3.2.9", "Pages/Frame/PanelFrameBuilder.cs", "(\"Marka\", PanelFramePaths.Brand)", "(\"Marka\", PanelFramePaths.Members)")]
    [InlineData("3.2.10", "Pages/Frame/PanelFrameBuilder.cs", "Screen(\"Üyeler\", PanelFramePaths.Members,", "Screen(\"Üyeler\", PanelFramePaths.Brand,")]
    [InlineData("3.2.11", "Pages/Frame/PanelFrameBuilder.cs", "(\"Satış özeti\", PanelFramePaths.SalesSummary)", "(\"Satış özeti\", PanelFramePaths.Brand)")]
    [InlineData("3.2.12", "Pages/Frame/PanelFrameBuilder.cs", "(\"İşlem izi\", PanelFramePaths.OperationTrail)", "(\"İşlem izi\", PanelFramePaths.Brand)")]
    [InlineData("3.2.13", "Pages/Frame/PanelFrameBuilder.cs", "(\"Dışa aktarma\", PanelFramePaths.Export)", "(\"Dışa aktarma\", PanelFramePaths.Brand)")]
    [InlineData("3.2.14", "Pages/Frame/PanelFrameBuilder.cs", "(\"Firma kimliği ve satış\", PanelFramePaths.FirmIdentityAndSales)", "(\"Firma kimliği ve satış\", PanelFramePaths.Brand)")]
    [InlineData("3.2.15", "Pages/Frame/PanelFrameBuilder.cs", "(\"Ödeme yöntemleri\", PanelFramePaths.PaymentMethods)", "(\"Ödeme yöntemleri\", PanelFramePaths.Brand)")]
    [InlineData("3.2.16", "Pages/Frame/PanelFrameBuilder.cs", "(\"Kargo, süreler ve varsayılanlar\", PanelFramePaths.ShippingAndDefaults)", "(\"Kargo, süreler ve varsayılanlar\", PanelFramePaths.Brand)")]
    [InlineData("3.2.17", "Pages/Frame/PanelFrameBuilder.cs", "(\"Yasal metinler\", PanelFramePaths.LegalTexts)", "(\"Yasal metinler\", PanelFramePaths.Brand)")]
    [InlineData("3.2.18", "Pages/Frame/PanelFrameBuilder.cs", "(\"Yönetici hesapları\", PanelFramePaths.AdminAccounts)", "(\"Yönetici hesapları\", PanelFramePaths.Brand)")]
    [InlineData("3.2.19", "Pages/Frame/PanelFrameBuilder.cs", "new PanelSalesIndicator(false, PanelFramePaths.SalesClosedText, PanelFramePaths.SalesStateSection)", "new PanelSalesIndicator(false, PanelFramePaths.SalesClosedText, PanelFramePaths.Brand)")]
    [InlineData("3.2.20", "Pages/Frame/PanelFrameBuilder.cs", "\"/\",\n            new PanelAdminArea(name,", "\"/sepet\",\n            new PanelAdminArea(name,")]
    [InlineData("3.2.21", "Pages/Frame/PanelFrameBuilder.cs", "new PanelAdminArea(name, PanelFramePaths.MyAccount,", "new PanelAdminArea(name, PanelFramePaths.Brand,")]
    [InlineData("3.2.22", "Pages/Frame/PanelFrameBuilder.cs", "new PanelSignOutForm(PanelFramePaths.SignOut)", "new PanelSignOutForm(PanelFramePaths.Home)")]
    [InlineData("3.5.8", "Pages/Payment/Return.cshtml.cs", "OrderPath = OrderRoutes.AccessPath(", "OrderPath = OrderRoutes.MemberPath(")]
    [InlineData("3.5.9", "Pages/Storefront/GoogleReturn.cshtml.cs", "ReturnPath.AfterSignIn(flow.State.ReturnUrl)", "ReturnPath.AfterSignIn(null)")]
    [InlineData("3.5.13", "Pages/Frame/FramedNotFoundPageRenderer.cs", "StatusCode = StatusCodes.Status404NotFound", "StatusCode = StatusCodes.Status200OK")]
    [InlineData("3.5.14", "Infrastructure/Security/SessionAccessSetup.cs", "options.SignInPath = SessionSchemes.AdminSignInPath;", "options.SignInPath = SessionSchemes.CustomerSignInPath;")]
    [InlineData("3.6.1", "Pages/Storefront/SignIn.cshtml.cs", "ReturnPath.AfterSignIn(input.ReturnUrl)", "ReturnPath.AfterSignIn(null)")]
    [InlineData("3.6.1", "Infrastructure/Security/ReturnPath.cs", "public const string PanelHome = \"/panel\";", "public const string PanelHome = \"/panel/marka\";")]
    [InlineData("3.6.2", "Pages/Storefront/Checkout.cshtml.cs", "FramePaths.SignIn + \"?returnUrl=\" + FramePaths.Checkout", "FramePaths.SignIn + \"?returnUrl=\" + FramePaths.Cart")]
    [InlineData("3.6.3", "Pages/Storefront/GoogleReturn.cshtml.cs", "ReturnPath.AfterSignIn(flow.State.ReturnUrl)", "ReturnPath.AfterSignIn(null)")]
    [InlineData("3.6.4", "Infrastructure/Security/SessionAuthenticationHandler.cs", "? SessionSchemes.AdminSignInPath", "? SessionSchemes.CustomerSignInPath")]
    [InlineData("3.6.5", "Pages/Panel/AdminAccounts.cshtml.cs", "PanelFramePaths.SignIn", "PanelFramePaths.Home")]
    [InlineData("3.7.1", "Pages/Frame/PanelFrameBuilder.cs", "\"/\",\n            new PanelAdminArea(name,", "\"/sepet\",\n            new PanelAdminArea(name,")]
    [InlineData("3.7.2", "Modules/CorporateContent/PanelContentEditing.cs", "pagePath = \"/\" + AddressRules.AboutPage;", "pagePath = \"/\" + AddressRules.FaqPage;")]
    [InlineData("3.7.2", "Pages/Panel/_ProductFormBody.cshtml", "href=\"/@AddressRules.ProductPrefix/@record.Slug\"", "href=\"/baska-yer\"")]
    [InlineData("3.7.3", "Pages/Frame/StorefrontFrameBuilder.cs", "isAdministrator ? new FrameAdminStrip(FramePaths.Panel) : null", "isAdministrator ? new FrameAdminStrip(FramePaths.Cart) : null")]
    [InlineData("3.7.4", "Pages/Product.cshtml.cs", "SetDraftPreview($\"{PanelFramePaths.Products}/{draftId:D}\")", "SetDraftPreview($\"{PanelFramePaths.Members}/{draftId:D}\")")]
    [InlineData("3.7.4", "Pages/Content/ContentRecordPages.cs", "SetDraftPreview($\"{PanelFramePaths.CorporateContent}/{PanelType}/{page.RecordId:D}\")", "SetDraftPreview($\"{PanelFramePaths.Members}/{PanelType}/{page.RecordId:D}\")")]
    [InlineData("3.7.4", "Pages/Content/ContentRecordPages.cs", "HttpContext.SetDraftPreview($\"{PanelFramePaths.CorporateContent}/{PanelType}/{page.RecordId:D}\");", "")]
    [InlineData("3.7.4", "Pages/Content/ContentRecordPages.cs", "protected override string PanelType => \"hizmet\";", "protected override string PanelType => \"referans\";")]
    [InlineData("3.7.5", "Modules/Catalog/ProductPageReader.cs", "PublishStatus.Draft && !isAdministrator", "PublishStatus.Archived && !isAdministrator")]
    public void TheTargetOfEveryFrameRow_ChangedInTheCode_IsReportedOnTheRow(string id, string file, string from, string to)
    {
        IReadOnlyList<string> problems = Problems(sources: Changed(file, from, to));

        Assert.Contains(problems, p => p.StartsWith("EvidenceMissing:" + id + ":", StringComparison.Ordinal) || ((p.StartsWith("UndocumentedTransition", StringComparison.Ordinal) || p.StartsWith("SiteCountDiffers", StringComparison.Ordinal)) && p.Contains(file, StringComparison.Ordinal)));
    }

    [Fact]
    public void TheDraftEditAddress_OfBothKindsOfRecord_IsCarriedByTheRowOfTheReturnToEditing()
    {
        // 04 3.7.4: the draft product returns to E-33 (the product's form), the draft content record to E-41 (the record's form): the two calls that hand the band its address are the row's evidence.
        NavigationEntry row = NavigationInventory.Entries.Single(e => e.Id == "3.7.4");

        Assert.Contains(row.Evidence, v => v.Kind == NavigationEvidenceKind.Text && v.File.EndsWith("Product.cshtml.cs", StringComparison.Ordinal) && v.Text.Contains("SetDraftPreview($\"{PanelFramePaths.Products}/", StringComparison.Ordinal));
        Assert.Contains(row.Evidence, v => v.Kind == NavigationEvidenceKind.Text && v.File.EndsWith("ContentRecordPages.cs", StringComparison.Ordinal) && v.Text.Contains("SetDraftPreview($\"{PanelFramePaths.CorporateContent}/", StringComparison.Ordinal));
    }

    [Fact]
    public void AnEntryFromOutside_WhosePageLeavesTheRoutes_IsReportedOnItsRow()
    {
        // 04 3.5: every row whose proof is a page loses it when no page has the route any more (the route is read from the compiled pages).
        foreach (NavigationEntry entry in NavigationInventory.Entries.Where(e => e.Id.StartsWith("3.5.", StringComparison.Ordinal)))
        {
            foreach (NavigationEvidence page in entry.Evidence.Where(v => v.Kind == NavigationEvidenceKind.Page))
            {
                string[] routes = [.. Routes.Where(r => !(page.Text.EndsWith('*') ? r.StartsWith(page.Text[..^1], StringComparison.Ordinal) : string.Equals(r, page.Text, StringComparison.Ordinal)))];

                Assert.Contains(Problems(routes: routes), p => p.StartsWith("EvidenceMissing:" + entry.Id + ":", StringComparison.Ordinal));
            }
        }
    }

    [Fact]
    public void AnExemptSite_WhoseAddressIsAVariable_IsBoundToTheLineThatGivesItsTarget()
    {
        // T546 correction 5 (D-1641): a site that stays on its screen by a variable (a model's property, a local, a list root) has no address in its expression; the line that gives the variable its value
        // is its evidence. The two shared empty-state components draw what each call gives them (each call is a site of its own) and the unexpected-error recovery is D-1636.
        string[] components = ["Pages/Shared/_EmptyRow.cshtml", "Pages/Shared/_EmptyState.cshtml"];
        List<string> without = [];
        foreach (NavigationExemption exemption in NavigationInventory.Exemptions)
        {
            bool isVariable = !exemption.Key.Contains("FramePaths.", StringComparison.Ordinal) && !exemption.Key.Contains('"', StringComparison.Ordinal);
            if (isVariable && !components.Contains(exemption.File, StringComparer.Ordinal) && exemption.Binds.Count == 0)
            {
                without.Add($"{exemption.File} '{exemption.Key}'");
            }
        }

        Assert.True(without.Count == 0, "Exempt sites whose address is a variable and that are bound to no line of the code: " + string.Join("; ", without));
    }

    [Theory]
    [InlineData("Pages/Panel/RequestDetail.cshtml.cs", "$\"{PanelFramePaths.Requests}/{id:D}\";", "$\"{PanelFramePaths.Members}/{id:D}\";")]
    [InlineData("Pages/Orders/ConfirmationScreenModels.cs", "public string ScreenPath => Request.Path.Value ?? string.Empty;", "public string ScreenPath => FramePaths.Cart;")]
    [InlineData("Pages/Orders/OrderPageModels.cs", "public string RootPath => Request.Path.Value ?? string.Empty;", "public string RootPath => FramePaths.Cart;")]
    [InlineData("Pages/Panel/Orders.cshtml.cs", "public string ListRoot => PanelFramePaths.Orders;", "public string ListRoot => PanelFramePaths.Members;")]
    [InlineData("Pages/Panel/Requests.cshtml.cs", "public string ListRoot => PanelFramePaths.Requests;", "public string ListRoot => PanelFramePaths.Members;")]
    [InlineData("Pages/Panel/AuditTrail.cshtml.cs", "public string ListRoot => PanelFramePaths.OperationTrail;", "public string ListRoot => PanelFramePaths.Members;")]
    [InlineData("Pages/Panel/Products.cshtml.cs", "public string ListRoot => PanelFramePaths.Products;", "public string ListRoot => PanelFramePaths.Members;")]
    [InlineData("Pages/Category.cshtml.cs", "string path = CategoryPaths.Of(category.Slug);", "string path = FramePaths.Cart;")]
    [InlineData("Pages/Search.cshtml.cs", "string clearAddress = ListAddress.Of(Frame.FramePaths.Search,", "string clearAddress = ListAddress.Of(Frame.FramePaths.Cart,")]
    [InlineData("Pages/Account/Addresses.cshtml", "string path = FramePaths.AccountAddresses;", "string path = FramePaths.Cart;")]
    [InlineData("Pages/Lists/ListModels.cs", "current < pageCount ? ListAddress.Of(listRoot, query, current + 1) : null,", "current < pageCount ? ListAddress.Of(FramePaths.Cart, query, current + 1) : null,")]
    [InlineData("Pages/Frame/FramedErrorPageRenderer.cs", "storefront ? \"/\" : PanelFramePaths.Home,", "storefront ? \"/\" : PanelFramePaths.Members,")]
    public void TheAddressOfAnExemptSite_ChangedAtTheLineThatGivesIt_IsReported(string file, string from, string to)
    {
        // The exempt sites stay on their screen: the line that gives the variable its value is bound to the exemption, so the new target breaks it (the variable's name does not change).
        Assert.Contains(Problems(sources: Changed(file, from, to)), p => p.StartsWith("EvidenceMissing:Exemption(", StringComparison.Ordinal));
    }

    private static List<string> MenuProblems(string document)
    {
        // The screen each menu row names, as the address the panel gives it (07 2.1.5: /panel/{name of the screen}).
        Dictionary<string, string> addressOfScreen = new(StringComparer.Ordinal)
        {
            ["E-31"] = PanelFramePaths.Home,
            ["E-36"] = PanelFramePaths.Orders,
            ["E-38"] = PanelFramePaths.Requests,
            ["E-32"] = PanelFramePaths.Products,
            ["E-34"] = PanelFramePaths.Categories,
            ["E-35"] = PanelFramePaths.Coupons,
            ["E-41"] = PanelFramePaths.CorporateContent,
            ["E-42"] = PanelFramePaths.HomeAndMenu,
            ["E-43"] = PanelFramePaths.Brand,
            ["E-40"] = PanelFramePaths.Members,
            ["E-51"] = PanelFramePaths.SalesSummary,
            ["E-52"] = PanelFramePaths.OperationTrail,
            ["E-53"] = PanelFramePaths.Export,
            ["E-44"] = PanelFramePaths.FirmIdentityAndSales,
            ["E-45"] = PanelFramePaths.PaymentMethods,
            ["E-46"] = PanelFramePaths.ShippingAndDefaults,
            ["E-47"] = PanelFramePaths.LegalTexts,
            ["E-49"] = PanelFramePaths.AdminAccounts,
        };

        List<string> documented = [];
        foreach (string[] cells in DataModelDocument.Tables(DataModelDocument.Section(document, "### 3.2 "))[0])
        {
            string screen = Regex.Match(cells[3], @"E-\d\d", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)).Value;
            string address = addressOfScreen.TryGetValue(screen, out string? found) ? found : $"unknown screen {screen}";
            documented.Add($"{cells[1]} | {cells[2]} | {address}");
        }

        List<string> built = [];
        foreach (PanelMenuEntry entry in PanelFrameBuilder.MenuOf("/panel", ordersPending: false, requestsPending: false))
        {
            if (entry.Path is not null)
            {
                built.Add($"{entry.Title} | — | {entry.Path}");
            }

            built.AddRange(entry.Items.Select(i => $"{entry.Title} | {i.Title} | {i.Path}"));
        }

        List<string> problems = [.. SetComparison.Compare("04 3.2 menu", documented, built)];
        if (problems.Count == 0 && !documented.SequenceEqual(built, StringComparer.Ordinal))
        {
            problems.Add("04 3.2 menu: the entries are the document's, in another order.");
        }

        return problems;
    }
}
