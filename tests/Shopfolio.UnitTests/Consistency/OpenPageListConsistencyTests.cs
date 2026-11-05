using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Security;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the open pages of the code equal to the closed list of <c>07 2.3.1</c> in both directions (T207 criteria 2 and 3; KK2, KK3; 09 21.4 item 1): a page
/// opened in the code without the document, or listed in the document without an entry in the code, turns this red. The two cookie names of the schemes are
/// compared with the same paragraph. Reads repository files only. Runs in the Lint step.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class OpenPageListConsistencyTests
{
    private const string ParagraphStart = "**2.3.1 Şemalar alana göre uygulanır:**";

    private const string PanelListStart = "açık sayfalar ";

    private const string PanelListEnd = "`/panel/yeniden-dogrulama`";

    private const string AccountListStart = "tek istisna hesap silmenin sonuç hâlidir,";

    private const string AccountListEnd = "(§4.25.7";

    private static string Paragraph()
    {
        string[] lines = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "07_API_DESIGN.md")).ReplaceLineEndings("\n").Split('\n');
        return Assert.Single(lines, l => l.StartsWith(ParagraphStart, StringComparison.Ordinal));
    }

    private static string Between(string text, string start, string end)
    {
        int from = text.IndexOf(start, StringComparison.Ordinal);
        Assert.True(from >= 0, $"'{start}' is not in 07 2.3.1.");
        int to = text.IndexOf(end, from, StringComparison.Ordinal);
        Assert.True(to > from, $"'{end}' does not follow '{start}' in 07 2.3.1.");
        return text[from..to];
    }

    // `/panel/sifre-sifirlama[/{anahtar}]` is two addresses; `{anahtar}` is a parameter and the open list writes it as {}.
    private static IEnumerable<string> Addresses(string section, string area)
    {
        foreach (Match match in AddressInBackticks().Matches(section))
        {
            string address = match.Groups[1].Value;
            if (!address.StartsWith("/" + area, StringComparison.Ordinal))
            {
                continue;
            }

            int optional = address.IndexOf('[', StringComparison.Ordinal);
            if (optional < 0)
            {
                yield return ParameterForm(address);
                continue;
            }

            yield return address[..optional];
            yield return ParameterForm(address.Remove(optional, 1).Remove(address.IndexOf(']', StringComparison.Ordinal) - 1, 1));
        }
    }

    private static string ParameterForm(string address) => ParameterName().Replace(address, "{}");

    [Fact]
    public void OpenPanelPagesOf07_EqualTheListOfTheCode_InBothDirections()
    {
        string documented = Between(Paragraph(), PanelListStart, PanelListEnd);

        string[] fromDocument = [.. Addresses(documented, "panel").Distinct(StringComparer.Ordinal)];

        Assert.Equal(6, fromDocument.Length);
        Assert.Equal(fromDocument.Order(StringComparer.Ordinal), SessionSchemes.OpenPanelAddresses.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void OpenAccountPageOf07_EqualsTheListOfTheCode_InBothDirections()
    {
        string documented = Between(Paragraph(), AccountListStart, AccountListEnd);

        string[] fromDocument = [.. Addresses(documented, "hesap").Distinct(StringComparer.Ordinal)];

        Assert.NotEmpty(fromDocument);
        Assert.Equal(fromDocument.Order(StringComparer.Ordinal), SessionSchemes.OpenAccountAddresses.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void ThePanelsReauthenticationPage_IsNotInTheOpenList()
    {
        Assert.DoesNotContain(SessionSchemes.OpenPanelAddresses, a => a.Contains("yeniden-dogrulama", StringComparison.Ordinal));
        Assert.Contains("`/panel/yeniden-dogrulama` oturum ister", Paragraph(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheTwoSessionCookiesOf07_AreTheNamesTheSchemesRead()
    {
        string paragraph = Paragraph();

        Assert.Contains($"vitrinin oturumu `{CookieNames.Session}`", paragraph, StringComparison.Ordinal);
        Assert.Contains($"panelin oturumu `{CookieNames.AdminSession}`", paragraph, StringComparison.Ordinal);
        Assert.NotEqual(CookieNames.Session, CookieNames.AdminSession);
    }

    [GeneratedRegex(@"`(/[^`]+)`", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex AddressInBackticks();

    [GeneratedRegex(@"\{[^}]+\}", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ParameterName();
}
