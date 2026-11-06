namespace Shopfolio.Infrastructure.Operations;

/// <summary>
/// What the server decided about one operation of a screen at this moment (07 8.1.1, 8.1.4): whether it is open and, for the values the page carries,
/// the open quantity, the default, the upper limit and the last day of the interval it is open in. The page shows the button from <see cref="IsOpen"/>
/// and writes the values; the browser derives none of them. The value is the result of the one rule function of the operation (<see cref="IOperationRule{TContext}"/>),
/// which both the page's GET and the POST handler call (07 8.1.2). <c>default</c> is the closed operation. An operation that has no quantity (an
/// administrator's operation on the order, a cancellation of the whole order) is open with <see cref="Open"/> and carries 0 for the three quantities.
/// </summary>
public readonly record struct OperationAvailability
{
    private OperationAvailability(bool isOpen, int openQuantity, int defaultQuantity, int maxQuantity, DateOnly? lastDay)
    {
        IsOpen = isOpen;
        OpenQuantity = openQuantity;
        DefaultQuantity = defaultQuantity;
        MaxQuantity = maxQuantity;
        LastDay = lastDay;
    }

    /// <summary>The operation is open now: the screen offers it and the POST handler may apply it.</summary>
    public bool IsOpen { get; }

    /// <summary>The quantity of the item the operation is open for (03 1.7.4); 0 for an operation that has no quantity and for a closed one.</summary>
    public int OpenQuantity { get; }

    /// <summary>The quantity the form starts with (07 8.1.4; K-1170 (2)): the whole open quantity for a cancellation and a withdrawal, one for a defect claim.</summary>
    public int DefaultQuantity { get; }

    /// <summary>The largest quantity the request may ask for; a larger one is refused (07 8.1.3).</summary>
    public int MaxQuantity { get; }

    /// <summary>The last day the operation is open in (03 1.11; 04 2.6.6), or <see langword="null"/> when its interval has no end day or the end is not known yet.</summary>
    public DateOnly? LastDay { get; }

    /// <summary>True when the operation carries a quantity: the request names one and the page has a quantity field.</summary>
    public bool HasQuantity => MaxQuantity > 0;

    /// <summary>The closed operation: not shown, not applied.</summary>
    public static OperationAvailability Closed => default;

    /// <summary>An open operation that has no quantity.</summary>
    public static OperationAvailability Open(DateOnly? lastDay = null) => new(true, 0, 0, 0, lastDay);

    /// <summary>
    /// An open operation on an item with an open quantity. The default and the upper limit are inside the open quantity; the lower end of the
    /// request is one (a request of less is a field error of the quantity rule, not a refusal of the operation).
    /// </summary>
    public static OperationAvailability OpenForQuantity(int openQuantity, int defaultQuantity, int maxQuantity, DateOnly? lastDay = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(openQuantity, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxQuantity, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(maxQuantity, openQuantity);
        ArgumentOutOfRangeException.ThrowIfLessThan(defaultQuantity, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(defaultQuantity, maxQuantity);
        return new OperationAvailability(true, openQuantity, defaultQuantity, maxQuantity, lastDay);
    }
}
