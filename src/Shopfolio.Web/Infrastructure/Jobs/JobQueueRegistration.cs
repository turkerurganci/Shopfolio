using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Jobs;

/// <summary>
/// The registration of the job queue (09 13.1 item 1): the queue writer, the wake-up signal, the handler registry and the one runner service.
/// <c>Program.cs</c> calls it after every module's registration, so hosted services start in this order: the start-up checks, the start-up
/// rewrite of the pending revert links' digests, then the runner (05 6.x.20). A module that owns a job type adds its handler with
/// <see cref="AddJobHandler{THandler}"/> or <see cref="AddTransactionalJobHandler{THandler}"/> in its own registration.
/// </summary>
internal static class JobQueueRegistration
{
    public static IServiceCollection AddJobQueue(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddScoped<JobEnqueueTracker>();
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddSingleton(provider => new JobHandlerRegistry(provider.GetServices<JobHandlerRegistration>()));
        services.AddHostedService<JobRunnerService>();
        return services;
    }

    /// <summary>Registers the handler of a job that calls outside (09 13.1 item 4); one job type has one handler.</summary>
    public static IServiceCollection AddJobHandler<THandler>(this IServiceCollection services, JobType jobType)
        where THandler : class, IJobHandler
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddScoped<THandler>();
        services.AddSingleton(new JobHandlerRegistration(jobType, IsTransactional: false, typeof(THandler)));
        return services;
    }

    /// <summary>Registers the handler of a job that only writes to the database (09 13.1 item 4); one job type has one handler.</summary>
    public static IServiceCollection AddTransactionalJobHandler<THandler>(this IServiceCollection services, JobType jobType)
        where THandler : class, ITransactionalJobHandler
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddScoped<THandler>();
        services.AddSingleton(new JobHandlerRegistration(jobType, IsTransactional: true, typeof(THandler)));
        return services;
    }

    /// <summary>Registers a condition that must hold before the runner takes any row (D-96 K1).</summary>
    public static IServiceCollection AddJobStartGate<TGate>(this IServiceCollection services)
        where TGate : class, IJobStartGate
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IJobStartGate, TGate>();
        return services;
    }
}
