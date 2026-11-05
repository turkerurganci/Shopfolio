using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The scan for a metric collection or a dashboard in the dependencies and the deployment files (T225 criterion 7; 05 9.1.7, 9.1.8): there is no
/// separate metric collection and no dashboard — a metric client, a metric exporter, a paid log service and the collection and dashboard stack
/// (Prometheus, Grafana, Loki) are not added. What is needed comes from the status endpoint and the status command. The scan reads the package
/// declarations (<c>Directory.Packages.props</c>, the project files), the application's lock file (the transitive packages) and the deployment
/// files (the Compose definitions, the Dockerfiles); a line that is only a comment is not scanned.
/// </summary>
internal static class MetricsPackageScan
{
    public const string MetricsOrDashboardName = "MetricsOrDashboardNameInTheDependencies";

    /// <summary>The names the scan refuses; the pattern is built from this list, and the test shows that each name goes red on a sample line.</summary>
    public static readonly IReadOnlyList<string> Names =
    [
        "prometheus", "opentelemetry", "app.metrics", "appmetrics", "statsd", "datadog", "grafana", "loki", "influxdb", "newrelic",
        "applicationinsights", "elastic.apm", "sentry", "serilog.sinks.seq",
    ];

    private static readonly Regex _pattern = new(
        "(?<![A-Za-z])(" + string.Join('|', Names.Select(Regex.Escape)) + ")(?![A-Za-z])",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(5));

    public static IReadOnlyList<(string Rule, int Line)> Scan(string text)
    {
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        List<(string Rule, int Line)> findings = [];
        for (int i = 0; i < lines.Length; i++)
        {
            string trimmed = lines[i].TrimStart();
            bool comment = trimmed.StartsWith('#') || trimmed.StartsWith("<!--", StringComparison.Ordinal) || trimmed.StartsWith("//", StringComparison.Ordinal);
            if (!comment && _pattern.IsMatch(lines[i]))
            {
                findings.Add((MetricsOrDashboardName, i + 1));
            }
        }

        return findings;
    }
}
