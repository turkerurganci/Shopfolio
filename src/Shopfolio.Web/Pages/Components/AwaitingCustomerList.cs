namespace Shopfolio.Pages.Components;

/// <summary>One line of the "Müşteriden beklenenler" list: the order it belongs to and the item or the order the IBAN is awaited for, with the period when the line has one (04 2.6.2).</summary>
/// <param name="OrderNumber">The order's number as the screen writes it.</param>
/// <param name="OrderPath">The order's page: every line leads to its order.</param>
/// <param name="Text">What is awaited ("IBAN bekleniyor"; the item's name, or the order for a late payment's refund - 02 7.2.8).</param>
/// <param name="Period">The line's running period (the refund period); null when the line has none.</param>
internal sealed record AwaitingCustomerRow(string OrderNumber, string OrderPath, string Text, PeriodIndicator? Period);

/// <summary>
/// The "IBAN bekleniyor" list of the panel home (T542; 04 2.6.2; 02 10.6.1; K-750): a list of lines, not a counter - nothing here is counted and no number is drawn. The lines with a period come
/// first and the one that runs out soonest comes before the others; the lines without a period follow in order-number order. A long list shows its first ten lines and a link to the order list
/// filtered by the IBAN mark.
/// </summary>
internal sealed class AwaitingCustomerList
{
    /// <summary>How many lines the home shows (04 2.6.2).</summary>
    public const int ShownLines = 10;

    private AwaitingCustomerList(IReadOnlyList<AwaitingCustomerRow> shown, bool hasMore)
    {
        Shown = shown;
        HasMore = hasMore;
    }

    /// <summary>The lines drawn, at most <see cref="ShownLines"/>.</summary>
    public IReadOnlyList<AwaitingCustomerRow> Shown { get; }

    /// <summary>Whether there are more lines than shown: the screen then adds the link to the whole list.</summary>
    public bool HasMore { get; }

    /// <summary>The page that holds the whole list (04 2.6.2): the order list's IBAN filter.</summary>
    public static string AllPath => PendingCounters.IbanPendingListPath;

    /// <summary>The list from every awaiting line: ordered, cut to ten.</summary>
    public static AwaitingCustomerList Of(IEnumerable<AwaitingCustomerRow> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        AwaitingCustomerRow[] ordered =
        [
            .. rows
                .OrderBy(r => r.Period is null)
                .ThenBy(r => r.Period?.Standing.EndsAt ?? DateTimeOffset.MaxValue)
                .ThenBy(r => r.OrderNumber, StringComparer.Ordinal),
        ];
        return new AwaitingCustomerList([.. ordered.Take(ShownLines)], ordered.Length > ShownLines);
    }
}
