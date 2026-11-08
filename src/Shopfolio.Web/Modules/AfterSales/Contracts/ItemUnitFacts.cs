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
    public int AwaitingReceipt => Withdrawn - Received - Rejected - NotReturned;
}
