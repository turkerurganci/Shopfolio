namespace Shopfolio.Modules.Notifications.Contracts;

/// <summary>
/// The open surface of the Notifications module (09 6 item 1; 05 3.4.1): what other modules, the pages and the commands may call. The module owns the dispatch records and the channel state (05 3.3.8).
/// The members arrive with the tasks that own the module's rules, as methods that take and give immutable records and join the caller's
/// database transaction (09 6 item 3).
/// </summary>
public interface INotificationsModule
{
    /// <summary>
    /// Queues the customer notice B-1…B-16 for <paramref name="recipientEmail"/> - the order's contact e-mail, the new address on a B-1 correction, the old address on B-15 (08 2.2.6; T306, T307):
    /// one dispatch row in the "queued" state, written in the caller's open transaction, which the method joins and never opens - the row commits with the event or
    /// goes with it. The row carries the order link and the event's values; the body is made when the row is sent. Returns the number of rows written: one, or
    /// zero for a B-3 that is not due (the window's reminder is already queued, or the window has run out). The caller holds the order's row lock (09 9.1 item 5).
    /// </summary>
    Task<int> QueueCustomerNoticeAsync(CustomerNotice notice, string recipientEmail, CancellationToken ct);

    /// <summary>
    /// Queues the firm notice F-1…F-4 for the firm's contact e-mail (08 2.2.7; T308): one dispatch row in the "queued" state, written in the caller's open
    /// transaction, which the method joins and never opens - the row commits with the event or goes with it. The row carries the event's values (the inner key
    /// of the panel screen, the order number, the amounts) and no customer data. Returns the number of rows written: zero while the firm has entered no
    /// contact e-mail yet (no recipient exists; the firm cannot sell before it has one).
    /// </summary>
    Task<int> QueueCompanyNoticeAsync(CompanyNotice notice, CancellationToken ct);

    /// <summary>
    /// Queues the notice F-5 or F-6 (08 2.2.7; T308): one separate row for every address in <paramref name="adminEmails"/> - the address of every administrator
    /// account, the one who acted and, on a removal, the removed one included; the caller (the Administration module, which owns the accounts) hands them in.
    /// Written in the caller's open transaction like <see cref="QueueCompanyNoticeAsync"/>. Returns the number of rows written.
    /// </summary>
    Task<int> QueueAdminNoticeAsync(AdminNotice notice, IReadOnlyCollection<string> adminEmails, CancellationToken ct);

    /// <summary>
    /// Queues one of the account and administration e-mails (08 2.2.8; T309) for the address the record names: one dispatch row in the "queued" state, written in the
    /// caller's open transaction, which the method joins and never opens. The link record of a link e-mail is made by the caller in the same transaction and named by
    /// id; the row carries that id and never the key. Returns the number of rows written (one). Refuses a record that breaks the rule of its kind.
    /// </summary>
    Task<int> QueueAccountEmailAsync(AccountEmail email, CancellationToken ct);

    /// <summary>
    /// Queues the Google sign-in confirmation e-mail for a customer account (08 2.2.8; K-1281, K-1282) and makes its link record in the caller's transaction - unless the
    /// account already has a valid confirmation link, when no link and no e-mail is made and the result is false. True means the link and the row were written.
    /// </summary>
    Task<bool> QueueGoogleLinkConfirmationAsync(Guid customerId, string email, CancellationToken ct);

    /// <summary>
    /// Whether the order carries the "e-posta ulaşmadı" mark (T385; 02 9.1.6; 06 4.58): the latest row of any customer notice group of the order is "undelivered". Read from the rows at every call, never stored;
    /// a firm notice or an account e-mail never puts it. A plain read; nothing is written.
    /// </summary>
    Task<bool> OrderHasUndeliveredMarkAsync(Guid orderId, CancellationToken ct);

    /// <summary>
    /// Whether a defect claim shows the mark on its own row in the panel (T385; 02 9.1.6; 06 4.58): its B-9 ends in an "undelivered" row. A withdrawal declaration or a late delivery termination
    /// shows the mark on the order's row only, and any other kind gives false. A plain read.
    /// </summary>
    Task<bool> ClaimHasUndeliveredMarkAsync(DispatchRelatedRecordType recordType, Guid recordId, CancellationToken ct);

    /// <summary>
    /// The defect claims among <paramref name="claimIds"/> that show the mark on their own row (T532; the panel's requests list): the same reading as <see cref="ClaimHasUndeliveredMarkAsync"/>, for all the claims
    /// of a page in one query. A claim with no mark is not in the answer. A plain read.
    /// </summary>
    Task<IReadOnlySet<Guid>> FindClaimsWithUndeliveredMarkAsync(IReadOnlyCollection<Guid> claimIds, CancellationToken ct);

    /// <summary>
    /// The administrator invitations among <paramref name="invitationIds"/> whose invitation e-mail carries the "e-posta ulaşmadı" mark (T522; 02 9.4; 04 9.20.4): the reading of <c>AccountEmailHasMarkAsync</c> for
    /// all the invitations of the panel's list in one query. An invitation with no mark is not in the answer. A plain read.
    /// </summary>
    Task<IReadOnlySet<Guid>> FindAdminInvitationsWithUndeliveredMarkAsync(IReadOnlyCollection<Guid> invitationIds, CancellationToken ct);

    /// <summary>The order's customer notices that carry the mark - what the order detail's re-send buttons stand for (T385; 02 9.1.6); oldest first. A plain read.</summary>
    Task<IReadOnlyList<UndeliveredNotice>> FindUndeliveredNoticesAsync(Guid orderId, CancellationToken ct);

    /// <summary>
    /// The order's customer notices, oldest first (T527; 04 9.9.12; K-804): every dispatch row bound to the order - a firm notice or an account e-mail has no order link and is never among
    /// them. A plain read.
    /// </summary>
    Task<IReadOnlyList<OrderNoticeLine>> ListOrderNoticesAsync(Guid orderId, CancellationToken ct);

    /// <summary>
    /// One page of the orders that carry the mark, by order id after <paramref name="afterOrderId"/> (null starts at the first): the filtered list the home page's "işaretli siparişler" row leads
    /// to (T385; 02 10.6.1; K-761). An empty page is the end. A plain read.
    /// </summary>
    Task<IReadOnlyList<Guid>> FindMarkedOrderIdsAsync(Guid? afterOrderId, int maxOrders, CancellationToken ct);

    /// <summary>
    /// The orders among <paramref name="orderIds"/> that carry the "e-posta ulaşmadı" mark (T527; 04 9.8.5): the same rule as the order's own mark, for a page of the panel's order list in one query.
    /// A plain read.
    /// </summary>
    Task<IReadOnlySet<Guid>> FindMarkedOrdersAmongAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken ct);

    /// <summary>
    /// Sends a marked customer notice again (T385; 02 9.1.6): writes a new "queued" row into the notice's group, which the ordinary queue sends, and the trail row of the re-sending, in the caller's open
    /// transaction, which the method joins and never opens. Nothing is written when the named row is not the latest row of an "undelivered" group of the order (<see cref="NoticeResendResult.NotMarked"/>).
    /// The order's row lock and the concurrent second request are the caller's (05 4.4.3).
    /// </summary>
    Task<NoticeResendResult> ResendCustomerNoticeAsync(NoticeResendRequest request, CancellationToken ct);
}
