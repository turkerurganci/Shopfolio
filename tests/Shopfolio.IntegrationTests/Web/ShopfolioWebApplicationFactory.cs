using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

using Shopfolio.Infrastructure;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// Opens the application with the database of the test collection (09 19.1 item 5): the factory gives the application's own
/// connection setting (<c>Database:ConnectionString</c>) the collection database's connection string, so the application
/// registers its database context and data source the way it does in production.
/// </summary>
public sealed class ShopfolioWebApplicationFactory : WebApplicationFactory<Program>
{
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
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(DatabaseOptions.ConnectionStringKey, _connectionString);
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
