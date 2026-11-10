using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>What sending one due row came to.</summary>
internal enum DispatchSendOutcome
{
    /// <summary>The row is gone, no longer "queued", not due yet, or of a type this sender does not pick: nothing was done.</summary>
    NotDue,

    /// <summary>The per-recipient limit refused the row at its first attempt: it was written "undelivered" and no call was made (08 2.2.4).</summary>
    Refused,

    /// <summary>The link of an account e-mail was not valid any more at the time of sending: the row was closed without a call and without an attempt (05 4.4.3).</summary>
    ClosedLinkNotValid,

    /// <summary>The body could not be made (the row's values are unreadable, or no recipient is left): written as a recipient error without a call (08 2.2.2 call conditions).</summary>
    BodyNotMade,

    /// <summary>An attempt was made and its result was written on the row and on the channel state (<see cref="DispatchAttempts"/>).</summary>
    Attempted,
}

/// <summary>
/// The sender of the dispatch record's "queued" rows (T385; 08 2.2.2, 2.2.4; 05 4.4.2, 4.4.3; 06 4.58): for one due row it asks the per-recipient limit once, at the first attempt
/// (<see cref="RecipientLimitCheck"/>), makes the body at the time of sending - from the event's values and, for an account link, from the link record read first
/// (<see cref="AccountEmailBodies"/>) - and sends it once through <see cref="DispatchAttempts"/>, whose result writes the row by <see cref="DispatchRetrySchedule"/> (the
/// retries 5 minutes, 30 minutes and 2 hours after the first attempt, then "undelivered") and the channel state in one transaction. Nothing here is a transaction: the call goes out outside
/// any (09 10.7 item 5). The "e-mail did not arrive" mark is read from the row's state afterwards (<see cref="DispatchMarks"/>) and is never written. A link e-mail whose link is no longer
/// valid is closed without a call and without an attempt - "undelivered" with no last outcome - which puts no mark and touches no channel state (05 4.4.3; D-233).
/// The customer notices B-1…B-16 are not picked yet: their bodies need the order's facts (D-965), so those rows wait "queued" until the sender that makes them is added.
/// </summary>
internal sealed class DispatchSender
{
    // The families whose body this sender makes: the firm notices and the account and administration e-mails.
    private static readonly DispatchType[] Picked =
    [
        DispatchType.F1, DispatchType.F2, DispatchType.F3, DispatchType.F4, DispatchType.F5, DispatchType.F6,
        DispatchType.EmailVerificationLink, DispatchType.PasswordResetLink, DispatchType.NewEmailVerificationLink, DispatchType.EmailChangeNotice,
        DispatchType.AdminInvitation, DispatchType.AccountDeletedNotice, DispatchType.GoogleLinkConfirmationLink,
    ];

    private readonly AppDbContext _db;
    private readonly ITransactionRunner _transactions;
    private readonly TimeProvider _time;
    private readonly DispatchAttempts _attempts;
    private readonly RecipientLimitCheck _limit;
    private readonly AccountEmailBodies _accountBodies;
    private readonly EmailComposer _composer;
    private readonly EmailLinks _links;
    private readonly ISettingsModule _settings;
    private readonly ICorporateContentModule _brand;

    public DispatchSender(
        AppDbContext db,
        ITransactionRunner transactions,
        TimeProvider time,
        DispatchAttempts attempts,
        RecipientLimitCheck limit,
        AccountEmailBodies accountBodies,
        EmailComposer composer,
        EmailLinks links,
        ISettingsModule settings,
        ICorporateContentModule brand)
    {
        _db = db;
        _transactions = transactions;
        _time = time;
        _attempts = attempts;
        _limit = limit;
        _accountBodies = accountBodies;
        _composer = composer;
        _links = links;
        _settings = settings;
        _brand = brand;
    }

    /// <summary>True for the types this sender picks.</summary>
    public static bool Picks(DispatchType type) => Picked.Contains(type);

    /// <summary>The "queued" rows of the picked types whose time has come - the first attempt when none is set, else the next attempt - earliest first (06 7.5: the queue's index).</summary>
    public static async Task<IReadOnlyList<DueRecord>> FindDueAsync(AppDbContext db, DateTimeOffset now, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        var rows = await db.DispatchRecords.AsNoTracking()
            .Where(d => d.DispatchState == DispatchState.Queued && (d.NextAttemptAt == null || d.NextAttemptAt <= now) && Picked.Contains(d.DispatchType))
            .Select(d => new { d.Id, Due = d.NextAttemptAt ?? d.CreatedAt })
            .ToListAsync(ct);
        return [.. rows.OrderBy(r => r.Due).ThenBy(r => r.Id).Select(r => new DueRecord(r.Id, r.Due))];
    }

    /// <summary>Sends the row <paramref name="dispatchId"/> once, if it is a due "queued" row of a picked type.</summary>
    public async Task<DispatchSendOutcome> SendAsync(Guid dispatchId, CancellationToken ct)
    {
        DateTimeOffset now = _time.GetUtcNow();
        var row = await _db.DispatchRecords.AsNoTracking()
            .Where(d => d.Id == dispatchId)
            .Select(d => new { d.DispatchType, d.DispatchState, d.NextAttemptAt, d.RecipientEmail, d.RelatedRecordType, d.RelatedRecordId, d.EventValues })
            .SingleOrDefaultAsync(ct);
        if (row is null || row.DispatchState != DispatchState.Queued || (row.NextAttemptAt is { } due && due > now) || !Picks(row.DispatchType))
        {
            return DispatchSendOutcome.NotDue;
        }

        if (await _limit.AskAsync(dispatchId, ct) == RecipientLimitVerdict.Refused)
        {
            return DispatchSendOutcome.Refused;
        }

        EmailContent? content = null;
        if (!string.IsNullOrWhiteSpace(row.RecipientEmail))
        {
            BrandIdentity brand = await _brand.GetBrandIdentityAsync(ct);
            if (DispatchTypeFamily.IsAccountEmail(row.DispatchType))
            {
                AccountEmailBody body = await _accountBodies.MakeAsync(row.DispatchType, row.RelatedRecordType, row.RelatedRecordId, row.EventValues, brand.DisplayName, ct);
                if (body.Status == AccountEmailBodyStatus.LinkNotValid)
                {
                    return await CloseWithoutAttemptAsync(dispatchId, ct) ? DispatchSendOutcome.ClosedLinkNotValid : DispatchSendOutcome.NotDue;
                }

                content = body.Content;
            }
            else
            {
                content = CompanyNoticeTemplates.Compose(row.DispatchType, row.EventValues, _links);
            }

            if (content is not null)
            {
                LegalIdentity identity = await _settings.GetLegalIdentityAsync(ct);
                EmailBranding branding = new(brand.HasBrandName ? brand.DisplayName : null, null, brand.IsDefaultColor ? null : brand.Color);
                EmailFirm firm = new(identity.LegalName ?? string.Empty, identity.Email?.Trim() ?? string.Empty);
                EmailMessage message = await _composer.ComposeAsync(row.RecipientEmail.Trim(), branding, firm, content, ct);
                await _attempts.SendAsync(dispatchId, message, ct);
                return DispatchSendOutcome.Attempted;
            }
        }

        // No body, or no recipient: the attempt is written as a recipient error without a call, and goes the retry way like every other failure.
        await _attempts.RecordAsync(dispatchId, new EmailSendResult.RecipientError("BodyNotMade"), now, ct);
        return DispatchSendOutcome.BodyNotMade;
    }

    private async Task<bool> CloseWithoutAttemptAsync(Guid dispatchId, CancellationToken ct)
    {
        CloseResult closed = await _transactions.RunAsync(
            async cancel =>
            {
                DispatchRecord? row = await _db.DispatchRecords.SingleOrDefaultAsync(d => d.Id == dispatchId && d.DispatchState == DispatchState.Queued, cancel);
                if (row is null)
                {
                    return new CloseResult(false);
                }

                // "Undelivered" without a last outcome and without an attempt: the closed-unsent row the marks read past (DispatchMarks.Row.ClosedWithoutAttempt).
                row.DispatchState = DispatchState.Undelivered;
                row.NextAttemptAt = null;
                return new CloseResult(true);
            },
            ct);
        return closed.Applied;
    }

    private sealed record CloseResult(bool Applied) : IOperationResult
    {
        public bool IsRejected => !Applied;
    }
}

/// <summary>The family of a dispatch type, where more than one place asks it.</summary>
internal static class DispatchTypeFamily
{
    /// <summary>True for the seven account and administration e-mails (08 2.2.8): the body is made by <see cref="AccountEmailBodies"/>.</summary>
    public static bool IsAccountEmail(DispatchType type) => type is
        DispatchType.EmailVerificationLink or DispatchType.PasswordResetLink or DispatchType.NewEmailVerificationLink or DispatchType.EmailChangeNotice
        or DispatchType.AdminInvitation or DispatchType.AccountDeletedNotice or DispatchType.GoogleLinkConfirmationLink;
}
