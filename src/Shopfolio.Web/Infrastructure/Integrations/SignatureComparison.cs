using System.Security.Cryptography;
using System.Text;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The one comparison of a computed signature with the signature that came in (08 3.2 principle 1; 09 11.1 item 5): fixed time, through
/// <see cref="CryptographicOperations.FixedTimeEquals"/>, so the time it takes does not depend on how many leading characters match. The link
/// layer compares a signature only here; the source scan keeps any other comparison of a signature out of the layer.
/// </summary>
internal static class SignatureComparison
{
    /// <summary>
    /// True when <paramref name="received"/> is the same text as <paramref name="computed"/>, byte by byte in UTF-8. A missing or empty received
    /// signature never matches. Only the length can be told apart by time; the length of a signature is its algorithm's and no secret.
    /// </summary>
    public static bool Matches(string computed, string? received)
    {
        ArgumentException.ThrowIfNullOrEmpty(computed);
        if (string.IsNullOrEmpty(received))
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(computed), Encoding.UTF8.GetBytes(received));
    }
}
