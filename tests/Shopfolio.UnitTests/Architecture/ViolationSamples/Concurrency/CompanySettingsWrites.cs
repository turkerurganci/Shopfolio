// Deliberate violation sample of the company settings write scan (09 10.3 item 4): bulk writes of the settings row outside its one writer.
// The scan reads this file as text; it is not compiled. Line numbers are part of the test.
internal sealed class CompanySettingsWrites(AppDbContext db)
{
    public Task BulkUpdate(CancellationToken ct) => db.CompanySettings.Where(s => s.ReturnAddressVersion == 1).ExecuteUpdateAsync(s => s.SetProperty(x => x.ReturnPhone, "x"), ct);
    public Task BulkDelete(CancellationToken ct) => db.CompanySettings.ExecuteDeleteAsync(ct);
    public Task ThroughAVariable(CancellationToken ct)
    {
        IQueryable<CompanySettings> rows = db.CompanySettings;
        return rows
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ReturnPhone, "x"), ct);
    }
    public Task RawUpdate(CancellationToken ct) => db.Database.ExecuteSqlAsync(
        $"UPDATE company_settings SET return_phone = 'x'", ct);
    public Task RawQuotedUpdate(CancellationToken ct) => db.Database.ExecuteSqlAsync($"update public.\"company_settings\" set return_phone = 'x'", ct);
    public Task RawDelete(CancellationToken ct) => db.Database.ExecuteSqlAsync($"DELETE FROM company_settings", ct);

    // Not findings: a comment that says ExecuteUpdate on CompanySettings, a read, and another table written raw.
    public Task Read(CancellationToken ct) => db.CompanySettings.AsNoTracking().SingleAsync(ct);
    public Task OtherTable(CancellationToken ct) => db.Database.ExecuteSqlAsync($"UPDATE variants SET sku = 'x'", ct);
}
