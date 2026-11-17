using System.Reflection;

using Microsoft.Playwright;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>
/// The states of the screens the level A pass scans besides the screens themselves (T552 criterion 1, KK1; 12 12.2 (1), VAL-A776, VAL-A781, VAL-A790): the confirmation windows with the window open, the pages that
/// answer a form sent with a value that is not allowed, and the empty states, in each engine and each way. No list is written here: the windows are the walks of <see cref="ConfirmWindowTests"/> (every theory of that class is run
/// with a hook that reads the open window), the forms are the cases of <see cref="ValidationStateTests"/> (the forms the audit's error message heading sends) and the empty states are the stops of
/// <see cref="EmptyStateTests"/> (each run with a hook that reads the state open). In a window of the administrator the focus
/// starts on "Vazgeç" (VAL-A781), which is read in the browser at every window.
/// </summary>
public sealed class AccessibilityStatesTests(BrowserHost host) : BrowserTestBase(host)
{
    internal const string WindowScanStep = "onay penceresi taraması";
    internal const string WindowFocusStep = "onay penceresi odağı";
    internal const string ErrorScanStep = "hata hâli taraması";
    internal const string EmptyScanStep = "boş durum taraması";

    /// <summary>The confirmation windows' walks: the theories of <see cref="ConfirmWindowTests"/>, found by reflection so that a window added there is scanned here with no second list.</summary>
    internal static List<MethodInfo> WindowWalks() =>
        [.. typeof(ConfirmWindowTests).GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.GetCustomAttribute<TheoryAttribute>() is not null).OrderBy(m => m.Name, StringComparer.Ordinal)];

    /// <summary>The element that holds the focus, named by its tag, id, class and text.</summary>
    internal const string ActiveElement = """
        () => {
          const a = document.activeElement;
          if (!a) { return 'none'; }
          return a.tagName + (a.id ? '#' + a.id : '') + (a.className && typeof a.className === 'string' ? '.' + a.className.trim().split(/\s+/).join('.') : '') + ' "' + (a.textContent || '').replace(/\s+/g, ' ').trim().slice(0, 30) + '"';
        }
        """;

    /// <summary>The focus is on the window's "Vazgeç" (VAL-A781).</summary>
    internal const string FocusIsOnCancel = "() => !!document.activeElement && document.activeElement.classList.contains('confirm__cancel')";

    /// <summary>What the pass reads while a confirmation window is open: the scan, and where the focus started.</summary>
    private async Task InspectWindowAsync(string screen, string operation, ScriptPath path, string engine, WatchedPage page)
    {
        // A window of the administrator is a dialog (confirm--window): the focus starts on its "Vazgeç"; the customer's confirmation is the last step inside the page, not a window.
        bool isWindow = await page.Page.Locator(".confirm--window").CountAsync() == 1;
        if (isWindow)
        {
            await AccessibilityAuditTests.WaitForFocusAsync(page.Page, FocusIsOnCancel);
        }

        (List<AccessibilityScanTests.Finding> violations, int passed, List<string> incomplete, string version) = await AccessibilityScanTests.ScanAsync(page.Page);
        string observed = $"axe-core {version}: {violations.Count} violations, {passed} rules passed, {incomplete.Count} incomplete" + (violations.Count > 0 ? ": " + string.Join(" / ", violations.Take(5).Select(v => $"{v.Rule} ({v.Criteria}) {v.Target}")) : string.Empty);
        await ObserveAsync(screen, path, engine, page, $"{WindowScanStep}: {operation}", "axe-core WCAG A kural kümesi: pencere açıkken ihlal yok", observed, violations.Count == 0);

        if (!isWindow)
        {
            await ObserveAsync(screen, path, engine, page, $"{WindowFocusStep}: {operation}", "pencere değil, sayfanın kendi son adımı: odağın Vazgeç'te başlaması koşulu uygulanmaz (04 2.4)", "pencere yok, sayfanın son adımı", true);
            return;
        }

        bool onCancel = await page.Page.EvaluateAsync<bool>(FocusIsOnCancel);
        string active = await page.Page.EvaluateAsync<string>(ActiveElement);
        await ObserveAsync(screen, path, engine, page, $"{WindowFocusStep}: {operation}", "onay penceresi açılınca odak penceredeki Vazgeç'te başlar (VAL-A781; WCAG 2.4.3)", $"odak {active}", onCancel);
    }

    [Theory]
    [MemberData(nameof(Ways))]
    public async Task EveryConfirmationWindowOfTheInventory_HasNoViolationOfTheScan_AndTheFocusStartsOnCancel_InTheWay_KK1(string engine, ScriptPath path)
    {
        List<MethodInfo> walks = WindowWalks();
        int inspected = 0;
        foreach (MethodInfo walk in walks)
        {
            ConfirmWindowTests confirm = new(Host)
            {
                AtWindow = async (screen, operation, page) =>
                {
                    await InspectWindowAsync(screen, operation, path, engine, page);
                    inspected++;
                },
            };
            await (Task)walk.Invoke(confirm, [engine, path])!;
        }

        Assert.True(walks.Count >= 9, $"ConfirmWindowTests has {walks.Count} walks; at least 9 windows are expected.");
        Assert.True(inspected == walks.Count, $"{walks.Count} windows are walked and {inspected} were read open: a window that did not open is not scanned.");
        Assert.Empty(Failures);
    }

    [Theory]
    [MemberData(nameof(Ways))]
    public async Task EveryEmptyStateOfTheInventory_HasNoViolationOfTheScan_InTheWay_KK1(string engine, ScriptPath path)
    {
        List<EmptyStop> stops = [.. EmptyStateTests.Stops(Host.Seed)];
        int scanned = 0;
        EmptyStateTests empty = new(Host)
        {
            AtEmptyState = async (stop, way, eng, page) =>
            {
                (List<AccessibilityScanTests.Finding> violations, int passed, List<string> incomplete, string version) = await AccessibilityScanTests.ScanAsync(page.Page);
                string observed = $"{stop.Key}: axe-core {version}: {violations.Count} violations, {passed} rules passed, {incomplete.Count} incomplete" + (violations.Count > 0 ? ": " + string.Join(" / ", violations.Take(5).Select(v => $"{v.Rule} ({v.Criteria}) {v.Target}")) : string.Empty);
                await ObserveAsync(stop.Screen, way, eng, page, $"{EmptyScanStep}: {stop.State}", "axe-core WCAG A kural kümesi: boş hâlde ihlal yok", observed, violations.Count == 0);
                scanned++;
            },
        };
        foreach (EmptyStop stop in stops)
        {
            await empty.RunStopAsync(engine, path, stop.Key);
        }

        // The list of empty states is the stops of EmptyStateTests (read from 04 and checked there): this is a floor against a list that shrank, not the list.
        Assert.True(stops.Count >= 22, $"EmptyStateTests has {stops.Count} stops; at least 22 empty states are expected.");
        Assert.True(scanned == stops.Count, $"{stops.Count} empty states are opened and {scanned} were scanned: a state that did not open is not scanned.");
        Assert.Empty(Failures);
    }

    [Theory]
    [MemberData(nameof(Ways))]
    public async Task EveryFormAnswerWithAnError_HasNoViolationOfTheScan_InTheWay_KK1(string engine, ScriptPath path)
    {
        ValidationStateTests validation = new(Host);
        List<string> keys = [.. ValidationStateTests.ErrorStateKeys().Order(StringComparer.Ordinal)];
        int scanned = 0;
        foreach (string key in keys)
        {
            string screen = key.Split('|')[0];
            ValidationStateTests.Probe probe = await validation.ProbeAsync(engine, path, key, async page =>
            {
                // The answer is read when the browser has taken the focus to the wrong field, as a person sees it (the scan does not move the focus, but the page the audit reads is the page a person is on).
                if (await page.Page.Locator("[data-js=error-summary], [aria-invalid=true]").CountAsync() > 0)
                {
                    await AccessibilityAuditTests.WaitForFocusAsync(page.Page, AccessibilityAuditTests.FocusIsOnAnElement);
                }

                (List<AccessibilityScanTests.Finding> violations, int passed, List<string> incomplete, string version) = await AccessibilityScanTests.ScanAsync(page.Page);
                string observed = $"{key}: axe-core {version}: {violations.Count} violations, {passed} rules passed, {incomplete.Count} incomplete" + (violations.Count > 0 ? ": " + string.Join(" / ", violations.Take(5).Select(v => $"{v.Rule} ({v.Criteria}) {v.Target}")) : string.Empty);
                await ObserveAsync(screen, path, engine, page, $"{ErrorScanStep}: {key}", "axe-core WCAG A kural kümesi: hatalı gönderimin cevabında ihlal yok", observed, violations.Count == 0);
                scanned++;
                return (violations.Count == 0, observed);
            });

            // The page that was scanned is the answer to a refused send (422), not the form again as it was.
            Assert.True(probe.Posts.Count > 0 && probe.Posts[^1] == 422, $"{key}: the send was not answered with 422 ({string.Join(",", probe.Posts)}); the page scanned is not an error state.");
        }

        Assert.True(keys.Count >= 40, $"{keys.Count} forms can answer with an error; at least 40 are expected.");
        Assert.True(scanned == keys.Count, $"{keys.Count} forms are sent and {scanned} answers were scanned.");
        Assert.Empty(Failures);
    }
}
