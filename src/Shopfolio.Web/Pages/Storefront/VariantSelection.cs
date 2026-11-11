using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Shopfolio.Infrastructure.Binding;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Pages.Storefront;

/// <summary>
/// The query model of the product page (E-04; 07 5.3.2 <c>VariantQuery</c>; K-1176, K-1178): the two parameters of the no-script variant selection. Each is the place of the option value in its own
/// option (<c>OptionValue.SortOrder</c>). Every field is text so that a wrong value is never a binding error (07 2.6.6): it is read by <see cref="VariantSelection.ParseOrder"/> and a value that is not
/// a number means "no selection".
/// </summary>
public sealed class VariantQuery : IQueryModel
{
    public const string FirstParameter = "firstOptionValue";

    public const string SecondParameter = "secondOptionValue";

    [SpecAssignedName]
    public string? FirstOptionValue { get; init; }

    [SpecAssignedName]
    public string? SecondOptionValue { get; init; }
}

/// <summary>
/// The input of the add-to-cart form (07 5.3.1 <c>AddCartItemInput</c>; 04 7.1.2): <see cref="VariantId"/> is the hidden field the server filled from the variant the page shows (or the script's selection),
/// <see cref="Quantity"/> is the quantity field of a physical product and a service - a digital product has none and its quantity is one. Both are text so that a wrong value is a field message of the
/// page, never a binding failure (04 7.1.2.1, 7.1.2.2).
/// </summary>
public sealed class AddCartItemInput : IFormInput
{
    [SpecAssignedName]
    public string? VariantId { get; set; }

    [SpecAssignedName]
    public string? Quantity { get; set; }

    /// <summary>The variant the form names; null when the field is empty or not an identifier.</summary>
    public Guid? ReadVariantId() => Guid.TryParse(VariantId, out Guid id) && id != Guid.Empty ? id : null;
}

/// <summary>One value of an option as the page's list draws it (04 5.4.6): the sold-out value carries "Tükendi" and cannot be chosen; a combination the firm did not open is listed and cannot be chosen.</summary>
public sealed record OptionChoice(int Order, string Text, bool IsSelected, bool IsSelectable, bool IsSoldOut);

/// <summary>An option dimension of the page's selection: the field name (<c>firstOptionValue</c> or <c>secondOptionValue</c>), the option's name and its values.</summary>
public sealed record OptionField(string FieldName, string Name, IReadOnlyList<OptionChoice> Choices);

/// <summary>
/// Which variant a product page shows (E-04; 07 4.4.1; 04 5.4.6, 5.4.9; K-1178). The selection comes from the two parameters; a missing dimension, an unknown place, a combination the firm did
/// not open, a sold-out variant and - without a panel session, where the reader leaves them out - a draft variant all mean "no selection" and the page is made with the default variant, with no error.
/// </summary>
public static class VariantSelection
{
    private const int MaxDigits = 9;

    /// <summary>The place a parameter carries, or null when the text is not a whole non-negative number.</summary>
    public static int? ParseOrder(string? raw)
    {
        if (string.IsNullOrEmpty(raw) || raw.Length > MaxDigits)
        {
            return null;
        }

        return int.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out int value) ? value : null;
    }

    /// <summary>
    /// The product's default variant: the first published variant that can be bought now, else the first published one (all sold out), else the first variant of the list (a draft preview). The
    /// document does not define "default"; this is the reading of 04 5.4.4's "ürünün varsayılan varyantı" (T503 report).
    /// </summary>
    public static ProductPageVariant? Default(IReadOnlyList<ProductPageVariant> variants)
    {
        ArgumentNullException.ThrowIfNull(variants);
        return variants.FirstOrDefault(v => v.Status == PublishStatus.Published && !v.IsSoldOut)
            ?? variants.FirstOrDefault(v => v.Status == PublishStatus.Published)
            ?? variants.FirstOrDefault();
    }

    /// <summary>The variant the page shows for the two places; null only for a product with no variant to show.</summary>
    public static ProductPageVariant? Resolve(ProductPage page, int? first, int? second)
    {
        ArgumentNullException.ThrowIfNull(page);
        int options = page.Options.Count;
        if (options > 0 && first is not null && (options < 2 || second is not null))
        {
            ProductPageVariant? match = page.Variants.FirstOrDefault(v => v.FirstValueOrder == first && (options < 2 || v.SecondValueOrder == second));
            if (match is not null && (match.Status == PublishStatus.Draft || !match.IsSoldOut))
            {
                return match;
            }
        }

        return Default(page.Variants);
    }

    /// <summary>The variant of a given identifier among the page's variants (the cart form's hidden field); null when it is none of them.</summary>
    public static ProductPageVariant? Find(ProductPage page, Guid variantId)
    {
        ArgumentNullException.ThrowIfNull(page);
        return page.Variants.FirstOrDefault(v => v.Id == variantId);
    }

    /// <summary>
    /// The selection fields of the page for the shown variant (04 5.4.6): one field per option; each value is listed with the variant it would give while the other option keeps its place. A
    /// value whose combination does not exist is listed and cannot be chosen; a sold-out one carries the mark and cannot be chosen; the value now shown is always the chosen one.
    /// </summary>
    public static IReadOnlyList<OptionField> Fields(ProductPage page, ProductPageVariant? shown)
    {
        ArgumentNullException.ThrowIfNull(page);
        List<OptionField> fields = [];
        for (int dimension = 0; dimension < page.Options.Count && dimension < 2; dimension++)
        {
            ProductPageOption option = page.Options[dimension];
            int? shownOrder = shown is null ? null : OrderOf(shown, dimension);
            int? otherOrder = shown is null || page.Options.Count < 2 ? null : OrderOf(shown, 1 - dimension);
            List<OptionChoice> choices = [];
            foreach (ProductPageOptionValue value in option.Values)
            {
                ProductPageVariant? candidate = page.Variants.FirstOrDefault(v =>
                    OrderOf(v, dimension) == value.Order && (page.Options.Count < 2 || OrderOf(v, 1 - dimension) == otherOrder));
                bool selected = shownOrder == value.Order;
                bool soldOut = candidate is { Status: PublishStatus.Published, IsSoldOut: true };
                choices.Add(new OptionChoice(value.Order, value.Text, selected, selected || (candidate is not null && !soldOut), soldOut));
            }

            fields.Add(new OptionField(dimension == 0 ? VariantQuery.FirstParameter : VariantQuery.SecondParameter, option.Name, choices));
        }

        return fields;
    }

    /// <summary>The address of the product page for a variant (07 4.4.2: the success redirect names the variant by its two places); the page's own address when the product has no option.</summary>
    public static string PathFor(string productPath, ProductPageVariant variant)
    {
        ArgumentException.ThrowIfNullOrEmpty(productPath);
        ArgumentNullException.ThrowIfNull(variant);
        List<string> parts = [];
        if (variant.FirstValueOrder is { } first)
        {
            parts.Add($"{VariantQuery.FirstParameter}={first.ToString(CultureInfo.InvariantCulture)}");
        }

        if (variant.SecondValueOrder is { } second)
        {
            parts.Add($"{VariantQuery.SecondParameter}={second.ToString(CultureInfo.InvariantCulture)}");
        }

        return parts.Count == 0 ? productPath : productPath + "?" + string.Join('&', parts);
    }

    private static int? OrderOf(ProductPageVariant variant, int dimension) => dimension == 0 ? variant.FirstValueOrder : variant.SecondValueOrder;
}

/// <summary>The quantity the add-to-cart form carries (04 7.1.2.2): a digital product has none and is one; the others need a whole number from one up.</summary>
public static class CartQuantity
{
    public const string InvalidText = "Adet 1 veya daha büyük bir tam sayı olmalı.";

    private const int MaxDigits = 9;

    /// <summary>The quantity of the form, or null when the text is no whole number from one up. A digital product's quantity is one whatever the form sent.</summary>
    public static int? Read(ProductType type, string? raw)
    {
        if (type == ProductType.Digital)
        {
            return 1;
        }

        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        string text = raw.Trim();
        if (text.Length > MaxDigits || !int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int value) || value < 1)
        {
            return null;
        }

        return value;
    }
}

/// <summary>The in-place texts of the add-to-cart answers (04 5.4.10; 03 3.2.1.6-3.2.1.9): the stock is never told as a number, the limit is.</summary>
public static class CartAddMessages
{
    public const string NoStockText = "Bu adette stok yok";

    public const string SoldOutText = "Bu ürün tükendi";

    public const string AlreadyInCartText = "Bu ürün zaten sepetinde";

    public const string RepurchaseWarningText = "Bu ürünü daha önce aldınız — sipariş sayfanızdan indirebilirsiniz";

    public const string SelectOptionText = "Bir seçenek belirleyin";

    public const string AddedText = "Ürün sepete eklendi";

    public static string OrderLimitText(int limit) => $"Bu üründen bir siparişte en fazla {limit.ToString(CultureInfo.InvariantCulture)} adet alınabilir";
}
