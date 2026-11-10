using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.SearchEngine;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.UnitTests.Notifications;

/// <summary>
/// The firm notices F-1…F-6 (T308 criteria 1-10, KK1-KK10; 08 2.2.7; 09 11.4): the subject without an order suffix, the greeting without a name and a body
/// without customer data, the panel button and the inner key as the event's value, no re-send sentence and no revert link, the four values of F-1, the topic
/// only of F-2, the four F-3 headlines, the reason and the amount of F-4, the last four digits only of F-5, the F-6 invitation and removal apart. The body is
/// made from the stored values and nothing else, so a deleted event changes nothing. No network and no database.
/// </summary>
public sealed class CompanyNoticeTemplateTests
{
    private const string Domain = "magaza.example.com";
    private const string Recipient = "firma@example.com";

    // A whole IBAN and its last four digits: only the four may ever reach a row or a body.
    private const string WholeIban = "TR330006100519786457841326";
    private const string LastFour = "1326";

    private static readonly DateTimeOffset At = new(2026, 11, 4, 9, 30, 0, TimeSpan.FromHours(3));
    private static readonly Guid OrderId = Guid.Parse("018f0000-0000-7000-8000-0000000000a1");
    private static readonly Guid RecordId = Guid.Parse("018f0000-0000-7000-8000-0000000000b2");

    private static readonly ServiceProvider _services = new ServiceCollection().AddLogging().BuildServiceProvider();
    private static readonly EmailFirm _firm = new("Örnek Ticaret Anonim Şirketi", "destek@magaza.example.com");
    private static readonly EmailBranding _brand = new("Örnek Mağaza", null, null);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static EmailLinks Links() => new(new SiteAddress(Options.Create(new SiteOptions { Domain = Domain })));

    private static EmailComposer Composer()
    {
        IOptions<SiteOptions> options = Options.Create(new SiteOptions { Domain = Domain });
        return new EmailComposer(new EmailLinks(new SiteAddress(options)), options, _services, _services.GetRequiredService<ILoggerFactory>());
    }

    private static CompanyNoticeRow RowOf(CompanyNotice notice) => CompanyNoticeValues.ForCompany(notice);

    private static CompanyNoticeRow RowOf(AdminNotice notice) => CompanyNoticeValues.ForAdmins(notice);

    private static EmailContent Content(CompanyNoticeRow row) =>
        CompanyNoticeTemplates.Compose(row.Type, row.EventValues, Links()) ?? throw new InvalidOperationException("The values of the row cannot be read.");

    private static async Task<EmailMessage> MessageOf(CompanyNoticeRow row) =>
        await Composer().ComposeAsync(Recipient, _brand, _firm, Content(row), Ct);

    private static string Decoded(EmailMessage message) => WebUtility.HtmlDecode(message.HtmlBody);

    private static NewOrderNotice Order() => new(OrderId, "SP-1001", PaymentMethod.BankTransfer, 1200m, At);

    private static CardRefundNotice Refund(RefundType type = RefundType.Cancellation, decimal amount = 266.67m) =>
        new(RecordId, OrderId, "SP-1001", type, amount, At);

    // ---------- KK2: the subject has no order, the body no customer data ----------

    public static TheoryData<string> AllSix() => new() { "F1", "F2", "F3", "F4", "F5", "F6" };

    private static CompanyNoticeRow RowFor(string label) => label switch
    {
        "F1" => RowOf(Order()),
        "F2" => RowOf(new NewContactRequestNotice(RecordId, ContactTopic.AboutOrder, At)),
        "F3" => RowOf(new NewClaimNotice(DispatchRelatedRecordType.Withdrawal, RecordId, OrderId, "SP-1001", false, At)),
        "F4" => RowOf(Refund()),
        "F5" => RowOf(IbanChangedNotice.For(WholeIban, "Ali Yılmaz", false, At)),
        _ => RowOf(new AdminInvitedNotice(RecordId, "Ali Yılmaz", "yeni@example.com", At)),
    };

    [Theory]
    [MemberData(nameof(AllSix))]
    public async Task TheSubject_IsBrandAndTitleOnly_WithNoOrderSuffix(string label)
    {
        CompanyNoticeRow row = RowFor(label);
        EmailMessage message = await MessageOf(row);

        Assert.Equal(row.Type.ToString(), label);
        Assert.StartsWith("Örnek Mağaza: ", message.Subject, StringComparison.Ordinal);
        Assert.DoesNotContain("Sipariş", message.Subject, StringComparison.Ordinal);
        Assert.DoesNotContain("SP-1001", message.Subject, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(AllSix))]
    public async Task TheBody_GreetsWithoutAName_AndHasNoResendSentence_AndOneAddressOnlyTheMainButton(string label)
    {
        CompanyNoticeRow row = RowFor(label);
        EmailMessage message = await MessageOf(row);

        Assert.Equal(row.Type.ToString(), label);
        Assert.Null(Content(row).Resend);
        Assert.Contains("Merhaba,", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("yeniden gönderil", Decoded(message), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("yeniden gönderil", message.TextBody, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(Domain, new Uri(Regex.Match(message.TextBody, @"https://\S+").Value).Host);
        Assert.Single(Regex.Matches(message.TextBody, @"https://\S+"));
    }

    [Fact]
    public void TheNoticeRecords_HoldNoMemberThatCouldCarryTheCustomersPersonalData()
    {
        string[] allowedText = ["OrderNumber", "ActorName", "InvitedEmail", "RemovedName", "LastFourDigits"];
        string[] personal = ["Name", "Email", "Phone", "Address", "Message", "Text", "Note", "Body", "Description"];
        Type[] records = [.. typeof(CompanyNotice).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && (t.IsSubclassOf(typeof(CompanyNotice)) || t.IsSubclassOf(typeof(AdminNotice))))];

        Assert.Equal(7, records.Length);
        foreach (Type record in records)
        {
            foreach (PropertyInfo property in record.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.PropertyType == typeof(string)))
            {
                Assert.Contains(property.Name, allowedText);
            }

            foreach (PropertyInfo property in record.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                Assert.DoesNotContain(personal, word => string.Equals(property.Name, word, StringComparison.Ordinal));
            }
        }
    }

    // ---------- KK3: the button goes to a panel screen; the inner key is a value of F-1...F-4 ----------

    [Theory]
    [InlineData("F1", "/panel/siparisler/018f0000-0000-7000-8000-0000000000a1", "Siparişi aç")]
    [InlineData("F2", "/panel/talepler/018f0000-0000-7000-8000-0000000000b2", "Talebi aç")]
    [InlineData("F3", "/panel/siparisler/018f0000-0000-7000-8000-0000000000a1", "Siparişi aç")]
    [InlineData("F4", "/panel/siparisler/018f0000-0000-7000-8000-0000000000a1", "Siparişi aç")]
    [InlineData("F5", "/panel/odeme-yontemleri", "Ödeme yöntemlerini aç")]
    [InlineData("F6", "/panel/yonetici-hesaplari", "Yönetici hesaplarını aç")]
    public void TheMainButton_GoesToThePanelScreen_OnTheDomainSetting(string type, string path, string label)
    {
        CompanyNoticeRow row = type switch
        {
            "F1" => RowOf(Order()),
            "F2" => RowOf(new NewContactRequestNotice(RecordId, ContactTopic.GeneralQuestion, At)),
            "F3" => RowOf(new NewClaimNotice(DispatchRelatedRecordType.DefectClaim, RecordId, OrderId, "SP-1001", false, At)),
            "F4" => RowOf(Refund()),
            "F5" => RowOf(IbanChangedNotice.For(WholeIban, "Ali", true, At)),
            _ => RowOf(new AdminRemovedNotice(RecordId, "Ali", "Veli", At)),
        };

        EmailButton button = Assert.Single(Content(row).Blocks.OfType<EmailButton>());

        Assert.Equal(label, button.Label);
        Assert.Equal("https://" + Domain + path, button.Link.AbsoluteUri);
        Assert.False(button.IsExternal);
        Assert.True(ReturnPath.IsLocalPath(button.Link.AbsolutePath));
        Assert.Equal(button.Link.AbsolutePath, ReturnPath.Resolve(button.Link.AbsolutePath, ReturnArea.Panel));
    }

    [Fact]
    public void TheInnerKey_IsAValueOfTheRow_ForF1ToF4_AndNotForF5AndF6()
    {
        Assert.Equal(OrderId.ToString("D"), CompanyNoticeValues.Read(RowOf(Order()).EventValues)![CompanyNoticeValues.InnerKey]);
        Assert.Equal(RecordId.ToString("D"), CompanyNoticeValues.Read(RowOf(new NewContactRequestNotice(RecordId, ContactTopic.Other, At)).EventValues)![CompanyNoticeValues.InnerKey]);
        Assert.Equal(OrderId.ToString("D"), CompanyNoticeValues.Read(RowOf(new NewClaimNotice(DispatchRelatedRecordType.DefectClaim, RecordId, OrderId, "SP-1", true, At)).EventValues)![CompanyNoticeValues.InnerKey]);
        Assert.Equal(OrderId.ToString("D"), CompanyNoticeValues.Read(RowOf(Refund()).EventValues)![CompanyNoticeValues.InnerKey]);
        Assert.False(CompanyNoticeValues.Read(RowOf(IbanChangedNotice.For(WholeIban, "Ali", false, At)).EventValues)!.ContainsKey(CompanyNoticeValues.InnerKey));
    }

    [Fact]
    public async Task F5AndF6_HaveNoRevertLink_AndNoSecondAddress()
    {
        CompanyNoticeRow[] rows =
        [
            RowOf(IbanChangedNotice.For(WholeIban, "Ali", false, At)),
            RowOf(new AdminInvitedNotice(RecordId, "Ali", "yeni@example.com", At)),
            RowOf(new AdminRemovedNotice(RecordId, "Ali", "Veli", At)),
        ];

        foreach (CompanyNoticeRow row in rows)
        {
            EmailMessage message = await MessageOf(row);
            Assert.DoesNotContain("geri-al", message.HtmlBody, StringComparison.Ordinal);
            Assert.DoesNotContain("geri-al", message.TextBody, StringComparison.Ordinal);
            Assert.DoesNotContain("Geri al", message.TextBody, StringComparison.OrdinalIgnoreCase);
            Assert.Single(Regex.Matches(message.TextBody, @"https://\S+"));
        }
    }

    // ---------- KK5: F-1 carries its four values ----------

    [Fact]
    public async Task F1_HasTheMomentTheNumberThePaymentMethodAndTheTotal()
    {
        EmailMessage message = await MessageOf(RowOf(Order()));
        string body = Decoded(message);

        Assert.Equal("Örnek Mağaza: Yeni sipariş", message.Subject);
        Assert.Contains("4 Kasım 2026, 09:30 tarihinde SP-1001 numaralı yeni bir sipariş alındı.", body, StringComparison.Ordinal);
        Assert.Contains("Ödeme yöntemi: Havale/EFT. Toplam: 1.200 TL.", body, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(PaymentMethod.Card, "Ödeme yöntemi: Kart.")]
    [InlineData(PaymentMethod.BankTransfer, "Ödeme yöntemi: Havale/EFT.")]
    public void F1_WritesTheMethodInItsOwnWords(PaymentMethod method, string expected)
    {
        EmailContent content = Content(RowOf(new NewOrderNotice(OrderId, "SP-7", method, 99.5m, At)));

        Assert.Contains(content.Blocks.OfType<EmailParagraph>(), p => p.Text.StartsWith(expected, StringComparison.Ordinal));
        Assert.Contains(content.Blocks.OfType<EmailParagraph>(), p => p.Text.EndsWith("Toplam: 99,50 TL.", StringComparison.Ordinal));
    }

    // ---------- KK6: F-2 has the topic and no message ----------

    [Theory]
    [InlineData(ContactTopic.GeneralQuestion, "Genel soru")]
    [InlineData(ContactTopic.AboutOrder, "Sipariş hakkında")]
    [InlineData(ContactTopic.AboutProduct, "Ürün hakkında")]
    [InlineData(ContactTopic.PersonalDataRequest, "KVKK talebi")]
    [InlineData(ContactTopic.Other, "Diğer")]
    public async Task F2_WritesTheTopicOnly(ContactTopic topic, string text)
    {
        CompanyNoticeRow row = RowOf(new NewContactRequestNotice(RecordId, topic, At));
        EmailMessage message = await MessageOf(row);

        Assert.Equal("Örnek Mağaza: Yeni iletişim talebi", message.Subject);
        Assert.Contains($"tarihinde yeni bir iletişim talebi alındı. Konu: {text}.", Decoded(message), StringComparison.Ordinal);
        Assert.Equal(["OccurredAt", "InnerKey", "Topic"], CompanyNoticeValues.Read(row.EventValues)!.Keys.ToArray());
        Assert.Equal(3, Content(row).Blocks.Count);
    }

    // ---------- KK7: the four F-3 headlines are apart ----------

    [Fact]
    public void F3_HasFourSeparateHeadlines_AndTheSentenceNamesTheDeclaration()
    {
        (DispatchRelatedRecordType Kind, bool Reopened, string Title, string Sentence)[] cases =
        [
            (DispatchRelatedRecordType.Withdrawal, false, "Yeni cayma beyanı", "cayma beyanı kaydedildi."),
            (DispatchRelatedRecordType.LateDeliveryTermination, false, "Yeni gecikme feshi", "gecikme nedeniyle fesih beyanı kaydedildi."),
            (DispatchRelatedRecordType.DefectClaim, false, "Yeni ayıp talebi", "ayıp talebi kaydedildi."),
            (DispatchRelatedRecordType.DefectClaim, true, "Ayıp talebi yeniden açıldı", "ayıp talebi yeniden açıldı."),
        ];

        foreach ((DispatchRelatedRecordType kind, bool reopened, string title, string sentence) in cases)
        {
            CompanyNoticeRow row = RowOf(new NewClaimNotice(kind, RecordId, OrderId, "SP-1001", reopened, At));
            EmailContent content = Content(row);

            Assert.Equal(title, content.Title);
            Assert.Null(content.OrderNumber);
            Assert.Equal(kind, row.RelatedRecordType);
            Assert.Contains(content.Blocks.OfType<EmailParagraph>(), p => p.Text.StartsWith("SP-1001 numaralı siparişte ", StringComparison.Ordinal) && p.Text.EndsWith(sentence, StringComparison.Ordinal));
        }

        Assert.Equal(4, cases.Select(c => Content(RowOf(new NewClaimNotice(c.Kind, RecordId, OrderId, "SP-1", c.Reopened, At))).Title).Distinct().Count());
    }

    [Fact]
    public void F3_RefusesAKindItDoesNotKnow_AndAReopeningOfAnythingButADefectClaim()
    {
        Assert.Throws<ArgumentException>(() => RowOf(new NewClaimNotice(DispatchRelatedRecordType.Refund, RecordId, OrderId, "SP-1", false, At)));
        Assert.Throws<ArgumentException>(() => RowOf(new NewClaimNotice(DispatchRelatedRecordType.Withdrawal, RecordId, OrderId, "SP-1", true, At)));
    }

    // ---------- KK8: F-4 has the reason and the amount, and asks for nothing ----------

    [Theory]
    [InlineData(RefundType.Cancellation, "iptal")]
    [InlineData(RefundType.ItemRemoval, "kalem çıkarma")]
    [InlineData(RefundType.LateDeliveryTermination, "gecikme nedeniyle fesih")]
    [InlineData(RefundType.LatePayment, "iptal edilmiş siparişe gelen ödeme")]
    public async Task F4_WritesTheReasonAndTheAmount_AndSaysNothingIsToBeDone(RefundType type, string reason)
    {
        EmailMessage message = await MessageOf(RowOf(Refund(type, 1200m)));
        string body = Decoded(message);

        Assert.Equal("Örnek Mağaza: Karta geri ödeme yapıldı", message.Subject);
        Assert.Contains($"SP-1001 numaralı siparişte sistem, {reason} nedeniyle 1.200 TL tutarını ödeme yapılan karta kendiliğinden iade etti.", body, StringComparison.Ordinal);
        Assert.Contains("Sizin bir işlem yapmanız gerekmez.", body, StringComparison.Ordinal);
    }

    [Fact]
    public void F4_ShowsTheAmountWithKurus_AndRefusesARefundTheSystemDoesNotMake()
    {
        Assert.Contains(Content(RowOf(Refund(amount: 266.67m))).Blocks.OfType<EmailParagraph>(), p => p.Text.Contains("266,67 TL", StringComparison.Ordinal));
        foreach (RefundType other in new[] { RefundType.Withdrawal, RefundType.DefectClaimRescission, RefundType.AmountBased })
        {
            Assert.Throws<ArgumentException>(() => RowOf(Refund(other)));
        }
    }

    // ---------- KK9: F-5 keeps the last four digits only ----------

    [Fact]
    public async Task F5_WritesTheLastFourDigitsOnly_AndNeverTheWholeIban()
    {
        CompanyNoticeRow row = RowOf(IbanChangedNotice.For("TR33 0006 1005 1978 6457 8413 26", "Ali Yılmaz", false, At));
        EmailMessage message = await MessageOf(row);
        string body = Decoded(message);

        Assert.Equal("Örnek Mağaza: Havale IBAN'ı değiştirildi", message.Subject);
        Assert.Contains("tarafından değiştirildi. Yeni IBAN'ın son dört hanesi: 1326.", body, StringComparison.Ordinal);
        Assert.Contains("Ali Yılmaz tarafından", body, StringComparison.Ordinal);
        Assert.DoesNotContain("TR33", row.EventValues, StringComparison.Ordinal);
        Assert.DoesNotContain("5197", row.EventValues, StringComparison.Ordinal);
        Assert.DoesNotContain("TR33", body, StringComparison.Ordinal);
        Assert.DoesNotContain("78413", message.TextBody, StringComparison.Ordinal);
        Assert.Contains(LastFour, row.EventValues, StringComparison.Ordinal);
        Assert.Null(row.RelatedRecordType);
        Assert.Null(row.RelatedRecordId);
    }

    [Fact]
    public void F5_SaysEnteredOnTheFirstEntry_AndChangedAfterIt()
    {
        Assert.Contains(Content(RowOf(IbanChangedNotice.For(WholeIban, "Ali", true, At))).Blocks.OfType<EmailParagraph>(), p => p.Text.Contains("Ali tarafından girildi.", StringComparison.Ordinal));
        Assert.Contains(Content(RowOf(IbanChangedNotice.For(WholeIban, "Ali", false, At))).Blocks.OfType<EmailParagraph>(), p => p.Text.Contains("Ali tarafından değiştirildi.", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("")]
    [InlineData("132")]
    [InlineData("13260")]
    [InlineData("13a6")]
    public void F5_RefusesAnythingButFourDigits(string lastFour)
    {
        Assert.Throws<ArgumentException>(() => new IbanChangedNotice("Ali", lastFour, false, At));
    }

    [Fact]
    public void F5_RefusesAnIbanShorterThanItsKeptDigits()
    {
        Assert.Throws<ArgumentException>(() => IbanChangedNotice.For("TR1", "Ali", false, At));
    }

    // ---------- KK10: F-6 values are the event's: the removed administrator and the invitation are not read ----------

    [Fact]
    public async Task F6_Invitation_AndRemoval_HaveTheirOwnSubjectAndBody()
    {
        CompanyNoticeRow invited = RowOf(new AdminInvitedNotice(RecordId, "Ali Yılmaz", "yeni@example.com", At));
        CompanyNoticeRow removed = RowOf(new AdminRemovedNotice(RecordId, "Ali Yılmaz", "Veli Demir", At));
        EmailMessage invitedMessage = await MessageOf(invited);
        EmailMessage removedMessage = await MessageOf(removed);

        Assert.Equal("Örnek Mağaza: Yönetici daveti gönderildi", invitedMessage.Subject);
        Assert.Equal("Örnek Mağaza: Bir yönetici kaldırıldı", removedMessage.Subject);
        Assert.Contains("Ali Yılmaz, yeni@example.com adresine yönetici daveti gönderdi.", Decoded(invitedMessage), StringComparison.Ordinal);
        Assert.Contains("Ali Yılmaz, Veli Demir adlı yöneticiyi kaldırdı.", Decoded(removedMessage), StringComparison.Ordinal);
        Assert.DoesNotContain("Veli Demir", invitedMessage.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("yeni@example.com", removedMessage.TextBody, StringComparison.Ordinal);
        Assert.Equal(DispatchRelatedRecordType.AdminInvitation, invited.RelatedRecordType);
        Assert.Equal(DispatchRelatedRecordType.Admin, removed.RelatedRecordType);
        Assert.All(new[] { invitedMessage, removedMessage }, m => Assert.Contains("Bu işlemi siz ya da bir çalışma arkadaşınız yapmadıysanız", m.TextBody, StringComparison.Ordinal));
    }

    [Fact]
    public async Task TheBody_IsMadeFromTheStoredValuesAlone_SoADeletedEventChangesNothing()
    {
        CompanyNoticeRow removed = RowOf(new AdminRemovedNotice(RecordId, "Ali Yılmaz", "Veli Demir", At));

        // Only the type and the stored list reach the template: there is no table, no id lookup, no clock.
        EmailMessage first = await Composer().ComposeAsync(Recipient, _brand, _firm, CompanyNoticeTemplates.Compose(DispatchType.F6, removed.EventValues, Links())!, Ct);
        EmailMessage again = await Composer().ComposeAsync(Recipient, _brand, _firm, CompanyNoticeTemplates.Compose(DispatchType.F6, removed.EventValues, Links())!, Ct);

        Assert.Equal(first.HtmlBody, again.HtmlBody);
        Assert.Contains("Veli Demir", first.TextBody, StringComparison.Ordinal);
    }

    // ---------- the values are data, never markup or a header ----------

    [Fact]
    public async Task AValueWithMarkupAndLineBreaks_IsTextInTheHtml_AndOneLineInThePlainText()
    {
        const string name = "<b>Ali</b>\r\nBcc: x@evil.example";
        CompanyNoticeRow row = RowOf(new AdminRemovedNotice(RecordId, name, "<script>alert(1)</script>", At));
        EmailMessage message = await MessageOf(row);

        Assert.DoesNotContain("<b>Ali</b>", message.HtmlBody, StringComparison.Ordinal);
        Assert.DoesNotContain("<script>alert", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("&lt;b&gt;Ali&lt;/b&gt;", message.HtmlBody, StringComparison.Ordinal);
        Assert.DoesNotContain("\r", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotMatch(@"Ali</b>\s*\nBcc", message.TextBody);
        Assert.Equal("Örnek Mağaza: Bir yönetici kaldırıldı", message.Subject);
    }

    // ---------- the reader is tolerant: a row whose values cannot be read gives no content ----------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("""[{"Name":"OccurredAt"}]""")]
    [InlineData("""[{"Name":"OccurredAt","Value":"yesterday"}]""")]
    public void ValuesThatCannotBeRead_GiveNoContent(string? values)
    {
        foreach (DispatchType type in new[] { DispatchType.F1, DispatchType.F2, DispatchType.F3, DispatchType.F4, DispatchType.F5, DispatchType.F6 })
        {
            Assert.Null(CompanyNoticeTemplates.Compose(type, values, Links()));
        }
    }

    [Fact]
    public void AnUnknownEnumValueInTheList_GivesNoContent_AndATypeThatIsNotAFirmNoticeGivesNone()
    {
        string values = RowOf(Order()).EventValues.Replace("BankTransfer", "99", StringComparison.Ordinal);

        Assert.Null(CompanyNoticeTemplates.Compose(DispatchType.F1, values, Links()));
        Assert.Null(CompanyNoticeTemplates.Compose(DispatchType.B1, RowOf(Order()).EventValues, Links()));
    }

    // ---------- the row's type and related record agree with 06 4.58 ----------

    [Fact]
    public void TheRows_CarryTheTypeAndTheRelatedRecordOf06_4_58()
    {
        Assert.Equal((DispatchType.F1, DispatchRelatedRecordType.Order, (Guid?)OrderId), Triple(RowOf(Order())));
        Assert.Equal((DispatchType.F2, DispatchRelatedRecordType.ContactRequest, (Guid?)RecordId), Triple(RowOf(new NewContactRequestNotice(RecordId, ContactTopic.Other, At))));
        Assert.Equal((DispatchType.F4, DispatchRelatedRecordType.Refund, (Guid?)RecordId), Triple(RowOf(Refund())));
        Assert.Equal((DispatchType.F6, DispatchRelatedRecordType.AdminInvitation, (Guid?)RecordId), Triple(RowOf(new AdminInvitedNotice(RecordId, "Ali", "a@example.com", At))));
    }

    private static (DispatchType, DispatchRelatedRecordType?, Guid?) Triple(CompanyNoticeRow row) => (row.Type, row.RelatedRecordType, row.RelatedRecordId);

    // ---------- KK4: the retry applies to every row; the order's mark does not read a firm notice ----------

    [Fact]
    public void AFirmNoticeThatCannotBeSent_IsRetriedOnTheSameRow_AndNeverPutsTheMarkOfAnOrder()
    {
        DateTimeOffset start = At;
        foreach (CompanyNoticeRow notice in new[] { RowOf(Order()), RowOf(IbanChangedNotice.For(WholeIban, "Ali", false, At)) })
        {
            DispatchRecord row = DispatchRecord.Queue(notice.Type, Recipient, null, notice.RelatedRecordType, notice.RelatedRecordId, start);

            for (int attempt = 0; attempt < DispatchRetrySchedule.AttemptLimit - 1; attempt++)
            {
                DispatchRetrySchedule.Apply(row, DispatchOutcome.ChannelError, start, start);
                Assert.Equal(DispatchState.Queued, row.DispatchState);
                Assert.NotNull(row.NextAttemptAt);
            }

            DispatchRetrySchedule.Apply(row, DispatchOutcome.ChannelError, start, start);

            Assert.Equal(DispatchState.Undelivered, row.DispatchState);
            Assert.Null(row.OrderId);
            Assert.False(RecipientSendLimit.TakesPart(row.DispatchType, row.OrderId));
        }
    }

    [Fact]
    public void ARowThatIsRetriedAndSentLater_IsSentOnTheSameRow()
    {
        CompanyNoticeRow notice = RowOf(Order());
        DispatchRecord row = DispatchRecord.Queue(notice.Type, Recipient, null, notice.RelatedRecordType, notice.RelatedRecordId, At);

        DispatchRetrySchedule.Apply(row, DispatchOutcome.RecipientError, At, At);
        DispatchRetrySchedule.Apply(row, DispatchOutcome.Sent, At.AddMinutes(5), At.AddMinutes(5));

        Assert.Equal(DispatchState.Sent, row.DispatchState);
        Assert.Equal(2, row.AttemptCount);
    }
}
