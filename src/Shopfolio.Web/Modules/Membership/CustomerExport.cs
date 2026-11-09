using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Membership;

/// <summary>One line of the member list file: the account's name and e-mail (02 10.7.4; 02 3.13.21).</summary>
internal sealed record CustomerExportRow(string Name, string Email);

/// <summary>The columns of the member list (09 10.10 item 5): two text columns; the formula rule covers the name and the e-mail like every text cell (K-1217 (3)).</summary>
internal static class CustomerExportColumns
{
    public static IReadOnlyList<CsvColumn<CustomerExportRow>> All { get; } =
    [
        // Customer.Name
        new("Ad", CsvColumnKind.Text, r => r.Name),

        // Customer.Email
        new("E-posta", CsvColumnKind.Text, r => r.Email),
    ];
}

/// <summary>
/// The read behind the member list export (T384; 02 10.7.4; 09 10.10 items 4, 5): every account - one that never ordered, and the unverified pending registration too - by its id, page by
/// page, with no date range. The name is the account's name and the e-mail the account's address. The list screens themselves export nothing (02 10.7.4; K-836).
/// </summary>
internal sealed class CustomerExport
{
    private readonly AppDbContext _db;

    public CustomerExport(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Writes the file's lines to <paramref name="destination"/> and returns the number of lines.</summary>
    public async Task<int> WriteAsync(Stream destination, long maxBytes, CancellationToken ct)
    {
        return await CsvExportWriter.WriteAsync(destination, CustomerExportColumns.All, ReadAsync(ct), new ExportCeilings(maxBytes), ct);
    }

    public async IAsyncEnumerable<CustomerExportRow> ReadAsync([EnumeratorCancellation] CancellationToken ct, int pageSize = ExportLimits.PageSize)
    {
        Guid? after = null;
        while (true)
        {
            IQueryable<Customer> query = _db.Customers.AsNoTracking();
            if (after is { } last)
            {
                query = query.Where(c => c.Id.CompareTo(last) > 0);
            }

            var page = await query.OrderBy(c => c.Id).Take(pageSize)
                .Select(c => new { c.Id, c.Name, c.Email }).ToListAsync(ct);
            if (page.Count == 0)
            {
                yield break;
            }

            after = page[^1].Id;
            foreach (var row in page)
            {
                yield return new CustomerExportRow(row.Name, row.Email);
            }

            if (page.Count < pageSize)
            {
                yield break;
            }
        }
    }
}
