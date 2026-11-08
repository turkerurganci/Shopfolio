using System.Text.RegularExpressions;

using Shopfolio.Infrastructure;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// There is no wish list, no favourite button and no cart reminder e-mail (T347 criterion 7, KK7; 02 3.16.13): no source file names one, no address of the application is one, and no notice of
/// the Notifications module is about the cart. The source is scanned for the names a wish list, a favourite and a cart reminder would carry in English and Turkish, with the samples that
/// prove the pattern sees each form - and does not see the browser icon (<c>favicon</c>).
/// </summary>
public sealed partial class WishlistAbsenceScanTests
{
    [Fact]
    public void NoSourceFile_NamesAWishList_AFavouriteOrACartReminder_KK7()
    {
        string root = RepositoryRoot.Find();
        string sourceRoot = Path.Combine(root, "src");
        string[] files = [.. Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories)
            .Where(f => (f.EndsWith(".cs", StringComparison.Ordinal) || f.EndsWith(".cshtml", StringComparison.Ordinal))
                && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))];
        Assert.NotEmpty(files);

        List<string> findings = [];
        foreach (string file in files)
        {
            foreach (Match match in AbsentFeature().Matches(File.ReadAllText(file)))
            {
                findings.Add($"{Path.GetRelativePath(root, file)}: {match.Value}");
            }
        }

        Assert.Empty(findings);
    }

    [Fact]
    public void NoNoticeOfTheNotificationsModule_IsAboutTheCart_KK7()
    {
        Type[] contracts = [.. typeof(ProductConstants).Assembly.GetTypes().Where(t => t.Namespace == "Shopfolio.Modules.Notifications.Contracts")];
        Assert.NotEmpty(contracts);

        Assert.DoesNotContain(contracts, t => AbsentFeature().IsMatch(t.Name) || t.Name.Contains("Cart", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            contracts.Where(t => t.IsEnum).SelectMany(t => Enum.GetNames(t)),
            name => AbsentFeature().IsMatch(name) || name.Contains("Cart", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("class WishlistPage")]
    [InlineData("var wishList = new List<Guid>();")]
    [InlineData("AddToFavorites(variantId)")]
    [InlineData("services.AddScoped<IFavouriteService>();")]
    [InlineData("MapPost(\"/favori/ekle\")")]
    [InlineData("MapGet(\"/hesabim/istek-listesi\")")]
    [InlineData("class IstekListesiPage")]
    [InlineData("public sealed record CartReminderNotice(Guid CartId);")]
    [InlineData("var abandonedCarts = await db.Carts.ToListAsync();")]
    [InlineData("AbandonedCartJob")]
    [InlineData("SepetHatirlatma")]
    [InlineData("sepet hatırlatma e-postası")]
    public void ThePattern_SeesEachFormOfTheAbsentFeatures_KK7(string sample)
    {
        Assert.Matches(AbsentFeature(), sample);
    }

    [Theory]
    [InlineData("<link rel=\"icon\" href=\"/favicon.ico\" />")]
    [InlineData("order.CartId = null;")]
    [InlineData("public sealed record CartAddResult(CartAddOutcome Outcome, Guid? CartId);")]
    [InlineData("CartPaymentBlock.NoOrderableItem")]
    public void ThePattern_DoesNotSee_TheBrowserIconOrTheCartsOwnNames_KK7(string sample)
    {
        Assert.DoesNotMatch(AbsentFeature(), sample);
    }

    [GeneratedRegex(
        @"wish[-_ ]?list|favou?rite|favori|istek[-_ ]?listesi|cart[-_ ]?reminder|abandoned[-_ ]?cart|sepet[-_ ]?hat[ıi]rlat",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex AbsentFeature();
}
