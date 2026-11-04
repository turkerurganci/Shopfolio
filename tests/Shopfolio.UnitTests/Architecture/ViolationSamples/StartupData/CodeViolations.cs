// Deliberate violation sample of the start-up data source scan (09 19.2 item 3): "create it when it is missing" paths in application code.
// This file is excluded from compilation and from the real scan; the scan test reads it as text. The violations are on lines 7-14.
public static class StartupDataCodeSample
{
    public static void EnsureRows(AppDbContext db, DateTimeOffset now)
    {
        if (!db.Companies.Any()) { db.Companies.Add(Company.Create(now)); }
        db.Provinces.AddRange(Province.Create("82", "Yeni", now));
        db.HeartbeatRecords.Add(new HeartbeatRecord { CreatedAt = now });
        db.LegalTextVersions.Add(LegalTextVersion.CreateDraft(LegalTextType.PrivacyNotice, "<p>x</p>", now));
        db.Database.ExecuteSqlRaw("INSERT INTO public.company_settings (id) VALUES ('x')");
        db.Database.ExecuteSqlRaw("insert into countries (id) values ('x')");
        await db.Brandings.AddAsync(Branding.Create(now));
        db.ChannelStateRecords.Add(new ChannelStateRecord());
    }
}
