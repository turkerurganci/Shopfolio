using System.Reflection;

using Microsoft.Extensions.Configuration;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.UnitTests.Architecture;

namespace Shopfolio.UnitTests.InstallationSettings;

/// <summary>
/// The three parameter layers as code (T381; 02 10.1.3, 11; KK1-KK3, KK6-KK8): the assignment criterion reads the layer of a new parameter, the firm settings have their
/// fences in one place, the installation settings and the product constants are not firm settings and have no panel field, and P-2 and P-11 stay removed.
/// </summary>
public sealed class ParameterLayersTests
{
    // ---- KK1 and KK2: the layers and the assignment criterion ----

    [Theory]
    [InlineData(false, ParameterHarm.None, true, ParameterLayer.CompanySetting)]
    [InlineData(false, ParameterHarm.None, false, ParameterLayer.ProductConstant)]
    [InlineData(false, ParameterHarm.Security, true, ParameterLayer.ProductConstant)]
    [InlineData(false, ParameterHarm.LegalCompliance, true, ParameterLayer.ProductConstant)]
    [InlineData(false, ParameterHarm.Customer, true, ParameterLayer.ProductConstant)]
    [InlineData(false, ParameterHarm.Customer | ParameterHarm.LegalCompliance, true, ParameterLayer.ProductConstant)]
    [InlineData(true, ParameterHarm.None, true, ParameterLayer.InstallationSetting)]
    [InlineData(true, ParameterHarm.Security, false, ParameterLayer.InstallationSetting)]
    public void ANewParameter_TakesTheLayerTheCriterionGives(bool fromInstallation, ParameterHarm harm, bool isBusinessDecision, ParameterLayer expected)
    {
        Assert.Equal(expected, ParameterLayers.Assign(fromInstallation, harm, isBusinessDecision));
    }

    [Fact]
    public void TheExistingParameters_AreWhatTheCriterionWouldAssign()
    {
        // A firm setting harms only the company and is a business decision (the shipping fee, the periods); the return of goods in the legal period and a password length touch the
        // customer, the law and security - product constants.
        Assert.Equal(ParameterLayer.CompanySetting, ParameterLayers.Assign(false, ParameterHarm.None, true));
        Assert.Equal(ParameterLayer.ProductConstant, ParameterLayers.Assign(false, ParameterHarm.Security, false));
        Assert.Equal(ParameterLayer.ProductConstant, ParameterLayers.Assign(false, ParameterHarm.LegalCompliance, false));
    }

    [Fact]
    public void TheInventory_HasTheThreeLayers_AndTheRemovedRowsAreNotInAny()
    {
        Assert.Equal(10, ParameterInventory.Entries.Count(e => e.Layer == ParameterLayer.CompanySetting));
        Assert.Equal(37, ParameterInventory.Entries.Count(e => e.Layer == ParameterLayer.ProductConstant));
        Assert.Equal(5, ParameterInventory.InstallationSettings.Count);
        Assert.All(ParameterInventory.Entries.Where(e => e.IsRemoved), e => Assert.Empty(e.Constants));
    }

    // ---- KK8: P-2 and P-11 are not in the inventory as parameters and their ids are not reused ----

    [Theory]
    [InlineData("P-2")]
    [InlineData("P-11")]
    public void ARemovedId_IsMarkedRemoved_NamesNoConstantAndIsNotAFirmSetting(string id)
    {
        ParameterEntry entry = ParameterInventory.Of(id);

        Assert.True(entry.IsRemoved);
        Assert.Null(entry.Layer);
        Assert.Throws<ArgumentException>(() => FirmSettingFences.BoundsOf(id));
        Assert.DoesNotContain(ParameterInventory.Entries.SelectMany(e => e.Constants), c => c.Name.Contains(id.Replace("-", string.Empty, StringComparison.Ordinal), StringComparison.Ordinal));
    }

    [Fact]
    public void AnIdOutsideTheInventory_IsAProgrammingError()
    {
        Assert.Throws<ArgumentException>(() => ParameterInventory.Of("P-50"));
        Assert.Throws<ArgumentException>(() => ParameterInventory.Of("P-0"));
    }

    // ---- KK6: the fences ----

    [Theory]
    [InlineData("P-5", 0, FenceSide.Within, 0)]
    [InlineData("P-5", 10, FenceSide.Within, 0)]
    [InlineData("P-5", 11, FenceSide.AboveUpper, 10)]
    [InlineData("P-5", -1, FenceSide.BelowLower, 0)]
    [InlineData("P-6", 10, FenceSide.Within, 0)]
    [InlineData("P-6", 11, FenceSide.AboveUpper, 10)]
    [InlineData("P-7", 1, FenceSide.Within, 0)]
    [InlineData("P-7", 3, FenceSide.Within, 0)]
    [InlineData("P-7", 0, FenceSide.BelowLower, 1)]
    [InlineData("P-7", 4, FenceSide.AboveUpper, 3)]
    [InlineData("P-8", 1, FenceSide.Within, 0)]
    [InlineData("P-8", 1000, FenceSide.Within, 0)]
    [InlineData("P-8", 0, FenceSide.BelowLower, 1)]
    [InlineData("P-9", 1, FenceSide.Within, 0)]
    [InlineData("P-9", 0, FenceSide.BelowLower, 1)]
    [InlineData("P-44", 1, FenceSide.Within, 0)]
    [InlineData("P-44", 0, FenceSide.BelowLower, 1)]
    [InlineData("P-1", 999, FenceSide.Within, 0)]
    [InlineData("P-10", 99, FenceSide.Within, 0)]
    public void AValue_IsCheckedAgainstTheFenceOfItsSetting_AndTheVerdictSaysTheSideAndTheFence(string id, int value, FenceSide side, int fence)
    {
        PeriodFences.Verdict verdict = FirmSettingFences.Check(id, value);

        Assert.Equal(side, verdict.Side);
        Assert.Equal(fence, verdict.Fence);
        Assert.Equal(side == FenceSide.Within, verdict.IsWithin);
    }

    [Fact]
    public void EveryFirmSettingHasBounds_AndNoOtherIdDoes()
    {
        string[] firm = ["P-1", "P-3", "P-4", "P-5", "P-6", "P-7", "P-8", "P-9", "P-10", "P-44"];

        Assert.Equal(firm.Order(StringComparer.Ordinal), ParameterInventory.Entries.Where(e => e.Layer == ParameterLayer.CompanySetting).Select(e => e.Id).Order(StringComparer.Ordinal));
        Assert.All(firm, id => FirmSettingFences.BoundsOf(id));
        Assert.Throws<ArgumentException>(() => FirmSettingFences.BoundsOf("P-12"));
        Assert.Throws<ArgumentException>(() => FirmSettingFences.Check("P-12", 1));
    }

    // ---- KK3 and KK7: the installation settings and the product constants have no field of the firm settings ----

    [Fact]
    public void TheFirmSettingInputsAndSections_CarryNoInstallationSettingAndNoProductConstant()
    {
        HashSet<string> forbidden = SettingLayerScan.ConstantNames(typeof(ProductConstants));
        foreach (InstallationSettingEntry setting in ParameterInventory.InstallationSettings)
        {
            forbidden.Add(SettingLayerScan.Normalize(setting.Name));
            forbidden.UnionWith(setting.Keys.Select(k => SettingLayerScan.Normalize(k[(k.LastIndexOf(':') + 1)..])));
        }

        Type[] records =
        [
            typeof(ShippingFeeInput), typeof(ShippingFeeSection), typeof(PeriodsInput), typeof(PeriodsSection), typeof(ProductDefaultsInput), typeof(ProductDefaultsSection),
            typeof(CompanySettings),
        ];
        foreach (Type record in records)
        {
            string[] names = [.. record.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => SettingLayerScan.Normalize(p.Name))];
            Assert.DoesNotContain(names, forbidden.Contains);
        }
    }

    [Fact]
    public void TheSettingsModulesSurface_TakesNoInstallationSettingAndNoProductConstant()
    {
        HashSet<string> forbidden = SettingLayerScan.ConstantNames(typeof(ProductConstants));
        foreach (InstallationSettingEntry setting in ParameterInventory.InstallationSettings)
        {
            forbidden.UnionWith(setting.Keys.Select(k => SettingLayerScan.Normalize(k[(k.LastIndexOf(':') + 1)..])));
        }

        foreach (MethodInfo method in typeof(ISettingsModule).GetMethods())
        {
            Assert.DoesNotContain(method.GetParameters().Select(p => SettingLayerScan.Normalize(p.Name!)), forbidden.Contains);
            Assert.DoesNotContain(SettingLayerScan.Normalize(method.Name), forbidden);
        }
    }

    [Fact]
    public void TheInstallationSettingKeys_AreTheOnesTheApplicationReads_AndNoneNamesAProductConstant()
    {
        string[] keys = [.. ParameterInventory.InstallationSettings.SelectMany(s => s.Keys)];

        Assert.Contains(SiteOptions.DomainKey, keys);
        Assert.Contains(EmailOptions.SenderAddressKey, keys);
        Assert.Contains(EmailOptions.CredentialKey, keys);
        Assert.Contains(PaymentOptions.ProviderKeysKey, keys);
        Assert.Contains(GoogleOptions.ClientIdKey, keys);
        Assert.Contains(GoogleOptions.ClientSecretKey, keys);
        Assert.Equal(keys.Length, keys.Distinct(StringComparer.Ordinal).Count());
        Assert.Empty(ProductConstantNameGuardKeys(keys));
    }

    private static IReadOnlyList<string> ProductConstantNameGuardKeys(string[] keys)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(keys.Select(k => new KeyValuePair<string, string?>(k, "x")))
            .Build();
        return ProductConstantNameGuard.FindKeys(configuration);
    }
}
