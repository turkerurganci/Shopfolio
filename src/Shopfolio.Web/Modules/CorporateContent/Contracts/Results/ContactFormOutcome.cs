namespace Shopfolio.Modules.CorporateContent.Contracts.Results;

/// <summary>What became of a send of the contact form (T365; 02 3.32; 07 UC-15). Not stored.</summary>
public enum ContactFormOutcome
{
    /// <summary>A request was written open and the firm's notice F-2 was queued with it.</summary>
    Sent,

    /// <summary>The trap field was filled: nothing was written and nothing was counted, and the visitor is shown the state of <see cref="Sent"/> (02 3.32.7).</summary>
    DroppedSilently,

    /// <summary>The gate of the data-collecting entries is closed: the form is closed and nothing was read (02 3.32.8).</summary>
    GateClosed,

    /// <summary>A required field is empty or a value is not of its form; every refused field is listed and nothing was written or counted.</summary>
    FieldsRefused,

    /// <summary>The limit L-4 is exceeded for the address (02 8.2): nothing was written; the message is neutral.</summary>
    LimitExceeded,
}
