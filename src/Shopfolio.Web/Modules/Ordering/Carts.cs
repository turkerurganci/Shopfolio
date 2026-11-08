using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The cart's own operations (T346; 02 3.16.1-3.16.7; 06 4.15, 4.16): who owns it, the page's live read, the add, and the sign-in merge. The cart has no timer and no lifetime of its own: nothing here
/// empties it (02 3.16.2). Every method joins the caller's transaction, takes the cart's advisory lock first (09 10.2 item 5) and leaves the save to the transaction's owner (09 6 item 3).
/// The rows that two requests may write at once - the cart itself and an item of the same variant - are written by the one conditional statement of <see cref="CartStatements"/>
/// (<c>ON CONFLICT</c>), so a request that lost the race adds to the winner's row and no unique-index exception reaches the customer.
/// </summary>
internal sealed class Carts
{
    private readonly AppDbContext _db;
    private readonly AdvisoryLocks _locks;
    private readonly ICatalogModule _catalog;
    private readonly CartStatements _statements;
    private readonly TimeProvider _time;

    public Carts(AppDbContext db, AdvisoryLocks locks, ICatalogModule catalog, CartStatements statements, TimeProvider time)
    {
        _db = db;
        _locks = locks;
        _catalog = catalog;
        _statements = statements;
        _time = time;
    }

    /// <summary>The once-told items taken off the showcase: their names, deleted in the same call. The caller holds the cart's lock.</summary>
    public async Task<IReadOnlyList<string>> TakeRemovedNamesAsync(Guid cartId, CancellationToken ct)
    {
        List<CartItem> marked = await _db.CartItems
            .Where(i => i.CartId == cartId && i.RemovedProductName != null)
            .OrderBy(i => i.CreatedAt).ThenBy(i => i.Id)
            .ToListAsync(ct);
        if (marked.Count == 0)
        {
            return [];
        }

        string[] names = [.. marked.Select(i => i.RemovedProductName!)];
        _db.CartItems.RemoveRange(marked);
        return names;
    }

    public async Task<CartView> OpenAsync(CartOwner owner, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(owner);
        Guid? found = await FindCartIdAsync(owner, ct);
        if (found is not Guid cartId)
        {
            return Empty(null);
        }

        // The lock heads the transaction: a merge or an add that arrived together with this read is waited for, and what it wrote is read after it (09 10.2 item 5).
        await _locks.LockAsync(AdvisoryLockScope.Cart, cartId, ct);
        Cart? cart = await _db.Carts.SingleOrDefaultAsync(c => c.Id == cartId, ct);
        if (cart is null)
        {
            return Empty(null);
        }

        // A guest cart that is opened lives again for the full cookie life (06 9.4.3); a member's cart has no access moment.
        if (cart.GuestCartKeyHash is not null)
        {
            cart.LastAccessedAt = _time.GetUtcNow();
        }

        IReadOnlyList<string> removed = await TakeRemovedNamesAsync(cartId, ct);
        List<CartItem> items = await _db.CartItems
            .Where(i => i.CartId == cartId && i.RemovedProductName == null && i.VariantId != null)
            .OrderBy(i => i.CreatedAt).ThenBy(i => i.Id)
            .ToListAsync(ct);
        Dictionary<Guid, CartVariantFact> facts = (await _catalog.ListCartVariantFactsAsync([.. items.Select(i => i.VariantId!.Value)], ct)).ToDictionary(f => f.VariantId);

        List<CartLine> lines = [];
        List<string> mergeAdded = [];
        foreach (CartItem item in items)
        {
            if (!facts.TryGetValue(item.VariantId!.Value, out CartVariantFact? fact))
            {
                continue;
            }

            lines.Add(new CartLine(
                item.Id,
                fact.VariantId,
                fact.ProductId,
                fact.ProductName,
                item.Quantity,
                fact.UnitPrice,
                CartRules.PriceChanged(item.FirstAddedUnitPrice, fact.UnitPrice),
                fact.IsSoldOut,
                CartRules.EntersOrder(fact.IsOnShowcase, fact.IsSoldOut)));
            if (item.IsMergeNoticePending)
            {
                // Told once: the mark goes in the request that tells it (06 4.16; K-1183).
                item.IsMergeNoticePending = false;
                mergeAdded.Add(fact.ProductName);
            }
        }

        return new CartView(cartId, lines, removed, [.. mergeAdded.Distinct()], CartRules.PaymentBlockOf(lines), CartRules.OrderableSubtotal(lines));
    }

    public async Task<CartAddResult> AddAsync(CartOwner owner, Guid variantId, int quantity, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(owner);
        if (quantity <= 0)
        {
            return new CartAddResult(CartAddOutcome.InvalidQuantity, null);
        }

        IReadOnlyList<CartVariantFact> facts = await _catalog.ListCartVariantFactsAsync([variantId], ct);
        if (facts.Count == 0)
        {
            return new CartAddResult(CartAddOutcome.VariantNotFound, null);
        }

        CartVariantFact fact = facts[0];
        if (!fact.IsOnShowcase)
        {
            return new CartAddResult(CartAddOutcome.NotOnSale, null);
        }

        if (fact.IsSoldOut)
        {
            return new CartAddResult(CartAddOutcome.SoldOut, null);
        }

        Guid cartId = await FindCartIdAsync(owner, ct) ?? await EnsureCartAsync(owner, ct);
        await _locks.LockAsync(AdvisoryLockScope.Cart, cartId, ct);
        DateTimeOffset now = _time.GetUtcNow();
        if (owner.GuestCartKey is not null)
        {
            Cart cart = await _db.Carts.SingleAsync(c => c.Id == cartId, ct);
            cart.LastAccessedAt = now;
        }

        // The first add remembers the price of this moment; the same variant again only raises the quantity and leaves that price and its moment alone (02 3.16.5).
        await _statements.UpsertItemAsync(EntityIds.New(), cartId, variantId, quantity, fact.UnitPrice, now, ct);
        return new CartAddResult(CartAddOutcome.Added, cartId);
    }

    public async Task<CartMergeResult> MergeAsync(Guid customerId, string guestCartKey, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(guestCartKey);
        string hash = SecretKey.Hash(guestCartKey);
        Guid? found = await _db.Carts.AsNoTracking().Where(c => c.GuestCartKeyHash == hash).Select(c => (Guid?)c.Id).SingleOrDefaultAsync(ct);
        if (found is not Guid guestCartId)
        {
            return new CartMergeResult(null, false, 0);
        }

        // The guest cart ends here, so its lock is the one taken: a request of the same browser that arrived together is waited for and read after it. The member's side is written by
        // conditional statements, which another merge or add into the same account's cart does not disturb (05 7.3.5 allows one cart lock).
        await _locks.LockAsync(AdvisoryLockScope.Cart, guestCartId, ct);
        Cart? guestCart = await _db.Carts.SingleOrDefaultAsync(c => c.Id == guestCartId, ct);
        if (guestCart is null)
        {
            return new CartMergeResult(null, false, 0);
        }

        Guid memberCartId = await FindCartIdAsync(CartOwner.ForMember(customerId), ct) ?? await EnsureCartAsync(CartOwner.ForMember(customerId), ct);
        List<CartItem> guestItems = await _db.CartItems.Where(i => i.CartId == guestCartId).ToListAsync(ct);
        List<CartItem> guestLive = [.. guestItems.Where(i => i.RemovedProductName == null && i.VariantId != null)];

        // What the customer did not see in the guest cart is marked first, from the member's lines as they stood before the merge (02 3.16.4).
        Dictionary<Guid, int> seen = guestLive.ToDictionary(i => i.VariantId!.Value, i => i.Quantity);
        List<CartItem> memberItems = await _db.CartItems
            .Where(i => i.CartId == memberCartId && i.RemovedProductName == null && i.VariantId != null)
            .ToListAsync(ct);
        int unseen = 0;
        foreach (CartItem item in memberItems)
        {
            if (CartRules.MergeShowsNew(seen.TryGetValue(item.VariantId!.Value, out int guestQuantity) ? guestQuantity : null, item.Quantity))
            {
                item.IsMergeNoticePending = true;
                unseen++;
            }
        }

        // The guest lines fall into the member's cart: the larger quantity stays, and the line added first sets the reference price (02 3.16.3, 3.16.5).
        foreach (CartItem item in guestLive)
        {
            await _statements.MergeItemAsync(EntityIds.New(), memberCartId, item.VariantId!.Value, item.Quantity, item.FirstAddedUnitPrice, item.CreatedAt, ct);
        }

        // The told-once marks of items taken off the showcase move with the cart, so the message is not lost; the rest of the guest cart goes with it (06 7.2).
        foreach (CartItem marked in guestItems.Where(i => i.RemovedProductName != null))
        {
            marked.CartId = memberCartId;
        }

        // The told-once "no payment method" mark moves with the cart too, so the message is not lost with the guest cart (D-579).
        if (guestCart.IsPaymentUnavailableNoticePending)
        {
            Cart memberCart = await _db.Carts.SingleAsync(c => c.Id == memberCartId, ct);
            memberCart.IsPaymentUnavailableNoticePending = true;
        }

        _db.CartItems.RemoveRange(guestLive);
        _db.Carts.Remove(guestCart);
        return new CartMergeResult(memberCartId, true, unseen);
    }

    private async Task<Guid?> FindCartIdAsync(CartOwner owner, CancellationToken ct)
    {
        if (owner.CustomerId is Guid customerId)
        {
            return await _db.Carts.AsNoTracking().Where(c => c.CustomerId == customerId).Select(c => (Guid?)c.Id).SingleOrDefaultAsync(ct);
        }

        string hash = SecretKey.Hash(owner.GuestCartKey!);
        return await _db.Carts.AsNoTracking().Where(c => c.GuestCartKeyHash == hash).Select(c => (Guid?)c.Id).SingleOrDefaultAsync(ct);
    }

    // The owner's cart is made once: a request that lost the race finds the winner's row and writes nothing.
    private async Task<Guid> EnsureCartAsync(CartOwner owner, CancellationToken ct)
    {
        DateTimeOffset now = _time.GetUtcNow();
        Guid id = EntityIds.New();
        if (owner.CustomerId is Guid customerId)
        {
            await _statements.InsertMemberCartAsync(id, customerId, now, ct);
        }
        else
        {
            await _statements.InsertGuestCartAsync(id, owner.GuestCartKey!, now, ct);
        }

        return await FindCartIdAsync(owner, ct) ?? throw new InvalidOperationException("The cart that was just made is not found (06 4.15).");
    }

    private static CartView Empty(Guid? cartId) => new(cartId, [], [], [], CartPaymentBlock.NoOrderableItem, 0m);
}
