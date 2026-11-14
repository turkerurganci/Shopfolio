namespace Shopfolio.Modules.CorporateContent.Contracts;

/// <summary>What became of a move in the manual order (T534; 07 UC-161).</summary>
public enum PanelContentReorderOutcome
{
    Moved,

    /// <summary>The two records' orders are no longer the shown ones: 409, nothing is written (K-1063).</summary>
    OrderChanged,

    /// <summary>Hakkımızda and the announcement have no manual order: 422.</summary>
    NotOrderable,

    /// <summary>The record is first and moves up, or last and moves down: 422.</summary>
    AtEdge,

    NotFound,
}
