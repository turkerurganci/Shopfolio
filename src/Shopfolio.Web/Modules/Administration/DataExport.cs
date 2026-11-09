using System.Globalization;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// The three exports (T384; 02 10.7.1, 10.7.4; 07 9.24; 09 10.10 items 6, 7): the file is made through the one writer in the directory <see cref="ExportTempDirectory"/> opens, the trail row
/// (<c>Export</c>, with the kind and the range in its details, never a kind of record) is written after the file is made and before it is sent, and the file is deleted when its
/// stream closes. The time ceiling is a cancellation source of its own, so a client that left is told apart from a file that took too long; both end the production, and only the ceiling
/// is an <see cref="ExportLimitExceededException"/> (the unexpected-operation error). A file that cannot be made, an empty set and a wrong range write no row.
/// </summary>
internal sealed class DataExport : IDataExport
{
    private readonly IOrderingModule _orders;
    private readonly IMembershipModule _members;
    private readonly ICorporateContentModule _contactRequests;
    private readonly IAuditTrail _trail;
    private readonly ITransactionRunner _transactions;
    private readonly ExportTempDirectory _directory;
    private readonly TimeProvider _time;

    public DataExport(
        IOrderingModule orders,
        IMembershipModule members,
        ICorporateContentModule contactRequests,
        IAuditTrail trail,
        ITransactionRunner transactions,
        ExportTempDirectory directory,
        TimeProvider time)
    {
        _orders = orders;
        _members = members;
        _contactRequests = contactRequests;
        _trail = trail;
        _transactions = transactions;
        _directory = directory;
        _time = time;
    }

    public async Task<ExportFileResult> ExportOrdersAsync(DateOnly? startDay, DateOnly? endDay, CancellationToken ct)
    {
        ExportRangeError error = CheckRange(startDay, endDay, TurkeyTime.DateOf(_time.GetUtcNow()));
        if (error != ExportRangeError.None)
        {
            return ExportFileResult.Invalid(error);
        }

        DateOnly start = startDay!.Value;
        DateOnly end = endDay!.Value;
        return await ProduceAsync(
            AuditOperation.ExportOrders,
            [new AuditValue("exportType", "Orders"), new AuditValue("startDate", Day(start)), new AuditValue("endDate", Day(end))],
            $"siparisler-{Day(start)}-{Day(end)}.csv",
            (stream, max, token) => _orders.WriteOrderExportAsync(start, end, stream, max, token),
            ct);
    }

    public Task<ExportFileResult> ExportCustomersAsync(CancellationToken ct) => ProduceAsync(
        AuditOperation.ExportCustomers,
        [new AuditValue("exportType", "Customers")],
        $"uyeler-{Day(TurkeyTime.DateOf(_time.GetUtcNow()))}.csv",
        _members.WriteCustomerExportAsync,
        ct);

    public Task<ExportFileResult> ExportContactRequestsAsync(CancellationToken ct) => ProduceAsync(
        AuditOperation.ExportContactRequests,
        [new AuditValue("exportType", "ContactRequests")],
        $"iletisim-talepleri-{Day(TurkeyTime.DateOf(_time.GetUtcNow()))}.csv",
        _contactRequests.WriteContactRequestExportAsync,
        ct);

    /// <summary>The range's faults, the first by the order of 04 9.24.8: both days are required, neither may be a day to come, and the end may not be before the start.</summary>
    internal static ExportRangeError CheckRange(DateOnly? startDay, DateOnly? endDay, DateOnly today)
    {
        if (startDay is null)
        {
            return ExportRangeError.StartMissing;
        }

        if (endDay is null)
        {
            return ExportRangeError.EndMissing;
        }

        if (startDay.Value > today)
        {
            return ExportRangeError.StartInFuture;
        }

        if (endDay.Value > today)
        {
            return ExportRangeError.EndInFuture;
        }

        return endDay.Value < startDay.Value ? ExportRangeError.EndBeforeStart : ExportRangeError.None;
    }

    private async Task<ExportFileResult> ProduceAsync(
        AuditOperation operation,
        IReadOnlyList<AuditValue> details,
        string fileName,
        Func<Stream, long, CancellationToken, Task<int>> write,
        CancellationToken ct)
    {
        _directory.SweepOld();
        FileStream file = _directory.OpenNew();
        bool handedOver = false;
        try
        {
            int rows = await WriteWithinCeilingsAsync(file, write, ct);
            if (rows == 0)
            {
                return ExportFileResult.Empty();
            }

            // The trail row after the file is made and before it is sent (07 9.24.2): the row is written in its own transaction, and a file that was not made left no row.
            AuditEntry entry = new(operation, null, null, new AuditDetails(values: [.. details, new AuditValue("count", rows.ToString(CultureInfo.InvariantCulture))]));
            await _transactions.RunAsync(async token =>
            {
                await _trail.WriteAsync(entry, token);
                return TrailWritten.Instance;
            }, ct);

            await file.FlushAsync(ct);
            file.Seek(0, SeekOrigin.Begin);
            handedOver = true;
            return ExportFileResult.Done(file, fileName, rows);
        }
        finally
        {
            if (!handedOver)
            {
                await file.DisposeAsync();
            }
        }
    }

    private async Task<int> WriteWithinCeilingsAsync(FileStream file, Func<Stream, long, CancellationToken, Task<int>> write, CancellationToken ct)
    {
        using var ceiling = new CancellationTokenSource(ExportLimits.MaxProductionTime, _time);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, ceiling.Token);
        try
        {
            return await write(file, ExportLimits.MaxFileBytes, linked.Token);
        }
        catch (OperationCanceledException) when (ceiling.IsCancellationRequested && !ct.IsCancellationRequested)
        {
            throw new ExportLimitExceededException("The export took longer than its time ceiling.");
        }
    }

    private static string Day(DateOnly day) => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private sealed class TrailWritten : IOperationResult
    {
        public static TrailWritten Instance { get; } = new();

        public bool IsRejected => false;
    }
}
