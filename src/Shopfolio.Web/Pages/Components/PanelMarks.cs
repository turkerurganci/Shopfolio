namespace Shopfolio.Pages.Components;

/// <summary>The seven panel marks (04 2.5.3; 03 1.7.3). A mark is not a status: it shows while its condition lasts and goes when the condition goes.</summary>
public enum PanelMark
{
    EmailNotDelivered,
    PaymentResultMissing,
    RefundNotCompleted,
    IbanPending,
    ReturnGoodsPending,
    EmailCorrected,
    RefundResultPending,
}

/// <summary>Where a mark list is drawn: the order list shows only the marks 04 2.5.3 gives a place there; the order's page shows them all.</summary>
public enum PanelMarkPlace
{
    OrderList,
    OrderDetail,
}

/// <summary>Which of the seven conditions hold now. They are read from the records at every drawing and never stored, so a mark cannot outlive its condition (04 2.5.3).</summary>
public sealed record PanelMarkConditions(
    bool EmailNotDelivered = false,
    bool PaymentResultMissing = false,
    bool RefundNotCompleted = false,
    bool IbanPending = false,
    bool ReturnGoodsPending = false,
    bool EmailCorrected = false,
    bool RefundResultPending = false);

/// <summary>
/// The panel marks of an order (T542; 04 2.5.3): the text of each (a mark carries text, never a colour alone) and the marks that hold now, in the document's order. The order list has no
/// "IBAN bekleniyor" and no "iade malı bekleniyor" - those two live on the item (04 2.5.3.4, 2.5.3.5).
/// </summary>
public static class PanelMarks
{
    /// <summary>The mark's text as the screen writes it (04 2.5.3).</summary>
    public static string TextOf(PanelMark mark) => mark switch
    {
        PanelMark.EmailNotDelivered => "E-posta ulaşmadı",
        PanelMark.PaymentResultMissing => "Ödeme sonucu alınamadı",
        PanelMark.RefundNotCompleted => "Geri ödeme gerçekleşmedi",
        PanelMark.IbanPending => "IBAN bekleniyor",
        PanelMark.ReturnGoodsPending => "İade malı bekleniyor",
        PanelMark.EmailCorrected => "E-posta düzeltildi",
        PanelMark.RefundResultPending => "Geri ödeme sonucu bekleniyor",
        _ => throw new ArgumentOutOfRangeException(nameof(mark), mark, "Not a panel mark."),
    };

    /// <summary>Whether the mark has a place in <paramref name="place"/> (04 2.5.3 columns "Sipariş listesinde" and "Sipariş ayrıntısında").</summary>
    public static bool IsShownIn(PanelMark mark, PanelMarkPlace place) =>
        place == PanelMarkPlace.OrderDetail || mark is not (PanelMark.IbanPending or PanelMark.ReturnGoodsPending);

    /// <summary>The marks whose condition holds and that have a place in <paramref name="place"/>, in the document's order; none holding gives an empty list and the screen draws nothing.</summary>
    public static IReadOnlyList<PanelMark> Of(PanelMarkConditions conditions, PanelMarkPlace place)
    {
        ArgumentNullException.ThrowIfNull(conditions);
        (PanelMark Mark, bool Holds)[] all =
        [
            (PanelMark.EmailNotDelivered, conditions.EmailNotDelivered),
            (PanelMark.PaymentResultMissing, conditions.PaymentResultMissing),
            (PanelMark.RefundNotCompleted, conditions.RefundNotCompleted),
            (PanelMark.IbanPending, conditions.IbanPending),
            (PanelMark.ReturnGoodsPending, conditions.ReturnGoodsPending),
            (PanelMark.EmailCorrected, conditions.EmailCorrected),
            (PanelMark.RefundResultPending, conditions.RefundResultPending),
        ];
        return [.. all.Where(a => a.Holds && IsShownIn(a.Mark, place)).Select(a => a.Mark)];
    }
}
