using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// The read of the closed province list (02 3.14.3; 06 4.3): the one place other modules get the provinces from, so an address form's list and the check of an
/// address's province are the same list. Only reads (<see cref="IProvinceList"/>); the list is start data, never written by the application or a form.
/// </summary>
internal sealed class ProvinceList : IProvinceList
{
    private readonly AppDbContext _db;

    public ProvinceList(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    /// <summary>Every province, in plate-code order (the code is unique, so the order is fixed).</summary>
    public async Task<IReadOnlyList<ProvinceInfo>> GetAllAsync(CancellationToken ct)
    {
        return await _db.Provinces.AsNoTracking()
            .OrderBy(p => p.Code)
            .Select(p => new ProvinceInfo(p.Id, p.Code, p.Name))
            .ToListAsync(ct);
    }

    public Task<bool> ExistsAsync(Guid provinceId, CancellationToken ct) => _db.Provinces.AsNoTracking().AnyAsync(p => p.Id == provinceId, ct);

    public async Task<IReadOnlyDictionary<Guid, string>> GetNamesAsync(IReadOnlyCollection<Guid> provinceIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(provinceIds);
        if (provinceIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        return await _db.Provinces.AsNoTracking().Where(p => provinceIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Name, ct);
    }
}
