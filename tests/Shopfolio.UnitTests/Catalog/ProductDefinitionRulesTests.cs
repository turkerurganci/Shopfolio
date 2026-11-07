using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.UnitTests.Catalog;

/// <summary>
/// The type-dependent rules of the product definition as pure functions (T332; 02 3.2.2, 3.2.3, 7.3.4; 06 4.5; 11 T332 criteria 2, 9, 10; KK2, KK9, KK10): the withdrawal exemption is
/// only for a physical product and never enters the gate; a published service needs a fulfilment period of at least one day and has no default; a draft service, a physical
/// product and a digital product need none. The integration tests run the same rules through the save guard on a real database.
/// </summary>
public sealed class ProductDefinitionRulesTests
{
    private static readonly int?[] Periods = [null, -5, 0, 1, 2, 30];

    private static readonly WithdrawalExemptionReason?[] Reasons = [null, .. Enum.GetValues<WithdrawalExemptionReason>().Select(r => (WithdrawalExemptionReason?)r)];

    [Fact]
    public void EveryCombinationOfTypeStatusReasonAndPeriod_BreaksExactlyTheRulesOfTheDocument()
    {
        foreach (ProductType type in Enum.GetValues<ProductType>())
        {
            foreach (PublishStatus status in Enum.GetValues<PublishStatus>())
            {
                foreach (WithdrawalExemptionReason? reason in Reasons)
                {
                    foreach (int? period in Periods)
                    {
                        List<ProductDefinitionProblem> expected = [];
                        if (reason is not null && type != ProductType.Physical)
                        {
                            expected.Add(ProductDefinitionProblem.ExemptionOutsideAPhysicalProduct);
                        }

                        if (type == ProductType.Service && status == PublishStatus.Published && (period is null || period < 1))
                        {
                            expected.Add(ProductDefinitionProblem.ServiceWithoutFulfilmentPeriodIsPublished);
                        }

                        Assert.Equal(expected, ProductDefinitionRules.Check(type, status, reason, period));
                    }
                }
            }
        }
    }

    [Fact]
    public void TheExemption_NeverEntersTheGate_APhysicalProductIsFreeToCarryItOrNot()
    {
        foreach (WithdrawalExemptionReason? reason in Reasons)
        {
            Assert.Empty(ProductDefinitionRules.Check(ProductType.Physical, PublishStatus.Published, reason, null));
            Assert.Empty(ProductDefinitionRules.Check(ProductType.Physical, PublishStatus.Draft, reason, null));
            Assert.Empty(ProductDefinitionRules.Check(ProductType.Physical, PublishStatus.Archived, reason, 7));
        }
    }

    [Fact]
    public void AServiceNeedsItsFulfilmentPeriodOnlyToBePublished_AndAnArchivedOrDraftServiceNeedsNone()
    {
        Assert.Equal(
            [ProductDefinitionProblem.ServiceWithoutFulfilmentPeriodIsPublished],
            ProductDefinitionRules.Check(ProductType.Service, PublishStatus.Published, null, null));
        Assert.Empty(ProductDefinitionRules.Check(ProductType.Service, PublishStatus.Draft, null, null));
        Assert.Empty(ProductDefinitionRules.Check(ProductType.Service, PublishStatus.Archived, null, null));
        Assert.Empty(ProductDefinitionRules.Check(ProductType.Service, PublishStatus.Published, null, 1));
        Assert.Empty(ProductDefinitionRules.Check(ProductType.Digital, PublishStatus.Published, null, null));
    }

    [Fact]
    public void TheFulfilmentPeriod_IsEnteredFromTheLowerFenceUp_AndHasNoDefault()
    {
        Assert.False(ProductDefinitionRules.IsFulfilmentPeriodEntered(null));
        Assert.False(ProductDefinitionRules.IsFulfilmentPeriodEntered(ParameterFences.FulfilmentPeriodLower - 1));
        Assert.True(ProductDefinitionRules.IsFulfilmentPeriodEntered(ParameterFences.FulfilmentPeriodLower));
        Assert.True(ProductDefinitionRules.IsFulfilmentPeriodEntered(ParameterFences.FulfilmentPeriodLower + 1));
        Assert.True(ProductDefinitionRules.IsFulfilmentPeriodEntered(int.MaxValue));
        Assert.Null(new Product().FulfilmentPeriod);
    }

    [Fact]
    public void TheType_IsOneOfThreeClosedValues_AndANewProductIsADraftWithNoExemption()
    {
        Assert.Equal([ProductType.Physical, ProductType.Digital, ProductType.Service], Enum.GetValues<ProductType>());
        Assert.False(Enum.IsDefined((ProductType)Enum.GetValues<ProductType>().Length));

        Product product = Product.Create("Ad", "ad", ProductType.Service, 20m, DateTimeOffset.UnixEpoch);

        Assert.Equal(PublishStatus.Draft, product.PublishStatus);
        Assert.Null(product.WithdrawalExemptionReason);
        Assert.Null(product.FulfilmentPeriod);
    }
}
