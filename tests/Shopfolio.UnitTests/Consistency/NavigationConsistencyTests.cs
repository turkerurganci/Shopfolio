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
    [InlineData("3.3.45", "Pages/Storefront/Contact.cshtml", "href=\"@mapUrl\"")]
    [InlineData("3.4.24", "Pages/Panel/PaymentMethods.cshtml", "href=\"@Shopfolio.Pages.Frame.PanelFramePaths.ShippingAndDefaults\"")]
    [InlineData("3.4.25", "Pages/Panel/OrderDetail.cshtml", "href=\"@link.Href\"")]
    public void TheLinksOfTheScreenSpecificationRows_LeavingTheCode_AreReported(string id, string file, string link)
    {
        IReadOnlyDictionary<string, string> sources = Changed(file, link, "href=\"#\"");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("EvidenceMissing:" + id, StringComparison.Ordinal));
    }

    [Fact]
    public void TheInventory_HasNoOpenItemAndNoCitedExemption()
    {
        // Equality has no list of known differences: every kind of evidence is a place of the code, and an exemption is a place that stays on its screen.
        Assert.All(NavigationInventory.Entries, e => Assert.NotEmpty(e.Evidence));
        Assert.All(NavigationInventory.Exemptions, x => Assert.False(string.IsNullOrWhiteSpace(x.Reason)));
        Assert.Equal(["Link", "Redirect", "Text", "Page"], Enum.GetNames<NavigationEvidenceKind>());
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
