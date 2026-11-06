using Microsoft.EntityFrameworkCore;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The one paging helper (09 10.5 item 2; 09 20 item 2). The parameter is an <see cref="IOrderedQueryable{T}"/>, so an unordered query does not compile;
/// that the order ends with the record's inner key <c>Id</c> (07 2.6.5; K-1228) is the business of the list's boundary test (09 10.5 item 6). The helper
/// counts first, computes <c>PageCount = max(1, ceil(TotalCount / pageSize))</c>, clamps the asked page to it - a page past the last shows the last
/// (07 2.6.2) - and reads that page with <c>Skip</c> and <c>Take</c>. The count and the read are two queries; a write between them may shift a record
/// and that is accepted: a list is not a snapshot.
/// </summary>
internal static class Paging
{
    public static async Task<PagedList<T>> ToPageAsync<T>(this IOrderedQueryable<T> query, int page, int pageSize, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        int total = await query.CountAsync(ct);
        int pageCount = PageCountOf(total, pageSize);
        int shown = Math.Clamp(page, PageNumber.First, pageCount);

        // The product of the last page and the page size can pass the range of an int only for a list of that size; long keeps the arithmetic exact.
        int skip = checked((int)((long)(shown - 1) * pageSize));
        List<T> items = await query.Skip(skip).Take(pageSize).ToListAsync(ct);
        return new PagedList<T>(items, shown, pageSize, total, pageCount);
    }

    /// <summary>The number of pages of a list: at least one, so an empty list is one empty page.</summary>
    public static int PageCountOf(int totalCount, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        return (int)Math.Max(1L, ((long)totalCount + pageSize - 1) / pageSize);
    }
}
