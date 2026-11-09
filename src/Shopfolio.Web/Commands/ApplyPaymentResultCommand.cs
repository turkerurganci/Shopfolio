using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Commands;

/// <summary>
/// <c>apply-payment-result</c> (05 8.5.12, 8.5.7, 4.5.3; 02 5.5 Ö1; 08 2.1.2; K-1283; 09 13.2 items 2, 3, 7): after the provider's keys are renewed, the old transaction's final query may stay
/// unanswered and the card order keeps the "payment outcome unknown" mark. The installer reads the result on the provider's panel and gives the order number and that result: <c>paid</c> with the
/// panel's order amount (the instalment difference left out - 08 2.1.5's <c>payment_amount</c>) or <c>none</c> (no payment, or a failed one). Only a card order with the mark is accepted;
/// any other order is refused (3). A paid amount that is not equal to <c>Order.Total</c> in whole kuruş is refused (3) and nothing is written. Otherwise the result goes through the one
/// path every payment result takes: a payment to <see cref="IOrderTransitions.ApplyCardPaymentAsync"/> (Ödendi, B-4, delivery), "none" to the system's cancellation by the payment window's
/// rule (<see cref="IOrderTransitions.CancelOrderAsync"/>; S3 with Ö2, B-7). That path writes its transition record and queues its customer notice, which the running application's queue
/// sends; the command itself calls no provider, needs no provider secret, sends no e-mail and runs no job queue (05 8.5.7). The line goes to the command's own log file before the work
/// (09 13.2 item 5); the command names no account, so the id is the empty one (D-701).
/// </summary>
internal sealed class ApplyPaymentResultCommand : IAdminCommand
{
    public const string CommandName = "apply-payment-result";

    public const string OrderNumberOption = "order-number";

    public const string ResultOption = "result";

    public const string AmountOption = "amount";

    public const string PaidResult = "paid";

    public const string NoneResult = "none";

    public string Name => CommandName;

    public string Description => "\"Ödeme sonucu alınamadı\" işaretli kart siparişine sağlayıcının panelinde okunan ödeme sonucunu olağan yoldan uygular.";

    public IReadOnlyList<CommandOption> Options { get; } =
    [
        new CommandOption(OrderNumberOption, "Siparişin numarası (2026-7K4M9P).", Required: true),
        new CommandOption(ResultOption, $"Panelde okunan sonuç: {PaidResult} (başarılı ödeme) ya da {NoneResult} (ödeme yok ya da başarısız).", Required: true),
        new CommandOption(AmountOption, $"Panelin sipariş tutarı, taksit farkı hariç (noktalı ondalık, ör. 1250.50); yalnız {PaidResult} ile ve zorunlu.", Required: false, CommandOptionKind.Amount),
    ];

    public async Task<CommandExit> ExecuteAsync(CommandInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        ITerminal terminal = input.Terminal;
        string orderNumber = input.Text(OrderNumberOption) ?? throw new InvalidOperationException("The dispatcher gives a required option or ends with a usage error.");
        bool? paid = input.Text(ResultOption) switch
        {
            PaidResult => true,
            NoneResult => false,
            _ => null,
        };
        if (paid is not { } isPaid)
        {
            terminal.WriteErrorLine($"--{ResultOption} seçeneğinin değeri geçersiz: {PaidResult} ya da {NoneResult} bekleniyor.");
            return CommandExit.UsageError;
        }

        long? amountInKurus = input.AmountInKurus(AmountOption);
        if (isPaid != amountInKurus.HasValue)
        {
            terminal.WriteErrorLine(isPaid
                ? $"--{ResultOption} {PaidResult} ile --{AmountOption} zorunludur."
                : $"--{ResultOption} {NoneResult} ile --{AmountOption} verilmez.");
            return CommandExit.UsageError;
        }

        await using AsyncServiceScope scope = input.Services.CreateAsyncScope();
        IServiceProvider services = scope.ServiceProvider;
        DateTimeOffset now = services.GetRequiredService<TimeProvider>().GetUtcNow();
        InstallerPaymentTarget? target = await services.GetRequiredService<IOrderingModule>().FindInstallerPaymentTargetAsync(orderNumber, now, ct);

        // The run's line (05 8.5.12; 09 13.2 item 5): the order's internal id and the result as closed values, written after the read-only look-up and before any refusal or write.
        input.Services.GetRequiredService<IAuditLineWriter>().Write(
            AuditLine.PaymentResult(CommandName, target?.OrderId ?? Guid.Empty, isPaid ? InstallerPaymentResult.Paid : InstallerPaymentResult.None, amountInKurus));
        if (target is null)
        {
            terminal.WriteErrorLine("Bu numarayla bir sipariş yok.");
            return CommandExit.Refused;
        }

        if (!target.IsCard || !target.PaymentOutcomeUnknown)
        {
            terminal.WriteErrorLine("Sipariş \"ödeme sonucu alınamadı\" işaretli bir kart siparişi değil; komut yalnız o siparişte çalışır.");
            return CommandExit.Refused;
        }

        if (isPaid && amountInKurus != PaymentAmounts.ToKurus(target.Total))
        {
            terminal.WriteErrorLine("Paneldeki tutar siparişin toplamına eşit değil; sonuç uygulanmadı.");
            return CommandExit.Refused;
        }

        IOrderTransitions transitions = services.GetRequiredService<IOrderTransitions>();
        ITransactionRunner runner = services.GetRequiredService<ITransactionRunner>();
        if (isPaid)
        {
            CardPaymentSucceeded payment = new(target.OrderId, CardPaymentReference.OfOrderNumber(target.OrderNumber));
            PaymentTransitionResult applied = await runner.RunAsync(cancel => transitions.ApplyCardPaymentAsync(payment, cancel), ct);
            if (applied.IsRejected)
            {
                terminal.WriteErrorLine("Sipariş bu arada değişti; ödeme uygulanmadı.");
                return CommandExit.Refused;
            }

            terminal.WriteLine(applied.IsAlreadyApplied
                ? "Bu ödeme zaten uygulanmış; hiçbir şey yapılmadı."
                : "Ödeme olağan yoldan uygulandı; müşteri bildirimi çalışan uygulamanın kuyruğundan gider.");
            return CommandExit.Success;
        }

        OrderTransitionResult cancelled = await runner.RunAsync(
            cancel => transitions.CancelOrderAsync(CancelOrderRequest.BySystem(target.OrderId, TransitionReason.PaymentWindowExpired), cancel),
            ct);
        if (cancelled.IsRejected)
        {
            terminal.WriteErrorLine("Sipariş bu arada değişti; iptal uygulanmadı.");
            return CommandExit.Refused;
        }

        terminal.WriteLine("Sipariş olağan iptal yolundan iptal edildi; müşteri bildirimi çalışan uygulamanın kuyruğundan gider.");
        return CommandExit.Success;
    }
}
