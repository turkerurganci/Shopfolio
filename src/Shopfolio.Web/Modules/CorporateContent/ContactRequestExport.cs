using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>One line of the contact request file: the name and e-mail the sender typed in the form (02 10.7.4; 02 3.32.2).</summary>
internal sealed record ContactRequestExportRow(string Name, string Email);

/// <summary>The columns of the contact request list (09 10.10 item 5): two text columns; the formula rule covers the name and the e-mail like every text cell (K-1217 (3)).</summary>
internal static class ContactRequestExportColumns
{
    public static IReadOnlyList<CsvColumn<ContactRequestExportRow>> All { get; } =
    [
        // ContactRequest.Name
        new("Ad", CsvColumnKind.Text, r => r.Name),

        // ContactRequest.Email
        new("E-posta", CsvColumnKind.Text, r => r.Email),
    ];
}

/// <summary>
/// The read behind the contact request export (T384; 02 10.7.4; 09 10.10 items 4, 5): every request that is still kept - the ones the retention has not yet removed - by its id, page by
/// page, with no date range and no filter on the status; the sender's name and e-mail only (the message, the phone and the topic are not part of the file). The request list screen
/// itself exports nothing (K-836).
/// </summary>
internal sealed class ContactRequestExport
{
    private readonly AppDbContext _db;

    public ContactRequestExport(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Writes the file's lines to <paramref name="destination"/> and returns the number of lines.</summary>
    public async Task<int> WriteAsync(Stream destination, long maxBytes, CancellationToken ct)
    {
        return await CsvExportWriter.WriteAsync(destination, ContactRequestExportColumns.All, ReadAsync(ct), new ExportCeilings(maxBytes), ct);
    }

    public async IAsyncEnumerable<ContactRequestExportRow> ReadAsync([EnumeratorCancellation] CancellationToken ct, int pageSize = ExportLimits.PageSize)
    {
        Guid? after = null;
        while (true)
        {
            IQueryable<ContactRequest> query = _db.ContactRequests.AsNoTracking();
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
                yield return new ContactRequestExportRow(row.Name, row.Email);
            }

            if (page.Count < pageSize)
            {
                yield break;
            }
        }
    }
}
