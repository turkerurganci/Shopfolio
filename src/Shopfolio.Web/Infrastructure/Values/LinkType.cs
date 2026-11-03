namespace Shopfolio.Infrastructure.Values;

/// <summary>The closed values of LinkType (06 5.4.2). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum LinkType
{
    EmailVerification,
    PasswordReset,
    NewEmailVerification,
    EmailChangeRevert,
    AdminInvitation,
    GoogleLinkConfirmation,
}
