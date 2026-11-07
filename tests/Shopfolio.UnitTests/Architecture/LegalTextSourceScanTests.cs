using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The legal text rules on the application source (T366 KK5, KK6, KK10; 11 T366 criteria 3, 5, 6, 10; 09 19.2 item 3): no consent, seen or banner record, no attachment of the two
/// generated texts, and a legal text row is made only by the generator and the publication. Each rule goes red on a deliberate violation sample, in every form the scan claims to see.
/// </summary>
public sealed class LegalTextSourceScanTests
{
    private const string SamplePath = "src/Shopfolio.Web/Pages/Samples/ConsentFileAndRowCreation.cs";

    private static string Sample() => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "LegalText", "ConsentAttachmentAndRowCreation.cs"));

    [Fact]
    public void ApplicationSource_HasNoConsentRecordNoAttachment_AndMakesLegalTextRowsOnlyInTheOwners()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (!LegalTextSourceScan.Applies(relative))
            {
                continue;
            }

            scanned.Add(relative);
            foreach ((string rule, int line) in LegalTextSourceScan.Scan(relative, File.ReadAllText(file)))
            {
                findings.Add($"{relative}:{line} {rule}");
            }
        }

        // The scan reached the owners and the order, so an empty result is not an empty scan.
        Assert.All(LegalTextSourceScan.RowOwners, f => Assert.Contains(f, scanned));
        Assert.Contains("src/Shopfolio.Web/Modules/Ordering/Order.cs", scanned);
        Assert.Empty(findings);
    }

    [Fact]
    public void TheRowOwners_ReallyCreateRows_SoNoFileOfTheClosedListIsDead()
    {
        string root = RepositoryRoot.Find();

        foreach (string file in LegalTextSourceScan.RowOwners.Where(f => !f.EndsWith("LegalTextVersion.cs", StringComparison.Ordinal)))
        {
            Assert.Contains("LegalTextVersion.CreatePublished(", File.ReadAllText(Path.Combine(root, file)), StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ViolationSample_EachRuleReportsItsLines_AndNotTheCommentsOrTheNeighbours()
    {
        IReadOnlyList<(string Rule, int Line)> findings = LegalTextSourceScan.Scan(SamplePath, Sample());

        Assert.Equal(
            [
                (LegalTextSourceScan.ConsentOrSeenRecord, 5),
                (LegalTextSourceScan.ConsentOrSeenRecord, 6),
                (LegalTextSourceScan.ConsentOrSeenRecord, 7),
                (LegalTextSourceScan.ConsentOrSeenRecord, 8),
                (LegalTextSourceScan.ConsentOrSeenRecord, 9),
                (LegalTextSourceScan.AttachesALegalText, 10),
                (LegalTextSourceScan.AttachesALegalText, 11),
                (LegalTextSourceScan.CreatesARowOutsideTheOwners, 12),
                (LegalTextSourceScan.CreatesARowOutsideTheOwners, 13),
                (LegalTextSourceScan.CreatesARowOutsideTheOwners, 14),
            ],
            findings);
    }

    [Fact]
    public void TheOwners_MayCreateRows_ButNoNeighbourMay()
    {
        const string make = "var row = LegalTextVersion.CreatePublished(type, 1, now, text, now);";

        foreach (string owner in LegalTextSourceScan.RowOwners)
        {
            Assert.DoesNotContain(LegalTextSourceScan.Scan(owner, make), f => f.Rule == LegalTextSourceScan.CreatesARowOutsideTheOwners);
        }

        Assert.Contains(LegalTextSourceScan.Scan("src/Shopfolio.Web/Modules/Settings/CompanyIdentities.cs", make), f => f.Rule == LegalTextSourceScan.CreatesARowOutsideTheOwners);
        Assert.Contains(LegalTextSourceScan.Scan("src/Shopfolio.Web/Modules/Ordering/OrderingModule.cs", make), f => f.Rule == LegalTextSourceScan.CreatesARowOutsideTheOwners);
        Assert.Contains(LegalTextSourceScan.Scan("src/Shopfolio.Web/Pages/Panel/Legal.cshtml", "var r = new LegalTextVersion();"), f => f.Rule == LegalTextSourceScan.CreatesARowOutsideTheOwners);
    }

    [Theory]
    [InlineData("// public bool AcceptedPrivacyNotice { get; set; }")]
    [InlineData("/* CookiePolicySeenAt */ var x = 1;")]
    [InlineData("var s = new CheckboxAcceptance();")]
    [InlineData("var t = LegalTextType.PrivacyNotice;")]
    [InlineData("var v = await settings.GetLatestLegalTextVersionAsync(type, ct);")]
    [InlineData("var p = Order.PreInformationFormText; // no .pdf attached")]
    public void CommentsAndTheNeighbours_AreNoFindings(string line)
    {
        Assert.Empty(LegalTextSourceScan.Scan("src/Shopfolio.Web/Pages/Samples/Other.cs", line));
    }

    [Theory]
    [InlineData("string a = \"PrivacyNoticeConsent\";", 1)]
    [InlineData("string a = \"privacy_notice_seen_at\";", 1)]
    [InlineData("string a = \"consent_cookie_policy\";", 1)]
    [InlineData("var b = new CookieBanner();", 1)]
    [InlineData("var c = ExplicitConsent.Given;", 1)]
    public void EveryFormOfAConsentRecord_IsFound(string line, int expectedLine)
    {
        Assert.Contains(LegalTextSourceScan.Scan("src/Shopfolio.Web/Pages/Samples/Form.cs", line), f => f.Rule == LegalTextSourceScan.ConsentOrSeenRecord && f.Line == expectedLine);
    }

    [Fact]
    public void AnAttachmentInAFileThatNamesNeitherText_IsNotThisRulesBusiness_AndASplitCallIsFound()
    {
        Assert.Empty(LegalTextSourceScan.Scan("src/Shopfolio.Web/Pages/Samples/Image.cs", "var f = File(bytes, \"application/pdf\");"));

        const string split = "var a = order.DistanceSalesContractText;\nmessage.\n    AddAttachment(a);\nvar r = new\n    LegalTextVersion\n    {\n    };";
        IReadOnlyList<(string Rule, int Line)> findings = LegalTextSourceScan.Scan("src/Shopfolio.Web/Pages/Samples/Split.cs", split);
        Assert.Contains((LegalTextSourceScan.AttachesALegalText, 3), findings);
        Assert.Contains((LegalTextSourceScan.CreatesARowOutsideTheOwners, 4), findings);
    }
}
