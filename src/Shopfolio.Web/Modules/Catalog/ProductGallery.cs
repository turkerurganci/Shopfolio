using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// A product's gallery and its variants' images (T339; 02 3.11.1-3.11.4; 06 4.6, 4.71; 09 9.1 item 3). The writes join the caller's open transaction and never open or save one: they change
/// tracked rows and the entry point's runner saves them (the one statement that is sent at once - the clearing of the old main mark, below - is inside that transaction too). Called outside a
/// transaction they throw. Order, main mark and the firm's alternative text live on the link, not on the image. The limit counts the gallery's and the variants' images together, each image
/// once, against P-24; a refusal writes nothing. The reads are plain reads.
/// </summary>
internal sealed class ProductGallery
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;

    public ProductGallery(AppDbContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    /// <summary>
    /// Saves the gallery as the form posts it (02 3.11.2): the links are made to match the list - those not in it go (the image row stays and the disposal job removes it once nothing
    /// links it), new ones come, the position in the list is the order, the alternative text is the firm's - and the main mark is set by <see cref="GalleryRules.MainOf"/>. The old main
    /// mark is cleared before the new one is written, because the database allows one main image per product at any moment, not only at the end of the statement.
    /// </summary>
    public async Task<GallerySaveResult> SaveAsync(Guid productId, IReadOnlyList<GalleryEntry> entries, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entries);
        RequireTransaction();
        if (!await _db.Products.AnyAsync(p => p.Id == productId, ct))
        {
            return GallerySaveResult.ProductNotFound;
        }

        List<Guid> imageIds = [.. entries.Select(e => e.ImageId)];
        if (imageIds.Distinct().Count() != imageIds.Count)
        {
            return GallerySaveResult.DuplicateImage;
        }

        if (await _db.Images.CountAsync(i => imageIds.Contains(i.Id), ct) != imageIds.Count)
        {
            return GallerySaveResult.ImageNotFound;
        }

        List<Guid> variantImageIds = await VariantImageIdsAsync(productId, null, ct);
        if (GalleryRules.IsOverTheLimit(GalleryRules.ImageCount(imageIds, variantImageIds)))
        {
            return GallerySaveResult.LimitExceeded;
        }

        List<ProductImage> links = await _db.ProductImages.Where(p => p.ProductId == productId).ToListAsync(ct);
        ProductImage? currentMain = links.FirstOrDefault(l => l.IsMain);
        Guid? mainImageId = GalleryRules.MainOf(entries, currentMain?.ImageId);

        if (currentMain is not null && currentMain.ImageId != mainImageId)
        {
            await GalleryMainMark.ClearAsync(_db, productId, ct);
            currentMain.IsMain = false;
        }

        _db.ProductImages.RemoveRange(links.Where(l => !imageIds.Contains(l.ImageId)));
        DateTimeOffset now = _time.GetUtcNow();
        for (int i = 0; i < entries.Count; i++)
        {
            GalleryEntry entry = entries[i];
            string? altText = GalleryRules.CleanAltText(entry.AltText);
            bool isMain = entry.ImageId == mainImageId;
            ProductImage? link = links.FirstOrDefault(l => l.ImageId == entry.ImageId);
            if (link is null)
            {
                ProductImage created = ProductImage.Create(productId, entry.ImageId, i + 1, isMain, now);
                created.AltText = altText;
                _db.ProductImages.Add(created);
                continue;
            }

            link.SortOrder = i + 1;
            link.AltText = altText;
            link.IsMain = isMain;
        }

        return GallerySaveResult.Saved;
    }

    /// <summary>Gives the variant its own image, or - with null - takes it away so that it takes the product's again (02 3.11.1). The limit of 02 3.11.3 is checked with the new image counted.</summary>
    public async Task<VariantImageResult> SetVariantImageAsync(Guid variantId, Guid? imageId, CancellationToken ct)
    {
        RequireTransaction();
        Variant? variant = await _db.Variants.SingleOrDefaultAsync(v => v.Id == variantId, ct);
        if (variant is null)
        {
            return VariantImageResult.VariantNotFound;
        }

        if (imageId is { } id)
        {
            if (!await _db.Images.AnyAsync(i => i.Id == id, ct))
            {
                return VariantImageResult.ImageNotFound;
            }

            List<Guid> galleryImageIds = await _db.ProductImages.Where(p => p.ProductId == variant.ProductId).Select(p => p.ImageId).ToListAsync(ct);
            List<Guid> otherVariantImageIds = await VariantImageIdsAsync(variant.ProductId, variantId, ct);
            if (GalleryRules.IsOverTheLimit(GalleryRules.ImageCount(galleryImageIds, [.. otherVariantImageIds, id])))
            {
                return VariantImageResult.LimitExceeded;
            }
        }

        variant.ImageId = imageId;
        return VariantImageResult.Set;
    }

    /// <summary>The gallery in its order, each image with the firm's text and the text the page uses (02 3.11.4); empty for a product without images or without a row.</summary>
    public async Task<IReadOnlyList<GalleryImage>> ReadAsync(Guid productId, CancellationToken ct)
    {
        string? name = await _db.Products.Where(p => p.Id == productId).Select(p => p.Name).SingleOrDefaultAsync(ct);
        List<ProductImage> links = await OrderedLinksAsync(productId, ct);
        return GalleryOf(name, links);
    }

    /// <summary>
    /// The images of all the variants of a product, by variant (09 20 item 4): the product's name, its gallery links, its variants and their option values are each one read, whatever the number of
    /// variants. Each is the image <see cref="GetVariantImageAsync"/> gives; a variant with neither its own image nor a product main image is left out.
    /// </summary>
    public async Task<IReadOnlyDictionary<Guid, VariantImage>> GetProductVariantImagesAsync(Guid productId, CancellationToken ct)
    {
        List<Variant> variants = await _db.Variants.AsNoTracking().Where(v => v.ProductId == productId).ToListAsync(ct);
        Dictionary<Guid, VariantImage> images = [];
        if (variants.Count == 0)
        {
            return images;
        }

        string? name = await _db.Products.Where(p => p.Id == productId).Select(p => p.Name).SingleOrDefaultAsync(ct);
        List<ProductImage> links = await OrderedLinksAsync(productId, ct);
        GalleryImage? main = GalleryOf(name, links).FirstOrDefault(g => g.IsMain);
        List<Guid> valueIds = [.. variants.SelectMany(v => new[] { v.FirstOptionValueId, v.SecondOptionValueId }).OfType<Guid>().Distinct()];
        Dictionary<Guid, string> valueTexts = valueIds.Count == 0
            ? []
            : await _db.OptionValues.AsNoTracking().Where(v => valueIds.Contains(v.Id)).ToDictionaryAsync(v => v.Id, v => v.Value, ct);
        foreach (Variant variant in variants)
        {
            if (variant.ImageId is { } ownId)
            {
                string? firmText = links.FirstOrDefault(l => l.ImageId == ownId)?.AltText;
                List<string?> values = [.. new[] { variant.FirstOptionValueId, variant.SecondOptionValueId }.OfType<Guid>().Select(id => valueTexts.GetValueOrDefault(id))];
                images[variant.Id] = new VariantImage(ownId, IsOwn: true, ImageAltText.Choose(firmText, () => ImageAltText.ForVariant(name, values)));
            }
            else if (main is not null)
            {
                images[variant.Id] = new VariantImage(main.ImageId, IsOwn: false, main.EffectiveAltText);
            }
        }

        return images;
    }

    private static IReadOnlyList<GalleryImage> GalleryOf(string? name, List<ProductImage> links) =>
        [.. links.Select((l, i) => new GalleryImage(
            l.ImageId,
            i + 1,
            l.IsMain,
            l.AltText,
            ImageAltText.Choose(l.AltText, () => ImageAltText.ForGallery(name, i + 1, links.Count))))];

    /// <summary>The text of the image the card and the list show - the main image - (02 3.11.4): the firm's when it wrote one, else the product's name alone; null when the product does not exist.</summary>
    public async Task<string?> GetCardAltTextAsync(Guid productId, CancellationToken ct)
    {
        string? name = await _db.Products.Where(p => p.Id == productId).Select(p => p.Name).SingleOrDefaultAsync(ct);
        if (name is null)
        {
            return null;
        }

        string? firmText = await _db.ProductImages.Where(p => p.ProductId == productId && p.IsMain).Select(p => p.AltText).SingleOrDefaultAsync(ct);
        return ImageAltText.Choose(firmText, () => ImageAltText.ForCard(name));
    }

    /// <summary>
    /// The image a variant shows (02 3.11.1): its own, else the product's main image; null when it has neither or the variant does not exist. The text of an own image is the firm's when the same
    /// image sits in the gallery with a text of its own, else the product's name and the variant's option values; the text of the product's main image is the gallery's.
    /// </summary>
    public async Task<VariantImage?> GetVariantImageAsync(Guid variantId, CancellationToken ct)
    {
        Variant? variant = await _db.Variants.AsNoTracking().SingleOrDefaultAsync(v => v.Id == variantId, ct);
        if (variant is null)
        {
            return null;
        }

        if (variant.ImageId is { } ownId)
        {
            string? name = await _db.Products.Where(p => p.Id == variant.ProductId).Select(p => p.Name).SingleOrDefaultAsync(ct);
            string? firmText = await _db.ProductImages.Where(p => p.ProductId == variant.ProductId && p.ImageId == ownId).Select(p => p.AltText).SingleOrDefaultAsync(ct);
            List<string?> values = await OptionValueTextsAsync(variant, ct);
            return new VariantImage(ownId, IsOwn: true, ImageAltText.Choose(firmText, () => ImageAltText.ForVariant(name, values)));
        }

        GalleryImage? main = (await ReadAsync(variant.ProductId, ct)).FirstOrDefault(g => g.IsMain);
        return main is null ? null : new VariantImage(main.ImageId, IsOwn: false, main.EffectiveAltText);
    }

    private async Task<List<ProductImage>> OrderedLinksAsync(Guid productId, CancellationToken ct)
    {
        return await _db.ProductImages.AsNoTracking()
            .Where(p => p.ProductId == productId)
            .OrderBy(p => p.SortOrder).ThenBy(p => p.CreatedAt).ThenBy(p => p.Id)
            .ToListAsync(ct);
    }

    private async Task<List<Guid>> VariantImageIdsAsync(Guid productId, Guid? exceptVariantId, CancellationToken ct)
    {
        return await _db.Variants.AsNoTracking()
            .Where(v => v.ProductId == productId && v.ImageId != null && v.Id != exceptVariantId)
            .Select(v => v.ImageId!.Value) // non-null: the Where above keeps only variants with an image
            .ToListAsync(ct);
    }

    private async Task<List<string?>> OptionValueTextsAsync(Variant variant, CancellationToken ct)
    {
        List<Guid> ids = [.. new[] { variant.FirstOptionValueId, variant.SecondOptionValueId }.OfType<Guid>()];
        if (ids.Count == 0)
        {
            return [];
        }

        Dictionary<Guid, string> texts = await _db.OptionValues.AsNoTracking().Where(v => ids.Contains(v.Id)).ToDictionaryAsync(v => v.Id, v => v.Value, ct);
        return [.. ids.Select(id => texts.GetValueOrDefault(id))];
    }

    private void RequireTransaction()
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The gallery is written only inside the caller's transaction (09 9.1 item 3).");
        }
    }
}
