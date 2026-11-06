using System.Collections.Frozen;

using Microsoft.Extensions.Primitives;

namespace Shopfolio.Infrastructure.Notices;

/// <summary>
/// The <c>notice</c> parameter of a redirect address (07 2.2.3, 6.3; 09 5.3 item 6, 8.2 item 4): a short-lived notice, a result state or the reason of a return
/// travels in the address as the name of a member of <see cref="NoticeCode"/> and nowhere else - no cookie and no record on the server. The address carries
/// no free text and no personal data, because the only value that can be written is a member of the closed list. A value that is not a member is ignored:
/// the page opens without a notice and no error is returned (07 2.6.6), and the value is never reflected in the page. The read is exact - the member name
/// as written, not a number, a list or another letter case - because a number or a comma list would reach members that the document's list does not name by that spelling.
/// </summary>
internal static class NoticeParameter
{
    /// <summary>The name of the query parameter (07 2.2.3).</summary>
    public const string ParameterName = "notice";

    private static readonly FrozenDictionary<string, NoticeCode> _byName =
        Enum.GetValues<NoticeCode>().ToFrozenDictionary(c => c.ToString(), c => c, StringComparer.Ordinal);

    /// <summary>The member a raw value names, or <c>false</c> when the value is not exactly the name of a member.</summary>
    public static bool TryParse(string? raw, out NoticeCode code)
    {
        if (raw is not null && _byName.TryGetValue(raw, out code))
        {
            return true;
        }

        code = default;
        return false;
    }

    /// <summary>
    /// The notice a request carries, when it is a member of the list <paramref name="kind"/>; otherwise <c>null</c>. A repeated parameter, a value that is not a
    /// member and a member of another list are all ignored. Nothing is consumed or written: the notice stays in the address, so a reload repeats it (07 2.2.8).
    /// </summary>
    public static NoticeCode? Read(HttpRequest request, NoticeKind kind)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!request.Query.TryGetValue(ParameterName, out StringValues values) || values.Count != 1)
        {
            return null;
        }

        return TryParse(values[0], out NoticeCode code) && NoticeKinds.Of(code) == kind ? code : null;
    }

    /// <summary>
    /// The redirect address with the notice: the path of this site and its own query, then <c>notice=&lt;Code&gt;</c> (07 2.2.3). The code is the member's name,
    /// which is ASCII letters only, so nothing needs escaping. A path that already carries a <c>notice</c> parameter, a fragment or a member that is not
    /// defined is refused: the address of a redirect holds one notice, and the writer is the one that knows the code.
    /// </summary>
    public static string Address(string localPath, NoticeCode code)
    {
        ArgumentNullException.ThrowIfNull(localPath);
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(nameof(code), code, "A notice code is a member of the closed list (07 6.3).");
        }

        if (!Security.ReturnPath.IsLocalPath(localPath))
        {
            throw new ArgumentException("The target of a redirect is a path of this site that starts with one slash (05 6.5.6).", nameof(localPath));
        }

        if (localPath.Contains('#', StringComparison.Ordinal))
        {
            throw new ArgumentException("The address of a redirect with a notice carries no fragment.", nameof(localPath));
        }

        int queryStart = localPath.IndexOf('?', StringComparison.Ordinal);
        if (queryStart >= 0 && HasNotice(localPath[(queryStart + 1)..]))
        {
            throw new ArgumentException("The address already carries a notice parameter.", nameof(localPath));
        }

        string separator = queryStart < 0 ? "?" : (localPath.EndsWith('?') || localPath.EndsWith('&') ? string.Empty : "&");
        return localPath + separator + ParameterName + "=" + code;
    }

    private static bool HasNotice(string query) =>
        query.Split('&').Any(p => p.Split('=', 2)[0].Equals(ParameterName, StringComparison.OrdinalIgnoreCase));
}
