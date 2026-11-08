using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.FileSecrets;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Status;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Settings;

namespace Shopfolio.Commands;

/// <summary>
/// The command mode of the application (05 8.5.1; 09 13.2 item 1): <c>Program.cs</c> hands the arguments here when the first one is <c>admin</c>. The host builds a <b>service collection
/// only</b> - the database context with the <see cref="DatabaseSessionProfile.Maintenance"/> times, the nine modules' registrations, the command log - and no web host: there is no
/// server, and every <see cref="IHostedService"/> the registrations added is taken out again, so the scheduler, the heartbeat, the job runner and the disposal job do not run in a command
/// process. The migration is the migration service's work and does not run here (09 10.6). The process opens the database only when a command asks for a scope.
/// </summary>
internal static class CommandHost
{
    /// <summary>The first argument that selects the command mode (<c>admin create-admin</c>).</summary>
    public const string ModeArgument = "admin";

    public static bool IsCommandMode(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        return args.Length > 0 && string.Equals(args[0], ModeArgument, StringComparison.Ordinal);
    }

    /// <summary>Runs the command named after <c>admin</c> and returns the process exit code.</summary>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        ConsoleTerminal terminal = new();

        // The settings and the provider are made inside the dispatcher's guarded call, after the command line has been read.
        CommandDispatcher dispatcher = new(AdminCommandCatalog.All, terminal, () => CreateServices(BuildConfiguration(), terminal).BuildServiceProvider());

        // Ctrl+C keeps its default: the process ends and the open transaction is rolled back by the server when the connection drops. A handler that only cancels a token would leave
        // a password prompt that cannot be left.
        return await dispatcher.ExecuteAsync(args[1..], CancellationToken.None);
    }

    /// <summary>
    /// The settings the way the application reads them (<c>Program.cs</c>): the settings file, the environment, then the mounted secret files, which win (09 3.5 item 2; 05 6.x.1).
    /// </summary>
    internal static IConfiguration BuildConfiguration()
    {
        // The host builder's default sources (settings files, environment) are the ones the web host has; only its configuration is taken, no host is built.
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { Args = [], ContentRootPath = AppContext.BaseDirectory });
        ((IConfigurationBuilder)builder.Configuration).Add(new SecretFileConfigurationSource(new SecretFiles(SecretFiles.DefaultDirectory)));
        return builder.Configuration;
    }

    /// <summary>The command mode's service collection: no hosted service and no server service (architecture test, 09 13.2 item 1).</summary>
    internal static IServiceCollection CreateServices(IConfiguration configuration, ITerminal terminal)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(terminal);

        ServiceCollection services = new();
        services.AddSingleton(configuration);
        services.AddSingleton(terminal);
        services.AddLogging();
        services.AddTimeSource();

        services.AddOptions<DatabaseOptions>().BindConfiguration(DatabaseOptions.SectionName);
        services.AddOptions<LinkKeyOptions>().BindConfiguration(LinkKeyOptions.SectionName);

        // The command's own log file (09 18.1 item 2): the application's file is not written by a second process.
        services.AddSingleton<IAuditLineWriter>(provider => LoggingSetup.CreateAuditLineWriter(
            LoggingOptions.FromConfiguration(provider.GetRequiredService<IConfiguration>()),
            AuditFile.Command,
            provider.GetRequiredService<TimeProvider>()));

        // The maintenance session times (09 10.7 item 2; 09 13.2 item 6): a longer statement time and client time than the web profile's.
        static string MaintenanceConnectionString(IServiceProvider provider) =>
            DatabaseConnection.Create(provider.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString, DatabaseSessionProfile.Maintenance);

        services.AddSingleton(provider => NpgsqlDataSource.Create(MaintenanceConnectionString(provider)));
        services.AddDbContext<AppDbContext>((provider, options) => AppDbContext.Configure(options, MaintenanceConnectionString(provider)));

        services.AddAppCache();
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddScoped<AdvisoryLocks>();
        services.AddScoped<AttemptCounter>();
        services.AddScoped<LoginRecorder>();
        services.AddSingleton<PasswordService>();
        services.AddScoped<SessionStore>();
        services.AddScoped<ReauthenticationValues>();
        services.AddScoped<PasswordRegime>();
        services.AddLinkRecords();
        services.AddJobQueue();

        // The status command evaluates the endpoint's conditions (05 8.5.6): their thresholds, their sources and the error counter that some of them are constructed with. The conditions
        // that read the web process's memory are listed by the command and not evaluated (IStatusCondition.ReadsProcessMemory).
        services.AddOptions<StatusThresholdOptions>().BindConfiguration(StatusThresholdOptions.SectionName);
        services.TryAddSingleton<UnexpectedErrorCounter>();
        services.AddStatus();

        // The registrations the modules stand on, the same calls the web host makes (Program.cs): a module chain that reaches the Catalog module's files needs the image processor.
        services.AddFileStore();
        services.AddImageProcessing();

        // The audit actor source of the Administration module reads the request's accessor; in a command process there is no request, so it has no actor (AuditActorSource.cs). Not a server service.
        services.AddHttpContextAccessor();

        // The conditional lines (05 8.4.6), worked out from the same settings classes as the web's start-up check; the status command writes them.
        services.AddOptions<PaymentOptions>().BindConfiguration(PaymentOptions.SectionName);
        services.AddOptions<GoogleOptions>().BindConfiguration(GoogleOptions.SectionName);
        services.AddConditionalLines();
        services.AddCatalogModule();
        services.AddOrderingModule();
        services.AddPaymentModule();
        services.AddAfterSalesModule();
        services.AddMembershipModule();
        services.AddCorporateContentModule();
        services.AddNotificationsModule();
        services.AddAdministrationModule();
        services.AddSettingsModule();

        // The registrations above add the job runner and two start-up services: none of them runs in a command process (05 8.5.1).
        services.RemoveAll<IHostedService>();
        return services;
    }
}
