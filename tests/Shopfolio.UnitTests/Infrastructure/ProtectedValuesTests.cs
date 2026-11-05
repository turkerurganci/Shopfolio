using Microsoft.AspNetCore.DataProtection;

using Shopfolio.Infrastructure.Security;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The protected values (T232; 09 15.5 items 2, 3 and 7): a round trip per purpose, the 6x5 cross reading refused, every kind of bad value
/// answered with Invalid and never an exception, and a changed purpose suffix refusing the old value. The ring is the framework's in-memory
/// provider (no file, no database); a second provider is a second ring with other keys.
/// </summary>
public sealed class ProtectedValuesTests
{
    public static TheoryData<string> Purposes => [.. DataProtectionPurposes.All];

    /// <summary>Every ordered pair of different purposes, built from the closed list (6 x 5 = 30 pairs while the list holds six).</summary>
    public static TheoryData<string, string> CrossPairs
    {
        get
        {
            TheoryData<string, string> pairs = [];
            foreach (string written in DataProtectionPurposes.All)
            {
                foreach (string read in DataProtectionPurposes.All.Where(p => p != written))
                {
                    pairs.Add(written, read);
                }
            }

            return pairs;
        }
    }

    private static ProtectedValues Create(IDataProtectionProvider? provider = null) => new(provider ?? new EphemeralDataProtectionProvider());

    [Fact]
    public void Purposes_AreTheClosedList_EachShapedShopfolioUseV1AndDistinct()
    {
        Assert.Equal(6, DataProtectionPurposes.All.Count);
        Assert.Equal(DataProtectionPurposes.All.Count, DataProtectionPurposes.All.Distinct(StringComparer.Ordinal).Count());
        Assert.All(DataProtectionPurposes.All, purpose => Assert.Matches(@"^Shopfolio\.[A-Z][A-Za-z]+\.v1$", purpose));
        string[] constants = [.. typeof(DataProtectionPurposes).GetFields()
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)];
        Assert.Equal(constants.Order(StringComparer.Ordinal), DataProtectionPurposes.All.Order(StringComparer.Ordinal));
    }

    [Theory]
    [MemberData(nameof(Purposes))]
    public void RoundTrip_ForEachPurpose_GivesTheSamePayload(string purpose)
    {
        ProtectedValues values = Create();

        string value = values.Protect(purpose, "payload-" + purpose);
        ProtectedValueResult result = values.Unprotect(purpose, value);

        Assert.True(result.IsValid);
        Assert.Equal("payload-" + purpose, result.Payload);
        Assert.Matches(@"^[A-Za-z0-9_-]+$", value);
    }

    [Theory]
    [MemberData(nameof(CrossPairs))]
    public void Unprotect_UnderAnotherPurpose_IsInvalid(string written, string read)
    {
        ProtectedValues values = Create();
        string value = values.Protect(written, "payload");

        ProtectedValueResult result = values.Unprotect(read, value);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void CrossPairs_CoverEveryOrderedPairOfDifferentPurposes()
    {
        int count = DataProtectionPurposes.All.Count;

        Assert.Equal(count * (count - 1), CrossPairs.Count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("not-a-protected-value")]
    [InlineData("!!!???")]
    [InlineData("A")]
    public void Unprotect_OnEmptyOrDamagedText_IsInvalidAndDoesNotThrow(string? value)
    {
        ProtectedValues values = Create();

        foreach (string purpose in DataProtectionPurposes.All)
        {
            Assert.False(values.Unprotect(purpose, value).IsValid);
        }
    }

    [Theory]
    [MemberData(nameof(Purposes))]
    public void Unprotect_OnTruncatedAndTamperedValue_IsInvalid(string purpose)
    {
        ProtectedValues values = Create();
        string value = values.Protect(purpose, "payload");

        string truncated = value[..(value.Length / 2)];
        char flipped = value[^5] == 'A' ? 'B' : 'A';
        string tampered = value[..^5] + flipped + value[^4..];

        Assert.False(values.Unprotect(purpose, truncated).IsValid);
        Assert.False(values.Unprotect(purpose, tampered).IsValid);
        Assert.False(values.Unprotect(purpose, value + "A").IsValid);
    }

    [Theory]
    [MemberData(nameof(Purposes))]
    public void Unprotect_OfAValueWhoseKeyIsNotInTheRing_IsInvalid(string purpose)
    {
        // The other provider is a ring with its own keys: the value names a key this ring does not hold.
        string value = Create().Protect(purpose, "payload");

        ProtectedValueResult result = Create().Unprotect(purpose, value);

        Assert.False(result.IsValid);
    }

    [Theory]
    [MemberData(nameof(Purposes))]
    public void Unprotect_OfAValueFromTheSameRing_IsValidAcrossTwoInstancesOfTheClass(string purpose)
    {
        EphemeralDataProtectionProvider ring = new();
        string value = Create(ring).Protect(purpose, "payload");

        ProtectedValueResult result = Create(ring).Unprotect(purpose, value);

        Assert.True(result.IsValid);
        Assert.Equal("payload", result.Payload);
    }

    [Theory]
    [MemberData(nameof(Purposes))]
    public void ChangedSuffix_RefusesTheOldValue_AndTheOldPurposeRefusesTheNewValue(string purpose)
    {
        EphemeralDataProtectionProvider ring = new();
        ProtectedValues values = Create(ring);
        string next = purpose.Replace(".v1", ".v2", StringComparison.Ordinal);
        Assert.NotEqual(purpose, next);
        IDataProtector nextProtector = ring.CreateProtector(next);
        string oldValue = values.Protect(purpose, "payload");

        Assert.ThrowsAny<System.Security.Cryptography.CryptographicException>(() => nextProtector.Unprotect(oldValue));
        Assert.False(values.Unprotect(purpose, nextProtector.Protect("payload")).IsValid);
    }

    [Fact]
    public void ProtectAndUnprotect_OnAPurposeOutsideTheList_ThrowAsAProgrammingError()
    {
        ProtectedValues values = Create();

        Assert.Throws<ArgumentException>(() => values.Protect("Shopfolio.Invented.v1", "payload"));
        Assert.Throws<ArgumentException>(() => values.Unprotect("Shopfolio.Invented.v1", "value"));
        Assert.Throws<ArgumentException>(() => values.Unprotect(string.Empty, null));
    }

    [Fact]
    public void Payload_OfAnInvalidResult_IsRefused()
    {
        Assert.False(ProtectedValueResult.Invalid.IsValid);
        Assert.Throws<InvalidOperationException>(() => ProtectedValueResult.Invalid.Payload);
        Assert.Equal("x", ProtectedValueResult.Valid("x").Payload);
    }

    [Fact]
    public void RoundTrip_OfAnEmptyPayload_IsValidAndEmpty()
    {
        ProtectedValues values = Create();

        ProtectedValueResult result = values.Unprotect(DataProtectionPurposes.CheckoutCookie, values.Protect(DataProtectionPurposes.CheckoutCookie, string.Empty));

        Assert.True(result.IsValid);
        Assert.Equal(string.Empty, result.Payload);
    }
}
