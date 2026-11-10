using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the key and link scan over the application source and shows on a deliberate violation sample that each rule goes red (T222 KK1, KK5;
/// 09 15.1 items 1, 3 and 4; 09 19.2 item 3). The scan reads files and never writes one.
/// </summary>
public sealed class LinkKeySourceScanTests
{
    private const string SamplePath = "Architecture/ViolationSamples/LinkKeys/KeyMisuse.cs";

    private static string Sample() => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", SamplePath));

    [Fact]
    public void ApplicationSource_KeepsTheGeneratorInItsFolders_TheDerivationInNotificationsAndTheConsumingInPostHandlers()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file);
            string path = relative.Replace('\\', '/');
            if (path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal))
            {
                continue;
            }

            scanned.Add(path);
            foreach ((string rule, int line) in LinkKeySourceScan.Scan(relative, File.ReadAllText(file)))
            {
                findings.Add($"{path}:{line} {rule}");
            }
        }

        Assert.Contains(LinkKeySourceScan.LinkKeysFile, scanned);
        Assert.Contains(LinkKeySourceScan.LinkRecordsFile, scanned);
        Assert.Contains(scanned, f => f.StartsWith(LinkKeySourceScan.DeriveFolder, StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void NamedFilesAndFolders_ExistInTheApplicationSource()
    {
        string root = RepositoryRoot.Find();

        Assert.True(File.Exists(Path.Combine(root, LinkKeySourceScan.LinkKeysFile)));
        Assert.True(File.Exists(Path.Combine(root, LinkKeySourceScan.LinkRecordsFile)));
        Assert.True(Directory.Exists(Path.Combine(root, LinkKeySourceScan.DeriveFolder)));
        Assert.All(LinkKeySourceScan.GeneratorFolders, folder => Assert.True(Directory.Exists(Path.Combine(root, folder)), folder));
        Assert.All(LinkKeySourceScan.GeneratorExceptions, file => Assert.True(File.Exists(Path.Combine(root, file)), file));
    }

    [Fact]
    public void Scan_OnViolationSampleOutsideTheSecurityFolders_ReportsTheGeneratorTheDerivationAndTheConsuming()
    {
        IReadOnlyList<(string Rule, int Line)> findings = LinkKeySourceScan.Scan("src/Shopfolio.Web/Pages/Account/Misuse.cs", Sample());

        Assert.Equal(
            [
                (LinkKeySourceScan.GeneratorOutsideSecurity, 5),
                (LinkKeySourceScan.GeneratorOutsideSecurity, 6),
                (LinkKeySourceScan.GeneratorOutsideSecurity, 7),
                (LinkKeySourceScan.DeriveOutsideNotifications, 13),
                (LinkKeySourceScan.DeriveOutsideNotifications, 15),
                (LinkKeySourceScan.ConsumeOutsidePost, 18),
                (LinkKeySourceScan.ConsumeOutsidePost, 26),
                (LinkKeySourceScan.ConsumeOutsidePost, 29),
                (LinkKeySourceScan.ConsumeOutsidePost, 31),
            ],
            findings);
    }

    [Theory]
    [InlineData("src/Shopfolio.Web/Infrastructure/Security/Misuse.cs")]
    [InlineData("src/Shopfolio.Web/Modules/Membership/Misuse.cs")]
    public void Scan_OnViolationSampleInAGeneratorFolder_AllowsTheGeneratorAndReportsTheWeakSources(string path)
    {
        IReadOnlyList<(string Rule, int Line)> findings = LinkKeySourceScan.Scan(path, Sample());

        Assert.Equal(
            [
                (LinkKeySourceScan.WeakKeySource, 8),
                (LinkKeySourceScan.WeakKeySource, 9),
                (LinkKeySourceScan.WeakKeySource, 10),
                (LinkKeySourceScan.WeakKeySource, 11),
                (LinkKeySourceScan.DeriveOutsideNotifications, 13),
                (LinkKeySourceScan.DeriveOutsideNotifications, 15),
                (LinkKeySourceScan.ConsumeOutsidePost, 18),
                (LinkKeySourceScan.ConsumeOutsidePost, 26),
                (LinkKeySourceScan.ConsumeOutsidePost, 29),
                (LinkKeySourceScan.ConsumeOutsidePost, 31),
            ],
            findings);
    }

    [Fact]
    public void Scan_TheGeneratorExceptionIsOneFile_ItsNeighbourNamingTheSameDigestIsReported()
    {
        const string digest = "string Key(byte[] content) => Convert.ToHexStringLower(SHA256.HashData(content));";

        Assert.Equal(
            [
                "src/Shopfolio.Web/Modules/Catalog/ImageUploads.cs",
                "src/Shopfolio.Web/Modules/Settings/LegalTextTemplates.cs",
                "src/Shopfolio.Web/Infrastructure/Integrations/GoogleAuthorizationRequest.cs",
            ],
            LinkKeySourceScan.GeneratorExceptions);
        Assert.Empty(LinkKeySourceScan.Scan("src/Shopfolio.Web/Infrastructure/Integrations/GoogleAuthorizationRequest.cs", digest));
        Assert.Equal(
            [(LinkKeySourceScan.GeneratorOutsideSecurity, 1)],
            LinkKeySourceScan.Scan("src/Shopfolio.Web/Infrastructure/Integrations/GoogleIdentityGateway.cs", digest));
        Assert.Empty(LinkKeySourceScan.Scan("src/Shopfolio.Web/Modules/Settings/LegalTextTemplates.cs", digest));
        Assert.Empty(LinkKeySourceScan.Scan("src/Shopfolio.Web/Modules/Catalog/ImageUploads.cs", digest));
        Assert.Equal(
            [(LinkKeySourceScan.GeneratorOutsideSecurity, 1)],
            LinkKeySourceScan.Scan("src/Shopfolio.Web/Modules/Catalog/ImageServing.cs", digest));
    }

    [Fact]
    public void Scan_InNotifications_AllowsTheDerivation_ButNotTheGeneratorNorAnUnpostedConsume()
    {
        IReadOnlyList<(string Rule, int Line)> findings = LinkKeySourceScan.Scan(LinkKeySourceScan.DeriveFolder + "Body.cs", Sample());

        Assert.DoesNotContain(findings, f => f.Rule == LinkKeySourceScan.DeriveOutsideNotifications);
        Assert.Contains(findings, f => f.Rule == LinkKeySourceScan.GeneratorOutsideSecurity);
        Assert.Contains(findings, f => f.Rule == LinkKeySourceScan.ConsumeOutsidePost);
    }

    [Fact]
    public void Scan_InTheTwoNamedFiles_AllowsTheirOwnRuleOnly()
    {
        const string deriveText = "string Digest(Guid id) => SecretKey.Hash(Derive(id)); string K(LinkKeys k, Guid id) => k.Derive(id);";
        const string consumeText = "public Task<bool> TryConsumeAsync(Guid id, CancellationToken ct) => Run(id); void X() { TryConsumeAsync(id, ct); }";

        Assert.Empty(LinkKeySourceScan.Scan(LinkKeySourceScan.LinkKeysFile, deriveText));
        Assert.Equal([(LinkKeySourceScan.DeriveOutsideNotifications, 1)], LinkKeySourceScan.Scan(LinkKeySourceScan.LinkRecordsFile, deriveText));
        Assert.Empty(LinkKeySourceScan.Scan(LinkKeySourceScan.LinkRecordsFile, consumeText));
        Assert.Contains(LinkKeySourceScan.Scan("src/Shopfolio.Web/Pages/Account/X.cs", consumeText), f => f.Rule == LinkKeySourceScan.ConsumeOutsidePost);
    }

    [Fact]
    public void Scan_OnAPostHandlerThatConsumesInsideARunnerLambda_FindsNothing()
    {
        const string text = """
            public sealed class VerifyModel
            {
                public async Task<IActionResult> OnPostAsync(CancellationToken ct)
                {
                    Result r = await _runner.RunAsync(async token =>
                    {
                        bool taken = await _links.TryConsumeAsync(_linkId, token);
                        return new Result(taken);
                    }, ct);
                    return Page();
                }

                public async Task<IActionResult> OnGetAsync(CancellationToken ct)
                {
                    LinkReading reading = await _links.ReadAsync(Key, ct);
                    return Page();
                }
            }
            """;

        Assert.Empty(LinkKeySourceScan.Scan("src/Shopfolio.Web/Pages/Account/Verify.cshtml.cs", text));
    }

    [Fact]
    public void Scan_OnACallOfTheVerificationCommand_FindsItInAnOnGetHandlerAndNotInAnOnPostHandler()
    {
        const string onGet = "public async Task OnGetAsync(CancellationToken ct) { await _membership.VerifyEmailAsync(key, ct); }";
        const string onPost = "public async Task OnPostAsync(CancellationToken ct) { await _membership.VerifyEmailAsync(key, ct); }";
        const string page = "src/Shopfolio.Web/Pages/Account/Verify.cshtml.cs";

        Assert.Contains(LinkKeySourceScan.Scan(page, onGet), f => f.Rule == LinkKeySourceScan.ConsumeOutsidePost);
        Assert.Empty(LinkKeySourceScan.Scan(page, onPost));
    }

    [Fact]
    public void Scan_OnTheConsumingCommandsFile_AllowsTheConsumeInItsOneNamedMethodOnly()
    {
        const string own = "public async Task<R> VerifyEmailAsync(string key, CancellationToken ct) { return await _links.TryConsumeAsync(id, ct); }";
        const string other = "public async Task<R> ResendAsync(string key, CancellationToken ct) { return await _links.TryConsumeAsync(id, ct); }";
        const string callsIt = "public async Task<R> ResendAsync(string key, CancellationToken ct) { return await _module.VerifyEmailAsync(key, ct); }";
        const string file = "src/Shopfolio.Web/Modules/Membership/MembershipModule.cs";

        Assert.Empty(LinkKeySourceScan.Scan(file, own));
        Assert.Contains(LinkKeySourceScan.Scan(file, other), f => f.Rule == LinkKeySourceScan.ConsumeOutsidePost);
        Assert.Contains(LinkKeySourceScan.Scan(file, callsIt), f => f.Rule == LinkKeySourceScan.ConsumeOutsidePost);
        Assert.Contains(LinkKeySourceScan.Scan("src/Shopfolio.Web/Modules/Membership/Other.cs", own), f => f.Rule == LinkKeySourceScan.ConsumeOutsidePost);
    }

    [Fact]
    public void Scan_OnTheResetButton_AllowsTheConsumeInTheRegimeAndTheWrapperAndNothingElse_T342()
    {
        const string regime = "src/Shopfolio.Web/Infrastructure/Security/PasswordRegime.cs";
        const string wrapperFile = "src/Shopfolio.Web/Modules/Administration/AdministrationModule.cs";
        const string consumes = "public async Task<R> ResetPasswordAsync(A a, string key, CancellationToken ct) { return await _links.TryConsumeAsync(id, ct); }";
        const string consumesElsewhere = "public async Task<R> RequestResetAsync(A a, string key, CancellationToken ct) { return await _links.TryConsumeAsync(id, ct); }";
        const string wrapper = "public Task<R> ResetPasswordAsync(string key, string n, CancellationToken ct) { return _passwords.ResetPasswordAsync(_accounts, key, n, ct); }";
        const string otherCaller = "public Task<R> ReadAsync(string key, string n, CancellationToken ct) { return _passwords.ResetPasswordAsync(_accounts, key, n, ct); }";
        const string verifyInWrapper = "public Task<R> ResetPasswordAsync(string key, CancellationToken ct) { return _passwords.VerifyEmailAsync(key, ct); }";
        const string onGet = "public async Task OnGetAsync(CancellationToken ct) { await _admin.ResetPasswordAsync(key, n, null, ct); }";
        const string onPost = "public async Task OnPostAsync(CancellationToken ct) { await _admin.ResetPasswordAsync(key, n, null, ct); }";
        const string page = "src/Shopfolio.Web/Pages/Panel/Reset.cshtml.cs";

        Assert.Empty(LinkKeySourceScan.Scan(regime, consumes));
        Assert.Contains(LinkKeySourceScan.Scan(regime, consumesElsewhere), f => f.Rule == LinkKeySourceScan.ConsumeOutsidePost);
        Assert.Contains(LinkKeySourceScan.Scan(wrapperFile, consumes), f => f.Rule == LinkKeySourceScan.ConsumeOutsidePost);
        Assert.Empty(LinkKeySourceScan.Scan(wrapperFile, wrapper));
        Assert.Contains(LinkKeySourceScan.Scan(wrapperFile, otherCaller), f => f.Rule == LinkKeySourceScan.ConsumeOutsidePost);
        Assert.Contains(LinkKeySourceScan.Scan(wrapperFile, verifyInWrapper), f => f.Rule == LinkKeySourceScan.ConsumeOutsidePost);
        Assert.Contains(LinkKeySourceScan.Scan("src/Shopfolio.Web/Pages/Other.cs", wrapper), f => f.Rule == LinkKeySourceScan.ConsumeOutsidePost);
        Assert.Contains(LinkKeySourceScan.Scan(page, onGet), f => f.Rule == LinkKeySourceScan.ConsumeOutsidePost);
        Assert.Empty(LinkKeySourceScan.Scan(page, onPost));
    }

    [Theory]
    [InlineData("src/Shopfolio.Web/Modules/Membership/GoogleSignIns.cs", "ConfirmGoogleLinkAsync")]
    [InlineData("src/Shopfolio.Web/Modules/Membership/EmailChanges.cs", "VerifyNewEmailAsync")]
    public void Scan_OnTheGoogleAndEmailChangeCommands_AllowsTheConsumeInTheOneNamedMethodAndHoldsCallersToOnPost_T343(string file, string method)
    {
        string own = $"public async Task<R> {method}(string key, CancellationToken ct) {{ return await _links.TryConsumeAsync(id, ct); }}";
        const string other = "public async Task<R> ReadAsync(string key, CancellationToken ct) { return await _links.TryConsumeAsync(id, ct); }";
        string wrapper = $"public Task<R> {method}(string key, CancellationToken ct) => _inner.{method}(key, ct);";
        string otherCaller = $"public Task<R> ReadAsync(string key, CancellationToken ct) => _inner.{method}(key, ct);";
        string onGet = $"public async Task OnGetAsync(CancellationToken ct) {{ await _membership.{method}(key, ct); }}";
        string onPost = $"public async Task OnPostAsync(CancellationToken ct) {{ await _membership.{method}(key, ct); }}";
        const string page = "src/Shopfolio.Web/Pages/Account/Link.cshtml.cs";
        const string moduleFile = "src/Shopfolio.Web/Modules/Membership/MembershipModule.cs";

        Assert.Empty(LinkKeySourceScan.Scan(file, own));
        Assert.Contains(LinkKeySourceScan.Scan(file, other), f => f.Rule == LinkKeySourceScan.ConsumeOutsidePost);
        Assert.Contains(LinkKeySourceScan.Scan(page, onGet), f => f.Rule == LinkKeySourceScan.ConsumeOutsidePost);
        Assert.Empty(LinkKeySourceScan.Scan(page, onPost));
        Assert.Empty(LinkKeySourceScan.Scan(moduleFile, wrapper));
        Assert.Contains(LinkKeySourceScan.Scan(moduleFile, otherCaller), f => f.Rule == LinkKeySourceScan.ConsumeOutsidePost);
        Assert.Contains(LinkKeySourceScan.Scan("src/Shopfolio.Web/Modules/Membership/Other.cs", own), f => f.Rule == LinkKeySourceScan.ConsumeOutsidePost);
    }

    [Fact]
    public void ConsumingMethods_AndWrappers_NameOnlyExistingFiles_EachWithTheMethodsThatExist()
    {
        string root = RepositoryRoot.Find();
        foreach ((string path, string[] methods) in LinkKeySourceScan.ConsumingMethods)
        {
            string text = File.ReadAllText(Path.Combine(root, path));
            Assert.Contains("TryConsumeAsync(", text, StringComparison.Ordinal);
            foreach (string method in methods)
            {
                Assert.Contains(method + "(", text, StringComparison.Ordinal);
            }
        }

        foreach ((string path, string[] methods) in LinkKeySourceScan.WrapperMethods)
        {
            string text = File.ReadAllText(Path.Combine(root, path));
            foreach (string method in methods)
            {
                Assert.Contains("." + method + "(", text, StringComparison.Ordinal);
            }
        }
    }
}
