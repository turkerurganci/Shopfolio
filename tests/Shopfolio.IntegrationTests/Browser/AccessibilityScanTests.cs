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
    /// <summary>The axe-core tags of the WCAG 2.x level A criteria; the list is the tool's own tag vocabulary (axe-core 4.x has no level A tag for 2.2).</summary>
    internal static readonly string[] LevelATags = ["wcag2a", "wcag21a"];

    /// <summary>The rules that run beside the tags: contrast (WCAG 1.4.3, level AA; the target of the product is AA, 02 3.34.5, VAL-A343).</summary>
    internal static readonly string[] ExtraRules = ["color-contrast"];

    /// <summary>The option sets of the scan: the level A tags, then the contrast rule, nothing else (a best-practice rule is not a criterion). Two runs, because a tag run is exclusive.</summary>
    internal static IEnumerable<AxeRunOptions> OptionSets()
    {
        yield return new AxeRunOptions { RunOnly = new RunOnlyOptions { Type = "tag", Values = [.. LevelATags] } };
        yield return new AxeRunOptions { RunOnly = new RunOnlyOptions { Type = "rule", Values = [.. ExtraRules] } };
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

    /// <summary>One finding of the tool: the rule, the criteria it names, the impact and the element.</summary>
    internal sealed record Finding(string Rule, string Impact, string Criteria, string Target, string Html);

    /// <summary>Runs the scan on the page as it is; in the scriptless way the tool's own script runs in the page's isolated evaluation and the page's script stays off.</summary>
    internal static async Task<(List<Finding> Violations, int Passed, List<string> Incomplete, string Version)> ScanAsync(IPage page)
    {
        List<Finding> violations = [];
        int passed = 0;
        List<string> incomplete = [];
        string version = "?";
        foreach (AxeRunOptions options in OptionSets())
        {
            AxeResult result = await page.RunAxe(options);
            foreach (AxeResultItem item in result.Violations)
            {
                foreach (AxeResultNode node in item.Nodes)
                {
                    violations.Add(new Finding(item.Id, item.Impact ?? string.Empty, string.Join(' ', item.Tags.Where(t => t.StartsWith("wcag", StringComparison.Ordinal))), node.Target.ToString() ?? string.Empty, node.Html ?? string.Empty));
                }
            }

            passed += result.Passes.Length;
            incomplete.AddRange(result.Incomplete.Select(i => i.Id));
            version = result.TestEngine?.Version ?? version;
        }

        return (violations, passed, incomplete, version);
    }

    [Theory]
    [MemberData(nameof(Ways))]
    public async Task EveryScreenOfTheInventory_HasNoViolationOfTheLevelAScan_InTheWay_KK1(string engine, ScriptPath path)
    {
        foreach (IGrouping<Actor, ScreenInventoryTests.Stop> group in new ScreenInventoryTests(Host).Stops().GroupBy(s => s.Actor))
        {
            await using IBrowserContext context = await AuditContextAsync(Host, engine, path, group.Key);
            foreach (ScreenInventoryTests.Stop stop in group)
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
        }

        Assert.Empty(Failures);
    }
}
