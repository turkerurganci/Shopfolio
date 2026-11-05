using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>The Settings module's implementation of its open surface; internal, reached only through <see cref="ISettingsModule"/>.</summary>
internal sealed class SettingsModule : ISettingsModule
{
}

/// <summary>The module's single registration (09 6 item 3): <c>Program.cs</c> calls <c>AddSettingsModule</c> once.</summary>
internal static class SettingsModuleRegistration
{
    public static IServiceCollection AddSettingsModule(this IServiceCollection services)
    {
        services.AddScoped<ISettingsModule, SettingsModule>();
        services.AddScoped<Shopfolio.Infrastructure.Security.ICookiePolicyPublication, CookiePolicyPublication>();
        return services;
    }
}
