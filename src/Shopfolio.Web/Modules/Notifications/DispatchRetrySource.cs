using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// The e-mail queue as a scheduler source (T385; 05 5.1.1, 5.2.1, 4.4.3; 09 13.1 item 6): a dispatch row's own time - its first attempt when it is written, then the
/// <c>NextAttemptAt</c> the retry rule sets (5 minutes, 30 minutes and 2 hours after the first attempt, <see cref="DispatchRetrySchedule"/>) - is what the scheduler orders it by; there is no
/// job calendar and no job row (09 13.1; D-97). The source does not wait for a fresh heartbeat: a row that fell due during an outage is sent on the return in its turn, and the outage
/// writes nothing else (08 3.3). The source is a singleton and builds the sender in the scope it is given; it opens no transaction around the call - the gateway is called outside
/// any (09 10.7 item 5) and <see cref="DispatchAttempts"/> writes the result in its own. While no <see cref="IEmailGateway"/> is registered (the service's gateway is T407's; development
/// has the capture one) there is nothing to send with: no row is due and the queue keeps its rows.
/// </summary>
internal sealed class DispatchRetrySource : IDueRecordSource
{
    public string Name => "dispatch-queue";

    public bool WaitsForFreshHeartbeat => false;

    public async Task<IReadOnlyList<DueRecord>> FindDueAsync(IServiceProvider services, DateTimeOffset now, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (services.GetService<IEmailGateway>() is null)
        {
            return [];
        }

        return await DispatchSender.FindDueAsync(services.GetRequiredService<AppDbContext>(), now, ct);
    }

    public async Task ProcessAsync(IServiceProvider services, Guid recordId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (services.GetService<IEmailGateway>() is not { } gateway)
        {
            return;
        }

        AppDbContext db = services.GetRequiredService<AppDbContext>();
        ITransactionRunner transactions = services.GetRequiredService<ITransactionRunner>();
        TimeProvider time = services.GetRequiredService<TimeProvider>();
        DispatchAttempts attempts = new(db, transactions, gateway, time);
        DispatchSender sender = new(
            db,
            transactions,
            time,
            attempts,
            services.GetRequiredService<RecipientLimitCheck>(),
            services.GetRequiredService<AccountEmailBodies>(),
            services.GetRequiredService<EmailComposer>(),
            services.GetRequiredService<EmailLinks>(),
            services.GetRequiredService<ISettingsModule>(),
            services.GetRequiredService<ICorporateContentModule>());
        await sender.SendAsync(recordId, ct);
    }
}
