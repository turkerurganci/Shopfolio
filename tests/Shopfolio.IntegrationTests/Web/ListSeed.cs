using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The products the list tests read (T210): the same rows for the page tests and the paging tests, so a test names the row by its number.</summary>
internal static class ListSeed
{
    public static readonly DateTimeOffset First = new(2026, 11, 1, 9, 0, 0, TimeSpan.Zero);

    /// <summary>"Urun 001" - the name of the product made for the number; the three-digit number sorts as text.</summary>
    public static string NameOf(int number) => $"Urun {number:000}";

    /// <summary>The published products numbered 1..count, created one minute apart (number n at <see cref="First"/> plus n minutes), each with one variant priced n and, for even n, five in stock.</summary>
    public static async Task SeedPublishedAsync(string dsn, int count)
    {
        await using AppDbContext db = Open(dsn);
        for (int n = 1; n <= count; n++)
        {
            Product product = Product.Create(NameOf(n), $"urun-{n:000}", ProductType.Physical, 20m, First.AddMinutes(n));
            product.PublishStatus = PublishStatus.Published;
            db.Products.Add(product);
            Variant variant = Variant.Create(product.Id, $"SKU-{n:000}", n, First.AddMinutes(n));
            variant.PublishStatus = PublishStatus.Published;
            variant.StockQuantity = n % 2 == 0 ? 5 : 0;
            db.Variants.Add(variant);
        }

        await db.SaveChangesAsync(CancellationToken.None);
    }

    /// <summary>Products with the given names, published or not, all created at the same moment: ties for the order.</summary>
    public static async Task<IReadOnlyList<Guid>> SeedTiedAsync(string dsn, IReadOnlyList<string> names, DateTimeOffset createdAt, PublishStatus status)
    {
        await using AppDbContext db = Open(dsn);
        List<Guid> ids = [];
        for (int i = 0; i < names.Count; i++)
        {
            Product product = Product.Create(names[i], $"tied-{Guid.NewGuid():N}", ProductType.Physical, 20m, createdAt);
            product.PublishStatus = status;
            db.Products.Add(product);
            ids.Add(product.Id);
        }

        await db.SaveChangesAsync(CancellationToken.None);
        return ids;
    }

    public static async Task ClearAsync(string dsn)
    {
        await using AppDbContext db = Open(dsn);
        await db.Database.ExecuteSqlRawAsync("TRUNCATE variants, products CASCADE", CancellationToken.None);
    }

    public static AppDbContext Open(string dsn)
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, dsn);
        return new AppDbContext(builder.Options);
    }
}
