using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The one place of the product row lock (09 10.2 items 5, 6; T525, D-401, D-362, D-381): <c>SELECT ... FOR UPDATE</c> over one product row, held for the rest of the transaction -
/// the same raw SQL pattern as <see cref="CouponLocks"/> and <see cref="DigitalFileLocks"/>. Every write that can change what the product's publish gate, its type rule or its unit rule
/// reads takes it first, before it reads the product: the form's save (the type, the unit, the variants, the stock, the files), the product's and the variant's publish change, the deletion
/// of the product or of a variant and the removal of a digital file. So a file removal and a publish that both see the gate open, or a type change and a stock write that both see their
/// own rule kept, run one after the other and the second reads the first's result. The lock is reported to <see cref="LockSequence"/> as <see cref="LockKind.Product"/>, the head of its
/// transaction: the variant rows' conditional writes and one digital file row may follow it, nothing comes before it.
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
            .SqlQuery<Guid>($"SELECT id AS \"Value\" FROM products WHERE id = {productId} FOR UPDATE")
            .ToListAsync(ct);
        return locked.Count > 0;
    }
}
