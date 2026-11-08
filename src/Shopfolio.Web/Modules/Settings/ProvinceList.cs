using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// The read of the closed province list (02 3.14.3; 06 4.3): the one place other modules get the provinces from, so an address form's list and the check of an
/// address's province are the same list. Only reads; the list is start data, never written by the application.
/// </summary>
internal sealed class ProvinceList
{
    private readonly AppDbContext _db;

    public ProvinceList(AppDbContext db)
    {
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
}
