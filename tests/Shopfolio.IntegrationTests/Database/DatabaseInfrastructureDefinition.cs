namespace Shopfolio.IntegrationTests.Database;

/// <summary>
/// The collection of the tests that check the test infrastructure itself; it owns one database copied from the template.
/// </summary>
[CollectionDefinition(Name)]
public sealed class DatabaseInfrastructureDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "DatabaseInfrastructure";
}
