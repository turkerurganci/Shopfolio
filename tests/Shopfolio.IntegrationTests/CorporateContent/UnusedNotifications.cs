using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.IntegrationTests.CorporateContent;

/// <summary>
/// The Notifications surface for the harnesses that build the CorporateContent module but never send a contact request (T365): the module's contact request
/// writer asks for it when the module is resolved, and every member fails if it is called.
/// </summary>
internal sealed class UnusedNotifications : INotificationsModule
{
    public Task<int> QueueCustomerNoticeAsync(CustomerNotice notice, string recipientEmail, CancellationToken ct) => throw new NotSupportedException();

    public Task<int> QueueCompanyNoticeAsync(CompanyNotice notice, CancellationToken ct) => throw new NotSupportedException();

    public Task<int> QueueAdminNoticeAsync(AdminNotice notice, IReadOnlyCollection<string> adminEmails, CancellationToken ct) => throw new NotSupportedException();

    public Task<int> QueueAccountEmailAsync(AccountEmail email, CancellationToken ct) => throw new NotSupportedException();

    public Task<bool> QueueGoogleLinkConfirmationAsync(Guid customerId, string email, CancellationToken ct) => throw new NotSupportedException();

    public Task<bool> OrderHasUndeliveredMarkAsync(Guid orderId, CancellationToken ct) => throw new NotSupportedException();

    public Task<bool> ClaimHasUndeliveredMarkAsync(DispatchRelatedRecordType recordType, Guid recordId, CancellationToken ct) => throw new NotSupportedException();

    public Task<IReadOnlyList<UndeliveredNotice>> FindUndeliveredNoticesAsync(Guid orderId, CancellationToken ct) => throw new NotSupportedException();

    public Task<IReadOnlyList<Guid>> FindMarkedOrderIdsAsync(Guid? afterOrderId, int maxOrders, CancellationToken ct) => throw new NotSupportedException();

    public Task<NoticeResendResult> ResendCustomerNoticeAsync(NoticeResendRequest request, CancellationToken ct) => throw new NotSupportedException();
}
