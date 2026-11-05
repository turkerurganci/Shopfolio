using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>The Ordering module's implementation of its open surface; internal, reached only through <see cref="IOrderingModule"/>.</summary>
internal sealed class OrderingModule : IOrderingModule
{
}

/// <summary>The module's single registration (09 6 item 3): <c>Program.cs</c> calls <c>AddOrderingModule</c> once.</summary>
internal static class OrderingModuleRegistration
{
    public static IServiceCollection AddOrderingModule(this IServiceCollection services)
    {
        services.AddScoped<OrderLocks>();
        services.AddScoped<IOrderingModule, OrderingModule>();
        return services;
    }
}
