namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// Registers the payment provider's implementation, PayTR (09 11.1 item 1; KD-64): the one <see cref="IPaymentGateway"/>. It is registered whether the keys are there or
/// not - without them the card line is closed (02 3.21.4) and a start that still arrives is refused without a call (<see cref="PayTrPaymentGateway.KeysMissingReason"/>).
/// The name of the extension carries no provider's name, so the layer's common registration calls it without naming the provider (08 3.1 rule 1).
/// </summary>
internal static class PayTrRegistration
{
    public static IServiceCollection AddPaymentProvider(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<PaymentFailureReasonLog>();
        services.AddSingleton<IPaymentGateway, PayTrPaymentGateway>();
        return services;
    }
}
