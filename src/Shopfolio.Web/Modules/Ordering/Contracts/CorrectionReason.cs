namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>The closed values of CorrectionReason (06 5.3.9). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum CorrectionReason
{
    WrongOrder,
    MarkedBeforeOccurred,
    Other,
}
