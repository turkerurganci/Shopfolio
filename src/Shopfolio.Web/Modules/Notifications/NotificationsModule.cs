using Microsoft.Extensions.DependencyInjection;

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
        return services;
    }
}
