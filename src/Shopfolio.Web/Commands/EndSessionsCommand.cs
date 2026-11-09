namespace Shopfolio.Commands;

/// <summary>
/// <c>end-sessions</c> (05 8.5.3; 06 6.1.2): deletes every session record of the administrator account the e-mail names. The e-mail comes from the terminal only; the command's line carries the
/// account's internal id (or "no target" when there is no such account) and is written before the work (09 13.2 item 5). The command writes no operation trail line and makes no notification
/// (05 8.5.7).
/// </summary>
internal sealed class EndSessionsCommand : IAdminCommand
{
    public const string CommandName = "end-sessions";

    public string Name => CommandName;

    public string Description => "Yönetici hesabının bütün oturumlarını sonlandırır; hesabın e-postası terminalden sorulur.";

    public IReadOnlyList<CommandOption> Options { get; } = [];

    public Task<CommandExit> ExecuteAsync(CommandInput input, CancellationToken ct) =>
        AccountTargetFlow.RunAsync(input, CommandName, (administration, adminId, token) => administration.EndAdminSessionsAsync(adminId, token), "Hesabın bütün oturumları sonlandırıldı.", ct);
}
