using System.Diagnostics.CodeAnalysis;

namespace Shopfolio.Infrastructure.Notices;

/// <summary>
/// The text of a short-lived notice (07 6.3.1), written once: the notice code travels in the address (<see cref="NoticeParameter"/>) and the page that reads it draws the text here. The
/// codes whose text the table gives as a quoted sentence are here, and the two contact request codes, which name a fact ("the request was closed"), with the sentences of the screen that shows them (T532).
/// A code the address carries that has no sentence here shows nothing - the address is the request's and never decides a failure.
/// </summary>
internal static class NoticeTexts
{
    /// <summary>07 6.3.1: "Kaydedildi" - a save whose result is already on the screen (UC-35, UC-64 ... UC-173).</summary>
    public const string Saved = "Kaydedildi";

    /// <summary>07 6.3.1: "Sepete eklendi".</summary>
    public const string CartItemAdded = "Sepete eklendi";

    /// <summary>07 6.3.1: "Davet gönderildi".</summary>
    public const string InvitationSent = "Davet gönderildi";

    /// <summary>The notice after a contact request is closed (T532; UC-155; 04 9.11.8).</summary>
    public const string ContactRequestClosed = "Talep kapatıldı";

    /// <summary>The notice after a closed contact request is reopened (T532; UC-156; 04 9.11.8).</summary>
    public const string ContactRequestReopened = "Talep yeniden açıldı";

    /// <summary>The text of a short-lived notice; false for a code that has no sentence in the table.</summary>
    public static bool TryShortLived(NoticeCode code, [NotNullWhen(true)] out string? text)
    {
        text = code switch
        {
            NoticeCode.Saved => Saved,
            NoticeCode.CartItemAdded => CartItemAdded,
            NoticeCode.InvitationSent => InvitationSent,
            NoticeCode.ContactRequestClosed => ContactRequestClosed,
            NoticeCode.ContactRequestReopened => ContactRequestReopened,
            _ => null,
        };
        return text is not null;
    }
}
