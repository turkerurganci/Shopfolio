using Shopfolio.Infrastructure;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// The per-recipient sending limit of the customer notifications (08 2.2.4; K-1242): the counter is not stored, it is read from the dispatch
/// record. The rule here is pure - it counts and it changes the row it is given, nothing else; <see cref="RecipientLimitCheck"/> reads the rows
/// and writes the result.
/// <para>
/// A row's earlier rows are the customer notice rows (B-1…B-16: the rows with an order link, 06 4.58) of the same recipient - compared with the
/// one identity normalization, 06 2.8.1 - that were made in the 24 hours before the row's own moment, in the order of <c>CreatedAt</c> and, when
/// the moments are equal, of <c>Id</c> (06 2.3.1); the row itself and the rows made after it are not counted. A row whose state is of no
/// importance is counted: an "undelivered" row that the limit itself refused is a made row. When the count has reached the threshold the row
/// is over the limit. Firm notices and account and administration e-mails (no order link) are neither counted nor limited. A resending from the
/// panel and the B-1 after a correction (<c>OriginalDispatchId</c> filled) are counted by later rows but are never refused (08 2.2.4 "Muafiyet").
/// </para>
/// </summary>
internal static class RecipientSendLimit
{
    /// <summary>The threshold of 08 2.2.4 "Eşik": 20 customer notifications to one address (a design value, not a target - K-1242 (2)).</summary>
    public const int Threshold = 20;

    /// <summary>The sliding window of 08 2.2.4 "Eşik": 24 hours before the evaluated row's own moment.</summary>
    public static readonly TimeSpan Window = TimeSpan.FromHours(24);

    /// <summary>
    /// The window of the thinned security line of an over-the-limit row (08 2.2.4 "Kayıt"; K-941 (1)'s pattern): the 60 seconds of the general
    /// rate limit's sliding window, one kind of window (K-1570). One window for the whole product - the address is not written and not kept.
    /// </summary>
    public static readonly TimeSpan LogWindow = TimeSpan.FromSeconds(60);

    /// <summary>True for the customer notifications B-1…B-16; the firm notices (F), the account and the administration e-mails are not limited and not counted.</summary>
    public static bool IsCustomerNotice(DispatchType type) => type is >= DispatchType.B1 and <= DispatchType.B16;

    /// <summary>True when the row takes part in the limit at all: a customer notice that carries its order link (06 4.58).</summary>
    public static bool TakesPart(DispatchType type, Guid? orderId) => IsCustomerNotice(type) && orderId is not null;

    /// <summary>True when the row is exempt from the limit but still counted: a resending or a B-1 after a correction (<c>OriginalDispatchId</c> filled).</summary>
    public static bool IsExempt(Guid? originalDispatchId) => originalDispatchId is not null;

    /// <summary>The first moment that is still inside the window of a row made at <paramref name="at"/>.</summary>
    public static DateTimeOffset WindowStart(DateTimeOffset at) => at - Window;

    /// <summary>
    /// How many of <paramref name="candidates"/> are earlier rows of <paramref name="recipient"/> inside the window of the row at
    /// (<paramref name="at"/>, <paramref name="id"/>). The candidates may contain the row itself and later rows; they are not counted.
    /// </summary>
    public static int CountEarlier(DateTimeOffset at, Guid id, string recipient, IEnumerable<Candidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(recipient);
        ArgumentNullException.ThrowIfNull(candidates);
        string wanted = TextNormalizer.NormalizeIdentity(recipient);
        DateTimeOffset start = WindowStart(at);
        return candidates.Count(c =>
            c.RecipientEmail is not null
            && c.CreatedAt >= start
            && UuidOrder.IsBefore(c.CreatedAt, c.Id, at, id)
            && string.Equals(TextNormalizer.NormalizeIdentity(c.RecipientEmail), wanted, StringComparison.Ordinal));
    }

    /// <summary>True when the count of earlier rows has reached the threshold: the row is over the limit.</summary>
    public static bool IsOver(int earlierCount) => earlierCount >= Threshold;

    /// <summary>
    /// Writes the refusal on a "queued" row that was never tried: the first attempt ends at once as "undelivered" with the recipient error, no
    /// next attempt is set, the sending service is not called and the channel state is not touched (08 2.2.4 "Aşım"). The attempt count
    /// grows by one as it does on every attempt (06 4.58): the refusal is the row's first attempt, so the limit is not asked again.
    /// </summary>
    public static void Refuse(DispatchRecord row)
    {
        ArgumentNullException.ThrowIfNull(row);
        if (row.DispatchState != DispatchState.Queued || row.AttemptCount != 0)
        {
            throw new InvalidOperationException("Only a queued dispatch row that was never tried can be refused by the limit.");
        }

        row.AttemptCount = 1;
        row.LastOutcome = DispatchOutcome.RecipientError;
        row.DispatchState = DispatchState.Undelivered;
        row.NextAttemptAt = null;
    }

    /// <summary>An earlier row as the count sees it: its place in the order and its recipient as entered.</summary>
    internal readonly record struct Candidate(Guid Id, DateTimeOffset CreatedAt, string? RecipientEmail);
}

/// <summary>
/// The order of two rows by (<c>CreatedAt</c>, <c>Id</c>) as the database sorts them (06 2.3.1): PostgreSQL compares a <c>uuid</c> byte by byte
/// from the first, which is the order of its text form; the bytes are read in that (big-endian) order here, so the in-memory order and the
/// database's are the same by construction and not by the layout of <see cref="Guid"/>.
/// </summary>
internal static class UuidOrder
{
    /// <summary>The comparison of two ids as PostgreSQL's <c>uuid</c> type makes it.</summary>
    public static int Compare(Guid left, Guid right)
    {
        Span<byte> a = stackalloc byte[16];
        Span<byte> b = stackalloc byte[16];
        left.TryWriteBytes(a, bigEndian: true, out _);
        right.TryWriteBytes(b, bigEndian: true, out _);
        return a.SequenceCompareTo(b);
    }

    /// <summary>True when the first row is made before the second: earlier moment, or the same moment and the smaller id.</summary>
    public static bool IsBefore(DateTimeOffset leftAt, Guid leftId, DateTimeOffset rightAt, Guid rightId)
    {
        int byMoment = leftAt.UtcDateTime.CompareTo(rightAt.UtcDateTime);
        return byMoment != 0 ? byMoment < 0 : Compare(leftId, rightId) < 0;
    }
}
