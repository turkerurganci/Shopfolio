using Microsoft.EntityFrameworkCore;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The single database context (09 4.1 item 2, 5.2). It carries no entity yet: the migration bundle needs one context
/// from the start (T03); the model rules and the first entities arrive in T101.
/// </summary>
internal sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// The one place where the provider and the naming convention are set up (09 5.2 item 1).
    /// </summary>
    public static DbContextOptionsBuilder Configure(DbContextOptionsBuilder builder, string connectionString)
    {
        return builder
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();
    }
}
