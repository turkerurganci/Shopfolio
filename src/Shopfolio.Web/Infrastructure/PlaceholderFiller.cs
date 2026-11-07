using System.Text.RegularExpressions;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The one function that fills the placeholders of a document template (09 9.3 item 3; 09 11.4 item 4): the token is <c>{{Name}}</c>, the name is a field name of the order
/// (06 4.17) or, in the generated legal texts, a company value. Production and display both call this and nothing else, so the same template and the same values
/// give the same text everywhere. The filler does not escape: the escape belongs to the display channel (09 9.3 item 6).
/// <list type="bullet">
/// <item>A value is never scanned again: a value that itself contains <c>{{Total}}</c> does not expand (one pass of one regular expression).</item>
/// <item>A token that has no entry stays as it is (the production stage: the order's tokens wait in the version text); a <see langword="null"/> value is an empty text (an erased field).</item>
/// <item><c>requireAll</c> throws for every token that has no entry. The template is scanned, never the result, because a value may contain <c>{{</c>.</item>
/// </list>
/// </summary>
internal static partial class PlaceholderFiller
{
    public static string Fill(string template, IReadOnlyDictionary<string, string?> values, bool requireAll)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(values);
        if (requireAll)
        {
            string[] missing = [.. TokenNames(template).Where(name => !values.ContainsKey(name)).Distinct(StringComparer.Ordinal)];
            if (missing.Length > 0)
            {
                throw new InvalidOperationException($"The template has tokens without a value: {string.Join(", ", missing)}.");
            }
        }

        return TokenPattern().Replace(
            template,
            match => values.TryGetValue(match.Groups[1].Value, out string? value) ? value ?? string.Empty : match.Value);
    }

    /// <summary>The names of the tokens in a template, in the order they appear (a name more than once appears more than once).</summary>
    public static IEnumerable<string> TokenNames(string template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return TokenPattern().Matches(template).Select(m => m.Groups[1].Value);
    }

    [GeneratedRegex(@"\{\{([A-Za-z]+)\}\}", RegexOptions.CultureInvariant)]
    private static partial Regex TokenPattern();
}
