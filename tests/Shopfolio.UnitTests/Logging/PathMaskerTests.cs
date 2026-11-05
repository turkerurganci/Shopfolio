using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.UnitTests.Logging;

/// <summary>
/// The path layer of the masking (T230, T231; 09 18.2 items 2b, 7). The table has one row for every keyed path (KK4: <c>KeyedPaths</c> is
/// the masker's own list, and <c>LoggingConsistencyTests</c> keeps it equal to the guidelines' list); the documented example (KK3); the paths that
/// do not change; the empty segments, the case and the query values.
/// </summary>
public sealed class PathMaskerTests
{
    private const string Value = "ABC123";

    public static TheoryData<string> KeyedPaths => [.. PathMasker.KeyedPaths];

    [Fact]
    public void Mask_TheDocumentedExample_MasksTheKeyAndTheQueryValue()
    {
        Assert.Equal("/siparis/erisim/{key}/teyit?email=*", PathMasker.Mask("/siparis/erisim/ABC123/teyit?email=a@b.com"));
    }

    [Theory]
    [MemberData(nameof(KeyedPaths))]
    public void Mask_EveryKeyedPath_ReplacesTheKeySegmentOnly(string keyedPath)
    {
        string path = keyedPath.Replace(PathMasker.KeyPlaceholder, Value, StringComparison.Ordinal);

        Assert.Equal(keyedPath, PathMasker.Mask(path));
        Assert.Equal(keyedPath + "/sonraki", PathMasker.Mask(path + "/sonraki"));
    }

    [Fact]
    public void Mask_QueryString_KeepsTheNamesAndReplacesEveryValue()
    {
        Assert.Equal("/ara?q=*&sayfa=*&bos=*", PathMasker.Mask("/ara?q=a@b.com&sayfa=2&bos="));
    }

    [Fact]
    public void Mask_AQueryTokenWithoutAnEqualsSign_IsReplacedWhole()
    {
        Assert.Equal("/ara?*", PathMasker.Mask("/ara?a@b.com"));
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/urunler")]
    [InlineData("/siparis/12345")]
    [InlineData("/siparis/erisim")]
    [InlineData("/indir/")]
    public void Mask_APathWithoutAKey_StaysAsItIs(string path)
    {
        Assert.Equal(path, PathMasker.Mask(path));
    }

    [Fact]
    public void Mask_AKeyedPathWithAQuery_MasksBoth()
    {
        Assert.Equal("/indir/{key}?token=*", PathMasker.Mask("/indir/" + Value + "?token=" + Value));
    }

    [Theory]
    [MemberData(nameof(KeyedPaths))]
    public void Mask_EveryKeyedPathWithEmptySegments_MasksTheKeyAndKeepsTheSegmentsAsTheyAre(string keyedPath)
    {
        // D-88: a path with an empty segment ('//') in front of or inside the prefix must not let the key through.
        string[] segments = keyedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        string doubled = "//" + string.Join("//", segments.Select(s => s == PathMasker.KeyPlaceholder ? Value : s));
        string expected = "//" + string.Join("//", segments);

        string masked = PathMasker.Mask(doubled);

        Assert.Equal(expected, masked);
        Assert.DoesNotContain(Value, masked, StringComparison.Ordinal);
    }

    [Fact]
    public void Mask_TheDocumentedExampleWithADoubleSlash_MasksTheKey()
    {
        Assert.Equal("//siparis/erisim/{key}/teyit?email=*", PathMasker.Mask("//siparis/erisim/ABC123/teyit?email=a@b.com"));
    }

    [Theory]
    [MemberData(nameof(KeyedPaths))]
    public void Mask_EveryKeyedPathInUpperCase_StillMasksTheKey(string keyedPath)
    {
        // The router matches without regard to case, so the masker does too; the key keeps no trace.
        string path = keyedPath.Replace(PathMasker.KeyPlaceholder, Value, StringComparison.Ordinal).ToUpperInvariant();

        Assert.DoesNotContain(Value, PathMasker.Mask(path), StringComparison.Ordinal);
        Assert.EndsWith(PathMasker.KeyPlaceholder, PathMasker.Mask(path), StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(KeyedPaths))]
    public void Mask_EveryKeyedPathWithoutItsKey_StaysAsItIs(string keyedPath)
    {
        string withoutKey = keyedPath[..keyedPath.LastIndexOf('/')];

        Assert.Equal(withoutKey, PathMasker.Mask(withoutKey));
        Assert.Equal(withoutKey + "/", PathMasker.Mask(withoutKey + "/"));
    }

    [Theory]
    [InlineData("/siparis/12345")]
    [InlineData("/siparis/12345/fatura")]
    [InlineData("/panel/siparisler/ABC123")]
    [InlineData("/urunler/ABC123?sayfa=2")]
    public void Mask_AMembersOrderPathAndOtherPathsWithoutAKeyedPrefix_KeepTheirSegments(string path)
    {
        string expected = path.Contains('?', StringComparison.Ordinal) ? path[..path.IndexOf('?', StringComparison.Ordinal)] + "?sayfa=*" : path;

        Assert.Equal(expected, PathMasker.Mask(path));
    }

    [Fact]
    public void Mask_AnEmptyQuery_KeepsTheQuestionMark()
    {
        Assert.Equal("/ara?", PathMasker.Mask("/ara?"));
    }
}
