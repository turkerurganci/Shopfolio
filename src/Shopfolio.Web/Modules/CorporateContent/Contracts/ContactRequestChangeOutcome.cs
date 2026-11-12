namespace Shopfolio.Modules.CorporateContent.Contracts;

// What became of a close or a reopening of a contact request (T532; 07 UC-155, UC-156): Changed - the status changed; Unchanged - the request was already in the asked status (another administrator acted
// first), nothing was written; NotFound - never there, or already destroyed at the end of its retention (K-1192).
public enum ContactRequestChangeOutcome
{
    Changed,
    Unchanged,
    NotFound,
}
