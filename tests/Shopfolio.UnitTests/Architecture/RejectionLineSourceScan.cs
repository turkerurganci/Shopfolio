using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The scan of the rejection line (T213 criterion 8, KK8; 09 8.2 item 3, 18.2 item 4; D-132): a line that carries an error code is written in two
/// places only - the catching layer's unexpected-error line (<see cref="LayerFile"/>) and the rejection writer (<see cref="WriterFile"/>) - so a refusal
/// that writes its own line, or any other line that carries a <c>code</c> or a <c>fields</c> array, is a finding. Three forms are found, each on a
/// line (comments are not dropped and a string is not parsed, so every quoting form of the template - plain, verbatim, interpolated, raw, joined - is
/// caught, line by line): the name of the rejection event outside the writer, a template property named <c>Code</c> or <c>Fields</c> (in any case, with a
/// destructuring mark, an alignment or a format; the hole of an interpolated string is no template property), and the same names handed to a scope or an enricher as a string. The two files are named by their exact paths, never
/// by a pattern, and the real files are checked to still hold the forms (the scan is not empty).
/// </summary>
internal static partial class RejectionLineSourceScan
{
    public const string EventOutsideTheWriter = "RejectionEventOutsideTheWriter";

    public const string CodeFieldOutsideTheTwoWriters = "CodeOrFieldsPropertyOutsideTheTwoWriters";

    public const string ScopeProperty = "CodeOrFieldsPropertyInAScope";

    public const string WriterFile = "src/Shopfolio.Web/Infrastructure/Errors/RejectionLog.cs";

    public const string LayerFile = "src/Shopfolio.Web/Infrastructure/Errors/UnexpectedErrorLayer.cs";

    private const string EventListFile = "src/Shopfolio.Web/Infrastructure/Logging/LogEvents.cs";

    /// <summary>The scan applies to every source file of the application; the two writers and the event list are the exact files it spares, each for its own rule.</summary>
    public static bool AppliesTo(string relativePath) =>
        Normalize(relativePath).StartsWith("src/", StringComparison.Ordinal);

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = Normalize(relativePath);
        bool writer = string.Equals(path, WriterFile, StringComparison.Ordinal);
        bool layer = string.Equals(path, LayerFile, StringComparison.Ordinal);
        bool eventList = string.Equals(path, EventListFile, StringComparison.Ordinal);

        // No comment is dropped: a comment line that spells a forbidden form is a finding too (a comment cannot hide a string, and a string cannot hide in a
        // comment-looking tail of a raw or verbatim literal); the files that need the words in a comment use other words.
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        List<(string Rule, int Line)> findings = [];
        for (int i = 0; i < lines.Length; i++)
        {
            string code = lines[i];
            if (!writer && !eventList && RejectionEvent().IsMatch(code))
            {
                findings.Add((EventOutsideTheWriter, i + 1));
            }

            if (!writer && !layer && HasTemplateProperty(code))
            {
                findings.Add((CodeFieldOutsideTheTwoWriters, i + 1));
            }

            if (!writer && !layer && ScopeName().IsMatch(code))
            {
                findings.Add((ScopeProperty, i + 1));
            }
        }

        return findings;
    }

    private static string Normalize(string path) => path.Replace('\\', '/');

    // The name of the event, as a constant of the list or spelled out.
    [GeneratedRegex(@"\bValidationRejected\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RejectionEvent();

    // {Code} {@Code} {$Code} {Code,5} {Code:l} {Fields} {@Fields}: the property whose name the formatter writes as "code" or "fields" (the convention of the
    // templates is PascalCase, so this form is a finding on any line).
    [GeneratedRegex(@"\{[@$]?(?:Code|Fields)\s*[,:}]", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PascalTemplateProperty();

    // {code} {CODE} {@fields}: the same property in another spelling; the formatter writes it all the same. Only a line without an interpolated string is a
    // finding, because the hole of an interpolated string ($"{code}") is a variable and no template property.
    [GeneratedRegex(@"\{[@$]?(?:Code|Fields)\s*[,:}]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex AnyCaseTemplateProperty();

    [GeneratedRegex(@"\$@?""|@\$""|\$""""""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex InterpolatedString();

    private static bool HasTemplateProperty(string line) =>
        PascalTemplateProperty().IsMatch(line) || (!InterpolatedString().IsMatch(line) && AnyCaseTemplateProperty().IsMatch(line));

    // ForContext("Code", ...), PushProperty("Fields", ...), a scope dictionary key: the quoted name next to a scope or enricher call on the same line.
    [GeneratedRegex(@"(?:ForContext|PushProperty|BeginScope|Enrich\w*|WithProperty)\b[^;]*""@?(?:Code|Fields)""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ScopeName();
}
