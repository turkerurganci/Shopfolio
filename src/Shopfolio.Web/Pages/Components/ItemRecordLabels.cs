namespace Shopfolio.Pages.Components;

/// <summary>The ten item records the order item's row names (04 2.5.2; 02 5.8; 03 1.7.1). The item has no status badge of its own (K-179): its records are its path.</summary>
public enum ItemRecordName
{
    Cancellation,
    ItemRemoval,
    LateDeliveryTermination,
    Withdrawal,
    ReturnReceipt,
    ReturnRejection,
    NotReturnedClosure,
    RefundIbanRequest,
    DefectClaim,
    DeliveryMark,
}

/// <summary>One record on an item's row: its name, the units it covers when it covers units, and the moment it was born (04 2.5.2; 03 1.7.1).</summary>
public sealed record ItemRecordEntry(ItemRecordName Name, int? Quantity, DateTimeOffset? At)
{
    /// <summary>The moment as the row writes it, or null when the record carries no moment (the view only prints it; it derives nothing from the date).</summary>
    public string? MomentText => At is DateTimeOffset moment ? Infrastructure.DisplayFormat.Date(moment) : null;
}

/// <summary>
/// The names an item record shows with on the item's row (04 2.5.2): the record's own name, in the order the document lists them. The row is the place of the record, so no badge
/// is drawn: <see cref="StatusBadges"/> has no item family.
/// </summary>
public static class ItemRecordLabels
{
    /// <summary>The ten names in the document's order.</summary>
    public static IReadOnlyList<ItemRecordName> All { get; } = Enum.GetValues<ItemRecordName>();

    /// <summary>The record's name as the item's row writes it.</summary>
    public static string Of(ItemRecordName name) => name switch
    {
        ItemRecordName.Cancellation => "İptal kaydı",
        ItemRecordName.ItemRemoval => "Çıkarma kaydı",
        ItemRecordName.LateDeliveryTermination => "Gecikme feshi",
        ItemRecordName.Withdrawal => "Cayma beyanı",
        ItemRecordName.ReturnReceipt => "İade teslim alma",
        ItemRecordName.ReturnRejection => "İade reddi",
        ItemRecordName.NotReturnedClosure => "\"Mal dönmedi\" kapanışı",
        ItemRecordName.RefundIbanRequest => "IBAN isteği",
        ItemRecordName.DefectClaim => "Ayıp talebi",
        ItemRecordName.DeliveryMark => "Teslim işareti",
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Not an item record."),
    };
}
