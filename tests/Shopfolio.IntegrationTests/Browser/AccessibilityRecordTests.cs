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

    [Theory]
    [InlineData("document.addEventListener(\"keydown\", (e) => { if (e.key === \"s\") { save(); } });")]
    [InlineData("window.onkeydown = (e) => { if (e.key == 'k') { go(); } };")]
    [InlineData("el.addEventListener('touchmove', move);")]
    [InlineData("<button type=\"button\" onpointerdown=\"press()\">x</button>")]
    [InlineData("<a href=\"/x\" accesskey=\"s\">x</a>")]
    [InlineData("window.addEventListener(\"devicemotion\", shake);")]
    [InlineData("document.addEventListener(\"keydown\", (event) => { const { key } = event; if (key === \"/\") { focusSearch(); } });")]
    [InlineData("document.addEventListener(\"keyup\", (event) => { if ([\"j\", \"k\"].includes(event.key)) { next(); } });")]
    [InlineData("document.addEventListener(\"keydown\", (event) => { switch (event.key) { case \"g\": go(); break; } });")]
    [InlineData("el.addEventListener(`mousedown`, press);")]
    [InlineData("el.setAttribute(\"onmouseup\", \"release()\");")]
    [InlineData("document.addEventListener(\"keydown\", (e) => { if (\"s\" === e.key) { save(); } });")]
    [InlineData("document.addEventListener(\"keydown\", (e) => { if (e.keyCode === 83) { save(); } });")]
    [InlineData("<div draggable=\"true\">x</div>")]
    public void TheSourceScan_CatchesEachFormOfWhatTheCriteriaForbid_KK3(string source)
    {
        Assert.NotEmpty(Violations("sample", source));
    }

    [Theory]
    [InlineData("panel.addEventListener(\"keydown\", (event) => { if (event.key === \"Escape\") { close(); } });")]
    [InlineData("button.addEventListener(\"click\", () => open());")]
    [InlineData("group.addEventListener(\"keydown\", (event) => { if ([\"ArrowDown\", \"ArrowUp\", \" \"].includes(event.key)) { move(); } });")]
    [InlineData("form.addEventListener(\"change\", () => send());")]
    public void TheSourceScan_LetsTheEscapeKeyTheArrowsAndTheClickPass_KK3(string source)
    {
        Assert.Empty(Violations("sample", source));
    }
}
