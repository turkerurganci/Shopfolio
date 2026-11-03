using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure;

namespace Shopfolio.UnitTests.Infrastructure;

public sealed class DatabaseOptionsValidatorTests
{
    private const string ValidConnectionString = "Host=localhost;Database=shopfolio";

    private readonly DatabaseOptionsValidator _validator = new();

    [Fact]
    public void Validate_WithDefaultTimeoutAndValidConnectionString_Succeeds()
    {
        ValidateOptionsResult result = _validator.Validate(null, new DatabaseOptions { ConnectionString = ValidConnectionString });

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(20)]
    [InlineData(21)]
    public void Validate_WithTimeoutNotBelowClientCommandTimeOrNotPositive_FailsAndNamesTheKey(int seconds)
    {
        DatabaseOptions options = new() { ConnectionString = ValidConnectionString, LivenessTimeoutSeconds = seconds };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains(DatabaseOptions.LivenessTimeoutKey, Assert.Single(result.Failures), StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_WithTimeoutJustBelowClientCommandTime_Succeeds()
    {
        DatabaseOptions options = new() { ConnectionString = ValidConnectionString, LivenessTimeoutSeconds = 19 };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithMissingConnectionString_FailsAndNamesTheKey(string connectionString)
    {
        ValidateOptionsResult result = _validator.Validate(null, new DatabaseOptions { ConnectionString = connectionString });

        Assert.True(result.Failed);
        Assert.Contains(DatabaseOptions.ConnectionStringKey, Assert.Single(result.Failures), StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_WithMalformedConnectionString_FailsWithoutPrintingIt()
    {
        const string malformed = "not-a-connection-string-with-secret-value";

        ValidateOptionsResult result = _validator.Validate(null, new DatabaseOptions { ConnectionString = malformed });

        Assert.True(result.Failed);
        string failure = Assert.Single(result.Failures);
        Assert.Contains(DatabaseOptions.ConnectionStringKey, failure, StringComparison.Ordinal);
        Assert.DoesNotContain("secret-value", failure, StringComparison.Ordinal);
    }
}
