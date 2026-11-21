using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Catalog;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// T604 D-1843 (07 2.5.4; 04 2.7.3.1): the forgery check's 400 on the coupon screen shows the list and the address's coupon form with the sent values through the one hook
/// (<see cref="IReadsOnForgeryRefusal"/>) and writes nothing.
/// </summary>
public sealed partial class CouponsPageTests
{
    [Fact]
    public async Task TheForgeryRefusal_ShowsTheListAndTheCouponsFormWithTheSentValues_AndWritesNothing_D1843()
    {
        Coupon coupon = await SeedAsync(Coupon.CreatePercentage("SAHTEKOD", 10m, Today.AddDays(-1), Today.AddDays(30), 5, DateTimeOffset.UtcNow.AddHours(-1)));
        await SeedAsync(Coupon.CreatePercentage("LISTEKOD", 10m, Today.AddDays(-1), Today.AddDays(30), 5, DateTimeOffset.UtcNow.AddHours(-2)));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await ForgedPosts.SendAsync(client, $"{Path}/{coupon.Id:D}?handler=SaveCoupon", Form(("Input.Code", "DEGISKOD"), ("Input.Percentage", "25"), ("Input.Version", "1")));

        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.Contains("LISTEKOD", html, StringComparison.Ordinal);
        Assert.Contains("value=\"DEGISKOD\"", html, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.Version\"", html, StringComparison.Ordinal);
        Assert.Contains(ForgeryProtectionFilter.Message, html, StringComparison.Ordinal);
        Assert.Equal("SAHTEKOD|10.00|1", await ScalarAsync<string>($"SELECT code || '|' || percentage::text || '|' || version FROM coupons WHERE id = '{coupon.Id}'"));
    }
}
