using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// What the corporate content records share (06 2.9.2, 4.46-4.52, 4.72): the two-valued publish status, the field lengths of 06 11.2 and
/// the format of the page address. The status is the same enum the catalog uses; the content records use only <c>Draft</c> and
/// <c>Published</c>, and the column's check constraint lists exactly those.
/// </summary>
internal static class ContentRules
{
    /// <summary>A name or title (06 11.2 P-27).</summary>
    public const int TitleMaxLength = 150;

    /// <summary>A short description or a question (06 11.2 P-28).</summary>
    public const int ShortDescriptionMaxLength = 200;

    /// <summary>The short introduction of the about page (06 11.2 P-29).</summary>
    public const int ShortIntroductionMaxLength = 400;

    /// <summary>The announcement text (06 11.2 P-30).</summary>
    public const int AnnouncementTextMaxLength = 120;

    /// <summary>Lowercase ASCII letters and digits joined by single hyphens, never starting or ending with one (06 4.72.4).</summary>
    public const string SlugFormat = "slug ~ '^[a-z0-9]+(-[a-z0-9]+)*$'";

    /// <summary>The publish status of a content record: draft by default, and only draft or published (06 2.9.2; K-1026 (2)).</summary>
    public static PropertyBuilder<PublishStatus> AsContentPublishStatus(this PropertyBuilder<PublishStatus> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.HasDefaultValue(PublishStatus.Draft).HasAllowedValues(PublishStatus.Draft, PublishStatus.Published);
    }
}
