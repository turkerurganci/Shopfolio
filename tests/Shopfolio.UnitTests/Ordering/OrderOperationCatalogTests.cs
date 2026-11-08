using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// The decision table of the forty-one operations (T323 criteria 1-8, KK1-KK8; 03 1.11): the rows themselves, the interval of the item-type rules (physical, digital, service),
/// the server-side check of a forged request, the sides (no role model) and the closed state of anything the table does not list. The equality with the document is
/// <c>OperationTableConsistencyTests</c>; the reading of the records is in the integration tests.
/// </summary>
public sealed class OrderOperationCatalogTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 8, 12, 0, 0, TimeSpan.Zero);

    private static ItemPeriodState NoPeriod() => new(0, false, null, false, null, null, false, false, 0, 0, 0);

    private static OperationItemFacts Item(ProductType type, int open = 1, bool marked = false, ItemPeriodState? period = null, int downloads = 0, DateOnly? delivery = null) =>
        new(Guid.NewGuid(), type, open, open, delivery, marked, downloads, period ?? NoPeriod());

    private static OrderOperationFacts Order(
        OrderStatus status,
        PaymentStatus payment,
        PaymentMethod method,
        params OperationItemFacts[] items) =>
        new(Guid.NewGuid(), 1, status, payment, method, Now.AddDays(-1), Now, false, false, false, false, false, false, false, false, 0m, false, items);

    /// <summary>
    /// What the POST does: the request names a quantity when the operation carries one (the field rule makes it required), so an operation with a quantity is asked with one unit
    /// unless the test chooses another; the gate refuses a missing quantity as a programming error.
    /// </summary>
    private static OperationVerdict Ask(OrderOperationFacts facts, OrderOperation operation, Guid? itemId, int? quantity)
    {
        if (quantity is null && OrderOperationCatalog.Find(operation) is { } row)
        {
            OperationItemFacts? item = itemId is { } id ? facts.Items.FirstOrDefault(i => i.ItemId == id) : null;
            if (row.Rule.Evaluate(new OperationInput(facts, item)).HasQuantity)
            {
                quantity = 1;
            }
        }

        return OrderOperationCatalog.Check(facts, operation, itemId, quantity);
    }

    private static bool IsOpen(OrderOperationFacts facts, OrderOperation operation, Guid? itemId = null, int? quantity = null) => Ask(facts, operation, itemId, quantity).IsAllowed;

    [Fact]
    public void Rows_AreTheFortyOne_WithTheDocumentsIdentities_AndTheMembersOfTheEnum()
    {
        Assert.Equal(41, OrderOperationCatalog.Rows.Count);
        Assert.Equal(Enumerable.Range(1, 41).Select(n => "1.11." + n), OrderOperationCatalog.Rows.Select(r => r.Id));
        Assert.Equal(Enum.GetValues<OrderOperation>().Order(), OrderOperationCatalog.Rows.Select(r => r.Operation).Order());
        Assert.Equal(14, OrderOperationCatalog.Rows.Count(r => r.Actor == ActorKind.Buyer));
        Assert.Equal(27, OrderOperationCatalog.Rows.Count(r => r.Actor == ActorKind.Admin));
        Assert.Equal(41, OrderOperationCatalog.Rows.Select(r => r.Operation).Distinct().Count());
    }

    [Fact]
    public void ThereIsNoRoleModel_TheSidesNeverMix()
    {
        OperationItemFacts physical = Item(ProductType.Physical);
        OrderOperationFacts facts = Order(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, physical);

        IReadOnlyList<OrderOperationEntry> buyer = OrderOperationCatalog.Decide(facts, ActorKind.Buyer);
        IReadOnlyList<OrderOperationEntry> admin = OrderOperationCatalog.Decide(facts, ActorKind.Admin);

        Assert.All(buyer, e => Assert.True((int)e.Operation < 14));
        Assert.All(admin, e => Assert.True((int)e.Operation >= 14));
        Assert.Empty(OrderOperationCatalog.Decide(facts, ActorKind.System));
        Assert.Contains(admin, e => e.Operation == OrderOperation.ShipOrder);
    }

    [Fact]
    public void AnOperationNotInTheTable_IsClosed()
    {
        OrderOperationFacts facts = Order(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.BankTransfer, Item(ProductType.Physical));

        Assert.Null(OrderOperationCatalog.Find((OrderOperation)99));
        Assert.Null(OrderOperationCatalog.Find((OrderOperation)(-1)));
        Assert.Equal(OperationDecision.Closed, OrderOperationCatalog.Check(facts, (OrderOperation)99, null, null).Decision);
        Assert.Equal(OperationDecision.Closed, OrderOperationCatalog.Check(facts, (OrderOperation)(-1), null, 1).Decision);
    }

    [Fact]
    public void CancelOrder_IsOpenOnlyWhilePlacedAndPending()
    {
        Assert.True(IsOpen(Order(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.Card, Item(ProductType.Physical)), OrderOperation.CancelOrder));
        Assert.False(IsOpen(Order(OrderStatus.Placed, PaymentStatus.Paid, PaymentMethod.Card, Item(ProductType.Physical)), OrderOperation.CancelOrder));
        Assert.False(IsOpen(Order(OrderStatus.Preparing, PaymentStatus.Pending, PaymentMethod.Card, Item(ProductType.Physical)), OrderOperation.CancelOrder));
    }

    [Fact]
    public void DigitalItem_HasNoCancellationAndNoWithdrawal_AtAnyState()
    {
        OperationItemFacts digital = Item(ProductType.Digital, downloads: 3);
        foreach (OrderStatus status in Enum.GetValues<OrderStatus>())
        {
            foreach (PaymentStatus payment in Enum.GetValues<PaymentStatus>())
            {
                OrderOperationFacts facts = Order(status, payment, PaymentMethod.Card, digital);
                foreach (OrderOperation operation in new[] { OrderOperation.CancelDigitalItem, OrderOperation.WithdrawDigitalItem, OrderOperation.CancelPhysicalItem, OrderOperation.CancelServiceItem, OrderOperation.WithdrawPhysicalItem, OrderOperation.WithdrawServiceItem })
                {
                    Assert.False(IsOpen(facts, operation, digital.ItemId), $"{operation} {status} {payment}");
                }
            }
        }
    }

    [Fact]
    public void PhysicalItem_CancelsUntilShipped_AfterThePaymentIsConfirmed()
    {
        OperationItemFacts item = Item(ProductType.Physical, open: 3);
        Assert.False(IsOpen(Order(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.Card, item), OrderOperation.CancelPhysicalItem, item.ItemId));
        Assert.True(IsOpen(Order(OrderStatus.Placed, PaymentStatus.Paid, PaymentMethod.Card, item), OrderOperation.CancelPhysicalItem, item.ItemId, 3));
        Assert.True(IsOpen(Order(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, item), OrderOperation.CancelPhysicalItem, item.ItemId, 1));
        Assert.False(IsOpen(Order(OrderStatus.Shipped, PaymentStatus.Paid, PaymentMethod.Card, item), OrderOperation.CancelPhysicalItem, item.ItemId));
        Assert.False(IsOpen(Order(OrderStatus.DeliveryFailed, PaymentStatus.Paid, PaymentMethod.Card, item), OrderOperation.CancelPhysicalItem, item.ItemId));
        Assert.False(IsOpen(Order(OrderStatus.Delivered, PaymentStatus.Paid, PaymentMethod.Card, item), OrderOperation.CancelPhysicalItem, item.ItemId));
    }

    [Fact]
    public void ServiceItemCancellation_ClosesAtTheCompletedMark_WhateverTheShipmentStatusIs()
    {
        OperationItemFacts open = Item(ProductType.Service);
        OperationItemFacts done = Item(ProductType.Service, marked: true);
        foreach (OrderStatus status in new[] { OrderStatus.Placed, OrderStatus.Preparing, OrderStatus.Shipped, OrderStatus.Delivered })
        {
            Assert.True(IsOpen(Order(status, PaymentStatus.Paid, PaymentMethod.Card, open), OrderOperation.CancelServiceItem, open.ItemId), status.ToString());
            Assert.False(IsOpen(Order(status, PaymentStatus.Paid, PaymentMethod.Card, done), OrderOperation.CancelServiceItem, done.ItemId), status.ToString());
        }

        Assert.False(IsOpen(Order(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.Card, open), OrderOperation.CancelServiceItem, open.ItemId));
    }

    [Fact]
    public void PhysicalWithdrawal_IsClosedBeforeTheShipment_AndOpenInsideTheWindow()
    {
        ItemPeriodState window = NoPeriod() with { WithdrawalWindowOpen = true, WithdrawalLastDay = new DateOnly(2026, 11, 20) };
        OperationItemFacts item = Item(ProductType.Physical, open: 2, period: window);

        Assert.False(IsOpen(Order(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, item), OrderOperation.WithdrawPhysicalItem, item.ItemId));
        Assert.False(IsOpen(Order(OrderStatus.Placed, PaymentStatus.Paid, PaymentMethod.Card, item), OrderOperation.WithdrawPhysicalItem, item.ItemId));
        foreach (OrderStatus status in new[] { OrderStatus.Shipped, OrderStatus.DeliveryFailed, OrderStatus.Delivered })
        {
            OperationVerdict verdict = OrderOperationCatalog.Check(Order(status, PaymentStatus.Paid, PaymentMethod.Card, item), OrderOperation.WithdrawPhysicalItem, item.ItemId, 2);
            Assert.True(verdict.IsAllowed, status.ToString());
            Assert.Equal(new DateOnly(2026, 11, 20), verdict.Availability.LastDay);
        }

        OperationItemFacts closedWindow = Item(ProductType.Physical, period: window with { WithdrawalWindowOpen = false });
        Assert.False(IsOpen(Order(OrderStatus.Delivered, PaymentStatus.Paid, PaymentMethod.Card, closedWindow), OrderOperation.WithdrawPhysicalItem, closedWindow.ItemId));
    }

    [Fact]
    public void PhysicalWithdrawal_AbsoluteExemptionHasNone_ConditionalExemptionIsOpen()
    {
        ItemPeriodState window = NoPeriod() with { WithdrawalWindowOpen = true };
        OperationItemFacts absolute = Item(ProductType.Physical, period: window with { IsExempt = true });
        OperationItemFacts conditional = Item(ProductType.Physical, period: window with { IsExempt = true, IsConditionallyExempt = true });

        Assert.False(IsOpen(Order(OrderStatus.Shipped, PaymentStatus.Paid, PaymentMethod.Card, absolute), OrderOperation.WithdrawPhysicalItem, absolute.ItemId));
        Assert.True(IsOpen(Order(OrderStatus.Shipped, PaymentStatus.Paid, PaymentMethod.Card, conditional), OrderOperation.WithdrawPhysicalItem, conditional.ItemId));
    }

    [Fact]
    public void DefectClaim_PhysicalItemOnlyOnceShipped_OneClaimPerItem()
    {
        ItemPeriodState channel = NoPeriod() with { DefectChannelOpen = true, DefectLastDay = new DateOnly(2027, 11, 8) };
        OperationItemFacts physical = Item(ProductType.Physical, open: 4, period: channel);
        OperationItemFacts withClaim = Item(ProductType.Physical, period: channel with { DefectClaim = DefectClaimStatus.Open });

        // The period state is the AfterSales module's (it reads "shipped" for a physical item); the decision adds nothing but the claim and the open quantity.
        OperationVerdict verdict = OrderOperationCatalog.Check(Order(OrderStatus.Delivered, PaymentStatus.Paid, PaymentMethod.Card, physical), OrderOperation.OpenDefectClaim, physical.ItemId, 4);
        Assert.True(verdict.IsAllowed);
        Assert.Equal(1, verdict.Availability.DefaultQuantity);
        Assert.Equal(4, verdict.Availability.MaxQuantity);
        Assert.False(IsOpen(Order(OrderStatus.Delivered, PaymentStatus.Paid, PaymentMethod.Card, withClaim), OrderOperation.OpenDefectClaim, withClaim.ItemId));
        Assert.False(IsOpen(Order(OrderStatus.Delivered, PaymentStatus.Paid, PaymentMethod.Card, Item(ProductType.Physical)), OrderOperation.OpenDefectClaim, Guid.NewGuid()));
    }

    [Fact]
    public void LateTermination_FollowsZ11_AndTheFirmsRecordingHasTheSameInterval()
    {
        OrderOperationFacts late = Order(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, Item(ProductType.Physical)) with { CreatedAt = Now.AddDays(-40) };
        OrderOperationFacts soon = late with { CreatedAt = Now.AddDays(-2) };
        OrderOperationFacts dated = late with { Items = [Item(ProductType.Physical, delivery: new DateOnly(2026, 11, 20))] };
        OrderOperationFacts digitalOnly = late with { Items = [Item(ProductType.Digital)] };

        Assert.True(IsOpen(late, OrderOperation.TerminateForLateDelivery));
        Assert.True(IsOpen(late, OrderOperation.RecordLateTerminationFromOtherChannel));
        Assert.False(IsOpen(soon, OrderOperation.TerminateForLateDelivery));
        Assert.False(IsOpen(soon, OrderOperation.RecordLateTerminationFromOtherChannel));
        Assert.False(IsOpen(dated, OrderOperation.TerminateForLateDelivery));
        Assert.False(IsOpen(digitalOnly, OrderOperation.TerminateForLateDelivery));
        Assert.False(IsOpen(late with { OrderStatus = OrderStatus.Delivered }, OrderOperation.TerminateForLateDelivery));
    }

    [Fact]
    public void Download_NeedsAConfirmedPayment_AndDownloadsLeft()
    {
        OperationItemFacts item = Item(ProductType.Digital, downloads: 2);
        OperationItemFacts spent = Item(ProductType.Digital, downloads: 0);

        Assert.True(IsOpen(Order(OrderStatus.Delivered, PaymentStatus.Paid, PaymentMethod.Card, item), OrderOperation.DownloadDigitalFile, item.ItemId));
        Assert.False(IsOpen(Order(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.Card, item), OrderOperation.DownloadDigitalFile, item.ItemId));
        Assert.False(IsOpen(Order(OrderStatus.Delivered, PaymentStatus.Paid, PaymentMethod.Card, spent), OrderOperation.DownloadDigitalFile, spent.ItemId));
        Assert.True(IsOpen(Order(OrderStatus.Delivered, PaymentStatus.Paid, PaymentMethod.Card, spent), OrderOperation.RenewDownloadAllowance, spent.ItemId));
    }

    [Fact]
    public void ShippingOperations_FollowTheStatus()
    {
        OperationItemFacts physical = Item(ProductType.Physical);
        Assert.True(IsOpen(Order(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, physical), OrderOperation.ShipOrder));
        Assert.False(IsOpen(Order(OrderStatus.Placed, PaymentStatus.Paid, PaymentMethod.Card, physical), OrderOperation.ShipOrder));
        Assert.False(IsOpen(Order(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, Item(ProductType.Digital)), OrderOperation.ShipOrder));
        Assert.True(IsOpen(Order(OrderStatus.Shipped, PaymentStatus.Paid, PaymentMethod.Card, physical), OrderOperation.MarkDelivered));
        Assert.True(IsOpen(Order(OrderStatus.Shipped, PaymentStatus.Paid, PaymentMethod.Card, physical), OrderOperation.MarkDeliveryFailed));
        Assert.False(IsOpen(Order(OrderStatus.Delivered, PaymentStatus.Paid, PaymentMethod.Card, physical), OrderOperation.MarkDelivered));
        Assert.True(IsOpen(Order(OrderStatus.DeliveryFailed, PaymentStatus.Paid, PaymentMethod.Card, physical), OrderOperation.ResendOrder));
        Assert.False(IsOpen(Order(OrderStatus.Shipped, PaymentStatus.Paid, PaymentMethod.Card, physical), OrderOperation.ResendOrder));
    }

    [Fact]
    public void MarkTransferPaid_OnlyOnAPlacedPendingTransferOrder()
    {
        OperationItemFacts item = Item(ProductType.Physical);
        Assert.True(IsOpen(Order(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.BankTransfer, item), OrderOperation.MarkTransferPaid));
        Assert.False(IsOpen(Order(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.Card, item), OrderOperation.MarkTransferPaid));
        Assert.False(IsOpen(Order(OrderStatus.Cancelled, PaymentStatus.Pending, PaymentMethod.BankTransfer, item), OrderOperation.MarkTransferPaid));
    }

    [Fact]
    public void MarkServiceCompleted_NeedsAConfirmedPayment_AndAnOpenUnmarkedItem()
    {
        OperationItemFacts item = Item(ProductType.Service);
        OperationItemFacts marked = Item(ProductType.Service, marked: true);
        Assert.True(IsOpen(Order(OrderStatus.Placed, PaymentStatus.Paid, PaymentMethod.Card, item), OrderOperation.MarkServiceCompleted, item.ItemId));
        Assert.False(IsOpen(Order(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.Card, item), OrderOperation.MarkServiceCompleted, item.ItemId));
        Assert.False(IsOpen(Order(OrderStatus.Placed, PaymentStatus.Failed, PaymentMethod.Card, item), OrderOperation.MarkServiceCompleted, item.ItemId));
        Assert.False(IsOpen(Order(OrderStatus.Placed, PaymentStatus.Paid, PaymentMethod.Card, marked), OrderOperation.MarkServiceCompleted, marked.ItemId));
    }

    [Fact]
    public void FirmCancellation_IsWholeOrderBeforePayment_AndItemAfter()
    {
        OperationItemFacts item = Item(ProductType.Physical, open: 2);
        OrderOperationFacts unpaid = Order(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.BankTransfer, item);
        OrderOperationFacts paid = Order(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, item);
        OrderOperationFacts returned = Order(OrderStatus.DeliveryFailed, PaymentStatus.Paid, PaymentMethod.Card, item);

        Assert.True(IsOpen(unpaid, OrderOperation.CancelByFirm));
        Assert.False(IsOpen(unpaid, OrderOperation.CancelByFirm, item.ItemId));
        Assert.False(IsOpen(paid, OrderOperation.CancelByFirm));
        Assert.True(IsOpen(paid, OrderOperation.CancelByFirm, item.ItemId, 2));
        Assert.True(IsOpen(returned, OrderOperation.CancelByFirm, item.ItemId));
    }

    [Fact]
    public void InternalNote_IsOpenAtEveryStatus_UntilThePersonalDataIsDisposed()
    {
        foreach (OrderStatus status in Enum.GetValues<OrderStatus>())
        {
            OrderOperationFacts facts = Order(status, PaymentStatus.Paid, PaymentMethod.Card, Item(ProductType.Physical));
            Assert.True(IsOpen(facts, OrderOperation.WriteInternalNote), status.ToString());
            Assert.False(IsOpen(facts with { PersonalDataDisposed = true }, OrderOperation.WriteInternalNote), status.ToString());
        }
    }

    [Fact]
    public void ARequest_ForAnItemOfAnotherOrder_OrAnItemOnAnOrderOperation_OrNoItemOnAnItemOperation_IsClosed()
    {
        OperationItemFacts item = Item(ProductType.Physical);
        OrderOperationFacts facts = Order(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, item);

        Assert.True(IsOpen(facts, OrderOperation.CancelPhysicalItem, item.ItemId));
        Assert.False(IsOpen(facts, OrderOperation.CancelPhysicalItem, Guid.NewGuid()));
        Assert.False(IsOpen(facts, OrderOperation.CancelPhysicalItem, null));
        Assert.True(IsOpen(facts, OrderOperation.ShipOrder));
        Assert.False(IsOpen(facts, OrderOperation.ShipOrder, item.ItemId));
    }

    [Fact]
    public void AQuantityAboveTheOpenQuantity_IsRefusedWhole()
    {
        OperationItemFacts item = Item(ProductType.Physical, open: 2);
        OrderOperationFacts facts = Order(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, item);

        Assert.Equal(OperationDecision.Allowed, OrderOperationCatalog.Check(facts, OrderOperation.CancelPhysicalItem, item.ItemId, 2).Decision);
        Assert.Equal(OperationDecision.QuantityExceedsOpen, OrderOperationCatalog.Check(facts, OrderOperation.CancelPhysicalItem, item.ItemId, 3).Decision);
    }

    [Fact]
    public void ADecisionRead_AndACheck_AgreeOnEveryOperationOfEveryState()
    {
        // KK2: the screen draws from Decide, the POST asks Check - for every status pair, item type and side they must say the same thing.
        foreach (OrderStatus status in Enum.GetValues<OrderStatus>())
        {
            foreach (PaymentStatus payment in Enum.GetValues<PaymentStatus>())
            {
                foreach (ProductType type in Enum.GetValues<ProductType>())
                {
                    OperationItemFacts item = Item(type, open: 2, downloads: 1);
                    OrderOperationFacts facts = Order(status, payment, PaymentMethod.BankTransfer, item);
                    foreach (ActorKind actor in new[] { ActorKind.Buyer, ActorKind.Admin })
                    {
                        IReadOnlyList<OrderOperationEntry> open = OrderOperationCatalog.Decide(facts, actor);
                        foreach (OperationRow row in OrderOperationCatalog.Rows.Where(r => r.Actor == actor))
                        {
                            foreach (Guid? itemId in new Guid?[] { null, item.ItemId })
                            {
                                bool listed = open.Any(e => e.Operation == row.Operation && e.OrderItemId == itemId);
                                bool allowed = Ask(facts, row.Operation, itemId, null).IsAllowed;
                                Assert.Equal(listed, allowed);
                            }
                        }
                    }
                }
            }
        }
    }
}
