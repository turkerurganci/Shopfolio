using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.Commands;

/// <summary>
/// <c>job</c> (05 8.5.9, 4.3.4, 9.4.16; 09 13.2 items 3, 7): requeues or closes a "failed" job row; the job queue condition of the status endpoint goes only this way. A row that is not failed,
/// or is gone, is refused (3); a requeue while the same work is already pending is refused too - the operator closes the failed row instead. The command does not run the row: the running
/// application's runner takes a requeued row (05 8.5.7). The line goes to the command's own log file before the work; the command has no target account, so the id is the empty one (09 13.2
/// item 5; D-701).
/// </summary>
internal sealed class JobCommand : IAdminCommand
{
    public const string CommandName = "job";

    public const string IdOption = "id";

    public const string ActionOption = "action";

    public const string RequeueAction = "requeue";

    public const string CloseAction = "close";

    public string Name => CommandName;

    public string Description => "\"Başarısız\" iş satırını yeniden kuyruğa alır ya da kapatır.";

    public IReadOnlyList<CommandOption> Options { get; } =
    [
        new CommandOption(IdOption, "İş satırının kimliği (durum komutunun yazdığı kimlik).", Required: true),
        new CommandOption(ActionOption, $"{RequeueAction} (yeniden kuyruğa al) ya da {CloseAction} (kapat).", Required: true),
    ];

    public async Task<CommandExit> ExecuteAsync(CommandInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        ITerminal terminal = input.Terminal;
        if (!Guid.TryParseExact(input.Text(IdOption), "D", out Guid jobId))
        {
            terminal.WriteErrorLine($"--{IdOption} seçeneğinin değeri geçersiz: iş satırının kimliği bekleniyor (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).");
            return CommandExit.UsageError;
        }

        FailedJobAction? action = input.Text(ActionOption) switch
        {
            RequeueAction => FailedJobAction.Requeue,
            CloseAction => FailedJobAction.Close,
            _ => null,
        };
        if (action is not { } chosen)
        {
            terminal.WriteErrorLine($"--{ActionOption} seçeneğinin değeri geçersiz: {RequeueAction} ya da {CloseAction} bekleniyor.");
            return CommandExit.UsageError;
        }

        input.Services.GetRequiredService<IAuditLineWriter>().Write(AuditLine.AdminCommandWithoutTarget(CommandName));

        await using AsyncServiceScope scope = input.Services.CreateAsyncScope();
        FailedJobRows rows = scope.ServiceProvider.GetRequiredService<FailedJobRows>();
        FailedJobResult result = await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(cancel => rows.ApplyAsync(jobId, chosen, cancel), ct);
        switch (result.Refusal)
        {
            case FailedJobRefusal.None:
                terminal.WriteLine(chosen == FailedJobAction.Requeue
                    ? "İş satırı yeniden kuyruğa alındı; çalışan uygulamanın iş kuyruğu onu alır."
                    : "İş satırı kapatıldı.");
                return CommandExit.Success;
            case FailedJobRefusal.NotFound:
                terminal.WriteErrorLine("Bu kimlikle bir iş satırı yok.");
                return CommandExit.Refused;
            case FailedJobRefusal.NotFailed:
                terminal.WriteErrorLine("İş satırı \"başarısız\" değil; yalnız başarısız satır yeniden kuyruğa alınır ya da kapatılır.");
                return CommandExit.Refused;
            default:
                terminal.WriteErrorLine($"Aynı işin bekleyen bir satırı kuyrukta var; başarısız satır yeniden kuyruğa alınmaz, --{ActionOption} {CloseAction} ile kapatılır.");
                return CommandExit.Refused;
        }
    }
}
