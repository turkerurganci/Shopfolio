using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Security;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The key function (T222 KK1; 09 15.1 item 1; 05 6.4.1): the length and the character set of a made key, two keys that differ, and a stable
/// digest. The expected length is worked out from the constant (unpadded Base64Url of <see cref="SecretKey.KeyBytes"/> bytes), not copied.
/// </summary>
public sealed partial class SecretKeyTests
{
    // Unpadded Base64Url: 4 characters for every 3 bytes, the rest as many characters as the bits need.
    private static int ExpectedKeyLength => (SecretKey.KeyBytes * 4 + 2) / 3;

    [GeneratedRegex(@"^[A-Za-z0-9_-]+$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Base64UrlText();

    [GeneratedRegex(@"^[0-9a-f]+$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LowerHex();

    [Fact]
    public void Create_GivesAtLeast128Bits_AsUnpaddedBase64UrlOfTheConstantsSize()
    {
        string key = SecretKey.Create();

        Assert.True(SecretKey.KeyBytes * 8 >= 128, "05 6.4.4 asks for at least 128 bits.");
        Assert.Equal(ExpectedKeyLength, key.Length);
        Assert.Matches(Base64UrlText(), key);
        Assert.Equal(SecretKey.KeyBytes, Convert.FromBase64String(ToPaddedBase64(key)).Length);
    }

    [Fact]
    public void Create_TwoCalls_GiveTwoKeys()
    {
        HashSet<string> keys = [.. Enumerable.Range(0, 64).Select(_ => SecretKey.Create())];

        Assert.Equal(64, keys.Count);
    }

    [Fact]
    public void Hash_IsTheLowerCaseHexOfTheSha256OfTheUtf8Bytes_AndStable()
    {
        const string key = "a-key";
        string expected = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key))).ToLowerInvariant();

        string first = SecretKey.Hash(key);

        Assert.Equal(expected, first);
        Assert.Equal(first, SecretKey.Hash(key));
        Assert.Equal(SecretKey.HashLength, first.Length);
        Assert.Matches(LowerHex(), first);
    }

    [Fact]
    public void Hash_OfTwoKeys_Differs_AndNeverEqualsTheKey()
    {
        string a = SecretKey.Create();
        string b = SecretKey.Create();

        Assert.NotEqual(SecretKey.Hash(a), SecretKey.Hash(b));
        Assert.NotEqual(a, SecretKey.Hash(a));
    }

    [Theory]
    [InlineData("")]
    public void Hash_OfAnEmptyKey_IsRefused(string key)
    {
        Assert.Throws<ArgumentException>(() => SecretKey.Hash(key));
    }

    private static string ToPaddedBase64(string base64Url)
    {
        string text = base64Url.Replace('-', '+').Replace('_', '/');
        return text.PadRight(text.Length + ((4 - (text.Length % 4)) % 4), '=');
    }
}
