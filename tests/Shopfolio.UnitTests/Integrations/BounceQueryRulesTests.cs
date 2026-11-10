using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Jobs;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The returned-address query's rules that need no database and no service (T408 criteria 1, 7, 8, KK1, KK7, KK8; 08 2.2.3, 4.1 DV-22; K-1241, K-1248): the window and the
/// interval are the integration document's numbers; no code deletes or adds an address of the service's list; the runbook states the account-level list setting
/// ("Bounces" only) and the manual removal; the DV-22 row carries the date of this task and names what waits for the real account.
/// </summary>
public sealed class BounceQueryRulesTests
{
    private static string Root => RepositoryRoot.Find();

    [Fact]
    public void TheWindowIsTwentyFourHours_AndTheInterval_FifteenMinutes()
    {
        Assert.Equal(TimeSpan.FromHours(24), JobTimings.BounceQueryWindow);
        Assert.Equal(TimeSpan.FromMinutes(15), JobTimings.BounceQueryInterval);
    }

    [Fact]
    public void NoSourceFile_CallsADeleteOrAPutOfTheServicesSuppressionList_KK7()
    {
        // Comments are blanked first: a comment may say "the product never deletes" and is not a call. The words are the library's operation names.
        Regex operation = new(@"\b(Delete|Put)\w*Suppress\w*", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(Root, "src"), "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                || file.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            {
                continue;
            }

            string code = Regex.Replace(File.ReadAllText(file), @"//[^\r\n]*|/\*.*?\*/", string.Empty, RegexOptions.Singleline | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));
            if (operation.IsMatch(code))
            {
                findings.Add(Path.GetRelativePath(Root, file));
            }
        }

        Assert.Empty(findings);
    }

    [Fact]
    public void TheSuppressionListIsReadOnlyThroughOneCall_InTheGatewayFileOnly_KK7()
    {
        string gateway = Path.Combine("src", "Shopfolio.Web", "Infrastructure", "Integrations", "SesEmailGateway.cs");
        List<string> users = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(Root, "src"), "*.cs", SearchOption.AllDirectories))
        {
            if (File.ReadAllText(file).Contains("ListSuppressedDestinationsAsync", StringComparison.Ordinal))
            {
                users.Add(Path.GetRelativePath(Root, file));
            }
        }

        Assert.Equal([gateway], users);
    }

    [Fact]
    public void TheRunbook_SaysTheAccountListIsOpenedForBouncesOnly_AndThatTheAddressIsRemovedByHandInTheConsole_KK7()
    {
        string runbook = File.ReadAllText(Path.Combine(Root, "Docs", "INTEGRATION_RUNBOOKS", "AMAZON_SES.md"));

        Assert.Matches(new Regex(@"yalnız \*\*Bounces\*\* açık, Complaints kapalı", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)), runbook);
        Assert.Contains("ürün listeden adres silmez", runbook, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"(?m)^## \d+\. Geri dönen adresler", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)), runbook);
    }

    [Fact]
    public void TheDv22Row_CarriesTheDateOfThisTask_AndNamesWhatWaitsForTheRealAccount_KK8()
    {
        string[] lines = File.ReadAllLines(Path.Combine(Root, "Docs", "08_INTEGRATION_SPEC.md"));

        string row = Assert.Single(lines, l => l.StartsWith("| DV-22 |", StringComparison.Ordinal));
        string lastCell = row.TrimEnd().TrimEnd('|').Split('|')[^1].Trim();

        Assert.DoesNotContain("Bekliyor", lastCell, StringComparison.Ordinal);
        Assert.Matches(@"\(\d{4}-\d{2}-\d{2}, T408", lastCell);
        Assert.Contains("OWNER_INPUT", lastCell, StringComparison.Ordinal);
    }
}
