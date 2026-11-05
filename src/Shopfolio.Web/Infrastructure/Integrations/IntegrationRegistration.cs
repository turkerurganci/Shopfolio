using Microsoft.Extensions.Options;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// Registers the link layer of the outside services (09 11.1, 11.2): the common runner, the named HTTP clients and, in development, the e-mail
/// gateway that writes to the capture tool (09 4.3 item 2). The provider implementations of the three interfaces are registered here by the
/// tasks that build them (the payment provider, the e-mail service and Google tasks); a module takes only the interface.
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

        return services;
    }
}
