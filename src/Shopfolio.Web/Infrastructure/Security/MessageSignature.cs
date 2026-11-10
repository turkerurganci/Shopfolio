using System.Security.Cryptography;
using System.Text;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// A keyed message signature: HMAC-SHA256 of the message's UTF-8 bytes with the key's UTF-8 bytes, in Base64 (08 2.1 "Kimlik doğrulama",
/// 2.1.5: the outside payment service's request token and notification signature). It lives in this folder because the digest functions are
/// named only here and in <c>Modules/Membership/</c> (09 15.1 item 1); the link layer calls it and names no digest function itself. A
/// comparison with an incoming value is made in constant time by the caller, never with an equality here.
/// </summary>
internal static class MessageSignature
{
    public static string HmacSha256Base64(string key, string message)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(message);
        return Convert.ToBase64String(HMACSHA256.HashData(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(message)));
    }
}
