using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The derived key of an account link (T222 KK2; 09 15.1 item 4; 05 6.4.4; K-1081): the same record id and secret give the same key, another
/// secret or another id gives another key, the key is the HMAC of the id's 32-character form under the secret, and the stored digest is the
/// key's digest. The secrets are made at run time (the repository's secret guard stops a literal next to a secret name).
/// </summary>
public sealed class LinkKeysTests
{
    private static byte[] NewSecret() => RandomNumberGenerator.GetBytes(LinkKeyOptions.DerivationSecretMinBytes);

    private static LinkKeys KeysFor(byte[] secret) =>
        new(Options.Create(new LinkKeyOptions { DerivationSecret = Convert.ToBase64String(secret) }));

    [Fact]
    public void Derive_SameIdAndSecret_GiveTheSameKey_EvenFromAnotherInstance()
    {
        byte[] secret = NewSecret();
        Guid id = Guid.CreateVersion7();

        string first = KeysFor(secret).Derive(id);

        Assert.Equal(first, KeysFor(secret).Derive(id));
        Assert.Equal(first, LinkKeys.Derive(secret, id));
    }

    [Fact]
    public void Derive_AnotherSecret_GivesAnotherKey()
    {
        Guid id = Guid.CreateVersion7();

        Assert.NotEqual(KeysFor(NewSecret()).Derive(id), KeysFor(NewSecret()).Derive(id));
    }

    [Fact]
    public void Derive_AnotherId_GivesAnotherKey()
    {
        LinkKeys keys = KeysFor(NewSecret());

        Assert.NotEqual(keys.Derive(Guid.CreateVersion7()), keys.Derive(Guid.CreateVersion7()));
    }

    [Fact]
    public void Derive_IsTheBase64UrlOfTheHmacOfTheIdWithoutHyphens()
    {
        byte[] secret = NewSecret();
        Guid id = Guid.CreateVersion7();
        byte[] mac = HMACSHA256.HashData(secret, Encoding.UTF8.GetBytes(id.ToString("N")));
        string expected = Convert.ToBase64String(mac).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        Assert.Equal(expected, KeysFor(secret).Derive(id));
        Assert.True(mac.Length * 8 >= 128, "05 6.4.4 asks for at least 128 bits.");
    }

    [Fact]
    public void DigestFor_IsTheDigestOfTheDerivedKey_AndNotTheKey()
    {
        LinkKeys keys = KeysFor(NewSecret());
        Guid id = Guid.CreateVersion7();

        string digest = keys.DigestFor(id);

        Assert.Equal(SecretKey.Hash(keys.Derive(id)), digest);
        Assert.NotEqual(keys.Derive(id), digest);
    }

    [Fact]
    public void ForCustomer_WithKeys_StoresTheDigestOfTheKeyDerivedFromItsOwnId()
    {
        LinkKeys keys = KeysFor(NewSecret());
        DateTimeOffset now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

        LinkRecord record = LinkRecord.ForCustomer(keys, Shopfolio.Infrastructure.Values.LinkType.PasswordReset, Guid.CreateVersion7(), now.AddHours(1), now);

        Assert.Equal(keys.DigestFor(record.Id), record.TokenHash);
        Assert.Equal(SecretKey.Hash(keys.Derive(record.Id)), record.TokenHash);
    }
}
