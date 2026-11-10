using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Core;
using Serilog.Events;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Web;
using Shopfolio.UnitTests.Fakes;

using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Shopfolio.IntegrationTests.Logging;

/// <summary>
/// The application log as the system logs task sees it (T225; 05 9.1.1, 9.1.3, 9.1.5, 9.1.6, 9.2.1; 09 18.2 items 1, 2, 4, 7). The application opens
/// on a real database and writes a real file in a temporary directory (09 19.2 item 2). Four requests reach it through a test pipeline (500, 422,
/// 401 and a success) and the tests read the lines they leave: KK2 the request id of the proxy's header on every line of a request, KK3 no
/// forbidden value in any line, KK4 the level setting reaches the application's categories only, KK5 a file that has not expired stays on the
/// disk at the day's turn and at a restart. The proxy's, the operating system's and the database's own logs are checked at the installation
/// (KK1), not here.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed class SystemLogsTests
{
    private const string TestCategory = "Shopfolio.T225Test";

    private const string FrameworkCategory = "Microsoft.AspNetCore.T225Test";

    private const string DataCategory = "Microsoft.EntityFrameworkCore.T225Test";

    // The values the requests carry and no line may contain (05 9.1.5): a form body, a cookie value, an e-mail address in the query and in an
    // exception message, an address in an exception message.
    private const string FormEmail = "form.person@example.com";

    private const string FormPassword = "hunter2-form-password";

    private const string CookieValue = "cookie-session-value-6f1d";

    private const string QueryEmail = "query.person@example.com";

    private const string ExceptionEmail = "exception.person@example.com";

    private const string ExceptionAddress = "5 Main Street Flat 3";

    private readonly CollectionDatabaseFixture _database;

    public SystemLogsTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    /// <summary>Every level of the library, as the setting names them.</summary>
    public static TheoryData<LogEventLevel> Levels => [.. Enum.GetValues<LogEventLevel>()];

    // --- KK2: the request id of the proxy's header on every line of the request (09 18.2 items 1, 7) ---

    [Fact]
    public async Task ErrorRequest_WritesLinesWithTheRequestFieldsAndTheIdOfTheHeader()
    {
        await using ShopfolioWebApplicationFactory baseFactory = new(_database.Dsn);
        await using WebApplicationFactory<Program> factory = WithTestPipeline(baseFactory);
        using HttpClient client = factory.CreateClient();
        string requestId = NewRequestId();

        using HttpResponseMessage response = await Send(client, HttpMethod.Post, "/t225/error?email=" + QueryEmail, requestId, formBody: true);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        List<JsonElement> lines = LinesOfRequest(baseFactory, requestId);
        JsonElement framework = Assert.Single(lines, l => Category(l) == "Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware");
        JsonElement application = Assert.Single(lines, l => Event(l) == LogEvents.UnexpectedError);
        Assert.Equal("Error", Level(framework));
        Assert.Equal(typeof(InvalidOperationException).FullName, framework.GetProperty("exceptionType").GetString());
        foreach (JsonElement line in new[] { framework, application })
        {
            Assert.Equal(requestId, line.GetProperty("requestId").GetString());
            Assert.Equal("POST", line.GetProperty("method").GetString());
            Assert.Equal("/t225/error?email=*", line.GetProperty("path").GetString());
        }
    }

    [Fact]
    public async Task RejectedRequest_WritesTheRejectionLineWithTheFieldsAndTheIdOfTheHeader()
    {
        await using ShopfolioWebApplicationFactory baseFactory = new(_database.Dsn);
        await using WebApplicationFactory<Program> factory = WithTestPipeline(baseFactory);
        using HttpClient client = factory.CreateClient();
        string requestId = NewRequestId();

        using HttpResponseMessage response = await Send(client, HttpMethod.Post, "/t225/invalid", requestId, formBody: true);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        JsonElement line = Assert.Single(LinesOfRequest(baseFactory, requestId));
        Assert.Equal(LogEvents.ValidationRejected, Event(line));
        Assert.Equal(requestId, line.GetProperty("requestId").GetString());
        Assert.Equal("POST", line.GetProperty("method").GetString());
        Assert.Equal("/t225/invalid", line.GetProperty("path").GetString());
        JsonElement fields = line.GetProperty("fields");
        Assert.Equal(["name", "quantity"], fields.EnumerateArray().Select(f => f.GetProperty("field").GetString()));
        Assert.Equal(["Required", "OutOfRange"], fields.EnumerateArray().Select(f => f.GetProperty("code").GetString()));
        Assert.False(line.TryGetProperty("code", out _));
    }

    [Fact]
    public async Task UnauthorizedRequest_WritesTheAccessDeniedLineWithTheIdOfTheHeader()
    {
        await using ShopfolioWebApplicationFactory baseFactory = new(_database.Dsn);
        await using WebApplicationFactory<Program> factory = WithTestPipeline(baseFactory);
        using HttpClient client = factory.CreateClient();
        string requestId = NewRequestId();

        using HttpResponseMessage response = await Send(client, HttpMethod.Get, "/t225/denied", requestId, formBody: false);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        JsonElement line = Assert.Single(LinesOfRequest(baseFactory, requestId));
        Assert.Equal(LogEvents.AccessDenied, Event(line));
        Assert.Equal(requestId, line.GetProperty("requestId").GetString());
        Assert.Equal("GET", line.GetProperty("method").GetString());
        Assert.Equal("/t225/denied", line.GetProperty("path").GetString());
    }

    [Fact]
    public async Task TwoRequests_KeepTheirOwnIds_AndASuccessfulRequestWritesNoLine()
    {
        await using ShopfolioWebApplicationFactory baseFactory = new(_database.Dsn);
        await using WebApplicationFactory<Program> factory = WithTestPipeline(baseFactory);
        using HttpClient client = factory.CreateClient();
        string first = NewRequestId();
        string second = NewRequestId();
        string third = NewRequestId();

        using HttpResponseMessage one = await Send(client, HttpMethod.Get, "/t225/denied", first, formBody: false);
        using HttpResponseMessage two = await Send(client, HttpMethod.Get, "/t225/denied", second, formBody: false);
        using HttpResponseMessage ok = await Send(client, HttpMethod.Get, "/t225/ok", third, formBody: false);

        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.Equal(first, Assert.Single(LinesOfRequest(baseFactory, first)).GetProperty("requestId").GetString());
        Assert.Equal(second, Assert.Single(LinesOfRequest(baseFactory, second)).GetProperty("requestId").GetString());
        Assert.Empty(LinesOfRequest(baseFactory, third));
    }

    [Fact]
    public async Task RequestWithoutOrWithAnImplausibleHeader_WritesNoRequestId()
    {
        await using ShopfolioWebApplicationFactory baseFactory = new(_database.Dsn);
        await using WebApplicationFactory<Program> factory = WithTestPipeline(baseFactory);
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage none = await client.GetAsync(new Uri("/t225/denied", UriKind.Relative), Ct);
        using HttpResponseMessage odd = await Send(client, HttpMethod.Get, "/t225/denied", "a@b.com <script>", formBody: false);

        Assert.Equal(HttpStatusCode.Unauthorized, none.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, odd.StatusCode);
        List<JsonElement> denied = [.. ReadApplicationLines(baseFactory.LogDirectory).Where(l => Event(l) == LogEvents.AccessDenied)];
        Assert.Equal(2, denied.Count);
        Assert.All(denied, l => Assert.False(l.TryGetProperty("requestId", out _)));
    }

    // --- KK3: no forbidden value in any line, at any level (05 9.1.5) ---

    [Fact]
    public async Task SampleLines_ContainNoFormBodyCookieAddressEmailOrPassword()
    {
        await using ShopfolioWebApplicationFactory baseFactory = new(_database.Dsn, new Dictionary<string, string> { [LoggingOptions.LevelKey] = nameof(LogEventLevel.Verbose) });
        await using WebApplicationFactory<Program> factory = WithTestPipeline(baseFactory);
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage error = await Send(client, HttpMethod.Post, "/t225/error?email=" + QueryEmail, NewRequestId(), formBody: true);
        using HttpResponseMessage invalid = await Send(client, HttpMethod.Post, "/t225/invalid", NewRequestId(), formBody: true);
        using HttpResponseMessage denied = await Send(client, HttpMethod.Get, "/t225/denied?token=abc123", NewRequestId(), formBody: false);

        Assert.Equal(HttpStatusCode.InternalServerError, error.StatusCode);
        string text = ReadApplicationText(baseFactory.LogDirectory);
        Assert.Contains(LogEvents.UnexpectedError, text, StringComparison.Ordinal);
        foreach (string forbidden in new[] { FormEmail, FormPassword, CookieValue, QueryEmail, ExceptionEmail, ExceptionAddress, "abc123" })
        {
            Assert.DoesNotContain(forbidden, text, StringComparison.Ordinal);
        }

        // Besides the values: no field of a forbidden name in any line (the names of 09 18.2 item 2a, in the formatter's own list).
        foreach (JsonElement line in ReadApplicationLines(baseFactory.LogDirectory))
        {
            Assert.DoesNotContain(
                line.EnumerateObject().Select(p => p.Name),
                name => ApplicationLogFormatter.IsForbiddenName(name));
        }
    }

    // --- KK4: the level setting reaches the application's categories only (05 9.1.6; 09 18.1 item 4) ---

    [Theory]
    [MemberData(nameof(Levels))]
    public async Task LevelSetting_ChangesTheApplicationCategoryAndNeverTheFrameworkOrDataAccessCategory(LogEventLevel setting)
    {
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn, new Dictionary<string, string> { [LoggingOptions.LevelKey] = setting.ToString() });
        using HttpClient client = factory.CreateClient();
        ILoggerFactory loggers = factory.Services.GetRequiredService<ILoggerFactory>();

        foreach ((LogLevel written, LogEventLevel library) in Emitted)
        {
            foreach (string category in Categories)
            {
                Emit(loggers.CreateLogger(category), written, library);
            }
        }

        List<JsonElement> lines = ReadApplicationLines(factory.LogDirectory);
        foreach ((_, LogEventLevel library) in Emitted)
        {
            bool application = lines.Any(l => Category(l) == TestCategory && Level(l) == library.ToString());
            bool framework = lines.Any(l => Category(l) == FrameworkCategory && Level(l) == library.ToString());
            bool data = lines.Any(l => Category(l) == DataCategory && Level(l) == library.ToString());

            Assert.Equal(library >= setting, application);
            Assert.Equal(library >= LogEventLevel.Warning, framework);
            Assert.Equal(library >= LogEventLevel.Warning, data);
        }
    }

    // --- KK5: a file that has not expired stays at the turn of the day and at a restart (05 9.2.1, 9.2.2) ---

    [Fact]
    public void NormalWriter_WhenItStartsANewDaysFile_KeepsTheFilesOfTheEarlierDays_ManyMoreThanTheLibraryKeepsByDefault()
    {
        // The library takes its day from the server's clock, which a test cannot move; the moment the day turns is the moment the writer creates
        // the new day's file, and that is when the library applies its retention. So the earlier days' files are laid down first and the writer
        // creates today's: with the default retention the library would delete the oldest ones (the control group of T230), with ours nothing goes.
        string directory = NewDirectory();
        try
        {
            DateTime today = DateTime.Now.Date;
            string[] earlier = [.. Enumerable.Range(1, LibraryDefaultRetainedFiles + 9).Select(age => OldFile(directory, LoggingSetup.ApplicationPrefix, today.AddDays(-age)))];

            using (Logger logger = LoggingSetup.Configure(new LoggerConfiguration(), new LoggingOptions { Directory = directory }, new NoEnricher()).CreateLogger())
            {
                logger.Write(Warning(DateTimeOffset.Now, "today"));
            }

            Assert.All(earlier, path => Assert.True(File.Exists(path), $"{path} was deleted."));
            string newFile = Assert.Single(Directory.GetFiles(directory, "app-*.jsonl").Except(earlier));
            Assert.Equal("today", Assert.Single(ReadLines(newFile)).GetProperty("message").GetString());
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AuditAndCommandWriters_AtTheTurnOfTheTurkeyDay_KeepTheOldFile(bool commandWriter)
    {
        AuditFile file = commandWriter ? AuditFile.Command : AuditFile.Audit;
        string directory = NewDirectory();
        try
        {
            ManualTimeProvider time = new(new DateTimeOffset(2026, 11, 5, 20, 59, 0, TimeSpan.Zero));
            IAuditLineWriter writer = LoggingSetup.CreateAuditLineWriter(new LoggingOptions { Directory = directory }, file, time);
            string prefix = LoggingSetup.Prefix(file);

            writer.Write(AuditLine.InstantDeletion("Draft", Guid.NewGuid()));
            time.Advance(TimeSpan.FromMinutes(2));
            writer.Write(AuditLine.InstantDeletion("Draft", Guid.NewGuid()));

            Assert.Equal([$"{prefix}-20261105.jsonl", $"{prefix}-20261106.jsonl"], Directory.GetFiles(directory).Select(p => Path.GetFileName(p)!).Order(StringComparer.Ordinal));
            Assert.Single(ReadLines(Path.Combine(directory, $"{prefix}-20261105.jsonl")));
            Assert.Single(ReadLines(Path.Combine(directory, $"{prefix}-20261106.jsonl")));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Restart_KeepsEveryFileOfTheDirectory_TheOldLinesIncludedAndAppendsToTheDaysFile()
    {
        string directory = NewDirectory();
        try
        {
            Dictionary<string, string> settings = new() { [LoggingOptions.DirectoryKey] = directory };
            DateTime today = DateTime.UtcNow.Date;

            // Files of the three kinds from a day ago, three months ago and five months and three weeks ago (P-45 is six months), and one the
            // application did not write: nothing here has expired, and the writers delete nothing in any case.
            string[] kept = [.. NotExpiredAgesInDays.SelectMany(age => Prefixes.Select(prefix => OldFile(directory, prefix, today.AddDays(-age))))];
            File.WriteAllText(Path.Combine(directory, "unrelated.txt"), "not a log");

            string first = await RunOnce(settings, "first run", kept);
            string second = await RunOnce(settings, "second run", kept);

            Assert.Equal(first, second);
            foreach (string path in kept)
            {
                Assert.Equal("{\"old\":true}\n", await File.ReadAllTextAsync(path, Ct));
            }

            Assert.True(File.Exists(Path.Combine(directory, "unrelated.txt")));
            string[] applicationLines = [.. ReadLines(first).Select(l => l.GetProperty("message").GetString() ?? string.Empty)];
            Assert.Contains(applicationLines, m => m.Contains("first run", StringComparison.Ordinal));
            Assert.Contains(applicationLines, m => m.Contains("second run", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    // --- Helpers ---

    private static readonly string[] Categories = [TestCategory, FrameworkCategory, DataCategory];

    private static readonly string[] Prefixes = [LoggingSetup.ApplicationPrefix, LoggingSetup.Prefix(AuditFile.Audit), LoggingSetup.Prefix(AuditFile.Command)];

    // Two days ago, three months ago and five months and three weeks ago: P-45 is six months, so none of these has expired.
    private static readonly int[] NotExpiredAgesInDays = [2, 90, 173];

    // The number of files the library keeps when it is not told otherwise: the default of the parameter of its own file sink method (31 in
    // Serilog.Sinks.File 7.0.0), read from the library, not typed here.
    private static readonly int LibraryDefaultRetainedFiles = (int)typeof(FileLoggerConfigurationExtensions)
        .GetMethods()
        .Where(method => method.Name == "File")
        .SelectMany(method => method.GetParameters())
        .First(parameter => parameter.Name == "retainedFileCountLimit" && parameter.DefaultValue is int)
        .DefaultValue!;

    // The levels of the host's logger, with the library's level each one becomes.
    private static readonly (LogLevel Written, LogEventLevel Library)[] Emitted =
    [
        (LogLevel.Debug, LogEventLevel.Debug),
        (LogLevel.Information, LogEventLevel.Information),
        (LogLevel.Warning, LogEventLevel.Warning),
        (LogLevel.Error, LogEventLevel.Error),
        (LogLevel.Critical, LogEventLevel.Fatal),
    ];

    // Opens the application on the directory, writes one line, closes it, and returns the day's application file.
    private async Task<string> RunOnce(Dictionary<string, string> settings, string text, string[] alreadyThere)
    {
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn, settings);
        using HttpClient client = factory.CreateClient();
        Warn(factory.Services.GetRequiredService<ILoggerFactory>().CreateLogger(TestCategory), LogEvents.JobStarted, text);
        string directory = settings[LoggingOptions.DirectoryKey];
        return Assert.Single(Directory.GetFiles(directory, "app-*.jsonl").Except(alreadyThere));
    }

    private static string OldFile(string directory, string prefix, DateTime day)
    {
        string path = Path.Combine(directory, $"{prefix}-{day.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}.jsonl");
        File.WriteAllText(path, "{\"old\":true}\n");
        return path;
    }

    private static string NewDirectory()
    {
        string directory = Path.Combine(Path.GetTempPath(), "shopfolio-test-logs-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static string NewRequestId() => "t225-" + Guid.NewGuid().ToString("N");

    private static LogEvent Warning(DateTimeOffset time, string text) =>
        new(time, LogEventLevel.Warning, null, new Serilog.Parsing.MessageTemplateParser().Parse(text), []);

    private static WebApplicationFactory<Program> WithTestPipeline(ShopfolioWebApplicationFactory baseFactory) =>
        baseFactory.WithWebHostBuilder(builder => builder.ConfigureServices(services => services.AddTransient<IStartupFilter, TestPipelineFilter>()));

    // A request as the proxy would hand it on: the id in the header, a form body and a cookie on the request that has a body.
    private static Task<HttpResponseMessage> Send(HttpClient client, HttpMethod method, string path, string requestId, bool formBody)
    {
        HttpRequestMessage request = new(method, new Uri(path, UriKind.Relative));
        request.Headers.TryAddWithoutValidation("X-Request-Id", requestId);
        if (formBody)
        {
            request.Headers.TryAddWithoutValidation("Cookie", "session=" + CookieValue);
            request.Content = new StringContent($"email={FormEmail}&password={FormPassword}", Encoding.UTF8, "application/x-www-form-urlencoded");
        }

        return client.SendAsync(request, Ct);
    }

    private static List<JsonElement> LinesOfRequest(ShopfolioWebApplicationFactory factory, string requestId) =>
        [.. ReadApplicationLines(factory.LogDirectory).Where(l => l.TryGetProperty("requestId", out JsonElement id) && id.GetString() == requestId)];

    private static string Category(JsonElement line) => line.TryGetProperty("category", out JsonElement c) ? c.GetString() ?? string.Empty : string.Empty;

    private static string Event(JsonElement line) => line.TryGetProperty("event", out JsonElement e) ? e.GetString() ?? string.Empty : string.Empty;

    private static string Level(JsonElement line) => line.GetProperty("level").GetString() ?? string.Empty;

    private static List<JsonElement> ReadApplicationLines(string directory) =>
        [.. Directory.GetFiles(directory, "app-*.jsonl").SelectMany(ReadLines)];

    private static string ReadApplicationText(string directory) =>
        string.Concat(Directory.GetFiles(directory, "app-*.jsonl").Select(path => ReadAllShared(path)));

    private static string ReadAllShared(string path)
    {
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

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

#pragma warning disable CA1848, CA1873 // The tests need the plain extension calls (a dynamic level, a destructured value); a generated delegate would hide what is under test.
    private static void Emit(ILogger logger, LogLevel level, LogEventLevel library) =>
        logger.Log(level, "{Event}: {Level}", LogEvents.JobFinished, library.ToString());

    private static void Warn(ILogger logger, string @event, string text) =>
        logger.LogWarning("{Event}: {Text}", @event, text);

    private sealed class NoEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
        }
    }

    /// <summary>
    /// The test pipeline in front of the application's own: an exception handler that answers 500 (the framework's handler logs the failure as
    /// the real one will), and four paths — an error, a rejection (422), a refusal (401) and a success.
    /// </summary>
    private sealed class TestPipelineFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            ArgumentNullException.ThrowIfNull(next);
            return app =>
            {
                app.UseExceptionHandler(new ExceptionHandlerOptions { ExceptionHandler = HandleFailure });
                app.UseWhen(context => context.Request.Path.StartsWithSegments("/t225", StringComparison.Ordinal), branch => branch.Run(Answer));
                next(app);
            };
        }

        private static Task HandleFailure(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            ILogger logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(TestCategory);
            logger.LogError("{Event}: the request failed.", LogEvents.UnexpectedError);
            return Task.CompletedTask;
        }

        private static Task Answer(HttpContext context)
        {
            ILogger logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(TestCategory);
            switch (context.Request.Path.Value)
            {
                case "/t225/error":
                    throw new InvalidOperationException($"{ExceptionEmail} {ExceptionAddress}");
                case "/t225/invalid":
                    var fields = new[] { new { Field = "name", Code = "Required" }, new { Field = "quantity", Code = "OutOfRange" } };
                    logger.LogWarning("{Event} {@Fields}", LogEvents.ValidationRejected, fields);
                    context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                    break;
                case "/t225/denied":
                    logger.LogWarning("{Event}: the request was refused.", LogEvents.AccessDenied);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    break;
                default:
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    break;
            }

            return Task.CompletedTask;
        }
    }
#pragma warning restore CA1848, CA1873
}
