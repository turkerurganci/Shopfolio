using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>The Settings module's implementation of its open surface; internal, reached only through <see cref="ISettingsModule"/>.</summary>
internal sealed class SettingsModule : ISettingsModule
{
    private readonly CompanyIdentities _identities;
    private readonly CompanySettingsWriter _writer;

    private readonly SalesGates _gates;
    private readonly SalesSwitches _switches;
    private readonly FirmSettings _firm;
    private readonly LegalTexts _legalTexts;
    private readonly LegalTextGenerator _generator;
    private readonly ProvinceList _provinces;
    private readonly DeliveryProvinces _delivery;

    public SettingsModule(
        CompanyIdentities identities,
        CompanySettingsWriter writer,
        SalesGates gates,
        SalesSwitches switches,
        FirmSettings firm,
        LegalTexts legalTexts,
        LegalTextGenerator generator,
        ProvinceList provinces,
        DeliveryProvinces delivery)
    {
        _provinces = provinces;
        _delivery = delivery;
        _identities = identities;
        _writer = writer;
        _gates = gates;
        _switches = switches;
        _firm = firm;
        _legalTexts = legalTexts;
        _generator = generator;
    }

    public Task<SalesGate> GetSalesGateAsync(CancellationToken ct) => _gates.GetAsync(ct);

    public Task<DateTimeOffset?> FindSalesFirstOpenedAtAsync(CancellationToken ct) => _gates.FindFirstOpenedAtAsync(ct);

    public Task<SalesSwitchResult> CloseSalesTemporarilyAsync(bool shownIsClosed, CancellationToken ct) => _switches.CloseAsync(shownIsClosed, ct);

    public Task<SalesSwitchResult> ReopenSalesAsync(bool shownIsClosed, CancellationToken ct) => _switches.ReopenAsync(shownIsClosed, ct);

    public Task<CompanyIdentity> GetCompanyIdentityAsync(CancellationToken ct) => _identities.GetAsync(ct);

    public Task<CompanyIdentityCompleteness> GetCompanyIdentityCompletenessAsync(CancellationToken ct) => _identities.GetCompletenessAsync(ct);

    public Task<LegalIdentity> GetLegalIdentityAsync(CancellationToken ct) => _identities.GetLegalAsync(ct);

    public async Task<SettingsSaveResult> SaveCompanyIdentityAsync(CompanyIdentityInput input, int formVersion, CancellationToken ct)
    {
        SettingsSaveResult result = await _identities.SaveAsync(input, formVersion, ct);
        if (result.IsSaved)
        {
            // The identity feeds the two generated texts (02 3.33.3): the new versions are made under the row lock this save still holds (09 11.4 item 6; K-1088).
            IReadOnlyList<GeneratedVersion> versions = await _generator.RegenerateAsync(ct);
            await _generator.WriteTrailAsync(versions, ct);
        }

        return result;
    }

    public Task<ReturnAddressSection> GetReturnAddressAsync(CancellationToken ct) => _writer.ReadReturnAddressAsync(ct);

    public async Task<IReadOnlyList<FieldRejection>> CheckReturnAddressAsync(ReturnAddressInput input, CancellationToken ct) =>
        await _writer.CheckReturnAddressAsync(input, await _writer.ReadReturnAddressAsync(ct), ct);

    public Task<ShippingFeeSection> GetShippingFeeAsync(CancellationToken ct) => _firm.ReadShippingFeeAsync(ct);

    public Task<HomepageSection> GetHomepageAsync(CancellationToken ct) => _firm.ReadHomepageAsync(ct);

    public Task<SettingsSaveResult> SaveHomepageAsync(HomepageInput input, int formVersion, CancellationToken ct) => _firm.SaveHomepageAsync(input, formVersion, ct);

    public async Task<SettingsSaveResult> SaveShippingFeeAsync(ShippingFeeInput input, int formVersion, CancellationToken ct) =>
        await RegenerateWhenFedAsync(await _firm.SaveShippingFeeAsync(input, formVersion, ct), ct);

    /// <summary>
    /// After a section's save that changed a value feeding the two generated texts - the shipping fee, the firm default of the shipping period, the delivery provinces - their new versions are
    /// written under the row lock the save still holds, and the trail rows of those versions with them (T536; 02 3.33.3, 10.3.1 (5); 03 8.7.2.3; 04 9.18.9; D-681, D-493). A save that changed
    /// no feeding value writes no version (K-1215).
    /// </summary>
    private async Task<SettingsSaveResult> RegenerateWhenFedAsync(SectionSaveOutcome outcome, CancellationToken ct)
    {
        if (outcome.Result.IsSaved && outcome.FeedsGeneratedTexts)
        {
            IReadOnlyList<GeneratedVersion> versions = await _generator.RegenerateAsync(feedingSettingChanged: true, ct);
            await _generator.WriteTrailAsync(versions, ct);
        }

        return outcome.Result;
    }

    public Task<PeriodsSection> GetPeriodsAsync(CancellationToken ct) => _firm.ReadPeriodsAsync(ct);

    public Task<string?> GetBankTransferIbanAsync(CancellationToken ct) => _firm.ReadBankTransferIbanAsync(ct);

    public Task<string?> GetWhatsAppNumberAsync(CancellationToken ct) => _firm.ReadWhatsAppNumberAsync(ct);

    public async Task<SettingsSaveResult> SavePeriodsAsync(PeriodsInput input, int formVersion, CancellationToken ct) =>
        await RegenerateWhenFedAsync(await _firm.SavePeriodsAsync(input, formVersion, ct), ct);

    public Task<ProductDefaultsSection> GetProductDefaultsAsync(CancellationToken ct) => _firm.ReadProductDefaultsAsync(ct);

    public Task<SettingsSaveResult> SaveProductDefaultsAsync(ProductDefaultsInput input, int formVersion, CancellationToken ct) =>
        _firm.SaveProductDefaultsAsync(input, formVersion, ct);

    public async Task<SettingsSaveResult> SaveReturnAddressAsync(ReturnAddressInput input, int formVersion, CancellationToken ct)
    {
        SettingsSaveResult result = await _firm.SaveReturnAddressAsync(input, formVersion, ct);
        if (result.IsSaved)
        {
            // The return address is written into both generated texts (02 7.4.4): same order as the identity's - a version only when the text changed, with its trail row (D-220).
            IReadOnlyList<GeneratedVersion> versions = await _generator.RegenerateAsync(ct);
            await _generator.WriteTrailAsync(versions, ct);
        }

        return result;
    }

    public Task<IReadOnlyList<LegalTextSummary>> GetLegalTextsAsync(CancellationToken ct) => _legalTexts.GetSummariesAsync(ct);

    public Task<LegalTextDraft> GetLegalTextDraftAsync(LegalTextType type, CancellationToken ct) => _legalTexts.GetDraftAsync(type, ct);

    public Task<LegalTextDraftPreview> GetLegalTextDraftPreviewAsync(LegalTextType type, CancellationToken ct) => _legalTexts.GetDraftPreviewAsync(type, ct);

    public Task<SettingsSaveResult> SaveLegalTextDraftAsync(LegalTextType type, string text, string? hostingLocation, int formVersion, CancellationToken ct) =>
        _legalTexts.SaveDraftAsync(type, text, hostingLocation, formVersion, ct);

    public Task<LegalTextPublishResult> PublishLegalTextAsync(LegalTextType type, int formVersion, CancellationToken ct) => _legalTexts.PublishAsync(type, formVersion, ct);

    public Task<LegalTextVersionIds> GetCurrentLegalTextVersionsAsync(CancellationToken ct) => _legalTexts.GetCurrentVersionIdsAsync(ct);

    public Task<bool> AreShownLegalTextVersionsCurrentAsync(Guid shownPreInformationFormId, Guid shownDistanceSalesContractId, CancellationToken ct) =>
        _legalTexts.AreShownCurrentAsync(shownPreInformationFormId, shownDistanceSalesContractId, ct);

    public Task<LegalTextVersionInfo?> GetLegalTextVersionAsync(Guid versionId, CancellationToken ct) => _legalTexts.GetVersionAsync(versionId, ct);

    public Task<LegalTextVersionInfo?> GetLatestLegalTextVersionAsync(LegalTextType type, CancellationToken ct) => _legalTexts.GetLatestVersionAsync(type, ct);

    public async Task SynchronizeGeneratedLegalTextsAsync(CancellationToken ct) => await _generator.RegenerateAsync(ct);

    public Task<IReadOnlyList<ProvinceInfo>> GetProvincesAsync(CancellationToken ct) => _provinces.GetAllAsync(ct);

    public Task<DeliveryProvincesSection> GetDeliveryProvincesAsync(CancellationToken ct) => _delivery.ReadAsync(ct);

    // The delivery restriction belongs to the pre-information form (02 3.24.3, 3.33.3): a changed selection writes the texts' new versions under the row lock this save still holds (D-493).
    public async Task<SettingsSaveResult> SaveDeliveryProvincesAsync(IReadOnlyCollection<Guid> provinceIds, int formVersion, CancellationToken ct) =>
        await RegenerateWhenFedAsync(await _delivery.SaveAsync(provinceIds, formVersion, ct), ct);

    public Task<DeliveryArea> GetDeliveryAreaAsync(CancellationToken ct) => _delivery.GetAreaAsync(ct);
}

/// <summary>The module's single registration (09 6 item 3): <c>Program.cs</c> calls <c>AddSettingsModule</c> once.</summary>
internal static class SettingsModuleRegistration
{
    public static IServiceCollection AddSettingsModule(this IServiceCollection services)
    {
        services.AddScoped<CompanySettingsWriter>();
        services.AddScoped<CompanyIdentities>();
        services.AddScoped<SalesGates>();
        services.AddScoped<SalesSwitches>();
        services.AddScoped<FirmSettings>();
        services.AddScoped<LegalTexts>();
        services.AddScoped<LegalTextGenerator>();
        services.AddScoped<ProvinceList>();
        services.AddScoped<DeliveryProvinces>();

        // The privacy notice condition (T366) and the transfer line (T353: BankTransferCondition, switched on with its IBAN) are bound here. The card line is bound by the
        // Payment module's registration (T352: CardPaymentCondition); the card default below stands only where the Payment module is not registered: unbound is not met.
        services.TryAddScoped<IPrivacyNoticeCondition, PrivacyNoticeCondition>();
        services.TryAddScoped<ICardPaymentCondition, UnboundCardPaymentCondition>();
        services.TryAddScoped<IBankTransferCondition, BankTransferCondition>();
        services.AddScoped<IPaymentMethodSettings, PaymentMethodSettings>();
        services.AddScoped<ISettingsModule, SettingsModule>();
        services.AddScoped<ISitemapSource, SitemapSource>();
        services.AddScoped<IProvinceList, ProvinceList>();
        services.AddScoped<ICountryList, CountryList>();
        services.AddScoped<Shopfolio.Infrastructure.Security.ICookiePolicyPublication, CookiePolicyPublication>();
        services.AddHostedService<GeneratedLegalTextsStartup>();
        return services;
    }
}
