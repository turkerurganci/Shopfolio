// Clean sample of the start-up data source scan: reading and changing the start-up rows, and adding a delivery province, are allowed.
// This file is excluded from compilation and from the real scan; the scan test reads it as text.
public static class StartupDataCleanSample
{
    public static async Task ChangeAsync(AppDbContext db, Guid provinceId, DateTimeOffset now)
    {
        Company company = await db.Companies.SingleAsync();
        company.LegalName = "Örnek";
        db.CompanyDeliveryProvinces.Add(CompanyDeliveryProvince.Create(provinceId, now));
        db.LegalTextVersions.Add(LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 1, now, "<p>x</p>", now));
        // db.Companies.Add(Company.Create(now)) - a comment line is not code.
        await db.Database.ExecuteSqlRawAsync("UPDATE public.heartbeat_records SET last_heartbeat_at = @p0", now);
    }
}
