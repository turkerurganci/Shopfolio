using Microsoft.Extensions.DependencyInjection.Extensions;

using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Jobs;

namespace Shopfolio.Infrastructure.Status;

/// <summary>
/// The registration of the status endpoint's conditions (05 9.4.2; 09 3.5 item 3). <c>Program.cs</c> calls it after the scheduler, whose heartbeat
/// client it hands the certificate reading. A module that owns a condition (the channel warning) adds it with <see cref="AddStatusCondition{TCondition}"/>
/// in its own registration. The sources are interfaces so that a test replaces one with a fake measurement.
/// </summary>
internal static class StatusRegistration
{
    public static IServiceCollection AddStatus(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<PaymentStartFailureSeries>();
        services.TryAddSingleton<CertificateExpiryMemory>();
        services.TryAddSingleton<IDiskUsageReader, DriveDiskUsageReader>();
        services.TryAddScoped<ILogShippingReader, PostgresLogShippingReader>();
        services.TryAddSingleton<IFileBackupStatusReader>(new JsonFileBackupStatusReader(JsonFileBackupStatusReader.DefaultDirectory));

        services.AddStatusCondition<DiskUsageCondition>();
        services.AddStatusCondition<LogShippingCondition>();
        services.AddStatusCondition<FullBackupCondition>();
        services.AddStatusCondition<FileBackupCondition>();
        services.AddStatusCondition<HeartbeatCondition>();
        services.AddStatusCondition<UnexpectedErrorCondition>();
        services.AddStatusCondition<PaymentStartFailureCondition>();
        services.AddStatusCondition<CertificateCondition>();
        services.AddStatusCondition<JobQueueStatusCondition>();
        services.AddStatusCondition<NightlyJobCondition>();
        services.AddScoped<StatusEvaluator>();

        // The beat reads the end of the site's certificate in its handshake (05 9.4.9); connections are renewed every few minutes so a renewed
        // certificate is seen soon. The platform's validation decides acceptance; only the end date is kept.
        services.AddHttpClient(HeartbeatPulse.ClientName)
            .ConfigurePrimaryHttpMessageHandler(provider =>
            {
                CertificateExpiryMemory memory = provider.GetRequiredService<CertificateExpiryMemory>();
                return new SocketsHttpHandler
                {
                    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                    SslOptions = { RemoteCertificateValidationCallback = (_, certificate, _, errors) => memory.Observe(certificate, errors) },
                };
            });
        return services;
    }

    /// <summary>Registers a warning condition of the status endpoint. It is scoped: it runs in the status request's scope.</summary>
    public static IServiceCollection AddStatusCondition<TCondition>(this IServiceCollection services)
        where TCondition : class, IStatusCondition
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddScoped<IStatusCondition, TCondition>();
        return services;
    }
}
