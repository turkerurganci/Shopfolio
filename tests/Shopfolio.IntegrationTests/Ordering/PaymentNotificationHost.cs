using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.SignedEndpoints;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Settings;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The one test server that hosts the application's payment notification endpoint with the modules, the job runner's registry and a fake gateway (D-828): the
/// notification tests, the main flow (T329) and the branch flows (T390) start it with their own clock, gateway and extras. The Settings module's start-up
/// regeneration of the legal texts is left out, so the seeded versions are the ones shown; the firm's contact e-mail is <see cref="OrderingTestServices.CompanyEmail"/>.
/// </summary>
internal static class PaymentNotificationHost
{
    /// <summary>
    /// Builds and starts the server. <paramref name="configure"/> runs after the common registrations, so a service it adds (an audit actor that names an
    /// administrator, the open card line, a closed transfer line) is the one resolved; <paramref name="database"/> adds to the context options (an interceptor).
    /// </summary>
    public static async Task<WebApplication> StartAsync(
        string dsn,
        TimeProvider clock,
        IPaymentGateway gateway,
        CancellationToken ct,
        Action<IServiceCollection>? configure = null,
        Action<DbContextOptionsBuilder>? database = null,
        ILoggerProvider? logs = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        if (logs is not null)
        {
            builder.Logging.AddProvider(logs);
        }

        IServiceCollection services = builder.Services;
        services.AddDbContext<AppDbContext>(options =>
        {
            AppDbContext.Configure(options, dsn);
            database?.Invoke(options);
        });
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<AdvisoryLocks>();
        services.AddSingleton(clock);
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddFileStore();
        services.AddOptions<StatusThresholdOptions>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.IDiskUsageReader, Shopfolio.Infrastructure.Status.DriveDiskUsageReader>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.DiskUsageGuard>();
        services.AddSingleton<ImageProcessor>();
        services.AddCatalogModule();
        services.AddAfterSalesModule();
        services.AddOrderingModule();
        services.AddPaymentModule();

        // The Settings module with its trail (the payment window the transition unit reads; T319, P-7), without its start-up regeneration of the legal texts.
        services.AddScoped<IAuditActorSource, NoAuditActor>();
        services.AddScoped<Shopfolio.Modules.Administration.Contracts.IAuditTrail, AuditTrail>();
        services.AddSettingsModule();
        services.Remove(services.Single(d => d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService) && d.ImplementationType == typeof(GeneratedLegalTextsStartup)));

        // The Notifications module's open faces and the writers of the dispatch rows.
        services.AddScoped<INotificationsModule, NotificationsModule>();
        services.AddScoped<IOrderContactNotices, OrderContactNotices>();
        services.AddScoped<CustomerNoticeQueue>();
        services.AddScoped<CompanyNoticeQueue>();
        services.AddScoped<AccountEmailQueue>();
        services.AddScoped<ICompanyContactEmail, FixedCompanyContactEmail>();
        services.AddOptions<LinkKeyOptions>();
        services.AddSingleton<LinkKeys>();

        services.AddSingleton(gateway);
        services.AddSingleton(provider => new JobHandlerRegistry(provider.GetServices<JobHandlerRegistration>()));
        services.AddAntiforgery();
        services.AddSignedEndpoints();
        services.AddSingleton<UnexpectedErrorCounter>();
        services.AddSingleton<UnexpectedErrorLayer>();
        configure?.Invoke(services);

        WebApplication app = builder.Build();
        app.UseUnexpectedErrorLayer();
        app.UseAntiforgery();
        app.MapPaymentNotificationEndpoint();
        await app.StartAsync(ct);
        return app;
    }

    private sealed class FixedCompanyContactEmail : ICompanyContactEmail
    {
        public Task<string?> GetAsync(CancellationToken ct) => Task.FromResult<string?>(OrderingTestServices.CompanyEmail);
    }
}
