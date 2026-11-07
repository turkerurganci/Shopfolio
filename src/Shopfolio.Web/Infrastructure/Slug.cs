
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Routing;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The page address of a record, made from its name on the server (09 9.4 item 3; 05 3.7.1, MK-18; 02 3.30.2; 06 4.72). One function turns a name into the
/// address part: <see cref="TextNormalizer.Simplify"/> (the one folding the name search and the suggested stock code use too), every run of characters
/// other than <c>a-z</c> and <c>0-9</c> becomes one hyphen, the hyphens at both ends go; a name that leaves nothing gives the root of the record type
/// (<c>urun</c>, <c>kategori</c>, <c>hizmet</c>, <c>referans</c>, <c>sayfa</c>). There is no cut: the name has its own ceiling (P-27). A record's address
/// is never typed by the firm (02 3.30.2: K-280).
/// </summary>
internal static partial class Slug
{
    /// <summary>
    /// The address part of <paramref name="name"/>; <paramref name="rootName"/> is one of the five record prefixes of <see cref="AddressRules.RecordPrefixes"/>
    /// and is what a name without a Latin letter or digit becomes.
    /// </summary>
    public static string From(string name, string rootName)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(rootName);
        if (!AddressRules.RecordPrefixes.Contains(rootName, StringComparer.Ordinal))
        {
            throw new ArgumentException("The root is not one of the five record prefixes (06 4.72).", nameof(rootName));
        }

        string folded = TextNormalizer.Simplify(name);
        string hyphenated = NotAddressCharacters().Replace(folded, "-").Trim('-');
        return hyphenated.Length == 0 ? rootName : hyphenated;
    }

    /// <summary>
    /// The address that is free in a table: <paramref name="baseSlug"/> itself when no record holds it, otherwise <c>{base}-{n}</c> with the first
    /// <c>n</c> from 2 that no record holds (09 9.4 item 3: <c>celik-kapi</c>, <c>celik-kapi-2</c>). <paramref name="usedSlugs"/> are the addresses the table
    /// holds that are the base or the base with a number (<see cref="IsBaseOrNumbered"/>); a draft and an archived record's address counts as used (02 3.30.2),
    /// a permanently deleted one is not in the table and so is free again. The address has only <c>[a-z0-9-]</c>, so no <c>LIKE</c> meta character can be in it.
    /// </summary>
    public static string FirstFree(string baseSlug, IEnumerable<string> usedSlugs)
    {
        ArgumentNullException.ThrowIfNull(baseSlug);
        ArgumentNullException.ThrowIfNull(usedSlugs);
        HashSet<string> used = new(usedSlugs, StringComparer.Ordinal);
        if (!used.Contains(baseSlug))
        {
            return baseSlug;
        }

        for (int n = 2; ; n++)
        {
            string candidate = $"{baseSlug}-{n}";
            if (!used.Contains(candidate))
            {
                return candidate;
            }
        }
    }

    /// <summary>Whether <paramref name="slug"/> is <paramref name="baseSlug"/> or <c>{base}-{digits}</c>: the addresses that can take the base's number.</summary>
    public static bool IsBaseOrNumbered(string baseSlug, string slug)
    {
        ArgumentNullException.ThrowIfNull(baseSlug);
        ArgumentNullException.ThrowIfNull(slug);
        if (!slug.StartsWith(baseSlug, StringComparison.Ordinal))
        {
            return false;
        }

        if (slug.Length == baseSlug.Length)
        {
            return true;
        }

        ReadOnlySpan<char> rest = slug.AsSpan(baseSlug.Length);
        return rest.Length >= 2 && rest[0] == '-' && !rest[1..].ContainsAnyExceptInRange('0', '9');
    }

    /// <summary>
    /// The address a record is saved with (06 4.72.3): when the address is not fixed yet (<paramref name="fixedAt"/> is empty) it is made again from the name
    /// with the table's used addresses read by <paramref name="readUsed"/>; when it is fixed - the record's first publication - the current address stays,
    /// whatever the name has become (02 3.30.2). <paramref name="readUsed"/> gets the base and returns the table's addresses that start with it; a record's
    /// own current address is among them and is dropped here, so an unfixed record that is saved again with the same name keeps its address.
    /// </summary>
    public static async Task<string> ForSaveAsync(
        string name,
        string rootName,
        string currentSlug,
        DateTimeOffset? fixedAt,
        Func<string, CancellationToken, Task<IReadOnlyCollection<string>>> readUsed,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentSlug);
        ArgumentNullException.ThrowIfNull(readUsed);
        if (fixedAt is not null)
        {
            return currentSlug;
        }

        string baseSlug = From(name, rootName);
        IReadOnlyCollection<string> read = await readUsed(baseSlug, ct);
        return FirstFree(baseSlug, read.Where(s => IsBaseOrNumbered(baseSlug, s) && !string.Equals(s, currentSlug, StringComparison.Ordinal)));
    }

    [GeneratedRegex("[^a-z0-9]+", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex NotAddressCharacters();
}

/// <summary>
/// The stock code the variant form suggests (05 7.8.1, MK-32): the product address in capitals, its first twenty characters (a hyphen the cut leaves at the
/// end goes), a hyphen and the two-digit order of the variant - <c>AHSAP-MASA-01</c>. The suggestion is only a suggestion: the code the administrator writes
/// is kept as written (02 3.3.4). The address it starts from is the one <see cref="Slug.From"/> made, so the folding is the one common function.
/// </summary>
internal static class StockCodeSuggestion
{
    /// <summary>The characters of the address the suggestion takes (05 7.8.1).</summary>
    public const int AddressLength = 20;

    /// <summary>
    /// The suggested code for the variant at <paramref name="order"/> (1 for the first, and for the only variant of a product without options). A code another
    /// variant already holds raises the order by one until it is free; <paramref name="usedCodes"/> are compared in the identity form of the stock code column.
    /// </summary>
    public static string Suggest(string productSlug, int order, IEnumerable<string> usedCodes)
    {
        ArgumentNullException.ThrowIfNull(productSlug);
        ArgumentNullException.ThrowIfNull(usedCodes);
        ArgumentOutOfRangeException.ThrowIfLessThan(order, 1);
        HashSet<string> used = new(usedCodes.Select(TextNormalizer.NormalizeIdentity), StringComparer.Ordinal);
        string prefix = Prefix(productSlug);
        for (int n = order; ; n++)
        {
            string code = $"{prefix}-{n:00}";
            if (!used.Contains(TextNormalizer.NormalizeIdentity(code)))
            {
                return code;
            }
        }
    }

    /// <summary>The part of every suggested code of a product that precedes the hyphen and the order: the address in capitals, cut at <see cref="AddressLength"/>.</summary>
    public static string Prefix(string productSlug)
    {
        ArgumentNullException.ThrowIfNull(productSlug);
        string head = productSlug.Length > AddressLength ? productSlug[..AddressLength] : productSlug;
        return head.TrimEnd('-').ToUpperInvariant();
    }
}
