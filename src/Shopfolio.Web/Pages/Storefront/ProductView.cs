using System.Text.Json;

using Microsoft.AspNetCore.Html;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Categories;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Lists;

namespace Shopfolio.Pages.Storefront;

/// <summary>One step of the product's path (04 5.4.3): the category's name and its page's address.</summary>
public sealed record ProductCrumb(string Name, string Path);

/// <summary>An image of the product page: the address of the stored file and the alternative text, which is never empty (02 3.11.4).</summary>
public sealed record ProductImage(string Path, string AltText);

/// <summary>
/// The price block of the shown variant (04 5.4.4; 02 3.8.5): the VAT-included price - the discounted one while a discount runs, with the reference price, the mark that carries text and the dates of
/// the range on one line under the price row -, the day the price applies since, the unit price when it differs from the price, and the origin line.
/// </summary>
public sealed record ProductPriceBlock(
    string PriceText,
    string? ReferencePriceText,
    string? DiscountMark,
    string? DiscountDates,
    string? SinceText,
    string? UnitPriceText,
    OriginLabel? Origin);

/// <summary>The delivery information of the page (04 5.4.5): the type's line and, for a physical product, the firm's delivery restriction line.</summary>
public sealed record ProductDelivery(string? Line, DeliveryRestrictionLine? Restriction);

/// <summary>What stands in the place of the add-to-cart button (04 5.4.10, 5.4.13-5.4.15).</summary>
public enum BuyMode
{
    /// <summary>The form: the quantity field (not for a digital product) and the button.</summary>
    Form,

    /// <summary>Every variant is sold out: "Tükendi" instead of the button (04 5.4.13).</summary>
    SoldOut,

    /// <summary>Sales are closed: the in-place message instead of the button (04 5.4.14).</summary>
    SalesClosed,

    /// <summary>A preview: the page says it is a preview (04 5.4.15; K-829).</summary>
    Preview,

    /// <summary>Nothing is sold on the page (the archived product's page).</summary>
    None,
}

/// <summary>The buy area of the page: what stands in the button's place, the form's hidden variant, the typed quantity and the in-place message (kept beside the button) and warning.</summary>
public sealed record ProductBuyArea(
    BuyMode Mode, Guid? VariantId, bool ShowsQuantity, string QuantityValue, MessageView? Message, MessageView? Warning, MessageView? OptionMessage, MessageView? QuantityMessage)
{
    /// <summary>The id of the quantity field; its message names it in <c>aria-describedby</c> (04 2.3.1.1).</summary>
    public const string QuantityFieldId = "quantity";
}

/// <summary>The data the script gets for one published variant (04 5.4.9; 07 4.4.1): never a stock count (02 3.6.5), only whether the variant can be bought.</summary>
public sealed record VariantScriptData(
    Guid Id, int? First, int? Second, string Price, string? Reference, string? Mark, string? Dates, string? UnitPrice, string? ImagePath, string? ImageAlt, bool Purchasable);

/// <summary>
/// The product page as the screen draws it (E-04, E-05; 04 5.4): the name, the path from the root, the gallery, the price block, the delivery lines, the selection fields, the buy area, the description
/// and the data the script reads. An archived product's page carries only the name, the main image and the line that says it is not sold any more (04 5.5).
/// </summary>
public sealed record ProductView(
    ProductPageKind Kind,
    string Name,
    string Slug,
    string Path,
    IReadOnlyList<ProductCrumb> Trail,
    ProductImage? Lead,
    IReadOnlyList<ProductImage> Others,
    ProductPriceBlock? Price,
    ProductDelivery? Delivery,
    IReadOnlyList<OptionField> Options,
    ProductBuyArea Buy,
    IHtmlContent? Description,
    string? ScriptDataJson,
    string? Sku,
    decimal? PriceAmount,
    bool InStock,
    string? LeadImageForSearch,
    string? SearchDescription)
{
    public const string NoLongerSoldText = "Bu ürün artık satılmıyor";

    public const string SoldOutText = "Tükendi";

    public const string PreviewText = "Bu sayfa bir önizlemedir; sepete ekleme yoktur.";
}

/// <summary>The one place the product page's view is made from the module reads (T503; 04 5.4). The page model gives the modules and the answers; nothing here writes.</summary>
internal static class ProductViews
{
    private static readonly JsonSerializerOptions ScriptJson = new(JsonSerializerDefaults.Web);

    /// <summary>The address of a product's page (07 2.1.2).</summary>
    public static string PathOf(string slug) => ProductCards.ProductPath(slug);

    /// <summary>The unit's short text of the unit price line (02 3.8.5): the base unit of the line, not the unit the net quantity is written in.</summary>
    public static string UnitText(MeasurementUnit unit) => unit switch
    {
        MeasurementUnit.Kilogram or MeasurementUnit.Gram => "kg",
        MeasurementUnit.Litre or MeasurementUnit.Millilitre => "L",
        MeasurementUnit.Metre or MeasurementUnit.Centimetre => "m",
        MeasurementUnit.SquareMetre => "m²",
        MeasurementUnit.CubicMetre => "m³",
        _ => "adet",
    };

    /// <summary>The price block of a price label (04 5.4.4): the unit price is left out when it is the price itself.</summary>
    public static ProductPriceBlock PriceBlockOf(PriceLabel label, OriginLabel? origin)
    {
        ArgumentNullException.ThrowIfNull(label);
        string? unitPrice = label.UnitPrice is { } unit && unit.Amount != label.Price
            ? $"Birim fiyat: {DisplayFormat.Amount(unit.Amount)} / {UnitText(unit.BaseUnit)}"
            : null;
        string? since = label.PriceAppliesSince is { } instant ? $"Fiyatın geçerlilik başlangıcı: {DisplayFormat.Date(instant)}" : null;
        return label.Discount is { } discount
            ? new ProductPriceBlock(
                DisplayFormat.Amount(discount.DiscountedPrice),
                DisplayFormat.Amount(discount.ReferencePrice),
                "İndirim " + DisplayFormat.Percent(discount.Percentage),
                DisplayFormat.DateRange(discount.StartDate, discount.EndDate),
                since,
                unitPrice,
                origin)
            : new ProductPriceBlock(DisplayFormat.Amount(label.Price), null, null, null, since, unitPrice, origin);
    }

    /// <summary>The delivery information by the product's type (04 5.4.5).</summary>
    public static ProductDelivery DeliveryOf(ProductPage page, int defaultShippingPeriod, DeliveryArea area)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(area);
        switch (page.Type)
        {
            case ProductType.Physical:
                int? period = ShippingPromise.Effective(page.Type, page.ShippingPeriod, defaultShippingPeriod);
                return new ProductDelivery(period is { } days ? ShippingPromise.Text(days) : null, area.RestrictionLine());
            case ProductType.Service:
                return new ProductDelivery(
                    page.FulfilmentPeriod is { } fulfilment ? $"Hizmet, ödeme onayından itibaren {DisplayFormat.Days(fulfilment)} içinde yerine getirilir" : null,
                    null);
            default:
                return new ProductDelivery("İndirme, ödeme onaylandığında açılır", null);
        }
    }

    /// <summary>The path from the root to the product's primary category (04 5.4.3); empty when the product has none.</summary>
    public static IReadOnlyList<ProductCrumb> TrailOf(Guid? primaryCategoryId, IReadOnlyList<CategoryNode> tree)
    {
        ArgumentNullException.ThrowIfNull(tree);
        List<ProductCrumb> trail = [];
        CategoryNode? current = primaryCategoryId is null ? null : tree.FirstOrDefault(n => n.Id == primaryCategoryId);
        int guard = tree.Count + 1;
        while (current is not null && guard-- > 0)
        {
            trail.Insert(0, new ProductCrumb(current.Name, AddressRules.RecordAddress(AddressRules.CategoryPrefix, current.Slug)));
            Guid? parentId = current.ParentCategoryId;
            current = parentId is null ? null : tree.FirstOrDefault(n => n.Id == parentId);
        }

        return trail;
    }

    /// <summary>
    /// What stands in the button's place (04 5.4.10, 5.4.13-5.4.15): the archived page sells nothing; a preview - a draft product or a draft variant - does not sell; closed sales and a product with
    /// nothing to buy say so; otherwise the form.
    /// </summary>
    public static BuyMode ModeOf(ProductPage page, ProductPageVariant? shown, bool salesClosed)
    {
        ArgumentNullException.ThrowIfNull(page);
        if (page.Kind == ProductPageKind.Archived)
        {
            return BuyMode.None;
        }

        if (page.Kind == ProductPageKind.DraftPreview || shown is { Status: PublishStatus.Draft })
        {
            return BuyMode.Preview;
        }

        if (salesClosed)
        {
            return BuyMode.SalesClosed;
        }

        bool anyBuyable = page.Variants.Any(v => v.Status == PublishStatus.Published && !v.IsSoldOut);
        return anyBuyable ? BuyMode.Form : BuyMode.SoldOut;
    }

    /// <summary>The script's data for the published variants (and, for a panel session, the draft ones): the places of the two values, the price lines, the image and whether it can be bought; no stock count.</summary>
    public static string ScriptDataOf(IReadOnlyList<VariantScriptData> variants)
    {
        ArgumentNullException.ThrowIfNull(variants);
        return JsonSerializer.Serialize(variants, ScriptJson);
    }

    /// <summary>The page's description as the closed rich text set draws it (02 3.11.6); null when the product has none.</summary>
    public static IHtmlContent? DescriptionOf(string? stored) => string.IsNullOrWhiteSpace(stored) ? null : RichText.Render(stored);
}
