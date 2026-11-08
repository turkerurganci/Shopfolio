namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>Why an operation of the transition unit was refused (07 6.2.7; 09 9.1 item 4); the caller maps it to the error code of its own operation.</summary>
public enum TransitionRefusal
{
    /// <summary>No order with that id.</summary>
    OrderNotFound,

    /// <summary>The transition is not in the white list, or the order is not in its source state.</summary>
    NotInTheTable,

    /// <summary>The kind of actor may not trigger the transition, or the actor's data does not match its kind.</summary>
    ActorNotAllowed,

    /// <summary>A reason is given for a transition that takes none, a reason that the actor may not give, or two at once.</summary>
    ReasonNotAllowed,

    /// <summary>The administrator's screen showed an older version of the order.</summary>
    VersionConflict,

    /// <summary>The operation's own condition does not hold for the order as it is now.</summary>
    ConditionNotMet,

    /// <summary>The transitions together would leave the two axes in a forbidden pair (03 1.4.1).</summary>
    ForbiddenStatePair,

    /// <summary>The irreversible-action confirmation was not given (03 1.6.1).</summary>
    ConfirmationRequired,

    /// <summary>A value of the request is not valid (the carrier data, the delivery date, the reason or its note).</summary>
    InvalidInput,

    /// <summary>The correction can not be made (03 1.6.2; K-703); the result's <c>Obstacle</c> says why (07 6.2.7).</summary>
    CorrectionNotPossible,
}
