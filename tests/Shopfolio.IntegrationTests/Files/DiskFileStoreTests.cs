using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Files;

/// <summary>
/// The disk file store on a real directory (T223 criteria 1-4; 09 6 item 6; 05 7.7.1): the key shape refused before any path is made, a write that
/// never shows a half file, a replacement, the append of the digital section, the listing by age without the temporary files, and the two sections
/// kept apart. Each test has a directory of its own, deleted after.
/// </summary>
public sealed class DiskFileStoreTests : IDisposable
{
    private static readonly string Fingerprint = new('a', FileKeys.ImageFingerprintLength);

    private readonly string _root = Path.Combine(Path.GetTempPath(), "shopfolio-t223-disk-" + Guid.NewGuid().ToString("N"));
    private readonly ManualTimeProvider _time = new(DateTimeOffset.UtcNow);
    private readonly DiskFileStore _store;

    public DiskFileStoreTests()
    {
        _store = new DiskFileStore(new FileStoreDirectories(Path.Combine(_root, "images"), Path.Combine(_root, "digital")), _time);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    public async Task AWrite_CanBeReadBack_AndAWriteOfTheSameKeyReplacesIt()
    {
        await WriteAsync(FileArea.Image, Fingerprint, [1, 2, 3]);
        await WriteAsync(FileArea.Image, Fingerprint, [9, 9]);

        await using Stream stream = await _store.OpenReadAsync(FileArea.Image, Fingerprint, Ct);
        using MemoryStream copy = new();
        await stream.CopyToAsync(copy, Ct);

        Assert.Equal<byte>([9, 9], copy.ToArray());
        Assert.Equal(2L, await _store.GetSizeAsync(FileArea.Image, Fingerprint, Ct));
        Assert.True(await _store.ExistsAsync(FileArea.Image, Fingerprint, Ct));
    }

    [Fact]
    public async Task AWriteThatFailsHalfway_LeavesNoFileAndNoTemporaryFile()
    {
        await WriteAsync(FileArea.Image, Fingerprint, [1, 2, 3]);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _store.WriteAsync(FileArea.Image, Fingerprint, new FailingStream(), Ct));

        Assert.Equal(3L, await _store.GetSizeAsync(FileArea.Image, Fingerprint, Ct));
        Assert.Equal([Fingerprint], Directory.EnumerateFiles(Path.Combine(_root, "images")).Select(Path.GetFileName));
    }

    [Fact]
    public async Task AWriteThatFailsForAKeyWithNoFile_LeavesTheDirectoryEmpty()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => _store.WriteAsync(FileArea.Image, Fingerprint, new FailingStream(), Ct));

        Assert.False(await _store.ExistsAsync(FileArea.Image, Fingerprint, Ct));
        Assert.Empty(Directory.EnumerateFiles(Path.Combine(_root, "images")));
    }

    [Theory]
    [InlineData("")]
    [InlineData("../etc/passwd")]
    [InlineData("..\\secret")]
    [InlineData("a/b")]
    [InlineData("ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789")]
    [InlineData("aaaa")]
    [InlineData(".tmp-abc")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\n")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa-480\n")]
    public async Task AKeyThatDoesNotFitTheImageSection_IsRefused_ByEveryOperation_BeforeAPathIsMade(string key)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => WriteAsync(FileArea.Image, key, [1]));
        await Assert.ThrowsAsync<ArgumentException>(() => _store.OpenReadAsync(FileArea.Image, key, Ct));
        await Assert.ThrowsAsync<ArgumentException>(() => _store.ExistsAsync(FileArea.Image, key, Ct));
        await Assert.ThrowsAsync<ArgumentException>(() => _store.GetSizeAsync(FileArea.Image, key, Ct));
        await Assert.ThrowsAsync<ArgumentException>(() => _store.DeleteAsync(FileArea.Image, key, Ct));
        Assert.False(Directory.Exists(Path.Combine(_root, "images")));
    }

    [Fact]
    public async Task ADigitalFileKey_IsNotAnImageKey_AndAnImageKeyIsNotADigitalFileKey()
    {
        string digital = new('b', 32);

        await Assert.ThrowsAsync<ArgumentException>(() => WriteAsync(FileArea.DigitalFile, digital + "\n", [1]));
        await Assert.ThrowsAsync<ArgumentException>(() => WriteAsync(FileArea.Image, digital, [1]));
        await Assert.ThrowsAsync<ArgumentException>(() => WriteAsync(FileArea.DigitalFile, Fingerprint, [1]));
        await WriteAsync(FileArea.DigitalFile, digital, [1]);

        Assert.True(await _store.ExistsAsync(FileArea.DigitalFile, digital, Ct));
        Assert.Single(Directory.EnumerateFiles(Path.Combine(_root, "digital")));
        Assert.False(Directory.Exists(Path.Combine(_root, "images")));
    }

    [Fact]
    public async Task AnAppend_ExtendsADigitalFile_AndIsRefusedForAnImage()
    {
        string digital = new('c', 32);

        await AppendAsync(FileArea.DigitalFile, digital, [1, 2]);
        await AppendAsync(FileArea.DigitalFile, digital, [3]);

        Assert.Equal(3L, await _store.GetSizeAsync(FileArea.DigitalFile, digital, Ct));
        await Assert.ThrowsAsync<ArgumentException>(() => AppendAsync(FileArea.Image, Fingerprint, [1]));
    }

    [Fact]
    public async Task ADeleteOfAMissingFile_IsNotAnError_AndSizeOfAMissingFileIsZero()
    {
        await _store.DeleteAsync(FileArea.Image, Fingerprint, Ct);

        Assert.Equal(0L, await _store.GetSizeAsync(FileArea.Image, Fingerprint, Ct));
    }

    [Fact]
    public async Task TheListing_ReturnsFilesAtLeastThatOld_AndNeverTheTemporaryFilesOrForeignNames()
    {
        string old = new('1', 64);
        string young = new('2', 64);
        await WriteAsync(FileArea.Image, old, [1]);
        await WriteAsync(FileArea.Image, young, [2]);
        File.SetLastWriteTimeUtc(Path.Combine(_root, "images", old), _time.GetUtcNow().AddDays(-2).UtcDateTime);
        File.SetLastWriteTimeUtc(Path.Combine(_root, "images", young), _time.GetUtcNow().AddHours(-1).UtcDateTime);
        await File.WriteAllBytesAsync(Path.Combine(_root, "images", ".tmp-halfway"), [1], Ct);
        await File.WriteAllBytesAsync(Path.Combine(_root, "images", "notes.txt"), [1], Ct);
        File.SetLastWriteTimeUtc(Path.Combine(_root, "images", ".tmp-halfway"), _time.GetUtcNow().AddDays(-9).UtcDateTime);
        File.SetLastWriteTimeUtc(Path.Combine(_root, "images", "notes.txt"), _time.GetUtcNow().AddDays(-9).UtcDateTime);

        List<string> listed = [];
        await foreach (string key in _store.ListAsync(FileArea.Image, TimeSpan.FromDays(1), Ct))
        {
            listed.Add(key);
        }

        Assert.Equal([old], listed);
    }

    [Fact]
    public async Task TheListing_OfADirectoryThatIsNotThereYet_IsEmpty()
    {
        List<string> listed = [];
        await foreach (string key in _store.ListAsync(FileArea.DigitalFile, TimeSpan.Zero, Ct))
        {
            listed.Add(key);
        }

        Assert.Empty(listed);
    }

    // ---- KK8: the digital product file goes out as an attachment; the ceiling P-26 is refused ----

    [Fact]
    public async Task ADigitalFileOfAnyContent_IsServedFromTheDisk_AsAnAttachmentOfAGeneralBinaryType()
    {
        string key = new('d', 32);
        byte[] html = "<html><script>alert(1)</script></html>"u8.ToArray();
        await WriteAsync(FileArea.DigitalFile, key, html);

        await using DigitalFileDownload? download = await DigitalFileDelivery.OpenAsync(_store, key, "..\\..\\evil \"page\".html", Ct);

        Assert.NotNull(download);
        Assert.Equal("application/octet-stream", DigitalFileDelivery.ContentType);
        Assert.Equal("nosniff", DigitalFileDelivery.NoSniff);
        Assert.StartsWith("attachment; filename=\"evil page.html\"", download.ContentDisposition, StringComparison.Ordinal);
        Assert.Equal(html.Length, download.Length);
        using MemoryStream copy = new();
        await download.Content.CopyToAsync(copy, Ct);
        Assert.Equal(html, copy.ToArray());
        Assert.Null(await DigitalFileDelivery.OpenAsync(_store, new string('e', 32), "x.zip", Ct));
    }

    [Fact]
    public async Task ADigitalFileOverTheCeiling_IsRefusedByName_AndOneOfExactlyTheCeilingIsNot()
    {
        string atCeiling = new('1', 32);
        string overCeiling = new('2', 32);
        Directory.CreateDirectory(Path.Combine(_root, "digital"));
        await using (FileStream file = new(Path.Combine(_root, "digital", atCeiling), FileMode.CreateNew))
        {
            file.SetLength(ProductConstants.DigitalFileMaxBytes);
        }

        await using (FileStream file = new(Path.Combine(_root, "digital", overCeiling), FileMode.CreateNew))
        {
            file.SetLength(ProductConstants.DigitalFileMaxBytes + 1);
        }

        long atSize = await _store.GetSizeAsync(FileArea.DigitalFile, atCeiling, Ct);
        long overSize = await _store.GetSizeAsync(FileArea.DigitalFile, overCeiling, Ct);

        Assert.False(DigitalFileDelivery.CheckSize(atSize).IsRejected);
        Assert.Same(DigitalFileSizeResult.TooLarge, DigitalFileDelivery.CheckSize(overSize));
    }

    private Task WriteAsync(FileArea area, string key, byte[] content)
    {
        using MemoryStream stream = new(content, writable: false);
        return _store.WriteAsync(area, key, stream, Ct);
    }

    private Task AppendAsync(FileArea area, string key, byte[] content)
    {
        using MemoryStream stream = new(content, writable: false);
        return _store.AppendAsync(area, key, stream, Ct);
    }

    /// <summary>A stream that gives a few bytes and then fails, as a lost connection does.</summary>
    private sealed class FailingStream : Stream
    {
        private bool _gave;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_gave)
            {
                throw new InvalidOperationException("The connection was lost.");
            }

            _gave = true;
            buffer.Span[0] = 7;
            return ValueTask.FromResult(1);
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
