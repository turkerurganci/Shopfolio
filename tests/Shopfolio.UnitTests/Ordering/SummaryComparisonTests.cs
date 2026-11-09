using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// The comparison of the confirmed summary with the current one (T348 criterion 2, KK2; 02 3.17.2): every difference of the amount, its breakdown or the items is a change, in either
/// direction; an item that left and an item that came are both changes; an equal summary has none; the visibility rule hides exactly the four automatic cancellations (KK8).
/// </summary>
public sealed class SummaryComparisonTests
{
    private static readonly ConfirmationSummaryLine Lamp = new("Lamba", null, null, ProductType.Physical, 1, 1000m, null, null, 0m, 20m, 166.67m, 1000m, null, null);

    [Fact]
    public void AnEqualSummary_HasNoChange()
    {
        Assert.Empty(SummaryComparison.Compare(Summary([Lamp]), Summary([Lamp])));
    }

    [Theory]
    [InlineData(1200)]
    [InlineData(800)]
    public void APriceChange_EitherWay_IsAChange_WithBothValues(int price)
    {
        ConfirmationSummaryLine changed = Lamp with { UnitPrice = price, Payable = price };

        IReadOnlyList<SummaryChange> changes = SummaryComparison.Compare(Summary([Lamp]), Summary([changed], total: price));

        Assert.Contains(new SummaryChange(SummaryChangeKind.UnitPrice, "Lamba", 1000m, price), changes);
        Assert.Contains(changes, c => c.Kind == SummaryChangeKind.Total);
    }

    [Fact]
    public void ADiscountThatBegan_AndACouponShareThatChanged_AreBreakdownChanges()
    {
        ConfirmationSummaryLine discounted = Lamp with { ListPrice = 1000m, DiscountPercentage = 10m };
        ConfirmationSummaryLine couponed = Lamp with { CouponShare = 50m, Payable = 950m };

        Assert.Contains(SummaryComparison.Compare(Summary([Lamp]), Summary([discounted])), c => c.Kind == SummaryChangeKind.Breakdown);
        Assert.Contains(SummaryComparison.Compare(Summary([Lamp]), Summary([couponed])), c => c.Kind == SummaryChangeKind.Breakdown);
    }

    [Fact]
    public void AnItemThatLeft_AndAnItemThatCame_AreBothChanges()
    {
        ConfirmationSummaryLine table = Lamp with { ProductName = "Masa" };

        Assert.Contains(new SummaryChange(SummaryChangeKind.ItemLeft, "Masa", 1, null), SummaryComparison.Compare(Summary([Lamp, table]), Summary([Lamp])));
        Assert.Contains(new SummaryChange(SummaryChangeKind.ItemAdded, "Masa", null, 1), SummaryComparison.Compare(Summary([Lamp]), Summary([Lamp, table])));
        Assert.Contains(SummaryComparison.Compare(Summary([Lamp]), Summary([table])), c => c.Kind == SummaryChangeKind.ItemLeft && c.ProductName == "Lamba");
    }

    [Fact]
    public void AQuantityAShippingFeeOrATotalChange_IsAChange()
    {
        Assert.Contains(SummaryComparison.Compare(Summary([Lamp]), Summary([Lamp with { Quantity = 2 }])), c => c.Kind == SummaryChangeKind.Quantity);
        Assert.Contains(new SummaryChange(SummaryChangeKind.ShippingFee, null, 0m, 50m), SummaryComparison.Compare(Summary([Lamp]), Summary([Lamp], fee: 50m)));
        Assert.Contains(new SummaryChange(SummaryChangeKind.Total, null, 1000m, 1050m), SummaryComparison.Compare(Summary([Lamp]), Summary([Lamp], total: 1050m)));
    }

    [Fact]
    public void Visibility_HidesExactlyTheFourAutomaticCancellations_KK8()
    {
        Assert.Equal(
            [TransitionReason.PaymentWindowExpired, TransitionReason.CardPaymentFailed, TransitionReason.PaymentPageNotOpened, TransitionReason.SameCartReorder],
            OrderVisibility.HidingReasons);
        Assert.DoesNotContain(TransitionReason.Closure, OrderVisibility.HidingReasons);
    }

    private static ConfirmationSummary Summary(IReadOnlyList<ConfirmationSummaryLine> lines, decimal fee = 0m, decimal? total = null) =>
        new(lines, fee, total ?? lines.Sum(l => l.Payable) + fee, PaymentMethod.Card, 2, null, null, null);
}
