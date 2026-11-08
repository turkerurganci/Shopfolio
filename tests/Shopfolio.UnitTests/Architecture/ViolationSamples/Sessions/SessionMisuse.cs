// Deliberate violation sample of the session write scan (09 15.1 item 3; D-133). The scan reads this file as text; it is not compiled.
// Line numbers are part of the test. A full-line comment that says _db.Sessions.Add( or new Session is not a finding.
internal static class SessionMisuse
{
    public static void Add(AppDbContext db, Session session) => db.Sessions.Add(session);
    public static Task Delete(AppDbContext db, Guid id, CancellationToken ct) => db.Sessions
        .Where(s => s.CustomerId == id)
        .ExecuteDeleteAsync(ct);
    public static Task Invalidate(AppDbContext db, DateTimeOffset now, CancellationToken ct) => db.RecognizedBrowserMarks.Where(m => m.ExpiresAt > now).ExecuteUpdateAsync(s => s.SetProperty(m => m.ExpiresAt, now), ct);
    public static IQueryable<Session> Set(AppDbContext db) => db.Set<Session>();
    public static Session Make(Guid id, string hash, DateTimeOffset now) => Session.ForCustomer(id, hash, false, now);
    public static RecognizedBrowserMark Mark(Guid id, string hash, DateTimeOffset now) => new RecognizedBrowserMark { CustomerId = id, MarkHash = hash, ExpiresAt = now };
    public static Task<string?> Grant(ReauthenticationValues values, Guid id, CancellationToken ct) => values.GrantAsync(id, ReauthenticationOperation.PasswordChange, ct);
    public static Task Fields(AppDbContext db, Guid id, CancellationToken ct) => db.Sessions
        .Where(s => s.Id == id)
        .ExecuteUpdateAsync(s => s.SetProperty(x => x.ReauthenticatedOperation, (ReauthenticationOperation?)null), ct);
    public static Task<bool> ReadOnly(AppDbContext db, Guid id, CancellationToken ct) => db.Sessions.AnyAsync(s => s.Id == id, ct);
}
