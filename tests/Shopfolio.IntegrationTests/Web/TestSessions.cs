using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// Seeds accounts, session rows and orders into the collection database for the tests that sign in (T207): the raw session key is made at run time by
/// <see cref="SecretKey.Create"/> and only its digest is stored, the way the application keeps it (09 15.1 item 1). A test sends the key in the cookie.
/// </summary>
internal static class TestSessions
{
    public static async Task<(string Key, Guid AdminId)> AdminAsync(string dsn, bool longLived = false, DateTimeOffset? lastActivity = null)
    {
        string key = SecretKey.Create();
        DateTimeOffset at = lastActivity ?? DateTimeOffset.UtcNow;
        Admin admin = Admin.Create("Test yonetici", $"admin-{Guid.NewGuid():N}@example.com", "digest-placeholder", at);
        await using AppDbContext db = NewContext(dsn);
        db.Admins.Add(admin);
        db.Sessions.Add(Session.ForAdmin(admin.Id, SecretKey.Hash(key), longLived, at));
        await db.SaveChangesAsync(CancellationToken.None);
        return (key, admin.Id);
    }

    public static async Task<(string Key, Guid CustomerId)> CustomerAsync(string dsn, bool longLived = false, DateTimeOffset? lastActivity = null)
    {
        (string key, Guid customerId, _) = await CustomerWithSessionAsync(dsn, longLived, lastActivity);
        return (key, customerId);
    }

    public static async Task<(string Key, Guid CustomerId, Guid SessionId)> CustomerWithSessionAsync(
        string dsn, bool longLived = false, DateTimeOffset? lastActivity = null)
    {
        string key = SecretKey.Create();
        DateTimeOffset at = lastActivity ?? DateTimeOffset.UtcNow;
        Customer customer = Customer.Create("Test musteri", $"customer-{Guid.NewGuid():N}@example.com", "digest-placeholder", at, null, at);
        Session session = Session.ForCustomer(customer.Id, SecretKey.Hash(key), longLived, at);
        await using AppDbContext db = NewContext(dsn);
        db.Customers.Add(customer);
        db.Sessions.Add(session);
        await db.SaveChangesAsync(CancellationToken.None);
        return (key, customer.Id, session.Id);
    }

    /// <summary>An order of the given customer (a guest order when null) with a made-up number and a made-up access key; returns the order, the number and the key.</summary>
    public static async Task<(Guid OrderId, string Number, string AccessKey)> OrderAsync(string dsn, Guid? customerId)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        string number = $"2026-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        string generated = SecretKey.Create();
        Order order = Order.Create(number, generated, SecretKey.Create(), PaymentMethod.BankTransfer, customerId is null, "alici@example.com", 100m, 0m, now.AddDays(1), now);
        order.CustomerId = customerId;
        await using AppDbContext db = NewContext(dsn);
        (Guid provinceId, Guid formId, Guid contractId, Guid noticeId) = await ReferencesAsync(db, now);
        order.IpAddress = System.Net.IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.BillingRecipientName = "Ali Veli";
        order.BillingProvinceId = provinceId;
        order.BillingDistrict = "Cankaya";
        order.BillingAddressLine = "Ataturk Bulvari 1";
        order.BankTransferIban = "TR000000000000000000000000";
        order.CompanyType = CompanyType.Tradesman;
        order.CompanyLegalName = "Ornek Ticaret";
        order.CompanyTaxIdNumber = "1234567890";
        order.CompanyProvinceId = provinceId;
        order.CompanyDistrict = "Cankaya";
        order.CompanyAddressLine = "Kizilay 2";
        order.CompanyPhone = "3121234567";
        order.CompanyEmail = "firma@example.com";
        order.CompanyKepAddress = "firma@hs01.kep.tr";
        order.PreInformationFormVersionId = formId;
        order.DistanceSalesContractVersionId = contractId;
        order.PrivacyNoticeVersionId = noticeId;
        order.FreezeDocuments("on bilgilendirme", "mesafeli satis");
        db.Orders.Add(order);
        await db.SaveChangesAsync(CancellationToken.None);
        return (order.Id, number, generated);
    }

    /// <summary>The province and the three published legal texts every order points at; made once per database and found again after that.</summary>
    private static async Task<(Guid ProvinceId, Guid FormId, Guid ContractId, Guid NoticeId)> ReferencesAsync(AppDbContext db, DateTimeOffset now)
    {
        if (!await db.Provinces.AnyAsync())
        {
            db.Provinces.Add(Province.Create("06", "Ankara", now));
            await db.SaveChangesAsync(CancellationToken.None);
        }

        Guid province = await db.Provinces.Select(p => p.Id).FirstAsync();
        return (
            province,
            await PublishedTextAsync(db, LegalTextType.PreInformationForm, now),
            await PublishedTextAsync(db, LegalTextType.DistanceSalesContract, now),
            await PublishedTextAsync(db, LegalTextType.PrivacyNotice, now));
    }

    private static async Task<Guid> PublishedTextAsync(AppDbContext db, LegalTextType type, DateTimeOffset now)
    {
        Guid? found = await db.LegalTextVersions.Where(l => l.LegalTextType == type && l.VersionNumber != null).Select(l => (Guid?)l.Id).FirstOrDefaultAsync();
        if (found is not null)
        {
            return found.Value;
        }

        LegalTextVersion text = LegalTextVersion.CreatePublished(type, 1, now, "metin", now);
        db.LegalTextVersions.Add(type == LegalTextType.PrivacyNotice ? text.WithHosting() : text);
        await db.SaveChangesAsync(CancellationToken.None);
        return text.Id;
    }

    public static async Task<DateTimeOffset> LastActivityAsync(string dsn, Guid sessionId)
    {
        await using AppDbContext db = NewContext(dsn);
        return await db.Sessions.AsNoTracking().Where(s => s.Id == sessionId).Select(s => s.LastActivityAt).SingleAsync();
    }

    private static AppDbContext NewContext(string dsn)
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, dsn);
        return new AppDbContext(builder.Options);
    }
}
