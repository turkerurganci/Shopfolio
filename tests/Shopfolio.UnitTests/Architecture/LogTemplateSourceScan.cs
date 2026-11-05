using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scans of the writing layer of the masking (T231; 09 18.2 items 2a, 2c, 7): (1) a log template names no property of personal data —
/// the names come from <see cref="ApplicationLogFormatter.ForbiddenPropertyNames"/>, the same list the formatter leaves out — and none of the
/// request field names the enricher writes (<see cref="RequestFieldNames"/>): a template that brought its own <c>{Path}</c> would put an unmasked
/// second <c>path</c> next to the masked one and into the message (D-88); (2) the data access layer puts no value into an exception message:
/// <c>EnableSensitiveDataLogging</c> and the provider's <c>Include Error Detail</c> are not in <c>src/</c>. A template is the message of a
/// <c>[LoggerMessage]</c>, the first string argument of a log call, and the template of an <c>AuditLine</c>; comments are not scanned.
/// </summary>
internal static partial class LogTemplateSourceScan
{
    public const string ForbiddenTemplateName = "PersonalDataNameInALogTemplate";

    public const string RequestFieldInATemplate = "RequestFieldNameInALogTemplate";

    public const string ValueInExceptionMessage = "ValueInExceptionMessageSwitchedOn";

    /// <summary>The request fields that only the enricher writes: the masked <c>path</c> and the <c>method</c> (the other three are not template names, see D-88).</summary>
    public static readonly IReadOnlyList<string> RequestFieldNames = ["Path", "Method"];

    /// <summary>The names in a template (<c>{Email}</c>, <c>{@Order}</c>, <c>{Count:N0}</c> give <c>Email</c>, <c>Order</c>, <c>Count</c>).</summary>
    public static IReadOnlyList<string> Placeholders(string template) =>
        [.. Placeholder().Matches(template).Select(m => m.Groups["name"].Value.TrimStart('@', '$'))];

    public static bool IsPersonalDataName(string name) =>
        ApplicationLogFormatter.ForbiddenPropertyNames.Any(forbidden => name.EndsWith(forbidden, StringComparison.OrdinalIgnoreCase));

    public static bool IsRequestFieldName(string name) =>
        RequestFieldNames.Contains(name, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<(string Rule, int Line)> ScanTemplates(string text)
    {
        string code = WithoutComments(text);
        List<(string Rule, int Line)> findings = [];
        foreach (Regex pattern in new[] { LoggerMessageTemplate(), LogCallTemplate(), AuditLineTemplate() })
        {
            foreach (Match match in pattern.Matches(code))
            {
                Group template = match.Groups["t"];
                foreach (string name in Placeholders(template.Value))
                {
                    if (IsPersonalDataName(name))
                    {
                        findings.Add((ForbiddenTemplateName, LineOf(code, template.Index)));
                    }
                    else if (IsRequestFieldName(name))
                    {
                        findings.Add((RequestFieldInATemplate, LineOf(code, template.Index)));
                    }
                }
            }
        }

        return [.. findings.Distinct().OrderBy(f => f.Line)];
    }

    public static IReadOnlyList<(string Rule, int Line)> ScanExceptionMessageSwitches(string text)
    {
        string code = WithoutComments(text);
        return [.. SensitiveSwitch().Matches(code).Select(m => (ValueInExceptionMessage, LineOf(code, m.Index)))];
    }

    private static string WithoutComments(string text) =>
        string.Join('\n', text.ReplaceLineEndings("\n").Split('\n').Select(line =>
        {
            int comment = line.IndexOf("//", StringComparison.Ordinal);
            return comment < 0 ? line : line[..comment];
        }));

    private static int LineOf(string text, int index) => text.AsSpan(0, index).Count('\n') + 1;

    [GeneratedRegex(@"\{(?<name>[@$]?\w+)(?:[,:][^}]*)?\}", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Placeholder();

    // [LoggerMessage(Level = ..., Message = "...")]
    [GeneratedRegex(@"\bMessage\s*=\s*""(?<t>[^""]*)""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LoggerMessageTemplate();

    // logger.LogWarning("...", ...), logger.Error(exception, "...", ...)
    [GeneratedRegex(@"\.(?:Log\w*|Verbose|Debug|Information|Warning|Error|Fatal)\(\s*(?:[^""()]*,\s*)?""(?<t>[^""]*)""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LogCallTemplate();

    // new AuditLine(LogEvents.X, "{Event} {Kind} {RecordId}", ...)
    [GeneratedRegex(@"\bnew\s+AuditLine\s*\([^""]*,\s*""(?<t>[^""]*)""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex AuditLineTemplate();

    // The connection string keyword has two spellings; the switch is a method name.
    [GeneratedRegex(@"EnableSensitiveDataLogging|Include\s*Error\s*Detail", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SensitiveSwitch();
}
