using System.Text.RegularExpressions;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>
/// The two checks of the accessibility pass that need no browser (T552 criterion 3, KK3): the record of the 31 criteria agrees with the table of 12 12.2 in both directions (<see cref="WcagCriteria"/>), and the source of
/// the pages' scripts and templates defines no single-character shortcut, no path or multi-point gesture, no press-down action and no device motion (WCAG 2.1.4, 2.5.1, 2.5.2, 2.5.4; VAL-A787, VAL-A789).
/// </summary>
public sealed class AccessibilityRecordTests
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(5);

    private static string RepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(System.IO.Path.Combine(directory.FullName, "Docs", "12_VALIDATION_PROTOCOL.md")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Docs/12_VALIDATION_PROTOCOL.md was not found above the test's folder.");
    }

    // ---- KK3: the record against the table of 12 12.2 ----

    /// <summary>The rows of the 31-criteria table of 12 12.2: the criteria of each row, whether it applies, the rows of 12 it names.</summary>
    internal static List<(string[] Ids, bool Applies, string[] Rows)> DocumentTable()
    {
        string[] lines = File.ReadAllLines(System.IO.Path.Combine(RepositoryRoot(), "Docs", "12_VALIDATION_PROTOCOL.md"));
        int start = Array.FindIndex(lines, l => l.StartsWith("| WCAG 2.2 A ölçütü | Durum | Satır |", StringComparison.Ordinal));
        Assert.True(start >= 0, "The table of the 31 criteria was not found in 12 12.2.");
        List<(string[] Ids, bool Applies, string[] Rows)> table = [];
        for (int i = start + 2; i < lines.Length && lines[i].StartsWith('|'); i++)
        {
            string[] cells = lines[i].Split('|', StringSplitOptions.TrimEntries);
            string[] ids = [.. Regex.Matches(cells[1], @"\b\d\.\d\.\d\b", RegexOptions.None, RegexTimeout).Select(m => m.Value)];
            string[] rows = [.. Regex.Matches(cells[3], @"VAL-A\d{3}", RegexOptions.None, RegexTimeout).Select(m => m.Value)];
            Assert.True(cells[2].StartsWith("Uygulanır", StringComparison.Ordinal) || cells[2].StartsWith("Uygulanmaz", StringComparison.Ordinal), $"The status of row {cells[1]} is neither Uygulanır nor Uygulanmaz.");
            table.Add((ids, cells[2].StartsWith("Uygulanır", StringComparison.Ordinal), rows));
        }

        return table;
    }

    /// <summary>The differences between a record of the criteria and the table of the document: a criterion on one side only, another status, other rows, a row of 12 that does not exist, a missing reason or evidence.</summary>
    internal static List<string> Differences(IReadOnlyList<WcagCriteria.Criterion> record, List<(string[] Ids, bool Applies, string[] Rows)> table, string document)
    {
        List<string> found = [];
        Dictionary<string, (bool Applies, string[] Rows)> inDocument = [];
        foreach ((string[] ids, bool applies, string[] rows) in table)
        {
            foreach (string id in ids)
            {
                if (!inDocument.TryAdd(id, (applies, rows)))
                {
                    found.Add($"criterion {id} is in the table of the document twice");
                }
            }
        }

        found.AddRange(inDocument.Keys.Except(record.Select(c => c.Id)).Select(id => $"criterion {id} is in the document and not in the record"));
        found.AddRange(record.Select(c => c.Id).Except(inDocument.Keys).Select(id => $"criterion {id} is in the record and not in the document"));
        foreach (WcagCriteria.Criterion criterion in record.Where(c => inDocument.ContainsKey(c.Id)))
        {
            (bool applies, string[] rows) = inDocument[criterion.Id];
            if (applies != criterion.Applies)
            {
                found.Add($"criterion {criterion.Id}: the document says {(applies ? "Uygulanır" : "Uygulanmaz")}, the record says the opposite");
            }

            if (!rows.Order(StringComparer.Ordinal).SequenceEqual(criterion.Rows.Order(StringComparer.Ordinal)))
            {
                found.Add($"criterion {criterion.Id}: the rows differ ({string.Join(",", rows)} against {string.Join(",", criterion.Rows)})");
            }

            if (!criterion.Applies && criterion.Reason.Length == 0)
            {
                found.Add($"criterion {criterion.Id} does not apply and carries no reason");
            }

            if (criterion.Evidence.Length == 0)
            {
                found.Add($"criterion {criterion.Id} carries no evidence");
            }

            found.AddRange(criterion.Rows.Where(row => !document.Contains($"| {row} |", StringComparison.Ordinal)).Select(row => $"criterion {criterion.Id}: row {row} is not a row of 12"));
        }

        return found;
    }

    /// <summary>The evidence the record names as a test ("Class.MethodPrefix...") is a test that exists: a class or a method renamed or never written breaks here (T552 D-1732 (a)).</summary>
    [Fact]
    public void EveryTestTheRecordNamesAsEvidence_Exists_KK3()
    {
        Type[] types = typeof(WcagCriteria).Assembly.GetTypes();
        List<string> missing = [];
        int named = 0;
        foreach (string evidence in WcagCriteria.All.SelectMany(c => c.Evidence).Distinct(StringComparer.Ordinal))
        {
            Match match = Regex.Match(evidence, @"\((\w+)\.(\w+)\.\.\.\)", RegexOptions.None, RegexTimeout);
            if (!match.Success)
            {
                Assert.DoesNotContain("Tests", evidence, StringComparison.Ordinal);
                continue;
            }

            named++;
            Type? type = types.FirstOrDefault(t => t.Name == match.Groups[1].Value);
            if (type is null || !type.GetMethods().Any(m => m.Name.StartsWith(match.Groups[2].Value, StringComparison.Ordinal) && m.GetCustomAttributes(typeof(FactAttribute), inherit: true).Length > 0))
            {
                missing.Add(evidence);
            }
        }

        Assert.True(named >= 4, $"The record names {named} tests as evidence; at least 4 are expected.");
        Assert.True(missing.Count == 0, "Evidence naming a test that does not exist: " + string.Join(" / ", missing));
    }

    [Fact]
    public void AnUnmetCriterion_IsNotWrittenAsMet_AndAMetOneIsNot_KK3()
    {
        Assert.Equal("karşılandı", AccessibilityAuditTests.ResultOf(applies: true, []));
        Assert.StartsWith("karşılanmadı (ekran: E-34, E-47", AccessibilityAuditTests.ResultOf(applies: true, ["E-34", "E-47"]), StringComparison.Ordinal);
        Assert.Equal("uygulanmaz", AccessibilityAuditTests.ResultOf(applies: false, ["E-34"]));
    }

    [Fact]
    public void TheRecordOfTheCriteria_IsTheTableOfTheProtocol_InBothDirections_KK3()
    {
        List<(string[] Ids, bool Applies, string[] Rows)> table = DocumentTable();
        string document = File.ReadAllText(System.IO.Path.Combine(RepositoryRoot(), "Docs", "12_VALIDATION_PROTOCOL.md"));
        Assert.Equal(31, table.SelectMany(t => t.Ids).Count());
        Assert.Equal(31, WcagCriteria.All.Count);
        List<string> found = Differences(WcagCriteria.All, table, document);
        Assert.True(found.Count == 0, string.Join(Environment.NewLine, found));
    }

    [Fact]
    public void ABrokenRecord_IsCaught_InEachOfItsForms_KK3()
    {
        List<(string[] Ids, bool Applies, string[] Rows)> table = DocumentTable();
        string document = File.ReadAllText(System.IO.Path.Combine(RepositoryRoot(), "Docs", "12_VALIDATION_PROTOCOL.md"));
        WcagCriteria.Criterion first = WcagCriteria.All[0];
        Assert.NotEmpty(Differences([.. WcagCriteria.All.Skip(1)], table, document));
        Assert.NotEmpty(Differences([.. WcagCriteria.All, new("9.9.9", "Uydurma", true, string.Empty, ["VAL-A344"], ["x"])], table, document));
        Assert.NotEmpty(Differences([first with { Applies = !first.Applies, Reason = "x" }, .. WcagCriteria.All.Skip(1)], table, document));
        Assert.NotEmpty(Differences([first with { Rows = ["VAL-A345"] }, .. WcagCriteria.All.Skip(1)], table, document));
        Assert.NotEmpty(Differences([first with { Rows = ["VAL-A999"] }, .. WcagCriteria.All.Skip(1)], table, document));
        Assert.NotEmpty(Differences([WcagCriteria.All[1] with { Reason = string.Empty }, first, .. WcagCriteria.All.Skip(2)], table, document));
        Assert.NotEmpty(Differences([first with { Evidence = [] }, .. WcagCriteria.All.Skip(1)], table, document));
    }

    // ---- KK3: the criteria the source answers (2.1.4, 2.5.1, 2.5.2, 2.5.4) ----

    private static readonly HashSet<string> ForbiddenEvents = new(StringComparer.OrdinalIgnoreCase)
    {
        "touchstart", "touchmove", "touchend", "touchcancel", "pointerdown", "pointermove", "pointerup", "pointercancel", "pointerrawupdate", "mousedown", "mouseup", "mousemove",
        "drag", "dragstart", "dragend", "dragenter", "dragover", "dragleave", "drop", "wheel", "gesturestart", "gesturechange", "gestureend", "devicemotion", "deviceorientation", "deviceorientationabsolute",
    };

    private static readonly string[] KeyEvents = ["keydown", "keyup", "keypress"];

    /// <summary>The keys a handler may name: a key of more than one character is a named key (Escape, Enter, the arrows), never a printable shortcut; the space is the activation of a focused control.</summary>
    private static bool IsShortcutKey(string key) => key.Length == 1 && key != " ";

    /// <summary>The findings of one source text (a script or a template): what the criteria forbid and the text holds.</summary>
    internal static List<string> Violations(string name, string text)
    {
        List<string> found = [];
        List<string> events = [];
        events.AddRange(Regex.Matches(text, @"addEventListener\(\s*([""'`])(\w+)\1", RegexOptions.None, RegexTimeout).Select(m => m.Groups[2].Value));
        events.AddRange(Regex.Matches(text, @"\.on(\w+)\s*=(?!=)", RegexOptions.None, RegexTimeout).Select(m => m.Groups[1].Value));
        events.AddRange(Regex.Matches(text, @"\bon(\w+)\s*=\s*[""']", RegexOptions.IgnoreCase, RegexTimeout).Select(m => m.Groups[1].Value));
        events.AddRange(Regex.Matches(text, @"setAttribute\(\s*([""'`])on(\w+)\1", RegexOptions.IgnoreCase, RegexTimeout).Select(m => m.Groups[2].Value));
        foreach (string e in events.Where(ForbiddenEvents.Contains))
        {
            found.Add($"{name}: handler of {e} (a gesture, a press-down action or a device motion)");
        }

        if (Regex.IsMatch(text, @"\baccesskey\s*=", RegexOptions.IgnoreCase, RegexTimeout))
        {
            found.Add($"{name}: accesskey (a single-character shortcut)");
        }

        if (Regex.IsMatch(text, @"\bdraggable\s*=\s*[""']?true", RegexOptions.IgnoreCase, RegexTimeout))
        {
            found.Add($"{name}: draggable (a path-based gesture)");
        }

        if (events.Any(e => KeyEvents.Contains(e, StringComparer.OrdinalIgnoreCase)))
        {
            // The literals a key handler names: compared with the key (either side), listed in an array it is looked up in, or the label of a switch case.
            List<string> literals = [];
            literals.AddRange(Regex.Matches(text, @"(?:\.|\b)(?:key|code)\s*[!=]==?\s*([""'`])([^""'`]*)\1", RegexOptions.None, RegexTimeout).Select(m => m.Groups[2].Value));
            literals.AddRange(Regex.Matches(text, @"([""'`])([^""'`]*)\1\s*[!=]==?\s*(?:\w+\.)?(?:key|code)\b", RegexOptions.None, RegexTimeout).Select(m => m.Groups[2].Value));
            foreach (Match list in Regex.Matches(text, @"\[(?<l>[^\]]*)\]\s*\.includes\(\s*(?:\w+\.)?(?:key|code)\b", RegexOptions.None, RegexTimeout))
            {
                literals.AddRange(Regex.Matches(list.Groups["l"].Value, @"([""'`])([^""'`]*)\1", RegexOptions.None, RegexTimeout).Select(m => m.Groups[2].Value));
            }

            literals.AddRange(Regex.Matches(text, @"\bcase\s*([""'`])([^""'`]*)\1\s*:", RegexOptions.None, RegexTimeout).Select(m => m.Groups[2].Value));
            foreach (string key in literals.Where(IsShortcutKey))
            {
                found.Add($"{name}: a key handler names the single character \"{key}\" (a single-character shortcut)");
            }

            if (Regex.IsMatch(text, @"\b(?:keyCode|charCode)\b|\.which\b", RegexOptions.None, RegexTimeout))
            {
                found.Add($"{name}: a key handler reads a key code (the characters it names cannot be told)");
            }
        }

        return found;
    }

    /// <summary>The scripts and templates the pages are made of: every script under <c>wwwroot</c> and every template (<c>.cshtml</c>) under the application's folders.</summary>
    internal static IEnumerable<(string Name, string Text)> Sources()
    {
        string web = System.IO.Path.Combine(RepositoryRoot(), "src", "Shopfolio.Web");
        foreach (string file in Directory.EnumerateFiles(System.IO.Path.Combine(web, "wwwroot"), "*.js", SearchOption.AllDirectories))
        {
            yield return (System.IO.Path.GetRelativePath(web, file), File.ReadAllText(file));
        }

        foreach (string file in Directory.EnumerateFiles(web, "*.cshtml", SearchOption.AllDirectories).Where(f => !f.Contains($"{System.IO.Path.DirectorySeparatorChar}obj{System.IO.Path.DirectorySeparatorChar}", StringComparison.Ordinal) && !f.Contains($"{System.IO.Path.DirectorySeparatorChar}bin{System.IO.Path.DirectorySeparatorChar}", StringComparison.Ordinal)))
        {
            yield return (System.IO.Path.GetRelativePath(web, file), File.ReadAllText(file));
        }

        // The style sheets carry no behaviour, but a script embedded in a template is covered above; a source with no script at all would make the scan vacuous, so the scan asserts it read some.
    }

    [Fact]
    public void TheSourceOfThePages_DefinesNoShortcutGestureOrDeviceMotion_KK3()
    {
        List<(string Name, string Text)> sources = [.. Sources()];
        Assert.Contains(sources, s => s.Name.EndsWith("common.js", StringComparison.Ordinal));
        Assert.True(sources.Count(s => s.Name.EndsWith(".cshtml", StringComparison.Ordinal)) > 50, "The scan read too few templates to mean anything.");
        List<string> found = [.. sources.SelectMany(s => Violations(s.Name, s.Text))];
        Assert.True(found.Count == 0, string.Join(Environment.NewLine, found.Take(10)));
    }

    [Fact]
    public void TheSourceScan_CatchesEachFormOfWhatTheCriteriaForbid_KK3()
    {
        string[] samples =
        [
            "document.addEventListener(\"keydown\", (e) => { if (e.key === \"s\") { save(); } });",
            "window.onkeydown = (e) => { if (e.key == 'k') { go(); } };",
            "el.addEventListener('touchmove', move);",
            "<button type=\"button\" onpointerdown=\"press()\">x</button>",
            "<a href=\"/x\" accesskey=\"s\">x</a>",
            "window.addEventListener(\"devicemotion\", shake);",
            "document.addEventListener(\"keydown\", (event) => { const { key } = event; if (key === \"/\") { focusSearch(); } });",
            "document.addEventListener(\"keyup\", (event) => { if ([\"j\", \"k\"].includes(event.key)) { next(); } });",
            "document.addEventListener(\"keydown\", (event) => { switch (event.key) { case \"g\": go(); break; } });",
            "el.addEventListener(`mousedown`, press);",
            "el.setAttribute(\"onmouseup\", \"release()\");",
            "document.addEventListener(\"keydown\", (e) => { if (\"s\" === e.key) { save(); } });",
            "document.addEventListener(\"keydown\", (e) => { if (e.keyCode === 83) { save(); } });",
            "<div draggable=\"true\">x</div>",
        ];
        foreach (string source in samples)
        {
            Assert.NotEmpty(Violations("sample", source));
        }
    }

    private static readonly string[] SendingMarkers = ["data-js=\"submit-on-change\"", "data-js=\"checkout\"", "data-js=\"variant-form\""];

    private static readonly string[] SendingScripts = ["common.js", "cart.js", "checkout.js", "product.js"];

    /// <summary>
    /// 3.2.2 (T552): a form whose value change sends it by itself carries the one notice (<c>_ChangeNotice</c>) before the change is made. A form sends itself when its tag carries the marker of a script that does it (the sales
    /// summary period, the payment step, a product's variant choice) or when it is a quantity form (<c>name="Quantity"</c> in a <c>submit-once</c> form); a script that sends a form (<c>requestSubmit</c>) outside the four known
    /// scripts is a new way to send by a change that this scan has not been told about.
    /// </summary>
    internal static List<string> ChangeNoticeViolations(string name, string text)
    {
        List<string> found = [];
        if (name.EndsWith(".cshtml", StringComparison.Ordinal))
        {
            foreach (System.Text.RegularExpressions.Match form in System.Text.RegularExpressions.Regex.Matches(text, "<form\\b([^>]*)>(.*?)</form>", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.CultureInvariant, RegexTimeout))
            {
                string tag = form.Groups[1].Value;
                string body = form.Groups[2].Value;
                bool sends = SendingMarkers.Any(m => tag.Contains(m, StringComparison.Ordinal))
                    || (tag.Contains("data-js=\"submit-once\"", StringComparison.Ordinal) && body.Contains("name=\"Quantity\"", StringComparison.Ordinal));
                if (sends && !body.Contains("<partial name=\"_ChangeNotice\"", StringComparison.Ordinal))
                {
                    found.Add($"{name}: a form that sends itself by a change has no notice (T552, 3.2.2).");
                }
            }
        }
        else if (text.Contains("requestSubmit", StringComparison.Ordinal) && !SendingScripts.Any(s => name.EndsWith(s, StringComparison.Ordinal)))
        {
            found.Add($"{name}: sends a form by script outside the scripts this scan knows (T552, 3.2.2).");
        }

        return found;
    }

    [Fact]
    public void EveryFormThatSendsItselfByAChange_CarriesTheNoticeOnce_AndTheKnownScriptsAreTheOnlySenders_T552()
    {
        List<(string Name, string Text)> sources = [.. Sources()];
        List<string> found = [.. sources.SelectMany(s => ChangeNoticeViolations(s.Name, s.Text))];
        Assert.True(found.Count == 0, string.Join(Environment.NewLine, found.Take(10)));
        Assert.Equal(4, sources.Count(s => s.Text.Contains("<partial name=\"_ChangeNotice\"", StringComparison.Ordinal)));
        Assert.All(SendingScripts, s => Assert.Contains(sources, x => x.Name.EndsWith(s, StringComparison.Ordinal) && x.Text.Contains("requestSubmit", StringComparison.Ordinal)));
    }

    [Theory]
    [InlineData("a.cshtml", "<form method=\"get\" data-js=\"submit-on-change\"><select></select></form>")]
    [InlineData("a.cshtml", "<form data-js=\"checkout\"><input /></form>")]
    [InlineData("a.cshtml", "<form data-js=\"variant-form\">x</form>")]
    [InlineData("a.cshtml", "<form data-js=\"submit-once\"><input name=\"Quantity\" /></form>")]
    [InlineData("new.js", "form.requestSubmit(button);")]
    public void TheNoticeScan_CatchesAFormOrAScriptThatSendsWithoutTheNotice_T552(string name, string source)
    {
        Assert.NotEmpty(ChangeNoticeViolations(name, source));
    }

    [Theory]
    [InlineData("a.cshtml", "<form data-js=\"submit-once\"><partial name=\"_ChangeNotice\" /><input name=\"Quantity\" /></form>")]
    [InlineData("a.cshtml", "<form data-js=\"submit-once\"><input name=\"Other\" /></form>")]
    [InlineData("a.cshtml", "<form method=\"post\"><input /></form>")]
    [InlineData("cart.js", "owner.requestSubmit(button);")]
    public void TheNoticeScan_LetsTheNoticedFormAndTheOtherFormsPass_T552(string name, string source)
    {
        Assert.Empty(ChangeNoticeViolations(name, source));
    }

    [Fact]
    public void TheSourceScan_LetsTheEscapeKeyTheArrowsAndTheClickPass_KK3()
    {
        string[] samples =
        [
            "panel.addEventListener(\"keydown\", (event) => { if (event.key === \"Escape\") { close(); } });",
            "button.addEventListener(\"click\", () => open());",
            "group.addEventListener(\"keydown\", (event) => { if ([\"ArrowDown\", \"ArrowUp\", \" \"].includes(event.key)) { move(); } });",
            "form.addEventListener(\"change\", () => send());",
        ];
        foreach (string source in samples)
        {
            Assert.Empty(Violations("sample", source));
        }
    }
}
