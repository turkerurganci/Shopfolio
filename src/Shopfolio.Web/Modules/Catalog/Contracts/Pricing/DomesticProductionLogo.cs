using System.Security.Cryptography;

using Shopfolio.Infrastructure.Routing;

namespace Shopfolio.Modules.Catalog.Contracts.Pricing;

/// <summary>
/// The Ministry's domestic production logo (02 3.8.5, 12.1.5; 08 4.3; 09 11.1 item 7; K-610): an asset that comes with the code. The file is an embedded resource of the
/// assembly - no setting, no panel field and no switch names, replaces or hides it; a maintenance release replaces it (08 4.3 item 1). The decision whether a product shows it
/// looks at the product's type and the code of its place of origin and at nothing else (11 T501 criterion 7): a physical product made in Turkey shows it, every other product
/// does not. Whether the goods count as domestic is the firm's declaration; the product does not check it (08 4.3 item 4).
/// </summary>
public static class DomesticProductionLogo
{
    /// <summary>The code of Turkey in the country list (06 4.4): the row that triggers the logo.</summary>
    public const string TurkeyCountryCode = "TR";

    /// <summary>The media type of the file the Ministry announced (JPEG, DV-34).</summary>
    public const string ContentType = "image/jpeg";

    /// <summary>The embedded resource's name (see the project file).</summary>
    internal const string ResourceName = "Shopfolio.Modules.Catalog.Assets.DomesticProductionLogo.jpg";

    private static readonly byte[] FileBytes = ReadResource();

    // A fingerprint of the file for its address, not a secret and not a key: any digest of the content serves, so the key generators' one home (09 15.1 item 1) is not touched.
    private static readonly string FileHash = Convert.ToHexStringLower(SHA512.HashData(FileBytes));

    /// <summary>The file's bytes; the endpoint serves them and nothing reads the logo from anywhere else.</summary>
    public static ReadOnlyMemory<byte> Content => FileBytes;

    /// <summary>
    /// The address of the file: under the static area with ten characters of its content hash in the name, like the static files (07 2.1.2, 2.10.3), so a replaced file gets a
    /// new address and the old one can be cached for good.
    /// </summary>
    public static string Address { get; } = $"{AddressRules.StaticArea}/yerli-uretim-logosu.{FileHash[..10]}.jpg";

    /// <summary>
    /// Whether a product shows the logo: it is a physical product and its place of origin is Turkey (02 3.8.5). A digital product and a service never show it; another country
    /// and an empty place of origin never show it. The decision reads the product's type and the country's code only.
    /// </summary>
    public static bool Shows(ProductType type, string? originCountryCode) =>
        type == ProductType.Physical && string.Equals(originCountryCode, TurkeyCountryCode, StringComparison.Ordinal);

    private static byte[] ReadResource()
    {
        using Stream stream = typeof(DomesticProductionLogo).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"The embedded resource '{ResourceName}' is missing from the assembly.");
        using MemoryStream copy = new();
        stream.CopyTo(copy);
        return copy.ToArray();
    }
}

/// <summary>
/// The origin line of a price label with its logo (02 3.8.5; 04 5.4.4): the country's name - always written when the product carries a place of origin, with the logo or without -
/// and whether the logo stands beside it. The one view component <c>_OriginLabel</c> draws it; the product page's price block calls that component (11 T501 criterion 5).
/// </summary>
public sealed record OriginLabel(string CountryName, bool ShowsLogo)
{
    /// <summary>
    /// The label of a product: <see langword="null"/> when the product shows no origin line (a service, or an empty place of origin); otherwise the country's name and the logo
    /// decision of <see cref="DomesticProductionLogo.Shows"/>. The name comes from the Settings module's country list (D-344).
    /// </summary>
    public static OriginLabel? Of(ProductType type, string? originCountryCode, string? countryName)
    {
        if (type == ProductType.Service || string.IsNullOrWhiteSpace(originCountryCode) || string.IsNullOrWhiteSpace(countryName))
        {
            return null;
        }

        return new OriginLabel(countryName, DomesticProductionLogo.Shows(type, originCountryCode));
    }
}
