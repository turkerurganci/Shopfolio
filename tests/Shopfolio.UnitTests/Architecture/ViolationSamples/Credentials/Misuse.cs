// Deliberate violation sample of the sign-in record and password scan (02 3.13.19; 09 15.1 item 5). The scan reads this file as text; it is not compiled.
// Line numbers are part of the test.
internal static class Misuse
{
    public static void Make(AppDbContext db, IPAddress ip)
    {
        db.LoginRecords.Add(LoginRecord.Create(ip, AccountType.Customer, "a@b.c", LoginOutcome.Failed, DateTimeOffset.UtcNow));
        db.Set<Shopfolio.Infrastructure.Security.LoginRecord>().AddRange(Shopfolio.Infrastructure.Security.LoginRecord
            .Create(
                now: DateTimeOffset.UtcNow,
                email: "a@b.c",
                ipAddress: ip,
                accountType: AccountType.Admin,
                outcome: LoginOutcome.Succeeded));
        LoginRecord row = new LoginRecord { NormalizedEmail = "a@b.c" };
        db.Add(new
            Shopfolio.Infrastructure.Security.LoginRecord());
        db.LoginRecords
            .AddAsync(row);
    }

    public static string Hash(string password) => new PasswordHasher<object>().HashPassword(new object(), password);
    public static IPasswordHasher<object> Own() => new Microsoft.AspNetCore.Identity.PasswordHasher<object>(new Microsoft.Extensions.Options.OptionsWrapper<PasswordHasherOptions>(new()));
    // A comment that says LoginRecord.Create( or LoginRecords.Add( or new LoginRecord or PasswordHasher<object> is not a finding.
}
