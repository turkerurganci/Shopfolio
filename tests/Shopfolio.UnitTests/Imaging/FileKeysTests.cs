using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;

namespace Shopfolio.UnitTests.Imaging;

/// <summary>The shape of a file key per section (T223 criterion 2; 09 6 item 6; 05 6.6.4): the application makes the key and nothing typed by a user reaches a path.</summary>
public sealed class FileKeysTests
{
    private static readonly string Fingerprint = new('b', FileKeys.ImageFingerprintLength);

    [Fact]
    public void ADigitalFileKey_IsTheRandomStorageKey_AndAnImageKeyAFingerprint()
    {
        Assert.True(FileKeys.IsValid(FileArea.DigitalFile, EntityIds.NewStorageKey()));
        Assert.True(FileKeys.IsValid(FileArea.Image, Fingerprint));
        Assert.True(FileKeys.IsValid(FileArea.Image, FileKeys.DerivativeKey(Fingerprint, 480)));
        Assert.NotEqual(EntityIds.NewStorageKey(), EntityIds.NewStorageKey());
    }

    [Theory]
    [InlineData("")]
    [InlineData("../secret")]
    [InlineData("..\\secret")]
    [InlineData("a/b")]
    [InlineData("report.pdf")]
    [InlineData("0123456789ABCDEF0123456789ABCDEF")]
    [InlineData("0123456789abcdef0123456789abcde")]
    public void AKeyOfAnotherShape_IsRefusedForTheDigitalSection(string key)
    {
        Assert.False(FileKeys.IsValid(FileArea.DigitalFile, key));
        Assert.Throws<ArgumentException>(() => FileKeys.Require(FileArea.DigitalFile, key));
    }

    [Theory]
    [InlineData("../")]
    [InlineData("-0")]
    [InlineData("-480x")]
    public void AKeyOfAnotherShape_IsRefusedForTheImageSection(string suffix)
    {
        Assert.False(FileKeys.IsValid(FileArea.Image, Fingerprint + suffix));
        Assert.False(FileKeys.IsValid(FileArea.Image, EntityIds.NewStorageKey()));
        Assert.False(FileKeys.IsValid(FileArea.Image, null));
    }

    [Fact]
    public void TheSourceKeyOfADerivativeKey_IsTheFingerprint()
    {
        Assert.Equal(Fingerprint, FileKeys.SourceKeyOf(FileKeys.DerivativeKey(Fingerprint, 960)));
        Assert.Equal(Fingerprint, FileKeys.SourceKeyOf(Fingerprint));
    }
}
