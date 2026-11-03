using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.Settings;

/// <summary>The closed list of the 81 provinces (06 4.3): a start-up data table, never deleted, its code never reused.</summary>
internal sealed class Province
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Plate code, two digits ("06").</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Turkish name of the province.</summary>
    public string Name { get; set; } = string.Empty;

    public static Province Create(string code, string name, DateTimeOffset now)
    {
        return new Province { Id = EntityIds.New(), CreatedAt = now, Code = code, Name = name };
    }
}

internal sealed class ProvinceConfiguration : IEntityTypeConfiguration<Province>
{
    public void Configure(EntityTypeBuilder<Province> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Code).IsUnique();
        builder.HasRule("code_two_digits", "code ~ '^[0-9]{2}$'");
    }
}
