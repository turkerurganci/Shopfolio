using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Payment.Contracts;

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
        db.Orders.Add(order);
        await db.SaveChangesAsync(CancellationToken.None);
        return (order.Id, number, generated);
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
