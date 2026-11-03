using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The single database context (09 4.1 item 2, 5.2). It carries no entity yet: the entities arrive with the tasks that own them
/// (T103 and on), each with its <c>DbSet</c> property (09 5.2 item 2) and its entity configuration in the module's folder; the context
/// applies those configurations and does no other work. A module never opens a context of its own.
/// </summary>
internal sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// The one place where the provider, the naming convention and the save interceptors are set up (09 5.2 item 1; 10.1 item 2).
    /// </summary>
    public static DbContextOptionsBuilder Configure(DbContextOptionsBuilder builder, string connectionString)
    {
        return builder
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(EmptyKeyGuard.Instance, EmptyTextGuard.Instance);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);
        configurationBuilder.Conventions.Add(_ => new ModelRulesConvention());

        // Every enum of the application is stored as its code name (06 2.9.1; 09 10.1 item 3). The conversion is made here once;
        // an enum property never writes its own. The audit operation is the one exception and is mapped by the audit trail itself.
        foreach (Type enumType in typeof(EntityIds).Assembly.GetTypes().Where(t => t.IsEnum && t.FullName != ModelRulesConvention.AuditOperationTypeName))
        {
            configurationBuilder.Properties(enumType).HaveConversion(typeof(EnumTextConverter<>).MakeGenericType(enumType));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
