using System.Reflection;

using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// The open face of the shipment axis (T316; 09 9.1 items 3, 4): the operations of the unit are a value-returning, cancellable set, the refusals the caller
/// sees are the core's refusals (the correction's actor and reason refusals apart - the open face says ActorNotAllowed and InvalidInput for them, T319), and
/// the cancellation request's factories carry each actor's data and no one else's.
/// </summary>
public sealed class ShipmentTransitionContractTests
{
    private static readonly string[] CorrectionOnly =
    [
        nameof(TransitionRejection.CorrectionRequiresPanel),
        nameof(TransitionRejection.CorrectionReasonRequired),
    ];

    [Fact]
    public void TheRefusalsOfTheOpenFace_AreTheCoresRefusals_ExceptTheCorrections_KK1()
    {
        string[] core = Enum.GetNames<TransitionRejection>().Except(CorrectionOnly).Order().ToArray();
        string[] open = Enum.GetNames<TransitionRefusal>().Order().ToArray();

        Assert.Equal(core, open);
    }

    [Fact]
    public void EveryOperationOfTheOpenFace_ReturnsTheResultAsATask_AndEndsWithTheCancellationToken_KK1()
    {
        MethodInfo[] methods = typeof(IOrderTransitions).GetMethods();

        Assert.NotEmpty(methods);
        // The shipment axis returns OrderTransitionResult, the payment axis (T317) PaymentTransitionResult.
        Assert.All(methods, m =>
        {
            Assert.Contains(m.ReturnType, new[] { typeof(Task<OrderTransitionResult>), typeof(Task<PaymentTransitionResult>), typeof(Task<ItemRecordResult>) });
            Assert.Equal(typeof(CancellationToken), m.GetParameters()[^1].ParameterType);
        });
    }

    [Fact]
    public void TheShipmentAxis_HasAnOperationForS3S4S9AndS5ToS8_AndNoneForS1OrTheClosures_KK4()
    {
        // S1 has no operation of its own: it comes with Ö1, in the payment axis's approval (T318; 03 1.4.2.1; D-307).
        // The shipment axis's operations are the ones that return OrderTransitionResult; the payment axis's (T317) return PaymentTransitionResult.
        string[] names = typeof(IOrderTransitions).GetMethods()
            .Where(m => m.ReturnType == typeof(Task<OrderTransitionResult>) && m.Name != nameof(IOrderTransitions.CorrectTransitionAsync))
            .Select(m => m.Name).Order().ToArray();

        // S2, S10 and S11 are written by the closure rule's last event, not by an operation of their own (03 1.2). The service item's "completed" mark (T351) is an item mark, not a
        // transition: it writes a record only when it was the last open item and the closure rule ends the line. The customer's item cancellation (T367) is an item
        // record: its last open units are the customer's S3 or S4 through CancelOrderAsync, any other one ends the line only through the closure rule. The firm's item
        // cancellation (T368) works alike with the firm's S3, S4 or S9; the late delivery termination (T368) writes item records only and ends the line through the
        // closure rule.
        string[] expected =
        [
            nameof(IOrderTransitions.MarkServiceCompletedAsync),
            nameof(IOrderTransitions.CancelItemAsync),
            nameof(IOrderTransitions.CancelItemByFirmAsync),
            nameof(IOrderTransitions.TerminateForLateDeliveryAsync),
            nameof(IOrderTransitions.CancelOrderAsync),
            nameof(IOrderTransitions.CloseReturnedOrderAsync),
            nameof(IOrderTransitions.MarkDeliveredAsync),
            nameof(IOrderTransitions.MarkDeliveryFailedAsync),
            nameof(IOrderTransitions.ResendOrderAsync),
            nameof(IOrderTransitions.ShipOrderAsync),
        ];
        Assert.Equal(expected.Order().ToArray(), names);
    }

    [Fact]
    public void TheCancellationsTriggers_AreTheOnesOfTheTable_KK5()
    {
        // S3: the customer, the firm and the system; S4: the customer and the firm (the system only as the closure); S9: the firm alone.
        Assert.Equivalent(new[] { ActorKind.Buyer, ActorKind.Admin, ActorKind.System }, TransitionTable.ActorsOf(TransitionCode.S3));
        Assert.Contains(ActorKind.Buyer, TransitionTable.ActorsOf(TransitionCode.S4)!);
        Assert.Contains(ActorKind.Admin, TransitionTable.ActorsOf(TransitionCode.S4)!);
        Assert.Equal([ActorKind.Admin], TransitionTable.ActorsOf(TransitionCode.S9));
    }

    [Fact]
    public void S5AndS8_AreTheAdministratorsAlone_KK7()
    {
        Assert.Equal([ActorKind.Admin], TransitionTable.ActorsOf(TransitionCode.S5));
        Assert.Equal([ActorKind.Admin], TransitionTable.ActorsOf(TransitionCode.S8));
    }

    [Fact]
    public void S6AndS7_AreTheAdministratorsAlone_KK8_KK9()
    {
        Assert.Equal([ActorKind.Admin], TransitionTable.ActorsOf(TransitionCode.S6));
        Assert.Equal([ActorKind.Admin], TransitionTable.ActorsOf(TransitionCode.S7));
    }

    [Fact]
    public void TheCancellationRequestsFactories_CarryEachActorsDataAndNoOthers_KK5()
    {
        Guid orderId = Guid.NewGuid();
        TransitionAdmin admin = new(Guid.NewGuid(), "Firma Yöneticisi");
        CancelledItem item = new("Ürün", null, null, 1);

        CancelOrderRequest buyer = CancelOrderRequest.ByCustomer(orderId, item);
        CancelOrderRequest firm = CancelOrderRequest.ByAdministrator(orderId, admin, 3, true, CancellationReason.Other, "not", item);
        CancelOrderRequest system = CancelOrderRequest.BySystem(orderId, TransitionReason.PaymentWindowExpired);

        Assert.Equal((ActorKind.Buyer, (TransitionAdmin?)null, (int?)null, false, (CancellationReason?)null, (TransitionReason?)null, item), (buyer.Actor, buyer.Admin, buyer.ExpectedVersion, buyer.Confirmed, buyer.FirmReason, buyer.SystemReason, buyer.Item));
        Assert.Equal((ActorKind.Admin, admin, (int?)3, true, (CancellationReason?)CancellationReason.Other, (TransitionReason?)null, "not"), (firm.Actor, firm.Admin, firm.ExpectedVersion, firm.Confirmed, firm.FirmReason, firm.SystemReason, firm.ReasonNote));
        Assert.Equal((ActorKind.System, (TransitionAdmin?)null, (CancellationReason?)null, (TransitionReason?)TransitionReason.PaymentWindowExpired, (CancelledItem?)null), (system.Actor, system.Admin, system.FirmReason, system.SystemReason, system.Item));
    }

    [Fact]
    public void ARefusedResult_IsRejected_AndAnAcceptedOneIsNot()
    {
        OrderTransitionResult refused = new(TransitionRefusal.NotInTheTable, [], null, []);
        OrderTransitionResult accepted = new(null, [Guid.NewGuid()], null, []);

        Assert.True(refused.IsRejected);
        Assert.False(accepted.IsRejected);
    }
}
