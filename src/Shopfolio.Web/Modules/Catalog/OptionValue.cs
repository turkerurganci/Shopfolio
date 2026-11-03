using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The value an option takes ("Red", "M") and its order (06 4.8). It goes with its option; a variant's link to it is restrictive, so a value
/// bound to a variant cannot be deleted alone. The order is not unique: reordering changes rows.
/// </summary>
internal sealed class OptionValue
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid OptionId { get; set; }

    public string Value { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public static OptionValue Create(Guid optionId, string value, int sortOrder, DateTimeOffset now)
    {
        return new OptionValue { Id = EntityIds.New(), CreatedAt = now, OptionId = optionId, Value = value, SortOrder = sortOrder };
    }
}

internal sealed class OptionValueConfiguration : IEntityTypeConfiguration<OptionValue>
{
    public void Configure(EntityTypeBuilder<OptionValue> builder)
    {
        builder.HasKey(v => v.Id);
        builder.HasOne<Option>().WithMany().HasForeignKey(v => v.OptionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(v => new { v.OptionId, v.SortOrder });
    }
}
