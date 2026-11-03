using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.FileSecrets;

/// <summary>Keeps secret values out of the repository (05 6.x.1): the ignore rules and the settings template.</summary>
public sealed partial class SecretRepositoryTests
{
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

    [GeneratedRegex(@"^([A-Za-z_][A-Za-z0-9_]*)=(.*)$")]
    private static partial Regex Assignment();

    [GeneratedRegex("(PASSWORD|SECRET|TOKEN|CREDENTIAL|PRIVATE|APIKEY|_KEY)", RegexOptions.IgnoreCase)]
    private static partial Regex SecretWord();
}
