using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.UnitTests.Logging;

/// <summary>
/// The path layer of the masking (T230; 09 18.2 item 2b). The table of every keyed path and the unchanged paths is T231's (KK3, KK4); here the
/// working of the masker: the documented example, each keyed prefix, the query values.
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
}
