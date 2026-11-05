using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// Which file of the image section serves an image (05 7.7.2, 5.2.10): a derivative when the derivatives are ready and one is wide enough,
/// the source copy otherwise - a picture whose derivatives are not made yet is served from the source copy, never missing.
/// </summary>
internal static class ImageServing
{
    /// <summary>The widths of the storefront that are smaller than the source's width; a derivative as wide as the source is not made.</summary>
    public static IReadOnlyList<int> DerivativeWidthsFor(int sourceWidth) => [.. ImagingLimits.DerivativeWidths.Where(w => w < sourceWidth)];

    /// <summary>The key to serve for a wanted width: the narrowest derivative that is at least that wide, else the source copy.</summary>
    public static string KeyFor(string sourceKey, int sourceWidth, DateTimeOffset? derivativesReadyAt, int wantedWidth)
    {
        if (derivativesReadyAt is null)
        {
            return sourceKey;
        }

        foreach (int width in DerivativeWidthsFor(sourceWidth).Order())
        {
            if (width >= wantedWidth)
            {
                return FileKeys.DerivativeKey(sourceKey, width);
            }
        }

        return sourceKey;
    }
}
