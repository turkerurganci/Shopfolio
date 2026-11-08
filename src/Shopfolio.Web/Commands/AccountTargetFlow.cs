using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Modules.Administration.Contracts;

namespace Shopfolio.Commands;

/// <summary>
/// The flow the commands that name an administrator account by its e-mail share (<c>end-sessions</c>, <c>invalidate-password</c>; 05 8.5.3, 8.5.4; 09 13.2 item 5): the e-mail comes from the
/// terminal only, the account's internal id is found, the command's line is written to its own log file with that id - or with the empty id, "no target", when there is no such account -
/// and only then the work is done in one transaction. The e-mail and the name never reach the line, the terminal output or the exit text.
/// </summary>
internal static class AccountTargetFlow
{
    /// <param name="input">The command's input.</param>
    /// <param name="commandName">The command's name, written to the line.</param>
    /// <param name="work">The work on the found account, inside the transaction; refused when the account is gone by then.</param>
    /// <param name="doneText">What is printed when the work is done (no value in it).</param>
    /// <param name="ct">Cancellation.</param>
    public static async Task<CommandExit> RunAsync(
        CommandInput input,
        string commandName,
        Func<IAdministrationModule, Guid, CancellationToken, Task<AdminAccountWorkResult>> work,
        string doneText,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);
        ArgumentNullException.ThrowIfNull(work);
        ITerminal terminal = input.Terminal;
        TerminalPrompt prompt = new(terminal);

        // Nothing is read, written or opened before this look (09 13.2 item 4).
        if (!prompt.IsInteractive)
        {
            terminal.WriteErrorLine("Hesabın e-postası yalnız terminalden okunur; girdi yönlendirilmiş. Komutu etkileşimli bir terminalde çalıştırın (docker compose run, -T olmadan).");
            return CommandExit.UsageError;
        }

        string email = prompt.Ask("Hesabın e-postası: ").Trim();

        await using AsyncServiceScope scope = input.Services.CreateAsyncScope();
        IAdministrationModule administration = scope.ServiceProvider.GetRequiredService<IAdministrationModule>();
        IAuditLineWriter writer = input.Services.GetRequiredService<IAuditLineWriter>();

        // The order of 09 13.2 item 5: the e-mail from the terminal, the account's internal id, the line, the work. A line that cannot be written goes up to the dispatcher and nothing is done.
        Guid? adminId = await administration.FindAdminIdAsync(email, ct);
        if (adminId is not { } id)
        {
            writer.Write(AuditLine.AdminCommandWithoutTarget(commandName));
            terminal.WriteErrorLine("Bu e-postayla bir yönetici hesabı yok.");
            return CommandExit.Refused;
        }

        writer.Write(AuditLine.AdminCommand(commandName, id));

        ITransactionRunner runner = scope.ServiceProvider.GetRequiredService<ITransactionRunner>();
        AdminAccountWorkResult result = await runner.RunAsync(token => work(administration, id, token), ct);
        if (!result.IsDone)
        {
            terminal.WriteErrorLine("Hesap bu arada kaldırılmış; hiçbir şey yapılmadı.");
            return CommandExit.Refused;
        }

        terminal.WriteLine(doneText);
        return CommandExit.Success;
    }
}
