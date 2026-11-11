namespace Shopfolio.Pages.Components;

/// <summary>
/// One e-mail that did not arrive (04 2.5.3.1, 2.5.4): the notice's name, the date of its first sending and the address it went to. <paramref name="CanResend"/> is true for the customer
/// notices B-1...B-16 (02 9.1.6) and false for an administrator's invitation: that row carries the mark but is never resent, its way is a new invitation (04 9.20.4, 9.20.9; K-759, K-760).
/// </summary>
public sealed record UndeliveredEmailRow(string Key, string NoticeName, DateTimeOffset FirstSentAt, string Address, bool CanResend);

/// <summary>
/// The "e-posta ulaşmadı" block of an order's page (04 2.5.3.1, 2.5.4): the undelivered e-mails one by one with "Yeniden gönder" on the resendable ones, and "Hepsini yeniden gönder" when more
/// than one can be resent. A sending that succeeded is gone from <paramref name="Rows"/> at the next drawing; the last row gone means no block at all. When the sending failed again the row stays and
/// <paramref name="ResendFailed"/> says so in place. The resending is a job in the queue (05 4.4.3, 5.1.4): the answer to the request does not carry its result, the next drawing does.
/// The posts go to <paramref name="PagePath"/> with the named handlers <paramref name="ResendHandler"/> (field <c>key</c>) and <paramref name="ResendAllHandler"/>.
/// </summary>
public sealed record UndeliveredEmailList(string PagePath, string ResendHandler, string ResendAllHandler, IReadOnlyList<UndeliveredEmailRow> Rows, bool ResendFailed)
{
    /// <summary>The text of the lasting message when a resending failed again (04 2.5.4).</summary>
    public const string ResendFailedMessage = "E-posta yeniden gönderilemedi; satır listede kaldı. Biraz sonra yeniden deneyin.";

    /// <summary>The reminder under the block: an order whose address is wrong needs the e-mail corrected, not resent (04 2.5.4; 02 10.4.11).</summary>
    public const string WrongAddressReminder = "Adres yanlışsa e-postayı yeniden göndermek yetmez; önce siparişin e-postasını düzeltin.";

    /// <summary>The label of the per-row button.</summary>
    public const string ResendLabel = "Yeniden gönder";

    /// <summary>The label of the button that resends every resendable row of the order (never across orders - 04 2.5.4).</summary>
    public const string ResendAllLabel = "Hepsini yeniden gönder";

    /// <summary>The label the button shows while its sending is under way (04 2.7.2; K-744).</summary>
    public const string ResendingLabel = "Gönderiliyor";

    /// <summary>Whether the "Hepsini yeniden gönder" button is drawn: more than one row can be resent.</summary>
    public bool ShowsResendAll => Rows.Count(r => r.CanResend) > 1;
}
