using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Shopfolio.Infrastructure;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The one time source (T227 criterion 2; KK2; 09 7.1 items 1, 4; 05 7.2.4): the application registers <see cref="TimeProvider.System"/>
/// once as a singleton, and a test replaces that one registration with the fake clock.
/// </summary>
public sealed class TimeSourceTests
{
    [Fact]
    public void AddTimeSource_RegistersTheSystemClockOnceAsSingleton()
    {
        ServiceCollection services = new();

        services.AddTimeSource();

        ServiceDescriptor descriptor = Assert.Single(services, d => d.ServiceType == typeof(TimeProvider));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        using ServiceProvider provider = services.BuildServiceProvider();
        Assert.Same(TimeProvider.System, provider.GetRequiredService<TimeProvider>());
    }

    [Fact]
    public void TimeSource_ReplacedInATest_GivesTheFakeClocksInstant()
    {
        DateTimeOffset instant = new(2026, 3, 1, 21, 0, 0, TimeSpan.Zero);
        ServiceCollection services = new();
        services.AddTimeSource();
        services.Replace(ServiceDescriptor.Singleton<TimeProvider>(new ManualTimeProvider(instant)));

        using ServiceProvider provider = services.BuildServiceProvider();

        Assert.Equal(instant, provider.GetRequiredService<TimeProvider>().GetUtcNow());
    }
}
