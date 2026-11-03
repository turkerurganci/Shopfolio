namespace Shopfolio.Infrastructure.Values;

/// <summary>The closed values of ReauthenticationOperation (06 5.6.8). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum ReauthenticationOperation
{
    PasswordChange,
    EmailChange,
    AccountDeletion,
}
