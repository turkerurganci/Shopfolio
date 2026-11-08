using System.Globalization;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Status;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Commands;

/// <summary>
/// <c>status</c> (05 8.5.6; 09 13.2, 13.3 item 5): the detail behind the status endpoint's one bit, written to the terminal. It writes the running version, every warning condition of the
/// endpoint - the open ones and the closed ones, the unreadable source among the open - the job queue's "failed" and due rows, what the provider's keys cannot be removed or changed
/// without (the open card jobs and the orders paid by card that could still cause a refund; 08 2.1.2, K-1287) and, for every registered large data conversion, the number of rows left.
/// It writes no data. Like every command it writes its line to its own log file first, with no target account. The second provider number is the installer's alone: it is read from
/// the orders and is in no counter of the panel (02 10.6.1).
/// </summary>
internal sealed class StatusCommand : IAdminCommand
{
    public const string CommandName = "status";

    public string Name => CommandName;

    public string Description => "Durum ucunun koşullarını, sürümü, iş kuyruğunu, sağlayıcı anahtarlarının şartlarını ve veri dönüşümlerinin kalanını yazar.";

    public IReadOnlyList<CommandOption> Options { get; } = [];

    public async Task<CommandExit> ExecuteAsync(CommandInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        ITerminal terminal = input.Terminal;
        input.Services.GetRequiredService<IAuditLineWriter>().Write(AuditLine.AdminCommandWithoutTarget(CommandName));

        await using AsyncServiceScope scope = input.Services.CreateAsyncScope();
        IServiceProvider services = scope.ServiceProvider;

        terminal.WriteLine("Sürüm: " + RunningVersion());
        terminal.WriteLine(string.Empty);

        await WriteConditionsAsync(terminal, services.GetRequiredService<StatusEvaluator>(), ct);
        await WriteJobQueueAsync(terminal, services.GetRequiredService<JobQueueStatusReader>(), ct);
        await WriteProviderKeysAsync(terminal, services, ct);
        await WriteConversionsAsync(terminal, services.GetServices<IDataConversion>(), ct);
        return CommandExit.Success;
    }

    private static string RunningVersion()
    {
        string? version = typeof(StatusCommand).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        return string.IsNullOrWhiteSpace(version) ? "bilinmiyor" : version;
    }

    private static async Task WriteConditionsAsync(ITerminal terminal, StatusEvaluator evaluator, CancellationToken ct)
    {
        IReadOnlyList<StatusConditionStanding> standings = await evaluator.StandingsAsync(ct);
        int open = standings.Count(s => s.IsOpen);
        terminal.WriteLine(open == 0
            ? "Durum ucu: 200 (açık uyarı yok; web sürecinin belleğindeki koşullar bu komutta ölçülemez)"
            : $"Durum ucu: 503 ({open} açık uyarı)");
        terminal.WriteLine("Koşullar:");
        foreach (StatusConditionStanding standing in standings)
        {
            string text = standing switch
            {
                { NotMeasurable: true } => "ölçülemez (web sürecinin belleğinde)",
                { SourceUnreadable: true } => "AÇIK (kaynak okunamadı)",
                { IsOpen: true } => "AÇIK",
                _ => "kapalı",
            };
            terminal.WriteLine($"  {standing.Name}: {text}");
        }

        terminal.WriteLine(string.Empty);
    }

    private static async Task WriteJobQueueAsync(ITerminal terminal, JobQueueStatusReader reader, CancellationToken ct)
    {
        JobQueueDetails queue = await reader.ReadAsync(ct);
        terminal.WriteLine("İş kuyruğu:");
        terminal.WriteLine($"  Başarısız iş satırı: {queue.FailedCount}" + (queue.FailedCount > 0 ? " (iş satırı komutuyla yeniden kuyruğa alınır ya da kapatılır)" : string.Empty));
        terminal.WriteLine($"  Zamanı gelmiş iş satırı: {queue.DueCount}" + (queue.OldestDueAt is { } oldest ? $" (en eskisinin zamanı {Moment(oldest)})" : string.Empty));
        terminal.WriteLine(string.Empty);
    }

    private static async Task WriteProviderKeysAsync(ITerminal terminal, IServiceProvider services, CancellationToken ct)
    {
        int awaiting = await services.GetRequiredService<IOrderingModule>().CountCardOrdersAwaitingPaymentAsync(ct);
        int refunds = await services.GetRequiredService<IPaymentModule>().CountOpenCardRefundsAsync(ct);
        int exposed = await services.GetRequiredService<IAfterSalesModule>().CountCardOrdersWithRefundExposureAsync(ct);

        terminal.WriteLine("Sağlayıcı anahtarları (kaldırmanın ve değiştirmenin şartları):");
        terminal.WriteLine($"  Açık kart işi: {awaiting + refunds} (ödemesi bekleyen kart siparişi {awaiting}, bitmemiş kart iadesi {refunds}); anahtar değişikliği 0 iken yapılır");
        terminal.WriteLine($"  Geri ödemesi doğabilecek kart siparişi: {exposed}; anahtarlar kaldırılmadan önce 0 olmalı");
        terminal.WriteLine(string.Empty);
    }

    private static async Task WriteConversionsAsync(ITerminal terminal, IEnumerable<IDataConversion> conversions, CancellationToken ct)
    {
        terminal.WriteLine("Veri dönüşümleri:");
        bool any = false;
        foreach (IDataConversion conversion in conversions)
        {
            any = true;
            long remaining = await conversion.CountRemainingAsync(ct);
            terminal.WriteLine($"  {conversion.Type}: kalan {remaining} satır");
        }

        if (!any)
        {
            terminal.WriteLine("  Kayıtlı dönüşüm yok.");
        }
    }

    private static string Moment(DateTimeOffset moment) => moment.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " UTC";
}
