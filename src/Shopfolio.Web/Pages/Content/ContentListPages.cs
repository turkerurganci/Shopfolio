using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Lists;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.SearchEngine;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Lists;

namespace Shopfolio.Pages.Content;

/// <summary>The query model of a content list page (T505; 07 2.6, 5.3.1): the page number alone - the two lists have no search, no filter and no sort control (04 2.13.1).</summary>
public sealed class ContentListQuery : PagedQuery
{
}

/// <summary>
/// The common part of the two content list pages (T505; 07 UC-07, UC-08; 04 5.7): the published records of one type in the firm's manual order, 24 a page, under the type's name as the firm gave
/// it (the menu's name, K-851). A type with no published record answers the empty-state line with a way back - the page is no error (04 5.7.8). The list pattern sets the closed mark
/// (<see cref="IListPage"/>); the page's head writes it through the search engine pattern.
/// </summary>
public abstract class ContentListPageModel : PageModel, IListPage
{
    private readonly ICorporateContentModule _content;

    protected ContentListPageModel(ICorporateContentModule content)
    {
        ArgumentNullException.ThrowIfNull(content);
        _content = content;
    }

    public bool IsSearchPage => false;

    public bool ClosedToSearchEngines { get; set; }

    public string Title { get; private set; } = string.Empty;

    public IReadOnlyList<ContentCard> Cards { get; private set; } = [];

    public Pager Pager { get; private set; } = Pager.Of("/", new ContentListQuery(), 1, 1);

    public EmptyState? Empty { get; private set; }

    internal IHtmlContent? Head { get; private set; }

    /// <summary>Which type the page lists.</summary>
    protected abstract ContentListKind Kind { get; }

    /// <summary>The list's own address (07 2.1.2).</summary>
    protected abstract string Root { get; }

    public async Task<IActionResult> OnGetAsync(ContentListQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        ContentListView view = await _content.GetContentListAsync(Kind, query.RequestedPage(), ct);
        BrandIdentity brand = await _content.GetBrandIdentityAsync(ct);

        Title = view.Title;
        Cards = ContentCards.Of(view.Cards);
        Pager = Pager.Of(Root, query, view.PageNumber, view.PageCount);
        if (Cards.Count == 0)
        {
            Empty = new EmptyState("Henüz yayında kayıt yok.", "Ana sayfaya dön", "/");
        }

        Head = HeadOf(HttpContext, RecordPages.ContentList(Root, view.Title, brand.LogoId is { } logo ? FrameStyle.ImagePath(logo) : null, brand.HasBrandName ? brand.DisplayName : null, ClosedToSearchEngines));
        return Page();
    }

    private static IHtmlContent HeadOf(HttpContext context, PageSearchData data) => context.RequestServices.GetRequiredService<SearchEngineData>().Head(context, data);
}

/// <summary>The service introductions' list page (T505; 07 UC-07).</summary>
public sealed class ServiceListModel : ContentListPageModel
{
    public ServiceListModel(ICorporateContentModule content)
        : base(content)
    {
    }

    protected override ContentListKind Kind => ContentListKind.Offerings;

    protected override string Root => "/" + AddressRules.ServiceListPage;
}

/// <summary>The reference works' list page (T505; 07 UC-08).</summary>
public sealed class ReferenceListModel : ContentListPageModel
{
    public ReferenceListModel(ICorporateContentModule content)
        : base(content)
    {
    }

    protected override ContentListKind Kind => ContentListKind.PortfolioItems;

    protected override string Root => "/" + AddressRules.ReferenceListPage;
}
