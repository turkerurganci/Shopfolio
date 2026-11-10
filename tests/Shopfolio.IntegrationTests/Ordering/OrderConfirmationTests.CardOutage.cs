using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Status;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The start's connection error on a real PostgreSQL (T404 KK3, KK4; 08 2.1.8; 02 6.2.15, 8.2 L-7): the form that could not open cancels the order with the reason L-7
/// does not count, while the form that opened and whose payment failed is counted; for two minutes on the fake clock the card is "şu an kullanılamıyor" and a card
/// confirmation is refused with 422's refusal without a token request, while a confirmed order's start still asks for its token; a restart (a new process) has no
/// state, and the end of the two minutes takes the card back.
/// </summary>
public sealed partial class OrderConfirmationTests
{
    [Fact]
    public async Task ConnectionErrorOfTheStart_CardUnavailableForTwoMinutes_RefusedWithoutAToken_AndTheTwoCancellationsApartInL7_T404_KK3_KK4()
    {
        FakePaymentGateway gateway = new();
        CardUnavailability memory = new(_clock);
        PaymentStartFailureSeries series = new(_clock);
        _extra = services =>
        {
            services.AddSingleton<IPaymentGateway>(gateway);
            services.AddSingleton(memory);
            services.AddSingleton(series);
        };

        Guid variantId = await SeedVariantAsync("Lamba", 1000m, stock: 10);
        Guid unopenedCustomer = await SeedCustomerAsync();
        Guid openedCustomer = await SeedCustomerAsync();
        CartOwner unopenedOwner = CartOwner.ForMember(unopenedCustomer);
        CartOwner openedOwner = CartOwner.ForMember(openedCustomer);
        await SeedCartAsync(unopenedOwner, (variantId, 1));
        await SeedCartAsync(openedOwner, (variantId, 1));
        OrderSummaryInput unopenedInput = Input(unopenedOwner, PaymentMethod.Card, null);
        OrderSummaryInput openedInput = Input(openedOwner, PaymentMethod.Card, null);
        Guid unopened = (await ConfirmAsync(unopenedInput, (await SummarizeAsync(unopenedInput))!)).OrderId!.Value;
        Guid opened = (await ConfirmAsync(openedInput, (await SummarizeAsync(openedInput))!)).OrderId!.Value;

        // The token request finds the provider unreachable: the step cancels the order with the unopened form's reason (the page's transaction, UC-29).
        gateway.EnqueueStart(new PaymentStartResult.ConnectionFailed(ExternalCallFailure.TimeoutReason));
        Assert.Equal(PaymentStartStep.CancelAsUnavailable, (await StartPaymentAsync(unopened)).Step);
        await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().CancelOrderAsync(CancelOrderRequest.BySystem(unopened, PaymentStartRule.UnopenedCancellationReason), Ct));

        // KK4: the card is unavailable before the confirmation - shown, not offered, and the confirmation is refused without a token request.
        Assert.True(await RunAsync(sp => sp.GetRequiredService<IPaymentMethods>().IsCardUnavailableNowAsync(Ct)));
        Assert.Equal([PaymentMethod.BankTransfer], await RunAsync(sp => sp.GetRequiredService<IPaymentMethods>().ListOpenAsync(Ct)));
        Guid lateCustomer = await SeedCustomerAsync();
        CartOwner lateOwner = CartOwner.ForMember(lateCustomer);
        await SeedCartAsync(lateOwner, (variantId, 1));
        OrderSummaryInput lateInput = Input(lateOwner, PaymentMethod.Card, null);
        OrderConfirmationResult refused = await ConfirmAsync(lateInput, (await SummarizeAsync(lateInput))!);
        Assert.Equal(OrderConfirmationRefusal.PaymentMethodUnavailable, refused.Refusal);
        Assert.Equal(1, StartCalls(gateway));

        // KK4: the state touches only what comes before the confirmation - the confirmed order's start still asks for its token.
        Assert.Equal(PaymentStartStep.OpenPaymentForm, (await StartPaymentAsync(opened)).Step);
        Assert.Equal(2, StartCalls(gateway));

        // KK3: the opened form's payment failed (the notification's cancellation): L-7 counts it, and not the unopened one.
        await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().CancelOrderAsync(CancelOrderRequest.BySystem(opened, TransitionReason.CardPaymentFailed), Ct));
        Assert.Equal(0, await RunAsync(sp => sp.GetRequiredService<UnpaidCancellationLimit>().CountByMemberAsync(unopenedCustomer, Ct)));
        Assert.Equal(1, await RunAsync(sp => sp.GetRequiredService<UnpaidCancellationLimit>().CountByMemberAsync(openedCustomer, Ct)));

        // KK4: a restart is a new process with no state; on the running process the card is back when the two minutes end.
        _extra = services => services.AddSingleton<IPaymentGateway>(gateway);
        Assert.Contains(PaymentMethod.Card, await RunAsync(sp => sp.GetRequiredService<IPaymentMethods>().ListOpenAsync(Ct)));
        _extra = services =>
        {
            services.AddSingleton<IPaymentGateway>(gateway);
            services.AddSingleton(memory);
        };
        Assert.True(memory.IsUnavailable);
        _clock.Advance(CardUnavailability.Duration);
        Assert.False(await RunAsync(sp => sp.GetRequiredService<IPaymentMethods>().IsCardUnavailableNowAsync(Ct)));
        Assert.False((await ConfirmAsync(lateInput, (await SummarizeAsync(lateInput))!)).IsRejected);
        _extra = null;
    }

    private static int StartCalls(FakePaymentGateway gateway) => gateway.Calls.Count(c => c.Job == nameof(IPaymentGateway.StartAsync));

    // The start goes outside any transaction (09 11.1; the outside call is never inside one).
    private async Task<PaymentStartOutcome> StartPaymentAsync(Guid orderId)
    {
        Order order = await ReadOrderAsync(orderId);
        await using ServiceProvider provider = OrderingTestServices.Build(_collection.Dsn, _clock, _cardLineOpen, logs: null, _extra);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        PaymentStartRequest request = new(
            order.OrderNumber, order.Total, "musteri@example.com", OrderingTestServices.CompanyEmail, "Ayşe Yılmaz", "Bağdat Cad. 1", "5321234567", "2121234567", "203.0.113.5",
            _clock.GetUtcNow().AddMinutes(30), "ak_t404");
        return await scope.ServiceProvider.GetRequiredService<PaymentStarts>().StartAsync(request, Ct);
    }
}
