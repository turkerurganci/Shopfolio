using System.Globalization;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Settings.Contracts;

using FieldRejection = Shopfolio.Modules.Settings.Contracts.FieldRejection;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// The saves of the firm settings that are numbers (02 11.1; 09 10.3; T381): the shipping fee section (P-1, P-3, P-4), the periods section (P-5, P-7) and the product
/// defaults section (P-8, P-10). The three read their section and write it by the conditional update of the section's own version, behind the settings row lock
/// (<see cref="CompanySettingsWriter.LockAsync"/>), in the caller's transaction. The fences are applied here (02 4.1.5, 11.1): a value outside a fence is a refusal that
/// names the side and the fence, never a stored value. Every changed field is written to the operation trail with its old and new value in the same transaction
/// (02 10.3.1 (6); K-414, K-523), by the administrator of the request, so a change without its row cannot happen. A save that changes nothing writes nothing: no
/// version step and no trail row.
/// </summary>
internal sealed class FirmSettings
{
    /// <summary>The names of the sections as the trail shows them (02 10.3.1 (6)).</summary>
    internal const string ShippingFeeDisplayName = "Kargo ücreti ve eşikler";

    internal const string PeriodsDisplayName = "Süreler";

    internal const string ProductDefaultsDisplayName = "Ürün varsayılanları";

    internal const string HomepageDisplayName = "Ana sayfa ve menü";

    internal const string ReturnAddressDisplayName = "İade adresi";

    /// <summary>The prefix of a social link in the trail row's field name; the platform's name follows.</summary>
    internal const string SocialLinkFieldPrefix = "SocialLink.";

    private readonly AppDbContext _db;
    private readonly CompanySettingsWriter _writer;
    private readonly IAuditTrail _trail;

    public FirmSettings(AppDbContext db, CompanySettingsWriter writer, IAuditTrail trail)
    {
        _db = db;
        _writer = writer;
        _trail = trail;
    }

    public async Task<ShippingFeeSection> ReadShippingFeeAsync(CancellationToken ct)
    {
        return await _db.CompanySettings.AsNoTracking()
            .Select(s => new ShippingFeeSection(s.ShippingFee, s.FreeShippingThreshold, s.MinimumOrderAmount, s.ShippingFeeVersion))
            .SingleAsync(ct);
    }

    /// <summary>The home page and menu section (T361; 06 4.2): the layout, the six names and the section's version. The save of the section is the panel's (T535).</summary>
    public async Task<HomepageSection> ReadHomepageAsync(CancellationToken ct)
    {
        return await _db.CompanySettings.AsNoTracking()
            .Select(s => new HomepageSection(
                s.HomepageLayout,
                new MenuNames(s.MenuNameProducts, s.MenuNameOfferings, s.MenuNamePortfolioItems, s.MenuNameAboutPage, s.MenuNameFaqItems, s.MenuNameContact),
                s.HomepageVersion))
            .SingleAsync(ct);
    }

    public async Task<PeriodsSection> ReadPeriodsAsync(CancellationToken ct)
    {
        return await _db.CompanySettings.AsNoTracking()
            .Select(s => new PeriodsSection(s.DefaultShippingPeriod, s.BankTransferPaymentWindow, s.PeriodsVersion))
            .SingleAsync(ct);
    }

    public async Task<string?> ReadBankTransferIbanAsync(CancellationToken ct)
    {
        return await _db.CompanySettings.AsNoTracking().Select(s => s.BankTransferIban).SingleAsync(ct);
    }

    public async Task<string?> ReadWhatsAppNumberAsync(CancellationToken ct)
    {
        return await _db.CompanySettings.AsNoTracking().Select(s => s.WhatsAppNumber).SingleAsync(ct);
    }

    public async Task<ProductDefaultsSection> ReadProductDefaultsAsync(CancellationToken ct)
    {
        return await _db.CompanySettings.AsNoTracking()
            .Select(s => new ProductDefaultsSection(s.DownloadAllowance, s.DefaultVatRate, s.ProductDefaultsVersion))
            .SingleAsync(ct);
    }

    /// <summary>
    /// The save of the home page and menu section (T535; 07 UC-162): the row lock, the checks (a required layout, six names that are not empty, an optional WhatsApp number of the telephone form),
    /// the changed values with their old and new value - the social links the content module changed in the same transaction included - and the one conditional update of the section's version.
    /// </summary>
    public async Task<SettingsSaveResult> SaveHomepageAsync(HomepageInput input, int formVersion, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        await _writer.LockAsync(ct);
        HomepageSection current = await ReadHomepageAsync(ct);
        string? currentWhatsApp = await ReadWhatsAppNumberAsync(ct);

        List<FieldRejection> rejections = [];
        if (input.HomepageLayout is not { } layout || !Enum.IsDefined(layout))
        {
            rejections.Add(new FieldRejection(nameof(HomepageInput.HomepageLayout), input.HomepageLayout is null ? FieldCodes.FieldRequired : FieldCodes.InvalidListValue));
        }

        string? products = RequiredName(rejections, nameof(HomepageInput.MenuNameProducts), input.MenuNameProducts);
        string? offerings = RequiredName(rejections, nameof(HomepageInput.MenuNameOfferings), input.MenuNameOfferings);
        string? portfolio = RequiredName(rejections, nameof(HomepageInput.MenuNamePortfolioItems), input.MenuNamePortfolioItems);
        string? about = RequiredName(rejections, nameof(HomepageInput.MenuNameAboutPage), input.MenuNameAboutPage);
        string? faq = RequiredName(rejections, nameof(HomepageInput.MenuNameFaqItems), input.MenuNameFaqItems);
        string? contact = RequiredName(rejections, nameof(HomepageInput.MenuNameContact), input.MenuNameContact);
        string? whatsApp = CompanyIdentityRules.Phone(input.WhatsAppNumber);
        if (whatsApp is not null && new InvalidPhoneAttribute().GetValidationResult(whatsApp, new System.ComponentModel.DataAnnotations.ValidationContext(whatsApp)) is not null)
        {
            rejections.Add(new FieldRejection(nameof(HomepageInput.WhatsAppNumber), FieldCodes.InvalidPhone));
        }

        if (rejections.Count > 0)
        {
            return SettingsSaveResult.Refused(rejections);
        }

        MenuNames names = new(products!, offerings!, portfolio!, about!, faq!, contact!); // non-null: RequiredName refuses an empty name, refused above
        HomepageLayout chosen = input.HomepageLayout!.Value; // non-null: a missing layout is refused above
        List<AuditValueChange> changes = [];
        TrackText(changes, nameof(CompanySettings.HomepageLayout), current.Layout.ToString(), chosen.ToString());
        TrackText(changes, nameof(CompanySettings.MenuNameProducts), current.MenuNames.Products, names.Products);
        TrackText(changes, nameof(CompanySettings.MenuNameOfferings), current.MenuNames.Offerings, names.Offerings);
        TrackText(changes, nameof(CompanySettings.MenuNamePortfolioItems), current.MenuNames.PortfolioItems, names.PortfolioItems);
        TrackText(changes, nameof(CompanySettings.MenuNameAboutPage), current.MenuNames.AboutPage, names.AboutPage);
        TrackText(changes, nameof(CompanySettings.MenuNameFaqItems), current.MenuNames.FaqItems, names.FaqItems);
        TrackText(changes, nameof(CompanySettings.MenuNameContact), current.MenuNames.Contact, names.Contact);
        TrackText(changes, nameof(CompanySettings.WhatsAppNumber), currentWhatsApp, whatsApp);
        foreach (SocialLinkChange link in input.SocialLinkChanges ?? [])
        {
            changes.Add(new AuditValueChange(SocialLinkFieldPrefix + link.Platform, link.Before, link.After));
        }

        if (changes.Count == 0)
        {
            return Unchanged(current.Version, formVersion);
        }

        bool written = await _writer.WriteHomepageAsync(chosen, names, whatsApp, formVersion, ct);
        return await FinishAsync(written, HomepageDisplayName, changes, ct);
    }

    private static string? RequiredName(List<FieldRejection> rejections, string field, string? typed)
    {
        string? name = CompanyIdentityRules.Text(typed);
        if (name is null)
        {
            rejections.Add(new FieldRejection(field, FieldCodes.FieldRequired));
        }

        return name;
    }

    private static void TrackText(List<AuditValueChange> changes, string field, string? oldValue, string? newValue)
    {
        if (!string.Equals(oldValue, newValue, StringComparison.Ordinal))
        {
            changes.Add(new AuditValueChange(field, oldValue, newValue));
        }
    }

    /// <summary>The save of the shipping fee section; the outcome says whether the fee itself changed - the fee feeds the generated texts, the two amounts do not (04 9.18.9; K-1215).</summary>
    public async Task<SectionSaveOutcome> SaveShippingFeeAsync(ShippingFeeInput input, int formVersion, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        await _writer.LockAsync(ct);
        ShippingFeeSection current = await ReadShippingFeeAsync(ct);

        List<FieldRejection> rejections = [];
        CheckRequiredAmount(rejections, nameof(ShippingFeeInput.ShippingFee), input.ShippingFee);
        CheckOptionalAmount(rejections, nameof(ShippingFeeInput.FreeShippingThreshold), input.FreeShippingThreshold);
        CheckOptionalAmount(rejections, nameof(ShippingFeeInput.MinimumOrderAmount), input.MinimumOrderAmount);
        if (rejections.Count > 0)
        {
            return new SectionSaveOutcome(SettingsSaveResult.Refused(rejections), false);
        }

        decimal fee = input.ShippingFee!.Value; // non-null: CheckRequiredAmount refuses an empty fee, refused above
        List<AuditValueChange> changes = [];
        Track(changes, nameof(CompanySettings.ShippingFee), current.ShippingFee, fee);
        Track(changes, nameof(CompanySettings.FreeShippingThreshold), current.FreeShippingThreshold, input.FreeShippingThreshold);
        Track(changes, nameof(CompanySettings.MinimumOrderAmount), current.MinimumOrderAmount, input.MinimumOrderAmount);
        if (changes.Count == 0)
        {
            return new SectionSaveOutcome(Unchanged(current.Version, formVersion), false);
        }

        bool written = await _writer.WriteShippingFeeAsync(fee, input.FreeShippingThreshold, input.MinimumOrderAmount, formVersion, ct);
        SettingsSaveResult result = await FinishAsync(written, ShippingFeeDisplayName, changes, ct);
        return new SectionSaveOutcome(result, result.IsSaved && current.ShippingFee != fee);
    }

    /// <summary>The save of the periods section; the outcome says whether the firm default of the shipping period changed - it feeds the generated texts, the payment window does not (04 9.18.9).</summary>
    public async Task<SectionSaveOutcome> SavePeriodsAsync(PeriodsInput input, int formVersion, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        await _writer.LockAsync(ct);
        PeriodsSection current = await ReadPeriodsAsync(ct);

        List<FieldRejection> rejections = [];
        CheckFenced(rejections, nameof(PeriodsInput.DefaultShippingPeriod), "P-5", input.DefaultShippingPeriod);
        CheckFenced(rejections, nameof(PeriodsInput.BankTransferPaymentWindow), "P-7", input.BankTransferPaymentWindow);
        if (rejections.Count > 0)
        {
            return new SectionSaveOutcome(SettingsSaveResult.Refused(rejections), false);
        }

        int shipping = input.DefaultShippingPeriod!.Value; // non-null: CheckFenced refuses an empty period, refused above
        int window = input.BankTransferPaymentWindow!.Value; // non-null: CheckFenced refuses an empty window, refused above
        List<AuditValueChange> changes = [];
        Track(changes, nameof(CompanySettings.DefaultShippingPeriod), current.DefaultShippingPeriod, shipping);
        Track(changes, nameof(CompanySettings.BankTransferPaymentWindow), current.BankTransferPaymentWindow, window);
        if (changes.Count == 0)
        {
            return new SectionSaveOutcome(Unchanged(current.Version, formVersion), false);
        }

        bool written = await _writer.WritePeriodsAsync(shipping, window, formVersion, ct);
        SettingsSaveResult result = await FinishAsync(written, PeriodsDisplayName, changes, ct);
        return new SectionSaveOutcome(result, result.IsSaved && current.DefaultShippingPeriod != shipping);
    }

    /// <summary>
    /// The save of the return address section (T536; 07 UC-172; 02 3.1.7): the writer's checks and its conditional update (<see cref="CompanySettingsWriter.SaveReturnAddressAsync"/>), then the trail row with
    /// the old and new value of each changed part in the same transaction (02 10.3.1 (6); K-523; D-165 (a), D-1377). The province is written by its plate code. A save that changes no part writes no row.
    /// </summary>
    public async Task<SettingsSaveResult> SaveReturnAddressAsync(ReturnAddressInput input, int formVersion, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        // The writer takes the row lock and reads the section under it; the value it read is the "before" of the trail row.
        ReturnAddressSection? read = null;
        SettingsSaveResult result = await _writer.SaveReturnAddressAsync(input, formVersion, ct, section => read = section);
        if (!result.IsSaved || read is not { } before)
        {
            return result;
        }

        ReturnAddressSection after = await _writer.ReadReturnAddressAsync(ct);
        Dictionary<Guid, string> codes = await _db.Provinces.AsNoTracking().ToDictionaryAsync(p => p.Id, p => p.Code, ct);
        string? Code(ReturnAddressSnapshot? address) => address is null ? null : codes.GetValueOrDefault(address.ProvinceId);
        List<AuditValueChange> changes = [];
        TrackText(changes, nameof(CompanySettings.ReturnRecipientName), before.Address?.RecipientName, after.Address?.RecipientName);
        TrackText(changes, nameof(CompanySettings.ReturnProvinceId), Code(before.Address), Code(after.Address));
        TrackText(changes, nameof(CompanySettings.ReturnDistrict), before.Address?.District, after.Address?.District);
        TrackText(changes, nameof(CompanySettings.ReturnAddressLine), before.Address?.AddressLine, after.Address?.AddressLine);
        TrackText(changes, nameof(CompanySettings.ReturnPhone), before.Address?.Phone, after.Address?.Phone);
        if (changes.Count > 0)
        {
            Guid rowId = await _db.CompanySettings.AsNoTracking().Select(s => s.Id).SingleAsync(ct);
            await _trail.WriteAsync(new AuditEntry(AuditOperation.UpdateSettings, nameof(CompanySettings), rowId, new AuditDetails(ReturnAddressDisplayName, changes)), ct);
        }

        return result;
    }

    public async Task<SettingsSaveResult> SaveProductDefaultsAsync(ProductDefaultsInput input, int formVersion, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        await _writer.LockAsync(ct);
        ProductDefaultsSection current = await ReadProductDefaultsAsync(ct);

        List<FieldRejection> rejections = [];
        CheckFenced(rejections, nameof(ProductDefaultsInput.DownloadAllowance), "P-8", input.DownloadAllowance);
        if (input.DefaultVatRate is not { } rate)
        {
            rejections.Add(new FieldRejection(nameof(ProductDefaultsInput.DefaultVatRate), FieldCodes.FieldRequired));
        }
        else if (rate < 0m || rate > Money.MaxPercentage || HasMoreThanTwoDecimals(rate))
        {
            rejections.Add(new FieldRejection(nameof(ProductDefaultsInput.DefaultVatRate), FieldCodes.InvalidPercentage));
        }

        if (rejections.Count > 0)
        {
            return SettingsSaveResult.Refused(rejections);
        }

        int allowance = input.DownloadAllowance!.Value; // non-null: CheckFenced refuses an empty allowance, refused above
        decimal vat = input.DefaultVatRate!.Value; // non-null: an empty rate is refused above (FieldRequired)
        List<AuditValueChange> changes = [];
        Track(changes, nameof(CompanySettings.DownloadAllowance), current.DownloadAllowance, allowance);
        Track(changes, nameof(CompanySettings.DefaultVatRate), current.DefaultVatRate, vat);
        if (changes.Count == 0)
        {
            return Unchanged(current.Version, formVersion);
        }

        bool written = await _writer.WriteProductDefaultsAsync(allowance, vat, formVersion, ct);
        return await FinishAsync(written, ProductDefaultsDisplayName, changes, ct);
    }

    private static SettingsSaveResult Unchanged(int currentVersion, int formVersion)
    {
        return currentVersion == formVersion ? SettingsSaveResult.Saved : SettingsSaveResult.VersionConflict;
    }

    private async Task<SettingsSaveResult> FinishAsync(bool written, string displayName, List<AuditValueChange> changes, CancellationToken ct)
    {
        if (!written)
        {
            return SettingsSaveResult.VersionConflict;
        }

        Guid rowId = await _db.CompanySettings.AsNoTracking().Select(s => s.Id).SingleAsync(ct);
        await _trail.WriteAsync(new AuditEntry(AuditOperation.UpdateSettings, nameof(CompanySettings), rowId, new AuditDetails(displayName, changes)), ct);
        return SettingsSaveResult.Saved;
    }

    private static void CheckRequiredAmount(List<FieldRejection> rejections, string field, decimal? value)
    {
        if (value is null)
        {
            rejections.Add(new FieldRejection(field, FieldCodes.FieldRequired));
            return;
        }

        CheckOptionalAmount(rejections, field, value);
    }

    private static void CheckOptionalAmount(List<FieldRejection> rejections, string field, decimal? value)
    {
        // An amount is not negative, has at most two decimals and is not over the limit of the money type (04 7.3.2.8; K-1214).
        if (value is { } amount && (amount < 0m || amount > Money.MaxAmount || HasMoreThanTwoDecimals(amount)))
        {
            rejections.Add(new FieldRejection(field, FieldCodes.InvalidAmount));
        }
    }

    private static void CheckFenced(List<FieldRejection> rejections, string field, string parameterId, int? value)
    {
        if (value is not { } number)
        {
            rejections.Add(new FieldRejection(field, FieldCodes.FieldRequired));
            return;
        }

        PeriodFences.Verdict verdict = FirmSettingFences.Check(parameterId, number);
        if (!verdict.IsWithin)
        {
            rejections.Add(new FieldRejection(field, nameof(ErrorCode.ValueOutOfBounds), verdict.Side, verdict.Fence));
        }
    }

    private static bool HasMoreThanTwoDecimals(decimal value) => value * 100m % 1m != 0m;

    private static void Track(List<AuditValueChange> changes, string field, decimal? oldValue, decimal? newValue)
    {
        if (oldValue != newValue)
        {
            changes.Add(new AuditValueChange(field, Text(oldValue), Text(newValue)));
        }
    }

    private static void Track(List<AuditValueChange> changes, string field, int oldValue, int newValue)
    {
        if (oldValue != newValue)
        {
            changes.Add(new AuditValueChange(field, oldValue.ToString(CultureInfo.InvariantCulture), newValue.ToString(CultureInfo.InvariantCulture)));
        }
    }

    private static string? Text(decimal? value) => value?.ToString("0.00", CultureInfo.InvariantCulture);
}

/// <summary>The result of a section's save and whether a value that feeds the two generated texts really changed (02 3.33.3; K-1215): only then does the caller write their new versions.</summary>
internal sealed record SectionSaveOutcome(SettingsSaveResult Result, bool FeedsGeneratedTexts);
