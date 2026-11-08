namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The administrator's correction of a wrong status transition (03 1.6.2; 02 10.4.6; 05 4.1.1, 4.1.4): a new transition that sets the order back to where the
/// corrected transition left it from. <paramref name="TransitionRecordId"/> is the wrong transition's record; <paramref name="Reason"/> is from the closed list
/// (06 5.3.9) and <paramref name="ReasonNote"/> is the free note of "Other" - no other reason takes one. <paramref name="ExpectedVersion"/> is the version the
/// screen showed (05 7.3.2). A correction asks no irreversible-action confirmation (04 9.9.31): it is the path that corrects, not a result that goes outside.
/// </summary>
public sealed record CorrectTransitionRequest(
    Guid OrderId,
    Guid TransitionRecordId,
    TransitionAdmin Admin,
    int ExpectedVersion,
    CorrectionReason? Reason,
    string? ReasonNote);
