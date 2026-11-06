using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the data protection rules (T232; 09 15.5 items 1 and 2): the ring is set up in one file, nothing sets a key encryptor or a key
/// lifetime, every <c>CreateProtector</c> call takes exactly one constant of <c>DataProtectionPurposes</c> (no sub-purpose argument), <c>GetDataProtector</c> is not called, and a purpose string is written nowhere but in that
/// class. Two files are allowed, each for one rule only: <see cref="SetupFile"/> may call <c>AddDataProtection</c> and <see cref="PurposesFile"/>
/// may hold purpose literals. The scan reads the whole text, so a call split over several lines is found; full-line comments are blanked first and
/// keep their line. The test projects are not scanned.
/// </summary>
internal static partial class DataProtectionSourceScan
{
    public const string KeyEncryptor = "sets a key encryptor (ProtectKeysWith...; the ring stays in clear in the database, 09 15.5 item 1)";
    public const string KeyLifetime = "sets the key lifetime (SetDefaultKeyLifetime; the framework default stands, 09 15.5 item 1)";
    public const string SecondSetup = "sets up data protection outside DataProtectionSetup (AddDataProtection; the setup is one place, 09 15.5 item 1)";
    public const string SecondRing = "builds a second ring (DataProtectionProvider.Create or EphemeralDataProtectionProvider; the ring is the one DataProtectionSetup sets up, 09 15.5 item 1; D-90 K1)";
    public const string ProtectorPurpose = "calls CreateProtector with something other than a DataProtectionPurposes constant (09 15.5 item 2)";
    public const string ExtensionProtector = "calls GetDataProtector (the extension takes the purpose strings itself; the only call is CreateProtector with one DataProtectionPurposes constant, 09 15.5 item 2)";
    public const string PurposeLiteral = "writes a purpose string outside DataProtectionPurposes (09 15.5 item 2)";

    /// <summary>The one file that may set up the ring.</summary>
    public const string SetupFile = "src/Shopfolio.Web/Infrastructure/Security/DataProtectionSetup.cs";

    /// <summary>The one file that may hold the purpose strings.</summary>
    public const string PurposesFile = "src/Shopfolio.Web/Infrastructure/Security/DataProtectionPurposes.cs";

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        string clean = BlankCommentLines(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];
        Add(findings, KeyEncryptor, ProtectKeysWith(), clean);
        Add(findings, KeyLifetime, SetKeyLifetime(), clean);
        if (!string.Equals(path, SetupFile, StringComparison.Ordinal))
        {
            Add(findings, SecondSetup, AddDataProtectionCall(), clean);
        }

        Add(findings, SecondRing, SecondRingCall(), clean);
        Add(findings, ProtectorPurpose, ProtectorWithOtherArgument(), clean);
        Add(findings, ExtensionProtector, GetDataProtectorCall(), clean);
        if (!string.Equals(path, PurposesFile, StringComparison.Ordinal))
        {
            Add(findings, PurposeLiteral, PurposeString(), clean);
        }

        return [.. findings.OrderBy(f => f.Line)];
    }

    private static void Add(List<(string Rule, int Line)> findings, string rule, Regex pattern, string text)
    {
        foreach (Match match in pattern.Matches(text))
        {
            int line = text.AsSpan(0, match.Index).Count('\n') + 1;
            if (!findings.Contains((rule, line)))
            {
                findings.Add((rule, line));
            }
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

    [GeneratedRegex(@"\bProtectKeysWith\w*", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ProtectKeysWith();

    [GeneratedRegex(@"\bSetDefaultKeyLifetime\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SetKeyLifetime();

    [GeneratedRegex(@"\bAddDataProtection\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex AddDataProtectionCall();

    // The shared framework's other ways to build a ring: the static factory (also with a namespace or global:: prefix, an added argument or a split line) and the in-memory provider.
    [GeneratedRegex(@"\bDataProtectionProvider\s*\.\s*Create\b|\bEphemeralDataProtectionProvider\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SecondRingCall();

    // A CreateProtector call whose argument list is not exactly one DataProtectionPurposes.<Name> (a sub-purpose argument after it is a finding).
    [GeneratedRegex(@"\bCreateProtector\s*\((?!\s*DataProtectionPurposes\s*\.\s*\w+\s*\))", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ProtectorWithOtherArgument();

    // The extension method takes the purpose string itself, so the call is a finding whatever its argument is.
    [GeneratedRegex(@"\bGetDataProtector\s*[(<]", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex GetDataProtectorCall();

    // The shape of a purpose string: a string literal "Shopfolio.<Use>.v<number>".
    [GeneratedRegex(@"""Shopfolio\.\w+\.v\d+""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PurposeString();
}
