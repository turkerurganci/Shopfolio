namespace Shopfolio.Infrastructure;

/// <summary>The two kinds of a call that leaves the application, by who waits for it (05 10.1.1; 08 3.1 rule 2).</summary>
public enum ExternalCallKind
{
    /// <summary>A user waits for the answer: the start of a payment, the end of a Google sign-in.</summary>
    UserWaiting,

    /// <summary>The call leaves the job queue: e-mail, the payment query, the card refund and its query, the bounce query.</summary>
    Job,
}
