using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.Settings;

/// <summary>The country list of the product's place of origin (06 4.4): a start-up data table, never deleted.</summary>
internal sealed class Country
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Two-letter international country code ("TR").</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Turkish name of the country.</summary>
    public string Name { get; set; } = string.Empty;

    public static Country Create(string code, string name, DateTimeOffset now)
    {
        return new Country { Id = EntityIds.New(), CreatedAt = now, Code = code, Name = name };
    }
}

internal sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.Code).IsUnique();
        builder.HasRule("code_two_capital_letters", "code ~ '^[A-Z]{2}$'");
    }
}
