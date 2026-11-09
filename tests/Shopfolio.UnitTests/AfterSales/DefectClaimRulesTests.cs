using System.Reflection;

using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings;

namespace Shopfolio.UnitTests.AfterSales;

/// <summary>
/// The defect claim's fixed rules (T374 KK1, KK3, KK6, KK9; 02 7.5.1, 7.5.2, 7.5.4; Z-18): the two years are a legal constant and no setting; the claim carries no file;
/// the channel's end sends the customer to the contact form; no warranty period field exists. The checks read the types; nothing is written.
/// </summary>
public sealed class DefectClaimRulesTests
{
    private static readonly DateOnly Delivered = new(2026, 11, 8);

    [Fact]
    public void ThePeriod_IsTheLegalConstantOfTwoYears_AndNoSettingCarriesIt_KK1()
    {
        Assert.Equal(2, AfterSalesPeriodRules.DefectClaimYears);
        Assert.True(typeof(AfterSalesPeriodRules).GetField(nameof(AfterSalesPeriodRules.DefectClaimYears))!.IsLiteral);
        Assert.DoesNotContain(PropertyNames(typeof(CompanySettings)), n => n.Contains("Defect", StringComparison.Ordinal) || n.Contains("Claim", StringComparison.Ordinal));
    }

    [Fact]
    public void TheClaim_CarriesNoFile_OnlyTheUnitsAndTheDescription_KK3()
    {
        string[] fileLike = [.. PropertyNames(typeof(DefectClaim))
            .Where(n => n.Contains("File", StringComparison.Ordinal) || n.Contains("Photo", StringComparison.Ordinal) || n.Contains("Attachment", StringComparison.Ordinal))];
        Assert.Empty(fileLike);
        Assert.Contains(nameof(DefectClaim.Quantity), PropertyNames(typeof(DefectClaim)));
        Assert.Contains(nameof(DefectClaim.Description), PropertyNames(typeof(DefectClaim)));
    }

    [Fact]
    public void TheChannel_Expires_AtTheEndOfZ18_AndTheOrderPageThenSendsToTheContactForm_KK6()
    {
        DateTimeOffset end = AfterSalesPeriodRules.DefectClaimChannelEnd(Delivered);

        Assert.False(AfterSalesPeriodRules.IsDefectClaimChannelExpired(Delivered, end.AddTicks(-1)));
        Assert.True(AfterSalesPeriodRules.IsDefectClaimChannelExpired(Delivered, end));

        // Open and expired are the two sides of one end: the channel is never both, and never neither once the date is there.
        Assert.False(AfterSalesPeriodRules.IsDefectClaimChannelOpen(ProductType.Physical, OrderStatus.Delivered, Delivered, end));
        Assert.True(AfterSalesPeriodRules.IsDefectClaimChannelOpen(ProductType.Physical, OrderStatus.Delivered, Delivered, end.AddTicks(-1)));
    }

    [Fact]
    public void AnItemWithoutTheDeliveryDate_NeverExpires_TheRiskIsTheFirms_KK5_KK6()
    {
        Assert.False(AfterSalesPeriodRules.IsDefectClaimChannelExpired(null, new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public void TheContactForm_HasNoDefectClaimTopic_KK6()
    {
        string[] topics = Enum.GetNames<ContactTopic>();
        Assert.DoesNotContain(topics, t => t.Contains("Defect", StringComparison.Ordinal) || t.Contains("Claim", StringComparison.Ordinal));
    }

    [Fact]
    public void NoWarrantyPeriodField_IsOnTheProductOrItsFile_KK9()
    {
        Type[] types = [typeof(Product), typeof(DigitalFile), typeof(CompanySettings)];
        string[] warranty = [.. types.SelectMany(PropertyNames)
            .Where(n => n.Contains("Warranty", StringComparison.Ordinal) || n.Contains("Guarantee", StringComparison.Ordinal))];
        Assert.Empty(warranty);
    }

    [Fact]
    public void TheRemedyCheck_IsARefusalOrTheClaim_NeverBoth()
    {
        DefectRemedyCheck refused = DefectRemedyCheck.Rejected(ItemRecordRefusal.ClaimNotFound);

        Assert.True(refused.IsRejected);
        Assert.Null(refused.ClaimId);
        Assert.Null(refused.Amount);
    }

    private static IEnumerable<string> PropertyNames(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Select(p => p.Name);
}
