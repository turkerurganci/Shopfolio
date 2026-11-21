namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// A page that reads its content for the page shown again when the forgery check refuses a POST (07 2.5.4; 04 2.7.3.1; K-1627 (2); D-1773). The check answers 400 before the handler, so the page's
/// reads - the ones its GET makes - have not run, and the page shown again would draw its content empty. <see cref="ForgeryProtectionFilter"/> calls <see cref="ReadAfterForgeryRefusalAsync"/> once, after
/// it has cleared the rule codes and put the action-level message, and before the page is drawn: the one place that asks a page for its read.
/// The implementation only reads: the refusal writes nothing, and what a forged request could make a page do is a read of what the signed-in user already sees on the page's GET. Where the GET
/// makes a write (drops a permission), consumes a single-use value or decides by a query value, the implementation is the page's own safe split, not the GET's call (D-1843): it reads from the route's
/// key and the sent values - the read the page's 422 already makes -, reads a link's record without using it, and checks a single-use value without consuming it. The reauthentication
/// permission keeps the rule every page request follows (05 6.1.5; <see cref="ReauthenticationPermissionFilter"/>): the granting screen's refused POST leaves it, the account page shown without
/// the permission's form drops it, as its GET and its 422 do.
/// The inventory test (<c>ForgeryRefusalReadInventoryTests</c>) holds every page with a POST handler to this interface, to one of the named exemptions, or to the closed list of pages still waiting for
/// their own safe read.
/// </summary>
internal interface IReadsOnForgeryRefusal
{
    /// <summary>Reads what the page's GET reads for the page shown again: from the route and the sent values, with no query value, writing nothing.</summary>
    Task ReadAfterForgeryRefusalAsync(CancellationToken ct);
}
