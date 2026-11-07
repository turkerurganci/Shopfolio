using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// Who asks for a transition (06 5.4.10): a customer, an administrator (the firm, from the panel) or the system. An administrator carries the id and
/// the name the transition record keeps; the other two carry neither (05 4.1.2).
/// </summary>
internal sealed record TransitionActor(ActorKind Kind, Guid? AdminId, string? AdminName)
{
    public static TransitionActor Buyer { get; } = new(ActorKind.Buyer, null, null);

    public static TransitionActor System { get; } = new(ActorKind.System, null, null);

    public static TransitionActor Admin(Guid adminId, string adminName) => new(ActorKind.Admin, adminId, adminName);

    /// <summary>True when the identity fields match the kind: an administrator has both, a customer and the system neither.</summary>
    public bool IsConsistent => Kind == ActorKind.Admin ? AdminId is not null && AdminName is not null : AdminId is null && AdminName is null;
}

/// <summary>The reason a cancelling transition carries (06 4.19): at most one of the two, and the free note of "Other"; empty everywhere else.</summary>
internal sealed record TransitionReasons(TransitionReason? Automatic, CancellationReason? Firm, string? Note)
{
    public bool IsEmpty => Automatic is null && Firm is null && Note is null;

    /// <summary>The kind of automatic cancellation, or the closure.</summary>
    public static TransitionReasons OfKind(TransitionReason reason) => new(reason, null, null);

    public static TransitionReasons OfFirm(CancellationReason reason, string? note = null) => new(null, reason, note);
}

/// <summary>
/// A request for one or more transitions of one order, by their identity (05 4.1.1: the panel, the storefront and the background job ask for the
/// transition by name). More than one code is for the transitions that happen together with one actor - the unpaid cancellation S3 with O2, the card
/// payment's approval O1 with S1 by the system (02 5.6) - applied in the order given, in one lock, one version raise and one set of transition records.
/// A request carries one actor: the bank transfer's approval is two requests under the same lock - O1 by the administrator, S1 by the system (03 1.2 S1;
/// D-291) - and the version is still raised once. <paramref name="ExpectedVersion"/>
/// is the version the administrator's screen showed; the customer's and the system's operations leave it empty and compare their condition with
/// the order as it is now (09 10.2 item 3).
/// </summary>
internal sealed record TransitionRequest(
    Guid OrderId,
    IReadOnlyList<TransitionCode> Codes,
    TransitionActor Actor,
    TransitionReasons? Reasons = null,
    int? ExpectedVersion = null)
{
    public TransitionRequest(Guid orderId, TransitionCode code, TransitionActor actor, TransitionReasons? reasons = null, int? expectedVersion = null)
        : this(orderId, [code], actor, reasons, expectedVersion)
    {
    }
}

/// <summary>
/// The administrator's correction (05 4.1.1; 02 5.4): a new transition that sets the order back to where the corrected transition left it from,
/// with a reason from the closed list. It is asked only from the panel, so the actor is an administrator and the screen's version is required.
/// </summary>
internal sealed record CorrectionRequest(
    Guid OrderId,
    Guid CorrectedTransitionRecordId,
    CorrectionReason? Reason,
    string? Note,
    TransitionActor Actor,
    int ExpectedVersion);

/// <summary>Why a transition was not applied; the caller maps it to the error code of its own operation (07 6.2.7; 09 9.1 item 4).</summary>
internal enum TransitionRejection
{
    /// <summary>No order with that id.</summary>
    OrderNotFound,

    /// <summary>The code is not in the table, or the order is not in the row's source state; the correction is not in the table.</summary>
    NotInTheTable,

    /// <summary>The kind of actor may not trigger the transition (03 1.2, 1.3), or the actor's identity does not match its kind.</summary>
    ActorNotAllowed,

    /// <summary>A reason is given for a transition that takes none, or two reasons at once.</summary>
    ReasonNotAllowed,

    /// <summary>The administrator's screen showed an older version of the order (05 7.3.2).</summary>
    VersionConflict,

    /// <summary>The transition's own condition (the operation's, not the table's) does not hold for the order as it is now (05 4.1.2).</summary>
    ConditionNotMet,

    /// <summary>The transitions together would leave the two axes in a forbidden pair (03 1.4.1).</summary>
    ForbiddenStatePair,

    /// <summary>The correction is asked by something other than an administrator from the panel.</summary>
    CorrectionRequiresPanel,

    /// <summary>The correction has no reason from the closed list.</summary>
    CorrectionReasonRequired,

    /// <summary>The corrected record is not this order's, is a correction itself, was corrected already, or its result is no longer the state.</summary>
    CorrectionNotPossible,
}

/// <summary>The result of a transition or correction (09 9.1 item 3): a refusal is a value, not an exception; the runner rolls back on it.</summary>
internal sealed record TransitionOutcome(
    TransitionRejection? Rejection,
    IReadOnlyList<Guid> TransitionRecordIds,
    ClosureResult Closure) : IOperationResult
{
    public bool IsRejected => Rejection is not null;

    public static TransitionOutcome Rejected(TransitionRejection rejection) => new(rejection, [], ClosureResult.None);

    public static TransitionOutcome Applied(IReadOnlyList<Guid> recordIds, ClosureResult closure) => new(null, recordIds, closure);
}

/// <summary>
/// What the closure rule did at the end of an operation (05 4.1.3; 03 1.4.3): the closing transition and its record, or nothing. An item record
/// that closed the order's last open units links the record by <see cref="TransitionRecordId"/> (06 4.22).
/// </summary>
internal sealed record ClosureResult(TransitionCode? Code, Guid? TransitionRecordId)
{
    public static ClosureResult None { get; } = new(null, null);

    public bool Closed => Code is not null;
}
