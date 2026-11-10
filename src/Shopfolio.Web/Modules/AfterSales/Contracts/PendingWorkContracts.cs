namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>
/// One page of the orders that owe the firm a refund step (T382; 02 10.6.1): the orders of the page and the id the next page starts after; <see cref="LastId"/> is empty at the end.
/// The page is read from the item records - the subject ids are the cursor - so a page can hold no order and still not be the end.
/// </summary>
public sealed record RefundProcessingPage(Guid? LastId, IReadOnlyList<Guid> OrderIds);
