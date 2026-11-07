using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Status;

using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>The Notifications module's implementation of its open surface; internal, reached only through <see cref="INotificationsModule"/>.</summary>
internal sealed class NotificationsModule : INotificationsModule
{
    private readonly CustomerNoticeQueue _customerNotices;
    private readonly CompanyNoticeQueue _companyNotices;
    private readonly AccountEmailQueue _accountEmails;

    public NotificationsModule(CustomerNoticeQueue customerNotices, CompanyNoticeQueue companyNotices, AccountEmailQueue accountEmails)
    {
        ArgumentNullException.ThrowIfNull(customerNotices);
        ArgumentNullException.ThrowIfNull(companyNotices);
        ArgumentNullException.ThrowIfNull(accountEmails);
        _customerNotices = customerNotices;
        _companyNotices = companyNotices;
        _accountEmails = accountEmails;
    }

    public Task<int> QueueCustomerNoticeAsync(CustomerNotice notice, string recipientEmail, CancellationToken ct) =>
        _customerNotices.QueueAsync(notice, recipientEmail, ct);

    public Task<int> QueueCompanyNoticeAsync(CompanyNotice notice, CancellationToken ct) => _companyNotices.QueueCompanyAsync(notice, ct);

    public Task<int> QueueAdminNoticeAsync(AdminNotice notice, IReadOnlyCollection<string> adminEmails, CancellationToken ct) =>
        Task.FromResult(_companyNotices.QueueAdmins(notice, adminEmails));

    public Task<int> QueueAccountEmailAsync(AccountEmail email, CancellationToken ct) => Task.FromResult(_accountEmails.Queue(email));

    public Task<bool> QueueGoogleLinkConfirmationAsync(Guid customerId, string email, CancellationToken ct) => _accountEmails.QueueGoogleConfirmationAsync(customerId, email, ct);
}

/// <summary>The module's single registration (09 6 item 3): <c>Program.cs</c> calls <c>AddNotificationsModule</c> once.</summary>
internal static class NotificationsModuleRegistration
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
    {
        services.AddScoped<INotificationsModule, NotificationsModule>();

        // The start-up rewrite of the pending revert links' digests (05 6.x.20): hosted services start in registration order, so it is added
        // here, after the start-up checks, and the job queue and the e-mail sending are registered after it.
        services.AddScoped<RevertLinkDigestRewrite>();
        services.AddSingleton<RevertLinkDigestRewriteStartup>();
        services.AddHostedService(provider => provider.GetRequiredService<RevertLinkDigestRewriteStartup>());

        // The job queue takes no row before the step has run (D-96 K1; 09 15.1 item 4).
        services.AddJobStartGate<RevertLinkDigestRewriteGate>();

        // The e-mail common pattern (T305; 08 2.2.5; 09 11.4): the addresses and the composer that makes the subject, the HTML and the plain text part.
        services.AddSingleton<EmailLinks>();
        services.AddSingleton<EmailComposer>();

        // The customer notices B-1…B-8 (T306; 08 2.2.6): the writer of their dispatch rows, in the caller's transaction, and the templates of their bodies.
        services.AddSingleton(CarrierCatalog.Default);
        services.AddScoped<CustomerNoticeQueue>();
        services.AddSingleton<CustomerNoticeTemplates>();

        // The firm notices F-1…F-6 (T308; 08 2.2.7): the writer of their dispatch rows, in the caller's transaction; F-1…F-4 go to the firm's contact e-mail.
        services.AddScoped<ICompanyContactEmail, CompanyContactEmail>();
        services.AddScoped<CompanyNoticeQueue>();

        // The account and administration e-mails (T309; 08 2.2.8): the writer of their dispatch rows and the body made at the time of sending, which reads the link record first.
        services.AddScoped<AccountEmailQueue>();
        services.AddScoped<AccountEmailBodies>();

        // The per-recipient sending limit asked at a row's first attempt (08 2.2.4); its thinned security line keeps one window in memory.
        services.AddScoped<RecipientLimitCheck>();
        services.AddSingleton<RecipientLimitLog>();

        // The e-mail channel warning is a condition of the status endpoint (05 9.4.7).
        services.AddStatusCondition<ChannelWarningCondition>();
        return services;
    }
}
