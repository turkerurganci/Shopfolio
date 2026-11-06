// Deliberate violation sample of the client address and attempt record scan (09 15.1 item 8, 15.4 item 5). The scan reads this file as text; it is not compiled.
// Line numbers are part of the test.
internal static class Misuse
{
    public static string Raw(HttpContext c) => c.Connection.RemoteIpAddress.ToString();
    public static string Optional(HttpContext c) => c.Connection?.RemoteIpAddress?.ToString() ?? "";
    public static bool Pattern(HttpContext c) => c.Connection
        .RemoteIpAddress is { } a && a.IsIPv6LinkLocal;
    public static string Loopback(HttpContext c) => c.Request.Headers["X-Forwarded-For"].ToString();
    public static void Trust(IServiceCollection s) => s.Configure<ForwardedHeadersOptions>(o => o.ForwardedHeaders = ForwardedHeaders.All);
    public static void Use(IApplicationBuilder app) => app.UseForwardedHeaders();
    public static void Make(AppDbContext db, AttemptSubject who)
    {
        db.AttemptRecords.Add(AttemptRecord.Create(AttemptLimit.L1, who, DateTimeOffset.UtcNow));
        db.Set<AttemptRecord>().Add(Shopfolio.Infrastructure.Security.AttemptRecord.Create(
            now: DateTimeOffset.UtcNow,
            subject: who,
            attemptLimit: AttemptLimit.L1));
        AttemptRecord row = new AttemptRecord { CreatedAt = DateTimeOffset.UtcNow };
        db.Add(new
            Shopfolio.Infrastructure.Security.AttemptRecord());
        IQueryable<AttemptRecord> all = db.Query();
        AttemptSubject forged = new AttemptSubject(AttemptAxis.Ip, "198.51.100.1");
        AttemptSubject qualified = new Shopfolio.Infrastructure.Security.AttemptSubject(AttemptAxis.Email, "a@b.c");
    }
    // A comment that says RemoteIpAddress or X-Forwarded-For or AttemptRecords.Add( or new AttemptSubject( is not a finding.
}
