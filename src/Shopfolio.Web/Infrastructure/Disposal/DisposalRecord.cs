using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Disposal;

/// <summary>
/// The disposal record (06 4.64): the date, the record type and the number of every periodic disposal's part and of every operation that
/// deletes personal data at once; it carries no personal data. "Insert only" is a rule of the application (the disposal never updates a row),
/// not a database privilege (06 2.2.3). The row's date is its creation moment. The server's log rotation is the one type without a number:
/// it carries the range of the days the rotation deleted.
/// </summary>
internal sealed class DisposalRecord
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DisposalRecordType DisposalRecordType { get; set; }

    /// <summary>The number of deleted or emptied records (of files, in a part that deletes files); empty only on the server's log rotation.</summary>
    public int? Count { get; set; }

    /// <summary>The first day of the range the server's log rotation deleted; only on that type.</summary>
    public DateOnly? RangeStartDate { get; set; }

    /// <summary>The last day of the range; only on the server's log rotation.</summary>
    public DateOnly? RangeEndDate { get; set; }

    /// <summary>A row with the number of deleted or emptied records.</summary>
    public static DisposalRecord Counted(DisposalRecordType disposalRecordType, int count, DateTimeOffset now)
    {
        return new DisposalRecord { Id = EntityIds.New(), CreatedAt = now, DisposalRecordType = disposalRecordType, Count = count };
    }

    /// <summary>The server's log rotation: the range of the deleted days, no number.</summary>
    public static DisposalRecord ServerLogRotation(DateOnly rangeStartDate, DateOnly rangeEndDate, DateTimeOffset now)
    {
        return new DisposalRecord
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            DisposalRecordType = DisposalRecordType.ServerLogRotation,
            RangeStartDate = rangeStartDate,
            RangeEndDate = rangeEndDate,
        };
    }
}

internal sealed class DisposalRecordConfiguration : IEntityTypeConfiguration<DisposalRecord>
{
    private const string ServerLogRotation = nameof(DisposalRecordType.ServerLogRotation);

    public void Configure(EntityTypeBuilder<DisposalRecord> builder)
    {
        builder.HasKey(d => d.Id);

        // No link to any table (06 4.64). The time scan of Z-44 and the reading command's date range (06 7.5).
        builder.HasIndex(d => d.CreatedAt);

        // 06 4.64, 7.3 (K-1068): the server's log rotation has no number and both dates; every other type has a number above zero and no
        // date; the range never ends before it starts. The type column is never empty, so each arm decides on its own.
        builder.HasRule(
            "count_and_range_by_type",
            $"CASE WHEN disposal_record_type = '{ServerLogRotation}'"
            + " THEN count IS NULL AND range_start_date IS NOT NULL AND range_end_date IS NOT NULL"
            + " ELSE count IS NOT NULL AND count > 0 AND range_start_date IS NULL AND range_end_date IS NULL END");
        builder.HasRule("range_end_not_before_start", "range_end_date >= range_start_date");
    }
}
