using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Integrations;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The payment provider's keys stay out of development and CI (T406 KK3, KK4; 08 2.1.10, 2.1.3; 05 8.1.2): no tracked settings, deploy or script file gives the keys a
/// value, no CI file names the provider or its keys, and the local reference installation's made-up secret file does not define the setting - its card line stays
/// closed until the installer writes the keys after the provider's written 3D confirmation (DEPLOY_RUNBOOK N.1). Each rule has its violation samples.
/// </summary>
public sealed partial class PaymentKeysScanTests
{
    public static TheoryData<string> KeyValueFiles() => [.. KeyValueFilePaths().Select(Relative)];

    public static TheoryData<string> CiFiles() => [.. CiFilePaths().Select(Relative)];

    [Theory]
    [MemberData(nameof(KeyValueFiles))]
    public void TrackedSettingsDeployAndScriptFile_GivesThePaymentKeysNoValue_T406_KK4(string file)
    {
        Assert.Empty(PaymentKeysScan.ScanForKeyValues(File.ReadAllText(Path.Combine(RepositoryRoot.Find(), file))));
    }

    [Theory]
    [MemberData(nameof(CiFiles))]
    public void CiFile_NamesNeitherTheProviderNorItsKeys_T406_KK4(string file)
    {
        Assert.Empty(PaymentKeysScan.ScanCiFile(File.ReadAllText(Path.Combine(RepositoryRoot.Find(), file))));
    }

    [Fact]
    public void TheScannedSets_HoldTheSettingsTheDevelopmentOverlayAndTheWorkflows_T406_KK4()
    {
        List<string> values = [.. KeyValueFilePaths().Select(Relative)];
        List<string> ci = [.. CiFilePaths().Select(Relative)];

        Assert.Contains("src/Shopfolio.Web/appsettings.json", values);
        Assert.Contains("deploy/compose.dev.yaml", values);
        Assert.Contains("deploy/.env.example", values);
        Assert.Contains("scripts/local-reference/host-secrets.sh", values);
        Assert.Contains(".github/workflows/ci.yml", ci);
        Assert.Contains(".github/workflows/publish.yml", ci);
        Assert.Contains(".github/ci-run.sh", ci);
    }

    // KK3: the local reference installation is an installation without the provider's confirmation; its made-up content is not the three-line setting.
    [Fact]
    public void LocalReferenceInstallation_SecretFileDoesNotDefineTheKeys_TheCardLineStaysClosed_T406_KK3()
    {
        string script = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "scripts", "local-reference", "host-secrets.sh"));

        Match written = LocalReferenceKeysLine().Match(script);

        Assert.True(written.Success);
        Assert.Null(PaymentProviderKeys.TryParse(written.Groups[1].Value.Replace("%s", "generated", StringComparison.Ordinal)));
    }

    [Theory]
    [InlineData("Payment__ProviderKeys=merchant_id=1")]
    [InlineData("      Payment__ProviderKeys: \"merchant_id=1\"")]
    [InlineData("Payment:ProviderKeys = x")]
    [InlineData("{ \"Payment\": { \"ProviderKeys\": \"merchant_id=1\" } }")]
    [InlineData("merchant_key=abc")]
    [InlineData("MERCHANT_SALT: abc")]
    [InlineData("printf '\"merchant_key\"=%s' x")]
    [InlineData("echo 'merchant_salt = s' > keys")]
    [InlineData("{ \"Payment\": { \"ProviderKeys\":\n      \"merchant_id=1\" } }")]
    [InlineData("      Payment__ProviderKeys:\n        merchant_id=1")]
    [InlineData("merchant_salt =\n\tabc")]
    public void KeyValueScan_OnViolationSample_ReportsTheRuleOnItsLine(string sample)
    {
        Assert.Equal([(PaymentKeysScan.KeyValue, 2)], PaymentKeysScan.ScanForKeyValues("first line\n" + sample + "\nlast line"));
    }

    [Theory]
    [InlineData("      - payment_provider_keys")]
    [InlineData("  payment_provider_keys:\n    file: ${SHOPFOLIO_SECRETS_DIR:-/etc/shopfolio/secrets}/payment_provider_keys")]
    [InlineData("{ \"Payment\": { \"ProviderKeys\": \"\" } }")]
    [InlineData("# merchant_key and merchant_salt are in the secret file")]
    [InlineData("printf 'made-up:%s' \"$(gen)\" | put payment_provider_keys 1654")]
    [InlineData("MERCHANT_KEY=\nOTHER=1")]
    [InlineData("Payment__ProviderKeys:\n  # set in the secret file")]
    public void KeyValueScan_OnCleanSample_ReportsNothing(string sample)
    {
        Assert.Empty(PaymentKeysScan.ScanForKeyValues(sample));
    }

    [Theory]
    [InlineData("        run: dotnet test --filter-class \"*PayTrNotificationTests\"")]
    [InlineData("      MERCHANT_KEY: ${{ secrets.STORE_KEY }}")]
    [InlineData("      KEYS: ${{ secrets.PAYTR_KEYS }}")]
    [InlineData("      Payment__ProviderKeys: ${{ secrets.KEYS }}")]
    [InlineData("      - payment_provider_keys")]
    public void CiScan_OnViolationSample_ReportsTheRuleOnItsLine(string sample)
    {
        Assert.Equal([(PaymentKeysScan.CiMention, 2)], PaymentKeysScan.ScanCiFile("jobs:\n" + sample + "\n"));
    }

    [Fact]
    public void CiScan_OnCleanSample_ReportsNothing()
    {
        Assert.Empty(PaymentKeysScan.ScanCiFile("jobs:\n  test:\n    steps:\n      - run: dotnet test --project tests/Shopfolio.UnitTests\n"));
    }

    // The tracked files where a key could be written: the application's settings, everything under deploy, the GitHub folder and the CI and local reference scripts.
    private static IEnumerable<string> KeyValueFilePaths()
    {
        string root = RepositoryRoot.Find();
        return Directory.EnumerateFiles(Path.Combine(root, "src", "Shopfolio.Web"), "appsettings*.json", SearchOption.TopDirectoryOnly)
            .Concat(TextFiles(Path.Combine(root, "deploy")))
            .Concat(TextFiles(Path.Combine(root, ".github")))
            .Concat(TextFiles(Path.Combine(root, "scripts", "ci")))
            .Concat(TextFiles(Path.Combine(root, "scripts", "local-reference")))
            .Order(StringComparer.Ordinal);
    }

    private static IEnumerable<string> CiFilePaths()
    {
        string root = RepositoryRoot.Find();
        return Directory.EnumerateFiles(Path.Combine(root, ".github", "workflows"), "*.yml", SearchOption.TopDirectoryOnly)
            .Append(Path.Combine(root, ".github", "ci-commands.env"))
            .Append(Path.Combine(root, ".github", "ci-run.sh"))
            .Concat(TextFiles(Path.Combine(root, "scripts", "ci")))
            .Order(StringComparer.Ordinal);
    }

    private static IEnumerable<string> TextFiles(string directory) =>
        Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Where(path => !File.ReadAllText(path).Contains('\0', StringComparison.Ordinal));

    private static string Relative(string path) => Path.GetRelativePath(RepositoryRoot.Find(), path).Replace('\\', '/');

    // The line of host-secrets.sh that writes the payment provider's secret file: printf '<content>' ... | put payment_provider_keys.
    [GeneratedRegex(@"printf '([^']*)'[^\n|]*\|\s*put payment_provider_keys\b")]
    private static partial Regex LocalReferenceKeysLine();
}
