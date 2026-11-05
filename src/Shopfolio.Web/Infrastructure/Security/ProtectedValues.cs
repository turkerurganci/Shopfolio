using System.Security.Cryptography;

using Microsoft.AspNetCore.DataProtection;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// Writes and reads the values that are not kept in a record and travel through the browser (09 15.5 items 2 and 3). There is one protector per
/// purpose of <see cref="DataProtectionPurposes"/>, created with that purpose's constant; the six calls below are the only <c>CreateProtector</c>
/// calls of the application (source scan). Reading never throws for a bad value: an empty, damaged, truncated or foreign value, a value protected
/// for another purpose and a value whose key is not in the key ring are all <see cref="ProtectedValueResult.Invalid"/>, and the caller handles
/// that as the value not having come.
/// </summary>
internal sealed class ProtectedValues
{
    private readonly Dictionary<string, IDataProtector> _protectors;

    public ProtectedValues(IDataProtectionProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _protectors = new Dictionary<string, IDataProtector>(StringComparer.Ordinal)
        {
            [DataProtectionPurposes.DownloadLink] = provider.CreateProtector(DataProtectionPurposes.DownloadLink),
            [DataProtectionPurposes.PendingRegistration] = provider.CreateProtector(DataProtectionPurposes.PendingRegistration),
            [DataProtectionPurposes.AcceptedInvitation] = provider.CreateProtector(DataProtectionPurposes.AcceptedInvitation),
            [DataProtectionPurposes.ExternalLoginFlow] = provider.CreateProtector(DataProtectionPurposes.ExternalLoginFlow),
            [DataProtectionPurposes.ExternalLoginState] = provider.CreateProtector(DataProtectionPurposes.ExternalLoginState),
            [DataProtectionPurposes.CheckoutCookie] = provider.CreateProtector(DataProtectionPurposes.CheckoutCookie),
        };
    }

    /// <summary>Protects a text payload for one purpose; the result is URL-safe Base64 and travels in an address or a hidden field as it is.</summary>
    public string Protect(string purpose, string payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return ProtectorFor(purpose).Protect(payload);
    }

    /// <summary>
    /// Reads a value for one purpose. A purpose outside the closed list is a programming error and throws; any fault of the value itself is
    /// <see cref="ProtectedValueResult.Invalid"/>.
    /// </summary>
    public ProtectedValueResult Unprotect(string purpose, string? value)
    {
        IDataProtector protector = ProtectorFor(purpose);
        if (string.IsNullOrEmpty(value))
        {
            return ProtectedValueResult.Invalid;
        }

        try
        {
            return ProtectedValueResult.Valid(protector.Unprotect(value));
        }
        catch (CryptographicException)
        {
            // Damaged, truncated, protected for another purpose, or protected with a key that is not in the ring (09 15.5 item 3).
            return ProtectedValueResult.Invalid;
        }
    }

    private IDataProtector ProtectorFor(string purpose)
    {
        ArgumentNullException.ThrowIfNull(purpose);
        return _protectors.TryGetValue(purpose, out IDataProtector? protector)
            ? protector
            : throw new ArgumentException("The purpose is not in the closed list of DataProtectionPurposes.", nameof(purpose));
    }
}
