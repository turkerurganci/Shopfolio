using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.UnitTests.Notifications;

/// <summary>
/// The pure rule of the per-recipient sending limit (T304 criteria 1-6, KK1, KK3-KK5; 08 2.2.4 "Sayaç", "Sayılmayanlar", "Eşik", "Aşım"; 06 2.8.1,
/// 4.58): which rows take part, how the earlier rows of a recipient are counted, where the row is over the limit and what the refusal writes.
/// The numbers come from <see cref="RecipientSendLimit"/>; the tests write none of them.
/// </summary>
public sealed class RecipientSendLimitTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 6, 12, 0, 0, TimeSpan.Zero);

    private static readonly Guid Evaluated = new("00000000-0000-7000-8000-000000000100");

    private static RecipientSendLimit.Candidate At(DateTimeOffset createdAt, string recipient = "ali@example.com", Guid? id = null) =>
        new(id ?? Guid.CreateVersion7(), createdAt, recipient);

    private static List<RecipientSendLimit.Candidate> Earlier(int count, string recipient = "ali@example.com") =>
        [.. Enumerable.Range(1, count).Select(i => At(Now.AddMinutes(-i), recipient))];

    // ---- KK1: who is limited and counted ----

    [Theory]
    [InlineData(DispatchType.B1, true)]
    [InlineData(DispatchType.B9, true)]
    [InlineData(DispatchType.B16, true)]
    [InlineData(DispatchType.F1, false)]
    [InlineData(DispatchType.F5, false)]
    [InlineData(DispatchType.F6, false)]
    [InlineData(DispatchType.EmailVerificationLink, false)]
    [InlineData(DispatchType.PasswordResetLink, false)]
    [InlineData(DispatchType.NewEmailVerificationLink, false)]
    [InlineData(DispatchType.EmailChangeNotice, false)]
    [InlineData(DispatchType.AdminInvitation, false)]
    [InlineData(DispatchType.AccountDeletedNotice, false)]
    [InlineData(DispatchType.GoogleLinkConfirmationLink, false)]
    public void OnlyTheCustomerNoticesB1ToB16_TakePartInTheLimit(DispatchType type, bool expected)
    {
        Assert.Equal(expected, RecipientSendLimit.TakesPart(type, Guid.NewGuid()));
    }

    [Fact]
    public void ACustomerNoticeWithoutOrderLink_DoesNotTakePart()
    {
        Assert.False(RecipientSendLimit.TakesPart(DispatchType.B1, null));
    }

    [Fact]
    public void EveryCustomerNotice_IsAnEnumValueFromB1ToB16_AndNoOtherValueIs()
    {
        DispatchType[] customer = [.. Enum.GetValues<DispatchType>().Where(RecipientSendLimit.IsCustomerNotice)];
        Assert.Equal(16, customer.Length);
        Assert.Equal(Enumerable.Range(1, 16).Select(n => Enum.Parse<DispatchType>($"B{n}")), customer);
    }

    // ---- KK4: the threshold ----

    [Fact]
    public void TheTwentiethRow_Passes_AndTheTwentyFirst_IsOver()
    {
        int nineteenEarlier = RecipientSendLimit.CountEarlier(Now, Evaluated, "ali@example.com", Earlier(RecipientSendLimit.Threshold - 1));
        int twentyEarlier = RecipientSendLimit.CountEarlier(Now, Evaluated, "ali@example.com", Earlier(RecipientSendLimit.Threshold));

        Assert.False(RecipientSendLimit.IsOver(nineteenEarlier));
        Assert.True(RecipientSendLimit.IsOver(twentyEarlier));
    }

    [Fact]
    public void TheRowItself_AndTheRowsMadeAfterIt_AreNotCounted()
    {
        List<RecipientSendLimit.Candidate> candidates = Earlier(RecipientSendLimit.Threshold - 1);
        candidates.Add(At(Now, id: Evaluated));
        candidates.AddRange(Enumerable.Range(1, 5).Select(i => At(Now.AddMinutes(i))));

        Assert.Equal(RecipientSendLimit.Threshold - 1, RecipientSendLimit.CountEarlier(Now, Evaluated, "ali@example.com", candidates));
    }

    [Fact]
    public void RowsMadeAtTheSameMoment_AreOrderedById_TheSmallerIdIsEarlier()
    {
        Guid smaller = new("00000000-0000-7000-8000-000000000001");
        Guid larger = new("ffffffff-0000-7000-8000-000000000001");
        List<RecipientSendLimit.Candidate> candidates = [At(Now, id: smaller), At(Now, id: larger)];

        Assert.Equal(1, RecipientSendLimit.CountEarlier(Now, larger, "ali@example.com", candidates));
        Assert.Equal(0, RecipientSendLimit.CountEarlier(Now, smaller, "ali@example.com", candidates));
    }

    [Fact]
    public void TheIdOrder_IsTheDatabasesByteOrder_NotTheGuidStructOrder()
    {
        // The first byte decides in PostgreSQL's uuid order, whatever the later bytes are.
        Guid first = new("10000000-0000-0000-0000-0000000000ff");
        Guid second = new("20000000-0000-0000-0000-000000000001");
        Guid sameButLastByte = new("10000000-0000-0000-0000-000000000100");

        Assert.True(UuidOrder.Compare(first, second) < 0);
        Assert.True(UuidOrder.Compare(first, sameButLastByte) < 0);
        Assert.True(UuidOrder.IsBefore(Now, first, Now, second));
        Assert.False(UuidOrder.IsBefore(Now, second, Now, first));
    }

    [Fact]
    public void TwentyRowsAtOneMoment_TheTwentyFirstByIdIsOver_AndTheTwentiethIsNot()
    {
        Guid[] ids = [.. Enumerable.Range(1, 21).Select(i => new Guid($"00000000-0000-7000-8000-{i:D12}"))];
        List<RecipientSendLimit.Candidate> candidates = [.. ids.Select(id => At(Now, id: id))];

        Assert.False(RecipientSendLimit.IsOver(RecipientSendLimit.CountEarlier(Now, ids[19], "ali@example.com", candidates)));
        Assert.True(RecipientSendLimit.IsOver(RecipientSendLimit.CountEarlier(Now, ids[20], "ali@example.com", candidates)));
    }

    // ---- KK3: the recipient is compared with the identity normalization ----

    [Theory]
    [InlineData("ALİ@Example.COM", "ali@example.com")]
    [InlineData("  ali@example.com ", "ali@example.com")]
    [InlineData("ALI@EXAMPLE.COM", "alı@example.com")]
    public void AddressesThatDifferOnlyInCaseSpaceOrTheTurkishI_AreTheSameRecipient(string earlier, string evaluated)
    {
        List<RecipientSendLimit.Candidate> candidates = [At(Now.AddMinutes(-1), earlier)];

        Assert.Equal(1, RecipientSendLimit.CountEarlier(Now, Evaluated, evaluated, candidates));
    }

    [Fact]
    public void AnotherAddress_AndARowWithoutAnAddress_AreNotCounted()
    {
        List<RecipientSendLimit.Candidate> candidates =
        [
            At(Now.AddMinutes(-1), "veli@example.com"),
            new(Guid.CreateVersion7(), Now.AddMinutes(-2), null),
        ];

        Assert.Equal(0, RecipientSendLimit.CountEarlier(Now, Evaluated, "ali@example.com", candidates));
    }

    // ---- KK5: the window slides ----

    [Fact]
    public void ARowOlderThanTheWindow_FallsOut_AndOneInsideItIsCounted()
    {
        List<RecipientSendLimit.Candidate> candidates =
        [
            At(Now - RecipientSendLimit.Window),
            At(Now - RecipientSendLimit.Window - TimeSpan.FromTicks(1)),
        ];

        Assert.Equal(1, RecipientSendLimit.CountEarlier(Now, Evaluated, "ali@example.com", candidates));
    }

    [Fact]
    public void WhenTheWindowSlides_TheSameTwentyRowsAreNoLongerOverForALaterRow()
    {
        List<RecipientSendLimit.Candidate> twenty = [.. Enumerable.Range(0, RecipientSendLimit.Threshold).Select(i => At(Now.AddMinutes(-i)))];

        Assert.True(RecipientSendLimit.IsOver(RecipientSendLimit.CountEarlier(Now.AddMinutes(1), Evaluated, "ali@example.com", twenty)));

        // The oldest of the twenty is one minute outside the window of a row made 24 hours and a minute after it.
        DateTimeOffset oldest = Now.AddMinutes(-(RecipientSendLimit.Threshold - 1));
        DateTimeOffset later = oldest + RecipientSendLimit.Window + TimeSpan.FromMinutes(1);
        int count = RecipientSendLimit.CountEarlier(later, Evaluated, "ali@example.com", twenty);

        Assert.Equal(RecipientSendLimit.Threshold - 1, count);
        Assert.False(RecipientSendLimit.IsOver(count));
    }

    // ---- KK6, KK8: the refusal and the exemption ----

    [Fact]
    public void Refuse_WritesUndelivered_WithTheRecipientError_AndNoNextAttempt()
    {
        DispatchRecord row = DispatchRecord.Queue(DispatchType.B2, "ali@example.com", Guid.NewGuid(), null, null, Now);

        RecipientSendLimit.Refuse(row);

        Assert.Equal(DispatchState.Undelivered, row.DispatchState);
        Assert.Equal(DispatchOutcome.RecipientError, row.LastOutcome);
        Assert.Equal(1, row.AttemptCount);
        Assert.Null(row.NextAttemptAt);
        Assert.Null(row.SentAt);
    }

    [Fact]
    public void Refuse_OnARowThatWasAlreadyTried_IsAProgrammingError()
    {
        DispatchRecord row = DispatchRecord.Queue(DispatchType.B2, "ali@example.com", Guid.NewGuid(), null, null, Now);
        row.AttemptCount = 1;

        Assert.Throws<InvalidOperationException>(() => RecipientSendLimit.Refuse(row));
    }

    [Fact]
    public void ARowWithAnOriginalDispatchId_IsExempt_AndOtherRowsAreNot()
    {
        Assert.True(RecipientSendLimit.IsExempt(Guid.NewGuid()));
        Assert.False(RecipientSendLimit.IsExempt(null));
    }
}
