using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The administrator accounts' structural rules (T379; 02 10.2.1, 10.2.3, 10.2.7): (1) no page or route opens an administrator account by the person's own registration -
/// the account is born from an invitation or from the management command alone; (2) no page or route recovers lost access to every administrator account - the recovery is the
/// installer's command; (3) the open call that writes an account without an invitation, <c>CreateAdminAsync</c>, is called from the management commands only; (4) the
/// Administration module declares no role, permission or privilege name - the administrators have one authority. The scan reads every source and page file of the web project;
/// full-line comments are blanked first. The sample lines of the theory are the scan's own violation examples.
/// </summary>
public sealed partial class AdminAccountRulesScanTests
{
    private const string SelfRegistration = "opens an administrator account by self-registration (02 10.2.1)";

    private const string AccessRecovery = "recovers lost administrator access inside the product (02 10.2.7)";

    private const string CreateOutsideCommands = "calls CreateAdminAsync outside the management commands (02 10.2.1, 10.2.7)";

    private const string RoleName = "names a role, permission or privilege in the Administration module (02 10.2.3; KD-18)";

    private const string AdminModuleFolder = "/Modules/Administration/";

    private const string CommandsFolder = "/Commands/";

    [Fact]
    public void TheApplicationSource_HasNoAdministratorRegistration_NoAccessRecovery_NoCreationOutsideTheCommands_AndNoRoleName()
    {
        string web = Path.Combine(RepositoryRoot.Find(), "src", "Shopfolio.Web");
        List<string> found = [];
        foreach (string file in Directory.EnumerateFiles(web, "*.*", SearchOption.AllDirectories).Where(IsSource))
        {
            string relative = Path.GetRelativePath(RepositoryRoot.Find(), file).Replace('\\', '/');
            found.AddRange(Scan(relative, File.ReadAllText(file)).Select(f => $"{relative}:{f.Line}: {f.Rule}"));
        }

        Assert.Empty(found);
    }

    [Theory]
    [InlineData("src/Shopfolio.Web/Pages/Admin/Register.cshtml.cs", "public sealed class RegisterModel : PageModel { }", SelfRegistration)]
    [InlineData("src/Shopfolio.Web/Pages/Admin/Index.cshtml", "@page \"/yonetim/yonetici-kayit\"", SelfRegistration)]
    [InlineData("src/Shopfolio.Web/Pages/Admin/Index.cshtml", "@page \"/admin/sign-up\"", SelfRegistration)]
    [InlineData("src/Shopfolio.Web/Pages/Admin/Recover.cshtml.cs", "public sealed class RecoverModel : PageModel { }", AccessRecovery)]
    [InlineData("src/Shopfolio.Web/Pages/Admin/Index.cshtml", "@page \"/yonetim/erisim-kurtarma\"", AccessRecovery)]
    [InlineData("src/Shopfolio.Web/Pages/Account/Register.cshtml.cs", "public sealed class RegisterModel : PageModel { }", null)]
    [InlineData("src/Shopfolio.Web/Pages/Admin/Login.cshtml", "@page \"/yonetim/giris\"", null)]
    [InlineData("src/Shopfolio.Web/Pages/Admin/ForgotPassword.cshtml", "@page \"/yonetim/sifremi-unuttum\"", null)]
    [InlineData("src/Shopfolio.Web/Pages/Admin/Invitation.cshtml", "@page \"/yonetim/davet/{key}\"", null)]
    [InlineData("src/Shopfolio.Web/Pages/Admin/Index.cshtml.cs", "await _admin.CreateAdminAsync(id, name, email, password, ct);", CreateOutsideCommands)]
    [InlineData("src/Shopfolio.Web/Commands/CreateAdminCommand.cs", "await _admin.CreateAdminAsync(id, name, email, password, ct);", null)]
    [InlineData("src/Shopfolio.Web/Modules/Administration/AdministrationModule.cs", "public async Task<AdminCreationResult> CreateAdminAsync(Guid adminId, string name)", null)]
    [InlineData("src/Shopfolio.Web/Modules/Administration/Admin.cs", "public string Role { get; set; }", RoleName)]
    [InlineData("src/Shopfolio.Web/Modules/Administration/Contracts/IAdministrationModule.cs", "Task GrantPermissionAsync(Guid id);", RoleName)]
    [InlineData("src/Shopfolio.Web/Modules/Membership/Customer.cs", "public string Role { get; set; }", null)]
    [InlineData("src/Shopfolio.Web/Modules/Administration/Admin.cs", "// There is no role here.", null)]
    public void Scan_FindsEachViolation_AndOnlyThose(string path, string text, string? expectedRule)
    {
        IReadOnlyList<(string Rule, int Line)> findings = Scan(path, text);

        if (expectedRule is null)
        {
            Assert.Empty(findings);
        }
        else
        {
            Assert.Contains(findings, f => f.Rule == expectedRule);
        }
    }

    private static bool IsSource(string file)
    {
        string normalized = file.Replace('\\', '/');
        return !normalized.Contains("/obj/", StringComparison.Ordinal) && !normalized.Contains("/bin/", StringComparison.Ordinal)
            && !normalized.Contains("/Infrastructure/Migrations/", StringComparison.Ordinal)
            && (normalized.EndsWith(".cs", StringComparison.Ordinal) || normalized.EndsWith(".cshtml", StringComparison.Ordinal));
    }

    private static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        string clean = BlankCommentLines(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];

        bool isPageOrRoute = path.Contains("/Pages/", StringComparison.Ordinal) || path.Contains("/Areas/", StringComparison.Ordinal);
        if (isPageOrRoute)
        {
            // A page or route is the file's own name and every @page / route template it declares.
            string subject = path + "\n" + string.Join("\n", RouteText().Matches(clean).Select(m => m.Value));
            if (SelfRegistrationName().IsMatch(subject))
            {
                findings.Add((SelfRegistration, 1));
            }

            if (AccessRecoveryName().IsMatch(subject))
            {
                findings.Add((AccessRecovery, 1));
            }
        }

        if (!path.Contains(CommandsFolder, StringComparison.Ordinal) && !path.Contains(AdminModuleFolder, StringComparison.Ordinal))
        {
            Add(findings, CreateOutsideCommands, CreateAdminCall(), clean);
        }

        if (path.Contains(AdminModuleFolder, StringComparison.Ordinal))
        {
            Add(findings, RoleName, RoleWord(), clean);
        }

        return [.. findings.OrderBy(f => f.Line)];
    }

    private static void Add(List<(string Rule, int Line)> findings, string rule, Regex pattern, string text)
    {
        foreach (Match match in pattern.Matches(text))
        {
            findings.Add((rule, text.Take(match.Index).Count(c => c == '\n') + 1));
        }
    }

    // A full-line comment is blanked and keeps its line, so a line number stays true.
    private static string BlankCommentLines(string text) => string.Join('\n', text.Split('\n').Select(l => l.TrimStart().StartsWith("//", StringComparison.Ordinal) ? string.Empty : l));

    [GeneratedRegex(@"@page\s+""[^""]*""|\[(?:Http\w+|Route)\(""[^""]*""\)\]|MapGet\(""[^""]*""|MapPost\(""[^""]*""", RegexOptions.IgnoreCase)]
    private static partial Regex RouteText();

    // The administrator side of a page: a path or route that names the administrator or the panel together with a registration word.
    [GeneratedRegex(@"(?i)(admin|yonetim|yonetici)[^\n]*(register|signup|sign-up|kayit)|(register|signup|sign-up|kayit)[^\n]*(admin|yonetim|yonetici)")]
    private static partial Regex SelfRegistrationName();

    [GeneratedRegex(@"(?i)(admin|yonetim|yonetici)[^\n]*(recover|recovery|kurtar|rescue|restore-access)|(recover|recovery|kurtar|rescue|restore-access)[^\n]*(admin|yonetim|yonetici)")]
    private static partial Regex AccessRecoveryName();

    [GeneratedRegex(@"\bCreateAdminAsync\s*\(")]
    private static partial Regex CreateAdminCall();

    [GeneratedRegex(@"\b\w*(Role|Permission|Privilege)\w*\b")]
    private static partial Regex RoleWord();
}
