using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Lists;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Catalog.Contracts;
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

    private readonly IPanelProductForm _form;
    private readonly IProductTransitions _transitions;
    private readonly IProductRemoval _removal;
    private readonly ITransactionRunner _transactions;
    private readonly RejectionLog _rejections;

    public ProductFormModel(IPanelProductForm form, IProductTransitions transitions, IProductRemoval removal, ITransactionRunner transactions, RejectionLog rejections)
    {
        ArgumentNullException.ThrowIfNull(form);
        ArgumentNullException.ThrowIfNull(transitions);
        ArgumentNullException.ThrowIfNull(removal);
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentNullException.ThrowIfNull(rejections);
        _form = form;
        _transitions = transitions;
        _removal = removal;
        _transactions = transactions;
        _rejections = rejections;
    }

    /// <summary>The record the form shows; null on the new product's form.</summary>
    public PanelProductForm? Record { get; private set; }

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

    /// <summary>UC-88: the form. The new product's form is empty; an existing one's key that is no product is 404.</summary>
    public async Task<IActionResult> OnGetAsync(Guid? id, string? returnUrl, CancellationToken ct)
    {
        ReturnPath = ListReturn.Target(returnUrl, PanelFramePaths.Products);
        if (id is null)
        {
            return Page();
        }

        return await LoadAsync(id.Value, ct) ? Page() : NotFound();
    }

    /// <summary>UC-115, the product branch: the target status is absolute - a record already in it is written nothing (07 2.5.1). The gate's refusal names the unmet conditions (422).</summary>
    public async Task<IActionResult> OnPostChangeProductPublishStatusAsync(Guid? id, ChangeProductPublishStatusInput input, string? returnUrl, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
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
        if (id is not { } productId)
        {
            return NotFound();
        }

        if (!input.Confirmed)
        {
            RemovalImpact? impact = await _removal.GetProductImpactAsync(productId, ct);
            if (impact is null || !await LoadAsync(productId, ct))
            {
                return NotFound();
            }

            Confirmation = DeletionConfirmation(Record!.Name, impact.OpenOrderCount, "DeleteProduct", [new("Input.Confirmed", "true")], returnUrl);
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
        if (id is not { } productId || !await LoadAsync(productId, ct) || Record!.Variants.All(v => v.Id != input.VariantId))
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

        ErrorCode code = result.Code!.Value;
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
        Confirmations.ForPermanentDeletion(DeletionSentence(name, openOrderCount), DeleteLabel, FormPath, handler, FormAddress(Record!.Id, returnUrl), fields);

    private static string FormAddress(Guid productId, string? returnUrl)
    {
        string path = PathOf(productId);
        return Shopfolio.Infrastructure.Security.ReturnPath.IsLocalPath(returnUrl) ? ListReturn.LinkTo(path, returnUrl!) : path;
    }

    private async Task<bool> LoadAsync(Guid productId, CancellationToken ct)
    {
        PanelProductForm? record = await _form.GetAsync(productId, ct);
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
        return true;
    }
}
