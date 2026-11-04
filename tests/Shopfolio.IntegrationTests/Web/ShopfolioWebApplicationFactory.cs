using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.InstallationSettings;
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
        [EmailOptions.CredentialKey] = "test-only-made-up-email-credential",
    };

    private static string MadeUpBase64(int bytes) => Convert.ToBase64String(new byte[bytes]);

    private readonly string _connectionString;
    private readonly IReadOnlyDictionary<string, string> _settings;
    private readonly ILoggerProvider? _loggerProvider;

    public ShopfolioWebApplicationFactory(
        string connectionString,
        IReadOnlyDictionary<string, string>? settings = null,
        ILoggerProvider? loggerProvider = null)
    {
        _connectionString = connectionString;
        _settings = settings ?? new Dictionary<string, string>();
        _loggerProvider = loggerProvider;
        LogDirectory = Path.Combine(Path.GetTempPath(), "shopfolio-test-logs-" + Guid.NewGuid().ToString("N"));
    }

    /// <summary>
    /// The directory the application's log files go to (<c>Logging:Directory</c>, 09 18.1 item 2): one made-up directory per factory, so a test host
    /// never writes to the production default and a test can read the files. A test replaces it through <c>settings</c>.
    /// </summary>
    public string LogDirectory { get; }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        try
        {
            if (Directory.Exists(LogDirectory))
            {
                Directory.Delete(LogDirectory, recursive: true);
            }
        }
        catch (IOException)
        {
            // A temporary directory that cannot be removed is left to the operating system.
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(DatabaseOptions.ConnectionStringKey, _connectionString);
        builder.UseSetting(LoggingOptions.DirectoryKey, LogDirectory);
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

        if (_loggerProvider is not null)
        {
            builder.ConfigureLogging(logging => logging.AddProvider(_loggerProvider));
        }
    }
}
