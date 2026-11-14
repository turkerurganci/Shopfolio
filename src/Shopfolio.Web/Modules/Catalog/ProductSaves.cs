using System.Globalization;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Categories;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.Catalog.Contracts.Variants;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The product form's save (07 UC-114; 11 T525 criteria 3-6): the product, its options and values, its variants, categories, gallery, variant images, digital files and discount are written
/// in the caller's one transaction - the method joins it and never opens one (09 9.1 item 3); the record's save is the record variant (<see cref="RecordSave"/>; 09 10.2 item 2), which
/// raises the version once for the whole form. A new product and a new variant are born Draft (03 8.1.1; 06 4.6); the publish status is not in the form (K-1189 (1)). The rules are their
/// homes': the definition (<see cref="ProductDefinitionRules"/>), the price (<see cref="ItemPricing.IsValidPrice"/>), the net quantity (<see cref="PriceLabelRules.CheckNetQuantity"/>), the
/// stock (<see cref="StockRules"/>), the option binding (<see cref="OptionStructureRules"/>), the gallery limit (<see cref="GalleryRules"/>), the gate (<see cref="PublishGateRules"/>), the
/// discount (<see cref="Discounts"/>), the price history (<see cref="VariantPrices"/>, <see cref="PriceHistoryRecorder"/>) and the stock's conditional write (<see cref="StockCounters"/>; K-980).
/// The refusals come in the order of K-800: 404, the fields' 422, the version's and the shown stock's 409, the reserved quantity's 422 - and a refusal writes nothing: the runner rolls back.
/// </summary>
internal sealed class ProductSaves
{
    private static readonly InvalidPercentageAttribute VatRateField = new() { AllowZero = true };

    private readonly AppDbContext _db;
    private readonly TimeProvider _time;
    private readonly ProductCategoryLinks _links;
    private readonly ProductGallery _gallery;
    private readonly VariantPrices _prices;
    private readonly PriceHistoryRecorder _recorder;
    private readonly Discounts _discounts;
    private readonly StockCounters _counters;
    private readonly StockCodes _stockCodes;
    private readonly ProductLocks _locks;
    private readonly IServiceProvider _services;

    public ProductSaves(
        AppDbContext db,
        TimeProvider time,
        ProductCategoryLinks links,
        ProductGallery gallery,
        VariantPrices prices,
        PriceHistoryRecorder recorder,
        Discounts discounts,
        StockCounters counters,
        StockCodes stockCodes,
        ProductLocks locks,
        IServiceProvider services)
    {
        _locks = locks;
        _services = services;
        _db = db;
        _time = time;
        _links = links;
        _gallery = gallery;
        _prices = prices;
        _recorder = recorder;
        _discounts = discounts;
        _counters = counters;
        _stockCodes = stockCodes;
    }

    public async Task<ProductSaveResult> SaveAsync(ProductSaveCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The product form is saved only inside the caller's transaction (09 9.1 item 3).");
        }

        DateTimeOffset now = _time.GetUtcNow();

        // 404: the product and every keyed row of the form must be the product's.
        Product? product = null;
        List<Variant> variants = [];
        List<Option> options = [];
        List<OptionValue> values = [];
        if (command.ProductId is Guid productId)
        {
            // The product's row lock heads the save (D-401, D-362, D-381; 09 10.2 item 5): a publish change, a file removal, a deletion or a stock write of the same product waits,
            // so the type, the unit, the variants and the files this save checks are the ones it writes over.
            product = await _locks.LockAsync(productId, ct) ? await _db.Products.SingleOrDefaultAsync(p => p.Id == productId, ct) : null;
            if (product is null)
            {
                return ProductSaveResult.NotFound;
            }

            variants = await _db.Variants.Where(v => v.ProductId == productId).ToListAsync(ct);
            options = await _db.Options.Where(o => o.ProductId == productId).ToListAsync(ct);
            List<Guid> optionIds = [.. options.Select(o => o.Id)];
            values = await _db.OptionValues.Where(v => optionIds.Contains(v.OptionId)).ToListAsync(ct);
        }

        if (command.Variants.Any(r => r.VariantId is Guid id && variants.All(v => v.Id != id))
            || command.Options.Any(r => r.OptionId is Guid id && options.All(o => o.Id != id))
            || command.Options.Any(r => r.Values.Any(v => v.OptionValueId is Guid id && values.All(x => x.Id != id || x.OptionId != r.OptionId))))
        {
            return ProductSaveResult.NotFound;
        }

        List<ProductSaveFieldError> errors = [];
        CheckProductFields(command, errors);
        if (errors.Count > 0 || command.ProductType is not ProductType type || command.VatRate is not decimal vatRate)
        {
            return ProductSaveResult.Refused(errors);
        }

        // The product's own fields. An existing product's three firm-like settings (P-6, P-9, P-44) are kept as they were for the trail (K-980; D-164; K-1615), and its planes' files
        // as the database holds them for the items' file links (D-527).
        SettingsBefore? settingsBefore = product is null ? null : new SettingsBefore(product.ShippingPeriod, product.MaxQuantityPerOrder, product.FulfilmentPeriod);
        OrderItemFiles.FilePlanes? filesBefore = product is null ? null : await OrderItemFiles.ReadAsync(_db, product.Id, ct);
        string name = command.Name?.Trim() ?? string.Empty;
        if (product is null)
        {
            product = Product.Create(name, await ProductAddresses.ForSaveAsync(_db, name, string.Empty, null, ct), type, vatRate, now);
            _db.Products.Add(product);
        }
        else
        {
            product.SetName(name);
            product.SetAddress(await ProductAddresses.ForSaveAsync(_db, name, product.Slug, product.SlugFixedAt, ct));
            product.ProductType = type;
            product.VatRate = vatRate;
        }

        product.SetDescription(command.Description);
        product.OriginCountryId = command.OriginCountryId;
        product.MeasurementUnit = command.MeasurementUnit;
        product.ShippingPeriod = command.ShippingPeriod;
        product.FulfilmentPeriod = command.FulfilmentPeriod;
        product.MaxQuantityPerOrder = command.MaxQuantityPerOrder;
        product.WithdrawalExemptionReason = command.WithdrawalExemptionReason;

        ApplyOptions(product.Id, command.Options, options, values, now, errors);
        List<(int Row, Variant Variant, ProductSaveVariant Input)> rows = await ApplyVariantsAsync(product, command, variants, options, values, now, errors, ct);
        await CheckImagesAsync(command, rows, variants, errors, ct);
        await CheckCategoriesAsync(command, errors, ct);
        if (errors.Count > 0)
        {
            return ProductSaveResult.Refused(errors);
        }

        await BindFilesAsync(product, command, rows, now, errors, ct);
        if (errors.Count > 0)
        {
            return ProductSaveResult.Refused(errors);
        }

        await AdvanceItemFilesAsync(product, command, rows, filesBefore, ct);

        (ProductCategoriesResult _, bool _) = await _links.ApplyAsync(product.Id, command.CategoryIds, command.PrimaryCategoryId, ct);
        await _gallery.ApplyLinksAsync(product.Id, command.Gallery, ct);

        // The prices: an existing variant's change goes through the price's one writer (the history and the discount's zero); a new variant's first price enters the history below.
        foreach ((int row, Variant variant, ProductSaveVariant input) in rows.Where(r => _db.Entry(r.Variant).State != EntityState.Added))
        {
            VariantPriceResult price = await _prices.ChangeAsync(variant.Id, input.Price ?? variant.Price, ct);
            if (price == VariantPriceResult.DiscountedPriceZero)
            {
                errors.Add(new ProductSaveFieldError(Field(row, nameof(ProductSaveVariant.Price)), ErrorCode.DiscountedPriceZero));
            }
        }

        await _recorder.RecordAsync(product.Id, ct);
        await SaveDiscountAsync(product.Id, command.Discount, rows, errors, ct);
        CheckGate(product, command, variants, errors);
        if (errors.Count > 0)
        {
            return ProductSaveResult.Refused(errors);
        }

        // 409 and the reserved quantity (K-800): the version first, then each variant's stock against its shown value.
        if (_db.Entry(product).State != EntityState.Added)
        {
            if (command.Version is not int version)
            {
                return ProductSaveResult.VersionConflict;
            }

            RecordSaveResult saved = await RecordSave.SaveAsync(_db, product, version, _ => { }, ct);
            if (saved.IsRejected)
            {
                return ProductSaveResult.VersionConflict;
            }
        }

        ProductSaveResult result = await WriteStockAsync(product.Id, rows, ct);
        if (!result.IsRejected && settingsBefore is not null)
        {
            await WriteSettingsTrailAsync(product, settingsBefore, ct);
        }

        return result;
    }

    /// <summary>
    /// The trail row of the product form's firm-like settings (02 10.3.1 (6); 11 T525 criterion 4; K-980; D-164; K-1615): P-6 (shipping period), P-9 (most per order) and P-44 (fulfilment
    /// period), each one only when it changed, with its old and new value, as <c>UpdateSettings</c> on the product. A new product's birth writes none: no setting was changed; it was given.
    /// </summary>
    private async Task WriteSettingsTrailAsync(Product product, SettingsBefore before, CancellationToken ct)
    {
        List<AuditValueChange> changes = [];
        AddChange(changes, nameof(Product.ShippingPeriod), before.ShippingPeriod, product.ShippingPeriod);
        AddChange(changes, nameof(Product.MaxQuantityPerOrder), before.MaxQuantityPerOrder, product.MaxQuantityPerOrder);
        AddChange(changes, nameof(Product.FulfilmentPeriod), before.FulfilmentPeriod, product.FulfilmentPeriod);
        if (changes.Count > 0)
        {
            await Trail.WriteAsync(new AuditEntry(AuditOperation.UpdateSettings, nameof(Product), product.Id, new AuditDetails(product.Name, changes)), ct);
        }
    }

    private static void AddChange(List<AuditValueChange> changes, string field, int? before, int? after)
    {
        if (before != after)
        {
            changes.Add(new AuditValueChange(field, before?.ToString(CultureInfo.InvariantCulture), after?.ToString(CultureInfo.InvariantCulture)));
        }
    }

    /// <summary>
    /// The items' last known file (D-527; 06 4.70 "Uygulamada"): the planes after this save are the form's files - the product's plane and each existing variant's own plane hold what the
    /// form sent - and each existing variant whose current file moved gets its items written to the file they download now, or keep the last one when none is left.
    /// </summary>
    private async Task AdvanceItemFilesAsync(
        Product product,
        ProductSaveCommand command,
        List<(int Row, Variant Variant, ProductSaveVariant Input)> rows,
        OrderItemFiles.FilePlanes? before,
        CancellationToken ct)
    {
        if (before is null || product.ProductType != ProductType.Digital)
        {
            return;
        }

        Dictionary<Guid, Guid> variantFiles = new(before.VariantFiles);
        foreach ((int _, Variant variant, ProductSaveVariant input) in rows)
        {
            if (input.DigitalFileId is Guid file)
            {
                variantFiles[variant.Id] = file;
            }
            else
            {
                variantFiles.Remove(variant.Id);
            }
        }

        OrderItemFiles.FilePlanes after = new(before.Variants, command.DigitalFileId, variantFiles);
        Dictionary<Guid, Guid> changes = OrderItemFiles.Changes(before.Variants, before, after);
        if (changes.Count > 0)
        {
            await Ordering.AdvanceItemFilesAsync(changes, ct);
        }
    }

    // The ordering module takes the catalog module in its constructor and the catalog module holds this class, so it is resolved when the items' file links are written, not when this
    // class is built (the pattern of OrderExport's after-sales module).
    private IOrderingModule Ordering => _services.GetRequiredService<IOrderingModule>();

    // The trail is resolved when a setting changed, so a host that builds the catalog module without the administration module (the concurrency tests) still builds it.
    private IAuditTrail Trail => _services.GetRequiredService<IAuditTrail>();

    private sealed record SettingsBefore(int? ShippingPeriod, int? MaxQuantityPerOrder, int? FulfilmentPeriod);

    /// <summary>The fields of the product itself (04 7.2.4; 06 4.5): the type, the VAT rate, the definition's rules and the parameters' fences (P-6, P-9, P-44).</summary>
    private static void CheckProductFields(ProductSaveCommand command, List<ProductSaveFieldError> errors)
    {
        if (command.ProductType is not ProductType type || !Enum.IsDefined(type))
        {
            errors.Add(new ProductSaveFieldError(nameof(ProductSaveCommand.ProductType), ErrorCode.FieldRequired));
            return;
        }

        if ((command.Name?.Trim().Length ?? 0) > ProductConstants.ProductNameMaxLength)
        {
            errors.Add(new ProductSaveFieldError(nameof(ProductSaveCommand.Name), ErrorCode.TextTooLong, ProductConstants.ProductNameMaxLength));
        }

        if (command.VatRate is not decimal vatRate)
        {
            errors.Add(new ProductSaveFieldError(nameof(ProductSaveCommand.VatRate), ErrorCode.FieldRequired));
        }
        else if (!VatRateField.Allows(vatRate))
        {
            errors.Add(new ProductSaveFieldError(nameof(ProductSaveCommand.VatRate), ErrorCode.InvalidPercentage));
        }

        // The fences are the firm settings' one home (T381 criterion 6; D-279): P-6 inside ProductDefinitionRules below, P-9 and P-44 here - the numbers are not written again.
        if (command.FulfilmentPeriod is int fulfilmentPeriod && !FirmSettingFences.Check("P-44", fulfilmentPeriod).IsWithin)
        {
            errors.Add(new ProductSaveFieldError(nameof(ProductSaveCommand.FulfilmentPeriod), ErrorCode.ValueOutOfBounds));
        }

        if (command.MaxQuantityPerOrder is int maxQuantity && !FirmSettingFences.Check("P-9", maxQuantity).IsWithin)
        {
            errors.Add(new ProductSaveFieldError(nameof(ProductSaveCommand.MaxQuantityPerOrder), ErrorCode.ValueOutOfBounds));
        }

        // The status is the record's: the form does not move it, so a published service keeps asking for its fulfilment period here.
        foreach (ProductDefinitionProblem problem in ProductDefinitionRules.Check(
            type, PublishStatus.Draft, command.WithdrawalExemptionReason, command.FulfilmentPeriod, command.OriginCountryId, command.ShippingPeriod))
        {
            errors.Add(problem switch
            {
                ProductDefinitionProblem.ExemptionOutsideAPhysicalProduct => new ProductSaveFieldError(nameof(ProductSaveCommand.WithdrawalExemptionReason), ErrorCode.InvalidListValue),
                ProductDefinitionProblem.PhysicalProductWithoutOrigin => new ProductSaveFieldError(nameof(ProductSaveCommand.OriginCountryId), ErrorCode.FieldRequired),
                ProductDefinitionProblem.OriginOnAService => new ProductSaveFieldError(nameof(ProductSaveCommand.OriginCountryId), ErrorCode.InvalidListValue),
                ProductDefinitionProblem.ShippingPeriodOutsideTheFence => new ProductSaveFieldError(nameof(ProductSaveCommand.ShippingPeriod), ErrorCode.ValueOutOfBounds),
                _ => throw new InvalidOperationException($"The definition problem {problem} has no field."),
            });
        }
    }

    /// <summary>The options and their values (06 4.7, 4.8): a keyed row is renamed, a new row with a name is added at the next free order; a blank new row is not read (K-1189 (2)).</summary>
    private void ApplyOptions(
        Guid productId,
        IReadOnlyList<ProductSaveOption> rows,
        List<Option> options,
        List<OptionValue> values,
        DateTimeOffset now,
        List<ProductSaveFieldError> errors)
    {
        for (int i = 0; i < rows.Count; i++)
        {
            ProductSaveOption row = rows[i];
            string? optionName = row.Name?.Trim();
            Option? option = row.OptionId is Guid id ? options.Single(o => o.Id == id) : null;
            if (option is null && string.IsNullOrEmpty(optionName) && row.Values.All(v => string.IsNullOrWhiteSpace(v.Value)))
            {
                continue;
            }

            if (string.IsNullOrEmpty(optionName))
            {
                errors.Add(new ProductSaveFieldError($"Options[{i}].Name", ErrorCode.FieldRequired));
                continue;
            }

            if (option is null)
            {
                if (OptionStructureRules.NextOptionOrder(options.Select(o => o.SortOrder)) is not int order)
                {
                    errors.Add(new ProductSaveFieldError($"Options[{i}].Name", ErrorCode.ValueOutOfBounds));
                    continue;
                }

                option = Option.Create(productId, optionName, order, now);
                options.Add(option);
                _db.Options.Add(option);
            }
            else
            {
                option.Name = optionName;
            }

            for (int j = 0; j < row.Values.Count; j++)
            {
                ProductSaveOptionValue valueRow = row.Values[j];
                string? text = valueRow.Value?.Trim();
                OptionValue? value = valueRow.OptionValueId is Guid valueId ? values.Single(v => v.Id == valueId) : null;
                if (string.IsNullOrEmpty(text))
                {
                    if (value is not null)
                    {
                        errors.Add(new ProductSaveFieldError($"Options[{i}].Values[{j}].Value", ErrorCode.FieldRequired));
                    }

                    continue;
                }

                if (value is not null)
                {
                    value.Value = text;
                    continue;
                }

                Guid ownerId = option.Id;
                int nextOrder = values.Where(v => v.OptionId == ownerId).Select(v => v.SortOrder).DefaultIfEmpty(0).Max() + 1;
                OptionValue created = OptionValue.Create(ownerId, text, nextOrder, now);
                values.Add(created);
                _db.OptionValues.Add(created);
            }
        }
    }

    /// <summary>
    /// The variant rows (06 4.6; 04 7.2.4.7-7.2.4.13): the binding, the stock code - a blank code of a new row takes the suggested one (K-1189 (3)) -, the price, the net quantity and the stock
    /// field's form. The existing row's price and stock are written later by their own writers; a new row is born Draft with its first price and stock.
    /// </summary>
    private async Task<List<(int Row, Variant Variant, ProductSaveVariant Input)>> ApplyVariantsAsync(
        Product product,
        ProductSaveCommand command,
        List<Variant> variants,
        List<Option> options,
        List<OptionValue> values,
        DateTimeOffset now,
        List<ProductSaveFieldError> errors,
        CancellationToken ct)
    {
        List<(int, Variant, ProductSaveVariant)> applied = [];
        HashSet<int> optionOrders = [.. options.Select(o => o.SortOrder)];
        HashSet<string> formCodes = [];
        for (int i = 0; i < command.Variants.Count; i++)
        {
            ProductSaveVariant row = command.Variants[i];
            Variant? variant = row.VariantId is Guid id ? variants.Single(v => v.Id == id) : null;
            if (variant is null && IsBlank(row))
            {
                continue;
            }

            int before = errors.Count;
            VariantStructureCheck binding = OptionStructureRules.CheckBinding(optionOrders, Bound(row.FirstOptionValueId, options, values), Bound(row.SecondOptionValueId, options, values));
            if (binding != VariantStructureCheck.Valid)
            {
                errors.Add(new ProductSaveFieldError(
                    Field(i, binding == VariantStructureCheck.ValueMissing || row.FirstOptionValueId is not null ? nameof(ProductSaveVariant.SecondOptionValueId) : nameof(ProductSaveVariant.FirstOptionValueId)),
                    binding == VariantStructureCheck.ValueMissing ? ErrorCode.FieldRequired : ErrorCode.InvalidListValue));
            }

            string? sku = row.SKU?.Trim();
            if (string.IsNullOrEmpty(sku) && variant is null)
            {
                sku = await _stockCodes.SuggestAsync(_db.Entry(product).State == EntityState.Added ? null : product.Id, product.Name, ct);
                if (sku is null)
                {
                    // K-1189 (3): no name, no code - both fields take the required message.
                    errors.Add(new ProductSaveFieldError(nameof(ProductSaveCommand.Name), ErrorCode.FieldRequired));
                }
            }

            if (string.IsNullOrEmpty(sku))
            {
                errors.Add(new ProductSaveFieldError(Field(i, nameof(ProductSaveVariant.SKU)), ErrorCode.FieldRequired));
            }
            else if (!formCodes.Add(TextNormalizer.NormalizeIdentity(sku))
                || await _stockCodes.CheckAsync(sku, variant?.Id, ct) == StockCodeCheck.Taken)
            {
                errors.Add(new ProductSaveFieldError(Field(i, nameof(ProductSaveVariant.SKU)), ErrorCode.DuplicateSku));
            }

            if (row.Price is not decimal price)
            {
                errors.Add(new ProductSaveFieldError(Field(i, nameof(ProductSaveVariant.Price)), ErrorCode.FieldRequired));
            }
            else if (!ItemPricing.IsValidPrice(price))
            {
                errors.Add(new ProductSaveFieldError(Field(i, nameof(ProductSaveVariant.Price)), ErrorCode.InvalidAmount));
            }

            switch (PriceLabelRules.CheckNetQuantity(command.MeasurementUnit, row.NetQuantity))
            {
                case NetQuantityProblem.Missing:
                    errors.Add(new ProductSaveFieldError(Field(i, nameof(ProductSaveVariant.NetQuantity)), ErrorCode.FieldRequired));
                    break;
                case NetQuantityProblem.WithoutUnit:
                    errors.Add(new ProductSaveFieldError(nameof(ProductSaveCommand.MeasurementUnit), ErrorCode.FieldRequired));
                    break;
                case NetQuantityProblem.NotAboveZero:
                case NetQuantityProblem.TooManyDecimals:
                    errors.Add(new ProductSaveFieldError(Field(i, nameof(ProductSaveVariant.NetQuantity)), ErrorCode.InvalidAmount));
                    break;
            }

            IReadOnlyList<StockProblem> stock = StockRules.Check(product.ProductType, row.StockQuantity);
            if (stock.Contains(StockProblem.PhysicalWithoutStock))
            {
                errors.Add(new ProductSaveFieldError(Field(i, nameof(ProductSaveVariant.StockQuantity)), ErrorCode.FieldRequired));
            }
            else if (stock.Count > 0)
            {
                errors.Add(new ProductSaveFieldError(Field(i, nameof(ProductSaveVariant.StockQuantity)), ErrorCode.InvalidQuantity));
            }

            if (errors.Count > before || string.IsNullOrEmpty(sku) || row.Price is not decimal rowPrice)
            {
                continue;
            }

            if (variant is null)
            {
                variant = Variant.Create(product.Id, sku, rowPrice, now, row.StockQuantity);
                variants.Add(variant);
                _db.Variants.Add(variant);
            }
            else
            {
                variant.SetSku(sku);
            }

            variant.FirstOptionValueId = row.FirstOptionValueId;
            variant.SecondOptionValueId = row.SecondOptionValueId;
            variant.NetQuantity = row.NetQuantity;
            applied.Add((i, variant, row));
        }

        // The combination is unique among all the product's variants (06 4.6), the rows the form did not carry included.
        foreach ((int row, Variant variant, ProductSaveVariant _) in applied)
        {
            if (variants.Any(v => v.Id != variant.Id && v.FirstOptionValueId == variant.FirstOptionValueId && v.SecondOptionValueId == variant.SecondOptionValueId))
            {
                errors.Add(new ProductSaveFieldError(Field(row, nameof(ProductSaveVariant.FirstOptionValueId)), ErrorCode.InvalidListValue));
            }
        }

        return applied;
    }

    /// <summary>
    /// The images the form links (K-1171 (4); 07 6.4.1): a gallery image or a variant image whose row is gone is the upload field's <c>UploadMissing</c>; the limit counts the gallery and
    /// the variants' images as the form leaves them (02 3.11.3; P-24).
    /// </summary>
    private async Task CheckImagesAsync(
        ProductSaveCommand command,
        List<(int Row, Variant Variant, ProductSaveVariant Input)> rows,
        List<Variant> variants,
        List<ProductSaveFieldError> errors,
        CancellationToken ct)
    {
        List<Guid> galleryIds = [.. command.Gallery.Select(e => e.ImageId)];
        List<Guid> asked = [.. galleryIds, .. rows.Select(r => r.Input.ImageId).OfType<Guid>()];
        HashSet<Guid> known = [.. await _db.Images.Where(i => asked.Contains(i.Id)).Select(i => i.Id).ToListAsync(ct)];
        if (galleryIds.Distinct().Count() != galleryIds.Count || galleryIds.Any(id => !known.Contains(id)))
        {
            errors.Add(new ProductSaveFieldError("Images", ErrorCode.UploadMissing));
        }

        foreach ((int row, Variant variant, ProductSaveVariant input) in rows)
        {
            if (input.ImageId is Guid imageId && !known.Contains(imageId))
            {
                errors.Add(new ProductSaveFieldError(Field(row, nameof(ProductSaveVariant.ImageId)), ErrorCode.UploadMissing));
                continue;
            }

            variant.ImageId = input.ImageId;
        }

        IEnumerable<Guid> variantImages = variants.Select(v => v.ImageId).OfType<Guid>();
        if (GalleryRules.IsOverTheLimit(GalleryRules.ImageCount(galleryIds, variantImages)))
        {
            errors.Add(new ProductSaveFieldError("Images", ErrorCode.ImageLimitExceeded, ProductConstants.ImagesPerProductMax));
        }
    }

    /// <summary>A category the form chose and that was deleted meanwhile is the category field's message, the other inputs kept (K-1171 (4)'s pattern; 07 UC-114).</summary>
    private async Task CheckCategoriesAsync(ProductSaveCommand command, List<ProductSaveFieldError> errors, CancellationToken ct)
    {
        List<Guid> wanted = [.. command.CategoryIds.Distinct()];
        int known = await _db.Categories.CountAsync(c => wanted.Contains(c.Id), ct);
        if (known != wanted.Count)
        {
            errors.Add(new ProductSaveFieldError("Categories", ErrorCode.InvalidListValue));
        }
        else if (wanted.Count > 0 && (command.PrimaryCategoryId is not Guid primary || !wanted.Contains(primary)))
        {
            errors.Add(new ProductSaveFieldError("Categories", ErrorCode.FieldRequired));
        }
    }

    /// <summary>
    /// The digital files (06 4.70; 07 UC-114): the product-level file and the variants' own files. A file whose row is gone or whose upload has not completed is the field's
    /// <c>UploadMissing</c> (K-1100, K-1118); the current file that is replaced or removed is detached, the new one attached. A removal that leaves a Published variant of a digital product
    /// without a file is <c>DigitalFileRequired</c> (03 3.5.1.6).
    /// </summary>
    private async Task BindFilesAsync(
        Product product,
        ProductSaveCommand command,
        List<(int Row, Variant Variant, ProductSaveVariant Input)> rows,
        DateTimeOffset now,
        List<ProductSaveFieldError> errors,
        CancellationToken ct)
    {
        if (product.ProductType != ProductType.Digital)
        {
            return;
        }

        bool productHasFile = await BindFileAsync(command.DigitalFileId, product.Id, null, nameof(ProductSaveCommand.DigitalFileId), now, errors, ct);
        foreach ((int row, Variant variant, ProductSaveVariant input) in rows)
        {
            bool variantHasFile = await BindFileAsync(input.DigitalFileId, null, variant.Id, Field(row, nameof(ProductSaveVariant.DigitalFileId)), now, errors, ct);
            if (variant.PublishStatus == PublishStatus.Published && !productHasFile && !variantHasFile)
            {
                errors.Add(new ProductSaveFieldError(Field(row, nameof(ProductSaveVariant.DigitalFileId)), ErrorCode.DigitalFileRequired));
            }
        }
    }

    private async Task<bool> BindFileAsync(Guid? fileId, Guid? productId, Guid? variantId, string field, DateTimeOffset now, List<ProductSaveFieldError> errors, CancellationToken ct)
    {
        Guid? currentId = await _db.DigitalFiles.AsNoTracking()
            .Where(f => f.DetachedAt == null && f.AttachedAt != null && (productId != null ? f.ProductId == productId : f.VariantId == variantId))
            .Select(f => (Guid?)f.Id)
            .SingleOrDefaultAsync(ct);
        if (fileId == currentId)
        {
            return currentId is not null;
        }

        DigitalFile? file = null;
        if (fileId is Guid id)
        {
            file = await _db.DigitalFiles.SingleOrDefaultAsync(f => f.Id == id, ct);
            if (file is null || file.UploadCompletedAt is null || file.AttachedAt is not null)
            {
                errors.Add(new ProductSaveFieldError(field, ErrorCode.UploadMissing));
                return false;
            }
        }

        if (currentId is Guid current)
        {
            // Through the tracker, not a bulk statement (the product form's records are written only so; 09 10.2): the current file is detached in the same unit of work.
            DigitalFile detached = await _db.DigitalFiles.SingleAsync(f => f.Id == current, ct);
            detached.DetachedAt = now;
        }

        if (file is not null)
        {
            file.ProductId = productId;
            file.VariantId = variantId;
            file.AttachedAt = now;
        }

        return file is not null;
    }

    /// <summary>The discount (06 4.12; 02 3.9.1, 3.9.5): all three fields save it through its one writer, none removes it; a part of it is the field's required message.</summary>
    private async Task SaveDiscountAsync(
        Guid productId,
        ProductSaveDiscount? discount,
        List<(int Row, Variant Variant, ProductSaveVariant Input)> rows,
        List<ProductSaveFieldError> errors,
        CancellationToken ct)
    {
        if (discount is null || (discount.Percentage is null && discount.StartDate is null && discount.EndDate is null))
        {
            if (await _db.Discounts.AnyAsync(d => d.ProductId == productId, ct))
            {
                await _discounts.RemoveAsync(productId, ct);
            }

            return;
        }

        if (discount.Percentage is not decimal percentage || discount.StartDate is not DateOnly start || discount.EndDate is not DateOnly end)
        {
            string missing = discount.Percentage is null ? "Discount.Percentage" : discount.StartDate is null ? "Discount.StartDate" : "Discount.EndDate";
            errors.Add(new ProductSaveFieldError(missing, ErrorCode.FieldRequired));
            return;
        }

        DiscountSaveResult saved = await _discounts.SaveAsync(productId, percentage, start, end, ct);
        if (!saved.IsRejected)
        {
            return;
        }

        string variantField = rows.FirstOrDefault(r => r.Variant.Id == saved.VariantId) is { Variant: not null } row
            ? Field(row.Row, nameof(ProductSaveVariant.Price))
            : "Discount.Percentage";
        errors.Add(saved.Reason switch
        {
            "InvalidPercentage" => new ProductSaveFieldError("Discount.Percentage", ErrorCode.InvalidPercentage),
            "EndBeforeStart" => new ProductSaveFieldError("Discount.EndDate", ErrorCode.InvalidDateRange),
            "DiscountedPriceZero" => new ProductSaveFieldError(variantField, ErrorCode.DiscountedPriceZero, 0m, saved.ReferencePrice),
            _ => new ProductSaveFieldError(variantField, ErrorCode.DiscountedPriceNotBelowReference, saved.DiscountedPrice, saved.ReferencePrice),
        });
    }

    /// <summary>
    /// A published product's save that breaks its gate is refused at the field that breaks it (04 2.12.1.5, 9.5.12; K-799; 07 6.2 <c>PublishedRecordGateBroken</c>): the name, the main category,
    /// the file of a digital product's Published variant, the fulfilment period of a service.
    /// </summary>
    private static void CheckGate(
        Product product,
        ProductSaveCommand command,
        List<Variant> variants,
        List<ProductSaveFieldError> errors)
    {
        if (product.PublishStatus != PublishStatus.Published)
        {
            return;
        }

        IReadOnlyList<string> unmet = PublishGateRules.UnmetOfProduct(
            product.Name,
            command.PrimaryCategoryId is not null,
            product.ProductType,
            product.FulfilmentPeriod,
            variants.Count(v => v.PublishStatus == PublishStatus.Published),
            0);
        foreach (string condition in unmet)
        {
            string field = condition switch
            {
                PublishCondition.Name => nameof(ProductSaveCommand.Name),
                PublishCondition.MainCategory => "Categories",
                PublishCondition.FulfilmentPeriod => nameof(ProductSaveCommand.FulfilmentPeriod),
                _ => nameof(ProductSaveCommand.ProductType),
            };
            errors.Add(new ProductSaveFieldError(field, ErrorCode.PublishedRecordGateBroken));
        }

    }

    /// <summary>
    /// The stock and capacity fields (K-980; 07 2.4.3): each changed one is written by its conditional update against the value the form showed, in the order of the variants' keys (05 7.3.5).
    /// The 409 of a shown value comes before any reserved quantity's 422 (K-800): every shown value is compared first. A shown value the form sent back unreadable is no match (D-129):
    /// that row is the 409 whatever the record holds.
    /// </summary>
    private async Task<ProductSaveResult> WriteStockAsync(Guid productId, List<(int Row, Variant Variant, ProductSaveVariant Input)> rows, CancellationToken ct)
    {
        List<(int Row, Variant Variant, ProductSaveVariant Input)> changed = [.. rows
            .Where(r => _db.Entry(r.Variant).State != EntityState.Added && (r.Input.IsShownStockUnreadable || r.Input.StockQuantity != r.Input.ShownStockQuantity))
            .OrderBy(r => r.Variant.Id.ToString("D"), StringComparer.Ordinal)];
        List<Guid> ids = [.. changed.Select(r => r.Variant.Id)];
        Dictionary<Guid, int?> current = await _db.Variants.AsNoTracking()
            .Where(v => ids.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, v => v.StockQuantity, ct);
        if (changed.FirstOrDefault(r => r.Input.IsShownStockUnreadable || current[r.Variant.Id] != r.Input.ShownStockQuantity) is { Variant: not null } stale)
        {
            return ProductSaveResult.ShownStockChanged(Field(stale.Row, nameof(ProductSaveVariant.StockQuantity)));
        }

        foreach ((int row, Variant variant, ProductSaveVariant input) in changed)
        {
            StockQuantityWriteResult written = await _counters.SaveStockQuantityAsync(variant.Id, input.ShownStockQuantity, input.StockQuantity, ct);
            if (written.IsShownValueChanged)
            {
                return ProductSaveResult.ShownStockChanged(Field(row, nameof(ProductSaveVariant.StockQuantity)));
            }

            if (written.IsBelowReserved)
            {
                return ProductSaveResult.BelowReserved(Field(row, nameof(ProductSaveVariant.StockQuantity)), written.ReservedQuantity ?? 0);
            }
        }

        return ProductSaveResult.Saved(productId);
    }

    private static BoundValue Bound(Guid? valueId, List<Option> options, List<OptionValue> values)
    {
        if (valueId is not Guid id)
        {
            return BoundValue.None;
        }

        OptionValue? value = values.FirstOrDefault(v => v.Id == id);
        return BoundValue.Of(value is null ? null : options.FirstOrDefault(o => o.Id == value.OptionId)?.SortOrder);
    }

    private static bool IsBlank(ProductSaveVariant row)
        => row.FirstOptionValueId is null && row.SecondOptionValueId is null && string.IsNullOrWhiteSpace(row.SKU) && row.Price is null && row.NetQuantity is null
            && row.StockQuantity is null && row.ImageId is null && row.DigitalFileId is null;

    private static string Field(int row, string name) => $"Variants[{row}].{name}";
}
