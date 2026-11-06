using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The pure parts of the form and site security (T209; 07 2.5; 05 6.5; 09 15.2): which policy is in force on a page, the content security policy's
/// text, the closed list of the unprotected endpoints, the nonce, the confirmation key's addresses and the reauthentication field. The behaviour on
/// the running application is in <c>ForgeryProtectionTests</c>, <c>SecurityHeadersTests</c> and <c>ReauthenticationValuesTests</c>.
/// </summary>
public sealed class FormSecurityTests
{
    // ---- the policy in force (09 15.2 item 1, 2) ----

    [Fact]
    public void Protection_IsInForceOnAPageWithoutAnyMark()
    {
        Assert.True(ForgeryProtectionFilter.IsInForce([new ForgeryProtectionPolicy(), new AutoValidateAntiforgeryTokenAttribute()]));
        Assert.True(ForgeryProtectionFilter.IsInForce([new AutoValidateAntiforgeryTokenAttribute(), new ForgeryProtectionPolicy()]));
    }

    [Fact]
    public void Protection_IsOffOnAPageThatCarriesTheIgnoreAttribute_TheOneExceptionTheSourceScanKeepsOutOfTheApplication()
    {
        Assert.False(ForgeryProtectionFilter.IsInForce([new ForgeryProtectionPolicy(), new AutoValidateAntiforgeryTokenAttribute(), new IgnoreAntiforgeryTokenAttribute()]));
    }

    [Fact]
    public void Protection_IsOffWhenTheMarkIsMissing_SoTheRegistrationIsWhatSwitchesItOn()
    {
        Assert.False(ForgeryProtectionFilter.IsInForce([new AutoValidateAntiforgeryTokenAttribute()]));
        Assert.False(ForgeryProtectionFilter.IsInForce([]));
    }

    [Fact]
    public void TheFilter_RunsBeforeTheFieldValidation_AndTheMarkAboveTheFrameworksCheckButBelowTheIgnoreAttribute()
    {
        Assert.True(ForgeryProtectionFilter.FilterOrder < 0);
        Assert.True(ForgeryProtectionPolicy.PolicyOrder > new AutoValidateAntiforgeryTokenAttribute().Order);
        Assert.True(ForgeryProtectionPolicy.PolicyOrder < new IgnoreAntiforgeryTokenAttribute().Order);
    }

    [Fact]
    public void ForgeryRefused_ShowsThePageAgainWithStatus400()
    {
        Assert.Equal(400, FormResults.ForgeryRefusedStatus);
        PageResult result = Assert.IsType<PageResult>(FormResults.ForgeryRefused(new SamplePage()));
        Assert.Equal(400, result.StatusCode);
    }

    // ---- the closed list (07 2.5.5; 09 15.2 item 2) ----

    [Fact]
    public void TheClosedList_HoldsOnlyTheSignedPaymentNotification_AndTheReturnIsNotOnIt()
    {
        Assert.Equal(["/sistem/odeme-bildirimi"], AntiforgeryExemptions.Paths);
        Assert.True(AntiforgeryExemptions.Contains("/sistem/odeme-bildirimi"));
        Assert.True(AntiforgeryExemptions.Contains("/SISTEM/odeme-bildirimi"));
        Assert.False(AntiforgeryExemptions.Contains("/odeme/donus/{accessKey}"));
        Assert.False(AntiforgeryExemptions.Contains("/sistem/canlilik"));
        Assert.False(AntiforgeryExemptions.Contains(null));
    }

    // ---- the policy and the headers (09 15.2 items 3, 4; 05 6.5.4) ----

    [Fact]
    public void Policy_HoldsEveryDirectiveOfTheRule_AndNothingThatWeakensIt()
    {
        string policy = ContentSecurityPolicy.Build("AAAA");

        string[] directives = policy.Split("; ");
        Assert.Equal(
            [
                "default-src 'self'",
                "script-src 'self'",
                "style-src 'self' 'nonce-AAAA'",
                "img-src 'self'",
                "font-src 'self'",
                "connect-src 'self'",
                "object-src 'none'",
                "base-uri 'self'",
                "frame-src 'none'",
                "frame-ancestors 'none'",
                "form-action 'self' https://accounts.google.com",
            ],
            directives);
        Assert.DoesNotContain("unsafe-inline", policy, StringComparison.Ordinal);
        Assert.DoesNotContain("unsafe-eval", policy, StringComparison.Ordinal);
        Assert.DoesNotContain("'nonce-AAAA'", string.Join("; ", directives.Where(d => d.StartsWith("script-src", StringComparison.Ordinal))), StringComparison.Ordinal);
        Assert.DoesNotContain("paytr", policy, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Nonce_IsSixteenRandomBytesInBase64_AndDiffersEachTime()
    {
        string first = SecurityHeaders.NewNonce();
        string second = SecurityHeaders.NewNonce();

        Assert.NotEqual(first, second);
        Assert.Equal(16, Convert.FromBase64String(first).Length);
        Assert.Equal(24, first.Length);
    }

    [Fact]
    public void NonceOf_WithoutTheMiddleware_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => SecurityHeaders.NonceOf(new Microsoft.AspNetCore.Http.DefaultHttpContext()));
    }

    [Fact]
    public void HeaderValues_AreTheOnesOfTheRule()
    {
        Assert.Equal("nosniff", SecurityHeaders.NoSniff);
        Assert.Equal("strict-origin-when-cross-origin", SecurityHeaders.ReferrerPolicyDefault);
        Assert.Equal("no-referrer", SecurityHeaders.ReferrerPolicyKeyedPage);
        Assert.Equal("camera=(), microphone=(), geolocation=()", SecurityHeaders.PermissionsPolicy);
        Assert.Equal("no-store", SecurityHeaders.NoStore);
    }

    // ---- the one-time values of the forms (07 2.5.1, 2.5.2) ----

    [Fact]
    public void ConfirmationKey_IsFreshEachTime_AndTheFieldAndThePathsAreTheSpecs()
    {
        string first = ConfirmationKeys.NewKey();
        string second = ConfirmationKeys.NewKey();

        Assert.NotEqual(first, second);
        Assert.Equal("ConfirmationKey", ConfirmationKeys.FieldName);
        Assert.Equal($"<input type=\"hidden\" name=\"ConfirmationKey\" value=\"{first}\" />", ConfirmationKeys.Render(ConfirmationKeys.Hidden(first)));
        Assert.Equal("/siparis/2026-ABC123/teyit", ConfirmationKeys.MemberConfirmationPath("2026-ABC123"));
        Assert.Equal("/siparis/erisim/Ab-_9/teyit", ConfirmationKeys.AccessConfirmationPath("Ab-_9"));
        Assert.True(FormResults.IsLocalPath(ConfirmationKeys.MemberConfirmationPath("2026-ABC123")));
        Assert.True(FormResults.IsLocalPath(ConfirmationKeys.AccessConfirmationPath("Ab-_9")));
    }

    [Fact]
    public void ReauthenticationField_IsAHiddenFieldOfThatName_WithOrWithoutTheModelPath()
    {
        Assert.Equal("ReauthenticationToken", ReauthenticationValues.FieldName);
        Assert.Equal(
            "<input type=\"hidden\" name=\"ReauthenticationToken\" value=\"v1\" />",
            ReauthenticationValues.Render(ReauthenticationValues.Hidden("v1")));
        Assert.Equal(
            "<input type=\"hidden\" name=\"Input.ReauthenticationToken\" value=\"v&gt;1\" />",
            ReauthenticationValues.Render(ReauthenticationValues.Hidden("v>1", "Input")));
        Assert.Equal(3, Enum.GetValues<ReauthenticationOperation>().Length);
    }

    private sealed class SamplePage : PageModel
    {
    }
}
