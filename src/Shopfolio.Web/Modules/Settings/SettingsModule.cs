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

    public SettingsModule(CompanyIdentities identities, CompanySettingsWriter writer, SalesGates gates, SalesSwitches switches)
    {
        _identities = identities;
        _writer = writer;
        _gates = gates;
        _switches = switches;
    }

    public Task<SalesGate> GetSalesGateAsync(CancellationToken ct) => _gates.GetAsync(ct);

    public Task<SalesSwitchResult> CloseSalesTemporarilyAsync(bool shownIsClosed, CancellationToken ct) => _switches.CloseAsync(shownIsClosed, ct);

    public Task<SalesSwitchResult> ReopenSalesAsync(bool shownIsClosed, CancellationToken ct) => _switches.ReopenAsync(shownIsClosed, ct);

    public Task<CompanyIdentity> GetCompanyIdentityAsync(CancellationToken ct) => _identities.GetAsync(ct);

    public Task<CompanyIdentityCompleteness> GetCompanyIdentityCompletenessAsync(CancellationToken ct) => _identities.GetCompletenessAsync(ct);

    public Task<LegalIdentity> GetLegalIdentityAsync(CancellationToken ct) => _identities.GetLegalAsync(ct);

    public Task<SettingsSaveResult> SaveCompanyIdentityAsync(CompanyIdentityInput input, int formVersion, CancellationToken ct) =>
        _identities.SaveAsync(input, formVersion, ct);

    public Task<ReturnAddressSection> GetReturnAddressAsync(CancellationToken ct) => _writer.ReadReturnAddressAsync(ct);

    public Task<SettingsSaveResult> SaveReturnAddressAsync(ReturnAddressInput input, int formVersion, CancellationToken ct) =>
        _writer.SaveReturnAddressAsync(input, formVersion, ct);
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

        // The three outside conditions of the sales gate are unbound until their tasks register an implementation in place of these (T352, T353, T366): unbound is not met.
        services.TryAddScoped<ICardPaymentCondition, UnboundCardPaymentCondition>();
        services.TryAddScoped<IBankTransferCondition, UnboundBankTransferCondition>();
        services.TryAddScoped<IPrivacyNoticeCondition, UnboundPrivacyNoticeCondition>();
        services.AddScoped<ISettingsModule, SettingsModule>();
        services.AddScoped<ISitemapSource, SitemapSource>();
        services.AddScoped<Shopfolio.Infrastructure.Security.ICookiePolicyPublication, CookiePolicyPublication>();
        return services;
    }
}
