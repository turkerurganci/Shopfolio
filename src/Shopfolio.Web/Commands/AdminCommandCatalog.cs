namespace Shopfolio.Commands;

/// <summary>
/// The one static list of the management commands (09 13.2 item 2), in the order of the <c>05 8.5</c> table. The <c>--help</c> list and the table are equal (T387; a test reads both
/// ways).
/// </summary>
internal static class AdminCommandCatalog
{
    public static IReadOnlyList<IAdminCommand> All { get; } =
    [
        new CreateAdminCommand(),
        new EndSessionsCommand(),
        new InvalidatePasswordCommand(),
        new ReadLogsCommand(),
        new StatusCommand(),
        new JobCommand(),
        new ReapplyDeletionsCommand(),
        new RollbackCheckCommand(),
        new ApplyPaymentResultCommand(),
    ];
}
