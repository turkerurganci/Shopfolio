using System.Security.Cryptography;

using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Catalog;

/// <summary>
/// The domestic production logo's decision and its file (T501 KK1-KK3, KK6, KK7; 02 3.8.5; 08 4.3, 4.1 DV-34; 09 11.1 item 7): the logo shows for a physical product whose place of origin is Turkey
/// and for nothing else; the decision reads the product's type and the country's code only; the origin line keeps the country's name with the logo and without it; the file is the Ministry's
/// announced one, embedded in the assembly, and it is the one DV-34 records - a replaced file fails here until the DV-34 row and the hash below are updated together.
/// </summary>
public sealed class DomesticProductionLogoTests
{
    /// <summary>The SHA-256 of the file as announced (DV-34, 2026-11-11); the DV-34 row in 08 4.1 writes the same value.</summary>
    private const string AnnouncedFileHash = "5316c50050b9be32d6ed8ed1c4547f1d1bec42e1adf00e7c0292e484273955b6";

    [Fact]
    public void TheLogo_ShowsForAPhysicalProductMadeInTurkey_AndForNothingElse_KK2()
    {
        string?[] codes = ["TR", "DE", "US", "CN", "tr", "TR ", "", null];

        foreach (ProductType type in Enum.GetValues<ProductType>())
        {
            foreach (string? code in codes)
            {
                bool expected = type == ProductType.Physical && code == "TR";

                Assert.Equal(expected, DomesticProductionLogo.Shows(type, code));
            }
        }
    }

    [Theory]
    [InlineData(ProductType.Physical, "TR", "Türkiye", true)]
    [InlineData(ProductType.Physical, "DE", "Almanya", false)]
    [InlineData(ProductType.Digital, "TR", "Türkiye", false)]
    [InlineData(ProductType.Digital, "DE", "Almanya", false)]
    public void TheOriginLine_KeepsTheCountrysName_WithTheLogoAndWithoutIt_KK3(ProductType type, string code, string name, bool showsLogo)
    {
        OriginLabel? label = OriginLabel.Of(type, code, name);

        Assert.Equal(new OriginLabel(name, showsLogo), label);
    }

    [Theory]
    [InlineData(ProductType.Service, "TR", "Türkiye")]
    [InlineData(ProductType.Service, null, null)]
    [InlineData(ProductType.Physical, null, null)]
    [InlineData(ProductType.Physical, "", "")]
    [InlineData(ProductType.Digital, null, null)]
    [InlineData(ProductType.Physical, "TR", null)]
    [InlineData(ProductType.Physical, "TR", " ")]
    public void TheOriginLine_IsAbsent_ForAServiceAndForAnEmptyPlaceOfOrigin_KK3(ProductType type, string? code, string? name)
    {
        Assert.Null(OriginLabel.Of(type, code, name));
    }

    [Fact]
    public void TheDecision_TakesTheProductTypeAndTheCountryCodeOnly_KK7()
    {
        System.Reflection.MethodInfo shows = typeof(DomesticProductionLogo).GetMethod(nameof(DomesticProductionLogo.Shows)) ?? throw new InvalidOperationException();

        Assert.Equal([typeof(ProductType), typeof(string)], shows.GetParameters().Select(p => p.ParameterType));
        Assert.Equal("TR", DomesticProductionLogo.TurkeyCountryCode);
    }

    [Fact]
    public void TheFile_IsEmbeddedInTheAssembly_IsAJpegAndIsTheOneDv34Records_KK1_KK6()
    {
        byte[] content = DomesticProductionLogo.Content.ToArray();

        Assert.True(content.Length > 0);
        Assert.Equal(new byte[] { 0xFF, 0xD8, 0xFF }, content[..3]);
        Assert.Equal(AnnouncedFileHash, Convert.ToHexStringLower(SHA256.HashData(content)));
        Assert.Equal("image/jpeg", DomesticProductionLogo.ContentType);
    }

    [Fact]
    public void TheEmbeddedFile_IsTheSourceFileOfTheRepositoryByteForByte_KK1()
    {
        string source = Path.Combine(RepositoryRoot.Find(), "src", "Shopfolio.Web", "Modules", "Catalog", "Assets", "DomesticProductionLogo.jpg");

        Assert.Equal(File.ReadAllBytes(source), DomesticProductionLogo.Content.ToArray());
    }

    [Fact]
    public void TheAddress_CarriesTenCharactersOfTheContentHash_UnderTheStaticArea_KK1()
    {
        string fingerprint = Convert.ToHexStringLower(SHA512.HashData(DomesticProductionLogo.Content.ToArray()))[..10];

        Assert.Equal($"/statik/yerli-uretim-logosu.{fingerprint}.jpg", DomesticProductionLogo.Address);
    }
}
