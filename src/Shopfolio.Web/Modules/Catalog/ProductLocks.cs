using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The one place of the product row lock (09 10.2 items 5, 6; T525, D-401, D-362, D-381): <c>SELECT ... FOR NO KEY UPDATE</c> over one product row, held for the rest of the transaction -
/// the same raw SQL pattern as <see cref="CouponLocks"/> and <see cref="DigitalFileLocks"/>. Every write that can change what the product's publish gate, its type rule or its unit rule
/// reads takes it first, before it reads the product: the form's save (the type, the unit, the variants, the stock, the files), the product's and the variant's publish change, the deletion
/// of the product or of a variant and the removal of a digital file. So a file removal and a publish that both see the gate open, or a type change and a stock write that both see their
/// own rule kept, run one after the other and the second reads the first's result. The lock is reported to <see cref="LockSequence"/> as <see cref="LockKind.Product"/>, the head of its
/// transaction: the variant rows' conditional writes and one digital file row may follow it, nothing comes before it.
/// The mode is <c>NO KEY UPDATE</c>, not the plain <c>UPDATE</c> (T525 fix 1; K-1615 (2)): an order confirmation writes the variant first and then inserts its lines, whose foreign key
/// to the product asks the product row for <c>FOR KEY SHARE</c>; the plain mode blocks that, so a panel write holding the product and waiting for the same variant would close a cycle
/// (Postgres 40P01). The NO KEY mode does not conflict with the key share and still orders the panel writers among themselves.
/// </summary>
internal sealed class ProductLocks
{
    private readonly AppDbContext _db;

    public ProductLocks(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Locks the product's row; false when no product has <paramref name="productId"/>. A lock the transaction already holds is not asked again.</summary>
    public async Task<bool> LockAsync(Guid productId, CancellationToken ct)
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The product row lock is taken only inside a transaction (09 9.1 item 5).");
        }

        if (LockSequence.Holds(_db, LockKind.Product, productId))
        {
            return true;
        }

        LockSequence.Enter(_db, LockKind.Product, productId);
        List<Guid> locked = await _db.Database
            .SqlQuery<Guid>($"SELECT id AS \"Value\" FROM products WHERE id = {productId} FOR NO KEY UPDATE")
            .ToListAsync(ct);
        return locked.Count > 0;
    }

    /// <summary>
    /// Share-locks the rows of the products among <paramref name="productIds"/> (<c>FOR KEY SHARE</c>, the mode a foreign key to the row asks for) for the rest of the transaction and returns their ids
    /// (D-720; 06 4.53): a content record's link to a product is written on a product the same transaction saw and kept, so the product's deletion waits for the commit instead of stopping the
    /// link with a foreign key error. The mode does not conflict with <see cref="LockAsync"/>'s, so a product's own edit is not held up and no wait cycle closes. Not a family of
    /// <see cref="LockSequence"/>: it orders nothing against another writer, it only holds the rows.
    /// </summary>
    public async Task<IReadOnlyList<Guid>> ShareKeysAsync(IReadOnlyCollection<Guid> productIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(productIds);
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The product key share is taken only inside a transaction (09 9.1 item 5).");
        }

        Guid[] ids = [.. productIds.Distinct()];
        return await _db.Database
            .SqlQuery<Guid>($"SELECT id AS \"Value\" FROM products WHERE id = ANY({ids}) ORDER BY id FOR KEY SHARE")
            .ToListAsync(ct);
    }
}
