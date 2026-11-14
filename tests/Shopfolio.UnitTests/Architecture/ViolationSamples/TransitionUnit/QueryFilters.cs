// Sample of the transition unit scan: a query model's repeated text filters named like a status field hold no status. Excluded from compilation.
public sealed class SampleQuery
{
    public string[]? OrderStatus { get; init; }
    public string[] PaymentStatus { get; set; }
    public string OrderStatus { get; init; }
    public string[]? Other { get; init; }
    public List<string>? PaymentStatus { get; init; }
    public OrderStatus[]? OrderStatus { get; set; }
}
