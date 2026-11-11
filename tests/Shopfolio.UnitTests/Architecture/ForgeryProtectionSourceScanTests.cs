using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the form and site security scan over the application source and shows on deliberate violation samples that each rule goes red, for every
/// call form the rule names (T209; KK6, KK7, KK9; 07 2.5.4-2.5.6; 05 6.5; 09 15.2; 09 19.2 item 3). The scan reads files and never writes one.
/// </summary>
public sealed class ForgeryProtectionSourceScanTests
{
    private const string Code = "src/Shopfolio.Web/Infrastructure/Misuse.cs";
    private const string View = "src/Shopfolio.Web/Pages/Misuse.cshtml";

    [Fact]
    public void ApplicationSource_HasNoWayOutOfTheProtection_NoForeignHeaderWrite_NoHsts_AndItsViewsKeepTheMarkupOfThePolicy()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string pattern in new[] { "*.cs", "*.cshtml" })
        {
            foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), pattern, SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(root, file);
                string path = relative.Replace('\\', '/');
                if (path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal))
                {
                    continue;
                }

                scanned.Add(path);
                foreach ((string rule, int line) in ForgeryProtectionSourceScan.Scan(relative, File.ReadAllText(file)))
                {
                    findings.Add($"{path}:{line} {rule}");
                }
            }
        }

        Assert.Contains(scanned, f => f.EndsWith("Infrastructure/SignedEndpoints/SignedEndpointRegistration.cs", StringComparison.Ordinal));
        Assert.Contains(scanned, f => f.EndsWith("Infrastructure/Security/SecurityHeadersMiddleware.cs", StringComparison.Ordinal));
        Assert.Contains(scanned, f => f.EndsWith("Program.cs", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Theory]
    [InlineData("[IgnoreAntiforgeryToken]")]
    [InlineData("[Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryToken]")]
    [InlineData("[IgnoreAntiforgeryTokenAttribute]")]
    [InlineData("class A : IIgnoreAntiforgeryToken { }")]
    [InlineData("builder.Services.AddSingleton<IgnoreAntiforgeryTokenAttribute>();")]
    [InlineData("[RequireAntiforgeryToken(false)]")]
    [InlineData("[RequireAntiforgeryToken(required: false)]")]
    [InlineData("[ Microsoft.AspNetCore.Mvc.RequireAntiforgeryTokenAttribute ( false ) ]")]
    public void Scan_OfAnyNameThatSwitchesTheProtectionOff_IsAFinding(string line)
    {
        Assert.Equal([(ForgeryProtectionSourceScan.IgnoresProtection, 1)], ForgeryProtectionSourceScan.Scan(Code, line));
    }

    [Theory]
    [InlineData("[RequireAntiforgeryToken]")]
    [InlineData("[RequireAntiforgeryToken(true)]")]
    [InlineData("// [IgnoreAntiforgeryToken] is what the sample page of the old task carried")]
    public void Scan_OfTheProtectionSwitchedOnOrOnlyTalkedAbout_IsQuiet(string line)
    {
        Assert.Empty(ForgeryProtectionSourceScan.Scan(Code, line));
    }

    [Theory]
    [InlineData("endpoints.MapGet(\"/x\", h).DisableAntiforgery();")]
    [InlineData("builder\n    .DisableAntiforgery();")]
    [InlineData("RouteHandlerBuilderExtensions.DisableAntiforgery(builder);")]
    public void Scan_OfDisableAntiforgeryOutsideTheRegistrationFile_IsAFinding_AndInsideItIsQuiet(string line)
    {
        Assert.Contains((ForgeryProtectionSourceScan.DisableOutsideRegistration, line.Contains('\n', StringComparison.Ordinal) ? 2 : 1), ForgeryProtectionSourceScan.Scan(Code, line));
        Assert.DoesNotContain(
            ForgeryProtectionSourceScan.Scan("src/Shopfolio.Web/Infrastructure/SignedEndpoints/SignedEndpointRegistration.cs", line),
            f => f.Rule == ForgeryProtectionSourceScan.DisableOutsideRegistration);
    }

    [Theory]
    [InlineData("app.MapSignedEndpoint<A, B>(\"/sistem/odeme-bildirimi\");")]
    [InlineData("app.MapSignedEndpoint<A, B>(path: \"/x\");")]
    [InlineData("SignedEndpointRegistration.MapSignedEndpoint<A, B>(app, \"/x\");")]
    [InlineData("app.MapSignedEndpoint<A, Notice<B>>(somePath);")]
    [InlineData("app.MapSignedEndpoint<A, B>(\n    \"/sistem/\" + AntiforgeryExemptions.PaymentNotificationPath);")]
    [InlineData("app.MapSignedEndpoint<A, B>(string.Concat(AntiforgeryExemptions.PaymentNotificationPath, \"/x\"));")]
    public void Scan_OfASignedEndpointWhosePathIsNotFromTheList_IsAFinding(string line)
    {
        Assert.Contains(ForgeryProtectionSourceScan.Scan(Code, line), f => f.Rule == ForgeryProtectionSourceScan.SignedEndpointPathNotListed);
    }

    [Theory]
    [InlineData("app.MapSignedEndpoint<A, B>(AntiforgeryExemptions.PaymentNotificationPath);")]
    [InlineData("app.MapSignedEndpoint<A, B>(path: Security.AntiforgeryExemptions.PaymentNotificationPath);")]
    [InlineData("SignedEndpointRegistration.MapSignedEndpoint<A, Notice<B>>(app, AntiforgeryExemptions.Paths[0]);")]
    public void Scan_OfASignedEndpointWhosePathIsFromTheList_IsQuiet(string line)
    {
        Assert.Empty(ForgeryProtectionSourceScan.Scan(Code, line));
    }

    [Theory]
    [InlineData("app.MapPost(\"/x\", h);")]
    [InlineData("endpoints.MapPut(\"/x\", h);")]
    [InlineData("endpoints.MapDelete(\"/x\", h);")]
    [InlineData("endpoints.MapPatch(\"/x\", h);")]
    [InlineData("endpoints.MapMethods(\"/x\", [\"POST\"], h);")]
    [InlineData("group.MapPost<A>(\"/x\", h);")]
    [InlineData("EndpointRouteBuilderExtensions.MapPost(endpoints, \"/x\", h);")]
    public void Scan_OfAnyOtherWritingEndpoint_IsAFinding_AndInsideTheRegistrationFileIsQuiet(string line)
    {
        Assert.Equal([(ForgeryProtectionSourceScan.OtherWritingEndpoint, 1)], ForgeryProtectionSourceScan.Scan(Code, line));
        Assert.Empty(ForgeryProtectionSourceScan.Scan("src/Shopfolio.Web/Infrastructure/SignedEndpoints/SignedEndpointRegistration.cs", line));
    }

    // D-131: the two POSTs of the chunked upload, in the one file, with the path from the header-token list, the panel's session and the header check.
    [Theory]
    [InlineData("endpoints.MapPost(AntiforgeryExemptions.UploadStartPath, StartAsync).WithMetadata(M).RequireAdminSession().RequireHeaderForgeryToken();")]
    [InlineData("endpoints.MapPost(AntiforgeryExemptions.UploadChunkPath, ChunkAsync)\n    .WithMetadata(M, new RequestBodyClassAttribute(C.Chunk))\n    .RequireAdminSession()\n    .RequireHeaderForgeryToken();")]
    public void Scan_OfTheUploadPostsInTheUploadFileWithTheHeaderCheck_IsQuiet_AndTheSameShapeInAnyOtherFileIsAFinding(string statement)
    {
        Assert.Empty(ForgeryProtectionSourceScan.Scan(ForgeryProtectionSourceScan.UploadEndpointsFile, statement));
        Assert.Contains(ForgeryProtectionSourceScan.Scan(Code, statement), f => f.Rule == ForgeryProtectionSourceScan.OtherWritingEndpoint);
        Assert.Contains(
            ForgeryProtectionSourceScan.Scan("src/Shopfolio.Web/Endpoints/Files/DownloadEndpoint.cs", statement),
            f => f.Rule == ForgeryProtectionSourceScan.OtherWritingEndpoint);
        Assert.Contains(
            ForgeryProtectionSourceScan.Scan("other/src/Shopfolio.Web/Endpoints/Files/UploadEndpoints.cs", statement),
            f => f.Rule == ForgeryProtectionSourceScan.OtherWritingEndpoint);
    }

    // In the upload file itself the relaxation is narrow: a literal path, a path of another list, a missing session or header check, or another writing method is a finding.
    [Theory]
    [InlineData("endpoints.MapPost(\"/panel/yukleme\", StartAsync).RequireAdminSession().RequireHeaderForgeryToken();")]
    [InlineData("endpoints.MapPost(AntiforgeryExemptions.PaymentNotificationPath, StartAsync).RequireAdminSession().RequireHeaderForgeryToken();")]
    [InlineData("endpoints.MapPost(AntiforgeryExemptions.UploadStartPath + \"/x\", StartAsync).RequireAdminSession().RequireHeaderForgeryToken();")]
    [InlineData("endpoints.MapPost(AntiforgeryExemptions.UploadStartPath, StartAsync).RequireAdminSession();")]
    [InlineData("endpoints.MapPost(AntiforgeryExemptions.UploadStartPath, StartAsync).RequireHeaderForgeryToken();")]
    [InlineData("endpoints.MapPost(AntiforgeryExemptions.UploadStartPath, StartAsync);")]
    [InlineData("endpoints.MapPut(AntiforgeryExemptions.UploadStartPath, StartAsync).RequireAdminSession().RequireHeaderForgeryToken();")]
    [InlineData("endpoints.MapMethods(AntiforgeryExemptions.UploadStartPath, [\"POST\"], StartAsync).RequireAdminSession().RequireHeaderForgeryToken();")]
    [InlineData("endpoints.MapDelete(AntiforgeryExemptions.UploadChunkPath, StartAsync).RequireAdminSession().RequireHeaderForgeryToken();")]
    public void Scan_OfAnUploadFilePostThatLacksPartOfTheShape_IsAFinding(string statement)
    {
        Assert.Contains(ForgeryProtectionSourceScan.Scan(ForgeryProtectionSourceScan.UploadEndpointsFile, statement), f => f.Rule == ForgeryProtectionSourceScan.OtherWritingEndpoint);
    }

    [Theory]
    [InlineData("endpoints.MapGet(\"/x\", h);")]
    [InlineData("var map = MapPostcode(x);")]
    public void Scan_OfAReadingEndpointOrAnUnrelatedName_IsQuiet(string line)
    {
        Assert.Empty(ForgeryProtectionSourceScan.Scan(Code, line));
    }

    [Theory]
    [InlineData("<form method=\"post\" action=\"/panel/x?handler=Save\">")]
    [InlineData("<form method=post action='/panel/x?handler=Save'>")]
    [InlineData("<form\n  method=\"POST\"\n  action=\"/panel/x\"\n  class=\"a\">")]
    [InlineData("<form method=\"post\" action=\"/panel/x\" asp-antiforgery=\"@flag\">")]
    public void Scan_OfAPostFormWithItsOwnActionAndNoTokenSwitch_IsAFinding(string tag)
    {
        Assert.Contains(ForgeryProtectionSourceScan.Scan(View, tag + "</form>"), f => f.Rule == ForgeryProtectionSourceScan.PostFormWithoutToken);
    }

    [Theory]
    [InlineData("<form method=\"post\" asp-page-handler=\"Add\" asp-antiforgery=\"false\">")]
    [InlineData("<form method=\"post\" asp-page-handler=\"Add\" asp-antiforgery=false>")]
    [InlineData("<form method=\"post\" action=\"/x\" asp-antiforgery='False'>")]
    public void Scan_OfAFormThatSwitchesItsTokenOff_IsAFinding(string tag)
    {
        Assert.Contains(ForgeryProtectionSourceScan.Scan(View, tag + "</form>"), f => f.Rule == ForgeryProtectionSourceScan.TokenSwitchedOff);
    }

    [Theory]
    [InlineData("<form method=\"post\" action=\"https://evil.example/x\" asp-antiforgery=\"true\">")]
    [InlineData("<form method=\"post\" action=\"//evil.example/x\" asp-antiforgery=\"true\">")]
    [InlineData("<form method=post action=http://evil.example asp-antiforgery=true>")]
    public void Scan_OfAPostFormToAnotherSite_IsAFinding(string tag)
    {
        Assert.Contains(ForgeryProtectionSourceScan.Scan(View, tag + "</form>"), f => f.Rule == ForgeryProtectionSourceScan.PostFormToAnotherSite);
    }

    [Theory]
    [InlineData("<form method=\"post\" asp-page-handler=\"Add\">")]
    [InlineData("<form method=\"post\" action=\"/panel/x?handler=Save\" asp-antiforgery=\"true\">")]
    [InlineData("<form method=\"get\" action=\"/arama\">")]
    [InlineData("<form action=\"/arama\">")]
    [InlineData("<form method=\"post\" asp-page=\"/Panel/X\" asp-page-handler=\"Add\" class=\"a\">")]
    public void Scan_OfAFormThatCarriesItsToken_OrIsNotAPost_IsQuiet(string tag)
    {
        Assert.Empty(ForgeryProtectionSourceScan.Scan(View, tag + "</form>"));
    }

    [Theory]
    [InlineData("headers[\"Content-Security-Policy\"] = x;")]
    [InlineData("headers.ContentSecurityPolicy = x;")]
    [InlineData("response.Headers.Append(HeaderNames.ContentSecurityPolicy, x);")]
    [InlineData("headers[\"referrer-policy\"] = \"no-referrer\";")]
    [InlineData("headers.Append(\"Permissions-Policy\", x);")]
    [InlineData("headers[HeaderNames.ReferrerPolicy] = x;")]
    [InlineData("headers.PermissionsPolicy = x;")]
    public void Scan_OfASecurityHeaderWrittenOutsideItsOwnFiles_IsAFinding_AndInsideThemIsQuiet(string line)
    {
        Assert.Equal([(ForgeryProtectionSourceScan.HeaderWrittenElsewhere, 1)], ForgeryProtectionSourceScan.Scan(Code, line));
        Assert.Empty(ForgeryProtectionSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Security/SecurityHeadersMiddleware.cs", line));
    }

    [Theory]
    [InlineData("\"script-src 'self' 'unsafe-inline'\"")]
    [InlineData("\"script-src 'self' 'UNSAFE-EVAL'\"")]
    [InlineData("headers[\"Content-Security-Policy-Report-Only\"] = x;")]
    [InlineData("headers.ContentSecurityPolicyReportOnly = x;")]
    public void Scan_OfAWeakenedPolicy_IsAFinding_EvenInTheHeaderFiles(string line)
    {
        Assert.Contains(
            ForgeryProtectionSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Security/ContentSecurityPolicy.cs", line),
            f => f.Rule == ForgeryProtectionSourceScan.UnsafePolicy);
    }

    [Theory]
    [InlineData("app.UseHsts();")]
    [InlineData("app.UseHsts(  );")]
    [InlineData("builder.Services.AddHsts(o => o.MaxAge = TimeSpan.FromDays(365));")]
    [InlineData("headers[\"Strict-Transport-Security\"] = \"max-age=31536000\";")]
    [InlineData("headers.StrictTransportSecurity = x;")]
    [InlineData("headers.Append(HeaderNames.StrictTransportSecurity, x);")]
    [InlineData("services.Configure<HstsOptions>(o => { });")]
    public void Scan_OfHstsInTheApplication_IsAFinding(string line)
    {
        Assert.Equal([(ForgeryProtectionSourceScan.HstsInApplication, 1)], ForgeryProtectionSourceScan.Scan(Code, line));
    }

    [Theory]
    [InlineData("<div style=\"color: red\">")]
    [InlineData("<div STYLE='color:red'>")]
    [InlineData("<div\n  class=\"a\"\n  style=\"x\">", 1)]
    [InlineData("<p style = \"x\">")]
    public void Scan_OfAStyleAttribute_IsAFinding(string tag, int line = 1)
    {
        Assert.Contains((ForgeryProtectionSourceScan.StyleAttribute, line), ForgeryProtectionSourceScan.Scan(View, tag));
    }

    [Theory]
    [InlineData("<button onclick=\"go()\">")]
    [InlineData("<body ONLOAD='x'>")]
    [InlineData("<img src=\"a.png\" onerror=\"x()\">")]
    [InlineData("<a href=\"javascript:alert(1)\">")]
    [InlineData("<form action=\"javascript:x()\" method=\"get\">")]
    public void Scan_OfAnInlineEventAttributeOrAJavascriptAddress_IsAFinding(string tag)
    {
        Assert.Contains(ForgeryProtectionSourceScan.Scan(View, tag), f => f.Rule == ForgeryProtectionSourceScan.InlineEventAttribute);
    }

    [Theory]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("<script type=\"text/javascript\">x</script>")]
    [InlineData("<SCRIPT\n>x</SCRIPT>")]
    public void Scan_OfAScriptWithNeitherSourceNorStructuredData_IsAFinding(string tag)
    {
        Assert.Contains(ForgeryProtectionSourceScan.Scan(View, tag), f => f.Rule == ForgeryProtectionSourceScan.ScriptWithoutSource);
    }

    [Theory]
    [InlineData("<style>a { color: red }</style>")]
    [InlineData("<style nonce=\"@n\">a { color: red }</style>")]
    public void Scan_OfAStyleBlockOutsideTheLayout_IsAFinding(string tag)
    {
        Assert.Contains(ForgeryProtectionSourceScan.Scan(View, tag), f => f.Rule == ForgeryProtectionSourceScan.StyleBlockOutsideLayout);
    }

    [Fact]
    public void Scan_OfAStyleBlockInTheLayoutWithoutTheNonce_IsAFinding()
    {
        Assert.Contains(
            ForgeryProtectionSourceScan.Scan("src/Shopfolio.Web/Pages/Shared/_Layout.cshtml", "<style>:root { --brand: red }</style>"),
            f => f.Rule == ForgeryProtectionSourceScan.StyleBlockOutsideLayout);
    }

    [Theory]
    [InlineData("<script src=\"/statik/js/site.js\"></script>")]
    [InlineData("<script type=\"application/ld+json\">{}</script>")]
    [InlineData("<script\n  src='/statik/js/x.js'></script>")]
    [InlineData("<link rel=\"stylesheet\" href=\"/statik/css/site.css\">")]
    [InlineData("<button class=\"primary\" data-confirm=\"x\">")]
    [InlineData("<input name=\"Version\" value=\"3\" type=\"hidden\">")]
    [InlineData("<a href=\"/hesap\" data-style=\"x\" asp-route-on=\"y\">")]
    [InlineData("@* <button onclick=\"x\"> <div style=\"a\"> *@")]
    [InlineData("<!-- <script>x</script> -->")]
    public void Scan_OfTheMarkupThePolicyAllows_IsQuiet(string markup)
    {
        Assert.Empty(ForgeryProtectionSourceScan.Scan(View, markup));
    }

    [Fact]
    public void Scan_OfTheNonceBlockOfTheStorefrontFrameLayout_IsQuiet_AndOfAnyOtherLayoutIsAFinding()
    {
        const string block = "<style nonce=\"@nonce\">@FrameStyle.BrandVariables(frame.Brand)</style>";

        Assert.Empty(ForgeryProtectionSourceScan.Scan("src/Shopfolio.Web/Pages/Shared/_StorefrontLayout.cshtml", block));
        Assert.Contains(ForgeryProtectionSourceScan.Scan("src/Shopfolio.Web/Pages/Shared/_PanelLayout.cshtml", block), f => f.Rule == ForgeryProtectionSourceScan.StyleBlockOutsideLayout);
        Assert.Contains(ForgeryProtectionSourceScan.Scan("src/Shopfolio.Web/Pages/Shared/_StorefrontLayout.cshtml", "<style>:root{}</style>"), f => f.Rule == ForgeryProtectionSourceScan.StyleBlockOutsideLayout);
    }

    [Fact]
    public void Scan_OfTheNonceBlockOfTheLayout_IsQuiet()
    {
        Assert.Empty(ForgeryProtectionSourceScan.Scan("src/Shopfolio.Web/Pages/Shared/_Layout.cshtml", "<style nonce=\"@SecurityHeaders.NonceOf(Context)\">:root { --brand: red }</style>"));
    }

    [Fact]
    public void Scan_OfAViewRule_DoesNotApplyToCodeFiles()
    {
        Assert.Empty(ForgeryProtectionSourceScan.Scan(Code, "string a = \"<div style='x' onclick='y'><script>z</script>\";"));
    }
}
