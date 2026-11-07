using Microsoft.AspNetCore.DataProtection;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The one place that sets up the framework's data protection ring (09 15.5 item 1; 05 7.6.3). The application name is fixed, so the keys do not
/// depend on the path inside the image; the keys are kept in the database through <see cref="AppDbContext"/> and stay there in clear, because a
/// key encryptor on the same server would protect nothing the database reader cannot also read (K-1082). No key encryptor is set up and the key
/// lifetime is the framework's default; an expired key is never removed from the ring (K-1035). The framework writes a start-up warning that the
/// keys are not encrypted: that warning is expected and no level filter removes it.
/// </summary>
internal static class DataProtectionSetup
{
    /// <summary>The fixed application name of the ring (09 15.5 item 1).</summary>
    public const string ApplicationName = "Shopfolio";

    public static IServiceCollection Add(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddDataProtection()
            .SetApplicationName(ApplicationName)
            .PersistKeysToDbContext<AppDbContext>();
        services.AddSingleton<ProtectedValues>();
        services.AddSingleton<DownloadLinks>();
        return services;
    }
}
