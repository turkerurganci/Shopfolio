using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Status;

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

        // The e-mail common pattern (T305; 08 2.2.5; 09 11.4): the addresses and the composer that makes the subject, the HTML and the plain text part.
        services.AddSingleton<EmailLinks>();
        services.AddSingleton<EmailComposer>();

        // The per-recipient sending limit asked at a row's first attempt (08 2.2.4); its thinned security line keeps one window in memory.
        services.AddScoped<RecipientLimitCheck>();
        services.AddSingleton<RecipientLimitLog>();

        // The e-mail channel warning is a condition of the status endpoint (05 9.4.7).
        services.AddStatusCondition<ChannelWarningCondition>();
        return services;
    }
}
