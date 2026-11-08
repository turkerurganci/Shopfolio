using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Infrastructure.Security;
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
    public const int TitleMaxLength = ProductConstants.ProductNameMaxLength;

    /// <summary>A short description or a question (06 11.2 P-28).</summary>
    public const int ShortDescriptionMaxLength = ProductConstants.ShortTextMaxLength;

    /// <summary>The short introduction of the about page (06 11.2 P-29).</summary>
    public const int ShortIntroductionMaxLength = ProductConstants.ShortIntroductionMaxLength;

    /// <summary>The announcement text (06 11.2 P-30).</summary>
    public const int AnnouncementTextMaxLength = ProductConstants.AnnouncementTextMaxLength;

    /// <summary>Lowercase ASCII letters and digits joined by single hyphens, never starting or ending with one (06 4.72.4).</summary>
    public const string SlugFormat = "slug ~ '^[a-z0-9]+(-[a-z0-9]+)*$'";

    /// <summary>
    /// The long text of a record as it is kept (06 2.10.1; 09 15.3 item 1): the closed text format set's cleaning is applied here and only here, and an
    /// empty text - or one the cleaner empties - is no text. Every long text setter of the module calls this.
    /// </summary>
    public static string? CleanLongText(string? text)
    {
        string cleaned = RichTextSanitizer.Sanitize(text);
        return cleaned.Length == 0 ? null : cleaned;
    }

    /// <summary>The publish status of a content record: draft by default, and only draft or published (06 2.9.2; K-1026 (2)).</summary>
    public static PropertyBuilder<PublishStatus> AsContentPublishStatus(this PropertyBuilder<PublishStatus> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.HasDefaultValue(PublishStatus.Draft).HasAllowedValues(PublishStatus.Draft, PublishStatus.Published);
    }
}
