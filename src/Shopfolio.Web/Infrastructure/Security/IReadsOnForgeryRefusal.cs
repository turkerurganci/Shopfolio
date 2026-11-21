namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// A page that reads its content for the page shown again when the forgery check refuses a POST (07 2.5.4; 04 2.7.3.1; K-1627 (2); D-1773). The check answers 400 before the handler, so the page's
/// reads - the ones its GET makes - have not run, and the page shown again would draw its content empty. <see cref="ForgeryProtectionFilter"/> calls <see cref="ReadAfterForgeryRefusalAsync"/> once, after
/// it has cleared the rule codes and put the action-level message, and before the page is drawn: the one place that asks a page for its read.
/// The implementation only reads: the refusal writes nothing, and what a forged request could make a page do is a read of what the signed-in user already sees on the page's GET. A page whose GET
/// makes a write, consumes a single-use value or decides by a query value does not implement it - the inventory test (<c>ForgeryRefusalReadInventoryTests</c>) holds every page with a POST handler to
/// this interface, to one of the named exemptions, or to the closed list of pages still waiting for their own safe read.
/// </summary>
internal interface IReadsOnForgeryRefusal
{
    /// <summary>Reads what the page's GET reads, with no query value, for the page shown again.</summary>
    Task ReadAfterForgeryRefusalAsync(CancellationToken ct);
}
