using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// The reads and the save of the company identity (02 3.1.1-3.1.4; 06 4.1). The company is the installation's one row: it is read with
/// <c>Single</c>, which fails loudly if the start data is gone, and there is no method here that adds or removes a row (the context's guard also
/// refuses a deletion). The save is the record variant (09 10.2 item 2) behind the settings row lock (05 7.3.5; K-1088).
/// </summary>
internal sealed class CompanyIdentities
{
    /// <summary>The identity as the trail names it (02 10.3.1 (1)).</summary>
    internal const string IdentityDisplayName = "Firma kimliği";

    private readonly AppDbContext _db;
    private readonly CompanySettingsWriter _writer;
    private readonly CacheInvalidations _invalidations;
    private readonly IAuditTrail _trail;

    public CompanyIdentities(AppDbContext db, CompanySettingsWriter writer, CacheInvalidations invalidations, IAuditTrail trail)
    {
        _db = db;
        _writer = writer;
        _invalidations = invalidations;
        _trail = trail;
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

        Dictionary<string, string?> before = await ValuesOfAsync(company.CompanyType, company.LegalName, company.TaxIdNumber, company.MersisNumber, company.TradeRegistryNumber, company.ProvinceId, company.District, company.AddressLine, company.Phone, company.Email, company.KepAddress, company.ChamberName, company.BusinessName, company.ProfessionalConductRules, company.EtbisVerificationUrl, ct);
        Dictionary<string, string?> after = await ValuesOfAsync(input.CompanyType, input.LegalName, input.TaxIdNumber, input.MersisNumber, input.TradeRegistryNumber, input.ProvinceId, input.District, input.AddressLine, input.Phone, input.Email, input.KepAddress, input.ChamberName, input.BusinessName, input.ProfessionalConductRules, input.EtbisVerificationUrl, ct);
        RecordSaveResult saved = await RecordSave.SaveAsync(_db, company, formVersion, c => Apply(c, input), ct);
        if (!saved.IsSaved)
        {
            return SettingsSaveResult.VersionConflict;
        }

        _invalidations.Add(CacheArea.Settings);

        // Every changed field goes to the trail with its old and new value, in the same transaction (02 10.3.1 (1); 07 UC-164); a save that changes nothing writes no row.
        List<AuditValueChange> changes = [.. after.Where(pair => !string.Equals(before[pair.Key], pair.Value, StringComparison.Ordinal)).Select(pair => new AuditValueChange(pair.Key, before[pair.Key], pair.Value))];
        if (changes.Count > 0)
        {
            await _trail.WriteAsync(new AuditEntry(AuditOperation.UpdateCompany, nameof(Company), company.Id, new AuditDetails(IdentityDisplayName, changes)), ct);
        }

        return SettingsSaveResult.Saved;
    }

    /// <summary>The identity's values as the trail shows them, keyed by the members of the identity input; the province by its name.</summary>
    private async Task<Dictionary<string, string?>> ValuesOfAsync(
        CompanyType? type,
        string? legalName,
        string? taxIdNumber,
        string? mersisNumber,
        string? tradeRegistryNumber,
        Guid? provinceId,
        string? district,
        string? addressLine,
        string? phone,
        string? email,
        string? kepAddress,
        string? chamberName,
        string? businessName,
        string? professionalConductRules,
        string? etbisVerificationUrl,
        CancellationToken ct)
    {
        string? province = provinceId is Guid id ? await _db.Provinces.AsNoTracking().Where(p => p.Id == id).Select(p => p.Name).FirstOrDefaultAsync(ct) : null;
        return new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            [nameof(CompanyIdentityInput.CompanyType)] = type?.ToString(),
            [nameof(CompanyIdentityInput.LegalName)] = legalName,
            [nameof(CompanyIdentityInput.TaxIdNumber)] = taxIdNumber,
            [nameof(CompanyIdentityInput.MersisNumber)] = mersisNumber,
            [nameof(CompanyIdentityInput.TradeRegistryNumber)] = tradeRegistryNumber,
            [nameof(CompanyIdentityInput.ProvinceId)] = province,
            [nameof(CompanyIdentityInput.District)] = district,
            [nameof(CompanyIdentityInput.AddressLine)] = addressLine,
            [nameof(CompanyIdentityInput.Phone)] = phone,
            [nameof(CompanyIdentityInput.Email)] = email,
            [nameof(CompanyIdentityInput.KepAddress)] = kepAddress,
            [nameof(CompanyIdentityInput.ChamberName)] = chamberName,
            [nameof(CompanyIdentityInput.BusinessName)] = businessName,
            [nameof(CompanyIdentityInput.ProfessionalConductRules)] = professionalConductRules,
            [nameof(CompanyIdentityInput.EtbisVerificationUrl)] = etbisVerificationUrl,
        };
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
