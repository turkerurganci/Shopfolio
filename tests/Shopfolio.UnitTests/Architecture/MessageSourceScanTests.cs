using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The style and script rules of the message, confirmation and state components on the source (T541 KK1, KK4, KK6, KK7; 04 2.3, 2.4, 2.7; 09 16.1, 16.2): the colours of the meanings are apart from the brand
/// colour, the short-lived notice goes by itself without a script and only for the success, the window is full screen on the narrow class, and the scripts that guard the form's second sending, the
/// confirmation window and the closable message are the ones the views name. Each rule goes red on a deliberate violation and stays quiet on the allowed near miss.
/// </summary>
public sealed class MessageSourceScanTests
{
    private const string Web = "src/Shopfolio.Web";

    private static readonly TimeSpan PatternTimeout = TimeSpan.FromSeconds(5);

    private static readonly string[] Meanings = ["error", "warning", "info", "success"];

    private static string ReadWeb(string relative) => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), Web, relative));

    private static string StripComments(string text) => Regex.Replace(text, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline | RegexOptions.CultureInvariant, PatternTimeout);

    private static string RuleOf(string css, string selector)
    {
        Match match = Regex.Match(StripComments(css), Regex.Escape(selector) + @"\s*\{([^}]*)\}", RegexOptions.CultureInvariant, PatternTimeout);
        Assert.True(match.Success, $"No rule for {selector}.");
        return match.Groups[1].Value;
    }

    // ---- KK1: each meaning has its own colour pair, and none of them is the brand colour ----

    private static List<string> MeaningViolations(string baseCss, string componentsCss)
    {
        List<string> found = [];
        foreach (string key in Meanings)
        {
            if (!Regex.IsMatch(baseCss, $@"--color-{key}\s*:", RegexOptions.CultureInvariant, PatternTimeout) || !Regex.IsMatch(baseCss, $@"--color-{key}-surface\s*:", RegexOptions.CultureInvariant, PatternTimeout))
            {
                found.Add($"{key}: colour pair is not defined in the base file");
            }

            string rule = Regex.Match(StripComments(componentsCss), $@"\.message--{key}\s*\{{([^}}]*)\}}", RegexOptions.CultureInvariant, PatternTimeout).Groups[1].Value;
            if (!rule.Contains($"var(--color-{key})", StringComparison.Ordinal) || !rule.Contains($"var(--color-{key}-surface)", StringComparison.Ordinal))
            {
                found.Add($"{key}: the message rule does not use its own pair");
            }

            if (rule.Contains("brand", StringComparison.Ordinal))
            {
                found.Add($"{key}: the message rule uses the brand colour");
            }
        }

        string[] values = [.. Meanings.Select(key => Regex.Match(baseCss, $@"--color-{key}\s*:\s*([^;]+);", RegexOptions.CultureInvariant, PatternTimeout).Groups[1].Value.Trim())];
        if (values.Distinct(StringComparer.OrdinalIgnoreCase).Count() == 1 && values[0].Length > 0)
        {
            found.Add("every meaning has the same colour");
        }

        return found;
    }

    [Fact]
    public void TheFourMeanings_HaveTheirOwnColourPairs_ApartFromTheBrandColour_KK1()
    {
        Assert.Empty(MeaningViolations(ReadWeb("wwwroot/css/base.css"), ReadWeb("wwwroot/css/components.css")));
    }

    [Theory]
    [InlineData(":root { --color-error: #111; } ", ".message--error { color: var(--color-error); }")]
    [InlineData(":root { --color-error: #111; --color-error-surface: #eee; } ", ".message--error { color: var(--brand-color); background: var(--color-error-surface); }")]
    public void AMeaningWithoutItsOwnPair_OrWithTheBrandColour_IsFound_KK1(string baseCss, string componentsCss)
    {
        Assert.NotEmpty(MeaningViolations(baseCss, componentsCss));
    }

    // ---- KK4: the short-lived notice goes by itself, without a script; the duration is one variable ----

    [Fact]
    public void TheShortLivedNotice_GoesByItselfByAStyleAnimation_OfOneDurationVariable_KK4()
    {
        string css = ReadWeb("wwwroot/css/components.css");

        Assert.Matches(new Regex(@"animation:\s*message-go\s+var\(--notice-duration\)", RegexOptions.CultureInvariant, PatternTimeout), RuleOf(css, ".message--short-lived"));
        Assert.Contains("@keyframes message-go", css, StringComparison.Ordinal);
        Assert.Single(Regex.Matches(StripComments(ReadWeb("wwwroot/css/base.css")), @"--notice-duration\s*:", RegexOptions.CultureInvariant, PatternTimeout));
        Assert.DoesNotContain("setTimeout", ReadWeb("wwwroot/js/common.js"), StringComparison.Ordinal);
    }

    [Fact]
    public void OnlyTheShortLivedRule_ReadsTheNoticeDuration_KK4()
    {
        foreach (string file in new[] { "components.css", "storefront.css", "admin.css" })
        {
            string css = StripComments(ReadWeb($"wwwroot/css/{file}"));
            foreach (Match use in Regex.Matches(css, @"([^{}]*)\{[^{}]*--notice-duration", RegexOptions.CultureInvariant, PatternTimeout))
            {
                Assert.Contains("message--short-lived", use.Groups[1].Value, StringComparison.Ordinal);
            }
        }
    }

    // ---- KK6: the window is full screen on the narrow class and a centred window from the first limit ----

    [Fact]
    public void TheConfirmationWindow_IsFullScreenByDefault_AndACentredWindowFromTheFirstLimit_KK6()
    {
        string css = StripComments(ReadWeb("wwwroot/css/components.css"));
        string byDefault = RuleOf(css, ".confirm--window");

        Assert.Contains("inset: 0", byDefault, StringComparison.Ordinal);
        Assert.Contains("width: 100%", byDefault, StringComparison.Ordinal);
        Assert.Contains("height: 100%", byDefault, StringComparison.Ordinal);
        string[] wideBlocks = [.. css.Split("@media (min-width: 768px)", StringSplitOptions.None).Skip(1).Select(part => part.Split("\n}", 2)[0]).Where(part => part.Contains(".confirm--window {", StringComparison.Ordinal))];
        Assert.Single(wideBlocks);
        Assert.Contains("height: auto", wideBlocks[0], StringComparison.Ordinal);
    }

    // ---- KK6, KK7: the scripts the views name exist; the views carry the hooks ----

    [Theory]
    [InlineData("message-close", "_Message")]
    [InlineData("error-summary", "_FormErrorSummary")]
    [InlineData("confirm-window", "_Confirm")]
    [InlineData("submit-once", "_ConfirmForm")]
    public void EveryHook_AViewDraws_HasItsScript_KK7(string hook, string view)
    {
        Assert.Contains($"data-js=\"{hook}\"", ReadWeb($"Pages/Shared/{view}.cshtml"), StringComparison.Ordinal);
        Assert.Contains($"data-js=\"{hook}\"", ReadWeb("wwwroot/js/common.js"), StringComparison.Ordinal);
    }

    [Fact]
    public void TheConfirmationsCancel_TakesTheFocusInTheWindow_AndNotInTheCustomersStep_KK6()
    {
        string form = ReadWeb("Pages/Shared/_ConfirmForm.cshtml");

        Assert.Matches(new Regex(@"autofocus=""@\(Model\.Surface == ConfirmSurface\.AdminWindow\)""", RegexOptions.CultureInvariant, PatternTimeout), form);
        Assert.DoesNotContain("<button type=\"submit\" class=\"button button--primary confirm__submit\" autofocus", form, StringComparison.Ordinal);
    }

    [Fact]
    public void TheSubmitOnceScript_DoesNotDisableTheButton_AndKeepsItsStateInAriaBusy_KK7()
    {
        string script = StripComments(ReadWeb("wwwroot/js/common.js"));
        int start = script.IndexOf("function bindSubmitOnce", StringComparison.Ordinal);
        Assert.True(start >= 0);
        string body = script[start..script.IndexOf("for (const form of document.querySelectorAll('[data-js=\"submit-once\"]'", start, StringComparison.Ordinal)];

        Assert.Contains("aria-busy", body, StringComparison.Ordinal);
        Assert.Contains("preventDefault", body, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"\.disabled\s*=|setAttribute\(\s*[""']disabled", RegexOptions.CultureInvariant, PatternTimeout), body);
    }

    [Fact]
    public void TheEmptyAndLoadingViews_DrawNoErrorSymbol_AndTheLoadingIndicatorStopsIfTheUserWantsNoMotion_KK8()
    {
        Assert.DoesNotContain("MessageSymbols", ReadWeb("Pages/Shared/_EmptyRow.cshtml"), StringComparison.Ordinal);
        Assert.DoesNotContain("role=\"alert\"", ReadWeb("Pages/Shared/_EmptyRow.cshtml"), StringComparison.Ordinal);
        Assert.Contains("role=\"status\"", ReadWeb("Pages/Shared/_Loading.cshtml"), StringComparison.Ordinal);
        Assert.Matches(new Regex(@"@media \(prefers-reduced-motion: reduce\)\s*\{\s*\.loading__indicator\s*\{\s*animation:\s*none", RegexOptions.CultureInvariant, PatternTimeout), StripComments(ReadWeb("wwwroot/css/components.css")));
    }
}
