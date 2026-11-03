using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.FileSecrets;

/// <summary>Keeps secret values out of the repository (05 6.x.1): the ignore rules. The settings template scan is in Architecture/DeploySecretScanTests.</summary>
public sealed class SecretRepositoryTests
{
    [Fact]
    public void Gitignore_KeepsSettingsAndSecretFilesOutOfTheRepository()
    {
        string[] lines = File.ReadAllLines(Path.Combine(RepositoryRoot.Find(), ".gitignore")).Select(l => l.Trim()).ToArray();

        string[] required = [".env", ".env.*", "!.env.example", "secrets/", "**/secrets/", "*.secret", "*.pem", "*.key", "*.pfx", "appsettings.Production.json", "*credentials*.json"];
        Assert.All(required, pattern => Assert.Contains(pattern, lines));
    }
}
