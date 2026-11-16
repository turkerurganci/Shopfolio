using Shopfolio.Infrastructure.Validation;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// KK6 for a field row that covers several property places (T547 correction 3, C1): the row's rule and requirement used to be read from the union of its places, so a place that lost its attribute was "proved"
/// by the other. Each place is read on its own against <see cref="FormFieldPlaceMarks"/>; the checks that must fail give the comparison a changed copy of the marks, never the source files (09 19.2 item 3).
/// </summary>
[Trait("Consistency", "true")]
public sealed class FormFieldPlaceMarksTests
{
    private static List<(string Where, string[] Marks)> RealPlaces(FieldEntry entry)
        => [.. FormFieldCheck.PropertyPlaces(entry).Select(w => (w, Marks: FormFieldEnforcement.RuleMarks(FormFieldCheck.Find(w)!).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray()))];

    private static IEnumerable<FieldEntry> RowsWithSeveralPlaces() => FormFieldInventory.Entries.Where(e => FormFieldCheck.PropertyPlaces(e).Count() >= 2);

    // The code as it is: every place of every such row carries what its pin says, and every place is found in the code.
    [Fact]
    public void EveryPlace_OfARowWithSeveralPlaces_CarriesItsPinnedMarks()
    {
        foreach (FieldEntry entry in RowsWithSeveralPlaces())
        {
            List<(string Where, string[] Marks)> places = RealPlaces(entry);
            Assert.Empty(FormFieldCheck.PlaceMarkProblems(entry.Row, places, FormFieldPlaceMarks.Marks));
        }

        Assert.NotEmpty(FormFieldPlaceMarks.Marks);
        Assert.Empty(FormFieldCheck.StrayPlacePins(FormFieldInventory.Entries, FormFieldPlaceMarks.Marks));
    }

    // C1: one place of two (or more) loses one of its attributes - the other place still carries the kind, but the lost one is reported on its own, with its place named.
    [Fact]
    public void EveryAttribute_TakenOffOnePlaceOfARowWithSeveralPlaces_IsReportedForThatPlace()
    {
        List<string> silent = [];
        int experiments = 0;
        foreach (FieldEntry entry in RowsWithSeveralPlaces())
        {
            List<(string Where, string[] Marks)> places = RealPlaces(entry);
            for (int i = 0; i < places.Count; i++)
            {
                foreach (string mark in places[i].Marks)
                {
                    experiments++;
                    List<(string Where, string[] Marks)> changed = [.. places];
                    changed[i] = (places[i].Where, [.. places[i].Marks.Where(m => m != mark)]);
                    if (!FormFieldCheck.PlaceMarkProblems(entry.Row, changed, FormFieldPlaceMarks.Marks).Any(p => p.StartsWith($"PlaceMarksDiffer:{entry.Row}:", StringComparison.Ordinal) && p.Contains($"'{places[i].Where}'", StringComparison.Ordinal)))
                    {
                        silent.Add($"{entry.Row}: {places[i].Where} - {mark}");
                    }
                }
            }
        }

        Assert.True(experiments >= 10, "The experiment must cover the attribute-bearing places of the rows with several places.");
        Assert.True(silent.Count == 0, "An attribute taken off one place was not reported:\n" + string.Join('\n', silent));
    }

    // The two rows of the report: the category name's required mark and the discount row's end-date mark (the other place of the row still carries the kind).
    [Fact]
    public void RequiredMark_OfTheCreateInput_AndDateMark_OfTheEndDate_AreReported_WhileTheOtherPlaceStillCarriesThem()
    {
        FieldEntry category = FormFieldInventory.Entries.Single(e => e.Row == "7.2.5.1");
        List<(string Where, string[] Marks)> places = RealPlaces(category);
        places[0] = (places[0].Where, [.. places[0].Marks.Where(m => m != nameof(FieldRequiredAttribute))]);

        Assert.Contains(FormFieldCheck.PlaceMarkProblems("7.2.5.1", places, FormFieldPlaceMarks.Marks), p => p.Contains("CreateCategoryInput.Name", StringComparison.Ordinal) && p.StartsWith("PlaceMarksDiffer:7.2.5.1", StringComparison.Ordinal));
        Assert.Contains(nameof(FieldRequiredAttribute), places[1].Marks);

        FieldEntry discount = FormFieldInventory.Entries.Single(e => e.Row == "7.2.4.18");
        List<(string Where, string[] Marks)> dates = RealPlaces(discount);
        int end = dates.FindIndex(p => p.Where.EndsWith(".EndDate", StringComparison.Ordinal));
        dates[end] = (dates[end].Where, []);

        Assert.Contains(FormFieldCheck.PlaceMarkProblems("7.2.4.18", dates, FormFieldPlaceMarks.Marks), p => p.Contains("SaveProductDiscountRow.EndDate", StringComparison.Ordinal) && p.StartsWith("PlaceMarksDiffer:7.2.4.18", StringComparison.Ordinal));
        Assert.Contains(nameof(InvalidDateAttribute), dates.Single(p => p.Where.EndsWith(".StartDate", StringComparison.Ordinal)).Marks);
    }

    // A place that gains an attribute, a pin whose place is gone and a pin of a row with one place are reported too (the table must say what the code has).
    [Fact]
    public void AttributeGained_ByAPlace_AndAStrayPin_AreReported()
    {
        FieldEntry entry = FormFieldInventory.Entries.Single(e => e.Row == "7.2.3.3");
        List<(string Where, string[] Marks)> places = RealPlaces(entry);
        places[0] = (places[0].Where, [nameof(FieldRequiredAttribute)]);

        Assert.Contains(FormFieldCheck.PlaceMarkProblems("7.2.3.3", places, FormFieldPlaceMarks.Marks), p => p.StartsWith("PlaceMarksDiffer:7.2.3.3", StringComparison.Ordinal));

        Dictionary<string, string[]> pins = new(FormFieldPlaceMarks.Marks, StringComparer.Ordinal)
        {
            ["7.2.5.1|Panel.CreateCategoryInput.Gone"] = [nameof(FieldRequiredAttribute)],
            ["7.2.5.2|Panel.MoveCategoryInput.ParentCategoryId"] = [nameof(FieldRequiredAttribute)],
        };
        IReadOnlyList<string> stray = [.. FormFieldCheck.StrayPlacePins(FormFieldInventory.Entries, pins)];

        Assert.Equal(2, stray.Count);
        Assert.All(stray, p => Assert.StartsWith("PlaceMarksNotInInventory:", p, StringComparison.Ordinal));
    }

    // A row with one property place is not read this way (its marks are the row's): no problem even without a pin.
    [Fact]
    public void RowWithOnePropertyPlace_NeedsNoPin()
    {
        Assert.Empty(FormFieldCheck.PlaceMarkProblems("7.9.9.9", [("Panel.X.Y", [nameof(FieldRequiredAttribute)])], new Dictionary<string, string[]>()));
    }
}
