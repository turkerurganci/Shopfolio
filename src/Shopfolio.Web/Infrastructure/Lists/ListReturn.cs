using Shopfolio.Infrastructure.Security;

namespace Shopfolio.Infrastructure.Lists;

/// <summary>
/// The return from a record to the list it was opened from (07 2.6.8; 05 6.5.6; 04 2.13.3). The link from a panel list to a record carries the list's address in
/// <c>returnUrl</c> (<see cref="LinkTo"/>); the record's "back to the list" and the redirect after the record is saved use that value (<see cref="Target"/>), so
/// the user finds the page and the filter they left. The value comes from the request, so every endpoint that reads it asks <see cref="Target"/> where it reads
/// it: the check is the one local-path rule of <see cref="ReturnPath.IsLocalPath"/> (T206, T207), not a copy of it. A value that is not a path of this site, or
/// is missing, returns to the list's root. A redirect to the result goes through <c>FormResults.SeeOther</c>, which asks the same rule again. The member's order
/// history (UC-74) is outside this rule: its order page reads no <c>returnUrl</c> and returns to the first page of the history (07 2.6.8; K-1208).
/// </summary>
internal static class ListReturn
{
    /// <summary>The address of a record with the list's address as its <c>returnUrl</c> (the list's address is escaped as a query value).</summary>
    public static string LinkTo(string recordPath, string listAddress)
    {
        ArgumentException.ThrowIfNullOrEmpty(recordPath);
        ArgumentNullException.ThrowIfNull(listAddress);
        if (!ReturnPath.IsLocalPath(recordPath) || !ReturnPath.IsLocalPath(listAddress))
        {
            throw new ArgumentException("A link to a record and the list address it returns to are paths of this site (05 6.5.6).");
        }

        char separator = recordPath.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return recordPath + separator + ReturnPath.ParameterName + "=" + Uri.EscapeDataString(listAddress);
    }

    /// <summary>The address the record returns to: the <c>returnUrl</c> value when it is a path of this site, else the list's root.</summary>
    public static string Target(string? returnUrl, string listRoot) => ReturnPath.ResolveOr(returnUrl, listRoot);
}
