using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

namespace Shopfolio.IntegrationTests;

/// <summary>
/// The one way a test opens the application behind a real Kestrel socket. A client takes its address from the factory
/// (<c>CreateClient().BaseAddress</c>), which reads the address the server really listens on.
/// </summary>
public static class RealServer
{
    /// <summary>
    /// Starts the factory's host on a free loopback port chosen by the operating system.
    /// </summary>
    /// <remarks>
    /// <c>UseKestrel(0)</c> alone is not enough: the factory applies its port to the server's address list after the host is built, and a
    /// host started before that (the entry point's own <c>app.Run()</c> races the factory) falls back to Kestrel's default
    /// <c>http://localhost:5000</c>, which fails with "address already in use" when anything else holds 5000. An endpoint listed in
    /// the Kestrel options is taken before that default, so the port is fixed there as well.
    /// </remarks>
    public static void Start<TEntryPoint>(this WebApplicationFactory<TEntryPoint> factory)
        where TEntryPoint : class
    {
        factory.UseKestrel(options => options.Listen(IPAddress.Loopback, 0));
        factory.StartServer();
    }
}
