using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The panel's product list (T525; 07 UC-87; 04 9.4): every product whatever its status, newest first (07 2.6.5), the name searched through the one function of 09 10.5 item 5 - so a draft and an
/// archived product are found here (04 9.4.5; K-737) - and the publish status filter. The row's price range and stock column are computed here, not on the page (07 8.1.4). Four reads whatever the page
/// holds: the count and the page, the variants' figures of the page's keys, the main images of the page's keys (09 20 item 4). A plain read; it writes nothing.
/// </summary>
internal sealed class PanelProductListReader : IPanelProductList
{
    private readonly AppDbContext _db;

    public PanelProductListReader(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PanelProductPage> ListAsync(string? search, PublishStatus? status, int requestedPage, int pageSize, CancellationToken ct)
    {
        IQueryable<Product> products = _db.Products.AsNoTracking();
        if (SearchTerm.ToLikePattern(search) is { } pattern)
        {
            products = products.Where(p => EF.Functions.Like(p.NameSearchForm, pattern, SearchTerm.EscapeCharacter));
        }

        if (status is { } wanted)
        {
            products = products.Where(p => p.PublishStatus == wanted);
        }

        // Only the columns the row shows and the order's two are read - the description is not (09 20 item 3).
        PagedList<RowHead> paged = await products
            .Select(p => new RowHead { Id = p.Id, Name = p.Name, ProductType = p.ProductType, PublishStatus = p.PublishStatus, CreatedAt = p.CreatedAt })
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id)
            .ToPageAsync(requestedPage, pageSize, ct);

        List<Guid> keys = [.. paged.Items.Select(r => r.Id)];
        Dictionary<Guid, List<VariantFigure>> figures = (await _db.Variants.AsNoTracking()
                .Where(v => keys.Contains(v.ProductId))
                .Select(v => new VariantFigure(v.ProductId, v.Price, v.StockQuantity, v.ReservedQuantity))
                .ToListAsync(ct))
            .GroupBy(f => f.ProductId)
            .ToDictionary(g => g.Key, g => g.ToList());
        Dictionary<Guid, MainImageRow> images = await _db.ProductImages.AsNoTracking()
            .Where(i => keys.Contains(i.ProductId) && i.IsMain)
            .Select(i => new MainImageRow(i.ProductId, i.ImageId, i.AltText))
            .ToDictionaryAsync(i => i.ProductId, ct);

        List<PanelProductRow> rows = [.. paged.Items.Select(head =>
        {
            List<VariantFigure> variants = figures.GetValueOrDefault(head.Id) ?? [];
            images.TryGetValue(head.Id, out MainImageRow? image);
            (PanelStockKind kind, int total) = StockOf(head.ProductType, variants);
            return new PanelProductRow(
                head.Id,
                head.Name,
                head.ProductType,
                head.PublishStatus,
                image?.ImageId,
                image is null ? null : ImageAltText.Choose(image.AltText, () => ImageAltText.ForCard(head.Name)),
                variants.Count,
                variants.Count == 0 ? null : variants.Min(v => v.Price),
                variants.Count == 0 ? null : variants.Max(v => v.Price),
                kind,
                total);
        })];
        return new PanelProductPage(rows, paged.PageNumber, paged.PageCount, paged.TotalCount);
    }

    /// <summary>
    /// The stock column (04 9.4.3; K-802): a digital product and a product without a variant have none; a service with a variant without a quota is unlimited; otherwise the total of the variants'
    /// stock, or sold out when no variant has a unit left once the reserved units are taken off (02 3.6.3, 3.6.4).
    /// </summary>
    internal static (PanelStockKind Kind, int Total) StockOf(ProductType type, IReadOnlyCollection<VariantFigure> variants)
    {
        ArgumentNullException.ThrowIfNull(variants);
        if (type == ProductType.Digital || variants.Count == 0)
        {
            return (PanelStockKind.None, 0);
        }

        if (variants.Any(v => v.StockQuantity is null))
        {
            return type == ProductType.Service ? (PanelStockKind.Unlimited, 0) : (PanelStockKind.None, 0);
        }

        int total = variants.Sum(v => v.StockQuantity ?? 0);
        bool anyLeft = variants.Any(v => v.StockQuantity - v.ReservedQuantity > 0);
        return anyLeft ? (PanelStockKind.Counted, total) : (PanelStockKind.SoldOut, total);
    }

    internal sealed record VariantFigure(Guid ProductId, decimal Price, int? StockQuantity, int ReservedQuantity);

    private sealed record MainImageRow(Guid ProductId, Guid ImageId, string? AltText);

    private sealed class RowHead
    {
        public required Guid Id { get; init; }

        public required string Name { get; init; }

        public required ProductType ProductType { get; init; }

        public required PublishStatus PublishStatus { get; init; }

        public required DateTimeOffset CreatedAt { get; init; }
    }
}
