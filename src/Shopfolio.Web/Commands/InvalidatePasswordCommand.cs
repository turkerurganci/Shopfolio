namespace Shopfolio.Commands;

/// <summary>
/// <c>invalidate-password</c> (05 8.5.4; 06 4.37): makes the password of the administrator account the e-mail names unusable and deletes all the account's sessions in the same database
/// transaction; the account opens again only through the password reset. The e-mail comes from the terminal only; the command's line carries the account's internal id (or "no target")
/// and is written before the work (09 13.2 item 5). In the recovery of a taken-over account it comes after <c>end-sessions</c> (05 6.9.2): a session opened between the two commands falls
/// here too. The command writes no operation trail line and makes no notification (05 8.5.7).
/// </summary>
internal sealed class InvalidatePasswordCommand : IAdminCommand
{
    public const string CommandName = "invalidate-password";

    public string Name => CommandName;

    public string Description => "Yönetici hesabının şifresini kullanılamaz kılar ve oturumlarını siler; hesabın e-postası terminalden sorulur.";

    public IReadOnlyList<CommandOption> Options { get; } = [];

    public Task<CommandExit> ExecuteAsync(CommandInput input, CancellationToken ct) =>
        AccountTargetFlow.RunAsync(input, CommandName, (administration, adminId, token) => administration.InvalidateAdminPasswordAsync(adminId, token), "Hesabın şifresi kullanılamaz kılındı ve oturumları silindi; hesap yalnız şifre sıfırlamayla açılır.", ct);
}
