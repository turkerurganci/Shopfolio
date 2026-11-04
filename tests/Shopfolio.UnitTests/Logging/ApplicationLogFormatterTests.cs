using System.Text.Json;
using System.Text.RegularExpressions;

using Npgsql;

using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

using Shopfolio.Infrastructure.Logging;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Logging;

/// <summary>
/// The line format of the application log (T230; 09 18.2 items 1, 2c, 3, 4, 5): one JSON object per line, camelCase field names, the closed
/// event list, no exception message, no unmasked request path. The name and type of every common field are tested in T231 (KK7); here the format
/// itself. Nothing is written to a file.
/// </summary>
public sealed partial class ApplicationLogFormatterTests
{
    // An exception message that is a fingerprint: it must never come back in a line.
    private const string MessageMarker = "distinctive-exception-text@example.invalid";

    private const string ValueMarker = "distinctive-property-value";

    public static TheoryData<string> KnownEvents => [.. LogEvents.All];

    public static TheoryData<string> ForbiddenNames => [.. ApplicationLogFormatter.ForbiddenPropertyNames, .. ApplicationLogFormatter.ForbiddenPropertyNames.Select(n => "Customer" + n)];

    public static TheoryData<string> FrameworkScopeNames => [.. ApplicationLogFormatter.FrameworkScopeNames];

    public static TheoryData<string> OwnedNames => [.. ApplicationLogFormatter.OwnedFieldNames.Select(n => char.ToUpperInvariant(n[0]) + n[1..])];

    [Fact]
    public void Format_AnyEvent_IsOneJsonObjectOnOneLine()
    {
        LogEvent logEvent = LogCapture.One(logger => logger.Warning("{Event} text with a {Count}", LogEvents.JobStarted, 3));

        string line = LogCapture.Line(logEvent);

        Assert.EndsWith("\n", line, StringComparison.Ordinal);
        Assert.Equal(line.Length - 1, line.IndexOf('\n', StringComparison.Ordinal));
        Assert.Equal(JsonValueKind.Object, JsonDocument.Parse(line).RootElement.ValueKind);
    }

    [Fact]
    public void Format_Time_IsUtcWithMillisecondsAndAZ()
    {
        LogEvent logEvent = LogCapture.One(logger => logger.Information("text"));

        using JsonDocument json = LogCapture.Json(logEvent);

        string time = json.RootElement.GetProperty("time").GetString()!;
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$", time);
        Assert.Equal(logEvent.Timestamp.UtcDateTime, DateTime.Parse(time, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal), TimeSpan.FromMilliseconds(1));
    }

    [Theory]
    [InlineData(LogEventLevel.Information)]
    [InlineData(LogEventLevel.Warning)]
    [InlineData(LogEventLevel.Error)]
    public void Format_Level_IsTheLevelName(LogEventLevel level)
    {
        LogEvent logEvent = LogCapture.One(logger => logger.Write(level, "text"));

        using JsonDocument json = LogCapture.Json(logEvent);

        Assert.Equal(level.ToString(), json.RootElement.GetProperty("level").GetString());
    }

    [Fact]
    public void Format_Category_IsTheSourceContext()
    {
        LogEvent logEvent = LogCapture.One(logger => logger.ForContext("SourceContext", "Shopfolio.Some.Category").Warning("text"));

        using JsonDocument json = LogCapture.Json(logEvent);

        Assert.Equal("Shopfolio.Some.Category", json.RootElement.GetProperty("category").GetString());
        Assert.False(json.RootElement.TryGetProperty("sourceContext", out _));
    }

    [Theory]
    [MemberData(nameof(KnownEvents))]
    public void Format_EventOnTheList_IsWritten(string name)
    {
        LogEvent logEvent = LogCapture.One(logger => logger.Warning("{Event}: text", name));

        using JsonDocument json = LogCapture.Json(logEvent);

        Assert.Equal(name, json.RootElement.GetProperty("event").GetString());
    }

    [Fact]
    public void Format_EventNotOnTheList_IsNotWrittenEitherAsFieldOrInTheMessage()
    {
        const string freeText = "a free text that is not an event name";
        LogEvent logEvent = LogCapture.One(logger => logger.Warning("{Event}: text", freeText));

        string line = LogCapture.Line(logEvent);

        using JsonDocument json = JsonDocument.Parse(line);
        Assert.False(json.RootElement.TryGetProperty("event", out _));
        Assert.DoesNotContain(freeText, line, StringComparison.Ordinal);
    }

    [Fact]
    public void Format_Properties_BecomeCamelCaseFieldsWithTheirTypes()
    {
        Guid recordId = Guid.NewGuid();
        LogEvent logEvent = LogCapture.One(logger => logger.Warning("{Event} {RecordId} {MissingKeys} {Count} {Flag}", LogEvents.InstantDeletion, recordId, "A, B", 7, true));

        using JsonDocument json = LogCapture.Json(logEvent);

        Assert.Equal(recordId.ToString(), json.RootElement.GetProperty("recordId").GetString());
        Assert.Equal("A, B", json.RootElement.GetProperty("missingKeys").GetString());
        Assert.Equal(7, json.RootElement.GetProperty("count").GetInt32());
        Assert.True(json.RootElement.GetProperty("flag").GetBoolean());
        Assert.All(json.RootElement.EnumerateObject().Select(p => p.Name), name => Assert.Matches(CamelCase(), name));
    }

    [Theory]
    [MemberData(nameof(ForbiddenNames))]
    public void Format_PropertyWithAPersonalDataName_IsNotWrittenNorRenderedInTheMessage(string name)
    {
        LogEvent logEvent = new(
            DateTimeOffset.UnixEpoch,
            LogEventLevel.Warning,
            null,
            new Serilog.Parsing.MessageTemplateParser().Parse("shown {" + name + "}"),
            [new LogEventProperty(name, new ScalarValue(ValueMarker))]);

        string line = LogCapture.Line(logEvent);

        Assert.DoesNotContain(ValueMarker, line, StringComparison.Ordinal);
        Assert.False(JsonDocument.Parse(line).RootElement.TryGetProperty(ApplicationLogFormatter.FieldName(name), out _));
    }

    [Theory]
    [MemberData(nameof(FrameworkScopeNames))]
    public void Format_FrameworkRequestScopeProperty_IsNotWritten(string name)
    {
        // The framework opens a scope with these names around a request; the library's provider turns scope values into properties of the line.
        CollectingSink sink = new();
        using Logger root = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Sink(sink).CreateLogger();
        using SerilogLoggerFactory factory = new(root);
        Microsoft.Extensions.Logging.ILogger logger = factory.CreateLogger("Shopfolio.Test");

        using (logger.BeginScope(new Dictionary<string, object> { [name] = ValueMarker }))
        {
            Microsoft.Extensions.Logging.LoggerExtensions.LogWarning(logger, "{Text}", "inside the scope");
        }

        LogEvent logEvent = Assert.Single(sink.Events);
        Assert.Contains(name, logEvent.Properties.Keys);
        Assert.DoesNotContain(ValueMarker, LogCapture.Line(logEvent), StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(OwnedNames))]
    public void Format_PropertyWithAFormatterOwnedName_DoesNotReplaceTheOwnedField(string name)
    {
        LogEvent logEvent = LogCapture.One(logger => logger.ForContext(name, ValueMarker).Warning("text"));

        string line = LogCapture.Line(logEvent);

        Assert.DoesNotContain(ValueMarker, line, StringComparison.Ordinal);
    }

    [Fact]
    public void Format_Exception_WritesTypesAndNeverAnyMessage()
    {
        Exception exception = Thrown(new InvalidOperationException(MessageMarker, new ArgumentException(MessageMarker + "-inner", new TimeoutException(MessageMarker + "-innermost"))));
        LogEvent logEvent = LogCapture.One(logger => logger.Error(exception, "{Event}", LogEvents.UnexpectedError));

        string line = LogCapture.Line(logEvent);

        Assert.DoesNotContain(MessageMarker, line, StringComparison.Ordinal);
        using JsonDocument json = JsonDocument.Parse(line);
        Assert.Equal(exception.GetType().FullName, json.RootElement.GetProperty("exceptionType").GetString());
        Assert.Equal(
            [exception.InnerException!.GetType().FullName, exception.InnerException.InnerException!.GetType().FullName],
            json.RootElement.GetProperty("innerExceptionTypes").EnumerateArray().Select(e => e.GetString()));
        Assert.False(json.RootElement.TryGetProperty("sqlState", out _));
    }

    [Fact]
    public void Format_ExceptionData_WritesTheKeysAndNotTheValues()
    {
        InvalidOperationException exception = new("text");
        exception.Data["order-lookup"] = ValueMarker;
        LogEvent logEvent = LogCapture.One(logger => logger.Error(exception, "{Event}", LogEvents.UnexpectedError));

        string line = LogCapture.Line(logEvent);

        Assert.DoesNotContain(ValueMarker, line, StringComparison.Ordinal);
        using JsonDocument json = JsonDocument.Parse(line);
        Assert.Equal(["order-lookup"], json.RootElement.GetProperty("dataKeys").EnumerateArray().Select(e => e.GetString()));
    }

    [Fact]
    public void Format_StackTrace_IsTheStackTraceAndNotTheTextFormOfTheException()
    {
        Exception exception = Thrown(new InvalidOperationException(MessageMarker));
        LogEvent logEvent = LogCapture.One(logger => logger.Error(exception, "{Event}", LogEvents.UnexpectedError));

        using JsonDocument json = LogCapture.Json(logEvent);

        string stackTrace = json.RootElement.GetProperty("stackTrace").GetString()!;
        Assert.Equal(exception.StackTrace, stackTrace);
        Assert.DoesNotContain(MessageMarker, stackTrace, StringComparison.Ordinal);
        Assert.DoesNotContain(exception.GetType().FullName!, stackTrace.Split('\n')[0], StringComparison.Ordinal);
    }

    [Fact]
    public void Format_PostgresException_AddsTheSqlStateEvenWhenItIsAnInnerException()
    {
        PostgresException postgres = new(MessageMarker, "ERROR", "ERROR", "23505");
        InvalidOperationException wrapper = new("wrapper", postgres);
        LogEvent logEvent = LogCapture.One(logger => logger.Error(wrapper, "{Event}", LogEvents.UnexpectedError));

        string line = LogCapture.Line(logEvent);

        Assert.DoesNotContain(MessageMarker, line, StringComparison.Ordinal);
        using JsonDocument json = JsonDocument.Parse(line);
        Assert.Equal(postgres.SqlState, json.RootElement.GetProperty("sqlState").GetString());
    }

    [Fact]
    public void Format_FieldValidationRejection_IsOneLineWithAFieldsArray()
    {
        // The shape of the rejection line (09 18.2 item 4); the call that writes it and the code list are T213's.
        var fields = new[] { new { Field = "name", Code = "Required" }, new { Field = "quantity", Code = "OutOfRange" } };
        LogEvent logEvent = LogCapture.One(logger => logger.Warning("{Event} {@Fields}", LogEvents.ValidationRejected, fields));

        string line = LogCapture.Line(logEvent);

        Assert.Equal(1, line.Count(c => c == '\n'));
        using JsonDocument json = JsonDocument.Parse(line);
        JsonElement array = json.RootElement.GetProperty("fields");
        Assert.Equal(fields.Length, array.GetArrayLength());
        for (int i = 0; i < fields.Length; i++)
        {
            Assert.Equal(fields[i].Field, array[i].GetProperty("field").GetString());
            Assert.Equal(fields[i].Code, array[i].GetProperty("code").GetString());
        }

        Assert.False(json.RootElement.TryGetProperty("code", out _));
    }

    [Fact]
    public void Format_OtherRejection_HasASingleCode()
    {
        LogEvent logEvent = LogCapture.One(logger => logger.Warning("{Event} {Code}", LogEvents.ValidationRejected, "NotFound"));

        using JsonDocument json = LogCapture.Json(logEvent);

        Assert.Equal("NotFound", json.RootElement.GetProperty("code").GetString());
        Assert.False(json.RootElement.TryGetProperty("fields", out _));
    }

    [GeneratedRegex("^[a-z][A-Za-z0-9]*$", RegexOptions.CultureInvariant)]
    private static partial Regex CamelCase();

    // An exception has a stack trace only once it has been thrown.
    private static Exception Thrown(Exception exception)
    {
        try
        {
            throw exception;
        }
        catch (Exception caught)
        {
            return caught;
        }
    }
}
