using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>What the making of an account e-mail's body came to.</summary>
internal enum AccountEmailBodyStatus
{
    /// <summary>The content is made and the e-mail may be sent.</summary>
    Ready,

    /// <summary>The link was used, superseded or expired - or its record is gone - when the body was asked for: no call is made and the row is closed as "not sent - link invalid" (08 2.2 call conditions (4); 05 4.4.3).</summary>
    LinkNotValid,

    /// <summary>The row's values cannot be read, or the link record does not belong to the row: no call is made and the attempt is a recipient error (08 2.2 call conditions (5)).</summary>
    Unreadable,
}

/// <summary>The outcome of <see cref="AccountEmailBodies.MakeAsync"/>: the status and, on <see cref="AccountEmailBodyStatus.Ready"/>, the content.</summary>
internal sealed record AccountEmailBody(AccountEmailBodyStatus Status, EmailContent? Content);

/// <summary>
/// The making of an account e-mail's body at the time of sending (08 2.2.8; 05 4.4.2, 4.4.3, 6.4.6; T309): the link record the row names is read first and its
/// state is taken at this moment - a link used, superseded or expired in the meantime (or whose owner is gone and with it the record) gives
/// <see cref="AccountEmailBodyStatus.LinkNotValid"/> and no body is made, so the key is not even derived. A retried e-mail is judged the same way every time it is
/// attempted: the life runs from the event and never waits for the e-mail (08 2.2.8 rule 6). The link record must be of the type the e-mail's type needs and belong
/// to the record the row names (the account, or the invitation); a row whose link is another's is unreadable, never sent. The method reads and writes nothing else
/// and closes no row - the sender does, as "not sent - link invalid" or as the attempt of a recipient error.
/// </summary>
internal sealed class AccountEmailBodies
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;
    private readonly EmailLinks _links;
    private readonly LinkKeys _keys;

    public AccountEmailBodies(AppDbContext db, TimeProvider time, EmailLinks links, LinkKeys keys)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(time);
        ArgumentNullException.ThrowIfNull(links);
        ArgumentNullException.ThrowIfNull(keys);
        _db = db;
        _time = time;
        _links = links;
        _keys = keys;
    }

    /// <summary>The body of the row: <paramref name="brand"/> is the brand name, or the domain while it is empty (02 3.29.3).</summary>
    public async Task<AccountEmailBody> MakeAsync(DispatchType type, DispatchRelatedRecordType? relatedRecordType, Guid? relatedRecordId, string? eventValues, string brand, CancellationToken ct)
    {
        if (AccountEmailTemplates.CarriesLink(type))
        {
            if (AccountEmailTemplates.LinkRecordIdOf(eventValues) is not { } linkId || relatedRecordId is not { } owner || LinkTypeOf(type) is not { } expected)
            {
                return Unreadable;
            }

            LinkRecord? link = await _db.LinkRecords.AsNoTracking().SingleOrDefaultAsync(l => l.Id == linkId, ct);
            if (link is null)
            {
                return new AccountEmailBody(AccountEmailBodyStatus.LinkNotValid, null);
            }

            if (link.LinkType != expected || !BelongsTo(link, relatedRecordType, owner))
            {
                return Unreadable;
            }

            if (LinkRecords.StateOf(link, _time.GetUtcNow()) != LinkState.Valid)
            {
                return new AccountEmailBody(AccountEmailBodyStatus.LinkNotValid, null);
            }
        }

        EmailContent? content = AccountEmailTemplates.Compose(type, relatedRecordType, eventValues, brand, _links, _keys);
        return content is null ? Unreadable : new AccountEmailBody(AccountEmailBodyStatus.Ready, content);
    }

    private static AccountEmailBody Unreadable { get; } = new(AccountEmailBodyStatus.Unreadable, null);

    /// <summary>The link type each e-mail type carries; null for the type without a link.</summary>
    public static LinkType? LinkTypeOf(DispatchType type) => type switch
    {
        DispatchType.EmailVerificationLink => LinkType.EmailVerification,
        DispatchType.PasswordResetLink
            => LinkType.PasswordReset,
        DispatchType.NewEmailVerificationLink => LinkType.NewEmailVerification,
        DispatchType.EmailChangeNotice => LinkType.EmailChangeRevert,
        DispatchType.AdminInvitation => LinkType.AdminInvitation,
        DispatchType.GoogleLinkConfirmationLink => LinkType.GoogleLinkConfirmation,
        _ => null,
    };

    private static bool BelongsTo(LinkRecord link, DispatchRelatedRecordType? relatedRecordType, Guid owner) => relatedRecordType switch
    {
        DispatchRelatedRecordType.Customer => link.CustomerId == owner,
        DispatchRelatedRecordType.Admin => link.AdminId == owner,
        DispatchRelatedRecordType.AdminInvitation => link.AdminInvitationId == owner,
        _ => false,
    };
}
