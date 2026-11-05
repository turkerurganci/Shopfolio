using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Logging;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the places that name the same system log fact equal (T225; 05 9.1.1, 9.1.2, 9.1.5, 9.2.2; 09 21.4 item 1): the reverse proxy's request id
/// and its access log (the Caddyfile) against the header the application reads (<see cref="RequestLogEnricher.RequestIdHeader"/>), the three
/// log directories against each other (KK1: the application's log, the access log and the server's logs are separate files), the container
/// output rotation of every service, the database image's log settings and the operating system's rotation in the runbook (section L). The
/// proxy's, the operating system's and the database's own logs are not tested here — they are checked at the installation (KK1; runbook L.5).
/// Runs in the Lint step.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class SystemLogsDeployConsistencyTests
{
    // --- Request id (criterion 2) ---

    [Fact]
    public void Caddyfile_HandsTheRequestIdToTheApplicationInTheHeaderTheApplicationReads()
    {
        string caddyfile = Read("deploy", "caddy", "Caddyfile");

        Match forwarded = HeaderUp().Match(caddyfile);

        Assert.True(forwarded.Success, "The Caddyfile does not set a request header for the application.");
        Assert.Equal(RequestLogEnricher.RequestIdHeader, forwarded.Groups["name"].Value);
    }

    [Fact]
    public void Caddyfile_WritesTheSameValueInItsAccessLineAndHandsItOn()
    {
        string caddyfile = Read("deploy", "caddy", "Caddyfile");

        string handedOn = HeaderUp().Match(caddyfile).Groups["value"].Value;
        Match written = LogAppend().Match(caddyfile);

        Assert.True(written.Success, "The Caddyfile does not append the request id to the access line.");
        Assert.Equal("{http.request.uuid}", handedOn);
        Assert.Equal(handedOn, written.Groups["value"].Value);
    }

    [Fact]
    public void Caddyfile_DoesNotWriteRequestHeadersInTheAccessLine()
    {
        string caddyfile = Read("deploy", "caddy", "Caddyfile");

        Assert.Contains("request>headers delete", caddyfile, StringComparison.Ordinal);
        Assert.Contains("resp_headers delete", caddyfile, StringComparison.Ordinal);
    }

    [Fact]
    public void Caddyfile_WritesTheBrowserIdentificationAsItsOwnFieldAndNoOtherHeader()
    {
        string caddyfile = Read("deploy", "caddy", "Caddyfile");

        // The delete removes every request header from the line (05 9.1.5); the one field 05 9.1.2 names is appended on its own.
        Match written = UserAgentAppend().Match(caddyfile);

        Assert.True(written.Success, "The Caddyfile does not append the browser identification to the access line.");
        Assert.Equal("{http.request.header.User-Agent}", written.Groups["value"].Value);
        string[] appended = [.. LogAppendFields().Matches(caddyfile).Select(m => m.Groups["name"].Value).Order(StringComparer.Ordinal)];
        Assert.Equal(["request_id", "user_agent"], appended);
    }

    // --- The fields of the access line: the closed list of 05 9.1.2 (criterion 3; 05 9.1.5 "only the fields of 9.1.2 are written") ---

    // What 05 9.1.2 names, as the fields of Caddy's json line: time, IP, method, path, status, size, duration (these seven are Caddy's own
    // fields), the browser identification and the request id (both appended in the Caddyfile) — and the three envelope fields of the format.
    private static readonly string[] AccessLineFields =
    [
        "ts", "request>remote_ip", "request>method", "request>uri", "status", "size", "duration", "user_agent", "request_id",
        "level", "logger", "msg",
    ];

    // Every field Caddy 2.11 writes in an access line before any filter (read from the line of the real image); a Caddy upgrade that adds a field
    // shows up in the installation check (runbook L.5 item 3), not here.
    private static readonly string[] CaddyDefaultFields =
    [
        "ts", "level", "logger", "msg",
        "request>remote_ip", "request>remote_port", "request>client_ip", "request>proto", "request>method", "request>host", "request>uri",
        "request>headers", "request>tls", "bytes_read", "user_id", "duration", "size", "status", "resp_headers", "uuid",
    ];

    [Fact]
    public void Caddyfile_RemovesEveryDefaultFieldThatIsNotInTheClosedListOf0512_AndNoneOfTheOthers()
    {
        string caddyfile = Read("deploy", "caddy", "Caddyfile");

        string[] deleted = [.. FieldDelete().Matches(caddyfile).Select(m => m.Groups["field"].Value).Order(StringComparer.Ordinal)];
        string[] expected = [.. CaddyDefaultFields.Except(AccessLineFields, StringComparer.Ordinal).Order(StringComparer.Ordinal)];

        Assert.Equal(expected, deleted);
    }

    [Fact]
    public void Runbook_NamesEveryFieldOfTheAccessLineAndEveryFieldTheCaddyfileRemoves()
    {
        string section = RunbookSystemLogs();
        string caddyfile = Read("deploy", "caddy", "Caddyfile");
        string[] removed = [.. FieldDelete().Matches(caddyfile).Select(m => m.Groups["field"].Value)];

        // A name is the last part of the field's path (request>remote_port is remote_port).
        // The runbook writes a field by its name or by its whole path, in code quotes.
        foreach (string path in AccessLineFields.Concat(removed))
        {
            string name = path[(path.LastIndexOf('>') + 1)..];
            Assert.True(
                section.Contains($"`{name}`", StringComparison.Ordinal) || section.Contains($"`{path}`", StringComparison.Ordinal),
                $"The runbook does not name the field {path}.");
        }
    }

    // --- The masking of the address in the access line (criterion 3; 05 9.1.5, 6.4.8; 09 18.2 item 2b; K-1162 (1)) ---

    [Fact]
    public void Caddyfile_MasksTheKeyedPathsOfPathMasker_AndNoOthers()
    {
        string caddyfile = Read("deploy", "caddy", "Caddyfile");
        string pathFilter = UriFilters(caddyfile).First().Pattern;

        Match alternation = KeyedAlternation().Match(pathFilter);
        Assert.True(alternation.Success, "The first request>uri filter is not the keyed path filter.");

        // The proxy's list is read from the Caddyfile, the application's list from PathMasker; each prefix segment is joined by '/+' (empty segments are skipped).
        string[] proxyPrefixes = [.. alternation.Groups["alternatives"].Value.Split('|').Select(alternative => alternative.Replace("/+", "/", StringComparison.Ordinal))];
        string[] applicationPrefixes = [.. PathMasker.KeyedPrefixes.Select(prefix => string.Join('/', prefix))];

        Assert.Equal(applicationPrefixes.Order(StringComparer.Ordinal), proxyPrefixes.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Caddyfile_UriFilters_GiveWhatPathMaskerGivesForEveryKeyedPathAndQueryShape()
    {
        List<(string Pattern, string Replacement)> filters = UriFilters(Read("deploy", "caddy", "Caddyfile"));

        List<string> samples =
        [
            "/arama?search=gizlisozcuk&email=a@b.com",
            "/arama?a&b&c&d",
            "/arama?x=1&bare&y=",
            "/arama?a=b?c=d",
            "/arama?a=1=2",
            // Mixed shapes: bare tokens after, between and before pairs, empty values and empty pieces (the first RE2 version lost the
            // alignment after a pair: '?a=1&b&ali@x.com' left the address in the line).
            "/arama?a=1&b&c",
            "/arama?a=1&b&c&d&e",
            "/arama?a&b=1&c&d",
            "/arama?a=1&b&ali@x.com",
            "/arama?q=1&a&b&c&d",
            "/arama?a=",
            "/arama?a=&b=&c",
            "/arama?a=1&",
            "/arama?&",
            "/arama?&&",
            "/arama?&&&&x",
            "/arama?a&&b",
            "/arama?=v",
            "/arama?=",
            "/arama?a&b?c",
            "/arama??a",
            "/arama?a=b?c=d&e&f=g",
            "/arama?ali%40x.com&b=1",
            "/arama?",
            "/arama",
            "/siparis/erisim",
            "/siparis/erisim/",
            "/indir/?x=1",
            "/siparis/erisim-x/ABC",
            "/odeme/donus-x/ABC",
            "/panel/ABC",
            "/gorsel/abc/def.webp",
        ];
        foreach (string keyed in PathMasker.KeyedPaths)
        {
            string path = keyed.Replace(PathMasker.KeyPlaceholder, "SECRET123", StringComparison.Ordinal);
            samples.AddRange([path, path + "/teyit?email=a@b.com&token=SECRET9", "/" + path, path.ToUpperInvariant(), path.Replace("/", "//", StringComparison.Ordinal), path.Replace("SECRET123", "/SECRET123", StringComparison.Ordinal) + "?bare"]);
        }

        foreach (string sample in samples)
        {
            string viaProxy = sample;
            foreach ((string pattern, string replacement) in filters)
            {
                viaProxy = Regex.Replace(viaProxy, pattern, replacement, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));
            }

            Assert.Equal(PathMasker.Mask(sample), viaProxy);
        }
    }

    [Fact]
    public void Caddyfile_UriFilters_ReadAnAmpersandInThePathAsASeparator_SoTheyMaskMoreThanPathMaskerNeverLess()
    {
        // The one known difference (runbook L.2): RE2 cannot tell the '&' of a path from the '&' of the query, so the proxy masks the part
        // after a literal '&' in the path as if it were a query piece. The application's mask leaves the path alone. The difference is on
        // the safe side: the proxy's line never holds more than the application's.
        List<(string Pattern, string Replacement)> filters = UriFilters(Read("deploy", "caddy", "Caddyfile"));
        string viaProxy = "/gorsel/a&b/c";
        foreach ((string pattern, string replacement) in filters)
        {
            viaProxy = Regex.Replace(viaProxy, pattern, replacement, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));
        }

        Assert.Equal("/gorsel/a&*", viaProxy);
        Assert.Equal("/gorsel/a&b/c", PathMasker.Mask("/gorsel/a&b/c"));
    }

    // --- Nothing deletes a log file but the disposal job (criterion 5) ---

    [Fact]
    public void Caddyfile_HasItsOwnRollingOff_SoTheProxyDeletesNoLogFile()
    {
        string caddyfile = Read("deploy", "caddy", "Caddyfile");

        Assert.Matches(RollDisabled(), caddyfile);
        Assert.DoesNotMatch(RollLimit(), caddyfile);
    }

    // --- KK1: three kinds of log, separate directories ---

    [Fact]
    public void AccessLog_TheApplicationLogAndTheDatabaseLog_AreInThreeDifferentVolumesAndDirectories()
    {
        string compose = Read("deploy", "compose.yaml");
        string caddyfile = Read("deploy", "caddy", "Caddyfile");
        string accessFile = AccessLogFile().Match(caddyfile).Groups["path"].Value;
        string accessDirectory = Path.GetDirectoryName(accessFile)!.Replace('\\', '/');

        // Each kind of log has a volume of its own, mounted where its writer writes: the proxy at the access log's directory, the application at its
        // log directory setting, the database at its log root.
        Assert.StartsWith(accessDirectory + "/", accessFile, StringComparison.Ordinal);
        Assert.Contains(VolumeMounts(compose, "access-logs"), path => path == accessDirectory);
        Assert.Contains(VolumeMounts(compose, "app-logs"), path => path == LoggingOptions.DefaultDirectory);
        Assert.Contains(VolumeMounts(compose, "db-logs"), path => path == DatabaseLogSettings()["log_directory"][..DatabaseLogSettings()["log_directory"].LastIndexOf('/')]);
        Assert.NotEqual(LoggingOptions.DefaultDirectory, accessDirectory);
        Assert.DoesNotContain(VolumeMounts(compose, "app-logs"), path => path == accessDirectory);
    }

    // --- The container's own output (criterion 1, 05 9.2.3) ---

    [Fact]
    public void EveryComposeService_TakesTheSmallContainerOutputRotation()
    {
        string compose = Read("deploy", "compose.yaml").ReplaceLineEndings("\n");
        string services = compose[(compose.IndexOf("\nservices:\n", StringComparison.Ordinal) + 1)..compose.IndexOf("\nnetworks:\n", StringComparison.Ordinal)];

        // Each chunk starts with the service's name line; the text before the first service is skipped.
        string[] blocks = [.. ServiceStart().Split(services).Skip(1)];

        Assert.True(blocks.Length >= 7, "The Compose definition lost services.");
        foreach (string block in blocks)
        {
            string name = block[..block.IndexOf(':', StringComparison.Ordinal)];
            Assert.True(block.Contains("\n    logging: *container-log\n", StringComparison.Ordinal), $"Service {name} has no container output rotation.");
        }

        Match anchor = ContainerLogAnchor().Match(compose);
        Assert.True(anchor.Success, "The x-container-log block was not found.");
        Assert.Equal("json-file", anchor.Groups["driver"].Value);
        Assert.Equal(("10m", "3"), (anchor.Groups["size"].Value, anchor.Groups["files"].Value));
    }

    [Fact]
    public void Runbook_NamesTheContainerOutputRotationOfTheCompose()
    {
        string compose = Read("deploy", "compose.yaml");
        Match anchor = ContainerLogAnchor().Match(compose.ReplaceLineEndings("\n"));
        string section = RunbookSystemLogs();

        Assert.Contains($"max-size:{anchor.Groups["size"].Value} max-file:{anchor.Groups["files"].Value}", section, StringComparison.Ordinal);
    }

    // --- The database's log (criterion 3, 05 9.1.5) ---

    [Fact]
    public void DatabaseImage_WritesItsLogToAFileWithoutParametersAndWithoutDetail()
    {
        Dictionary<string, string> settings = DatabaseLogSettings();

        Assert.Equal("on", settings["logging_collector"]);
        Assert.Equal("terse", settings["log_error_verbosity"]);
        Assert.Equal("0", settings["log_parameter_max_length"]);
        Assert.Equal("0", settings["log_parameter_max_length_on_error"]);
        Assert.Equal("none", settings["log_statement"]);
    }

    [Fact]
    public void DatabaseImage_LeavesRotationAndDeletionToTheOperatingSystem()
    {
        Dictionary<string, string> settings = DatabaseLogSettings();

        Assert.Equal("0", settings["log_rotation_age"]);
        Assert.Equal("0", settings["log_rotation_size"]);
        Assert.Equal("off", settings["log_truncate_on_rotation"]);
    }

    [Fact]
    public void DatabaseLogFile_IsTheSamePathInTheConfigurationTheDockerfileAndTheRunbook()
    {
        Dictionary<string, string> settings = DatabaseLogSettings();
        string directory = settings["log_directory"];
        string dockerfile = Read("deploy", "images", "database", "Dockerfile");

        Assert.Contains($"mkdir -p {directory}", dockerfile, StringComparison.Ordinal);
        Assert.Contains($"chown postgres:logread {directory}", dockerfile, StringComparison.Ordinal);
        Assert.Contains($"{directory}/{settings["log_filename"]}", RunbookSystemLogs(), StringComparison.Ordinal);
        Assert.Contains(directory.Replace("/var/log/postgresql/", string.Empty, StringComparison.Ordinal) + "/" + settings["log_filename"], RunbookSystemLogs(), StringComparison.Ordinal);
    }

    // --- The runbook (criterion 1: settings in DEPLOY_RUNBOOK) ---

    [Fact]
    public void Runbook_NamesTheRequestIdLinesOfTheCaddyfileAndTheClosedRolling()
    {
        string section = RunbookSystemLogs();
        string caddyfile = Read("deploy", "caddy", "Caddyfile");

        Assert.Contains(HeaderUp().Match(caddyfile).Value.Trim(), section, StringComparison.Ordinal);
        Assert.Contains(LogAppend().Match(caddyfile).Value.Trim(), section, StringComparison.Ordinal);
        Assert.Contains("roll_disabled", section, StringComparison.Ordinal);
        Assert.Contains(LoggingOptions.DefaultDirectory, section, StringComparison.Ordinal);
    }

    [Fact]
    public void Runbook_KeepsTheAccessLogForTheDisposalJobAndDeletesTheServersLogsAfterSixMonths()
    {
        string section = RunbookSystemLogs();

        // The access log's rotation entry deletes nothing in any realistic time; the servers' entry deletes after the same count of days twice.
        Match access = AccessLogRotation().Match(section);
        Match servers = ServerLogRotation().Match(section);

        Assert.True(access.Success && servers.Success, "The runbook lost one of the two rotation entries.");
        Assert.True(int.Parse(access.Groups["count"].Value, System.Globalization.CultureInfo.InvariantCulture) >= 36500);
        Assert.False(access.Value.Contains("maxage", StringComparison.Ordinal), "The access log's rotation entry must not have an age.");
        Assert.Equal(servers.Groups["count"].Value, servers.Groups["age"].Value);
        Assert.Equal(183, int.Parse(servers.Groups["count"].Value, System.Globalization.CultureInfo.InvariantCulture));
        Assert.Contains("MaxRetentionSec=6month", section, StringComparison.Ordinal);
    }

    // --- Helpers ---

    private static string Read(params string[] parts) => File.ReadAllText(Path.Combine([RepositoryRoot.Find(), .. parts]));

    private static string RunbookSystemLogs()
    {
        string text = Read("Docs", "DEPLOY_RUNBOOK.md").ReplaceLineEndings("\n");
        int start = text.IndexOf("\n## L. ", StringComparison.Ordinal);
        Assert.True(start >= 0, "Section L of the runbook was not found.");
        return text[start..];
    }

    private static Dictionary<string, string> DatabaseLogSettings()
    {
        Dictionary<string, string> settings = new(StringComparer.Ordinal);
        foreach (string line in Read("deploy", "images", "database", "postgresql.conf.d", "30-logging.conf").ReplaceLineEndings("\n").Split('\n'))
        {
            Match match = ConfSetting().Match(line);
            if (match.Success)
            {
                settings[match.Groups["key"].Value] = match.Groups["value"].Value.Trim('\'');
            }
        }

        return settings;
    }

    // The container-side paths a named volume is mounted at, in every service.
    private static List<string> VolumeMounts(string compose, string volume) =>
    [
        .. new Regex(
            $@"^\s*-\s*{Regex.Escape(volume)}:(?<path>[^\s:]+)(:ro)?\s*$",
            RegexOptions.Multiline | RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(5)).Matches(compose).Select(m => m.Groups["path"].Value),
    ];

    [GeneratedRegex(@"^\s*header_up\s+(?<name>\S+)\s+(?<value>\{[^}]+\})\s*$", RegexOptions.Multiline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex HeaderUp();

    [GeneratedRegex(@"^\s*log_append\s+request_id\s+(?<value>\{[^}]+\})\s*$", RegexOptions.Multiline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LogAppend();

    // The request>uri filters of the access log in the order they run; the replacement keeps the Go placeholders ${1} which .NET reads the same way.
    private static List<(string Pattern, string Replacement)> UriFilters(string caddyfile) =>
    [
        .. UriFilter().Matches(caddyfile).Select(m => (Unquote(m.Groups["pattern"].Value), Unquote(m.Groups["replacement"].Value))),
    ];

    private static string Unquote(string token) => token.Length >= 2 && token[0] == '"' && token[^1] == '"' ? token[1..^1] : token;

    // A token is a word or a quoted string (a pattern or a replacement that holds a space, or is empty, is quoted).
    [GeneratedRegex(@"^\s*request>uri\s+regexp\s+(?<pattern>""[^""]*""|\S+)\s+(?<replacement>""[^""]*""|\S+)\s*$", RegexOptions.Multiline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex UriFilter();

    [GeneratedRegex(@"^\s*(?<field>\S+)\s+delete\s*$", RegexOptions.Multiline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FieldDelete();

    [GeneratedRegex(@"\(\?:(?<alternatives>[^()]+)\)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex KeyedAlternation();

    [GeneratedRegex(@"^\s*log_append\s+user_agent\s+(?<value>\{[^}]+\})\s*$", RegexOptions.Multiline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex UserAgentAppend();

    [GeneratedRegex(@"^\s*log_append\s+(?<name>\S+)\s", RegexOptions.Multiline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LogAppendFields();

    [GeneratedRegex(@"^\s*roll_disabled\s*$", RegexOptions.Multiline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RollDisabled();

    [GeneratedRegex(@"^\s*roll_(size|keep|keep_for|local_time|uncompressed)\b", RegexOptions.Multiline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RollLimit();

    [GeneratedRegex(@"output\s+file\s+(?<path>\S+)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex AccessLogFile();

    [GeneratedRegex(@"^  (?=[a-z][a-z-]*:[ \t]*$)", RegexOptions.Multiline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ServiceStart();

    [GeneratedRegex(
        @"x-container-log:\s*&container-log\s*\n\s+driver:\s*(?<driver>\S+)\s*\n\s+options:\s*\n\s+max-size:\s*""(?<size>[^""]+)""\s*\n\s+max-file:\s*""(?<files>[^""]+)""",
        RegexOptions.CultureInvariant,
        matchTimeoutMilliseconds: 5000)]
    private static partial Regex ContainerLogAnchor();

    [GeneratedRegex(@"^(?<key>[a-z_]+)\s*=\s*(?<value>[^#\r\n]+?)\s*(#.*)?$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ConfSetting();

    [GeneratedRegex(@"access\.log \{(?:(?!\n\}).)*?rotate (?<count>\d+)(?:(?!\n\}).)*\n\}", RegexOptions.Singleline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex AccessLogRotation();

    [GeneratedRegex(@"pgbackrest/\*\.log[^\n]*\{(?:(?!\n\}).)*?rotate (?<count>\d+)\s+maxage (?<age>\d+)(?:(?!\n\}).)*\n\}", RegexOptions.Singleline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ServerLogRotation();
}
