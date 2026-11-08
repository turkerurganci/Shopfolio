using Shopfolio.Modules.Administration;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>
/// The acting administrator the test chooses (T379): the application reads it from the request, the Model tests hold it here. Registered as a singleton; a test sets
/// <see cref="Actor"/> before the call and the module under test sees that administrator as the signed-in one.
/// </summary>
internal sealed class SwitchableAuditActor : IAuditActorSource
{
    public AuditActor? Actor { get; set; }

    public Task<AuditActor?> CurrentAsync(CancellationToken ct) => Task.FromResult(Actor);
}
