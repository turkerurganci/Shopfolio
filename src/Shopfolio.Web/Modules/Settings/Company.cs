using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// The single company of the installation: legal identity and contact details (06 4.1). The row is born empty at installation (the type is
/// not chosen) and is never deleted. The fields a company type requires are rules of the application, not of the database: the row
/// cannot carry a database requirement because it starts empty.
/// </summary>
internal sealed class Company
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public CompanyType? CompanyType { get; set; }

    public string? LegalName { get; set; }

    public string? TaxIdNumber { get; set; }

    public string? MersisNumber { get; set; }

    public string? TradeRegistryNumber { get; set; }

    public Guid? ProvinceId { get; set; }

    public string? District { get; set; }

    public string? AddressLine { get; set; }

    /// <summary>Ten digits, without the leading zero or "+90" (06 2.5.1).</summary>
    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? KepAddress { get; set; }

    public string? ChamberName { get; set; }

    public string? BusinessName { get; set; }

    public string? ProfessionalConductRules { get; set; }

    public string? EtbisVerificationUrl { get; set; }

    /// <summary>The one version of the identity form (06 2.6.3); a concurrency token.</summary>
    public int Version { get; private set; } = 1;

    public static Company Create(DateTimeOffset now)
    {
        return new Company { Id = EntityIds.New(), CreatedAt = now };
    }

    public void BumpVersion() => Version++;
}

internal sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Phone).HasMaxLength(10);
        builder.HasOne<Province>().WithMany().HasForeignKey(c => c.ProvinceId).OnDelete(DeleteBehavior.NoAction);

        builder.HasRule("phone_ten_digits", "phone ~ '^[0-9]{10}$'");

        // The single row (06 2.3.2) is kept by a unique index on a constant expression; EF cannot model it, so the migration writes it.
    }
}
