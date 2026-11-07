using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// The outcome of the panel's write of a variant's stock or capacity field (05 7.3.1, 7.3.3; 06 2.6.5; K-980; 02 3.6.2, 3.6.7): written; refused because the field no longer holds
/// the value the form showed (the message of <c>04</c> 2.10.1); refused because the new value is below the reserved quantity (the refusal of <c>02 §3.6.7</c>, which carries
/// the reserved quantity at the moment of the refusal); refused because a physical product's variant needs a number (<c>FieldRequired</c> of <c>07 §6.2</c>); or refused because a
/// digital product's variant has no stock field. The refusal's error codes are the error code registry's (<c>07 §6.2</c>).
/// </summary>
public sealed class StockQuantityWriteResult : IOperationResult
{
    private StockQuantityWriteResult(bool isWritten, bool isShownValueChanged, bool isBelowReserved, bool isStockRequired, bool isNotTracked, int? reservedQuantity)
    {
        IsWritten = isWritten;
        IsShownValueChanged = isShownValueChanged;
        IsBelowReserved = isBelowReserved;
        IsStockRequired = isStockRequired;
        IsNotTracked = isNotTracked;
        ReservedQuantity = reservedQuantity;
    }

    public static StockQuantityWriteResult Written { get; } = new(true, false, false, false, false, null);

    public static StockQuantityWriteResult ShownValueChanged { get; } = new(false, true, false, false, false, null);

    /// <summary>The refusal below the reserved quantity, without the quantity (the writer that has none to give); <see cref="BelowReservedBy"/> carries it.</summary>
    public static StockQuantityWriteResult BelowReserved { get; } = new(false, false, true, false, false, null);

    /// <summary>A physical product's variant was given no number (02 3.6.2): the number is required.</summary>
    public static StockQuantityWriteResult StockRequired { get; } = new(false, false, false, true, false, null);

    /// <summary>A digital product's variant was given a number (02 3.6.2): it has no stock field and is sold without limit.</summary>
    public static StockQuantityWriteResult NotTracked { get; } = new(false, false, false, false, true, null);

    public bool IsWritten { get; }

    public bool IsShownValueChanged { get; }

    public bool IsBelowReserved { get; }

    public bool IsStockRequired { get; }

    public bool IsNotTracked { get; }

    /// <summary>On a refusal below the reserved quantity: the units reserved when it was refused (06 2.7.1); otherwise null.</summary>
    public int? ReservedQuantity { get; }

    public bool IsRejected => !IsWritten;

    /// <summary>The refusal below the reserved quantity, with the reserved quantity the refusal tells (02 3.6.7).</summary>
    public static StockQuantityWriteResult BelowReservedBy(int reservedQuantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(reservedQuantity);
        return new StockQuantityWriteResult(false, false, true, false, false, reservedQuantity);
    }
}
