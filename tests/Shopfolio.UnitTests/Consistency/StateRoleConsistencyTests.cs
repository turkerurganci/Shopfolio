using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Equality check of the state by role matrix (T547; 04 6.1 to 6.3; 09 19.2 item 3, 21.4 item 1): the fifty-three screens each carry a block (6.2.n = E-n, the panel's 6.3), the sales gate and the gate of the
/// data-collecting entries change only the screens 6.1.5.2 names, the administrator's rows are only in the screens 6.1.5.1 names, the status names are those of <c>02 5</c> without a synonym, every operation of
/// <c>03 1.11</c> is a row's source, and the panel is one role under <c>/panel</c>. The checks that must fail read a changed copy of the document or of the sources, never the files (09 19.2 item 3). Reads
/// repository files only; runs in the Lint step. The page test of the matrix's rows (KK2) is <c>Shopfolio.IntegrationTests</c>' <c>StateRolePageTests</c>.
/// </summary>
[Trait("Consistency", "true")]
public sealed class StateRoleConsistencyTests
{
    private static readonly Lazy<string> Document = new(StateRoleDocument.Load);

    private static readonly Lazy<string> Requirements = new(() => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "02_PRODUCT_REQUIREMENTS.md")).ReplaceLineEndings("\n"));

    private static readonly Lazy<IReadOnlyDictionary<string, string>> Sources = new(() => NavigationSites.Load(RepositoryRoot.Find()));

    private static IReadOnlyList<string> Problems(string? document = null, IReadOnlyDictionary<string, string>? sources = null)
        => StateRoleCheck.Problems(document ?? Document.Value, sources ?? Sources.Value, Requirements.Value);

    private static string Changed(string from, string to)
    {
        string changed = Document.Value.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(Document.Value, changed);
        return changed;
    }

    private static Dictionary<string, string> ChangedSource(string file, string from, string to)
    {
        Dictionary<string, string> copy = new(Sources.Value, StringComparer.Ordinal);
        Assert.Contains(from, copy[file], StringComparison.Ordinal);
        copy[file] = copy[file].Replace(from, to, StringComparison.Ordinal);
        return copy;
    }

    private static string WithoutBlock(string block)
    {
        string changed = Regex.Replace(Document.Value, "^\\*\\*" + Regex.Escape(block) + " E-.*?(?=^\\*\\*6\\.|^### |^\\*Kaynak)", string.Empty, RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));
        Assert.NotEqual(Document.Value, changed);
        return changed;
    }

    [Fact]
    public void StateRoleMatrix_EqualsTheCode_InBothDirections()
    {
        IReadOnlyList<string> problems = Problems();

        Assert.True(problems.Count == 0, string.Join('\n', problems));
    }

    // KK1: a screen's block deleted from the document, a screen dropped from the code and a new page without a screen.
    [Fact]
    public void Block_DeletedFromTheDocument_IsReported()
    {
        IReadOnlyList<string> problems = Problems(document: WithoutBlock("6.2.9"));

        Assert.Contains(problems, p => p.StartsWith("BlockMissing", StringComparison.Ordinal) && p.Contains("E-09", StringComparison.Ordinal));
    }

    [Fact]
    public void PanelBlock_DeletedFromTheDocument_IsReported()
    {
        IReadOnlyList<string> problems = Problems(document: WithoutBlock("6.3.20"));

        Assert.Contains(problems, p => p.StartsWith("BlockMissing", StringComparison.Ordinal) && p.Contains("E-49", StringComparison.Ordinal));
    }

    [Fact]
    public void Block_NumberedAgainstTheInventory_IsReported()
    {
        string changed = Changed("**6.2.9 E-09 — ", "**6.2.19 E-09 — ");

        Assert.Contains(Problems(document: changed), p => p.StartsWith("BlockIdDiffers:E-09", StringComparison.Ordinal));
    }

    [Fact]
    public void Screen_WhosePageLeavesTheCode_IsReported()
    {
        Dictionary<string, string> sources = new(Sources.Value, StringComparer.Ordinal);
        sources.Remove("Pages/Storefront/Faq.cshtml");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("PageMissing:E-09", StringComparison.Ordinal));
    }

    [Fact]
    public void Page_WithoutAScreen_IsReported()
    {
        Dictionary<string, string> sources = new(Sources.Value, StringComparer.Ordinal)
        {
            ["Pages/Storefront/Brochure.cshtml"] = "@page \"/brosur\"\n",
        };

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("PageWithoutScreen: Pages/Storefront/Brochure.cshtml", StringComparison.Ordinal));
    }

    [Fact]
    public void Row_WithAnEmptyCell_IsReported()
    {
        string changed = Changed("| 6.2.9.3 | Ziyaretçi · Müşteri | Yayında soru yok — adres doğrudan açıldı | Boş hâl satırı; menü öğesi görünmez (5.9.8) | Dönüş |", "| 6.2.9.3 | Ziyaretçi · Müşteri | Yayında soru yok — adres doğrudan açıldı | Boş hâl satırı; menü öğesi görünmez (5.9.8) |  |");

        Assert.Contains(Problems(document: changed), p => p.StartsWith("EmptyCell:6.2.9.3", StringComparison.Ordinal));
    }

    // KK2 (the document's half): an operation of 03 1.11 that no row's source names, and the two that do not exist must say so.
    [Fact]
    public void Operation_NoRowNames_IsReported()
    {
        string changed = Regex.Replace(Document.Value, @"(?m)^(\| 6\.3\.9\.\d+ \|.*?)`03 §1\.11\.41`", "$1`03 §1.11.40`", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));
        changed = Regex.Replace(changed, @"§1\.11\.41\b", "§1.11.40", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));

        Assert.Contains(Problems(document: changed), p => p.StartsWith("OperationNotInMatrix:1.11.41", StringComparison.Ordinal));
    }

    [Fact]
    public void AbsentOperation_PresentedAsAButton_IsReported()
    {
        string changed = Changed("iptal ve çıkarma sunulmaz", "iptal ve çıkarma düğmesi");
        changed = changed.Replace("iptal ve cayma yoktur", "iptal ve cayma düğmesi", StringComparison.Ordinal);

        Assert.Contains(Problems(document: changed), p => p.StartsWith("OperationNotInMatrix:1.11.4", StringComparison.Ordinal) && p.Contains("not presented", StringComparison.Ordinal));
    }

    // KK3: the two gates change only the six storefront screens.
    [Fact]
    public void SalesGate_ChangingAnotherScreen_IsReported()
    {
        string changed = Changed("| 6.2.2.2 | Ziyaretçi · Müşteri | Hâl: yayında ürün yok ya da süzgecin sonucu boş |", "| 6.2.2.2 | Ziyaretçi · Müşteri | Hâl: satış kapalı |");

        Assert.Contains(Problems(document: changed), p => p.StartsWith("GateRowsDiffer: the sales gate", StringComparison.Ordinal));
    }

    [Fact]
    public void SalesGate_ReadByAnotherScreensPage_IsReported()
    {
        Dictionary<string, string> sources = new(Sources.Value, StringComparer.Ordinal);
        sources["Pages/Storefront/Faq.cshtml.cs"] += "\n// SalesGateOperation.AddToCart\n";

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("GateReadDiffers: the sales gate", StringComparison.Ordinal));
    }

    [Fact]
    public void DataGate_ChangingAnotherScreen_IsReported()
    {
        string changed = Changed("| 6.2.9.3 | Ziyaretçi · Müşteri | Yayında soru yok — adres doğrudan açıldı |", "| 6.2.9.3 | Ziyaretçi · Müşteri | Hâl: veri toplayan girişlerin kapısı kapalı |");

        Assert.Contains(Problems(document: changed), p => p.StartsWith("GateRowsDiffer: the data gate", StringComparison.Ordinal));
    }

    [Fact]
    public void DataGate_ReadByAnotherScreensPage_IsReported()
    {
        Dictionary<string, string> sources = new(Sources.Value, StringComparer.Ordinal);
        sources["Pages/Storefront/Cart.cshtml.cs"] += "\n// DataCollectionClosed\n";

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("GateReadDiffers: the data gate", StringComparison.Ordinal));
    }

    [Fact]
    public void GateSentence_ChangedInTheDocument_IsReported()
    {
        string changed = Changed("Satış kapısı** vitrinde yalnız E-04, E-12 ve E-13'ü", "Satış kapısı** vitrinde yalnız E-04, E-12, E-13 ve E-14'ü");

        Assert.Contains(Problems(document: changed), p => p.StartsWith("GateRowsDiffer: the sales gate", StringComparison.Ordinal));
    }

    // KK4: a synonym of a status name in the state column.
    [Theory]
    [InlineData("| 6.2.16.6 | Müşteri | Kargoya verildi + Ödendi", "| 6.2.16.6 | Müşteri | Gönderildi + Ödendi")]
    [InlineData("| 6.2.16.5 | Müşteri | Hazırlanıyor + Ödendi", "| 6.2.16.5 | Müşteri | Hazırlanıyor + Ödenmiş")]
    [InlineData("| 6.3.5.3 | Yönetici | Yayında |", "| 6.3.5.3 | Yönetici | Yayınlandı |")]
    [InlineData("| 6.3.11.2 | Yönetici | İletişim talebi Kapatıldı |", "| 6.3.11.2 | Yönetici | İletişim talebi Kapandı |")]
    [InlineData("| 6.2.19.3 | Müşteri | Ayıp talebi Çözüldü — Z-18 içinde |", "| 6.2.19.3 | Müşteri | Ayıp talebi Çözümlendi — Z-18 içinde |")]
    public void SynonymOfAStatusName_IsReported(string from, string to)
    {
        Assert.Contains(Problems(document: Changed(from, to)), p => p.StartsWith("StatusNameDiffers", StringComparison.Ordinal));
    }

    [Fact]
    public void StatusBadgeText_OfTheCode_EqualsTheNameOf02()
    {
        string changed = Requirements.Value.Replace("| Teslim edilemedi | `DeliveryFailed` |", "| Teslim yapılamadı | `DeliveryFailed` |", StringComparison.Ordinal);
        Assert.NotEqual(Requirements.Value, changed);

        IReadOnlyList<string> problems = StateRoleCheck.Problems(Document.Value, Sources.Value, changed);

        Assert.Contains(problems, p => p.StartsWith("StatusNameDiffers", StringComparison.Ordinal) && p.Contains("DeliveryFailed", StringComparison.Ordinal));
    }

    // KK5: the administrator's row boundary.
    [Fact]
    public void AdministratorRow_InAScreenOutsideTheList_IsReported()
    {
        string changed = Changed("| 6.2.9.3 | Ziyaretçi · Müşteri | Yayında soru yok", "| 6.2.9.3 | Ziyaretçi · Müşteri · Yönetici | Yayında soru yok");
        changed = changed.Replace("| 6.2.2.2 | Ziyaretçi · Müşteri |", "| 6.2.2.2 | Ziyaretçi · Müşteri · Yönetici |", StringComparison.Ordinal);

        Assert.Contains(Problems(document: changed), p => p.StartsWith("AdministratorRowOutsideList:E-02", StringComparison.Ordinal));
    }

    [Fact]
    public void AdministratorRow_OfAListedScreenDeleted_IsReported()
    {
        string changed = Changed("| 6.2.9.2 | Yönetici |", "| 6.2.9.2 | Ziyaretçi |");

        Assert.Contains(Problems(document: changed), p => p.StartsWith("AdministratorRowMissing:E-09", StringComparison.Ordinal));
    }

    [Fact]
    public void AdministratorRow_OfE05_ThatShowsSomethingElse_IsReported()
    {
        string changed = Changed("yöneticiye aynı — arşiv önizleme değildir (5.5.7)", "yöneticiye taslak bandı (5.5.7)");

        Assert.Contains(Problems(document: changed), p => p.StartsWith("AdministratorRowChanges:6.2.5.1", StringComparison.Ordinal));
    }

    [Fact]
    public void DraftRead_ByAnotherScreensPage_IsReported()
    {
        Dictionary<string, string> sources = new(Sources.Value, StringComparer.Ordinal);
        sources["Pages/Category.cshtml.cs"] += "\n// HttpContext.SetDraftPreview(\"/panel/x\")\n";

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("DraftReadDiffers", StringComparison.Ordinal));
    }

    [Fact]
    public void DraftRead_LeavingAListedScreen_IsReported()
    {
        IReadOnlyDictionary<string, string> sources = ChangedSource("Pages/Storefront/Faq.cshtml.cs", "asAdministrator", "asVisitor");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("DraftReadDiffers", StringComparison.Ordinal));
    }

    // KK8: the panel is one role and everything of it is under /panel.
    [Fact]
    public void PanelRow_OfAnotherRole_IsReported()
    {
        string changed = Changed("| 6.3.4.1 | Yönetici |", "| 6.3.4.1 | Yönetici · Editör |");

        Assert.Contains(Problems(document: changed), p => p.StartsWith("PanelNotSingleRole:6.3.4.1", StringComparison.Ordinal));
    }

    [Fact]
    public void PanelPage_OutsideTheRoot_IsReported()
    {
        IReadOnlyDictionary<string, string> sources = ChangedSource("Pages/Panel/Brand.cshtml", "@page \"/panel/marka\"", "@page \"/marka\"");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("PanelPageOutsidePanel: Pages/Panel/Brand.cshtml", StringComparison.Ordinal));
    }

    [Fact]
    public void PagePanelRoute_OutsideThePanelFolder_IsReported()
    {
        IReadOnlyDictionary<string, string> sources = ChangedSource("Pages/Storefront/Faq.cshtml", "@page \"/sss\"", "@page \"/panel/sss\"");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("PanelPageOutsidePanel: Pages/Storefront/Faq.cshtml", StringComparison.Ordinal));
    }

    [Fact]
    public void PanelFolder_NotClosedByThePolicy_IsReported()
    {
        IReadOnlyDictionary<string, string> sources = ChangedSource("Infrastructure/Security/SessionAccessSetup.cs", "options.Conventions.AuthorizeFolder(SessionSchemes.PanelFolder, SessionSchemes.AdminPolicy);", "// no panel policy");

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("PanelFolderNotClosed", StringComparison.Ordinal));
    }

    [Fact]
    public void PanelForm_PostingOutsidePanel_IsReported()
    {
        Dictionary<string, string> sources = new(Sources.Value, StringComparer.Ordinal);
        sources["Pages/Panel/Brand.cshtml"] += "\n<form method=\"post\" action=\"/hesap\"></form>\n";

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("PanelFormOutsidePanel: Pages/Panel/Brand.cshtml", StringComparison.Ordinal));
    }

    [Fact]
    public void CustomerForm_PostingToThePanel_IsReported()
    {
        Dictionary<string, string> sources = new(Sources.Value, StringComparer.Ordinal);
        sources["Pages/Storefront/Faq.cshtml"] += "\n<form method=\"post\" action=\"/panel/siparisler\"></form>\n";

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("PanelFormOutsidePanel: Pages/Storefront/Faq.cshtml", StringComparison.Ordinal));
    }

    [Fact]
    public void Document_HasBothFamiliesOfBlocks_AndTheInventoryHasThem()
    {
        IReadOnlyList<ScreenBlock> blocks = StateRoleDocument.Blocks(Document.Value);

        Assert.Equal(blocks.Count, StateRoleInventory.Screens.Count);
        Assert.Equal(blocks.Select(b => b.Block), StateRoleInventory.Screens.Select(s => s.Block));
    }
}
