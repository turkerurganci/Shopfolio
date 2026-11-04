using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>The AfterSales module's implementation of its open surface; internal, reached only through <see cref="IAfterSalesModule"/>.</summary>
internal sealed class AfterSalesModule : IAfterSalesModule
{
}

/// <summary>The module's single registration (09 6 item 3): <c>Program.cs</c> calls <c>AddAfterSalesModule</c> once.</summary>
internal static class AfterSalesModuleRegistration
{
    public static IServiceCollection AddAfterSalesModule(this IServiceCollection services)
    {
        services.AddScoped<IAfterSalesModule, AfterSalesModule>();
        return services;
    }
}
