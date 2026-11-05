using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.InstallationSettings;

namespace Shopfolio.Infrastructure.Jobs;

/// <summary>
/// The registration of the scheduler (09 13.1 item 1): the heartbeat, the scan, the nightly job with its clean-up step, the heartbeat's HTTP
/// client and the one scheduler service. <c>Program.cs</c> calls it after <see cref="JobQueueRegistration.AddJobQueue"/>, so the scheduler
/// starts after the runner. A module that owns a due record, a return-from-outage change or a nightly step adds it with
/// <see cref="AddDueRecordSource{TSource}"/>, <see cref="AddOutageEffect{TEffect}"/> or <see cref="AddNightlyStep{TStep}"/> in its own
/// registration.
/// </summary>
internal static class SchedulerRegistration
{
    public static IServiceCollection AddScheduler(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<HeartbeatPulse>();
        services.AddSingleton<SchedulerScan>();
        services.AddSingleton<NightlyJobRunner>();
        services.AddSingleton<INightlyStep, JobQueueCleanupStep>();

        // The beat asks the site's public address through the reverse proxy (05 5.4.1); the limit is set on each request, so the client has none.
        services.AddHttpClient(HeartbeatPulse.ClientName)
            .ConfigureHttpClient((provider, client) =>
            {
                client.BaseAddress = new Uri($"https://{provider.GetRequiredService<IOptions<SiteOptions>>().Value.Domain}/");
                client.Timeout = Timeout.InfiniteTimeSpan;
            });
        services.AddHostedService<SchedulerService>();
        return services;
    }

    /// <summary>Registers a source of due records (09 13.1 item 6). It is a singleton and opens its own scopes through those it is given.</summary>
    public static IServiceCollection AddDueRecordSource<TSource>(this IServiceCollection services)
        where TSource : class, IDueRecordSource
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IDueRecordSource, TSource>();
        return services;
    }

    /// <summary>Registers a change that the return from an outage makes (09 13.1 item 8). It is scoped: it runs in the return's scope and transaction.</summary>
    public static IServiceCollection AddOutageEffect<TEffect>(this IServiceCollection services)
        where TEffect : class, IOutageEffect
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddScoped<IOutageEffect, TEffect>();
        return services;
    }

    /// <summary>Registers a step of the nightly job (09 13.1 item 9); steps run in the order of their registration.</summary>
    public static IServiceCollection AddNightlyStep<TStep>(this IServiceCollection services)
        where TStep : class, INightlyStep
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<INightlyStep, TStep>();
        return services;
    }
}
