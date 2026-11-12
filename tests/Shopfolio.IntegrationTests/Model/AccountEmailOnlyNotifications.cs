using Shopfolio.Infrastructure;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>
/// The Notifications surface the Membership tests need (T343): the two account e-mail writers are the real ones (<see cref="AccountEmailQueue"/>, in the caller's
/// transaction), every other member is not used by the Membership module and fails if it is called.
/// </summary>
internal sealed class AccountEmailOnlyNotifications(AccountEmailQueue queue, AppDbContext db, TimeProvider time) : INotificationsModule
{
    // The administrators' F-6 notices (T379) are the real writer; it never reads the firm's contact e-mail, so a stub stands in for it.
    private readonly CompanyNoticeQueue _adminNotices = new(db, new NoContactEmail(), time);

    private sealed class NoContactEmail : ICompanyContactEmail
    {
        public Task<string?> GetAsync(CancellationToken ct) => Task.FromResult<string?>(null);
    }

    public Task<int> QueueAccountEmailAsync(AccountEmail email, CancellationToken ct) => Task.FromResult(queue.Queue(email));

    public Task<bool> QueueGoogleLinkConfirmationAsync(Guid customerId, string email, CancellationToken ct) => queue.QueueGoogleConfirmationAsync(customerId, email, ct);

    public Task<bool> OrderHasUndeliveredMarkAsync(Guid orderId, CancellationToken ct) => throw new NotSupportedException();

    public Task<bool> ClaimHasUndeliveredMarkAsync(DispatchRelatedRecordType recordType, Guid recordId, CancellationToken ct) => throw new NotSupportedException();
    public Task<IReadOnlySet<Guid>> FindClaimsWithUndeliveredMarkAsync(IReadOnlyCollection<Guid> claimIds, CancellationToken ct) => throw new NotSupportedException();

    public Task<IReadOnlySet<Guid>> FindAdminInvitationsWithUndeliveredMarkAsync(IReadOnlyCollection<Guid> invitationIds, CancellationToken ct) => throw new NotSupportedException();

    public Task<IReadOnlyList<UndeliveredNotice>> FindUndeliveredNoticesAsync(Guid orderId, CancellationToken ct) => throw new NotSupportedException();

    public Task<IReadOnlyList<Guid>> FindMarkedOrderIdsAsync(Guid? afterOrderId, int maxOrders, CancellationToken ct) => throw new NotSupportedException();

    public Task<NoticeResendResult> ResendCustomerNoticeAsync(NoticeResendRequest request, CancellationToken ct) => throw new NotSupportedException();

    public Task<int> QueueCustomerNoticeAsync(CustomerNotice notice, string recipientEmail, CancellationToken ct) => throw new NotSupportedException();

    public Task<int> QueueCompanyNoticeAsync(CompanyNotice notice, CancellationToken ct) => throw new NotSupportedException();

    public Task<int> QueueAdminNoticeAsync(AdminNotice notice, IReadOnlyCollection<string> adminEmails, CancellationToken ct) => Task.FromResult(_adminNotices.QueueAdmins(notice, adminEmails));
}
