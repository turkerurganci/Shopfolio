using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>A version the generator wrote: the type, the row's id and number, and whether the cause was the shipped template (a system event, no trail row) or a changed setting.</summary>
internal sealed record GeneratedVersion(LegalTextType Type, Guid Id, int VersionNumber, bool IsTemplateChange);

/// <summary>
/// The writer of the generated legal texts' new versions (09 11.4 item 6; 06 4.56; 02 3.33.3): the Pre-Information Form and the Distance Sales Contract. It is called from
/// two places - the save of the company identity and of the return address (every setting that feeds the texts calls it after its own write; the shipping fee and period
/// sections of T350 and T362 will do the same) and the application's start - and it follows one order: the settings row is locked, the text is made <i>under that lock</i>
/// from the current values (<see cref="PlaceholderFiller.Fill"/>, the company's values filled and the order's tokens left in place), it is compared with the last published row of
/// the type, and a row is added only when the text or the template's version id differs. A save that changes no value therefore writes no version. A text made outside the
/// lock and carried in would let two concurrent saves drop one of the changes (05 7.3.5; K-1088).
/// <para>
/// The trail row of a version that a changed setting caused belongs with that setting's own trail row, which the saves write when the trail is wired to them (D-165 (a));
/// a version a changed template caused is a system event and never has a row (02 3.33.3, 10.3.3).
/// </para>
/// </summary>
internal sealed class LegalTextGenerator
{
    private readonly AppDbContext _db;
    private readonly CompanySettingsWriter _writer;
    private readonly TimeProvider _time;
    private readonly IAuditTrail _trail;

    public LegalTextGenerator(AppDbContext db, CompanySettingsWriter writer, TimeProvider time, IAuditTrail trail)
    {
        _trail = trail;
        _db = db;
        _writer = writer;
        _time = time;
    }

    /// <summary>Writes the new version of each generated text whose text or template changed; returns the versions written, none when nothing changed.</summary>
    public Task<IReadOnlyList<GeneratedVersion>> RegenerateAsync(CancellationToken ct) => RegenerateAsync(feedingSettingChanged: false, ct);

    /// <summary>
    /// As <see cref="RegenerateAsync(CancellationToken)"/>, and when <paramref name="feedingSettingChanged"/> is true a version is written even though the text is the same (T536; 02 3.33.3;
    /// 03 8.7.2.3; 04 9.18.9): the shipping fee, the firm default of the shipping period and the delivery provinces feed the two texts through the order's tokens and a general sentence, so the
    /// text of the version does not change with them - the version still rises when such a value really changed (D-681, D-493). A save that changed no feeding value passes false.
    /// </summary>
    public async Task<IReadOnlyList<GeneratedVersion>> RegenerateAsync(bool feedingSettingChanged, CancellationToken ct)
    {
        await _writer.LockAsync(ct);
        IReadOnlyDictionary<string, string?> values = await ReadCompanyValuesAsync(ct);
        DateTimeOffset now = _time.GetUtcNow();
        List<GeneratedVersion> written = [];
        foreach (LegalTextType type in LegalTextTemplates.GeneratedTypes)
        {
            LegalTextTemplate template = LegalTextTemplates.Get(type);
            string text = PlaceholderFiller.Fill(template.Text, values, requireAll: false);
            var last = await _db.LegalTextVersions.AsNoTracking()
                .Where(l => l.LegalTextType == type && l.PublishedAt != null)
                .OrderByDescending(l => l.VersionNumber)
                .Select(l => new { l.VersionNumber, l.Text, l.TemplateVersion })
                .FirstOrDefaultAsync(ct);
            if (last is not null && last.Text == text && last.TemplateVersion == template.Version && !feedingSettingChanged)
            {
                continue;
            }

            LegalTextVersion row = LegalTextVersion.CreatePublished(type, (last?.VersionNumber ?? 0) + 1, now, text, now);
            row.TemplateVersion = template.Version;
            _db.LegalTextVersions.Add(row);
            written.Add(new GeneratedVersion(type, row.Id, row.VersionNumber!.Value, last is null || last.TemplateVersion != template.Version)); // non-null: CreatePublished sets the version number
        }

        // The rows are added to the caller's unit of work: the command pipeline and the start-up transaction runner save it before they commit. The one writer of the settings
        // row stays CompanySettingsWriter (09 10.3 item 4), so this file calls no save.
        return written;
    }

    /// <summary>
    /// Writes the trail row of each version a changed setting caused (D-165 (a), D-220; 02 10.3.1 (6)): the same operation and shape as the publication of an edited text, in the caller's transaction.
    /// A version a changed template caused is a system event and gets no row (02 3.33.3, 10.3.3). Needs the signed-in administrator of the request, as the trail does.
    /// </summary>
    public async Task WriteTrailAsync(IReadOnlyList<GeneratedVersion> versions, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(versions);
        foreach (GeneratedVersion version in versions.Where(v => !v.IsTemplateChange))
        {
            AuditDetails details = new(LegalTexts.DisplayNameOf(version.Type), values: [new AuditValue("versionNumber", version.VersionNumber.ToString(System.Globalization.CultureInfo.InvariantCulture))]);
            await _trail.WriteAsync(new AuditEntry(AuditOperation.PublishLegalText, nameof(LegalTextVersion), version.Id, details), ct);
        }
    }

    private async Task<IReadOnlyDictionary<string, string?>> ReadCompanyValuesAsync(CancellationToken ct)
    {
        Company company = await _db.Companies.AsNoTracking().SingleAsync(ct);
        CompanySettings settings = await _db.CompanySettings.AsNoTracking().SingleAsync(ct);
        Guid[] provinceIds = [.. new[] { company.ProvinceId, settings.ReturnProvinceId }.OfType<Guid>()];
        Dictionary<Guid, string> provinces = await _db.Provinces.AsNoTracking().Where(p => provinceIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Name, ct);
        string? CompanyProvince() => company.ProvinceId is Guid id && provinces.TryGetValue(id, out string? name) ? name : null;

        string? returnAddress = null;
        if (settings.ReturnRecipientName is not null && settings.ReturnProvinceId is Guid returnProvince && settings.ReturnDistrict is not null && settings.ReturnAddressLine is not null)
        {
            string province = provinces.GetValueOrDefault(returnProvince, string.Empty);
            returnAddress = $"{settings.ReturnRecipientName}, {settings.ReturnAddressLine}, {settings.ReturnDistrict} / {province}";
            if (settings.ReturnPhone is not null)
            {
                returnAddress += $", Telefon: {settings.ReturnPhone}";
            }
        }

        return new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["CompanyLegalName"] = PlaceholderFiller.Inert(company.LegalName),
            ["CompanyTaxIdNumber"] = PlaceholderFiller.Inert(company.TaxIdNumber),
            ["CompanyMersisNumber"] = PlaceholderFiller.Inert(company.MersisNumber),
            ["CompanyTradeRegistryNumber"] = PlaceholderFiller.Inert(company.TradeRegistryNumber),
            ["CompanyChamberName"] = PlaceholderFiller.Inert(company.ChamberName),
            ["CompanyProvince"] = PlaceholderFiller.Inert(CompanyProvince()),
            ["CompanyDistrict"] = PlaceholderFiller.Inert(company.District),
            ["CompanyAddressLine"] = PlaceholderFiller.Inert(company.AddressLine),
            ["CompanyPhone"] = PlaceholderFiller.Inert(company.Phone),
            ["CompanyEmail"] = PlaceholderFiller.Inert(company.Email),
            ["CompanyKepAddress"] = PlaceholderFiller.Inert(company.KepAddress),
            ["ReturnAddress"] = PlaceholderFiller.Inert(returnAddress),
            ["HowTheOrderIsPlaced"] = ProcessGuide.Text,
        };
    }
}
