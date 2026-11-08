using System.Security.Cryptography;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Settings;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>The transition tests write no audited setting change, so no administrator is needed; only the Settings module's switch asks for the source.</summary>
internal sealed class NoAuditActor : IAuditActorSource
{
    public Task<AuditActor?> CurrentAsync(CancellationToken ct) => Task.FromResult<AuditActor?>(null);
}

/// <summary>What the transition unit's notification queue needs around it: the Settings module with its trail and the link keys of the account e-mails.</summary>
internal static class TransitionTestServices
{
    public static IServiceCollection AddNoticeDependencies(this IServiceCollection services)
    {
        services.AddSettingsDependencies();
        services.AddSingleton(new LinkKeys(Options.Create(new LinkKeyOptions { DerivationSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(LinkKeyOptions.DerivationSecretMinBytes)) })));
        services.AddNotificationsModule();
        return services;
    }

    /// <summary>The Settings module with its trail: the AfterSales writers read the firm's return address from it (T320; 06 4.25, 4.30).</summary>
    public static IServiceCollection AddSettingsDependencies(this IServiceCollection services)
    {
        services.AddScoped<IAuditActorSource, NoAuditActor>();
        services.AddScoped<Shopfolio.Modules.Administration.Contracts.IAuditTrail, AuditTrail>();
        services.AddSettingsModule();
        return services;
    }
}
