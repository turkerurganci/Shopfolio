using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// A link to one of the firm's accounts on the closed list of platforms (06 4.55): at most one row per platform. It carries no link to the
/// settings row and no order (the storefront follows the list's order). An emptied form field deletes the row.
/// </summary>
internal sealed class SocialLink
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public SocialPlatform SocialPlatform { get; set; }

    public string Url { get; set; } = string.Empty;

    public static SocialLink Create(SocialPlatform socialPlatform, string url, DateTimeOffset now)
    {
        return new SocialLink { Id = EntityIds.New(), CreatedAt = now, SocialPlatform = socialPlatform, Url = url };
    }
}

internal sealed class SocialLinkConfiguration : IEntityTypeConfiguration<SocialLink>
{
    public void Configure(EntityTypeBuilder<SocialLink> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Url).HasColumnType("text");

        builder.HasIndex(s => s.SocialPlatform).IsUnique();
    }
}
