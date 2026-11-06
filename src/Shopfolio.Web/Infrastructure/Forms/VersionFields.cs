using System.Globalization;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Shopfolio.Infrastructure.Forms;

/// <summary>
/// The hidden fields a form carries (07 2.4): the version the record had when the form was opened, and the value the screen showed for the
/// fields that are written against the shown value. Both are fields of the POST body - never part of the address and never a header (07 2.4.7:
/// a bookmark or a refresh would carry an old version, and the form sends no conditional header).
/// <list type="bullet">
/// <item>The version carries the column's name from 06 4: <c>Version</c>; a company-settings section carries its own column (<c>PaymentMethodsVersion</c>,
/// 06 2.6.3, 07 2.4.1).</item>
/// <item>A value written against the shown value carries the field's name behind the <c>Original</c> prefix (07 2.4.3): <c>OriginalStockQuantity</c>,
/// <c>OriginalText</c>, <c>OriginalOpenQuantity</c>. It never enters the record's version (06 2.6.5).</item>
/// </list>
/// The helpers write the value they are given and nothing else, so a re-displayed form carries the value its handler chose - the record's
/// current version after a conflict (07 2.4.5), the first form's shown value in the last step (07 2.4.3) - and never the value the framework would
/// restore from the posted model state.
/// </summary>
internal static class VersionFields
{
    /// <summary>The name of the version column of a record with one version (06 2.6.1, 4).</summary>
    public const string RecordVersion = "Version";

    /// <summary>The prefix of the field that carries a shown value (07 2.4.3).</summary>
    public const string OriginalPrefix = "Original";

    /// <summary>The field of a customer's item operation that carries the open quantity the screen showed, per item (07 2.4.2, 2.4.3; K-1202).</summary>
    public const string OpenQuantityField = OriginalPrefix + "OpenQuantity";

    private const string VersionSuffix = "Version";

    /// <summary>The hidden version field of a record with one version, named <c>Version</c> (07 2.4.1); <paramref name="prefix"/> is the model path of the input, when it has one.</summary>
    public static IHtmlContent Version(int version, string prefix = "") => Hidden(Join(prefix, RecordVersion), version.ToString(CultureInfo.InvariantCulture));

    /// <summary>The hidden version field of a section, named after the section's version column (<c>PaymentMethodsVersion</c>; 06 2.6.3, 07 2.4.1).</summary>
    public static IHtmlContent SectionVersion(string column, int version, string prefix = "")
    {
        ArgumentException.ThrowIfNullOrEmpty(column);
        if (!IsVersionColumn(column))
        {
            throw new ArgumentException("The version column of a section ends with Version (06 2.6.3).", nameof(column));
        }

        return Hidden(Join(prefix, column), version.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>The hidden field of a shown value, named <c>Original</c> + the field's name (07 2.4.3). An empty value (an unlimited stock) is sent as an empty field.</summary>
    public static IHtmlContent Original(string field, string? shown, string prefix = "")
    {
        ArgumentException.ThrowIfNullOrEmpty(field);
        return Hidden(Join(prefix, OriginalName(field)), shown ?? string.Empty);
    }

    /// <summary>The hidden field of a number shown on the screen; <see langword="null"/> is the empty value.</summary>
    public static IHtmlContent Original(string field, int? shown, string prefix = "") =>
        Original(field, shown?.ToString(CultureInfo.InvariantCulture), prefix);

    /// <summary>The name of the field that carries the shown value of <paramref name="field"/>.</summary>
    public static string OriginalName(string field)
    {
        ArgumentException.ThrowIfNullOrEmpty(field);
        return OriginalPrefix + field;
    }

    /// <summary>True for a name that carries a shown value: the <c>Original</c> prefix and a field's name after it.</summary>
    public static bool IsOriginalName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return name.Length > OriginalPrefix.Length && name.StartsWith(OriginalPrefix, StringComparison.Ordinal);
    }

    /// <summary>True for the name of a version column: <c>Version</c> or a section's <c>...Version</c>.</summary>
    public static bool IsVersionColumn(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return name.EndsWith(VersionSuffix, StringComparison.Ordinal);
    }

    /// <summary>Renders a helper's output as text (for a test or a log line; the page renders the content itself).</summary>
    public static string Render(IHtmlContent content)
    {
        ArgumentNullException.ThrowIfNull(content);
        using StringWriter writer = new(CultureInfo.InvariantCulture);
        content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    private static string Join(string prefix, string name) => string.IsNullOrEmpty(prefix) ? name : $"{prefix}.{name}";

    private static TagBuilder Hidden(string name, string value)
    {
        TagBuilder input = new("input");
        input.Attributes["type"] = "hidden";
        input.Attributes["name"] = name;
        input.Attributes["value"] = value;
        input.TagRenderMode = TagRenderMode.SelfClosing;
        return input;
    }
}
