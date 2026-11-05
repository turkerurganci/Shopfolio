using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Notifications;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The state of a link read from its record (T222 KK4; 05 6.4.6; 06 5.4.3) and the event values that name a link record (06 4.58). "No record"
/// is not a state, so the five outcomes are the missing reading and the four values of <see cref="LinkState"/>.
/// </summary>
public sealed class LinkRecordStateTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    private static readonly string Placeholder = "digest-placeholder";

    private static LinkRecord Record(DateTimeOffset expiresAt, DateTimeOffset? usedAt = null, DateTimeOffset? supersededAt = null) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            CreatedAt = Now.AddHours(-1),
            LinkType = LinkType.PasswordReset,
            CustomerId = Guid.CreateVersion7(),
            TokenHash = Placeholder,
            ExpiresAt = expiresAt,
            UsedAt = usedAt,
            SupersededAt = supersededAt,
        };

    [Fact]
    public void StateOf_EachOfTheFourStates_GivesItsOwnValue()
    {
        Assert.Equal(LinkState.Valid, LinkRecords.StateOf(Record(Now.AddMinutes(1)), Now));
        Assert.Equal(LinkState.Expired, LinkRecords.StateOf(Record(Now.AddMinutes(-1)), Now));
        Assert.Equal(LinkState.Used, LinkRecords.StateOf(Record(Now.AddMinutes(1), usedAt: Now.AddMinutes(-5)), Now));
        Assert.Equal(LinkState.Superseded, LinkRecords.StateOf(Record(Now.AddMinutes(1), supersededAt: Now.AddMinutes(-5)), Now));
    }

    [Fact]
    public void StateOf_TheFiveOutcomesWithTheMissingReading_AreAllDifferent()
    {
        LinkState?[] outcomes =
        [
            LinkReading.Missing.State,
            LinkRecords.StateOf(Record(Now.AddMinutes(1)), Now),
            LinkRecords.StateOf(Record(Now.AddMinutes(-1)), Now),
            LinkRecords.StateOf(Record(Now.AddMinutes(1), usedAt: Now), Now),
            LinkRecords.StateOf(Record(Now.AddMinutes(1), supersededAt: Now), Now),
        ];

        Assert.Equal(Enum.GetValues<LinkState>().Length + 1, outcomes.Distinct().Count());
        Assert.False(LinkReading.Missing.Exists);
    }

    [Fact]
    public void StateOf_TheLifetimeEndsAtItsMoment()
    {
        Assert.Equal(LinkState.Valid, LinkRecords.StateOf(Record(Now.AddTicks(1)), Now));
        Assert.Equal(LinkState.Expired, LinkRecords.StateOf(Record(Now), Now));
    }

    [Fact]
    public void StateOf_UsedWinsOverSupersededAndExpired_AndSupersededOverExpired()
    {
        DateTimeOffset past = Now.AddDays(-1);

        Assert.Equal(LinkState.Used, LinkRecords.StateOf(Record(past, usedAt: past), Now));
        Assert.Equal(LinkState.Superseded, LinkRecords.StateOf(Record(past, supersededAt: past), Now));
    }

    [Fact]
    public void EventValues_RoundTripTheLinkRecordId()
    {
        Guid id = Guid.CreateVersion7();

        Assert.Equal(id, LinkEventValues.ReadLinkRecordId(LinkEventValues.ForLink(id)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("[1,2]")]
    [InlineData("""[{"Name":"Other","Value":"00000000-0000-0000-0000-000000000001"}]""")]
    [InlineData("""[{"Name":"LinkRecordId","Value":"not a guid"}]""")]
    [InlineData("""[{"Name":"LinkRecordId"}]""")]
    public void EventValues_ThatNameNoLinkRecord_GiveNull(string? eventValues)
    {
        Assert.Null(LinkEventValues.ReadLinkRecordId(eventValues));
    }

    [Fact]
    public void EventValues_TheLinkEntryAmongOthers_IsFound()
    {
        Guid id = Guid.CreateVersion7();
        string values = $$"""[{"Name":"Date","Value":"2026-11-05"},{"Name":"{{LinkEventValues.LinkRecordIdName}}","Value":"{{id:D}}"}]""";

        Assert.Equal(id, LinkEventValues.ReadLinkRecordId(values));
    }
}
