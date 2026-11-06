using System.Globalization;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.Infrastructure.Forms;

/// <summary>
/// The confirmation key of the payment step (07 2.5.1; 05 4.5.1; 06 4.17): the only form whose second submission is decided by a value of its own
/// and not by the record's present state. Every time the step is shown the form carries a fresh key in the hidden field <see cref="FieldName"/>; the
/// key is unique on the order row, so a second submission with the same key - a double click, the back button, a resent form - opens no second
/// order and goes to the confirmation screen of the existing one by 303 (<see cref="MemberConfirmationPath"/>, <see cref="AccessConfirmationPath"/>:
/// the member's number root or the guest's access-key root, 07 2.1.3). Two tabs show two different keys. The order's creation, the lookup by key
/// and the uniqueness belong to the Ordering module's task; this class is what the form and the redirect are made of.
/// </summary>
internal static class ConfirmationKeys
{
    /// <summary>The name of the hidden field and of the input property (06 4.17: the column's name).</summary>
    public const string FieldName = "ConfirmationKey";

    /// <summary>The screen of the order page a repeated submission is sent to (07 2.1.3).</summary>
    public const string ConfirmationScreen = "teyit";

    /// <summary>A new key for a shown step: random, URL-safe, never derived from the cart or the account.</summary>
    public static string NewKey() => SecretKey.Create();

    /// <summary>The hidden field of the payment step's form.</summary>
    public static IHtmlContent Hidden(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        TagBuilder input = new("input");
        input.Attributes["type"] = "hidden";
        input.Attributes["name"] = FieldName;
        input.Attributes["value"] = key;
        input.TagRenderMode = TagRenderMode.SelfClosing;
        return input;
    }

    /// <summary>The confirmation screen of a member's order: <c>/siparis/{number}/teyit</c>.</summary>
    public static string MemberConfirmationPath(string orderNumber)
    {
        ArgumentException.ThrowIfNullOrEmpty(orderNumber);
        return $"/siparis/{Uri.EscapeDataString(orderNumber)}/{ConfirmationScreen}";
    }

    /// <summary>The confirmation screen of a guest's order: <c>/siparis/erisim/{access key}/teyit</c>.</summary>
    public static string AccessConfirmationPath(string accessKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(accessKey);
        return $"/siparis/{OrderRoutes.AccessSegment}/{Uri.EscapeDataString(accessKey)}/{ConfirmationScreen}";
    }

    /// <summary>Renders the field as text (a test or a log line).</summary>
    public static string Render(IHtmlContent content)
    {
        ArgumentNullException.ThrowIfNull(content);
        using StringWriter writer = new(CultureInfo.InvariantCulture);
        content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }
}
