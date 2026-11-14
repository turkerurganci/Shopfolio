using Shopfolio.Modules.Membership.Contracts.Results;

namespace Shopfolio.Pages.Components;

/// <summary>What the revert link's landing page tells for one state (T519; 04 5.28): one sentence and, only in the revert step, the one button.</summary>
/// <param name="Sentence">The one sentence.</param>
/// <param name="Meaning">The message's meaning.</param>
/// <param name="ButtonLabel">The revert step's one button; null in every other state - the next step is in the e-mail (04 5.28.2).</param>
public sealed record EmailRevertLanding(string Sentence, MessageMeaning Meaning, string? ButtonLabel);

/// <summary>
/// The texts of the revert link's landing page (T519; 04 5.28; 07 UC-75, UC-76): the four states of the screen. Forms and account screens speak "siz" (04 8.2.9). The screen has no password field
/// (the password is set through the reset link of the e-mail, 04 5.28.5) and no question beside the one button (K-839). Written here once; the page and its tests read them from here.
/// </summary>
public static class EmailRevertLandings
{
    /// <summary>The screen's name: its heading and, with the brand name, its tab title.</summary>
    public const string ScreenName = "E-posta değişikliği";

    /// <summary>04 5.28.6: the one button, named for the operation (K-839).</summary>
    public const string ButtonLabel = "E-posta değişikliğini geri al";

    /// <summary>04 5.28.3: the revert step - what the button does, in one sentence (K-839; 02 3.13.14).</summary>
    public const string RevertStep =
        "Geri alırsanız hesabın e-postası bu adrese döner, hesabın bütün oturumları kapanır, değişiklik süresince bağlanan Google girişi kaldırılır ve hesabın şifresi varsa geçersiz olur; şifresi varsa bu adrese bir şifre sıfırlama bağlantısı gönderilir. Değişiklik süresince hesaba bağlanan siparişler hesapta kalır.";

    /// <summary>04 5.28.3: the result after the button - on the address of a used link that carries the result code (K-1186 (1)).</summary>
    public const string Reverted =
        "E-posta değişikliği geri alındı. Hesabın e-postası bu adrese döndü ve bütün oturumları kapandı. Değişiklik süresince bağlanan Google girişi kaldırıldı; hesabın şifresi varsa geçersiz oldu ve bu adrese bir şifre sıfırlama bağlantısı gönderildi, yeni şifre o bağlantıyla kurulur. Değişiklik süresince hesaba bağlanan siparişler hesapta kaldı.";

    /// <summary>04 5.28.3, 6.2.28.3: the earlier address belongs to another verified account now; the revert is not applied and the consumption is taken back (K-1210).</summary>
    public const string NotApplied =
        "Geri alma uygulanmadı: bu e-posta adresi artık başka bir hesaba ait. Firmaya İletişim sayfasından ulaşabilirsiniz.";

    /// <summary>04 5.28.4: a link that ran out, was replaced or is unknown - no way back inside the product; the orders open from their e-mails; the firm is reached through Contact.</summary>
    public const string Invalid =
        "Bu geri alma bağlantısı geçerli değil; ürünün içinden e-posta değişikliğini geri alma yolu kalmadı. Siparişlerinize sipariş e-postalarındaki bağlantıyla girmeye devam edebilirsiniz. Firmaya İletişim sayfasından ulaşabilirsiniz.";

    /// <summary>04 5.28.4: an earlier-used link without the result code tells that the revert was done.</summary>
    public const string UsedEarlier =
        "Bu geri alma bağlantısı daha önce kullanıldı ve e-posta değişikliği geri alındı; ürünün içinden başka geri alma yolu kalmadı. Siparişlerinize sipariş e-postalarındaki bağlantıyla girmeye devam edebilirsiniz. Firmaya İletişim sayfasından ulaşabilirsiniz.";

    /// <summary>
    /// The landing for what the link reads now. <paramref name="done"/> is the result code of the button that was just pressed (the notice of the 303) and counts only on a link that is now used - the
    /// code alone, on the address of a valid or an unknown link, tells nothing (K-1186 (1)). <paramref name="notApplied"/> is the 422 of the button.
    /// </summary>
    public static EmailRevertLanding Of(VerificationLinkStatus status, bool done, bool notApplied)
    {
        if (notApplied)
        {
            return new(NotApplied, MessageMeaning.Warning, null);
        }

        return status switch
        {
            VerificationLinkStatus.Valid => new(RevertStep, MessageMeaning.Info, ButtonLabel),
            VerificationLinkStatus.Used when done => new(Reverted, MessageMeaning.Success, null),
            VerificationLinkStatus.Used => new(UsedEarlier, MessageMeaning.Warning, null),
            _ => new(Invalid, MessageMeaning.Warning, null),
        };
    }
}
