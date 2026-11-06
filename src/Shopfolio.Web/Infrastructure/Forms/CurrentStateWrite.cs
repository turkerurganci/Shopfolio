using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Shopfolio.Infrastructure.Forms;

/// <summary>
/// The write of a state-changing operation on a record that carries no version (06 2.6.5; 07 2.4.4; 09 10.2 item 4): a contact request that is
/// closed, an administrator that is removed, a member whose e-mail change is confirmed, an address-book entry. These records have no screen with the
/// concurrent-edit warning (OB-10), so nothing is carried in the form: the operation names the state it needs in the condition of one conditional
/// update, and the row is written only if it is in that state now. Zero rows means the state had already changed - the caller shows the record as it
/// is; it is not an error and nothing is written twice. A plain field edit of such a record (a category's name) is the last writer's and does not use
/// this class (06 2.6.5).
/// <para>
/// This is not for the order side or for the product form: those records are written under the order's lock or with the product's version
/// (<see cref="RecordSave"/>, 09 10.2 items 2, 3), and the bulk-write scan refuses a statement that names them.
/// </para>
/// </summary>
internal static class CurrentStateWrite
{
    /// <summary>
    /// Writes <paramref name="set"/> to the rows of <paramref name="rows"/> that are in the state <paramref name="currentState"/> now, in one
    /// statement; true when a row was written.
    /// </summary>
    public static async Task<bool> ApplyAsync<TRecord>(
        IQueryable<TRecord> rows,
        Expression<Func<TRecord, bool>> currentState,
        Action<UpdateSettersBuilder<TRecord>> set,
        CancellationToken ct)
        where TRecord : class
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(currentState);
        ArgumentNullException.ThrowIfNull(set);
        int written = await rows.Where(currentState).ExecuteUpdateAsync(set, ct);
        return written > 0;
    }
}
