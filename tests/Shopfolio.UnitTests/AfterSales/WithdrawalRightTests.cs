using System.Reflection;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.AfterSales;

/// <summary>
/// The withdrawal right (T371 criteria 1, 2, 4, 6, 7, 10; 02 7.1.2, 7.3.1-7.3.4): the window is a fixed legal period that no setting, product constant or
/// installation key carries; it runs from the delivery date entered, not from the moment the mark was given, so a late mark neither restarts it nor
/// pulls a missing date back to the order's; no record keeps the start of a service's fulfilment or a performed share, so a partly performed service is
/// withdrawn in full; and the buyer's side of an order carries no commercial field - the one consumer regime asks nothing of the buyer.
/// </summary>
public sealed class WithdrawalRightTests
{
    // Monday 9 November 2026.
    private static readonly DateOnly Day = new(2026, 11, 9);

    private static readonly DateTimeOffset OrderedAt = TurkeyTime.StartOf(Day).AddHours(10);

    private static readonly int Window = AfterSalesPeriodRules.WithdrawalWindowDays;

    // ---- KK1: fixed, not a setting ----

    [Fact]
    public void TheWindow_IsTheFourteenDayLegalPeriodInCalendarDays_AndNoSettingConstantOrInstallationKeyCarriesIt_KK1()
    {
        // 02 7.3.1 writes the length itself ("on dört gündür ve sabittir"); the value is the requirement under test, not a second copy of a table.
        Assert.Equal(14, Window);
        Assert.Equal(DayCounting.CalendarDay, PeriodCounting.CountingOf(CountedPeriod.WithdrawalWindow));

        const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        Assert.DoesNotContain(typeof(CompanySettings).GetProperties(all), p => IsWithdrawalName(p.Name));
        Assert.DoesNotContain(typeof(ProductConstants).GetMembers(all), m => IsWithdrawalName(m.Name));
        Assert.DoesNotContain(ParameterInventory.Entries.SelectMany(e => e.Constants), c => IsWithdrawalName(c.Name));

        string appsettings = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "src", "Shopfolio.Web", "appsettings.json"));
        Assert.DoesNotContain("withdraw", appsettings, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cayma", appsettings, StringComparison.OrdinalIgnoreCase);
    }

    // ---- KK2, KK4: the start, and no restart ----

    [Fact]
    public void ALateDeliveryMark_DoesNotRestartThePhysicalWindow_TheEnteredDateDoes_AndItClosesAtTheMarkWhenAlreadyRunOut_KK2_KK4()
    {
        DateTimeOffset end = AfterSalesPeriodRules.WithdrawalWindowEnd(Day);
        DateTimeOffset lateMark = end.AddDays(6);
        WithdrawalWindowFacts marked = new(ProductType.Physical, Day, lateMark, OrderedAt, true, Day);

        Assert.Equal(TurkeyTime.StartOf(Day.AddDays(Window + 1)), end);
        Assert.False(AfterSalesPeriodRules.IsWithdrawalWindowOpen(marked, lateMark));
        Assert.True(AfterSalesPeriodRules.IsWithdrawalWindowOpen(marked, end.AddTicks(-1)));

        // A corrected (earlier) date is entered too and shortens the window (02 10.4.5); the moment of the correction restarts nothing.
        WithdrawalWindowFacts corrected = marked with { ItemDeliveryDate = Day.AddDays(-3) };
        Assert.False(AfterSalesPeriodRules.IsWithdrawalWindowOpen(corrected, AfterSalesPeriodRules.WithdrawalWindowEnd(Day.AddDays(-3))));
    }

    [Fact]
    public void AServiceOnAnOrderThatCarriedAPhysicalItem_HasNoStartWithoutTheDeliveryDate_AndIsNotPulledBackToTheOrderDate_KK2_KK6()
    {
        DateTimeOffset orderDateEnd = AfterSalesPeriodRules.WithdrawalWindowEnd(Day);
        WithdrawalWindowFacts undated = new(ProductType.Service, null, null, OrderedAt, true, null);

        Assert.Null(AfterSalesPeriodRules.ServiceWithdrawalStartDay(OrderedAt, true, null));
        Assert.True(AfterSalesPeriodRules.IsWithdrawalWindowOpen(undated, orderDateEnd));
        Assert.True(AfterSalesPeriodRules.IsWithdrawalWindowOpen(undated, orderDateEnd.AddYears(2)));

        // Only the "tamamlandı" mark ends it then.
        DateTimeOffset completedAt = orderDateEnd.AddDays(40);
        WithdrawalWindowFacts completed = undated with { ItemDeliveryDate = TurkeyTime.DateOf(completedAt), ItemDeliveryMarkedAt = completedAt };
        Assert.False(AfterSalesPeriodRules.IsWithdrawalWindowOpen(completed, completedAt));
    }

    [Fact]
    public void AServiceWindow_EndsAtWhicheverComesFirst_TheMarkOrTheFourteenthDay_KK6()
    {
        DateTimeOffset end = AfterSalesPeriodRules.WithdrawalWindowEnd(Day);
        WithdrawalWindowFacts open = new(ProductType.Service, null, null, OrderedAt, false, null);

        DateTimeOffset earlyMark = OrderedAt.AddDays(2);
        WithdrawalWindowFacts markedEarly = open with { ItemDeliveryDate = TurkeyTime.DateOf(earlyMark), ItemDeliveryMarkedAt = earlyMark };
        Assert.True(AfterSalesPeriodRules.IsWithdrawalWindowOpen(open, earlyMark));
        Assert.False(AfterSalesPeriodRules.IsWithdrawalWindowOpen(markedEarly, earlyMark));

        Assert.True(AfterSalesPeriodRules.IsWithdrawalWindowOpen(open, end.AddTicks(-1)));
        Assert.False(AfterSalesPeriodRules.IsWithdrawalWindowOpen(open, end));
    }

    // ---- KK6, KK7: no fulfilment start, no performed share ----

    [Fact]
    public void NoRecordKeepsTheStartOfAServicesFulfilmentOrAPerformedShare_SoAPartlyPerformedServiceIsWithdrawnInFull_KK6_KK7()
    {
        Regex performance = new("(Start|Began|Begun|Perform|Progress|Deduct|Usage|Consum|Partial)", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        foreach (Type type in new[] { typeof(OrderItem), typeof(Withdrawal), typeof(WithdrawalDeclaration), typeof(ItemAmounts) })
        {
            Assert.DoesNotContain(type.GetProperties(), p => performance.IsMatch(p.Name));
        }

        // A withdrawal sets aside the same three amounts as every item record (02 7.1.7); there is no fourth value to keep back for the performed part.
        Assert.Equal(
            [nameof(ItemAmounts.ProductAmount), nameof(ItemAmounts.CouponShare), nameof(ItemAmounts.VatAmount)],
            typeof(ItemAmounts).GetProperties().Select(p => p.Name).ToArray());
    }

    // ---- KK10: the one consumer regime ----

    [Fact]
    public void TheBuyersSide_CarriesNoCommercialField_AndTheOrdersCompanyFieldsAreOnlyTheSellersFrozenIdentity_KK10()
    {
        HashSet<string> seller = [.. typeof(FrozenCompanyIdentity).GetProperties().Select(p => p.Name)];
        PropertyInfo[] orderProperties = typeof(Order).GetProperties();

        // The order's Company* fields are the seller's identity frozen at the order (T357), never the buyer's.
        Assert.All(orderProperties.Where(p => p.Name.StartsWith("Company", StringComparison.Ordinal)), p => Assert.Contains(p.Name, seller));

        Regex commercial = new(
            "(Tax|Mersis|TradeRegistry|Kep|Commercial|Corporate|Business|LegalName|CompanyType|Company|Vkn|Tckn|IdentityNumber)",
            RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(1));
        Assert.DoesNotContain(orderProperties.Where(p => !seller.Contains(p.Name)), p => commercial.IsMatch(p.Name));
        foreach (Type buyerSide in new[] { typeof(Customer), typeof(AddressBook), typeof(PartyAddress), typeof(ConfirmationParties), typeof(OrderAddressHistory), typeof(Cart) })
        {
            Assert.DoesNotContain(buyerSide.GetProperties(), p => commercial.IsMatch(p.Name));
        }
    }

    private static bool IsWithdrawalName(string name) => name.Contains("Withdrawal", StringComparison.OrdinalIgnoreCase) || name.Contains("Cayma", StringComparison.OrdinalIgnoreCase);
}
