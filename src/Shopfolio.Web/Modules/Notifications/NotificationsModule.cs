using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Jobs;

using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>The Notifications module's implementation of its open surface; internal, reached only through <see cref="INotificationsModule"/>.</summary>
internal sealed class NotificationsModule : INotificationsModule
{
}

/// <summary>The module's single registration (09 6 item 3): <c>Program.cs</c> calls <c>AddNotificationsModule</c> once.</summary>
internal static class NotificationsModuleRegistration
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
    {
        services.AddScoped<INotificationsModule, NotificationsModule>();

        // The start-up rewrite of the pending revert links' digests (05 6.x.20): hosted services start in registration order, so it is added
        // here, after the start-up checks, and the job queue and the e-mail sending are registered after it.
        services.AddScoped<RevertLinkDigestRewrite>();
        services.AddSingleton<RevertLinkDigestRewriteStartup>();
        services.AddHostedService(provider => provider.GetRequiredService<RevertLinkDigestRewriteStartup>());

        // The job queue takes no row before the step has run (D-96 K1; 09 15.1 item 4).
        services.AddJobStartGate<RevertLinkDigestRewriteGate>();
        return services;
    }
}
