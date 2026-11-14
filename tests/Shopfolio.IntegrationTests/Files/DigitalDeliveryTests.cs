using System.Globalization;
using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Web;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.IntegrationTests.Files;

/// <summary>
/// The digital product's delivery on the opened application and a real PostgreSQL (T340 criteria 1, 2, 4 to 9; KK1, KK2, KK4 to KK9; 02 3.12.1 to 3.12.9; 05 7.7.4; 09 10.2 item 4):
/// the download opens only for an item whose payment is confirmed; a variant without a file gives the product's; one conditional update takes a download, so ten requests at the
/// same moment on an allowance of three take exactly three and no clock closes the right; the firm's renewal puts the counter back to zero under the order's version; a changed
/// file changes neither the counter nor the allowance and the next download spends one; the repurchase check warns a member and never stops the add. The size ceiling (KK3, P-26)
/// is tested where the upload starts (<see cref="UploadEndpointsTests"/>) and at the check itself (<c>DigitalFileDeliveryTests</c>).
/// </summary>
[Collection(FileEndpointsDefinition.Name)]
public sealed class DigitalDeliveryTests
{
    private static readonly byte[] Hundred = [.. Enumerable.Range(0, 100).Select(i => (byte)i)];

    private static readonly byte[] Other = [.. Enumerable.Range(0, 60).Select(i => (byte)(255 - i))];

    private readonly CollectionDatabaseFixture _database;

    public DigitalDeliveryTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static Uri Link(string value) => new("/indir/" + value, UriKind.Relative);

    private static Task<DownloadTakeResult> TakeAsync(FileEndpointsHost host, Guid itemId)
        => host.InTransactionAsync((services, ct) => services.GetRequiredService<IOrderingModule>().TakeDownloadAsync(itemId, ct));

    private static Task<DownloadLinkIssue> IssueAsync(FileEndpointsHost host, Guid itemId)
        => host.InTransactionAsync((services, ct) => services.GetRequiredService<IDigitalDelivery>().IssueLinkAsync(itemId, ct));

    private static Task<DownloadRenewalResult> RenewAsync(FileEndpointsHost host, Guid itemId, int version)
        => host.InTransactionAsync((services, ct) => services.GetRequiredService<IOrderingModule>().RenewDownloadAllowanceAsync(itemId, version, ct));

    private static Task<DigitalAddCheck> CheckAddAsync(FileEndpointsHost host, DigitalAddRequest request)
        => host.InScopeAsync((services, ct) => services.GetRequiredService<IOrderingModule>().CheckDigitalAddAsync(request, ct));

    private static Task<DownloadableItem?> FindAsync(FileEndpointsHost host, Guid itemId)
        => host.InScopeAsync((services, ct) => services.GetRequiredService<IOrderingModule>().FindDownloadableItemAsync(itemId, ct));

    private static async Task<int> CountOfAsync(FileEndpointsHost host, Guid itemId)
        => (int)await host.CountAsync(string.Create(CultureInfo.InvariantCulture, $"SELECT download_count FROM order_items WHERE id = '{itemId}'"));

    private static async Task<int> AllowanceOfAsync(FileEndpointsHost host, Guid itemId)
        => (int)await host.CountAsync(string.Create(CultureInfo.InvariantCulture, $"SELECT download_allowance FROM order_items WHERE id = '{itemId}'"));

    private static async Task<int> VersionOfAsync(FileEndpointsHost host, Guid orderId)
    {
        await using AppDbContext db = host.NewContext();
        return await db.Orders.AsNoTracking().Where(o => o.Id == orderId).Select(o => o.Version).SingleAsync(Ct);
    }

    private static async Task<Guid> OrderOfAsync(FileEndpointsHost host, Guid itemId)
    {
        await using AppDbContext db = host.NewContext();
        return await db.OrderItems.AsNoTracking().Where(i => i.Id == itemId).Select(i => i.OrderId).SingleAsync(Ct);
    }

    // --- KK1: the download follows the confirmed payment (criterion 1) ---

    [Theory]
    [InlineData(PaymentStatus.Pending)]
    [InlineData(PaymentStatus.Failed)]
    [InlineData(PaymentStatus.Refunded)]
    public async Task Download_OfAnItemWhosePaymentIsNotConfirmed_IsRefusedEverywhere_AndTakesNothing_KK1(PaymentStatus payment)
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid item, _, _) = await host.SeedDownloadAsync(Hundred, payment: payment);

        DownloadTakeResult taken = await TakeAsync(host, item);
        DownloadLinkIssue issue = await IssueAsync(host, item);
        using HttpResponseMessage response = await host.Client.GetAsync(Link(host.LinkFor(item)), Ct);

        Assert.Same(DownloadTakeResult.NotAvailable, taken);
        Assert.Same(DownloadLinkIssue.NotAvailable, issue);
        Assert.Null(await FindAsync(host, item));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.StartsWith("text/html", response.Content.Headers.ContentType?.ToString(), StringComparison.Ordinal);
        Assert.Contains("<h1 class=\"not-found__title\">Sayfa bulunamadı</h1>", System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)), StringComparison.Ordinal);
        Assert.Equal(0, await CountOfAsync(host, item));
    }

    [Theory]
    [InlineData(PaymentStatus.Paid)]
    [InlineData(PaymentStatus.PartiallyRefunded)]
    public async Task Download_OfAnItemWhosePaymentIsConfirmed_Opens_KK1(PaymentStatus payment)
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid item, _, _) = await host.SeedDownloadAsync(Hundred, payment: payment);

        DownloadLinkIssue issue = await IssueAsync(host, item);
        using HttpResponseMessage response = await host.Client.GetAsync(Link(issue.Value!), Ct);

        Assert.False(issue.IsRejected);
        Assert.NotNull(await FindAsync(host, item));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Hundred, await response.Content.ReadAsByteArrayAsync(Ct));
        Assert.Equal(1, await CountOfAsync(host, item));
    }

    [Fact]
    public async Task Download_WhenTheTransferIsMarkedPaidLater_OpensFromThatMoment_KK1()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid item, _, _) = await host.SeedDownloadAsync(Hundred, payment: PaymentStatus.Pending);
        Guid order = await OrderOfAsync(host, item);
        DownloadTakeResult before = await TakeAsync(host, item);

        await host.SetPaymentAsync(order, PaymentStatus.Paid);
        DownloadTakeResult after = await TakeAsync(host, item);

        Assert.Same(DownloadTakeResult.NotAvailable, before);
        Assert.Same(DownloadTakeResult.Taken, after);
    }

    [Fact]
    public async Task Download_OfAnItemThatIsNotDigital_OrDoesNotExist_IsNotAvailable()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid physical, _, _) = await host.SeedDownloadAsync(Hundred, itemType: ProductType.Physical);

        Assert.Same(DownloadTakeResult.NotAvailable, await TakeAsync(host, physical));
        Assert.Same(DownloadTakeResult.NotAvailable, await TakeAsync(host, Guid.NewGuid()));
        Assert.Equal(0, await CountOfAsync(host, physical));
    }

    // --- KK2: the variant's file or, without one, the product's (criterion 2) ---

    [Fact]
    public async Task Download_OfAVariantWithoutAFile_GivesTheProductsFile_KK2()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid item, Guid variantId, Guid fileId) = await host.SeedDownloadAsync(Hundred);
        await using (AppDbContext db = host.NewContext())
        {
            Guid productId = await db.Variants.Where(v => v.Id == variantId).Select(v => v.ProductId).SingleAsync(Ct);
            DigitalFile variantFile = await db.DigitalFiles.SingleAsync(f => f.Id == fileId, Ct);
            variantFile.DetachedAt = host.Time.GetUtcNow();
            await db.SaveChangesAsync(Ct);

            DateTimeOffset now = host.Time.GetUtcNow();
            DigitalFile productFile = DigitalFile.Create(EntityIds.NewStorageKey(), "urun.pdf", Other.Length, now);
            productFile.ProductId = productId;
            productFile.AttachedAt = now;
            productFile.UploadCompletedAt = now;
            using MemoryStream stream = new(Other);
            await host.Files.WriteAsync(Shopfolio.Infrastructure.Files.FileArea.DigitalFile, productFile.StorageKey, stream, Ct);
            db.DigitalFiles.Add(productFile);
            await db.SaveChangesAsync(Ct);
        }

        DownloadLinkIssue issue = await IssueAsync(host, item);
        using HttpResponseMessage response = await host.Client.GetAsync(Link(issue.Value!), Ct);

        Assert.False(issue.IsRejected);
        Assert.Equal(Other, await response.Content.ReadAsByteArrayAsync(Ct));
    }

    [Fact]
    public async Task IssueLink_WhenThereIsNoFileToGive_SpendsNoRight_KK2()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid item, _, Guid fileId) = await host.SeedDownloadAsync(Hundred);
        await using (AppDbContext db = host.NewContext())
        {
            await db.Database.ExecuteSqlAsync($"UPDATE order_items SET digital_file_id = NULL WHERE id = {item}", Ct);
            await db.Database.ExecuteSqlAsync($"UPDATE digital_files SET variant_id = NULL, detached_at = {host.Time.GetUtcNow()} WHERE id = {fileId}", Ct);
        }

        DownloadLinkIssue issue = await IssueAsync(host, item);

        Assert.Same(DownloadLinkIssue.NoFile, issue);
        Assert.Equal(0, await CountOfAsync(host, item));
    }

    // --- KK4: the quantity and the second add (criterion 4) ---

    [Fact]
    public async Task CheckDigitalAdd_RefusesAQuantityAboveOne_AndTheSameVariantTwiceInACart_KK4()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, Guid variantId, _) = await host.SeedDownloadAsync(Hundred);
        Guid cartId = await SeedCartAsync(host, variantId);

        DigitalAddCheck two = await CheckAddAsync(host, new DigitalAddRequest(variantId, ProductType.Digital, 2, null, null));
        DigitalAddCheck one = await CheckAddAsync(host, new DigitalAddRequest(variantId, ProductType.Digital, 1, null, null));
        DigitalAddCheck again = await CheckAddAsync(host, new DigitalAddRequest(variantId, ProductType.Digital, 1, cartId, null));
        DigitalAddCheck physical = await CheckAddAsync(host, new DigitalAddRequest(variantId, ProductType.Physical, 5, cartId, null));

        Assert.False(two.IsQuantityAllowed);
        Assert.True(two.IsBlocked);
        Assert.False(one.IsBlocked);
        Assert.True(again.IsAlreadyInCart);
        Assert.True(again.IsBlocked);
        Assert.False(physical.IsBlocked);
    }

    // --- KK5: the right is a count, never a clock (criterion 5) ---

    [Fact]
    public async Task Take_WhenTheAllowanceIsUsed_IsRefused_AndNoTimePassingChangesIt_KK5()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid item, _, _) = await host.SeedDownloadAsync(Hundred, allowance: 2);

        Assert.Same(DownloadTakeResult.Taken, await TakeAsync(host, item));
        Assert.Same(DownloadTakeResult.Taken, await TakeAsync(host, item));
        Assert.Same(DownloadTakeResult.AllowanceUsed, await TakeAsync(host, item));
        host.Time.Advance(TimeSpan.FromDays(800));
        DownloadTakeResult later = await TakeAsync(host, item);
        DownloadLinkIssue issue = await IssueAsync(host, item);

        Assert.Same(DownloadTakeResult.AllowanceUsed, later);
        Assert.Same(DownloadLinkIssue.AllowanceUsed, issue);
        Assert.Equal(2, await CountOfAsync(host, item));
        Assert.Equal(2, await AllowanceOfAsync(host, item));
    }

    [Fact]
    public async Task Take_TenRequestsAtOnceOnAnAllowanceOfThree_TakeExactlyThree_KK5()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid item, _, _) = await host.SeedDownloadAsync(Hundred, allowance: 3);

        DownloadTakeResult[] results = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => Task.Run(() => TakeAsync(host, item), Ct)));

        Assert.Equal(3, results.Count(r => ReferenceEquals(r, DownloadTakeResult.Taken)));
        Assert.Equal(7, results.Count(r => ReferenceEquals(r, DownloadTakeResult.AllowanceUsed)));
        Assert.Equal(3, await CountOfAsync(host, item));
    }

    // --- KK6: the firm's renewal (criterion 6) ---

    [Fact]
    public async Task Renew_PutsTheCounterBackToZero_RaisesTheOrdersVersionByOne_AndTheBuyerCanDownloadAgain_KK6()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid item, _, _) = await host.SeedDownloadAsync(Hundred, allowance: 1);
        Guid order = await OrderOfAsync(host, item);
        await TakeAsync(host, item);
        int version = await VersionOfAsync(host, order);

        DownloadRenewalResult renewed = await RenewAsync(host, item, version);

        Assert.Same(DownloadRenewalResult.Renewed, renewed);
        Assert.Equal(0, await CountOfAsync(host, item));
        Assert.Equal(1, await AllowanceOfAsync(host, item));
        Assert.Equal(version + 1, await VersionOfAsync(host, order));
        Assert.Same(DownloadTakeResult.Taken, await TakeAsync(host, item));
    }

    [Fact]
    public async Task Renew_WithAStaleVersion_OnANonDigitalItem_OrAMissingOne_IsRefused_AndChangesNothing_KK6()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid item, _, _) = await host.SeedDownloadAsync(Hundred, allowance: 1);
        (Guid physical, _, _) = await host.SeedDownloadAsync(Hundred, itemType: ProductType.Physical);
        Guid order = await OrderOfAsync(host, item);
        await TakeAsync(host, item);
        int version = await VersionOfAsync(host, order);

        DownloadRenewalResult stale = await RenewAsync(host, item, version + 1);
        DownloadRenewalResult notDigital = await RenewAsync(host, physical, await VersionOfAsync(host, await OrderOfAsync(host, physical)));
        DownloadRenewalResult missing = await RenewAsync(host, Guid.NewGuid(), 1);

        Assert.Same(DownloadRenewalResult.VersionConflict, stale);
        Assert.Same(DownloadRenewalResult.NotAvailable, notDigital);
        Assert.Same(DownloadRenewalResult.NotFound, missing);
        Assert.Equal(1, await CountOfAsync(host, item));
        Assert.Equal(version, await VersionOfAsync(host, order));
    }

    // --- KK7 and KK8: the current file, and an update that grants nothing (criteria 7, 8) ---

    [Fact]
    public async Task ReplacedFile_IsWhatTheEarlierBuyerGets_AndTheCounterAndTheAllowanceDoNotChange_ButTheNextDownloadSpendsOne_KK7_KK8()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid item, Guid variantId, _) = await host.SeedDownloadAsync(Hundred, allowance: 3);
        await TakeAsync(host, item);

        await host.ReplaceFileAsync(variantId, Other);

        Assert.Equal(1, await CountOfAsync(host, item));
        Assert.Equal(3, await AllowanceOfAsync(host, item));
        DownloadLinkIssue issue = await IssueAsync(host, item);
        using HttpResponseMessage response = await host.Client.GetAsync(Link(issue.Value!), Ct);
        Assert.Equal(Other, await response.Content.ReadAsByteArrayAsync(Ct));
        Assert.Equal(2, await CountOfAsync(host, item));
        Assert.Equal(3, await AllowanceOfAsync(host, item));
    }

    [Fact]
    public async Task RemovedFileWithoutANewOne_AndADeletedProduct_GiveTheLastFile_KK7()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid removed, _, Guid removedFile) = await host.SeedDownloadAsync(Hundred);
        (Guid gone, Guid goneVariant, _) = await host.SeedDownloadAsync(Other);
        await using (AppDbContext db = host.NewContext())
        {
            await db.Database.ExecuteSqlAsync($"UPDATE digital_files SET detached_at = {host.Time.GetUtcNow()} WHERE id = {removedFile}", Ct);
        }

        await using (AppDbContext db = host.NewContext())
        {
            Guid productId = await db.Variants.Where(v => v.Id == goneVariant).Select(v => v.ProductId).SingleAsync(Ct);
            await db.Database.ExecuteSqlAsync($"UPDATE order_items SET variant_id = NULL, product_id = NULL WHERE id = {gone}", Ct);
            await db.Database.ExecuteSqlAsync($"UPDATE digital_files SET variant_id = NULL, detached_at = {host.Time.GetUtcNow()} WHERE variant_id = {goneVariant}", Ct);
            await db.Database.ExecuteSqlAsync($"DELETE FROM variants WHERE id = {goneVariant}", Ct);
            await db.Database.ExecuteSqlAsync($"DELETE FROM products WHERE id = {productId}", Ct);
        }

        using HttpResponseMessage first = await host.Client.GetAsync(Link((await IssueAsync(host, removed)).Value!), Ct);
        using HttpResponseMessage second = await host.Client.GetAsync(Link((await IssueAsync(host, gone)).Value!), Ct);

        Assert.Equal(Hundred, await first.Content.ReadAsByteArrayAsync(Ct));
        Assert.Equal(Other, await second.Content.ReadAsByteArrayAsync(Ct));
    }

    // --- D-527 (T525): the form's binding, the removal and the deletion move the item's last known file (06 4.70 "Uygulamada") ---

    [Fact]
    public async Task AFileTheFormBinds_ThenTheProductDeleted_LeavesTheItemTheBoundFile_D527()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid item, Guid variantId, _) = await host.SeedDownloadAsync(Hundred);
        Guid bound = await SeedUploadAsync(host, Other);
        (Guid productId, int version, string sku) = await ProductOfAsync(host, variantId);

        ProductSaveResult saved = await host.InTransactionAsync((s, ct) => s.GetRequiredService<ProductSaves>().SaveAsync(BindCommand(productId, version, variantId, sku, bound), ct));
        Guid? afterBinding = await ItemFileAsync(host, item);
        RemovalResult deleted = await host.InTransactionAsync((s, ct) => s.GetRequiredService<IProductRemoval>().DeleteProductAsync(productId, version + 1, ct));

        Assert.False(saved.IsRejected);
        Assert.Equal(bound, afterBinding);
        Assert.Same(RemovalResult.Done, deleted);
        Assert.Equal(bound, await ItemFileAsync(host, item));
        using HttpResponseMessage response = await host.Client.GetAsync(Link((await IssueAsync(host, item)).Value!), Ct);
        Assert.Equal(Other, await response.Content.ReadAsByteArrayAsync(Ct));
    }

    [Fact]
    public async Task AFileTheFormBinds_ThenRemovedWithoutANewOne_StaysTheItemsLastFile_D527()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid item, Guid variantId, _) = await host.SeedDownloadAsync(Hundred);
        Guid bound = await SeedUploadAsync(host, Other);
        (Guid productId, int version, string sku) = await ProductOfAsync(host, variantId);

        await host.InTransactionAsync((s, ct) => s.GetRequiredService<ProductSaves>().SaveAsync(BindCommand(productId, version, variantId, sku, bound), ct));
        RemovalResult removed = await host.InTransactionAsync((s, ct) => s.GetRequiredService<IProductRemoval>().RemoveDigitalFileAsync(bound, ct));

        Assert.Same(RemovalResult.Done, removed);
        Assert.Equal(bound, await ItemFileAsync(host, item));
        using HttpResponseMessage response = await host.Client.GetAsync(Link((await IssueAsync(host, item)).Value!), Ct);
        Assert.Equal(Other, await response.Content.ReadAsByteArrayAsync(Ct));
    }

    private static ProductSaveCommand BindCommand(Guid productId, int version, Guid variantId, string sku, Guid fileId)
        => new(
            productId, version, ProductType.Digital, "Kitap", null, 20m, null, null, null, null, null, null,
            [], null, [], [new ProductSaveVariant(variantId, null, null, sku, 10m, null, null, null, null, fileId)], [], null, null);

    private static async Task<(Guid ProductId, int Version, string Sku)> ProductOfAsync(FileEndpointsHost host, Guid variantId)
    {
        await using AppDbContext db = host.NewContext();
        Variant variant = await db.Variants.AsNoTracking().SingleAsync(v => v.Id == variantId, Ct);
        int version = await db.Products.Where(p => p.Id == variant.ProductId).Select(p => p.Version).SingleAsync(Ct);
        return (variant.ProductId, version, variant.SKU);
    }

    private static async Task<Guid?> ItemFileAsync(FileEndpointsHost host, Guid itemId)
    {
        await using AppDbContext db = host.NewContext();
        return await db.OrderItems.Where(i => i.Id == itemId).Select(i => i.DigitalFileId).SingleAsync(Ct);
    }

    /// <summary>A completed upload of the given bytes that no plane holds yet - what the form binds.</summary>
    private static async Task<Guid> SeedUploadAsync(FileEndpointsHost host, byte[] content)
    {
        DateTimeOffset now = host.Time.GetUtcNow();
        DigitalFile file = DigitalFile.Create(EntityIds.NewStorageKey(), "yeni.pdf", content.Length, now);
        file.UploadCompletedAt = now;
        using (MemoryStream stream = new(content))
        {
            await host.Files.WriteAsync(Shopfolio.Infrastructure.Files.FileArea.DigitalFile, file.StorageKey, stream, Ct);
        }

        await using AppDbContext db = host.NewContext();
        db.DigitalFiles.Add(file);
        await db.SaveChangesAsync(Ct);
        return file.Id;
    }

    // --- KK9: the repurchase warning (criterion 9) ---

    [Fact]
    public async Task CheckDigitalAdd_WarnsAMemberWhoBoughtItBefore_AndNeverStopsTheAdd_KK9()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, Guid variantId, _) = await host.SeedDownloadAsync(Hundred);
        (_, Guid memberId) = await TestSessions.CustomerAsync(_database.Dsn);
        await host.SeedPurchaseAsync(variantId, memberId, PaymentStatus.Paid);

        DigitalAddCheck check = await CheckAddAsync(host, new DigitalAddRequest(variantId, ProductType.Digital, 1, null, memberId));

        Assert.True(check.WasBoughtBefore);
        Assert.True(check.ShowsRepurchaseWarning);
        Assert.False(check.IsBlocked);
    }

    [Fact]
    public async Task CheckDigitalAdd_GivesAGuestNoWarning_EvenWhenAGuestOrderOfTheVariantExists_KK9()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, Guid variantId, _) = await host.SeedDownloadAsync(Hundred);
        await host.SeedPurchaseAsync(variantId, null, PaymentStatus.Paid);

        DigitalAddCheck check = await CheckAddAsync(host, new DigitalAddRequest(variantId, ProductType.Digital, 1, null, null));

        Assert.False(check.WasBoughtBefore);
        Assert.False(check.ShowsRepurchaseWarning);
        Assert.False(check.IsBlocked);
    }

    [Theory]
    [InlineData(PaymentStatus.Pending)]
    [InlineData(PaymentStatus.Failed)]
    [InlineData(PaymentStatus.Refunded)]
    public async Task CheckDigitalAdd_GivesNoWarning_WhenTheEarlierOrderWasNotConfirmed_OrBelongsToAnotherMember_KK9(PaymentStatus earlier)
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, Guid variantId, _) = await host.SeedDownloadAsync(Hundred);
        (_, Guid member) = await TestSessions.CustomerAsync(_database.Dsn);
        (_, Guid stranger) = await TestSessions.CustomerAsync(_database.Dsn);
        await host.SeedPurchaseAsync(variantId, member, earlier);
        await host.SeedPurchaseAsync(variantId, stranger, PaymentStatus.Paid);

        DigitalAddCheck check = await CheckAddAsync(host, new DigitalAddRequest(variantId, ProductType.Digital, 1, null, member));

        Assert.False(check.WasBoughtBefore);
        Assert.False(check.ShowsRepurchaseWarning);
    }

    private static async Task<Guid> SeedCartAsync(FileEndpointsHost host, Guid variantId)
    {
        DateTimeOffset now = host.Time.GetUtcNow();
        Cart cart = Cart.ForGuest("guest-key-digest-" + Guid.NewGuid().ToString("N"), now);
        await using AppDbContext db = host.NewContext();
        db.Carts.Add(cart);
        db.CartItems.Add(CartItem.Create(cart.Id, variantId, 1, 10m, now));
        await db.SaveChangesAsync(Ct);
        return cart.Id;
    }
}
