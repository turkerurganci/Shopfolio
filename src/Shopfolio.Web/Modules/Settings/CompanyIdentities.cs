using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// The reads and the save of the company identity (02 3.1.1-3.1.4; 06 4.1). The company is the installation's one row: it is read with
/// <c>Single</c>, which fails loudly if the start data is gone, and there is no method here that adds or removes a row (the context's guard also
/// refuses a deletion). The save is the record variant (09 10.2 item 2) behind the settings row lock (05 7.3.5; K-1088).
/// </summary>
internal sealed class CompanyIdentities
{
    private readonly AppDbContext _db;
    private readonly CompanySettingsWriter _writer;
    private readonly CacheInvalidations _invalidations;

    public CompanyIdentities(AppDbContext db, CompanySettingsWriter writer, CacheInvalidations invalidations)
    {
        _db = db;
        _writer = writer;
        _invalidations = invalidations;
    }

    public async Task<CompanyIdentity> GetAsync(CancellationToken ct)
    {
        Company company = await _db.Companies.AsNoTracking().SingleAsync(ct);
        return new CompanyIdentity(
            company.Version,
            company.CompanyType,
            company.LegalName,
            company.TaxIdNumber,
            company.MersisNumber,
            company.TradeRegistryNumber,
            company.ProvinceId,
            company.District,
            company.AddressLine,
            company.Phone,
            company.Email,
            company.KepAddress,
            company.ChamberName,
            company.BusinessName,
            company.ProfessionalConductRules,
            company.EtbisVerificationUrl);
    }

    public async Task<CompanyIdentityCompleteness> GetCompletenessAsync(CancellationToken ct)
    {
        Company company = await _db.Companies.AsNoTracking().SingleAsync(ct);
        return new CompanyIdentityCompleteness(CompanyIdentityRules.MissingFields(company));
    }

    public async Task<LegalIdentity> GetLegalAsync(CancellationToken ct)
    {
        var row = await _db.Companies.AsNoTracking()
            .Select(c => new
            {
                Company = c,
                ProvinceName = _db.Provinces.Where(p => p.Id == c.ProvinceId).Select(p => p.Name).FirstOrDefault(),
            })
            .SingleAsync(ct);
        Company company = row.Company;
        IReadOnlyList<string> ownFields = CompanyIdentityRules.TypeFields(company.CompanyType);
        string? Own(string field, string? value) => ownFields.Contains(field) ? value : null;
        return new LegalIdentity(
            company.CompanyType,
            Own(nameof(CompanyIdentityInput.LegalName), company.LegalName),
            Own(nameof(CompanyIdentityInput.TaxIdNumber), company.TaxIdNumber),
            Own(nameof(CompanyIdentityInput.MersisNumber), company.MersisNumber),
            Own(nameof(CompanyIdentityInput.TradeRegistryNumber), company.TradeRegistryNumber),
            row.ProvinceName,
            company.District,
            company.AddressLine,
            company.Phone,
            company.Email,
            company.KepAddress,
            company.ChamberName,
            company.ProfessionalConductRules,
            company.BusinessName,
            company.EtbisVerificationUrl);
    }

    public async Task<SettingsSaveResult> SaveAsync(CompanyIdentityInput raw, int formVersion, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(raw);
        await _writer.LockAsync(ct);

        Company company = await _db.Companies.SingleAsync(ct);
        CompanyIdentityInput input = CompanyIdentityRules.ToStoredForm(raw);
        bool provinceExists = input.ProvinceId is not Guid province || await _db.Provinces.AnyAsync(p => p.Id == province, ct);
        IReadOnlyList<Contracts.FieldRejection> rejections = CompanyIdentityRules.Check(company, input, provinceExists);
        if (rejections.Count > 0)
        {
            return SettingsSaveResult.Refused(rejections);
        }

        RecordSaveResult saved = await RecordSave.SaveAsync(_db, company, formVersion, c => Apply(c, input), ct);
        if (!saved.IsSaved)
        {
            return SettingsSaveResult.VersionConflict;
        }

        _invalidations.Add(CacheArea.Settings);
        return SettingsSaveResult.Saved;
    }

    private static void Apply(Company company, CompanyIdentityInput input)
    {
        company.CompanyType = input.CompanyType;
        company.LegalName = input.LegalName;
        company.TaxIdNumber = input.TaxIdNumber;
        company.MersisNumber = input.MersisNumber;
        company.TradeRegistryNumber = input.TradeRegistryNumber;
        company.ProvinceId = input.ProvinceId;
        company.District = input.District;
        company.AddressLine = input.AddressLine;
        company.Phone = input.Phone;
        company.Email = input.Email;
        company.KepAddress = input.KepAddress;
        company.ChamberName = input.ChamberName;
        company.BusinessName = input.BusinessName;
        company.ProfessionalConductRules = input.ProfessionalConductRules;
        company.EtbisVerificationUrl = input.EtbisVerificationUrl;
    }
}
