// Violation sample of the transition unit scan: the writers named outside the unit and the status fields with an open setter. Excluded from compilation.
// A comment line that names order.ApplyTransition(row) is not a finding.
public sealed class Sample
{
    public OrderStatus OrderStatus { get; set; }
    public PaymentStatus PaymentStatus { get; internal set; }
    public DefectClaimStatus DefectClaimStatus { get; init; }
    public OrderStatus OrderStatus { get; private protected set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public OrderStatus OrderStatus { get; protected set; }
    public OrderStatus OrderStatus
    {
        get;
        set;
    }
    public OrderStatus OrderStatus { get; }
    public OrderStatus OrderStatus => OrderStatus.Placed;
    public OrderStatus OrderStatus;
    public PaymentStatus PaymentStatus = PaymentStatus.Pending;
    private OrderStatus OrderStatus;
    public OrderStatus From { get; init; }
    internal void ApplyTransition(ShipmentTransition transition) { }
    internal void ApplyCorrection(OrderStatus orderStatus, PaymentStatus paymentStatus) { }
    internal void ApplyStatus(DefectClaimStatus status) { }
    public void Call(Order order, DefectClaim claim)
    {
        order.ApplyTransition(row);
        order.ApplyCorrection(OrderStatus.Placed, PaymentStatus.Paid);
        Action<ShipmentTransition> apply = order.ApplyTransition;
        string name = nameof(Order.ApplyCorrection);
        claim.ApplyStatus(DefectClaimStatus.Resolved);
        afterSales.ChangeDefectClaimStatusAsync(change, ct);
    }
}
