using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Pages.Panel;

namespace Shopfolio.UnitTests.Pages;

/// <summary>The closed address list of the corporate content's types and the enums the panel mirrors (T534; 07 3.1.1; 04 9.13.3).</summary>
public sealed class ContentRoutesTests
{
    [Fact]
    public void EveryType_HasOneSegment_TheSegmentParsesBack_AndTheSegmentsAreDistinct()
    {
        PanelContentType[] types = Enum.GetValues<PanelContentType>();

        string[] segments = [.. types.Select(ContentRoutes.SegmentOf)];

        Assert.Equal(7, types.Length);
        Assert.Equal(types.Length, segments.Distinct(StringComparer.Ordinal).Count());
        foreach (PanelContentType type in types)
        {
            Assert.True(ContentRoutes.TryParse(ContentRoutes.SegmentOf(type), out PanelContentType parsed));
            Assert.Equal(type, parsed);
            Assert.False(string.IsNullOrWhiteSpace(ContentRoutes.GroupNameOf(type)));
            Assert.False(string.IsNullOrWhiteSpace(ContentRoutes.NewLabelOf(type)));
            Assert.False(string.IsNullOrWhiteSpace(ContentRoutes.EmptyTextOf(type)));
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("HAKKIMIZDA")]
    [InlineData("urun")]
    [InlineData("hizmet/")]
    public void ASegmentOutsideTheClosedList_IsNoType(string? segment) => Assert.False(ContentRoutes.TryParse(segment, out _));

    [Fact]
    public void TheFormPaths_AreUnderTheRoot_WithTheKeyOrTheNewSegment()
    {
        Guid id = Guid.CreateVersion7();

        Assert.Equal($"/panel/kurumsal-icerik/hizmet/{id:D}", ContentRoutes.FormPath(PanelContentType.Offering, id));
        Assert.Equal("/panel/kurumsal-icerik/sss/yeni", ContentRoutes.NewPath(PanelContentType.FaqItem));
    }

    [Fact]
    public void OnlyTheAboutPageAndTheAnnouncement_AreSingleRecords()
    {
        PanelContentType[] singles = [.. Enum.GetValues<PanelContentType>().Where(ContentRoutes.IsSingle)];

        Assert.Equal([PanelContentType.AboutPage, PanelContentType.Announcement], singles);
    }

    [Fact]
    public void ThePanelContentType_HasTheNamesAndOrderOfTheContentRecordKind()
    {
        Assert.Equal(Enum.GetNames<ContentRecordKind>(), Enum.GetNames<PanelContentType>());
    }

    [Fact]
    public void ThePanelContentField_HasTheNamesAndOrderOfTheGateField()
    {
        Assert.Equal(Enum.GetNames<ContentGateField>(), Enum.GetNames<PanelContentField>());
    }
}
