using System.Globalization;
using System.Text;

namespace Shopfolio.Infrastructure;

/// <summary>The six kinds of a CSV cell (09 10.10 item 1): each has its own writer; the kind is the definition's, never the code that makes the value.</summary>
internal enum CsvColumnKind
{
    Text,
    Amount,
    Quantity,
    Rate,
    Date,
    Moment,
}

/// <summary>One column of an export file (09 10.10 item 1): the Turkish header, the kind of the cell and the value of a row (<see langword="null"/> is an empty cell).</summary>
internal sealed record CsvColumn<TRow>(string Header, CsvColumnKind Kind, Func<TRow, object?> Value);

/// <summary>The production ceilings of one export (09 3.5 item 4; 09 10.10 item 7): the size of the file. The time ceiling is the caller's cancellation source.</summary>
internal sealed record ExportCeilings(long MaxBytes)
{
    /// <summary>The ceiling of the product: 100 MB and 120 seconds, the page of 500 orders (K-1321 (4), (8)).</summary>
    public static ExportCeilings Default { get; } = new(ExportLimits.MaxFileBytes);
}

/// <summary>The values of 09 3.5 item 4 for the export file; the constants of the product, not settings (K-1321).</summary>
internal static class ExportLimits
{
    /// <summary>The longest the production of one file may take.</summary>
    public static readonly TimeSpan MaxProductionTime = TimeSpan.FromSeconds(120);

    /// <summary>The largest file: 100 MB.</summary>
    public const long MaxFileBytes = 100L * 1024 * 1024;

    /// <summary>The page of the order read: the most orders one query takes.</summary>
    public const int PageSize = 500;

    /// <summary>The age from which an earlier file in the directory is swept by the next request.</summary>
    public static readonly TimeSpan SweepAge = TimeSpan.FromHours(1);

    /// <summary>How long the reverse proxy waits for the response header on the export paths; the proxy's own configuration, kept here so the test can compare the three (09 3.5 item 4).</summary>
    public static readonly TimeSpan ReverseProxyHeaderWait = TimeSpan.FromSeconds(150);
}

/// <summary>The export grew past its size or time ceiling; the file is dropped, no trail row is written and the answer is the unexpected-operation error (09 10.10 item 7).</summary>
internal sealed class ExportLimitExceededException : Exception
{
    public ExportLimitExceededException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// The one writer of the three export files (09 10.10; 02 10.7): a UTF-8 file with a BOM, <c>\r\n</c> line ends and the semicolon as the separator, written row by row from an
/// asynchronous sequence - the set is never held whole. Every cell goes through the writer of its kind; only a text cell is made safe against a formula (a cell that starts with
/// <c>=</c>, <c>+</c>, <c>-</c>, <c>@</c>, a tab or a CR gets a single quote in front, and quoting comes after), so a negative amount stays a number. It knows no module.
/// </summary>
internal static class CsvExportWriter
{
    private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

    private static readonly System.Buffers.SearchValues<char> QuotingCharacters = System.Buffers.SearchValues.Create(";\"\r\n");

    /// <summary>
    /// Writes the header and every row to <paramref name="destination"/> and leaves the stream open; returns the number of rows. The size ceiling is checked after every row, counted from the bytes
    /// written. Whatever the cancellation token reports is the caller's to read (the time ceiling and a client that left are told apart there).
    /// </summary>
    public static async Task<int> WriteAsync<TRow>(
        Stream destination,
        IReadOnlyList<CsvColumn<TRow>> columns,
        IAsyncEnumerable<TRow> rows,
        ExportCeilings ceilings,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(columns);
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(ceilings);

        long written = Utf8WithBom.GetPreamble().Length;
        int count = 0;
        StreamWriter writer = new(destination, Utf8WithBom, bufferSize: 16 * 1024, leaveOpen: true) { NewLine = "\r\n" };
        await using (writer.ConfigureAwait(false))
        {
            written += await WriteLineAsync(writer, string.Join(';', columns.Select(c => TextCell(c.Header))), ct).ConfigureAwait(false);
            await foreach (TRow row in rows.WithCancellation(ct).ConfigureAwait(false))
            {
                written += await WriteLineAsync(writer, string.Join(';', columns.Select(c => Cell(c, row))), ct).ConfigureAwait(false);
                count++;
                if (written > ceilings.MaxBytes)
                {
                    throw new ExportLimitExceededException("The export file grew past its size ceiling.");
                }
            }

            await writer.FlushAsync(ct).ConfigureAwait(false);
        }

        return count;
    }

    /// <summary>One cell of the kind the column declares (09 10.10 item 2, 3); a value of another type is an unexpected error.</summary>
    internal static string Cell<TRow>(CsvColumn<TRow> column, TRow row)
    {
        object? value = column.Value(row);
        if (value is null)
        {
            return string.Empty;
        }

        return column.Kind switch
        {
            CsvColumnKind.Text => value is string text ? TextCell(text) : throw WrongType(column, value),
            CsvColumnKind.Amount => value is decimal amount ? AmountCell(amount) : throw WrongType(column, value),
            CsvColumnKind.Rate => value is decimal rate ? RateCell(rate) : throw WrongType(column, value),
            CsvColumnKind.Quantity => value is int quantity ? quantity.ToString(CultureInfo.InvariantCulture) : throw WrongType(column, value),
            CsvColumnKind.Date => value is DateTimeOffset day ? TurkeyTime.DateOf(day).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : throw WrongType(column, value),
            CsvColumnKind.Moment => value is DateTimeOffset moment ? moment.ToOffset(TurkeyTime.Offset).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) : throw WrongType(column, value),
            _ => throw new InvalidOperationException("Unknown cell kind."),
        };
    }

    /// <summary>Two decimals, the comma as the separator, no thousands separator (<c>1250,00</c>; <c>-12,50</c>).</summary>
    internal static string AmountCell(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',');

    /// <summary>A percentage without trailing zeros (<c>20</c>, <c>7,5</c>).</summary>
    internal static string RateCell(decimal value) => value.ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ',');

    /// <summary>A text cell: made safe against a formula first, quoted after.</summary>
    internal static string TextCell(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value[0] is '=' or '+' or '-' or '@' or '\t' or '\r')
        {
            value = "'" + value;
        }

        return NeedsQuoting(value) ? "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"" : value;
    }

    private static bool NeedsQuoting(string value) => value.AsSpan().IndexOfAny(QuotingCharacters) >= 0;

    private static async Task<long> WriteLineAsync(StreamWriter writer, string line, CancellationToken ct)
    {
        await writer.WriteAsync((line + "\r\n").AsMemory(), ct).ConfigureAwait(false);
        return Utf8WithBom.GetByteCount(line) + 2;
    }

    private static InvalidOperationException WrongType<TRow>(CsvColumn<TRow> column, object value)
        => new($"The column '{column.Header}' is of kind {column.Kind} and got a {value.GetType().Name}.");
}
