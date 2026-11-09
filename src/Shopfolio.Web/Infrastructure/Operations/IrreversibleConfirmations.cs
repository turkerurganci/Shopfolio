namespace Shopfolio.Infrastructure.Operations;

/// <summary>
/// The eight operations of 03 1.6.1 that ask the administrator's irreversible-action confirmation (02 5.9; 04 2.4.2.1), in the table's order. The list is
/// closed: an operation that is not here asks no such confirmation - the system's own card refund on a cancellation is not one (03 1.6.1; 02 5.9). The panel
/// offers no "undo" for any of them: a wrong transition is corrected by a new transition (02 10.4.6; 05 4.1.4).
/// </summary>
public enum IrreversibleAction
{
    /// <summary>03 1.6.1.1: the bank transfer's "paid" mark (Ö1 and its simultaneous transitions).</summary>
    TransferPaidMark,

    /// <summary>03 1.6.1.2: the shipment and the re-sending (S5, S8; K-677).</summary>
    Shipment,

    /// <summary>03 1.6.1.3: the firm's cancellation of the order or of an item (S3, S4, S9 or the item's cancellation record).</summary>
    FirmCancellation,

    /// <summary>03 1.6.1.4: the administrator's processing of a refund, the amount-based partial refund included (Ö3-Ö5).</summary>
    RefundProcessing,

    /// <summary>03 1.6.1.5: an item removal that sends money to the card on the card line.</summary>
    CardRefundingRemoval,

    /// <summary>03 1.6.1.6: the return rejection (K-656).</summary>
    ReturnRejection,

    /// <summary>03 1.6.1.7: the record of a withdrawal or a late delivery termination notice that came from another channel (K-688, K-715).</summary>
    OtherChannelRecord,

    /// <summary>03 1.6.1.8: the member account's deletion on request (K-715).</summary>
    AccountDeletion,
}

/// <summary>
/// What the confirmation window of an irreversible operation says before the administrator confirms (04 2.4.1; K-797, K-798): the warning that comes before
/// the sentence, when the operation has one, the one sentence of the result, and <see cref="IrreversibleConfirmations.IrreversibleSentence"/>.
/// </summary>
public sealed record IrreversibleConfirmation(IrreversibleAction Action, string? Warning, string Sentence)
{
    /// <summary>The lines of the window in their order: the warning (if any), the result sentence and "Bu işlem geri alınamaz.".</summary>
    public IReadOnlyList<string> Lines => Warning is null
        ? [Sentence, IrreversibleConfirmations.IrreversibleSentence]
        : [Warning, Sentence, IrreversibleConfirmations.IrreversibleSentence];
}

/// <summary>
/// The one home of the irreversible-action confirmation texts (03 1.6.1; 02 5.9; 04 2.4.1, 2.4.3): the result sentences of K-797 and the digital-item
/// warning of K-798 (5) on the transfer's "paid" mark (03 1.6.1.1; 02 10.4.6: that mark can not be corrected). The values are the screen's: {n} is the
/// sum of the units the operation takes (K-795), an amount is the system's calculation written with <see cref="DisplayFormat.Amount"/>, a date with
/// <see cref="DisplayFormat.Date(DateOnly)"/>; no period is written. The administrator is addressed; the sentence says the result.
/// </summary>
public static class IrreversibleConfirmations
{
    /// <summary>The sentence every irreversible operation's window ends with (04 2.4.1; K-797).</summary>
    public const string IrreversibleSentence = "Bu işlem geri alınamaz.";

    /// <summary>02 7.2.4: the warning of the "stokta bulunamadı" reason - Yönetmelik m.16/4 does not take a missing stock as the impossibility of performance.</summary>
    public const string OutOfStockLegalNotice = "Stokta bulunmaması yasal olarak ifanın imkânsızlaşması sayılmaz; bu iptalin sonucu firmadadır.";

    /// <summary>K-798 (5): the warning before the "paid" mark's sentence on an order with a digital item.</summary>
    public const string DigitalItemPaidMarkWarning = "Siparişte dijital ürün var: işaretle birlikte indirme açılır ve bu işaret sonradan düzeltilemez.";

    /// <summary>03 1.6.1.1: the bank transfer's "paid" mark; on an order with a digital item the warning that the mark can not be corrected comes first.</summary>
    public static IrreversibleConfirmation TransferPaidMark(bool hasDigitalItem) => new(
        IrreversibleAction.TransferPaidMark,
        hasDigitalItem ? DigitalItemPaidMarkWarning : null,
        "Siparişin ödemesi alınmış sayılacak ve müşteriye ödeme onayı gönderilecek.");

    /// <summary>03 1.6.1.2: the shipment (S5) or the re-sending (S8); the own-vehicle declaration has its own sentence.</summary>
    public static IrreversibleConfirmation Shipment(bool isResending, bool isOwnVehicleDelivery) => new(
        IrreversibleAction.Shipment,
        null,
        isOwnVehicleDelivery
            ? "Müşteriye siparişin kendi aracınızla teslim edileceği bildirilecek."
            : isResending ? "Müşteriye yeni takip numarası gönderilecek." : "Müşteriye takip numarası gönderilecek.");

    /// <summary>
    /// 03 1.6.1.3: the firm's cancellation. An unpaid order is cancelled as a whole; a paid one by its units, with the line's second sentence (the card
    /// refund starts at once, or the customer is asked for an IBAN) and, on "delivery failed - caused by the customer", that the outbound shipping fee is
    /// not paid back.
    /// </summary>
    public static IrreversibleConfirmation FirmCancellation(bool isPaid, int units, bool isCardLine, bool isCustomerCausedDeliveryFailure, bool isOutOfStock = false)
    {
        // "Stokta bulunamadı": before the confirmation the panel says that a missing stock is not a legal impossibility and the outcome is the firm's (02 7.2.4; T368 k8).
        string? warning = isOutOfStock ? OutOfStockLegalNotice : null;
        if (!isPaid)
        {
            return new(IrreversibleAction.FirmCancellation, warning, "Sipariş iptal edilecek ve müşteriye sebebiyle bildirilecek.");
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(units);
        string sentence = DisplayFormat.Number(units) + " ürün iptal edilecek ve müşteriye sebebiyle bildirilecek. "
            + (isCardLine ? "Ödenen tutarın kart iadesi hemen başlatılacak." : "Müşteriden geri ödeme için IBAN istenecek.");
        if (isCustomerCausedDeliveryFailure)
        {
            sentence += " Gidiş kargo bedeli geri ödenmeyecek.";
        }

        return new(IrreversibleAction.FirmCancellation, warning, sentence);
    }

    /// <summary>03 1.6.1.4: the processing of a refund - to the customer's IBAN on the transfer line, to the card on the card line.</summary>
    public static IrreversibleConfirmation RefundProcessing(decimal amount, string? iban)
    {
        string sentence = iban is null
            ? DisplayFormat.Amount(amount) + " müşterinin kartına geri ödenecek ve müşteriye bildirilecek."
            : DisplayFormat.Amount(amount) + " tutarındaki geri ödemenin " + DisplayFormat.Iban(iban) + " hesabına gönderildiği kaydedilecek ve müşteriye bildirilecek.";
        return new(IrreversibleAction.RefundProcessing, null, sentence);
    }

    /// <summary>03 1.6.1.5: an item removal that sends money to the card.</summary>
    public static IrreversibleConfirmation CardRefundingRemoval(int units, decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(units);
        return new(
            IrreversibleAction.CardRefundingRemoval,
            null,
            DisplayFormat.Number(units) + " ürün siparişten çıkarılacak ve " + DisplayFormat.Amount(amount) + " müşterinin kartına geri ödenecek.");
    }

    /// <summary>03 1.6.1.6: the return rejection.</summary>
    public static IrreversibleConfirmation ReturnRejection(int units)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(units);
        return new(
            IrreversibleAction.ReturnRejection,
            null,
            DisplayFormat.Number(units)
                + " adetlik iade reddedilecek; geri ödeme yapılmayacak, müşteriye bildirilecek ve mal firmanın bedeliyle müşterinin teslimat adresine geri gönderilecek.");
    }

    /// <summary>03 1.6.1.7: the record of a withdrawal notice that came from another channel; the record is not corrected.</summary>
    public static IrreversibleConfirmation OtherChannelWithdrawal(int units, DateOnly arrivalDate)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(units);
        return new(
            IrreversibleAction.OtherChannelRecord,
            null,
            DisplayFormat.Number(units) + " ürün için " + DisplayFormat.Date(arrivalDate) + " tarihli cayma beyanı kaydedilecek ve müşteriye bildirilecek; kayıt düzeltilemez.");
    }

    /// <summary>03 1.6.1.7: the record of a late delivery termination notice that came from another channel; the record is not corrected.</summary>
    public static IrreversibleConfirmation OtherChannelTermination(int units, DateOnly arrivalDate)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(units);
        return new(
            IrreversibleAction.OtherChannelRecord,
            null,
            "Teslim edilmeyen " + DisplayFormat.Number(units) + " ürün için " + DisplayFormat.Date(arrivalDate)
                + " tarihli gecikme feshi kaydedilecek ve müşteriye bildirilecek; kayıt düzeltilemez.");
    }

    /// <summary>03 1.6.1.8: the member account's deletion on request.</summary>
    public static IrreversibleConfirmation AccountDeletion() => new(
        IrreversibleAction.AccountDeletion,
        null,
        "Hesap ve hesaptaki ad, giriş bilgileri, adres defteri ve sepet hemen silinecek; siparişler sürer.");
}
