using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the backup files of <c>deploy/</c> equal to the documents (05 7.6; 09 17; 11 T07 KK1, KK2, KK3, KK5, KK6, KK7; 09 21.4
/// item 1). Reads repository files only: no database, no network, no file written. Runs in the Lint step, not in the Unit step.
/// The behavior of the scripts is tested by the script tests under <c>tests/scripts/</c>.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class BackupLayoutConsistencyTests
{
    private static string ReadRepositoryFile(params string[] parts) => File.ReadAllText(Path.Combine([RepositoryRoot.Find(), .. parts]));

    private static string ReadLines(string text) => text.ReplaceLineEndings("\n");

    /// <summary>The setting lines of a configuration file: comments and blank lines dropped.</summary>
    private static string[] SettingLines(string text) =>
        ReadLines(text).Split('\n').Select(line => line.Trim()).Where(line => line.Length > 0 && !line.StartsWith('#')).ToArray();

    /// <summary>The lines of one service block of the Compose file (two-space service name, four-space keys).</summary>
    private static List<string> ServiceBlock(string compose, string service)
    {
        List<string> block = [];
        bool inside = false;
        foreach (string line in ReadLines(compose).Split('\n'))
        {
            if (line.StartsWith("  ", StringComparison.Ordinal) && !line.StartsWith("   ", StringComparison.Ordinal) && line.TrimEnd().EndsWith(':'))
            {
                inside = line.Trim() == service + ":";
                continue;
            }

            if (!line.StartsWith(' ') && line.Length > 0)
            {
                inside = false;
            }

            if (inside)
            {
                block.Add(line);
            }
        }

        return block;
    }

    // ---- KK1: the archive setting is in the file ----------------------------------------------------------------------------

    [Fact]
    public void ArchiveSettings_CloseTheLogFileInFiftySecondsAndHandItToPgBackRest()
    {
        IReadOnlyList<string> lines = SettingLines(ReadRepositoryFile("deploy", "images", "database", "postgresql.conf.d", "20-archive.conf"));

        Assert.Contains("archive_mode = on", lines);
        Assert.Contains("archive_timeout = 50s", lines);
        Assert.Contains(lines, line => line.StartsWith("archive_command = 'pgbackrest --stanza=shopfolio archive-push ", StringComparison.Ordinal));
        // A queue limit would drop log files and break point-in-time recovery (05 7.6.9).
        Assert.DoesNotContain(lines, line => line.Contains("archive-push-queue-max", StringComparison.Ordinal));
    }

    // ---- KK6: retention is by time, thirty days -------------------------------------------------------------------------------

    [Fact]
    public void PgBackRestRetention_IsByTimeAndOffServerHoldsThirtyDays()
    {
        IReadOnlyList<string> lines = SettingLines(ReadRepositoryFile("deploy", "images", "database", "pgbackrest.conf"));

        string[] typeLines = lines.Where(line => line.Contains("-retention-full-type=", StringComparison.Ordinal)).ToArray();
        Assert.Equal(2, typeLines.Length);
        Assert.All(typeLines, line => Assert.EndsWith("=time", line, StringComparison.Ordinal));
        Assert.Contains("repo2-retention-full=30", lines);
        Assert.DoesNotContain(lines, line => line.Contains("retention-diff", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, line => line.Contains("-type=count", StringComparison.Ordinal));
    }

    [Fact]
    public void PgBackRestConfig_EncryptsTheOffServerRepositoryAndCarriesNoSecret()
    {
        IReadOnlyList<string> lines = SettingLines(ReadRepositoryFile("deploy", "images", "database", "pgbackrest.conf"));

        Assert.Contains("repo2-type=s3", lines);
        Assert.Contains("repo2-cipher-type=aes-256-cbc", lines);
        Assert.DoesNotContain(lines, line => line.Contains("cipher-pass", StringComparison.Ordinal) || line.Contains("s3-key", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, line => line.Contains("s3-endpoint", StringComparison.Ordinal) || line.Contains("s3-bucket", StringComparison.Ordinal));
    }

    [Fact]
    public void FileBackupJob_KeepsThirtyDaysByTimeAndPrunesWithZeroUnused()
    {
        string job = ReadLines(ReadRepositoryFile("deploy", "images", "backup", "file-backup-job.sh"));
        string[] code = job.Split('\n').Where(line => !line.TrimStart().StartsWith('#')).ToArray();
        string codeText = string.Join('\n', code);

        Assert.Contains("forget --host shopfolio --keep-within 30d", codeText, StringComparison.Ordinal);
        Assert.Contains("prune --max-unused 0", codeText, StringComparison.Ordinal);
        Assert.DoesNotContain("--prune", codeText, StringComparison.Ordinal);
        Assert.DoesNotContain("--keep-last", codeText, StringComparison.Ordinal);
        Assert.DoesNotContain("--read-data", codeText, StringComparison.Ordinal);
    }

    [Fact]
    public void FileBackupJob_RunsTheFourStepsInTheOrderOfTheGuidelines()
    {
        string job = ReadRepositoryFile("deploy", "images", "backup", "file-backup-job.sh");

        Match chain = FourStepChain().Match(job);
        Assert.True(chain.Success, "main() must run backup, forget, prune and check as one && chain");
    }

    // ---- KK3: the paths the backup reads equal the read-only mounts ------------------------------------------------------------

    [Fact]
    public void FileBackupPaths_AreTheReadOnlyMountsOfTheBackupService_WithoutSettingsOrSecrets()
    {
        string compose = ReadRepositoryFile("deploy", "compose.yaml");
        IReadOnlyList<string> block = ServiceBlock(compose, "backup");

        string[] readOnlyTargets = block
            .Select(line => ComposeMount().Match(line))
            .Where(match => match.Success && match.Groups["mode"].Value == "ro")
            .Select(match => match.Groups["target"].Value)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Match defaults = DefaultPaths().Match(ReadRepositoryFile("deploy", "images", "backup", "file-backup-job.sh"));
        Assert.True(defaults.Success, "DEFAULT_BACKUP_PATHS was not found in file-backup-job.sh");
        string[] jobPaths = defaults.Groups["paths"].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries).Order(StringComparer.Ordinal).ToArray();

        Assert.NotEmpty(jobPaths);

        // The backup tool's own log directory is a server log too (05 9.1.4): it is mounted writable (the tool writes it) and is
        // the one path beside the read-only mounts that the job reads.
        const string ownLogDirectory = "/var/log/backup";
        Assert.Contains(block, line => ComposeMount().Match(line) is { Success: true } mount
            && mount.Groups["target"].Value == ownLogDirectory
            && mount.Groups["mode"].Value != "ro");
        string[] expectedPaths = readOnlyTargets.Append(ownLogDirectory).Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(expectedPaths, jobPaths);

        // No settings file and no secret directory is mounted into the backup container or read by the job (05 7.6.3).
        string mounts = string.Join('\n', block.Where(line => ComposeMount().IsMatch(line)));
        Assert.DoesNotContain("/etc/shopfolio", mounts, StringComparison.Ordinal);
        Assert.DoesNotContain("secrets", mounts, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("settings", mounts, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(jobPaths, path => path.Contains("etc", StringComparison.Ordinal) || path.Contains("run/secrets", StringComparison.Ordinal));
    }

    [Fact]
    public void BackupService_RunsTheFileBackupJob_AndTheDatabaseServiceKeepsTheImageCommand()
    {
        string compose = ReadRepositoryFile("deploy", "compose.yaml");

        Assert.Contains("    command: [\"/usr/local/bin/file-backup-job.sh\"]", ServiceBlock(compose, "backup"));
        Assert.DoesNotContain(ServiceBlock(compose, "database"), line => line.TrimStart().StartsWith("command:", StringComparison.Ordinal));
    }

    // ---- KK2: both jobs start long before the destruction job --------------------------------------------------------------------

    [Fact]
    public void BackupSchedule_HasOneDefaultForBothJobsThatEndsBeforeTheDestructionJob()
    {
        string compose = ReadRepositoryFile("deploy", "compose.yaml");
        string example = ReadRepositoryFile("deploy", ".env.example");
        string runbook = ReadRepositoryFile("Docs", "DEPLOY_RUNBOOK.md");

        string[] services = ["database", "backup"];
        string[] runTimes = services.Select(service => Default(ServiceBlock(compose, service), "BACKUP_RUN_TIME")).ToArray();
        string[] zones = services.Select(service => Default(ServiceBlock(compose, service), "BACKUP_TIME_ZONE")).ToArray();
        Assert.Equal(runTimes[0], runTimes[1]);
        Assert.Equal(zones[0], zones[1]);

        // The destruction job runs at 03:00 Turkey time (05 5.3.2, 7.4.1); the zone of the schedule is the same.
        Assert.Equal("Europe/Istanbul", zones[0]);
        TimeSpan runTime = TimeSpan.ParseExact(runTimes[0], @"hh\:mm", CultureInfo.InvariantCulture);
        Assert.True(runTime <= TimeSpan.FromHours(1.5), $"the default run time {runTimes[0]} leaves less than 90 minutes before 03:00");
        Assert.True(runTime >= TimeSpan.FromHours(0.5), $"the default run time {runTimes[0]} is too close to midnight (a day's change of the date)");

        // The same values in the settings template and the runbook.
        Assert.Contains($"BACKUP_RUN_TIME={runTimes[0]}", example, StringComparison.Ordinal);
        Assert.Contains($"BACKUP_TIME_ZONE={zones[0]}", example, StringComparison.Ordinal);
        Assert.Contains($"| `BACKUP_RUN_TIME` | `{runTimes[0]}` |", runbook, StringComparison.Ordinal);
        Assert.Contains($"| `BACKUP_TIME_ZONE` | `{zones[0]}` |", runbook, StringComparison.Ordinal);
    }

    [Fact]
    public void BackupSettings_AreInComposeTheSettingsTemplateAndTheRunbook()
    {
        string compose = ReadRepositoryFile("deploy", "compose.yaml");
        string example = ReadRepositoryFile("deploy", ".env.example");
        string runbook = ReadRepositoryFile("Docs", "DEPLOY_RUNBOOK.md");

        string[] names = ["BACKUP_S3_ENDPOINT", "BACKUP_S3_REGION", "BACKUP_S3_BUCKET", "BACKUP_S3_URI_STYLE", "BACKUP_RUN_TIME", "BACKUP_TIME_ZONE"];
        foreach (string name in names)
        {
            Assert.Contains(ServiceBlock(compose, "database"), line => line.Contains($"- {name}=", StringComparison.Ordinal));
            Assert.Contains(ServiceBlock(compose, "backup"), line => line.Contains($"- {name}=", StringComparison.Ordinal));
            Assert.Contains($"\n{name}=", ReadLines(example), StringComparison.Ordinal);
            Assert.Contains($"| `{name}` |", runbook, StringComparison.Ordinal);
        }
    }

    // ---- KK5 / KK6: the bucket rule and the key policy -----------------------------------------------------------------------------

    [Fact]
    public void BucketLifecycleRule_ExpiresOnlyOldVersionsAndAbortsUnfinishedUploads()
    {
        using JsonDocument document = JsonDocument.Parse(ReadRepositoryFile("deploy", "backup", "bucket-lifecycle.json"));
        JsonElement rule = Assert.Single(document.RootElement.GetProperty("Rules").EnumerateArray());

        Assert.Equal("Enabled", rule.GetProperty("Status").GetString());
        Assert.Equal(30, rule.GetProperty("NoncurrentVersionExpiration").GetProperty("NoncurrentDays").GetInt32());
        Assert.Equal(7, rule.GetProperty("AbortIncompleteMultipartUpload").GetProperty("DaysAfterInitiation").GetInt32());
        // Nothing may expire the CURRENT object: no Expiration, no transition (05 7.6.6).
        string[] properties = rule.EnumerateObject().Select(property => property.Name).Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(["AbortIncompleteMultipartUpload", "Filter", "ID", "NoncurrentVersionExpiration", "Status"], properties);
    }

    [Fact]
    public void KeyPolicy_AllowsTheCurrentObjectOfOneOwnPrefixAndNeverTheOldVersionOrBucketSettings()
    {
        using JsonDocument document = JsonDocument.Parse(ReadRepositoryFile("deploy", "backup", "key-policy.template.json"));
        JsonElement[] statements = document.RootElement.GetProperty("Statement").EnumerateArray().ToArray();
        string[] allowed = statements
            .Where(statement => statement.GetProperty("Effect").GetString() == "Allow")
            .SelectMany(statement => statement.GetProperty("Action").EnumerateArray().Select(action => action.GetString()!))
            .ToArray();

        Assert.Contains("s3:PutObject", allowed);
        Assert.Contains("s3:DeleteObject", allowed);
        Assert.Contains("s3:GetLifecycleConfiguration", allowed);
        Assert.Contains("s3:GetBucketVersioning", allowed);
        Assert.DoesNotContain(allowed, action => action.Contains("Version", StringComparison.Ordinal) && !action.StartsWith("s3:GetBucketVersioning", StringComparison.Ordinal));
        Assert.DoesNotContain(allowed, action => action.StartsWith("s3:PutBucket", StringComparison.Ordinal) || action.StartsWith("s3:PutLifecycle", StringComparison.Ordinal)
            || action.StartsWith("s3:PutObjectRetention", StringComparison.Ordinal) || action.StartsWith("s3:BypassGovernance", StringComparison.Ordinal) || action == "s3:*");

        // Every object resource sits under the one prefix placeholder.
        string[] objectResources = statements
            .Select(statement => statement.GetProperty("Resource").GetString()!)
            .Where(resource => resource.Contains('/', StringComparison.Ordinal))
            .ToArray();
        Assert.NotEmpty(objectResources);
        Assert.All(objectResources, resource => Assert.Equal("arn:aws:s3:::__BUCKET__/__PREFIX__/*", resource));
    }

    [Fact]
    public void BucketChecks_TryEveryRefusedAction()
    {
        string script = ReadRepositoryFile("deploy", "backup", "bucket-checks.sh");

        foreach (string denied in new[]
        {
            "writes outside its prefix",
            "deletes an old version",
            "switches versioning off",
            "changes the lifecycle rule",
            "deletes the lifecycle rule",
            "changes the object lock",
            "writes the bucket policy",
        })
        {
            Assert.Contains($"expect_denied \"${{_name}}: {denied}", script, StringComparison.Ordinal);
        }

        Assert.Contains("--version-id", script, StringComparison.Ordinal);
    }

    // ---- KK7: the drill step of the runbook --------------------------------------------------------------------------------------

    [Fact]
    public void RunbookDrill_HasTheFourChecksAndTheThreeRecordFields()
    {
        string runbook = ReadLines(ReadRepositoryFile("Docs", "DEPLOY_RUNBOOK.md"));
        int start = runbook.IndexOf("\n### J.5 Kurtarma provası", StringComparison.Ordinal);
        Assert.True(start >= 0, "DEPLOY_RUNBOOK section J.5 (the drill) is missing");
        string drill = runbook[start..];

        // The four checks of 05 7.6.8 / criterion 7.
        Assert.Contains("sunucunun dışındaki depodan döner", drill, StringComparison.Ordinal);
        Assert.Contains("--repo=2", drill, StringComparison.Ordinal);
        Assert.Contains("örnek geri alır", drill, StringComparison.Ordinal);
        Assert.Contains("depo bütünlüğünü veri okuyarak", drill, StringComparison.Ordinal);
        Assert.Contains("Yaşam döngüsü kuralının varlığı", drill, StringComparison.Ordinal);

        // The record: the repository restored from, the duration, the last moment.
        Assert.Contains("| Dönülen depo | Geri dönüşün süresi (indirme dahil) | Dönülen son an |", drill, StringComparison.Ordinal);

        // The drill scripts print exactly those three fields.
        string databaseDrill = ReadRepositoryFile("deploy", "images", "database", "database-restore-drill.sh");
        string fileDrill = ReadRepositoryFile("deploy", "images", "backup", "file-restore-drill.sh");
        foreach (string script in new[] { databaseDrill, fileDrill })
        {
            Assert.Contains("drill-record: repository=", script, StringComparison.Ordinal);
            Assert.Contains("drill-record: restore-duration-seconds=", script, StringComparison.Ordinal);
            Assert.Contains("drill-record: last-moment=", script, StringComparison.Ordinal);
        }

        Assert.Contains("--repo=\"$REPOSITORY\"", databaseDrill, StringComparison.Ordinal);
        Assert.Contains("read-data-subset", fileDrill, StringComparison.Ordinal);
    }

    // ---- the database job and the images ------------------------------------------------------------------------------------------

    [Fact]
    public void DatabaseJob_NamesTheRepositoryOfEveryBackup()
    {
        string job = ReadRepositoryFile("deploy", "images", "database", "database-job.sh");

        MatchCollection backups = BackupCommand().Matches(job);
        Assert.Equal(2, backups.Count);
        Assert.All(backups, match => Assert.Contains("--repo=", match.Value, StringComparison.Ordinal));
        Assert.Contains("--type=full", job, StringComparison.Ordinal);
    }

    [Fact]
    public void DatabaseImage_KeepsTheDatabaseUserAndWrapsTheOfficialEntrypoint()
    {
        string dockerfile = ReadRepositoryFile("deploy", "images", "database", "Dockerfile");

        Assert.Contains("USER postgres", dockerfile, StringComparison.Ordinal);
        Assert.Contains("ENTRYPOINT [\"/usr/local/bin/database-entrypoint.sh\"]", dockerfile, StringComparison.Ordinal);
        Assert.Contains("CMD [\"postgres\", \"-c\", \"config_file=/etc/postgresql/postgresql.conf\"]", dockerfile, StringComparison.Ordinal);
        string entrypoint = ReadRepositoryFile("deploy", "images", "database", "database-entrypoint.sh");
        Assert.Contains("/usr/local/bin/docker-entrypoint.sh \"$@\"", entrypoint, StringComparison.Ordinal);
        Assert.DoesNotContain("gosu", entrypoint, StringComparison.Ordinal);
        Assert.DoesNotContain("set -x", entrypoint, StringComparison.Ordinal);
    }

    private static string Default(IReadOnlyList<string> block, string name)
    {
        Regex pattern = new($@"- {name}=\$\{{{name}:-(?<value>[^}}]+)\}}", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));
        foreach (string line in block)
        {
            Match match = pattern.Match(line);
            if (match.Success)
            {
                return match.Groups["value"].Value;
            }
        }

        throw new InvalidOperationException($"{name} has no default in the service block.");
    }

    [GeneratedRegex(@"run_step backup step_backup && run_step forget step_forget && run_step prune step_prune && run_step check step_check", RegexOptions.CultureInvariant)]
    private static partial Regex FourStepChain();

    [GeneratedRegex(@"^      - (?<source>[a-z0-9\-/]+):(?<target>/[a-z0-9\-/]+)(?::(?<mode>ro))?\s*$", RegexOptions.CultureInvariant | RegexOptions.Multiline)]
    private static partial Regex ComposeMount();

    [GeneratedRegex(@"^DEFAULT_BACKUP_PATHS=""(?<paths>[^""]+)""", RegexOptions.CultureInvariant | RegexOptions.Multiline)]
    private static partial Regex DefaultPaths();

    [GeneratedRegex(@"pgbackrest_run [^\n]*\bbackup\b", RegexOptions.CultureInvariant)]
    private static partial Regex BackupCommand();
}
