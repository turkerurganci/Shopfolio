using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.FileSecrets;

public sealed partial class SecretSourceScanTests
{
    [Fact]
    public void ApplicationSource_ReadsNoSecretFromTheEnvironment()
    {
        string root = RepositoryRoot.Find();
        string[] files = Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !IsBuildOutput(f))
            .ToArray();

        Assert.Contains(files, f => f.EndsWith("Program.cs", StringComparison.Ordinal));
        List<string> findings = [];
        foreach (string file in files)
        {
            foreach ((string rule, int line) in SecretSourceScan.Scan(File.ReadAllText(file)))
            {
                findings.Add($"{Path.GetRelativePath(root, file)}:{line} {rule}");
            }
        }

        Assert.Empty(findings);
    }

    [Theory]
    [InlineData("var p = Environment.GetEnvironmentVariable(\"X\");", SecretSourceScan.EnvironmentRead)]
    [InlineData("var all = Environment . GetEnvironmentVariables();", SecretSourceScan.EnvironmentRead)]
    [InlineData("builder.Configuration.AddEnvironmentVariables();", SecretSourceScan.EnvironmentConfiguration)]
    [InlineData("var key = \"PGPASSWORD\";", SecretSourceScan.DatabasePasswordVariable)]
    public void Scan_OnViolationSample_ReportsTheRuleOnItsLine(string code, string rule)
    {
        IReadOnlyList<(string Rule, int Line)> findings = SecretSourceScan.Scan("// header\n" + code);

        Assert.Equal((rule, 2), Assert.Single(findings));
    }

    [Fact]
    public void Scan_OnCommentAndOrdinaryCode_ReportsNothing()
    {
        string text = "// Environment.GetEnvironmentVariable is not used here\nstring name = Environment.MachineName;\n";

        Assert.Empty(SecretSourceScan.Scan(text));
    }

    [Fact]
    public void Gitignore_KeepsSettingsAndSecretFilesOutOfTheRepository()
    {
        string[] lines = File.ReadAllLines(Path.Combine(RepositoryRoot.Find(), ".gitignore")).Select(l => l.Trim()).ToArray();

        string[] required = [".env", ".env.*", "!.env.example", "secrets/", "**/secrets/", "*.secret", "*.pem", "*.key", "*.pfx", "appsettings.Production.json", "*credentials*.json"];
        Assert.All(required, pattern => Assert.Contains(pattern, lines));
    }

    [Fact]
    public void SettingsTemplate_CarriesNoSecretKeyAndNoSecretLikeValue()
    {
        string[] lines = File.ReadAllLines(Path.Combine(RepositoryRoot.Find(), "deploy", ".env.example"));

        List<string> keys = [];
        foreach (string line in lines.Where(l => l.Length > 0 && !l.TrimStart().StartsWith('#')))
        {
            Match assignment = Assignment().Match(line);
            Assert.True(assignment.Success, $"Not a KEY=value line: {line}");
            keys.Add(assignment.Groups[1].Value);
            Assert.DoesNotMatch(SecretWord(), assignment.Groups[1].Value);
            Assert.DoesNotMatch(@"^\s*[A-Za-z0-9+/=_-]{32,}\s*$", assignment.Groups[2].Value);
        }

        Assert.NotEmpty(keys);
    }

    private static bool IsBuildOutput(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
        || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal);

    [GeneratedRegex(@"^([A-Za-z_][A-Za-z0-9_]*)=(.*)$")]
    private static partial Regex Assignment();

    [GeneratedRegex("(PASSWORD|SECRET|TOKEN|CREDENTIAL|PRIVATE|APIKEY|_KEY)", RegexOptions.IgnoreCase)]
    private static partial Regex SecretWord();
}
