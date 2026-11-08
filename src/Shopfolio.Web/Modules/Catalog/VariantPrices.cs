using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts.Pricing;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The change of a variant's price (T337; 02 3.8.4, 3.9.1, 3.9.3; 06 4.6, 4.11). The price follows the panel's price field rule (<see cref="ItemPricing.IsValidPrice"/>);
/// while the product's discount has not ended - not started yet or running - a price whose rounded discounted price would be 0,00 TL is refused (02 3.9.1: "İndirim
/// yürürken ... bir fiyat da girilemez"; K-600): a discount that has not started will run on the price, so the refusal holds for it too. The price is written to the
/// tracked variant and the history is brought up to it (<see cref="PriceHistoryRecorder"/>); the same price writes nothing. The write joins the caller's open
/// transaction and never opens or saves one (09 9.1 item 3); outside one it throws. The variant is a part of the product form, whose save raises the version once.
/// </summary>
internal sealed class VariantPrices
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;
    private readonly PriceHistoryRecorder _recorder;

    public VariantPrices(AppDbContext db, TimeProvider time, PriceHistoryRecorder recorder)
    {
        _db = db;
        _time = time;
        _recorder = recorder;
    }

    public async Task<VariantPriceResult> ChangeAsync(Guid variantId, decimal price, CancellationToken ct)
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("A variant's price is written only inside the caller's transaction (09 9.1 item 3).");
        }

        if (!ItemPricing.IsValidPrice(price))
        {
            return VariantPriceResult.InvalidPrice;
        }

        Variant? variant = await _db.Variants.SingleOrDefaultAsync(v => v.Id == variantId, ct);
        if (variant is null)
        {
            return VariantPriceResult.VariantNotFound;
        }

        if (variant.Price == price)
        {
            return VariantPriceResult.Unchanged;
        }

        StoredDiscount? stored = await PriceHistoryReads.StoredDiscountAsync(_db, variant.ProductId, ct);
        DiscountTerms? discount = PriceHistoryReads.DiscountAsSaved(_db, variant.ProductId, stored?.Terms);
        if (discount is not null
            && DiscountRules.StateAt(discount, _time.GetUtcNow()) != DiscountState.Ended
            && DiscountRules.ReachesZero(price, discount.Percentage))
        {
            return VariantPriceResult.DiscountedPriceZero;
        }

        variant.Price = price;
        await _recorder.RecordAsync(variant.ProductId, ct);
        return VariantPriceResult.Changed;
    }
}
