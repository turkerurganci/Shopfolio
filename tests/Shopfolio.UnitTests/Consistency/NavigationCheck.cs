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
                        NavigationSiteKind kind = evidence.Kind switch
                        {
                            NavigationEvidenceKind.Link => NavigationSiteKind.Link,
                            NavigationEvidenceKind.Redirect => NavigationSiteKind.Redirect,
                            _ => NavigationSiteKind.EmptyExit,
                        };
                        claimed.Add((evidence.File, kind, evidence.Text));
                        if (!scannedSet.Contains((evidence.File, kind, evidence.Text)))
                        {
                            problems.Add($"EvidenceMissing:{entry.Id}: {kind} '{evidence.Text}' is not in {evidence.File}.");
                        }

                        break;
                    case NavigationEvidenceKind.Text:
                        if (!sources.TryGetValue(evidence.File, out string? text) || !text.Contains(evidence.Text, StringComparison.Ordinal))
                        {
                            problems.Add($"EvidenceMissing:{entry.Id}: the text '{evidence.Text}' is not in {evidence.File}.");
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
        }

        foreach ((string file, NavigationSiteKind kind, string key) in scannedSet.Order())
        {
            if (!claimed.Contains((file, kind, key)) && !exempt.Contains((file, kind, key)))
            {
                problems.Add($"UndocumentedTransition: {kind} '{key}' in {file} is in no row of 04 3 and no exemption.");
            }
        }

        return problems;
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
