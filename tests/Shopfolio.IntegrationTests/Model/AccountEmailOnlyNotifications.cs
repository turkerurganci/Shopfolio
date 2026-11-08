using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>
/// The Notifications surface the Membership tests need (T343): the two account e-mail writers are the real ones (<see cref="AccountEmailQueue"/>, in the caller's
/// transaction), every other member is not used by the Membership module and fails if it is called.
/// </summary>
internal sealed class AccountEmailOnlyNotifications(AccountEmailQueue queue) : INotificationsModule
{
    public Task<int> QueueAccountEmailAsync(AccountEmail email, CancellationToken ct) => Task.FromResult(queue.Queue(email));

    public Task<bool> QueueGoogleLinkConfirmationAsync(Guid customerId, string email, CancellationToken ct) => queue.QueueGoogleConfirmationAsync(customerId, email, ct);

    public Task<int> QueueCustomerNoticeAsync(CustomerNotice notice, string recipientEmail, CancellationToken ct) => throw new NotSupportedException();

    public Task<int> QueueCompanyNoticeAsync(CompanyNotice notice, CancellationToken ct) => throw new NotSupportedException();

    public Task<int> QueueAdminNoticeAsync(AdminNotice notice, IReadOnlyCollection<string> adminEmails, CancellationToken ct) => throw new NotSupportedException();
}
