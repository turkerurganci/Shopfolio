using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;

namespace Shopfolio.UnitTests.CorporateContent;

/// <summary>
/// Where a content record is seen (T322 criteria 4-5; KK4, KK5; 02 5.2, 3.27.21, 3.27.25; 03 1.8.4): a draft is not on the site and the administrator
/// previews it with the "Taslak" band; a published announcement with dates shows only inside its range, both ends inclusive.
/// </summary>
public sealed class ContentVisibilityTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly Today = new(2026, 11, 7);

    [Fact]
    public void ADraft_IsNotOnTheSite_AndTheAdministratorPreviewsItWithTheBand_KK4()
    {
        Assert.Equal(ContentShown.Hidden, ContentVisibility.Decide(PublishStatus.Draft, ContentAudience.Visitor));
        Assert.Equal(ContentShown.ShownWithDraftBand, ContentVisibility.Decide(PublishStatus.Draft, ContentAudience.Administrator));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void APublishedRecord_IsOnTheSite_ForEveryone_WithoutTheBand_KK4(bool administrator)
    {
        Assert.Equal(ContentShown.Shown, ContentVisibility.Decide(PublishStatus.Published, administrator ? ContentAudience.Administrator : ContentAudience.Visitor));
    }

    [Fact]
    public void TheArchive_IsNotAContentStatus_KK1()
    {
        Assert.Throws<InvalidOperationException>(() => ContentVisibility.Decide(PublishStatus.Archived, ContentAudience.Visitor));
    }

    [Theory]
    [InlineData(null, null, true)]
    [InlineData("2026-11-07", null, true)]
    [InlineData("2026-11-08", null, false)]
    [InlineData(null, "2026-11-07", true)]
    [InlineData(null, "2026-11-06", false)]
    [InlineData("2026-11-01", "2026-11-30", true)]
    [InlineData("2026-11-07", "2026-11-07", true)]
    [InlineData("2026-11-08", "2026-11-30", false)]
    [InlineData("2026-10-01", "2026-11-06", false)]
    public void TheRange_HoldsBothEndDays_AndAnEmptyDateIsNoLimit_KK5(string? start, string? end, bool expected)
    {
        Assert.Equal(expected, ContentVisibility.IsInRange(Parse(start), Parse(end), Today));
    }

    [Fact]
    public void APublishedAnnouncementOutsideItsDates_IsNotShownToAnyone_EvenThoughItIsPublished_KK5()
    {
        Announcement expired = Published(start: new DateOnly(2026, 10, 1), end: new DateOnly(2026, 10, 31));
        Announcement notYet = Published(start: new DateOnly(2026, 12, 1), end: null);

        foreach (ContentAudience audience in Enum.GetValues<ContentAudience>())
        {
            Assert.Equal(PublishStatus.Published, expired.PublishStatus);
            Assert.Equal(ContentShown.Hidden, ContentVisibility.Decide(expired, audience, Today));
            Assert.Equal(ContentShown.Hidden, ContentVisibility.Decide(notYet, audience, Today));
        }
    }

    [Fact]
    public void APublishedAnnouncementInsideItsDates_OrWithNone_IsShown_KK5()
    {
        Announcement inside = Published(start: new DateOnly(2026, 11, 1), end: new DateOnly(2026, 11, 30));
        Announcement undated = Published(start: null, end: null);

        Assert.Equal(ContentShown.Shown, ContentVisibility.Decide(inside, ContentAudience.Visitor, Today));
        Assert.Equal(ContentShown.Shown, ContentVisibility.Decide(undated, ContentAudience.Visitor, Today));
    }

    [Fact]
    public void ADraftAnnouncement_IsPreviewedWhateverItsDates_AndNeverShownToAVisitor_KK4()
    {
        Announcement draft = Announcement.Create(Now);
        draft.Text = "Yakında";
        draft.StartDate = new DateOnly(2026, 12, 1);

        Assert.Equal(ContentShown.ShownWithDraftBand, ContentVisibility.Decide(draft, ContentAudience.Administrator, Today));
        Assert.Equal(ContentShown.Hidden, ContentVisibility.Decide(draft, ContentAudience.Visitor, Today));
    }

    private static Announcement Published(DateOnly? start, DateOnly? end)
    {
        Announcement announcement = Announcement.Create(Now);
        announcement.Text = "Duyuru";
        announcement.StartDate = start;
        announcement.EndDate = end;
        announcement.PublishStatus = PublishStatus.Published;
        return announcement;
    }

    private static DateOnly? Parse(string? text) => text is null ? null : DateOnly.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
}
