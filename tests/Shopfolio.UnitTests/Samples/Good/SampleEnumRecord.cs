using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.UnitTests.Samples.Good;

/// <summary>
/// A sample record with enum properties (T102): a whole enum, a subset of an enum, an optional enum and two enums whose code names
/// are short identifiers (<c>S1</c>, <c>B10</c>). No property writes its own conversion or constraint: the context's rules do both.
/// </summary>
public sealed class SampleEnumRecord
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public PublishStatus Status { get; set; }

    /// <summary>A field that uses only two members of the enum, as the corporate content does (06 2.9.2).</summary>
    public PublishStatus SubsetStatus { get; set; }

    public PaymentMethod? Method { get; set; }

    public TransitionCode Transition { get; set; }

    public DispatchType Dispatch { get; set; }

    public static SampleEnumRecord Create(DateTimeOffset now)
    {
        return new SampleEnumRecord
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            Status = PublishStatus.Archived,
            SubsetStatus = PublishStatus.Draft,
            Transition = TransitionCode.O1,
            Dispatch = DispatchType.B10,
        };
    }
}

internal sealed class SampleEnumRecordConfiguration : IEntityTypeConfiguration<SampleEnumRecord>
{
    public void Configure(EntityTypeBuilder<SampleEnumRecord> builder)
    {
        builder.ToTable("sample_enum_records");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.SubsetStatus).HasAllowedValues(PublishStatus.Draft, PublishStatus.Published);
    }
}
