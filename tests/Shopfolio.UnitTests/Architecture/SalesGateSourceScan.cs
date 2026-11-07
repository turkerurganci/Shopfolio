using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the sales gate (T331; 02 3.1.5, 3.1.6; 05 4.1.5; 11 T331 criterion 8): the gate is decided in one place and every surface reads that one value.
/// (1) The "close sales temporarily" switch - the property <c>IsSalesTemporarilyClosed</c> or its column <c>is_sales_temporarily_closed</c> - is named only by the entity,
/// the section writer, the gate's reader and the switch writer, and by the migrations (the storefront, the cart, the payment step and the panel read the gate, never the
/// switch, so none of them can decide on its own); a hidden field that carries the shown value, <c>OriginalIsSalesTemporarilyClosed</c>, is another word. (2) A gate value is
/// built only by the gate's rule (<c>new SalesGate(</c> in <c>SalesGateRules.cs</c>). The scan reads whole texts (a call split over lines is found), blanks comments first
/// and keeps their lines; the <c>.cs</c> and <c>.cshtml</c> files of <c>src</c> are scanned, the test projects are not.
/// </summary>
internal static partial class SalesGateSourceScan
{
    public const string NamesTheSwitchOutsideTheGate = "names the sales switch (IsSalesTemporarilyClosed / is_sales_temporarily_closed) outside the entity, the section writer, the gate's reader and the switch writer (11 T331 criterion 8: every surface reads the one gate value, none decides from the switch)";
    public const string BuildsAGateOutsideTheRule = "builds a SalesGate outside SalesGateRules (11 T331 criterion 8: the gate is decided in one place)";

    /// <summary>The closed list of files that may name the switch (migrations aside).</summary>
    public static readonly string[] SwitchFiles =
    [
        "src/Shopfolio.Web/Modules/Settings/CompanySettings.cs",
        "src/Shopfolio.Web/Modules/Settings/CompanySettingsWriter.cs",
        "src/Shopfolio.Web/Modules/Settings/SalesGates.cs",
        "src/Shopfolio.Web/Modules/Settings/SalesSwitches.cs",
    ];

    /// <summary>The one file that builds a gate.</summary>
    public const string RuleFile = "src/Shopfolio.Web/Modules/Settings/SalesGateRules.cs";

    public static bool Applies(string relativePath)
    {
        string path = relativePath.Replace('\\', '/');
        return (path.EndsWith(".cs", StringComparison.Ordinal) || path.EndsWith(".cshtml", StringComparison.Ordinal))
            && path.StartsWith("src/", StringComparison.Ordinal)
            && !path.Contains("/obj/", StringComparison.Ordinal)
            && !path.Contains("/bin/", StringComparison.Ordinal);
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        string clean = BlankComments(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];

        if (!SwitchFiles.Contains(path, StringComparer.Ordinal) && !path.Contains("/Infrastructure/Migrations/", StringComparison.Ordinal))
        {
            Add(findings, NamesTheSwitchOutsideTheGate, SwitchName(), clean);
        }

        if (!string.Equals(path, RuleFile, StringComparison.Ordinal))
        {
            Add(findings, BuildsAGateOutsideTheRule, GateConstruction(), clean);
        }

        return [.. findings.Distinct().OrderBy(f => f.Line).ThenBy(f => f.Rule, StringComparer.Ordinal)];
    }

    private static void Add(List<(string Rule, int Line)> findings, string rule, Regex pattern, string text)
    {
        foreach (Match match in pattern.Matches(text))
        {
            findings.Add((rule, text.AsSpan(0, match.Index).Count('\n') + 1));
        }
    }

    // Comments are blanked and keep their lines: block comments (and the Razor comment) by their newlines, full-line and trailing line comments by their text.
    internal static string BlankComments(string text)
    {
        string withoutBlocks = BlockComment().Replace(text, m => new string('\n', m.Value.Count(c => c == '\n')));
        return string.Join('\n', withoutBlocks.Split('\n').Select(StripLineComment));
    }

    // A line comment starts at "//" that is not inside a string or character literal of the line (a URL such as "https://" is kept).
    private static string StripLineComment(string line)
    {
        bool inString = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inString)
            {
                if (c == '\\')
                {
                    i++;
                }
                else if (c == '"')
                {
                    inString = false;
                }
            }
            else if (c == '"')
            {
                inString = true;
            }
            else if (c == '\'' && i + 2 < line.Length)
            {
                i = line[i + 1] == '\\' ? line.IndexOf('\'', i + 2) : i + 2;
                if (i < 0)
                {
                    return line;
                }
            }
            else if (c == '/' && i + 1 < line.Length && line[i + 1] == '/')
            {
                return line[..i];
            }
        }

        return line;
    }

    [GeneratedRegex(@"/\*.*?\*/|@\*.*?\*@", RegexOptions.CultureInvariant | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BlockComment();

    // The property in any form (a member access, nameof, a string, a lambda, a camel-cased JSON name) and the column as a word. "OriginalIsSalesTemporarilyClosed" has no boundary
    // before "Is", so the hidden field that carries the shown value is not a finding.
    [GeneratedRegex(@"(?<![A-Za-z0-9_])[Ii]sSalesTemporarilyClosed(?![A-Za-z0-9_])|(?<![A-Za-z0-9_])is_sales_temporarily_closed(?![A-Za-z0-9_])", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SwitchName();

    // A constructor call of the gate: the type spelled out or qualified, a space or a line break before the parenthesis, and the target-typed form after a declaration of the type
    // ("SalesGate gate = new(...)"). Another type that starts with the name (SalesGateFacts, SalesGateRules) has a letter after "SalesGate" and is not this.
    [GeneratedRegex(@"\bnew\s+(?:[A-Za-z_][A-Za-z0-9_]*\s*\.\s*)*SalesGate\s*\(|\bSalesGate\??\s+[A-Za-z_][A-Za-z0-9_]*\s*=\s*new\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex GateConstruction();
}
