using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Payment;

/// <summary>The Payment module's implementation of its open surface; internal, reached only through <see cref="IPaymentModule"/>.</summary>
internal sealed class PaymentModule : IPaymentModule
{
}

/// <summary>The module's single registration (09 6 item 3): <c>Program.cs</c> calls <c>AddPaymentModule</c> once.</summary>
internal static class PaymentModuleRegistration
{
    public static IServiceCollection AddPaymentModule(this IServiceCollection services)
    {
        services.AddScoped<IPaymentModule, PaymentModule>();
        return services;
    }
}
