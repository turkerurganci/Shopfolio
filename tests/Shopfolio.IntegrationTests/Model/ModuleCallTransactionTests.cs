using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.CorporateContent;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the module call transaction tests (T218): one database copied from the template.</summary>
[CollectionDefinition(ModuleCallTransactionDefinition.Name)]
public sealed class ModuleCallTransactionDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "ModuleCallTransaction";
}

/// <summary>
/// A module-to-module call runs synchronously inside the caller's database transaction (T218 criterion 5, KK5; 05 3.4.2; 09 6 item 3): the
/// callee takes the scope's one context, opens no transaction and saves nothing, so the caller's commit makes both writes and the caller's
/// roll back makes neither. The caller and the callee are test-local stand-ins for two modules' open surfaces; the real surfaces arrive with the
/// tasks that own the rules.
/// </summary>
[Collection(ModuleCallTransactionDefinition.Name)]
public sealed class ModuleCallTransactionTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 12, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    public ModuleCallTransactionTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task CallerCommit_MakesTheCallersAndTheCalleesWrite()
    {
        Guid faqId = Guid.Empty;
        Guid categoryId = Guid.Empty;
        await using ServiceProvider provider = BuildProvider();
        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(Ct);

            (faqId, categoryId) = scope.ServiceProvider.GetRequiredService<SampleCaller>().Run(Now);
            await context.SaveChangesAsync(Ct);
            await transaction.CommitAsync(Ct);
        }

        await using AppDbContext reader = NewContext();
        Assert.True(await reader.FaqItems.AnyAsync(f => f.Id == faqId, Ct));
        Assert.True(await reader.Categories.AnyAsync(c => c.Id == categoryId, Ct));
    }

    [Fact]
    public async Task CallerRollBack_UndoesTheCallersAndTheCalleesWriteTogether()
    {
        Guid faqId;
        Guid categoryId;
        await using ServiceProvider provider = BuildProvider();
        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(Ct);

            (faqId, categoryId) = scope.ServiceProvider.GetRequiredService<SampleCaller>().Run(Now);
            await context.SaveChangesAsync(Ct);
            await transaction.RollbackAsync(Ct);
        }

        await using AppDbContext reader = NewContext();
        Assert.False(await reader.FaqItems.AnyAsync(f => f.Id == faqId, Ct));
        Assert.False(await reader.Categories.AnyAsync(c => c.Id == categoryId, Ct));
    }

    [Fact]
    public async Task CalleeAlone_WritesNothingUntilTheCallersSave()
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();

        Guid categoryId = scope.ServiceProvider.GetRequiredService<ISampleCallee>().AddCategory(Now);

        await using AppDbContext reader = NewContext();
        Assert.False(await reader.Categories.AnyAsync(c => c.Id == categoryId, Ct));
    }

    private ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddScoped<ISampleCallee, SampleCallee>();
        services.AddScoped<SampleCaller>();
        return services.BuildServiceProvider();
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    /// <summary>Stand-in for the callee module's open interface: joins the caller's transaction, never opens or commits one.</summary>
    private interface ISampleCallee
    {
        Guid AddCategory(DateTimeOffset now);
    }

    private sealed class SampleCallee : ISampleCallee
    {
        private readonly AppDbContext _context;

        public SampleCallee(AppDbContext context)
        {
            _context = context;
        }

        public Guid AddCategory(DateTimeOffset now)
        {
            Category category = Category.Create("Kapılar", "kapilar-" + Guid.NewGuid().ToString("N"), null, 1, now);
            _context.Categories.Add(category);
            return category.Id;
        }
    }

    /// <summary>Stand-in for the caller module's transition unit: its own write and the callee's call, both left to the caller's commit.</summary>
    private sealed class SampleCaller
    {
        private readonly AppDbContext _context;
        private readonly ISampleCallee _callee;

        public SampleCaller(AppDbContext context, ISampleCallee callee)
        {
            _context = context;
            _callee = callee;
        }

        public (Guid FaqId, Guid CategoryId) Run(DateTimeOffset now)
        {
            FaqItem faq = FaqItem.Create("Soru?", 1, now);
            _context.FaqItems.Add(faq);
            Guid categoryId = _callee.AddCategory(now);
            return (faq.Id, categoryId);
        }
    }
}
