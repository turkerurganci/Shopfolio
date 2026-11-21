using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;

using Microsoft.Playwright;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>
/// The automatic scan of the level A accessibility pass (T552 criterion 1, KK1; 12 12.2 (1)-(3); VAL-A790): axe-core's rule set tagged for the WCAG 2.0 and 2.1 level A criteria
/// (WCAG 2.2 adds no level A rule to axe-core; its two level A criteria, 3.2.6 and 3.3.7, are checked by <see cref="AccessibilityAuditTests"/>) and the contrast rule (the target is AA, VAL-A343)
/// run on every screen of the inventory walk (<see cref="ScreenInventoryTests.Stops"/>) in each engine and each way, the script on and the script off.
/// </summary>
public sealed class AccessibilityScanTests(BrowserHost host) : BrowserTestBase(host)
{
    /// <summary>How many pages of one context are read at the same time.</summary>
    internal const int PagesAtOnce = 6;

    /// <summary>The axe-core tags of the WCAG 2.x level A criteria; the list is the tool's own tag vocabulary (axe-core 4.x has no level A tag for 2.2).</summary>
    internal static readonly string[] LevelATags = ["wcag2a", "wcag21a"];

    /// <summary>The rules that run beside the tags: contrast (WCAG 1.4.3, level AA; the target of the product is AA, 02 3.34.5, VAL-A343).</summary>
    internal static readonly string[] ExtraRules = ["color-contrast"];

    /// <summary>The rule ids the tool itself runs for the level A tags (read once from a tag run of the tool, so a new tool version brings its own list); the scan runs these and <see cref="ExtraRules"/>, nothing else (a best-practice rule is not a criterion).</summary>
    private static string[]? _levelARules;

    /// <summary>
    /// The option set of the scan: every rule of the level A tags and the contrast rule, in one run. A tag run is exclusive, so the contrast rule cannot join it by tag; the tag run's rules are named by id instead
    /// (the list of that tag run), which gives one run, one injection of the tool's script and one reading of the page's style per screen instead of two (D-1736: the Firefox group's time).
    /// </summary>
    internal static async Task<AxeRunOptions> OptionSetAsync(IPage page)
    {
        string[]? ids = _levelARules;
        if (ids is null)
        {
            // The first scan of the run is a tag run: the rules it ran (every rule the tool runs for the tags, whatever the page's answer: a violation, a pass, incomplete or not applicable) are the list. A tag run leaves out the
            // rules the tool switches off by default and its experimental ones, which the tool's own rule list for the tags (getRules) would carry, so the list is read from a run, not from the rule catalogue.
            AxeResult tagRun = await page.RunAxe(new AxeRunOptions { RunOnly = new RunOnlyOptions { Type = "tag", Values = [.. LevelATags] } });
            ids = [.. tagRun.Violations.Concat(tagRun.Passes).Concat(tagRun.Incomplete).Concat(tagRun.Inapplicable).Select(r => r.Id).Distinct()];
            Assert.NotEmpty(ids);
            _levelARules = ids;
        }

        return new AxeRunOptions { RunOnly = new RunOnlyOptions { Type = "rule", Values = [.. ids, .. ExtraRules] } };
    }

    /// <summary>
    /// A context the tool can read the page in. A context with the script switched off cannot run the tool's own script (the engines refuse an evaluation in it: "Execution context was destroyed"),
    /// so the scriptless way of a scan is a context whose page script is cut off at the network: every script request is aborted, the page's script never runs, and the tool's script runs from outside
    /// the page. The application's pages carry no inline script (every script is an external module, 07 8.2.1), so the page the tool reads is the page of the scriptless way.
    /// </summary>
    internal static async Task<IBrowserContext> AuditContextAsync(BrowserHost host, string engine, ScriptPath path, Actor actor, int width = BrowserHost.DesktopWidth)
    {
        IBrowserContext context = await host.ContextAsync(engine, ScriptPath.Scripted, actor, width);
        if (path == ScriptPath.Scriptless)
        {
            await context.RouteAsync("**/*", async route =>
            {
                if (route.Request.ResourceType == "script")
                {
                    await route.AbortAsync();
                }
                else
                {
                    await route.ContinueAsync();
                }
            });
        }

        return context;
    }

    /// <summary>
    /// The stops of the accessibility pass: the inventory walk of T551 and the states that walk does not open because they need a filled cart (the cart with an item, the payment step with a form; 04 E-12, E-13), so that the
    /// forms of the payment step are scanned and audited too. A stop with <see cref="ScreenInventoryTests.Stop.Prepare"/> gets a context of its own (<see cref="ContextsOf"/>) so that the cart stays out of the other stops.
    /// </summary>
    internal IReadOnlyList<ScreenInventoryTests.Stop> AuditStops() =>
    [
        .. new ScreenInventoryTests(Host).Stops(),
        new("E-12", "sepet, ürünlü", Actor.Visitor, "/sepet", Prepare: FillCartAsync),
        new("E-13", "ödeme adımı, ürünlü sepetle", Actor.Visitor, "/odeme", Prepare: FillCartAsync),
    ];

    /// <summary>Puts the plain product in the page's cart (the buy form's own button).</summary>
    internal async Task FillCartAsync(WatchedPage page)
    {
        await GoAsync(page, $"/urun/{Host.Seed.Product("plain").Slug}");
        await NavigateAsync(page, () => page.Page.Locator("form.buy-form button[type=submit]").First.ClickAsync());
    }

    /// <summary>The stops grouped by the context they are opened in: one per actor, and one more for the stops that prepare their context.</summary>
    internal static IEnumerable<IGrouping<(Actor Actor, bool Prepared), ScreenInventoryTests.Stop>> ContextsOf(IEnumerable<ScreenInventoryTests.Stop> stops) => stops.GroupBy(s => (s.Actor, s.Prepare is not null));

    /// <summary>Puts a new context in the state the group's stops show: the first stop's preparation runs once on a page of the context.</summary>
    internal static async Task PrepareAsync(IBrowserContext context, IGrouping<(Actor Actor, bool Prepared), ScreenInventoryTests.Stop> group)
    {
        if (group.Key.Prepared)
        {
            WatchedPage page = await NewPageAsync(context);
            await group.First().Prepare!(page);
            await page.Page.CloseAsync();
        }
    }

    /// <summary>One finding of the tool: the rule, the criteria it names, the impact and the element.</summary>
    internal sealed record Finding(string Rule, string Impact, string Criteria, string Target, string Html);

    /// <summary>Runs the scan on the page as it is; in the scriptless way the tool's own script runs in the page's isolated evaluation and the page's script stays off.</summary>
    internal static async Task<(List<Finding> Violations, int Passed, List<string> Incomplete, string Version)> ScanAsync(IPage page)
    {
        AxeResult result = await page.RunAxe(await OptionSetAsync(page));
        List<Finding> violations = [];
        foreach (AxeResultItem item in result.Violations)
        {
            foreach (AxeResultNode node in item.Nodes)
            {
                violations.Add(new Finding(item.Id, item.Impact ?? string.Empty, string.Join(' ', item.Tags.Where(t => t.StartsWith("wcag", StringComparison.Ordinal))), node.Target.ToString() ?? string.Empty, node.Html ?? string.Empty));
            }
        }

        return (violations, result.Passes.Length, [.. result.Incomplete.Select(i => i.Id)], result.TestEngine?.Version ?? "?");
    }

    [Theory]
    [MemberData(nameof(Ways))]
    public async Task EveryScreenOfTheInventory_HasNoViolationOfTheLevelAScan_InTheWay_KK1(string engine, ScriptPath path)
    {
        foreach (IGrouping<(Actor Actor, bool Prepared), ScreenInventoryTests.Stop> group in ContextsOf(AuditStops()))
        {
            await using IBrowserContext context = await AuditContextAsync(Host, engine, path, group.Key.Actor);
            await PrepareAsync(context, group);
            // The stops of a context are read on pages of their own, a few at a time (D-1736: the time of the Firefox group); the first scan reads the rule list alone.
            using SemaphoreSlim gate = new(PagesAtOnce);
            await Task.WhenAll(group.Select(async stop =>
            {
                await gate.WaitAsync();
                try
                {
                    WatchedPage page = await NewPageAsync(context);
                    await GoAsync(page, stop.Path);
                    List<Finding> violations;
                    int passed;
                    List<string> incomplete;
                    string version;
                    try
                    {
                        (violations, passed, incomplete, version) = await ScanAsync(page.Page);
                    }
                    catch (PlaywrightException e)
                    {
                        throw new InvalidOperationException($"{stop.Screen} {stop.State} ({stop.Path}) at {page.Page.Url}: {e.Message}", e);
                    }

                    string observed = $"axe-core {version}: {violations.Count} violations, {passed} rules passed, {incomplete.Count} incomplete" + (violations.Count > 0 ? ": " + string.Join(" / ", violations.Take(5).Select(v => $"{v.Rule} ({v.Criteria}) {v.Target}")) : string.Empty);
                    await ObserveAsync(stop.Screen, path, engine, page, "otomatik tarama: " + stop.State, "axe-core WCAG A kural kümesi: ihlal yok", observed, violations.Count == 0);
                    await page.Page.CloseAsync();
                }
                finally
                {
                    gate.Release();
                }
            }));
        }

        Assert.Empty(Failures);
    }
}
