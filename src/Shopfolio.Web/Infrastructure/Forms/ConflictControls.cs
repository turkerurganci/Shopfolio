using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Shopfolio.Infrastructure.Forms;

/// <summary>
/// The controls of a form that was saved against a changed record (07 2.4.5, 2.4.6; 04 2.10; K-800, K-799). The record variant shows, in the place of
/// the save button, a message that stays (it closes by nothing) and two ways: "Güncel hâli aç" - a link that opens the record's own edit page in a new tab, so
/// what the user typed stays on this page - and "Yine de kaydet" - the form's one submit button, which sends the same form with the record's
/// current version. The order variant shows only a message: the operation is not applied, the order's current state is shown below it, and there is no
/// way to overwrite (05 7.3.2; 02 3.31.2). The text of the messages is the product's own (K-800, K-799) and stays here, in one place.
/// </summary>
internal static class ConflictControls
{
    /// <summary>The label of the ordinary save button.</summary>
    public const string SaveLabel = "Kaydet";

    /// <summary>The label of the one submit button of a re-displayed form (07 2.4.5; K-800).</summary>
    public const string OverwriteLabel = "Yine de kaydet";

    /// <summary>The label of the link that opens the record's current state in a new tab (07 2.4.5; K-800).</summary>
    public const string OpenCurrentLabel = "Güncel hâli aç";

    /// <summary>The message of the record variant (04 2.10.1; K-800).</summary>
    public const string RecordMessage = "Bu kayıt siz düzenlerken başka bir yöneticice kaydedildi. Yine de kaydederseniz onun değişiklikleri sizinkilerle değişir.";

    /// <summary>The message of the order variant (04 2.10.2; K-799).</summary>
    public const string OrderMessage = "Sipariş siz bakarken değişti; işlem uygulanmadı. Siparişin güncel hâli aşağıda.";

    /// <summary>The message of the order variant when the customer changed the refund IBAN (04 2.10.2; K-799).</summary>
    public const string OrderIbanMessage = "Müşteri IBAN'ını değiştirdi; geri ödeme işlenmedi. Güncel IBAN aşağıda.";

    /// <summary>
    /// The save control of a record's form. Without a conflict it is the ordinary button. With one it is the message, the link to the record's
    /// current state (<paramref name="currentPath"/>, the path of the record's own GET page) and the one submit button "Yine de kaydet" - the form
    /// the control sits in carries the record's current version, so the button's send is a save like any other.
    /// </summary>
    public static IHtmlContent RecordSaveControl(bool conflicted, string currentPath)
    {
        ArgumentNullException.ThrowIfNull(currentPath);
        if (!conflicted)
        {
            return Button(SaveLabel);
        }

        if (!FormResults.IsLocalPath(currentPath))
        {
            throw new ArgumentException("The link to the current state is a path of this site that starts with one slash (05 6.5.6).", nameof(currentPath));
        }

        TagBuilder notice = new("p");
        notice.AddCssClass("conflict-notice");
        notice.Attributes["role"] = "alert";
        notice.InnerHtml.Append(RecordMessage);

        // The link opens the current state in a new tab: what was typed stays on this page (04 2.10.1; 07 2.4.5, 2.4.7).
        TagBuilder open = new("a");
        open.Attributes["href"] = currentPath;
        open.Attributes["target"] = "_blank";
        open.Attributes["rel"] = "noopener noreferrer";
        open.InnerHtml.Append(OpenCurrentLabel);

        HtmlContentBuilder builder = new();
        builder.AppendHtml(notice);
        builder.AppendHtml(open);
        builder.AppendHtml(Button(OverwriteLabel));
        return builder;
    }

    /// <summary>The message of the order variant, in the place the confirmation window or the operation's button was; there is no button to overwrite.</summary>
    public static IHtmlContent OrderNotice(string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        TagBuilder notice = new("p");
        notice.AddCssClass("conflict-notice");
        notice.Attributes["role"] = "alert";
        notice.InnerHtml.Append(message);
        return notice;
    }

    private static TagBuilder Button(string label)
    {
        TagBuilder button = new("button");
        button.Attributes["type"] = "submit";
        button.InnerHtml.Append(label);
        return button;
    }
}
