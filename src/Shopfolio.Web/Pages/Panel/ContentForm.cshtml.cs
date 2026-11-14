using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Panel;

/// <summary>An image of a content record's gallery as the form carries it (06 4.71; 04 7.2.16.9): the image's key, its place, the main mark and the optional alternative text.</summary>
public sealed class SaveContentImageRow
{
    public Guid ImageId { get; set; }

    public int SortOrder { get; set; }

    public bool IsMain { get; set; }

    public string? AltText { get; set; }
}

/// <summary>A linked product as the form carries it (06 4.53; 04 7.2.16.10): the product's key, its place in the choice and the mark that unlinks it.</summary>
public sealed class SaveContentProductRow
{
    public Guid ProductId { get; set; }

    public int SortOrder { get; set; }

    public bool Remove { get; set; }
}

/// <summary>
/// The form input of the content record (07 5.2 <c>SaveContentInput</c>, UC-158, UC-117; 04 7.2.16): the fields of every type in one model - a type reads its own - the gallery, the linked products in the
/// chosen order and the version the form was opened with. The publish status and the manual order are not in the form. <see cref="Search"/> is read by the product search only; <see cref="AddProducts"/> are the
/// ticked results of the last search, which join the end of the linked products on the next send (K-1193 (3)); <see cref="ImageTarget"/> and the file <c>ImageFile</c> are read by the upload only (07 5.2.3).
/// </summary>
public sealed class SaveContentInput : IFormInput
{
    [Display(Name = "Kısa tanıtım")]
    public string? ShortIntroduction { get; set; }

    [Display(Name = "Uzun metin")]
    public string? LongText { get; set; }

    [Display(Name = "Ad")]
    public string? Name { get; set; }

    [Display(Name = "Başlık")]
    public string? Title { get; set; }

    [Display(Name = "Kısa açıklama")]
    public string? ShortDescription { get; set; }

    [Display(Name = "Metin")]
    public string? Text { get; set; }

    public bool IsShownOnHomepage { get; set; }

    [Display(Name = "Soru")]
    public string? Question { get; set; }

    [Display(Name = "Cevap")]
    public string? Answer { get; set; }

    [Display(Name = "İl")]
    public Guid? ProvinceId { get; set; }

    [Display(Name = "İlçe")]
    public string? District { get; set; }

    [Display(Name = "Adres")]
    public string? AddressLine { get; set; }

    [Display(Name = "Telefon")]
    public string? Phone { get; set; }

    [Display(Name = "Çalışma saatleri")]
    public string? WorkingHours { get; set; }

    [Display(Name = "Harita bağlantısı")]
    public string? MapUrl { get; set; }

    public Guid? ImageId { get; set; }

    [Display(Name = "Görselin alternatif metni")]
    public string? ImageAltText { get; set; }

    public bool IsShownInMenu { get; set; }

    [Display(Name = "Bağlantı")]
    public string? LinkUrl { get; set; }

    [Display(Name = "Başlangıç")]
    [InvalidDate]
    public DateOnly? StartDate { get; set; }

    [Display(Name = "Bitiş")]
    [InvalidDate]
    public DateOnly? EndDate { get; set; }

    [SpecAssignedName]
    public IList<SaveContentImageRow> Images { get; set; } = [];

    [SpecAssignedName]
    public IList<SaveContentProductRow> RelatedProducts { get; set; } = [];

    [SpecAssignedName]
    public IList<Guid> AddProducts { get; set; } = [];

    public int? Version { get; set; }

    [SpecAssignedName]
    public string? Search { get; set; }

    [SpecAssignedName]
    public string? ImageTarget { get; set; }
}

/// <summary>The form of UC-159 (07 5.2 <c>ChangePublishStatusInput</c>): the target publish status, written absolutely. It carries no version (K-1193 (1)).</summary>
public sealed class ChangePublishStatusInput : IFormInput
{
    public PublishStatus? PublishStatus { get; set; }
}

/// <summary>The form of UC-160 (07 5.2 <c>DeleteContentInput</c>): the confirmation mark; without it the answer is the confirmation state and nothing is written. It carries no version (K-1193 (1)).</summary>
public sealed class DeleteContentInput : IFormInput
{
    [SpecAssignedName]
    public bool Confirmed { get; set; }
}

/// <summary>One linked product of the form: its key, its name and status as the catalog says them, its place and whether the site shows it (a product that is not published is not shown; 02 3.27.10).</summary>
public sealed record LinkedProductView(Guid ProductId, string Name, PublishStatus PublishStatus, int SortOrder, bool Remove);

/// <summary>One result of the product search: the product, its status badge and whether it is already linked.</summary>
public sealed record ProductSearchHit(Guid ProductId, string Name, PublishStatus PublishStatus, bool IsLinked);

/// <summary>
/// E-41's record form (T534; 04 9.13.4, 9.13.5, 9.13.7-9.13.13; 07 UC-96, UC-158...UC-160, UC-117): the new record's empty form at <c>/panel/kurumsal-icerik/{tip}/yeni</c> and the record's form at
/// <c>/panel/kurumsal-icerik/{tip}/{iç anahtar}</c> - one model behind two addresses. The form carries the record's <c>Version</c>; the side column carries the badge, the gate's empty fields by name, the
/// preview or site link, the publish moves and the deletion, each its own form with no version (K-1193 (1)). A new Hakkımızda or announcement address with the record already there answers 303 to the record
/// (K-1193 (2)). The figures and the gate are the module's (07 8.1.4); the page decides none.
/// </summary>
[HandlerBodyClass("SaveContent", RequestBodyClass.LongText)]
[HandlerBodyClass("SearchProducts", RequestBodyClass.LongText)]
[HandlerBodyClass("UploadImage", RequestBodyClass.ImageUpload)]
internal sealed class ContentFormModel : PageModel, IOutcomeTarget
{
    public const string DeleteLabel = "Kalıcı olarak sil";

    public const string PreviewLabel = "Önizle";

    public const string ViewOnSiteLabel = "Sitede gör";

    public const string GateHeading = "Yayın için eksik";

    public const string NoImageText = "Yüklenecek görseli seçin.";

    public const string ExpiredImageText = "Görsel artık yok; yeniden yükleyin.";

    public const string GalleryTarget = ContentTargets.Gallery;

    public const string BranchImageTarget = ContentTargets.BranchImage;

    public const string ImageFileName = "Input.ImageFile";

    public const string AboutNotDeletableText = "Hakkımızda silinemez; yerine taslağa alabilirsiniz.";

    public const string NotInDraftText = "Taslağa almak için kayıt yayında olmalı.";

    public const string DraftNote = "Taslak kayıt sitede görünmez.";

    public const string UnpublishedProductText = "Yayında değil; vitrinde görünmez.";

    /// <summary>The unique indexes of the three page addresses: two records that made the same address at once - the second is retried (09 9.4 item 3; 06 4.72.3).</summary>
    private static readonly string[] AddressIndexes = ["ix_offerings_slug", "ix_portfolio_items_slug", "ix_custom_pages_slug"];

    private readonly ICorporateContentModule _content;
    private readonly ICatalogModule _catalog;
    private readonly IPanelProductList _products;
    private readonly ISettingsModule _settings;
    private readonly ITransactionRunner _transactions;
    private readonly RejectionLog _rejections;
    private bool _ran;
    private string _outcomeScreen = PanelFramePaths.CorporateContent;

    public ContentFormModel(
        ICorporateContentModule content,
        ICatalogModule catalog,
        IPanelProductList products,
        ISettingsModule settings,
        ITransactionRunner transactions,
        RejectionLog rejections)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(products);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentNullException.ThrowIfNull(rejections);
        _content = content;
        _catalog = catalog;
        _products = products;
        _settings = settings;
        _transactions = transactions;
        _rejections = rejections;
    }

    public PanelContentType Type { get; private set; }

    /// <summary>The record as stored; null on the new record's form. Its version, status, gate state and address are the side column's.</summary>
    public PanelContentForm? Record { get; private set; }

    /// <summary>The entries the form shows: the record's values when it opens, the sent values when it is shown again.</summary>
    public SaveContentInput Form { get; private set; } = new();

    public IReadOnlyList<ProvinceInfo> Provinces { get; private set; } = [];

    public IReadOnlyList<LinkedProductView> LinkedProducts { get; private set; } = [];

    public IReadOnlyList<ProductSearchHit> SearchHits { get; private set; } = [];

    public bool HasSearched { get; private set; }

    public MessageView? Notice { get; private set; }

    /// <summary>The in-place message of a refused publish move, edit or deletion (K-812); null otherwise.</summary>
    public MessageView? Refusal { get; private set; }

    public ConfirmView? Confirmation { get; private set; }

    public MessageView? UploadRefusal { get; private set; }

    public string? UploadRefusalTarget { get; private set; }

    public bool IsConflicted { get; private set; }

    public string FormPath { get; private set; } = ContentRoutes.Root;

    public string Title => Record?.Id is null ? ContentRoutes.NewLabelOf(Type) : ContentRoutes.GroupNameOf(Type);

    public string TypeName => ContentRoutes.GroupNameOf(Type);

    public string ListPath => PanelFramePaths.CorporateContent;

    /// <summary>The place a result that is uncertain returns to: the list shows what happened (07 UC-160; 09 8.1 item 4).</summary>
    public string OutcomeScreen() => _outcomeScreen;

    public bool HasGallery => Type is PanelContentType.AboutPage or PanelContentType.Offering or PanelContentType.PortfolioItem or PanelContentType.FaqItem or PanelContentType.CustomPage;

    public bool HasProducts => Type is PanelContentType.Offering or PanelContentType.PortfolioItem;

    public bool HasMark => Type is PanelContentType.Offering or PanelContentType.PortfolioItem;

    public bool IsBranch => Type == PanelContentType.Branch;

    /// <summary>The publish move buttons of the side column (04 9.13.5): "Yayına al" for a draft, "Taslağa al" for a published record.</summary>
    public static (PublishStatus Target, string Label) MoveOf(PublishStatus current) =>
        current == PublishStatus.Published ? (PublishStatus.Draft, "Taslağa al") : (PublishStatus.Published, "Yayına al");

    /// <summary>The screen names of the gate's fields (04 9.13.5; K-584).</summary>
    public static string FieldName(PanelContentField field) => field switch
    {
        PanelContentField.Name => "ad",
        PanelContentField.Title => "başlık",
        PanelContentField.Question => "soru",
        PanelContentField.Answer => "cevap",
        PanelContentField.Province => "il",
        PanelContentField.District => "ilçe",
        PanelContentField.AddressLine => "adres",
        PanelContentField.ShortIntroduction => "kısa tanıtım",
        PanelContentField.Text => "metin",
        _ => throw new ArgumentOutOfRangeException(nameof(field), field, "Not a gate field."),
    };

    /// <summary>K-812: the gate's refusal; the fields are named in place of <c>{eksik alanlar}</c>.</summary>
    public static string GateRefusal(IEnumerable<PanelContentField> missing) => $"Kayıt yayına alınamadı: {string.Join(", ", missing.Select(FieldName))} gerekli.";

    /// <summary>K-812 (03 3.5.3.11): the edit of a published record that would empty a gate field is not saved; the text names the field and suggests the draft first.</summary>
    public static string PublishedGateRefusal(IEnumerable<PanelContentField> missing) =>
        $"Yayındaki kaydın {string.Join(", ", missing.Select(FieldName))} alanı yayın için gerekli; boşaltılamaz. Önce kaydı taslağa alın.";

    /// <summary>K-812's deletion sentence: "{ad} kalıcı olarak silinecek."</summary>
    public static string DeletionSentence(string name) => $"{name} kalıcı olarak silinecek.";

    /// <summary>The form member a gate field is written at.</summary>
    public static string MemberOf(PanelContentField field) => field switch
    {
        PanelContentField.Province => nameof(SaveContentInput.ProvinceId),
        _ => field.ToString(),
    };

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        PageHandlerExecutedContext executed = await next();
        if (!_ran && executed.Exception is null && executed.Result is PageResult { StatusCode: StatusCodes.Status422UnprocessableEntity }
            && context.HandlerArguments.TryGetValue("tip", out object? segment) && ContentRoutes.TryParse(segment as string, out PanelContentType type))
        {
            // A form whose fields break their rules is answered 422 by the validation filter without the handler: the record must still exist (404 first), and the page needs the sent entries.
            Guid? id = context.HandlerArguments.TryGetValue("id", out object? key) ? key as Guid? : null;
            if (!await LoadAsync(type, id, HttpContext.RequestAborted))
            {
                executed.Result = NotFound();
                return;
            }

            Form = context.HandlerArguments.Values.OfType<SaveContentInput>().FirstOrDefault() ?? Form;
            await LoadLinkedAsync(HttpContext.RequestAborted);
        }
    }

    /// <summary>UC-96: the new record's form or the record's form; a type outside the list or a key that is no record of the type is 404; a new Hakkımızda or announcement with the record there is 303 to it.</summary>
    public async Task<IActionResult> OnGetAsync(string tip, Guid? id, CancellationToken ct)
    {
        if (!ContentRoutes.TryParse(tip, out PanelContentType type))
        {
            return NotFound();
        }

        if (id is null && ContentRoutes.IsSingle(type) && await _content.FindSinglePanelContentAsync(type, ct) is { } existing)
        {
            return FormResults.SeeOther(ContentRoutes.FormPath(type, existing));
        }

        if (!await LoadAsync(type, id, ct))
        {
            return NotFound();
        }

        Form = InputOf(Record!);
        await LoadLinkedAsync(ct);
        return Page();
    }

    /// <summary>
    /// UC-158 <c>SaveContent</c>: the whole form in one transaction through the module's one writer. The order of the refusals is the module's: 404, the fields' 422 at their fields, 409 (the version, or the
    /// single record that opened meanwhile - the form then binds to the record that is there), the published record's gate (422). Saved: 303 to the record's form.
    /// </summary>
    public async Task<IActionResult> OnPostSaveContentAsync(string tip, Guid? id, SaveContentInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _ran = true;
        if (!ContentRoutes.TryParse(tip, out PanelContentType type))
        {
            return NotFound();
        }

        PanelContentSaveCommand command = CommandOf(type, id, input);
        PanelContentSaveResult result = await _transactions.RunAsync(token => _content.SavePanelContentAsync(command, token), AddressIndexes, ct);
        if (!result.IsRejected && result.Id is { } savedId)
        {
            return FormResults.SeeOther(ContentRoutes.FormPath(type, savedId), NoticeCode.Saved);
        }

        if (result.Outcome == PanelContentSaveOutcome.NotFound || (id is { } gone && !await LoadAsync(type, gone, ct)))
        {
            return NotFound();
        }

        // The form is shown again with the sent entries; the record (or the one that is there now) is the side column's.
        Guid? shownId = result.Outcome == PanelContentSaveOutcome.AlreadyExists ? result.ExistingId : id;
        if (id is null && shownId is not null)
        {
            await LoadAsync(type, shownId, ct);
        }
        else if (Record is null)
        {
            await LoadAsync(type, id, ct);
        }

        Form = input;
        OmitRemoved(input);
        await LoadLinkedAsync(ct);
        if (CodeOf(result.Outcome) is { } code)
        {
            _rejections.Rejected(code);
        }

        switch (result.Outcome)
        {
            case PanelContentSaveOutcome.VersionConflict or PanelContentSaveOutcome.AlreadyExists:
                // 07 2.4.5, 2.4.8: the typed entries stay; the form carries the record's current version (and, for the single record that opened meanwhile, its address), so "Yine de kaydet" is a save like any other.
                IsConflicted = true;
                input.Version = Record?.Version;
                ModelState.Clear();
                return FormResults.Conflict(this);
            case PanelContentSaveOutcome.PublishedGateBroken:
                foreach (PanelContentField missing in result.Missing)
                {
                    ModelState.AddModelError("Input." + MemberOf(missing), PublishedGateRefusal([missing]));
                }

                Refusal = Messages.InPlace(MessageMeaning.Error, PublishedGateRefusal(result.Missing), "content-save-refused");
                return FormResults.Redisplay(this);
            default:
                AddRejections(result.Rejections);
                return FormResults.Redisplay(this);
        }
    }

    /// <summary>
    /// UC-158 <c>SearchProducts</c>: the interim send of the same form - it writes nothing. The form is shown again with every sent entry and its version and, under the search field, the products whose name has the
    /// word - draft and archived ones too, each with its status badge; the ticked ones join the end of the linked products on the next send (K-1193 (3)).
    /// </summary>
    public async Task<IActionResult> OnPostSearchProductsAsync(string tip, Guid? id, SaveContentInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _ran = true;
        if (!ContentRoutes.TryParse(tip, out PanelContentType type) || !await LoadAsync(type, id, ct))
        {
            return NotFound();
        }

        Form = input;
        AppendChosen(input);
        await LoadLinkedAsync(ct);
        string? word = string.IsNullOrWhiteSpace(input.Search) ? null : input.Search.Trim();
        HasSearched = true;
        if (word is not null)
        {
            PanelProductPage page = await _products.ListAsync(word, null, 1, ListCatalog_PageSize, ct);
            HashSet<Guid> linked = [.. input.RelatedProducts.Select(r => r.ProductId)];
            SearchHits = [.. page.Rows.Select(row => new ProductSearchHit(row.Id, row.Name, row.PublishStatus, linked.Contains(row.Id)))];
        }

        ModelState.Clear();
        return Page();
    }

    /// <summary>
    /// UC-117: the image is uploaded before the form is saved. Nothing of the record is written and its version stays: the answer is 200 with the form's sent entries and the new image in its field (K-1171 (5)). 422 at
    /// the target field: the content is no image of P-22 or cannot be decoded, it is over P-23 or the pixel ceiling, the gallery would pass P-25, or the branch's one image is there already. 404 when the form's record is gone.
    /// </summary>
    public async Task<IActionResult> OnPostUploadImageAsync(string tip, Guid? id, SaveContentInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _ran = true;
        if (!ContentRoutes.TryParse(tip, out PanelContentType type) || !await LoadAsync(type, id, ct))
        {
            return NotFound();
        }

        Form = input;
        AppendChosen(input);
        await LoadLinkedAsync(ct);
        bool gallery = HasGallery && string.Equals(input.ImageTarget, GalleryTarget, StringComparison.Ordinal);
        bool branch = IsBranch && string.Equals(input.ImageTarget, BranchImageTarget, StringComparison.Ordinal);
        byte[]? content = await UploadRequests.ReadFileAsync(Request, ImageFileName, ct);
        if (content is null || !(gallery || branch))
        {
            return RefuseUpload(input.ImageTarget, ErrorCode.InvalidImage, NoImageText);
        }

        UploadSurface surface = gallery ? UploadSurface.ContentGallery : UploadSurface.Logo;
        if (gallery && input.Images.Count >= ProductConstants.ImagesPerContentRecordMax)
        {
            return RefuseUpload(input.ImageTarget, ErrorCode.ImageLimitExceeded, UploadChecks.CountMessage(surface));
        }

        if (branch && input.ImageId is not null)
        {
            return RefuseUpload(input.ImageTarget, ErrorCode.ImageLimitExceeded, UploadChecks.CountMessage(surface));
        }

        ImageUploadResult result = await _transactions.RunAsync(inner => _catalog.UploadImageAsync(content, inner), ct);
        if (result.IsRejected || result.ImageId is not { } imageId)
        {
            return RefuseUpload(input.ImageTarget, ErrorCode.InvalidImage, UploadRefusalText(result, surface));
        }

        if (gallery)
        {
            input.Images.Add(new SaveContentImageRow
            {
                ImageId = imageId,
                SortOrder = input.Images.Count == 0 ? 1 : input.Images.Max(i => i.SortOrder) + 1,
                IsMain = input.Images.Count == 0,
            });
        }
        else
        {
            input.ImageId = imageId;
        }

        // The form is drawn from Form, not from the posted model state: the new image's key reaches its hidden field.
        ModelState.Clear();
        return Page();
    }

    /// <summary>UC-159: the target status is absolute - a record already in it is written nothing (07 2.5.1). The gate's refusal names the empty fields (422); the move carries no version (K-1193 (1)).</summary>
    public async Task<IActionResult> OnPostChangePublishStatusAsync(string tip, Guid? id, ChangePublishStatusInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _ran = true;
        if (!ContentRoutes.TryParse(tip, out PanelContentType type) || id is not { } key || input.PublishStatus is not { } target || target is not (PublishStatus.Draft or PublishStatus.Published))
        {
            return NotFound();
        }

        PanelContentChangeResult result = await _transactions.RunAsync(token => _content.ChangePanelContentStatusAsync(type, key, target, token), ct);
        if (!result.IsRejected)
        {
            return FormResults.SeeOther(ContentRoutes.FormPath(type, key));
        }

        if (result.Outcome == PanelContentChangeOutcome.NotFound || !await LoadAsync(type, key, ct))
        {
            return NotFound();
        }

        _rejections.Rejected(ErrorCode.PublishGateIncomplete);
        Form = InputOf(Record!);
        await LoadLinkedAsync(ct);
        Refusal = Messages.InPlace(MessageMeaning.Error, GateRefusal(result.Missing), "content-publish-refused");
        return FormResults.Redisplay(this);
    }

    /// <summary>
    /// UC-160: without the mark the answer is the confirmation state (200, nothing written; K-797, K-812); with it the record is deleted and the answer is 303 to the list. Hakkımızda is 422 - the window is not offered; a record
    /// that is not there is 404; a repeated send finds no record - 404. The result of a confirmed deletion that is uncertain returns to the list (09 8.1 item 4).
    /// </summary>
    [OutcomeUncertain("UC-160")]
    public async Task<IActionResult> OnPostDeleteContentAsync(string tip, Guid? id, DeleteContentInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _ran = true;
        _outcomeScreen = PanelFramePaths.CorporateContent;
        if (!ContentRoutes.TryParse(tip, out PanelContentType type) || id is not { } key || !await LoadAsync(type, key, ct))
        {
            return NotFound();
        }

        Form = InputOf(Record!);
        await LoadLinkedAsync(ct);
        if (type == PanelContentType.AboutPage)
        {
            _rejections.Rejected(ErrorCode.AboutPageNotDeletable);
            Refusal = Messages.InPlace(MessageMeaning.Error, AboutNotDeletableText, "content-delete-refused");
            return FormResults.Redisplay(this);
        }

        if (!input.Confirmed)
        {
            string name = Record!.Values.Name ?? Record.Values.Title ?? Record.Values.Question ?? Record.Values.Text ?? TypeName;
            Confirmation = Confirmations.ForPermanentDeletion(DeletionSentence(name), DeleteLabel, FormPath, "DeleteContent", FormPath, [new("Input.Confirmed", "true")]);
            return Page();
        }

        PanelContentDeleteResult result = await _transactions.RunAsync(token => _content.DeletePanelContentAsync(type, key, token), ct);
        return result.Outcome switch
        {
            PanelContentDeleteOutcome.Deleted => FormResults.SeeOther(PanelFramePaths.CorporateContent),
            PanelContentDeleteOutcome.Forbidden => FormResults.Redisplay(this),
            _ => NotFound(),
        };
    }

    /// <summary>The field message of the image module's refusal (04 2.12.8.3): the format and an undecodable file, the size, the pixel ceiling.</summary>
    public static string UploadRefusalText(ImageUploadResult result, UploadSurface surface)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (ReferenceEquals(result, ImageUploadResult.TooLarge))
        {
            return UploadChecks.SizeMessage(surface);
        }

        return ReferenceEquals(result, ImageUploadResult.OverPixelLimit) ? ProductFormModel.PixelLimitText : UploadChecks.FormatMessage();
    }

    /// <summary>The element id of an upload target's field.</summary>
    public static string UploadFieldId(string? target) => string.Equals(target, BranchImageTarget, StringComparison.Ordinal) ? "content-branch-image" : "content-images";

    /// <summary>The fields of the type in the order the screen draws them (04 9.13.4); the gate's fields are "yayın için zorunlu" (K-584).</summary>
    public IReadOnlyList<FieldView> TypeFields()
    {
        FormErrorSummary summary = FormErrorSummary.From(ModelState);
        SaveContentInput f = Form;
        string? Day(DateOnly? value) => value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        List<FieldView> fields = [];
        switch (Type)
        {
            case PanelContentType.AboutPage:
                fields.Add(Components.Fields.TextArea("Input.ShortIntroduction", "Kısa tanıtım", FieldRequirement.RequiredForPublish, f.ShortIntroduction, ProductConstants.ShortIntroductionMaxLength));
                fields.Add(Components.Fields.TextArea("Input.LongText", "Uzun metin", FieldRequirement.Optional, f.LongText));
                break;
            case PanelContentType.Offering:
                fields.Add(Components.Fields.Text("Input.Name", "Ad", FieldRequirement.RequiredForPublish, f.Name, ProductConstants.ProductNameMaxLength));
                fields.Add(Components.Fields.TextArea("Input.ShortDescription", "Kısa açıklama", FieldRequirement.Optional, f.ShortDescription, ProductConstants.ShortTextMaxLength));
                fields.Add(Components.Fields.TextArea("Input.Text", "Metin", FieldRequirement.Optional, f.Text));
                break;
            case PanelContentType.PortfolioItem:
                fields.Add(Components.Fields.Text("Input.Title", "Başlık", FieldRequirement.RequiredForPublish, f.Title, ProductConstants.ProductNameMaxLength));
                fields.Add(Components.Fields.TextArea("Input.ShortDescription", "Kısa açıklama", FieldRequirement.Optional, f.ShortDescription, ProductConstants.ShortTextMaxLength));
                fields.Add(Components.Fields.TextArea("Input.Text", "Metin", FieldRequirement.Optional, f.Text));
                break;
            case PanelContentType.FaqItem:
                fields.Add(Components.Fields.Text("Input.Question", "Soru", FieldRequirement.RequiredForPublish, f.Question, ProductConstants.ShortTextMaxLength));
                fields.Add(Components.Fields.TextArea("Input.Answer", "Cevap", FieldRequirement.RequiredForPublish, f.Answer));
                break;
            case PanelContentType.Branch:
                fields.Add(Components.Fields.Text("Input.Name", "Ad", FieldRequirement.RequiredForPublish, f.Name, ProductConstants.ProductNameMaxLength));
                fields.Add(Components.Fields.ClosedList("Input.ProvinceId", "İl", FieldRequirement.RequiredForPublish, [.. Provinces.Select(p => new FieldOption(p.Id.ToString("D"), p.Name))], f.ProvinceId?.ToString("D")));
                fields.Add(Components.Fields.Text("Input.District", "İlçe", FieldRequirement.RequiredForPublish, f.District));
                fields.Add(Components.Fields.TextArea("Input.AddressLine", "Adres", FieldRequirement.RequiredForPublish, f.AddressLine));
                fields.Add(Components.Fields.Telephone("Input.Phone", "Telefon", FieldRequirement.Optional, f.Phone));
                fields.Add(Components.Fields.Text("Input.WorkingHours", "Çalışma saatleri", FieldRequirement.Optional, f.WorkingHours, ProductConstants.ShortTextMaxLength));
                fields.Add(Components.Fields.Text("Input.MapUrl", "Harita bağlantısı", FieldRequirement.Optional, f.MapUrl));
                break;
            case PanelContentType.CustomPage:
                fields.Add(Components.Fields.Text("Input.Title", "Başlık", FieldRequirement.RequiredForPublish, f.Title, ProductConstants.ProductNameMaxLength));
                fields.Add(Components.Fields.TextArea("Input.Text", "Metin", FieldRequirement.Optional, f.Text));
                break;
            default:
                fields.Add(Components.Fields.TextArea("Input.Text", "Duyuru metni", FieldRequirement.RequiredForPublish, f.Text, ProductConstants.AnnouncementTextMaxLength));
                fields.Add(Components.Fields.Text("Input.LinkUrl", "Bağlantı", FieldRequirement.Optional, f.LinkUrl));
                fields.Add(Components.Fields.Date("Input.StartDate", "Başlangıç", FieldRequirement.Optional, Day(f.StartDate)));
                fields.Add(Components.Fields.Date("Input.EndDate", "Bitiş", FieldRequirement.Optional, Day(f.EndDate)));
                break;
        }

        return Components.Fields.BindAll(fields, summary);
    }

    /// <summary>The field messages of the refused save whose member starts with <paramref name="prefix"/>: the gallery and the list of products are drawn by this page, so their messages are drawn beside them.</summary>
    public IReadOnlyList<MessageView> RefusalsOf(string prefix, string fieldId) =>
    [
        .. ModelState
            .Where(entry => entry.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .SelectMany(entry => entry.Value?.Errors.Select(error => error.ErrorMessage) ?? [])
            .Select(text => Messages.ForField(fieldId, text)),
    ];

    /// <summary>The form's entries of a record as it stands (07 UC-96): its fields, gallery, linked products and <c>Version</c>.</summary>
    public static SaveContentInput InputOf(PanelContentForm record)
    {
        ArgumentNullException.ThrowIfNull(record);
        PanelContentValues v = record.Values;
        return new SaveContentInput
        {
            ShortIntroduction = v.ShortIntroduction,
            LongText = v.LongText,
            Name = v.Name,
            Title = v.Title,
            ShortDescription = v.ShortDescription,
            Text = v.Text,
            IsShownOnHomepage = v.IsShownOnHomepage,
            Question = v.Question,
            Answer = v.Answer,
            ProvinceId = v.ProvinceId,
            District = v.District,
            AddressLine = v.AddressLine,
            Phone = v.Phone,
            WorkingHours = v.WorkingHours,
            MapUrl = v.MapUrl,
            ImageId = v.ImageId,
            ImageAltText = v.ImageAltText,
            IsShownInMenu = v.IsShownInMenu,
            LinkUrl = v.LinkUrl,
            StartDate = v.StartDate,
            EndDate = v.EndDate,
            Images = [.. record.Images.Select((image, i) => new SaveContentImageRow { ImageId = image.ImageId, SortOrder = i + 1, IsMain = image.IsMain, AltText = image.AltText })],
            RelatedProducts = [.. record.ProductIds.Select((productId, i) => new SaveContentProductRow { ProductId = productId, SortOrder = i + 1 })],
            Version = record.Id is null ? null : record.Version,
        };
    }

    // The field refusals are logged field by field when their messages are added; the others are logged here by their code.
    private static ErrorCode? CodeOf(PanelContentSaveOutcome outcome) => outcome switch
    {
        PanelContentSaveOutcome.VersionConflict or PanelContentSaveOutcome.AlreadyExists => ErrorCode.RecordVersionConflict,
        PanelContentSaveOutcome.PublishedGateBroken => ErrorCode.PublishedRecordGateBroken,
        _ => null,
    };

    private const int ListCatalog_PageSize = 25;

    /// <summary>The save's command: the values of the type, the gallery in the posted order and the linked products in the chosen order with the unlinked and the freshly ticked applied.</summary>
    private static PanelContentSaveCommand CommandOf(PanelContentType type, Guid? id, SaveContentInput input)
    {
        AppendChosen(input);
        PanelContentValues values = new()
        {
            ShortIntroduction = input.ShortIntroduction,
            LongText = input.LongText,
            Name = input.Name,
            Title = input.Title,
            ShortDescription = input.ShortDescription,
            Text = input.Text,
            IsShownOnHomepage = input.IsShownOnHomepage,
            Question = input.Question,
            Answer = input.Answer,
            ProvinceId = input.ProvinceId,
            District = input.District,
            AddressLine = input.AddressLine,
            Phone = input.Phone,
            WorkingHours = input.WorkingHours,
            MapUrl = input.MapUrl,
            ImageId = input.ImageId,
            ImageAltText = input.ImageAltText,
            IsShownInMenu = input.IsShownInMenu,
            LinkUrl = input.LinkUrl,
            StartDate = input.StartDate,
            EndDate = input.EndDate,
        };
        IReadOnlyList<GalleryEntry> images = [.. input.Images.OrderBy(i => i.SortOrder).Select(i => new GalleryEntry(i.ImageId, i.AltText, i.IsMain))];
        IReadOnlyList<Guid> products = [.. input.RelatedProducts.Where(r => !r.Remove).OrderBy(r => r.SortOrder).Select(r => r.ProductId)];
        return new PanelContentSaveCommand(type, id, id is null ? null : input.Version ?? 0, values, images, products);
    }

    // The ticked results of the last search join the end of the linked products (K-1193 (3)); a product already linked is not added twice.
    private static void AppendChosen(SaveContentInput input)
    {
        int next = input.RelatedProducts.Count == 0 ? 1 : input.RelatedProducts.Max(r => r.SortOrder) + 1;
        foreach (Guid productId in input.AddProducts.Distinct())
        {
            if (input.RelatedProducts.All(r => r.ProductId != productId))
            {
                input.RelatedProducts.Add(new SaveContentProductRow { ProductId = productId, SortOrder = next++ });
            }
        }

        input.AddProducts = [];
    }

    private static void OmitRemoved(SaveContentInput input) => input.RelatedProducts = [.. input.RelatedProducts.Where(r => !r.Remove)];

    private void AddRejections(IReadOnlyList<PanelContentRejection> rejections)
    {
        foreach (PanelContentRejection rejection in rejections)
        {
            string key = "Input." + rejection.Field;
            if (rejection.Code == FieldCodes.InvalidImage)
            {
                // K-1100: an image the form binds is gone - the upload was cleaned away; the field asks for a new upload.
                ModelState.AddModelError(key, ExpiredImageText);
            }
            else if (rejection.Code == FieldCodes.ImageLimitExceeded)
            {
                ModelState.AddModelError(key, UploadChecks.CountMessage(UploadSurface.ContentGallery));
            }
            else
            {
                ModelState.AddFieldError(key, rejection.Code, ParameterOf(rejection));
            }
        }
    }

    // The limit a catalog text names: the ceiling of the field's own parameter.
    private static string? ParameterOf(PanelContentRejection rejection) => rejection.Code == FieldCodes.TextTooLong
        ? rejection.Field switch
        {
            "ShortIntroduction" => ProductConstants.ShortIntroductionMaxLength.ToString(CultureInfo.InvariantCulture),
            "Name" or "Title" => ProductConstants.ProductNameMaxLength.ToString(CultureInfo.InvariantCulture),
            "Text" => ProductConstants.AnnouncementTextMaxLength.ToString(CultureInfo.InvariantCulture),
            _ => ProductConstants.ShortTextMaxLength.ToString(CultureInfo.InvariantCulture),
        }
        : null;

    private IActionResult RefuseUpload(string? target, ErrorCode code, string text)
    {
        _rejections.Rejected(code);
        UploadRefusalTarget = target;
        UploadRefusal = Messages.ForField(UploadFieldId(target), text);
        return FormResults.Redisplay(this);
    }

    private async Task<bool> LoadAsync(PanelContentType type, Guid? id, CancellationToken ct)
    {
        Type = type;
        PanelContentForm? record = await _content.GetPanelContentFormAsync(type, id, ct);
        if (record is null)
        {
            return false;
        }

        Record = record;
        FormPath = id is { } key ? ContentRoutes.FormPath(type, key) : ContentRoutes.NewPath(type);
        if (type == PanelContentType.Branch)
        {
            Provinces = await _settings.GetProvincesAsync(ct);
        }

        NoticeCode? notice = NoticeParameter.Read(Request, NoticeKind.ShortLived);
        Notice = notice is { } code && NoticeTexts.TryShortLived(code, out string? text) ? Messages.ShortLivedSuccess(text) : null;
        Form = InputOf(record);
        return true;
    }

    // The linked products with their names and statuses as the catalog says them (04 9.13.4); a product that is gone is not listed.
    private async Task LoadLinkedAsync(CancellationToken ct)
    {
        if (!HasProducts)
        {
            return;
        }

        IReadOnlyList<PanelProductName> names = await _products.NamesOfAsync([.. Form.RelatedProducts.Select(r => r.ProductId)], ct);
        Dictionary<Guid, PanelProductName> byId = names.ToDictionary(n => n.Id);
        LinkedProducts =
        [
            .. Form.RelatedProducts
                .OrderBy(r => r.SortOrder)
                .Where(r => byId.ContainsKey(r.ProductId))
                .Select(r => new LinkedProductView(r.ProductId, byId[r.ProductId].Name, byId[r.ProductId].PublishStatus, r.SortOrder, r.Remove)),
        ];
    }
}

/// <summary>The upload targets of the content form (07 5.2.3): the gallery's field and the branch's one image field.</summary>
internal static class ContentTargets
{
    public const string Gallery = "Images";

    public const string BranchImage = "ImageId";
}
