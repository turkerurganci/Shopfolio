using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.UnitTests.Samples.Good;

/// <summary>A sample record that follows the model rules: one property of every storage type of 06 2.5.1, a version and a check rule.</summary>
public sealed class SampleOwner
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public int Version { get; private set; } = 1;

    public string Name { get; set; } = string.Empty;

    public string? Note { get; set; }

    public string Code { get; set; } = string.Empty;

    public string NormalizedCode { get; set; } = string.Empty;

    public string LongText { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public decimal VatRate { get; set; }

    public decimal NetQuantity { get; set; }

    public int Count { get; set; }

    public long ByteSize { get; set; }

    public bool IsActive { get; set; }

    public DateOnly StartDate { get; set; }

    public IPAddress ClientAddress { get; set; } = IPAddress.Loopback;

    public string Payload { get; set; } = "[]";

    public static SampleOwner Create(string code, DateTimeOffset now)
    {
        return new SampleOwner
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            Name = "Sample",
            Code = code,
            NormalizedCode = TextNormalizer.NormalizeIdentity(code),
            LongText = "<p>text</p>",
            StartDate = DateOnly.FromDateTime(now.UtcDateTime),
        };
    }

    public void BumpVersion() => Version++;
}

internal sealed class SampleOwnerConfiguration : IEntityTypeConfiguration<SampleOwner>
{
    public void Configure(EntityTypeBuilder<SampleOwner> builder)
    {
        builder.ToTable("sample_owners");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Code).HasMaxLength(50);
        builder.Property(o => o.LongText).HasColumnType("text");
        builder.Property(o => o.Payload).HasColumnType("jsonb");
        builder.Property(o => o.Price).AsMoney();
        builder.Property(o => o.VatRate).AsPercent();
        builder.Property(o => o.NetQuantity).AsNetQuantity();
        builder.HasIndex(o => o.NormalizedCode).IsUnique();
        builder.HasRule("price_not_negative", "price >= 0");
    }
}
