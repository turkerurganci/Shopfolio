using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>Who looks at a corporate content record: a visitor of the site, or the administrator previewing it from the panel (02 3.27.25).</summary>
internal enum ContentAudience
{
    Visitor,
    Administrator,
}

/// <summary>What a record looks like to its audience.</summary>
internal enum ContentShown
{
    /// <summary>Not on the site: a page answers "page not found", a record without a page of its own is left out of the lists.</summary>
    Hidden,

    /// <summary>On the site, as the visitor sees it.</summary>
    Shown,

    /// <summary>A draft the administrator previews, marked with the "Taslak" band (02 5.2; 03 1.8.4).</summary>
    ShownWithDraftBand,
}

/// <summary>
/// Where a corporate content record is seen (02 5.2; 03 1.8.4): a draft is not on the site and the administrator previews it with the "Taslak" band;
/// a published record is on the site, and an announcement with dates shows only inside its range (Z-23). The visibility inside the range is read from
/// the status and the dates and is never stored (06 4.52).
/// </summary>
internal static class ContentVisibility
{
    /// <summary>The visibility of a record other than the announcement.</summary>
    public static ContentShown Decide(PublishStatus status, ContentAudience audience)
    {
        return status switch
        {
            PublishStatus.Published => ContentShown.Shown,
            PublishStatus.Draft => audience == ContentAudience.Administrator ? ContentShown.ShownWithDraftBand : ContentShown.Hidden,
            _ => throw new InvalidOperationException($"A content record is never {status}: it is a draft or published (02 5.2)."),
        };
    }

    /// <summary>
    /// The announcement's visibility on <paramref name="today"/> (a Turkey day). A published announcement outside its dates is not on the site for
    /// anyone; a draft is previewed by the administrator whatever its dates, since the preview shows what the text will look like.
    /// </summary>
    public static ContentShown Decide(Announcement announcement, ContentAudience audience, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(announcement);
        ContentShown shown = Decide(announcement.PublishStatus, audience);
        if (announcement.PublishStatus == PublishStatus.Published && !IsInRange(announcement.StartDate, announcement.EndDate, today))
        {
            return ContentShown.Hidden;
        }

        return shown;
    }

    /// <summary>
    /// True when <paramref name="today"/> is inside the dates: the start day is the first day it shows and the end day the last, both inclusive; an
    /// empty date is no limit on that side (02 3.27.21; 06 2.5.2).
    /// </summary>
    public static bool IsInRange(DateOnly? start, DateOnly? end, DateOnly today)
    {
        return (start is null || today >= start) && (end is null || today <= end);
    }
}
