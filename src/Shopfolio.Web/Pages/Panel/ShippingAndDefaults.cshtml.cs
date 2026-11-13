using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

using FieldRejection = Shopfolio.Modules.Settings.Contracts.FieldRejection;

namespace Shopfolio.Pages.Panel;

/// <summary>The shipping fee section's input (07 9.18.2 <c>SaveShippingFeeInput</c>; 04 7.2.21.1, 7.2.21.2): the fee (required), the two optional amounts and the section's version.</summary>
public sealed class SaveShippingFeeInput : IFormInput
{
    [Display(Name = "Kargo ücreti")]
    [FieldRequired]
    [InvalidAmount(AllowZero = true)]
    public decimal? ShippingFee { get; set; }

    [Display(Name = "Ücretsiz kargo eşiği")]
    [InvalidAmount(AllowZero = true)]
    public decimal? FreeShippingThreshold { get; set; }

    [Display(Name = "Asgari sipariş tutarı")]
    [InvalidAmount(AllowZero = true)]
    public decimal? MinimumOrderAmount { get; set; }

    public int ShippingFeeVersion { get; set; }
}

/// <summary>The delivery provinces section's input (07 9.18.3 <c>SaveDeliveryProvincesInput</c>; 04 7.2.21.3): the selected provinces' ids - the parameter repeats - and the section's version.</summary>
public sealed class SaveDeliveryProvincesInput : IFormInput
{
    [Display(Name = "Teslimat illeri")]
    [SpecAssignedName]
    public List<string> ProvinceIds { get; set; } = [];

    public int DeliveryProvincesVersion { get; set; }
}

/// <summary>The periods section's input (07 9.18.4 <c>SavePeriodsInput</c>; 04 7.2.21.4): two numbers of business days and the section's version.</summary>
public sealed class SavePeriodsInput : IFormInput
{
    [Display(Name = "Kargoya verme süresi")]
    [FieldRequired]
    public int? DefaultShippingPeriod { get; set; }

    [Display(Name = "Havale ödeme süresi")]
    [FieldRequired]
    public int? BankTransferPaymentWindow { get; set; }

    public int PeriodsVersion { get; set; }
}

/// <summary>The product defaults section's input (07 9.18.5 <c>SaveProductDefaultsInput</c>; 04 7.2.21.5): the download allowance, the VAT rate default and the section's version.</summary>
public sealed class SaveProductDefaultsInput : IFormInput
{
    [Display(Name = "İndirme hakkı")]
    [FieldRequired]
    public int? DownloadAllowance { get; set; }

    [Display(Name = "KDV oranının varsayılanı")]
    [FieldRequired]
    [InvalidPercentage(AllowZero = true)]
    public decimal? DefaultVatRate { get; set; }

    public int ProductDefaultsVersion { get; set; }
}

/// <summary>The return address section's input (07 9.18.6 <c>SaveReturnAddressInput</c>; 04 7.2.21.6, 7.2.21.7): four required parts, the optional telephone, the version and the confirmation mark.</summary>
public sealed class SaveReturnAddressInput : IFormInput
{
    [Display(Name = "Alıcının adı")]
    public string? ReturnRecipientName { get; set; }

    [Display(Name = "İl")]
    public string? ReturnProvinceId { get; set; }

    [Display(Name = "İlçe")]
    public string? ReturnDistrict { get; set; }

    [Display(Name = "Açık adres")]
    public string? ReturnAddressLine { get; set; }

    [Display(Name = "Telefon")]
    public string? ReturnPhone { get; set; }

    public int ReturnAddressVersion { get; set; }

    /// <summary>True when the confirmation window's button sent the form (07 2.2.4; K-1194 (2)).</summary>
    [SpecAssignedName]
    public bool Confirmed { get; set; }
}

/// <summary>One province box of the delivery provinces section: the id, the name and whether it is ticked.</summary>
public sealed record ProvinceChoiceView(string Id, string Name, bool IsSelected);

/// <summary>
/// E-46, the shipping, periods and defaults screen (T536; 04 9.18; 07 UC-101, UC-168...UC-172): five sections in the order of K-752, each its own form with its own version and its own save; a conflict
/// shows only on the section that was saved (04 9.18.11). The GET writes nothing. The rules are the Settings module's; the page turns their refusals into the field messages - the fence texts are
/// K-817's - and asks the confirmation of a return address change with the number of the items whose returned goods are awaited (K-665, K-814).
/// </summary>
[NoStore]
public sealed class ShippingAndDefaultsModel : PageModel
{
    public const string FrozenNote = "Bu bölümdeki değerler siparişe o günkü hâliyle yazılır; değişiklik yalnız yeni siparişlere işler.";

    public const string FeeVatNote = "Kargonun KDV'si bir ayar değildir; siparişteki fiziksel kalemlerin oranını izler.";

    public const string ThresholdOffText = "Boş bırakılırsa ücretsiz kargo uygulanmaz.";

    public const string MinimumOffText = "Boş bırakılırsa asgari sipariş tutarı uygulanmaz.";

    public const string FreeTextReminder = "SSS ya da sayfa metinlerinde bu değerlerden söz ediyorsanız onları da güncelleyin.";

    /// <summary>K-817: the line of a section with no province selected.</summary>
    public const string NoProvinceText = "Hiçbir il seçili değil: fiziksel ürün içeren sipariş verilemez.";

    public const string ProvinceRestrictionNote = "Kısıt varsa vitrinde adres adımından önce görünür; değişiklik Ön Bilgilendirme Formu'nun ve Mesafeli Satış Sözleşmesi'nin yeni sürümünü üretir.";

    public const string ProductPeriodNote = "Bir ürün kendi kargoya verme süresini ürün formunda taşıyabilir.";

    public const string DefaultsNote = "İndirme hakkı sipariş anında kaleme donar; KDV varsayılanı yalnız yeni ürünün formuna gelir, var olan ürünün oranını değiştirmez.";

    public const string ReturnAddressNote = "İade adresi zorunlu bir ayardır ve satışın açılmasının koşuludur; firma kimliğindeki adresten ayrıdır.";

    public const string ChangeReturnAddressLabel = "İade adresini değiştir";

    public const string ShippingFeeSection = "kargo-ucreti";

    public const string ProvincesSection = "teslimat-illeri";

    public const string PeriodsSection = "sureler";

    public const string DefaultsSection = "urun-varsayilanlari";

    public const string ReturnAddressSection = "iade-adresi";

    private readonly ISettingsModule _settings;
    private readonly IAfterSalesModule _afterSales;
    private object? _posted;
    private string? _conflictedSection;
    private Loaded? _loaded;
    private FormErrorSummary? _summary;

    public ShippingAndDefaultsModel(ISettingsModule settings, IAfterSalesModule afterSales)
    {
        _settings = settings;
        _afterSales = afterSales;
    }

    public string? ConflictedSection => _conflictedSection;

    public FormErrorSummary Summary => _summary ??= FormErrorSummary.From(ModelState, FormOrder);

    public ConfirmView? Confirm { get; private set; }

    public MessageView? Notice { get; private set; }

    public int ShippingFeeVersion => VersionOf<SaveShippingFeeInput>(Data.ShippingFee.Version, i => i.ShippingFeeVersion, ShippingFeeSection);

    public int DeliveryProvincesVersion => VersionOf<SaveDeliveryProvincesInput>(Data.Provinces.Version, i => i.DeliveryProvincesVersion, ProvincesSection);

    public int PeriodsVersion => VersionOf<SavePeriodsInput>(Data.Periods.Version, i => i.PeriodsVersion, PeriodsSection);

    public int ProductDefaultsVersion => VersionOf<SaveProductDefaultsInput>(Data.Defaults.Version, i => i.ProductDefaultsVersion, DefaultsSection);

    public int ReturnAddressVersion => VersionOf<SaveReturnAddressInput>(Data.ReturnAddress.Version, i => i.ReturnAddressVersion, ReturnAddressSection);

    public IReadOnlyList<FieldView> ShippingFeeFields => Fields.BindAll(
    [
        Fields.Text("Input.ShippingFee", "Kargo ücreti (sipariş başına, KDV dahil)", FieldRequirement.Required, Sent<SaveShippingFeeInput>(i => Amount(i.ShippingFee), Amount(Data.ShippingFee.ShippingFee))),
        Fields.Text("Input.FreeShippingThreshold", "Ücretsiz kargo eşiği", FieldRequirement.Optional, Sent<SaveShippingFeeInput>(i => Amount(i.FreeShippingThreshold), Amount(Data.ShippingFee.FreeShippingThreshold)), hint: ThresholdOffText),
        Fields.Text("Input.MinimumOrderAmount", "Asgari sipariş tutarı", FieldRequirement.Optional, Sent<SaveShippingFeeInput>(i => Amount(i.MinimumOrderAmount), Amount(Data.ShippingFee.MinimumOrderAmount)), hint: MinimumOffText),
    ], Summary);

    /// <summary>The province boxes in plate order; ticked from the sent form after a refusal, from the stored selection otherwise.</summary>
    public IReadOnlyList<ProvinceChoiceView> ProvinceChoices
    {
        get
        {
            HashSet<string> selected = _posted is SaveDeliveryProvincesInput sent
                ? [.. sent.ProvinceIds]
                : [.. Data.Provinces.ProvinceIds.Select(id => id.ToString())];
            return [.. Data.AllProvinces.Select(p => new ProvinceChoiceView(p.Id.ToString(), p.Name, selected.Contains(p.Id.ToString())))];
        }
    }

    public int SelectedProvinceCount => ProvinceChoices.Count(p => p.IsSelected);

    /// <summary>True when the stored selection is empty: the section says so (K-817; 07 UC-101).</summary>
    public bool ShowsNoProvince => Data.Provinces.ProvinceIds.Count == 0;

    public string? ProvincesError => ModelState.TryGetValue("Input.ProvinceIds", out Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateEntry? entry) && entry.Errors.Count > 0 ? entry.Errors[0].ErrorMessage : null;

    public IReadOnlyList<FieldView> PeriodFields => Fields.BindAll(
    [
        Fields.Number("Input.DefaultShippingPeriod", "Kargoya verme süresi (iş günü)", FieldRequirement.Required, Sent<SavePeriodsInput>(i => Whole(i.DefaultShippingPeriod), Whole(Data.Periods.DefaultShippingPeriod))),
        Fields.Number("Input.BankTransferPaymentWindow", "Havale ödeme süresi (iş günü)", FieldRequirement.Required, Sent<SavePeriodsInput>(i => Whole(i.BankTransferPaymentWindow), Whole(Data.Periods.BankTransferPaymentWindow))),
    ], Summary);

    public IReadOnlyList<FieldView> DefaultsFields => Fields.BindAll(
    [
        Fields.Number("Input.DownloadAllowance", "Dijital kalemin indirme hakkı", FieldRequirement.Required, Sent<SaveProductDefaultsInput>(i => Whole(i.DownloadAllowance), Whole(Data.Defaults.DownloadAllowance))),
        Fields.Text("Input.DefaultVatRate", "KDV oranının varsayılanı (%)", FieldRequirement.Required, Sent<SaveProductDefaultsInput>(i => Amount(i.DefaultVatRate), Amount(Data.Defaults.DefaultVatRate))),
    ], Summary);

    public IReadOnlyList<FieldView> ReturnAddressFields
    {
        get
        {
            ReturnAddressSnapshot? stored = Data.ReturnAddress.Address;
            SaveReturnAddressInput? sent = _posted as SaveReturnAddressInput;
            string? Pick(Func<SaveReturnAddressInput, string?> fromSent, string? fromStored) => sent is not null ? fromSent(sent) : fromStored;
            return Fields.BindAll(
            [
                Fields.Text("Input.ReturnRecipientName", "Alıcının adı", FieldRequirement.Required, Pick(i => i.ReturnRecipientName, stored?.RecipientName)),
                Fields.ClosedList("Input.ReturnProvinceId", "İl", FieldRequirement.Required, [.. Data.AllProvinces.Select(p => new FieldOption(p.Id.ToString(), p.Name))], Pick(i => i.ReturnProvinceId, stored?.ProvinceId.ToString())),
                Fields.Text("Input.ReturnDistrict", "İlçe", FieldRequirement.Required, Pick(i => i.ReturnDistrict, stored?.District)),
                Fields.Text("Input.ReturnAddressLine", "Açık adres", FieldRequirement.Required, Pick(i => i.ReturnAddressLine, stored?.AddressLine)),
                Fields.Telephone("Input.ReturnPhone", "Telefon", FieldRequirement.Optional, Pick(i => i.ReturnPhone, stored?.Phone)),
            ], Summary);
        }
    }

    private static readonly string[] FormOrder =
    [
        "Input.ShippingFee", "Input.FreeShippingThreshold", "Input.MinimumOrderAmount", "Input.ProvinceIds", "Input.DefaultShippingPeriod", "Input.BankTransferPaymentWindow",
        "Input.DownloadAllowance", "Input.DefaultVatRate", "Input.ReturnRecipientName", "Input.ReturnProvinceId", "Input.ReturnDistrict", "Input.ReturnAddressLine", "Input.ReturnPhone",
    ];

    private Loaded Data => _loaded ?? throw new InvalidOperationException("The screen is read before it is loaded.");

    public override async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        await LoadAsync(HttpContext.RequestAborted);
    }

    public IActionResult OnGet() => Page();

    /// <summary>UC-168: the fee and the two amounts; 422 by field, 409 on the section's version.</summary>
    public async Task<IActionResult> OnPostSaveShippingFeeAsync(SaveShippingFeeInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        return await AnswerAsync(input, ShippingFeeSection, await Runner.RunAsync(token => _settings.SaveShippingFeeAsync(new ShippingFeeInput(input.ShippingFee, input.FreeShippingThreshold, input.MinimumOrderAmount), input.ShippingFeeVersion, token), ct), ct);
    }

    /// <summary>UC-169: the selected provinces in one transaction; a province off the closed list is 422; no selection at all is saved (K-817).</summary>
    public async Task<IActionResult> OnPostSaveDeliveryProvincesAsync(SaveDeliveryProvincesInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);

        // A value that is no id cannot be on the list of 81: it is the same refusal as an unknown id (07 UC-169 422).
        List<Guid> ids = [.. input.ProvinceIds.Select(v => Guid.TryParse(v, out Guid id) ? id : Guid.Empty)];
        return await AnswerAsync(input, ProvincesSection, await Runner.RunAsync(token => _settings.SaveDeliveryProvincesAsync(ids, input.DeliveryProvincesVersion, token), ct), ct);
    }

    /// <summary>UC-170: the two periods; a value over P-5's upper fence or outside P-7's fences is 422 with K-817's text.</summary>
    public async Task<IActionResult> OnPostSavePeriodsAsync(SavePeriodsInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        return await AnswerAsync(input, PeriodsSection, await Runner.RunAsync(token => _settings.SavePeriodsAsync(new PeriodsInput(input.DefaultShippingPeriod, input.BankTransferPaymentWindow), input.PeriodsVersion, token), ct), ct);
    }

    /// <summary>UC-171: the allowance and the VAT default; a value outside the fence is 422. The rate of an existing product does not change.</summary>
    public async Task<IActionResult> OnPostSaveProductDefaultsAsync(SaveProductDefaultsInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        return await AnswerAsync(input, DefaultsSection, await Runner.RunAsync(token => _settings.SaveProductDefaultsAsync(new ProductDefaultsInput(input.DownloadAllowance, input.DefaultVatRate), input.ProductDefaultsVersion, token), ct), ct);
    }

    /// <summary>
    /// UC-172: the first entry is saved without a confirmation; a change of the entered address answers its first send with the confirmation that says how many items' returned goods are awaited
    /// (200, nothing written; K-665, K-814); the confirmed send saves. 422 by field, 409 on the section's version.
    /// </summary>
    [OutcomeUncertain("UC-172")]
    public async Task<IActionResult> OnPostSaveReturnAddressAsync(SaveReturnAddressInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        ReturnAddressSection current = Data.ReturnAddress;
        if (!input.Confirmed && current.Address is { } stored && input.ReturnAddressVersion == current.Version && Differs(input, stored))
        {
            int awaiting = await _afterSales.CountItemsAwaitingReturnGoodsAsync(ct);
            Confirm = Confirmations.ForSettingChange(
                ChangeReturnAddressLabel,
                ReturnAddressChangeSentence(awaiting),
                ChangeReturnAddressLabel,
                PanelFramePaths.ShippingAndDefaults,
                "SaveReturnAddress",
                PanelFramePaths.ShippingAndDefaults,
                [
                    new("Input.ReturnRecipientName", input.ReturnRecipientName ?? string.Empty),
                    new("Input.ReturnProvinceId", input.ReturnProvinceId ?? string.Empty),
                    new("Input.ReturnDistrict", input.ReturnDistrict ?? string.Empty),
                    new("Input.ReturnAddressLine", input.ReturnAddressLine ?? string.Empty),
                    new("Input.ReturnPhone", input.ReturnPhone ?? string.Empty),
                    new("Input.ReturnAddressVersion", input.ReturnAddressVersion.ToString(CultureInfo.InvariantCulture)),
                    new("Input.Confirmed", "true"),
                ]);
            _posted = input;
            return Page();
        }

        ReturnAddressInput values = new(
            input.ReturnRecipientName,
            Guid.TryParse(input.ReturnProvinceId, out Guid province) ? province : (string.IsNullOrWhiteSpace(input.ReturnProvinceId) ? null : Guid.Empty),
            input.ReturnDistrict,
            input.ReturnAddressLine,
            input.ReturnPhone);
        return await AnswerAsync(input, ReturnAddressSection, await Runner.RunAsync(token => _settings.SaveReturnAddressAsync(values, input.ReturnAddressVersion, token), ct), ct);
    }

    /// <summary>K-814: the return address change's sentence with the number of the items marked "iade malı bekleniyor".</summary>
    public static string ReturnAddressChangeSentence(int awaiting) =>
        "Yeni cayma ve ayıp beyanları yeni adresi alacak. 'İade malı bekleniyor' işaretli "
        + awaiting.ToString(CultureInfo.InvariantCulture)
        + " kalemin beyanındaki adres değişmez; bu kalemlerin malı eski adrese gelirse teslim almak sizin yükümlülüğünüzdür.";

    /// <summary>K-817: the fence texts of the periods and the lower fence of the download allowance (04 9.18.12).</summary>
    public static string FenceText(string field)
    {
        return field switch
        {
            nameof(PeriodsInput.DefaultShippingPeriod) => $"Kargoya verme süresi {Fence("P-5").Upper} iş gününü aşamaz.",
            nameof(PeriodsInput.BankTransferPaymentWindow) => $"Havale ödeme süresi {Fence("P-7").Lower} ile {Fence("P-7").Upper} iş günü arasında olmalı.",
            nameof(ProductDefaultsInput.DownloadAllowance) => $"İndirme hakkı {Fence("P-8").Lower}'den küçük olamaz.",
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, "No fenced field."),
        };
    }

    private static FenceBounds Fence(string parameterId) => FirmSettingFences.BoundsOf(parameterId);

    private static bool Differs(SaveReturnAddressInput input, ReturnAddressSnapshot stored)
    {
        static string Trimmed(string? value) => value?.Trim() ?? string.Empty;
        string? phone = string.IsNullOrWhiteSpace(input.ReturnPhone) ? null : RequestFormat.StoredPhone(input.ReturnPhone);
        return !string.Equals(Trimmed(input.ReturnRecipientName), stored.RecipientName, StringComparison.Ordinal)
            || !string.Equals(Trimmed(input.ReturnProvinceId), stored.ProvinceId.ToString(), StringComparison.OrdinalIgnoreCase)
            || !string.Equals(Trimmed(input.ReturnDistrict), stored.District, StringComparison.Ordinal)
            || !string.Equals(Trimmed(input.ReturnAddressLine), stored.AddressLine, StringComparison.Ordinal)
            || !string.Equals(phone ?? Trimmed(input.ReturnPhone), stored.Phone ?? string.Empty, StringComparison.Ordinal);
    }

    // Each handler hands the runner its work as a lambda written at the call, so the transaction body scan reads it (D-82).
    private ITransactionRunner Runner => HttpContext.RequestServices.GetRequiredService<ITransactionRunner>();

    /// <summary>The section's answer after its save: 303 with the notice, or the screen again with the section's 422 or 409.</summary>
    private async Task<IActionResult> AnswerAsync(object input, string section, SettingsSaveResult result, CancellationToken ct)
    {
        if (result.IsSaved)
        {
            return FormResults.SeeOther(PanelFramePaths.ShippingAndDefaults, NoticeCode.Saved);
        }

        _posted = input;
        _conflictedSection = result.IsVersionConflict ? section : null;
        foreach (FieldRejection rejection in result.Rejections)
        {
            AddRejection(rejection, section);
        }

        await LoadAsync(ct);
        return result.IsVersionConflict ? FormResults.Conflict(this) : FormResults.Redisplay(this);
    }

    private void AddRejection(FieldRejection rejection, string section)
    {
        // The module names its own fields; the form's fields of the provinces and of the return address carry the names of 07 9.18.3 and 9.18.6.
        string field = "Input." + section switch
        {
            ProvincesSection => nameof(SaveDeliveryProvincesInput.ProvinceIds),
            ReturnAddressSection => "Return" + rejection.Field,
            _ => rejection.Field,
        };
        if (rejection.Code == nameof(ErrorCode.ValueOutOfBounds))
        {
            // K-817's fence texts are the decision's own; the log line carries the code (09 8.2 item 3).
            HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(ErrorCode.ValueOutOfBounds);
            ModelState.AddModelError(field, FenceText(rejection.Field));
            return;
        }

        string? parameter = rejection.Code switch
        {
            FieldCodes.InvalidAmount => DisplayFormat.Amount(Money.MaxAmount),
            FieldCodes.InvalidPercentage => DisplayFormat.Percent(Money.MaxPercentage),
            _ => null,
        };
        ModelState.AddFieldError(field, rejection.Code, parameter);
    }

    private int VersionOf<T>(int stored, Func<T, int> sent, string section)
        where T : class =>
        _posted is T input && _conflictedSection != section ? sent(input) : stored;

    private string? Sent<T>(Func<T, string?> fromSent, string? fromStored)
        where T : class =>
        _posted is T input ? fromSent(input) : fromStored;

    private static string? Amount(decimal? value) => value is { } amount ? RequestFormat.FormatDecimal(amount) : null;

    private static string? Whole(int? value) => value?.ToString(CultureInfo.InvariantCulture);

    private async Task LoadAsync(CancellationToken ct)
    {
        _loaded = new Loaded(
            await _settings.GetShippingFeeAsync(ct),
            await _settings.GetDeliveryProvincesAsync(ct),
            await _settings.GetPeriodsAsync(ct),
            await _settings.GetProductDefaultsAsync(ct),
            await _settings.GetReturnAddressAsync(ct),
            await _settings.GetProvincesAsync(ct));
        _summary = null;
        NoticeCode? notice = NoticeParameter.Read(Request, NoticeKind.ShortLived);
        Notice = notice is { } code && NoticeTexts.TryShortLived(code, out string? noticeText) ? Messages.ShortLivedSuccess(noticeText) : null;
    }

    private sealed record Loaded(
        ShippingFeeSection ShippingFee,
        DeliveryProvincesSection Provinces,
        PeriodsSection Periods,
        ProductDefaultsSection Defaults,
        ReturnAddressSection ReturnAddress,
        IReadOnlyList<ProvinceInfo> AllProvinces);
}
