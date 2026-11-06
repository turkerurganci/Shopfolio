using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// A version of one of the four legal texts (06 4.56). A published row is only ever added: it is never updated and never deleted
/// (<see cref="ProtectedRecordsGuard"/>), so every version frozen into an order stays readable. The row of a firm-edited text (the privacy
/// notice and the cookie policy) that has no publish time is the draft, at most one per type; it stays in place after a publication and is the row
/// of the next edit. The two generated texts have no draft: their versions are written when the settings that feed them change.
/// </summary>
internal sealed class LegalTextVersion : IVersionedRecord
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public LegalTextType LegalTextType { get; set; }

    /// <summary>The increasing number within the type; empty on a draft. Never typed by hand.</summary>
    public int? VersionNumber { get; set; }

    /// <summary>The moment the row came into force as a version; empty means the row is a draft.</summary>
    public DateTimeOffset? PublishedAt { get; set; }

    public string Text { get; set; } = string.Empty;

    /// <summary>The hosting and e-mail infrastructure location; only on the privacy notice.</summary>
    public string? HostingLocation { get; set; }

    /// <summary>The version id of the template that came with the product; only on a published generated text.</summary>
    public string? TemplateVersion { get; set; }

    public int Version { get; private set; } = 1;

    /// <summary>Raises the version by one; every save of the text calls it (06 2.6.2; 09 10.2 item 1).</summary>
    public void BumpVersion() => Version++;

    public bool IsDraft => PublishedAt is null;

    public static LegalTextVersion CreateDraft(LegalTextType type, string text, DateTimeOffset now)
    {
        return new LegalTextVersion { Id = EntityIds.New(), CreatedAt = now, LegalTextType = type, Text = text };
    }

    public static LegalTextVersion CreatePublished(LegalTextType type, int versionNumber, DateTimeOffset publishedAt, string text, DateTimeOffset now)
    {
        return new LegalTextVersion
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            LegalTextType = type,
            VersionNumber = versionNumber,
            PublishedAt = publishedAt,
            Text = text,
        };
    }
}

internal sealed class LegalTextVersionConfiguration : IEntityTypeConfiguration<LegalTextVersion>
{
    private const string FirmEdited = $"('{nameof(LegalTextType.PrivacyNotice)}', '{nameof(LegalTextType.CookiePolicy)}')";

    private const string Generated = $"('{nameof(LegalTextType.PreInformationForm)}', '{nameof(LegalTextType.DistanceSalesContract)}')";

    public void Configure(EntityTypeBuilder<LegalTextVersion> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Ignore(l => l.IsDraft);
        builder.Property(l => l.Text).HasColumnType("text");
        builder.Property(l => l.Version).HasDefaultValue(1);

        // The number is unique within the type among filled rows; there is at most one draft per type (06 4.56).
        builder.HasIndex(l => new { l.LegalTextType, l.VersionNumber }).IsUnique().HasFilter("version_number IS NOT NULL");
        builder.HasIndex(l => l.LegalTextType).IsUnique().HasFilter("published_at IS NULL");

        builder.HasRule("number_and_published_at_together", "(version_number IS NULL) = (published_at IS NULL)");
        builder.HasRule("draft_only_for_firm_edited_text", $"published_at IS NOT NULL OR legal_text_type IN {FirmEdited}");
        builder.HasRule("template_version_only_generated", $"template_version IS NULL OR (published_at IS NOT NULL AND legal_text_type IN {Generated})");
        builder.HasRule(
            "hosting_location_only_on_privacy_notice",
            $"(hosting_location IS NULL OR legal_text_type = '{nameof(LegalTextType.PrivacyNotice)}') "
            + $"AND (legal_text_type <> '{nameof(LegalTextType.PrivacyNotice)}' OR published_at IS NULL OR hosting_location IS NOT NULL)");
    }
}
