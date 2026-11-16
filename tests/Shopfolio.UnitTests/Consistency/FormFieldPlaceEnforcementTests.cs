using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// KK6 for the places of a field row that covers several property places (T547 correction 4, E1): the check of a place that carries no rule attribute lives in a handler or a module, and the row's rule used
/// to be proved by the line of one place for all of them (the union by kind). Each such place has its own lines in <see cref="FormFieldPlaceEnforcement"/>; the checks that must fail read a changed copy of
/// the sources (the real code with one check changed or removed) or a changed copy of the table, never the files (09 19.2 item 3).
/// </summary>
[Trait("Consistency", "true")]
public sealed class FormFieldPlaceEnforcementTests
{
    private static readonly Lazy<string> Document = new(StateRoleDocument.Load);

    private static readonly Lazy<IReadOnlyDictionary<string, string>> Sources = new(() => NavigationSites.Load(RepositoryRoot.Find()));

    private static IEnumerable<FieldEntry> RowsWithSeveralPlaces() => FormFieldInventory.Entries.Where(e => FormFieldCheck.PropertyPlaces(e).Count() >= 2);

    private static string[] UnpinnedKeys() =>
    [
        .. RowsWithSeveralPlaces().SelectMany(e => FormFieldCheck.PropertyPlaces(e).Select(p => FormFieldCheck.PlaceKey(e.Row, p))).Where(k => !FormFieldPlaceMarks.Marks.ContainsKey(k)),
    ];

    private static IReadOnlyList<string> ProblemsWith(string file, string old, string replacement)
    {
        string text = Sources.Value[file];
        Assert.Equal(1, FormFieldCheck.Count(text, old));
        return FormFieldCheck.Problems(Document.Value, new Dictionary<string, string>(Sources.Value, StringComparer.Ordinal) { [file] = text.Replace(old, replacement, StringComparison.Ordinal) });
    }

    private static string[] PlacesReported(IEnumerable<string> problems)
        => [.. problems.Where(p => p.StartsWith("PlaceLineNotInCode:", StringComparison.Ordinal)).Select(p => p["PlaceLineNotInCode:".Length..p.IndexOf(": ", StringComparison.Ordinal)]).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];

    // The code as it is: every place of a row with several places has its entry (67 places carry no attribute), every line of an entry is held once by its file, no entry is of another place.
    [Fact]
    public void EveryPlace_OfARowWithSeveralPlaces_HasItsEntry_AndItsLinesAreInTheCode()
    {
        string[] keys = UnpinnedKeys();

        Assert.Equal(67, keys.Length);
        Assert.True(keys.All(FormFieldPlaceEnforcement.Lines.ContainsKey), "A place without an entry: " + string.Join(", ", keys.Where(k => !FormFieldPlaceEnforcement.Lines.ContainsKey(k))));
        foreach (FieldEntry entry in RowsWithSeveralPlaces())
        {
            Assert.Empty(FormFieldCheck.PlaceLineProblems(entry, Sources.Value, FormFieldPlaceEnforcement.Lines, FormFieldPlaceMarks.Marks));
        }

        Assert.Empty(FormFieldCheck.StrayPlaceLines(FormFieldInventory.Entries, FormFieldPlaceEnforcement.Lines));
    }

    // E1: a place with no entry is reported on its own (not passed in silence), and an entry of a place the inventory does not hold is reported.
    [Fact]
    public void PlaceWithoutAnEntry_AndAnEntryOfNoPlace_AreReported()
    {
        List<string> silent = [];
        foreach (string key in UnpinnedKeys())
        {
            Dictionary<string, string[]> changed = new(FormFieldPlaceEnforcement.Lines, StringComparer.Ordinal);
            changed.Remove(key);
            string row = key[..key.IndexOf('|', StringComparison.Ordinal)];
            string place = key[(key.IndexOf('|', StringComparison.Ordinal) + 1)..];
            FieldEntry entry = FormFieldInventory.Entries.Single(e => e.Row == row);
            if (!FormFieldCheck.PlaceLineProblems(entry, Sources.Value, changed, FormFieldPlaceMarks.Marks).Any(p => p.StartsWith($"PlaceEnforcementMissing:{row}:", StringComparison.Ordinal) && p.Contains($"'{place}'", StringComparison.Ordinal)))
            {
                silent.Add(key);
            }
        }

        Assert.True(silent.Count == 0, "A place without an entry was not reported:\n" + string.Join('\n', silent));

        Dictionary<string, string[]> stray = new(FormFieldPlaceEnforcement.Lines, StringComparer.Ordinal) { ["7.2.3.1|Panel.AdminProductQuery.NotAPlace"] = [], ["7.1.1.1|Panel.NoRow.Name"] = [] };
        Assert.Equal(2, FormFieldCheck.StrayPlaceLines(FormFieldInventory.Entries, stray).Count(p => p.StartsWith("PlaceEnforcementNotInInventory:", StringComparison.Ordinal)));
    }

    // E1, every (place, kind) line of the real table: the piece leaves the real code (or is held twice) and the comparison reports exactly the places that hold that piece - a place is reported for its own
    // code, whatever the other places of its row keep.
    [Fact]
    public void EveryLineOfEveryPlace_RemovedFromTheCode_IsReportedForThePlacesThatHoldIt()
    {
        List<string> wrong = [];
        List<(string Key, string File, string Piece)> all =
        [
            .. FormFieldPlaceEnforcement.Lines.SelectMany(p => p.Value.Select(l => (p.Key, File: l[(l.IndexOf('|', StringComparison.Ordinal) + 1)..l.IndexOf('#', StringComparison.Ordinal)], Piece: l[(l.IndexOf('#', StringComparison.Ordinal) + 1)..]))),
        ];
        Assert.True(all.Count >= 80, $"The table must hold the lines of the places ({all.Count}).");
        foreach ((string file, string piece) in all.Select(a => (a.File, a.Piece)).Distinct())
        {
            string text = FormFieldCheck.Collapse(Sources.Value[file]);
            string collapsed = FormFieldCheck.Collapse(piece);
            int at = text.IndexOf(collapsed, StringComparison.Ordinal);
            if (at < 0 || FormFieldCheck.Count(text, collapsed) != 1)
            {
                wrong.Add($"{file}: '{piece}' is held {FormFieldCheck.Count(text, collapsed)} times");
                continue;
            }

            string removed = text[..at] + "RemovedFromTheCode" + text[(at + collapsed.Length)..];
            string[] reported = PlacesReported(FormFieldCheck.Problems(Document.Value, new Dictionary<string, string>(Sources.Value, StringComparer.Ordinal) { [file] = removed }));
            foreach (string key in all.Where(a => a.File == file && a.Piece == piece).Select(a => a.Key).Distinct(StringComparer.Ordinal))
            {
                if (!reported.Contains(key))
                {
                    wrong.Add($"{file}: removing '{piece}' did not report the place {key} (reported: {string.Join(", ", reported)})");
                }
            }
        }

        Assert.True(wrong.Count == 0, "A removed piece of code was not reported for its place:\n" + string.Join('\n', wrong));
    }

    // E1, the experiments of the validation report on the real code: one handler's or one module's check of a place changes and the place is reported while the places of the same row that keep their check
    // are not (a place's evidence is its own code, not the other place's line of the same kind).
    [Theory]
    [InlineData("Modules/CorporateContent/ContentInputs.cs", "RequiredWithCeiling(rejections, nameof(PortfolioItemInput.Title), input.Title, ContentRules.TitleMaxLength)", "RequiredWithCeiling(rejections, nameof(PortfolioItemInput.Title), input.Title, int.MaxValue)", "7.2.16.1|Panel.SaveContentInput.Title")]
    [InlineData("Modules/CorporateContent/ContentInputs.cs", "RequiredWithCeiling(rejections, nameof(CustomPageInput.Title), input.Title, ContentRules.TitleMaxLength)", "RequiredWithCeiling(rejections, nameof(CustomPageInput.Title), input.Title, int.MaxValue)", "7.2.16.1|Panel.SaveContentInput.Title")]
    [InlineData("Modules/CorporateContent/ContentInputs.cs", "RequiredWithCeiling(rejections, nameof(BranchInput.Name), input.Name, ContentRules.TitleMaxLength)", "RequiredWithCeiling(rejections, nameof(BranchInput.Name), input.Name, int.MaxValue)", "7.2.16.1|Panel.SaveContentInput.Name")]
    [InlineData("Modules/CorporateContent/ContentInputs.cs", "Ceiling(rejections, nameof(BranchInput.WorkingHours), Text(input.WorkingHours), ContentRules.ShortDescriptionMaxLength)", "Ceiling(rejections, nameof(BranchInput.WorkingHours), Text(input.WorkingHours), int.MaxValue)", "7.2.16.2|Panel.SaveContentInput.WorkingHours")]
    [InlineData("Modules/CorporateContent/ContentInputs.cs", "Ceiling(rejections, nameof(PortfolioItemInput.ShortDescription), Text(input.ShortDescription), ContentRules.ShortDescriptionMaxLength)", "Ceiling(rejections, nameof(PortfolioItemInput.ShortDescription), Text(input.ShortDescription), int.MaxValue)", "7.2.16.2|Panel.SaveContentInput.ShortDescription")]
    [InlineData("Modules/Ordering/PanelItemSteps.cs", "!NoticeIbanRule.Allows(request.NoticeIban)", "false", "7.2.10.4|Panel.RecordWithdrawalInput.Iban")]
    [InlineData("Modules/Ordering/PanelOrderSteps.cs", "sameDay.Count > 0 && deliveredBeforeDeclaration is null", "false", "7.2.8.2|Panel.MarkDeliveredInput.IsDeliveredBeforeDeclaration")]
    public void ChangeOfOneHandlersCheck_OnTheRealCode_ReportsThatPlaceAndNoOtherPlace(string file, string old, string replacement, string expectedPlace)
    {
        string[] reported = PlacesReported(ProblemsWith(file, old, replacement));

        Assert.Equal([expectedPlace], reported);
    }

    // The two places of a row whose two handlers (or two record kinds) check the same field of two inputs do not share a line: each reads the check in its own code (a statement that checks both fields at once -
    // the audit trail's range, the shipment's carrier and name - is the line of both, and those rows are not in this list).
    [Fact]
    public void TwoPlacesOfOneRow_WithTheSameKindOfCheck_ReadItInTheirOwnHandlers()
    {
        string[][] pairs =
        [
            ["7.2.8.1|Panel.MarkDeliveredInput.DeliveryDate", "7.2.8.1|Panel.CorrectDeliveryDateInput.DeliveryDate"],
            ["7.2.8.2|Panel.MarkDeliveredInput.IsDeliveredBeforeDeclaration", "7.2.8.2|Panel.CorrectDeliveryDateInput.IsDeliveredBeforeDeclaration"],
            ["7.2.8.3|Panel.ReceiveReturnInput.ArrivalDate", "7.2.8.3|Panel.CorrectArrivalDateInput.ArrivalDate"],
            ["7.2.10.1|Panel.RecordWithdrawalInput.DeclarationDate", "7.2.10.1|Panel.RecordLateDeliveryTerminationInput.DeclarationDate"],
            ["7.2.10.3|Panel.RecordWithdrawalInput.DeclarationDate", "7.2.10.3|Panel.RecordLateDeliveryTerminationInput.DeclarationDate"],
            ["7.2.16.1|Panel.SaveContentInput.Name", "7.2.16.1|Panel.SaveContentInput.Title"],
        ];
        foreach (string[] pair in pairs)
        {
            Assert.NotEmpty(FormFieldPlaceEnforcement.Lines[pair[0]]);
            Assert.NotEmpty(FormFieldPlaceEnforcement.Lines[pair[1]]);
            Assert.Empty(FormFieldPlaceEnforcement.Lines[pair[0]].Intersect(FormFieldPlaceEnforcement.Lines[pair[1]], StringComparer.Ordinal));
        }
    }
}
