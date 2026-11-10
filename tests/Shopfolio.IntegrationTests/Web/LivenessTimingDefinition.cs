using Shopfolio.IntegrationTests.Database;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The liveness endpoint's wall-clock limit (09 10.7 item 6) is measured while no other collection runs: on a loaded CI host the parallel classes stretched
/// the answer past the margin (D-841). The collection owns its own database copied from the template, like the others.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class LivenessTimingDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "LivenessTiming";
}
