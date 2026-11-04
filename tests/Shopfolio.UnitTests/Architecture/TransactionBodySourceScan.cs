using System.Text;
using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the transaction bodies (T233; 09 10.7 item 5): the idle-in-transaction time assumes that an open transaction waits for no
/// outside service, so the work handed to <c>ITransactionRunner.RunAsync</c> never uses an <c>HttpClient</c> or an outside service interface
/// (<c>IPaymentGateway</c>, <c>IEmailGateway</c>, <c>IGoogleIdentityGateway</c> and any other <c>I...Gateway</c>). A file is looked at when it
/// names <c>ITransactionRunner</c>; the arguments of each <c>RunAsync(...)</c> call are the body, and a body is a finding when it names one of
/// those types or a field, parameter or variable the file declares with one of them. A body that is only a method group is not followed.
/// Full-line comments are blanked first and keep their line.
/// </summary>
internal static partial class TransactionBodySourceScan
{
    public const string OutsideCallInTransaction = "uses an HttpClient or an outside service interface inside the work of ITransactionRunner.RunAsync (the outside call belongs outside the transaction)";

    /// <summary>The scan applies to every application source file; the runner's own file holds no outside call to find.</summary>
    public static bool Applies(string relativePath)
    {
        return relativePath.Replace('\\', '/').EndsWith(".cs", StringComparison.Ordinal);
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string text)
    {
        string clean = BlankCommentLines(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];
        if (!clean.Contains("ITransactionRunner", StringComparison.Ordinal))
        {
            return findings;
        }

        List<string> outsideNames = DeclaredOutsideNames(clean);
        foreach (Match call in RunAsyncCall().Matches(clean))
        {
            int open = call.Index + call.Length - 1;
            int close = ClosingParenthesis(clean, open);
            string body = clean[open..close];
            Match hit = OutsideType().Match(body);
            int hitIndex = hit.Success ? open + hit.Index : -1;
            foreach (string name in outsideNames)
            {
                Match named = Regex.Match(body, $@"(?<![\w.]){Regex.Escape(name)}\b", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));
                if (named.Success && (hitIndex < 0 || open + named.Index < hitIndex))
                {
                    hitIndex = open + named.Index;
                }
            }

            if (hitIndex >= 0)
            {
                findings.Add((OutsideCallInTransaction, clean.AsSpan(0, hitIndex).Count('\n') + 1));
            }
        }

        return findings;
    }

    private static List<string> DeclaredOutsideNames(string text)
    {
        return OutsideDeclaration().Matches(text).Select(m => m.Groups[1].Value).Distinct(StringComparer.Ordinal).ToList();
    }

    private static int ClosingParenthesis(string text, int open)
    {
        int depth = 0;
        for (int i = open; i < text.Length; i++)
        {
            if (text[i] == '(')
            {
                depth++;
            }
            else if (text[i] == ')' && --depth == 0)
            {
                return i;
            }
        }

        return text.Length - 1;
    }

    private static string BlankCommentLines(string text)
    {
        StringBuilder builder = new();
        foreach (string line in text.Split('\n'))
        {
            builder.Append(line.TrimStart().StartsWith("//", StringComparison.Ordinal) ? string.Empty : line).Append('\n');
        }

        return builder.ToString(0, builder.Length - 1);
    }

    [GeneratedRegex(@"\bRunAsync\s*(?:<[^()]*?>)?\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RunAsyncCall();

    [GeneratedRegex(@"\b(?:HttpClient|IHttpClientFactory|I\w*Gateway)\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex OutsideType();

    [GeneratedRegex(@"\b(?:HttpClient|IHttpClientFactory|I\w*Gateway)\s+(\w+)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex OutsideDeclaration();
}
