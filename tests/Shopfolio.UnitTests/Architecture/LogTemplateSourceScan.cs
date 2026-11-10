using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scans of the writing layer of the masking (T231, rewritten in T411; 09 18.1 item 4, 18.2 items 2a, 2c, 7; D-89, D-90):
/// (1) a log template names no property of personal data - the rule is <see cref="ApplicationLogFormatter.IsForbiddenName"/>, the same method the
/// formatter leaves the property out by (the name <em>contains</em> a forbidden word, any case, unless it is on the short allow list) - and none of
/// the request field names the enricher writes (<see cref="RequestFieldNames"/>): a template that brought its own <c>{Path}</c> would put an unmasked
/// second <c>path</c> next to the masked one and into the message (D-88); (2) no exception is handed to a template as an argument
/// (<see cref="ExceptionAsArgument"/>): the exception of a line belongs to the exception slot, where the formatter writes its type and never its
/// message; (3) the data access layer puts no value into an exception message: <c>EnableSensitiveDataLogging</c> and the provider's
/// <c>Include Error Detail</c> are not in <c>src/</c>.
/// The file is read by its lexical layer (<see cref="LogSourceText"/>): comments and strings are told apart, so a <c>//</c> in a string hides
/// nothing, and every string form (plain, verbatim, interpolated, raw, joined) is one template. A template is a string in: a log call (<c>LogXxx</c>,
/// the level methods of the library, <c>Write</c> with a level), <c>[LoggerMessage]</c> (named or positional, with or without the <c>Attribute</c>
/// suffix), <c>LoggerMessage.Define</c>, <c>new AuditLine</c>, a <c>const string</c>; a property name is a string in <c>BeginScope</c>,
/// <c>ForContext</c>, <c>PushProperty</c>, <c>WithProperty</c> (also as the key of a dictionary scope).
/// </summary>
internal static partial class LogTemplateSourceScan
{
    public const string ForbiddenTemplateName = "PersonalDataNameInALogTemplate";

    public const string RequestFieldInATemplate = "RequestFieldNameInALogTemplate";

    public const string ExceptionAsArgument = "ExceptionAsALogTemplateArgument";

    public const string ValueInExceptionMessage = "ValueInExceptionMessageSwitchedOn";

    /// <summary>The request fields that only the enricher writes: the masked <c>path</c> and the <c>method</c> (the other three are not template names, see D-88).</summary>
    public static readonly IReadOnlyList<string> RequestFieldNames = ["Path", "Method"];

    /// <summary>The names in a template (<c>{Email}</c>, <c>{@Order}</c>, <c>{Count:N0}</c> give <c>Email</c>, <c>Order</c>, <c>Count</c>).</summary>
    public static IReadOnlyList<string> Placeholders(string template) =>
        [.. Placeholder().Matches(template).Select(m => m.Groups["name"].Value.TrimStart('@', '$'))];

    public static bool IsPersonalDataName(string name) => ApplicationLogFormatter.IsForbiddenName(name);

    public static bool IsRequestFieldName(string name) =>
        RequestFieldNames.Contains(name, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<(string Rule, int Line)> ScanTemplates(string text)
    {
        LogSourceText source = LogSourceText.Parse(text);
        List<(string Rule, int Line)> findings = [];
        foreach (LogCallSite site in source.CallSites())
        {
            foreach (SourceLiteral literal in site.Literals)
            {
                IEnumerable<string> names = Placeholders(literal.Template);
                if (site.Kind == LogCallKind.Constant && !names.Contains("Event", StringComparer.Ordinal))
                {
                    // A constant is a template only when it is shaped like one: every template of the application opens with {Event} (09 18.2 item 3);
                    // route patterns such as "/x/{key}" are constants too and are no log template.
                    continue;
                }

                if (site.Kind == LogCallKind.Scope && !names.Any() && PlainName().IsMatch(literal.Template))
                {
                    // A property name handed to a scope or an enricher is the whole string: ForContext("Email", value), ["Path"] = value.
                    names = [literal.Template.TrimStart('@', '$')];
                }

                foreach (string name in names)
                {
                    int line = literal.Line + literal.Template[..Math.Max(0, literal.Template.IndexOf(name, StringComparison.Ordinal))].Count(c => c == '\n');
                    if (IsPersonalDataName(name))
                    {
                        findings.Add((ForbiddenTemplateName, line));
                    }
                    else if (IsRequestFieldName(name))
                    {
                        findings.Add((RequestFieldInATemplate, line));
                    }
                }
            }

            if (site.Kind == LogCallKind.Log)
            {
                findings.AddRange(ExceptionArguments(source, site));
            }

            if (site.Kind == LogCallKind.LoggerMessage)
            {
                findings.AddRange(ExceptionParametersNamedInTheTemplate(source, site));
            }
        }

        return [.. findings.Distinct().OrderBy(f => f.Line).ThenBy(f => f.Rule, StringComparer.Ordinal)];
    }

    public static IReadOnlyList<(string Rule, int Line)> ScanExceptionMessageSwitches(string text)
    {
        LogSourceText source = LogSourceText.Parse(text);
        List<(string Rule, int Line)> findings = [];
        // The method name is code; the connection string keyword is a string (a comment that names either is nothing).
        findings.AddRange(SensitiveSwitch().Matches(source.Code).Select(m => (ValueInExceptionMessage, source.LineOf(m.Index))));
        findings.AddRange(source.Literals.Where(l => SensitiveSwitch().IsMatch(l.Template)).Select(l => (ValueInExceptionMessage, l.Line)));
        return [.. findings.Distinct().OrderBy(f => f.Line)];
    }

    // An exception handed to the template as an argument writes its whole text (ToString() and Message carry the message): the arguments after the
    // template that name an exception (a variable declared with an exception type, the usual names, .InnerException) are a finding. The exception
    // slot before the template is the right place and is not read.
    private static IEnumerable<(string Rule, int Line)> ExceptionArguments(LogSourceText source, LogCallSite site)
    {
        int template = -1;
        for (int i = 0; i < site.Arguments.Count; i++)
        {
            if (source.IsLiteralArgument(site.Arguments[i]))
            {
                template = i;
                break;
            }
        }

        if (template < 0)
        {
            yield break;
        }

        HashSet<string> names = ExceptionNames(source.Code);
        for (int i = template + 1; i < site.Arguments.Count; i++)
        {
            string argument = source.ArgumentCode(site.Arguments[i]);
            if (names.Any(name => Regex.IsMatch(argument, @"(?<![\w.])" + Regex.Escape(name) + @"\b", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)))
                || ExceptionMember().IsMatch(argument))
            {
                yield return (ExceptionAsArgument, source.LineOf(site.Arguments[i].Start + (argument.Length - argument.TrimStart().Length)));
            }
        }
    }

    // [LoggerMessage] methods: an Exception parameter is the exception of the line unless the template names it, then it is an argument.
    private static IEnumerable<(string Rule, int Line)> ExceptionParametersNamedInTheTemplate(LogSourceText source, LogCallSite site)
    {
        int open = source.Code.IndexOf('(', site.Close + 1);
        int semicolon = source.Code.IndexOf(';', site.Close + 1);
        if (open < 0 || (semicolon >= 0 && open > semicolon))
        {
            yield break;
        }

        int close = source.Code.IndexOf(')', open);
        if (close < 0)
        {
            yield break;
        }

        string[] parameters = [.. ExceptionParameter().Matches(source.Code[open..close]).Select(m => m.Groups["n"].Value)];
        foreach (SourceLiteral literal in site.Literals)
        {
            if (Placeholders(literal.Template).Any(placeholder => parameters.Contains(placeholder, StringComparer.OrdinalIgnoreCase)))
            {
                yield return (ExceptionAsArgument, literal.Line);
            }
        }
    }

    private static HashSet<string> ExceptionNames(string code)
    {
        HashSet<string> names = new(StringComparer.Ordinal) { "ex", "exception", "innerException" };
        foreach (Match match in DeclaredException().Matches(code))
        {
            names.Add(match.Groups["n"].Value);
        }

        return names;
    }

    [GeneratedRegex(@"\{(?<name>[@$]?\w+)(?:[,:][^}]*)?\}", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Placeholder();

    [GeneratedRegex(@"^[@$]?\w+$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PlainName();

    // catch (SqlException e), Exception ex = ..., void M(Exception error): the type is an exception type, the next word is the variable.
    [GeneratedRegex(@"\b\w*Exception\??\s+(?<n>[A-Za-z_]\w*)\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex DeclaredException();

    [GeneratedRegex(@"\b\w*Exception\??\s+(?<n>[A-Za-z_]\w*)\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ExceptionParameter();

    // result.Exception, e.InnerException, args.Exception
    [GeneratedRegex(@"\.\s*(?:Inner)?Exception\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ExceptionMember();

    // The connection string keyword has two spellings; the switch is a method name.
    [GeneratedRegex(@"EnableSensitiveDataLogging|Include\s*Error\s*Detail", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SensitiveSwitch();
}
