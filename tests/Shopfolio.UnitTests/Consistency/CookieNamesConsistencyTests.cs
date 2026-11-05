using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Security;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the cookie name list of the code equal to the closed list of the coding guidelines in both directions (T224 KK3; 09 5.3 item 5; 07 2; 09 21.4
/// item 1). A name added in the code without the inventory, or an inventory name without a constant, turns this red. Reads repository files only.
/// Runs in the Lint step.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class CookieNamesConsistencyTests
{
    [Fact]
    public void CookieNamesOf09_EqualTheClosedListOfTheCode_InBothDirections()
    {
        string[] lines = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "09_CODING_GUIDELINES.md")).ReplaceLineEndings("\n").Split('\n');
        string item = Assert.Single(lines, l => l.StartsWith("5. **Çerez adı**", StringComparison.Ordinal));

        string[] documented = [.. HostNameInBackticks().Matches(item).Select(m => m.Groups[1].Value)];

        Assert.NotEmpty(documented);
        Assert.Equal(documented.Order(StringComparer.Ordinal), CookieNames.All.Order(StringComparer.Ordinal));
        Assert.All(CookieNames.All, name => Assert.StartsWith(CookieNames.Prefix, name, StringComparison.Ordinal));
    }

    [GeneratedRegex(@"`(__Host-[a-z-]+)`", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex HostNameInBackticks();
}
