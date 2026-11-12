using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Lists;
using Shopfolio.Pages.Panel;

namespace Shopfolio.UnitTests.Pages;

/// <summary>The words and shapes of the panel's requests list and the contact request's screen (T532; 04 9.10, 9.11; 07 UC-93, UC-94).</summary>
public sealed class RequestScreensModelTests
{
    [Theory]
    [InlineData(PanelRequestKind.ContactRequest, "İletişim talebi")]
    [InlineData(PanelRequestKind.DefectClaim, "Ayıp talebi")]
    public void KindOf_NamesTheTwoKinds(PanelRequestKind kind, string expected)
    {
        Assert.Equal(expected, RequestTexts.KindOf(kind));
    }

    [Theory]
    [InlineData(PanelRequestStatus.Open, "Açık")]
    [InlineData(PanelRequestStatus.Closed, "Kapatıldı")]
    [InlineData(PanelRequestStatus.Resolved, "Çözüldü")]
    public void StatusOf_NamesTheThreeFilterStatuses(PanelRequestStatus status, string expected)
    {
        Assert.Equal(expected, RequestTexts.StatusOf(status));
    }

    [Theory]
    [InlineData(ContactTopic.GeneralQuestion, "Genel soru")]
    [InlineData(ContactTopic.AboutOrder, "Sipariş hakkında")]
    [InlineData(ContactTopic.AboutProduct, "Ürün hakkında")]
    [InlineData(ContactTopic.PersonalDataRequest, "KVKK talebi")]
    [InlineData(ContactTopic.Other, "Diğer")]
    public void TopicOf_NamesTheFiveTopics(ContactTopic topic, string expected)
    {
        Assert.Equal(expected, RequestTexts.TopicOf(topic));
    }

    [Fact]
    public void TopicOf_AndKindOf_CoverEveryMemberOfTheirEnum()
    {
        foreach (ContactTopic topic in Enum.GetValues<ContactTopic>())
        {
            Assert.False(string.IsNullOrWhiteSpace(RequestTexts.TopicOf(topic)));
        }

        foreach (PanelRequestKind kind in Enum.GetValues<PanelRequestKind>())
        {
            Assert.False(string.IsNullOrWhiteSpace(RequestTexts.KindOf(kind)));
        }

        foreach (PanelRequestStatus status in Enum.GetValues<PanelRequestStatus>())
        {
            Assert.False(string.IsNullOrWhiteSpace(RequestTexts.StatusOf(status)));
        }
    }

    [Theory]
    [InlineData(PanelRequestKind.ContactRequest, PanelRequestStatus.Open, "Açık")]
    [InlineData(PanelRequestKind.ContactRequest, PanelRequestStatus.Closed, "Kapatıldı")]
    [InlineData(PanelRequestKind.DefectClaim, PanelRequestStatus.Open, "Açık")]
    [InlineData(PanelRequestKind.DefectClaim, PanelRequestStatus.Resolved, "Çözüldü")]
    public void BadgeOf_GivesTheBadgeOfTheKindsOwnStatus(PanelRequestKind kind, PanelRequestStatus status, string text)
    {
        Assert.Equal(text, RequestTexts.BadgeOf(kind, status).Text);
    }

    [Theory]
    [InlineData(PanelRequestKind.ContactRequest, PanelRequestStatus.Resolved)]
    [InlineData(PanelRequestKind.DefectClaim, PanelRequestStatus.Closed)]
    public void BadgeOf_RefusesAStatusThatDoesNotFitTheKind(PanelRequestKind kind, PanelRequestStatus status)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RequestTexts.BadgeOf(kind, status));
    }

    [Fact]
    public void ItemNameOf_WritesTheProductThenTheOptionValuesInParentheses()
    {
        Guid id = Guid.NewGuid();

        Assert.Equal("Tişört", PanelRequestListService.ItemNameOf(new OrderItemLabel(id, id, "S-1", "Tişört", null, null)));
        Assert.Equal("Tişört (Kırmızı)", PanelRequestListService.ItemNameOf(new OrderItemLabel(id, id, "S-1", "Tişört", "Kırmızı", null)));
        Assert.Equal("Tişört (Kırmızı, M)", PanelRequestListService.ItemNameOf(new OrderItemLabel(id, id, "S-1", "Tişört", "Kırmızı", "M")));
        Assert.Equal("Tişört (M)", PanelRequestListService.ItemNameOf(new OrderItemLabel(id, id, "S-1", "Tişört", " ", "M")));
    }

    [Fact]
    public void DefectClaimFragmentOf_IsThePartIdentifierOfTheClaimsRow()
    {
        Guid id = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e");

        Assert.Equal("#ayip-talebi-0f8fad5b-d9cb-469f-a165-70867728950e", RequestsModel.DefectClaimFragmentOf(id));
    }

    [Fact]
    public void PanelTableRow_DrawsABadgeOnlyInTheColumnItNames_AndTheTableRefusesAColumnOutsideItself()
    {
        PanelTableColumn[] columns = [new("Ad", true), new("Durum", false)];
        Badge badge = new("Açık", BadgeTone.Info);
        PanelTableRow row = new(["x", "Açık"], null, new Dictionary<int, Badge> { [1] = badge });

        Assert.Null(row.BadgeAt(0));
        Assert.Equal(badge, row.BadgeAt(1));
        Assert.Null(new PanelTableRow(["x", "y"], null).BadgeAt(1));
        Assert.Throws<ArgumentException>(() => new PanelTable("T", columns, [new PanelTableRow(["x", "y"], null, new Dictionary<int, Badge> { [2] = badge })]));
        _ = new PanelTable("T", columns, [row]);
    }

    [Fact]
    public void TheDetailScreen_KeepsTheRetentionLinesApart()
    {
        Assert.NotEqual(RequestDetailModel.RetentionRunsText, RequestDetailModel.RetentionKeepsRunningText);
        Assert.Contains("devam", RequestDetailModel.RetentionKeepsRunningText, StringComparison.Ordinal);
        Assert.Equal("Talebi kapat", RequestDetailModel.CloseLabel);
        Assert.Equal("Yeniden aç", RequestDetailModel.ReopenLabel);
    }
}
