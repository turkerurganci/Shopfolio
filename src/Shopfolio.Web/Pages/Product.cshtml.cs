using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.SearchEngine;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Categories;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Storefront;

namespace Shopfolio.Pages;

/// <summary>
/// The product page (E-04, E-05; T503; 07 UC-04, UC-05; 04 5.4): <c>GET /urun/{Slug}</c> is the published product's page, the administrator's preview of a draft and the archived product's page;
/// a draft for a visitor and an address that holds no product are the same 404 (07 2.3.3). <c>POST ?handler=AddCartItem</c> adds the variant to the cart: 303 back to the page with the variant's two
/// places and the "added" notice, or 422 (the page again with the selection and the typed quantity, the message in place beside the button) or 404.
/// </summary>
public sealed class ProductModel : PageModel
{
    private readonly ICatalogModule _catalog;
    private readonly ISettingsModule _settings;
    private readonly ICountryList _countries;
    private readonly IOrderingModule _ordering;

    public ProductModel(ICatalogModule catalog, ISettingsModule settings, ICountryList countries, IOrderingModule ordering)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(countries);
        ArgumentNullException.ThrowIfNull(ordering);
        _catalog = catalog;
        _settings = settings;
        _countries = countries;
        _ordering = ordering;
    }

    /// <summary>The screen's model; set when the page is drawn.</summary>
    public ProductView? View { get; private set; }

    /// <summary>The "added" notice the redirect's <c>notice</c> parameter names (07 2.2.3, 6.3); null when the request carries none.</summary>
    public MessageView? Notice { get; private set; }

    /// <summary>The page is closed to the search engines (02 3.30.6; K-1178): a draft preview, the archived page and any address that carries a selection or a notice.</summary>
    public bool ClosedToSearchEngines { get; private set; }

    public async Task<IActionResult> OnGetAsync(string slug, VariantQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        bool isAdministrator = await HttpContext.HasAdminSessionAsync();
        ProductPage page = await _catalog.GetProductPageAsync(AddressRules.SlugForLookup(slug), isAdministrator, ct);
        if (page.Kind == ProductPageKind.NotFound)
        {
            return NotFound();
        }

        ProductPageVariant? shown = VariantSelection.Resolve(page, VariantSelection.ParseOrder(query.FirstOptionValue), VariantSelection.ParseOrder(query.SecondOptionValue));
        bool hasSelection = Request.Query.ContainsKey(VariantQuery.FirstParameter) || Request.Query.ContainsKey(VariantQuery.SecondParameter);
        ClosedToSearchEngines = !page.IsIndexable || hasSelection || Request.Query.ContainsKey(NoticeParameter.ParameterName);
        if (NoticeParameter.Read(Request, NoticeKind.ShortLived) == NoticeCode.CartItemAdded)
        {
            Notice = Messages.ShortLivedSuccess(CartAddMessages.AddedText);
        }

        View = await BuildAsync(page, shown, BuyState.Empty, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAddCartItemAsync(string slug, AddCartItemInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);

        // A product that is not published - a draft, an archived one, one that is gone - and a preview sell nothing: the same 404 for every reason (07 2.3.3; 04 5.4.15).
        ProductPage page = await _catalog.GetProductPageAsync(AddressRules.SlugForLookup(slug), false, ct);
        if (page.Kind != ProductPageKind.Published)
        {
            return NotFound();
        }

        Guid? variantId = input.ReadVariantId();
        if (variantId is null)
        {
            // No variant chosen: the field message of the selection (04 7.1.2.1) through the common rule (09 3.4); the page is shown with the default variant.
            ModelState.AddFieldError(nameof(AddCartItemInput.VariantId), FieldCodes.FieldRequired);
            View = await BuildAsync(page, VariantSelection.Default(page.Variants), BuyState.Of(input.Quantity), ct);
            return FormResults.Redisplay(this);
        }

        ProductPageVariant? variant = VariantSelection.Find(page, variantId.Value);
        if (variant is not { Status: PublishStatus.Published })
        {
            return NotFound();
        }

        ClosedToSearchEngines = true;
        string productPath = ProductViews.PathOf(page.Slug ?? slug);
        SalesGate gate = await _settings.GetSalesGateAsync(ct);
        if (!gate.Permits(SalesGateOperation.AddToCart))
        {
            return await RefuseAsync(page, variant, input.Quantity, ErrorCode.SalesClosed, SalesClosedMessages.StorefrontText, ct);
        }

        int? quantity = CartQuantity.Read(page.Type, input.Quantity);
        if (quantity is null)
        {
            // The quantity field's rule (04 7.1.2.2) through the common rule (09 3.4): the filter writes the field line and the text comes from the catalog.
            ModelState.AddFieldError(nameof(AddCartItemInput.Quantity), FieldCodes.InvalidQuantity, DisplayFormat.Number(CartQuantity.Max));
            return await RefuseAsync(page, variant, input.Quantity, null, null, ct);
        }

        CartOwner? owner = OwnerOf(HttpContext, out bool isGuest, out string? guestKey);
        if (page.Type == ProductType.Digital)
        {
            Guid? cartId = owner is null ? null : await _ordering.FindCartIdAsync(owner, ct);
            DigitalAddCheck check = await _ordering.CheckDigitalAddAsync(new DigitalAddRequest(variant.Id, page.Type, quantity.Value, cartId, StorefrontFrameBuilder.CustomerOf(HttpContext)), ct);
            if (check.IsBlocked)
            {
                return await RefuseAsync(page, variant, input.Quantity, ErrorCode.DigitalVariantAlreadyInCart, CartAddMessages.AlreadyInCartText, ct);
            }
        }

        // A first add of a browser without a cart key makes the key; it is written only when the add went through, so a refusal opens no cart and sets no cookie (07 4.4.2).
        owner ??= CartOwner.ForGuest(guestKey = SecretKey.Create());
        CartOwner cartOwner = owner;
        ITransactionRunner transactions = HttpContext.RequestServices.GetRequiredService<ITransactionRunner>();
        CartAddAttempt attempt = await transactions.RunAsync(
            async stepCt => new CartAddAttempt(await _ordering.AddToCartAsync(cartOwner, variant.Id, quantity.Value, stepCt)), ct);

        if (!attempt.IsRejected)
        {
            if (isGuest && guestKey is not null)
            {
                BrowserCookies.WriteGuestCart(HttpContext, guestKey);
            }

            return FormResults.SeeOther(VariantSelection.PathFor(productPath, variant), NoticeCode.CartItemAdded);
        }

        // Every named refusal writes its one rejection line with 07's code (07 6.1.3, 6.2.3; D-638): the mapping is the cart's, written once in CartRefusalCodes. The 404 answers write none (07 2.3.3).
        CartAddOutcome outcome = attempt.Result.Outcome;
        if (outcome is CartAddOutcome.NotOnSale or CartAddOutcome.VariantNotFound)
        {
            return NotFound();
        }

        ErrorCode code = CartRefusalCodes.CodeOf(outcome);
        if (outcome == CartAddOutcome.InvalidQuantity)
        {
            // The module's own quantity rule (a digital product's quantity other than one, a number out of range) is the quantity field's message (04 7.1.2.2; 09 3.4); the filter writes the line.
            ModelState.AddFieldError(nameof(AddCartItemInput.Quantity), FieldCodes.InvalidQuantity, DisplayFormat.Number(CartQuantity.Max));
            return await RefuseAsync(page, variant, input.Quantity, null, null, ct);
        }

        string text = outcome switch
        {
            CartAddOutcome.OverOrderLimit => CartAddMessages.OrderLimitText(attempt.Result.OrderLimit ?? 0),
            CartAddOutcome.SoldOut => CartAddMessages.SoldOutText,
            CartAddOutcome.AlreadyInCart => CartAddMessages.AlreadyInCartText,
            _ => CartAddMessages.NoStockText,
        };
        return await RefuseAsync(page, variant, input.Quantity, code, text, ct);
    }

    /// <summary>The cart the request adds to: a member's, or the guest browser's from its cookie; null for a browser with no cart key yet (<paramref name="guestKey"/> stays null).</summary>
    private static CartOwner? OwnerOf(HttpContext context, out bool isGuest, out string? guestKey)
    {
        guestKey = null;
        if (StorefrontFrameBuilder.CustomerOf(context) is Guid customerId)
        {
            isGuest = false;
            return CartOwner.ForMember(customerId);
        }

        isGuest = true;
        string? key = BrowserCookies.Read(context, CookieNames.Cart);
        if (string.IsNullOrWhiteSpace(key) || key.Length > StorefrontFrameBuilder.CartCookieValueLimit)
        {
            return null;
        }

        guestKey = key;
        return CartOwner.ForGuest(key);
    }

    // The 422: the page again with the variant the form named, the typed quantity and the message beside the button (04 5.4.10, 2.3.1.2). A named refusal writes its rejection line first (07 6.1.3; D-638);
    // a field error (code null, the message in the model state) leaves the line to the validation filter (09 3.4).
    private async Task<IActionResult> RefuseAsync(ProductPage page, ProductPageVariant variant, string? typedQuantity, ErrorCode? code, string? message, CancellationToken ct)
    {
        if (code is { } refused)
        {
            HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(refused);
        }

        View = await BuildAsync(page, variant, BuyState.Of(typedQuantity, message), ct);
        return FormResults.Redisplay(this);
    }

    private async Task<ProductView> BuildAsync(ProductPage page, ProductPageVariant? shown, BuyState state, CancellationToken ct)
    {
        string slug = page.Slug ?? string.Empty;
        string path = ProductViews.PathOf(slug);
        string name = page.Name ?? string.Empty;
        if (page.Kind == ProductPageKind.DraftPreview && page.ProductId is Guid draftId)
        {
            HttpContext.SetDraftPreview($"{PanelFramePaths.Products}/{draftId:D}");
        }

        if (page.Kind == ProductPageKind.Archived)
        {
            IReadOnlyList<GalleryImage> archivedGallery = page.ProductId is Guid archivedId ? await _catalog.GetProductGalleryAsync(archivedId, ct) : [];
            ProductImage? main = ImageOf(archivedGallery.FirstOrDefault(i => i.IsMain) ?? (archivedGallery.Count > 0 ? archivedGallery[0] : null));
            return new ProductView(page.Kind, name, slug, path, [], main, [], null, null, [], new ProductBuyArea(BuyMode.None, null, false, string.Empty, null, null), null, null, null, null, false, main?.Path, null);
        }

        IReadOnlyList<CategoryNode> tree = await _catalog.GetCategoryTreeAsync(ct);
        IReadOnlyList<GalleryImage> gallery = page.ProductId is Guid productId ? await _catalog.GetProductGalleryAsync(productId, ct) : [];
        ProductPeriods periods = await ReadPeriodsAsync(ct);
        SalesGate gate = await _settings.GetSalesGateAsync(ct);

        // The variants' labels and images are read once for the product, not once per variant (09 20 item 4); the place of origin is the product's, so one lookup serves every label.
        IReadOnlyDictionary<Guid, PriceLabel> labels = page.ProductId is Guid labelsProductId ? await _catalog.GetProductPriceLabelsAsync(labelsProductId, ct) : new Dictionary<Guid, PriceLabel>();
        IReadOnlyDictionary<Guid, VariantImage> variantImages = page.ProductId is Guid imagesProductId ? await _catalog.GetProductVariantImagesAsync(imagesProductId, ct) : new Dictionary<Guid, VariantImage>();
        Guid? originCountryId = labels.Values.Select(l => l.OriginCountryId).FirstOrDefault(id => id is not null);
        CountryInfo? country = originCountryId is Guid countryId ? await _countries.FindAsync(countryId, ct) : null;

        List<VariantScriptData> script = [];
        ProductPriceBlock? shownPrice = null;
        decimal? shownAmount = null;
        ProductImage? shownVariantImage = null;
        foreach (ProductPageVariant variant in page.Variants)
        {
            if (!labels.TryGetValue(variant.Id, out PriceLabel? label))
            {
                continue;
            }

            VariantImage? variantImage = variantImages.GetValueOrDefault(variant.Id);
            OriginLabel? origin = label.OriginCountryId is not null && country is not null ? OriginLabel.Of(page.Type, country.Code, country.Name) : null;
            ProductPriceBlock block = ProductViews.PriceBlockOf(label, origin);
            ProductImage? image = variantImage is null ? null : new ProductImage(FrameStyle.ImagePath(variantImage.ImageId), variantImage.AltText);
            script.Add(new VariantScriptData(
                variant.Id, variant.FirstValueOrder, variant.SecondValueOrder, block.PriceText, block.ReferencePriceText, block.DiscountMark, block.DiscountDates, block.UnitPriceText,
                image?.Path, image?.AltText, variant.Status == PublishStatus.Published && !variant.IsSoldOut, variant.Status == PublishStatus.Published && variant.IsSoldOut));
            if (shown is not null && variant.Id == shown.Id)
            {
                shownPrice = block;
                shownAmount = label.Price;
                shownVariantImage = variantImage is { IsOwn: true } ? image : null;
            }
        }

        ProductImage? lead = shownVariantImage ?? ImageOf(gallery.FirstOrDefault(i => i.IsMain) ?? (gallery.Count > 0 ? gallery[0] : null));
        List<ProductImage> others = [.. gallery.Select(g => ImageOf(g)).OfType<ProductImage>().Where(i => lead is null || i.Path != lead.Path)];

        FrameSales sales = new(gate.ShowsClosedNotice, gate.IsTemporarilyClosed, gate.IsDataCollectionOpen);
        BuyMode mode = ProductViews.ModeOf(page, shown, sales.IsSalesClosed);
        MessageView? message = mode == BuyMode.SalesClosed ? SalesClosedMessages.ForStorefront(sales, SalesGateOperation.AddToCart) : null;
        if (state.Message is { } refusal)
        {
            message = Messages.InPlace(MessageMeaning.Warning, refusal);
        }

        MessageView? warning = null;
        if (mode == BuyMode.Form && page.Type == ProductType.Digital && shown is not null && StorefrontFrameBuilder.CustomerOf(HttpContext) is Guid memberId)
        {
            Guid? cartId = await _ordering.FindCartIdAsync(CartOwner.ForMember(memberId), ct);
            if (cartId is not null)
            {
                DigitalAddCheck check = await _ordering.CheckDigitalAddAsync(new DigitalAddRequest(shown.Id, page.Type, 1, cartId, memberId), ct);
                if (check.IsAlreadyInCart && check.WasBoughtBefore)
                {
                    warning = Messages.InPlace(MessageMeaning.Warning, CartAddMessages.RepurchaseWarningText);
                }
            }
        }

        ProductBuyArea buy = new(
            mode,
            shown?.Id,
            page.Type != ProductType.Digital,
            state.Quantity ?? "1",
            message,
            warning);

        IReadOnlyList<OptionField> options = VariantSelection.Fields(page, shown);
        string? sku = shown?.Sku;
        return new ProductView(
            page.Kind,
            name,
            slug,
            path,
            ProductViews.TrailOf(page.PrimaryCategoryId, tree),
            lead,
            others,
            shownPrice,
            ProductViews.DeliveryOf(page, periods.DefaultShippingPeriod, periods.Area),
            options,
            buy,
            ProductViews.DescriptionOf(page.Description),
            page.OffersCart ? ProductViews.ScriptDataOf(script) : null,
            sku,
            shownAmount,
            shown is { IsSoldOut: false },
            lead?.Path,
            SearchText.Description(null, page.Description));
    }

    private async Task<ProductPeriods> ReadPeriodsAsync(CancellationToken ct) =>
        new((await _settings.GetPeriodsAsync(ct)).DefaultShippingPeriod, await _settings.GetDeliveryAreaAsync(ct));

    private static ProductImage? ImageOf(GalleryImage? image) => image is null ? null : new ProductImage(FrameStyle.ImagePath(image.ImageId), image.EffectiveAltText);

    private sealed record ProductPeriods(int DefaultShippingPeriod, DeliveryArea Area);

    // What a refused add carries back to the page: the typed quantity and the messages.
    private sealed record BuyState(string? Quantity, string? Message)
    {
        public static BuyState Empty { get; } = new(null, null);

        public static BuyState Of(string? quantity, string? message = null) => new(quantity, message);
    }

    // The add's result as the transaction's value: anything but an added line rolls the work back, so a refusal leaves no cart behind (07 4.4.2).
    private sealed record CartAddAttempt(CartAddResult Result) : IOperationResult
    {
        public bool IsRejected => Result.Outcome != CartAddOutcome.Added;
    }
}
