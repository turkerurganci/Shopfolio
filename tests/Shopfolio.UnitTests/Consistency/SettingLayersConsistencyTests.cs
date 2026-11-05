using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.FileSecrets;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the places that name the same setting equal (T226 KK1, KK2, KK3; 09 3.5 items 2 and 3; 09 4.3 item 3; 09 21.4 item 1): the tunable
/// design values of the code, <c>DEPLOY_RUNBOOK</c> sections A and C, the settings template, and the Compose files. Runs in the Lint step,
/// not in the Unit step.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class SettingLayersConsistencyTests
{
    private static readonly string[] _requiredKeys =
    [
        SiteOptions.DomainKey,
        EmailOptions.SenderAddressKey,
        EmailOptions.RegionKey,
        EmailOptions.CredentialKey,
        DatabaseOptions.ConnectionStringKey,
        LinkKeyOptions.DerivationSecretKey,
    ];

    [Fact]
    public void RunbookSectionC_NamesEveryTunableDesignValueWithItsCodeDefault()
    {
        Dictionary<string, string> documented = RunbookDefaults();
        Dictionary<string, string> code = CodeDefaults();

        Assert.Equal(code.Keys.Order(StringComparer.Ordinal), documented.Keys.Order(StringComparer.Ordinal));
        Assert.All(code, pair => Assert.Equal(pair.Value, documented[pair.Key]));
    }

    [Fact]
    public void SettingsTemplate_TunableLines_AreCodeDefaultsOfKnownTunableValues()
    {
        Dictionary<string, string> code = CodeDefaults();
        Dictionary<string, string> template = TemplateTunableLines();

        Assert.Subset(code.Keys.ToHashSet(StringComparer.Ordinal), template.Keys.ToHashSet(StringComparer.Ordinal));
        Assert.All(template, pair => Assert.Equal(code[pair.Key], pair.Value));
        Assert.Contains(RateLimitOptions.PageRequestsPerMinuteKey, template.Keys);
        Assert.Contains(RateLimitOptions.FormSubmissionsPerMinuteKey, template.Keys);
        Assert.Contains(LoggingOptions.LevelKey, template.Keys);
    }

    [Fact]
    public void TunableDesignValues_AreOnlyTheRateLimitTheStatusThresholdsAndTheLogLevel()
    {
        // 09 3.5 item 3: of the design values only these are changed in the settings file. Response time, concurrent visitors and volume
        // assumptions (05 11.1.1-11.1.3; 10 4.2 SK-5) have no setting: the system does not apply them (KK6).
        string[] sections = [.. CodeDefaults().Keys.Select(k => k.Split(':')[0]).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];

        Assert.Equal(["Database", "Logging", "RateLimit", "StatusThreshold"], sections);
        Assert.DoesNotContain(CodeDefaults().Keys, key => Regex.IsMatch(key, "Response|Concurrent|Visitor|Volume", RegexOptions.IgnoreCase));
    }

    [Fact]
    public void RunbookSectionA_ListsTheSixRequiredSettings()
    {
        string section = Section(File.ReadAllText(RepoPath("Docs", "DEPLOY_RUNBOOK.md")), "## A. ", "## B. ");

        Assert.All(_requiredKeys, key => Assert.Contains($"`{key}`", section, StringComparison.Ordinal));
        Assert.Equal(_requiredKeys.Length, RunbookRowStart().Count(section));
    }

    [Fact]
    public void SecretSettings_AreInNoSettingsTemplateAndNoComposeEnvironmentExceptTheDevelopmentConnection()
    {
        // KK3: a secret reaches the application as a file only. The development overlay's made-up connection string is the one
        // deliberate exception (09 4.3 item 2; DeploySecretScan.AllowedEnvironmentSettings).
        string[] files = ["deploy/.env.example", "deploy/compose.yaml", "deploy/compose.dev.yaml", "deploy/compose.local-reference.yaml"];
        foreach (SecretSetting secret in SecretNames.Settings)
        {
            string environmentName = secret.Key.Replace(":", "__", StringComparison.Ordinal);
            foreach (string file in files)
            {
                bool allowed = secret.Key == DatabaseOptions.ConnectionStringKey && file == "deploy/compose.dev.yaml";
                string text = File.ReadAllText(RepoPath(file.Split('/')));
                Assert.True(allowed || !text.Contains(environmentName, StringComparison.Ordinal), $"{file} names the secret setting {secret.Key}.");
            }
        }
    }

    [Fact]
    public void Compose_GivesTheApplicationItsDomainFromTheSameValueTheReverseProxyReads()
    {
        string compose = File.ReadAllText(RepoPath("deploy", "compose.yaml"));
        string template = File.ReadAllText(RepoPath("deploy", ".env.example"));

        // D-32 (a): one value, SHOPFOLIO_DOMAIN, written once in the settings template.
        MatchCollection lines = DomainEnvironmentLine().Matches(compose);
        Assert.Single(lines);
        Assert.StartsWith("${SHOPFOLIO_DOMAIN", lines[0].Groups["value"].Value, StringComparison.Ordinal);
        Assert.Matches(@"(?m)^SHOPFOLIO_DOMAIN=\S+$", template);
        Assert.DoesNotContain("Site__Domain", template, StringComparison.Ordinal);
    }

    private static string RepoPath(params string[] parts) => Path.Combine([RepositoryRoot.Find(), .. parts]);

    private static string Section(string text, string from, string to)
    {
        int start = text.IndexOf(from, StringComparison.Ordinal);
        int end = text.IndexOf(to, start + 1, StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start, $"The section '{from}' was not found.");
        return text[start..end];
    }

    private static Dictionary<string, string> RunbookDefaults()
    {
        string section = Section(File.ReadAllText(RepoPath("Docs", "DEPLOY_RUNBOOK.md")), "## C. ", "## D. ");
        Dictionary<string, string> rows = new(StringComparer.Ordinal);
        foreach (Match row in TunableRunbookRow().Matches(section))
        {
            rows.Add(row.Groups["key"].Value, row.Groups["default"].Value);
        }

        return rows;
    }

    private static Dictionary<string, string> TemplateTunableLines()
    {
        Dictionary<string, string> lines = new(StringComparer.Ordinal);
        foreach (Match line in TemplateLine().Matches(File.ReadAllText(RepoPath("deploy", ".env.example"))))
        {
            string key = line.Groups["key"].Value.Replace("__", ":", StringComparison.Ordinal);
            if (key.Split(':')[0] is "Database" or "RateLimit" or "StatusThreshold" or "Logging")
            {
                lines.Add(key, line.Groups["value"].Value.Trim());
            }
        }

        return lines;
    }

    // The tunable design values and their defaults, read from the settings classes themselves.
    private static Dictionary<string, string> CodeDefaults()
    {
        Dictionary<string, string> defaults = new(StringComparer.Ordinal);
        AddDefaults(defaults, RateLimitOptions.SectionName, new RateLimitOptions());
        AddDefaults(defaults, StatusThresholdOptions.SectionName, new StatusThresholdOptions());
        AddDefaults(defaults, LoggingOptions.SectionName, new LoggingOptions());
        defaults[DatabaseOptions.LivenessTimeoutKey] = new DatabaseOptions().LivenessTimeoutSeconds.ToString(CultureInfo.InvariantCulture);
        return defaults;
    }

    private static void AddDefaults(Dictionary<string, string> defaults, string section, object options)
    {
        foreach (PropertyInfo property in options.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            defaults[section + ":" + property.Name] = Convert.ToString(property.GetValue(options), CultureInfo.InvariantCulture)!;
        }
    }

    // | 3 | `RateLimit:PageRequestsPerMinute` (`RateLimit__PageRequestsPerMinute`) | `300` | ...
    [GeneratedRegex(@"^\|\s*\d+\s*\|\s*`(?<key>(?:Database:Liveness|RateLimit:|StatusThreshold:|Logging:)\w+)`[^|]*\|\s*`(?<default>[^`]+)`\s*\|", RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex TunableRunbookRow();

    [GeneratedRegex(@"^\|\s*\d+\s*\|\s*`", RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex RunbookRowStart();

    [GeneratedRegex(@"^(?<key>[A-Za-z][A-Za-z0-9_]*)=(?<value>.*)$", RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex TemplateLine();

    [GeneratedRegex(@"^\s*Site__Domain:\s*(?<value>.+)$", RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex DomainEnvironmentLine();
}
