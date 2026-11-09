using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The sign-in limit L-1 is the password's alone (T377 criterion 10, KK10; 02 8.2.6): "sign-in with Google does not enter L-1". Two source rules: (1) the limit is named only by the
/// files that own the attempt mechanism - the password regime, the limit values and inventory, the attempt record and the temporary-record parts - in every written form: the
/// qualified member, the same split over lines or spaces, a <c>using static</c> import of the enum and a type alias; (2) the Google sign-in source touches no attempt counter, no
/// password regime and no limit. Comments are blanked first. The theory lines are the scan's own violation examples.
/// </summary>
public sealed partial class SignInLimitSourceScanTests
{
    private const string NamesL1 = "names the sign-in limit L-1 outside the files that own it (02 8.2.6)";

    private const string GoogleCounts = "counts or limits inside the Google sign-in (02 8.2.6)";

    private static readonly string[] _owners =
    [
        "src/Shopfolio.Web/Infrastructure/Security/PasswordRegime.cs",
        "src/Shopfolio.Web/Infrastructure/Security/AttemptLimits.cs",
        "src/Shopfolio.Web/Infrastructure/Security/AttemptLimitInventory.cs",
        "src/Shopfolio.Web/Infrastructure/Security/AttemptRecord.cs",
        "src/Shopfolio.Web/Infrastructure/Disposal/TemporaryRecordParts.cs",
        "src/Shopfolio.Web/Infrastructure/Values/AttemptLimit.cs",
    ];

    private static readonly string[] _googleFiles =
    [
        "src/Shopfolio.Web/Modules/Membership/GoogleSignIns.cs",
        "src/Shopfolio.Web/Modules/Membership/Contracts/GoogleRecords.cs",
        "src/Shopfolio.Web/Modules/Membership/Contracts/Results/GoogleSignInOutcome.cs",
        "src/Shopfolio.Web/Infrastructure/Integrations/IGoogleIdentityGateway.cs",
        "src/Shopfolio.Web/Infrastructure/Integrations/GoogleIdentityResult.cs",
    ];

    [Fact]
    public void TheApplicationSource_NamesL1OnlyInTheOwningFiles_AndTheGoogleSignInCountsNothing()
    {
        string root = RepositoryRoot.Find();
        string web = Path.Combine(root, "src", "Shopfolio.Web");
        List<string> found = [];
        foreach (string file in Directory.EnumerateFiles(web, "*.*", SearchOption.AllDirectories).Where(IsSource))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            found.AddRange(Scan(relative, File.ReadAllText(file)).Select(f => $"{relative}: {f}"));
        }

        Assert.Empty(found);
        foreach (string google in _googleFiles)
        {
            Assert.True(File.Exists(Path.Combine(root, google)), google + " is missing; update the scan with the move.");
        }

        foreach (string owner in _owners)
        {
            Assert.True(File.Exists(Path.Combine(root, owner)), owner + " is missing; update the scan with the move.");
        }
    }

    [Theory]
    [InlineData("src/Shopfolio.Web/Modules/Membership/GoogleSignIns.cs", "await _attempts.RecordAsync(AttemptLimit.L1, subject, ct);", NamesL1)]
    [InlineData("src/Shopfolio.Web/Modules/Membership/GoogleSignIns.cs", "AttemptLimit . L1", NamesL1)]
    [InlineData("src/Shopfolio.Web/Modules/Membership/MembershipModule.cs", "AttemptLimit\n    .L1", NamesL1)]
    [InlineData("src/Shopfolio.Web/Modules/Membership/MembershipModule.cs", "AttemptLimit\r\n.L1", NamesL1)]
    [InlineData("src/Shopfolio.Web/Modules/Membership/MembershipModule.cs", "using static Shopfolio.Infrastructure.Values.AttemptLimit;", NamesL1)]
    [InlineData("src/Shopfolio.Web/Modules/Membership/MembershipModule.cs", "using Limit = Shopfolio.Infrastructure.Values.AttemptLimit;", NamesL1)]
    [InlineData("src/Shopfolio.Web/Modules/Membership/MembershipModule.cs", "var l = Enum.Parse<AttemptLimit>(\"L1\");", NamesL1)]
    [InlineData("src/Shopfolio.Web/Modules/Membership/GoogleSignIns.cs", "private readonly AttemptCounter _attempts;", GoogleCounts)]
    [InlineData("src/Shopfolio.Web/Modules/Membership/GoogleSignIns.cs", "private readonly PasswordRegime _passwords;", GoogleCounts)]
    [InlineData("src/Shopfolio.Web/Modules/Membership/GoogleSignIns.cs", "AttemptSubject subject = AttemptSubject.Ip(address);", GoogleCounts)]
    [InlineData("src/Shopfolio.Web/Infrastructure/Security/PasswordRegime.cs", "AttemptLimit.L1", null)]
    [InlineData("src/Shopfolio.Web/Infrastructure/Security/AttemptLimits.cs", "AttemptLimit.L1 => new Rule();", null)]
    [InlineData("src/Shopfolio.Web/Modules/Membership/MembershipModule.cs", "// AttemptLimit.L1 is the password's", null)]
    [InlineData("src/Shopfolio.Web/Modules/Membership/MembershipModule.cs", "/* AttemptLimit.L1 */ int x = 1;", null)]
    [InlineData("src/Shopfolio.Web/Modules/Membership/MembershipModule.cs", "AttemptLimit.L2", null)]
    [InlineData("src/Shopfolio.Web/Modules/Membership/GoogleSignIns.cs", "// no attempt counter here", null)]
    public void Scan_FindsEachViolation_AndOnlyThose(string path, string text, string? expectedRule)
    {
        List<string> findings = Scan(path, text);

        if (expectedRule is null)
        {
            Assert.Empty(findings);
        }
        else
        {
            Assert.Contains(expectedRule, findings);
        }
    }

    private static bool IsSource(string file)
    {
        string normalized = file.Replace('\\', '/');
        return !normalized.Contains("/obj/", StringComparison.Ordinal) && !normalized.Contains("/bin/", StringComparison.Ordinal)
            && !normalized.Contains("/Infrastructure/Migrations/", StringComparison.Ordinal)
            && (normalized.EndsWith(".cs", StringComparison.Ordinal) || normalized.EndsWith(".cshtml", StringComparison.Ordinal));
    }

    private static List<string> Scan(string path, string text)
    {
        string clean = BlockComments().Replace(text, m => new string('\n', m.Value.Count(c => c == '\n')));
        clean = LineComments().Replace(clean, string.Empty);
        List<string> findings = [];
        bool owner = _owners.Contains(path, StringComparer.Ordinal);
        if (!owner && (QualifiedL1().IsMatch(clean) || StaticImport().IsMatch(clean) || TypeAlias().IsMatch(clean) || ParsedL1().IsMatch(clean)))
        {
            findings.Add(NamesL1);
        }

        if (_googleFiles.Contains(path, StringComparer.Ordinal) && GoogleCounting().IsMatch(clean))
        {
            findings.Add(GoogleCounts);
        }

        return findings;
    }

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex BlockComments();

    [GeneratedRegex(@"//[^\n]*")]
    private static partial Regex LineComments();

    [GeneratedRegex(@"\bAttemptLimit\s*\.\s*L1\b")]
    private static partial Regex QualifiedL1();

    [GeneratedRegex(@"\busing\s+static\s+[\w.]*\bAttemptLimit\s*;")]
    private static partial Regex StaticImport();

    [GeneratedRegex(@"\busing\s+\w+\s*=\s*[\w.]*\bAttemptLimit\s*;")]
    private static partial Regex TypeAlias();

    [GeneratedRegex(@"\bEnum\s*\.\s*(Try)?Parse\s*(<\s*AttemptLimit\s*>|\(\s*typeof\s*\(\s*AttemptLimit\s*\))")]
    private static partial Regex ParsedL1();

    [GeneratedRegex(@"\b(AttemptCounter|PasswordRegime|AttemptLimits?|AttemptSubject|AttemptRecord)\b")]
    private static partial Regex GoogleCounting();
}
