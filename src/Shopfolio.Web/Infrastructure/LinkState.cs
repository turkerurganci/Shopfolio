namespace Shopfolio.Infrastructure;

/// <summary>The closed values of LinkState (06 5.4.3 (not a column: the state is read from the moments of the record)). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum LinkState
{
    Valid,
    Expired,
    Used,
    Superseded,
}
