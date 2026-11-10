using System.Reflection;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;

namespace Shopfolio.UnitTests.Administration;

/// <summary>
/// The panel home's pure rules (T382 KK1, KK2, KK6, KK7, KK8, KK9; 02 10.6.1, 10.8.2, 05 10.2.3): six counters and no seventh, their names and lists, the open quantity count, the channel
/// warning's thresholds and the setup checklist. The records' side - what the modules read from the database - is the integration tests' (PanelHomeTests).
/// </summary>
public sealed class PanelHomeRulesTests
{
    // ---- KK1, KK2: six counters, no seventh; each has its own name and source ----

    [Fact]
    public void TheHome_HasSixCounters_AndNoSeventh_KK1()
    {
        Assert.Equal(6, Enum.GetValues<PanelCounterKind>().Length);
        Assert.Equal(6, PanelHomeRules.CounterKinds.Count);

        IReadOnlyList<PanelCounter> counters = PanelHomeRules.Counters(new Dictionary<PanelCounterKind, int>());

        Assert.Equal(6, counters.Count);
        Assert.Equal(Enum.GetValues<PanelCounterKind>(), counters.Select(c => c.Kind));
        Assert.All(counters, c => Assert.Equal(0, c.Count));
    }

    [Fact]
    public void TheCounters_AreNamedAsTheDocumentNamesThem_AndTakeTheirCountFromTheirOwnKey_KK2()
    {
        Assert.Equal(
            ["PaymentApproval", "ToShip", "DeliveryMarkPending", "ServicePending", "OpenRequest", "RefundAndReturnPending"],
            Enum.GetNames<PanelCounterKind>());

        Dictionary<PanelCounterKind, int> counts = Enum.GetValues<PanelCounterKind>().ToDictionary(k => k, k => ((int)k + 1) * 10);
        IReadOnlyList<PanelCounter> counters = PanelHomeRules.Counters(counts);

        Assert.All(counters, c => Assert.Equal(((int)c.Kind + 1) * 10, c.Count));
    }

    // ---- KK6: each counter leads to its own list ----

    [Fact]
    public void EveryCounter_LeadsToItsOwnList_KK6()
    {
        PanelListTarget[] targets = [.. PanelHomeRules.CounterKinds.Select(PanelHomeRules.ListOf)];

        Assert.Equal(6, targets.Distinct().Count());
        Assert.Equal(Enum.GetValues<PanelListTarget>().Order(), targets.Order());
        Assert.Throws<ArgumentOutOfRangeException>(() => PanelHomeRules.ListOf((PanelCounterKind)99));
    }

    // ---- KK3, KK5: the open quantity decides whether an order is counted ----

    [Fact]
    public void AnOrder_IsCounted_WhileOneItemHasAnOpenUnit_AndOnce()
    {
        Guid order = Guid.NewGuid();
        PendingWorkItem a = new(order, Guid.NewGuid(), 2);
        PendingWorkItem b = new(order, Guid.NewGuid(), 1);

        Assert.Equal(1, PanelHomeRules.CountOrdersWithOpenItems([a, b], new Dictionary<Guid, int>()));
        Assert.Equal(1, PanelHomeRules.CountOrdersWithOpenItems([a, b], new Dictionary<Guid, int> { [a.OrderItemId] = 2 }));
        Assert.Equal(1, PanelHomeRules.CountOrdersWithOpenItems([a, b], new Dictionary<Guid, int> { [a.OrderItemId] = 1, [b.OrderItemId] = 1 }));
    }

    [Fact]
    public void AnOrder_WhoseEveryItemIsClosed_IsNotCounted_AndAReopenedClosureBringsItBack_KK5()
    {
        Guid order = Guid.NewGuid();
        PendingWorkItem a = new(order, Guid.NewGuid(), 2);
        PendingWorkItem b = new(order, Guid.NewGuid(), 1);
        Dictionary<Guid, int> closed = new() { [a.OrderItemId] = 2, [b.OrderItemId] = 1 };

        Assert.Equal(0, PanelHomeRules.CountOrdersWithOpenItems([a, b], closed));
        Assert.Equal(0, PanelHomeRules.CountOrdersWithOpenItems([], closed));

        closed[b.OrderItemId] = 0;
        Assert.Equal(1, PanelHomeRules.CountOrdersWithOpenItems([a, b], closed));
        Assert.Equal(1, PanelHomeRules.OpenQuantity(b, closed));
    }

    [Fact]
    public void TwoOrders_AreCountedSeparately()
    {
        PendingWorkItem a = new(Guid.NewGuid(), Guid.NewGuid(), 1);
        PendingWorkItem b = new(Guid.NewGuid(), Guid.NewGuid(), 1);

        Assert.Equal(2, PanelHomeRules.CountOrdersWithOpenItems([a, b], new Dictionary<Guid, int>()));
        Assert.Equal(1, PanelHomeRules.CountOrdersWithOpenItems([a, b], new Dictionary<Guid, int> { [a.OrderItemId] = 1 }));
    }

    // ---- KK7: the warnings' thresholds (05 10.2.3) ----

    [Theory]
    [InlineData(4, 20, false)]
    [InlineData(5, 14, false)]
    [InlineData(5, 15, true)]
    [InlineData(9, 60, true)]
    [InlineData(0, 60, false)]
    public void TheChannelWarning_OpensAtFiveErrorsAndFifteenMinutes_KK7(int errors, int minutesSinceFirstError, bool expected)
    {
        DateTimeOffset now = new(2026, 11, 10, 12, 0, 0, TimeSpan.Zero);

        Assert.Equal(expected, ChannelWarningCondition.IsOpen(errors, now.AddMinutes(-minutesSinceFirstError), now));
    }

    [Fact]
    public void TheChannelWarning_NeedsTheFirstErrorsMoment_AndTheThresholdsAreTheDesignValues_KK7()
    {
        DateTimeOffset now = new(2026, 11, 10, 12, 0, 0, TimeSpan.Zero);

        Assert.False(ChannelWarningCondition.IsOpen(7, null, now));
        Assert.Equal(5, ChannelWarningCondition.ErrorsInARow);
        Assert.Equal(15, ChannelWarningCondition.FirstErrorAgeMinutes);
    }

    // ---- KK8: a missing condition is named; KK9: no wizard, the list lifts by itself ----

    [Fact]
    public void TheChecklist_NamesEachMissingCondition_AndTheMissingIdentityFields_KK8()
    {
        IReadOnlyList<SetupChecklistItem>? list = PanelHomeRules.SetupChecklist(
            [SalesGateCondition.CompanyIdentity, SalesGateCondition.PaymentMethod, SalesGateCondition.PrivacyNotice, SalesGateCondition.CookiePolicy, SalesGateCondition.ReturnAddress],
            ["LegalName", "KepAddress"]);

        Assert.NotNull(list);
        Assert.Equal(Enum.GetValues<SetupChecklistItemKind>(), list.Select(i => i.Kind));
        Assert.All(list, i => Assert.False(i.IsDone));
        Assert.Equal(["LegalName", "KepAddress"], list.Single(i => i.Kind == SetupChecklistItemKind.CompanyIdentity).MissingFields);
    }

    [Fact]
    public void TheChecklist_MarksTheCompletedItem_AndKeepsTheOthers_KK9()
    {
        IReadOnlyList<SetupChecklistItem>? list = PanelHomeRules.SetupChecklist([SalesGateCondition.CookiePolicy, SalesGateCondition.TemporaryClosure], []);

        Assert.NotNull(list);
        Assert.Equal(5, list.Count);
        Assert.Equal([SetupChecklistItemKind.CookiePolicy], list.Where(i => !i.IsDone).Select(i => i.Kind));
        Assert.All(list.Where(i => i.IsDone), i => Assert.Empty(i.MissingFields));
    }

    [Fact]
    public void TheChecklist_Disappears_WhenEveryItemIsDone_AndTheTemporaryClosureIsNotAnItem_KK9()
    {
        Assert.Null(PanelHomeRules.SetupChecklist([], []));
        Assert.Null(PanelHomeRules.SetupChecklist([SalesGateCondition.TemporaryClosure], []));
        Assert.Equal(5, Enum.GetValues<SetupChecklistItemKind>().Length);
    }

    // ---- The home keeps nothing and reads only the modules' surfaces ----

    [Fact]
    public void TheHome_ReadsOnlyTheModulesSurfaces_AndKeepsNothing()
    {
        Type[] parameters = [.. typeof(PanelHomeService).GetConstructors().Single().GetParameters().Select(p => p.ParameterType)];
        Assert.All(parameters, t => Assert.EndsWith(".Contracts", t.Namespace!, StringComparison.Ordinal));

        FieldInfo[] fields = typeof(PanelHomeService).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.DoesNotContain(fields, f => f.FieldType == typeof(AppDbContext));

        Assert.Equal(["GetAsync"], typeof(IPanelHome).GetMethods().Select(m => m.Name));
        Assert.DoesNotContain(typeof(AppDbContext).GetProperties(), p => p.Name.Contains("PanelHome", StringComparison.OrdinalIgnoreCase));
    }
}
