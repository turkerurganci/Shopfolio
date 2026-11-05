namespace Shopfolio.Infrastructure.Imaging;

/// <summary>The formats of an image read from its bytes (P-22; 09 15.6 item 2a). The infrastructure's own list: the module maps it to its stored ImageFormat.</summary>
internal enum ImageKind
{
    Jpeg,
    Png,
    WebP,
}
