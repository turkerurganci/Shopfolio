using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>T604's item on the confirmation screen (D-1450): the transfer's last payment day is the module's one rule - the day before the window's end, as the notices and the panel name it.</summary>
public sealed partial class CheckoutPageTests
{
    [Fact]
    public async Task TheTransferScreen_NamesTheLastPaymentDay_TheDayBeforeTheWindowsEnd_NotTheEndsOwnDate_D1450()
    {
        FakePaymentGateway gateway = new();
        await using WebApplicationFactory<Program> factory = OpenWithGateway(gateway);
        (string screen, _) = await PlaceGuestOrderAsync(factory, "BankTransfer");

        // A window ends at the 00:00 (Turkey) after its last day (TurkeyTime.EndOfPeriod): 2030-01-11 00:00 +03:00 is the end of 10 January.
        DateOnly lastDay = new(2030, 1, 10);
        await ExecuteAsync("UPDATE orders SET payment_window_ends_at = '2030-01-10 21:00:00+00'");
        using HttpClient plain = PlainClient(factory);

        string html = await BodyOf(await SendAsync(plain, HttpMethod.Get, screen));
        HtmlEncoder encoder = factory.Services.GetRequiredService<HtmlEncoder>();
        Assert.Contains($"<dd>{encoder.Encode(DisplayFormat.Date(lastDay))}</dd>", html, StringComparison.Ordinal);
        Assert.DoesNotContain(encoder.Encode(DisplayFormat.Date(lastDay.AddDays(1))), html, StringComparison.Ordinal);
    }
}
