using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Settings.Contracts;

using FieldRejection = Shopfolio.Modules.Settings.Contracts.FieldRejection;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// The company's delivery provinces (02 3.20.1; 06 4.2; 07 UC-169; T350): a row of <see cref="CompanyDeliveryProvince"/> says the province is selected, so the selection is a
/// subset of the closed list of 81 and the restriction is at province level only. The read gives the section or the <see cref="DeliveryArea"/>; the save follows the section order of
/// 09 10.3: the settings row lock, the check against the stored selection, the conditional update of this section's version (through <see cref="CompanySettingsWriter"/>, the one
/// writer of the settings row), the province rows deleted and added in the same transaction, and the trail row with the old and the new selection. A save that selects what is selected
/// writes nothing - no version step, no row, no trail row.
/// </summary>
internal sealed class DeliveryProvinces
{
    internal const string DisplayName = "Teslimat illeri";

    internal const string SelectionField = "DeliveryProvinces";

    private readonly AppDbContext _db;
    private readonly CompanySettingsWriter _writer;
    private readonly ProvinceList _provinces;
    private readonly IAuditTrail _trail;
    private readonly TimeProvider _time;

    public DeliveryProvinces(AppDbContext db, CompanySettingsWriter writer, ProvinceList provinces, IAuditTrail trail, TimeProvider time)
    {
        _db = db;
        _writer = writer;
        _provinces = provinces;
        _trail = trail;
        _time = time;
    }

    public async Task<DeliveryProvincesSection> ReadAsync(CancellationToken ct)
    {
        IReadOnlyList<ProvinceInfo> all = await _provinces.GetAllAsync(ct);
        HashSet<Guid> selected = await SelectedIdsAsync(ct);
        int version = await _db.CompanySettings.AsNoTracking().Select(s => s.DeliveryProvincesVersion).SingleAsync(ct);
        return new DeliveryProvincesSection([.. all.Where(p => selected.Contains(p.Id)).Select(p => p.Id)], version);
    }

    public async Task<DeliveryArea> GetAreaAsync(CancellationToken ct)
    {
        IReadOnlyList<ProvinceInfo> all = await _provinces.GetAllAsync(ct);
        HashSet<Guid> selected = await SelectedIdsAsync(ct);
        return new DeliveryArea([.. all.Where(p => selected.Contains(p.Id))], [.. all.Where(p => !selected.Contains(p.Id))]);
    }

    public async Task<SettingsSaveResult> SaveAsync(IReadOnlyCollection<Guid> provinceIds, int formVersion, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(provinceIds);
        await _writer.LockAsync(ct);

        IReadOnlyList<ProvinceInfo> all = await _provinces.GetAllAsync(ct);
        HashSet<Guid> listed = [.. all.Select(p => p.Id)];
        HashSet<Guid> wanted = [.. provinceIds];
        if (wanted.Any(id => !listed.Contains(id)))
        {
            return SettingsSaveResult.Refused([new FieldRejection(nameof(CompanyDeliveryProvince.ProvinceId), FieldCodes.InvalidProvince)]);
        }

        HashSet<Guid> stored = await SelectedIdsAsync(ct);
        int currentVersion = await _db.CompanySettings.AsNoTracking().Select(s => s.DeliveryProvincesVersion).SingleAsync(ct);
        if (stored.SetEquals(wanted))
        {
            return currentVersion == formVersion ? SettingsSaveResult.Saved : SettingsSaveResult.VersionConflict;
        }

        if (!await _writer.WriteDeliveryProvincesVersionAsync(formVersion, ct))
        {
            return SettingsSaveResult.VersionConflict;
        }

        Guid[] removed = [.. stored.Where(id => !wanted.Contains(id))];
        if (removed.Length > 0)
        {
            await _db.CompanyDeliveryProvinces.Where(p => removed.Contains(p.ProvinceId)).ExecuteDeleteAsync(ct);
        }

        DateTimeOffset now = _time.GetUtcNow();
        foreach (Guid id in wanted.Where(id => !stored.Contains(id)).Order())
        {
            _db.CompanyDeliveryProvinces.Add(CompanyDeliveryProvince.Create(id, now));
        }

        Guid rowId = await _db.CompanySettings.AsNoTracking().Select(s => s.Id).SingleAsync(ct);
        AuditValueChange change = new(SelectionField, Codes(all, stored), Codes(all, wanted));
        await _trail.WriteAsync(new AuditEntry(AuditOperation.UpdateSettings, nameof(CompanySettings), rowId, new AuditDetails(DisplayName, [change])), ct);
        return SettingsSaveResult.Saved;
    }

    private async Task<HashSet<Guid>> SelectedIdsAsync(CancellationToken ct)
    {
        return [.. await _db.CompanyDeliveryProvinces.AsNoTracking().Select(p => p.ProvinceId).ToListAsync(ct)];
    }

    /// <summary>The selection as plate codes in plate order - a code, never a free text (09 9.2).</summary>
    private static string Codes(IReadOnlyList<ProvinceInfo> all, HashSet<Guid> selected) => string.Join(',', all.Where(p => selected.Contains(p.Id)).Select(p => p.Code));
}
