using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the notification channel rules (T328 KK2, KK4, KK5; 03 7; 02 9.1, 9.5, 12.1.11; 10 KD-10, KD-16, KD-17, KD-52; 09 19.2 item 3). Five rules over the application source
/// (<c>.cs</c>, <c>.razor</c>, <c>.cshtml</c> and the JSON files of <c>src/</c>, and the words of every file's path):
/// (1) <see cref="UnknownChannel"/> - no SMS, push notification, in-app notification centre or messaging-app channel: e-mail is the only channel;
/// (2) <see cref="MarketingMessage"/> - no marketing message, newsletter, İYS link, opt-in or opt-out, unsubscribe, abandoned-cart reminder or back-in-stock notice;
/// (3) <see cref="TransactionSwitch"/> - nothing that turns a transaction notice off or lets the user choose which ones to get (a notification preference or setting);
/// (4) <see cref="GatewayOutsideTheSender"/> - the e-mail gateway is named only by its own files, by the dispatch attempts and by the returned-address query's handler (which only reads the service's list; a test keeps it from calling the send), so no e-mail leaves except through a dispatch row;
/// (5) <see cref="DispatchRowOutsideTheQueues"/> - a dispatch row is made and added only by the three queues (and its own type), each of which asks the notification map first.
/// Rules 1-3 read the words of every identifier, JSON key and string (a name is split at its capitals, underscores and hyphens, so <c>ISmsGateway</c>, <c>sms_gateway</c> and
/// <c>sms-gateway</c> are all found); comments are blanked first and keep their lines, so a comment that says "no SMS" is not a finding. Rules 4 and 5 name their allowed files
/// exactly - there is no folder and no pattern in the lists.
/// </summary>
internal static partial class NotificationChannelSourceScan
{
    public const string UnknownChannel = "names a channel the product does not have (02 9.1.1: e-mail is the only channel; 10 KD-16)";
    public const string MarketingMessage = "names a marketing message (02 9.5, 12.1.11: no marketing e-mail, no İYS link, no consent; 10 KD-10, KD-17)";
    public const string TransactionSwitch = "names a switch or a preference for transaction notices (02 9.1.3: they cannot be turned off; 10 KD-52)";
    public const string GatewayOutsideTheSender = "names the e-mail gateway outside the files that may (03 7.1: an e-mail leaves only through a dispatch row)";
    public const string DispatchRowOutsideTheQueues = "makes or adds a dispatch row outside the queues (08 2.2.6-2.2.8: every row passes the notification map)";

    /// <summary>The files that may name the e-mail gateway: its interface, its registration, its two implementations (the service's and the development capture) and the sender of the dispatch rows.</summary>
    public static readonly string[] GatewayFiles =
    [
        "src/Shopfolio.Web/Infrastructure/Integrations/IEmailGateway.cs",
        "src/Shopfolio.Web/Infrastructure/Integrations/IntegrationRegistration.cs",
        "src/Shopfolio.Web/Infrastructure/Integrations/SesEmailGateway.cs",
        "src/Shopfolio.Web/Infrastructure/Integrations/SesRegistration.cs",
        "src/Shopfolio.Web/Infrastructure/Integrations/SmtpCaptureEmailGateway.cs",
        "src/Shopfolio.Web/Modules/Notifications/DispatchAttempts.cs",
        "src/Shopfolio.Web/Modules/Notifications/BounceQueryJobHandler.cs",
    ];

    /// <summary>The files that may make and add a dispatch row: the three queues and the row's own type.</summary>
    public static readonly string[] QueueFiles =
    [
        "src/Shopfolio.Web/Modules/Notifications/AccountEmailQueue.cs",
        "src/Shopfolio.Web/Modules/Notifications/CompanyNoticeQueue.cs",
        "src/Shopfolio.Web/Modules/Notifications/CustomerNoticeQueue.cs",
        "src/Shopfolio.Web/Modules/Notifications/DispatchRecord.cs",
    ];

    // The phrases are word sequences in lower case; a "|" lists the words that may stand in a place of the phrase.
    private static readonly string[][] ChannelPhrases =
    [
        ["sms"],
        ["whatsapp"],
        ["push", "notification|notifications|message|messages|token|tokens|subscription|subscriptions"],
        ["web", "push"],
        ["notification|notifications", "center|centre|inbox|bell|feed"],
        ["in", "app", "notification|notifications|message|messages"],
        ["bildirim", "merkezi|merkez"],
    ];

    private static readonly string[][] MarketingPhrases =
    [
        ["marketing"],
        ["newsletter"],
        ["iys"],
        ["bulten"],
        ["opt", "in|out"],
        ["optin"],
        ["optout"],
        ["unsubscribe"],
        ["mailing", "list|lists"],
        ["promotional"],
        ["abandoned", "cart|carts"],
        ["cart", "reminder|reminders"],
        ["back", "in", "stock"],
        ["stock", "alert|alerts|notification|notifications|reminder|reminders"],
        ["notify", "me"],
    ];

    private static readonly string[][] SwitchPhrases =
    [
        ["notice|notices|notification|notifications|email|emails|mail", "enabled|disabled|off|muted"],
        ["disable|suppress|mute|silence", "email|emails|notice|notices|notification|notifications"],
        ["notice|notices|notification|notifications", "preference|preferences|setting|settings"],
        ["email|mail", "preference|preferences"],
        ["bildirim", "tercih|tercihi|tercihleri"],
    ];

    public static bool InScope(string relativePath)
    {
        string path = relativePath.Replace('\\', '/');
        if (!path.StartsWith("src/", StringComparison.Ordinal) || path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal)
            || path.EndsWith("packages.lock.json", StringComparison.Ordinal) || path.Contains("/wwwroot/", StringComparison.Ordinal))
        {
            return false;
        }

        return path.EndsWith(".cs", StringComparison.Ordinal) || path.EndsWith(".razor", StringComparison.Ordinal)
            || path.EndsWith(".cshtml", StringComparison.Ordinal) || path.EndsWith(".json", StringComparison.Ordinal);
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        if (!InScope(path))
        {
            return [];
        }

        List<(string Rule, int Line)> findings = [];
        foreach (string word in PathWordLists(path))
        {
            AddWordFindings(findings, 0, word.Split(' '));
        }

        string clean = BlankComments(text.ReplaceLineEndings("\n"));
        string[] lines = clean.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            foreach (Match token in Token().Matches(lines[i]))
            {
                AddWordFindings(findings, i + 1, Words(token.Value));
            }
        }

        bool code = path.EndsWith(".cs", StringComparison.Ordinal) || path.EndsWith(".razor", StringComparison.Ordinal) || path.EndsWith(".cshtml", StringComparison.Ordinal);
        if (code)
        {
            if (!GatewayFiles.Contains(path, StringComparer.Ordinal))
            {
                AddMatches(findings, GatewayOutsideTheSender, Gateway(), clean);
            }

            if (!QueueFiles.Contains(path, StringComparer.Ordinal))
            {
                AddMatches(findings, DispatchRowOutsideTheQueues, DispatchRow(), clean);
            }
        }

        return [.. findings.Distinct().OrderBy(f => f.Line).ThenBy(f => f.Rule, StringComparer.Ordinal)];
    }

    private static IEnumerable<string> PathWordLists(string path) => path.Split('/').Select(segment => string.Join(' ', Words(segment)));

    private static string[] Words(string token) => [.. WordPart().Matches(token).Select(m => m.Value.ToLowerInvariant())];

    private static void AddWordFindings(List<(string Rule, int Line)> findings, int line, string[] words)
    {
        if (ContainsPhrase(words, ChannelPhrases))
        {
            findings.Add((UnknownChannel, line));
        }

        if (ContainsPhrase(words, MarketingPhrases))
        {
            findings.Add((MarketingMessage, line));
        }

        if (ContainsPhrase(words, SwitchPhrases))
        {
            findings.Add((TransactionSwitch, line));
        }
    }

    private static bool ContainsPhrase(string[] words, string[][] phrases) =>
        phrases.Any(phrase => Enumerable.Range(0, Math.Max(0, words.Length - phrase.Length + 1))
            .Any(start => phrase.Select((alternatives, k) => alternatives.Split('|').Contains(words[start + k], StringComparer.Ordinal)).All(match => match)));

    private static void AddMatches(List<(string Rule, int Line)> findings, string rule, Regex pattern, string text)
    {
        foreach (Match match in pattern.Matches(text))
        {
            findings.Add((rule, text.AsSpan(0, match.Index).Count('\n') + 1));
        }
    }

    private static string BlankComments(string text)
    {
        string result = BlockComment().Replace(text, match => new string('\n', match.Value.Count(c => c == '\n')));
        return LineComment().Replace(result, string.Empty);
    }

    [GeneratedRegex(@"/\*.*?\*/|@\*.*?\*@|<!--.*?-->", RegexOptions.CultureInvariant | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BlockComment();

    // A "//" that does not follow a colon (a URL scheme) comments the rest of its line out.
    [GeneratedRegex(@"(?<!:)//[^\n]*", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LineComment();

    [GeneratedRegex(@"[A-Za-z][A-Za-z0-9_\-]*", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Token();

    // A word of a name: an acronym (SMS in SMSGateway), a capitalised or lower-case word, or digits; underscores, hyphens and dots only separate them.
    [GeneratedRegex(@"[A-Z]+(?![a-z])|[A-Z]?[a-z]+|\d+", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex WordPart();

    [GeneratedRegex(@"\bIEmailGateway\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Gateway();

    [GeneratedRegex(@"\bDispatchRecord\s*\.\s*Queue\b|\bDispatchRecords\s*\.\s*Add(?:Range)?\b|\bnew\s+DispatchRecord\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex DispatchRow();
}
