using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.CorporateContent.Contracts;

/// <summary>
/// The brand form as the panel shows it (T362; 02 3.29; 06 4.54; 04 9.15): the four settings, the platform signature switch and the version of the form. The values are the stored ones - a brand
/// name not entered yet is <see langword="null"/>, a colour not chosen is <see langword="null"/> - so the form can say what the site falls back to. There is no field for a script, a style or a
/// tracking code, no second colour and no theme (02 3.29.1; 10 KD-47).
/// </summary>
public sealed record BrandingSection(string? BrandName, Guid? LogoId, Guid? FaviconId, string? BrandColor, bool IsPlatformSignatureEnabled, int Version);

/// <summary>
/// What the brand form posts (T362; 07 UC-163): the name, the colour, the logo and the site icon as uploaded images (<c>ICatalogModule.UploadImageAsync</c>, which applies the image file rules
/// P-22 and P-23), and the platform signature switch. A logo or icon that is <see langword="null"/> is none. The colour is <c>#RRGGBB</c> in any letter case; blank is none.
/// </summary>
public sealed record BrandingInput(string? BrandName, Guid? LogoId, Guid? FaviconId, string? BrandColor, bool IsPlatformSignatureEnabled);

/// <summary>One refused field of the brand form: the member of <see cref="BrandingInput"/> and the field code (07 6.2).</summary>
public sealed record BrandingRejection(string Field, string Code);

/// <summary>
/// The outcome of the brand form's save (07 UC-163; 09 10.2 item 2): saved, refused because the version the form carried is no longer the record's (the conflict warning, 07 2.4.5), or refused
/// because a field is wrong (422) with the fields and their codes. A refusal is a value, not an exception (09 8.1): the transaction runner rolls back and hands it to the page.
/// </summary>
public sealed class BrandingSaveResult : IOperationResult
{
    private static readonly BrandingRejection[] NoFields = [];

    private BrandingSaveResult(bool isSaved, bool isVersionConflict, IReadOnlyList<BrandingRejection> rejections)
    {
        IsSaved = isSaved;
        IsVersionConflict = isVersionConflict;
        Rejections = rejections;
    }

    public static BrandingSaveResult Saved { get; } = new(true, false, NoFields);

    public static BrandingSaveResult VersionConflict { get; } = new(false, true, NoFields);

    public bool IsSaved { get; }

    public bool IsVersionConflict { get; }

    /// <summary>The refused fields; empty unless the save was refused for its values.</summary>
    public IReadOnlyList<BrandingRejection> Rejections { get; }

    public bool IsRejected => !IsSaved;

    public static BrandingSaveResult Refused(IReadOnlyList<BrandingRejection> rejections)
    {
        ArgumentNullException.ThrowIfNull(rejections);
        if (rejections.Count == 0)
        {
            throw new ArgumentException("A refusal for values names at least one field.", nameof(rejections));
        }

        return new BrandingSaveResult(false, false, rejections);
    }
}

/// <summary>
/// The brand as the storefront and the e-mails show it (T362; 02 3.29.2-3.29.6, 9.1.4): one read for both, never stored. <paramref name="DisplayName"/> is the brand name, or the installation's
/// domain until a name is entered (02 3.29.3); the header shows the logo when <paramref name="LogoId"/> is set and the name as text when it is not (02 3.29.2). The site icon is
/// <paramref name="FaviconId"/> or - when none - <paramref name="FaviconInitial"/> (the first letter of the displayed name) on <paramref name="Color"/>; no icon of the product is ever the site's
/// (02 3.29.4). <paramref name="Color"/> is the brand colour or the product's default (<paramref name="IsDefaultColor"/>); <paramref name="TextColor"/> is the black or white the system chose
/// for the text on it - never stored (02 3.29.5). <paramref name="ShowsPlatformSignature"/> is the footer's signature switch: the footer only, never an e-mail or the site icon (02 3.29.6).
/// The panel does not take the brand colour (02 3.29.1).
/// </summary>
public sealed record BrandIdentity(
    string DisplayName,
    bool HasBrandName,
    Guid? LogoId,
    Guid? FaviconId,
    string FaviconInitial,
    string Color,
    string TextColor,
    bool IsDefaultColor,
    bool ShowsPlatformSignature);
