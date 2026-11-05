using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.InstallationSettings;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The key of an account link, derived and never stored (09 15.1 item 4; 05 6.4.4; K-1081): the HMAC-SHA256 of the link record's id (the
/// 32-character form without hyphens, in UTF-8) under the derivation secret, as unpadded Base64Url text. The secret is the installation setting
/// <see cref="LinkKeyOptions.DerivationSecret"/>, made by the cryptographic generator at the installation (05 6.x.20). The stored value is the
/// key's digest (<see cref="DigestFor"/>, which the record's creation and the start-up rewrite use and which never shows the key); the key
/// itself (<see cref="Derive"/>) is made only where an e-mail body is made (Modules/Notifications; a source scan keeps every other caller out)
/// and the sending retry derives it again. The path that checks an arriving key never reads the secret: it digests the arriving key and
/// searches the digest.
/// </summary>
internal sealed class LinkKeys
{
    private readonly IOptions<LinkKeyOptions> _options;

    private byte[]? _secret;

    public LinkKeys(IOptions<LinkKeyOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <summary>The key of the link record: the same id and secret give the same key, and another secret gives another key.</summary>
    public string Derive(Guid linkRecordId) => Derive(Secret(), linkRecordId);

    /// <summary>The digest of the link record's key (<see cref="SecretKey.Hash"/> of <see cref="Derive(Guid)"/>): what the record stores.</summary>
    public string DigestFor(Guid linkRecordId) => SecretKey.Hash(Derive(linkRecordId));

    /// <summary>The derivation with an explicit secret; the pure form the tests use.</summary>
    internal static string Derive(byte[] secret, Guid linkRecordId)
    {
        ArgumentNullException.ThrowIfNull(secret);
        byte[] id = Encoding.UTF8.GetBytes(linkRecordId.ToString("N"));
        return Base64Url.EncodeToString(HMACSHA256.HashData(secret, id));
    }

    // Read on first use, so a singleton made before the start-up check refuses a bad value never throws on its own.
    private byte[] Secret() => _secret ??= Convert.FromBase64String(_options.Value.DerivationSecret.Trim());
}

/// <summary>The registration of the link layer's services (T222).</summary>
internal static class LinkKeysRegistration
{
    public static IServiceCollection AddLinkRecords(this IServiceCollection services)
    {
        services.AddSingleton<LinkKeys>();
        services.AddScoped<LinkRecords>();
        return services;
    }
}
