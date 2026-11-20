using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Serilog.Core;

using Shopfolio.Infrastructure.Errors;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The rejection lines of a test application (T604 D-1317, D-1508; 09 8.2 item 3): the collected lines of a factory and the question whether one refusal's line, with its code, is among them.
/// The application's lines reach the provider through the log sink the logging setup reads (<see cref="ProviderLogEventSink"/>).
/// </summary>
internal static class RejectionLines
{
    /// <summary>The same application with its lines collected in <paramref name="logs"/>.</summary>
    public static WebApplicationFactory<Program> Collecting(this WebApplicationFactory<Program> factory, CollectingLoggerProvider logs) =>
        factory.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddSingleton<ILogEventSink>(new ProviderLogEventSink(logs))));

    /// <summary>The number of rejection lines that carry <paramref name="code"/>.</summary>
    public static int Count(this CollectingLoggerProvider logs, ErrorCode code) =>
        logs.Lines.Count(l => l.Contains("RejectionLog|", StringComparison.Ordinal) && l.EndsWith(" " + code, StringComparison.Ordinal));
}
