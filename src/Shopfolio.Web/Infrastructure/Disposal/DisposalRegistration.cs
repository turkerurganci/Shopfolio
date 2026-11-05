using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.Infrastructure.Disposal;

/// <summary>
/// The registration of the disposal (05 5.2.8, 5.2.9; 09 13.1 item 9): the disposal step joins the nightly job - it adds no timer, no hosted
/// service and no lock of its own - and the parts of this task follow in the order they run. A task that owns another kind of record (the
/// order's part, the other time-bound records, the files) adds its part with <see cref="AddDisposalPart{TPart}"/> in its own registration.
/// <c>Program.cs</c> calls <see cref="AddDisposal"/> after <see cref="SchedulerRegistration.AddScheduler"/>, so the job queue's clean-up runs first.
/// </summary>
internal static class DisposalRegistration
{
    public static IServiceCollection AddDisposal(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return services.AddDisposal(new LogDirectories(LoggingOptions.FromConfiguration(configuration).Directory, LogDirectories.DefaultAccessDirectory));
    }

    /// <summary>The same registration with the log directories given; the tests use it with directories of their own.</summary>
    public static IServiceCollection AddDisposal(this IServiceCollection services, LogDirectories directories)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(directories);
        services.AddSingleton(directories);
        services.AddNightlyStep<DisposalStep>();

        // The audit trail first (a database function), then the log files, then the temporary records of 06 9.4.3.
        services.AddDisposalPart<AuditLogDisposalPart>();
        services.AddDisposalPart<LogFileDisposalPart>();
        services.AddDisposalPart<AttemptRecordDisposalPart>();
        services.AddDisposalPart<SessionDisposalPart>();
        services.AddDisposalPart<RecognizedBrowserMarkDisposalPart>();

        // The guest cart's and the administrator invitation's parts belong to their modules (09 6 item 2): AddOrderingModule and
        // AddAdministrationModule register them with AddDisposalPart.
        services.AddDisposalPart<RevertLinkAddressDisposalPart>();
        return services;
    }

    /// <summary>Registers a part of the disposal. Parts run in the order of their registration; each is scoped, so it shares the step's scope and its transactions.</summary>
    public static IServiceCollection AddDisposalPart<TPart>(this IServiceCollection services)
        where TPart : class, IDisposalPart
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddScoped<IDisposalPart, TPart>();
        return services;
    }
}
