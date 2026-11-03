namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>The closed values of ActorKind (06 5.4.10). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum ActorKind
{
    Buyer,
    Admin,
    System,
}
