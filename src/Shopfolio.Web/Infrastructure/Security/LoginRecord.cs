using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The sign-in record (06 4.44): every sign-in attempt, successful or not, customer or administrator, is one row. The row carries values and
/// <b>no link to any table</b> (06 2.2.6): the normalized email is a value, so an account's deletion leaves it; the retention period removes
/// it. The row is only ever inserted: every value has a private setter, <see cref="Create"/> is the one way to make a row and
/// <see cref="LoginRecordsGuard"/> refuses a changed row before any SQL is sent (06 4.44; 2.2.3).
/// </summary>
internal sealed class LoginRecord
{
    public Guid Id { get; private set; }

    /// <summary>The sign-in's moment: the row's creation moment (06 2.8.4).</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    public IPAddress IpAddress { get; private set; } = IPAddress.None;

    public AccountType AccountType { get; private set; }

    /// <summary>The entered email in its normalized form (06 2.8.1); for Google sign-in the verified address Google returned.</summary>
    public string NormalizedEmail { get; private set; } = string.Empty;

    public LoginOutcome Outcome { get; private set; }

    /// <summary>The one way to make a row. The entered address is normalized here (09 9.4 item 1; D-64), so the row never holds a value the caller normalized.</summary>
    public static LoginRecord Create(IPAddress ipAddress, AccountType accountType, string email, LoginOutcome outcome, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);
        ArgumentNullException.ThrowIfNull(email);
        return new LoginRecord { Id = EntityIds.New(), CreatedAt = now, IpAddress = ipAddress, AccountType = accountType, NormalizedEmail = TextNormalizer.NormalizeIdentity(email), Outcome = outcome };
    }
}

internal sealed class LoginRecordConfiguration : IEntityTypeConfiguration<LoginRecord>
{
    public void Configure(EntityTypeBuilder<LoginRecord> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.IpAddress).HasColumnType("inet");
        builder.Property(l => l.NormalizedEmail).HasColumnType("text");

        // No relationship to any table: the email is a value (06 2.2.6).
        builder.HasIndex(l => l.CreatedAt);
        builder.HasIndex(l => l.NormalizedEmail);
        builder.HasIndex(l => l.IpAddress);
    }
}

/// <summary>
/// Refuses the write the sign-in record never allows (06 4.44; 2.2.3): an update. The save is refused before any SQL is sent, so there is no
/// code path that changes a sign-in row. A bulk statement does not pass through a save and is not covered here; the retention job deletes
/// by the database, not by an update.
/// </summary>
internal sealed class LoginRecordsGuard : SaveChangesInterceptor
{
    public static LoginRecordsGuard Instance { get; } = new();

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Check(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Check(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private static void Check(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (EntityEntry entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Modified && entry.Entity is LoginRecord)
            {
                throw new InvalidOperationException("A sign-in record is only appended and never updated (06 4.44, 2.2.3).");
            }
        }
    }
}
