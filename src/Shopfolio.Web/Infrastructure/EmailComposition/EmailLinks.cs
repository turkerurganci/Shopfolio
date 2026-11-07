using Shopfolio.Infrastructure.SearchEngine;

namespace Shopfolio.Infrastructure.EmailComposition;

/// <summary>
/// The addresses an e-mail carries (08 2.2.5 items 2 and 4; 09 11.4 item 2): full <c>https</c> addresses whose root is the installation's domain setting
/// (<see cref="SiteAddress"/>), never the request's <c>Host</c> or <c>X-Forwarded-Host</c> header - a forged header changes nothing here. The order page is
/// the member's own address <c>/siparis/{number}</c> or the guest's access key address <c>/siparis/erisim/{key}</c> (07 2.1.3); a value that goes into
/// a path is percent-encoded.
/// </summary>
internal sealed class EmailLinks
{
    private const string ImagePrefix = "/gorsel/";

    private readonly SiteAddress _site;

    public EmailLinks(SiteAddress site)
    {
        ArgumentNullException.ThrowIfNull(site);
        _site = site;
    }

    /// <summary>The order page of a member (07 2.1.3): <c>/siparis/{order number}</c>; it opens behind the account's sign-in.</summary>
    public Uri MemberOrder(string orderNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderNumber);
        return Absolute("/siparis/" + Uri.EscapeDataString(orderNumber));
    }

    /// <summary>The order page of a guest, and of an order whose account is deleted (02 3.22.5): the access key address <c>/siparis/erisim/{key}</c>.</summary>
    public Uri GuestOrder(string accessKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessKey);
        return Absolute("/siparis/erisim/" + Uri.EscapeDataString(accessKey));
    }

    /// <summary>The source copy of the logo (08 2.2.5 item 2): a path under <c>/gorsel/</c> (07 2.1.2), made absolute with the domain setting.</summary>
    public Uri Logo(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (!path.StartsWith(ImagePrefix, StringComparison.Ordinal))
        {
            throw new ArgumentException("A logo address is a path under /gorsel/.", nameof(path));
        }

        return Absolute(path);
    }

    /// <summary>
    /// A screen of the panel (07 2.1.5; 08 2.2.7): <c>/panel/{screen}</c> or, with an inner key, <c>/panel/{screen}/{key}</c>. The screen is a fixed name the
    /// template writes (lower case letters and hyphens); the key is percent-encoded. A visitor without a session lands on the sign-in page and, after it, on
    /// this screen (07 2.3.2).
    /// </summary>
    public Uri Panel(string screen, string? key = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(screen);
        if (!screen.All(c => char.IsAsciiLetterLower(c) || c == '-'))
        {
            throw new ArgumentException("A panel screen is a name of lower case letters and hyphens (07 2.1.5).", nameof(screen));
        }

        return Absolute(key is null ? "/panel/" + screen : "/panel/" + screen + "/" + Uri.EscapeDataString(key));
    }

    private Uri Absolute(string path) => new(_site.Absolute(path), UriKind.Absolute);
}
