using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scans over the deploy files for secrets (05 6.x.1: a secret reaches a container only as a file; 09 17 item 7 and 09 4.3
/// item 3: no secret key and no secret value in a build or settings file). Two scans: the <c>environment</c> settings of a Compose
/// file (both the list form <c>- NAME=value</c> and the mapping form <c>NAME: value</c>) and the installation settings template.
/// </summary>
internal static partial class DeploySecretScan
{
    public const string SecretInEnvironment = "environment setting that looks like a secret";

    public const string NotAssignment = "line is not a KEY=value line";

    public const string SecretKey = "settings key names a secret";

    public const string SecretLikeValue = "settings value looks like a secret";

    /// <summary>
    /// Environment settings that are a deliberate exception, each with its reason. The key is the Compose file name and the setting name.
    /// </summary>
    public static readonly IReadOnlyDictionary<(string File, string Name), string> AllowedEnvironmentSettings =
        new Dictionary<(string File, string Name), string>
        {
            // 09 4.3 item 2: the development overlay carries the made-up connection string of the local development database. The value is
            // made up, belongs to no installation, and equals the made-up content of the db_setup_app_password file of the development directory.
            [("compose.dev.yaml", "Database__ConnectionString")] = "development value, 09 4.3 item 2",
        };

    /// <summary>Findings of the Compose <c>environment</c> scan: the rule and the 1-based line.</summary>
    public static IReadOnlyList<(string Rule, int Line)> ScanComposeEnvironment(string fileName, string composeText)
    {
        List<(string Rule, int Line)> findings = [];
        string[] lines = composeText.ReplaceLineEndings("\n").Split('\n');
        int environmentIndent = -1;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].TrimEnd();
            string trimmed = line.TrimStart();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            int indent = line.Length - trimmed.Length;
            if (environmentIndent >= 0 && indent <= environmentIndent)
            {
                environmentIndent = -1;
            }

            if (environmentIndent < 0)
            {
                if (trimmed == "environment:")
                {
                    environmentIndent = indent;
                }

                continue;
            }

            Match entry = trimmed.StartsWith("- ", StringComparison.Ordinal) ? ListEntry().Match(trimmed) : MappingEntry().Match(trimmed);
            if (!entry.Success)
            {
                continue;
            }

            string name = entry.Groups[1].Value;
            string value = entry.Groups[2].Value.Trim().Trim('"', '\'');
            if (!SecretWord().IsMatch(name) || AllowedEnvironmentSettings.ContainsKey((fileName, name)))
            {
                continue;
            }

            // A path to the secret file is not the secret (the official database image reads POSTGRES_PASSWORD_FILE).
            if (name.EndsWith("_FILE", StringComparison.Ordinal) && value.StartsWith("/run/secrets/", StringComparison.Ordinal))
            {
                continue;
            }

            findings.Add((SecretInEnvironment, i + 1));
        }

        return findings;
    }

    /// <summary>Findings of the settings template scan: the rule and the 1-based line.</summary>
    public static IReadOnlyList<(string Rule, int Line)> ScanSettingsTemplate(string templateText)
    {
        List<(string Rule, int Line)> findings = [];
        string[] lines = templateText.ReplaceLineEndings("\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.Length == 0 || line.TrimStart().StartsWith('#'))
            {
                continue;
            }

            Match assignment = SettingsAssignment().Match(line);
            if (!assignment.Success)
            {
                findings.Add((NotAssignment, i + 1));
                continue;
            }

            if (SecretWord().IsMatch(assignment.Groups[1].Value))
            {
                findings.Add((SecretKey, i + 1));
            }

            if (SecretLikeValueShape().IsMatch(assignment.Groups[2].Value))
            {
                findings.Add((SecretLikeValue, i + 1));
            }
        }

        return findings;
    }

    [GeneratedRegex(@"^- ([A-Za-z_][A-Za-z0-9_]*)=(.*)$")]
    private static partial Regex ListEntry();

    [GeneratedRegex(@"^([A-Za-z_][A-Za-z0-9_]*):\s*(.*)$")]
    private static partial Regex MappingEntry();

    [GeneratedRegex(@"^([A-Za-z_][A-Za-z0-9_]*)=(.*)$")]
    private static partial Regex SettingsAssignment();

    [GeneratedRegex("(PASSWORD|SECRET|TOKEN|CREDENTIAL|PRIVATE|APIKEY|_KEY|CONNECTIONSTRING)", RegexOptions.IgnoreCase)]
    private static partial Regex SecretWord();

    [GeneratedRegex(@"^\s*[A-Za-z0-9+/=_-]{32,}\s*$")]
    private static partial Regex SecretLikeValueShape();
}
