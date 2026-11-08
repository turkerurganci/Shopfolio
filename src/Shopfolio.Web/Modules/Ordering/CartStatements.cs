using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The cart's conditional statements (T346; 02 3.16.1, 3.16.3, 3.16.5; 09 10.2 item 2): the only raw statements of the cart, and only on <c>carts</c> and <c>cart_items</c> - a cart is no order,
/// so none of them touches a table of the order side or of the product form. The rows two requests may write at once - the cart itself and an item of the same variant - are written by one
/// <c>ON CONFLICT</c> statement, so a request that lost the race adds to the winner's row and no unique-index exception reaches the customer. Each statement joins the caller's transaction;
/// the caller holds the cart's advisory lock.
/// </summary>
internal sealed class CartStatements
{
    private readonly AppDbContext _db;

    public CartStatements(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>The member's cart is made once: a request that lost the race writes nothing.</summary>
    public Task InsertMemberCartAsync(Guid id, Guid customerId, DateTimeOffset now, CancellationToken ct)
    {
        return _db.Database.ExecuteSqlAsync(
            $"INSERT INTO carts (id, created_at, customer_id) VALUES ({id}, {now.ToUniversalTime()}, {customerId}) ON CONFLICT (customer_id) DO NOTHING",
            ct);
    }

    /// <summary>The guest cart is made once; only the digest of the key is stored (06 2.3.3).</summary>
    public Task InsertGuestCartAsync(Guid id, string guestCartKey, DateTimeOffset now, CancellationToken ct)
    {
        string hash = SecretKey.Hash(guestCartKey);
        DateTimeOffset at = now.ToUniversalTime();
        return _db.Database.ExecuteSqlAsync(
            $"INSERT INTO carts (id, created_at, guest_cart_key_hash, last_accessed_at) VALUES ({id}, {at}, {hash}, {at}) ON CONFLICT (guest_cart_key_hash) DO NOTHING",
            ct);
    }

    /// <summary>A new line, or - on the unmarked line of the same variant - the quantity raised; the reference price and its moment stay (02 3.16.5).</summary>
    public Task UpsertItemAsync(Guid id, Guid cartId, Guid variantId, int quantity, decimal unitPrice, DateTimeOffset now, CancellationToken ct)
    {
        return _db.Database.ExecuteSqlAsync(
            $"""
            INSERT INTO cart_items (id, created_at, cart_id, variant_id, quantity, first_added_unit_price)
            VALUES ({id}, {now.ToUniversalTime()}, {cartId}, {variantId}, {quantity}, {unitPrice})
            ON CONFLICT (cart_id, variant_id) WHERE removed_product_name IS NULL
            DO UPDATE SET quantity = cart_items.quantity + EXCLUDED.quantity
            """,
            ct);
    }

    /// <summary>
    /// The paid order's lines leave the cart (T347; 02 3.16.12): the unmarked items of the given variants in the given cart are deleted whole, whatever quantity they hold now. The items of other
    /// variants stay. A cart that no longer exists deletes nothing. One statement; it takes no cart lock because the payment transaction holds the order's lock first (09 10.2 item 5).
    /// </summary>
    public Task<int> DeleteOrderedItemsAsync(Guid cartId, Guid[] variantIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(variantIds);
        if (variantIds.Length == 0)
        {
            return Task.FromResult(0);
        }

        return _db.Database.ExecuteSqlAsync(
            $"DELETE FROM cart_items WHERE cart_id = {cartId} AND removed_product_name IS NULL AND variant_id = ANY({variantIds})",
            ct);
    }

    /// <summary>The merge of one guest line: the larger quantity stays; the line added first (the smaller moment) sets the reference price and keeps its moment (02 3.16.3, 3.16.5).</summary>
    public Task MergeItemAsync(Guid id, Guid memberCartId, Guid variantId, int quantity, decimal firstAddedUnitPrice, DateTimeOffset createdAt, CancellationToken ct)
    {
        return _db.Database.ExecuteSqlAsync(
            $"""
            INSERT INTO cart_items (id, created_at, cart_id, variant_id, quantity, first_added_unit_price)
            VALUES ({id}, {createdAt.ToUniversalTime()}, {memberCartId}, {variantId}, {quantity}, {firstAddedUnitPrice})
            ON CONFLICT (cart_id, variant_id) WHERE removed_product_name IS NULL
            DO UPDATE SET
                quantity = GREATEST(cart_items.quantity, EXCLUDED.quantity),
                first_added_unit_price = CASE WHEN EXCLUDED.created_at < cart_items.created_at THEN EXCLUDED.first_added_unit_price ELSE cart_items.first_added_unit_price END,
                created_at = LEAST(cart_items.created_at, EXCLUDED.created_at)
            """,
            ct);
    }
}
