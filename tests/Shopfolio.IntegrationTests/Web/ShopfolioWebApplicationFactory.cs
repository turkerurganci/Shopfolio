using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog.Core;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// Opens the application with the database of the test collection (09 19.1 item 5): the factory gives the application's own
/// connection setting (<c>Database:ConnectionString</c>) the collection database's connection string, so the application
/// registers its database context and data source the way it does in production. The other required settings (09 3.5 item 2, 4.3 item 2)
/// get made-up test values that belong to no installation; a test replaces one through <c>settings</c>, and an empty value is a missing one.
/// </summary>
public sealed class ShopfolioWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>Made-up values of the required settings other than the database connection; 32 bytes of Base64 for the link secret.</summary>
    public static readonly IReadOnlyDictionary<string, string> RequiredSettings = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        [SiteOptions.DomainKey] = "localhost",
        [EmailOptions.SenderAddressKey] = "test@example.com",
        [EmailOptions.RegionKey] = "eu-central-1",
        [EmailOptions.CredentialKey] = "test-only-made-up-key-id\ntest-only-made-up-secret-key",
    };

    private static string MadeUpBase64(int bytes) => Convert.ToBase64String(new byte[bytes]);

    private readonly string _connectionString;
    private readonly IReadOnlyDictionary<string, string> _settings;
    private readonly ILoggerProvider? _loggerProvider;
    private readonly bool _withJobRunner;
    private readonly bool _withScheduler;
    private readonly bool _keepEmailGateway;

    public ShopfolioWebApplicationFactory(
        string connectionString,
        IReadOnlyDictionary<string, string>? settings = null,
        ILoggerProvider? loggerProvider = null,
        bool withJobRunner = true,
        bool withScheduler = false,
        bool keepEmailGateway = false)
    {
        _keepEmailGateway = keepEmailGateway;
        _withScheduler = withScheduler;
        _connectionString = connectionString;
        _settings = settings ?? new Dictionary<string, string>();
        _loggerProvider = loggerProvider;
        _withJobRunner = withJobRunner;
        LogDirectory = Path.Combine(Path.GetTempPath(), "shopfolio-test-logs-" + Guid.NewGuid().ToString("N"));
        ExportDirectory = Path.Combine(Path.GetTempPath(), "shopfolio-test-export-" + Guid.NewGuid().ToString("N"));
    }

    /// <summary>
    /// The export temporary directory (<c>Export:TempDirectory</c>): one made-up directory per factory. The start-up sweep empties the directory,
    /// so two hosts sharing the default one delete each other's probe file (D-980). A test replaces it through <c>settings</c>.
    /// </summary>
    public string ExportDirectory { get; }

    /// <summary>
    /// The directory the application's log files go to (<c>Logging:Directory</c>, 09 18.1 item 2): one made-up directory per factory, so a test host
    /// never writes to the production default and a test can read the files. A test replaces it through <c>settings</c>.
    /// </summary>
    public string LogDirectory { get; }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        foreach (string directory in new[] { LogDirectory, ExportDirectory })
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: true);
                }
            }
            catch (IOException)
            {
                // A temporary directory that cannot be removed is left to the operating system.
            }
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(DatabaseOptions.ConnectionStringKey, _connectionString);
        builder.UseSetting(LoggingOptions.DirectoryKey, LogDirectory);
        builder.UseSetting(Shopfolio.Infrastructure.LogRead.LogExportFile.DirectoryKey, ExportDirectory);
        foreach (KeyValuePair<string, string> setting in RequiredSettings)
        {
            builder.UseSetting(setting.Key, setting.Value);
        }

        // Built at run time (32 bytes, Base64): the repository's secret guard stops a secret-looking literal next to a secret key.
        builder.UseSetting(LinkKeyOptions.DerivationSecretKey, MadeUpBase64(32));
        foreach (KeyValuePair<string, string> setting in _settings)
        {
            builder.UseSetting(setting.Key, setting.Value);
        }

        if (!_withJobRunner)
        {
            // A test that counts the pooled connections (a pool of one) cannot share it with the runner's session lock connection.
            builder.ConfigureServices(services =>
            {
                ServiceDescriptor runner = services.Single(d => d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(JobRunnerService));
                services.Remove(runner);
            });
        }

        if (!_keepEmailGateway)
        {
            // The application registers the e-mail service's gateway (T407); a test host never reaches the service, so with the job runner on, a queued
            // row waits as it did before the gateway existed. A test about the registration asks to keep it.
            builder.ConfigureServices(services => services.RemoveAll<IEmailGateway>());
        }

        if (!_withScheduler)
        {
            // The scheduler beats against the site's public address, which a test host does not have; a test about the scheduler asks for it.
            builder.ConfigureServices(services =>
            {
                ServiceDescriptor scheduler = services.Single(d => d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(SchedulerService));
                services.Remove(scheduler);
            });
        }

        if (_loggerProvider is not null)
        {
            // Not a provider of the host: the host does not forward its lines to the framework's providers (05 9.2.3); the sink is read by the logging setup.
            builder.ConfigureServices(services => services.AddSingleton<ILogEventSink>(new ProviderLogEventSink(_loggerProvider)));
        }
    }
}
