using System.Security.Cryptography;
using System.Text;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The PayTR signature (08 2.1 "Kimlik doğrulama", 2.1.5): HMAC-SHA256 of the concatenated inputs with <c>merchant_key</c>, in Base64; both are read as UTF-8. Every
/// request's <c>paytr_token</c> and the notification's <c>hash</c> are this one function over their own inputs. It is the one file of the link layer that names the
/// digest function (the closed exception list of the source scan, 09 15.1 item 1); a comparison with an incoming value goes through
/// <see cref="SignatureComparison.Matches"/>, never an equality here.
/// </summary>
internal static class PayTrSignature
{
    public static string Of(string merchantKey, string message)
    {
        ArgumentNullException.ThrowIfNull(merchantKey);
        ArgumentNullException.ThrowIfNull(message);
        return Convert.ToBase64String(HMACSHA256.HashData(Encoding.UTF8.GetBytes(merchantKey), Encoding.UTF8.GetBytes(message)));
    }
}
