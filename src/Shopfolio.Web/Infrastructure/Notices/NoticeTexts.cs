using System.Diagnostics.CodeAnalysis;

namespace Shopfolio.Infrastructure.Notices;

/// <summary>
/// The text of a short-lived notice (07 6.3.1), written once: the notice code travels in the address (<see cref="NoticeParameter"/>) and the page that reads it draws the text here. Only the
/// codes whose text the table gives as a quoted sentence are here; the two contact request codes name a fact ("the request was closed") and their text is the task's that builds the screen.
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

    /// <summary>The text of a short-lived notice; false for a code that has no sentence in the table.</summary>
    public static bool TryShortLived(NoticeCode code, [NotNullWhen(true)] out string? text)
    {
        text = code switch
        {
            NoticeCode.Saved => Saved,
            NoticeCode.CartItemAdded => CartItemAdded,
            NoticeCode.InvitationSent => InvitationSent,
            _ => null,
        };
        return text is not null;
    }
}
