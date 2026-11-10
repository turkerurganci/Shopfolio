namespace Shopfolio.Modules.Notifications.Contracts;

/// <summary>
/// A customer notice whose latest row is "undelivered" (02 9.1.6; 06 4.58 "İşaretlerin ve uyarıların okunuşu"): the group's first row id (the group is the first row and its re-sendings),
/// the group's latest row - the one a re-sending names - and what the notice is. B-9 also names its withdrawal, termination or claim, whose row in the panel shows the mark too.
/// </summary>
public sealed record UndeliveredNotice(
    Guid GroupId,
    Guid LatestDispatchId,
    DispatchType Type,
    DispatchRelatedRecordType? RelatedRecordType,
    Guid? RelatedRecordId);

/// <summary>The panel's request to send a marked customer notice again (02 9.1.6; 07 UC-153).</summary>
/// <param name="OrderId">The order the notice belongs to.</param>
/// <param name="OrderNumber">The order's number, which the trail row names (02 10.3.1); never a customer's data.</param>
/// <param name="LatestDispatchId">The latest row of the notice's group, as the panel showed it.</param>
/// <param name="CurrentContactEmail">The order's current contact e-mail; B-15 ignores it - it goes to the address the group's own row names, the one before the correction.</param>
public sealed record NoticeResendRequest(Guid OrderId, string OrderNumber, Guid LatestDispatchId, string CurrentContactEmail);

/// <summary>
/// What a re-sending request came to. A call result, not a stored value: it is no enum (an enum of the Contracts folder is a stored one, with a data model table and a lock file) but a
/// value with three named members.
/// </summary>
public readonly record struct NoticeResendResult
{
    private NoticeResendResult(string name) => Name = name;

    /// <summary>The member's name; the default value has none and is none of the three.</summary>
    public string? Name { get; }

    /// <summary>A new "queued" row was written into the notice's group and the trail row with it; the ordinary queue sends it.</summary>
    public static NoticeResendResult Queued { get; } = new("Queued");

    /// <summary>The named row is not the latest row of a customer notice's group of this order, or that row is not "undelivered": nothing was written.</summary>
    public static NoticeResendResult NotMarked { get; } = new("NotMarked");

    /// <summary>The address to send to is empty (the order's personal data is gone, or no address was given): nothing was written.</summary>
    public static NoticeResendResult NoRecipient { get; } = new("NoRecipient");
}
