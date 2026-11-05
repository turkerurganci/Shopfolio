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
