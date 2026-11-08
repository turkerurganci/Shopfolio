using System.ComponentModel.DataAnnotations;
using System.Reflection;

using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.UnitTests.CorporateContent;

/// <summary>
/// The rules of the brand identity that need no database (T362 criteria 1, 3, 4, 5, 6; KK1, KK3, KK4, KK5, KK6; 02 3.29; 06 4.54): the colour shape, the initial, the trail text limit,
/// the text colour the system chooses on the brand colour, and the closed lists of what the brand form and the stored row hold.
/// </summary>
public sealed class BrandingRulesTests
{
    private const double MinimumContrast = 4.5;

    // ---- The colour as it is stored ----

    [Theory]
    [InlineData("#1a56db", "#1A56DB")]
    [InlineData("#1A56DB", "#1A56DB")]
    [InlineData("  #ffff00 ", "#FFFF00")]
    public void AColour_IsStoredAsHashAndSixCapitalDigits(string raw, string expected)
    {
        Assert.True(BrandingRules.TryNormalizeColor(raw, out string? color));
        Assert.Equal(expected, color);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ABlankColour_IsNoColour_AndIsAccepted(string? raw)
    {
        Assert.True(BrandingRules.TryNormalizeColor(raw, out string? color));
        Assert.Null(color);
    }

    [Theory]
    [InlineData("#FFF")]
    [InlineData("FFFFFF")]
    [InlineData("#GGGGGG")]
    [InlineData("#12345")]
    [InlineData("#1234567")]
    [InlineData("#12345;")]
    [InlineData("#123456;color:red")]
    [InlineData("red")]
    [InlineData("rgb(1,2,3)")]
    [InlineData("#12 456")]
    [InlineData("#١٢٣٤٥٦")]
    public void AValueThatIsNotExactlyHashAndSixHexDigits_IsRefused(string raw)
    {
        Assert.False(BrandingRules.TryNormalizeColor(raw, out string? color));
        Assert.Null(color);
    }

    [Theory]
    [InlineData("#1a56db")]
    [InlineData("  #ffff00 ")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("#FFF")]
    [InlineData("#123456;color:red")]
    [InlineData("red")]
    public void AColourTheModuleStores_IsOneTheSingleRuleAccepts_AndARefusedOneIsRefusedByBoth(string raw)
    {
        bool stored = BrandingRules.TryNormalizeColor(raw, out string? color);
        string text = color ?? string.Empty;
        bool ruleAccepts = new InvalidColorAttribute().GetValidationResult(text, new ValidationContext(text)) is null;

        // What the module keeps always passes the one rule; what the module refuses is a value the rule refuses once trimmed.
        if (stored)
        {
            Assert.True(ruleAccepts);
        }
        else
        {
            string trimmed = raw.Trim();
            Assert.NotNull(new InvalidColorAttribute().GetValidationResult(trimmed, new ValidationContext(trimmed)));
        }
    }

    // ---- The name and the initial (KK3, KK4) ----

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData("  Ayşe Butik ", "Ayşe Butik")]
    public void TheBrandName_IsTrimmed_AndBlankIsNone(string? raw, string? expected)
    {
        Assert.Equal(expected, BrandingRules.NormalizeName(raw));
    }

    [Theory]
    [InlineData("ayşe butik", "A")]
    [InlineData("  istanbul", "İ")]
    [InlineData("ışık", "I")]
    [InlineData("Özgür", "Ö")]
    [InlineData("example.com", "E")]
    public void TheInitial_IsTheFirstLetterInCapitals_TheWayTheProductWritesTurkish(string name, string expected)
    {
        Assert.Equal(expected, BrandingRules.Initial(name));
    }

    [Fact]
    public void TheInitial_OfNothing_IsNothing_AndAPairOfCharactersIsNotSplit()
    {
        Assert.Equal(string.Empty, BrandingRules.Initial("   "));
        Assert.Equal("\U0001F600", BrandingRules.Initial("\U0001F600 shop"));
    }

    // ---- The trail text ----

    [Fact]
    public void TheTrailTextLimit_IsTheLimitOfATrailValue()
    {
        Assert.Equal(AuditEntryRules.MaxValueLength, BrandingRules.TrailValueMaxLength);
    }

    [Fact]
    public void ATrailText_StopsAtTheLimit_AndAShorterOneIsKept()
    {
        string exact = new('a', AuditEntryRules.MaxValueLength);
        string longer = exact + "b";

        Assert.Equal(exact, BrandingRules.TrailText(exact));
        Assert.Equal(exact, BrandingRules.TrailText(longer));
        Assert.Equal("kısa", BrandingRules.TrailText("kısa"));
        Assert.Null(BrandingRules.TrailText(null));
    }

    // ---- KK5: the text on the colour is the side with the higher contrast, at least 4,5:1 on every colour ----

    [Theory]
    [InlineData("#FFFF00", EmailColors.Black)]
    [InlineData("#000080", EmailColors.White)]
    [InlineData("#777777", EmailColors.Black)]
    [InlineData("#707070", EmailColors.White)]
    [InlineData("#FFFFFF", EmailColors.Black)]
    [InlineData("#000000", EmailColors.White)]
    public void TheTextColour_IsBlackOrWhite_WhicheverContrastsMore(string brand, string expectedText)
    {
        (string background, string text) = EmailColors.Button(brand);

        Assert.Equal(brand, background);
        Assert.Equal(expectedText, text);
    }

    [Fact]
    public void TheLimitColour_777777_HasWhiteBelowTheMinimum_AndBlackAbove()
    {
        Assert.True(Contrast("#777777", EmailColors.White) < MinimumContrast);
        Assert.True(Contrast("#777777", EmailColors.Black) >= MinimumContrast);
    }

    [Fact]
    public void TheChosenText_ReachesTheMinimumContrast_OnEveryColourOfAChannelGrid()
    {
        // Every combination of 17 levels per channel (0x00, 0x10 ... 0xFF): the rule holds on the whole range, not on the examples above.
        int[] levels = [.. Enumerable.Range(0, 16).Select(i => i * 17), 255];
        int checkedColours = 0;
        foreach (int r in levels)
        {
            foreach (int g in levels)
            {
                foreach (int b in levels)
                {
                    string color = $"#{r:X2}{g:X2}{b:X2}";
                    (string background, string text) = EmailColors.Button(color);

                    Assert.Equal(color, background);
                    double chosen = Contrast(color, text);
                    double other = Contrast(color, text == EmailColors.White ? EmailColors.Black : EmailColors.White);
                    Assert.True(chosen >= MinimumContrast, $"{color} with {text}: {chosen:F2}");
                    Assert.True(chosen >= other, $"{color}: the other side contrasts more");
                    checkedColours++;
                }
            }
        }

        Assert.Equal(17 * 17 * 17, checkedColours);
    }

    [Fact]
    public void TheProductsDefaultColour_ItselfReachesTheMinimumContrast_WithItsText()
    {
        (string background, string text) = EmailColors.Button(null);

        Assert.Equal(EmailColors.DefaultButtonBackground, background);
        Assert.True(Contrast(background, text) >= MinimumContrast);
    }

    // ---- KK1, KK6: the closed lists ----

    [Fact]
    public void TheBrandForm_HoldsTheFourSettingsAndTheSignatureSwitchOnly_NoScriptStyleTrackingThemeOrPanelColour()
    {
        Assert.Equal(["BrandColor", "BrandName", "FaviconId", "IsPlatformSignatureEnabled", "LogoId"], MembersOf(typeof(BrandingInput)));
        Assert.Equal(["BrandColor", "BrandName", "FaviconId", "IsPlatformSignatureEnabled", "LogoId", "Version"], MembersOf(typeof(BrandingSection)));
        Assert.Equal(
            ["BrandColor", "BrandName", "CreatedAt", "FaviconId", "Id", "IsPlatformSignatureEnabled", "LogoId", "Version"],
            MembersOf(typeof(Branding)));
    }

    [Fact]
    public void TheBrandIdentity_HasNoMemberForAThemeOrAPanelColour_AndNoStoredTextColour()
    {
        string[] members = MembersOf(typeof(BrandIdentity));

        Assert.Equal(
            ["Color", "DisplayName", "FaviconId", "FaviconInitial", "HasBrandName", "IsDefaultColor", "LogoId", "ShowsPlatformSignature", "TextColor"],
            members);
        Assert.DoesNotContain(MembersOf(typeof(Branding)), m => m.Contains("Text", StringComparison.Ordinal) || m.Contains("Theme", StringComparison.Ordinal) || m.Contains("Dark", StringComparison.Ordinal));
    }

    [Fact]
    public void TheEmail_HasNoMemberForThePlatformSignature_KK6()
    {
        foreach (Type type in new[] { typeof(EmailBranding), typeof(EmailFrameModel) })
        {
            Assert.DoesNotContain(MembersOf(type), m => m.Contains("Signature", StringComparison.OrdinalIgnoreCase) || m.Contains("Platform", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void TheSignatureSwitch_IsOnByDefault_OnTheStoredRow()
    {
        Assert.True(Branding.Create(DateTimeOffset.UnixEpoch).IsPlatformSignatureEnabled);
    }

    private static string[] MembersOf(Type type)
    {
        return
        [
            .. type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name != "EqualityContract")
                .Select(p => p.Name)
                .Order(StringComparer.Ordinal),
        ];
    }

    private static double Contrast(string background, string text)
    {
        double a = Luminance(background);
        double b = Luminance(text);
        return (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05);
    }

    private static double Luminance(string hex)
    {
        double Channel(int start)
        {
            double c = Convert.ToInt32(hex.Substring(start, 2), 16) / 255.0;
            return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }

        return (0.2126 * Channel(1)) + (0.7152 * Channel(3)) + (0.0722 * Channel(5));
    }
}
