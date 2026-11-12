using Microsoft.AspNetCore.Authentication;

using Shopfolio.Infrastructure.Security;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The next step of the two landing screens of E-54 (04 9.25.1, 9.25.4, 9.25.5; 04 3.4.23): the screens do not depend on a session, only their one link does - "Hesabım" (E-50) when the panel's session
/// is open, the panel's sign-in (E-29) when it is not.
/// </summary>
internal static class LandingNextStep
{
    /// <summary>The address and the words of the link that follows the landing screen.</summary>
    public static async Task<(string Path, string Text)> ResolveAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        AuthenticateResult session = await context.AuthenticateAsync(SessionSchemes.AdminScheme);
        return session.Succeeded ? (PanelFramePaths.MyAccount, PanelAccessTexts.ToMyAccountLink) : (PanelFramePaths.SignIn, PanelAccessTexts.ToSignInLink);
    }
}
