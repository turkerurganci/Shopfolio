using System.Net;
using System.Text.Json;

using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.SignedEndpoints;

namespace Shopfolio.UnitTests.SignedEndpoints;

/// <summary>
/// The thinned security line of a failed signature (T203 criterion 4, KK4; 08 3.2 principle 2; 05 4.5.2; 07 4.30.1; 09 18.2 item 3): the first
/// request of an address writes a line, the later ones of the window are only counted, the count is a field of the first line after the window.
/// The clock is moved; the window comes from <see cref="SignedEndpointLimits.FailureLogWindow"/>.
/// </summary>
public sealed class SignatureFailureLogTests : IDisposable
{
    private static readonly IPAddress First = IPAddress.Parse("192.0.2.10");
    private static readonly IPAddress Second = IPAddress.Parse("198.51.100.7");
    private static readonly TimeSpan Tick = TimeSpan.FromMilliseconds(1);

    private readonly SignedEndpointHarness _harness = new();

    public void Dispose() => _harness.Dispose();

    [Fact]
    public void FirstFailureOfAnAddress_WritesOneWarningLine_WithTheReasonAndACountOfZero()
    {
        Assert.True(_harness.Failures.Record(First, SignatureFailureReason.SignatureInvalid));

        JsonElement line = Line(0);
        Assert.Equal(LogEvents.SignatureFailureSampled, line.GetProperty("event").GetString());
        Assert.Equal("Warning", line.GetProperty("level").GetString());
        Assert.Equal(nameof(SignatureFailureReason.SignatureInvalid), line.GetProperty("reason").GetString());
        Assert.Equal(0, line.GetProperty("suppressedCount").GetInt32());
    }

    [Fact]
    public void LaterFailuresWithinTheWindow_WriteNoLine_AndTheFirstLineAfterTheWindowCarriesTheirCount()
    {
        _harness.Failures.Record(First, SignatureFailureReason.SignatureInvalid);
        for (int i = 0; i < 3; i++)
        {
            _harness.Time.Advance(Tick);
            Assert.False(_harness.Failures.Record(First, SignatureFailureReason.SignatureInvalid));
        }

        _harness.Time.SetUtcNow(SignedEndpointHarness.Now + SignedEndpointLimits.FailureLogWindow - Tick);
        Assert.False(_harness.Failures.Record(First, SignatureFailureReason.SignatureInvalid));
        Assert.Single(_harness.Lines);

        _harness.Time.SetUtcNow(SignedEndpointHarness.Now + SignedEndpointLimits.FailureLogWindow);
        Assert.True(_harness.Failures.Record(First, SignatureFailureReason.BodyUnreadable));

        Assert.Equal(2, _harness.Lines.Count);
        Assert.Equal(4, Line(1).GetProperty("suppressedCount").GetInt32());
        Assert.Equal(nameof(SignatureFailureReason.BodyUnreadable), Line(1).GetProperty("reason").GetString());
    }

    [Fact]
    public void TheWindowRestartsAtTheNewLine_SoItsCountStartsAgain()
    {
        _harness.Failures.Record(First, SignatureFailureReason.SignatureInvalid);
        _harness.Time.Advance(Tick);
        _harness.Failures.Record(First, SignatureFailureReason.SignatureInvalid);
        _harness.Time.Advance(SignedEndpointLimits.FailureLogWindow);
        _harness.Failures.Record(First, SignatureFailureReason.SignatureInvalid);

        _harness.Time.Advance(SignedEndpointLimits.FailureLogWindow - Tick);
        Assert.False(_harness.Failures.Record(First, SignatureFailureReason.SignatureInvalid));
        _harness.Time.Advance(Tick);
        Assert.True(_harness.Failures.Record(First, SignatureFailureReason.SignatureInvalid));

        Assert.Equal([0, 1, 1], Enumerable.Range(0, 3).Select(i => Line(i).GetProperty("suppressedCount").GetInt32()));
    }

    [Fact]
    public void AnotherAddress_HasItsOwnWindow()
    {
        Assert.True(_harness.Failures.Record(First, SignatureFailureReason.SignatureInvalid));
        Assert.True(_harness.Failures.Record(Second, SignatureFailureReason.SignatureInvalid));
        Assert.False(_harness.Failures.Record(First, SignatureFailureReason.SignatureInvalid));

        Assert.Equal(2, _harness.Lines.Count);
    }

    [Fact]
    public void AddressesOfOneIpv6Slash64_ShareAWindow_AndAMappedIpv4IsItsIpv4()
    {
        Assert.True(_harness.Failures.Record(IPAddress.Parse("2001:db8:1:2::1"), SignatureFailureReason.SignatureInvalid));
        Assert.False(_harness.Failures.Record(IPAddress.Parse("2001:db8:1:2:ffff::9"), SignatureFailureReason.SignatureInvalid));
        Assert.True(_harness.Failures.Record(IPAddress.Parse("2001:db8:1:3::1"), SignatureFailureReason.SignatureInvalid));

        Assert.True(_harness.Failures.Record(First, SignatureFailureReason.SignatureInvalid));
        Assert.False(_harness.Failures.Record(First.MapToIPv6(), SignatureFailureReason.SignatureInvalid));
        Assert.Equal(
            SignatureFailureLog.AddressKey(IPAddress.Parse("2001:db8:1:2::1")),
            SignatureFailureLog.AddressKey(IPAddress.Parse("2001:db8:1:2:abcd::")));
    }

    [Fact]
    public void UnknownAddress_IsOneAddress()
    {
        Assert.True(_harness.Failures.Record(null, SignatureFailureReason.SignatureInvalid));
        Assert.False(_harness.Failures.Record(null, SignatureFailureReason.SignatureInvalid));
    }

    [Fact]
    public void Line_CarriesNoAddress()
    {
        _harness.Failures.Record(First, SignatureFailureReason.SignatureInvalid);
        _harness.Failures.Record(IPAddress.Parse("2001:db8:1:2::1"), SignatureFailureReason.SignatureInvalid);

        Assert.All(_harness.Lines, line =>
        {
            Assert.DoesNotContain("192.0.2.10", line, StringComparison.Ordinal);
            Assert.DoesNotContain("2001:db8", line, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void AnEndedWindowWithACount_IsWrittenAtTheNextSweep_WithItsLastReason_AndDropped()
    {
        _harness.Failures.Record(First, SignatureFailureReason.SignatureInvalid);
        _harness.Failures.Record(First, SignatureFailureReason.BodyUnreadable);
        _harness.Time.Advance(SignedEndpointLimits.FailureLogWindow);

        _harness.Failures.Record(Second, SignatureFailureReason.SignatureInvalid);

        Assert.Equal(3, _harness.Lines.Count);
        Assert.Equal(1, Line(1).GetProperty("suppressedCount").GetInt32());
        Assert.Equal(nameof(SignatureFailureReason.BodyUnreadable), Line(1).GetProperty("reason").GetString());
        Assert.Equal(0, Line(2).GetProperty("suppressedCount").GetInt32());
        Assert.Equal(1, _harness.Failures.TrackedAddressCount);

        Assert.True(_harness.Failures.Record(First, SignatureFailureReason.SignatureInvalid));
        Assert.Equal(0, Line(3).GetProperty("suppressedCount").GetInt32());
    }

    [Fact]
    public void EndedWindows_AreDroppedAtTheSweep_SoTheMemoryHoldsOnlyTheLastWindowsAddresses()
    {
        for (int i = 1; i <= 50; i++)
        {
            IPAddress address = IPAddress.Parse("203.0.113." + i.ToString(System.Globalization.CultureInfo.InvariantCulture));
            _harness.Failures.Record(address, SignatureFailureReason.SignatureInvalid);
            _harness.Failures.Record(address, SignatureFailureReason.SignatureInvalid);
        }

        Assert.Equal(50, _harness.Failures.TrackedAddressCount);
        _harness.Time.Advance(SignedEndpointLimits.FailureLogWindow);

        _harness.Failures.Record(First, SignatureFailureReason.SignatureInvalid);

        Assert.Equal(1, _harness.Failures.TrackedAddressCount);
        Assert.Equal(50 + 50 + 1, _harness.Lines.Count);
    }

    [Fact]
    public void TheSweepingAddress_WritesOneLineWithItsOwnCount_NotASecondFlushedLine()
    {
        _harness.Failures.Record(First, SignatureFailureReason.SignatureInvalid);
        _harness.Failures.Record(First, SignatureFailureReason.SignatureInvalid);
        _harness.Time.Advance(SignedEndpointLimits.FailureLogWindow);

        Assert.True(_harness.Failures.Record(First, SignatureFailureReason.SignatureInvalid));

        Assert.Equal(2, _harness.Lines.Count);
        Assert.Equal(1, Line(1).GetProperty("suppressedCount").GetInt32());
    }

    private JsonElement Line(int index) => JsonDocument.Parse(_harness.Lines[index]).RootElement;
}
