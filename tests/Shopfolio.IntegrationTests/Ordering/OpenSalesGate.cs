using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Shopfolio.IntegrationTests.Web;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The order confirmation asks the sales gate in its transaction (T509; 05 4.2.2 (1)); the transition unit's tests and the main flow run with an open
/// sale, every other call goes to the real Settings module. The gate's closure is the payment step's test (<c>CheckoutPageTests</c> KK2, KK8).
/// </summary>
internal static class OpenSalesGate
{
    public static void Register(IServiceCollection services)
    {
        services.RemoveAll<ISettingsModule>();
        services.AddScoped(sp => CheckoutPageTests.GateSettings.Create(ActivatorUtilities.CreateInstance<SettingsModule>(sp), open: true));
    }
}
