using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;

using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.InstallationSettings;

namespace Shopfolio.Infrastructure.SearchEngine;

/// <summary>
/// The absolute address of a path of the site (05 3.7.2: a structured data address and a preview image are absolute). The host is the installation's domain
/// setting (<see cref="SiteOptions.Domain"/>), never the request's <c>Host</c> header, which a visitor writes; the scheme is <c>https</c>, as the reverse
/// proxy ends the TLS (05 6.3.3).
/// </summary>
internal sealed class SiteAddress
{
    private readonly string _origin;

    public SiteAddress(IOptions<SiteOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _origin = "https://" + options.Value.Domain;
    }

    /// <summary><paramref name="path"/> must be a path of the site: one leading <c>/</c>, no second one, no backslash, no scheme or host.</summary>
    public string Absolute(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (path.Length == 0 || path[0] != '/' || path.StartsWith("//", StringComparison.Ordinal) || path.Contains('\\', StringComparison.Ordinal))
        {
            throw new ArgumentException("An address of the site is a path that starts with a single slash.", nameof(path));
        }

        return _origin + path;
    }
}

/// <summary>
/// The search engine data of a page (05 3.7.2, MK-18; 02 3.30.5): the structured data blocks (JSON-LD, schema.org: product, firm, breadcrumb trail, FAQ) and the
/// head of the page - title, description, Open Graph preview and the closing <c>noindex</c> tag. Every value is handed in from the record; the code has no
/// field a firm could fill. One instance serves the application.
/// </summary>
internal sealed class SearchEngineData
{
    private const string SchemaContext = "https://schema.org";

    // The Latin ranges of the Turkish alphabet stay as they are; the characters that matter inside a script element (< > & ' +) are escaped by every
    // JavaScriptEncoder.Create encoder, so a name that holds "</script>" cannot leave the block.
    private static readonly JsonSerializerOptions _json = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement, UnicodeRanges.LatinExtendedA),
        WriteIndented = false,
    };

    private static readonly HtmlEncoder _html = HtmlEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement, UnicodeRanges.LatinExtendedA);

    private readonly SiteAddress _site;

    public SearchEngineData(SiteAddress site)
    {
        ArgumentNullException.ThrowIfNull(site);
        _site = site;
    }

    /// <summary>The product block: name, description, images, code, and the offer with the price in lira and the stock status (05 3.7.2).</summary>
    public JsonLdBlock Product(ProductData product)
    {
        ArgumentNullException.ThrowIfNull(product);
        JsonObject offer = new()
        {
            ["@type"] = "Offer",
            ["url"] = _site.Absolute(product.Path),
            ["price"] = JsonValue.Create(product.Price),
            ["priceCurrency"] = "TRY",
            ["availability"] = product.Availability == StockStatus.InStock ? SchemaContext + "/InStock" : SchemaContext + "/OutOfStock",
        };
        JsonObject root = Root("Product");
        root["name"] = product.Name;
        if (!string.IsNullOrWhiteSpace(product.Description))
        {
            root["description"] = product.Description;
        }

        if (product.ImagePaths.Count > 0)
        {
            root["image"] = new JsonArray([.. product.ImagePaths.Select(p => (JsonNode?)_site.Absolute(p))]);
        }

        if (!string.IsNullOrWhiteSpace(product.Sku))
        {
            root["sku"] = product.Sku;
        }

        root["offers"] = offer;
        return Block("Product", root);
    }

    /// <summary>The firm block: the brand name, the site's address, the logo, the telephone and e-mail when the record has them, and the social links.</summary>
    public JsonLdBlock Organization(OrganizationData firm)
    {
        ArgumentNullException.ThrowIfNull(firm);
        JsonObject root = Root("Organization");
        root["name"] = firm.Name;
        root["url"] = _site.Absolute("/");
        if (!string.IsNullOrWhiteSpace(firm.LogoPath))
        {
            root["logo"] = _site.Absolute(firm.LogoPath);
        }

        if (!string.IsNullOrWhiteSpace(firm.Telephone))
        {
            root["telephone"] = firm.Telephone;
        }

        if (!string.IsNullOrWhiteSpace(firm.Email))
        {
            root["email"] = firm.Email;
        }

        string[] links = [.. firm.SameAs.Where(IsWebAddress)];
        if (links.Length > 0)
        {
            root["sameAs"] = new JsonArray([.. links.Select(l => (JsonNode?)l)]);
        }

        return Block("Organization", root);
    }

    /// <summary>The breadcrumb trail (02 3.4.4): the steps in order, each with its position, name and address. <see langword="null"/> when there is no step.</summary>
    public JsonLdBlock? Breadcrumbs(IReadOnlyList<BreadcrumbItem> steps)
    {
        ArgumentNullException.ThrowIfNull(steps);
        if (steps.Count == 0)
        {
            return null;
        }

        JsonArray items = [];
        for (int i = 0; i < steps.Count; i++)
        {
            items.Add(new JsonObject
            {
                ["@type"] = "ListItem",
                ["position"] = i + 1,
                ["name"] = steps[i].Name,
                ["item"] = _site.Absolute(steps[i].Path),
            });
        }

        JsonObject root = Root("BreadcrumbList");
        root["itemListElement"] = items;
        return Block("BreadcrumbList", root);
    }

    /// <summary>The FAQ block: every question with its accepted answer in plain text. <see langword="null"/> when the page has no question.</summary>
    public static JsonLdBlock? Faq(IReadOnlyList<FaqEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        if (entries.Count == 0)
        {
            return null;
        }

        JsonArray questions = [];
        foreach (FaqEntry entry in entries)
        {
            questions.Add(new JsonObject
            {
                ["@type"] = "Question",
                ["name"] = entry.Question,
                ["acceptedAnswer"] = new JsonObject { ["@type"] = "Answer", ["text"] = PlainText.FromHtml(entry.AnswerHtml) },
            });
        }

        JsonObject root = Root("FAQPage");
        root["mainEntity"] = questions;
        return Block("FAQPage", root);
    }

    /// <summary>
    /// The head elements of a page (05 3.7.2, 3.7.4). An open page: the title, the description, the Open Graph preview - same title and description, the page's
    /// main image and, when it has none, the logo - and the JSON-LD blocks. A closed page - one the data says is closed, or one the request's address closes
    /// (<see cref="SearchEngineClosure.IsClosed"/>: the panel, the cart, the account ...) - carries its title and the robots meta tag <c>noindex</c> and nothing
    /// else for the search engines to read.
    /// </summary>
    public IHtmlContent Head(HttpContext context, PageSearchData data)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(data);
        StringBuilder head = new();
        head.Append("<title>").Append(_html.Encode(data.Title)).Append("</title>\n");
        if (data.Closed || SearchEngineClosure.IsClosed(context))
        {
            Meta(head, "name", "robots", SearchEngineClosure.Directive);
            return new HtmlString(head.ToString());
        }

        if (!string.IsNullOrWhiteSpace(data.Description))
        {
            Meta(head, "name", "description", data.Description);
        }

        Meta(head, "property", "og:type", data.OpenGraphType);
        if (!string.IsNullOrWhiteSpace(data.BrandName))
        {
            Meta(head, "property", "og:site_name", data.BrandName);
        }

        Meta(head, "property", "og:locale", "tr_TR");
        Meta(head, "property", "og:title", data.Title);
        if (!string.IsNullOrWhiteSpace(data.Description))
        {
            Meta(head, "property", "og:description", data.Description);
        }

        Meta(head, "property", "og:url", _site.Absolute(data.Path));
        string? image = data.ImagePath ?? data.LogoPath;
        if (!string.IsNullOrWhiteSpace(image))
        {
            Meta(head, "property", "og:image", _site.Absolute(image));
        }

        foreach (JsonLdBlock block in data.StructuredData)
        {
            head.Append("<script type=\"application/ld+json\">").Append(block.Json).Append("</script>\n");
        }

        return new HtmlString(head.ToString());
    }

    private static void Meta(StringBuilder head, string attribute, string name, string content)
    {
        head.Append("<meta ").Append(attribute).Append("=\"").Append(_html.Encode(name)).Append("\" content=\"").Append(_html.Encode(content)).Append("\">\n");
    }

    private static JsonObject Root(string type) => new() { ["@context"] = SchemaContext, ["@type"] = type };

    private static JsonLdBlock Block(string type, JsonObject root) => new(type, root.ToJsonString(_json));

    private static bool IsWebAddress(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}
