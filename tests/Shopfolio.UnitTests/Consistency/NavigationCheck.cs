namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The comparison of the navigation map of <c>04 3</c> with the code (T546; 04 3.1-3.7; 09 19.2 item 3, 21.4 item 1). A problem is a line that says what differs: <c>RowOnlyInDocument</c> and <c>RowOnlyInCode</c> -
/// a row of the tables or a paragraph of 3.6 without an entry, or an entry without a row; <c>FromDiffers</c> and <c>ToDiffers</c> - the source or the target the document names is not the one the entry names;
/// <c>NoEvidence</c> and <c>EvidenceMissing</c> - an entry that nothing in the code carries; <c>UndocumentedTransition</c> - a link or a redirect of the code that no row and no exemption names;
/// <c>DeadExemption</c> and <c>ExemptAndClaimed</c> - an exemption that is not in the code, or a site that is both a row's evidence and exempt. No row waits for the code and no known difference
/// is listed: a row of the tables is in the code or the check reports it.
/// </summary>
internal static class NavigationCheck
{
    public static IReadOnlyList<string> Problems(string document, IReadOnlyDictionary<string, string> sources, IReadOnlyList<string> routes)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(routes);

        IReadOnlyList<NavigationEntry> entries = NavigationInventory.Entries;
        IReadOnlyList<NavigationExemption> exemptions = NavigationInventory.Exemptions;
        List<NavigationRow> rows = [.. NavigationDocument.Rows(document), .. NavigationDocument.Returns(document).Select(r => new NavigationRow(r.Key, string.Empty, r.Key, r.Value))];
        List<string> problems = [];

        problems.AddRange(Relabel(SetComparison.Compare("04 3 rows", rows.Select(r => r.Id), entries.Select(e => e.Id))));

        problems.AddRange(rows.Where(r => r.From.Length == 0 && r.Id.StartsWith("3.6.", StringComparison.Ordinal) is false || r.Trigger.Length == 0 || r.To.Length == 0).Select(r => $"EmptyCell:{r.Id}: a row names where it starts, what triggers it and where it goes (04 3 introduction)."));

        Dictionary<string, NavigationRow> rowById = rows.GroupBy(r => r.Id, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        foreach (NavigationEntry entry in entries.Where(e => rowById.ContainsKey(e.Id)))
        {
            NavigationRow row = rowById[entry.Id];
            if (entry.From.Length > 0)
            {
                problems.AddRange(PinProblems(entry.Id, "FromDiffers", entry.From, row.From));
            }

            problems.AddRange(PinProblems(entry.Id, "ToDiffers", entry.To, row.To));
        }

        IReadOnlyList<NavigationSite> scanned = NavigationSites.Scan(sources);
        HashSet<(string, NavigationSiteKind, string)> scannedSet = [.. scanned.Select(s => (s.File, s.Kind, s.Key))];
        HashSet<(string, NavigationSiteKind, string)> claimed = [];

        foreach (NavigationEntry entry in entries)
        {
            if (entry.Evidence.Count == 0)
            {
                problems.Add($"NoEvidence:{entry.Id}");
            }

            foreach (NavigationEvidence evidence in entry.Evidence)
            {
                switch (evidence.Kind)
                {
                    case NavigationEvidenceKind.Link:
                    case NavigationEvidenceKind.Redirect:
                    case NavigationEvidenceKind.EmptyExit:
                    case NavigationEvidenceKind.FormAction:
                        NavigationSiteKind kind = evidence.Kind switch
                        {
                            NavigationEvidenceKind.Link => NavigationSiteKind.Link,
                            NavigationEvidenceKind.Redirect => NavigationSiteKind.Redirect,
                            NavigationEvidenceKind.FormAction => NavigationSiteKind.FormAction,
                            _ => NavigationSiteKind.EmptyExit,
                        };
                        claimed.Add((evidence.File, kind, evidence.Text));
                        if (!scannedSet.Contains((evidence.File, kind, evidence.Text)))
                        {
                            problems.Add($"EvidenceMissing:{entry.Id}: {kind} '{evidence.Text}' is not in {evidence.File}.");
                        }

                        break;
                    case NavigationEvidenceKind.Text:
                        if (TextProblem(sources, evidence) is { } textProblem)
                        {
                            problems.Add($"EvidenceMissing:{entry.Id}: {textProblem}");
                        }

                        break;
                    case NavigationEvidenceKind.Page:
                        if (!routes.Any(r => evidence.Text.EndsWith('*') ? r.StartsWith(evidence.Text[..^1], StringComparison.Ordinal) : string.Equals(r, evidence.Text, StringComparison.Ordinal)))
                        {
                            problems.Add($"EvidenceMissing:{entry.Id}: no page has the route '{evidence.Text}'.");
                        }

                        break;
                    default:
                        throw new InvalidOperationException($"Unknown evidence kind {evidence.Kind}.");
                }
            }
        }

        // A site stands once unless the inventory pins it as repeated: changing one of several equal sites to another address of the same file changes the count (T546 correction 4).
        foreach (IGrouping<(string, NavigationSiteKind, string), NavigationSite> group in scanned.GroupBy(s => (s.File, s.Kind, s.Key)))
        {
            int pinned = NavigationInventory.RepeatedSites.TryGetValue(group.Key, out int count) ? count : 1;
            if (group.Count() != pinned)
            {
                problems.Add($"SiteCountDiffers: {group.Key.Item2} '{group.Key.Item3}' is {group.Count()} times in {group.Key.Item1}, the inventory says {pinned}.");
            }
        }

        foreach (((string file, NavigationSiteKind kind, string key), int pinnedCount) in NavigationInventory.RepeatedSites)
        {
            if (!scannedSet.Contains((file, kind, key)))
            {
                problems.Add($"SiteCountDiffers: {kind} '{key}' is 0 times in {file}, the inventory says {pinnedCount}.");
            }
        }

        HashSet<(string, NavigationSiteKind, string)> exempt = [];
        foreach (NavigationExemption exemption in exemptions)
        {
            (string, NavigationSiteKind, string) site = (exemption.File, exemption.Kind, exemption.Key);
            exempt.Add(site);
            if (!scannedSet.Contains(site))
            {
                problems.Add($"DeadExemption: {exemption.Kind} '{exemption.Key}' is not in {exemption.File}.");
            }

            if (claimed.Contains(site))
            {
                problems.Add($"ExemptAndClaimed: {exemption.Kind} '{exemption.Key}' in {exemption.File} is a row's evidence.");
            }

            // The address of an exempt site that is a variable is bound to the line that gives it its target (T546 correction 5; D-1641): the screen it stays on is the screen the variable holds.
            foreach (NavigationEvidence bound in exemption.Binds)
            {
                if (TextProblem(sources, bound) is { } boundProblem)
                {
                    problems.Add($"EvidenceMissing:Exemption({exemption.File}|{exemption.Key}): {boundProblem}");
                }
            }
        }

        problems.AddRange(OutcomeScreenProblems(sources));

        foreach ((string file, NavigationSiteKind kind, string key) in scannedSet.Order())
        {
            if (!claimed.Contains((file, kind, key)) && !exempt.Contains((file, kind, key)))
            {
                problems.Add($"UndocumentedTransition: {kind} '{key}' in {file} is in no row of 04 3 and no exemption.");
            }
        }

        return problems;
    }

    /// <summary>
    /// The screens an uncertain outcome goes to (04 2.7.3.3; D-1636): every <c>OutcomeScreen()</c> body of the code is pinned in the inventory and every pinned one is in the code - a new implementer, a changed
    /// target or a deleted method each break it - and the lines that give the body its value are pinned as text.
    /// </summary>
    private static IEnumerable<string> OutcomeScreenProblems(IReadOnlyDictionary<string, string> sources)
    {
        List<(string File, string Body)> found = [.. NavigationSites.OutcomeScreens(sources)];
        foreach (NavigationOutcomeScreen pinned in NavigationInventory.OutcomeScreens)
        {
            if (!found.Contains((pinned.File, pinned.Body)))
            {
                yield return $"OutcomeScreenMissing: {pinned.File} has no 'OutcomeScreen() => {pinned.Body}'.";
            }

            foreach (NavigationEvidence bound in pinned.BoundTo)
            {
                if (TextProblem(sources, bound) is { } boundProblem)
                {
                    yield return $"EvidenceMissing:OutcomeScreen({pinned.File}): {boundProblem}";
                }
            }
        }

        foreach ((string file, string body) in found.Where(f => !NavigationInventory.OutcomeScreens.Any(p => p.File == f.File && p.Body == f.Body)))
        {
            yield return $"OutcomeScreenUndocumented: {file} names 'OutcomeScreen() => {body}', which the inventory does not pin (04 2.7.3.3).";
        }

        if (found.Count != NavigationInventory.OutcomeScreens.Count)
        {
            yield return $"OutcomeScreenCountDiffers: the code has {found.Count} OutcomeScreen() bodies, the inventory {NavigationInventory.OutcomeScreens.Count}.";
        }
    }

    /// <summary>The problem of a text evidence -the text is not in its file or stands another number of times - or null.</summary>
    private static string? TextProblem(IReadOnlyDictionary<string, string> sources, NavigationEvidence evidence)
    {
        if (!sources.TryGetValue(evidence.File, out string? text) || !text.Contains(evidence.Text, StringComparison.Ordinal))
        {
            return $"the text '{evidence.Text}' is not in {evidence.File}.";
        }

        int count = Count(text, evidence.Text);
        return count != evidence.Occurrences ? $"the text '{evidence.Text}' is {count} times in {evidence.File}, the entry says {evidence.Occurrences}." : null;
    }

    private static int Count(string text, string value)
    {
        int count = 0;
        for (int at = text.IndexOf(value, StringComparison.Ordinal); at >= 0; at = text.IndexOf(value, at + value.Length, StringComparison.Ordinal))
        {
            count++;
        }

        return count;
    }

    /// <summary>A pin is the screens of the cell (the <c>E-</c> items) and the texts it holds (the <c>~</c> items); a pin without screens says the cell names none.</summary>
    private static IEnumerable<string> PinProblems(string id, string label, string pin, string cell)
    {
        string[] items = [.. pin.Split(',').Select(p => p.Trim()).Where(p => p.Length > 0)];
        string[] screens = [.. items.Where(p => !p.StartsWith('~')).Order(StringComparer.Ordinal)];
        string[] texts = [.. items.Where(p => p.StartsWith('~')).Select(p => p[1..])];
        string[] named = [.. NavigationDocument.Screens(cell)];
        if (!screens.SequenceEqual(named, StringComparer.Ordinal))
        {
            yield return $"{label}:{id}: the document names [{string.Join(", ", named)}], the entry [{string.Join(", ", screens)}].";
        }

        foreach (string text in texts.Where(t => !cell.Contains(t, StringComparison.Ordinal)))
        {
            yield return $"{label}:{id}: the document does not say '{text}'.";
        }
    }

    private static IEnumerable<string> Relabel(IEnumerable<string> problems)
    {
        return problems.Select(p => p.Replace("only in the document", "RowOnlyInDocument", StringComparison.Ordinal).Replace("only in the model", "RowOnlyInCode", StringComparison.Ordinal));
    }
}
