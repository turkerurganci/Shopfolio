namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>The fault of the order export's date range (04 9.24.8): the fields are <c>StartDate</c> and <c>EndDate</c> (04 7.2.25); a result code, never stored.</summary>
public enum ExportRangeError
{
    None,
    StartMissing,
    EndMissing,
    StartInFuture,
    EndInFuture,
    EndBeforeStart,
}
