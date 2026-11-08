namespace Shopfolio.Commands;

/// <summary>
/// The one static list of the management commands (09 13.2 item 2). Each task that builds a command adds it here; the <c>--help</c> list and the <c>05 8.5</c> table are equal when the
/// last command is added (T387).
/// </summary>
internal static class AdminCommandCatalog
{
    public static IReadOnlyList<IAdminCommand> All { get; } =
    [
        new CreateAdminCommand(),
        new EndSessionsCommand(),
        new InvalidatePasswordCommand(),
        new StatusCommand(),
    ];
}
