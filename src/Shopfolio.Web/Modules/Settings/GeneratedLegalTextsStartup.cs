using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>The result of the start-up synchronization: never a refusal, only the need to give the transaction runner a result type.</summary>
internal sealed class StartupSyncResult : IOperationResult
{
    public static StartupSyncResult Done { get; } = new();

    public bool IsRejected => false;
}

/// <summary>
/// Writes the first versions of the two generated legal texts and a new version when the shipped template changed (06 4.56, 8.4.2; 09 11.4 item 5; 02 3.33.3): the application's
/// start. A system event, so the trail has no row for it. It runs after the start data check (registered by the module, which the program adds later), in one transaction behind the
/// settings row lock, through <see cref="ISettingsModule.SynchronizeGeneratedLegalTextsAsync"/>. A database that cannot be reached is not a failure of the start: the liveness
/// endpoint says so (09 10.7 item 6), and the texts are written by the next start or by the next change of a feeding setting.
/// </summary>
internal sealed class GeneratedLegalTextsStartup : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public GeneratedLegalTextsStartup(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
            ISettingsModule settings = scope.ServiceProvider.GetRequiredService<ISettingsModule>();
            await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
                async ct =>
                {
                    await settings.SynchronizeGeneratedLegalTextsAsync(ct);
                    return StartupSyncResult.Done;
                },
                cancellationToken);
        }
        catch (NpgsqlException)
        {
            // Nothing is written here: the start data check has already warned once that the database could not be reached (its single warning line), and the exception's text
            // can carry connection details.
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
