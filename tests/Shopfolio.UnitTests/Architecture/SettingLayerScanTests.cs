using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.FileSecrets;
using Shopfolio.Modules.Settings;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the setting layer scans over the real settings classes and settings template (T226 KK1, KK2, KK3; 05 8.4.1, MK-71) and shows on
/// deliberate violation samples that each scan goes red (09 19.2 item 3). The samples are read as text; no test writes a file.
/// </summary>
public sealed class SettingLayerScanTests
{
    [Fact]
    public void SettingsClasses_CarryNoMemberNamedLikeAProductConstantOrAFirmSetting()
    {
        string root = RepositoryRoot.Find();
        HashSet<string> forbidden = ForbiddenMemberNames();
        string[] files = Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !IsBuildOutput(f))
            .Select(f => (File: f, Text: File.ReadAllText(f)))
            .Where(f => SettingLayerScan.DeclaresOptionsClass(f.Text))
            .Select(f => f.File)
            .ToArray();

        // The scan really reads the settings classes: the Tunable design values and the installation settings are among them.
        Assert.Contains(files, f => f.EndsWith("TunableDesignValues.cs", StringComparison.Ordinal));
        Assert.Contains(files, f => f.EndsWith("EmailOptions.cs", StringComparison.Ordinal));
        List<string> findings = [];
        foreach (string file in files)
        {
            foreach ((string rule, int line) in SettingLayerScan.ScanOptionsSource(File.ReadAllText(file), forbidden))
            {
                findings.Add($"{Path.GetRelativePath(root, file)}:{line} {rule}");
            }
        }

        Assert.Empty(findings);
    }

    [Fact]
    public void ForbiddenNames_CoverTheProductConstantsAndTheFirmSettings()
    {
        HashSet<string> forbidden = ForbiddenMemberNames();

        Assert.Contains("CATEGORYLEVELMAX", forbidden);
        Assert.Contains("SHIPPINGFEE", forbidden);
        Assert.DoesNotContain("ID", forbidden);
    }

    [Fact]
    public void OptionsScan_OnViolationSample_ReportsTheRuleOnItsLine()
    {
        IReadOnlyList<(string Rule, int Line)> findings =
            SettingLayerScan.ScanOptionsSource(File.ReadAllText(SamplePath("MixedOptions.cs")), ForbiddenMemberNames());

        Assert.Equal((SettingLayerScan.OptionsMemberNamesAnotherLayer, 6), Assert.Single(findings));
    }

    [Fact]
    public void OptionsScan_OnCleanSample_ReportsNothing()
    {
        Assert.Empty(SettingLayerScan.ScanOptionsSource(File.ReadAllText(SamplePath("CleanOptions.cs")), ForbiddenMemberNames()));
    }

    [Fact]
    public void SettingsTemplate_NamesNoProductConstantAndListsNoSecretSetting()
    {
        string text = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "deploy", ".env.example"));

        Assert.Empty(SettingLayerScan.ScanSettingsTemplate(text, ProductConstantNames(), SecretSettingKeys()));
        Assert.Contains("RateLimit__PageRequestsPerMinute=", text, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("SettingsProductConstant.env", "settings template key names a product constant")]
    [InlineData("SettingsSecretSetting.env", "settings template lists a secret setting")]
    public void TemplateScan_OnViolationSample_ReportsTheRuleOnItsLine(string sample, string rule)
    {
        IReadOnlyList<(string Rule, int Line)> findings =
            SettingLayerScan.ScanSettingsTemplate(File.ReadAllText(SamplePath(sample)), ProductConstantNames(), SecretSettingKeys());

        Assert.Equal((rule, 6), Assert.Single(findings));
    }

    [Fact]
    public void PanelForms_HaveNoFieldForAProductConstant()
    {
        string root = RepositoryRoot.Find();
        string pages = Path.Combine(root, "src", "Shopfolio.Web", "Pages");
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(pages, "*.*", SearchOption.AllDirectories).Where(f => f.EndsWith(".cshtml", StringComparison.Ordinal) || f.EndsWith(".cs", StringComparison.Ordinal)))
        {
            foreach ((string rule, int line) in SettingLayerScan.ScanPanelFormSource(File.ReadAllText(file), ProductConstantNames()))
            {
                findings.Add($"{Path.GetRelativePath(root, file)}:{line} {rule}");
            }
        }

        Assert.Empty(findings);
    }

    [Theory]
    [InlineData("ProductConstantFormField.cshtml", 5)]
    [InlineData("BoundConstantPageModel.cs", 7)]
    public void PanelFormScan_OnViolationSample_ReportsTheRuleOnItsLine(string sample, int line)
    {
        IReadOnlyList<(string Rule, int Line)> findings = SettingLayerScan.ScanPanelFormSource(File.ReadAllText(SamplePath(sample)), ProductConstantNames());

        Assert.Equal((SettingLayerScan.FormFieldNamesProductConstant, line), Assert.Single(findings));
    }

    [Theory]
    [InlineData("<input asp-for=\"@Model.Input.CategoryLevelMax\" />")]
    [InlineData("<input id='CategoryLevelMax' />")]
    [InlineData("[Required, BindProperty] public int CategoryLevelMax { get; set; }")]
    [InlineData("[BindProperty(SupportsGet = true)]\npublic int CategoryLevelMax { get; set; }")]
    public void PanelFormScan_SeesEachWayAFieldIsWritten(string source)
    {
        Assert.Single(SettingLayerScan.ScanPanelFormSource(source, ProductConstantNames()));
    }

    [Fact]
    public void PanelFormScan_OnCleanSample_ReportsNothing_AndAFirmSettingFieldIsNotAConstant()
    {
        Assert.Empty(SettingLayerScan.ScanPanelFormSource(File.ReadAllText(SamplePath("CleanForm.cshtml")), ProductConstantNames()));
    }

    [Fact]
    public void TemplateScan_OnCleanSample_ReportsNothing()
    {
        Assert.Empty(SettingLayerScan.ScanSettingsTemplate(File.ReadAllText(SamplePath("SettingsClean.env")), ProductConstantNames(), SecretSettingKeys()));
    }

    [Fact]
    public void TemplateScan_ListsEverySecretSettingOfTheSecretLayout()
    {
        HashSet<string> keys = SecretSettingKeys();

        Assert.Equal(SecretNames.Settings.Count, keys.Count);
        Assert.Contains("EMAILCREDENTIAL", keys);
        Assert.Contains("LINKKEYDERIVATIONSECRET", keys);
    }

    private static HashSet<string> ProductConstantNames() => SettingLayerScan.ConstantNames(typeof(ProductConstants));

    private static HashSet<string> SecretSettingKeys() =>
        SecretNames.Settings.Select(s => SettingLayerScan.Normalize(s.Key)).ToHashSet(StringComparer.Ordinal);

    // The firm settings are the columns of CompanySettings (02 11.1); Id and CreatedAt are the row's own and are shared by every entity.
    private static HashSet<string> ForbiddenMemberNames()
    {
        HashSet<string> names = ProductConstantNames();
        names.UnionWith(SettingLayerScan.PropertyNames(typeof(CompanySettings), nameof(CompanySettings.Id), nameof(CompanySettings.CreatedAt)));
        return names;
    }

    private static string SamplePath(string fileName)
    {
        return Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "SettingLayers", fileName);
    }

    private static bool IsBuildOutput(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
        || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
}
