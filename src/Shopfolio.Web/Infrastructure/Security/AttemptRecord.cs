using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The attempt record (06 4.43): a technical record of the attempt limits L1-L6 and L9 (L7 and L8 are counted from the orders). Each counted
/// attempt is one row. The row carries values and <b>no link to any table</b> (06 2.2.6): the email, the IP, the cart id or the recognized
/// browser mark id is a value, so an account's deletion does not touch it; the row lives for its own window. The axis is the limit's axis
/// (a check constraint, 02 8.2).
/// </summary>
internal sealed class AttemptRecord
{
    public Guid Id { get; set; }

    /// <summary>The attempt's moment: the row's creation moment (06 2.8.4).</summary>
    public DateTimeOffset CreatedAt { get; set; }

    public AttemptLimit AttemptLimit { get; set; }

    public AttemptAxis AttemptAxis { get; set; }

    /// <summary>The axis value: a normalized email, an IP text (IPv6 as its /64 prefix), a cart id or a recognized browser mark id.</summary>
    public string NormalizedValue { get; set; } = string.Empty;

    public static AttemptRecord Create(AttemptLimit attemptLimit, AttemptAxis attemptAxis, string normalizedValue, DateTimeOffset now)
    {
        return new AttemptRecord { Id = EntityIds.New(), CreatedAt = now, AttemptLimit = attemptLimit, AttemptAxis = attemptAxis, NormalizedValue = normalizedValue };
    }
}

internal sealed class AttemptRecordConfiguration : IEntityTypeConfiguration<AttemptRecord>
{
    public void Configure(EntityTypeBuilder<AttemptRecord> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.NormalizedValue).HasColumnType("text");

        // The table counts L1-L6 and L9 (06 4.43; 2.9.2).
        builder.Property(a => a.AttemptLimit).HasAllowedValues(AttemptLimit.L1, AttemptLimit.L2, AttemptLimit.L3, AttemptLimit.L4, AttemptLimit.L5, AttemptLimit.L6, AttemptLimit.L9);

        // No relationship to any table: every axis value is a value (06 2.2.6).
        builder.HasIndex(a => new { a.AttemptLimit, a.AttemptAxis, a.NormalizedValue, a.CreatedAt });
        builder.HasIndex(a => a.CreatedAt);

        // The axis is the limit's axis (06 4.43; 02 8.2): L4 only the IP, L6 only the cart, the recognized browser only L1, the rest the email or the IP.
        builder.HasRule(
            "axis_is_the_limits_axis",
            "(attempt_limit = 'L4' AND attempt_axis = 'Ip') "
            + "OR (attempt_limit = 'L6' AND attempt_axis = 'Cart') "
            + "OR (attempt_limit = 'L1' AND attempt_axis = 'RecognizedBrowser') "
            + "OR (attempt_limit IN ('L1', 'L2', 'L3', 'L5', 'L9') AND attempt_axis IN ('Email', 'Ip'))");
    }
}
