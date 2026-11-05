using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;

using Shopfolio.Endpoints;
using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Web;
using Shopfolio.UnitTests.Fakes;

using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Shopfolio.IntegrationTests.Logging;

/// <summary>
/// The three log writers on real files, in a temporary directory per test (T230; 09 18.1 items 2-5, 8; 09 18.2 items 3, 5; 09 19.2 item 2: a test
/// that writes a file is an integration test). The opened application writes the normal file, the audit line writer and the command line writer
/// write theirs; the tests read the files with a share mode that lets the writer keep them open.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed partial class LoggingFilesTests
{
    // The number of files the library keeps when it is not told otherwise (Serilog.Sinks.File 7.0.0, retainedFileCountLimit: 31): the
    // control group of the "no file is deleted" tests. The number is the library's, not the application's.
    private const int LibraryDefaultRetainedFiles = 31;

    private const int ManyFiles = LibraryDefaultRetainedFiles + 9;

    private static readonly Regex _dayFileName = DayFileNameRegex();

    private readonly CollectionDatabaseFixture _database;

    public LoggingFilesTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public static TheoryData<string> AuditLineFactories => [nameof(AuditLine.InstantDeletion), nameof(AuditLine.AdminCommand)];

    [GeneratedRegex(@"^(?<prefix>[a-z]+)-(?<day>\d{8})\.jsonl$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex DayFileNameRegex();

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Event}: test line.")]
    private static partial void LogWarningLine(ILogger logger, string @event);

    [LoggerMessage(Level = LogLevel.Error, Message = "{Event}: {Email} {Address}")]
    private static partial void LogErrorWithPersonalData(ILogger logger, Exception exception, string @event, string email, string address);

    [LoggerMessage(Level = LogLevel.Critical, Message = "{Event}: {Email}")]
    private static partial void LogCriticalWithPersonalData(ILogger logger, string @event, string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Event}: test line.")]
    private static partial void LogInformationLine(ILogger logger, string @event);

    // --- KK2: three files, the day read from the name (09 18.1 item 2) ---

    [Fact]
    public async Task Writers_WriteThreeSeparateFilesNamedByPrefixAndDay()
    {
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        using HttpClient client = factory.CreateClient();
        ManualTimeProvider time = new(new DateTimeOffset(2026, 11, 5, 21, 30, 0, TimeSpan.Zero));
        IAuditLineWriter audit = factory.Services.GetRequiredService<IAuditLineWriter>();
        IAuditLineWriter command = LoggingSetup.CreateAuditLineWriter(new LoggingOptions { Directory = factory.LogDirectory }, AuditFile.Command, time);

        ILogger logger = factory.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Shopfolio.Test");
        LogWarningLine(logger, LogEvents.JobStarted);
        audit.Write(AuditLine.InstantDeletion("Draft", Guid.NewGuid()));
        command.Write(AuditLine.AdminCommand("reset-password", Guid.NewGuid()));

        // The application writer names the day by the server's own clock (the library); the project's writer by Turkey time (+03:00): 21:30 UTC is already the 6th.
        string[] names = [.. Directory.GetFiles(factory.LogDirectory).Select(Path.GetFileName).Select(n => n!).Order(StringComparer.Ordinal)];
        Assert.Equal(3, names.Length);
        Assert.Matches(_dayFileName, names[0]);
        Assert.Equal(["app", "audit", "command"], names.Select(n => _dayFileName.Match(n).Groups["prefix"].Value));
        Assert.Contains("command-20261106.jsonl", names);
        Assert.Contains("audit-" + TurkeyTime.DateOf(DateTimeOffset.UtcNow).ToString("yyyyMMdd", CultureInfo.InvariantCulture) + ".jsonl", names);

        // "Written" means handed to the operating system: the line is readable right after the call returns, the writer's own file still open.
        JsonElement auditLine = Assert.Single(ReadLines(Path.Combine(factory.LogDirectory, names[1])));
        Assert.Equal(LogEvents.InstantDeletion, auditLine.GetProperty("event").GetString());
        JsonElement commandLine = Assert.Single(ReadLines(Path.Combine(factory.LogDirectory, "command-20261106.jsonl")));
        Assert.Equal(LogEvents.AdminCommand, commandLine.GetProperty("event").GetString());
        Assert.Contains(ReadLines(Path.Combine(factory.LogDirectory, names[0])), l => l.TryGetProperty("event", out JsonElement e) && e.GetString() == LogEvents.JobStarted);
        Assert.DoesNotContain(ReadLines(Path.Combine(factory.LogDirectory, names[0])), l => l.TryGetProperty("event", out JsonElement e) && e.GetString() is LogEvents.InstantDeletion or LogEvents.AdminCommand);
    }

    // --- KK3: no file is deleted by the writers (09 18.1 items 3, 8) ---

    [Fact]
    public async Task Writers_WithMoreFilesThanTheLibraryDefaultKeeps_DeleteNoFile()
    {
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        Directory.CreateDirectory(factory.LogDirectory);
        string[] prefixes = ["app", "audit", "command"];
        foreach (string prefix in prefixes)
        {
            CreateOldFiles(factory.LogDirectory, prefix, ManyFiles);
        }

        using HttpClient client = factory.CreateClient();
        ILogger logger = factory.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Shopfolio.Test");
        LogWarningLine(logger, LogEvents.JobStarted);
        factory.Services.GetRequiredService<IAuditLineWriter>().Write(AuditLine.InstantDeletion("Draft", Guid.NewGuid()));
        LoggingSetup.CreateAuditLineWriter(new LoggingOptions { Directory = factory.LogDirectory }, AuditFile.Command, TimeProvider.System)
            .Write(AuditLine.AdminCommand("reset-password", Guid.NewGuid()));

        foreach (string prefix in prefixes)
        {
            Assert.Equal(ManyFiles + 1, Directory.GetFiles(factory.LogDirectory, prefix + "-*.jsonl").Length);
        }
    }

    [Fact]
    public void Library_WithItsDefaultRetention_DeletesTheOldFiles()
    {
        // The control group: the same files and the same library with its default retention lose files, so the test above is not passing for want of a deletion.
        string directory = Path.Combine(Path.GetTempPath(), "shopfolio-test-logs-" + Guid.NewGuid().ToString("N"));
        try
        {
            CreateOldFiles(directory, "lib", ManyFiles);
            using (Serilog.Core.Logger logger = new LoggerConfiguration()
                .WriteTo.File(Path.Combine(directory, "lib-.jsonl"), formatProvider: CultureInfo.InvariantCulture, rollingInterval: RollingInterval.Day)
                .CreateLogger())
            {
                logger.Warning("{Event}", "control");
            }

            Assert.True(Directory.GetFiles(directory, "lib-*.jsonl").Length <= LibraryDefaultRetainedFiles);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    // --- KK4: the level setting (09 18.1 item 4) ---

    [Theory]
    [InlineData("Debug")]
    [InlineData("Information")]
    public async Task NormalWriter_WithTheLevelSettingAtInformationOrBelow_WritesNoFrameworkOrDataAccessInformationLine(string level)
    {
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn, new Dictionary<string, string> { [LoggingOptions.LevelKey] = level });
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.GetAsync(new Uri(LivenessEndpoint.Path, UriKind.Relative), Ct);
        ILoggerFactory loggers = factory.Services.GetRequiredService<ILoggerFactory>();
        ILogger application = Named(loggers, "Shopfolio.Test");
        ILogger hosting = Named(loggers, "Microsoft.AspNetCore.Hosting.Diagnostics");
        ILogger data = Named(loggers, "Microsoft.EntityFrameworkCore.Database.Command");
        ILogger frameworkWarning = Named(loggers, "Microsoft.AspNetCore.Test");
        LogInformationLine(application, LogEvents.JobStarted);
        LogInformationLine(hosting, LogEvents.JobStarted);
        LogInformationLine(data, LogEvents.JobStarted);
        LogWarningLine(frameworkWarning, LogEvents.JobFinished);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<JsonElement> lines = ReadLines(Assert.Single(Directory.GetFiles(factory.LogDirectory, "app-*.jsonl")));
        Assert.Contains(lines, l => Category(l) == "Shopfolio.Test" && Level(l) == "Information");
        Assert.Contains(lines, l => Category(l) == "Microsoft.AspNetCore.Test" && Level(l) == "Warning");
        Assert.DoesNotContain(lines, l => Category(l).StartsWith("Microsoft.", StringComparison.Ordinal) && Level(l) is "Information" or "Debug" or "Trace" or "Verbose");
    }

    // The two data access failure events are not written; the other lines of the same categories stay at warning (05 9.1.6; 09 18.1 item 4).
    [Fact]
    public async Task NormalWriter_DoesNotWriteTheTwoDataAccessFailureEvents_ButWritesAWarningOfTheSameCategory()
    {
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.GetAsync(new Uri(LivenessEndpoint.Path, UriKind.Relative), Ct);
        ILoggerFactory loggers = factory.Services.GetRequiredService<ILoggerFactory>();
        const int OtherEventId = 1;

        foreach ((string category, int eventId) in LoggingSetup.SuppressedDataAccessFailures)
        {
            ILogger logger = Named(loggers, category);
            logger.Log(LogLevel.Error, new EventId(eventId), "failure", null, static (state, _) => state);
            logger.Log(LogLevel.Warning, new EventId(OtherEventId), "warning", null, static (state, _) => state);
        }

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<JsonElement> lines = ReadLines(Assert.Single(Directory.GetFiles(factory.LogDirectory, "app-*.jsonl")));
        foreach ((string category, _) in LoggingSetup.SuppressedDataAccessFailures)
        {
            Assert.DoesNotContain(lines, l => Category(l) == category && Level(l) == "Error");
            Assert.Contains(lines, l => Category(l) == category && Level(l) == "Warning");
        }
    }

    [Fact]
    public async Task Writers_WithTheLevelSettingAtError_StillWriteTheAuditLineAndTheCommandLine()
    {
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn, new Dictionary<string, string> { [LoggingOptions.LevelKey] = "Error" });
        using HttpClient client = factory.CreateClient();
        ILogger logger = factory.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Shopfolio.Test");
        LogWarningLine(logger, LogEvents.JobStarted);

        factory.Services.GetRequiredService<IAuditLineWriter>().Write(AuditLine.InstantDeletion("Draft", Guid.NewGuid()));
        LoggingSetup.CreateAuditLineWriter(new LoggingOptions { Directory = factory.LogDirectory, Level = Serilog.Events.LogEventLevel.Error }, AuditFile.Command, TimeProvider.System)
            .Write(AuditLine.AdminCommand("reset-password", Guid.NewGuid()));

        Assert.Single(ReadLines(Assert.Single(Directory.GetFiles(factory.LogDirectory, "audit-*.jsonl"))));
        Assert.Single(ReadLines(Assert.Single(Directory.GetFiles(factory.LogDirectory, "command-*.jsonl"))));

        // The warning is below the setting: the normal file has no line of it (the file may not even exist).
        Assert.DoesNotContain(
            Directory.GetFiles(factory.LogDirectory, "app-*.jsonl").SelectMany(ReadLines),
            l => Category(l) == "Shopfolio.Test");
    }

    // --- KK5: an unwritable directory (09 18.1 items 5, 6) ---

    [Fact]
    public async Task UnwritableDirectory_MakesTheAuditWriterThrowAndDoesNotDropTheRequest()
    {
        string blocker = Path.Combine(Path.GetTempPath(), "shopfolio-test-blocker-" + Guid.NewGuid().ToString("N"));
        await File.WriteAllTextAsync(blocker, "a file where a directory should be", Ct);
        try
        {
            string unwritable = Path.Combine(blocker, "logs");
            await using ShopfolioWebApplicationFactory factory = new(_database.Dsn, new Dictionary<string, string> { [LoggingOptions.DirectoryKey] = unwritable });
            using HttpClient client = factory.CreateClient();
            ILogger logger = factory.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Shopfolio.Test");

            // The normal writer: the call returns, the request is answered.
            LogWarningLine(logger, LogEvents.JobStarted);
            using HttpResponseMessage response = await client.GetAsync(new Uri(LivenessEndpoint.Path, UriKind.Relative), Ct);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // The audit writer: the failure comes back to the caller.
            IAuditLineWriter audit = factory.Services.GetRequiredService<IAuditLineWriter>();
            Assert.ThrowsAny<IOException>(() => audit.Write(AuditLine.InstantDeletion("Draft", Guid.NewGuid())));
            Assert.ThrowsAny<IOException>(() => LoggingSetup
                .CreateAuditLineWriter(new LoggingOptions { Directory = unwritable }, AuditFile.Command, TimeProvider.System)
                .Write(AuditLine.AdminCommand("reset-password", Guid.NewGuid())));
        }
        finally
        {
            File.Delete(blocker);
        }
    }

    // --- KK9: the deletion line and the command line carry no field value (09 18.2 item 5) ---

    [Theory]
    [MemberData(nameof(AuditLineFactories))]
    public void AuditLineFactories_TakeOnlyANameAndAnInternalId(string factoryName)
    {
        MethodInfo factory = typeof(AuditLine).GetMethod(factoryName, BindingFlags.Public | BindingFlags.Static)!;

        ParameterInfo[] parameters = factory.GetParameters();

        Assert.Equal([typeof(string), typeof(Guid)], parameters.Select(p => p.ParameterType));
    }

    [Fact]
    public async Task AuditAndCommandLines_CarryOnlyTheEventTheNameAndTheInternalId()
    {
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        using HttpClient client = factory.CreateClient();
        Guid recordId = Guid.NewGuid();
        Guid accountId = Guid.NewGuid();

        factory.Services.GetRequiredService<IAuditLineWriter>().Write(AuditLine.InstantDeletion("Draft", recordId));
        LoggingSetup.CreateAuditLineWriter(new LoggingOptions { Directory = factory.LogDirectory }, AuditFile.Command, TimeProvider.System)
            .Write(AuditLine.AdminCommand("reset-password", accountId));

        JsonElement deletion = Assert.Single(ReadLines(Assert.Single(Directory.GetFiles(factory.LogDirectory, "audit-*.jsonl"))));
        JsonElement admin = Assert.Single(ReadLines(Assert.Single(Directory.GetFiles(factory.LogDirectory, "command-*.jsonl"))));
        Assert.Equal(["time", "level", "category", "event", "kind", "recordId", "message"], deletion.EnumerateObject().Select(p => p.Name));
        Assert.Equal(["time", "level", "category", "event", "command", "accountId", "message"], admin.EnumerateObject().Select(p => p.Name));
        Assert.Equal("Draft", deletion.GetProperty("kind").GetString());
        Assert.Equal(recordId.ToString(), deletion.GetProperty("recordId").GetString());
        Assert.Equal("reset-password", admin.GetProperty("command").GetString());
        Assert.Equal(accountId.ToString(), admin.GetProperty("accountId").GetString());
    }

    // --- The start-up lines of T205 and T226 land in the application file (09 18.1 item 1) ---

    [Fact]
    public async Task StartupSettingsCheck_WritesItsLineToTheApplicationFileAsAJsonObjectWithTheEvent()
    {
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        using HttpClient client = factory.CreateClient();

        List<JsonElement> lines = ReadLines(Assert.Single(Directory.GetFiles(factory.LogDirectory, "app-*.jsonl")));

        Assert.Contains(lines, l => l.TryGetProperty("event", out JsonElement e) && e.GetString() == LogEvents.StartupSettingsChecked);
        Assert.All(lines, l => Assert.Equal(JsonValueKind.Object, l.ValueKind));
    }

    // --- 05 9.2.3: the container's own output carries no personal data; the host's lines do not go on to the framework's providers ---

    [Fact]
    public async Task ErrorLine_WithAnEmailAndAnAddress_ReachesNoProviderOfTheHostAndNoPersonalDataTheFile()
    {
        const string email = "secret.person@example.com";
        const string address = "Street 5 Flat 3";
        await using ShopfolioWebApplicationFactory baseFactory = new(_database.Dsn);
        CollectingLoggerProvider spy = new();
        await using WebApplicationFactory<Program> factory = baseFactory.WithWebHostBuilder(
            builder => builder.ConfigureLogging(logging => logging.AddProvider(spy)));
        using HttpClient client = factory.CreateClient();
        ILogger logger = factory.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Shopfolio.Test");

        LogErrorWithPersonalData(logger, new InvalidOperationException($"{email} {address}"), LogEvents.UnexpectedError, email, address);
        LogCriticalWithPersonalData(logger, LogEvents.UnexpectedError, email);

        // Nothing reached the provider the host has (the console, debug, event source and event log providers are registered the same way).
        Assert.Empty(spy.Lines);
        Assert.NotEmpty(factory.Services.GetServices<ILoggerProvider>());
        string fileText = string.Concat(Directory.GetFiles(baseFactory.LogDirectory, "app-*.jsonl").Select(path => ReadAllShared(path)));
        Assert.Contains(LogEvents.UnexpectedError, fileText, StringComparison.Ordinal);
        Assert.DoesNotContain(email, fileText, StringComparison.Ordinal);
        Assert.DoesNotContain(address, fileText, StringComparison.Ordinal);
    }

    // --- K1 (D-87): the audit writer reads the clock once, so the file's day and the line's time come from the same instant ---

    [Fact]
    public void AuditWriter_AtTheTurkeyMidnight_PutsTheLineInTheFileOfTheDayOfItsOwnTime()
    {
        string directory = Path.Combine(Path.GetTempPath(), "shopfolio-test-logs-" + Guid.NewGuid().ToString("N"));
        try
        {
            // 20:59:59.999 UTC is the 5th in Turkey; the second reading of a clock that moves would be the 6th.
            DateTimeOffset first = new(2026, 11, 5, 20, 59, 59, 999, TimeSpan.Zero);
            ReadCountingTimeProvider time = new(first, first.AddMilliseconds(2));
            IAuditLineWriter writer = LoggingSetup.CreateAuditLineWriter(new LoggingOptions { Directory = directory }, AuditFile.Audit, time);

            writer.Write(AuditLine.InstantDeletion("Draft", Guid.NewGuid()));

            Assert.Equal(1, time.Reads);
            string path = Assert.Single(Directory.GetFiles(directory));
            Assert.Equal("audit-20261105.jsonl", Path.GetFileName(path));
            Assert.Equal(
                first.UtcDateTime.ToString(ApplicationLogFormatter.TimeFormat, CultureInfo.InvariantCulture),
                Assert.Single(ReadLines(path)).GetProperty("time").GetString());
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    // --- Helpers ---

    private static string ReadAllShared(string path)
    {
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    // A clock that moves on every reading: the first reading is the one the writer must use for both the file name and the line.
    private sealed class ReadCountingTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _first;
        private readonly DateTimeOffset _later;
        private int _reads;

        public ReadCountingTimeProvider(DateTimeOffset first, DateTimeOffset later)
        {
            _first = first;
            _later = later;
        }

        public int Reads => _reads;

        public override DateTimeOffset GetUtcNow() => Interlocked.Increment(ref _reads) == 1 ? _first : _later;
    }

    private static void CreateOldFiles(string directory, string prefix, int count)
    {
        Directory.CreateDirectory(directory);
        DateOnly first = new(2020, 1, 1);
        for (int i = 0; i < count; i++)
        {
            string day = first.AddDays(i).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            File.WriteAllText(Path.Combine(directory, $"{prefix}-{day}.jsonl"), "{\"old\":true}\n");
        }
    }

    private static ILogger Named(ILoggerFactory loggers, string category) => loggers.CreateLogger(category);

    private static string Category(JsonElement line) => line.TryGetProperty("category", out JsonElement c) ? c.GetString() ?? string.Empty : string.Empty;

    private static string Level(JsonElement line) => line.GetProperty("level").GetString() ?? string.Empty;

    // The writer keeps its file open: the reader must allow the writer's access.
    private static List<JsonElement> ReadLines(string path)
    {
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using StreamReader reader = new(stream);
        List<JsonElement> lines = [];
        while (reader.ReadLine() is { } text)
        {
            lines.Add(JsonDocument.Parse(text).RootElement.Clone());
        }

        return lines;
    }
}
