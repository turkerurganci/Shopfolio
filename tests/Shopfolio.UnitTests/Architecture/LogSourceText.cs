using System.Text;
using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>A string literal of a C# file: where it sits, its line and its template text (09 18.2 item 2a; D-89).</summary>
/// <param name="Start">Index of the first character of the literal, prefix included (<c>$</c>, <c>@</c>).</param>
/// <param name="End">Index after the last character of the literal.</param>
/// <param name="Template">
/// The text a log template would be read from: the escapes of a verbatim string are resolved; for an interpolated string every hole is replaced by a
/// space and the doubled braces are single braces, so <c>$"x {{Email}} {name}"</c> reads <c>x {Email}  </c> (a hole is code, not a template property).
/// </param>
/// <param name="Line">Line (1-based) of the first character.</param>
internal sealed record SourceLiteral(int Start, int End, string Template, int Line);

/// <summary>The family of a call that carries a template or a property name (T411; 09 18.2 item 2a).</summary>
internal enum LogCallKind
{
    /// <summary><c>logger.LogWarning(...)</c>, <c>logger.Error(...)</c>, <c>logger.Log(...)</c>, <c>logger.Write(LogEventLevel...)</c>.</summary>
    Log,

    /// <summary><c>[LoggerMessage(...)]</c> and <c>[LoggerMessageAttribute(...)]</c>, named or positional.</summary>
    LoggerMessage,

    /// <summary><c>LoggerMessage.Define(...)</c> and <c>LoggerMessage.DefineScope(...)</c>.</summary>
    Define,

    /// <summary><c>new AuditLine(...)</c>.</summary>
    Audit,

    /// <summary><c>BeginScope(...)</c>, <c>ForContext(...)</c>, <c>PushProperty(...)</c>, <c>WithProperty(...)</c>: the strings are property names.</summary>
    Scope,

    /// <summary>The initializer of a <c>const string</c>: a template kept in a constant and passed by name.</summary>
    Constant,
}

/// <summary>One call (or declaration) with its parenthesised span and the literals in it.</summary>
internal sealed record LogCallSite(LogCallKind Kind, int Line, int Open, int Close, IReadOnlyList<SourceLiteral> Literals, IReadOnlyList<(int Start, int End)> Arguments);

/// <summary>
/// A C# file read the way the compiler reads its lexical layer (T411; D-89; K-1579): comments and literals are told apart, so a <c>//</c> inside a string
/// is no comment, a quote inside a comment opens no string, and every string form - plain, verbatim, interpolated, raw (with any number of quotes and
/// dollars), joined with <c>+</c> - is one literal. <see cref="Code"/> is the file with every comment and literal blanked to spaces (same length, same
/// line breaks), so the structure (parentheses, identifiers) is read without the text inside strings; the literals are kept apart.
/// </summary>
internal sealed partial class LogSourceText
{
    private readonly int[] _lineStarts;

    private LogSourceText(string code, IReadOnlyList<SourceLiteral> literals, int[] lineStarts)
    {
        Code = code;
        Literals = literals;
        _lineStarts = lineStarts;
    }

    public string Code { get; }

    public IReadOnlyList<SourceLiteral> Literals { get; }

    public static LogSourceText Parse(string text)
    {
        string source = text.ReplaceLineEndings("\n");
        char[] code = source.ToCharArray();
        List<int> lineStarts = [0];
        for (int n = 0; n < source.Length; n++)
        {
            if (source[n] == '\n')
            {
                lineStarts.Add(n + 1);
            }
        }

        int[] starts = [.. lineStarts];
        List<SourceLiteral> literals = [];
        int i = 0;
        while (i < source.Length)
        {
            char c = source[i];
            if (c == '/' && i + 1 < source.Length && source[i + 1] == '/')
            {
                int end = source.IndexOf('\n', i);
                end = end < 0 ? source.Length : end;
                Blank(code, i, end);
                i = end;
            }
            else if (c == '/' && i + 1 < source.Length && source[i + 1] == '*')
            {
                int close = source.IndexOf("*/", i + 2, StringComparison.Ordinal);
                int end = close < 0 ? source.Length : close + 2;
                Blank(code, i, end);
                i = end;
            }
            else if (c == '\'')
            {
                int j = i + 1;
                while (j < source.Length && source[j] != '\'' && source[j] != '\n')
                {
                    j += source[j] == '\\' ? 2 : 1;
                }

                int end = Math.Min(j + 1, source.Length);
                Blank(code, i, end);
                i = end;
            }
            else if (c == '"' || ((c == '$' || c == '@') && StartsLiteral(source, i)))
            {
                (int end, string template) = ReadLiteral(source, i);
                literals.Add(new SourceLiteral(i, end, template, LineIndex(starts, i) + 1));
                Blank(code, i, end);
                i = end;
            }
            else
            {
                i++;
            }
        }

        return new LogSourceText(new string(code), literals, starts);
    }

    public int LineOf(int index) => LineIndex(_lineStarts, index) + 1;

    /// <summary>The calls and declarations that carry a template or a property name, in the order of the file.</summary>
    public IReadOnlyList<LogCallSite> CallSites()
    {
        List<LogCallSite> sites = [];
        Collect(sites, LogCallKind.Log, LogCall(), openAtEnd: true);
        Collect(sites, LogCallKind.LoggerMessage, LoggerMessageAttribute(), openAtEnd: true);
        Collect(sites, LogCallKind.Define, LoggerMessageDefine(), openAtEnd: true);
        Collect(sites, LogCallKind.Audit, AuditLineCall(), openAtEnd: true);
        Collect(sites, LogCallKind.Scope, ScopeCall(), openAtEnd: true);
        foreach (Match match in ConstantString().Matches(Code))
        {
            int end = Code.IndexOf(';', match.Index + match.Length);
            end = end < 0 ? Code.Length : end;
            sites.Add(new LogCallSite(LogCallKind.Constant, LineOf(match.Index), match.Index + match.Length, end, LiteralsIn(match.Index + match.Length, end), [(match.Index + match.Length, end)]));
        }

        return [.. sites.OrderBy(s => s.Open)];
    }

    /// <summary>The text of an argument as code (comments and literals blanked).</summary>
    public string ArgumentCode((int Start, int End) argument) => Code[argument.Start..argument.End];

    /// <summary>True when the argument is nothing but literals (joined with <c>+</c>, perhaps named): the template argument of a log call.</summary>
    public bool IsLiteralArgument((int Start, int End) argument) =>
        Literals.Any(l => l.Start >= argument.Start && l.End <= argument.End) && JoinedLiterals().IsMatch(ArgumentCode(argument));

    private void Collect(List<LogCallSite> sites, LogCallKind kind, Regex pattern, bool openAtEnd)
    {
        foreach (Match match in pattern.Matches(Code))
        {
            int open = openAtEnd ? match.Index + match.Length - 1 : match.Index;
            int close = MatchingParenthesis(open);
            IReadOnlyList<(int Start, int End)> arguments = Arguments(open, close);
            if (kind == LogCallKind.Log && match.Groups["w"].Success && !arguments.Any(a => WriteLevel().IsMatch(ArgumentCode(a))))
            {
                // Write(...) is a log call only when its first argument is a level of the library; other Write calls are streams and writers.
                continue;
            }

            sites.Add(new LogCallSite(kind, LineOf(match.Index), open, close, LiteralsIn(open, close), arguments));
        }
    }

    private List<SourceLiteral> LiteralsIn(int start, int end) => [.. Literals.Where(l => l.Start >= start && l.End <= end + 1)];

    private int MatchingParenthesis(int open)
    {
        int depth = 0;
        for (int i = open; i < Code.Length; i++)
        {
            if (Code[i] == '(')
            {
                depth++;
            }
            else if (Code[i] == ')' && --depth == 0)
            {
                return i;
            }
        }

        return Code.Length - 1;
    }

    // The arguments of a call at its top level: commas inside (), [], {} and the angle brackets of a generic type do not split.
    private List<(int Start, int End)> Arguments(int open, int close)
    {
        List<(int Start, int End)> arguments = [];
        int depth = 0;
        int angle = 0;
        int start = open + 1;
        for (int i = open + 1; i < close; i++)
        {
            char c = Code[i];
            if (c is '(' or '[' or '{')
            {
                depth++;
            }
            else if (c is ')' or ']' or '}')
            {
                depth--;
            }
            else if (c == '<' && depth == 0 && i > 0 && (char.IsLetterOrDigit(Code[i - 1]) || Code[i - 1] == '_'))
            {
                angle++;
            }
            else if (c == '>' && angle > 0 && depth == 0 && Code[i - 1] is not ('=' or '-'))
            {
                angle--;
            }
            else if (c == ',' && depth == 0 && angle == 0)
            {
                arguments.Add((start, i));
                start = i + 1;
            }
        }

        if (close > start || arguments.Count > 0)
        {
            arguments.Add((start, Math.Max(start, close)));
        }

        return arguments;
    }

    private static void Blank(char[] code, int start, int end)
    {
        for (int i = start; i < end && i < code.Length; i++)
        {
            if (code[i] != '\n')
            {
                code[i] = ' ';
            }
        }
    }

    private static int LineIndex(int[] starts, int index)
    {
        int position = Array.BinarySearch(starts, index);
        return position >= 0 ? position : ~position - 1;
    }

    private static bool IsIdentifierChar(char c) => char.IsLetterOrDigit(c) || c == '_';

    // $"  @"  $@"  @$"  $$"""  - a prefix of dollars and ats that ends in a quote, not preceded by an identifier (so the end of a name is no prefix).
    private static bool StartsLiteral(string source, int i)
    {
        int j = i;
        while (j < source.Length && (source[j] == '$' || source[j] == '@'))
        {
            j++;
        }

        return j < source.Length && source[j] == '"' && (i == 0 || !IsIdentifierChar(source[i - 1]));
    }

    private static (int End, string Template) ReadLiteral(string source, int start)
    {
        int j = start;
        int dollars = 0;
        bool verbatim = false;
        while (j < source.Length && (source[j] == '$' || source[j] == '@'))
        {
            if (source[j] == '$')
            {
                dollars++;
            }
            else
            {
                verbatim = true;
            }

            j++;
        }

        int quotes = 0;
        while (j + quotes < source.Length && source[j + quotes] == '"')
        {
            quotes++;
        }

        StringBuilder template = new();
        if (quotes >= 3 && !verbatim)
        {
            int end = ReadBody(source, j + quotes, verbatim: false, dollars, quotes, template);
            return (end, template.ToString());
        }

        int bodyEnd = ReadBody(source, j + 1, verbatim, dollars, 0, template);
        return (bodyEnd, template.ToString());
    }

    // Reads the body of a literal from its first character and returns the index after the closing quote(s). rawQuotes > 0: a raw string closed by that many quotes.
    private static int ReadBody(string s, int k, bool verbatim, int dollars, int rawQuotes, StringBuilder template)
    {
        while (k < s.Length)
        {
            char c = s[k];
            if (rawQuotes > 0)
            {
                if (c == '"')
                {
                    int run = QuoteRun(s, k);
                    if (run >= rawQuotes)
                    {
                        template.Append('"', run - rawQuotes);
                        return k + run;
                    }

                    template.Append('"', run);
                    k += run;
                    continue;
                }
            }
            else if (verbatim)
            {
                if (c == '"')
                {
                    if (k + 1 < s.Length && s[k + 1] == '"')
                    {
                        template.Append('"');
                        k += 2;
                        continue;
                    }

                    return k + 1;
                }
            }
            else
            {
                if (c == '\\')
                {
                    template.Append(c);
                    if (k + 1 < s.Length)
                    {
                        template.Append(s[k + 1]);
                    }

                    k += 2;
                    continue;
                }

                if (c == '"')
                {
                    return k + 1;
                }

                if (c == '\n')
                {
                    return k;
                }
            }

            if (dollars > 0 && c == '{')
            {
                k = ReadOpeningBraces(s, k, dollars, rawQuotes > 0, template);
                continue;
            }

            if (dollars > 0 && c == '}' && rawQuotes == 0)
            {
                // A doubled closing brace is one brace in the text; a single one is the end of a hole and never reaches this point.
                template.Append('}');
                k += k + 1 < s.Length && s[k + 1] == '}' ? 2 : 1;
                continue;
            }

            template.Append(c);
            k++;
        }

        return k;
    }

    private static int QuoteRun(string s, int k)
    {
        int run = 0;
        while (k + run < s.Length && s[k + run] == '"')
        {
            run++;
        }

        return run;
    }

    // A run of opening braces in an interpolated literal: doubled braces are literal braces, the last single one (or the last `dollars` of a raw string) opens a hole.
    private static int ReadOpeningBraces(string s, int k, int dollars, bool raw, StringBuilder template)
    {
        int run = 0;
        while (k + run < s.Length && s[k + run] == '{')
        {
            run++;
        }

        int literalBraces;
        bool hole;
        if (raw)
        {
            hole = run >= dollars;
            literalBraces = hole ? run - dollars : run;
        }
        else
        {
            hole = run % 2 == 1;
            literalBraces = run / 2;
        }

        template.Append('{', literalBraces);
        int next = k + run;
        if (!hole)
        {
            return next;
        }

        template.Append(' ');
        return SkipHole(s, next, raw ? dollars : 1);
    }

    // The code of a hole, up to and including its closing brace(s): nested braces and nested strings are skipped.
    private static int SkipHole(string s, int k, int closing)
    {
        int depth = 1;
        while (k < s.Length)
        {
            char c = s[k];
            if (c == '"')
            {
                bool verbatim = k > 0 && s[k - 1] == '@';
                k = ReadBody(s, k + 1, verbatim, 0, 0, new StringBuilder());
                continue;
            }

            if (c == '\'')
            {
                k += k + 2 < s.Length && s[k + 1] == '\\' ? 4 : 3;
                continue;
            }

            if (c == '{')
            {
                depth++;
            }
            else if (c == '}')
            {
                depth--;
                if (depth == 0)
                {
                    int run = 1;
                    while (run < closing && k + run < s.Length && s[k + run] == '}')
                    {
                        run++;
                    }

                    return k + run;
                }
            }

            k++;
        }

        return k;
    }

    // logger.LogWarning(...), logger.Error(...), logger.Log(...), Log.Warning(...), logger.Write(LogEventLevel...): the name of a log call.
    [GeneratedRegex(@"\.\s*(?:Log\w*|Verbose|Debug|Information|Warning|Error|Fatal|(?<w>Write))\s*(?:<[^<>()]*>)?\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LogCall();

    [GeneratedRegex(@"\bLogEventLevel\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex WriteLevel();

    // [LoggerMessage(...)] and [LoggerMessageAttribute(...)], also new LoggerMessageAttribute(...) and a namespace in front.
    [GeneratedRegex(@"\bLoggerMessage(?:Attribute)?\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LoggerMessageAttribute();

    [GeneratedRegex(@"\bLoggerMessage\s*\.\s*Define(?:Scope)?\s*(?:<[^()]*>)?\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LoggerMessageDefine();

    [GeneratedRegex(@"\bnew\s+AuditLine\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex AuditLineCall();

    [GeneratedRegex(@"\b(?:BeginScope|ForContext|PushProperty|WithProperty)\s*(?:<[^()]*>)?\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ScopeCall();

    [GeneratedRegex(@"\bconst\s+string\s+\w+\s*=", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ConstantString();

    // The argument is literals and plus signs only, with an optional name: "a" + "b", message: "a"
    [GeneratedRegex(@"^\s*(?:\w+\s*:\s*)?[\s+]*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex JoinedLiterals();
}
