using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Status;
using Shopfolio.Infrastructure.Values;

using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>The Notifications module's implementation of its open surface; internal, reached only through <see cref="INotificationsModule"/>.</summary>
internal sealed class NotificationsModule : INotificationsModule
{
    private readonly CustomerNoticeQueue _customerNotices;
    private readonly CompanyNoticeQueue _companyNotices;
    private readonly AccountEmailQueue _accountEmails;
    private readonly DispatchResending _resending;
    private readonly AppDbContext _db;

    public NotificationsModule(
        CustomerNoticeQueue customerNotices,
        CompanyNoticeQueue companyNotices,
        AccountEmailQueue accountEmails,
        DispatchResending resending,
        AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(resending);
        ArgumentNullException.ThrowIfNull(db);
        _resending = resending;
        _db = db;
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

    public Task<bool> OrderHasUndeliveredMarkAsync(Guid orderId, CancellationToken ct) => DispatchMarks.HasMarkAsync(_db, orderId, ct);

    public Task<bool> ClaimHasUndeliveredMarkAsync(DispatchRelatedRecordType recordType, Guid recordId, CancellationToken ct) =>
        DispatchMarks.ClaimHasMarkAsync(_db, recordType, recordId, ct);

    public Task<IReadOnlyList<UndeliveredNotice>> FindUndeliveredNoticesAsync(Guid orderId, CancellationToken ct) => _resending.FindUndeliveredAsync(orderId, ct);

    public Task<IReadOnlyList<Guid>> FindMarkedOrderIdsAsync(Guid? afterOrderId, int maxOrders, CancellationToken ct) =>
        DispatchMarks.MarkedOrderIdsAsync(_db, afterOrderId, maxOrders, ct);

    public Task<NoticeResendResult> ResendCustomerNoticeAsync(NoticeResendRequest request, CancellationToken ct) => _resending.ResendAsync(request, ct);
}

/// <summary>The module's single registration (09 6 item 3): <c>Program.cs</c> calls <c>AddNotificationsModule</c> once.</summary>
internal static class NotificationsModuleRegistration
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
    {
        services.AddScoped<INotificationsModule, NotificationsModule>();
        services.AddScoped<IOrderContactNotices, OrderContactNotices>();
        services.AddScoped<IPanelWarnings, PanelWarningReader>();

        // The dispatch record's disposal (T302; 06 9.3.1): the rows past Z-34 or Z-43 and the unpaid arm's emptying; it asks the order's arm through the Ordering surface.
        services.AddDisposalPart<DispatchRecordDisposalPart>();

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

        // The customer notices B-1…B-16 (T306, T307; 08 2.2.6): the writer of their dispatch rows, in the caller's transaction, and the templates of their bodies.
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

        // The panel's re-sending of a marked customer notice (T385; 02 9.1.6): a new row in the notice's group and its trail row, in the caller's transaction.
        services.AddScoped<DispatchResending>();

        // The body context of a customer notice, read from the order when the row is sent (T385; D-232, D-293).
        services.AddScoped<OrderNoticeContexts>();

        // The e-mail queue is a scheduler source (T385; 05 5.1.1): the rows are sent, retried and given up on by their own time; it sends nothing while no gateway is registered (T407).
        services.AddDueRecordSource<DispatchRetrySource>();
        services.AddSingleton<RecipientLimitLog>();

        services.AddBounceQuery();

        // The e-mail channel warning is a condition of the status endpoint (05 9.4.7).
        services.AddStatusCondition<ChannelWarningCondition>();
        return services;
    }

    /// <summary>
    /// The returned-address query (T408; 08 2.2.3; 05 5.2.16): its handler on the job queue, and the scheduler source that keeps the type's one pending row in place. Both ask the
    /// container for the e-mail gateway, which is optional, so the registration holds in a host with none; a test that runs the job registers the gateway and calls this.
    /// </summary>
    internal static IServiceCollection AddBounceQuery(this IServiceCollection services)
    {
        services.AddJobHandler<BounceQueryJobHandler>(JobType.BounceQuery);
        services.AddDueRecordSource<BounceQueryRowSource>();
        return services;
    }
}
