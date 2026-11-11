using System.Text;
using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the null-forgiving operator (T550; 09 3.1 item 5; D-778 decision (c); K-1613): every postfix <c>!</c> in the
/// application source either goes (a pattern match, <c>is { } x</c>, or a check that throws) or carries its reason in a comment on
/// the same line that starts with <see cref="ReasonMarker"/>. The scan separates code from comments and literals first - string,
/// verbatim, raw and character literals and comments are blanked, the holes of interpolated strings stay code - so an exclamation
/// mark in a text or a comment is not a finding, and an operator inside an interpolation hole is. A <c>!</c> is postfix when it
/// directly follows an identifier, a closing parenthesis or bracket, a literal's closing quote, or a generic's closing angle
/// bracket, and is not part of <c>!=</c>; after white space it is postfix when only a member access, a closing parenthesis or
/// bracket, a comma or a semicolon follows it. Razor files have no lexer here: their lines are matched with the narrower form of a <c>!</c> followed by member
/// access, an index, a closing parenthesis, a comma or a semicolon, so a sentence ending with an exclamation mark is not a finding.
/// </summary>
internal static partial class NullForgivingSourceScan
{
    public const string Unexplained = "null-forgiving ! without a reason comment on the same line (09 3.1 item 5)";

    /// <summary>The reason comment's start; the reason follows it on the same line.</summary>
    public const string ReasonMarker = "// non-null:";

    public static IReadOnlyList<(string Rule, int Line)> Scan(string text)
    {
        List<(string Rule, int Line)> findings = [];
        (List<string> code, List<string> comments) = Split(text);
        for (int i = 0; i < code.Count; i++)
        {
            if (code[i].TrimStart().StartsWith('#'))
            {
                continue;
            }

            if (PostfixBang().IsMatch(code[i]) && !HasReason(comments[i]))
            {
                findings.Add((Unexplained, i + 1));
            }
        }

        return findings;
    }

    public static IReadOnlyList<(string Rule, int Line)> ScanRazor(string text)
    {
        List<(string Rule, int Line)> findings = [];
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            int comment = lines[i].IndexOf(ReasonMarker, StringComparison.Ordinal);
            string code = comment < 0 ? lines[i] : lines[i][..comment];
            if (RazorPostfixBang().IsMatch(code) && !HasReason(comment < 0 ? string.Empty : lines[i][comment..]))
            {
                findings.Add((Unexplained, i + 1));
            }
        }

        return findings;
    }

    private static bool HasReason(string comment)
    {
        string trimmed = comment.Trim();
        return trimmed.StartsWith(ReasonMarker, StringComparison.Ordinal)
            && trimmed[ReasonMarker.Length..].Trim().Length >= MinimumReasonLength;
    }

    private const int MinimumReasonLength = 10;

    /// <summary>What a literal's closing quote becomes in the code line: its contents are blanked, but a ! right after it is still an operand's.</summary>
    private const char LiteralEnd = '"';

    /// <summary>
    /// Splits the text into lines of code (comments and literal contents blanked) and lines of the <c>//</c> comment text.
    /// </summary>
    internal static (List<string> Code, List<string> Comments) Split(string text)
    {
        string source = text.ReplaceLineEndings("\n");
        List<string> code = [];
        List<string> comments = [];
        StringBuilder codeLine = new();
        StringBuilder commentLine = new();
        Stack<Context> stack = new();
        stack.Push(Context.CodeAt(0));
        int i = 0;
        while (i < source.Length)
        {
            char c = source[i];
            if (c == '\n')
            {
                code.Add(codeLine.ToString());
                comments.Add(commentLine.ToString());
                codeLine.Clear();
                commentLine.Clear();
                if (stack.Peek().Kind == Kind.LineComment)
                {
                    stack.Pop();
                }

                i++;
                continue;
            }

            Context top = stack.Peek();
            switch (top.Kind)
            {
                case Kind.Code:
                    i = StepCode(source, i, codeLine, commentLine, stack);
                    break;
                case Kind.LineComment:
                    commentLine.Append(c);
                    codeLine.Append(' ');
                    i++;
                    break;
                case Kind.BlockComment:
                    codeLine.Append(' ');
                    if (c == '*' && Next(source, i) == '/')
                    {
                        codeLine.Append(' ');
                        stack.Pop();
                        i += 2;
                    }
                    else
                    {
                        i++;
                    }

                    break;
                case Kind.Char:
                    codeLine.Append(c == '\'' ? LiteralEnd : ' ');
                    if (c == '\\')
                    {
                        if (Next(source, i) is char n && n != '\n')
                        {
                            codeLine.Append(' ');
                            i++;
                        }
                    }
                    else if (c == '\'')
                    {
                        stack.Pop();
                    }

                    i++;
                    break;
                default:
                    i = StepString(source, i, top, codeLine, stack);
                    break;
            }
        }

        code.Add(codeLine.ToString());
        comments.Add(commentLine.ToString());
        return (code, comments);
    }

    private static int StepCode(string source, int i, StringBuilder codeLine, StringBuilder commentLine, Stack<Context> stack)
    {
        char c = source[i];
        Context top = stack.Peek();
        if (c == '/' && Next(source, i) == '/')
        {
            stack.Push(new Context(Kind.LineComment, 0, 0, 0));
            commentLine.Append("//");
            codeLine.Append("  ");
            return i + 2;
        }

        if (c == '/' && Next(source, i) == '*')
        {
            stack.Push(new Context(Kind.BlockComment, 0, 0, 0));
            codeLine.Append("  ");
            return i + 2;
        }

        if (c == '\'')
        {
            stack.Push(new Context(Kind.Char, 0, 0, 0));
            codeLine.Append(' ');
            return i + 1;
        }

        if (c is '"' or '$' or '@' && TryOpenString(source, i, out Context opened, out int length))
        {
            stack.Push(opened);
            codeLine.Append(' ', length);
            return i + length;
        }

        if (top.Depth >= 0 && top.IsHole)
        {
            if (c == '{')
            {
                stack.Pop();
                stack.Push(top with { Depth = top.Depth + 1 });
            }
            else if (c == '}')
            {
                if (top.Depth == 0)
                {
                    stack.Pop();
                    codeLine.Append(' ');
                    return i + 1;
                }

                stack.Pop();
                stack.Push(top with { Depth = top.Depth - 1 });
            }
        }

        codeLine.Append(c);
        return i + 1;
    }

    /// <summary>Opens a literal at <paramref name="i"/>: regular, verbatim, interpolated or raw, in any prefix order.</summary>
    private static bool TryOpenString(string source, int i, out Context opened, out int length)
    {
        opened = default;
        length = 0;
        int j = i;
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

        if (j >= source.Length || source[j] != '"')
        {
            return false;
        }

        if (j > i && char.IsLetterOrDigit(Previous(source, i)))
        {
            return false;
        }

        int quotes = 0;
        while (j + quotes < source.Length && source[j + quotes] == '"')
        {
            quotes++;
        }

        if (quotes >= 3)
        {
            opened = new Context(Kind.Raw, dollars, quotes, 0);
            length = j - i + quotes;
            return true;
        }

        opened = new Context(verbatim ? Kind.Verbatim : Kind.Regular, dollars, 1, 0);
        length = j - i + 1;
        return true;
    }

    private static int StepString(string source, int i, Context top, StringBuilder codeLine, Stack<Context> stack)
    {
        char c = source[i];
        switch (top.Kind)
        {
            case Kind.Regular when c == '\\':
                codeLine.Append(' ');
                if (Next(source, i) is char n && n != '\n')
                {
                    codeLine.Append(' ');
                    return i + 2;
                }

                return i + 1;
            case Kind.Regular when c == '"':
                stack.Pop();
                codeLine.Append(LiteralEnd);
                return i + 1;
            case Kind.Verbatim when c == '"':
                if (Next(source, i) == '"')
                {
                    codeLine.Append("  ");
                    return i + 2;
                }

                stack.Pop();
                codeLine.Append(LiteralEnd);
                return i + 1;
            case Kind.Raw when c == '"':
                int run = Run(source, i, '"');
                if (run >= top.Quotes)
                {
                    stack.Pop();
                    codeLine.Append(' ', run - 1).Append(LiteralEnd);
                }
                else
                {
                    codeLine.Append(' ', run);
                }

                return i + run;
            default:
                break;
        }

        if (top.Dollars > 0 && c == '{')
        {
            int run = Run(source, i, '{');
            int needed = top.Kind == Kind.Raw ? top.Dollars : 1;
            bool opens = top.Kind == Kind.Raw ? run >= needed : run % 2 == 1;
            codeLine.Append(' ', run);
            if (opens)
            {
                stack.Push(new Context(Kind.Code, 0, 0, 0) { IsHole = true });
            }

            return i + run;
        }

        codeLine.Append(' ');
        return i + 1;
    }

    private static int Run(string source, int i, char c)
    {
        int run = 0;
        while (i + run < source.Length && source[i + run] == c)
        {
            run++;
        }

        return run;
    }

    private static char? Next(string source, int i)
    {
        return i + 1 < source.Length ? source[i + 1] : null;
    }

    private static char Previous(string source, int i)
    {
        return i > 0 ? source[i - 1] : ' ';
    }

    private enum Kind
    {
        Code,
        LineComment,
        BlockComment,
        Char,
        Regular,
        Verbatim,
        Raw,
    }

    private readonly record struct Context(Kind Kind, int Dollars, int Quotes, int Depth)
    {
        public bool IsHole { get; init; }

        public static Context CodeAt(int depth)
        {
            return new Context(Kind.Code, 0, 0, depth);
        }
    }

    // The direct form, and the form with white space before the operator: a prefix ! needs an operand after it, so a ! that only a
    // member access, a closing parenthesis or bracket, a comma or a semicolon follows is the postfix one ("return !flag" is not).
    [GeneratedRegex(@"(?:[\w)\]""]|(?<![=\-])>)!(?!=)|[\w)\]""]\s+!(?=\s*[.),;\]])")]
    private static partial Regex PostfixBang();

    [GeneratedRegex(@"[\w)\]]!(?=[.\[),;])")]
    private static partial Regex RazorPostfixBang();
}
