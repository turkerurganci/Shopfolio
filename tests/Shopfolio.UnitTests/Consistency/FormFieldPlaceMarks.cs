namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The rule attributes each property place of a field row that has several property places carries (T547 correction 3, C1; 04 7 introduction; 09 21.4 item 1). A row's rule and requirement are read from the
/// union of its places, so a place that loses its attribute would be "proved" by another place of the same row; this table pins the attributes place by place and
/// <see cref="FormFieldCheck.PlaceMarkProblems"/> compares each place with its pin (<c>PlaceMarksDiffer</c>): an attribute lost or gained by one place is reported on its own. A place of such a row that is
/// not in the table carries no rule attribute (its check, if any, is an enforcement line of <see cref="FormFieldEnforcementTable"/>). The key is <c>row|place</c>, the place written as in
/// <see cref="FormFieldInventory"/>; the value is the attribute type names.
/// </summary>
internal static class FormFieldPlaceMarks
{
    public static IReadOnlyDictionary<string, string[]> Marks { get; } = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["7.2.4.13|Panel.SaveProductVariantRow.NetQuantity"] = ["InvalidNetQuantityAttribute"],
        ["7.2.4.18|Panel.SaveProductDiscountRow.Percentage"] = ["InvalidPercentageAttribute"],
        ["7.2.4.18|Panel.SaveProductDiscountRow.StartDate"] = ["InvalidDateAttribute"],
        ["7.2.4.18|Panel.SaveProductDiscountRow.EndDate"] = ["InvalidDateAttribute"],
        ["7.2.5.1|Panel.CreateCategoryInput.Name"] = ["FieldRequiredAttribute", "TextTooLongAttribute"],
        ["7.2.5.1|Panel.RenameCategoryInput.Name"] = ["FieldRequiredAttribute", "TextTooLongAttribute"],
        ["7.2.16.8|Panel.SaveContentInput.StartDate"] = ["InvalidDateAttribute"],
        ["7.2.16.8|Panel.SaveContentInput.EndDate"] = ["InvalidDateAttribute"],
    };
}
