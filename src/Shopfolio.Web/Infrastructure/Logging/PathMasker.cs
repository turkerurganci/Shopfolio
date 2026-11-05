using System.Text;

namespace Shopfolio.Infrastructure.Logging;

/// <summary>
/// The path layer of the masking (09 18.2 item 2b; 05 9.1.5): the segment of a path that carries a link key becomes <c>{key}</c>, the other
/// segments stay, and the value of every query string pair becomes <c>*</c>. The paths that carry a key are a closed list derived from
/// 07 2.1.3, 2.1.4 and 2.1.9 (the same list as the reverse proxy's, DEPLOY_RUNBOOK); a PR that adds a keyed path to 07 updates this list too.
/// </summary>
internal static class PathMasker
{
    /// <summary>What replaces the key segment and the query values.</summary>
    public const string KeyPlaceholder = "{key}";

    public const string QueryValuePlaceholder = "*";

    /// <summary>
    /// The keyed paths: the segments before the key. <c>/siparis/erisim/{key}</c> and every screen below it is the first row (the key is the
    /// third segment); the others have the key right after the prefix.
    /// </summary>
    public static IReadOnlyList<IReadOnlyList<string>> KeyedPrefixes { get; } =
    [
        ["siparis", "erisim"],
        ["odeme", "donus"],
        ["indir"],
        ["dogrulama"],
        ["sifre-sifirlama"],
        ["geri-al"],
        ["panel", "sifre-sifirlama"],
        ["panel", "davet"],
        ["panel", "dogrulama"],
        ["panel", "geri-al"],
    ];

    /// <summary>The same list as paths, in the document's notation: <c>/siparis/erisim/{key}</c>.</summary>
    public static IReadOnlyList<string> KeyedPaths { get; } =
        [.. KeyedPrefixes.Select(prefix => "/" + string.Join('/', prefix) + "/" + KeyPlaceholder)];

    /// <summary>Masks a request path with its query string (<c>/siparis/erisim/ABC123/teyit?email=a@b.com</c> becomes <c>/siparis/erisim/{key}/teyit?email=*</c>).</summary>
    public static string Mask(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        int queryStart = path.IndexOf('?', StringComparison.Ordinal);
        string pathPart = queryStart < 0 ? path : path[..queryStart];
        string? query = queryStart < 0 ? null : path[(queryStart + 1)..];

        StringBuilder masked = new(MaskSegments(pathPart));
        if (query is not null)
        {
            masked.Append('?').Append(MaskQuery(query));
        }

        return masked.ToString();
    }

    private static string MaskSegments(string path)
    {
        string[] segments = path.Split('/');

        // Empty segments (the one before the first '/', and those of '//') are skipped: a path with an empty segment in front of the prefix
        // (//siparis/erisim/ABC123) would otherwise not match and its key would reach the line.
        List<int> filled = [];
        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i].Length > 0)
            {
                filled.Add(i);
            }
        }

        foreach (IReadOnlyList<string> prefix in KeyedPrefixes)
        {
            if (filled.Count > prefix.Count && StartsWithPrefix(segments, filled, prefix))
            {
                segments[filled[prefix.Count]] = KeyPlaceholder;
                break;
            }
        }

        return string.Join('/', segments);
    }

    private static bool StartsWithPrefix(string[] segments, List<int> filled, IReadOnlyList<string> prefix)
    {
        for (int i = 0; i < prefix.Count; i++)
        {
            if (!string.Equals(segments[filled[i]], prefix[i], StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    // Every pair keeps its name and loses its value; a token without '=' is a value too (a bare token can be an address), so it is replaced whole.
    private static string MaskQuery(string query)
    {
        if (query.Length == 0)
        {
            return string.Empty;
        }

        return string.Join('&', query.Split('&').Select(pair =>
        {
            int equals = pair.IndexOf('=', StringComparison.Ordinal);
            return equals < 0 ? QueryValuePlaceholder : pair[..equals] + "=" + QueryValuePlaceholder;
        }));
    }
}
