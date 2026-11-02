using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Shopfolio.Infrastructure;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// Opens the application with the database of the test collection (09 19.1 item 5): whatever database context registration the
/// application carries is replaced by one that points at the collection's database.
/// </summary>
public sealed class ShopfolioWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public ShopfolioWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _connectionString));
        });
    }
}
