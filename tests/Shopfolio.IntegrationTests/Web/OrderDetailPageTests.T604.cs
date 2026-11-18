using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>T604's items on E-37 (D-1544): a form value that does not bind is a 422 with the order on the screen - the validation filter's path loads the order too -, never a 500.</summary>
public sealed partial class OrderDetailPageTests
{
    [Fact]
    public async Task AFormValueThatDoesNotBind_Is422WithTheOrdersScreen_NotA500_AndAKeyThatIsNoOrder_Is404_D1544()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string page = $"{OrdersPath}/{orderId:D}";

        // A version that is not a number (T527's step, T530's corrections), a Guid and a bool that do not parse (T530): the filter answers before the handler.
        (HttpStatusCode version, string versionHtml) = await PostAsync(client, page + "?handler=MarkDelivered", [("Input.Version", "abc")], key);
        (HttpStatusCode carrier, string carrierHtml) = await PostAsync(client, page + "?handler=CorrectCarrier", [("Input.Version", "abc"), ("Input.IsOwnVehicleDelivery", "maybe")], key);
        (HttpStatusCode transition, _) = await PostAsync(client, page + "?handler=CorrectTransition", [("Input.Version", "1"), ("Input.TransitionRecordId", "abc")], key);
        (HttpStatusCode foreign, _) = await PostAsync(client, $"{OrdersPath}/{Guid.NewGuid():D}?handler=MarkDelivered", [("Input.Version", "abc")], key);

        Assert.Equal(
            [HttpStatusCode.UnprocessableEntity, HttpStatusCode.UnprocessableEntity, HttpStatusCode.UnprocessableEntity, HttpStatusCode.NotFound],
            new[] { version, carrier, transition, foreign });
        Assert.Contains(page + "?handler=", versionHtml, StringComparison.Ordinal);
        Assert.Contains(page + "?handler=", carrierHtml, StringComparison.Ordinal);
        await using AppDbContext reader = NewContext();
        Assert.Equal(1, await reader.Orders.Where(o => o.Id == orderId).Select(o => o.Version).SingleAsync(Ct));
    }
}
