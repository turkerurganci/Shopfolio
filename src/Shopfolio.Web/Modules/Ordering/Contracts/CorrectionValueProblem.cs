namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// Why a correction's value was refused, for the screen's message (T530; 04 9.9.36): <c>None</c> when the refusal is not about a value, <c>Missing</c> for a required value that is not there
/// (the carrier data - <c>ShipmentInfoMissing</c> -, the date, the e-mail, the reason or "diğer"'s note), <c>Invalid</c> for a value out of its rule (an e-mail's format, a date out of its bounds,
/// an address field), <c>Unchanged</c> for the order's own value - nothing to correct. A result code, never stored.
/// </summary>
public enum CorrectionValueProblem
{
    None,
    Missing,
    Invalid,
    Unchanged,
}
