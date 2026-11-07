using System.Globalization;
using System.Text.RegularExpressions;

namespace Shopfolio.Endpoints.Files;

/// <summary>
/// The <c>Content-Range</c> of a chunk (07 4.31.2; 09 15.7 item 6 (b)): <c>bytes from-to</c>, with or without the <c>/total</c> part - the total is ignored, the size is
/// the start's. The header is well formed only when it is the one header, <c>to</c> is not before <c>from</c> and the span (<c>to - from + 1</c>) is the length of the
/// body that came with it; anything else is the 400 of a missing or malformed header.
/// </summary>
internal static partial class ChunkRange
{
    /// <summary>The start of the chunk when the header is well formed for a body of <paramref name="bodyLength"/> bytes.</summary>
    public static bool TryParse(IReadOnlyList<string?> values, long bodyLength, out long start)
    {
        ArgumentNullException.ThrowIfNull(values);
        start = 0;
        if (values.Count != 1 || values[0] is not { Length: > 0 } header)
        {
            return false;
        }

        Match match = Shape().Match(header);
        if (!match.Success
            || !long.TryParse(match.Groups["from"].ValueSpan, NumberStyles.None, CultureInfo.InvariantCulture, out long from)
            || !long.TryParse(match.Groups["to"].ValueSpan, NumberStyles.None, CultureInfo.InvariantCulture, out long to)
            || to < from
            || to - from + 1 != bodyLength)
        {
            return false;
        }

        start = from;
        return true;
    }

    [GeneratedRegex(@"^bytes (?<from>\d{1,18})-(?<to>\d{1,18})(?:/(?:\d{1,18}|\*))?$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Shape();
}
