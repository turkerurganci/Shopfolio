using System.Globalization;

using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Membership.Contracts;

namespace Shopfolio.Commands;

/// <summary>
/// <c>reapply-deletions</c> (05 8.5.10, 7.4.5; 09 13.2 item 3): run after the database was returned to an earlier moment and before the application starts. It reads the instant deletion lines
/// of the application log from the restore moment to now - the log is a file outside the database and the restore did not take it back - and applies each one again by the removal path the
/// first deletion took, in the order the lines were written, each in its own transaction. A user's gates are not applied (the open revert link, the last administrator, the acting
/// administrator), a record that is not there already is skipped and no e-mail is queued. Then, in one transaction, it deletes every session record - sessions that came back with the restore
/// must not stay - and draws the lifetime of every unused account link to the restore moment, except the link that undoes an e-mail change (it cannot be asked for again). Running it twice is
/// harmless: the second run finds the records gone. A line that cannot be read, or of a kind the command does not know, is reported and the exit code is 3: the operator looks at the log by
/// hand. The command's line goes to its own log file before the work; it has no target account (the id is empty).
/// </summary>
internal sealed class ReapplyDeletionsCommand : IAdminCommand
{
    public const string CommandName = "reapply-deletions";

    public const string RestorePointOption = "restore-point";

    public string Name => CommandName;

    public string Description => "Veritabanı eski bir ana döndürüldükten sonra, uygulama başlamadan önce anlık silmeleri yeniden uygular, oturumları siler ve kullanılmamış hesap bağlantılarının ömrünü dönüş anına çeker.";

    public IReadOnlyList<CommandOption> Options { get; } =
    [
        new CommandOption(RestorePointOption, "Veritabanının döndürüldüğü an: ofsetli ISO 8601 an (2026-10-29T14:30:00Z ya da +03:00).", Required: true, CommandOptionKind.Moment),
    ];

    public async Task<CommandExit> ExecuteAsync(CommandInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        ITerminal terminal = input.Terminal;
        DateTimeOffset restorePoint = input.Moment(RestorePointOption) ?? throw new InvalidOperationException("The dispatcher gives a required option or ends with a usage error.");

        input.Services.GetRequiredService<IAuditLineWriter>().Write(AuditLine.AdminCommandWithoutTarget(CommandName));

        InstantDeletionReading reading = input.Services.GetRequiredService<InstantDeletionLog>().ReadSince(restorePoint);
        int applied = 0;
        int skipped = 0;
        int unknown = 0;
        foreach (InstantDeletionEntry entry in reading.Entries)
        {
            DeletionReapplyResult? result = await ReapplyAsync(input.Services, entry, ct);
            if (result is null)
            {
                unknown++;
                terminal.WriteErrorLine($"Bilinmeyen silme türü, uygulanmadı: {entry.Kind} {entry.RecordId}");
            }
            else if (result.WasApplied)
            {
                applied++;
            }
            else
            {
                skipped++;
            }
        }

        ReapplySweepResult sweep = await SweepAsync(input.Services, restorePoint, ct);

        terminal.WriteLine($"Yeniden uygulanan silme: {Number(applied)}; kaydı zaten olmayan, atlanan: {Number(skipped)}.");
        terminal.WriteLine($"Silinen oturum kaydı: {Number(sweep.Sessions)}; ömrü dönüş anına çekilen hesap bağlantısı: {Number(sweep.Links)}.");

        if (reading.UnreadableLines == 0 && unknown == 0)
        {
            return CommandExit.Success;
        }

        terminal.WriteErrorLine($"Okunamayan kayıt satırı: {Number(reading.UnreadableLines)}; bilinmeyen türde silme: {Number(unknown)}. Uygulama kaydı elle gözden geçirilmeli.");
        return CommandExit.Refused;
    }

    // One deletion in its own transaction, in a scope of its own: a failure rolls that deletion back and the exception goes to the dispatcher; the ones before it stay, and a second run skips them.
    private static async Task<DeletionReapplyResult?> ReapplyAsync(IServiceProvider services, InstantDeletionEntry entry, CancellationToken ct)
    {
        if (!InstantDeletionKinds.All.Contains(entry.Kind, StringComparer.Ordinal))
        {
            return null;
        }

        await using AsyncServiceScope scope = services.CreateAsyncScope();
        ITransactionRunner runner = scope.ServiceProvider.GetRequiredService<ITransactionRunner>();
        Guid recordId = entry.RecordId;
        if (string.Equals(entry.Kind, InstantDeletionKinds.CustomerAccount, StringComparison.Ordinal))
        {
            IMembershipModule membership = scope.ServiceProvider.GetRequiredService<IMembershipModule>();
            return await runner.RunAsync(cancel => membership.ReapplyAccountDeletionAsync(recordId, cancel), ct);
        }

        IAdministrationModule administration = scope.ServiceProvider.GetRequiredService<IAdministrationModule>();
        return await runner.RunAsync(cancel => administration.ReapplyAdminRemovalAsync(recordId, cancel), ct);
    }

    private static async Task<ReapplySweepResult> SweepAsync(IServiceProvider services, DateTimeOffset restorePoint, CancellationToken ct)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        ITransactionRunner runner = scope.ServiceProvider.GetRequiredService<ITransactionRunner>();
        SessionStore sessions = scope.ServiceProvider.GetRequiredService<SessionStore>();
        LinkRecords links = scope.ServiceProvider.GetRequiredService<LinkRecords>();
        return await runner.RunAsync(
            async cancel => new ReapplySweepResult(await sessions.EndEverySessionAsync(cancel), await links.EndUnusedAsync(restorePoint, cancel)),
            ct);
    }

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>What the sweep after the replay did: the session records deleted and the unused account links ended at the restore moment. It never refuses.</summary>
internal sealed record ReapplySweepResult(int Sessions, int Links) : IOperationResult
{
    public bool IsRejected => false;
}
