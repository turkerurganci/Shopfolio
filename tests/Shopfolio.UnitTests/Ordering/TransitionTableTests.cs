using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// The white list of the order's transitions (T315; 09 9.1 items 2, 4, 8): the table is read before any condition, every row names the kinds of
/// actor that may trigger it (KK4), a state that is not the row's source refuses the code, the terminal states are left by no row, and the pairs
/// of the two axes follow the combination rule of the database (03 1.4.1; 06 4.17).
/// </summary>
public sealed class TransitionTableTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void EveryRowOfTheTable_HasAtLeastOneActorKind_KK4()
    {
        foreach (TransitionCode code in TransitionTable.Codes)
        {
            IReadOnlyList<ActorKind>? actors = TransitionTable.ActorsOf(code);

            Assert.NotNull(actors);
            Assert.NotEmpty(actors);
            Assert.Equal(actors.Count, actors.Distinct().Count());
        }
    }

    [Fact]
    public void Correction_IsNotInTheTable_AndHasNoActorKinds()
    {
        Assert.Null(TransitionTable.ActorsOf(TransitionCode.Correction));
        Assert.Null(TransitionTable.FindShipment(TransitionCode.Correction));
        Assert.Null(TransitionTable.FindPayment(TransitionCode.Correction));
        Assert.DoesNotContain(TransitionCode.Correction, TransitionTable.Codes);
    }

    [Theory]
    [InlineData(TransitionCode.S1, new[] { ActorKind.System })]
    [InlineData(TransitionCode.S3, new[] { ActorKind.Buyer, ActorKind.Admin, ActorKind.System })]
    [InlineData(TransitionCode.S4, new[] { ActorKind.Buyer, ActorKind.Admin, ActorKind.System })]
    [InlineData(TransitionCode.S5, new[] { ActorKind.Admin })]
    [InlineData(TransitionCode.S6, new[] { ActorKind.Admin })]
    [InlineData(TransitionCode.S7, new[] { ActorKind.Admin })]
    [InlineData(TransitionCode.S8, new[] { ActorKind.Admin })]
    [InlineData(TransitionCode.S9, new[] { ActorKind.Admin })]
    [InlineData(TransitionCode.O1, new[] { ActorKind.Admin, ActorKind.System })]
    [InlineData(TransitionCode.O2, new[] { ActorKind.Buyer, ActorKind.Admin, ActorKind.System })]
    [InlineData(TransitionCode.O3, new[] { ActorKind.Admin, ActorKind.System })]
    [InlineData(TransitionCode.O4, new[] { ActorKind.Admin, ActorKind.System })]
    [InlineData(TransitionCode.O5, new[] { ActorKind.Admin, ActorKind.System })]
    public void ActorsOf_TheTriggersOf03Section1Point2(TransitionCode code, ActorKind[] expected)
    {
        Assert.Equal(expected.Order(), TransitionTable.ActorsOf(code)!.Order());
    }

    [Theory]
    [InlineData(TransitionCode.S2)]
    [InlineData(TransitionCode.S10)]
    [InlineData(TransitionCode.S11)]
    public void TheClosureDeliveries_TakeTheActorOfTheEventThatCompletesTheClosure(TransitionCode code)
    {
        Assert.Equal([ActorKind.Buyer, ActorKind.Admin, ActorKind.System], TransitionTable.ActorsOf(code)!);
    }

    [Fact]
    public void Allows_IsTrueOnlyForTheRowsSourceState_OnTheRowsOwnAxis()
    {
        foreach (ShipmentTransition row in TransitionTable.Shipment)
        {
            foreach (OrderStatus state in Enum.GetValues<OrderStatus>())
            {
                Order order = OrderIn(state, PaymentStatus.Pending);

                Assert.Equal(state == row.From, TransitionTable.Allows(row.Code, order));
            }
        }

        foreach (PaymentTransition row in TransitionTable.Payment)
        {
            foreach (PaymentStatus state in Enum.GetValues<PaymentStatus>())
            {
                Order order = OrderIn(OrderStatus.Placed, state);

                Assert.Equal(state == row.From, TransitionTable.Allows(row.Code, order));
            }
        }

        Assert.False(TransitionTable.Allows(TransitionCode.Correction, OrderIn(OrderStatus.Placed, PaymentStatus.Pending)));
    }

    [Fact]
    public void TheTerminalStates_AreTheSourceOfNoShipmentRow_AndNoBackwardRowExists()
    {
        foreach (OrderStatus state in Enum.GetValues<OrderStatus>().Where(TransitionTable.IsTerminal))
        {
            Assert.DoesNotContain(TransitionTable.Shipment, t => t.From == state);
        }

        Assert.Equal([OrderStatus.Delivered, OrderStatus.Cancelled], Enum.GetValues<OrderStatus>().Where(TransitionTable.IsTerminal).ToArray());
        Assert.DoesNotContain(TransitionTable.Shipment, t => t.From == OrderStatus.Shipped && t.To is OrderStatus.Placed or OrderStatus.Preparing);
        Assert.DoesNotContain(TransitionTable.Payment, t => t.From == PaymentStatus.Refunded);
        Assert.DoesNotContain(TransitionTable.Payment, t => t.From == PaymentStatus.Failed);
    }

    [Fact]
    public void IsPairAllowed_KeepsTheCombinationRuleOfTheTwoAxes()
    {
        int allowed = 0;
        int pairs = 0;
        foreach (OrderStatus orderStatus in Enum.GetValues<OrderStatus>())
        {
            foreach (PaymentStatus paymentStatus in Enum.GetValues<PaymentStatus>())
            {
                // A failed payment is a cancelled order; a pending payment is a placed order; the rest of the cells are open (03 1.4.1).
                bool expected = paymentStatus switch
                {
                    PaymentStatus.Failed => orderStatus == OrderStatus.Cancelled,
                    PaymentStatus.Pending => orderStatus == OrderStatus.Placed,
                    _ => true,
                };

                Assert.Equal(expected, TransitionTable.IsPairAllowed(orderStatus, paymentStatus));
                pairs++;
                allowed += expected ? 1 : 0;
            }
        }

        Assert.Equal(Enum.GetValues<OrderStatus>().Length * Enum.GetValues<PaymentStatus>().Length, pairs);
        Assert.True(allowed > 0 && allowed < pairs);
    }

    [Fact]
    public void ThePaymentAxisTerminalStates_AreExactlyTheStatesNoPaymentRowLeaves_T317KK1()
    {
        // The expected set is read from the table both ways: a terminal state is the source of no row, and a state no row leaves is terminal.
        PaymentStatus[] leftByNoRow = [.. Enum.GetValues<PaymentStatus>().Where(s => !TransitionTable.Payment.Any(t => t.From == s))];

        Assert.Equal(leftByNoRow, Enum.GetValues<PaymentStatus>().Where(TransitionTable.IsTerminal).ToArray());
        Assert.Equal([PaymentStatus.Failed, PaymentStatus.Refunded], leftByNoRow);
    }

    [Fact]
    public void FindPayment_ByTheTwoStates_ReturnsTheRowThatJoinsThem_AndNullWhereNoRowDoes()
    {
        foreach (PaymentTransition row in TransitionTable.Payment)
        {
            Assert.Same(row, TransitionTable.FindPayment(row.From, row.To));
        }

        Assert.Null(TransitionTable.FindPayment(PaymentStatus.PartiallyRefunded, PaymentStatus.PartiallyRefunded));
        Assert.Null(TransitionTable.FindPayment(PaymentStatus.Refunded, PaymentStatus.Paid));
    }

    [Fact]
    public void FindShipment_ByTheTwoStates_ReturnsTheRowThatJoinsThem()
    {
        Assert.Equal(TransitionCode.S10, TransitionTable.FindShipment(OrderStatus.Preparing, OrderStatus.Delivered)!.Code);
        Assert.Equal(TransitionCode.S11, TransitionTable.FindShipment(OrderStatus.DeliveryFailed, OrderStatus.Delivered)!.Code);
        Assert.Null(TransitionTable.FindShipment(OrderStatus.Delivered, OrderStatus.Placed));
    }

    [Fact]
    public void ApplyTransition_OnAStateThatIsNotTheRowsSource_Throws_AndLeavesTheOrderAsItWas()
    {
        Order order = OrderIn(OrderStatus.Placed, PaymentStatus.Pending);
        ShipmentTransition s5 = TransitionTable.FindShipment(TransitionCode.S5)!;

        Assert.Throws<InvalidOperationException>(() => order.ApplyTransition(s5));

        Assert.Equal(OrderStatus.Placed, order.OrderStatus);
        Assert.Equal(1, order.Version);
    }

    [Fact]
    public void ApplyTransition_WritesTheRowsTarget_AndDoesNotRaiseTheVersion()
    {
        Order order = OrderIn(OrderStatus.Placed, PaymentStatus.Pending);

        order.ApplyTransition(TransitionTable.FindPayment(TransitionCode.O1)!);
        order.ApplyTransition(TransitionTable.FindShipment(TransitionCode.S1)!);

        Assert.Equal((OrderStatus.Preparing, PaymentStatus.Paid), (order.OrderStatus, order.PaymentStatus));
        Assert.Equal(1, order.Version);
    }

    private static Order OrderIn(OrderStatus orderStatus, PaymentStatus paymentStatus)
    {
        Order order = Order.Create("2026-000001", "access-placeholder", "confirm-placeholder", PaymentMethod.Card, true, "ali@example.com", 120m, 0m, Now.AddHours(1), Now);
        order.ApplyCorrection(orderStatus, paymentStatus);
        return order;
    }
}
