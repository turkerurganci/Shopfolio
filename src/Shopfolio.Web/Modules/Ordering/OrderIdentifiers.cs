using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.Modules.Ordering;

/// <summary>The number and the access key a new order is born with (<see cref="OrderIdentifiers.IssueAsync(DateTimeOffset, CancellationToken)"/>).</summary>
internal sealed record NewOrderIdentifiers(string OrderNumber, string OrderAccessKey);

/// <summary>
/// Where an order gets its number and its access key (02 3.22.1, 3.22.2, 3.22.5; 09 15.1 item 1; 05 6.4.2, 6.4.3). The order's confirmation makes them in the same
/// operation that makes the order, before anything is written: a number that an existing order already holds is dropped and a new one is made, so no order is made
/// with a clashing number and a clash never reaches the confirmation as an error (02 3.22.2). The pre-read and the unique index work together: two confirmations
/// that pick the same new number at the same time are told apart by the index, and the caller runs the whole operation again with <see cref="UniqueIndexes"/> (09 9.4
/// item 3). A number is never taken back: the order is never deleted (<see cref="OrderingRecordsGuard"/>), a cancelled order keeps its number, so no later order can
/// make it again (02 3.22.2). The year is the Turkey year of the moment of the order (02 4.1.1).
/// </summary>
internal sealed class OrderIdentifiers
{
    /// <summary>
    /// The unique indexes of the order table that a clash of a made value breaks, in the form the transaction runner takes (09 5.2): the number's and the access key's.
    /// The confirmation key's index is the payment step's own (T510).
    /// </summary>
    public static readonly IReadOnlyCollection<string> UniqueIndexes = ["ix_orders_order_number", "ix_orders_order_access_key"];

    private readonly AppDbContext _db;

    public OrderIdentifiers(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    /// <summary>A number that no order holds and a new access key, for an order made at <paramref name="now"/>.</summary>
    public Task<NewOrderIdentifiers> IssueAsync(DateTimeOffset now, CancellationToken ct) => IssueAsync(now, OrderNumber.Create, ct);

    /// <summary>The same with the maker of the number given; the tests use it to force a clash.</summary>
    internal async Task<NewOrderIdentifiers> IssueAsync(DateTimeOffset now, Func<int, string> makeNumber, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(makeNumber);
        int year = TurkeyTime.DateOf(now).Year;

        // No cap on the tries: the block holds 32^6 values a year, a clash is rare and each try is one indexed read; the loop ends at the first free number.
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            string number = makeNumber(year);

            // The orders the caller added and has not saved are in the tracker, not yet in the table; the stored ones are read with the unique index.
            bool isTaken = _db.Orders.Local.Any(o => o.OrderNumber == number)
                || await _db.Orders.AsNoTracking().AnyAsync(o => o.OrderNumber == number, ct);
            if (!isTaken)
            {
                return new NewOrderIdentifiers(number, SecretKey.Create());
            }
        }
    }
}
