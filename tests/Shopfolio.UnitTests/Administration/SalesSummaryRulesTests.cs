using System.Reflection;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.UnitTests.Administration;

/// <summary>
/// The sales summary's pure rules (T383 KK1, KK5, KK7, KK8, KK9; 02 10.6.2, 10.6.3; 04 9.22.3-9.22.6): the period choice, the payment completion attempts, the shipping promise
/// compliance, the cancellation and return ratio, the zero denominator and the best sellers; and the summary's surface keeps nothing, has no export and reads no request.
/// The records' side - what the modules read from the database - is the integration tests' (SalesSummaryTests).
/// </summary>
public sealed class SalesSummaryRulesTests
{
    private static readonly DateOnly Day = new(2026, 11, 10);
    private static readonly DateTimeOffset From = TurkeyTime.StartOf(Day);
    private static readonly DateTimeOffset To = TurkeyTime.EndOfRange(Day);
    private static readonly DateTimeOffset Now = To.AddDays(5);

    // ---- The period choice (04 9.22.3; K-1215) ----

    [Fact]
    public void TheReadyPeriods_AreTurkeyDays_AsTheScreenNamesThem()
    {
        DateOnly today = new(2026, 3, 15);

        Assert.Equal((today, today), Days(SalesSummaryPeriod.Of(SalesSummaryPreset.Today, today, null)));
        Assert.Equal((today.AddDays(-1), today.AddDays(-1)), Days(SalesSummaryPeriod.Of(SalesSummaryPreset.Yesterday, today, null)));
        Assert.Equal((today.AddDays(-6), today), Days(SalesSummaryPeriod.Of(SalesSummaryPreset.Last7Days, today, null)));
        Assert.Equal((today.AddDays(-29), today), Days(SalesSummaryPeriod.Of(SalesSummaryPreset.Last30Days, today, null)));
        Assert.Equal((new DateOnly(2026, 3, 1), today), Days(SalesSummaryPeriod.Of(SalesSummaryPreset.ThisMonth, today, null)));
        Assert.Equal((new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28)), Days(SalesSummaryPeriod.Of(SalesSummaryPreset.LastMonth, today, null)));
        Assert.Equal((new DateOnly(2025, 12, 1), new DateOnly(2025, 12, 31)), Days(SalesSummaryPeriod.Of(SalesSummaryPreset.LastMonth, new DateOnly(2026, 1, 5), null)));
    }

    [Fact]
    public void TheMeasurementWindow_IsThreeCalendarMonthsLessADay_CutToToday_AndAbsentWhenSalesNeverOpened()
    {
        DateOnly open = new(2026, 1, 31);

        Assert.Null(SalesSummaryPeriod.Of(SalesSummaryPreset.MeasurementWindow, new DateOnly(2026, 6, 1), null));
        Assert.Equal((open, new DateOnly(2026, 4, 29)), Days(SalesSummaryPeriod.Of(SalesSummaryPreset.MeasurementWindow, new DateOnly(2026, 6, 1), open)));
        Assert.Equal((open, new DateOnly(2026, 2, 20)), Days(SalesSummaryPeriod.Of(SalesSummaryPreset.MeasurementWindow, new DateOnly(2026, 2, 20), open)));
    }

    [Fact]
    public void TheCustomRange_NeedsBothDates_NoFutureDate_AndNoEndBeforeStart()
    {
        DateOnly today = new(2026, 3, 15);

        Assert.Equal(SalesSummaryPeriodRefusal.DateMissing, SalesSummaryPeriod.Custom(null, today, today).Refusal);
        Assert.Equal(SalesSummaryPeriodRefusal.DateMissing, SalesSummaryPeriod.Custom(today, null, today).Refusal);
        Assert.Equal(SalesSummaryPeriodRefusal.FutureDate, SalesSummaryPeriod.Custom(today, today.AddDays(1), today).Refusal);
        Assert.Equal(SalesSummaryPeriodRefusal.EndBeforeStart, SalesSummaryPeriod.Custom(today, today.AddDays(-1), today).Refusal);

        (SalesSummaryPeriod? period, SalesSummaryPeriodRefusal? refusal) = SalesSummaryPeriod.Custom(today.AddDays(-3), today.AddDays(-3), today);
        Assert.Null(refusal);
        Assert.True(period!.IsSingleDay);
        Assert.Equal(SalesSummaryPeriod.OfDay(today.AddDays(-3)), period);
    }

    // ---- KK5: one attempt per cart until it is paid; the cart's first order's period; an open window waits ----

    [Fact]
    public void TheOrdersOfOneCart_UntilOneIsPaid_AreOneAttempt_KK5()
    {
        Guid cart = Guid.NewGuid();

        // A failed payment and its retry with a new order, then the paid one: one attempt, completed.
        SummaryRatio ratio = SalesSummaryRules.PaymentCompletion(
            [
                Attempt(cart, From.AddHours(1), PaymentStatus.Failed),
                Attempt(cart, From.AddHours(2), PaymentStatus.Failed),
                Attempt(cart, From.AddHours(3), PaymentStatus.Paid),
            ],
            From,
            To,
            Now);

        Assert.Equal(new SummaryRatio(1, 1), ratio);
    }

    [Fact]
    public void AnOrderAfterAPaidOne_StartsANewAttempt_AndACartlessOrderIsAnAttemptOfItsOwn_KK5()
    {
        Guid cart = Guid.NewGuid();

        SummaryRatio ratio = SalesSummaryRules.PaymentCompletion(
            [
                Attempt(cart, From.AddHours(1), PaymentStatus.Paid),
                Attempt(cart, From.AddHours(2), PaymentStatus.Failed),
                Attempt(null, From.AddHours(3), PaymentStatus.Failed),
                Attempt(null, From.AddHours(4), PaymentStatus.Refunded),
            ],
            From,
            To,
            Now);

        Assert.Equal(new SummaryRatio(2, 4), ratio);
    }

    [Fact]
    public void TheAttempt_BelongsToItsFirstOrdersPeriod_AndWaitsWhileAPaymentWindowIsOpen_KK5()
    {
        Guid earlier = Guid.NewGuid();
        Guid waiting = Guid.NewGuid();
        PaymentAttemptOrder open = Attempt(waiting, From.AddHours(5), PaymentStatus.Pending) with { PaymentWindowEndsAt = Now.AddHours(1) };

        // The cart whose first order was the day before is that day's attempt, paid here or not; the open window keeps its cart out until it runs out.
        PaymentAttemptOrder[] orders = [Attempt(earlier, From.AddHours(-2), PaymentStatus.Failed), Attempt(earlier, From.AddHours(1), PaymentStatus.Paid), open];

        Assert.Equal(new SummaryRatio(0, 0), SalesSummaryRules.PaymentCompletion(orders, From, To, Now));
        Assert.Equal(new SummaryRatio(1, 1), SalesSummaryRules.PaymentCompletion(orders, From.AddDays(-1), From, Now));
        Assert.Equal(new SummaryRatio(0, 1), SalesSummaryRules.PaymentCompletion(orders, From, To, Now.AddHours(2)));
    }

    // ---- KK7: shipped late, cancelled late or terminated after the promise ran out has not kept it; cancelled before it is out ----

    [Fact]
    public void ThePromise_IsKeptOnlyByAShipmentBeforeItRanOut_ACancellationBeforeItLeavesTheOrderOut_KK7()
    {
        DateTimeOffset end = From.AddHours(12);

        SummaryRatio ratio = SalesSummaryRules.ShippingPromiseCompliance(
            [
                new ShippingPromiseOrder(Guid.NewGuid(), end, end.AddHours(-1), null, []),
                new ShippingPromiseOrder(Guid.NewGuid(), end, end.AddDays(2), null, []),
                new ShippingPromiseOrder(Guid.NewGuid(), end, null, end.AddDays(1), []),
                new ShippingPromiseOrder(Guid.NewGuid(), end, null, null, []),
                new ShippingPromiseOrder(Guid.NewGuid(), end, end, null, []),
                new ShippingPromiseOrder(Guid.NewGuid(), end, null, end.AddHours(-3), []),
            ],
            new Dictionary<Guid, int>());

        Assert.Equal(new SummaryRatio(1, 5), ratio);
    }

    [Fact]
    public void AnOrderWhosePhysicalItemsWereAllClosedBeforeThePromiseRanOut_IsLeftOut_AnOpenPhysicalUnitKeepsItIn_KK7()
    {
        DateTimeOffset end = From.AddHours(12);
        PromisedItem closedA = new(Guid.NewGuid(), 2), closedB = new(Guid.NewGuid(), 1), partly = new(Guid.NewGuid(), 3), unread = new(Guid.NewGuid(), 1);
        PromisedItem shippedClosed = new(Guid.NewGuid(), 1);
        Dictionary<Guid, int> open = new() { [closedA.OrderItemId] = 0, [closedB.OrderItemId] = 0, [partly.OrderItemId] = 1, [shippedClosed.OrderItemId] = 0 };

        // Every physical unit closed by item records before the end: left out. One unit still open: in, not kept. An item the open quantities do not name counts as open.
        // A shipment before the end keeps the promise whatever was closed beside it.
        SummaryRatio ratio = SalesSummaryRules.ShippingPromiseCompliance(
            [
                new ShippingPromiseOrder(Guid.NewGuid(), end, null, null, [closedA, closedB]),
                new ShippingPromiseOrder(Guid.NewGuid(), end, null, null, [closedA, partly]),
                new ShippingPromiseOrder(Guid.NewGuid(), end, null, null, [unread]),
                new ShippingPromiseOrder(Guid.NewGuid(), end, end.AddHours(-2), null, [shippedClosed]),
            ],
            open);

        Assert.Equal(new SummaryRatio(1, 3), ratio);
    }

    // ---- KK8: the ratio counts an order once, whatever its records ----

    [Fact]
    public void TheCancellationAndReturnRatio_CountsAnOrderOnce_OverItsItems_KK8()
    {
        Guid first = Guid.NewGuid(), second = Guid.NewGuid(), third = Guid.NewGuid();
        Guid a = Guid.NewGuid(), b = Guid.NewGuid(), c = Guid.NewGuid(), d = Guid.NewGuid();
        OrderOfItem[] items = [new(a, first), new(b, first), new(c, second), new(d, third)];

        Assert.Equal(new SummaryRatio(1, 3), SalesSummaryRules.CancellationAndReturn(items, new HashSet<Guid> { a, b }));
        Assert.Equal(new SummaryRatio(0, 0), SalesSummaryRules.CancellationAndReturn([], new HashSet<Guid> { a }));
    }

    // ---- KK9: a zero denominator is not assessable, never zero per cent ----

    [Fact]
    public void ARatioWithAZeroDenominator_IsNotAssessable_KK9()
    {
        Assert.False(SalesSummaryRules.PaymentCompletion([], From, To, Now).IsAssessable);
        Assert.False(SalesSummaryRules.ShippingPromiseCompliance([], new Dictionary<Guid, int>()).IsAssessable);
        Assert.False(new SummaryRatio(0, 0).IsAssessable);
        Assert.True(new SummaryRatio(0, 4).IsAssessable);
        Assert.DoesNotContain(typeof(SummaryRatio).GetProperties(), p => p.Name.Contains("Percent", StringComparison.OrdinalIgnoreCase));
    }

    // ---- The best sellers (04 9.22.6) ----

    [Fact]
    public void TheBestSellers_AreTheTopTenByUnits_ThenByAmount()
    {
        List<ProductSalesFact> products = [.. Enumerable.Range(1, 12).Select(n => new ProductSalesFact(Guid.NewGuid(), $"Ürün {n:D2}", n, 10m * n))];
        products.Add(new ProductSalesFact(null, "Silinmiş", 12, 500m));

        IReadOnlyList<TopSeller> top = SalesSummaryRules.TopSellers(products);

        Assert.Equal(SalesSummaryRules.TopSellerCount, top.Count);
        Assert.Equal(["Silinmiş", "Ürün 12", "Ürün 11"], top.Take(3).Select(t => t.ProductName));
        Assert.DoesNotContain(top, t => t.ProductName is "Ürün 01" or "Ürün 02");
    }

    // ---- KK1: computed from the records, kept nowhere, no export, no request ----

    [Fact]
    public void TheSummary_ReadsOnlyTheModulesSurfaces_KeepsNothing_AndHasNoExport_KK1()
    {
        Type[] parameters = [.. typeof(SalesSummaryService).GetConstructors().Single().GetParameters().Select(p => p.ParameterType)];
        Assert.All(parameters, t => Assert.True(t == typeof(TimeProvider) || t.Namespace!.EndsWith(".Contracts", StringComparison.Ordinal), t.FullName));

        FieldInfo[] fields = typeof(SalesSummaryService).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.DoesNotContain(fields, f => f.FieldType == typeof(AppDbContext) || f.FieldType.Namespace!.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal));

        string[] methods = [.. typeof(ISalesSummary).GetMethods().Select(m => m.Name).Order(StringComparer.Ordinal)];
        Assert.Equal(["CountDailyOrdersAsync", "GetAsync"], methods);
        Assert.DoesNotContain(typeof(AppDbContext).GetProperties(), p => p.Name.Contains("SalesSummar", StringComparison.OrdinalIgnoreCase));
    }

    private static (DateOnly Start, DateOnly End) Days(SalesSummaryPeriod? period) => (period!.StartDay, period.EndDay);

    private static PaymentAttemptOrder Attempt(Guid? cart, DateTimeOffset at, PaymentStatus status) => new(Guid.NewGuid(), cart, at, status, at.AddHours(1));
}
