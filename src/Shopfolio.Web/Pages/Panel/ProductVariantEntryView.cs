namespace Shopfolio.Pages.Panel;

/// <summary>
/// One variant entry of the product form (04 9.5.5; 09 16.2 item 4): the same markup draws a row of the page and the script's row template (07 8.2.2), so the template carries no field name the
/// page does not carry. <see cref="Index"/> is the row's number, or the placeholder <see cref="IndexPlaceholder"/> in the template, which the script replaces with the number of rows on the form.
/// </summary>
/// <param name="Page">The form's page model: the option choices, the refusals and the upload state.</param>
/// <param name="Row">The values drawn; a blank row for a new entry and for the template.</param>
/// <param name="Index">The row's number as text, or the placeholder.</param>
/// <param name="IsNew">Whether the row is a new entry (no stored variant).</param>
/// <param name="IsTemplate">Whether the row is the script's template: no refusal, no upload message and no id the page's anchors point at.</param>
/// <param name="UploadAction">The gallery upload handler's address.</param>
internal sealed record ProductVariantEntryView(ProductFormModel Page, SaveProductVariantRow Row, string Index, bool IsNew, bool IsTemplate, string UploadAction)
{
    /// <summary>The symbol the server writes where the row's number goes in the template (09 16.2 item 4).</summary>
    public const string IndexPlaceholder = "__index__";
}
