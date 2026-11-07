using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.SearchEngine;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.UnitTests.Notifications;

/// <summary>
/// The account and administration e-mails (T309 criteria 1-10, KK1-KK10; 08 2.2.8; 09 11.4): the seven kinds of the closed list, the subject without an order suffix and
/// the greeting without a name, the key derived from the link record's id and never in the row's values, the confirmation-screen sentence in exactly four bodies, no
/// re-send sentence, the same text for a customer and an administrator account with only the path different, the whole life written from the product constants
/// (and the same body however late it is made), the new address and the old address each on its own e-mail without the other written, the invitation's sender and
/// the deletion notice's date and absent link. No network and no database.
/// </summary>
public sealed class AccountEmailTemplateTests
{
    private const string Domain = "magaza.example.com";
    private const string Brand = "Örnek Mağaza";

    private static readonly DateTimeOffset At = new(2026, 11, 4, 9, 30, 0, TimeSpan.FromHours(3));
    private static readonly Guid CustomerId = Guid.Parse("018f0000-0000-7000-8000-0000000000c1");
    private static readonly Guid AdminId = Guid.Parse("018f0000-0000-7000-8000-0000000000d2");
    private static readonly Guid InvitationId = Guid.Parse("018f0000-0000-7000-8000-0000000000e3");
    private static readonly Guid LinkId = Guid.Parse("018f0000-0000-7000-8000-0000000000f4");

    private static readonly ServiceProvider _services = new ServiceCollection().AddLogging().BuildServiceProvider();
    private static readonly EmailFirm _firm = new("Örnek Ticaret Anonim Şirketi", "destek@magaza.example.com");
    private static readonly EmailBranding _brand = new(Brand, null, null);
    private static readonly LinkKeys _keys = new(Options.Create(new LinkKeyOptions { DerivationSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(LinkKeyOptions.DerivationSecretMinBytes)) }));

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static EmailLinks Links() => new(new SiteAddress(Options.Create(new SiteOptions { Domain = Domain })));

    private static EmailComposer Composer()
    {
        IOptions<SiteOptions> options = Options.Create(new SiteOptions { Domain = Domain });
        return new EmailComposer(new EmailLinks(new SiteAddress(options)), options, _services, _services.GetRequiredService<ILoggerFactory>());
    }

    private static AccountEmailRow RowOf(AccountEmail email) => AccountEmailValues.For(email);

    private static EmailContent? TryContent(AccountEmailRow row) =>
        AccountEmailTemplates.Compose(row.Type, row.RelatedRecordType, row.EventValues, Brand, Links(), _keys);

    private static EmailContent Content(AccountEmailRow row) => TryContent(row) ?? throw new InvalidOperationException("The values of the row cannot be read.");

    private static async Task<EmailMessage> MessageOf(AccountEmailRow row) =>
        await Composer().ComposeAsync(row.Recipient, _brand, _firm, Content(row), Ct);

    private static string Decoded(EmailMessage message) => WebUtility.HtmlDecode(message.HtmlBody);

    private static AccountEmail Of(string label, DispatchRelatedRecordType account = DispatchRelatedRecordType.Customer) => label switch
    {
        "Verification" => new VerificationLinkEmail(CustomerId, LinkId, "ali@example.com"),
        "Reset" => new PasswordResetLinkEmail(account, AccountOf(account), LinkId, "ali@example.com", false),
        "ResetAfterRevert" => new PasswordResetLinkEmail(account, AccountOf(account), LinkId, "eski@example.com", true),
        "NewAddress" => new NewAddressVerificationEmail(account, AccountOf(account), LinkId, "yeni@example.com"),
        "ChangeNotice" => new AddressChangeNoticeEmail(account, AccountOf(account), LinkId, "eski@example.com", At),
        "Invitation" => new AdminInvitationEmail(InvitationId, LinkId, "davetli@example.com", "Ali Yılmaz"),
        _ => new AccountDeletedEmail(CustomerId, "ali@example.com", At),
    };

    private static Guid AccountOf(DispatchRelatedRecordType account) => account == DispatchRelatedRecordType.Admin ? AdminId : CustomerId;

    private static readonly string[] SixLabels = ["Verification", "Reset", "NewAddress", "ChangeNotice", "Invitation", "Deleted"];

    public static TheoryData<string> AllSeven() => [.. SixLabels];

    // ---------- KK1: the closed list ----------

    [Fact]
    public void TheSevenKinds_AreExactlyTheSevenTypesOfTheClosedList()
    {
        string[] expected =
        [
            nameof(DispatchType.EmailVerificationLink),
            nameof(DispatchType.PasswordResetLink),
            nameof(DispatchType.NewEmailVerificationLink),
            nameof(DispatchType.EmailChangeNotice),
            nameof(DispatchType.AdminInvitation),
            nameof(DispatchType.AccountDeletedNotice),
            nameof(DispatchType.GoogleLinkConfirmationLink),
        ];
        AccountEmailRow google = AccountEmailValues.ForGoogleLink(CustomerId, LinkId, "ali@example.com");
        AccountEmailRow[] rows = [.. SixLabels.Select(l => RowOf(Of(l))), google];

        Assert.Equal(expected.Order(), rows.Select(r => r.Type.ToString()).Order());
        Assert.All(rows, row => Assert.NotNull(TryContent(row)));
        Assert.Equal(7, typeof(AccountEmail).Assembly.GetTypes().Count(t => !t.IsAbstract && t.IsSubclassOf(typeof(AccountEmail))) + 1);
    }

    // ---------- KK2 / criterion 2: the key is derived from the id when the body is made; the row never holds it ----------

    [Fact]
    public async Task TheKey_IsDerivedFromTheRecordsId_ItIsInTheBodyAndNeverInTheRowsValues()
    {
        string key = _keys.Derive(LinkId);

        foreach (string label in new[] { "Verification", "Reset", "NewAddress", "ChangeNotice", "Invitation" })
        {
            AccountEmailRow row = RowOf(Of(label));
            EmailMessage message = await MessageOf(row);

            Assert.DoesNotContain(key, row.EventValues, StringComparison.Ordinal);
            Assert.Contains(LinkId.ToString("D"), row.EventValues, StringComparison.Ordinal);
            Assert.Contains(Uri.EscapeDataString(key), message.TextBody, StringComparison.Ordinal);
        }

        AccountEmailRow googleRow = AccountEmailValues.ForGoogleLink(CustomerId, LinkId, "ali@example.com");
        Assert.DoesNotContain(key, googleRow.EventValues, StringComparison.Ordinal);
        Assert.Contains(Uri.EscapeDataString(key), (await MessageOf(googleRow)).TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public void TheValuesOfARow_NameTheLinkRecord_UnderTheNameTheRevertRewriteReads()
    {
        AccountEmailRow row = RowOf(Of("ChangeNotice"));

        Assert.Equal(LinkId, LinkEventValues.ReadLinkRecordId(row.EventValues));
    }

    // ---------- criterion 1: subject and greeting ----------

    [Theory]
    [MemberData(nameof(AllSeven))]
    public async Task TheSubject_IsBrandAndTitleOnly_AndTheGreetingCarriesNoName(string label)
    {
        AccountEmailRow row = RowOf(Of(label));
        EmailMessage message = await MessageOf(row);

        Assert.StartsWith(Brand + ": ", message.Subject, StringComparison.Ordinal);
        Assert.DoesNotContain("Sipariş", message.Subject, StringComparison.Ordinal);
        Assert.Null(Content(row).OrderNumber);
        Assert.Contains("Merhaba,", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("Yılmaz", message.Subject, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Verification", "E-posta adresinizi doğrulayın")]
    [InlineData("Reset", "Şifre sıfırlama")]
    [InlineData("NewAddress", "Yeni e-posta adresinizi doğrulayın")]
    [InlineData("ChangeNotice", "Hesabınızın e-posta adresi değiştirildi")]
    [InlineData("Invitation", "Yönetici davetiniz")]
    [InlineData("Deleted", "Hesabınız silindi")]
    public async Task TheSubject_IsTheEventsTitle(string label, string title)
    {
        EmailMessage message = await MessageOf(RowOf(Of(label)));

        Assert.Equal($"{Brand}: {title}", message.Subject);
    }

    [Fact]
    public async Task TheGoogleConfirmation_HasItsOwnTitle_AndNoAccountName()
    {
        EmailMessage message = await MessageOf(AccountEmailValues.ForGoogleLink(CustomerId, LinkId, "ali@example.com"));

        Assert.Equal($"{Brand}: Google ile girişi onaylayın", message.Subject);
    }

    // ---------- KK4 / criterion 4: the confirmation-screen sentence stands in four bodies only ----------

    [Theory]
    [InlineData("Verification", true)]
    [InlineData("NewAddress", true)]
    [InlineData("ChangeNotice", true)]
    [InlineData("Reset", false)]
    [InlineData("Invitation", false)]
    [InlineData("Deleted", false)]
    public async Task TheConfirmationScreenSentence_IsInTheFourLinkBodiesAndInNoOther(string label, bool expected)
    {
        EmailMessage message = await MessageOf(RowOf(Of(label)));

        Assert.Equal(expected, message.TextBody.Contains(AccountEmailTemplates.ConfirmationScreenSentence, StringComparison.Ordinal));
        Assert.Equal(expected, Decoded(message).Contains(AccountEmailTemplates.ConfirmationScreenSentence, StringComparison.Ordinal));
    }

    [Fact]
    public async Task TheGoogleConfirmation_CarriesTheConfirmationScreenSentence_AndTheButtonSentence()
    {
        EmailMessage message = await MessageOf(AccountEmailValues.ForGoogleLink(CustomerId, LinkId, "ali@example.com"));

        Assert.Contains(AccountEmailTemplates.ConfirmationScreenSentence, message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Google girişini onayla", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("onay sizi giriş yapmış saymaz", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThePasswordResetAndTheInvitation_SayNothingOfAConfirmationScreen()
    {
        foreach (string label in new[] { "Reset", "Invitation", "Deleted" })
        {
            EmailMessage message = await MessageOf(RowOf(Of(label)));
            Assert.DoesNotContain("onay ekran", message.TextBody, StringComparison.OrdinalIgnoreCase);
        }
    }

    // ---------- KK5 / criterion 5: no re-send sentence ----------

    [Theory]
    [MemberData(nameof(AllSeven))]
    public async Task TheBody_HasNoResendSentence_AndNoResendNote(string label)
    {
        AccountEmailRow row = RowOf(Of(label));
        EmailMessage message = await MessageOf(row);

        Assert.Null(Content(row).Resend);
        Assert.DoesNotContain("yeniden gönderil", Decoded(message), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("yeniden gönderil", message.TextBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tarihli bildirimin", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OnlyTheDeletionNoticeHasNoAddressInItsBody_TheOtherSixHaveExactlyOne()
    {
        foreach (string label in new[] { "Verification", "Reset", "NewAddress", "ChangeNotice", "Invitation" })
        {
            Assert.Single(Regex.Matches((await MessageOf(RowOf(Of(label)))).TextBody, @"https://\S+"));
        }

        Assert.Empty(Regex.Matches((await MessageOf(RowOf(Of("Deleted")))).TextBody, @"https://\S+"));
        Assert.Empty(Content(RowOf(Of("Deleted"))).Blocks.OfType<EmailButton>());
    }

    // ---------- KK6 / criterion 6: one text, two paths ----------

    [Theory]
    [InlineData("Reset", "sifre-sifirlama")]
    [InlineData("NewAddress", "dogrulama")]
    [InlineData("ChangeNotice", "geri-al")]
    public async Task ACustomerAccountAndAnAdministratorAccount_ShareTheTextAndOnlyThePathDiffers(string label, string screen)
    {
        AccountEmailRow customer = RowOf(Of(label));
        AccountEmailRow admin = RowOf(Of(label, DispatchRelatedRecordType.Admin));
        string key = Uri.EscapeDataString(_keys.Derive(LinkId));

        EmailContent customerContent = Content(customer);
        EmailContent adminContent = Content(admin);

        Assert.Equal($"https://{Domain}/{screen}/{key}", Assert.Single(customerContent.Blocks.OfType<EmailButton>()).Link.AbsoluteUri);
        Assert.Equal($"https://{Domain}/panel/{screen}/{key}", Assert.Single(adminContent.Blocks.OfType<EmailButton>()).Link.AbsoluteUri);
        Assert.Equal(customerContent.Title, adminContent.Title);
        Assert.Equal(customerContent.Blocks.OfType<EmailParagraph>(), adminContent.Blocks.OfType<EmailParagraph>());
        Assert.Equal(Assert.Single(customerContent.Blocks.OfType<EmailButton>()).Label, Assert.Single(adminContent.Blocks.OfType<EmailButton>()).Label);
    }

    [Fact]
    public void TheInvitationLink_IsAlwaysAPanelAddress_AndTheVerificationAndGoogleLinksAreAlwaysACustomers()
    {
        string key = Uri.EscapeDataString(_keys.Derive(LinkId));

        Assert.Equal($"https://{Domain}/panel/davet/{key}", Assert.Single(Content(RowOf(Of("Invitation"))).Blocks.OfType<EmailButton>()).Link.AbsoluteUri);
        Assert.Equal($"https://{Domain}/dogrulama/{key}", Assert.Single(Content(RowOf(Of("Verification"))).Blocks.OfType<EmailButton>()).Link.AbsoluteUri);
        Assert.Equal($"https://{Domain}/dogrulama/{key}", Assert.Single(Content(AccountEmailValues.ForGoogleLink(CustomerId, LinkId, "a@example.com")).Blocks.OfType<EmailButton>()).Link.AbsoluteUri);
    }

    [Fact]
    public void ARowOfTheWrongAccountType_IsUnreadable_NotMadeIntoAnotherAccountsLink()
    {
        AccountEmailRow verification = RowOf(Of("Verification"));
        AccountEmailRow invitation = RowOf(Of("Invitation"));
        AccountEmailRow deleted = RowOf(Of("Deleted"));

        Assert.Null(AccountEmailTemplates.Compose(verification.Type, DispatchRelatedRecordType.Admin, verification.EventValues, Brand, Links(), _keys));
        Assert.Null(AccountEmailTemplates.Compose(invitation.Type, DispatchRelatedRecordType.Customer, invitation.EventValues, Brand, Links(), _keys));
        Assert.Null(AccountEmailTemplates.Compose(deleted.Type, DispatchRelatedRecordType.Admin, deleted.EventValues, Brand, Links(), _keys));
        Assert.Null(AccountEmailTemplates.Compose(DispatchType.GoogleLinkConfirmationLink, DispatchRelatedRecordType.Admin, RowOf(Of("Verification")).EventValues, Brand, Links(), _keys));
        Assert.Null(AccountEmailTemplates.Compose(DispatchType.F1, DispatchRelatedRecordType.Customer, verification.EventValues, Brand, Links(), _keys));
    }

    // ---------- KK7 / criterion 7: the whole life, from the product constants ----------

    [Fact]
    public async Task TheLife_IsWrittenWithItsValue_FromTheProductConstants()
    {
        Assert.Contains($"{ProductConstants.EmailVerificationLinkHours} saat geçerlidir", (await MessageOf(RowOf(Of("Verification")))).TextBody, StringComparison.Ordinal);
        Assert.Contains($"{ProductConstants.EmailVerificationLinkHours} saat geçerlidir", (await MessageOf(RowOf(Of("NewAddress")))).TextBody, StringComparison.Ordinal);
        Assert.Contains($"{ProductConstants.PasswordResetLinkMinutes} dakika geçerlidir", (await MessageOf(RowOf(Of("Reset")))).TextBody, StringComparison.Ordinal);
        Assert.Contains($"{ProductConstants.PasswordResetLinkMinutes} dakika geçerlidir", (await MessageOf(AccountEmailValues.ForGoogleLink(CustomerId, LinkId, "a@example.com"))).TextBody, StringComparison.Ordinal);
        Assert.Contains($"{ProductConstants.AdminInvitationLinkHours} saat geçerlidir", (await MessageOf(RowOf(Of("Invitation")))).TextBody, StringComparison.Ordinal);
        Assert.Contains($"{ProductConstants.EmailChangeRevertLinkDays} gün geçerlidir", (await MessageOf(RowOf(Of("ChangeNotice")))).TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public void TheBody_ReadsNoClock_SoARetryedEmailSaysTheWholeLifeAgain()
    {
        AccountEmailRow row = RowOf(Of("Verification"));

        EmailContent first = Content(row);
        EmailContent retried = Content(row);

        Assert.Equal(first.Blocks.OfType<EmailParagraph>(), retried.Blocks.OfType<EmailParagraph>());
        Assert.DoesNotContain(typeof(AccountEmailTemplates).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static), m => m.GetParameters().Any(p => p.ParameterType == typeof(TimeProvider)));
    }

    // ---------- KK8 / criterion 8: the new address and the old address ----------

    [Fact]
    public async Task TheNewAddressVerification_GoesToTheNewAddress_AndTheChangeNotice_ToTheOldOne_NeitherWritesTheOther()
    {
        AccountEmailRow verification = RowOf(Of("NewAddress"));
        AccountEmailRow notice = RowOf(Of("ChangeNotice"));
        EmailMessage verificationMessage = await MessageOf(verification);
        EmailMessage noticeMessage = await MessageOf(notice);

        Assert.Equal("yeni@example.com", verification.Recipient);
        Assert.Equal("eski@example.com", notice.Recipient);
        Assert.Equal("yeni@example.com", verificationMessage.RecipientEmail);
        Assert.Equal("eski@example.com", noticeMessage.RecipientEmail);
        Assert.DoesNotContain("eski@example.com", verificationMessage.TextBody + verification.EventValues, StringComparison.Ordinal);
        Assert.DoesNotContain("yeni@example.com", noticeMessage.TextBody + notice.EventValues, StringComparison.Ordinal);
        Assert.DoesNotContain("example.com", noticeMessage.TextBody.Replace(Domain, string.Empty, StringComparison.Ordinal), StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheChangeNotice_CarriesTheMomentAndTheRevertLink()
    {
        EmailMessage message = await MessageOf(RowOf(Of("ChangeNotice")));

        Assert.Contains("4 Kasım 2026, 09:30 tarihinde değiştirildi", message.TextBody.Replace("\n", " ", StringComparison.Ordinal), StringComparison.Ordinal);
        Assert.Contains("Bu değişikliği ben yapmadım", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("/geri-al/", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("şifreniz geçersiz kılınır", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheDeletionNotice_HasTheDateTheDeletedTheKeptAndTheWayToAnOpenOrder_AndNoLinkAndNoRevert()
    {
        EmailMessage message = await MessageOf(RowOf(Of("Deleted")));
        string text = message.TextBody.Replace("\n", " ", StringComparison.Ordinal);

        Assert.Contains("Hesabınız 4 Kasım 2026 tarihinde silindi.", text, StringComparison.Ordinal);
        Assert.Contains("Adınız, giriş bilgileriniz, şifreniz, açık oturumlarınız, adres defteriniz ve sepetiniz silindi.", text, StringComparison.Ordinal);
        Assert.Contains("Siparişleriniz yasal saklama süresince kayıtlı kalır", text, StringComparison.Ordinal);
        Assert.Contains("Açık siparişlerinize sipariş numaranız ve e-posta adresinizle girebilirsiniz.", text, StringComparison.Ordinal);
        Assert.DoesNotContain("href", message.HtmlBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("geri-al", message.TextBody + message.HtmlBody, StringComparison.Ordinal);
        Assert.DoesNotContain("https://", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThePasswordResetAfterARevert_AddsTheOneSentence_AndNotWithoutIt()
    {
        string sentence = "Hesabınızın e-posta adresi eski hâline döndürüldü ve şifreniz geçersiz kılındı; yeni bir şifre belirlemek için bu bağlantıyı kullanın.";

        Assert.Contains(sentence, (await MessageOf(RowOf(Of("ResetAfterRevert")))).TextBody.Replace("\n", " ", StringComparison.Ordinal), StringComparison.Ordinal);
        Assert.DoesNotContain(sentence, (await MessageOf(RowOf(Of("Reset")))).TextBody.Replace("\n", " ", StringComparison.Ordinal), StringComparison.Ordinal);
    }

    // ---------- criterion 9: the invitation ----------

    [Fact]
    public async Task TheInvitation_GoesToTheInvitedAddress_AndNamesTheInvitingAdministrator()
    {
        AccountEmailRow row = RowOf(Of("Invitation"));
        EmailMessage message = await MessageOf(row);

        Assert.Equal("davetli@example.com", message.RecipientEmail);
        Assert.Equal(DispatchRelatedRecordType.AdminInvitation, row.RelatedRecordType);
        Assert.Equal(InvitationId, row.RelatedRecordId);
        Assert.Contains($"Ali Yılmaz, sizi {Brand} yönetim paneline yönetici olarak davet etti.", message.TextBody.Replace("\n", " ", StringComparison.Ordinal), StringComparison.Ordinal);
        Assert.Contains("Daveti kabul et", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ASendersName_WithMarkupIsEscapedInTheHtml_AndTheBrandNeverEntersTheRow()
    {
        AccountEmailRow row = RowOf(new AdminInvitationEmail(InvitationId, LinkId, "davetli@example.com", "<b>Ali</b>"));
        EmailMessage message = await MessageOf(row);

        Assert.DoesNotContain("<b>Ali</b>", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("&lt;b&gt;Ali&lt;/b&gt;", message.HtmlBody, StringComparison.Ordinal);
        Assert.DoesNotContain(Brand, row.EventValues, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheBrandNameIsWritten_AndTheDomainStandsInItsPlaceWhenItIsEmpty()
    {
        AccountEmailRow row = RowOf(Of("Verification"));
        EmailContent withDomain = AccountEmailTemplates.Compose(row.Type, row.RelatedRecordType, row.EventValues, Domain, Links(), _keys)!;

        Assert.Contains(Brand + " sitesine", (await MessageOf(row)).TextBody, StringComparison.Ordinal);
        Assert.Contains(Domain + " sitesine", Assert.IsType<EmailParagraph>(withDomain.Blocks[1]).Text, StringComparison.Ordinal);
    }

    // ---------- the records' rules ----------

    [Fact]
    public void ARecordThatBreaksItsRule_IsRefused()
    {
        Assert.Throws<ArgumentException>(() => RowOf(new VerificationLinkEmail(CustomerId, LinkId, " ")));
        Assert.Throws<ArgumentException>(() => RowOf(new VerificationLinkEmail(Guid.Empty, LinkId, "a@example.com")));
        Assert.Throws<ArgumentException>(() => RowOf(new VerificationLinkEmail(CustomerId, Guid.Empty, "a@example.com")));
        Assert.Throws<ArgumentException>(() => RowOf(new PasswordResetLinkEmail(DispatchRelatedRecordType.Order, CustomerId, LinkId, "a@example.com", false)));
        Assert.Throws<ArgumentException>(() => RowOf(new AdminInvitationEmail(InvitationId, LinkId, "a@example.com", " ")));
        Assert.Throws<ArgumentException>(() => RowOf(new AccountDeletedEmail(Guid.Empty, "a@example.com", At)));
    }

    [Fact]
    public void TheRecipientIsTrimmed_AndTheRowCarriesTheOneAddressOnly()
    {
        AccountEmailRow row = RowOf(new NewAddressVerificationEmail(DispatchRelatedRecordType.Customer, CustomerId, LinkId, "  yeni@example.com "));

        Assert.Equal("yeni@example.com", row.Recipient);
        Assert.DoesNotContain("example.com", row.EventValues, StringComparison.Ordinal);
    }

    [Fact]
    public void TheValuesOfAnUnreadableRow_GiveNoContent()
    {
        Assert.Null(AccountEmailTemplates.Compose(DispatchType.EmailVerificationLink, DispatchRelatedRecordType.Customer, null, Brand, Links(), _keys));
        Assert.Null(AccountEmailTemplates.Compose(DispatchType.EmailVerificationLink, DispatchRelatedRecordType.Customer, "not json", Brand, Links(), _keys));
        Assert.Null(AccountEmailTemplates.Compose(DispatchType.EmailVerificationLink, DispatchRelatedRecordType.Customer, "[]", Brand, Links(), _keys));
        Assert.Null(AccountEmailTemplates.Compose(DispatchType.EmailChangeNotice, DispatchRelatedRecordType.Customer, LinkEventValues.ForLink(LinkId), Brand, Links(), _keys));
        Assert.Null(AccountEmailTemplates.Compose(DispatchType.AdminInvitation, DispatchRelatedRecordType.AdminInvitation, LinkEventValues.ForLink(LinkId), Brand, Links(), _keys));
        Assert.Null(AccountEmailTemplates.Compose(DispatchType.AccountDeletedNotice, DispatchRelatedRecordType.Customer, "[]", Brand, Links(), _keys));
        Assert.Null(AccountEmailTemplates.Compose(DispatchType.EmailVerificationLink, null, LinkEventValues.ForLink(LinkId), Brand, Links(), _keys));
    }

    [Fact]
    public void TheAccountEmailRecords_HoldNoMemberThatCouldCarryAPersonalValueButTheRecipientAndTheSendersName()
    {
        string[] allowedText = ["Recipient", "SenderAdminName"];
        Type[] records = [.. typeof(AccountEmail).Assembly.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(AccountEmail)))];

        Assert.Equal(6, records.Length);
        foreach (Type record in records)
        {
            foreach (PropertyInfo property in record.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.PropertyType == typeof(string)))
            {
                Assert.Contains(property.Name, allowedText);
            }
        }
    }
}
