using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The plain read of what an order item freezes of its variant (T355; 02 3.23.1): the product's name, type, VAT rate, exemption and fulfilment period; the variant's price as stored, its
/// option values, the image it shows (asked of <see cref="ProductGallery.GetVariantImageAsync"/>: its own, else the product's main image) and, on a digital product, its current file
/// (asked of <see cref="DigitalFileReader.CurrentFileAsync"/>; a file whose upload did not complete is none); and the running discount's declaration with the reference price the price
/// label shows (<see cref="PriceLabelReader"/>, 02 3.9.6). "Now" is <see cref="TimeProvider"/>'s: the moment of the read is the moment the order is born (02 3.23.4). A variant that does
/// not exist is not in the answer. Nothing is written and no lock is taken; the caller reads it inside the order's transaction.
/// </summary>
internal sealed class OrderItemSourceReader
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;
    private readonly ProductGallery _gallery;
    private readonly DigitalFileReader _files;

    public OrderItemSourceReader(AppDbContext db, TimeProvider time, ProductGallery gallery, DigitalFileReader files)
    {
        _db = db;
        _time = time;
        _gallery = gallery;
        _files = files;
    }

    public async Task<IReadOnlyList<OrderItemSource>> ListAsync(IReadOnlyCollection<Guid> variantIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(variantIds);
        Guid[] ids = [.. variantIds.Distinct()];
        if (ids.Length == 0)
        {
            return [];
        }

        var rows = await _db.Variants.AsNoTracking()
            .Where(v => ids.Contains(v.Id))
            .Join(
                _db.Products.AsNoTracking(),
                v => v.ProductId,
                p => p.Id,
                (v, p) => new
                {
                    VariantId = v.Id,
                    v.ProductId,
                    p.ProductType,
                    ProductName = p.Name,
                    v.FirstOptionValueId,
                    v.SecondOptionValueId,
                    v.Price,
                    v.FirstOnSaleAt,
                    p.VatRate,
                    p.WithdrawalExemptionReason,
                    p.FulfilmentPeriod,
                })
            .ToListAsync(ct);
        if (rows.Count == 0)
        {
            return [];
        }

        Guid[] productIds = [.. rows.Select(r => r.ProductId).Distinct()];
        Guid[] optionValueIds = [.. rows.SelectMany(r => new[] { r.FirstOptionValueId, r.SecondOptionValueId }).OfType<Guid>().Distinct()];
        Dictionary<Guid, string> optionValues = await _db.OptionValues.AsNoTracking()
            .Where(o => optionValueIds.Contains(o.Id))
            .ToDictionaryAsync(o => o.Id, o => o.Value, ct);

        DateTimeOffset now = _time.GetUtcNow();
        Dictionary<Guid, RunningDiscount> running = await PriceHistoryReads.RunningDiscountsAsync(_db, productIds, now, ct);
        List<OrderItemSource> sources = new(rows.Count);
        foreach (var row in rows)
        {
            DiscountLine? discount = null;
            if (running.TryGetValue(row.ProductId, out RunningDiscount? terms))
            {
                ReferenceInput variant = new(row.VariantId, row.Price, row.FirstOnSaleAt);
                Dictionary<Guid, decimal> references = await PriceHistoryReads.ReferencesAsync(_db, terms.Rule, DiscountRules.StartsAt(terms.Terms), [variant], ct);
                discount = DiscountRules.LineOf(terms.Terms, row.Price, references[row.VariantId]);
            }

            Guid? imageId = (await _gallery.GetVariantImageAsync(row.VariantId, ct))?.ImageId;
            Guid? fileId = null;
            if (row.ProductType == ProductType.Digital)
            {
                DigitalFile? file = await _files.CurrentFileAsync(row.VariantId, ct);
                fileId = file?.UploadCompletedAt is null ? null : file.Id;
            }

            sources.Add(new OrderItemSource(
                row.VariantId,
                row.ProductId,
                row.ProductType,
                row.ProductName,
                row.FirstOptionValueId is Guid first ? optionValues.GetValueOrDefault(first) : null,
                row.SecondOptionValueId is Guid second ? optionValues.GetValueOrDefault(second) : null,
                imageId,
                row.Price,
                row.VatRate,
                discount,
                row.ProductType == ProductType.Physical ? row.WithdrawalExemptionReason : null,
                row.ProductType == ProductType.Service ? row.FulfilmentPeriod : null,
                fileId));
        }

        return sources;
    }
}
