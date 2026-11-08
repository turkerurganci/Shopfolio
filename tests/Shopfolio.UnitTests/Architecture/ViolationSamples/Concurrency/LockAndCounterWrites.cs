// Deliberate violation sample of the concurrency source scan (09 10.2 items 4-6): lock texts and counter writes outside their one place.
// The scan reads this file as text; it is not compiled. Line numbers are part of the test.
internal sealed class LockAndCounterWrites(AppDbContext db)
{
    public Task AdvisoryLock(CancellationToken ct) => db.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock(1)", ct);
    public Task RowLock(CancellationToken ct) => db.Database.SqlQuery<Guid>($"SELECT id FROM orders FOR UPDATE").ToListAsync(ct);
    public Task WeakRowLock(CancellationToken ct) => db.Database.SqlQuery<Guid>($"select id from orders for no key update").ToListAsync(ct);
    public Task SharedLock(CancellationToken ct) => db.Database.SqlQuery<Guid>($"SELECT id FROM orders FOR SHARE").ToListAsync(ct);
    public Task KeySharedLock(CancellationToken ct) => db.Database.SqlQuery<Guid>($"SELECT id FROM orders FOR KEY SHARE").ToListAsync(ct);
    public Task Reserve(CancellationToken ct) => db.Variants.ExecuteUpdateAsync(s => s.SetProperty(v => v.ReservedQuantity, v => v.ReservedQuantity + 1), ct);
    public Task UseCoupon(CancellationToken ct) => db.Coupons.ExecuteUpdateAsync(
        s => s.SetProperty(
            c => c.UsedCount,
            c => c.UsedCount + 1),
        ct);
    public Task ByName(CancellationToken ct) => db.Coupons.ExecuteUpdateAsync(s => s.SetProperty(c => EF.Property<int>(c, "ReservedCount"), 0), ct);
    public Task RawCounter(CancellationToken ct) => db.Database.ExecuteSqlAsync($"UPDATE variants SET stock_quantity = 0 WHERE id = {Guid.Empty}", ct);

    // Not findings: a comment that says SELECT ... FOR UPDATE, a counter read in the value argument, another column written raw.
    public Task ReadsTheCounter(CancellationToken ct) => db.Variants.ExecuteUpdateAsync(s => s.SetProperty(v => v.Sku, v => v.ReservedQuantity.ToString()), ct);
    public Task OtherColumn(CancellationToken ct) => db.Database.ExecuteSqlAsync($"UPDATE variants SET sku = 'x' WHERE id = {Guid.Empty}", ct);
    public Task TryAdvisoryLock(CancellationToken ct) => db.Database.ExecuteSqlAsync($"SELECT pg_try_advisory_lock(1)", ct);
    public Task Download(CancellationToken ct) => db.OrderItems.ExecuteUpdateAsync(s => s.SetProperty(i => i.DownloadCount, i => i.DownloadCount + 1), ct);
    public Task RawDownload(CancellationToken ct) => db.Database.ExecuteSqlAsync($"UPDATE order_items SET download_count = 0 WHERE id = {Guid.Empty}", ct);
    public Task DownloadByName(CancellationToken ct) => db.OrderItems.ExecuteUpdateAsync(s => s.SetProperty(i => EF.Property<int>(i, "DownloadCount"), 0), ct);

    // Not findings: the download counter read in the value argument, another order item column written raw.
    public Task ReadsDownloadCount(CancellationToken ct) => db.OrderItems.ExecuteUpdateAsync(s => s.SetProperty(i => i.Sku, i => i.DownloadCount.ToString()), ct);
    public Task OtherItemColumn(CancellationToken ct) => db.Database.ExecuteSqlAsync($"UPDATE order_items SET sku = 'x' WHERE id = {Guid.Empty}", ct);
}
