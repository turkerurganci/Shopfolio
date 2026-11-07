using System.Security.Cryptography;
using System.Text;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>One shipped template of a generated legal text: its text and its version id (09 11.4 item 5).</summary>
internal sealed record LegalTextTemplate(LegalTextType Type, string Text, string Version);

/// <summary>
/// The templates that come with the product (02 3.33.1; 09 11.4 items 3 and 5): constant text files embedded in the assembly. The version id of a template is the first
/// twelve hexadecimal digits of the SHA-256 of its content with line endings normalized to <c>\n</c>, so it changes exactly when the text changes and never because of the
/// checkout's line endings; there is no hand-raised version constant. Only the two generated texts have a template.
/// </summary>
internal static class LegalTextTemplates
{
    /// <summary>The two generated types, in the order their versions are written.</summary>
    public static IReadOnlyList<LegalTextType> GeneratedTypes { get; } = [LegalTextType.PreInformationForm, LegalTextType.DistanceSalesContract];

    /// <summary>The tokens of a generated text that the version fills from the settings: the company identity and the return address. Every other token is an order field that waits (09 9.3 item 2).</summary>
    public static IReadOnlyList<string> CompanyTokens { get; } =
    [
        "CompanyLegalName", "CompanyTaxIdNumber", "CompanyMersisNumber", "CompanyTradeRegistryNumber", "CompanyProvince", "CompanyDistrict", "CompanyAddressLine",
        "CompanyPhone", "CompanyEmail", "CompanyKepAddress", "ReturnAddress",
    ];

    public static LegalTextTemplate Get(LegalTextType type)
    {
        string name = type switch
        {
            LegalTextType.PreInformationForm => "PreInformationForm.tr.txt",
            LegalTextType.DistanceSalesContract => "DistanceSalesContract.tr.txt",
            _ => throw new ArgumentException($"{type} is not a generated legal text and has no template.", nameof(type)),
        };
        string text = Read($"Shopfolio.Modules.Settings.Templates.{name}");
        return new LegalTextTemplate(type, text, VersionOf(text));
    }

    public static string VersionOf(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(text.ReplaceLineEndings("\n")));
        return Convert.ToHexStringLower(hash)[..12];
    }

    private static string Read(string resourceName)
    {
        using Stream stream = typeof(LegalTextTemplates).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"The template resource {resourceName} is not in the assembly.");
        using StreamReader reader = new(stream, Encoding.UTF8);
        return reader.ReadToEnd().ReplaceLineEndings("\n");
    }
}

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

    public LegalTextGenerator(AppDbContext db, CompanySettingsWriter writer, TimeProvider time)
    {
        _db = db;
        _writer = writer;
        _time = time;
    }

    /// <summary>Writes the new version of each generated text whose text or template changed; returns the versions written, none when nothing changed.</summary>
    public async Task<IReadOnlyList<GeneratedVersion>> RegenerateAsync(CancellationToken ct)
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
            if (last is not null && last.Text == text && last.TemplateVersion == template.Version)
            {
                continue;
            }

            LegalTextVersion row = LegalTextVersion.CreatePublished(type, (last?.VersionNumber ?? 0) + 1, now, text, now);
            row.TemplateVersion = template.Version;
            _db.LegalTextVersions.Add(row);
            written.Add(new GeneratedVersion(type, row.Id, row.VersionNumber!.Value, last is null || last.TemplateVersion != template.Version));
        }

        if (written.Count > 0)
        {
            await _db.SaveChangesAsync(ct);
        }

        return written;
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
            ["CompanyLegalName"] = company.LegalName,
            ["CompanyTaxIdNumber"] = company.TaxIdNumber,
            ["CompanyMersisNumber"] = company.MersisNumber,
            ["CompanyTradeRegistryNumber"] = company.TradeRegistryNumber,
            ["CompanyProvince"] = CompanyProvince(),
            ["CompanyDistrict"] = company.District,
            ["CompanyAddressLine"] = company.AddressLine,
            ["CompanyPhone"] = company.Phone,
            ["CompanyEmail"] = company.Email,
            ["CompanyKepAddress"] = company.KepAddress,
            ["ReturnAddress"] = returnAddress,
        };
    }
}
