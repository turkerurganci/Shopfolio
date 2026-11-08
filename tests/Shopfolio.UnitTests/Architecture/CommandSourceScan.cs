using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the management commands' folder (T303; 09 13.2 items 3 and 4; 05 8.5.1): (1) the console is touched in <c>Terminal.cs</c> only, so every read and every line of output
/// goes through the terminal abstraction; (2) a hidden key is read with <c>intercept: true</c>; (3) no command file names the database context (a command calls the modules'
/// Contracts interfaces; the context is registered by <c>CommandHost.cs</c> alone); (4) no file reads the environment variables or the process's command line by itself - a personal value
/// never arrives that way, and the settings come through the configuration that <c>CommandHost.cs</c> builds. The scan reads the whole text; full-line comments are blanked first and keep their line.
/// </summary>
internal static partial class CommandSourceScan
{
    public const string ConsoleOutsideTerminal = "touches Console outside Commands/Terminal.cs (09 13.2 item 4)";

    public const string KeyWithEcho = "reads a key without intercept: true (09 13.2 item 4)";

    public const string ContextInCommand = "names the database context outside Commands/CommandHost.cs (09 13.2 item 3)";

    public const string EnvironmentOrCommandLine = "reads the environment or the process's command line outside Commands/CommandHost.cs (05 8.5.1, 8.5.8)";

    public const string CommandsFolder = "src/Shopfolio.Web/Commands/";

    private const string TerminalFile = "src/Shopfolio.Web/Commands/Terminal.cs";

    private const string HostFile = "src/Shopfolio.Web/Commands/CommandHost.cs";

    public static bool Applies(string relativePath)
    {
        string path = relativePath.Replace('\\', '/');
        return path.Contains(CommandsFolder, StringComparison.Ordinal) && path.EndsWith(".cs", StringComparison.Ordinal);
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        string clean = BlankCommentLines(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];
        if (!path.EndsWith(TerminalFile, StringComparison.Ordinal))
        {
            Add(findings, ConsoleOutsideTerminal, ConsoleUse(), clean);
        }
        else
        {
            Add(findings, KeyWithEcho, KeyRead(), clean);
        }

        if (!path.EndsWith(HostFile, StringComparison.Ordinal))
        {
            Add(findings, ContextInCommand, ContextName(), clean);
            Add(findings, EnvironmentOrCommandLine, EnvironmentRead(), clean);
        }

        findings.Sort((a, b) => a.Line.CompareTo(b.Line));
        return findings;
    }

    private static void Add(List<(string Rule, int Line)> findings, string rule, Regex pattern, string text)
    {
        foreach (Match match in pattern.Matches(text))
        {
            findings.Add((rule, text.AsSpan(0, match.Index).Count('\n') + 1));
        }
    }

    private static string BlankCommentLines(string text)
    {
        string[] lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].TrimStart().StartsWith("//", StringComparison.Ordinal))
            {
                lines[i] = string.Empty;
            }
        }

        return string.Join('\n', lines);
    }

    [GeneratedRegex(@"\bConsole\s*\.", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ConsoleUse();

    // Every ReadKey( call whose argument list does not carry intercept: true.
    [GeneratedRegex(@"\bReadKey\s*\((?![^)]*\bintercept\s*:\s*true\b)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex KeyRead();

    [GeneratedRegex(@"\bAppDbContext\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ContextName();

    [GeneratedRegex(@"\b(?:GetEnvironmentVariable|GetEnvironmentVariables|GetCommandLineArgs|CommandLine)\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex EnvironmentRead();
}
