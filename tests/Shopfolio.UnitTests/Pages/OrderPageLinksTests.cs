using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Orders;

namespace Shopfolio.UnitTests.Pages;

/// <summary>The addresses of the order page and the buttons of its operations area (T511 KK4, KK5, KK7; 07 2.1.3; 04 5.16.6; 03 1.11).</summary>
public sealed class OrderPageLinksTests
{
    private const string MemberRootPath = "/siparis/2026-7K4M9P";

    private static OrderOperationsView ViewWith(params OrderOperationEntry[] open) => new(Guid.NewGuid(), ActorKind.Buyer, 1, open);

    private static OrderOperationEntry OnOrder(OrderOperation operation) => new(operation, null, OperationAvailability.Open());

    private static OrderOperationEntry OnItem(OrderOperation operation, Guid itemId) => new(operation, itemId, OperationAvailability.OpenForQuantity(1, 1, 1));

    // ---- Addresses (KK4, KK5) ----

    [Fact]
    public void MemberPath_IsTheMemberRootOfTheNumber_KK4()
    {
        Assert.Equal(MemberRootPath, OrderRoutes.MemberPath("2026-7K4M9P"));
        Assert.StartsWith("/" + AddressRules.OrderArea + "/", OrderRoutes.MemberPath("2026-7K4M9P"), StringComparison.Ordinal);
    }

    [Fact]
    public void AccessPath_KeepsTheKeyAsItIs_KK5()
    {
        const string key = "aB3-_xYz";
        Assert.Equal("/" + AddressRules.OrderArea + "/" + OrderRoutes.AccessSegment + "/" + key, OrderRoutes.AccessPath(key));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Paths_RefuseAnEmptyValue(string? value)
    {
        Assert.ThrowsAny<ArgumentException>(() => OrderRoutes.AccessPath(value!));
        Assert.ThrowsAny<ArgumentException>(() => OrderRoutes.MemberPath(value!));
    }

    [Fact]
    public void ScreenPath_SitsUnderTheRootItWasReachedFrom()
    {
        string accessRoot = OrderRoutes.AccessPath("aB3-_xYz");
        foreach (string screen in OrderRoutes.Screens)
        {
            Assert.Equal(MemberRootPath + "/" + screen, OrderRoutes.ScreenPath(MemberRootPath, screen));
            Assert.Equal(accessRoot + "/" + screen, OrderRoutes.ScreenPath(accessRoot, screen));
        }
    }

    [Fact]
    public void ScreenPath_RefusesAScreenThatIsNotAnOrderScreen() =>
        Assert.Throws<ArgumentException>(() => OrderRoutes.ScreenPath(MemberRootPath, "duzenle"));

    // ---- Operations area (KK7) ----

    [Fact]
    public void NoOpenOperation_DrawsNoButton_KK7() => Assert.Empty(OrderScreenLink.From(ViewWith(), MemberRootPath));

    [Fact]
    public void OperationsWithoutAScreen_DrawNoButton_KK7()
    {
        // Downloading, the transfer details and the IBAN field live in the page itself (04 5.16.6): none of them is a screen button.
        OrderOperationsView view = ViewWith(
            OnItem(OrderOperation.DownloadDigitalFile, Guid.NewGuid()),
            OnOrder(OrderOperation.ViewTransferDetails),
            OnItem(OrderOperation.EnterRefundIban, Guid.NewGuid()));
        Assert.Empty(OrderScreenLink.From(view, MemberRootPath));
    }

    [Fact]
    public void AnOperationOpenForSeveralItems_IsOneButton_KK7()
    {
        OrderOperationsView view = ViewWith(
            OnItem(OrderOperation.WithdrawPhysicalItem, Guid.NewGuid()),
            OnItem(OrderOperation.WithdrawPhysicalItem, Guid.NewGuid()),
            OnItem(OrderOperation.WithdrawDigitalItem, Guid.NewGuid()));
        OrderScreenLink link = Assert.Single(OrderScreenLink.From(view, MemberRootPath));
        Assert.Equal(OrderRoutes.ScreenPath(MemberRootPath, "cayma"), link.Href);
    }

    [Fact]
    public void Buttons_FollowTheOrderOfTheOperationsArea_KK7()
    {
        Guid item = Guid.NewGuid();
        OrderOperationsView view = ViewWith(
            OnOrder(OrderOperation.ReopenDefectClaimByCustomer),
            OnItem(OrderOperation.OpenDefectClaim, item),
            OnItem(OrderOperation.WithdrawServiceItem, item),
            OnOrder(OrderOperation.TerminateForLateDelivery),
            OnItem(OrderOperation.CancelServiceItem, item),
            OnOrder(OrderOperation.CancelOrder));
        IReadOnlyList<OrderScreenLink> links = OrderScreenLink.From(view, MemberRootPath);
        Assert.Equal(["iptal", "iptal", "gecikme-feshi", "cayma", "ayip-talebi", "ayip-talebi"], links.Select(e => e.Href[(MemberRootPath.Length + 1)..]));
        Assert.Equal(links.Count, links.Select(e => e.Text).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Buttons_KeepTheAccessRoot_KK5()
    {
        string accessRoot = OrderRoutes.AccessPath("aB3-_xYz");
        OrderScreenLink link = Assert.Single(OrderScreenLink.From(ViewWith(OnOrder(OrderOperation.CancelOrder)), accessRoot));
        Assert.Equal(accessRoot + "/iptal", link.Href);
    }
}
