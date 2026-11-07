using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// The templates of the seven account and administration e-mails (08 2.2.8; T309): a function from the dispatch row's type, related record type and event values to
/// the e-mail's <see cref="EmailContent"/>, which <c>EmailComposer</c> turns into the message. The common rules hold for all seven: the subject is "{brand}: {title}"
/// with no order suffix (the order number never enters <see cref="EmailContent.OrderNumber"/>), the greeting is "Merhaba," and carries no name, the key of a link is
/// derived from the link record's id here and from nowhere else (05 6.4.4; K-1081), the same text serves a customer and an administrator account and only the path
/// differs (<c>/…</c> or <c>/panel/…</c>, read from the row's related record type), the life of a link is written with its value from the product constants (02 11.2)
/// and is always the whole life - the body counts nothing from the moment it is made, so a retried e-mail says the same (08 2.2.8 rule 6) - and no content carries an
/// <see cref="EmailResend"/> (there is no re-send of these e-mails, 02 9.1.6; the user asks for the link again and that is a new row). The confirmation-screen
/// sentence stands in the four bodies whose link opens a screen with a button - verification, new address, change notice and Google confirmation - and in no other.
/// A row whose values cannot be read, or whose type does not fit its related record, gives null; the sender records that as a recipient error without a call
/// (08 2.2 call conditions (5)). Whether the link is still valid is not decided here but by <see cref="AccountEmailBodies"/>, which reads the link record first.
/// </summary>
internal static class AccountEmailTemplates
{
    public const string ConfirmationScreenSentence = "Bağlantı sizi onay ekranına götürür; işlem ekrandaki düğmeyle tamamlanır.";

    private const string Greeting = "Merhaba,";

    private const string VerifyScreen = "dogrulama";
    private const string ResetScreen = "sifre-sifirlama";
    private const string RevertScreen = "geri-al";
    private const string InvitationScreen = "davet";

    /// <summary>
    /// The link record the row's values name; null when they name none or cannot be read. A type that carries no link (the deletion notice) names none, and
    /// <see cref="AccountEmailBodies"/> does not ask for one.
    /// </summary>
    public static Guid? LinkRecordIdOf(string? eventValues) => LinkEventValues.ReadLinkRecordId(eventValues);

    /// <summary>True for the types that carry a link (every type but the deletion notice).</summary>
    public static bool CarriesLink(DispatchType type) => type is not DispatchType.AccountDeletedNotice;

    /// <summary>The content of the e-mail a row of <paramref name="type"/> carries; null for a type that is not an account e-mail and for values that cannot be read.</summary>
    public static EmailContent? Compose(DispatchType type, DispatchRelatedRecordType? relatedRecordType, string? eventValues, string brand, EmailLinks links, LinkKeys keys)
    {
        ArgumentNullException.ThrowIfNull(links);
        ArgumentNullException.ThrowIfNull(keys);
        ArgumentException.ThrowIfNullOrWhiteSpace(brand);
        IReadOnlyDictionary<string, string>? values = CompanyNoticeValues.Read(eventValues);
        if (values is null || relatedRecordType is null)
        {
            return null;
        }

        bool isCustomer = relatedRecordType == DispatchRelatedRecordType.Customer;
        bool isAdmin = relatedRecordType == DispatchRelatedRecordType.Admin;
        Guid? linkId = LinkEventValues.ReadLinkRecordId(eventValues);

        return type switch
        {
            DispatchType.EmailVerificationLink when isCustomer && linkId is { } id => Verification(brand, links, keys, id),
            DispatchType.PasswordResetLink when (isCustomer || isAdmin) && linkId is { } id => PasswordReset(values, links, keys, id, isAdmin),
            DispatchType.NewEmailVerificationLink when (isCustomer || isAdmin) && linkId is { } id => NewAddress(links, keys, id, isAdmin),
            DispatchType.EmailChangeNotice when (isCustomer || isAdmin) && linkId is { } id => ChangeNotice(values, links, keys, id, isAdmin),
            DispatchType.AdminInvitation when relatedRecordType == DispatchRelatedRecordType.AdminInvitation && linkId is { } id => Invitation(values, brand, links, keys, id),
            DispatchType.AccountDeletedNotice when isCustomer => Deleted(values),
            DispatchType.GoogleLinkConfirmationLink when isCustomer && linkId is { } id => GoogleConfirmation(links, keys, id),
            _ => null,
        };
    }

    // "{marka adı} sitesine bu e-posta adresiyle kaydoldunuz. Hesabınızı etkinleştirmek için adresinizi doğrulayın." · "Adresimi doğrula" · the screen sentence · the life · the warning (K-696).
    private static EmailContent Verification(string brand, EmailLinks links, LinkKeys keys, Guid linkId) => new(
        "E-posta adresinizi doğrulayın",
        null,
        [
            new EmailParagraph(Greeting),
            new EmailParagraph($"{brand} sitesine bu e-posta adresiyle kaydoldunuz. Hesabınızı etkinleştirmek için adresinizi doğrulayın."),
            new EmailButton("Adresimi doğrula", links.Account(VerifyScreen, keys.Derive(linkId), isAdmin: false)),
            new EmailParagraph(ConfirmationScreenSentence),
            new EmailParagraph($"Bağlantı kaydınızdan itibaren {Hours(ProductConstants.EmailVerificationLinkHours)} geçerlidir. Süre dolarsa kayıt silinir ve yeniden kaydolmanız gerekir. Yeniden istediğiniz bağlantı öncekini geçersiz kılar ve süreyi uzatmaz."),
            new EmailParagraph("Bu kaydı siz yapmadıysanız bağlantıya tıklamayın; doğrulanmayan kayıt süre dolunca silinir."),
        ]);

    // "Hesabınız için şifre sıfırlama istendi." · "Şifremi sıfırla" · the life and the single use · after a revert the extra sentence · "Bunu siz istemediyseniz…". No screen sentence.
    private static EmailContent? PasswordReset(IReadOnlyDictionary<string, string> values, EmailLinks links, LinkKeys keys, Guid linkId, bool isAdmin)
    {
        if (AccountEmailValues.ReadFlag(Get(values, AccountEmailValues.AfterRevert)) is not { } afterRevert)
        {
            return null;
        }

        List<EmailBlock> blocks =
        [
            new EmailParagraph(Greeting),
            new EmailParagraph("Hesabınız için şifre sıfırlama istendi."),
        ];
        if (afterRevert)
        {
            blocks.Add(new EmailParagraph("Hesabınızın e-posta adresi eski hâline döndürüldü ve şifreniz geçersiz kılındı; yeni bir şifre belirlemek için bu bağlantıyı kullanın."));
        }

        blocks.Add(new EmailButton("Şifremi sıfırla", links.Account(ResetScreen, keys.Derive(linkId), isAdmin)));
        blocks.Add(new EmailParagraph($"Bağlantı {Minutes(ProductConstants.PasswordResetLinkMinutes)} geçerlidir ve bir kez kullanılır. Şifre sıfırlandığında hesabınızın bütün oturumları kapanır."));
        blocks.Add(new EmailParagraph("Bunu siz istemediyseniz bu e-postayı dikkate almayın; şifreniz değişmez."));
        return new EmailContent("Şifre sıfırlama", null, blocks);
    }

    // To the NEW address; the old address is not written.
    private static EmailContent NewAddress(EmailLinks links, LinkKeys keys, Guid linkId, bool isAdmin) => new(
        "Yeni e-posta adresinizi doğrulayın",
        null,
        [
            new EmailParagraph(Greeting),
            new EmailParagraph("Hesabınızın e-posta adresinin bu adresle değiştirilmesi istendi. Değişikliği tamamlamak için yeni adresinizi doğrulayın."),
            new EmailButton("Yeni adresimi doğrula", links.Account(VerifyScreen, keys.Derive(linkId), isAdmin)),
            new EmailParagraph(ConfirmationScreenSentence),
            new EmailParagraph($"Bağlantı değişikliği istediğiniz andan itibaren {Hours(ProductConstants.EmailVerificationLinkHours)} geçerlidir; süre dolarsa değişiklik düşer. Yeniden istediğiniz bağlantı öncekini geçersiz kılar ve süreyi uzatmaz."),
            new EmailParagraph("Bunu siz istemediyseniz bu e-postayı dikkate almayın; adres doğrulanmadıkça hesabınızda hiçbir şey değişmez."),
        ]);

    // To the OLD address; the new address is not written and the revert link is the button.
    private static EmailContent? ChangeNotice(IReadOnlyDictionary<string, string> values, EmailLinks links, LinkKeys keys, Guid linkId, bool isAdmin)
    {
        if (AccountEmailValues.ReadMoment(Get(values, AccountEmailValues.OccurredAt)) is not { } at)
        {
            return null;
        }

        return new EmailContent(
            "Hesabınızın e-posta adresi değiştirildi",
            null,
            [
                new EmailParagraph(Greeting),
                new EmailParagraph($"Hesabınızın e-posta adresi {DisplayFormat.DateAndTime(at)} tarihinde değiştirildi."),
                new EmailParagraph("Bu değişikliği siz yapmadıysanız aşağıdaki bağlantıyla geri alın."),
                new EmailButton("Bu değişikliği ben yapmadım", links.Account(RevertScreen, keys.Derive(linkId), isAdmin)),
                new EmailParagraph(ConfirmationScreenSentence),
                new EmailParagraph($"Bağlantı {Days(ProductConstants.EmailChangeRevertLinkDays)} geçerlidir ve bir kez kullanılır. Geri alırsanız hesabınızın e-postası bu adrese döner, bütün oturumlar kapanır, değişiklik süresince hesaba bağlanan Google girişi kaldırılır ve şifreniz geçersiz kılınır; bu adrese yeni bir şifre belirleme bağlantısı gelir."),
            ]);
    }

    // The inviting administrator's name is the invitation's value; the link is always a panel address.
    private static EmailContent? Invitation(IReadOnlyDictionary<string, string> values, string brand, EmailLinks links, LinkKeys keys, Guid linkId)
    {
        if (Get(values, AccountEmailValues.SenderAdminName) is not { Length: > 0 } sender)
        {
            return null;
        }

        return new EmailContent(
            "Yönetici davetiniz",
            null,
            [
                new EmailParagraph(Greeting),
                new EmailParagraph($"{sender}, sizi {brand} yönetim paneline yönetici olarak davet etti."),
                new EmailButton("Daveti kabul et", links.Account(InvitationScreen, keys.Derive(linkId), isAdmin: true)),
                new EmailParagraph($"Bağlantıyı açınca adınızı yazar ve şifrenizi belirlersiniz; hesabınız o anda açılır. Bağlantı {Hours(ProductConstants.AdminInvitationLinkHours)} geçerlidir ve bir kez kullanılır."),
                new EmailParagraph("Bu daveti beklemiyorsanız bu e-postayı dikkate almayın."),
            ]);
    }

    // No link, no button and no revert path: what was deleted, what is kept and how an open order is entered.
    private static EmailContent? Deleted(IReadOnlyDictionary<string, string> values)
    {
        if (AccountEmailValues.ReadMoment(Get(values, AccountEmailValues.OccurredAt)) is not { } at)
        {
            return null;
        }

        return new EmailContent(
            "Hesabınız silindi",
            null,
            [
                new EmailParagraph(Greeting),
                new EmailParagraph($"Hesabınız {DisplayFormat.Date(at)} tarihinde silindi."),
                new EmailParagraph("Adınız, giriş bilgileriniz, şifreniz, açık oturumlarınız, adres defteriniz ve sepetiniz silindi."),
                new EmailParagraph("Siparişleriniz yasal saklama süresince kayıtlı kalır; sipariş kayıtlarınızdaki kişisel veriler bu süre dolunca imha edilir."),
                new EmailParagraph("Açık siparişlerinize sipariş numaranız ve e-posta adresinizle girebilirsiniz."),
            ]);
    }

    // Customer accounts only; the account's name is not written.
    private static EmailContent GoogleConfirmation(EmailLinks links, LinkKeys keys, Guid linkId) => new(
        "Google ile girişi onaylayın",
        null,
        [
            new EmailParagraph(Greeting),
            new EmailParagraph("Bu e-posta adresiyle açılmış hesabınıza Google ile giriş yapılmak istendi. Google bu adresin sahibini doğrulayamadığı için girişi sizin onayınız olmadan açmıyoruz."),
            new EmailButton("Google girişini onayla", links.Account(VerifyScreen, keys.Derive(linkId), isAdmin: false)),
            new EmailParagraph(ConfirmationScreenSentence),
            new EmailParagraph($"Bağlantı {Minutes(ProductConstants.PasswordResetLinkMinutes)} geçerlidir ve bir kez kullanılır. Onaylarsanız hesabınıza bundan sonra Google ile de girilebilir; onay sizi giriş yapmış saymaz, Google ile yeniden girmeniz gerekir."),
            new EmailParagraph("Bu girişi siz denemediyseniz bu e-postayı dikkate almayın; onaylamadığınız sürece hesabınıza Google ile girilemez ve hiçbir şey değişmez."),
        ]);

    /// <summary>The life of a link in hours as the body writes it: "24 saat".</summary>
    public static string Hours(int hours) => DisplayFormat.Number(hours) + " saat";

    /// <summary>The life of a link in minutes as the body writes it: "60 dakika".</summary>
    public static string Minutes(int minutes) => DisplayFormat.Number(minutes) + " dakika";

    /// <summary>The life of a link in days as the body writes it: "7 gün".</summary>
    public static string Days(int days) => DisplayFormat.Days(days);

    private static string? Get(IReadOnlyDictionary<string, string> values, string name) => values.TryGetValue(name, out string? value) ? value : null;
}
