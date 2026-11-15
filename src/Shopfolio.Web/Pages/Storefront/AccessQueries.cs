using Shopfolio.Infrastructure.Binding;

namespace Shopfolio.Pages.Storefront;

// The query models of the storefront's access screens (07 5.3.2): every parameter is text, so that a wrong value is never a binding error (07 2.6.6); the handlers read the values and ignore what does not hold.

/// <summary>The query of UC-47 (07 5.3.2 <c>RegisterQuery</c>): the protected value of the pending registration the 303 after the submit (or the resend) carries (K-1186 (2)).</summary>
public sealed class RegisterQuery : IQueryModel
{
    [SpecAssignedName]
    public string? PendingRegistration { get; init; }
}

/// <summary>The query of UC-54 (07 5.3.2 <c>SignInQuery</c>): the address the visitor was sent from and, after a sign-in with an unverified account, the pending registration's protected value (07 4.22.1).</summary>
public sealed class SignInQuery : IQueryModel
{
    [SpecAssignedName]
    public string? ReturnUrl { get; init; }

    [SpecAssignedName]
    public string? PendingRegistration { get; init; }
}

/// <summary>The query of UC-63 (07 5.3.2 <c>AccountQuery</c>): the operation whose form opens and the single-use value of the re-authentication that holds its permission (07 4.24.2).</summary>
public sealed class AccountQuery : IQueryModel
{
    public string? ReauthenticatedOperation { get; init; }

    [SpecAssignedName]
    public string? ReauthenticationToken { get; init; }
}

/// <summary>The query of UC-51 (07 5.3.2 <c>GoogleReturnQuery</c>): what Google's redirect carries - the authorization code, the state the flow started with, and the error the user's refusal or Google's failure names.</summary>
public sealed class GoogleReturnQuery : IQueryModel
{
    public string? Code { get; init; }

    public string? State { get; init; }

    [SpecAssignedName]
    public string? Error { get; init; }
}
