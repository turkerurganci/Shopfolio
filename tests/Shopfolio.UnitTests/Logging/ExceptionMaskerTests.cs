using System.Text.Json;

using Npgsql;

using Shopfolio.Infrastructure.Logging;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Logging;

/// <summary>
/// The exception layer of the masking (T231 criteria 5 and 6; 09 18.2 items 2c, 7): what <see cref="ExceptionMasker"/> hands the formatter and
/// what ends up in the line. The message of an exception, of the inner ones too, is a fingerprint here and never comes back; the data dictionary
/// gives its keys; the stack trace is the stack trace and not <c>ToString()</c>.
/// </summary>
public sealed class ExceptionMaskerTests
{
    private const string Address = "a@b.com";

    private const string AddressLine = "Rua das Flores 12, 34000 Istanbul";

    private const string DataValue = "distinctive-data-value";

    [Fact]
    public void Describe_ReturnsTheTypesOfTheOuterAndEveryInnerExceptionInOrder()
    {
        Exception exception = Thrown(new InvalidOperationException(Address, new ArgumentException(AddressLine, new TimeoutException(Address))));

        ExceptionFields fields = ExceptionMasker.Describe(exception);

        Assert.Equal(typeof(InvalidOperationException).FullName, fields.ExceptionType);
        Assert.Equal([typeof(ArgumentException).FullName!, typeof(TimeoutException).FullName!], fields.InnerExceptionTypes);
        Assert.Null(fields.SqlState);
        Assert.Empty(fields.DataKeys);
    }

    [Fact]
    public void Describe_AnExceptionWithoutInnerExceptions_HasNoInnerType()
    {
        ExceptionFields fields = ExceptionMasker.Describe(new InvalidOperationException(Address));

        Assert.Empty(fields.InnerExceptionTypes);
    }

    [Fact]
    public void Describe_DataDictionary_GivesTheKeysOfEveryExceptionOfTheChainAndNeverAValue()
    {
        InvalidOperationException inner = new("inner");
        inner.Data["inner-key"] = DataValue + "-inner";
        InvalidOperationException outer = new("outer", inner);
        outer.Data["outer-key"] = DataValue;
        outer.Data["order-id"] = Address;

        ExceptionFields fields = ExceptionMasker.Describe(outer);

        Assert.Equal(["outer-key", "order-id", "inner-key"], fields.DataKeys);
        Assert.DoesNotContain(fields.DataKeys, key => key.Contains(DataValue, StringComparison.Ordinal) || key.Contains(Address, StringComparison.Ordinal));
    }

    [Fact]
    public void Describe_ADataKeyThatIsNotAString_IsNamedByItsTypeSoItsTextFormCannotCarryAValue()
    {
        InvalidOperationException exception = new("text");
        exception.Data[new CustomerKey(Address)] = DataValue;

        ExceptionFields fields = ExceptionMasker.Describe(exception);

        Assert.Equal([nameof(CustomerKey)], fields.DataKeys);
    }

    [Fact]
    public void Describe_StackTrace_IsTheStackTraceOfTheOuterExceptionAndNotItsToString()
    {
        Exception exception = Thrown(new InvalidOperationException(Address, new ArgumentException(AddressLine)));

        ExceptionFields fields = ExceptionMasker.Describe(exception);

        Assert.Equal(exception.StackTrace, fields.StackTrace);
        Assert.NotEqual(exception.ToString(), fields.StackTrace);
        Assert.DoesNotContain(Address, fields.StackTrace!, StringComparison.Ordinal);
        Assert.DoesNotContain(AddressLine, fields.StackTrace!, StringComparison.Ordinal);
    }

    [Fact]
    public void Describe_AnExceptionThatWasNeverThrown_HasNoStackTrace()
    {
        Assert.Null(ExceptionMasker.Describe(new InvalidOperationException(Address)).StackTrace);
    }

    [Fact]
    public void Describe_PostgresExceptionAtAnyDepth_GivesTheSqlStateAndOnlyThat()
    {
        PostgresException postgres = new(AddressLine, "ERROR", "ERROR", "23505");
        Exception wrapped = new InvalidOperationException("wrapper", new ArgumentException("middle", postgres));

        ExceptionFields outerIsPostgres = ExceptionMasker.Describe(postgres);
        ExceptionFields innerIsPostgres = ExceptionMasker.Describe(wrapped);

        Assert.Equal("23505", outerIsPostgres.SqlState);
        Assert.Equal("23505", innerIsPostgres.SqlState);
        Assert.Contains(typeof(PostgresException).FullName!, innerIsPostgres.InnerExceptionTypes);
    }

    [Fact]
    public void FormattedLine_OfTheWholeChain_HoldsNoMessageNoDataValueAndTheKeys()
    {
        InvalidOperationException inner = new(AddressLine);
        inner.Data["inner-key"] = DataValue;
        Exception exception = Thrown(new InvalidOperationException(Address, inner));
        exception.Data["order-lookup"] = DataValue;

        string line = LogCapture.Line(LogCapture.One(logger => logger.Error(exception, "{Event}", LogEvents.UnexpectedError)));

        Assert.DoesNotContain(Address, line, StringComparison.Ordinal);
        Assert.DoesNotContain(AddressLine, line, StringComparison.Ordinal);
        Assert.DoesNotContain(DataValue, line, StringComparison.Ordinal);
        using JsonDocument json = JsonDocument.Parse(line);
        Assert.Equal(["order-lookup", "inner-key"], json.RootElement.GetProperty("dataKeys").EnumerateArray().Select(e => e.GetString()));
        Assert.Equal(exception.StackTrace, json.RootElement.GetProperty("stackTrace").GetString());
    }

    [Fact]
    public void FormattedLine_OfAnExceptionWithAnOverriddenMessageAndToString_WritesNeitherOfThem()
    {
        Exception exception = Thrown(new LeakyException());

        string line = LogCapture.Line(LogCapture.One(logger => logger.Error(exception, "{Event}", LogEvents.UnexpectedError)));

        Assert.DoesNotContain(Address, line, StringComparison.Ordinal);
        Assert.DoesNotContain(AddressLine, line, StringComparison.Ordinal);
        using JsonDocument json = JsonDocument.Parse(line);
        Assert.Equal(typeof(LeakyException).FullName, json.RootElement.GetProperty("exceptionType").GetString());
    }

    [Fact]
    public void FormattedLine_WhenTheCallPutsTheMessageIntoNoProperty_HasTheMessageNowhere()
    {
        // The exception is the only place a message could come from: the template has the event only.
        Exception exception = Thrown(new InvalidOperationException(Address));

        string line = LogCapture.Line(LogCapture.One(logger => logger.Error(exception, "{Event} failed", LogEvents.UnexpectedError)));

        Assert.DoesNotContain(Address, line, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(InvalidOperationException) + ": ", line, StringComparison.Ordinal);
    }

    private sealed record CustomerKey(string Email)
    {
        public override string ToString() => Email;
    }

    private sealed class LeakyException : Exception
    {
        public override string Message => Address + " " + AddressLine;

        public override string ToString() => Message;
    }

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
