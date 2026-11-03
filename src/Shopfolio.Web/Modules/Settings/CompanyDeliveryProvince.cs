using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// A province the company delivers to (06 4.2): the row's existence says the province is selected. It carries no link to the settings row
/// (there is one company and one settings row). The row is deleted when the company deselects the province.
/// </summary>
internal sealed class CompanyDeliveryProvince
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid ProvinceId { get; set; }

    public static CompanyDeliveryProvince Create(Guid provinceId, DateTimeOffset now)
    {
        return new CompanyDeliveryProvince { Id = EntityIds.New(), CreatedAt = now, ProvinceId = provinceId };
    }
}

internal sealed class CompanyDeliveryProvinceConfiguration : IEntityTypeConfiguration<CompanyDeliveryProvince>
{
    public void Configure(EntityTypeBuilder<CompanyDeliveryProvince> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.ProvinceId).IsUnique();
        builder.HasOne<Province>().WithMany().HasForeignKey(p => p.ProvinceId).OnDelete(DeleteBehavior.NoAction);
    }
}
