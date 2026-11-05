// Deliberate violation sample of the field rule scan (09 3.4 items 1 and 6; T211). Not compiled; the scan test reads it as text.
// [TextTooLong(150)] in this comment line is not a hit.
public sealed class Sample
{
    [TextTooLong(150)]
    public string Name { get; set; } = string.Empty;

    [TextTooLong(ProductConstants.ProductNameMaxLength)]
    public string Title { get; set; } = string.Empty;

    [InvalidQuantity(1000)]
    public int Quantity { get; set; }

    [InvalidAmount(AllowZero = true, Max = 99999)]
    public decimal Amount { get; set; }

    [InvalidAmount(Max = ProductConstants.AmountMax)]
    public decimal Price { get; set; }

    public void Setup(HtmlHelperOptions options)
    {
        options.ClientValidationEnabled = true;
        options.ClientValidationEnabled = false;
    }
}

// <input data-val="true" data-val-required="x" />
// <script src="~/lib/jquery-validation/dist/jquery.validate.min.js"></script>
/* a markup line that is not a comment line: */
<input data-val-length-max="150" />
<script src="~/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.js"></script>
