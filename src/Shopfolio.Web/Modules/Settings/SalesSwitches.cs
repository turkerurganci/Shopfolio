using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// Closing sales temporarily and opening them again (02 3.1.6; 07 9.16.3, 9.16.4; 11 T331 criteria 4-7). The switch is the company's own: it stops only the adding to the cart and the
/// payment step (the gate reads it), never an open order - the transfer approval, the shipping and the legal texts already in force run on. The write is one update against
/// the value the screen showed and takes no lock (09 10.3 item 3); a changed or already asked value writes nothing. The trail row (<c>UpdateSettings</c>) is written in the same
/// transaction as the switch, by the administrator of the request, so a change without its row cannot happen.
/// </summary>
internal sealed class SalesSwitches
{
    private readonly AppDbContext _db;
    private readonly CompanySettingsWriter _writer;
    private readonly SalesGates _gates;
    private readonly IAuditTrail _trail;

    public SalesSwitches(AppDbContext db, CompanySettingsWriter writer, SalesGates gates, IAuditTrail trail)
    {
        _db = db;
        _writer = writer;
        _gates = gates;
        _trail = trail;
    }

    public Task<SalesSwitchResult> CloseAsync(bool shownIsClosed, CancellationToken ct) => ChangeAsync(shownIsClosed, closed: true, ct);

    public Task<SalesSwitchResult> ReopenAsync(bool shownIsClosed, CancellationToken ct) => ChangeAsync(shownIsClosed, closed: false, ct);

    private async Task<SalesSwitchResult> ChangeAsync(bool shownIsClosed, bool closed, CancellationToken ct)
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The sales switch is written only inside a transaction, with its trail row (02 10.3.1; 09 9.2 item 1).");
        }

        // Asked for the state it is shown in already: the request is a repeat (or a stale tab); nothing is written.
        if (shownIsClosed == closed || !await _writer.WriteSalesSwitchAsync(shownIsClosed, closed, ct))
        {
            SalesGate current = await _gates.GetAsync(ct);
            return SalesSwitchResult.Changed(current.UnmetConditions.Contains(SalesGateCondition.TemporaryClosure), current);
        }

        Guid rowId = await _db.CompanySettings.AsNoTracking().Select(s => s.Id).SingleAsync(ct);
        AuditDetails details = new(SwitchDisplayName, [new AuditValueChange(nameof(CompanySettings.IsSalesTemporarilyClosed), Text(shownIsClosed), Text(closed))]);
        await _trail.WriteAsync(new AuditEntry(AuditOperation.UpdateSettings, nameof(CompanySettings), rowId, details), ct);
        return SalesSwitchResult.Written(closed, await _gates.GetAsync(ct));
    }

    /// <summary>The name of the setting as the trail shows it (02 10.3.1 (6)).</summary>
    internal const string SwitchDisplayName = "Satışı geçici kapatma";

    private static string Text(bool value) => value ? "true" : "false";
}
