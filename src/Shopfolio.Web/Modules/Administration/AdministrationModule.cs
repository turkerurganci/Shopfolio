using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Modules.Administration.Contracts;

namespace Shopfolio.Modules.Administration;

/// <summary>The Administration module's implementation of its open surface; internal, reached only through <see cref="IAdministrationModule"/>.</summary>
internal sealed class AdministrationModule : IAdministrationModule
{
}

/// <summary>The module's single registration (09 6 item 3): <c>Program.cs</c> calls <c>AddAdministrationModule</c> once.</summary>
internal static class AdministrationModuleRegistration
{
    public static IServiceCollection AddAdministrationModule(this IServiceCollection services)
    {
        services.AddScoped<IAdministrationModule, AdministrationModule>();
        services.AddScoped<IAuditActorSource, RequestAuditActorSource>();
        services.AddScoped<IAuditTrail, AuditTrail>();
        services.AddDisposalPart<AdminInvitationDisposalPart>();
        return services;
    }
}
