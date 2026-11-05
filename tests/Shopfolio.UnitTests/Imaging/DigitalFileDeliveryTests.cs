using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.UnitTests.Imaging;

/// <summary>
/// The delivery of a digital product file (T223 criterion 8, KK8; 05 6.6.3; 09 8.1 item 1): no type limit, an attachment with a general binary
/// type and the no-guess header, a safe file name, and the size ceiling P-26 refused by name. Made on the in-memory fake: no file is written.
/// </summary>
public sealed class DigitalFileDeliveryTests
{
    private static readonly string Key = new('c', 32);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(new byte[] { 0x4D, 0x5A, 0x90, 0x00 }, "setup.exe")]
    [InlineData(new byte[] { 0x3C, 0x68, 0x74, 0x6D, 0x6C, 0x3E }, "page.html")]
    [InlineData(new byte[] { 0x3C, 0x73, 0x76, 0x67, 0x3E }, "drawing.svg")]
    [InlineData(new byte[] { 0x50, 0x4B, 0x03, 0x04 }, "book.epub")]
    public async Task AnyKindOfFile_IsServedAsAnAttachment_WithAGeneralBinaryTypeAndTheNoGuessHeader(byte[] content, string name)
    {
        InMemoryFileStore files = new(TimeProvider.System);
        files.Seed(FileArea.DigitalFile, Key, content, DateTimeOffset.UtcNow);

        await using DigitalFileDownload? download = await DigitalFileDelivery.OpenAsync(files, Key, name, Ct);

        Assert.NotNull(download);
        Assert.Equal("application/octet-stream", DigitalFileDelivery.ContentType);
        Assert.Equal("nosniff", DigitalFileDelivery.NoSniff);
        Assert.StartsWith("attachment", download.ContentDisposition, StringComparison.Ordinal);
        Assert.DoesNotContain("inline", download.ContentDisposition, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(name, download.ContentDisposition, StringComparison.Ordinal);
        Assert.Equal(content.Length, download.Length);
        using MemoryStream copy = new();
        await download.Content.CopyToAsync(copy, Ct);
        Assert.Equal(content, copy.ToArray());
    }

    [Fact]
    public async Task AFileThatIsNotThere_GivesNoDownload()
    {
        InMemoryFileStore files = new(TimeProvider.System);

        Assert.Null(await DigitalFileDelivery.OpenAsync(files, Key, "x.zip", Ct));
    }

    [Fact]
    public async Task AKeyOfAnotherShape_IsRefusedBeforeAnythingIsOpened()
    {
        InMemoryFileStore files = new(TimeProvider.System);

        await Assert.ThrowsAsync<ArgumentException>(() => DigitalFileDelivery.OpenAsync(files, "../secret", "x.zip", Ct));
    }

    [Theory]
    [InlineData("report.pdf", "report.pdf")]
    [InlineData("C:\\Users\\me\\report.pdf", "report.pdf")]
    [InlineData("../../etc/passwd", "passwd")]
    [InlineData("a\"b;c.txt", "abc.txt")]
    [InlineData("line\r\nbreak: x.zip", "linebreak x.zip")]
    [InlineData(".htaccess", "htaccess")]
    [InlineData("  ", "download")]
    [InlineData("///", "download")]
    [InlineData(null, "download")]
    public void TheFileName_IsMadeSafeForAHeader(string? given, string expected)
    {
        Assert.Equal(expected, DigitalFileDelivery.SafeFileName(given));
    }

    [Fact]
    public void ALongFileName_IsShortened_AndKeepsItsExtension()
    {
        string name = DigitalFileDelivery.SafeFileName(new string('a', 400) + ".zip");

        Assert.Equal(150, name.Length);
        Assert.EndsWith(".zip", name, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ANameOutsideAscii_GoesInTheStarForm_AndNeverBreaksTheHeaderLine()
    {
        InMemoryFileStore files = new(TimeProvider.System);
        files.Seed(FileArea.DigitalFile, Key, [1], DateTimeOffset.UtcNow);

        await using DigitalFileDownload? download = await DigitalFileDelivery.OpenAsync(files, Key, "Çalışma Kılavuzu.pdf", Ct);

        Assert.NotNull(download);
        Assert.Contains("filename*=UTF-8''", download.ContentDisposition, StringComparison.Ordinal);
        Assert.DoesNotContain('\n', download.ContentDisposition);
        Assert.DoesNotContain('\r', download.ContentDisposition);
    }

    [Fact]
    public void TheCeilingIsP26_AFileOfExactlyThatSizeIsWithin_AndOneByteMoreIsRefusedByName()
    {
        DigitalFileSizeResult atCeiling = DigitalFileDelivery.CheckSize(ProductConstants.DigitalFileMaxBytes);
        DigitalFileSizeResult over = DigitalFileDelivery.CheckSize(ProductConstants.DigitalFileMaxBytes + 1);

        Assert.False(atCeiling.IsRejected);
        Assert.Same(DigitalFileSizeResult.Within, atCeiling);
        Assert.True(over.IsRejected);
        Assert.Same(DigitalFileSizeResult.TooLarge, over);
        Assert.Equal("TooLarge", over.Reason);
        Assert.False(DigitalFileDelivery.CheckSize(0).IsRejected);
        Assert.Throws<ArgumentOutOfRangeException>(() => DigitalFileDelivery.CheckSize(-1));
    }
}
