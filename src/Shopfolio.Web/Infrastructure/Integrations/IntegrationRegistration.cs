using Microsoft.Extensions.Options;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// Registers the link layer of the outside services (09 11.1, 11.2): the common runner, the named HTTP clients and, in development, the e-mail
/// gateway that writes to the capture tool (09 4.3 item 2); without the capture setting the e-mail service's gateway is registered (T407). Google's gateway is
/// registered always (T409). The payment provider's implementation is registered here by the task that builds it; a module takes only the interface.
/// </summary>
internal static class IntegrationRegistration
{
    public static IServiceCollection AddIntegrations(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton<ExternalCallRunner>();
        services.AddIntegrationHttpClients();

        services.AddOptions<EmailCaptureOptions>().BindConfiguration(EmailCaptureOptions.SectionName).ValidateOnStart();
        services.AddSingleton<IValidateOptions<EmailCaptureOptions>, EmailCaptureOptionsValidator>();
        if (!string.IsNullOrWhiteSpace(configuration[EmailCaptureOptions.HostKey]))
        {
            services.AddSingleton<ISmtpTransport, SmtpTransport>();
            services.AddSingleton<IEmailGateway, SmtpCaptureEmailGateway>();
        }
        else
        {
            // Everywhere else the e-mail service's gateway is the application's (08 2.2; 09 11.2 item 1).
            services.AddEmailSendingService();
        }

        services.AddGoogleSignIn();

        return services;
    }
}
