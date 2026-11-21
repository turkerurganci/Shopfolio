using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;

using Microsoft.AspNetCore.Html;

namespace Shopfolio.Infrastructure.Security;

/// <summary>Which attribute names a head meta element: <c>name</c> (description, robots) or <c>property</c> (Open Graph).</summary>
internal enum HeadMetaKind
{
    Name,
    Property,
}

/// <summary>One meta element of the head; the name and the content are encoded when the head is written.</summary>
internal sealed record HeadMeta(HeadMetaKind Kind, string Name, string Content);

/// <summary>
/// The one raw output besides <see cref="RichText.Render"/> (09 15.3 items 1, 7; 05 3.7.2; D-357): the search engine head - the title, the meta elements and the JSON-LD
/// blocks. Nothing raw comes in: the title and every meta value are HTML-encoded here, and a block's JSON is serialized here (<see cref="Serialize"/>) from a JSON tree with an
/// encoder that escapes <c>&lt;</c>, <c>&gt;</c>, <c>&amp;</c>, <c>'</c> and <c>+</c>, so a value that holds <c>&lt;/script&gt;</c> cannot leave its block. The Latin ranges
/// of the Turkish alphabet stay as they are. <c>HtmlString</c> is built here and in <c>RichText.cs</c> only (the source scan).
/// </summary>
internal static class StructuredData
{
    private static readonly JsonSerializerOptions _json = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement, UnicodeRanges.LatinExtendedA),
        WriteIndented = false,
    };

    private static readonly HtmlEncoder _html = HtmlEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement, UnicodeRanges.LatinExtendedA);

    /// <summary>The JSON of a block, from its tree; the only way a block's text is made.</summary>
    public static string Serialize(JsonObject root)
    {
        ArgumentNullException.ThrowIfNull(root);
        return root.ToJsonString(_json);
    }

    /// <summary>The head's elements in this order: the title, the meta elements, the JSON-LD blocks (each block's text from <see cref="Serialize"/>).</summary>
    public static IHtmlContent Render(string title, IReadOnlyList<HeadMeta> metas, IReadOnlyList<JsonObject> blocks)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(metas);
        ArgumentNullException.ThrowIfNull(blocks);
        StringBuilder head = new();
        head.Append("<title>").Append(_html.Encode(title)).Append("</title>\n");
        foreach (HeadMeta meta in metas)
        {
            string attribute = meta.Kind switch
            {
                HeadMetaKind.Name => "name",
                HeadMetaKind.Property => "property",
                _ => throw new ArgumentOutOfRangeException(nameof(metas), meta.Kind, "A meta element is named by name or property."),
            };
            head.Append("<meta ").Append(attribute).Append("=\"").Append(_html.Encode(meta.Name)).Append("\" content=\"").Append(_html.Encode(meta.Content)).Append("\">\n");
        }

        foreach (JsonObject block in blocks)
        {
            head.Append("<script type=\"application/ld+json\">").Append(Serialize(block)).Append("</script>\n");
        }

        return new HtmlString(head.ToString());
    }
}
