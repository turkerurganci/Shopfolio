using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Lists;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Panel;

/// <summary>The form of UC-115's product branch (07 5.1.5): the target publish status, written absolutely. It carries no version (K-1189 (1)).</summary>
public sealed class ChangeProductPublishStatusInput : IFormInput
{
    public PublishStatus? PublishStatus { get; set; }
}

/// <summary>The form of UC-115's variant branch (07 5.1.5): the variant's internal key and the target publish status. It carries no version (K-1189 (1)).</summary>
public sealed class ChangeVariantPublishStatusInput : IFormInput
{
    public Guid VariantId { get; set; }

    public PublishStatus? PublishStatus { get; set; }
}

/// <summary>The form of UC-116's product branch (07 5.1.5): the confirmation mark; without it the answer is the confirmation state and nothing is written.</summary>
public sealed class DeleteProductInput : IFormInput
{
    [SpecAssignedName]
    public bool Confirmed { get; set; }
}

/// <summary>The form of UC-116's variant branch (07 5.1.5): the variant's internal key and the confirmation mark.</summary>
public sealed class DeleteVariantInput : IFormInput
{
    public Guid VariantId { get; set; }

    [SpecAssignedName]
    public bool Confirmed { get; set; }
}

/// <summary>One variant row of the form (04 9.5.5): the values the table shows, the badge, the stock's shown value and the reserved units.</summary>
public sealed record ProductVariantView(Guid Id, string Label, string StockCode, string Price, int? StockQuantity, int ReservedQuantity, Badge Badge, PublishStatus PublishStatus);

/// <summary>One publish move button of the side column (04 9.5.10, 9.5.22): the target status and the button's name.</summary>
public sealed record PublishMoveView(PublishStatus Target, string Label);

/// <summary>
/// E-33, the product form (T525; 04 9.5; 07 UC-88, UC-115, UC-116): the new product's empty form at <c>/panel/urunler/yeni</c> and the existing product's form at <c>/panel/urunler/{iç anahtar}</c>
/// - one model behind two addresses (K-1175 (7)). The existing form carries the record's <c>Version</c>, each variant's shown stock value and reserved units and the publish gate's unmet
/// conditions (K-653); the figures are the server's (07 8.1.4). The publish moves and the permanent deletions are their own forms on the side column and the variant rows and carry no version
/// (K-1189 (1)): the handler reads the record's current version in the same transaction. Each operation is one transaction of the catalog module's write method (09 9.1).
/// </summary>
[HandlerBodyClass("SaveProduct", RequestBodyClass.LongText)]
[HandlerBodyClass("UploadImage", RequestBodyClass.ImageUpload)]
internal sealed class ProductFormModel : PageModel
{
    public const string NewTitle = "Yeni ürün";

    public const string SaveLabel = "Kaydet";

    public const string DeleteLabel = "Kalıcı olarak sil";

    public const string PreviewLabel = "Önizle";

    public const string ViewOnSiteLabel = "Sitede gör";

    /// <summary>K-799: the gate refusal; the conditions are named in place of <c>{eksik koşullar}</c>.</summary>
    public static string GateRefusal(string conditions) => $"Ürün yayına alınamadı: {conditions}.";

    /// <summary>K-799: the refusal of the last Published variant of a published product, with its way out.</summary>
    public const string LastPublishedVariantText = "Bu, yayındaki ürünün yayında kalan son seçeneği. Önce ürünü taslağa ya da arşive alın.";

    /// <summary>07 UC-115: a variant whose price is zero is not published under a published product (02 3.7.3).</summary>
    public const string ZeroPriceVariantText = "Seçenek yayına alınamadı: yayındaki üründe fiyatı sıfır olan seçenek yayına alınamaz. Önce fiyatı girin.";

    public const string GateHeading = "Yayın için eksik";

    /// <summary>07 5.2.3: the upload target of the gallery.</summary>
    public const string GalleryTarget = "Images";

    /// <summary>07 5.2.3: the member the upload's one image file is sent as (<c>ImageFile</c>), read through the upload component.</summary>
    public const string ImageFileName = "Input.ImageFile";

    /// <summary>The refusal of an image whose pixel size is over the decoder's ceiling (07 UC-117; 05 6.6.1).</summary>
    public const string PixelLimitText = "Görselin boyutları çok büyük; daha küçük bir görsel yükleyin.";

    /// <summary>The refusal of a request that names no image file or no image field of the form (07 UC-117).</summary>
    public const string NoImageText = "Yüklenecek görseli seçin.";

    private readonly IPanelProductForm _form;
    private readonly IProductTransitions _transitions;
    private readonly IProductRemoval _removal;
    private readonly ICatalogModule _catalog;
    private readonly ISettingsModule _settings;
    private readonly ITransactionRunner _transactions;
    private readonly RejectionLog _rejections;

    // True once a handler ran: the validation filter's 422 runs none (OnPageHandlerExecutionAsync).
    private bool _ran;

    public ProductFormModel(
        IPanelProductForm form,
        IProductTransitions transitions,
        IProductRemoval removal,
        ICatalogModule catalog,
        ISettingsModule settings,
        ITransactionRunner transactions,
        RejectionLog rejections)
    {
        ArgumentNullException.ThrowIfNull(form);
        ArgumentNullException.ThrowIfNull(transitions);
        ArgumentNullException.ThrowIfNull(removal);
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings;
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentNullException.ThrowIfNull(rejections);
        _form = form;
        _transitions = transitions;
        _removal = removal;
        _catalog = catalog;
        _transactions = transactions;
        _rejections = rejections;
    }

    /// <summary>The record the form shows; null on the new product's form.</summary>
    public PanelProductRecord? Record { get; private set; }

    /// <summary>
    /// The form's entries: the record's values when the form opens (07 UC-88), the sent values when it is shown again (UC-114 422, UC-117) - with the record's <c>Version</c> as the form
    /// was opened and, after an upload, the new image in its field (K-1171 (5)).
    /// </summary>
    public SaveProductInput Form { get; private set; } = new();

    /// <summary>The field message of a refused upload (07 UC-117; 04 2.12.8.3), keyed by the upload's target; null otherwise.</summary>
    public MessageView? UploadRefusal { get; private set; }

    /// <summary>The field the refused upload named (<see cref="SaveProductInput.ImageTarget"/>); null otherwise.</summary>
    public string? UploadRefusalTarget { get; private set; }

    public IReadOnlyList<ProductVariantView> Variants { get; private set; } = [];

    /// <summary>The gate's unmet conditions by their screen names (04 9.5.10); empty when the gate is open or on the new form.</summary>
    public IReadOnlyList<string> UnmetConditions { get; private set; } = [];

    public IReadOnlyList<PublishMoveView> ProductMoves { get; private set; } = [];

    /// <summary>The in-place message of a refused publish move or deletion (K-799); null otherwise.</summary>
    public MessageView? Refusal { get; private set; }

    /// <summary>The confirmation state of a deletion (04 9.5.15); null when none waits.</summary>
    public ConfirmView? Confirmation { get; private set; }

    /// <summary>The address the form posts to: its own (07 UC-114…UC-116).</summary>
    public string FormPath { get; private set; } = NewPath;

    /// <summary>The list's address the form returns to (07 2.6.8): the local <c>returnUrl</c>, else the list.</summary>
    public string ReturnPath { get; private set; } = PanelFramePaths.Products;

    public string Title => Record?.Name is { Length: > 0 } name ? name : NewTitle;

    public const string NewPath = PanelFramePaths.Products + "/yeni";

    public static string PathOf(Guid productId) => $"{PanelFramePaths.Products}/{productId:D}";

    /// <summary>
    /// A form whose fields break their rules is answered 422 by the validation filter without the handler (09 3.4 item 3): the form it shows again needs the record as it stands and the sent
    /// entries (07 2.7.1).
    /// </summary>
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        PageHandlerExecutedContext executed = await next();
        if (!_ran && executed.Exception is null && executed.Result is PageResult { StatusCode: StatusCodes.Status422UnprocessableEntity })
        {
            if (context.HandlerArguments.TryGetValue("id", out object? key) && key is Guid productId)
            {
                await LoadAsync(productId, HttpContext.RequestAborted);
            }

            Form = context.HandlerArguments.Values.OfType<SaveProductInput>().FirstOrDefault() ?? Form;
        }
    }

    /// <summary>UC-88: the form. The new product's form is empty; an existing one's key that is no product is 404.</summary>
    public async Task<IActionResult> OnGetAsync(Guid? id, string? returnUrl, CancellationToken ct)
    {
        ReturnPath = ListReturn.Target(returnUrl, PanelFramePaths.Products);
        if (id is null)
        {
            // The new product's VAT rate starts from the firm's default (P-10; 02 3.6.2); the rest is empty.
            Form = new SaveProductInput { VatRate = (await _settings.GetProductDefaultsAsync(ct)).DefaultVatRate, Variants = [new SaveProductVariantRow()] };
            return Page();
        }

        return await LoadAsync(id.Value, ct) ? Page() : NotFound();
    }

    /// <summary>
    /// UC-117: the image is uploaded before the form is saved. Nothing of the record is written and its version stays: the answer is 200 with the form's sent entries and the new image's id in the
    /// target field (K-1171 (5)). 422 at the target field: the content is no image of P-22 or cannot be decoded, it is over P-23 or the pixel ceiling (the module's refusal), the gallery would pass
    /// P-24, or the variant's single image field is already filled. 404 when the form's record is gone. The body's ceiling (P-23 + 1 MB, 413) is the request limit's (<see cref="HandlerBodyClassAttribute"/>).
    /// </summary>
    public async Task<IActionResult> OnPostUploadImageAsync(Guid? id, SaveProductInput input, string? returnUrl, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _ran = true;
        ReturnPath = ListReturn.Target(returnUrl, PanelFramePaths.Products);
        if (id is { } productId && !await LoadAsync(productId, ct))
        {
            return NotFound();
        }

        Form = input;
        int? variantRow = VariantRowOf(input.ImageTarget, input.Variants.Count);
        bool gallery = string.Equals(input.ImageTarget, GalleryTarget, StringComparison.Ordinal);
        byte[]? content = await UploadRequests.ReadFileAsync(Request, ImageFileName, ct);
        if (content is null || (!gallery && variantRow is null))
        {
            return RefuseUpload(input.ImageTarget, ErrorCode.InvalidImage, NoImageText);
        }

        // The count rules first, so a refused upload leaves no unlinked image behind: the gallery as the form holds it (P-24), and the variant's one image (07 5.2.3).
        if (gallery && input.Images.Count >= ProductConstants.ImagesPerProductMax)
        {
            return RefuseUpload(input.ImageTarget, ErrorCode.ImageLimitExceeded, UploadChecks.CountMessage(UploadSurface.ProductGallery));
        }

        if (variantRow is { } row && input.Variants[row].ImageId is not null)
        {
            return RefuseUpload(input.ImageTarget, ErrorCode.ImageLimitExceeded, UploadChecks.CountMessage(UploadSurface.Logo));
        }

        // The unlinked image row and its derivative job are one transaction the module joins (05 7.7.2); the record is not touched.
        ImageUploadResult result = await _transactions.RunAsync(inner => _catalog.UploadImageAsync(content, inner), ct);
        if (result.IsRejected || result.ImageId is not { } imageId)
        {
            return RefuseUpload(input.ImageTarget, ErrorCode.InvalidImage, UploadRefusalText(result));
        }

        if (gallery)
        {
            input.Images.Add(new SaveProductImageRow
            {
                ImageId = imageId,
                SortOrder = input.Images.Count == 0 ? 1 : input.Images.Max(i => i.SortOrder) + 1,
                IsMain = input.Images.Count == 0,
            });
        }
        else if (variantRow is { } variantTarget)
        {
            input.Variants[variantTarget].ImageId = imageId;
        }

        // The form is drawn from Form, not from the posted model state: the new image's id reaches its hidden field.
        ModelState.Clear();
        return Page();
    }

    /// <summary>The row of a variant image target <c>Variants[n].ImageId</c> (07 5.2.3); null when the target is no variant row of the sent form.</summary>
    public static int? VariantRowOf(string? target, int variantCount)
    {
        const string Prefix = "Variants[";
        const string Suffix = "].ImageId";
        if (target is null || !target.StartsWith(Prefix, StringComparison.Ordinal) || !target.EndsWith(Suffix, StringComparison.Ordinal))
        {
            return null;
        }

        string index = target[Prefix.Length..^Suffix.Length];
        return index.Length is > 0 and <= 4 && index.All(char.IsAsciiDigit)
            && int.Parse(index, System.Globalization.CultureInfo.InvariantCulture) is var n && n < variantCount
            ? n
            : null;
    }

    /// <summary>The field message of the image module's refusal (04 2.12.8.3): the format and an undecodable file, the size, the pixel ceiling.</summary>
    public static string UploadRefusalText(ImageUploadResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (ReferenceEquals(result, ImageUploadResult.TooLarge))
        {
            return UploadChecks.SizeMessage(UploadSurface.ProductGallery);
        }

        return ReferenceEquals(result, ImageUploadResult.OverPixelLimit) ? PixelLimitText : UploadChecks.FormatMessage();
    }

    /// <summary>UC-115, the product branch: the target status is absolute - a record already in it is written nothing (07 2.5.1). The gate's refusal names the unmet conditions (422).</summary>
    public async Task<IActionResult> OnPostChangeProductPublishStatusAsync(Guid? id, ChangeProductPublishStatusInput input, string? returnUrl, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _ran = true;
        if (id is not { } productId || input.PublishStatus is not { } target || !Enum.IsDefined(target))
        {
            return NotFound();
        }

        PublishChangeResult result = await _transactions.RunAsync(
            inner => AtCurrentVersionAsync(productId, null, (version, t) => _transitions.ChangeProductStatusAsync(productId, target, version, t), PublishChangeResult.NotFound, inner),
            ct);
        return await PublishAnswerAsync(productId, result, returnUrl, ct);
    }

    /// <summary>UC-115, the variant branch: the variant must be the product's (404 otherwise); the last Published variant of a published product is kept (422).</summary>
    public async Task<IActionResult> OnPostChangeVariantPublishStatusAsync(Guid? id, ChangeVariantPublishStatusInput input, string? returnUrl, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _ran = true;
        if (id is not { } productId || input.PublishStatus is not { } target || !Enum.IsDefined(target))
        {
            return NotFound();
        }

        PublishChangeResult result = await _transactions.RunAsync(
            inner => AtCurrentVersionAsync(productId, input.VariantId, (version, t) => _transitions.ChangeVariantStatusAsync(input.VariantId, target, version, t), PublishChangeResult.NotFound, inner),
            ct);
        return await PublishAnswerAsync(productId, result, returnUrl, ct);
    }

    /// <summary>
    /// UC-116, the product branch: without the mark the answer is the confirmation state with the open order count (200, nothing written; K-655, K-797); with it the product is deleted and
    /// the answer is 303 to the local <c>returnUrl</c>, else the list.
    /// </summary>
    public async Task<IActionResult> OnPostDeleteProductAsync(Guid? id, DeleteProductInput input, string? returnUrl, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _ran = true;
        if (id is not { } productId)
        {
            return NotFound();
        }

        if (!input.Confirmed)
        {
            RemovalImpact? impact = await _removal.GetProductImpactAsync(productId, ct);
            if (impact is null || !await LoadAsync(productId, ct) || Record is not { } loaded)
            {
                return NotFound();
            }

            Confirmation = DeletionConfirmation(loaded.Name, impact.OpenOrderCount, "DeleteProduct", [new("Input.Confirmed", "true")], returnUrl);
            return Page();
        }

        RemovalResult result = await _transactions.RunAsync(
            inner => AtCurrentVersionAsync(productId, null, (version, t) => _removal.DeleteProductAsync(productId, version, t), RemovalResult.NotFound, inner),
            ct);
        return result.IsDone ? FormResults.SeeOther(ListReturn.Target(returnUrl, PanelFramePaths.Products)) : NotFound();
    }

    /// <summary>
    /// UC-116, the variant branch: the confirmation state as with the product; the deletion of the last Published variant of a published product is 422 with K-799's message, and a done
    /// deletion is 303 to the product's form.
    /// </summary>
    public async Task<IActionResult> OnPostDeleteVariantAsync(Guid? id, DeleteVariantInput input, string? returnUrl, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _ran = true;
        if (id is not { } productId || !await LoadAsync(productId, ct) || Record is not { } loaded || loaded.Variants.All(v => v.Id != input.VariantId))
        {
            return NotFound();
        }

        if (!input.Confirmed)
        {
            RemovalImpact? impact = await _removal.GetVariantImpactAsync(input.VariantId, ct);
            if (impact is null)
            {
                return NotFound();
            }

            if (impact.IsLastPublishedVariant)
            {
                return RefuseLastPublishedVariant();
            }

            string label = Variants.Single(v => v.Id == input.VariantId).Label;
            Confirmation = DeletionConfirmation(
                label,
                impact.OpenOrderCount,
                "DeleteVariant",
                [new("Input.VariantId", input.VariantId.ToString("D")), new("Input.Confirmed", "true")],
                returnUrl);
            return Page();
        }

        RemovalResult result = await _transactions.RunAsync(
            inner => AtCurrentVersionAsync(productId, input.VariantId, (version, t) => _removal.DeleteVariantAsync(input.VariantId, version, t), RemovalResult.NotFound, inner),
            ct);
        if (result.Code == ErrorCode.LastPublishedVariant)
        {
            await LoadAsync(productId, ct);
            return RefuseLastPublishedVariant();
        }

        return result.IsDone ? FormResults.SeeOther(FormAddress(productId, returnUrl)) : NotFound();
    }

    /// <summary>The product's own fields of the form (04 9.5.2-9.5.4, 7.2.4): the entries of <see cref="Form"/>, each with its message after a refusal (04 2.3.2).</summary>
    public IReadOnlyList<FieldView> ProductFields()
    {
        FormErrorSummary summary = FormErrorSummary.From(ModelState);
        static string? Whole(int? value) => value?.ToString(System.Globalization.CultureInfo.InvariantCulture);
        List<FieldOption> types =
        [
            new(nameof(ProductType.Physical), ProductsModel.TypeOf(ProductType.Physical)),
            new(nameof(ProductType.Service), ProductsModel.TypeOf(ProductType.Service)),
            new(nameof(ProductType.Digital), ProductsModel.TypeOf(ProductType.Digital)),
        ];
        FieldView[] fields =
        [
            Fields.Text("Input.Name", "Ürün adı", FieldRequirement.Required, Form.Name, ProductConstants.ProductNameMaxLength),
            Fields.ClosedList("Input.ProductType", "Tip", FieldRequirement.Required, types, Form.ProductType?.ToString()),
            Fields.TextArea("Input.Description", "Açıklama", FieldRequirement.Optional, Form.Description),
            Fields.Number("Input.VatRate", "KDV oranı (%)", FieldRequirement.Required, Form.VatRate is { } vat ? RequestFormat.FormatDecimal(vat) : null),
            Fields.Number("Input.ShippingPeriod", "Kargoya verme süresi (gün)", FieldRequirement.Optional, Whole(Form.ShippingPeriod)),
            Fields.Number("Input.FulfilmentPeriod", "İfa süresi (gün)", FieldRequirement.Optional, Whole(Form.FulfilmentPeriod)),
            Fields.Number("Input.MaxQuantityPerOrder", "Siparişte en çok adet", FieldRequirement.Optional, Whole(Form.MaxQuantityPerOrder)),
        ];
        return Fields.BindAll(fields, summary);
    }

    /// <summary>The screen names of the gate's conditions (04 9.5.10): ad, ana kategori, tip, en az bir yayındaki seçenek, yayındaki her seçeneğin dosyası, ifa süresi.</summary>
    public static string ConditionName(string condition) => condition switch
    {
        PublishCondition.Name => "ad",
        PublishCondition.MainCategory => "ana kategori",
        PublishCondition.ProductType => "tip",
        PublishCondition.PublishedVariant => "en az bir yayındaki seçenek",
        PublishCondition.DigitalFile => "yayındaki her seçeneğin dosyası",
        PublishCondition.FulfilmentPeriod => "ifa süresi",
        _ => throw new ArgumentOutOfRangeException(nameof(condition), condition, "Not a publish gate condition."),
    };

    /// <summary>K-797's deletion sentence: "{ad} kalıcı olarak silinecek." and, with open orders, "Ona bağlı {n} açık sipariş donmuş bilgisiyle sürer."</summary>
    public static string DeletionSentence(string name, int openOrderCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(openOrderCount);
        string sentence = $"{name} kalıcı olarak silinecek.";
        return openOrderCount > 0
            ? $"{sentence} Ona bağlı {openOrderCount.ToString(System.Globalization.CultureInfo.InvariantCulture)} açık sipariş donmuş bilgisiyle sürer."
            : sentence;
    }

    /// <summary>The side column's moves of a status (04 9.5.22): every status but the current one, in the order publish, draft, archive.</summary>
    public static IReadOnlyList<PublishMoveView> MovesOf(PublishStatus current) =>
        [.. Enum.GetValues<PublishStatus>().Where(s => s != current).Select(s => new PublishMoveView(s, MoveLabel(s)))];

    public static string MoveLabel(PublishStatus target) => target switch
    {
        PublishStatus.Published => "Yayına al",
        PublishStatus.Draft => "Taslağa al",
        PublishStatus.Archived => "Arşive al",
        _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Not a publish status."),
    };

    // The publish moves and the deletions carry no version (K-1189 (1)): the record's current version is read in the operation's own transaction and handed to the module, which
    // compares it under its lock. A save that lands between the read and the lock moves the version on; the read is then repeated once, so the move applies to the current record.
    private async Task<T> AtCurrentVersionAsync<T>(Guid productId, Guid? variantId, Func<int, CancellationToken, Task<T>> write, T notFound, CancellationToken ct)
        where T : IOperationResult
    {
        for (int attempt = 0; attempt < 2; attempt++)
        {
            int? version = await _form.GetVersionAsync(productId, variantId, ct);
            if (version is null)
            {
                return notFound;
            }

            T result = await write(version.Value, ct);
            if (!IsVersionConflict(result))
            {
                return result;
            }
        }

        return notFound;
    }

    private static bool IsVersionConflict(IOperationResult result) => result switch
    {
        PublishChangeResult publish => publish.Code == ErrorCode.RecordVersionConflict,
        RemovalResult removal => removal.Code == ErrorCode.RecordVersionConflict,
        _ => false,
    };

    private async Task<IActionResult> PublishAnswerAsync(Guid productId, PublishChangeResult result, string? returnUrl, CancellationToken ct)
    {
        if (!result.IsRejected)
        {
            return FormResults.SeeOther(FormAddress(productId, returnUrl));
        }

        ErrorCode code = result.Code ?? throw new InvalidOperationException("A rejected publish change carries its code.");
        if (code == ErrorCode.NotFound || !await LoadAsync(productId, ct))
        {
            return NotFound();
        }

        _rejections.Rejected(code);
        if (code == ErrorCode.LastPublishedVariant)
        {
            Refusal = Messages.InPlace(MessageMeaning.Error, LastPublishedVariantText, "product-publish-refused");
        }
        else if (ReferenceEquals(result, PublishChangeResult.ZeroPriceVariant))
        {
            Refusal = Messages.InPlace(MessageMeaning.Error, ZeroPriceVariantText, "product-publish-refused");
        }
        else
        {
            string conditions = string.Join(", ", result.UnmetConditions.Select(ConditionName));
            Refusal = Messages.InPlace(MessageMeaning.Error, GateRefusal(conditions), "product-publish-refused");
        }

        return FormResults.Redisplay(this);
    }

    private IActionResult RefuseLastPublishedVariant()
    {
        _rejections.Rejected(ErrorCode.LastPublishedVariant);
        Refusal = Messages.InPlace(MessageMeaning.Error, LastPublishedVariantText, "product-publish-refused");
        return FormResults.Redisplay(this);
    }

    private ConfirmView DeletionConfirmation(string name, int openOrderCount, string handler, IReadOnlyList<KeyValuePair<string, string>> fields, string? returnUrl) =>
        Confirmations.ForPermanentDeletion(DeletionSentence(name, openOrderCount), DeleteLabel, FormPath, handler, FormAddress((Record ?? throw new InvalidOperationException("The confirmation is drawn for a loaded record.")).Id, returnUrl), fields);

    private static string FormAddress(Guid productId, string? returnUrl)
    {
        string path = PathOf(productId);
        return returnUrl is { } back && Shopfolio.Infrastructure.Security.ReturnPath.IsLocalPath(back) ? ListReturn.LinkTo(path, back) : path;
    }

    private async Task<bool> LoadAsync(Guid productId, CancellationToken ct)
    {
        PanelProductRecord? record = await _form.GetAsync(productId, ct);
        if (record is null)
        {
            return false;
        }

        Record = record;
        FormPath = PathOf(productId);
        Variants =
        [
            .. record.Variants.Select(v => new ProductVariantView(
                v.Id,
                v.OptionValues.Count > 0 ? string.Join(" / ", v.OptionValues) : record.Name,
                v.StockCode,
                DisplayFormat.Amount(v.Price),
                v.StockQuantity,
                v.ReservedQuantity,
                StatusBadges.For(v.PublishStatus),
                v.PublishStatus)),
        ];
        UnmetConditions = [.. record.UnmetGateConditions.Select(ConditionName)];
        ProductMoves = MovesOf(record.PublishStatus);
        Form = InputOf(record);
        return true;
    }

    /// <summary>The form's entries of a record as it stands (07 UC-88): its fields, variants with the shown stock value, gallery and <c>Version</c>.</summary>
    public static SaveProductInput InputOf(PanelProductRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        return new SaveProductInput
        {
            ProductType = record.ProductType,
            Name = record.Name,
            Description = record.Description,
            VatRate = record.VatRate,
            OriginCountryId = record.OriginCountryId,
            MeasurementUnit = record.MeasurementUnit,
            ShippingPeriod = record.ShippingPeriod,
            FulfilmentPeriod = record.FulfilmentPeriod,
            MaxQuantityPerOrder = record.MaxQuantityPerOrder,
            WithdrawalExemptionReason = record.WithdrawalExemptionReason,
            Variants =
            [
                .. record.Variants.Select(v => new SaveProductVariantRow
                {
                    VariantId = v.Id,
                    SKU = v.StockCode,
                    Price = v.Price,
                    NetQuantity = v.NetQuantity,
                    StockQuantity = v.StockQuantity,
                    OriginalStockQuantity = v.StockQuantity?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                    ImageId = v.ImageId,
                }),
            ],
            Images = [.. record.Images.Select(i => new SaveProductImageRow { ImageId = i.ImageId, SortOrder = i.SortOrder, IsMain = i.IsMain, AltText = i.AltText })],
            Version = record.Version,
        };
    }

    private IActionResult RefuseUpload(string? target, ErrorCode code, string text)
    {
        _rejections.Rejected(code);
        UploadRefusalTarget = target;
        UploadRefusal = Messages.ForField(UploadFieldId(target), text);
        return FormResults.Redisplay(this);
    }

    /// <summary>The element id of an upload target's field: <c>product-images</c> for the gallery, <c>product-variant-{n}-image</c> for a variant row.</summary>
    public static string UploadFieldId(string? target) =>
        VariantRowOf(target, int.MaxValue) is { } row
            ? $"product-variant-{row.ToString(System.Globalization.CultureInfo.InvariantCulture)}-image"
            : "product-images";
}
