using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.UnitTests.Notifications;

/// <summary>
/// The order's "e-mail did not arrive" mark is read from the group's latest row (T304 criterion 7, KK7, KK8; 06 4.58 "İşaretlerin ve uyarıların
/// okunuşu"; 02 9.1.6): an "undelivered" latest row puts it, a resending that is sent takes it, a later undelivered resending puts it again.
/// </summary>
public sealed class DispatchMarksTests
{
    private static readonly DateTimeOffset Start = new(2026, 11, 6, 9, 0, 0, TimeSpan.Zero);

    private static DispatchMarks.Row Row(DateTimeOffset at, DispatchState state, Guid? id = null, Guid? original = null) =>
        new(id ?? Guid.CreateVersion7(), original, at, state);

    [Fact]
    public void AnUndeliveredRow_PutsTheMark()
    {
        Assert.True(DispatchMarks.HasMark([Row(Start, DispatchState.Undelivered)]));
    }

    [Fact]
    public void SentAndQueuedRows_PutNoMark()
    {
        Assert.False(DispatchMarks.HasMark([Row(Start, DispatchState.Sent), Row(Start.AddMinutes(1), DispatchState.Queued)]));
        Assert.False(DispatchMarks.HasMark([]));
    }

    [Fact]
    public void ASentResending_TakesTheMark_AndALaterUndeliveredOneRestoresIt()
    {
        Guid first = Guid.CreateVersion7();
        DispatchMarks.Row refused = Row(Start, DispatchState.Undelivered, first);
        DispatchMarks.Row resentSent = Row(Start.AddHours(1), DispatchState.Sent, original: first);
        DispatchMarks.Row resentUndelivered = Row(Start.AddHours(2), DispatchState.Undelivered, original: first);

        Assert.True(DispatchMarks.HasMark([refused]));
        Assert.False(DispatchMarks.HasMark([refused, resentSent]));
        Assert.True(DispatchMarks.HasMark([refused, resentSent, resentUndelivered]));
    }

    [Fact]
    public void AnotherEmailsGroup_KeepsItsOwnMark()
    {
        DispatchMarks.Row refused = Row(Start, DispatchState.Undelivered);
        DispatchMarks.Row otherSent = Row(Start.AddHours(1), DispatchState.Sent);

        Assert.True(DispatchMarks.HasMark([refused, otherSent]));
    }

    [Fact]
    public void ARowClosedWithoutSending_PutsNoMark()
    {
        // 06 4.58: a row whose link was no longer valid is closed without an attempt; it is not an e-mail that failed to arrive.
        Assert.False(DispatchMarks.HasMark([new DispatchMarks.Row(Guid.CreateVersion7(), null, Start, DispatchState.Undelivered, ClosedUnsent: true)]));
    }

    private static DispatchMarks.NoticeRow Notice(DateTimeOffset at, DispatchState state, Guid? id = null, Guid? original = null, bool closedUnsent = false) =>
        new(id ?? Guid.CreateVersion7(), original, at, state, DispatchType.B2, null, null, closedUnsent);

    [Fact]
    public void UndeliveredGroups_ListsOnlyGroupsWhoseLatestRowIsUndelivered_OldestFirst()
    {
        Guid firstId = Guid.CreateVersion7();
        Guid secondId = Guid.CreateVersion7();
        DispatchMarks.NoticeRow refused = Notice(Start, DispatchState.Undelivered, firstId);
        DispatchMarks.NoticeRow refusedLater = Notice(Start.AddHours(1), DispatchState.Undelivered, secondId);
        DispatchMarks.NoticeRow thirdUndelivered = Notice(Start.AddHours(2), DispatchState.Undelivered);
        DispatchMarks.NoticeRow thirdSentAgain = Notice(Start.AddHours(3), DispatchState.Sent, original: thirdUndelivered.Id);

        IReadOnlyList<UndeliveredNotice> groups = DispatchMarks.UndeliveredGroups([refusedLater, thirdSentAgain, refused, thirdUndelivered]);

        Assert.Equal([firstId, secondId], groups.Select(g => g.LatestDispatchId));
    }

    [Fact]
    public void UndeliveredGroups_NamesTheLatestRowOfTheGroup_AndSkipsAClosedUnsentOne()
    {
        Guid firstId = Guid.CreateVersion7();
        DispatchMarks.NoticeRow first = Notice(Start, DispatchState.Undelivered, firstId);
        DispatchMarks.NoticeRow again = Notice(Start.AddHours(1), DispatchState.Undelivered, original: firstId);
        DispatchMarks.NoticeRow closed = Notice(Start.AddHours(2), DispatchState.Undelivered, closedUnsent: true);

        UndeliveredNotice only = Assert.Single(DispatchMarks.UndeliveredGroups([first, again, closed]));

        Assert.Equal((firstId, again.Id), (only.GroupId, only.LatestDispatchId));
    }
}
