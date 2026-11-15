namespace Shopfolio.UnitTests.Consistency;

internal static partial class FormFieldRuleEvidence
{
    /// <summary>
    /// The lines of <paramref name="own"/> united with the lines of <see cref="Lines"/> that are of the same kind of key: the place keys (<c>row|place</c>) when <paramref name="places"/> is set, the row keys
    /// otherwise. A key of <paramref name="own"/> that has an empty entry (no check of its own of a kind the row names) keeps the lines the evidence gives it.
    /// </summary>
    internal static IReadOnlyDictionary<string, string[]> United(IReadOnlyDictionary<string, string[]> own, bool places)
    {
        Dictionary<string, string[]> united = new(own, StringComparer.Ordinal);
        foreach ((string key, string[] lines) in Lines.Where(l => l.Key.Contains('|', StringComparison.Ordinal) == places))
        {
            string[] held = united.TryGetValue(key, out string[]? known) ? known : [];
            united[key] = [.. held.Concat(lines).Distinct(StringComparer.Ordinal)];
        }

        return united;
    }
}
