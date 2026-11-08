using System.Reflection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;

namespace Shopfolio.UnitTests.CorporateContent;

/// <summary>
/// The announcement strip, the publish status and the preview rules of the corporate content (T360 criteria 1, 3, 4, 5, 8; KK1, KK3, KK4, KK5, KK8; 02 3.27.21-3.27.28; P-30): the
/// ceiling and the form of the fields, the date range, the one place a draft is shown, and the absence of a later-dated publication anywhere but the announcement.
/// </summary>
public sealed class AnnouncementAndPublishRulesTests
{
    private static readonly string[] ScheduleWords = ["Schedul", "PublishAt", "PublishOn", "PublishDate", "PublishFrom", "Embargo", "ReleaseAt", "GoLive"];

    private static readonly string[] AnnouncementExtrasWords = ["Image", "Color", "Colour", "Sort", "Order", "Slider", "Banner", "Campaign", "Position"];

    private static AnnouncementInput Input(string? text = "Bayram tatili", string? link = null, DateOnly? start = null, DateOnly? end = null) => new(text, link, start, end);

    // ---- KK1: the ceiling and the plain text ----

    [Fact]
    public void TheAnnouncementCeiling_IsTheProductConstantP30_AndTheTextAtTheCeilingIsAccepted_KK1()
    {
        Assert.Equal(120, ContentRules.AnnouncementTextMaxLength);
        Assert.Empty(ContentInputRules.Check(Input(new string('a', ContentRules.AnnouncementTextMaxLength))));
    }

    [Fact]
    public void ATextOneCharacterOverTheCeiling_IsRefused_ByTheTextField_KK1()
    {
        List<FieldRejection> rejections = ContentInputRules.Check(Input(new string('a', ContentRules.AnnouncementTextMaxLength + 1)));

        Assert.Equal([new FieldRejection(nameof(AnnouncementInput.Text), FieldCodes.TextTooLong)], rejections);
    }

    [Fact]
    public void TheCeiling_CountsTheStoredFormOfTheText_NotTheWhiteSpaceAroundIt_KK1()
    {
        string atCeiling = new('b', ContentRules.AnnouncementTextMaxLength);

        Assert.Empty(ContentInputRules.Check(Input("   " + atCeiling + "  ")));
        Assert.Equal(atCeiling, ContentInputRules.Text("   " + atCeiling + "  "));
    }

    [Fact]
    public void TheCeiling_CountsCharactersNotBytes_KK1()
    {
        Assert.Empty(ContentInputRules.Check(Input(new string('ş', ContentRules.AnnouncementTextMaxLength))));
        Assert.NotEmpty(ContentInputRules.Check(Input(new string('ş', ContentRules.AnnouncementTextMaxLength + 1))));
    }

    [Fact]
    public void TheText_IsKeptAsTyped_NoMarkupIsReadOutOfItOrTakenAway_SoThePageCanPrintItEscaped_KK1()
    {
        const string typed = "<b>İndirim</b> & <script>x</script>";

        Assert.Equal(typed, ContentInputRules.Text(typed));
        Assert.Empty(ContentInputRules.Check(Input(typed)));
    }

    [Fact]
    public void AnEmptyAnnouncement_IsAcceptedToBeSavedAsADraft_ThePublishGateAsksForTheText_KK4()
    {
        Assert.Empty(ContentInputRules.Check(new AnnouncementInput(null, null, null, null)));
        Assert.Empty(ContentInputRules.Check(new AnnouncementInput("   ", "  ", null, null)));
        Announcement empty = Announcement.Create(DateTimeOffset.UnixEpoch);
        Assert.Equal([ContentGateField.Text], empty.MissingGateFields());
    }

    [Theory]
    [InlineData("https://example.com/kampanya", true)]
    [InlineData("http://example.com", true)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("ftp://example.com/x", false)]
    [InlineData("example.com", false)]
    [InlineData("https://a b.example.com", false)]
    public void TheLink_IsKeptOnlyWhenItIsAnHttpOrHttpsAddress_KK1(string link, bool accepted)
    {
        List<FieldRejection> rejections = ContentInputRules.Check(Input(link: link));

        Assert.Equal(accepted, rejections.Count == 0);
        if (!accepted)
        {
            Assert.Equal([new FieldRejection(nameof(AnnouncementInput.LinkUrl), FieldCodes.InvalidUrl)], rejections);
        }
    }

    // ---- KK1: the date range ----

    [Fact]
    public void TheEndDayBeforeTheStartDay_IsRefusedOnTheEndField_KK1()
    {
        List<FieldRejection> rejections = ContentInputRules.Check(Input(start: new DateOnly(2026, 11, 10), end: new DateOnly(2026, 11, 9)));

        Assert.Equal([new FieldRejection(nameof(AnnouncementInput.EndDate), FieldCodes.InvalidDateRange)], rejections);
    }

    [Theory]
    [InlineData("2026-11-10", "2026-11-10")]
    [InlineData("2026-11-10", "2026-11-11")]
    [InlineData("2026-11-10", null)]
    [InlineData(null, "2026-11-09")]
    [InlineData(null, null)]
    public void ASingleDayRange_OrAnOpenOne_IsAccepted_KK1(string? start, string? end)
    {
        Assert.Empty(ContentInputRules.Check(Input(
            start: start is null ? null : DateOnly.Parse(start, System.Globalization.CultureInfo.InvariantCulture),
            end: end is null ? null : DateOnly.Parse(end, System.Globalization.CultureInfo.InvariantCulture))));
    }

    [Fact]
    public void TheAnnouncementHasOneTextAndOneLink_NoImageColourOrderOrSecondRecordField_KK1()
    {
        Assert.Equal(["EndDate", "LinkUrl", "StartDate", "Text"], typeof(AnnouncementInput).GetProperties().Select(p => p.Name).Order().ToArray());
        Assert.DoesNotContain(
            typeof(Announcement).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
            p => AnnouncementExtrasWords.Any(word => p.Name.Contains(word, StringComparison.OrdinalIgnoreCase)));
        Assert.Contains(ContentRecordKind.Announcement, ContentTypes.SingleRecord);
    }

    // ---- KK3: the way back goes to draft, no second approval ----

    [Fact]
    public void ThePublishOperations_AreTwoAndNeitherAsksForASecondApproval_KK3()
    {
        string[] publishMethods = [.. typeof(ContentPublishing).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(m => m.Name)
            .Where(n => n.StartsWith("Publish", StringComparison.Ordinal) || n.StartsWith("Unpublish", StringComparison.Ordinal))];

        Assert.Equal(["PublishAsync", "UnpublishAsync"], publishMethods.Order().ToArray());
        Assert.DoesNotContain(
            typeof(ContentPublishing).GetMethods().SelectMany(m => m.GetParameters()),
            p => p.Name!.Contains("approv", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("second", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(Enum.GetNames<ContentOutcome>(), name => name.Contains("Approval", StringComparison.Ordinal) || name.Contains("Pending", StringComparison.Ordinal));
    }

    // ---- KK5: who sees a draft ----

    [Fact]
    public void TheVisitor_SeesPublishedOnly_TheAdministratorSeesDraftsToo_AndNeitherSeesTheArchive_KK5()
    {
        Assert.Equal([PublishStatus.Published], ContentVisibility.VisibleStatuses(ContentAudience.Visitor));
        Assert.Equal(
            [PublishStatus.Draft, PublishStatus.Published],
            ContentVisibility.VisibleStatuses(ContentAudience.Administrator).Order().ToArray());
        Assert.Throws<InvalidOperationException>(() => ContentVisibility.Decide(PublishStatus.Archived, ContentAudience.Administrator));
    }

    [Fact]
    public void ADraftOfAnyKind_IsHiddenFromTheVisitor_AndMarkedWithTheBandForTheAdministrator_KK5()
    {
        Assert.Equal(Enum.GetValues<ContentRecordKind>().Order(), [.. ContentTypes.ReadyMade, .. ContentTypes.Added]);
        Assert.Equal(ContentShown.Hidden, ContentVisibility.Decide(PublishStatus.Draft, ContentAudience.Visitor));
        Assert.Equal(ContentShown.ShownWithDraftBand, ContentVisibility.Decide(PublishStatus.Draft, ContentAudience.Administrator));
    }

    // ---- KK8: no later-dated publication anywhere but the announcement's range ----

    [Fact]
    public void NoRecordButTheAnnouncement_HasADateThatPublishesItLater_KK8()
    {
        Type[] others = [typeof(AboutPage), typeof(Offering), typeof(PortfolioItem), typeof(FaqItem), typeof(Branch), typeof(CustomPage)];

        foreach (Type type in others)
        {
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                Assert.DoesNotContain(ScheduleWords, word => property.Name.Contains(word, StringComparison.OrdinalIgnoreCase));
                Assert.False(property.PropertyType == typeof(DateOnly) || property.PropertyType == typeof(DateOnly?), $"{type.Name}.{property.Name} is a day field.");
                Assert.DoesNotContain("Start", property.Name, StringComparison.Ordinal);
                Assert.DoesNotContain("End", property.Name, StringComparison.Ordinal);
            }
        }

        Assert.Contains(typeof(Announcement).GetProperties(), p => p.Name == nameof(Announcement.StartDate) && p.PropertyType == typeof(DateOnly?));
        Assert.Contains(typeof(Announcement).GetProperties(), p => p.Name == nameof(Announcement.EndDate) && p.PropertyType == typeof(DateOnly?));
    }

    [Fact]
    public void ThePublishOperations_TakeNoMoment_ThePublicationIsAlwaysNow_KK8()
    {
        foreach (MethodInfo method in typeof(ContentPublishing).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            Assert.DoesNotContain(
                method.GetParameters(),
                p => p.ParameterType == typeof(DateTimeOffset) || p.ParameterType == typeof(DateTimeOffset?) || p.ParameterType == typeof(DateOnly) || p.ParameterType == typeof(DateOnly?));
            Assert.DoesNotContain(ScheduleWords, word => method.Name.Contains(word, StringComparison.OrdinalIgnoreCase));
        }
    }
}
