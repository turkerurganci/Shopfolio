using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>T604 (U-H, UH4): E-45 and E-46 show the sales state (D-1778; 04 6.1.5.2 "Panelde hâl E-31, E-44…E-47'de görünür").</summary>
public sealed partial class PaymentAndShippingPagesTests
{
    [Theory]
    [InlineData(PaymentPath)]
    [InlineData(ShippingPath)]
    public async Task TheSalesState_IsOnTheScreen_AsTheSalesGatesAnswer_AndFollowsTheGate_D1778(string path)
    {
        await using WebApplicationFactory<Program> factory = Open().WithWebHostBuilder(b => b.ConfigureTestServices(s => s.AddScoped<ICardPaymentCondition>(_ => new FixedCardCondition(false))));
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string closedHtml) = await GetAsync(client, path);
        SalesGate closedGate = await SalesGateOfAsync(factory);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.False(closedGate.IsOpen);
        Assert.Contains(SalesStateTag(PanelHomeTexts.SalesState(closedGate)), closedHtml, StringComparison.Ordinal);
        Assert.Contains(PanelHomeTexts.ConditionName(SalesGateCondition.PaymentMethod), closedHtml, StringComparison.Ordinal);

        // The payment method condition is met by an open transfer with its IBAN: the line is the gate's new answer, and no longer names that condition.
        await ExecuteAsync($"UPDATE company_settings SET is_bank_transfer_enabled = true, bank_transfer_iban = '{Iban}'");
        (_, string metHtml) = await GetAsync(client, path);
        SalesGate metGate = await SalesGateOfAsync(factory);

        Assert.DoesNotContain(SalesGateCondition.PaymentMethod, metGate.UnmetConditions);
        Assert.Contains(SalesStateTag(PanelHomeTexts.SalesState(metGate)), metHtml, StringComparison.Ordinal);
        Assert.DoesNotContain(PanelHomeTexts.ConditionName(SalesGateCondition.PaymentMethod), metHtml, StringComparison.Ordinal);
    }

    private static string SalesStateTag(string line) => $"id=\"sales-state\">{line}</p>";
}
