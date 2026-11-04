using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// A digital product file: one row per upload, every upload a new row (06 4.70). The file lives on the product level, a variant may carry its
/// own; each level has one current file, kept by the database: the product link and the variant link are each unique among the rows not
/// detached. The upload is chunked; until it completes, the links and the attach moment stay empty. The product and variant links are emptied
/// when the owner is deleted, so the row (and the file an order item may still point to) stays for the disposal job.
/// </summary>
internal sealed class DigitalFile
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The key in the file store: an unguessable name (<see cref="EntityIds.NewStorageKey"/>).</summary>
    public string StorageKey { get; set; } = string.Empty;

    /// <summary>The name of the uploaded file.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>The size declared at the start of the upload, in bytes; above zero.</summary>
    public long SizeBytes { get; set; }

    /// <summary>The moment the upload completed; empty while the chunked upload is running.</summary>
    public DateTimeOffset? UploadCompletedAt { get; set; }

    public Guid? ProductId { get; set; }

    public Guid? VariantId { get; set; }

    public DateTimeOffset? AttachedAt { get; set; }

    /// <summary>The moment the file was removed or replaced; a detached row is not the current file.</summary>
    public DateTimeOffset? DetachedAt { get; set; }

    public static DigitalFile Create(string storageKey, string fileName, long sizeBytes, DateTimeOffset now)
    {
        return new DigitalFile { Id = EntityIds.New(), CreatedAt = now, StorageKey = storageKey, FileName = fileName, SizeBytes = sizeBytes };
    }
}

internal sealed class DigitalFileConfiguration : IEntityTypeConfiguration<DigitalFile>
{
    public void Configure(EntityTypeBuilder<DigitalFile> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.FileName).HasColumnType("text");
        builder.HasOne<Product>().WithMany().HasForeignKey(d => d.ProductId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<Variant>().WithMany().HasForeignKey(d => d.VariantId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(d => d.StorageKey).IsUnique();

        // The plain indexes serve the emptying of the link and the reading of the latest file; the filtered unique indexes keep one current
        // file per level. Two indexes on the same column are told apart by the model's index name (EF keys an unnamed index by its columns).
        builder.HasIndex(d => d.ProductId);
        builder.HasIndex(d => d.VariantId);
        builder.HasIndex(d => d.ProductId, CurrentProductFileIndex).IsUnique().HasFilter("detached_at IS NULL");
        builder.HasIndex(d => d.VariantId, CurrentVariantFileIndex).IsUnique().HasFilter("detached_at IS NULL");

        // The nightly clean-up finds the incomplete uploads by their age.
        builder.HasIndex(d => d.CreatedAt).HasFilter("upload_completed_at IS NULL");

        builder.HasRule("at_most_one_owner", "product_id IS NULL OR variant_id IS NULL");
        builder.HasRule("incomplete_upload_unattached", "upload_completed_at IS NOT NULL OR (product_id IS NULL AND variant_id IS NULL AND attached_at IS NULL)");
        builder.HasRule("detached_needs_attached", "detached_at IS NULL OR attached_at IS NOT NULL");
        builder.HasRule("size_above_zero", "size_bytes > 0");
    }

    internal const string CurrentProductFileIndex = "CurrentProductFile";

    internal const string CurrentVariantFileIndex = "CurrentVariantFile";
}
