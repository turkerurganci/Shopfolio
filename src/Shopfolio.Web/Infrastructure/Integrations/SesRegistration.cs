using Amazon.SimpleEmailV2;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.InstallationSettings;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// Registers the e-mail service's gateway (09 11.2 item 1): the one client of the application - the client is thread safe and lives as long as the application -
/// made from the installation settings when it is first asked for, and <see cref="SesEmailGateway"/> as the application's <see cref="IEmailGateway"/>. The caller
/// registers it only where the development capture tool (09 4.3 item 2) is not set, so a development machine never makes the client and no e-mail leaves it.
/// </summary>
internal static class SesRegistration
{
    public static IServiceCollection AddEmailSendingService(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IAmazonSimpleEmailServiceV2>(provider =>
            SesClientFactory.Create(provider.GetRequiredService<IOptions<EmailOptions>>().Value));
        services.AddSingleton<IEmailGateway, SesEmailGateway>();
        return services;
    }
}
