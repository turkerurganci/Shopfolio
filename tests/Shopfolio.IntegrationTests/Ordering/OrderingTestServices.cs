using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Payment;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The services of the transition unit's tests (T315, T317): the database context and the transaction runner, the job queue, the modules the unit
/// calls - After-sales, Payment - and the Notifications module's writers of the dispatch rows. The firm's contact e-mail is <see cref="CompanyEmail"/>.
/// </summary>
internal static class OrderingTestServices
{
    public const string CompanyEmail = "firma-iletisim@example.com";

    public static ServiceProvider Build(string dsn)
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, dsn));
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<AdvisoryLocks>();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddFileStore();
        services.AddOptions<StatusThresholdOptions>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.IDiskUsageReader, Shopfolio.Infrastructure.Status.DriveDiskUsageReader>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.DiskUsageGuard>();
        services.AddSingleton<ImageProcessor>();
        services.AddCatalogModule();
        services.AddAfterSalesModule();
        services.AddSettingsDependencies();
        services.AddOrderingModule();
        services.AddPaymentModule();

        // The Notifications module's open face and the writers it needs; the firm's contact e-mail is a fixed address, not the Settings module's.
        services.AddScoped<INotificationsModule, NotificationsModule>();
        services.AddScoped<CustomerNoticeQueue>();
        services.AddScoped<CompanyNoticeQueue>();
        services.AddScoped<AccountEmailQueue>();
        services.AddScoped<ICompanyContactEmail, FixedCompanyContactEmail>();
        services.AddOptions<LinkKeyOptions>();
        services.AddSingleton<LinkKeys>();
        return services.BuildServiceProvider();
    }

    private sealed class FixedCompanyContactEmail : ICompanyContactEmail
    {
        public Task<string?> GetAsync(CancellationToken ct) => Task.FromResult<string?>(CompanyEmail);
    }
}
