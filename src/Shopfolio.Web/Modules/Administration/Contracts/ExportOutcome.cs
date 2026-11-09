namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>How an export request ended (07 9.24.2-9.24.4): the file is made, the range of the order export is wrong, or there is nothing to write; a result code, never stored.</summary>
public enum ExportOutcome
{
    /// <summary>The file is made and the trail row is written; <see cref="ExportFileResult.File"/> is the stream to send.</summary>
    Done,

    /// <summary>The order export's date range is wrong (<see cref="ExportFileResult.RangeError"/>); no file and no trail row.</summary>
    RangeInvalid,

    /// <summary>The set to write is empty (07 9.24.2-9.24.4: 422, <c>ExportEmpty</c>): no file stays and no trail row is written; the section says so in place.</summary>
    Empty,
}
