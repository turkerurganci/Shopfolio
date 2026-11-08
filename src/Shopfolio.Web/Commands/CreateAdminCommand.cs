using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Modules.Administration.Contracts;

namespace Shopfolio.Commands;

/// <summary>
/// <c>create-admin</c> (05 8.5.2; 06 8.3.1; 10 4.1 ÖK-7): opens an administrator account. The name, the e-mail and the password come from the terminal only - never from an argument, the
/// environment or a pipe (09 13.2 item 4). The command's line is written to its own log file before the work, with the id of the account about to be opened and no name or address
/// (05 8.5.7). The account is made by the Administration module's Contracts interface in one transaction; the command writes no operation trail line and makes no notification.
/// </summary>
internal sealed class CreateAdminCommand : IAdminCommand
{
    public const string CommandName = "create-admin";

    public string Name => CommandName;

    public string Description => "Yönetici hesabı açar; ad, e-posta ve şifre terminalden sorulur.";

    public IReadOnlyList<CommandOption> Options { get; } = [];

    public async Task<CommandExit> RunAsync(CommandInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        ITerminal terminal = input.Terminal;
        TerminalPrompt prompt = new(terminal);

        // Nothing is read, written or opened before this look (09 13.2 item 4).
        if (!prompt.IsInteractive)
        {
            terminal.WriteErrorLine("Ad, e-posta ve şifre yalnız terminalden okunur; girdi yönlendirilmiş. Komutu etkileşimli bir terminalde çalıştırın (docker compose run, -T olmadan).");
            return CommandExit.UsageError;
        }

        string name = prompt.Ask("Ad: ").Trim();
        if (name.Length == 0)
        {
            terminal.WriteErrorLine("Ad boş olamaz.");
            return CommandExit.Refused;
        }

        string email = prompt.Ask("E-posta: ").Trim();
        string entered = prompt.AskSecret("Şifre: ");
        string repeated = prompt.AskSecret("Şifre (tekrar): ");
        if (!string.Equals(entered, repeated, StringComparison.Ordinal))
        {
            terminal.WriteErrorLine("İki şifre aynı değil.");
            return CommandExit.Refused;
        }

        // The line first (09 13.2 item 5): a failing write goes up to the dispatcher and no account is made.
        Guid adminId = EntityIds.New();
        input.Services.GetRequiredService<IAuditLineWriter>().Write(AuditLine.AdminCommand(CommandName, adminId));

        await using AsyncServiceScope scope = input.Services.CreateAsyncScope();
        ITransactionRunner runner = scope.ServiceProvider.GetRequiredService<ITransactionRunner>();
        IAdministrationModule administration = scope.ServiceProvider.GetRequiredService<IAdministrationModule>();
        AdminCreationResult result = await runner.RunAsync(token => administration.CreateAdminAsync(adminId, name, email, entered, token), ct);

        if (result.IsCreated)
        {
            terminal.WriteLine("Yönetici hesabı açıldı.");
            return CommandExit.Success;
        }

        terminal.WriteErrorLine(RefusalText(result));
        return CommandExit.Refused;
    }

    // The texts name the field and the rule, never a value (09 13.2 item 4).
    private static string RefusalText(AdminCreationResult result)
    {
        return (result.Field, result.Code) switch
        {
            (AdminCreationField.Name, _) => "Ad boş olamaz.",
            (AdminCreationField.Email, ErrorCode.FieldRequired) => "E-posta boş olamaz.",
            (AdminCreationField.Email, ErrorCode.InvalidEmail) => "E-posta adresinin biçimi geçersiz.",
            (AdminCreationField.Email, ErrorCode.EmailAlreadyRegistered) => "Bu e-posta ile bir yönetici hesabı zaten var.",
            (AdminCreationField.Password, ErrorCode.FieldRequired) => "Şifre boş olamaz.",
            (AdminCreationField.Password, _) => $"Şifre politikasına uymuyor: en az {ProductConstants.PasswordMinLength} karakter olmalı ve yaygın şifrelerden biri olmamalı.",
            _ => "Hesap açılamadı.",
        };
    }
}
