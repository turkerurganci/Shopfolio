using System.Text;
using System.Xml;

using Shopfolio.Infrastructure.Routing;

namespace Shopfolio.Infrastructure.SearchEngine;

/// <summary>
/// The contents of the two files the search engines read (T313; 07 4.32.1; 05 3.7.3, 3.7.4; 02 3.30.5): the XML site map and <c>robots.txt</c>. The addresses come
/// from the modules as paths of published records; this code adds the fixed list pages, drops what is closed to the search engines, makes every address one
/// lower-case form and writes the absolute addresses with the installation's domain (<see cref="SiteAddress"/>, never the request's host). It reads no table and
/// reaches no module type. <c>robots.txt</c> names the site map and nothing else: it blocks no page, because a blocked page cannot be read and its
/// <c>noindex</c> would not be seen (05 3.7.4).
/// </summary>
internal static class SiteMapFiles
{
    /// <summary>
    /// The fixed pages of the list kind (07 2.1.2) that always answer: the home page, the services list, the references list, the FAQ, the contact page and the
    /// guide to the operations. The about page and the records' pages are published or not, so their modules hand them in; the legal texts and the cart,
    /// the payment step, the order tracking, the search and the account are not here (the legal texts answer only once published; the others are closed).
    /// </summary>
    public static readonly IReadOnlyList<string> FixedListPaths = ["/", "/hizmetler", "/referanslar", "/sss", "/iletisim", "/islem-rehberi"];

    private const string SiteMapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";

    /// <summary>
    /// The paths of the site map: the fixed list pages first, then the paths the modules handed in, each in one lower-case form without a trailing slash, once, in
    /// path order. A path that is closed to the search engines (<see cref="SearchEngineClosure.IsClosedPath"/>) or is not a path of the site is left out.
    /// </summary>
    public static IReadOnlyList<string> Compose(IEnumerable<string> catalogPaths, IEnumerable<string> contentPaths)
    {
        ArgumentNullException.ThrowIfNull(catalogPaths);
        ArgumentNullException.ThrowIfNull(contentPaths);

        HashSet<string> seen = new(FixedListPaths, StringComparer.Ordinal);
        List<string> records = [];
        foreach (string path in catalogPaths.Concat(contentPaths))
        {
            string? one = OneForm(path);
            if (one is not null && !SearchEngineClosure.IsClosedPath(one) && seen.Add(one))
            {
                records.Add(one);
            }
        }

        records.Sort(StringComparer.Ordinal);
        return [.. FixedListPaths, .. records];
    }

    /// <summary>The XML site map (07 4.32.1): one <c>url</c> with its absolute <c>loc</c> per path, UTF-8.</summary>
    public static string Xml(SiteAddress site, IEnumerable<string> paths)
    {
        ArgumentNullException.ThrowIfNull(site);
        ArgumentNullException.ThrowIfNull(paths);

        StringBuilder text = new("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
        XmlWriterSettings settings = new() { OmitXmlDeclaration = true, Indent = true, IndentChars = " ", NewLineChars = "\n", Encoding = new UTF8Encoding(false) };
        using (XmlWriter writer = XmlWriter.Create(text, settings))
        {
            writer.WriteStartElement("urlset", SiteMapNamespace);
            foreach (string path in paths)
            {
                writer.WriteStartElement("url");
                writer.WriteElementString("loc", site.Absolute(path));
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        return text.ToString();
    }

    /// <summary><c>robots.txt</c> (07 4.32.1): the address of the site map, with the installation's domain, on one line; no rule that blocks a page.</summary>
    public static string Robots(SiteAddress site)
    {
        ArgumentNullException.ThrowIfNull(site);
        return "Sitemap: " + site.Absolute("/" + AddressRules.SitemapFile) + "\n";
    }

    // The one form (07 2.1.7): lower case, no trailing slash (the root keeps its one), one leading slash; a query, a fragment or a second slash is not a page address.
    private static string? OneForm(string? path)
    {
        if (string.IsNullOrEmpty(path) || path[0] != '/' || path.AsSpan().ContainsAny("?#\\ "))
        {
            return null;
        }

        string lower = path.ToLowerInvariant();
        string trimmed = lower.Length > 1 ? lower.TrimEnd('/') : lower;
        return trimmed.Length == 0 || trimmed.StartsWith("//", StringComparison.Ordinal) ? null : trimmed;
    }
}
