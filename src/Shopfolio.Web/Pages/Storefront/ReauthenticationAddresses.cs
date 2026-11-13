using Shopfolio.Infrastructure.Values;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Storefront;

/// <summary>
/// The addresses of the re-authentication (T516; 07 UC-60 to UC-62, UC-51; 05 6.1.5; K-971, K-1186 (4)), written once for the screen (<c>/hesap/yeniden-dogrulama</c>) and for Google's return that lands on it or
/// on the account page: the screen is opened with the operation's name; a success goes to the account page with the operation and the single-use value, which the account page puts in the operation form's hidden
/// field (T517). The closed list is the three operations of 06 5.6.8 and no other text counts - a name in another case, a number or a name outside the list is not an operation.
/// </summary>
internal static class ReauthenticationAddresses
{
    /// <summary>The name of the address parameter that carries the operation (07 5.6.8; K-1186 (4)).</summary>
    public const string OperationParameter = "reauthenticatedOperation";

    /// <summary>The name of the address parameter that carries the single-use value; the system log masks it (05 9.1.5).</summary>
    public const string TokenParameter = "reauthenticationToken";

    /// <summary>The operation a text names; false for a missing text, a name outside the closed list or a name in another spelling (07 UC-60: the answer is a 303 to the account page).</summary>
    public static bool TryParse(string? raw, out ReauthenticationOperation operation)
    {
        operation = default;
        return raw is nameof(ReauthenticationOperation.PasswordChange) or nameof(ReauthenticationOperation.EmailChange) or nameof(ReauthenticationOperation.AccountDeletion)
            && Enum.TryParse(raw, ignoreCase: false, out operation);
    }

    /// <summary>The screen's address for an operation (07 UC-60).</summary>
    public static string Screen(ReauthenticationOperation operation) =>
        FramePaths.Reauthentication + "?" + OperationParameter + "=" + Uri.EscapeDataString(operation.ToString());

    /// <summary>The address a success goes to (07 UC-61, UC-51): the account page with the operation and the single-use value.</summary>
    public static string Granted(ReauthenticationOperation operation, string value) =>
        FramePaths.Account + "?" + OperationParameter + "=" + Uri.EscapeDataString(operation.ToString()) + "&" + TokenParameter + "=" + Uri.EscapeDataString(value);
}
