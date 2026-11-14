namespace Shopfolio.Modules.CorporateContent.Contracts;

/// <summary>What became of a deletion (T534; 07 UC-160).</summary>
public enum PanelContentDeleteOutcome
{
    Deleted,

    NotFound,

    /// <summary>Hakkımızda is never deleted (02 3.27.3): 422.</summary>
    Forbidden,
}
