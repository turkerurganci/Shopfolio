namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>How an export request ended (07 9.24.2-9.24.4): the file is made, the range of the order export is wrong, or there is nothing to write.</summary>
public enum ExportOutcome
{
    /// <summary>The file is made and the trail row is written; <see cref="ExportFileResult.File"/> is the stream to send.</summary>
    Done,

    /// <summary>The order export's date range is wrong (<see cref="ExportFileResult.RangeError"/>); no file and no trail row.</summary>
    RangeInvalid,

    /// <summary>The set to write is empty (07 9.24.2-9.24.4: 422, <c>ExportEmpty</c>): no file stays and no trail row is written; the section says so in place.</summary>
    Empty,
}

/// <summary>The fault of the order export's date range (04 9.24.8): the fields are <c>StartDate</c> and <c>EndDate</c> (04 7.2.25).</summary>
public enum ExportRangeError
{
    None,
    StartMissing,
    EndMissing,
    StartInFuture,
    EndInFuture,
    EndBeforeStart,
}

/// <summary>
/// The answer of an export (T384; 07 9.24.2-9.24.4). On <see cref="ExportOutcome.Done"/> it holds the open file stream - positioned at the start, deleted when it closes - and the download
/// name; the caller (the panel's handler) sends it as the response and ties it to the request's life with <c>Response.RegisterForDisposeAsync</c>, never with <c>await using</c> (09 10.10
/// item 7). <see cref="Rows"/> is the number of lines the file holds, the header not counted.
/// </summary>
public sealed class ExportFileResult
{
    private ExportFileResult(ExportOutcome outcome, FileStream? file, string? fileName, int rows, ExportRangeError rangeError)
    {
        Outcome = outcome;
        File = file;
        FileName = fileName;
        Rows = rows;
        RangeError = rangeError;
    }

    public ExportOutcome Outcome { get; }

    /// <summary>The file, positioned at its start; null unless <see cref="Outcome"/> is <see cref="ExportOutcome.Done"/>.</summary>
    public FileStream? File { get; }

    /// <summary>The download name (07 9.24.2): <c>siparisler-{start}-{end}.csv</c>, <c>uyeler-{day}.csv</c> or <c>iletisim-talepleri-{day}.csv</c>.</summary>
    public string? FileName { get; }

    public int Rows { get; }

    public ExportRangeError RangeError { get; }

    /// <summary>The content type of the file (07 9.24.2).</summary>
    public static string ContentType => "text/csv; charset=utf-8";

    internal static ExportFileResult Done(FileStream file, string fileName, int rows) => new(ExportOutcome.Done, file, fileName, rows, ExportRangeError.None);

    internal static ExportFileResult Invalid(ExportRangeError error) => new(ExportOutcome.RangeInvalid, null, null, 0, error);

    internal static ExportFileResult Empty() => new(ExportOutcome.Empty, null, null, 0, ExportRangeError.None);
}

/// <summary>
/// The three exports of the panel (T384; 02 10.7.1, 10.7.4; 07 UC-182 to UC-184; 09 10.10): the file is made in the temporary directory outside the backup, the trail row of the
/// <c>Export</c> operation is written after the file is made and before it is sent, and a file that cannot be made - past its size or time ceiling - leaves no row. Each call is an entry
/// point's work, like a page handler's: it opens its own transaction for the trail row. A request that ends in a file, an empty set or a wrong range writes nothing else. There is no
/// export of the catalog, the content, the trail or the sales summary and no import (02 10.7.2, 10.7.3).
/// </summary>
public interface IDataExport
{
    /// <summary>
    /// The orders created in the Turkey day range <paramref name="startDay"/> to <paramref name="endDay"/>, both included, whose payment was completed. A missing, future or reversed range is
    /// <see cref="ExportOutcome.RangeInvalid"/>; no order of that set in the range is <see cref="ExportOutcome.Empty"/>. Throws when the file passes its size or time ceiling: the file is gone and no row is written.
    /// </summary>
    Task<ExportFileResult> ExportOrdersAsync(DateOnly? startDay, DateOnly? endDay, CancellationToken ct);

    /// <summary>The name and e-mail of every member account, with no range. <see cref="ExportOutcome.Empty"/> when there is no account.</summary>
    Task<ExportFileResult> ExportCustomersAsync(CancellationToken ct);

    /// <summary>The name and e-mail of every contact request still kept, with no range. <see cref="ExportOutcome.Empty"/> when none is kept.</summary>
    Task<ExportFileResult> ExportContactRequestsAsync(CancellationToken ct);
}
