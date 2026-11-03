using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.FileSecrets;

/// <summary>
/// Source scan for code that reads a secret from the environment (05 6.x.1: secrets arrive as files, never as environment
/// variables). The application's own settings may come from the environment (the default configuration does that); a secret may
/// not, so code that reads environment variables by hand, or names the password variable of the database client, is a finding.
/// </summary>
internal static partial class SecretSourceScan
{
    public const string EnvironmentRead = "reads an environment variable by hand";

    public const string EnvironmentConfiguration = "adds the environment as an extra configuration source";

    public const string DatabasePasswordVariable = "names the database client's password variable";

    public static IReadOnlyList<(string Rule, int Line)> Scan(string text)
    {
        List<(string Rule, int Line)> findings = [];
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.TrimStart().StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            if (EnvironmentVariableCall().IsMatch(line))
            {
                findings.Add((EnvironmentRead, i + 1));
            }

            if (line.Contains("AddEnvironmentVariables", StringComparison.Ordinal))
            {
                findings.Add((EnvironmentConfiguration, i + 1));
            }

            if (line.Contains("PGPASSWORD", StringComparison.Ordinal))
            {
                findings.Add((DatabasePasswordVariable, i + 1));
            }
        }

        return findings;
    }

    [GeneratedRegex(@"Environment\s*\.\s*(GetEnvironmentVariable|GetEnvironmentVariables|ExpandEnvironmentVariables)\b")]
    private static partial Regex EnvironmentVariableCall();
}
