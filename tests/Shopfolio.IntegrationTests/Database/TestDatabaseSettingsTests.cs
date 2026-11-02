namespace Shopfolio.IntegrationTests.Database;

public sealed class TestDatabaseSettingsTests
{
    [Fact]
    public void Resolve_WithEnvironmentAndSecret_ReturnsEnvironmentValue()
    {
        string? resolved = TestDatabaseSettings.Resolve("Host=from-environment", "Host=from-secret");

        Assert.Equal("Host=from-environment", resolved);
    }

    [Fact]
    public void Resolve_WithOnlySecret_ReturnsSecretValue()
    {
        string? resolved = TestDatabaseSettings.Resolve(null, "Host=from-secret");

        Assert.Equal("Host=from-secret", resolved);
    }

    [Fact]
    public void Resolve_WithEmptyEnvironmentAndSecret_ReturnsSecretValue()
    {
        string? resolved = TestDatabaseSettings.Resolve("  ", "Host=from-secret");

        Assert.Equal("Host=from-secret", resolved);
    }

    [Fact]
    public void Resolve_WithOnlyEnvironment_ReturnsEnvironmentValue()
    {
        string? resolved = TestDatabaseSettings.Resolve("Host=from-environment", null);

        Assert.Equal("Host=from-environment", resolved);
    }

    [Fact]
    public void Resolve_WithNeither_ReturnsNull()
    {
        string? resolved = TestDatabaseSettings.Resolve(null, string.Empty);

        Assert.Null(resolved);
    }

    [Fact]
    public void SettingNames_AreTheDocumentedOnes()
    {
        Assert.Equal("SHOPFOLIO_TEST_DATABASE", TestDatabaseSettings.EnvironmentVariableName);
        Assert.Equal("Tests:Database", TestDatabaseSettings.SecretKey);
    }
}
