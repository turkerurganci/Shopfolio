using System.Net;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// The four legal texts (02 3.33; 06 4.56; 11 T366): the reads the panel and the order confirmation make, the save and the publication of the two firm-edited texts' draft,
/// and the version check. The module does not choose a text for a screen and does not build a page: it gives the texts, their versions and what is missing by name.
/// <list type="bullet">
/// <item>The two generated texts have no draft and no publish call; their versions are written by <see cref="LegalTextGenerator"/> when a feeding setting or the template changes.</item>
/// <item>A published row is only ever added (<see cref="ProtectedRecordsGuard"/>); the draft stays in place after a publication and is the row of the next edit.</item>
/// <item>There is no consent box, no "seen" record and no re-consent: a changed text is a new version for the next order, and the order holds the version of its day (02 3.33.4, 3.33.5).</item>
/// </list>
/// </summary>
internal sealed class LegalTexts
{
    /// <summary>The identity tokens of a firm-edited text's draft (06 4.1 field names); the publication fills them from the company identity, HTML-encoded.</summary>
    internal static readonly IReadOnlyList<string> IdentityTokens =
    [
        "LegalName", "TaxIdNumber", "MersisNumber", "TradeRegistryNumber", "ProvinceId", "District", "AddressLine", "Phone", "Email", "KepAddress", "ChamberName", "BusinessName",
    ];

    private readonly AppDbContext _db;
    private readonly CompanyIdentities _identities;
    private readonly IAuditTrail _trail;
    private readonly TimeProvider _time;

    public LegalTexts(AppDbContext db, CompanyIdentities identities, IAuditTrail trail, TimeProvider time)
    {
        _db = db;
        _identities = identities;
        _trail = trail;
        _time = time;
    }

    public static bool IsEditedByFirm(LegalTextType type) => type is LegalTextType.PrivacyNotice or LegalTextType.CookiePolicy;

    public async Task<IReadOnlyList<LegalTextSummary>> GetSummariesAsync(CancellationToken ct)
    {
        Dictionary<LegalTextType, int> lastNumbers = (await _db.LegalTextVersions.AsNoTracking()
            .Where(l => l.PublishedAt != null)
            .GroupBy(l => l.LegalTextType)
            .Select(g => new { Type = g.Key, Number = g.Max(l => l.VersionNumber) })
            .ToListAsync(ct)).ToDictionary(x => x.Type, x => x.Number ?? 0);
        string? draftHosting = await _db.LegalTextVersions.AsNoTracking()
            .Where(l => l.LegalTextType == LegalTextType.PrivacyNotice && l.PublishedAt == null)
            .Select(l => l.HostingLocation)
            .SingleOrDefaultAsync(ct);
        CompanyIdentityCompleteness identity = await _identities.GetCompletenessAsync(ct);

        List<LegalTextSummary> summaries = [];
        foreach (LegalTextType type in Enum.GetValues<LegalTextType>())
        {
            int? last = lastNumbers.TryGetValue(type, out int number) ? number : null;
            List<LegalTextMissingPart> missing = [];
            if (last is null)
            {
                missing.Add(LegalTextMissingPart.NotPublished);
            }

            if (type == LegalTextType.PrivacyNotice)
            {
                // A published privacy notice always carries its hosting location (the table's rule), so the section is missing only before the first publication.
                if (last is null && draftHosting is null)
                {
                    missing.Add(LegalTextMissingPart.HostingLocation);
                }

                if (identity.MissingFields.Count > 0)
                {
                    missing.Add(LegalTextMissingPart.CompanyIdentity);
                }
            }

            summaries.Add(new LegalTextSummary(type, last, IsEditedByFirm(type), missing));
        }

        return summaries;
    }

    public async Task<LegalTextDraft> GetDraftAsync(LegalTextType type, CancellationToken ct)
    {
        RequireFirmEdited(type);
        LegalTextVersion draft = await ReadDraftAsync(type, tracked: false, ct);
        return new LegalTextDraft(type, draft.Version, draft.Text, draft.HostingLocation);
    }

    public async Task<SettingsSaveResult> SaveDraftAsync(LegalTextType type, string text, string? hostingLocation, int formVersion, CancellationToken ct)
    {
        RequireFirmEdited(type);
        ArgumentNullException.ThrowIfNull(text);
        RequireTransaction();
        string? hosting = string.IsNullOrWhiteSpace(hostingLocation) ? null : hostingLocation.Trim();
        if (hosting is not null && type != LegalTextType.PrivacyNotice)
        {
            throw new ArgumentException("The hosting location exists on the privacy notice only (06 4.56).", nameof(hostingLocation));
        }

        LegalTextVersion draft = await ReadDraftAsync(type, tracked: true, ct);
        List<Contracts.FieldRejection> rejections = [];
        if (string.IsNullOrWhiteSpace(text))
        {
            rejections.Add(new Contracts.FieldRejection(nameof(LegalTextVersion.Text), FieldCodes.FilledFieldCleared));
        }

        if (hosting is null && draft.HostingLocation is not null)
        {
            rejections.Add(new Contracts.FieldRejection(nameof(LegalTextVersion.HostingLocation), FieldCodes.FilledFieldCleared));
        }

        if (rejections.Count > 0)
        {
            return SettingsSaveResult.Refused(rejections);
        }

        RecordSaveResult saved = await RecordSave.SaveAsync(_db, draft, formVersion, d =>
        {
            d.Text = text;
            d.HostingLocation = hosting;
        }, ct);
        return saved.IsSaved ? SettingsSaveResult.Saved : SettingsSaveResult.VersionConflict;
    }

    public async Task<LegalTextPublishResult> PublishAsync(LegalTextType type, int formVersion, CancellationToken ct)
    {
        RequireFirmEdited(type);
        RequireTransaction();
        LegalTextVersion draft = await ReadDraftAsync(type, tracked: false, ct);
        if (draft.Version != formVersion)
        {
            return LegalTextPublishResult.VersionConflict;
        }

        List<Contracts.FieldRejection> rejections = [];
        if (string.IsNullOrWhiteSpace(draft.Text))
        {
            rejections.Add(new Contracts.FieldRejection(nameof(LegalTextVersion.Text), FieldCodes.FieldRequired));
        }

        if (type == LegalTextType.PrivacyNotice && string.IsNullOrWhiteSpace(draft.HostingLocation))
        {
            rejections.Add(new Contracts.FieldRejection(nameof(LegalTextVersion.HostingLocation), FieldCodes.FieldRequired));
        }

        if (rejections.Count > 0)
        {
            return LegalTextPublishResult.Refused(rejections);
        }

        int? last = await _db.LegalTextVersions.AsNoTracking()
            .Where(l => l.LegalTextType == type && l.PublishedAt != null)
            .MaxAsync(l => l.VersionNumber, ct);
        DateTimeOffset now = _time.GetUtcNow();
        string text = PlaceholderFiller.Fill(draft.Text, await IdentityValuesAsync(ct), requireAll: false);
        LegalTextVersion row = LegalTextVersion.CreatePublished(type, (last ?? 0) + 1, now, text, now);
        row.HostingLocation = type == LegalTextType.PrivacyNotice ? draft.HostingLocation!.Trim() : null;
        _db.LegalTextVersions.Add(row);

        AuditDetails details = new(DisplayNameOf(type), values: [new AuditValue("versionNumber", row.VersionNumber!.Value.ToString(System.Globalization.CultureInfo.InvariantCulture))]);
        await _trail.WriteAsync(new AuditEntry(AuditOperation.PublishLegalText, nameof(LegalTextVersion), row.Id, details), ct);
        return LegalTextPublishResult.Published(row.VersionNumber!.Value, row.Id);
    }

    public async Task<LegalTextVersionIds> GetCurrentVersionIdsAsync(CancellationToken ct)
    {
        var latest = await _db.LegalTextVersions.AsNoTracking()
            .Where(l => l.PublishedAt != null)
            .GroupBy(l => l.LegalTextType)
            .Select(g => g.OrderByDescending(l => l.VersionNumber).Select(l => new { l.LegalTextType, l.Id }).First())
            .ToListAsync(ct);
        Guid? IdOf(LegalTextType type) => latest.Where(l => l.LegalTextType == type).Select(l => (Guid?)l.Id).SingleOrDefault();
        return new LegalTextVersionIds(IdOf(LegalTextType.PreInformationForm), IdOf(LegalTextType.DistanceSalesContract), IdOf(LegalTextType.PrivacyNotice));
    }

    public async Task<bool> AreShownCurrentAsync(Guid shownForm, Guid shownContract, CancellationToken ct)
    {
        LegalTextVersionIds current = await GetCurrentVersionIdsAsync(ct);
        return current.PreInformationForm == shownForm && current.DistanceSalesContract == shownContract;
    }

    public async Task<LegalTextVersionInfo?> GetVersionAsync(Guid versionId, CancellationToken ct)
    {
        return await _db.LegalTextVersions.AsNoTracking()
            .Where(l => l.Id == versionId && l.PublishedAt != null)
            .Select(l => new LegalTextVersionInfo(l.Id, l.LegalTextType, l.VersionNumber!.Value, l.PublishedAt!.Value, l.Text, l.HostingLocation))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<LegalTextVersionInfo?> GetLatestVersionAsync(LegalTextType type, CancellationToken ct)
    {
        return await _db.LegalTextVersions.AsNoTracking()
            .Where(l => l.LegalTextType == type && l.PublishedAt != null)
            .OrderByDescending(l => l.VersionNumber)
            .Select(l => new LegalTextVersionInfo(l.Id, l.LegalTextType, l.VersionNumber!.Value, l.PublishedAt!.Value, l.Text, l.HostingLocation))
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>The Turkish name of a text as the trail shows it (02 10.3.1 (5)).</summary>
    internal static string DisplayNameOf(LegalTextType type) => type switch
    {
        LegalTextType.PreInformationForm => "Ön Bilgilendirme Formu",
        LegalTextType.DistanceSalesContract => "Mesafeli Satış Sözleşmesi",
        LegalTextType.PrivacyNotice => "Aydınlatma metni",
        LegalTextType.CookiePolicy => "Çerez politikası",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Not a legal text type."),
    };

    private async Task<Dictionary<string, string?>> IdentityValuesAsync(CancellationToken ct)
    {
        LegalIdentity identity = await _identities.GetLegalAsync(ct);
        Company company = await _db.Companies.AsNoTracking().SingleAsync(ct);
        static string? Encode(string? value) => value is null ? null : WebUtility.HtmlEncode(value);
        return new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["LegalName"] = Encode(identity.LegalName),
            ["TaxIdNumber"] = Encode(identity.TaxIdNumber),
            ["MersisNumber"] = Encode(identity.MersisNumber),
            ["TradeRegistryNumber"] = Encode(identity.TradeRegistryNumber),
            ["ProvinceId"] = Encode(identity.ProvinceName),
            ["District"] = Encode(identity.District),
            ["AddressLine"] = Encode(identity.AddressLine),
            ["Phone"] = Encode(identity.Phone),
            ["Email"] = Encode(identity.Email),
            ["KepAddress"] = Encode(identity.KepAddress),
            ["ChamberName"] = Encode(company.ChamberName),
            ["BusinessName"] = Encode(identity.BusinessName),
        };
    }

    private async Task<LegalTextVersion> ReadDraftAsync(LegalTextType type, bool tracked, CancellationToken ct)
    {
        IQueryable<LegalTextVersion> drafts = _db.LegalTextVersions.Where(l => l.LegalTextType == type && l.PublishedAt == null);
        if (!tracked)
        {
            drafts = drafts.AsNoTracking();
        }

        // The draft is start data (06 8.1.1); a missing row fails loudly, like the single company row.
        return await drafts.SingleAsync(ct);
    }

    private static void RequireFirmEdited(LegalTextType type)
    {
        if (!IsEditedByFirm(type))
        {
            throw new ArgumentException($"{type} is a generated text: it has no draft and the firm does not publish it (02 3.33.1).", nameof(type));
        }
    }

    private void RequireTransaction()
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The legal text draft and its publication are written inside the caller's transaction (09 9.1 item 3).");
        }
    }
}

/// <summary>
/// The privacy notice part of the sales gate and of the data-collecting entries' gate (02 3.1.5, 3.33.2, 3.33.9; 11 T331 criterion 8): the last published version of the notice
/// carries its hosting location. The identity fields of the same definition are read by the gate itself. T331 left the interface unbound; this is its binding.
/// </summary>
internal sealed class PrivacyNoticeCondition : IPrivacyNoticeCondition
{
    private readonly AppDbContext _db;

    public PrivacyNoticeCondition(AppDbContext db)
    {
        _db = db;
    }

    public Task<bool> IsPublishedWithHostingLocationAsync(CancellationToken ct) =>
        _db.LegalTextVersions.AsNoTracking()
            .AnyAsync(l => l.LegalTextType == LegalTextType.PrivacyNotice && l.PublishedAt != null && l.HostingLocation != null, ct);
}
