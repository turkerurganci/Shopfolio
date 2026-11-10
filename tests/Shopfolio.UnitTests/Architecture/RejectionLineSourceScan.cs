using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The scan of the rejection line (T213 criterion 8, KK8; rewritten in T411 criterion 5, KK5; 09 8.2 item 3, 18.2 item 4; D-132, D-89 K3): a line that
/// carries an error code is written in two places only - the catching layer's unexpected-error line (<see cref="LayerFile"/>) and the rejection writer
/// (<see cref="WriterFile"/>) - so a refusal that writes its own line, or any other line that carries a <c>code</c> or a <c>fields</c> array, is a finding.
/// The file is read by its lexical layer like the template scan (<see cref="LogSourceText"/>: comments and strings told apart; plain, verbatim,
/// interpolated, raw and joined strings are one literal each; a comment is not a finding and a <c>//</c> in a string hides nothing). Five forms are
/// found: the name of the rejection event outside the writer (in code or in a string); a template property named for the code or the fields (see
/// <see cref="IsCodeOrFieldsName"/>: <c>Code</c>, <c>Fields</c> and the names that contain <c>ErrorCode</c> or <c>RejectionCode</c>, in any case, with a
/// destructuring mark, an alignment or a format) in any string of the file - a template kept in a constant is read as well; the same names handed to
/// <c>BeginScope</c>, <c>ForContext</c>, <c>PushProperty</c> or <c>WithProperty</c>; a value of the <c>ErrorCode</c> type handed to a log call or an
/// enricher as an argument (<see cref="ErrorCodeArgument"/>: the line then carries the code under any property name); and the same words on a line of
/// a view (<c>.cshtml</c>, <c>.razor</c>), which cannot be read as C# and is read line by line. The two writers are named by their exact paths, never
/// by a pattern, and the real files are checked to still hold the forms (the scan is not empty).
/// </summary>
internal static partial class RejectionLineSourceScan
{
    public const string EventOutsideTheWriter = "RejectionEventOutsideTheWriter";

    public const string CodeFieldOutsideTheTwoWriters = "CodeOrFieldsPropertyOutsideTheTwoWriters";

    public const string ScopeProperty = "CodeOrFieldsPropertyInAScope";

    public const string ErrorCodeArgument = "ErrorCodeValueInALogCallOutsideTheTwoWriters";

    public const string WriterFile = "src/Shopfolio.Web/Infrastructure/Errors/RejectionLog.cs";

    public const string LayerFile = "src/Shopfolio.Web/Infrastructure/Errors/UnexpectedErrorLayer.cs";

    private const string EventListFile = "src/Shopfolio.Web/Infrastructure/Logging/LogEvents.cs";

    private static readonly string[] MarkupExtensions = [".cshtml", ".razor"];

    /// <summary>The scan applies to every source file of the application, code and views; the two writers and the event list are the exact files it spares, each for its own rule.</summary>
    public static bool AppliesTo(string relativePath) =>
        Normalize(relativePath).StartsWith("src/", StringComparison.Ordinal)
        && (Normalize(relativePath).EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || IsMarkup(relativePath));

    /// <summary>True for a name that writes the code or the fields of a refusal under the formatter's field names: <c>Code</c>, <c>Fields</c>, or a name that contains <c>ErrorCode</c> or <c>RejectionCode</c> (any case).</summary>
    public static bool IsCodeOrFieldsName(string name) =>
        name.Equals("Code", StringComparison.OrdinalIgnoreCase)
        || name.Equals("Fields", StringComparison.OrdinalIgnoreCase)
        || name.Contains("ErrorCode", StringComparison.OrdinalIgnoreCase)
        || name.Contains("RejectionCode", StringComparison.OrdinalIgnoreCase);

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = Normalize(relativePath);
        bool writer = string.Equals(path, WriterFile, StringComparison.Ordinal);
        bool layer = string.Equals(path, LayerFile, StringComparison.Ordinal);
        bool eventList = string.Equals(path, EventListFile, StringComparison.Ordinal);
        List<(string Rule, int Line)> findings = [];

        if (IsMarkup(path))
        {
            ScanViewLines(text, writer, layer, findings);
            return [.. findings.Distinct().OrderBy(f => f.Line)];
        }

        LogSourceText source = LogSourceText.Parse(text);
        if (!writer && !eventList)
        {
            findings.AddRange(RejectionEvent().Matches(source.Code).Select(m => (EventOutsideTheWriter, source.LineOf(m.Index))));
            findings.AddRange(source.Literals.Where(l => RejectionEvent().IsMatch(l.Template)).Select(l => (EventOutsideTheWriter, l.Line)));
        }

        if (!writer && !layer)
        {
            foreach (SourceLiteral literal in source.Literals)
            {
                foreach (Match match in Placeholder().Matches(literal.Template))
                {
                    if (IsCodeOrFieldsName(match.Groups["name"].Value.TrimStart('@', '$')))
                    {
                        findings.Add((CodeFieldOutsideTheTwoWriters, literal.Line + literal.Template[..match.Index].Count(c => c == '\n')));
                    }
                }
            }

            foreach (LogCallSite site in source.CallSites())
            {
                if (site.Kind == LogCallKind.Scope)
                {
                    findings.AddRange(site.Literals
                        .Where(l => PlainName().IsMatch(l.Template) && IsCodeOrFieldsName(l.Template.TrimStart('@', '$')))
                        .Select(l => (ScopeProperty, l.Line)));
                }

                if (site.Kind is LogCallKind.Log or LogCallKind.Scope or LogCallKind.Audit)
                {
                    // The value of the ErrorCode type as an argument: the line carries the code under whatever property name the template gives it.
                    foreach ((int Start, int End) argument in site.Arguments)
                    {
                        string code = source.ArgumentCode(argument);
                        if (ErrorCodeValue().IsMatch(code))
                        {
                            findings.Add((ErrorCodeArgument, source.LineOf(argument.Start + (code.Length - code.TrimStart().Length))));
                        }
                    }
                }
            }
        }

        return [.. findings.Distinct().OrderBy(f => f.Line).ThenBy(f => f.Rule, StringComparer.Ordinal)];
    }

    private static void ScanViewLines(string text, bool writer, bool layer, List<(string Rule, int Line)> findings)
    {
        // A view is not C#: no comment is dropped and no string is parsed, so every form of the words is a finding line by line.
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (!writer && RejectionEvent().IsMatch(line))
            {
                findings.Add((EventOutsideTheWriter, i + 1));
            }

            if (!writer && !layer)
            {
                if (Placeholder().Matches(line).Any(m => IsCodeOrFieldsName(m.Groups["name"].Value.TrimStart('@', '$'))))
                {
                    findings.Add((CodeFieldOutsideTheTwoWriters, i + 1));
                }

                if (ViewScopeName().IsMatch(line))
                {
                    findings.Add((ScopeProperty, i + 1));
                }

                if (LogWordOnLine().IsMatch(line) && ErrorCodeValue().IsMatch(line))
                {
                    findings.Add((ErrorCodeArgument, i + 1));
                }
            }
        }
    }

    private static bool IsMarkup(string path) => MarkupExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);

    private static string Normalize(string path) => path.Replace('\\', '/');

    // The name of the event, as a constant of the list or spelled out.
    [GeneratedRegex(@"\bValidationRejected\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RejectionEvent();

    // {Code} {@Fields} {$ErrorCode} {code,5} {Reason:l}: the name of a template property.
    [GeneratedRegex(@"\{(?<name>[@$]?\w+)(?:[,:][^}]*)?\}", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Placeholder();

    [GeneratedRegex(@"^[@$]?\w+$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PlainName();

    // ErrorCode.X, ErrorCode.X.ToString(), nameof(ErrorCode.X), (ErrorCode)n, a variable cast: the type name as a word.
    [GeneratedRegex(@"\bErrorCode\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ErrorCodeValue();

    [GeneratedRegex(@"\.\s*(?:Log\w*|Verbose|Debug|Information|Warning|Error|Fatal)\s*\(|\bBeginScope\b|\bForContext\b|\bPushProperty\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LogWordOnLine();

    // ForContext("Code", ...), PushProperty("Fields", ...), a scope dictionary key: the quoted name next to a scope or enricher call on the same line of a view.
    [GeneratedRegex(@"(?:ForContext|PushProperty|BeginScope|Enrich\w*|WithProperty)\b[^;]*""@?(?:Code|Fields)""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ViewScopeName();
}
