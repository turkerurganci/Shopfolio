using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Shopfolio.Infrastructure;

/// <summary>
/// Lets `dotnet ef migrations bundle` create the context without running the application (09 10.6 item 1).
/// The connection string is a credential-free placeholder: the bundle receives the real connection string
/// when it is run, and nothing is ever opened at design time.
/// </summary>
internal sealed class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, "Host=localhost;Database=shopfolio_design_time");
        return new AppDbContext(builder.Options);
    }
}
