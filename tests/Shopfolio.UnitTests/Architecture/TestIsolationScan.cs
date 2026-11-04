using System.Reflection;
using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan and reflection check that keep the unit test project away from the database (T120 KK7, KK8; 09 19.2 item 2, 21.4 item 1):
/// no test of <c>UnitTests</c> applies a migration or looks for pending model changes (that check is the CI migration dry-run step), and an
/// equality check under <c>Consistency/</c> reads neither a connection string nor the environment and opens no connection or network call.
/// The forbidden names are assembled from parts so that this file does not match its own scan.
/// </summary>
internal static class TestIsolationScan
{
    public const string ModelChangeCheck = "ModelChangeCheckInUnitTests";
    public const string ConfigurationRead = "ConfigurationReadInConsistencyCheck";
    public const string DatabaseOrNetwork = "DatabaseOrNetworkInConsistencyCheck";
    public const string TraitMissing = "ConsistencyTraitMissing";
    public const string TraitMisplaced = "ConsistencyTraitOutsideConsistencyFolder";

    private static readonly TimeSpan _matchTimeout = TimeSpan.FromSeconds(5);

    private static readonly Regex _modelChange = new(
        string.Join("|", "HasPending" + "ModelChanges", "GetPending" + "Migrations", "GetApplied" + "Migrations", @"\bIMigrator\b", @"\.\s*Migrate(?:Async)?\s*\("),
        RegexOptions.CultureInvariant,
        _matchTimeout);

    private static readonly Regex _configurationRead = new(
        string.Join("|", "Get" + "ConnectionString", "Get" + "EnvironmentVariable", "ExpandEnvironment" + "Variables", @"\bIConfiguration\b", "Configuration" + "Builder"),
        RegexOptions.CultureInvariant,
        _matchTimeout);

    private static readonly Regex _databaseOrNetwork = new(
        string.Join("|", "Npgsql" + "Connection", "Npgsql" + "DataSource", @"\bHttpClient\b", @"\.\s*OpenAsync\s*\(", @"\.\s*Open\s*\(\s*\)", @"\.\s*Database\s*\.", "Testcontainers", @"\bSocket\b"),
        RegexOptions.CultureInvariant,
        _matchTimeout);

    /// <summary>Scans one C# file of the unit test project. <paramref name="inConsistencyFolder"/> adds the rules of the equality checks.</summary>
    public static IReadOnlyList<(string Rule, int Line)> Scan(string text, bool inConsistencyFolder)
    {
        List<(string Rule, int Line)> findings = [];
        Add(findings, ModelChangeCheck, _modelChange, text);
        if (inConsistencyFolder)
        {
            Add(findings, ConfigurationRead, _configurationRead, text);
            Add(findings, DatabaseOrNetwork, _databaseOrNetwork, text);
        }

        return findings;
    }

    /// <summary>
    /// The types that break the trait rule: a type of <c>Consistency</c> that has a test method and no <c>Consistency=true</c> trait runs in the
    /// Unit step, and a type outside it with the trait would be dropped from the Unit step although it is not an equality check.
    /// </summary>
    public static IReadOnlyList<(string Rule, string Type)> TraitProblems(IEnumerable<Type> types, Func<Type, bool> inFolder, Type? testAttribute = null)
    {
        List<(string Rule, string Type)> problems = [];
        foreach (Type type in types)
        {
            bool folder = inFolder(type);
            bool hasTests = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Any(m => m.IsDefined(testAttribute ?? typeof(FactAttribute), inherit: true));
            bool tagged = type.GetCustomAttributes<TraitAttribute>(inherit: true).Any(t => t.Name == "Consistency" && t.Value == "true");
            if (folder && hasTests && !tagged)
            {
                problems.Add((TraitMissing, type.FullName ?? type.Name));
            }

            if (!folder && tagged)
            {
                problems.Add((TraitMisplaced, type.FullName ?? type.Name));
            }
        }

        return problems;
    }

    private static void Add(List<(string Rule, int Line)> findings, string rule, Regex pattern, string text)
    {
        foreach (Match match in pattern.Matches(text))
        {
            findings.Add((rule, text.AsSpan(0, match.Index).Count('\n') + 1));
        }
    }
}
