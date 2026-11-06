// Deliberate violation sample of the versioned-side bulk write scan (D-95 (c); 09 10.2). The scan reads this file as text; it is not compiled.
// Line numbers are part of the test. A full-line comment that says db.Orders.ExecuteUpdateAsync or UPDATE orders SET is not a finding.
internal sealed class BulkWrites
{
    public async Task Named(AppDbContext db, Guid id, CancellationToken ct)
    {
        await db.Orders.Where(o => o.Id == id).ExecuteUpdateAsync(s => s.SetProperty(o => o.BillingDistrict, "x"), ct);
        await db.OrderItems
            .Where(i => i.OrderId == id)
            .ExecuteDeleteAsync(ct);
        await db.Set<Variant>().Where(v => v.ProductId == id).ExecuteDeleteAsync(ct);
        await CurrentStateWrite.ApplyAsync(db.Products.Where(p => p.Id == id), p => p.Version == 1, s => s.SetProperty(p => p.Name, "x"), ct);
        await db.PaymentRecords.ExecuteDeleteAsync(ct);
    }

    public async Task Raw(AppDbContext db, Guid id, CancellationToken ct)
    {
        await db.Database.ExecuteSqlAsync($"UPDATE orders SET billing_district = 'x' WHERE id = {id}", ct);
        await db.Database.ExecuteSqlRawAsync("DELETE FROM \"order_items\" WHERE order_id = {0}", id);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO public.refunds (id) VALUES ({id})",
            ct);
        await db.Database.ExecuteSqlAsync($"UPDATE sessions SET last_activity_at = now() WHERE id = {id}", ct);
    }

    public async Task Fine(AppDbContext db, Guid id, CancellationToken ct)
    {
        await db.Sessions.Where(s => s.Id == id).ExecuteDeleteAsync(ct);
        await db.HeartbeatRecords.ExecuteUpdateAsync(s => s.SetProperty(h => h.LastHeartbeatAt, DateTimeOffset.UtcNow), ct);
        List<Order> read = await db.Orders.Where(o => o.Id == id).ToListAsync(ct);
        int n = await db.Products.Include(p => p.Variants).CountAsync(ct);
        db.Orders.Remove(read[0]);
    }

    public async Task ThroughAVariable(AppDbContext db, Guid id, CancellationToken ct)
    {
        IQueryable<Order> rows = db.Orders.Where(o => o.Id == id);
        await rows.ExecuteUpdateAsync(s => s.SetProperty(o => o.BillingDistrict, "x"), ct);
        await db.Database.ExecuteSqlAsync($"SELECT 1", ct);
    }
}
