using System.Globalization;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// The brand identity (T362; 02 3.29; 06 4.54; 07 UC-163): the panel's read and save of the four settings and the platform signature, and the one read the storefront and the e-mails take the
/// brand from. The save is the record variant (09 10.2 item 2): the row is loaded tracked, the version the form carried is compared, the values are checked, and <see cref="RecordSave"/> writes
/// them and raises the version - it joins the caller's transaction and never opens one. The trail rows are written in the same transaction: the four settings as <c>UpdateBranding</c> with the
/// old and the new value of each changed field, the signature switch as <c>UpdateSettings</c> (02 10.3.1 (1), (6)). A save that changes nothing writes nothing: no version step, no trail row. The
/// row is the installation's one (06 4.54); it is read with <c>Single</c> and never added or removed here.
/// </summary>
internal sealed class BrandingSettings
{
    /// <summary>The names of the two trail rows as the trail shows them (02 10.3.1).</summary>
    internal const string BrandingDisplayName = "Marka";

    internal const string PlatformSignatureDisplayName = "Platform imzası";

    private readonly AppDbContext _db;
    private readonly ICatalogModule _catalog;
    private readonly IAuditTrail _trail;
    private readonly CacheInvalidations _invalidations;
    private readonly string _domain;

    public BrandingSettings(AppDbContext db, ICatalogModule catalog, IAuditTrail trail, CacheInvalidations invalidations, IOptions<SiteOptions> site)
    {
        ArgumentNullException.ThrowIfNull(site);
        _db = db;
        _catalog = catalog;
        _trail = trail;
        _invalidations = invalidations;
        _domain = site.Value.Domain;
    }

    public async Task<BrandingSection> ReadSectionAsync(CancellationToken ct)
    {
        return await _db.Brandings.AsNoTracking()
            .Select(b => new BrandingSection(b.BrandName, b.LogoId, b.FaviconId, b.BrandColor, b.IsPlatformSignatureEnabled, b.Version))
            .SingleAsync(ct);
    }

    public async Task<BrandIdentity> ReadIdentityAsync(CancellationToken ct)
    {
        BrandingSection section = await ReadSectionAsync(ct);
        bool hasName = section.BrandName is not null;
        string displayName = hasName ? section.BrandName! : _domain;

        // The colour and the text on it come from the product's one rule (02 3.29.5): the stored colour, or the product's default when none is chosen.
        string chosen = section.BrandColor ?? EmailColors.DefaultButtonBackground;
        (string color, string textColor) = EmailColors.Button(chosen);
        return new BrandIdentity(
            displayName,
            hasName,
            section.LogoId,
            section.FaviconId,
            BrandingRules.Initial(displayName),
            color,
            textColor,
            section.BrandColor is null,
            section.IsPlatformSignatureEnabled);
    }

    public async Task<BrandingSaveResult> SaveAsync(BrandingInput input, int formVersion, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The brand identity is written only inside the caller's transaction (09 9.1 item 3).");
        }

        Branding row = await _db.Brandings.SingleAsync(ct);
        if (row.Version != formVersion)
        {
            return BrandingSaveResult.VersionConflict;
        }

        string? name = BrandingRules.NormalizeName(input.BrandName);
        List<BrandingRejection> rejections = [];
        if (name is null && row.BrandName is not null)
        {
            // Once entered, the brand name cannot be emptied (02 3.29.3); an empty name beside an empty row is the row as it is born.
            rejections.Add(new BrandingRejection(nameof(BrandingInput.BrandName), FieldCodes.FilledFieldCleared));
        }

        if (!BrandingRules.TryNormalizeColor(input.BrandColor, out string? color))
        {
            rejections.Add(new BrandingRejection(nameof(BrandingInput.BrandColor), FieldCodes.InvalidColor));
        }

        await CheckImageAsync(rejections, nameof(BrandingInput.LogoId), input.LogoId, row.LogoId, ct);
        await CheckImageAsync(rejections, nameof(BrandingInput.FaviconId), input.FaviconId, row.FaviconId, ct);
        if (rejections.Count > 0)
        {
            return BrandingSaveResult.Refused(rejections);
        }

        List<AuditValueChange> changes = [];
        Track(changes, nameof(Branding.BrandName), row.BrandName, name);
        Track(changes, nameof(Branding.LogoId), row.LogoId, input.LogoId);
        Track(changes, nameof(Branding.FaviconId), row.FaviconId, input.FaviconId);
        Track(changes, nameof(Branding.BrandColor), row.BrandColor, color);
        bool signatureChanged = row.IsPlatformSignatureEnabled != input.IsPlatformSignatureEnabled;
        if (changes.Count == 0 && !signatureChanged)
        {
            return BrandingSaveResult.Saved;
        }

        bool oldSignature = row.IsPlatformSignatureEnabled;
        RecordSaveResult saved = await RecordSave.SaveAsync(
            _db,
            row,
            formVersion,
            b =>
            {
                b.BrandName = name;
                b.LogoId = input.LogoId;
                b.FaviconId = input.FaviconId;
                b.BrandColor = color;
                b.IsPlatformSignatureEnabled = input.IsPlatformSignatureEnabled;
            },
            ct);
        if (!saved.IsSaved)
        {
            return BrandingSaveResult.VersionConflict;
        }

        // The brand shows in the parts the storefront reads often (05 7.10.1): the settings area is dropped once this transaction commits.
        _invalidations.Add(CacheArea.Settings);
        if (changes.Count > 0)
        {
            await _trail.WriteAsync(new AuditEntry(AuditOperation.UpdateBranding, nameof(Branding), row.Id, new AuditDetails(BrandingDisplayName, changes)), ct);
        }

        if (signatureChanged)
        {
            AuditValueChange change = new(nameof(Branding.IsPlatformSignatureEnabled), Text(oldSignature), Text(input.IsPlatformSignatureEnabled));
            await _trail.WriteAsync(new AuditEntry(AuditOperation.UpdateSettings, nameof(Branding), row.Id, new AuditDetails(PlatformSignatureDisplayName, [change])), ct);
        }

        return BrandingSaveResult.Saved;
    }

    /// <summary>An image that is not the one already linked must be an uploaded image (the rules P-22 and P-23 were applied at its upload); an id that names none is a refusal.</summary>
    private async Task CheckImageAsync(List<BrandingRejection> rejections, string field, Guid? imageId, Guid? current, CancellationToken ct)
    {
        if (imageId is { } id && id != current && (id == Guid.Empty || !await _catalog.ImageExistsAsync(id, ct)))
        {
            rejections.Add(new BrandingRejection(field, FieldCodes.InvalidImage));
        }
    }

    private static void Track(List<AuditValueChange> changes, string field, string? oldValue, string? newValue)
    {
        if (!string.Equals(oldValue, newValue, StringComparison.Ordinal))
        {
            changes.Add(new AuditValueChange(field, BrandingRules.TrailText(oldValue), BrandingRules.TrailText(newValue)));
        }
    }

    private static void Track(List<AuditValueChange> changes, string field, Guid? oldValue, Guid? newValue)
    {
        if (oldValue != newValue)
        {
            changes.Add(new AuditValueChange(field, oldValue?.ToString("D", CultureInfo.InvariantCulture), newValue?.ToString("D", CultureInfo.InvariantCulture)));
        }
    }

    private static string Text(bool value) => value ? "true" : "false";
}
