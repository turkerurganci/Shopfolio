using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Settings.Contracts;

/// <summary>
/// The outcome of closing sales temporarily or opening them again (02 3.1.6; 07 9.16.3, 9.16.4): written, or not written because the switch is no longer the value the
/// screen showed or is already in the asked state - then the page answers with 303 and the current state, not 409 (<c>ErrorCode.SalesSwitchChanged</c>; K-1194). Either way the
/// result carries the gate as it is after the call, so the page can say whether reopening the switch really opened sales (02 3.1.6: the switch alone never opens them).
/// </summary>
public sealed class SalesSwitchResult : IOperationResult
{
    private SalesSwitchResult(bool isWritten, bool isClosedNow, SalesGate gate)
    {
        IsWritten = isWritten;
        IsClosedNow = isClosedNow;
        Gate = gate;
    }

    /// <summary>True when the switch was written; its trail row is in the caller's transaction.</summary>
    public bool IsWritten { get; }

    /// <summary>True when the switch is on (sales closed temporarily) after the call.</summary>
    public bool IsClosedNow { get; }

    /// <summary>The gate after the call: for a reopening, whether sales are open or which conditions still hold them closed.</summary>
    public SalesGate Gate { get; }

    /// <summary>The switch changed meanwhile or is already in the asked state: nothing was written (<c>SalesSwitchChanged</c>).</summary>
    public bool IsSwitchChanged => !IsWritten;

    public bool IsRejected => !IsWritten;

    internal static SalesSwitchResult Written(bool isClosedNow, SalesGate gate) => new(true, isClosedNow, gate);

    internal static SalesSwitchResult Changed(bool isClosedNow, SalesGate gate) => new(false, isClosedNow, gate);
}
