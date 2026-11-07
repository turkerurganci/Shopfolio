// Deliberate violation sample of the audit trail source scan (T380; 09 19.2 item 3). This file is excluded from compilation and from the real scan; the scan test reads it as text.
// The line numbers are asserted by the test: change them together.
internal sealed class AuditTrailMisuse
{
    // db.AuditLogs.Add(x) in a comment is no finding
    public async Task Touches(dynamic db, CancellationToken ct)
    {
        db.AuditLogs.Add(null);
        await db.AuditLogs.ExecuteDeleteAsync(ct);
        var set = db.Set<AuditLog>();
        var type = typeof(AuditLog);
        var row = new AuditLog();
        var made = AuditLog.Create(1, null, null, null, null, null, null, default);
        var viaDisposal = DisposalRecordType.AuditLog;
        var part = "AuditLog";
    }

    public Task RawSql(dynamic db)
    {
        db.Database.ExecuteSqlRaw("DELETE FROM audit_logs");
        db.Database.ExecuteSqlRaw("SELECT public.dispose_expired_audit_logs(10, now())");
        return Task.CompletedTask;
    }

    private IAuditTrail _trail;
    private AuditEntry _entry;

    public void DataBreach()
    {
        var detector = new BreachDetector();
        // BreachNotice in a comment is no finding
    }
}
