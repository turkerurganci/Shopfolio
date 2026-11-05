// Deliberate violation sample of the image library scan (09 15.6 item 1; 09 6 item 4). The scan reads this file as text; it is not compiled.
// Line numbers are part of the test. A full-line comment that says SkiaSharp, SixLabors or ImageMagick is not a finding.
using SkiaSharp;
using SixLabors.ImageSharp;
internal sealed class LibraryLeak
{
    public SKBitmap Bitmap { get; }
    public ImageMagick.MagickImage Rejected { get; }
    public string Clean => "an image";
}
