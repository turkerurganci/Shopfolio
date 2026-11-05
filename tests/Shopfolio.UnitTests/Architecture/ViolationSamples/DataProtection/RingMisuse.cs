// Deliberate violation sample of the data protection scan (09 15.5 items 1 and 2). The scan reads this file as text; it is not compiled.
// Line numbers are part of the test.
internal static class RingMisuse
{
    public static void Setup(IServiceCollection services) => services.AddDataProtection().ProtectKeysWithDpapi().SetDefaultKeyLifetime(TimeSpan.FromDays(30));
    public static IDataProtector Literal(IDataProtectionProvider p) => p.CreateProtector("Shopfolio.DownloadLink.v1");
    public static IDataProtector Variable(IDataProtectionProvider p, string purpose) => p.CreateProtector(purpose);
    public static IDataProtector Wrapped(IDataProtectionProvider p) => p.CreateProtector(
        "other");
    // A comment that says CreateProtector("x") or ProtectKeysWithDpapi is not a finding.
    public static IDataProtector Constant(IDataProtectionProvider p) => p.CreateProtector(DataProtectionPurposes.DownloadLink);
    public static IDataProtector Chained(IDataProtectionProvider p) => p.CreateProtector(DataProtectionPurposes.CheckoutCookie, "extra");
    public const string Purpose = "Shopfolio.Invented.v2";
    public const string NotAPurpose = "Shopfolio.Infrastructure.PublicHolidays.txt";
}
