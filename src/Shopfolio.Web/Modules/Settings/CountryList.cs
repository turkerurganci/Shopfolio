using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>The read of the closed country list (06 4.4; D-344): the one place other modules and the pages get a country's name and code from. Only reads; the list is start data.</summary>
internal sealed class CountryList : ICountryList
{
    private readonly AppDbContext _db;

    public CountryList(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    public Task<CountryInfo?> FindAsync(Guid countryId, CancellationToken ct) =>
        _db.Countries.AsNoTracking().Where(c => c.Id == countryId).Select(c => new CountryInfo(c.Id, c.Code, c.Name)).SingleOrDefaultAsync(ct);

    public async Task<IReadOnlyList<CountryInfo>> ListAsync(CancellationToken ct) =>
        await _db.Countries.AsNoTracking().OrderBy(c => c.Name).ThenBy(c => c.Code).Select(c => new CountryInfo(c.Id, c.Code, c.Name)).ToListAsync(ct);
}
