// Violation sample of the transition unit scan: the bulk and raw writes of a status column. Excluded from compilation.
// A comment line that names ExecuteUpdate(s => s.SetProperty(o => o.OrderStatus, x)) is not a finding.
public sealed class Sample
{
    public async Task Write(AppDbContext db, Guid id, CancellationToken ct)
    {
        await db.Orders.ExecuteUpdateAsync(s => s.SetProperty(o => o.OrderStatus, OrderStatus.Cancelled), ct);
        await db.Orders.ExecuteUpdateAsync(
            s => s
                .SetProperty(o => o.Version, o => o.Version + 1)
                .SetProperty(o => o.PaymentStatus, PaymentStatus.Paid),
            ct);
        await db.Database.ExecuteSqlRawAsync("UPDATE orders SET payment_status = 'Paid' WHERE id = {0}", id);
        await db.Database.ExecuteSqlAsync($"UPDATE orders SET order_status = 'Cancelled' WHERE id = {id}", ct);
        string sql = "UPDATE defect_claims SET defect_claim_status = 'Resolved'";
        await db.Database.ExecuteSqlRawAsync(
            """
            UPDATE orders
               SET version = version + 1,
                   order_status = 'Delivered'
             WHERE id = {0}
            """,
            id);
        List<Order> locked = await db.Orders.FromSql($"SELECT * FROM orders WHERE order_status = 'Placed' FOR UPDATE").ToListAsync(ct);
        db.Entry(locked[0]).Property("OrderStatus").CurrentValue = OrderStatus.Cancelled;
        db.Entry(locked[0]).Property(nameof(Order.PaymentStatus)).CurrentValue = PaymentStatus.Paid;
    }

    public async Task Allowed(AppDbContext db, ModelBuilder builder, CancellationToken ct)
    {
        await db.Orders.ExecuteUpdateAsync(s => s.SetProperty(o => o.Version, o => o.Version + 1), ct);
        builder.Entity<Order>().HasIndex(o => o.OrderStatus).HasFilter("order_status = 'Placed'");
        builder.Entity<Order>().HasRule("status_pair_allowed", "payment_status <> 'Failed' OR order_status = 'Cancelled'");
        List<Order> read = await db.Orders.Where(o => o.OrderStatus == OrderStatus.Placed).ToListAsync(ct);
        await db.Database.ExecuteSqlRawAsync("UPDATE orders SET version = version + 1 WHERE id = {0}", read[0].Id);
    }
}
