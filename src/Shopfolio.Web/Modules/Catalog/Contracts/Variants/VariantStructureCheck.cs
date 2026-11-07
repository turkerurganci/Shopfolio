namespace Shopfolio.Modules.Catalog.Contracts.Variants;

/// <summary>
/// The verdict on the option values a variant is bound to (02 3.3.2, 3.3.3; 06 4.6 "Uygulamada"): the values must be the product's own, the first value of the
/// option of order 1 and the second of the option of order 2; every option of the product needs its value; and the combination is held by one variant at most.
/// Not a stored value.
/// </summary>
public enum VariantStructureCheck
{
    Valid,
    ValueMissing,
    ValueNotOfTheProduct,
    ValueForAbsentOption,
    CombinationTaken,
}
