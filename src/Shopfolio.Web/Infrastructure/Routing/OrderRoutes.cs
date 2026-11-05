namespace Shopfolio.Infrastructure.Routing;

/// <summary>
/// The two roots of the order page (07 2.1.3; 02 3.22.5; K-562): the member's <c>/siparis/{orderNumber}</c> and the <c>/siparis/erisim/{accessKey}</c>
/// with the access key. This class sets the path patterns only; the pages are built by T511. A page template under <c>/siparis</c> starts with one of
/// the two root templates (<see cref="AddressConvention"/> checks it), so the screens bound to an order (<see cref="Screens"/>) sit under the root they
/// were reached from. A Razor <c>@page</c> directive takes a literal, so the page repeats the template text and the check keeps the copy equal.
/// </summary>
internal static class OrderRoutes
{
    /// <summary>
    /// The member's root. The order number starts with the year and a hyphen (<c>2026-7K4M9P</c>; 02 3.22.1), so <c>/siparis/erisim</c> never falls into
    /// this pattern (07 2.1.11). The constraint is a regular expression, which a route compares without regard to letter case: the number is matched
    /// case-insensitively (07 2.1.7). The block after the hyphen is not given a length here (the length is a product constant of 02 11.2).
    /// </summary>
    public const string MemberRoot = "/siparis/{orderNumber:regex(^[0-9][0-9][0-9][0-9]-[A-Za-z0-9]+\\z)}";

    /// <summary>
    /// The access-key root (07 2.1.3). The key is written in the address as base64url without padding (letters, digits, <c>-</c>, <c>_</c>) and is case
    /// sensitive (07 2.1.7): the route never changes its letters, and the lookup compares it exactly.
    /// </summary>
    public const string AccessRoot = "/siparis/erisim/{accessKey:regex(^[A-Za-z0-9_-]+\\z)}";

    /// <summary>The literal segment between <c>/siparis</c> and the access key.</summary>
    public const string AccessSegment = "erisim";

    /// <summary>The screens bound to an order (07 2.1.3): they sit under the root they were reached from.</summary>
    public static readonly IReadOnlyList<string> Screens = ["teyit", "iptal", "gecikme-feshi", "cayma", "ayip-talebi"];

    /// <summary>
    /// The order number as the lookup compares it: matched without regard to letter case (07 2.1.7), compared in upper case as the number is written
    /// (<c>2026-7K4M9P</c>). The fold is the invariant one for the same reason as the slug's. The access key is not passed through here: it is case sensitive.
    /// </summary>
    public static string NumberForLookup(string numberFromPath)
    {
        ArgumentNullException.ThrowIfNull(numberFromPath);
        return numberFromPath.ToUpperInvariant();
    }

    /// <summary>The template of a screen under the member's root.</summary>
    public static string MemberScreen(string screen) => Combine(MemberRoot, screen);

    /// <summary>The template of a screen under the access-key root.</summary>
    public static string AccessScreen(string screen) => Combine(AccessRoot, screen);

    /// <summary>Whether a page template under <c>/siparis</c> starts with one of the two roots.</summary>
    internal static bool StartsWithARoot(string template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return StartsWithRoot(template, MemberRoot) || StartsWithRoot(template, AccessRoot);
    }

    private static string Combine(string root, string screen)
    {
        ArgumentNullException.ThrowIfNull(screen);
        if (!Screens.Contains(screen, StringComparer.Ordinal))
        {
            throw new ArgumentException("The screen is not one of the order screens (07 2.1.3).", nameof(screen));
        }

        return root + "/" + screen;
    }

    private static bool StartsWithRoot(string template, string root) =>
        template.Equals(root, StringComparison.Ordinal) || template.StartsWith(root + "/", StringComparison.Ordinal);
}
