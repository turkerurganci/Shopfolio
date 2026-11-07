namespace Shopfolio.Modules.Catalog.Contracts.Variants;

/// <summary>
/// What the visitor learns of one variant's stock (02 3.6.3, 3.6.5; 10 KD-33): only whether it is sold out. There is no count, no threshold and no "last units" state - the
/// type has no number to carry one.
/// </summary>
public sealed record VariantAvailability(Guid VariantId, bool IsSoldOut);
