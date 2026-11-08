using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>The province list's open reads (<see cref="IProvinceList"/>); the table is a start-up data table that is never written by a form (06 4.3).</summary>
internal sealed class ProvinceList : IProvinceList
{
    private readonly AppDbContext _db;

    public ProvinceList(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
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
