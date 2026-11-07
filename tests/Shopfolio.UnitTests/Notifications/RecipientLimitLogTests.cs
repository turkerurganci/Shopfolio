using System.Text.Json;

using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;

using Shopfolio.Infrastructure.Logging;
using Shopfolio.Modules.Notifications;
using Shopfolio.UnitTests.Fakes;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Notifications;

/// <summary>
/// The thinned security line of a refused customer notification (T304 criterion 9, KK9; 08 2.2.4 "Kayıt"; 09 18.2 item 3): the first refusal
/// writes a line, the later ones of the window are only counted, the count is a field of the first line after the window, and no line names an
/// address. The clock is moved; the window comes from <see cref="RecipientSendLimit.LogWindow"/>. The log events are kept in memory.
/// </summary>
public sealed class RecipientLimitLogTests : IDisposable
{
    private static readonly DateTimeOffset Start = new(2026, 11, 6, 9, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan Tick = TimeSpan.FromMilliseconds(1);

    private readonly Logger _root;
    private readonly SerilogLoggerFactory _factory;
    private readonly CollectingSink _sink = new();
    private readonly ManualTimeProvider _time = new(Start);
    private readonly RecipientLimitLog _log;

    public RecipientLimitLogTests()
    {
        _root = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Sink(_sink).CreateLogger();
        _factory = new SerilogLoggerFactory(_root);
        _log = new RecipientLimitLog(_time, new Microsoft.Extensions.Logging.Logger<RecipientLimitLog>(_factory));
    }

    public void Dispose()
    {
        _factory.Dispose();
        _root.Dispose();
    }

    private IReadOnlyList<string> Lines => [.. _sink.Events.Select(LogCapture.Line)];

    private JsonElement Line(int index) => JsonDocument.Parse(Lines[index]).RootElement.Clone();

    [Fact]
    public void FirstRefusal_WritesOneWarningLine_WithACountOfZero_AndNoAddress()
    {
        Assert.True(_log.Record());

        JsonElement line = Line(0);
        Assert.Equal(LogEvents.RecipientLimitExceededSampled, line.GetProperty("event").GetString());
        Assert.Equal("Warning", line.GetProperty("level").GetString());
        Assert.Equal(0, line.GetProperty("suppressedCount").GetInt32());
        Assert.DoesNotContain("@", Lines[0], StringComparison.Ordinal);
    }

    [Fact]
    public void LaterRefusalsWithinTheWindow_WriteNoLine_AndTheFirstLineAfterTheWindowCarriesTheirCount()
    {
        _log.Record();
        for (int i = 0; i < 3; i++)
        {
            _time.Advance(Tick);
            Assert.False(_log.Record());
        }

        _time.SetUtcNow(Start + RecipientSendLimit.LogWindow - Tick);
        Assert.False(_log.Record());
        Assert.Single(Lines);

        _time.SetUtcNow(Start + RecipientSendLimit.LogWindow);
        Assert.True(_log.Record());

        Assert.Equal(2, Lines.Count);
        Assert.Equal(4, Line(1).GetProperty("suppressedCount").GetInt32());
    }

    [Fact]
    public void TheWindowRestartsAtTheNewLine_SoItsCountStartsAgain()
    {
        _log.Record();
        _time.Advance(Tick);
        _log.Record();
        _time.Advance(RecipientSendLimit.LogWindow);
        _log.Record();

        _time.Advance(RecipientSendLimit.LogWindow - Tick);
        Assert.False(_log.Record());
        _time.Advance(Tick);
        Assert.True(_log.Record());

        Assert.Equal([0, 1, 1], Enumerable.Range(0, 3).Select(i => Line(i).GetProperty("suppressedCount").GetInt32()));
    }

    [Fact]
    public void ARefusalAfterAQuietWindow_WritesALineWithACountOfZero()
    {
        _log.Record();
        _time.Advance(RecipientSendLimit.LogWindow * 3);

        Assert.True(_log.Record());
        Assert.Equal(0, Line(1).GetProperty("suppressedCount").GetInt32());
    }
}
