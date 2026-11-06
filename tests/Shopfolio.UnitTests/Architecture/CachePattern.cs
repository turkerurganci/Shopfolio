using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;

// The in-memory cache pattern as the application's modules and a REAL storefront page use it (T229; 09 10.4 items 1 to 5, 6 items 2 and 7; 05 7.10.1, 4.1.5).
// The namespaces are deliberately the application's: the module's reads in the module's namespace, its open surface in the module's Contracts namespace, the
// page in the page namespace - the rules of ModuleBoundaryTests read types by those namespaces, so the architecture test sees the shape (09 6 item 2). The file
// is linked into the integration test project too, where the same types run against a real PostgreSQL and the test host's sample page (Pages/Storefront/
// CachePatternSample.cshtml) renders this page model. The real readers are the page tasks' (T313 item 9, T361, T537) and the real settings writer is
// CompanySettingsWriter (09 10.3 item 2); the stand-ins below only show where the cache sits.
#pragma warning disable IDE0130
namespace Shopfolio.Modules.Settings.Contracts
{
    /// <summary>The settings a storefront page shows, read through the cache (09 10.4 item 5: the display path). The sales gate's switch is not here (09 10.4 item 1).</summary>
    public sealed record PatternShopSettingsView(decimal ShippingFee, decimal? FreeShippingThreshold, decimal? MinimumOrderAmount);

    /// <summary>The values the order confirmation's transaction reads from the database: the gate's switch and the settings that change the amount (05 4.1.5; K-980).</summary>
    public sealed record PatternConfirmationSettings(bool IsSalesTemporarilyClosed, decimal ShippingFee, decimal? FreeShippingThreshold, decimal? MinimumOrderAmount);

    /// <summary>
    /// The two read paths of a module that serves the same area for display and for a calculation (09 10.4 item 5): the display read goes through the
    /// cache, the confirmation read goes to the database in the caller's transaction. A save joins the caller's transaction and names the area it makes stale.
    /// </summary>
    public interface IPatternShopSettings
    {
        Task<PatternShopSettingsView> GetForDisplayAsync(CancellationToken ct);

        Task<PatternConfirmationSettings> ReadForConfirmationAsync(CancellationToken ct);

        Task<bool> SaveShippingFeeAsync(decimal shippingFee, int shippingFeeVersion, CancellationToken ct);
    }
}

namespace Shopfolio.Modules.Settings
{
    using Shopfolio.Modules.Settings.Contracts;

    /// <summary>The Settings module's side of the pattern: the cache is reached here, inside the module, never from a page.</summary>
    internal sealed class PatternShopSettings(AppDbContext db, AppCache cache, CacheInvalidations invalidations) : IPatternShopSettings
    {
        public Task<PatternShopSettingsView> GetForDisplayAsync(CancellationToken ct)
        {
            return cache.GetOrLoadAsync(CacheArea.Settings, token => LoadDisplayAsync(db, token), ct);
        }

        /// <summary>The display value's load: an untracked projection into an immutable record (09 10.4 item 1; 20 item 3).</summary>
        internal static Task<PatternShopSettingsView> LoadDisplayAsync(AppDbContext db, CancellationToken ct)
        {
            return db.CompanySettings.AsNoTracking()
                .Select(s => new PatternShopSettingsView(s.ShippingFee, s.FreeShippingThreshold, s.MinimumOrderAmount))
                .SingleAsync(ct);
        }

        public async Task<PatternConfirmationSettings> ReadForConfirmationAsync(CancellationToken ct)
        {
            if (db.Database.CurrentTransaction is null)
            {
                throw new InvalidOperationException("The confirmation reads its settings inside its own transaction (05 4.1.5).");
            }

            // Never the cache: the cached value may be the view of the save before the last one (09 10.4 item 5; K-980).
            return await db.CompanySettings.AsNoTracking()
                .Select(s => new PatternConfirmationSettings(s.IsSalesTemporarilyClosed, s.ShippingFee, s.FreeShippingThreshold, s.MinimumOrderAmount))
                .SingleAsync(ct);
        }

        /// <summary>The shipping fee section's save in the shape of 09 10.3 item 2 (the real one is CompanySettingsWriter's): the area is named, not dropped.</summary>
        public async Task<bool> SaveShippingFeeAsync(decimal shippingFee, int shippingFeeVersion, CancellationToken ct)
        {
            int written = await db.CompanySettings
                .Where(s => s.ShippingFeeVersion == shippingFeeVersion)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.ShippingFee, shippingFee).SetProperty(x => x.ShippingFeeVersion, x => x.ShippingFeeVersion + 1), ct);
            if (written == 0)
            {
                return false;
            }

            // The runner drops the area after the commit (09 10.4 item 3); a raw statement names it the same way, there is no SaveChanges hook.
            invalidations.Add(CacheArea.Settings);
            return true;
        }
    }
}

namespace Shopfolio.Modules.CorporateContent.Contracts
{
    /// <summary>The announcement a page shows now, if it shows one.</summary>
    public sealed record PatternVisibleAnnouncement(string Text, string? LinkUrl);

    /// <summary>The announcement read for a page: the record comes from the cache, the visibility is computed on every request (09 10.4 item 4).</summary>
    public interface IPatternAnnouncementSource
    {
        Task<PatternVisibleAnnouncement?> GetVisibleAsync(CancellationToken ct);
    }
}

namespace Shopfolio.Modules.CorporateContent
{
    using Shopfolio.Modules.Catalog.Contracts;
    using Shopfolio.Modules.CorporateContent.Contracts;

    /// <summary>The cached announcement record with its range; the cache holds the record, never the answer "visible now".</summary>
    internal sealed record PatternCachedAnnouncement(string? Text, string? LinkUrl, DateOnly? StartDate, DateOnly? EndDate, PublishStatus PublishStatus);

    /// <summary>The cached value of the area: the one announcement row, or none (a cached value is never null).</summary>
    internal sealed record PatternAnnouncementBlock(PatternCachedAnnouncement? Announcement);

    /// <summary>
    /// The CorporateContent module's side: the announcement stands in the homepage blocks' area in this sample only - which area holds which record is the
    /// content task's design (09 10.4 item 3). The range is checked against <see cref="TimeProvider"/> on every call; no expiry waits for its end.
    /// </summary>
    internal sealed class PatternAnnouncementSource(AppDbContext db, AppCache cache, TimeProvider time) : IPatternAnnouncementSource
    {
        public async Task<PatternVisibleAnnouncement?> GetVisibleAsync(CancellationToken ct)
        {
            PatternAnnouncementBlock block = await cache.GetOrLoadAsync(CacheArea.HomepageBlocks, LoadAsync, ct);
            PatternCachedAnnouncement? announcement = block.Announcement;
            DateTimeOffset now = time.GetUtcNow();
            bool isVisible = announcement is { Text: not null }
                && announcement.PublishStatus == PublishStatus.Published
                && (announcement.StartDate is null || now >= TurkeyTime.StartOf(announcement.StartDate.Value))
                && (announcement.EndDate is null || now < TurkeyTime.StartOf(announcement.EndDate.Value.AddDays(1)));
            return isVisible ? new PatternVisibleAnnouncement(announcement!.Text!, announcement.LinkUrl) : null;
        }

        private async Task<PatternAnnouncementBlock> LoadAsync(CancellationToken ct)
        {
            PatternCachedAnnouncement? row = await db.Announcements.AsNoTracking()
                .Select(a => new PatternCachedAnnouncement(a.Text, a.LinkUrl, a.StartDate, a.EndDate, a.PublishStatus))
                .SingleOrDefaultAsync(ct);
            return new PatternAnnouncementBlock(row);
        }
    }
}

namespace Shopfolio.Modules.Catalog.Contracts
{
    /// <summary>A product's price as a page shows it.</summary>
    public sealed record PatternPriceView(string Name, decimal Price);

    /// <summary>The price read: never cached, read from the database on every request (05 7.10.1; 09 10.4 item 1).</summary>
    public interface IPatternPriceSource
    {
        Task<PatternPriceView?> GetAsync(string slug, CancellationToken ct);
    }
}

namespace Shopfolio.Modules.Catalog
{
    using Shopfolio.Modules.Catalog.Contracts;

    /// <summary>The Catalog module's price read: no cache on this path.</summary>
    internal sealed class PatternPriceSource(AppDbContext db) : IPatternPriceSource
    {
        public Task<PatternPriceView?> GetAsync(string slug, CancellationToken ct)
        {
            return db.Variants.AsNoTracking()
                .Join(db.Products.Where(p => p.Slug == slug), v => v.ProductId, p => p.Id, (v, p) => new { p.Name, v.Price, v.Id })
                .OrderBy(x => x.Price).ThenBy(x => x.Id)
                .Select(x => new PatternPriceView(x.Name, x.Price))
                .FirstOrDefaultAsync(ct);
        }
    }
}

namespace Shopfolio.Pages.Storefront
{
    using Shopfolio.Modules.Catalog.Contracts;
    using Shopfolio.Modules.CorporateContent.Contracts;
    using Shopfolio.Modules.Settings.Contracts;

    /// <summary>
    /// A storefront page that shows a product's price (database, every request), the shipping settings (the module's cached display read) and the announcement
    /// (cached record, visibility now). The page reaches only the modules' Contracts interfaces - never the cache, the context or an entity - and its own output is
    /// never cached: every request renders it again (05 7.10.1).
    /// </summary>
    public sealed class CachePatternPage(IPatternPriceSource prices, IPatternShopSettings settings, IPatternAnnouncementSource announcements) : PageModel
    {
        public PatternPriceView? Product { get; private set; }

        public PatternShopSettingsView? Shipping { get; private set; }

        public PatternVisibleAnnouncement? Announcement { get; private set; }

        public async Task<IActionResult> OnGetAsync(string slug, CancellationToken ct)
        {
            Product = await prices.GetAsync(slug, ct);
            if (Product is null)
            {
                return NotFound();
            }

            Shipping = await settings.GetForDisplayAsync(ct);
            Announcement = await announcements.GetVisibleAsync(ct);
            return Page();
        }
    }
}
