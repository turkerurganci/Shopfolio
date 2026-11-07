using System.Reflection;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog;
using Shopfolio.UnitTests.Architecture;

namespace Shopfolio.UnitTests.Catalog;

/// <summary>
/// The product's text (T339 criteria 7-9; KK7-KK9; 02 3.11.5-3.11.7; 06 2.10): the name has a ceiling that is the product constant and the description none; the description is written only
/// through the one function that cleans it, so a format outside the closed set falls, the paragraphs and line breaks stay and the stored text is the cleaned one; and the application has no
/// warranty field - the warranty is text the firm writes in the description.
/// </summary>
public sealed class ProductTextTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    private static Product NewProduct() => Product.Create("Pamuk Tişört", "pamuk-tisort", Modules.Catalog.Contracts.ProductType.Physical, 20m, Now);

    [Fact]
    public void TheDescription_IsWrittenOnlyThroughTheCleaningFunction_ItsSetterIsNotPublic()
    {
        PropertyInfo property = typeof(Product).GetProperty(nameof(Product.Description))!;

        Assert.NotNull(property.GetMethod);
        Assert.True(property.SetMethod is null || !property.SetMethod.IsPublic);
    }

    [Fact]
    public void ADescription_IsCleanedWhenItIsSet_AndTheStoredTextIsTheCleanedOne()
    {
        Product product = NewProduct();

        product.SetDescription("<p><strong>Kalın</strong> <b>eğik değil</b> <script>kod()</script><a href=\"javascript:x()\">bağ</a></p><h1>Büyük</h1>");

        Assert.Equal("<p><strong>Kalın</strong> eğik değil <a rel=\"noopener noreferrer\">bağ</a></p>Büyük", product.Description);
    }

    [Fact]
    public void AFormatOutsideTheSet_Falls_WhileParagraphsAndLineBreaksStay()
    {
        Product product = NewProduct();

        product.SetDescription("<p>İlk paragraf.</p><p>İkinci<br>satır sonu.</p><div>blok</div><span style=\"color:red\">renk</span>");

        Assert.Equal("<p>İlk paragraf.</p><p>İkinci<br>satır sonu.</p>blokrenk", product.Description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("<script>x()</script>")]
    public void AnEmptyDescription_IsStoredAsNoDescription(string? description)
    {
        Product product = NewProduct();
        product.SetDescription("<p>önce</p>");

        product.SetDescription(description);

        Assert.Null(product.Description);
    }

    [Fact]
    public void TheDescription_HasNoLengthCeiling()
    {
        Product product = NewProduct();
        string long100K = "<p>" + new string('a', 100_000) + "</p>";

        product.SetDescription(long100K);

        Assert.Equal(long100K, product.Description);
    }

    [Fact]
    public void TheNameCeiling_IsTheProductConstant_AndTheModelCarriesIt()
    {
        Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, "Host=127.0.0.1;Database=never_opened");
        using AppDbContext context = new(builder.Options);
        Microsoft.EntityFrameworkCore.Metadata.IEntityType entity = context.Model.FindEntityType(typeof(Product))!;

        Assert.Equal(ProductConstants.ProductNameMaxLength, entity.FindProperty(nameof(Product.Name))!.GetMaxLength());
        Assert.Null(entity.FindProperty(nameof(Product.Description))!.GetMaxLength());
    }

    [Fact]
    public void NoTypeMemberOrParameterOfTheApplication_IsAWarrantyOrAGuarantee()
    {
        List<string> findings = [.. SalesModelShape.NamesOf(typeof(Product).Assembly).Where(n => IsWarrantyName(n.Name)).Select(n => n.Owner)];

        Assert.Empty(findings);
    }

    [Theory]
    [InlineData("WarrantyPeriod")]
    [InlineData("warranty_terms")]
    [InlineData("GuaranteeDocument")]
    [InlineData("GarantiSuresi")]
    [InlineData("HasWarranty")]
    [InlineData("warranty")]
    public void TheWordScan_FindsAWarrantyInAnySpelling(string name)
    {
        Assert.True(IsWarrantyName(name));
    }

    [Theory]
    [InlineData("Warrant")]
    [InlineData("DescriptionText")]
    [InlineData("Variant")]
    public void TheWordScan_DoesNotMistakeAnOrdinaryName(string name)
    {
        Assert.False(IsWarrantyName(name));
    }

    // A name is a warranty name when one of its words is a stem of the field the product does not have (02 3.11.7): the English words and the Turkish "garanti" (which "garantisi" and the like start with).
    private static bool IsWarrantyName(string name)
        => SalesModelShape.Words(name).Any(w => w is "warranty" or "warranties" or "guarantee" or "guarantees" || w.StartsWith("garanti", StringComparison.Ordinal));
}
