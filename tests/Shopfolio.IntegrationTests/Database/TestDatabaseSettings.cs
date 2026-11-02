using Microsoft.Extensions.Configuration;

namespace Shopfolio.IntegrationTests.Database;

/// <summary>
/// Where the administrator connection string of the integration database comes from (09 19.1 item 4):
/// the environment variable <c>SHOPFOLIO_TEST_DATABASE</c> or the user secret <c>Tests:Database</c>; the environment variable wins.
/// The value never enters the repository.
/// </summary>
internal static class TestDatabaseSettings
{
    public const string EnvironmentVariableName = "SHOPFOLIO_TEST_DATABASE";

    public const string SecretKey = "Tests:Database";

    /// <summary>
    /// Picks the connection string: a non-empty environment value first, then a non-empty secret value.
    /// </summary>
    public static string? Resolve(string? environmentValue, string? secretValue)
    {
        if (!string.IsNullOrWhiteSpace(environmentValue))
        {
            return environmentValue;
        }

        return string.IsNullOrWhiteSpace(secretValue) ? null : secretValue;
    }

    /// <summary>
    /// Reads both sources and returns the connection string; fails with a message that names both settings when neither is set
    /// (a test never skips itself: 09 19.2 item 6).
    /// </summary>
    public static string GetRequired()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddUserSecrets(typeof(TestDatabaseSettings).Assembly, optional: true)
            .Build();

        string? connectionString = Resolve(Environment.GetEnvironmentVariable(EnvironmentVariableName), configuration[SecretKey]);
        if (connectionString is null)
        {
            throw new InvalidOperationException(
                $"The integration database is not configured: set the environment variable {EnvironmentVariableName} or the user secret {SecretKey} (09 19.1 item 4).");
        }

        return connectionString;
    }
}
