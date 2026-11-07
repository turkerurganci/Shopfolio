using Shopfolio.Endpoints.Files;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The <c>Content-Range</c> of a chunk (T312 criterion 8; KK8; 07 4.31.2; 09 15.7 item 6 (b)): <c>bytes from-to</c> with the total ignored, well formed only when it is the one
/// header, the span is not empty and it is the length of the body that came with it; and the download's strong <c>ETag</c> (09 15.7 item 2; KK3).
/// </summary>
public sealed class ChunkRangeTests
{
    [Theory]
    [InlineData("bytes 0-4/12", 5, 0)]
    [InlineData("bytes 5-11/12", 7, 5)]
    [InlineData("bytes 5-11/*", 7, 5)]
    [InlineData("bytes 5-11", 7, 5)]
    [InlineData("BYTES 5-11/999999", 7, 5)]
    [InlineData("bytes 5242880-10485759/20971520", 5242880, 5242880)]
    [InlineData("bytes 7-7/8", 1, 7)]
    public void WellFormedHeader_WhoseSpanIsTheBodysLength_GivesTheStart_TheTotalIsIgnored(string header, long bodyLength, long expectedStart)
    {
        Assert.True(ChunkRange.TryParse([header], bodyLength, out long start));
        Assert.Equal(expectedStart, start);
    }

    [Theory]
    [InlineData("bytes 0-4/12", 4)]
    [InlineData("bytes 0-4/12", 6)]
    [InlineData("bytes 0-4/12", 0)]
    [InlineData("bytes 4-0/12", 5)]
    [InlineData("bytes 0-/12", 5)]
    [InlineData("bytes -4/12", 5)]
    [InlineData("bytes */12", 5)]
    [InlineData("bytes 0-4/", 5)]
    [InlineData("bytes 0-4/abc", 5)]
    [InlineData("items 0-4/12", 5)]
    [InlineData("bytes0-4/12", 5)]
    [InlineData("bytes  0-4/12", 5)]
    [InlineData(" bytes 0-4/12", 5)]
    [InlineData("bytes 0-4/12 ", 5)]
    [InlineData("bytes 0-4, 5-9/12", 5)]
    [InlineData("bytes -1-4/12", 5)]
    [InlineData("bytes 0-99999999999999999999/12", 5)]
    [InlineData("bytes 1.5-4/12", 5)]
    [InlineData("", 5)]
    public void MalformedHeaderOrAnotherLength_IsRefused(string header, long bodyLength)
    {
        Assert.False(ChunkRange.TryParse([header], bodyLength, out long start));
        Assert.Equal(0, start);
    }

    [Fact]
    public void NoHeaderANullValueOrTwoHeaders_IsRefused()
    {
        Assert.False(ChunkRange.TryParse([], 5, out _));
        Assert.False(ChunkRange.TryParse([null], 5, out _));
        Assert.False(ChunkRange.TryParse(["bytes 0-4/12", "bytes 0-4/12"], 5, out _));
    }

    [Fact]
    public void EntityTag_IsThirtyTwoLowerCaseHexCharacters_AndStable()
    {
        string tag = DownloadEntityTag.Value("0123456789abcdef0123456789abcdef", 12);

        Assert.Matches("^[0-9a-f]{32}$", tag);
        Assert.Equal(tag, DownloadEntityTag.Value("0123456789abcdef0123456789abcdef", 12));
    }

    [Fact]
    public void EntityTag_ChangesWithTheKeyAndWithTheSize_AndNeverHoldsTheKey()
    {
        const string key = "0123456789abcdef0123456789abcdef";
        string tag = DownloadEntityTag.Value(key, 12);

        Assert.NotEqual(tag, DownloadEntityTag.Value("0123456789abcdef0123456789abcdee", 12));
        Assert.NotEqual(tag, DownloadEntityTag.Value(key, 13));
        Assert.DoesNotContain(key, tag, StringComparison.Ordinal);
    }

    [Fact]
    public void EntityTag_OfNoKey_IsAProgrammingError()
    {
        Assert.Throws<ArgumentException>(() => DownloadEntityTag.Value(string.Empty, 1));
    }
}
