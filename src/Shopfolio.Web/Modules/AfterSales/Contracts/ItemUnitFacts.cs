namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>
/// One order item's units by its records (06 2.7.2, 4.14; K-1104): <paramref name="Closed"/> - cancelled, removed, terminated or withdrawn;
/// <paramref name="Withdrawn"/> - the withdrawal records' sum; <paramref name="Received"/> - the withdrawn goods the firm received back;
/// <paramref name="Rejected"/> - the withdrawn goods whose return was rejected; <paramref name="NotReturned"/> - the withdrawn goods closed as not
/// come back and not reopened.
/// </summary>
public sealed record ItemUnitFacts(int Closed, int Withdrawn, int Received, int Rejected, int NotReturned)
{
    /// <summary>The withdrawn units that neither came back, nor were rejected, nor were closed as not come back: the physical withdrawal is still open on them.</summary>
    public int AwaitingReceipt => ReturnUnits.AwaitingReceipt(Withdrawn, Received, Rejected, NotReturned);
}

/// <summary>
/// The one "units on their way back" formula (03 1.7.3.5; K-794; D-568): the declared units minus those received, rejected or closed as not come back (a
/// reopened closure no longer counts), never below zero. A withdrawal declaration's value and an item's value (the sums of its declarations) both come from here.
/// </summary>
public static class ReturnUnits
{
    /// <summary>The withdrawn units still on their way back.</summary>
    public static int AwaitingReceipt(int declared, int received, int rejected, int notReturned) => Math.Max(0, declared - received - rejected - notReturned);
}
