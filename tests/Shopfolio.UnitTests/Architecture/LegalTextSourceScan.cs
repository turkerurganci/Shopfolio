using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the legal texts (T366; 02 3.33.4-3.33.6; 10 KD-56; 09 11.4, 19.2 item 3). Three rules over the <c>.cs</c> and <c>.cshtml</c> files of <c>src</c>:
/// (1) there is no consent record, "seen" record, cookie banner or explicit consent for the legal texts - no name that joins a legal text (the privacy notice, the cookie policy)
/// with consent, acceptance, "seen", acknowledgement or banner, and none that spells the explicit consent (the proof is the version and the places of the links);
/// (2) the two generated texts are never attached: a file that names the form or the contract does not name an attachment API, a PDF or a file part;
/// (3) a legal text row is created only by the generator and by the publication (<c>LegalTextVersion.CreatePublished</c>/<c>CreateDraft</c> nowhere else), so the number is never typed by
/// hand. The scan reads whole texts (a call split over lines is found) and blanks comments first, keeping their lines.
/// </summary>
internal static partial class LegalTextSourceScan
{
    public const string ConsentOrSeenRecord = "names a consent, acceptance, \"seen\" or banner record for a legal text, or an explicit consent (02 3.33.5; 10 KD-56: the notice has no box, no seen record, no cookie banner and no explicit consent)";
    public const string AttachesALegalText = "names an attachment, a PDF or a file part in a file that names the Pre-Information Form or the Distance Sales Contract (02 3.33.6: the two texts go in the body, never as a file)";
    public const string CreatesARowOutsideTheOwners = "creates a legal text version row outside the generator and the publication (02 3.33.3: the number is never typed by hand)";

    /// <summary>The files that may create a legal text row.</summary>
    public static readonly string[] RowOwners =
    [
        "src/Shopfolio.Web/Modules/Settings/LegalTextGenerator.cs",
        "src/Shopfolio.Web/Modules/Settings/LegalTexts.cs",
        "src/Shopfolio.Web/Modules/Settings/LegalTextVersion.cs",
    ];

    public static bool Applies(string relativePath)
    {
        string path = relativePath.Replace('\\', '/');
        return (path.EndsWith(".cs", StringComparison.Ordinal) || path.EndsWith(".cshtml", StringComparison.Ordinal))
            && path.StartsWith("src/", StringComparison.Ordinal)
            && !path.Contains("/obj/", StringComparison.Ordinal)
            && !path.Contains("/bin/", StringComparison.Ordinal)
            && !path.Contains("/Infrastructure/Migrations/", StringComparison.Ordinal);
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        string clean = SalesGateSourceScan.BlankComments(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];
        Add(findings, ConsentOrSeenRecord, ConsentName(), clean);
        Add(findings, ConsentOrSeenRecord, ExplicitConsentName(), clean);
        if (NamesTheTwoGeneratedTexts().IsMatch(clean))
        {
            Add(findings, AttachesALegalText, AttachmentWord(), clean);
        }

        if (!RowOwners.Contains(path, StringComparer.Ordinal))
        {
            Add(findings, CreatesARowOutsideTheOwners, RowCreation(), clean);
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

    // A legal text and a consent word in one name, either order ("PrivacyNoticeConsent", "AcceptedPrivacyNotice", "CookiePolicySeenAt", "CookieBanner"), in code or a column name.
    [GeneratedRegex(
        @"(?:PrivacyNotice|PrivacyPolicy|CookiePolicy|LegalText|AydinlatmaMetni|Aydinlatma)[A-Za-z0-9_]*(?:Consent|Accept|Seen|Acknowledg|Banner)"
        + @"|(?:Consent|Accept|Seen|Acknowledg)[A-Za-z0-9_]*(?:PrivacyNotice|PrivacyPolicy|CookiePolicy|LegalText|AydinlatmaMetni|Aydinlatma)"
        + @"|(?:privacy_notice|privacy_policy|cookie_policy|legal_text)[a-z0-9_]*(?:consent|accept|seen|acknowledg|banner)"
        + @"|(?:consent|accept|seen|acknowledg)[a-z0-9_]*(?:privacy_notice|privacy_policy|cookie_policy|legal_text)"
        + @"|[Cc]ookie[-_ ]?[Bb]anner|[Cc]ookie[-_ ]?[Cc]onsent",
        RegexOptions.CultureInvariant,
        matchTimeoutMilliseconds: 5000)]
    private static partial Regex ConsentName();

    // The explicit consent (acik riza) in any spelling.
    [GeneratedRegex(@"(?<![A-Za-z])(?:Explicit|Open)[-_ ]?Consent|(?<![A-Za-z])[Aa][cç][ıi]k[-_ ]?[Rr][ıi]za", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ExplicitConsentName();

    [GeneratedRegex(@"PreInformationForm|DistanceSalesContract", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex NamesTheTwoGeneratedTexts();

    [GeneratedRegex(@"Attachment|\.pdf\b|application/pdf|MultipartFormDataContent|ContentDisposition\s*\(", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex AttachmentWord();

    // A call that makes a row: the type spelled out or qualified, the factory methods, and a constructor of the entity.
    [GeneratedRegex(@"\bLegalTextVersion\s*\.\s*Create(?:Published|Draft)\s*\(|\bnew\s+(?:[A-Za-z_][A-Za-z0-9_]*\s*\.\s*)*LegalTextVersion\s*[({]", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RowCreation();
}
