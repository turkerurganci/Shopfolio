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
}
