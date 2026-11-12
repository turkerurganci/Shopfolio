using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

using FieldRejection = Shopfolio.Modules.Settings.Contracts.FieldRejection;
using TextKind = Shopfolio.Modules.Settings.Contracts.LegalTextType;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The form input of the legal texts screen (E-47; 07 5.1.5): the model of the draft save, which the publication binds too (the publication is sent with the edited text; 04 9.19.3, K-1194 (3)). The text
/// type is one of the two firm-edited texts; the closed list of the rule is the enum's names, so a generated text reaches the handler and is refused there with the same code (07 9.19.2).
/// </summary>
public sealed class SaveLegalTextDraftInput : IFormInput
{
    /// <summary>The type of the text the section edits: <c>PrivacyNotice</c> or <c>CookiePolicy</c> (06 5).</summary>
    [Display(Name = "Metin türü")]
    [FieldRequired]
    [InvalidListValue(typeof(TextKind))]
    public string LegalTextType { get; set; } = string.Empty;

    /// <summary>The text of the draft, in the closed format set; the write step cleans it (09 15.3).</summary>
    [Display(Name = "Metin")]
    public string Text { get; set; } = string.Empty;

    /// <summary>The hosting and e-mail infrastructure location; the privacy notice's section only.</summary>
    [Display(Name = "Barındırma ve e-posta altyapısının konumu")]
    public string? HostingLocation { get; set; }

    /// <summary>The version of the draft row the form was opened with (06 4.56; 07 2.4.1).</summary>
    public int Version { get; set; }
}

/// <summary>One line of the screen's list: a text, and the version in force or the fact that it was never published (04 9.19.2).</summary>
public sealed record LegalTextRowView(string Title, string State);

/// <summary>One thing the completion line says is missing; <paramref name="Href"/> is the screen that fills it, when there is one (04 9.19.3).</summary>
public sealed record CompletionItemView(string Text, string? Href);

/// <summary>The section of one firm-edited text (04 9.19.3): its version, completion line, form values and the way the last request left it.</summary>
public sealed class EditedLegalTextView
{
    public required LegalTextType Type { get; init; }

    /// <summary>The prefix of the element ids of the section; two sections never share an id.</summary>
    public required string IdPrefix { get; init; }

    public required string Title { get; init; }

    /// <summary>The version in force and its date, or the fact that the text was never published.</summary>
    public required string PublishedLine { get; init; }

    /// <summary>True when the draft differs from the version in force (K-818).</summary>
    public required bool DiffersFromPublished { get; init; }

    public required IReadOnlyList<CompletionItemView> Completion { get; init; }

    public required bool IsComplete { get; init; }

    /// <summary>What the closed gates mean for this text (04 9.19.3, K-850); null when nothing is closed because of it.</summary>
    public required string? GateNote { get; init; }

    public required string Text { get; init; }

    public required string? HostingLocation { get; init; }

    public required int FormVersion { get; init; }

    /// <summary>The draft with the identity placeholders filled; shown on the privacy notice's section only (04 9.19.11).</summary>
    public required string? FilledPreview { get; init; }

    /// <summary>True when the last request was this section's save or publication; its field messages are drawn here and only here.</summary>
    public bool IsPosted { get; init; }

    /// <summary>True when the last request was refused because the draft's version changed; the conflict controls are drawn in this section only (04 9.19.8).</summary>
    public bool IsConflicted { get; init; }

    /// <summary>True when the last publication was refused because of the identity fields that wait (the completion line says which).</summary>
    public bool IsPublicationIncomplete { get; init; }

    public bool HasHostingLocation => Type == LegalTextType.PrivacyNotice;
}

/// <summary>The section of one generated text (04 9.19.4): read only.</summary>
public sealed record GeneratedLegalTextView(string Title, string VersionLine, string? Text);

/// <summary>
/// E-47, the legal texts screen (T507; 04 9.19; 07 UC-102, UC-173, UC-174): the four texts' lines, the two firm-edited texts' sections (the draft's save and publication, each in its own form) and
/// the two generated texts' current state. The GET writes nothing; each POST is a named handler (07 2.2). The version a form carries is the draft row's (06 4.56).
/// </summary>
[RequestBodyClass(RequestBodyClass.LongText)]
public sealed class LegalTextsModel : PageModel
{
    /// <summary>The texts' titles (04 9.19.3, 9.19.4), written once.</summary>
    public const string PrivacyNoticeTitle = "Aydınlatma metni";

    public const string CookiePolicyTitle = "Çerez politikası";

    public const string PreInformationFormTitle = "Ön Bilgilendirme Formu";

    public const string DistanceSalesContractTitle = "Mesafeli Satış Sözleşmesi";

    /// <summary>The line of a text that has no published version.</summary>
    public const string NotPublishedText = "Henüz yayına alınmadı";

    public const string DiffersText = "Düzenlenen metin yayındaki sürümden farklı.";

    public const string CompleteText = "Tamamlandı.";

    public const string HostingLocationMissingText = "Barındırma ve e-posta altyapısının konumu doldurulmadı.";

    public const string IdentityMissingPrefix = "Firma kimliğinde eksik: ";

    public const string PublicationIncompleteText = "Aydınlatma metni tamamlanmadığı için yayına alınamadı; eksikler yukarıdaki tamamlanma satırında.";

    /// <summary>04 9.19.3, 9.19.10 (K-850): the gates that stay closed while the two texts wait.</summary>
    public const string SalesAndEntriesClosedText = "Aydınlatma metni ve çerez politikası yayına alınana kadar satış ve veri toplayan girişler (iletişim formu, hesap kaydı, yeni hesap açacak Google ile giriş) kapalıdır.";

    public const string PrivacyNoticeGateNote = "Bu metin tamamlanıp yayına alınana ve çerez politikası yayına alınana kadar iletişim formu, hesap kaydı ve yeni hesap açacak Google ile giriş kapalıdır.";

    public const string CookiePolicyGateNote = "İlk yayınına kadar iletişim formu, hesap kaydı ve yeni hesap açacak Google ile giriş kapalıdır ve vitrin çerez yazmaz.";

    /// <summary>04 9.19.4: the settings that feed the two generated texts.</summary>
    public static IReadOnlyList<string> FeedingSettings { get; } = ["Firma kimliği", "Kargo ücreti", "Kargoya verme süresi", "Teslimat illeri", "İade adresi"];

    /// <summary>The Turkish names of the identity fields the completion line lists (the names are those of the identity input; 04 7.2.19).</summary>
    private static readonly IReadOnlyDictionary<string, string> IdentityFieldLabels = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["CompanyType"] = "Firma tipi",
        ["LegalName"] = "Unvan veya ad soyad",
        ["TaxIdNumber"] = "Vergi kimlik numarası",
        ["MersisNumber"] = "MERSİS numarası",
        ["TradeRegistryNumber"] = "Ticaret sicil numarası",
        ["ProvinceId"] = "İl",
        ["District"] = "İlçe",
        ["AddressLine"] = "Açık adres",
        ["Phone"] = "Telefon",
        ["Email"] = "E-posta",
        ["KepAddress"] = "KEP adresi",
        ["ChamberName"] = "Meslek odası",
        ["ProfessionalConductRules"] = "Meslekle ilgili davranış kuralları",
    };

    private readonly ISettingsModule _settings;
    private Posted? _posted;

    public LegalTextsModel(ISettingsModule settings)
    {
        _settings = settings;
    }

    public IReadOnlyList<LegalTextRowView> Rows { get; private set; } = [];

    public IReadOnlyList<EditedLegalTextView> Edited { get; private set; } = [];

    public IReadOnlyList<GeneratedLegalTextView> Generated { get; private set; } = [];

    /// <summary>The note of the closed sales and entries gates; null when neither of the two texts holds a gate closed.</summary>
    public string? GateNote { get; private set; }

    /// <summary>The short-lived notice the redirect after a draft save carries (07 6.3.1); null when the request has none.</summary>
    public MessageView? Notice { get; private set; }

    /// <summary>The identity labels in the completion line must name every field the identity can lack (unit test).</summary>
    public static IReadOnlyCollection<string> LabelledIdentityFields => [.. IdentityFieldLabels.Keys];

    // The screen's data is read before the handler is chosen to run: a form that fails to bind (a text type outside the list) is shown again by the validation filter without the handler (09 3.4 item 3).
    public override async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        await LoadAsync(HttpContext.RequestAborted);
    }

#pragma warning disable CA1822 // A page handler is an instance method the framework finds by its name; a static method is not a handler.
    public IActionResult OnGet() => Page();
#pragma warning restore CA1822

    /// <summary>UC-173: saves the draft of one firm-edited text. The write is one transaction (09 9.1); the section is saved with the version its form carried (06 4.56).</summary>
    public async Task<IActionResult> OnPostSaveLegalTextDraftAsync(SaveLegalTextDraftInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!TryEditedType(input, out LegalTextType type))
        {
            return await RefuseTypeAsync(ct);
        }

        string text = input.Text ?? string.Empty;
        string? hosting = HostingOf(type, input);
        ITransactionRunner runner = HttpContext.RequestServices.GetRequiredService<ITransactionRunner>();
        SettingsSaveResult result = await runner.RunAsync(token => _settings.SaveLegalTextDraftAsync(type, text, hosting, input.Version, token), ct);
        if (result.IsSaved)
        {
            return FormResults.SeeOther(PanelFramePaths.LegalTexts, NoticeCode.Saved);
        }

        _posted = new Posted(type, text, hosting, input.Version, result.IsVersionConflict, false);
        if (!result.IsVersionConflict)
        {
            AddRejections(result.Rejections);
        }

        await LoadAsync(ct);
        return result.IsVersionConflict ? FormResults.Conflict(this) : FormResults.Redisplay(this);
    }

    /// <summary>
    /// UC-174: publishes the edited text as a new version. One transaction saves the edited text as the draft and opens the published row; a refusal rolls back both, so an incomplete notice
    /// writes no draft (07 9.19.3; K-1194 (3)). A second publication that comes with the old draft version is refused at the save step (409).
    /// </summary>
    public async Task<IActionResult> OnPostPublishLegalTextAsync(SaveLegalTextDraftInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!TryEditedType(input, out LegalTextType type))
        {
            return await RefuseTypeAsync(ct);
        }

        string text = input.Text ?? string.Empty;
        string? hosting = HostingOf(type, input);
        ITransactionRunner runner = HttpContext.RequestServices.GetRequiredService<ITransactionRunner>();
        PublicationOutcome outcome = await runner.RunAsync(
            async token =>
            {
                SettingsSaveResult saved = await _settings.SaveLegalTextDraftAsync(type, text, hosting, input.Version, token);
                if (!saved.IsSaved)
                {
                    return PublicationOutcome.Refused(saved.IsVersionConflict, saved.Rejections);
                }

                LegalTextDraft draft = await _settings.GetLegalTextDraftAsync(type, token);
                LegalTextPublishResult published = await _settings.PublishLegalTextAsync(type, draft.FormVersion, token);
                return published.IsPublished ? PublicationOutcome.Done : PublicationOutcome.Refused(published.IsVersionConflict, published.Rejections);
            },
            ct);
        if (!outcome.IsRejected)
        {
            return FormResults.SeeOther(PanelFramePaths.LegalTexts);
        }

        bool identityWaits = outcome.Rejections.Any(r => r.Field is not (nameof(SaveLegalTextDraftInput.Text) or nameof(SaveLegalTextDraftInput.HostingLocation)));
        _posted = new Posted(type, text, hosting, input.Version, outcome.IsVersionConflict, identityWaits);
        if (!outcome.IsVersionConflict)
        {
            AddRejections(outcome.Rejections);
        }

        await LoadAsync(ct);
        return outcome.IsVersionConflict ? FormResults.Conflict(this) : FormResults.Redisplay(this);
    }

    private static string? HostingOf(LegalTextType type, SaveLegalTextDraftInput input) => type == LegalTextType.PrivacyNotice ? input.HostingLocation : null;

    // A type outside the two edited ones: the closed list's own names are already checked by the rule; the generated texts are refused here with the same code (07 9.19.2).
    private static bool TryEditedType(SaveLegalTextDraftInput input, out LegalTextType type) =>
        Enum.TryParse(input.LegalTextType, ignoreCase: false, out type) && type is LegalTextType.PrivacyNotice or LegalTextType.CookiePolicy;

    private async Task<IActionResult> RefuseTypeAsync(CancellationToken ct)
    {
        ModelState.AddModelError("input.LegalTextType", FieldError.Encode(FieldCodes.InvalidListValue));
        await LoadAsync(ct);
        return FormResults.Redisplay(this);
    }

    private void AddRejections(IReadOnlyList<FieldRejection> rejections)
    {
        foreach (FieldRejection rejection in rejections)
        {
            if (rejection.Field is nameof(SaveLegalTextDraftInput.Text) or nameof(SaveLegalTextDraftInput.HostingLocation))
            {
                ModelState.AddModelError("input." + rejection.Field, FieldError.Encode(rejection.Code));
            }
        }
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        IReadOnlyList<LegalTextSummary> summaries = await _settings.GetLegalTextsAsync(ct);
        CompanyIdentityCompleteness identity = await _settings.GetCompanyIdentityCompletenessAsync(ct);
        SalesGate gate = await _settings.GetSalesGateAsync(ct);
        Dictionary<LegalTextType, LegalTextVersionInfo?> latest = [];
        foreach (LegalTextType type in Enum.GetValues<LegalTextType>())
        {
            latest[type] = await _settings.GetLatestLegalTextVersionAsync(type, ct);
        }

        LegalTextType[] order = [LegalTextType.PrivacyNotice, LegalTextType.CookiePolicy, LegalTextType.PreInformationForm, LegalTextType.DistanceSalesContract];
        Rows = [.. order.Select(type => new LegalTextRowView(TitleOf(type), PublishedLineOf(latest[type])))];
        Generated =
        [
            .. new[] { LegalTextType.PreInformationForm, LegalTextType.DistanceSalesContract }
                .Select(type => new GeneratedLegalTextView(TitleOf(type), PublishedLineOf(latest[type]), latest[type]?.Text)),
        ];

        List<EditedLegalTextView> edited = [];
        foreach (LegalTextType type in new[] { LegalTextType.PrivacyNotice, LegalTextType.CookiePolicy })
        {
            LegalTextSummary summary = summaries.Single(s => s.Type == type);
            LegalTextDraft draft = await _settings.GetLegalTextDraftAsync(type, ct);
            LegalTextDraftPreview preview = await _settings.GetLegalTextDraftPreviewAsync(type, ct);
            bool posted = _posted is { } p && p.Type == type;
            Posted? sent = posted ? _posted : null;
            bool conflicted = sent?.IsConflict == true;
            edited.Add(new EditedLegalTextView
            {
                Type = type,
                IdPrefix = type == LegalTextType.PrivacyNotice ? "privacy-notice" : "cookie-policy",
                Title = TitleOf(type),
                PublishedLine = PublishedLineOf(latest[type]),
                DiffersFromPublished = preview.DiffersFromPublished,
                Completion = CompletionOf(summary, identity),
                IsComplete = summary.IsComplete,
                GateNote = GateNoteOf(type, gate, latest[type]),
                Text = sent?.Text ?? draft.Text,
                HostingLocation = type == LegalTextType.PrivacyNotice ? sent?.HostingLocation ?? draft.HostingLocation : null,

                // A refused save keeps the version its form carried, so the next send is compared with it; after a conflict the form carries the draft's current version (07 2.4.5).
                FormVersion = sent is not null && !conflicted ? sent.Version : draft.FormVersion,
                FilledPreview = type == LegalTextType.PrivacyNotice ? preview.FilledText : null,
                IsPosted = posted,
                IsConflicted = conflicted,
                IsPublicationIncomplete = sent?.IdentityWaits == true,
            });
        }

        Edited = edited;
        bool textsWait = gate.UnmetConditions.Contains(SalesGateCondition.PrivacyNotice)
            || gate.UnmetConditions.Contains(SalesGateCondition.CookiePolicy);
        GateNote = textsWait ? SalesAndEntriesClosedText : null;
        NoticeCode? notice = NoticeParameter.Read(Request, NoticeKind.ShortLived);
        Notice = notice is { } code && NoticeTexts.TryShortLived(code, out string? noticeText) ? Messages.ShortLivedSuccess(noticeText) : null;
    }

    private static string TitleOf(LegalTextType type) => type switch
    {
        LegalTextType.PrivacyNotice => PrivacyNoticeTitle,
        LegalTextType.CookiePolicy => CookiePolicyTitle,
        LegalTextType.PreInformationForm => PreInformationFormTitle,
        LegalTextType.DistanceSalesContract => DistanceSalesContractTitle,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "A legal text has one of four types (06 5)."),
    };

    private static string PublishedLineOf(LegalTextVersionInfo? version) =>
        version is null ? NotPublishedText : $"Yayındaki sürüm: {version.VersionNumber} · {DisplayFormat.Date(version.PublishedAt)}";

    private static string? GateNoteOf(LegalTextType type, SalesGate gate, LegalTextVersionInfo? latest) => type switch
    {
        LegalTextType.PrivacyNotice => gate.IsDataCollectionOpen ? null : PrivacyNoticeGateNote,
        LegalTextType.CookiePolicy => latest is null ? CookiePolicyGateNote : null,
        _ => null,
    };

    private static List<CompletionItemView> CompletionOf(LegalTextSummary summary, CompanyIdentityCompleteness identity)
    {
        List<CompletionItemView> items = [];
        foreach (LegalTextMissingPart part in summary.Missing)
        {
            if (part == LegalTextMissingPart.NotPublished)
            {
                items.Add(new CompletionItemView(NotPublishedText + ".", null));
            }
            else if (part == LegalTextMissingPart.HostingLocation)
            {
                items.Add(new CompletionItemView(HostingLocationMissingText, null));
            }
            else if (part == LegalTextMissingPart.CompanyIdentity)
            {
                items.AddRange(identity.MissingFields.Select(field => new CompletionItemView(IdentityMissingPrefix + LabelOf(field), PanelFramePaths.FirmIdentityAndSales)));
            }
        }

        return items;
    }

    internal static string LabelOf(string identityField) => IdentityFieldLabels.TryGetValue(identityField, out string? label) ? label : identityField;

    /// <summary>What the last POST sent, so the form can show it again (07 2.2.2).</summary>
    private sealed record Posted(LegalTextType Type, string Text, string? HostingLocation, int Version, bool IsConflict, bool IdentityWaits);

    /// <summary>The result of the publication's one transaction; a refusal rolls the draft save back as well.</summary>
    private sealed class PublicationOutcome : IOperationResult
    {
        private PublicationOutcome(bool isRejected, bool isVersionConflict, IReadOnlyList<FieldRejection> rejections)
        {
            IsRejected = isRejected;
            IsVersionConflict = isVersionConflict;
            Rejections = rejections;
        }

        public static PublicationOutcome Done { get; } = new(false, false, []);

        public bool IsRejected { get; }

        public bool IsVersionConflict { get; }

        public IReadOnlyList<FieldRejection> Rejections { get; }

        public static PublicationOutcome Refused(bool isVersionConflict, IReadOnlyList<FieldRejection> rejections) => new(true, isVersionConflict, rejections);
    }
}
