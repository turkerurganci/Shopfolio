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
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

using FieldRejection = Shopfolio.Modules.Settings.Contracts.FieldRejection;

namespace Shopfolio.Pages.Panel;

/// <summary>The form input of the identity form (E-44; 07 9.16.2 <c>SaveCompanyInput</c>; 04 7.2.19) and the version of the company record it was opened with (06 4.1).</summary>
public sealed class SaveCompanyInput : IFormInput
{
    public string? CompanyType { get; set; }

    public string? LegalName { get; set; }

    public string? TaxIdNumber { get; set; }

    public string? MersisNumber { get; set; }

    public string? TradeRegistryNumber { get; set; }

    public string? ProvinceId { get; set; }

    public string? District { get; set; }

    public string? AddressLine { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? KepAddress { get; set; }

    public string? ChamberName { get; set; }

    public string? BusinessName { get; set; }

    public string? ProfessionalConductRules { get; set; }

    public string? EtbisVerificationUrl { get; set; }

    public int Version { get; set; }
}

/// <summary>
/// The input of the temporary closure switch's closing form (E-44; 07 9.16.3): the value the switch had on the screen (<see cref="Original"/>; 07 2.4.3) and the confirmation
/// the window sends. The switch carries no version.
/// </summary>
public sealed class SuspendSalesInput : IFormInput
{
    /// <summary>The switch as the screen showed it: true when sales were closed temporarily.</summary>
    [SpecAssignedName]
    public bool Original { get; set; }

    /// <summary>True when the confirmation window's button sent the form (the first send has none).</summary>
    [SpecAssignedName]
    public bool Confirmed { get; set; }
}

/// <summary>The input of the temporary closure switch's reopening form (E-44; 07 9.16.4): the value the switch had on the screen (07 2.4.3). The switch carries no version and needs no confirmation.</summary>
public sealed class ResumeSalesInput : IFormInput
{
    /// <summary>The switch as the screen showed it: true when sales were closed temporarily.</summary>
    [SpecAssignedName]
    public bool Original { get; set; }
}

/// <summary>One line of the "Satış durumu" section: a condition by its name (the same names as the setup checklist; K-815), whether it is met and, when not, the screen that meets it (04 3.4.15).</summary>
public sealed record GateConditionRowView(string Name, bool IsMet, string? MissingText, string? Href);

/// <summary>
/// E-44, the firm identity and sales screen (T535; 04 9.16; 07 UC-99, UC-164...UC-166): the "Satış durumu" section with the gate's conditions and the temporary closure switch (its own forms and
/// confirmation) and the identity form. The GET writes nothing. The identity save is one transaction with the two generated texts' new versions (K-1088); the switch is written against the value it was
/// shown with and an answer that finds it changed or already as asked is a 303, never a 409 (K-1194).
/// </summary>
[NoStore]
public sealed class FirmIdentityModel : PageModel
{
    public const string SalesOpenText = "Satış açık";

    public const string SalesClosedText = "Satış kapalı";

    public const string TemporarilyClosedText = "Satış geçici olarak kapatıldı.";

    public const string MetText = "Tamam";

    public const string CloseLabel = "Satışı geçici olarak kapat";

    public const string ReopenLabel = "Satışı yeniden aç";

    public const string ConfirmLabel = "Satışı kapat";

    public const string ConfirmSentence = "Satış kapanacak: yeni sipariş alınmayacak; vitrin ve kurumsal içerik yayında kalacak. Açık siparişler etkilenmez; onları yürütmeye devam edersiniz.";

    public const string ReopenNote = "Anahtar tek başına satışı açmaz: kapının öteki koşulları da karşılanmalıdır.";

    public const string DataEntriesClosedText = "Aydınlatma metni ve çerez politikası yayına alınana kadar iletişim formu, hesap kaydı ve yeni hesap açacak Google ile giriş de kapalıdır.";

    public const string IdentityNote = "Kimlik bilgileri her siparişe o günkü hâliyle yazılır; değişiklik Ön Bilgilendirme Formu'nun ve Mesafeli Satış Sözleşmesi'nin yeni sürümünü üretir.";

    public const string TypeNote = "Firma tipini değiştirirseniz yeni tipin zorunlu alanları doldurulana kadar satış kapalı kalır.";

    public const string EmailHint = "Firmaya giden bildirimler bu adrese gönderilir.";

    public const string EtbisReminder = "ETBİS'e kayıt, satış yapan her firmanın yükümlülüğüdür; ürün kaydınızı denetlemez. Kaydı yaptırdıktan sonra ETBİS'in verdiği doğrulama bağlantısını girin. Alan boşken sitede doğrulama bandı görünmez.";

    public const string IdentityConditionName = "Firma kimliğinin zorunlu alanları";

    public const string PaymentConditionName = "En az bir açık ödeme yöntemi";

    public const string LegalTextsConditionName = "Aydınlatma metni ve çerez politikası yayında";

    public const string ReturnAddressConditionName = "İade adresi";

    public const string ClosureConditionName = "Satış geçici olarak kapatılmadı";

    /// <summary>The element id of the identity form's section; the identity condition links to it.</summary>
    public const string IdentitySectionId = "firma-kimligi";

    private static readonly string[] FormOrder =
    [
        "Input.CompanyType", "Input.LegalName", "Input.TaxIdNumber", "Input.MersisNumber", "Input.TradeRegistryNumber", "Input.ProvinceId", "Input.District", "Input.AddressLine", "Input.Phone",
        "Input.Email", "Input.KepAddress", "Input.ChamberName", "Input.ProfessionalConductRules", "Input.BusinessName", "Input.EtbisVerificationUrl",
    ];

    private readonly ISettingsModule _settings;
    private readonly IPanelHome _home;
    private SaveCompanyInput? _posted;
    private bool _conflicted;

    public FirmIdentityModel(ISettingsModule settings, IPanelHome home)
    {
        _settings = settings;
        _home = home;
    }

    public bool IsOpen { get; private set; }

    public bool IsTemporarilyClosed { get; private set; }

    public bool ShowsDataEntriesClosed { get; private set; }

    public IReadOnlyList<GateConditionRowView> Conditions { get; private set; } = [];

    public IReadOnlyList<FieldView> IdentityFields { get; private set; } = [];

    public FormErrorSummary Summary { get; private set; } = null!; // non-null: LoadAsync sets it before any handler returns Page()

    public int FormVersion { get; private set; }

    public bool IsConflicted => _conflicted;

    /// <summary>True when the firm notices warning is open: the e-mail field carries the lasting message (K-799).</summary>
    public string? FirmNoticesWarning { get; private set; }

    /// <summary>The confirmation window of the closing, the server's answer to the first send (07 2.2.4); null otherwise.</summary>
    public ConfirmView? Confirm { get; private set; }

    public MessageView? Notice { get; private set; }

    public override async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        await LoadAsync(HttpContext.RequestAborted);
    }

    public IActionResult OnGet() => Page();

    /// <summary>UC-164: saves the identity; the two generated texts' new versions come in the same transaction under the settings row lock (K-1088). 422 by field; 409 on the record's version.</summary>
    public async Task<IActionResult> OnPostSaveCompanyAsync(SaveCompanyInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        CompanyIdentityInput values = new(
            Enum.TryParse(input.CompanyType, ignoreCase: false, out CompanyType type) && Enum.IsDefined(type) ? type : null,
            input.LegalName,
            input.TaxIdNumber,
            input.MersisNumber,
            input.TradeRegistryNumber,
            Guid.TryParse(input.ProvinceId, out Guid province) ? province : null,
            input.District,
            input.AddressLine,
            input.Phone,
            input.Email,
            input.KepAddress,
            input.ChamberName,
            input.BusinessName,
            input.ProfessionalConductRules,
            input.EtbisVerificationUrl);
        ITransactionRunner runner = HttpContext.RequestServices.GetRequiredService<ITransactionRunner>();
        SettingsSaveResult result = await runner.RunAsync(token => _settings.SaveCompanyIdentityAsync(values, input.Version, token), ct);
        if (result.IsSaved)
        {
            return FormResults.SeeOther(PanelFramePaths.FirmIdentityAndSales, NoticeCode.Saved);
        }

        _posted = input;
        _conflicted = result.IsVersionConflict;
        if (!result.IsVersionConflict)
        {
            foreach (FieldRejection rejection in result.Rejections)
            {
                ModelState.AddFieldError("Input." + rejection.Field, rejection.Code);
            }
        }

        await LoadAsync(ct);
        return _conflicted ? FormResults.Conflict(this) : FormResults.Redisplay(this);
    }

    /// <summary>UC-165: the first send answers 200 with the confirmation; the confirmed send writes the switch against the shown value and answers 303 - also when the switch was changed meanwhile or is already closed (K-1194).</summary>
    public async Task<IActionResult> OnPostSuspendSalesAsync(SuspendSalesInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!input.Confirmed)
        {
            Confirm = Confirmations.ForSettingChange(
                CloseLabel,
                ConfirmSentence,
                ConfirmLabel,
                PanelFramePaths.FirmIdentityAndSales,
                "SuspendSales",
                PanelFramePaths.FirmIdentityAndSales,
                [new("Input.Original", input.Original ? "true" : "false"), new("Input.Confirmed", "true")]);
            return Page();
        }

        ITransactionRunner runner = HttpContext.RequestServices.GetRequiredService<ITransactionRunner>();
        await runner.RunAsync(token => _settings.CloseSalesTemporarilyAsync(input.Original, token), ct);
        return FormResults.SeeOther(PanelFramePaths.FirmIdentityAndSales);
    }

    /// <summary>UC-166: opens the switch again against the shown value; the gate's other conditions may keep sales closed, which the section tells by name. 303 in every case (K-1194).</summary>
    public async Task<IActionResult> OnPostResumeSalesAsync(ResumeSalesInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        ITransactionRunner runner = HttpContext.RequestServices.GetRequiredService<ITransactionRunner>();
        await runner.RunAsync(token => _settings.ReopenSalesAsync(input.Original, token), ct);
        return FormResults.SeeOther(PanelFramePaths.FirmIdentityAndSales);
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        SalesGate gate = await _settings.GetSalesGateAsync(ct);
        CompanyIdentity identity = await _settings.GetCompanyIdentityAsync(ct);
        IReadOnlyList<ProvinceInfo> provinces = await _settings.GetProvincesAsync(ct);
        PanelHome home = await _home.GetAsync(ct);
        IsOpen = gate.IsOpen;
        IsTemporarilyClosed = gate.IsTemporarilyClosed;
        ShowsDataEntriesClosed = !gate.IsDataCollectionOpen;
        Conditions = ConditionsOf(gate);
        Summary = FormErrorSummary.From(ModelState, FormOrder);
        SaveCompanyInput? sent = _posted;
        FormVersion = sent is null || _conflicted ? identity.Version : sent.Version;
        IdentityFields = [.. FieldsOf(identity, provinces, sent)];
        FirmNoticesWarning = home.FirmNoticesWarningOpen
            ? $"Firma bildirimleri ulaşmıyor — {(string.IsNullOrWhiteSpace(identity.Email) ? "firma" : identity.Email)} adresine giden e-postalar teslim edilemedi."
            : null;
        NoticeCode? notice = NoticeParameter.Read(Request, NoticeKind.ShortLived);
        Notice = notice is { } code && NoticeTexts.TryShortLived(code, out string? noticeText) ? Messages.ShortLivedSuccess(noticeText) : null;
    }

    private static List<GateConditionRowView> ConditionsOf(SalesGate gate)
    {
        bool Unmet(SalesGateCondition condition) => gate.UnmetConditions.Contains(condition);
        string IdentityMissing() => gate.MissingIdentityFields.Count == 0
            ? "Eksik"
            : "Eksik: " + string.Join(", ", gate.MissingIdentityFields.Select(LegalTextsModel.LabelOf));
        string LegalMissing()
        {
            List<string> parts = [];
            if (Unmet(SalesGateCondition.PrivacyNotice))
            {
                parts.Add("aydınlatma metni tamamlanıp yayına alınmadı");
            }

            if (Unmet(SalesGateCondition.CookiePolicy))
            {
                parts.Add("çerez politikası yayına alınmadı");
            }

            return "Eksik: " + string.Join("; ", parts);
        }

        return
        [
            new(IdentityConditionName, !Unmet(SalesGateCondition.CompanyIdentity), IdentityMissing(), "#" + IdentitySectionId),
            new(PaymentConditionName, !Unmet(SalesGateCondition.PaymentMethod), "Eksik: açık ödeme yöntemi yok", PanelFramePaths.PaymentMethods),
            new(LegalTextsConditionName, !Unmet(SalesGateCondition.PrivacyNotice) && !Unmet(SalesGateCondition.CookiePolicy), LegalMissing(), PanelFramePaths.LegalTexts),
            new(ReturnAddressConditionName, !Unmet(SalesGateCondition.ReturnAddress), "Eksik: iade adresi girilmedi", PanelFramePaths.ShippingAndDefaults),
            new(ClosureConditionName, !Unmet(SalesGateCondition.TemporaryClosure), "Satış geçici olarak kapatıldı", null),
        ];
    }

    private IEnumerable<FieldView> FieldsOf(CompanyIdentity identity, IReadOnlyList<ProvinceInfo> provinces, SaveCompanyInput? sent)
    {
        string? Pick(string? sentValue, string? stored) => sent is not null ? sentValue : stored;
        string? typeValue = sent is not null ? sent.CompanyType : identity.CompanyType?.ToString();
        CompanyType? type = Enum.TryParse(typeValue, ignoreCase: false, out CompanyType parsed) && Enum.IsDefined(parsed) ? parsed : null;
        IReadOnlyList<string> typeFields = CompanyIdentityFieldSets.TypeFields(type);
        FieldRequirement RequirementOf(string field) => typeFields.Contains(field, StringComparer.Ordinal) ? FieldRequirement.Required : FieldRequirement.Optional;
        FieldView TypeField(string field, string label, string? value)
        {
            FieldView view = Fields.Text("Input." + field, label, type is null ? FieldRequirement.Optional : RequirementOf(field), value);
            return type is null ? Fields.AsReadOnly(view) : view;
        }

        List<FieldOption> typeOptions =
        [
            new(nameof(CompanyType.Tradesman), "Esnaf"),
            new(nameof(CompanyType.NaturalPersonMerchant), "Gerçek kişi tacir"),
            new(nameof(CompanyType.LegalEntity), "Tüzel kişi"),
        ];
        string? provinceValue = Pick(sent?.ProvinceId, identity.ProvinceId?.ToString());
        List<FieldView> fields =
        [
            Fields.ClosedList("Input.CompanyType", "Firma tipi", FieldRequirement.Required, typeOptions, typeValue, TypeNote),
            TypeField(nameof(CompanyIdentityInput.LegalName), "Unvan veya ad soyad", Pick(sent?.LegalName, identity.LegalName)),
            TypeField(nameof(CompanyIdentityInput.TaxIdNumber), "Vergi kimlik numarası", Pick(sent?.TaxIdNumber, identity.TaxIdNumber)),
            TypeField(nameof(CompanyIdentityInput.MersisNumber), "MERSİS numarası", Pick(sent?.MersisNumber, identity.MersisNumber)),
            TypeField(nameof(CompanyIdentityInput.TradeRegistryNumber), "Ticaret sicil numarası", Pick(sent?.TradeRegistryNumber, identity.TradeRegistryNumber)),
            Fields.ClosedList("Input.ProvinceId", "İl", FieldRequirement.Required, [.. provinces.Select(p => new FieldOption(p.Id.ToString(), p.Name))], provinceValue),
            Fields.Text("Input.District", "İlçe", FieldRequirement.Required, Pick(sent?.District, identity.District)),
            Fields.Text("Input.AddressLine", "Açık adres", FieldRequirement.Required, Pick(sent?.AddressLine, identity.AddressLine)),
            Fields.Telephone("Input.Phone", "Telefon", FieldRequirement.Required, Pick(sent?.Phone, identity.Phone)),
            Fields.Email("Input.Email", "E-posta", FieldRequirement.Required, Pick(sent?.Email, identity.Email)),
            Fields.Text("Input.KepAddress", "KEP adresi", FieldRequirement.Required, Pick(sent?.KepAddress, identity.KepAddress)),
            Fields.Text("Input.ChamberName", "Meslek odası", FieldRequirement.Required, Pick(sent?.ChamberName, identity.ChamberName)),
            Fields.TextArea("Input.ProfessionalConductRules", "Meslekle ilgili davranış kuralları (bağlantı ya da kısa metin)", FieldRequirement.Required, Pick(sent?.ProfessionalConductRules, identity.ProfessionalConductRules)),
            Fields.Text("Input.BusinessName", "İşletme adı veya tescilli marka", FieldRequirement.Optional, Pick(sent?.BusinessName, identity.BusinessName)),
            Fields.Text("Input.EtbisVerificationUrl", "ETBİS doğrulama bilgisi", FieldRequirement.Optional, Pick(sent?.EtbisVerificationUrl, identity.EtbisVerificationUrl), hint: EtbisReminder),
        ];
        return fields.Select(f => Fields.Bind(f, Summary));
    }


}
