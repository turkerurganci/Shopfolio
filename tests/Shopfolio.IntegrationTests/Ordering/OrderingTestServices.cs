using Microsoft.EntityFrameworkCore;
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
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The services of the transition unit's tests (T315, T317): the database context and the transaction runner, the job queue, the modules the unit
/// calls - After-sales, Payment - and the Notifications module's writers of the dispatch rows. The firm's contact e-mail is <see cref="CompanyEmail"/>.
/// </summary>
internal static class OrderingTestServices
{
    public const string CompanyEmail = "firma-iletisim@example.com";

    public static ServiceProvider Build(string dsn) => Build(dsn, TimeProvider.System);

    /// <summary>
    /// The same services on the given clock (T348: the confirmation's window, coupon dates and moments read a fixed time). The card line is open unless
    /// <paramref name="cardLineOpen"/> is false (T352: the provider's keys missing at installation); the transfer line is the real one (T353), read from the seeded settings row.
    /// </summary>
    public static ServiceProvider Build(string dsn, TimeProvider time, bool cardLineOpen = true)
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, dsn));
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<AdvisoryLocks>();
        services.AddSingleton(time);
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

        // The Settings module (with its trail) for the payment window the correction of the transfer mark starts again (T319; P-7).
        services.AddScoped<Shopfolio.Modules.Administration.IAuditActorSource, NoAuditActor>();
        services.AddScoped<Shopfolio.Modules.Administration.Contracts.IAuditTrail, Shopfolio.Modules.Administration.AuditTrail>();
        services.AddSettingsModule();

        // The Notifications module's open face and the writers it needs; the firm's contact e-mail is a fixed address, not the Settings module's.
        services.AddScoped<INotificationsModule, NotificationsModule>();
        services.AddScoped<CustomerNoticeQueue>();
        services.AddScoped<CompanyNoticeQueue>();
        services.AddScoped<AccountEmailQueue>();
        services.AddScoped<ICompanyContactEmail, FixedCompanyContactEmail>();
        services.AddOptions<LinkKeyOptions>();
        services.AddSingleton<LinkKeys>();

        // The payment method lines (T352): the card line as the installation's keys say; the transfer line is the Settings module's own binding (T353: BankTransferCondition, D-787).
        services.AddSingleton<IReadOnlyList<ConditionalLine>>(
            [new ConditionalLine(ConditionalLine.CardPayment, cardLineOpen ? [] : [PaymentOptions.ProviderKeysKey])]);
        return services.BuildServiceProvider();
    }

    private sealed class FixedCompanyContactEmail : ICompanyContactEmail
    {
        public Task<string?> GetAsync(CancellationToken ct) => Task.FromResult<string?>(CompanyEmail);
    }
}
