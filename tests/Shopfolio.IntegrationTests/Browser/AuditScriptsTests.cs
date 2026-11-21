using System.Text.Json;

using Microsoft.Playwright;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>
/// The audit's own scripts, shown on pages made for the purpose (09 19.2 item 3: a rule is shown to go red on a case that breaks it) - not on the application's pages, so the demonstration does not depend on what a screen
/// happens to hold today: the accessible name reading sees a nameless control that has no size (D-1746 b; the panel's sign-out button emptied was skipped as invisible while axe's <c>button-name</c> caught it), still leaves out
/// what a person cannot reach, and counts links of one name that lead to different places (D-1732 d); and the style wait ends only when the style sheets are in (D-1746 a).
/// </summary>
public sealed class AuditScriptsTests(BrowserHost host) : BrowserTestBase(host)
{
    public static TheoryData<string> OnEveryEngine()
    {
        TheoryData<string> data = [];
        foreach (string engine in BrowserHost.Engines)
        {
            data.Add(engine);
        }

        return data;
    }

    private async Task<JsonElement> NamesOfAsync(string engine, string html)
    {
        await using IBrowserContext context = await Host.ContextAsync(engine, ScriptPath.Scripted);
        WatchedPage page = await NewPageAsync(context);
        await page.Page.SetContentAsync(html);
        using JsonDocument names = JsonDocument.Parse(await page.Page.EvaluateAsync<string>(AuditScripts.Names));
        return names.RootElement.Clone();
    }

    [Theory]
    [MemberData(nameof(OnEveryEngine))]
    public async Task Names_SeesANamelessControlThatHasNoSize_D1746(string engine)
    {
        JsonElement names = await NamesOfAsync(engine, "<main><button style=\"width:0;height:0;padding:0;border:0;overflow:hidden\"></button><button>Çıkış</button></main>");

        Assert.Equal(2, names.GetProperty("controls").GetInt32());
        Assert.Single(names.GetProperty("empty").EnumerateArray());
    }

    [Theory]
    [MemberData(nameof(OnEveryEngine))]
    public async Task Names_LeavesOutWhatNoPersonCanReach_D1746(string engine)
    {
        JsonElement names = await NamesOfAsync(
            engine,
            "<main><button hidden></button><button style=\"display:none\"></button><button aria-hidden=\"true\" tabindex=\"-1\"></button><div inert><button></button></div><details><summary>Aç</summary><button></button></details></main>");

        // Only the disclosure's summary is reachable: the closed disclosure's own button, the hidden, the display:none, the aria-hidden and the inert ones are not.
        Assert.Equal(1, names.GetProperty("controls").GetInt32());
        Assert.Empty(names.GetProperty("empty").EnumerateArray());
    }

    [Theory]
    [MemberData(nameof(OnEveryEngine))]
    public async Task Names_CountsLinksOfOneNameThatLeadToDifferentPlaces_D1732(string engine)
    {
        JsonElement different = await NamesOfAsync(engine, "<main><a href=\"/a\">Detay</a><a href=\"/b\">Detay</a></main>");
        JsonElement same = await NamesOfAsync(engine, "<main><a href=\"/a\">Detay</a><a href=\"/a\">Detay</a><a href=\"/b\">Başka</a></main>");

        Assert.Single(different.GetProperty("sameNameElsewhere").EnumerateArray());
        Assert.Empty(same.GetProperty("sameNameElsewhere").EnumerateArray());
    }

    [Theory]
    [MemberData(nameof(OnEveryEngine))]
    public async Task StylesLoaded_IsTrueOnceTheLinkedStyleSheetIsIn_AndTheWaitEndsThen_D1746(string engine)
    {
        await using IBrowserContext context = await Host.ContextAsync(engine, ScriptPath.Scripted);
        WatchedPage page = await NewPageAsync(context);
        await page.Page.SetContentAsync("<!doctype html><html><head><link rel=\"stylesheet\" href=\"data:text/css,body%7Bcolor:black%7D\"></head><body>x</body></html>");

        await WaitForStylesAsync(page, 10000);

        Assert.True(await page.Page.EvaluateAsync<bool>(StylesLoaded));
    }

    /// <summary>The walk's reading of a Tab walk that does not reach an element (D-1736 d): a control the page keeps out of the tab order, at the page's end, is "not in the Tab order" - never "keyboard trap" (Firefox keeps the focus on the last control).</summary>
    [Theory]
    [MemberData(nameof(OnEveryEngine))]
    public async Task Reach_OfAControlOutOfTheTabOrder_IsNamedAsSuch_AndNotAsAKeyboardTrap_D1736(string engine)
    {
        if (engine == "webkit")
        {
            // WebKit's walk sets the focus by script (ReachAsync), so tabindex=-1 takes it and the case does not arise there.
            return;
        }

        await using IBrowserContext context = await Host.ContextAsync(engine, ScriptPath.Scripted);
        WatchedPage page = await NewPageAsync(context);
        await page.Page.SetContentAsync("<!doctype html><html><body><a id=\"first\" href=\"#a\">Birinci</a><a id=\"last\" href=\"#b\">Son</a><button id=\"hidden-from-tab\" tabindex=\"-1\">Tab dışı</button></body></html>");
        await page.Page.Locator("#first").FocusAsync();

        (bool reached, int _, string problem) = await AccessibilityCriteriaTests.ReachAsync(page.Page, engine, "#hidden-from-tab");

        Assert.False(reached);
        Assert.DoesNotContain("keyboard trap", problem, StringComparison.Ordinal);
    }

    /// <summary>The other side of the same reading: a focus that stays on a control with others after it is a keyboard trap, and is named so on every engine.</summary>
    [Theory]
    [MemberData(nameof(OnEveryEngine))]
    public async Task Reach_OfAFocusThatDoesNotLeaveAControl_IsNamedAKeyboardTrap_D1736(string engine)
    {
        if (engine == "webkit")
        {
            return;
        }

        await using IBrowserContext context = await Host.ContextAsync(engine, ScriptPath.Scripted);
        WatchedPage page = await NewPageAsync(context);
        await page.Page.SetContentAsync("<!doctype html><html><body><input id=\"stuck\" type=\"text\"><a id=\"after\" href=\"#a\">Sonra</a><script>document.getElementById('stuck').addEventListener('keydown', (e) => { if (e.key === 'Tab') { e.preventDefault(); } });</script></body></html>");
        await page.Page.Locator("#stuck").FocusAsync();

        (bool reached, int _, string problem) = await AccessibilityCriteriaTests.ReachAsync(page.Page, engine, "#after");

        Assert.False(reached);
        Assert.StartsWith("keyboard trap", problem, StringComparison.Ordinal);
    }
}
