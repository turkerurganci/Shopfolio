using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.CorporateContent.Contracts;

/// <summary>
/// One contact request as the panel's requests list shows it (T532; 04 9.10.4): the topic, the sender's name and e-mail, the status and the moment the request reached the firm, which is the list's
/// order key (04 9.10.6). The message and the phone stay in <see cref="ContactRequestDetail"/>.
/// </summary>
public sealed record PanelContactRequestRow(Guid Id, ContactTopic Topic, string Name, string Email, ContactRequestStatus Status, DateTimeOffset CreatedAt);

/// <summary>
/// One contact request as its detail screen shows it (T532; 04 9.11.3-9.11.5): the topic, the status, the sender, the whole message, the moment the request reached the firm (the moment a KVKK
/// application reached the firm, 03 8.6.4.3) and the last closing, from which the retention runs (06 4.57).
/// </summary>
public sealed record ContactRequestDetail(
    Guid Id,
    ContactTopic Topic,
    ContactRequestStatus Status,
    string Name,
    string Email,
    string? Phone,
    string Message,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastClosedAt);

/// <summary>What became of a close or a reopening of a contact request (T532; 07 UC-155, UC-156).</summary>
public enum ContactRequestChangeOutcome
{
    /// <summary>The status changed (and, for a closing, the closing moment was written).</summary>
    Changed,

    /// <summary>The request was already in the asked status (another administrator acted first); nothing was written (07 9.11.2 Idempotency).</summary>
    Unchanged,

    /// <summary>No such request - never there, or already destroyed at the end of its retention (K-1192).</summary>
    NotFound,
}

/// <summary>The result of a close or a reopening of a contact request; a request that is not there is a refusal that rolls the transaction back (09 9.1 item 3).</summary>
public sealed class ContactRequestChangeResult : IOperationResult
{
    public ContactRequestChangeResult(ContactRequestChangeOutcome outcome)
    {
        Outcome = outcome;
    }

    public ContactRequestChangeOutcome Outcome { get; }

    public bool IsRejected => Outcome == ContactRequestChangeOutcome.NotFound;
}
