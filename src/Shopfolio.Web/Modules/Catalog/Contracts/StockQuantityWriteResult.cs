using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// The outcome of the panel's write of a variant's stock or capacity field (05 7.3.1, 7.3.3; 06 2.6.5; K-980): written; refused because the
/// field no longer holds the value the form showed (the message of <c>04</c> 2.10.1); or refused because the new value is below the reserved
/// quantity (the refusal of <c>02 §3.6.7</c>). The refusals' error codes are the error code registry's (<c>07 §6.2</c>).
/// </summary>
public sealed class StockQuantityWriteResult : IOperationResult
{
    private StockQuantityWriteResult(bool isWritten, bool isShownValueChanged)
    {
        IsWritten = isWritten;
        IsShownValueChanged = isShownValueChanged;
    }

    public static StockQuantityWriteResult Written { get; } = new(true, false);

    public static StockQuantityWriteResult ShownValueChanged { get; } = new(false, true);

    public static StockQuantityWriteResult BelowReserved { get; } = new(false, false);

    public bool IsWritten { get; }

    public bool IsShownValueChanged { get; }

    public bool IsRejected => !IsWritten;
}
