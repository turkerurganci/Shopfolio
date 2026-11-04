using Serilog;
using Serilog.Core;

namespace Shopfolio.Infrastructure.Logging;

/// <summary>The two files of the audit path: the application process writes <c>audit-</c>, the command process <c>command-</c> (09 18.1 item 2).</summary>
internal enum AuditFile
{
    Audit,
    Command,
}

/// <summary>
/// The one place that configures logging (09 18.1 item 1; 09 4.1). The modules ask for <c>ILogger&lt;T&gt;</c> and for <see cref="IAuditLineWriter"/>;
/// nothing outside this folder (and <c>Program.cs</c>) refers to Serilog. Three writers, three files: the normal writer, the audit writer and
/// the command writer (09 18.1 item 2). Packages: Serilog 4.3.1, Serilog.Sinks.File 7.0.0, Serilog.AspNetCore 10.0.0.
/// </summary>
internal static class LoggingSetup
{
    /// <summary>The own namespace of the application: the level setting reaches this namespace only.</summary>
    public const string ApplicationNamespace = "Shopfolio";

    public const string FileExtension = "jsonl";

    /// <summary>The prefix of the normal writer's file.</summary>
    public const string ApplicationPrefix = "app";

    /// <summary>The path template of a log file; the library puts the day between the prefix and the extension (<c>app-20261105.jsonl</c>).</summary>
    public static string FilePathTemplate(string directory, string prefix) => Path.Combine(directory, prefix + "-." + FileExtension);

    public static string Prefix(AuditFile file) => file == AuditFile.Audit ? "audit" : "command";

    /// <summary>
    /// Hooks the application's logging into the host (09 18.1 item 1). The framework's logger factory is replaced by the library's; the providers
    /// the host has besides stay (the console), so nothing the host writes goes missing.
    /// </summary>
    public static WebApplicationBuilder AddApplicationLogging(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Host.UseSerilog(
            (_, services, configuration) => Configure(
                configuration,
                LoggingOptions.FromConfiguration(services.GetRequiredService<IConfiguration>()),
                new RequestLogEnricher(services.GetRequiredService<IHttpContextAccessor>())),
            writeToProviders: true);
        return builder;
    }

    /// <summary>Registers the audit line writer of the application process (the <c>audit-</c> file).</summary>
    public static IServiceCollection AddAuditLineWriter(this IServiceCollection services)
    {
        services.AddSingleton<IAuditLineWriter>(provider => CreateAuditLineWriter(
            LoggingOptions.FromConfiguration(provider.GetRequiredService<IConfiguration>()),
            AuditFile.Audit,
            provider.GetRequiredService<TimeProvider>()));
        return services;
    }

    /// <summary>
    /// The normal writer (09 18.1 item 2, 4): warning for everything, the setting's level for the application's own namespace only — the
    /// framework's and the data access layer's categories stay at warning whatever the setting says. The file has no limit of its own on the
    /// number or the size of the files (09 18.1 item 3): only the disposal job deletes a file. A failure of this writer goes to the library's
    /// self-diagnostics and never to the caller (<c>WriteTo</c>).
    /// </summary>
    public static LoggerConfiguration Configure(LoggerConfiguration configuration, LoggingOptions options, ILogEventEnricher requestEnricher)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(requestEnricher);

        return configuration
            .MinimumLevel.Warning()
            .MinimumLevel.Override(ApplicationNamespace, options.Level)
            .Enrich.With(requestEnricher)
            .WriteTo.File(
                new ApplicationLogFormatter(),
                FilePathTemplate(options.Directory, ApplicationPrefix),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: null,
                fileSizeLimitBytes: null);
    }

    /// <summary>
    /// The audit path (09 18.1 item 2, 5): the library's audit file writer (<c>AuditTo.File</c>) has no day-by-day file and no limit parameters in
    /// Serilog.Sinks.File 7.0.0 (checked against the package: it writes one fixed path), so the audit and command writers are the project's own
    /// small writer, <see cref="AuditLogWriter"/>. It uses the same formatter and the fixed information level; the level setting does not reach it.
    /// </summary>
    public static IAuditLineWriter CreateAuditLineWriter(LoggingOptions options, AuditFile file, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(time);

        return new AuditLogWriter(options.Directory, Prefix(file), time);
    }
}
