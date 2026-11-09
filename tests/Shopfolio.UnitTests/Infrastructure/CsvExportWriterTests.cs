using System.Text;

using Shopfolio.Infrastructure;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The one CSV writer of the three exports (T384; 09 10.10 items 1-4, 9; 02 10.7): the six cell kinds each have their own writer, only a text cell is made safe against a formula (and quoting comes
/// after), a negative amount stays a number, the file is UTF-8 with a BOM, <c>\r\n</c> line ends and the semicolon, a day is the Turkey day, an empty cell is <see langword="null"/>, and the size
/// ceiling stops the write. The writer works on a memory stream here: the unit tests touch no file (09 19.2 item 2).
/// </summary>
public sealed class CsvExportWriterTests
{
    private static readonly byte[] Bom = [0xEF, 0xBB, 0xBF];

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    // ---- the text cell: the formula rule, then the quoting ----

    [Theory]
    [InlineData("=1+1", "'=1+1")]
    [InlineData("+x", "'+x")]
    [InlineData("-x", "'-x")]
    [InlineData("@x", "'@x")]
    [InlineData("\tx", "'\tx")]
    [InlineData("=SUM(A1;A2)", "\"'=SUM(A1;A2)\"")]
    [InlineData("\rx", "\"'\rx\"")]
    [InlineData("a=1", "a=1")]
    [InlineData("Ali Veli", "Ali Veli")]
    public void ATextCell_StartingWithAFormulaCharacter_GetsASingleQuote_AndQuotingComesAfter(string value, string expected)
    {
        Assert.Equal(expected, CsvExportWriter.TextCell(value));
    }

    [Theory]
    [InlineData("a;b", "\"a;b\"")]
    [InlineData("say \"hi\"", "\"say \"\"hi\"\"\"")]
    [InlineData("two\nlines", "\"two\nlines\"")]
    [InlineData("two\r\nlines", "\"two\r\nlines\"")]
    [InlineData("a,b", "a,b")]
    public void ATextCell_WithASeparatorQuoteOrLineBreak_IsQuoted_AndTheQuotesAreDoubled(string value, string expected)
    {
        Assert.Equal(expected, CsvExportWriter.TextCell(value));
    }

    [Fact]
    public void ANullOrEmptyTextCell_IsEmpty()
    {
        Assert.Equal(string.Empty, CsvExportWriter.TextCell(null));
        Assert.Equal(string.Empty, CsvExportWriter.TextCell(string.Empty));
    }

    // ---- the numeric kinds: no formula prefix; the comma; no thousands separator ----

    [Theory]
    [InlineData("1250.00", "1250,00")]
    [InlineData("-12.50", "-12,50")]
    [InlineData("0", "0,00")]
    [InlineData("1234567.8", "1234567,80")]
    [InlineData("0.005", "0,01")]
    public void AnAmountCell_HasTwoDecimals_TheCommaAndNoThousandsSeparator_AndANegativeAmountKeepsItsMinus(string value, string expected)
    {
        CsvColumn<decimal> column = new("Tutar", CsvColumnKind.Amount, v => v);

        Assert.Equal(expected, CsvExportWriter.Cell(column, decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture)));
    }

    [Theory]
    [InlineData("20", "20")]
    [InlineData("20.00", "20")]
    [InlineData("7.50", "7,5")]
    [InlineData("7.55", "7,55")]
    [InlineData("0", "0")]
    public void ARateCell_HasNoTrailingZeros(string value, string expected)
    {
        CsvColumn<decimal> column = new("Oran", CsvColumnKind.Rate, v => v);

        Assert.Equal(expected, CsvExportWriter.Cell(column, decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void AQuantityCell_IsAWholeNumber()
    {
        CsvColumn<int> column = new("Adet", CsvColumnKind.Quantity, v => v);

        Assert.Equal("3", CsvExportWriter.Cell(column, 3));
        Assert.Equal("1200", CsvExportWriter.Cell(column, 1200));
    }

    // ---- the date kinds: the Turkey day, never the UTC day ----

    [Fact]
    public void ADateCell_IsTheTurkeyDay_AndAMomentCellIsTheTurkeyTime()
    {
        CsvColumn<DateTimeOffset> day = new("Gün", CsvColumnKind.Date, v => v);
        CsvColumn<DateTimeOffset> moment = new("An", CsvColumnKind.Moment, v => v);
        DateTimeOffset lateEvening = new(2026, 3, 1, 21, 0, 0, TimeSpan.Zero);
        DateTimeOffset beforeMidnight = new(2026, 3, 1, 20, 59, 0, TimeSpan.Zero);

        Assert.Equal("2026-03-02", CsvExportWriter.Cell(day, lateEvening));
        Assert.Equal("2026-03-01", CsvExportWriter.Cell(day, beforeMidnight));
        Assert.Equal("2026-03-02 00:00", CsvExportWriter.Cell(moment, lateEvening));
        Assert.Equal("2026-03-01 23:59", CsvExportWriter.Cell(moment, beforeMidnight));
        Assert.Equal("2026-03-02", CsvExportWriter.Cell(day, new DateTimeOffset(2026, 3, 2, 0, 30, 0, TimeSpan.FromHours(3))));
    }

    // ---- an empty value is an empty cell in every kind; a value of another type is an unexpected error ----

    [Theory]
    [InlineData(nameof(CsvColumnKind.Text))]
    [InlineData(nameof(CsvColumnKind.Amount))]
    [InlineData(nameof(CsvColumnKind.Quantity))]
    [InlineData(nameof(CsvColumnKind.Rate))]
    [InlineData(nameof(CsvColumnKind.Date))]
    [InlineData(nameof(CsvColumnKind.Moment))]
    public void ANullValue_IsAnEmptyCell_InEveryKind(string kindName)
    {
        CsvColumn<object?> column = new("Sütun", Enum.Parse<CsvColumnKind>(kindName), v => v);

        Assert.Equal(string.Empty, CsvExportWriter.Cell(column, null));
    }

    [Theory]
    [InlineData(nameof(CsvColumnKind.Text), 5)]
    [InlineData(nameof(CsvColumnKind.Amount), 5)]
    [InlineData(nameof(CsvColumnKind.Quantity), 5.5)]
    [InlineData(nameof(CsvColumnKind.Rate), 5)]
    [InlineData(nameof(CsvColumnKind.Date), "2026-01-01")]
    [InlineData(nameof(CsvColumnKind.Moment), "2026-01-01")]
    public void AValueOfAnotherType_IsAnUnexpectedError_InEveryKind(string kindName, object value)
    {
        CsvColumn<object?> column = new("Sütun", Enum.Parse<CsvColumnKind>(kindName), v => v);

        Assert.Throws<InvalidOperationException>(() => CsvExportWriter.Cell(column, value));
    }

    // ---- the file: the BOM, the line ends, the separator, the header, the count ----

    [Fact]
    public async Task TheFile_HasTheBom_TheHeaderThenTheRows_WithCrLfAndTheSemicolon_AndTheStreamStaysOpen()
    {
        IReadOnlyList<CsvColumn<(string Name, decimal Amount)>> columns =
        [
            new("Ad", CsvColumnKind.Text, r => r.Name),
            new("Tutar", CsvColumnKind.Amount, r => r.Amount),
        ];
        using MemoryStream stream = new();

        int count = await CsvExportWriter.WriteAsync(
            stream, columns, Rows(("Ali", 12.5m), ("=cmd;x", -3m)), ExportCeilings.Default, Ct);

        byte[] bytes = stream.ToArray();
        Assert.Equal(2, count);
        Assert.Equal(Bom, bytes[..3]);
        Assert.Equal("Ad;Tutar\r\nAli;12,50\r\n\"'=cmd;x\";-3,00\r\n", new UTF8Encoding(false).GetString(bytes[3..]));
        Assert.True(stream.CanWrite);
    }

    [Fact]
    public async Task AnEmptySequence_WritesTheHeaderOnly_AndCountsZero()
    {
        using MemoryStream stream = new();

        int count = await CsvExportWriter.WriteAsync(stream, [new CsvColumn<string>("Ad", CsvColumnKind.Text, r => r)], Rows<string>(), ExportCeilings.Default, Ct);

        Assert.Equal(0, count);
        Assert.Equal("Ad\r\n", new UTF8Encoding(false).GetString(stream.ToArray()[3..]));
    }

    [Fact]
    public async Task AnAccentedTextCell_IsWrittenAsUtf8()
    {
        using MemoryStream stream = new();

        await CsvExportWriter.WriteAsync(stream, [new CsvColumn<string>("Ad", CsvColumnKind.Text, r => r)], Rows("Şükrü Ğüler İ"), ExportCeilings.Default, Ct);

        Assert.Contains("Şükrü Ğüler İ", Encoding.UTF8.GetString(stream.ToArray()), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ASetLargerThanOnePage_IsWrittenRowByRow_FromTheSequence()
    {
        int produced = 0;
        int producedWhenFirstRowWasWritten = -1;
        using MemoryStream stream = new();
        CsvColumn<int> column = new("No", CsvColumnKind.Quantity, r => r);

        async IAsyncEnumerable<int> Source()
        {
            for (int i = 1; i <= ExportLimits.PageSize * 3; i++)
            {
                produced++;
                yield return i;
                await Task.Yield();
            }
        }

        int count = await CsvExportWriter.WriteAsync(stream, [column with { Value = r => { if (producedWhenFirstRowWasWritten < 0) { producedWhenFirstRowWasWritten = produced; } return r; } }], Source(), ExportCeilings.Default, Ct);

        Assert.Equal(ExportLimits.PageSize * 3, count);
        Assert.Equal(1, producedWhenFirstRowWasWritten);
    }

    // ---- the size ceiling: counted from the bytes written, row by row ----

    [Fact]
    public async Task AFilePastTheSizeCeiling_StopsTheWrite_WithTheLimitException()
    {
        using MemoryStream stream = new();
        CsvColumn<string> column = new("Ad", CsvColumnKind.Text, r => r);
        long ceiling = Bom.Length + "Ad\r\n".Length + "bir\r\n".Length;

        await Assert.ThrowsAsync<ExportLimitExceededException>(
            () => CsvExportWriter.WriteAsync(stream, [column], Rows("bir", "iki", "üç"), new ExportCeilings(ceiling), Ct));
    }

    [Fact]
    public async Task AFileExactlyAtTheSizeCeiling_IsWritten_AndOneByteLess_IsNot()
    {
        CsvColumn<string> column = new("Ad", CsvColumnKind.Text, r => r);
        long exact = Bom.Length + "Ad\r\n".Length + "bir\r\n".Length;
        using MemoryStream fits = new();

        int count = await CsvExportWriter.WriteAsync(fits, [column], Rows("bir"), new ExportCeilings(exact), Ct);

        Assert.Equal(1, count);
        Assert.Equal(exact, fits.Length);
        using MemoryStream tooBig = new();
        await Assert.ThrowsAsync<ExportLimitExceededException>(() => CsvExportWriter.WriteAsync(tooBig, [column], Rows("bir"), new ExportCeilings(exact - 1), Ct));
    }

    [Fact]
    public async Task ACancelledToken_StopsTheWrite_WithTheCancellation_NotTheLimitException()
    {
        using CancellationTokenSource cancelled = new();
        await cancelled.CancelAsync();
        using MemoryStream stream = new();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => CsvExportWriter.WriteAsync(stream, [new CsvColumn<string>("Ad", CsvColumnKind.Text, r => r)], Rows("bir"), ExportCeilings.Default, cancelled.Token));
    }

    // ---- the ceilings of the product: one set of values, ordered (09 3.5 item 4; 09 10.7) ----

    [Fact]
    public void TheCeilings_AreTheDocumentedValues_AndThePageQueryIsInsideTheProductionCeilingInsideTheProxysWait()
    {
        Assert.Equal(TimeSpan.FromSeconds(120), ExportLimits.MaxProductionTime);
        Assert.Equal(100L * 1024 * 1024, ExportLimits.MaxFileBytes);
        Assert.Equal(500, ExportLimits.PageSize);
        Assert.Equal(TimeSpan.FromHours(1), ExportLimits.SweepAge);
        Assert.Equal(ExportLimits.MaxFileBytes, ExportCeilings.Default.MaxBytes);

        Assert.True(DatabaseTimeouts.StatementTimeout < ExportLimits.MaxProductionTime);
        Assert.True(ExportLimits.MaxProductionTime < ExportLimits.ReverseProxyHeaderWait);
    }

    private static async IAsyncEnumerable<T> Rows<T>(params T[] rows)
    {
        foreach (T row in rows)
        {
            await Task.Yield();
            yield return row;
        }
    }
}
