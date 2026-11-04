namespace Shopfolio.Infrastructure;

/// <summary>
/// The application's one time source (09 7.1 item 1; 05 7.2.4): <see cref="TimeProvider.System"/>, registered once as a singleton. A class
/// that needs the time takes <see cref="TimeProvider"/> from its constructor and reads the instant with <c>GetUtcNow()</c>; a test puts a
/// fake in its place. This is the only place that names <see cref="TimeProvider.System"/> (architecture test <c>TimeAndMoneyRulesTests</c>).
/// </summary>
internal static class TimeSource
{
    public static IServiceCollection AddTimeSource(this IServiceCollection services)
    {
        return services.AddSingleton(TimeProvider.System);
    }
}
