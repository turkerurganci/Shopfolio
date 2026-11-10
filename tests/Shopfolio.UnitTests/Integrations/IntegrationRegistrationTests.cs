using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.Integrations;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The registration of the link layer (T202 criterion 9, KK9; 09 4.3 item 2, 19.1 item 7): the development setting <c>EmailCapture:Host</c>
/// selects the capture gateway as the application's e-mail gateway; without it no e-mail gateway of the layer is registered; a set host without
/// a port is refused at start.
/// </summary>
public sealed class IntegrationRegistrationTests
{
    private static ServiceProvider Provider(Dictionary<string, string?> settings, out ServiceCollection services)
    {
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddLogging();
        services.AddSingleton(TimeProvider.System);
        services.AddIntegrations(configuration);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void WithACaptureHost_TheEmailGatewayIsTheCaptureGateway()
    {
        using ServiceProvider provider = Provider(
            new() { [EmailCaptureOptions.HostKey] = "mailpit", [EmailCaptureOptions.PortKey] = "1025" }, out ServiceCollection services);

        ServiceDescriptor descriptor = Assert.Single(services, d => d.ServiceType == typeof(IEmailGateway));
        Assert.Equal(typeof(SmtpCaptureEmailGateway), descriptor.ImplementationType);
        EmailCaptureOptions options = provider.GetRequiredService<IOptions<EmailCaptureOptions>>().Value;
        Assert.Equal("mailpit", options.Host);
        Assert.Equal(1025, options.Port);
    }

    [Fact]
    public void WithoutACaptureHost_TheEmailGatewayIsTheServicesGateway_AndTheCaptureSettingIsNotChecked()
    {
        using ServiceProvider provider = Provider([], out ServiceCollection services);

        ServiceDescriptor descriptor = Assert.Single(services, d => d.ServiceType == typeof(IEmailGateway));
        Assert.Equal(typeof(SesEmailGateway), descriptor.ImplementationType);
        Assert.DoesNotContain(services, d => d.ServiceType == typeof(ISmtpTransport));
        Assert.Equal(string.Empty, provider.GetRequiredService<IOptions<EmailCaptureOptions>>().Value.Host);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("0")]
    [InlineData("65536")]
    public void ACaptureHostWithoutAValidPort_IsRefused_NamingThePortKey(string? port)
    {
        using ServiceProvider provider = Provider(new() { [EmailCaptureOptions.HostKey] = "mailpit", [EmailCaptureOptions.PortKey] = port }, out _);

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<EmailCaptureOptions>>().Value);
        Assert.Contains(EmailCaptureOptions.PortKey, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCommonRunnerAndTheNamedClients_AreRegistered()
    {
        using ServiceProvider provider = Provider([], out ServiceCollection services);

        Assert.Equal(ServiceLifetime.Singleton, Assert.Single(services, d => d.ServiceType == typeof(ExternalCallRunner)).Lifetime);
        Assert.Same(provider.GetRequiredService<ExternalCallRunner>(), provider.GetRequiredService<ExternalCallRunner>());
        IHttpClientFactory factory = provider.GetRequiredService<IHttpClientFactory>();
        Assert.All(IntegrationHttpClients.Names, name =>
        {
            using HttpClient client = factory.CreateClient(name);
            Assert.Equal(Timeout.InfiniteTimeSpan, client.Timeout);
        });
    }
}
