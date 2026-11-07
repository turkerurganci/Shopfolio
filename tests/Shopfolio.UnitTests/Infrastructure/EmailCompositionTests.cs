using System.Net;
using System.Text.RegularExpressions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.SearchEngine;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The common pattern of every e-mail (T305 criteria 1-9, KK1-KK9; 08 2.2.4, 2.2.5; 09 11.4 items 1, 2, 7; 04 8.2): two parts from one model, the 600 px single
/// column without script, external style, form or tracking pixel, the logo and the brand colour, the subject, the encoding of every value, the links, the
/// display formats, the re-send note and the footer. No network and no database: the renderer is the framework's own HTML renderer.
/// </summary>
public sealed class EmailCompositionTests
{
    private const string Domain = "magaza.example.com";
    private const string Recipient = "musteri@example.com";

    // Control characters written by number (a source text must not carry them literally).
    private const char CarriageReturn = (char)0x0D;
    private const char Nel = (char)0x85;
    private const char Bell = (char)0x07;
    private const char LineSeparator = (char)0x2028;
    private const char ParagraphSeparator = (char)0x2029;

    private static readonly ServiceProvider _services = new ServiceCollection().AddLogging().BuildServiceProvider();

    private static readonly EmailFirm _firm = new("Örnek Ticaret Anonim Şirketi", "destek@magaza.example.com");
    private static readonly EmailBranding _brand = new("Örnek Mağaza", null, null);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static EmailLinks Links() => new(new SiteAddress(Options.Create(new SiteOptions { Domain = Domain })));

    private static EmailComposer Composer(string domain = Domain)
    {
        IOptions<SiteOptions> options = Options.Create(new SiteOptions { Domain = domain });
        return new EmailComposer(new EmailLinks(new SiteAddress(options)), options, _services, _services.GetRequiredService<ILoggerFactory>());
    }

    private static Task<EmailMessage> Compose(EmailContent content, EmailBranding? brand = null) =>
        Composer().ComposeAsync(Recipient, brand ?? _brand, _firm, content, Ct);

    private static EmailContent Plain(params EmailBlock[] blocks) => new("Siparişiniz alındı", "SP-1001", blocks);

    private static string Decoded(string html) => WebUtility.HtmlDecode(html);

    // ---------- KK1: two parts; the plain text carries the full address ----------

    [Fact]
    public async Task TheMessage_HasAnHtmlPartAndAPlainTextPart_AndTheRecipientAndTheReplyAddress()
    {
        EmailMessage message = await Compose(Plain(new EmailParagraph("Siparişiniz alındı.")));

        Assert.Equal(Recipient, message.RecipientEmail);
        Assert.Equal("destek@magaza.example.com", message.ReplyToEmail);
        Assert.StartsWith("<!DOCTYPE html>", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("Siparişiniz alındı.", Decoded(message.HtmlBody), StringComparison.Ordinal);
        Assert.Contains("Siparişiniz alındı.", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain('<', message.TextBody);
    }

    [Fact]
    public async Task TheLink_IsTheSameFullAddress_InTheHtmlAndInThePlainText()
    {
        Uri link = Links().GuestOrder("anahtar-123");
        EmailMessage message = await Compose(Plain(new EmailButton("Siparişi görüntüle", link)));

        Assert.Equal("https://magaza.example.com/siparis/erisim/anahtar-123", link.AbsoluteUri);
        Assert.Contains(link.AbsoluteUri, message.TextBody, StringComparison.Ordinal);
        Assert.Contains("href=\"" + link.AbsoluteUri + "\"", message.HtmlBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheTwoParts_CarryTheSameValuesAndTheSameAddresses_FromOneModel()
    {
        Uri link = Links().MemberOrder("SP-1001");
        EmailMessage message = await Compose(Plain(
            new EmailHeading("Ödemeniz alındı"),
            new EmailParagraph("Teşekkür ederiz."),
            EmailKeyValue.Amount("Toplam", 1200m),
            new EmailButton("Siparişi görüntüle", link)));
        string html = Decoded(message.HtmlBody);

        foreach (string value in new[] { "Ödemeniz alındı", "Teşekkür ederiz.", "Toplam", "1.200 TL", "Siparişi görüntüle", link.AbsoluteUri })
        {
            Assert.Contains(value, html, StringComparison.Ordinal);
            Assert.Contains(value, message.TextBody, StringComparison.Ordinal);
        }
    }

    // ---------- KK2: single column, 600 px, nothing active ----------

    [Fact]
    public async Task TheHtml_IsOneColumnOfAtMost600Pixels_WithInlineStyleOnly()
    {
        EmailMessage message = await Compose(Plain(new EmailParagraph("Merhaba.")));
        string html = message.HtmlBody;

        Assert.Contains("max-width:600px", html, StringComparison.Ordinal);
        Assert.Contains("width=\"600\"", html, StringComparison.Ordinal);
        Assert.Equal(0, Regex.Matches(html, @"width=""(\d+)""").Count(m => int.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) > 600));
        Assert.Contains("role=\"presentation\"", html, StringComparison.Ordinal);
        Assert.Contains("-apple-system", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheHtml_HasNoScript_NoExternalStyle_NoForm_NoTrackingPixel_AndNoImageWithoutALogo()
    {
        EmailMessage message = await Compose(Plain(new EmailParagraph("Merhaba."), new EmailButton("Git", Links().MemberOrder("SP-1001"))));
        string html = message.HtmlBody.ToUpperInvariant();

        foreach (string forbidden in new[] { "<SCRIPT", "<LINK", "<STYLE", "@IMPORT", "<FORM", "<INPUT", "<IFRAME", "<IMG", "<OBJECT", "<EMBED", "ONLOAD", "ONERROR", "BACKGROUND:URL", "BACKGROUND-IMAGE" })
        {
            Assert.DoesNotContain(forbidden, html, StringComparison.Ordinal);
        }
    }

    // ---------- KK3: logo, alternative text, brand colour ----------

    [Fact]
    public async Task TheLogo_IsTheSourceCopyByItsAbsoluteAddress_AndTheAlternativeTextIsTheBrandName()
    {
        EmailBranding brand = new("Örnek Mağaza", "/gorsel/ab12cd34/logo.png", null);

        EmailMessage message = await Compose(Plain(new EmailParagraph("Merhaba.")), brand);

        Assert.Equal(1, Regex.Count(message.HtmlBody, "<img "));
        Assert.Contains("src=\"https://magaza.example.com/gorsel/ab12cd34/logo.png\"", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("alt=\"" + HtmlAttribute("Örnek Mağaza") + "\"", message.HtmlBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WithoutALogo_TheBrandNameIsTheHeaderText_AndThereIsNoImage()
    {
        EmailMessage message = await Compose(Plain(new EmailParagraph("Merhaba.")));

        Assert.DoesNotContain("<img", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains(HtmlText("Örnek Mağaza"), message.HtmlBody, StringComparison.Ordinal);
        Assert.StartsWith("Örnek Mağaza\n", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ALogoPathOutsideTheImageAddress_IsRefused()
    {
        EmailBranding brand = new("Örnek Mağaza", "/yonetim/logo.png", null);

        await Assert.ThrowsAsync<ArgumentException>(() => Compose(Plain(new EmailParagraph("x")), brand));
    }

    [Theory]
    [InlineData("#FFFFFF", "#000000")]
    [InlineData("#F5E663", "#000000")]
    [InlineData("#000000", "#FFFFFF")]
    [InlineData("#1B3A6B", "#FFFFFF")]
    [InlineData("#c0392b", "#FFFFFF")]
    public void TheTextOnTheButton_IsBlackOrWhiteByContrast(string brandColor, string expectedText)
    {
        (string background, string text) = EmailColors.Button(brandColor);

        Assert.Equal(brandColor.ToUpperInvariant(), background);
        Assert.Equal(expectedText, text);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("red")]
    [InlineData("#FFF")]
    [InlineData("#GGGGGG")]
    [InlineData("#12345;background:url(x)")]
    [InlineData("#1B3A6B\"onload=\"x")]
    public void ABrandColorThatIsNotSixHexDigits_IsNotUsed_TheButtonTakesTheFixedColour(string? brandColor)
    {
        Assert.Equal((EmailColors.DefaultButtonBackground, EmailColors.White), EmailColors.Button(brandColor));
    }

    [Fact]
    public async Task TheBrandColour_IsOnlyTheMainButtonsBackground()
    {
        EmailBranding brand = new("Örnek Mağaza", null, "#C0392B");

        EmailMessage message = await Compose(Plain(new EmailHeading("Başlık"), new EmailParagraph("Metin"), new EmailButton("Git", Links().MemberOrder("SP-1001"))), brand);
        MatchCollection uses = Regex.Matches(message.HtmlBody, "#C0392B", RegexOptions.IgnoreCase);

        Assert.Equal(2, uses.Count);
        Assert.All(uses, use =>
        {
            string before = message.HtmlBody[..use.Index];
            Assert.True(before.EndsWith("bgcolor=\"", StringComparison.Ordinal) || before.EndsWith("background-color:", StringComparison.Ordinal), before[^20..]);
        });
        Assert.Contains("color:#FFFFFF;text-decoration:none", message.HtmlBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WithoutABrandColour_TheButtonIsTheFixedColour()
    {
        EmailMessage message = await Compose(Plain(new EmailButton("Git", Links().MemberOrder("SP-1001"))));

        Assert.Contains("background-color:" + EmailColors.DefaultButtonBackground, message.HtmlBody, StringComparison.Ordinal);
    }

    // ---------- KK4: the subject ----------

    [Fact]
    public async Task TheSubject_IsBrandColonTitle_AndAnOrderBoundOneAddsTheOrderNumber()
    {
        EmailMessage bound = await Compose(Plain(new EmailParagraph("x")));
        EmailMessage free = await Compose(new EmailContent("Hesabınız silindi", null, [new EmailParagraph("x")]));

        Assert.Equal("Örnek Mağaza: Siparişiniz alındı (Sipariş SP-1001)", bound.Subject);
        Assert.Equal("Örnek Mağaza: Hesabınız silindi", free.Subject);
        Assert.Equal("Örnek Mağaza", bound.SenderDisplayName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\r\n\t")]
    public async Task WithoutABrandName_TheDomainStandsInItsPlace_InTheSubjectAndTheSenderName(string? brandName)
    {
        EmailMessage message = await Compose(Plain(new EmailParagraph("x")), new EmailBranding(brandName, null, null));

        Assert.Equal("magaza.example.com: Siparişiniz alındı (Sipariş SP-1001)", message.Subject);
        Assert.Equal(Domain, message.SenderDisplayName);
        Assert.StartsWith(Domain + "\n", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ABrandNameWithLineBreaksAndControlCharacters_IsOneLine_InTheSubjectAndTheSenderName()
    {
        EmailBranding brand = new("Örnek" + CarriageReturn + "\nBcc: kurban@example.com" + Nel + LineSeparator + "Mağaza" + Bell, null, null);

        EmailMessage message = await Compose(Plain(new EmailParagraph("x")), brand);

        Assert.DoesNotContain('\r', message.Subject);
        Assert.DoesNotContain('\n', message.Subject);
        Assert.DoesNotContain(Nel, message.Subject);
        Assert.DoesNotContain(LineSeparator, message.Subject);
        Assert.DoesNotContain(Bell, message.Subject);
        Assert.Equal("Örnek  Bcc: kurban@example.com  Mağaza: Siparişiniz alındı (Sipariş SP-1001)", message.Subject);
        Assert.Equal("Örnek  Bcc: kurban@example.com  Mağaza", message.SenderDisplayName);
        Assert.DoesNotContain('\n', message.SenderDisplayName);
    }

    // ---------- KK5: encoding ----------

    [Fact]
    public async Task AValueWithMarkup_IsTextInTheHtml_AndTheLineBreaksAreBlanksInThePlainText()
    {
        const string hostile = "<b>Ahşap</b> Masa & \"Sandalye\" <script>alert(1)</script>\r\nikinci satır";

        EmailMessage message = await Compose(Plain(
            new EmailParagraph(hostile),
            new EmailKeyValue(hostile, hostile),
            new EmailHeading(hostile)));

        Assert.DoesNotContain("<b>", message.HtmlBody, StringComparison.Ordinal);
        Assert.DoesNotContain("<script>", message.HtmlBody, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("&lt;b&gt;", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains(hostile, Decoded(message.HtmlBody).ReplaceLineEndings("\r\n"), StringComparison.Ordinal);
        Assert.Contains("<b>Ahşap</b> Masa & \"Sandalye\" <script>alert(1)</script>  ikinci satır", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("\r", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheFirmsValues_AreEncodedToo_TheBrandNameAndTheFooter()
    {
        EmailFirm firm = new("<i>Örnek</i> A.Ş.", "<u>x</u>@example.com");
        EmailBranding brand = new("<b>Mağaza</b>", "/gorsel/ab12/logo.png", null);

        EmailMessage message = await Composer().ComposeAsync(Recipient, brand, firm, Plain(new EmailParagraph("x")), Ct);

        Assert.DoesNotContain("<b>", message.HtmlBody, StringComparison.Ordinal);
        Assert.DoesNotContain("<i>", message.HtmlBody, StringComparison.Ordinal);
        Assert.DoesNotContain("<u>", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("&lt;i&gt;", message.HtmlBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AValueThatLooksLikeALinkOrAnAttribute_StaysText()
    {
        const string trick = "\"><a href=\"https://kotu.example.com\">tıkla</a>";

        EmailMessage message = await Compose(Plain(new EmailParagraph(trick), new EmailKeyValue("Alıcı", trick)));

        Assert.DoesNotContain("<a href", message.HtmlBody, StringComparison.Ordinal);
        Assert.DoesNotContain("href=\"https://kotu", message.HtmlBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ALegalText_KeepsItsLinesAndParagraphs_AtTwelvePoints_AndIsEncoded()
    {
        const string legal = "MADDE 1 <taraflar>\nSatıcı: Örnek A.Ş.\n\nMADDE 2\nBedel: 1.200 TL";

        EmailMessage message = await Compose(Plain(new EmailLegalText("Mesafeli Satış Sözleşmesi", legal)));

        Assert.Contains("font-size:12pt", message.HtmlBody, StringComparison.Ordinal);
        Assert.Equal(2, Regex.Count(message.HtmlBody, "<p style=\"margin:0 0 12px 0;font-size:12pt"));
        Assert.Contains("<br>", message.HtmlBody, StringComparison.Ordinal);
        Assert.DoesNotContain("<taraflar>", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("MADDE 1 <taraflar>\nSatıcı: Örnek A.Ş.\n\nMADDE 2\nBedel: 1.200 TL", message.TextBody, StringComparison.Ordinal);
    }

    // ---------- KK6: links ----------

    [Fact]
    public async Task TheMainAction_IsAButton_AndTheSameAddressIsPlainTextUnderIt()
    {
        Uri link = Links().GuestOrder("anahtar-123");

        EmailMessage message = await Compose(Plain(new EmailButton("Siparişi görüntüle", link)));
        int anchor = message.HtmlBody.IndexOf("<a href=", StringComparison.Ordinal);
        int closing = message.HtmlBody.IndexOf("</a>", anchor, StringComparison.Ordinal);
        string after = message.HtmlBody[(closing + 4)..];

        Assert.Equal(1, Regex.Count(message.HtmlBody, "<a "));
        Assert.Matches(@"^\s*</td>\s*</tr>\s*</table>\s*<p [^>]*>" + Regex.Escape(link.AbsoluteUri) + @"</p>", after);
    }

    [Fact]
    public void TheGuestOrderLink_CarriesTheAccessKey_TheMemberOrderLinkTheOrderNumber_BothFromTheDomainSetting()
    {
        EmailLinks links = Links();

        Assert.Equal("https://magaza.example.com/siparis/erisim/AbC-123_x", links.GuestOrder("AbC-123_x").AbsoluteUri);
        Assert.Equal("https://magaza.example.com/siparis/SP-1001", links.MemberOrder("SP-1001").AbsoluteUri);
        Assert.NotEqual(links.GuestOrder("SP-1001"), links.MemberOrder("SP-1001"));
    }

    [Fact]
    public void AValueInThePath_IsPercentEncoded_SoItCannotLeaveThePath()
    {
        EmailLinks links = Links();

        Assert.Equal("https://magaza.example.com/siparis/erisim/a%2Fb%3Fc%23d", links.GuestOrder("a/b?c#d").AbsoluteUri);
        Assert.Equal("https://magaza.example.com/siparis/SP%2F1%20x", links.MemberOrder("SP/1 x").AbsoluteUri);
    }

    [Fact]
    public async Task TheAddressRoot_IsTheDomainSetting_Always()
    {
        EmailMessage message = await Composer("baska.example.org").ComposeAsync(
            Recipient,
            new EmailBranding(null, "/gorsel/ab12/logo.png", null),
            new EmailFirm("Örnek Ticaret Anonim Şirketi", "destek@baska.example.org"),
            Plain(new EmailButton("Git", new EmailLinks(new SiteAddress(Options.Create(new SiteOptions { Domain = "baska.example.org" }))).MemberOrder("SP-1"))),
            Ct);

        Assert.Contains("src=\"https://baska.example.org/gorsel/ab12/logo.png\"", message.HtmlBody, StringComparison.Ordinal);
        Assert.DoesNotContain(Domain, message.HtmlBody, StringComparison.Ordinal);
        Assert.DoesNotContain(Domain, message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnExternalAddress_CarriesNoReferrer_AnInternalOneDoesNot()
    {
        Uri tracking = new("https://kargo.example.net/takip?no=123");

        EmailMessage external = await Compose(Plain(new EmailButton("Takip et", tracking, isExternal: true)));
        EmailMessage own = await Compose(Plain(new EmailButton("Siparişi görüntüle", Links().MemberOrder("SP-1001"))));

        Assert.Contains("rel=\"noreferrer\"", external.HtmlBody, StringComparison.Ordinal);
        Assert.DoesNotContain("rel=", own.HtmlBody, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("http://magaza.example.com/siparis/SP-1")]
    [InlineData("javascript:alert(1)")]
    [InlineData("mailto:x@example.com")]
    [InlineData("ftp://magaza.example.com/x")]
    public void AButtonAddress_IsAFullHttpsAddress(string address)
    {
        Assert.Throws<ArgumentException>(() => new EmailButton("Git", new Uri(address)));
    }

    [Fact]
    public void ARelativeButtonAddress_IsRefused()
    {
        Assert.Throws<ArgumentException>(() => new EmailButton("Git", new Uri("/siparis/SP-1", UriKind.Relative)));
    }

    [Fact]
    public async Task AnEmailWithoutAButton_HasNoLinkAtAll_TheB15AndTheDeletionNoticeShape()
    {
        EmailMessage message = await Compose(new EmailContent("Hesabınız silindi", null, [new EmailParagraph("Hesabınız silindi.")]));

        Assert.DoesNotContain("<a ", message.HtmlBody, StringComparison.Ordinal);
        Assert.DoesNotContain("href", message.HtmlBody, StringComparison.Ordinal);
        Assert.DoesNotContain("https://", message.HtmlBody, StringComparison.Ordinal);
        Assert.DoesNotContain("https://", message.TextBody, StringComparison.Ordinal);
    }

    // ---------- KK7: formats ----------

    [Fact]
    public async Task AmountsDatesAndNumbers_AreInTheDisplayFormat()
    {
        DateTimeOffset utcEvening = new(2026, 10, 4, 21, 0, 0, TimeSpan.Zero);

        EmailMessage message = await Compose(Plain(
            EmailKeyValue.Amount("Toplam", 1200m),
            EmailKeyValue.Amount("Kargo", 266.67m),
            EmailKeyValue.Amount("İndirim", -50m),
            EmailKeyValue.Date("Tarih", new DateTimeOffset(2026, 10, 4, 12, 0, 0, TimeSpan.Zero)),
            EmailKeyValue.Date("Gece", utcEvening),
            EmailKeyValue.Number("Adet", 1250)));
        string html = Decoded(message.HtmlBody);

        foreach (string expected in new[] { "1.200 TL", "266,67 TL", "-50 TL", "4 Ekim 2026", "5 Ekim 2026", "1.250" })
        {
            Assert.Contains(expected, html, StringComparison.Ordinal);
            Assert.Contains(expected, message.TextBody, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("₺", html, StringComparison.Ordinal);
        Assert.DoesNotContain("TRY", message.TextBody, StringComparison.Ordinal);
    }

    // ---------- KK8: the re-send note ----------

    [Fact]
    public async Task AResend_KeepsTheSubject_AndPutsTheDatedSentenceOnTop_InBothParts()
    {
        EmailResend resend = new(new DateTimeOffset(2026, 10, 4, 12, 0, 0, TimeSpan.Zero));
        EmailContent first = Plain(new EmailParagraph("Siparişiniz alındı."));

        EmailMessage original = await Compose(first);
        EmailMessage again = await Compose(first with { Resend = resend });

        const string sentence = "Bu e-posta 4 Ekim 2026 tarihli bildirimin yeniden gönderilmesidir.";
        Assert.Equal(original.Subject, again.Subject);
        Assert.Equal(sentence, resend.Sentence);
        Assert.Contains(sentence, Decoded(again.HtmlBody), StringComparison.Ordinal);
        Assert.True(Decoded(again.HtmlBody).IndexOf(sentence, StringComparison.Ordinal) < Decoded(again.HtmlBody).IndexOf("Siparişiniz alındı.", StringComparison.Ordinal));
        Assert.StartsWith("Örnek Mağaza\n\n" + sentence + "\n\n", again.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ANewEventOfTheSameType_CarriesNoResendSentence()
    {
        EmailMessage message = await Compose(Plain(new EmailParagraph("İkinci hatırlatma.")));

        Assert.DoesNotContain("yeniden gönderilmesidir", Decoded(message.HtmlBody), StringComparison.Ordinal);
        Assert.DoesNotContain("yeniden gönderilmesidir", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public void TheResendDate_IsTheTurkeyDayOfTheFirstEmail()
    {
        EmailResend lateEvening = new(new DateTimeOffset(2026, 10, 4, 21, 30, 0, TimeSpan.Zero));

        Assert.Equal("Bu e-posta 5 Ekim 2026 tarihli bildirimin yeniden gönderilmesidir.", lateEvening.Sentence);
    }

    // ---------- KK9: the footer ----------

    [Fact]
    public async Task TheFooter_HasTheLegalNameTheContactEmailAndTheReplySentence_AndNoSubscriptionOrMarketing()
    {
        EmailMessage message = await Compose(Plain(new EmailParagraph("Merhaba.")));
        string html = Decoded(message.HtmlBody);

        foreach (string part in new[] { "Örnek Ticaret Anonim Şirketi", "destek@magaza.example.com", "Bu e-postayı yanıtlayarak firmaya ulaşabilirsiniz." })
        {
            Assert.Contains(part, html, StringComparison.Ordinal);
            Assert.Contains(part, message.TextBody, StringComparison.Ordinal);
        }

        foreach (string forbidden in new[] { "abonelik", "unsubscribe", "aboneliği", "bülten", "kampanya", "Shopfolio", "powered by" })
        {
            Assert.DoesNotContain(forbidden, html, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(forbidden, message.TextBody, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task TheHtmlLanguage_IsTurkish_AndTheCharsetIsUtf8()
    {
        EmailMessage message = await Compose(Plain(new EmailParagraph("Merhaba.")));

        Assert.Contains("<html lang=\"tr\">", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("<meta charset=\"utf-8\">", message.HtmlBody, StringComparison.Ordinal);
    }

    // ---------- the plain text writer and the helpers ----------

    [Fact]
    public void TheOneLineRule_TurnsEveryLineBreakAndControlCharacterIntoABlank_AndTrimsTheEnds()
    {
        string messy = " a" + CarriageReturn + "\nb\tc" + Nel + "d" + ParagraphSeparator + "e\n";

        Assert.Equal("a  b c d e", EmailText.OneLine(messy));
        Assert.Equal("Mağaza", EmailText.OneLine("Mağaza"));
    }

    [Fact]
    public async Task ACancelledToken_StopsTheComposition()
    {
        using CancellationTokenSource cancelled = new();
        await cancelled.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Composer().ComposeAsync(Recipient, _brand, _firm, Plain(new EmailParagraph("x")), cancelled.Token));
    }

    // The renderer encodes with the framework's default HTML encoder (the Basic Latin range stays, the rest becomes numeric references).
    private static string HtmlText(string value) => System.Text.Encodings.Web.HtmlEncoder.Default.Encode(value);

    private static string HtmlAttribute(string value) => HtmlText(value);
}
