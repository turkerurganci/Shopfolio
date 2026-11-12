using Shopfolio.Infrastructure.Routing;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Lists;
using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// The storefront's content pages as the page layer reads them (T505; 02 3.27.3-3.27.5, 3.27.9-3.27.11, 3.27.25; 04 5.7, 5.8): the two list pages - the published service introductions and reference
/// works in the firm's manual order, 24 a page, under the type's name as the firm gave it - and the four pages with an address of their own, each with its gallery in the firm's order and, for a
/// service introduction and a reference work, the linked products that are published. Whether a record is shown is <see cref="ContentVisibility"/>'s decision alone, through the readers this class
/// composes: a record that does not exist, one with another type's address and a draft a visitor asks for answer null alike (04 5.8.9). A plain read; nothing is written.
/// </summary>
internal sealed class StorefrontContentPages
{
    private readonly ContentReaders _readers;
    private readonly ContentPageReader _pages;
    private readonly ContentGallery _gallery;
    private readonly ContentProductLinks _links;
    private readonly StorefrontMenuReader _menu;

    public StorefrontContentPages(ContentReaders readers, ContentPageReader pages, ContentGallery gallery, ContentProductLinks links, StorefrontMenuReader menu)
    {
        ArgumentNullException.ThrowIfNull(readers);
        ArgumentNullException.ThrowIfNull(pages);
        ArgumentNullException.ThrowIfNull(gallery);
        ArgumentNullException.ThrowIfNull(links);
        ArgumentNullException.ThrowIfNull(menu);
        _readers = readers;
        _pages = pages;
        _gallery = gallery;
        _links = links;
        _menu = menu;
    }

    /// <summary>One page of the list of <paramref name="kind"/>; the page number past the last shows the last (09 10.5).</summary>
    public async Task<ContentListView> ListAsync(ContentListKind kind, int page, CancellationToken ct)
    {
        StorefrontMenu menu = await _menu.GetAsync(ct);
        MenuItemKind item = kind == ContentListKind.Offerings ? MenuItemKind.Offerings : MenuItemKind.PortfolioItems;
        string title = menu.Items.First(i => i.Kind == item).Name;
        ContentCardPage cards = kind == ContentListKind.Offerings
            ? await _readers.ListOfferingsAsync(page, ct)
            : await _readers.ListPortfolioItemsAsync(page, ct);
        return new ContentListView(title, cards.Cards, cards.PageNumber, cards.PageSize, cards.TotalCount, cards.PageCount);
    }

    /// <summary>
    /// The page of <paramref name="kind"/>; <paramref name="slug"/> is the address part and is ignored for the about page, which is one record at a fixed address. Null when there is no such
    /// page for this audience.
    /// </summary>
    public async Task<ContentRecordPage?> FindAsync(ContentPageKind kind, string? slug, bool asAdministrator, CancellationToken ct)
    {
        ContentAudience audience = asAdministrator ? ContentAudience.Administrator : ContentAudience.Visitor;
        if (kind == ContentPageKind.AboutPage)
        {
            ContentPageView? about = await _pages.FindAboutPageAsync(audience, ct);
            if (about is null)
            {
                return null;
            }

            StorefrontMenu menu = await _menu.GetAsync(ct);
            string title = menu.Items.First(i => i.Kind == MenuItemKind.AboutPage).Name;
            return await ComposeAsync(about, ContentPageKind.AboutPage, title, "/" + AddressRules.AboutPage, ct);
        }

        if (string.IsNullOrEmpty(slug) || !AddressRules.IsValidSegment(AddressRules.SlugForLookup(slug)))
        {
            return null;
        }

        string key = AddressRules.SlugForLookup(slug);
        (ContentRecordKind record, string prefix) = kind switch
        {
            ContentPageKind.Offering => (ContentRecordKind.Offering, AddressRules.ServicePrefix),
            ContentPageKind.PortfolioItem => (ContentRecordKind.PortfolioItem, AddressRules.ReferencePrefix),
            ContentPageKind.CustomPage => (ContentRecordKind.CustomPage, AddressRules.GeneralPagePrefix),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The about page is read without an address."),
        };
        ContentPageView? found = await _pages.FindAsync(record, key, audience, ct);
        return found is null ? null : await ComposeAsync(found, kind, found.Title ?? string.Empty, AddressRules.RecordAddress(prefix, key), ct);
    }

    private async Task<ContentRecordPage> ComposeAsync(ContentPageView view, ContentPageKind kind, string title, string path, CancellationToken ct)
    {
        IReadOnlyList<GalleryImage> images = await _gallery.ReadAsync(view.Kind, view.Id, ct);
        IReadOnlyList<ProductListCard> related = kind switch
        {
            ContentPageKind.Offering => await _links.ListRelatedCardsAsync(ContentLinkOwner.Offering, view.Id, ct),
            ContentPageKind.PortfolioItem => await _links.ListRelatedCardsAsync(ContentLinkOwner.PortfolioItem, view.Id, ct),
            _ => [],
        };
        return new ContentRecordPage(kind, view.Id, title, view.Summary, view.BodyHtml, path, view.IsDraft, images, related);
    }
}
