using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the outage independence (T204 KK7; 08 3.3: no time limit hangs on the e-mail service or on Google, an outage does not stop
/// the clock; 09 19.2 item 3). A deadline, a job or a page that waited for the e-mail gateway or for the Google gateway would let an outage
/// hold time back, so the gateways' names stay in their own folders:
/// (1) an e-mail gateway type (<c>IEmailGateway</c> or any type whose name ends with <c>EmailGateway</c>) is named only in
/// <c>Modules/Notifications/</c> - the dispatch record's attempts - and in the link layer <c>Infrastructure/Integrations/</c>;
/// (2) a Google identity gateway type is named only in <c>Modules/Membership/</c> and in the link layer.
/// Full-line comments are blanked (they keep their line); a string or code anywhere else is a finding. The scan reads text and writes nothing.
/// </summary>
internal static partial class OutageIndependenceSourceScan
{
    public const string EmailGatewayOutsideItsFolders = "names the e-mail gateway outside Modules/Notifications and the link layer (08 3.3: no time limit hangs on the e-mail service)";
    public const string GoogleGatewayOutsideItsFolders = "names the Google gateway outside Modules/Membership and the link layer (08 3.3: no time limit hangs on Google)";

    /// <summary>The link layer's folder: the interfaces and the implementations of every gateway.</summary>
    public const string LayerFolder = "src/Shopfolio.Web/Infrastructure/Integrations/";

    /// <summary>The only module that sends e-mail: the dispatch record and its attempts.</summary>
    public const string NotificationsFolder = "src/Shopfolio.Web/Modules/Notifications/";

    /// <summary>The only module that calls Google: sign-in, sign-up and re-authentication.</summary>
    public const string MembershipFolder = "src/Shopfolio.Web/Modules/Membership/";

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        string code = BlankCommentLines(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];

        if (!path.StartsWith(LayerFolder, StringComparison.Ordinal) && !path.StartsWith(NotificationsFolder, StringComparison.Ordinal))
        {
            Add(findings, EmailGatewayOutsideItsFolders, EmailGateway(), code);
        }

        if (!path.StartsWith(LayerFolder, StringComparison.Ordinal) && !path.StartsWith(MembershipFolder, StringComparison.Ordinal))
        {
            Add(findings, GoogleGatewayOutsideItsFolders, GoogleGateway(), code);
        }

        return [.. findings.OrderBy(f => f.Line).ThenBy(f => f.Rule, StringComparer.Ordinal)];
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

    // A whole type name ending with the gateway's name: IEmailGateway, SmtpCaptureEmailGateway; not EmailGatewayOptions, not SendEmail.
    [GeneratedRegex(@"\b\w*EmailGateway\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex EmailGateway();

    [GeneratedRegex(@"\b\w*GoogleIdentityGateway\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex GoogleGateway();
}
