using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Lists;

namespace Shopfolio.Pages.Lists;

/// <summary>One entry of the page navigation (04 2.13.3): a page number with its address, the page shown (no address - it is not a link), or a gap between two runs of numbers (<see cref="Number"/> null).</summary>
public sealed record PagerItem(int? Number, string? Path, bool IsCurrent)
{
    public bool IsGap => Number is null;
}

/// <summary>
/// The page navigation of a long list (04 2.13.3; K-766): previous, next and the page numbers - numbered pages, never an endless scroll. A list that fits one page shows no navigation
/// (<see cref="IsShown"/> false). Every address is the list's own address with the filters the request carried and <c>page</c> last; the first page is written without the parameter (07 2.6.2; <see cref="ListAddress"/>).
/// </summary>
public sealed record Pager(string? PrevPagePath, string? NextPath, IReadOnlyList<PagerItem> Items)
{
    /// <summary>How many page numbers stand on each side of the page shown before the rest becomes a gap.</summary>
    internal const int Neighbours = 1;

    public bool IsShown => Items.Count > 0;

    /// <summary>The navigation of the page <paramref name="pageNumber"/> of <paramref name="pageCount"/>; the page is clamped into the range (a page past the last shows the last, 07 2.6.2).</summary>
    public static Pager Of(string listRoot, PagedQuery query, int pageNumber, int pageCount)
    {
        ArgumentException.ThrowIfNullOrEmpty(listRoot);
        ArgumentNullException.ThrowIfNull(query);

        if (pageCount <= 1)
        {
            return new Pager(null, null, []);
        }

        int current = Math.Clamp(pageNumber, PageNumber.First, pageCount);
        List<PagerItem> items = [];
        int previous = 0;
        foreach (int number in Shown(current, pageCount))
        {
            if (number - previous == 2)
            {
                // A gap of one page is the page itself: a dots mark that hides a single number would only be longer.
                items.Add(Item(listRoot, query, previous + 1, current));
            }
            else if (number - previous > 2)
            {
                items.Add(new PagerItem(null, null, false));
            }

            items.Add(Item(listRoot, query, number, current));
            previous = number;
        }

        return new Pager(
            current > PageNumber.First ? ListAddress.Of(listRoot, query, current - 1) : null,
            current < pageCount ? ListAddress.Of(listRoot, query, current + 1) : null,
            items);
    }

    private static PagerItem Item(string listRoot, PagedQuery query, int number, int current) =>
        number == current ? new PagerItem(number, null, true) : new PagerItem(number, ListAddress.Of(listRoot, query, number), false);

    private static SortedSet<int> Shown(int current, int pageCount)
    {
        SortedSet<int> numbers = [PageNumber.First, pageCount];
        for (int n = current - Neighbours; n <= current + Neighbours; n++)
        {
            if (n >= PageNumber.First && n <= pageCount)
            {
                numbers.Add(n);
            }
        }

        return numbers;
    }
}

/// <summary>
/// The empty-state line of a list (04 2.7.1.2): one sentence that says what is missing, and a way out - a link back (home, the products) or, in a list the user opens the first record of, the button
/// that opens it. It is no error and carries no error mark. <see cref="ActionPath"/> and <see cref="ActionText"/> go together.
/// </summary>
public sealed record EmptyState
{
    public EmptyState(string message, string? actionText = null, string? actionPath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        if ((actionText is null) != (actionPath is null))
        {
            throw new ArgumentException("The way out is a text and an address together.", nameof(actionText));
        }

        Message = message;
        ActionText = actionText;
        ActionPath = actionPath;
    }

    public string Message { get; }

    public string? ActionText { get; }

    public string? ActionPath { get; }
}

/// <summary>The lists of 04 2.13 that a screen draws with the list components (T545).</summary>
public enum ListKind
{
    StorefrontProducts,
    ContentList,
    MemberOrders,
    PanelOrders,
    PanelRequests,
    PanelProducts,
    PanelCoupons,
    PanelContent,
    PanelMembers,
    PanelAuditTrail,
    PanelOther,
}

/// <summary>
/// The lists that have a search or a filter (04 2.13.2): the storefront's product lists (name search and the two fixed axes), the panel's order list, product list, member view, audit trail and requests
/// list. Coupons and the corporate content have neither (K-737); the member's order history and the other lists have none either. A screen asks here before it draws a search field.
/// </summary>
public static class ListCatalog
{
    public static bool HasSearch(ListKind kind) => kind switch
    {
        ListKind.StorefrontProducts or ListKind.PanelOrders or ListKind.PanelProducts or ListKind.PanelMembers => true,
        _ => false,
    };

    public static bool HasFilter(ListKind kind) => kind switch
    {
        ListKind.StorefrontProducts or ListKind.PanelOrders or ListKind.PanelProducts or ListKind.PanelAuditTrail or ListKind.PanelRequests => true,
        _ => false,
    };

    /// <summary>The page size the list shows (04 2.13.3; 09 10.5 item 4): the sizes of <see cref="PageSizes"/>, written there once.</summary>
    public static int PageSizeOf(ListKind kind) => kind switch
    {
        ListKind.StorefrontProducts => PageSizes.StorefrontProducts,
        ListKind.ContentList => PageSizes.ContentList,
        ListKind.MemberOrders => PageSizes.OrderHistory,
        _ => PageSizes.Panel,
    };
}

/// <summary>The search field of a list (04 2.13.2): the form of the GET list address with the one parameter <c>search</c>; <see cref="Value"/> is what the request carried.</summary>
public sealed record ListSearch(ListKind Kind, string ListRoot, string Label, string? Value, string? Message = null)
{
    /// <summary>The element id of the search field; the field's message is tied to it (04 2.12.1.1).</summary>
    public const string FieldId = "list-search-field";

    /// <summary>The parameter of the search word (07 2.6.3).</summary>
    public const string Parameter = "search";

    /// <summary>Whether the field is drawn: only in the lists the table gives a search.</summary>
    public bool IsShown => ListCatalog.HasSearch(Kind);
}

/// <summary>A column of a panel table (04 2.13.4): the title and whether it is primary - the primary columns stay in the row on the medium class; the secondary ones go under it.</summary>
public sealed record PanelTableColumn(string Title, bool IsPrimary);

/// <summary>
/// A row of a panel table: the text of every cell in the column order, and the address of the record the first primary cell links to (null for a row that opens nothing). A cell that stands for a status
/// is drawn as its badge (04 2.5.1) when <paramref name="Badges"/> names the column's index; the cell's text stays the badge's text, so the table is complete without the badge's tone (02 6.1.6).
/// </summary>
public sealed record PanelTableRow(IReadOnlyList<string> Cells, string? RecordPath, IReadOnlyDictionary<int, Shopfolio.Pages.Components.Badge>? Badges = null)
{
    /// <summary>The badge the cell at <paramref name="column"/> is drawn as, or null for a plain text cell.</summary>
    public Shopfolio.Pages.Components.Badge? BadgeAt(int column) => Badges is not null && Badges.TryGetValue(column, out Shopfolio.Pages.Components.Badge? badge) ? badge : null;
}

/// <summary>
/// A panel list (04 2.13.4; T545): a table on the wide class, the secondary columns under the row on the medium class and a card list on the narrow class - one markup, three layouts by the style
/// sheet; no cell is hidden and nothing scrolls sideways. There is no selection column and no bulk action (04 2.13.6; K-761). The cells are plain text; the constructor refuses a row whose cell count
/// is not the column count and a table without a primary column, so the markup can never be uneven.
/// </summary>
public sealed record PanelTable
{
    public PanelTable(string caption, IReadOnlyList<PanelTableColumn> columns, IReadOnlyList<PanelTableRow> rows)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caption);
        ArgumentNullException.ThrowIfNull(columns);
        ArgumentNullException.ThrowIfNull(rows);
        if (!columns.Any(c => c.IsPrimary))
        {
            throw new ArgumentException("A table has at least one primary column.", nameof(columns));
        }

        if (rows.Any(r => r.Cells.Count != columns.Count))
        {
            throw new ArgumentException("Every row has one cell for every column.", nameof(rows));
        }

        if (rows.Any(r => r.Badges is not null && r.Badges.Keys.Any(k => k < 0 || k >= columns.Count)))
        {
            throw new ArgumentException("A badge belongs to a column of the table.", nameof(rows));
        }

        Caption = caption;
        Columns = columns;
        Rows = rows;
    }

    public string Caption { get; }

    public IReadOnlyList<PanelTableColumn> Columns { get; }

    public IReadOnlyList<PanelTableRow> Rows { get; }

    /// <summary>The index of the column whose cell carries the row's link: the first primary one.</summary>
    public int LinkColumn => Columns.Select((c, i) => (c, i)).First(x => x.c.IsPrimary).i;
}
