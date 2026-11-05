using System.Diagnostics;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// The retry rule of an e-mail that could not be sent (08 2.2 "Retry stratejisi", 2.2.2 "(sonuç)"; 05 4.4.3; 02 11.2 P-38, Z-26; K-462): one
/// attempt is one row of the dispatch record, and a failed attempt - a recipient error as much as a channel error, 05 4.4.3 does not tell them
/// apart - is tried again on the same row 5 minutes, 30 minutes and 2 hours after the first attempt. The three moments are counted from the
/// first attempt and are not added one after the other; the first attempt's moment is not kept on the row, so <c>NextAttemptAt</c> is written
/// step by step: the first failure's start + 5 minutes, then the previous <c>NextAttemptAt</c> + 25 minutes, then + 90 minutes. A late queue
/// does not move the later moments. When the third retry fails as well the row is "undelivered". The rule is pure: it changes the row it is
/// given and nothing else; the channel state is <see cref="DispatchAttempts"/>'s.
/// </summary>
internal static class DispatchRetrySchedule
{
    /// <summary>The retries' distances from the first attempt (P-38), in order; the numbers live in <see cref="ProductConstants"/> only.</summary>
    public static IReadOnlyList<TimeSpan> RetriesFromFirstAttempt { get; } =
    [
        TimeSpan.FromMinutes(ProductConstants.DispatchFirstRetryMinutes),
        TimeSpan.FromMinutes(ProductConstants.DispatchSecondRetryMinutes),
        TimeSpan.FromMinutes(ProductConstants.DispatchThirdRetryMinutes),
    ];

    /// <summary>The attempts a row gets at most: the first one and the three retries.</summary>
    public static int AttemptLimit => RetriesFromFirstAttempt.Count + 1;

    /// <summary>The class of a sending's result as the dispatch record stores it (06 5.4.7; 05 10.2.2).</summary>
    public static DispatchOutcome OutcomeOf(EmailSendResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result switch
        {
            EmailSendResult.Sent => DispatchOutcome.Sent,
            EmailSendResult.RecipientError => DispatchOutcome.RecipientError,
            EmailSendResult.ChannelError => DispatchOutcome.ChannelError,
            _ => throw new UnreachableException(),
        };
    }

    /// <summary>
    /// Writes one attempt's result on a "queued" row: on "sent" the row is sent at <paramref name="now"/>; on an error the class is the last
    /// outcome and the next attempt's moment is set, or - after the last retry - the row is undelivered. The attempt count grows by one on
    /// every attempt, the first sending included (06 4.58 <c>AttemptCount</c>). A row that is not "queued" is a programming error: the caller
    /// reads only "queued" rows.
    /// </summary>
    public static void Apply(DispatchRecord row, DispatchOutcome outcome, DateTimeOffset attemptStartedAt, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(row);
        if (row.DispatchState != DispatchState.Queued)
        {
            throw new InvalidOperationException("Only a queued dispatch row takes an attempt's result.");
        }

        int attemptsBefore = row.AttemptCount;
        row.AttemptCount = attemptsBefore + 1;
        row.LastOutcome = outcome;
        switch (outcome)
        {
            case DispatchOutcome.Sent:
                row.DispatchState = DispatchState.Sent;
                row.SentAt = now;
                row.NextAttemptAt = null;
                break;
            case DispatchOutcome.RecipientError:
            case DispatchOutcome.ChannelError:
                DateTimeOffset? next = NextAttemptAfterFailure(attemptsBefore, row.NextAttemptAt, attemptStartedAt);
                row.NextAttemptAt = next;
                if (next is null)
                {
                    row.DispatchState = DispatchState.Undelivered;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Unknown dispatch outcome.");
        }
    }

    /// <summary>
    /// The next attempt's moment after a failed attempt, or <c>null</c> when that attempt was the last retry. <paramref name="attemptsBefore"/>
    /// is the count before the failed attempt (0 for the first attempt); <paramref name="scheduledAt"/> is the moment the failed attempt was
    /// due, the previous step of the schedule. A retry without its moment on the row (not written by this rule) is counted from its start.
    /// </summary>
    public static DateTimeOffset? NextAttemptAfterFailure(int attemptsBefore, DateTimeOffset? scheduledAt, DateTimeOffset attemptStartedAt)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(attemptsBefore);
        if (attemptsBefore >= RetriesFromFirstAttempt.Count)
        {
            return null;
        }

        if (attemptsBefore == 0)
        {
            return attemptStartedAt + RetriesFromFirstAttempt[0];
        }

        TimeSpan step = RetriesFromFirstAttempt[attemptsBefore] - RetriesFromFirstAttempt[attemptsBefore - 1];
        return (scheduledAt ?? attemptStartedAt) + step;
    }
}
