using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;

using Microsoft.AspNetCore.Http;

using Npgsql;

using Serilog.Events;

using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Values;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Logging;

/// <summary>
/// The formatter layer of the masking (T231 criterion 7, KK5, KK7; 09 18.2 items 1, 7): the name and the type of every common field, the
/// exception fields of an unexpected error, and the request fields next to the framework's request scope. One line carries every field; it is
/// checked against <see cref="CommonLogFields.All"/>. Nothing is written to a file.
/// </summary>
public sealed class CommonLogFieldsTests
{
    private const string Address = "a@b.com";

    private const string AddressLine = "Rua das Flores 12, 34000 Istanbul";

    // The list of the formatter (kept equal to the guidelines' list by LoggingConsistencyTests).
    public static TheoryData<string> ForbiddenNames => [.. ApplicationLogFormatter.ForbiddenPropertyNames];

    private sealed class FixedAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }

    [Fact]
    public void Line_WithEveryCommonField_HasEachFieldUnderItsNameAndType()
    {
        using JsonDocument json = JsonDocument.Parse(EveryFieldLine(out _));

        Assert.Empty(CommonLogFields.Problems(json.RootElement));
    }

    [Fact]
    public void Problems_OnALineWithARenamedField_ReportsTheMissingName()
    {
        string line = EveryFieldLine(out _).Replace("\"requestId\":", "\"requestID\":", StringComparison.Ordinal);
        using JsonDocument json = JsonDocument.Parse(line);

        Assert.Equal(["requestId: missing"], CommonLogFields.Problems(json.RootElement));
    }

    [Fact]
    public void Problems_OnALineWithAFieldOfAnotherType_ReportsTheType()
    {
        JsonObject line = JsonNode.Parse(EveryFieldLine(out _))!.AsObject();
        line["innerExceptionTypes"] = "not an array";
        using JsonDocument json = JsonDocument.Parse(line.ToJsonString());

        Assert.Equal(["innerExceptionTypes: String, expected Array"], CommonLogFields.Problems(json.RootElement));
    }

    [Fact]
    public void Problems_OnALineWithAnElementOfFieldsThatLacksTheName_ReportsIt()
    {
        string line = EveryFieldLine(out _).Replace("\"field\":\"name\"", "\"name\":\"name\"", StringComparison.Ordinal);
        using JsonDocument json = JsonDocument.Parse(line);

        Assert.Equal(["field: missing"], CommonLogFields.Problems(json.RootElement));
    }

    [Fact]
    public void Line_FieldNames_AreCamelCaseAndTheLineHasNoFieldOfTheFrameworkOrTheLibrary()
    {
        using JsonDocument json = JsonDocument.Parse(EveryFieldLine(out _));

        string[] names = [.. json.RootElement.EnumerateObject().Select(p => p.Name)];

        Assert.All(names, name => Assert.True(char.IsLower(name[0]), name));
        Assert.Equal(names.Length, names.Distinct(StringComparer.Ordinal).Count());
        Assert.All(ApplicationLogFormatter.FrameworkScopeNames.Append("SourceContext"), name => Assert.DoesNotContain(ApplicationLogFormatter.FieldName(name), names.Where(n => n != "requestId")));
    }

    [Fact]
    public void Line_OfAnUnexpectedError_HasTheExceptionTypeTheInnerTypesAndTheSqlState()
    {
        PostgresException postgres = new(AddressLine, "ERROR", "ERROR", "23505");
        Exception exception = Thrown(new InvalidOperationException(Address, new ArgumentException(AddressLine, postgres)));

        string line = LogCapture.Line(LogCapture.One(logger => logger.Error(exception, "{Event}", LogEvents.UnexpectedError)));

        using JsonDocument json = JsonDocument.Parse(line);
        Assert.Equal(typeof(InvalidOperationException).FullName, json.RootElement.GetProperty("exceptionType").GetString());
        Assert.Equal(
            [typeof(ArgumentException).FullName, typeof(PostgresException).FullName],
            json.RootElement.GetProperty("innerExceptionTypes").EnumerateArray().Select(e => e.GetString()));
        Assert.Equal("23505", json.RootElement.GetProperty("sqlState").GetString());
        Assert.DoesNotContain(Address, line, StringComparison.Ordinal);
        Assert.DoesNotContain(AddressLine, line, StringComparison.Ordinal);
    }

    [Fact]
    public void Line_OfAnErrorWithoutAPostgresException_HasNoSqlState()
    {
        Exception exception = Thrown(new InvalidOperationException(Address, new ArgumentException(AddressLine)));

        using JsonDocument json = LogCapture.Json(LogCapture.One(logger => logger.Error(exception, "{Event}", LogEvents.UnexpectedError)));

        Assert.False(json.RootElement.TryGetProperty("sqlState", out _));
    }

    [Fact]
    public void Line_WithTheFrameworkRequestScopeAndTheEnricher_HasTheRequestIdAndNoneOfTheScopeNames()
    {
        const string scopeValue = "distinctive-scope-value";
        DefaultHttpContext context = new();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/siparis/erisim/ABC123/teyit";
        context.Request.Headers[RequestLogEnricher.RequestIdHeader] = "req-42";
        RequestLogEnricher enricher = new(new FixedAccessor { HttpContext = context });

        LogEvent logEvent = LogCapture.One(
            logger => logger
                .ForContext("RequestPath", scopeValue + "-path")
                .ForContext("RequestId", scopeValue + "-id")
                .ForContext("ConnectionId", scopeValue + "-connection")
                .Warning("text"),
            enricher);

        string line = LogCapture.Line(logEvent);

        using JsonDocument json = JsonDocument.Parse(line);
        Assert.Equal("req-42", json.RootElement.GetProperty("requestId").GetString());
        Assert.Equal("/siparis/erisim/{key}/teyit", json.RootElement.GetProperty("path").GetString());
        Assert.DoesNotContain(scopeValue, line, StringComparison.Ordinal);
        Assert.DoesNotContain("ABC123", line, StringComparison.Ordinal);
        Assert.All(ApplicationLogFormatter.FrameworkScopeNames, name => Assert.False(json.RootElement.TryGetProperty(ApplicationLogFormatter.FieldName(name), out JsonElement value) && value.GetString() != "req-42", name));
    }

    [Theory]
    [MemberData(nameof(ForbiddenNames))]
    public void Line_WithAForbiddenNamedPropertyNextToAnAllowedOne_WritesOnlyTheAllowedOne(string name)
    {
        LogEvent logEvent = LogCapture.One(logger => logger.ForContext(name, Address).ForContext("RecordId", "id-1").Warning("text"));

        string line = LogCapture.Line(logEvent);

        Assert.DoesNotContain(Address, line, StringComparison.Ordinal);
        using JsonDocument json = JsonDocument.Parse(line);
        Assert.Equal("id-1", json.RootElement.GetProperty("recordId").GetString());
    }

    // One line that carries every common field: a request with a signed-in account, a known event, a state and an operation, a rejection with a
    // code and a fields array, and an unexpected error with a database exception inside.
    private static string EveryFieldLine(out LogEvent logEvent)
    {
        DefaultHttpContext context = new();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/odeme/donus/KEY123";
        context.Request.Headers[RequestLogEnricher.RequestIdHeader] = "req-1";
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(RequestLogEnricher.AccountTypeClaim, nameof(AccountType.Customer)),
                new Claim(RequestLogEnricher.AccountIdClaim, Guid.NewGuid().ToString()),
            ],
            "test"));
        RequestLogEnricher enricher = new(new FixedAccessor { HttpContext = context });
        Exception exception = Thrown(new InvalidOperationException(Address, new PostgresException(AddressLine, "ERROR", "ERROR", "40001")));
        var fields = new[] { new { Field = "name", Code = "Required" } };

        logEvent = LogCapture.One(
            logger => logger
                .ForContext("SourceContext", "Shopfolio.Test.Category")
                .Error(exception, "{Event} {State} {Operation} {Code} {@Fields}", LogEvents.UnexpectedError, "Paid", "Capture", "Conflict", fields),
            enricher);
        return LogCapture.Line(logEvent);
    }

    private static Exception Thrown(Exception exception)
    {
        try
        {
            throw exception;
        }
#pragma warning disable CA1031 // Test helper: it only captures a thrown exception to get a real stack trace; nothing is swallowed (09 8.1 item 6).
        catch (Exception caught)
        {
            return caught;
        }
#pragma warning restore CA1031
    }
}
