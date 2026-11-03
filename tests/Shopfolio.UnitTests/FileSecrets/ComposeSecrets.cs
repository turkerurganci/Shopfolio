using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.FileSecrets;

/// <summary>
/// Reads the secret wiring out of <c>deploy/compose.yaml</c> and the secret inventory out of <c>DEPLOY_RUNBOOK</c> section B
/// (05 6.x.2). No YAML library: the Compose file keeps one fixed layout (two-space steps, block lists), and the reader fails
/// loudly when a line does not fit it.
/// </summary>
internal static partial class ComposeSecrets
{
    /// <summary>Service name to the names of the secrets mounted into it (<c>services.&lt;name&gt;.secrets</c>).</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> ReadServiceSecrets(string composeText)
    {
        Dictionary<string, IReadOnlyList<string>> result = new(StringComparer.Ordinal);
        string? service = null;
        bool inServices = false;
        bool inSecretsList = false;
        List<string>? current = null;

        foreach (string raw in composeText.ReplaceLineEndings("\n").Split('\n'))
        {
            string line = raw.TrimEnd();
            if (line.Length == 0 || line.TrimStart().StartsWith('#'))
            {
                continue;
            }

            if (!line.StartsWith(' '))
            {
                inServices = line == "services:";
                inSecretsList = false;
                service = null;
                continue;
            }

            if (!inServices)
            {
                continue;
            }

            Match serviceLine = ServiceLine().Match(line);
            if (serviceLine.Success)
            {
                service = serviceLine.Groups[1].Value;
                current = [];
                result[service] = current;
                inSecretsList = false;
                continue;
            }

            if (line == "    secrets:")
            {
                inSecretsList = true;
                continue;
            }

            Match item = SecretItem().Match(line);
            if (inSecretsList && item.Success)
            {
                current!.Add(item.Groups[1].Value);
                continue;
            }

            inSecretsList = false;
        }

        return result;
    }

    /// <summary>Names defined in the top-level <c>secrets:</c> block, each with the file name its <c>file:</c> line ends in.</summary>
    public static IReadOnlyDictionary<string, string> ReadDefinedSecrets(string composeText)
    {
        Dictionary<string, string> result = new(StringComparer.Ordinal);
        bool inBlock = false;
        string? name = null;

        foreach (string raw in composeText.ReplaceLineEndings("\n").Split('\n'))
        {
            string line = raw.TrimEnd();
            if (line.Length == 0 || line.TrimStart().StartsWith('#'))
            {
                continue;
            }

            if (!line.StartsWith(' '))
            {
                inBlock = line == "secrets:";
                name = null;
                continue;
            }

            if (!inBlock)
            {
                continue;
            }

            Match entry = ServiceLine().Match(line);
            if (entry.Success)
            {
                name = entry.Groups[1].Value;
                continue;
            }

            Match fileLine = FileLine().Match(line);
            if (fileLine.Success && name is not null)
            {
                result[name] = fileLine.Groups[1].Value;
            }
        }

        return result;
    }

    /// <summary>Rows of the section B table: secret file name, the component names in the "Hangi bileşen alır" column, and the rotation cell.</summary>
    public static IReadOnlyList<RunbookSecretRow> ReadRunbookSecrets(string runbookText)
    {
        List<RunbookSecretRow> rows = [];
        bool inSection = false;

        foreach (string raw in runbookText.ReplaceLineEndings("\n").Split('\n'))
        {
            if (raw.StartsWith("## ", StringComparison.Ordinal))
            {
                inSection = raw.StartsWith("## B.", StringComparison.Ordinal);
                continue;
            }

            if (!inSection || !raw.StartsWith("| ", StringComparison.Ordinal))
            {
                continue;
            }

            string[] cells = raw.Trim().Trim('|').Split('|').Select(c => c.Trim()).ToArray();
            Match first = Backticked().Match(cells.Length > 1 ? cells[1] : string.Empty);
            if (cells.Length < 7 || !first.Success)
            {
                continue;
            }

            string[] receivers = Backticked().Matches(cells[3]).Select(m => m.Groups[1].Value).ToArray();
            rows.Add(new RunbookSecretRow(first.Groups[1].Value, receivers, cells[5]));
        }

        return rows;
    }

    [GeneratedRegex(@"^  ([a-z][a-z0-9_-]*):$")]
    private static partial Regex ServiceLine();

    [GeneratedRegex(@"^      - ([a-z][a-z0-9_]*)$")]
    private static partial Regex SecretItem();

    [GeneratedRegex(@"^    file: \$\{SHOPFOLIO_SECRETS_DIR:-/etc/shopfolio/secrets\}/([a-z][a-z0-9_]*)$")]
    private static partial Regex FileLine();

    [GeneratedRegex("`([^`]+)`")]
    private static partial Regex Backticked();
}

internal sealed record RunbookSecretRow(string Secret, IReadOnlyList<string> Receivers, string Rotation);
