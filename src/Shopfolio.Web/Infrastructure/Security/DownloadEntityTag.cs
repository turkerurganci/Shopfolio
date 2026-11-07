using System.Security.Cryptography;
using System.Text;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The <c>ETag</c> of a digital file's download (09 15.7 item 2; 05 7.7.1): strong (no <c>W/</c> prefix) and made from the file row - the first 16 bytes of the
/// SHA-256 digest of the storage key and the size, as lower case hexadecimal text. The name on the disk never shows in a header, and a changed file is a changed row
/// (a new key), so the tag changes with it and an <c>If-Range</c> that no longer matches gets the whole file. It lives in the security folder because it names the digest
/// function (09 15.1 item 1: the digest functions are named there only).
/// </summary>
internal static class DownloadEntityTag
{
    private const int DigestBytes = 16;

    /// <summary>The tag's value without the quotes: 32 hexadecimal characters.</summary>
    public static string Value(string storageKey, long sizeBytes)
    {
        ArgumentException.ThrowIfNullOrEmpty(storageKey);
        byte[] digest = SHA256.HashData(Encoding.UTF8.GetBytes(storageKey + ":" + sizeBytes.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        return Convert.ToHexStringLower(digest.AsSpan(0, DigestBytes));
    }
}
