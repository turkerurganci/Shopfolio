namespace Shopfolio.Modules.Notifications.Contracts;

/// <summary>The closed values of DispatchOutcome (06 5.4.7). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum DispatchOutcome
{
    Sent,
    RecipientError,
    ChannelError,
}
