using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scans over the files of the database image's setup (<c>deploy/images/database</c>; 09 10.8, 17 item 1; 05 6.x.16): the client
/// authentication template, the setup script and its SQL files. Each scan reports the rule it broke and the 1-based line.
/// </summary>
internal static partial class DatabaseImageScan
{
    public const string HbaTrust = "pg_hba.conf line uses an authentication method that needs no password proof (trust, password)";

    public const string HbaAllUsers = "pg_hba.conf line is open to every user";

    public const string HbaStatusUserScope = "the backup status user's line must be a local line naming the database template, and no other line may name that user";

    public const string HbaFounderOverNetwork = "pg_hba.conf lets the founding user in over the network";

    public const string ScriptShellTrace = "setup script switches the shell trace on";

    public const string ScriptPasswordOnCommandLine = "setup script gives a password on a command line, in a variable or in the environment";

    public const string SqlLiteralPassword = "SQL file writes a password as a literal";

    public const string SqlRoleAttributes = "CREATE ROLE does not state all five attributes the users must not have";

    public const string SqlPublicGrant = "SQL file grants a privilege to PUBLIC";

    private static readonly string[] _requiredNegatives = ["NOSUPERUSER", "NOCREATEDB", "NOCREATEROLE", "NOREPLICATION", "NOBYPASSRLS"];

    /// <summary>Scan of the client authentication template (<c>pg_hba.conf</c>).</summary>
    public static IReadOnlyList<(string Rule, int Line)> ScanHba(string text)
    {
        List<(string Rule, int Line)> findings = [];
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string[] fields = lines[i].Split('#')[0].Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length < 4)
            {
                continue;
            }

            bool local = fields[0] == "local";
            string database = fields[1];
            string users = fields[2];
            // local: TYPE DATABASE USER METHOD; host: TYPE DATABASE USER ADDRESS METHOD (an option such as map=founder may follow).
            string methodName = local ? fields[3] : fields.Length > 4 ? fields[4] : string.Empty;

            if (methodName is "trust" or "password")
            {
                findings.Add((HbaTrust, i + 1));
            }

            if (users == "all")
            {
                findings.Add((HbaAllUsers, i + 1));
            }

            bool namesStatusUser = users.Split(',').Contains("shopfolio_backup_status", StringComparer.Ordinal);
            if (namesStatusUser && (!local || database != "@DATABASE@" || users != "shopfolio_backup_status"))
            {
                findings.Add((HbaStatusUserScope, i + 1));
            }

            if (!local && users == "@FOUNDER@" && methodName != "reject")
            {
                findings.Add((HbaFounderOverNetwork, i + 1));
            }
        }

        return findings;
    }

    /// <summary>Scan of the setup script.</summary>
    public static IReadOnlyList<(string Rule, int Line)> ScanScript(string text)
    {
        List<(string Rule, int Line)> findings = [];
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.TrimStart().StartsWith('#'))
            {
                continue;
            }

            if (ShellTrace().IsMatch(line))
            {
                findings.Add((ScriptShellTrace, i + 1));
            }

            if (PasswordOnCommandLine().IsMatch(line))
            {
                findings.Add((ScriptPasswordOnCommandLine, i + 1));
            }
        }

        return findings;
    }

    /// <summary>Scan of one SQL file of the setup script (comments are ignored).</summary>
    public static IReadOnlyList<(string Rule, int Line)> ScanSql(string text)
    {
        List<(string Rule, int Line)> findings = [];
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string code = lines[i].Split("--")[0];
            if (LiteralPassword().IsMatch(code))
            {
                findings.Add((SqlLiteralPassword, i + 1));
            }

            if (code.Contains("CREATE ROLE", StringComparison.OrdinalIgnoreCase)
                && _requiredNegatives.Any(attribute => !code.Contains(attribute, StringComparison.OrdinalIgnoreCase)))
            {
                findings.Add((SqlRoleAttributes, i + 1));
            }

            if (ToPublic().IsMatch(code))
            {
                findings.Add((SqlPublicGrant, i + 1));
            }
        }

        return findings;
    }

    [GeneratedRegex(@"^\s*set\s+(-[a-zA-Z]*x|-o\s+xtrace)", RegexOptions.IgnoreCase)]
    private static partial Regex ShellTrace();

    // A -v option whose variable name carries "password", an assignment of such a variable, PGPASSWORD, or --password= on a command line.
    // The `\set <name>_password` lines the script prints for psql's own backtick command are no command line and do not match.
    [GeneratedRegex(@"(-v\s+""?\w*password|^\s*\w*password\w*=|PGPASSWORD|--password=)", RegexOptions.IgnoreCase)]
    private static partial Regex PasswordOnCommandLine();

    [GeneratedRegex(@"PASSWORD\s+'", RegexOptions.IgnoreCase)]
    private static partial Regex LiteralPassword();

    [GeneratedRegex(@"\bTO\s+PUBLIC\b", RegexOptions.IgnoreCase)]
    private static partial Regex ToPublic();
}
