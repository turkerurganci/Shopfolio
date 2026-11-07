namespace Shopfolio.Modules.Notifications.Contracts;

/// <summary>
/// The open surface of the Notifications module (09 6 item 1; 05 3.4.1): what other modules, the pages and the commands may call. The module owns the dispatch records and the channel state (05 3.3.8).
/// The members arrive with the tasks that own the module's rules, as methods that take and give immutable records and join the caller's
/// database transaction (09 6 item 3).
/// </summary>
public interface INotificationsModule
{
    /// <summary>
    /// Queues the customer notice B-1…B-8 for <paramref name="recipientEmail"/> - the order's contact e-mail, the new address on a B-1 correction (08 2.2.6; T306):
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
}
