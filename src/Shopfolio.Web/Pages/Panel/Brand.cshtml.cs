using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The form input of the brand screen (E-43; 07 5.2 <c>SaveBrandingInput</c>, UC-163, UC-117; 04 7.2.18): the name, the logo and the site icon as image keys - an emptied key is a removed image -, the colour,
/// the platform signature switch and the version. The two removal marks are the screen's own: a ticked mark empties its image key. The upload reads <see cref="ImageTarget"/> and the file
/// <c>ImageFile</c>; the save reads neither (K-1171 (5)).
/// </summary>
public sealed class SaveBrandingInput : IFormInput
{
    [Display(Name = "Marka adı")]
    public string? BrandName { get; set; }

    public Guid? LogoId { get; set; }

    public Guid? FaviconId { get; set; }

    [Display(Name = "Marka rengi")]
    public string? BrandColor { get; set; }

    public bool IsPlatformSignatureEnabled { get; set; }

    public int Version { get; set; }

    [SpecAssignedName]
    public bool RemoveLogo { get; set; }

    [SpecAssignedName]
    public bool RemoveFavicon { get; set; }

    /// <summary>UC-117: the form's image field the upload goes to - <c>LogoId</c> or <c>FaviconId</c> (07 5.2.3).</summary>
    [SpecAssignedName]
    public string? ImageTarget { get; set; }
}

/// <summary>
/// E-43, the brand screen (T534; 04 9.15; 07 UC-98, UC-163, UC-117): one form, five sections - the name, the logo, the site icon, the colour with its sample and the platform signature - and one save. The GET writes
/// nothing; the form carries the record's version. An empty part shows what the site falls back to (04 9.15.3-9.15.6); the sample's text colour is the system's contrast choice (K-742), never the page's. An image is uploaded
/// before the save and bound by the save; the removal of an image is its mark.
/// </summary>
[HandlerBodyClass("UploadImage", RequestBodyClass.ImageUpload)]
[NoStore]
internal sealed class BrandModel : PageModel
{
    public const string Title = "Marka";

    public const string AppliedNote = "Marka ayarları vitrine ve e-postalara uygulanır; siparişlere donmaz, kayıtlı siparişlerin görünümü değişmez.";

    public const string NameFallbackPrefix = "Marka adı girilene kadar sitenin alan adı gösterilir: ";

    public const string LogoFallbackText = "Logo yüklenmediyse marka adı yazıyla gösterilir.";

    public const string ColorPlaceholderHint = "#RRGGBB biçiminde, örneğin #1A56DB. Boş bırakırsanız ürünün varsayılan rengi kullanılır.";

    public const string ColorUseText = "Marka rengi vitrinde yalnız dolu zemin olarak kullanılır: birincil düğmelerde ve duyuru şeridinde. Panel bu renkle boyanmaz.";

    public const string SignatureLabel = "Sitenin altbilgisinde “Shopfolio ile kuruldu” imzası görünsün";

    public const string SignatureNote = "İmza yalnız sitenin altbilgisindedir; e-postalarda ve site simgesinde yer almaz.";

    public const string RemoveLogoLabel = "Logoyu kaldır";

    public const string RemoveFaviconLabel = "Site simgesini kaldır";

    public const string ExpiredImageText = "Görsel artık yok; yeniden yükleyin.";

    public const string NoImageText = "Yüklenecek görseli seçin.";

    public const string LogoTarget = nameof(SaveBrandingInput.LogoId);

    public const string FaviconTarget = nameof(SaveBrandingInput.FaviconId);

    public const string ImageFileName = "Input.ImageFile";

    private readonly ICorporateContentModule _content;
    private readonly ICatalogModule _catalog;
    private readonly ITransactionRunner _transactions;
    private readonly RejectionLog _rejections;
    private BrandingSection _section = new(null, null, null, null, true, 1);
    private BrandIdentity? _identity;
    private bool _ran;

    public BrandModel(ICorporateContentModule content, ICatalogModule catalog, ITransactionRunner transactions, RejectionLog rejections)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentNullException.ThrowIfNull(rejections);
        _content = content;
        _catalog = catalog;
        _transactions = transactions;
        _rejections = rejections;
    }

    /// <summary>The entries the form shows: the stored values when it opens, the sent values when it is shown again (with the new image after an upload).</summary>
    public SaveBrandingInput Form { get; private set; } = new();

    public bool IsConflicted { get; private set; }

    public MessageView? Notice { get; private set; }

    /// <summary>The field message of a refused upload, keyed by its target; null otherwise.</summary>
    public MessageView? UploadRefusal { get; private set; }

    public string? UploadRefusalTarget { get; private set; }

    /// <summary>The brand as the site shows it now: what an empty part falls back to (04 9.15.3-9.15.6).</summary>
    public BrandIdentity Identity => _identity ?? throw new InvalidOperationException("The screen is read before it is loaded.");

    /// <summary>The colour of the sample and the text on it: the typed colour when it is a colour, else the stored one; the text is the system's contrast choice (K-742).</summary>
    public FrameBrand SampleBrand { get; private set; } = new(string.Empty, null, null, string.Empty, FrameStyle.DefaultBrandColor, FrameStyle.DefaultBrandTextColor);

    /// <summary>The fields of the form in the order the screen draws them (04 9.15.3-9.15.7), each with its message after a refusal.</summary>
    public IReadOnlyList<FieldView> FormFields()
    {
        FormErrorSummary summary = FormErrorSummary.From(ModelState);
        return Components.Fields.BindAll(
        [
            Components.Fields.Text("Input.BrandName", "Marka adı", FieldRequirement.Required, Form.BrandName),
            Components.Fields.Text("Input.BrandColor", "Marka rengi", FieldRequirement.Optional, Form.BrandColor, maxLength: 7, hint: ColorPlaceholderHint),
        ],
        summary);
    }

    public FormErrorSummary Summary => FormErrorSummary.From(ModelState);

    public override async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        await LoadAsync(HttpContext.RequestAborted);
    }

    /// <summary>UC-98: the form with the record's version.</summary>
    public IActionResult OnGet()
    {
        Form = FormOf(_section);
        Draw();
        return Page();
    }

    /// <summary>UC-163: saves the four settings and the signature in one transaction; the module writes the trail rows. A conflict is the warning with "Güncel hâli aç" and "Yine de kaydet" (K-800).</summary>
    public async Task<IActionResult> OnPostSaveBrandingAsync(SaveBrandingInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _ran = true;
        BrandingInput command = new(input.BrandName, input.RemoveLogo ? null : input.LogoId, input.RemoveFavicon ? null : input.FaviconId, input.BrandColor, input.IsPlatformSignatureEnabled);
        BrandingSaveResult result = await _transactions.RunAsync(token => _content.SaveBrandingAsync(command, input.Version, token), ct);
        if (result.IsSaved)
        {
            return FormResults.SeeOther(PanelFramePaths.Brand, NoticeCode.Saved);
        }

        await LoadAsync(ct);
        Form = input;
        if (input.RemoveLogo)
        {
            input.LogoId = null;
            input.RemoveLogo = false;
        }

        if (input.RemoveFavicon)
        {
            input.FaviconId = null;
            input.RemoveFavicon = false;
        }

        if (result.IsVersionConflict)
        {
            // 07 2.4.5: the typed entries stay; the form carries the record's current version, so "Yine de kaydet" is a save like any other.
            _rejections.Rejected(ErrorCode.RecordVersionConflict);
            IsConflicted = true;
            input.Version = _section.Version;
            ModelState.Clear();
            Draw();
            return FormResults.Conflict(this);
        }

        foreach (BrandingRejection rejection in result.Rejections)
        {
            string key = "Input." + rejection.Field;
            if (rejection.Code == FieldCodes.InvalidImage)
            {
                // K-1100: a image the form binds is gone - the upload was cleaned away; the field asks for a new upload.
                ModelState.AddModelError(key, ExpiredImageText);
            }
            else
            {
                ModelState.AddFieldError(key, rejection.Code);
            }
        }

        Draw();
        return FormResults.Redisplay(this);
    }

    /// <summary>
    /// UC-117: the logo or the site icon is uploaded before the form is saved. Nothing of the record is written and its version stays: the answer is 200 with the form's sent entries and the new image's key in the target field
    /// (K-1171 (5)). 422 at the target field: no file, the content is no image, it is over the size or the pixel ceiling, or the field already holds an image.
    /// </summary>
    public async Task<IActionResult> OnPostUploadImageAsync(SaveBrandingInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _ran = true;
        Form = input;
        bool logo = string.Equals(input.ImageTarget, LogoTarget, StringComparison.Ordinal);
        bool favicon = string.Equals(input.ImageTarget, FaviconTarget, StringComparison.Ordinal);
        UploadSurface surface = logo ? UploadSurface.Logo : UploadSurface.SiteIcon;
        byte[]? content = await UploadRequests.ReadFileAsync(Request, ImageFileName, ct);
        if (content is null || !(logo || favicon))
        {
            return RefuseUpload(input.ImageTarget, ErrorCode.InvalidImage, NoImageText);
        }

        if ((logo && input.LogoId is not null && !input.RemoveLogo) || (favicon && input.FaviconId is not null && !input.RemoveFavicon))
        {
            return RefuseUpload(input.ImageTarget, ErrorCode.ImageLimitExceeded, UploadChecks.CountMessage(surface));
        }

        ImageUploadResult result = await _transactions.RunAsync(inner => _catalog.UploadImageAsync(content, inner), ct);
        if (result.IsRejected || result.ImageId is not { } imageId)
        {
            return RefuseUpload(input.ImageTarget, ErrorCode.InvalidImage, UploadRefusalText(result, surface));
        }

        if (logo)
        {
            input.LogoId = imageId;
            input.RemoveLogo = false;
        }
        else
        {
            input.FaviconId = imageId;
            input.RemoveFavicon = false;
        }

        // The form is drawn from Form, not from the posted model state: the new image's key reaches its hidden field.
        ModelState.Clear();
        Draw();
        return Page();
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
    public static string UploadFieldId(string? target) => string.Equals(target, FaviconTarget, StringComparison.Ordinal) ? "brand-favicon" : "brand-logo";

    private static SaveBrandingInput FormOf(BrandingSection section) => new()
    {
        BrandName = section.BrandName,
        LogoId = section.LogoId,
        FaviconId = section.FaviconId,
        BrandColor = section.BrandColor,
        IsPlatformSignatureEnabled = section.IsPlatformSignatureEnabled,
        Version = section.Version,
    };

    private IActionResult RefuseUpload(string? target, ErrorCode code, string text)
    {
        _rejections.Rejected(code);
        UploadRefusalTarget = target;
        UploadRefusal = Messages.ForField(UploadFieldId(target), text);
        Draw();
        return FormResults.Redisplay(this);
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        _section = await _content.GetBrandingAsync(ct);
        _identity = await _content.GetBrandIdentityAsync(ct);
        NoticeCode? notice = NoticeParameter.Read(Request, NoticeKind.ShortLived);
        Notice = notice is { } code && NoticeTexts.TryShortLived(code, out string? text) ? Messages.ShortLivedSuccess(text) : null;
        if (!_ran)
        {
            Form = FormOf(_section);
        }
    }

    // The sample uses the typed colour when it is a colour, else the stored one; the system chooses the black or white text on it (02 3.29.5; K-742).
    private void Draw()
    {
        BrandIdentity identity = Identity;
        string? typed = Form.BrandColor?.Trim();
        bool isColor = typed is { Length: 7 } && typed[0] == '#' && typed[1..].All(char.IsAsciiHexDigit);
        string color = typed is not null && isColor ? typed.ToUpperInvariant() : identity.Color;
        (string background, string text) = isColor ? EmailColors.Button(color) : (identity.Color, identity.TextColor);
        string name = string.IsNullOrWhiteSpace(Form.BrandName) ? identity.DisplayName : Form.BrandName.Trim();
        SampleBrand = new FrameBrand(name, Form.LogoId, Form.FaviconId, BrandingInitial(name), background, text);
    }

    private static string BrandingInitial(string name)
    {
        string trimmed = name.Trim();
        return trimmed.Length == 0 ? string.Empty : System.Globalization.StringInfo.GetNextTextElement(trimmed).ToUpper(RequestFormat.Culture);
    }
}
