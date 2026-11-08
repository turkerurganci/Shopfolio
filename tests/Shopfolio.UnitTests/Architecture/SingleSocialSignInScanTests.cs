using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Google is the only social sign-in (02 3.13.6; KK1 of T343): no other provider's sign-in exists in the application source. The scan reads every source and page file
/// of the web project; the two places that name a social network are not a sign-in - the generated migrations and snapshot (the corporate content's social link table) and
/// <c>CorporateContent/SocialPlatform.cs</c> (the list of networks a firm may link on its public pages).
/// </summary>
public sealed class SingleSocialSignInScanTests
{
    private static readonly string[] Providers = ["facebook", "appleid", "addapple", "signinwithapple", "twitter", "github", "microsoftaccount", "linkedin"];

    private static readonly string[] AllowedPaths = ["/Infrastructure/Migrations/", "/Modules/CorporateContent/Contracts/SocialPlatform.cs", "/obj/", "/bin/"];

    [Fact]
    public void TheApplicationSource_NamesNoSocialSignInProviderButGoogle()
    {
        string web = Path.Combine(RepositoryRoot.Find(), "src", "Shopfolio.Web");
        List<string> found = [];
        foreach (string file in Directory.EnumerateFiles(web, "*.*", SearchOption.AllDirectories).Where(f => f.EndsWith(".cs", StringComparison.Ordinal) || f.EndsWith(".cshtml", StringComparison.Ordinal)))
        {
            found.AddRange(Scan(file, File.ReadAllText(file)));
        }

        Assert.Empty(found);
    }

    [Theory]
    [InlineData("src/Shopfolio.Web/Program.cs", "builder.Services.AddAuthentication().AddFacebook(o => { });", true)]
    [InlineData("src/Shopfolio.Web/Pages/Account/Login.cshtml", "<a href=\"/signin-FACEBOOK\">", true)]
    [InlineData("src/Shopfolio.Web/Modules/Membership/GoogleSignIns.cs", "AddGoogle(o => { });", false)]
    [InlineData("src/Shopfolio.Web/Infrastructure/Migrations/20261104095121_X.cs", "facebook", false)]
    [InlineData("src/Shopfolio.Web/Modules/CorporateContent/Contracts/SocialPlatform.cs", "Facebook,", false)]
    [InlineData("src/Shopfolio.Web/Modules/CorporateContent/Contracts/OtherFile.cs", "Facebook,", true)]
    public void Scan_FindsAProviderNameInAnyCase_ExceptInTheTwoNamedPlaces(string path, string text, bool expectedFinding)
    {
        Assert.Equal(expectedFinding, Scan(path, text).Count > 0);
    }

    private static List<string> Scan(string path, string text)
    {
        string normalized = path.Replace('\\', '/');
        if (AllowedPaths.Any(a => normalized.Contains(a, StringComparison.Ordinal)))
        {
            return [];
        }

        return [.. Providers.Where(p => text.Contains(p, StringComparison.OrdinalIgnoreCase)).Select(p => $"{normalized}: {p}")];
    }
}
