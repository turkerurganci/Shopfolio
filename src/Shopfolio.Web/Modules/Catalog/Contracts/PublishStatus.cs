namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>The closed values of PublishStatus (06 5.2.3). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum PublishStatus
{
    Draft,
    Published,
    Archived,
}
