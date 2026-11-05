using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The one function that makes and digests a key (09 15.1 item 1; 05 6.4.1; MK-50): <see cref="Create"/> is 32 bytes of the operating system's
/// cryptographic generator as unpadded Base64Url text (256 bits; 05 6.4.4 asks for at least 128) and <see cref="Hash"/> is the lower-case
/// hexadecimal SHA-256 of the key's UTF-8 bytes. The digest is not a password hasher: the key has high entropy, so no salt and no slow digest.
/// A source scan keeps <c>RandomNumberGenerator</c>, <c>SHA256</c> and <c>HMACSHA256</c> out of every other folder (T222).
/// </summary>
internal static class SecretKey
{
    /// <summary>The size of a key before its text form, in bytes.</summary>
    public const int KeyBytes = 32;

    /// <summary>The length of <see cref="Hash"/>: 64 lower-case hexadecimal characters.</summary>
    public const int HashLength = 64;

    /// <summary>A new key from the operating system's cryptographic generator.</summary>
    public static string Create() => Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(KeyBytes));

    /// <summary>The digest of a key, in the form the key's record stores (a link record's <c>TokenHash</c>).</summary>
    public static string Hash(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
    }
}
