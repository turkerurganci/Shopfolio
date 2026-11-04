namespace Shopfolio.UnitTests.Consistency;

/// <summary>The two-directional comparison of a documented list and a model list (09 21.4 item 1): what is only in the document and what is only in the model, as the lines a failure shows; a repeated line is a problem of its own.</summary>
internal static class SetComparison
{
    public static IReadOnlyList<string> Compare(string label, IEnumerable<string> documented, IEnumerable<string> model)
    {
        List<string> documentedList = [.. documented];
        List<string> modelList = [.. model];
        List<string> problems = [];
        problems.AddRange(documentedList.GroupBy(k => k, StringComparer.Ordinal).Where(g => g.Count() > 1).Select(g => $"{label}: the document states '{g.Key}' {g.Count()} times."));
        problems.AddRange(modelList.GroupBy(k => k, StringComparer.Ordinal).Where(g => g.Count() > 1).Select(g => $"{label}: the model has '{g.Key}' {g.Count()} times."));
        problems.AddRange(documentedList.Distinct(StringComparer.Ordinal).Except(modelList, StringComparer.Ordinal).Select(k => $"{label}: only in the document: {k}"));
        problems.AddRange(modelList.Distinct(StringComparer.Ordinal).Except(documentedList, StringComparer.Ordinal).Select(k => $"{label}: only in the model: {k}"));
        return problems;
    }
}
