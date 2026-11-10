using System.Net;
using System.Text.Json;

using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Security;
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
            ClientAddress.Axis(IPAddress.Parse("2001:db8:1:2::1")),
            ClientAddress.Axis(IPAddress.Parse("2001:db8:1:2:abcd::")));
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
    public void AnEndedWindowWithACount_WritesNoLineAtAnotherAddressesRequest_AndItsAddressesNextLineCarriesTheCount()
    {
        // A fails twice; B fails after A's window: B's line carries none of A's count, and no line of A's is written at B's request.
        Assert.True(_harness.Failures.Record(First, SignatureFailureReason.SignatureInvalid));
        Assert.False(_harness.Failures.Record(First, SignatureFailureReason.SignatureInvalid));
        _harness.Time.Advance(SignedEndpointLimits.FailureLogWindow);

        Assert.True(_harness.Failures.Record(Second, SignatureFailureReason.SignatureInvalid));

        Assert.Equal(2, _harness.Lines.Count);
        Assert.Equal(0, Line(1).GetProperty("suppressedCount").GetInt32());

        // A comes back: its first line after the window carries its count.
        _harness.Time.Advance(Tick);
        Assert.True(_harness.Failures.Record(First, SignatureFailureReason.BodyUnreadable));

        Assert.Equal(3, _harness.Lines.Count);
        Assert.Equal(1, Line(2).GetProperty("suppressedCount").GetInt32());
        Assert.Equal(nameof(SignatureFailureReason.BodyUnreadable), Line(2).GetProperty("reason").GetString());
    }

    [Fact]
    public void ManyAddresses_EachCountIsWrittenOnlyOnItsOwnAddressesLine()
    {
        IPAddress third = IPAddress.Parse("2001:db8:7:8::1");
        IPAddress[] addresses = [First, Second, third];
        for (int i = 0; i < addresses.Length; i++)
        {
            Assert.True(_harness.Failures.Record(addresses[i], SignatureFailureReason.SignatureInvalid));
            for (int j = 0; j <= i; j++)
            {
                Assert.False(_harness.Failures.Record(addresses[i], SignatureFailureReason.SignatureInvalid));
            }
        }

        _harness.Time.Advance(SignedEndpointLimits.FailureLogWindow * 3);
        Assert.Equal(addresses.Length, _harness.Lines.Count);

        // Back in the reverse order, after several windows: each first line carries its own count (1, 2, 3), whichever comes first.
        for (int i = addresses.Length - 1; i >= 0; i--)
        {
            Assert.True(_harness.Failures.Record(addresses[i], SignatureFailureReason.SignatureInvalid));
            Assert.Equal(i + 1, Line(_harness.Lines.Count - 1).GetProperty("suppressedCount").GetInt32());
            _harness.Time.Advance(SignedEndpointLimits.FailureLogWindow);
        }

        Assert.Equal(addresses.Length * 2, _harness.Lines.Count);
    }

    [Fact]
    public void EndedWindowsWithoutACount_AreDroppedAtTheSweep_AndThoseWithACountStayUntilTheirAddressComesBack()
    {
        for (int i = 1; i <= 50; i++)
        {
            IPAddress address = IPAddress.Parse("203.0.113." + i.ToString(System.Globalization.CultureInfo.InvariantCulture));
            _harness.Failures.Record(address, SignatureFailureReason.SignatureInvalid);
            if (i <= 10)
            {
                _harness.Failures.Record(address, SignatureFailureReason.SignatureInvalid);
            }
        }

        Assert.Equal(50, _harness.Failures.TrackedAddressCount);
        _harness.Time.Advance(SignedEndpointLimits.FailureLogWindow);

        _harness.Failures.Record(First, SignatureFailureReason.SignatureInvalid);

        Assert.Equal(10 + 1, _harness.Failures.TrackedAddressCount);
        Assert.Equal(50 + 1, _harness.Lines.Count);

        _harness.Failures.Record(IPAddress.Parse("203.0.113.1"), SignatureFailureReason.SignatureInvalid);
        Assert.Equal(1, Line(_harness.Lines.Count - 1).GetProperty("suppressedCount").GetInt32());
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

    // ---- The ceiling of the windows in memory (T402; D-108; K-1582; 05 4.5.2) ----

    [Fact]
    public void AtTheCeiling_ANewAddress_WritesTheOldestCountedWindowsCountAtOnce_AndDropsIt_TheCountIsNotLost()
    {
        IPAddress oldest = Address(0);
        _harness.Failures.Record(oldest, SignatureFailureReason.SignatureInvalid);
        _harness.Failures.Record(oldest, SignatureFailureReason.BodyUnreadable);
        _harness.Failures.Record(oldest, SignatureFailureReason.BodyUnreadable);
        _harness.Time.Advance(Tick);
        IPAddress laterCounted = Address(1);
        _harness.Failures.Record(laterCounted, SignatureFailureReason.SignatureInvalid);
        _harness.Failures.Record(laterCounted, SignatureFailureReason.SignatureInvalid);
        for (int i = 2; i < SignedEndpointLimits.FailureLogMaxAddresses; i++)
        {
            _harness.Failures.Record(Address(i), SignatureFailureReason.SignatureInvalid);
        }

        Assert.Equal(SignedEndpointLimits.FailureLogMaxAddresses, _harness.Failures.TrackedAddressCount);
        int before = _harness.Lines.Count;

        Assert.True(_harness.Failures.Record(Address(SignedEndpointLimits.FailureLogMaxAddresses), SignatureFailureReason.SignatureInvalid));

        // Two lines: the flushed count of the oldest counted window (with its last counted reason), then the new address's own line.
        Assert.Equal(before + 2, _harness.Lines.Count);
        Assert.Equal(2, Line(before).GetProperty("suppressedCount").GetInt32());
        Assert.Equal(nameof(SignatureFailureReason.BodyUnreadable), Line(before).GetProperty("reason").GetString());
        Assert.Equal(0, Line(before + 1).GetProperty("suppressedCount").GetInt32());
        Assert.Equal(SignedEndpointLimits.FailureLogMaxAddresses, _harness.Failures.TrackedAddressCount);

        // The flushed address has no window any more: its next failure is a new address at the full ceiling - it flushes the next oldest counted window
        // (the later one, count 1) and writes its own line with no count (its count was already written).
        _harness.Time.Advance(Tick);
        Assert.True(_harness.Failures.Record(oldest, SignatureFailureReason.SignatureInvalid));
        Assert.Equal(1, Line(_harness.Lines.Count - 2).GetProperty("suppressedCount").GetInt32());
        Assert.Equal(0, Line(_harness.Lines.Count - 1).GetProperty("suppressedCount").GetInt32());
    }

    [Fact]
    public void AtTheCeiling_WithNoCountedWindow_TheOldestWindowIsDropped_WithoutALine()
    {
        for (int i = 0; i < SignedEndpointLimits.FailureLogMaxAddresses; i++)
        {
            _harness.Failures.Record(Address(i), SignatureFailureReason.SignatureInvalid);
        }

        Assert.True(_harness.Failures.Record(Address(SignedEndpointLimits.FailureLogMaxAddresses), SignatureFailureReason.SignatureInvalid));

        Assert.Equal(SignedEndpointLimits.FailureLogMaxAddresses + 1, _harness.Lines.Count);
        Assert.Equal(SignedEndpointLimits.FailureLogMaxAddresses, _harness.Failures.TrackedAddressCount);
        Assert.True(_harness.Failures.Record(Address(0), SignatureFailureReason.SignatureInvalid));
        Assert.False(_harness.Failures.Record(Address(1), SignatureFailureReason.SignatureInvalid));
    }

    [Fact]
    public void AtTheCeiling_EndedWindowsWithoutACount_GoFirst_AndNoCountIsFlushed()
    {
        // Opened at +30 s, before the sweep at +61 s and still open at it; ended at +95 s, where the sweep does not run yet (34 s after the last one).
        _harness.Time.Advance(TimeSpan.FromSeconds(30));
        for (int i = 1; i < SignedEndpointLimits.FailureLogMaxAddresses; i++)
        {
            _harness.Failures.Record(Address(i), SignatureFailureReason.SignatureInvalid);
        }

        _harness.Time.SetUtcNow(SignedEndpointHarness.Now + TimeSpan.FromSeconds(61));
        _harness.Failures.Record(Address(0), SignatureFailureReason.SignatureInvalid);
        _harness.Failures.Record(Address(0), SignatureFailureReason.SignatureInvalid);
        Assert.Equal(SignedEndpointLimits.FailureLogMaxAddresses, _harness.Failures.TrackedAddressCount);
        int before = _harness.Lines.Count;

        _harness.Time.SetUtcNow(SignedEndpointHarness.Now + TimeSpan.FromSeconds(95));
        Assert.True(_harness.Failures.Record(Address(SignedEndpointLimits.FailureLogMaxAddresses), SignatureFailureReason.SignatureInvalid));

        Assert.Equal(before + 1, _harness.Lines.Count);
        Assert.Equal(2, _harness.Failures.TrackedAddressCount);
    }

    // A distinct IPv4 address per index: 10.x.y.z.
    private static IPAddress Address(int index) => new([10, (byte)(index >> 16), (byte)(index >> 8), (byte)index]);

    private JsonElement Line(int index) => JsonDocument.Parse(_harness.Lines[index]).RootElement;
}
