using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Notifications.Contracts;

/// <summary>
/// An event the firm is told about at its contact e-mail (F-1…F-4; 08 2.2.7; 02 9.3.1): the notice is "something happened, look in the panel" and carries no
/// customer data - no name, address, telephone, e-mail or message text exists on any of these records, so none can reach the body or the dispatch row.
/// <see cref="OccurredAt"/> is the event's own moment. The records hold the event's values; the dispatch row keeps them as the event's values because the
/// event's record may be gone when the body is made (08 2.2.7 "Değerlerin saklanışı"; 06 4.58).
/// </summary>
public abstract record CompanyNotice(DateTimeOffset OccurredAt);

/// <summary>F-1: an order was placed. <paramref name="OrderId"/> is the panel's inner key of the order screen; the dispatch row's related record is the order.</summary>
public sealed record NewOrderNotice(Guid OrderId, string OrderNumber, PaymentMethod PaymentMethod, decimal Total, DateTimeOffset OccurredAt) : CompanyNotice(OccurredAt);

/// <summary>
/// F-2: a contact form was sent - the personal data request included. Only the topic travels; the request's text, the sender's name and e-mail do not
/// (08 2.2.7 F-2). <paramref name="ContactRequestId"/> is the inner key of the request screen and the dispatch row's related record.
/// </summary>
public sealed record NewContactRequestNotice(Guid ContactRequestId, ContactTopic Topic, DateTimeOffset OccurredAt) : CompanyNotice(OccurredAt);

/// <summary>
/// F-3: a withdrawal, a termination for late delivery or a defect claim was recorded, or a resolved defect claim was reopened. <paramref name="Kind"/> is one
/// of <see cref="DispatchRelatedRecordType.Withdrawal"/>, <see cref="DispatchRelatedRecordType.LateDeliveryTermination"/> and
/// <see cref="DispatchRelatedRecordType.DefectClaim"/>; <paramref name="RecordId"/> is that record's id (the dispatch row's related record) and
/// <paramref name="OrderId"/> the inner key of the order screen the record sits in. <paramref name="Reopened"/> is true for a defect claim only.
/// </summary>
public sealed record NewClaimNotice(
    DispatchRelatedRecordType Kind,
    Guid RecordId,
    Guid OrderId,
    string OrderNumber,
    bool Reopened,
    DateTimeOffset OccurredAt) : CompanyNotice(OccurredAt);

/// <summary>
/// F-4: the system itself refunded a card payment. <paramref name="RefundType"/> is one of <see cref="RefundType.Cancellation"/>,
/// <see cref="RefundType.ItemRemoval"/>, <see cref="RefundType.LateDeliveryTermination"/> and <see cref="RefundType.LatePayment"/>; <paramref name="RefundId"/> is
/// the dispatch row's related record.
/// </summary>
public sealed record CardRefundNotice(Guid RefundId, Guid OrderId, string OrderNumber, RefundType RefundType, decimal Amount, DateTimeOffset OccurredAt) : CompanyNotice(OccurredAt);

/// <summary>
/// An event every administrator is told about at his or her own address (F-5, F-6; 08 2.2.7; 02 9.3.3, 9.3.4). The names and the invited address are values
/// of the event: the administrator or the invitation may be removed before the body is made.
/// </summary>
public abstract record AdminNotice(DateTimeOffset OccurredAt);

/// <summary>
/// F-5: the bank transfer IBAN was entered or changed. The record never holds the whole IBAN: <see cref="For"/> takes the new IBAN as it was written and keeps
/// its last four digits only (08 2.2.7 F-5; K-1252), so the whole number cannot reach the dispatch row.
/// </summary>
public sealed record IbanChangedNotice : AdminNotice
{
    private const int KeptDigits = 4;

    public IbanChangedNotice(string actorName, string lastFourDigits, bool wasFirstEntry, DateTimeOffset occurredAt)
        : base(occurredAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actorName);
        ArgumentNullException.ThrowIfNull(lastFourDigits);
        if (lastFourDigits.Length != KeptDigits || !lastFourDigits.All(char.IsAsciiDigit))
        {
            throw new ArgumentException("F-5 carries the last four digits of the IBAN and nothing more (08 2.2.7).", nameof(lastFourDigits));
        }

        ActorName = actorName;
        LastFourDigits = lastFourDigits;
        WasFirstEntry = wasFirstEntry;
    }

    /// <summary>The administrator who made the change, by name as a value.</summary>
    public string ActorName { get; }

    /// <summary>The last four digits of the new IBAN.</summary>
    public string LastFourDigits { get; }

    /// <summary>True when the IBAN was entered for the first time ("girildi"), false when it was changed ("değiştirildi").</summary>
    public bool WasFirstEntry { get; }

    /// <summary>A notice of the IBAN as it was written: only its last four digits are kept.</summary>
    public static IbanChangedNotice For(string newIban, string actorName, bool wasFirstEntry, DateTimeOffset occurredAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newIban);
        string compact = new([.. newIban.Where(c => !char.IsWhiteSpace(c))]);
        if (compact.Length < KeptDigits)
        {
            throw new ArgumentException("An IBAN is longer than its last four digits.", nameof(newIban));
        }

        return new IbanChangedNotice(actorName, compact[^KeptDigits..], wasFirstEntry, occurredAt);
    }
}

/// <summary>F-6, invitation: an administrator invitation was sent. <paramref name="InvitationId"/> is the dispatch row's related record.</summary>
public sealed record AdminInvitedNotice(Guid InvitationId, string ActorName, string InvitedEmail, DateTimeOffset OccurredAt) : AdminNotice(OccurredAt);

/// <summary>F-6, removal: an administrator was removed. <paramref name="RemovedAdminId"/> is the dispatch row's related record (a value, the account is gone).</summary>
public sealed record AdminRemovedNotice(Guid RemovedAdminId, string ActorName, string RemovedName, DateTimeOffset OccurredAt) : AdminNotice(OccurredAt);
