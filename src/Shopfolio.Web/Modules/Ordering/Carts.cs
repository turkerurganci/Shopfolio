using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings.Contracts;

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
    private readonly AttemptCounter _attempts;
    private readonly TimeProvider _time;

    public Carts(AppDbContext db, AdvisoryLocks locks, ICatalogModule catalog, CartStatements statements, AttemptCounter attempts, TimeProvider time)
    {
        _attempts = attempts;
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

        string[] names = [.. marked.Select(i => i.RemovedProductName!)]; // non-null: the Where above keeps only lines with a removed product name
        _db.CartItems.RemoveRange(marked);
        return names;
    }

    /// <summary>
    /// The frame's cart badge (T537; 07 5.5.1): the number of the owner's live lines and whether a sign-in merge mark waits to be told. A plain read: no lock, no cart made, no guest access
    /// moment renewed. A line taken off the showcase (marked removed) is not live; a member with no cart, and a guest whose cookie matches no cart, read zero.
    /// </summary>
    public async Task<CartBadge> ReadBadgeAsync(CartOwner owner, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(owner);
        Guid? cartId = await FindCartIdAsync(owner, ct);
        if (cartId is not Guid id)
        {
            return CartBadge.Empty;
        }

        IQueryable<CartItem> live = _db.CartItems.AsNoTracking().Where(i => i.CartId == id && i.RemovedProductName == null && i.VariantId != null);
        int count = await live.CountAsync(ct);
        bool hasMergeNotice = owner.CustomerId is not null && await live.AnyAsync(i => i.IsMergeNoticePending, ct);
        return new CartBadge(count, hasMergeNotice);
    }

    /// <summary>
    /// The once-told merge message (T537; 07 4.34.3; 02 3.16.4): the product names of the member's lines still marked by the sign-in merge, the marks cleared in the same call. The caller holds nothing;
    /// the method takes the cart's lock first, so a request that arrived together waits, finds no mark and tells nothing. Joins the caller's transaction and does not save.
    /// </summary>
    public async Task<IReadOnlyList<string>> TakeMergeNoticeAsync(Guid customerId, CancellationToken ct)
    {
        Guid? cartId = await FindCartIdAsync(CartOwner.ForMember(customerId), ct);
        if (cartId is not Guid id)
        {
            return [];
        }

        await _locks.LockAsync(AdvisoryLockScope.Cart, id, ct);
        List<CartItem> marked = await _db.CartItems
            .Where(i => i.CartId == id && i.RemovedProductName == null && i.VariantId != null && i.IsMergeNoticePending)
            .OrderBy(i => i.CreatedAt).ThenBy(i => i.Id)
            .ToListAsync(ct);
        if (marked.Count == 0)
        {
            return [];
        }

        Dictionary<Guid, CartVariantFact> facts = (await _catalog.ListCartVariantFactsAsync([.. marked.Select(i => i.VariantId!.Value)], ct)).ToDictionary(f => f.VariantId); // non-null: the Where above keeps only lines with a variant
        List<string> names = [];
        foreach (CartItem item in marked)
        {
            item.IsMergeNoticePending = false;
            if (facts.TryGetValue(item.VariantId!.Value, out CartVariantFact? fact)) // non-null: marked holds only lines with a variant (the Where above)
            {
                names.Add(fact.ProductName);
            }
        }

        return [.. names.Distinct()];
    }

    public Task<CartView> OpenAsync(CartOwner owner, ShippingFeeSection shippingTerms, CancellationToken ct) => OpenAsync(owner, shippingTerms, null, ct);

    /// <summary>
    /// The cart's read with a step between the cart's lock and the read of its lines (T348; 02 3.17.7): the confirmation cancels the cart's earlier unpaid order there, so the lines are
    /// judged against the stock without the reservation the customer's own earlier order held. The step runs after the cart's lock, which heads the transaction (05 7.3.5).
    /// </summary>
    public async Task<CartView> OpenAsync(CartOwner owner, ShippingFeeSection shippingTerms, Func<Guid, CancellationToken, Task>? afterLock, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(owner);
        Guid? found = await FindCartIdAsync(owner, ct);
        if (found is not Guid cartId)
        {
            return Empty(null, shippingTerms);
        }

        // The lock heads the transaction: a merge or an add that arrived together with this read is waited for, and what it wrote is read after it (09 10.2 item 5).
        await _locks.LockAsync(AdvisoryLockScope.Cart, cartId, ct);
        Cart? cart = await _db.Carts.SingleOrDefaultAsync(c => c.Id == cartId, ct);
        if (cart is null)
        {
            return Empty(null, shippingTerms);
        }

        if (afterLock is not null)
        {
            await afterLock(cartId, ct);
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
        Dictionary<Guid, CartVariantFact> facts = (await _catalog.ListCartVariantFactsAsync([.. items.Select(i => i.VariantId!.Value)], ct)).ToDictionary(f => f.VariantId); // non-null: the Where above keeps only lines with a variant

        // The quantity of every line goes through the one stock and per-order-limit rule, the merged lines too; a quantity is never lowered here (02 3.16.8-3.16.10).
        Dictionary<Guid, CartQuantityVerdict> verdicts = (await _catalog.JudgeCartQuantitiesAsync([.. items.Select(i => new CartQuantityLine(i.VariantId!.Value, i.Quantity))], ct)) // non-null: items holds only lines with a variant (the Where above)
            .ToDictionary(v => v.VariantId);

        List<CartLine> lines = [];
        List<string> mergeAdded = [];
        foreach (CartItem item in items)
        {
            if (!facts.TryGetValue(item.VariantId!.Value, out CartVariantFact? fact)) // non-null: items holds only lines with a variant (the Where above)
            {
                continue;
            }

            CartQuantityVerdict verdict = verdicts.TryGetValue(fact.VariantId, out CartQuantityVerdict? judged) ? judged : new CartQuantityVerdict(fact.VariantId, CartQuantityProblem.None, null);
            lines.Add(new CartLine(
                item.Id,
                fact.VariantId,
                fact.ProductId,
                fact.ProductName,
                fact.ProductType,
                item.Quantity,
                fact.UnitPrice,
                CartRules.PriceChanged(item.FirstAddedUnitPrice, fact.UnitPrice),
                fact.IsSoldOut,
                CartRules.EntersOrder(fact.IsOnShowcase, fact.IsSoldOut, verdict.Problem),
                verdict.Problem,
                verdict.OrderLimit));
            if (item.IsMergeNoticePending)
            {
                // Told once: the mark goes in the request that tells it (06 4.16; K-1183).
                item.IsMergeNoticePending = false;
                mergeAdded.Add(fact.ProductName);
            }
        }

        ShippingAssessment shipping = CartRules.AssessShipping(lines, shippingTerms);
        return new CartView(cartId, lines, removed, [.. mergeAdded.Distinct()], CartRules.PaymentBlockOf(lines, shipping), CartRules.OrderableSubtotal(lines), shipping);
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

        // A cart that does not exist yet holds nothing else: a refusal on the first add must not leave an empty cart behind.
        Guid? existingCartId = await FindCartIdAsync(owner, ct);
        if (existingCartId is null && await RefusalAsync(variantId, quantity, [], ct) is { } firstRefusal)
        {
            return firstRefusal;
        }

        Guid cartId = existingCartId ?? await EnsureCartAsync(owner, ct);
        await _locks.LockAsync(AdvisoryLockScope.Cart, cartId, ct);

        // The sign-in merge of the same browser may have taken this guest cart's lock first and ended the cart while this add waited: the add then starts the browser's new guest cart. The new cart is
        // written by the conditional statements and has no earlier request to wait for (D-589).
        Cart? guestCart = owner.GuestCartKey is null ? null : await _db.Carts.SingleOrDefaultAsync(c => c.Id == cartId, ct);
        if (owner.GuestCartKey is not null && guestCart is null)
        {
            cartId = await EnsureCartAsync(owner, ct);
            guestCart = await _db.Carts.SingleAsync(c => c.Id == cartId, ct);
        }

        // The quantity the line would hold is judged under the cart's lock, from the lines as they stand now (02 3.16.8, 3.16.9).
        List<CartQuantityLine> held = [.. (await _db.CartItems.AsNoTracking()
            .Where(i => i.CartId == cartId && i.RemovedProductName == null && i.VariantId != null)
            .Select(i => new { VariantId = i.VariantId!.Value, i.Quantity }) // non-null: the Where above keeps only lines with a variant
            .ToListAsync(ct)).Select(i => new CartQuantityLine(i.VariantId, i.Quantity))];
        if (await RefusalAsync(variantId, quantity, held, ct) is { } refusal)
        {
            return refusal;
        }

        DateTimeOffset now = _time.GetUtcNow();
        if (guestCart is not null)
        {
            guestCart.LastAccessedAt = now;
        }

        // The first add remembers the price of this moment; the same variant again only raises the quantity and leaves that price and its moment alone (02 3.16.5).
        await _statements.UpsertItemAsync(EntityIds.New(), cartId, variantId, quantity, fact.UnitPrice, now, ct);
        return new CartAddResult(CartAddOutcome.Added, cartId);
    }

    // The refusal of an add, or null when the quantity the line would hold is allowed. The quantity is the line's own plus the added one; the firm's limit is judged over the product's lines
    // together and wins over the stock (02 3.16.8, 3.16.9). A total no count can hold is the stock's refusal too.
    private async Task<CartAddResult?> RefusalAsync(Guid variantId, int added, IReadOnlyList<CartQuantityLine> held, CancellationToken ct)
    {
        long own = held.Where(l => l.VariantId == variantId).Sum(l => (long)l.Quantity) + added;
        if (own > int.MaxValue)
        {
            return new CartAddResult(CartAddOutcome.NoStock, null);
        }

        CartQuantityVerdict verdict = await _catalog.JudgeCartAddAsync(new CartQuantityLine(variantId, (int)own), held, ct);
        return verdict.Problem switch
        {
            CartQuantityProblem.NoStock => new CartAddResult(CartAddOutcome.NoStock, null),
            CartQuantityProblem.OverOrderLimit => new CartAddResult(CartAddOutcome.OverOrderLimit, null, verdict.OrderLimit),
            _ => null,
        };
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
        Dictionary<Guid, int> seen = guestLive.ToDictionary(i => i.VariantId!.Value, i => i.Quantity); // non-null: guestLive holds only lines with a variant (the Where above)
        List<CartItem> memberItems = await _db.CartItems
            .Where(i => i.CartId == memberCartId && i.RemovedProductName == null && i.VariantId != null)
            .ToListAsync(ct);
        int unseen = 0;
        foreach (CartItem item in memberItems)
        {
            if (CartRules.MergeShowsNew(seen.TryGetValue(item.VariantId!.Value, out int guestQuantity) ? guestQuantity : null, item.Quantity)) // non-null: memberItems holds only lines with a variant (the Where above)
            {
                item.IsMergeNoticePending = true;
                unseen++;
            }
        }

        // The guest lines fall into the member's cart: the larger quantity stays, and the line added first sets the reference price (02 3.16.3, 3.16.5).
        foreach (CartItem item in guestLive)
        {
            await _statements.MergeItemAsync(EntityIds.New(), memberCartId, item.VariantId!.Value, item.Quantity, item.FirstAddedUnitPrice, item.CreatedAt, ct); // non-null: guestLive holds only lines with a variant (the Where above)
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

        // The invalid coupon attempts (L-6) of the two carts meet in the same call: the cart with more attempts in the window keeps them under the member's cart, the counters
        // are not added and not reset (02 8.2.2; 05 6.3.2; 09 15.4 item 6; D-561).
        await _attempts.MergeCartAsync(guestCartId, memberCartId, ct);

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

        string hash = SecretKey.Hash((owner.GuestCartKey ?? throw new InvalidOperationException("A cart owner without an account is a guest with a cart key.")));
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
            await _statements.InsertGuestCartAsync(id, (owner.GuestCartKey ?? throw new InvalidOperationException("A cart owner without an account is a guest with a cart key.")), now, ct);
        }

        return await FindCartIdAsync(owner, ct) ?? throw new InvalidOperationException("The cart that was just made is not found (06 4.15).");
    }

    private static CartView Empty(Guid? cartId, ShippingFeeSection shippingTerms) => new(cartId, [], [], [], CartPaymentBlock.NoOrderableItem, 0m, CartRules.AssessShipping([], shippingTerms));
}
