using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// The templates of the firm notices F-1…F-6 (08 2.2.7; T308): a function from the dispatch row's type and event values to the e-mail's
/// <see cref="EmailContent"/>, which <c>EmailComposer</c> turns into the message. The body is made from the stored values and from nothing else - no table is
/// read, so a deleted order, request, administrator or invitation changes nothing (08 2.2.7 "Değerlerin saklanışı"). The common rules hold for all six: the
/// title is the event's headline and the subject is "{brand}: {title}" without an order suffix (the order number never enters <see cref="EmailContent.OrderNumber"/>),
/// the greeting is "Merhaba," and carries no name, the body carries no customer data, the main button goes to a panel screen, and there is no re-send note
/// (08 2.2.7: the firm notices are retried by the dispatch row's own retries and never re-sent by hand, so the content never carries an <see cref="EmailResend"/>).
/// F-5 and F-6 carry no revert link and F-5 no whole IBAN. A row whose values cannot be read gives null; the sender records that as a recipient error without
/// a call (08 2.2 call conditions (5)).
/// </summary>
internal static class CompanyNoticeTemplates
{
    private const string Greeting = "Merhaba,";
    private const string OpenOrder = "Siparişi aç";
    private const string AdminCheckSentenceIban = "Bu değişikliği siz ya da bir çalışma arkadaşınız yapmadıysanız panele girip IBAN'ı düzeltin ve şifrenizi değiştirin.";
    private const string AdminCheckSentenceAccounts = "Bu işlemi siz ya da bir çalışma arkadaşınız yapmadıysanız panele girip yönetici hesaplarını kontrol edin ve şifrenizi değiştirin.";

    /// <summary>The content of the notice a row of <paramref name="type"/> carries; null for a type that is not F-1…F-6 and for values that cannot be read.</summary>
    public static EmailContent? Compose(DispatchType type, string? eventValues, EmailLinks links)
    {
        ArgumentNullException.ThrowIfNull(links);
        IReadOnlyDictionary<string, string>? values = CompanyNoticeValues.Read(eventValues);
        if (values is null)
        {
            return null;
        }

        return type switch
        {
            DispatchType.F1 => NewOrder(values, links),
            DispatchType.F2 => NewContactRequest(values, links),
            DispatchType.F3 => NewClaim(values, links),
            DispatchType.F4 => CardRefund(values, links),
            DispatchType.F5 => IbanChanged(values, links),
            DispatchType.F6 => AdminAccounts(values, links),
            _ => null,
        };
    }

    // F-1: "{tarih ve saat} tarihinde {numara} numaralı yeni bir sipariş alındı." · "Ödeme yöntemi: {Kart | Havale/EFT}. Toplam: {tutar}." · "Siparişi aç".
    private static EmailContent? NewOrder(IReadOnlyDictionary<string, string> values, EmailLinks links)
    {
        if (CompanyNoticeValues.ReadMoment(Get(values, CompanyNoticeValues.OccurredAt)) is not { } at
            || Get(values, CompanyNoticeValues.OrderNumber) is not { Length: > 0 } number
            || Get(values, CompanyNoticeValues.InnerKey) is not { Length: > 0 } key
            || Get(values, CompanyNoticeValues.PaymentMethodName) is not { } methodName
            || !Enum.TryParse(methodName, out PaymentMethod method) || !Enum.IsDefined(method)
            || CompanyNoticeValues.ReadMoney(Get(values, CompanyNoticeValues.Total)) is not { } total)
        {
            return null;
        }

        string methodText = method switch
        {
            PaymentMethod.Card => "Kart",
            PaymentMethod.BankTransfer => "Havale/EFT",
            _ => throw new ArgumentOutOfRangeException(nameof(values), method, "Unknown payment method."),
        };

        return new EmailContent(
            "Yeni sipariş",
            null,
            [
                new EmailParagraph(Greeting),
                new EmailParagraph($"{DisplayFormat.DateAndTime(at)} tarihinde {number} numaralı yeni bir sipariş alındı."),
                new EmailParagraph($"Ödeme yöntemi: {methodText}. Toplam: {DisplayFormat.Amount(total)}."),
                new EmailButton(OpenOrder, links.Panel(PanelScreens.Orders, key)),
            ]);
    }

    // F-2: "{tarih ve saat} tarihinde yeni bir iletişim talebi alındı. Konu: {konu tipi}." · "Talebi aç". No text, name or e-mail of the sender exists in the values.
    private static EmailContent? NewContactRequest(IReadOnlyDictionary<string, string> values, EmailLinks links)
    {
        if (CompanyNoticeValues.ReadMoment(Get(values, CompanyNoticeValues.OccurredAt)) is not { } at
            || Get(values, CompanyNoticeValues.InnerKey) is not { Length: > 0 } key
            || Get(values, CompanyNoticeValues.Topic) is not { } topicName
            || !Enum.TryParse(topicName, out ContactTopic topic) || !Enum.IsDefined(topic))
        {
            return null;
        }

        string topicText = topic switch
        {
            ContactTopic.GeneralQuestion => "Genel soru",
            ContactTopic.AboutOrder => "Sipariş hakkında",
            ContactTopic.AboutProduct => "Ürün hakkında",
            ContactTopic.PersonalDataRequest => "KVKK talebi",
            ContactTopic.Other => "Diğer",
            _ => throw new ArgumentOutOfRangeException(nameof(values), topic, "Unknown contact topic."),
        };

        return new EmailContent(
            "Yeni iletişim talebi",
            null,
            [
                new EmailParagraph(Greeting),
                new EmailParagraph($"{DisplayFormat.DateAndTime(at)} tarihinde yeni bir iletişim talebi alındı. Konu: {topicText}."),
                new EmailButton("Talebi aç", links.Panel(PanelScreens.Requests, key)),
            ]);
    }

    // F-3: four headlines, one per record kind and the reopening; the sentence names the declaration; the button opens the order screen.
    private static EmailContent? NewClaim(IReadOnlyDictionary<string, string> values, EmailLinks links)
    {
        if (CompanyNoticeValues.ReadMoment(Get(values, CompanyNoticeValues.OccurredAt)) is not { } at
            || Get(values, CompanyNoticeValues.OrderNumber) is not { Length: > 0 } number
            || Get(values, CompanyNoticeValues.InnerKey) is not { Length: > 0 } key
            || Get(values, CompanyNoticeValues.Kind) is not { } kindName
            || !Enum.TryParse(kindName, out DispatchRelatedRecordType kind) || !Enum.IsDefined(kind)
            || Get(values, CompanyNoticeValues.Reopened) is not { } reopenedText
            || !bool.TryParse(reopenedText, out bool reopened))
        {
            return null;
        }

        string when = DisplayFormat.DateAndTime(at);
        string title;
        string sentence;
        if (reopened && kind == DispatchRelatedRecordType.DefectClaim)
        {
            title = "Ayıp talebi yeniden açıldı";
            sentence = $"{number} numaralı siparişte {when} tarihinde ayıp talebi yeniden açıldı.";
        }
        else
        {
            (title, string declaration) = kind switch
            {
                DispatchRelatedRecordType.Withdrawal => ("Yeni cayma beyanı", "cayma beyanı"),
                DispatchRelatedRecordType.LateDeliveryTermination => ("Yeni gecikme feshi", "gecikme nedeniyle fesih beyanı"),
                DispatchRelatedRecordType.DefectClaim => ("Yeni ayıp talebi", "ayıp talebi"),
                _ => throw new InvalidOperationException("F-3 is for a withdrawal, a termination for late delivery or a defect claim."),
            };
            sentence = $"{number} numaralı siparişte {when} tarihinde {declaration} kaydedildi.";
        }

        return new EmailContent(
            title,
            null,
            [
                new EmailParagraph(Greeting),
                new EmailParagraph(sentence),
                new EmailButton(OpenOrder, links.Panel(PanelScreens.Orders, key)),
            ]);
    }

    // F-4: the reason and the amount, and that nothing is asked of the firm.
    private static EmailContent? CardRefund(IReadOnlyDictionary<string, string> values, EmailLinks links)
    {
        if (Get(values, CompanyNoticeValues.OrderNumber) is not { Length: > 0 } number
            || Get(values, CompanyNoticeValues.InnerKey) is not { Length: > 0 } key
            || Get(values, CompanyNoticeValues.RefundTypeName) is not { } typeName
            || !Enum.TryParse(typeName, out RefundType refundType) || !Enum.IsDefined(refundType)
            || CompanyNoticeValues.ReadMoney(Get(values, CompanyNoticeValues.Amount)) is not { } amount)
        {
            return null;
        }

        string? reason = refundType switch
        {
            RefundType.Cancellation => "iptal",
            RefundType.ItemRemoval => "kalem çıkarma",
            RefundType.LateDeliveryTermination => "gecikme nedeniyle fesih",
            RefundType.LatePayment => "iptal edilmiş siparişe gelen ödeme",
            _ => null,
        };
        if (reason is null)
        {
            return null;
        }

        return new EmailContent(
            "Karta geri ödeme yapıldı",
            null,
            [
                new EmailParagraph(Greeting),
                new EmailParagraph($"{number} numaralı siparişte sistem, {reason} nedeniyle {DisplayFormat.Amount(amount)} tutarını ödeme yapılan karta kendiliğinden iade etti."),
                new EmailParagraph("Sizin bir işlem yapmanız gerekmez."),
                new EmailButton(OpenOrder, links.Panel(PanelScreens.Orders, key)),
            ]);
    }

    // F-5: the last four digits only, the administrator by name as a value, no whole IBAN and no revert link.
    private static EmailContent? IbanChanged(IReadOnlyDictionary<string, string> values, EmailLinks links)
    {
        if (CompanyNoticeValues.ReadMoment(Get(values, CompanyNoticeValues.OccurredAt)) is not { } at
            || Get(values, CompanyNoticeValues.ActorName) is not { Length: > 0 } actor
            || Get(values, CompanyNoticeValues.LastFourDigits) is not { Length: 4 } lastFour || !lastFour.All(char.IsAsciiDigit)
            || Get(values, CompanyNoticeValues.FirstEntry) is not { } firstText
            || !bool.TryParse(firstText, out bool first))
        {
            return null;
        }

        string verb = first ? "girildi" : "değiştirildi";
        return new EmailContent(
            "Havale IBAN'ı değiştirildi",
            null,
            [
                new EmailParagraph(Greeting),
                new EmailParagraph($"Havale/EFT IBAN'ı {DisplayFormat.DateAndTime(at)} tarihinde {actor} tarafından {verb}. Yeni IBAN'ın son dört hanesi: {lastFour}."),
                new EmailParagraph(AdminCheckSentenceIban),
                new EmailButton("Ödeme yöntemlerini aç", links.Panel(PanelScreens.PaymentMethods)),
            ]);
    }

    // F-6: the invitation and the removal have their own subject and body; the names and the address are the event's values.
    private static EmailContent? AdminAccounts(IReadOnlyDictionary<string, string> values, EmailLinks links)
    {
        if (CompanyNoticeValues.ReadMoment(Get(values, CompanyNoticeValues.OccurredAt)) is not { } at
            || Get(values, CompanyNoticeValues.ActorName) is not { Length: > 0 } actor
            || Get(values, CompanyNoticeValues.Kind) is not { } kind)
        {
            return null;
        }

        string when = DisplayFormat.DateAndTime(at);
        string title;
        string sentence;
        if (string.Equals(kind, nameof(AdminInvitedNotice), StringComparison.Ordinal))
        {
            if (Get(values, CompanyNoticeValues.InvitedEmail) is not { Length: > 0 } invited)
            {
                return null;
            }

            title = "Yönetici daveti gönderildi";
            sentence = $"{when} tarihinde {actor}, {invited} adresine yönetici daveti gönderdi.";
        }
        else if (string.Equals(kind, nameof(AdminRemovedNotice), StringComparison.Ordinal))
        {
            if (Get(values, CompanyNoticeValues.RemovedName) is not { Length: > 0 } removed)
            {
                return null;
            }

            title = "Bir yönetici kaldırıldı";
            sentence = $"{when} tarihinde {actor}, {removed} adlı yöneticiyi kaldırdı.";
        }
        else
        {
            return null;
        }

        return new EmailContent(
            title,
            null,
            [
                new EmailParagraph(Greeting),
                new EmailParagraph(sentence),
                new EmailParagraph(AdminCheckSentenceAccounts),
                new EmailButton("Yönetici hesaplarını aç", links.Panel(PanelScreens.AdminAccounts)),
            ]);
    }

    private static string? Get(IReadOnlyDictionary<string, string> values, string name) => values.TryGetValue(name, out string? value) ? value : null;
}

/// <summary>The names of the panel screens the firm notices open (07 2.1.5; the panel menu's names, 04 3.2).</summary>
internal static class PanelScreens
{
    public const string Orders = "siparisler";
    public const string Requests = "talepler";
    public const string PaymentMethods = "odeme-yontemleri";
    public const string AdminAccounts = "yonetici-hesaplari";
}
