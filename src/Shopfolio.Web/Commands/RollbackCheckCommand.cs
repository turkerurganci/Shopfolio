using System.Globalization;

using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Backup;
using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.Commands;

/// <summary>
/// <c>rollback-check</c> (05 8.5.11, 8.7.4; K-962, K-1096; 09 13.2 item 3): run before the database is returned to an earlier moment, with the application stopped. It only reads. It counts, in
/// every table of the order side that <c>02 3.34.8</c> names and in the inbound notification record, the rows that transactions committed after the restore point added or changed - the
/// measure is the transaction's commit moment, not the row's creation time (<see cref="RollbackAudit"/>). A count above zero means the database is not returned and the command ends with 3;
/// zero ends with 0. The restore point is an instant with its offset: a date alone or an instant with no offset is a usage error (2) the dispatcher already gives (09 13.2 item 3). The
/// command's line goes to its own log file before the work; it has no target account (the id is empty) and no value of the data (05 8.5.7).
/// </summary>
internal sealed class RollbackCheckCommand : IAdminCommand
{
    public const string CommandName = "rollback-check";

    public const string RestorePointOption = "restore-point";

    public string Name => CommandName;

    public string Description => "Veritabanını eski bir ana döndürmeden önce sipariş tarafında dönüş noktasından sonra yazılmış kaydı sayar; yalnız okur, uygulama dururken çalıştırılır.";

    public IReadOnlyList<CommandOption> Options { get; } =
    [
        new CommandOption(RestorePointOption, "Dönüş noktası: ofsetli ISO 8601 an (2026-10-29T14:30:00Z ya da +03:00).", Required: true, CommandOptionKind.Moment),
    ];

    public async Task<CommandExit> ExecuteAsync(CommandInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        ITerminal terminal = input.Terminal;
        DateTimeOffset restorePoint = input.Moment(RestorePointOption) ?? throw new InvalidOperationException("The dispatcher gives a required option or ends with a usage error.");

        input.Services.GetRequiredService<IAuditLineWriter>().Write(AuditLine.AdminCommand(CommandName, Guid.Empty));

        await using AsyncServiceScope scope = input.Services.CreateAsyncScope();
        RollbackCount count = await scope.ServiceProvider.GetRequiredService<RollbackAudit>().CountAfterAsync(restorePoint, ct);
        if (!count.CommitMomentsRecorded)
        {
            terminal.WriteErrorLine("Veritabanı işlemlerin kesinleşme anını tutmuyor (track_commit_timestamp kapalı): sayım yapılamaz, dönülmez.");
            return CommandExit.Refused;
        }

        foreach (RollbackTableCount table in count.Tables)
        {
            terminal.WriteLine($"{table.Table}: {table.Rows.ToString(CultureInfo.InvariantCulture)}");
        }

        if (count.AllowsRollback)
        {
            terminal.WriteLine("Dönüş noktasından sonra sipariş tarafında yazılmış kayıt yok: dönülebilir.");
            return CommandExit.Success;
        }

        terminal.WriteErrorLine($"Dönüş noktasından sonra sipariş tarafında {count.Total.ToString(CultureInfo.InvariantCulture)} kayıt yazılmış: dönülmez.");
        return CommandExit.Refused;
    }
}
