namespace Shopfolio.Infrastructure.Security;

/// <summary>The two page areas a return address can belong to (07 2.3.2; 05 6.5.6): the answer to an address that is not the site's own is the area's home page.</summary>
internal enum ReturnArea
{
    /// <summary>The storefront: the home page is <c>/</c>.</summary>
    Storefront,

    /// <summary>The panel: the home page is <c>/panel</c>.</summary>
    Panel,
}

/// <summary>
/// The return address after a sign-in or a reauthentication, the sign-out target and the list return of the panel (05 6.5.6; 07 2.3.2; 09 15.2 item 6):
/// only a path of this site. A value is the site's own when it starts with one slash that is followed by neither a slash nor a backslash - no scheme, no
/// network path - and holds no control character (the tab, the line feed and the carriage return are dropped by a browser's address parser, so <c>/\t/evil.example</c>
/// becomes <c>//evil.example</c> there; D-118). Anything else is ignored and the area's home page is the answer. A redirect to a value that came from the request goes
/// through <see cref="Resolve"/> and no other way (<c>RedirectSourceScan</c>); <see cref="Forms.FormResults.SeeOther"/> asks <see cref="IsLocalPath"/> too.
/// </summary>
internal static class ReturnPath
{
    /// <summary>The home page of the storefront.</summary>
    public const string StorefrontHome = "/";

    /// <summary>The home page of the panel (07 9.3.1).</summary>
    public const string PanelHome = "/panel";

    /// <summary>The name of the query parameter and of the form field that carry the return address (07 2.3.2).</summary>
    public const string ParameterName = "returnUrl";

    /// <summary>The path the sign-in or the reauthentication returns to: the value when it is the site's own path, else the area's home page.</summary>
    public static string Resolve(string? value, ReturnArea area)
    {
        if (value is not null && IsLocalPath(value))
        {
            return value;
        }

        return HomeOf(area);
    }

    /// <summary>The home page of an area.</summary>
    public static string HomeOf(ReturnArea area) => area switch
    {
        ReturnArea.Storefront => StorefrontHome,
        ReturnArea.Panel => PanelHome,
        _ => throw new ArgumentOutOfRangeException(nameof(area), area, "Unknown return area."),
    };

    /// <summary>
    /// Whether the value is a path of this site (05 6.5.6): it starts with one slash, the second character is neither a slash nor a backslash, and no character
    /// is a control character (U+0000 to U+001F and U+007F).
    /// </summary>
    public static bool IsLocalPath(string? value)
    {
        if (string.IsNullOrEmpty(value) || value[0] != '/')
        {
            return false;
        }

        if (value.Length > 1 && (value[1] == '/' || value[1] == '\\'))
        {
            return false;
        }

        foreach (char c in value)
        {
            if (c <= '\u001F' || c == '\u007F')
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// The target of a sign-out (07 4.34.1): the page the request came from when it needs no session, else the home page. A page needs a session when it is under
    /// <c>/hesap</c> (but for <c>/hesap/silindi</c>, 07 2.3.1), under <c>/panel</c>, or is the member root of an order page (<c>/siparis/{number}</c>, not the
    /// access-key root <c>/siparis/erisim/...</c>, 07 2.1.3); a value that is not the site's own path is the home page (05 6.5.6).
    /// </summary>
    public static string AfterSignOut(string? value)
    {
        string path = Resolve(value, ReturnArea.Storefront);
        string pathOnly = path;
        int cut = pathOnly.AsSpan().IndexOfAny('?', '#');
        if (cut >= 0)
        {
            pathOnly = pathOnly[..cut];
        }

        string[] segments = pathOnly.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            return StorefrontHome;
        }

        bool needsSession = segments[0].Equals("hesap", StringComparison.OrdinalIgnoreCase)
            ? !(segments.Length == 2 && segments[1].Equals("silindi", StringComparison.OrdinalIgnoreCase))
            : segments[0].Equals("panel", StringComparison.OrdinalIgnoreCase)
                || (segments[0].Equals("siparis", StringComparison.OrdinalIgnoreCase)
                    && segments.Length >= 2
                    && !segments[1].Equals("erisim", StringComparison.OrdinalIgnoreCase));
        return needsSession ? StorefrontHome : path;
    }
}
