using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// The outcome of making a download link for a digital order item (05 7.7.4; 02 3.12.1, 3.12.5, 3.12.7): the link's protected value - the right dropped once, now - or a
/// named refusal that dropped nothing: the item cannot be downloaded (not a digital item, payment not confirmed, no such item), the item has no file to give (the right is
/// not spent on a download that would find none), or the item's right is used up (Z-19: a count, no time limit; the buyer asks the firm to renew it, 02 3.12.6).
/// </summary>
public sealed class DownloadLinkIssue : IOperationResult
{
    private DownloadLinkIssue(string? value, string? reason)
    {
        Value = value;
        Reason = reason;
    }

    public static DownloadLinkIssue NotAvailable { get; } = new(null, "NotAvailable");

    public static DownloadLinkIssue NoFile { get; } = new(null, "NoFile");

    public static DownloadLinkIssue AllowanceUsed { get; } = new(null, "AllowanceUsed");

    /// <summary>The protected value of the link, to be put after <c>/indir/</c>; null on a refusal.</summary>
    public string? Value { get; }

    /// <summary>Why no link was made, for the log; null when one was.</summary>
    public string? Reason { get; }

    public bool IsRejected => Reason is not null;

    public static DownloadLinkIssue Issued(string value) => new(value, null);
}

/// <summary>
/// The delivery of a digital product from the order page's button (02 3.12.1, 3.12.5, 3.12.7; 05 7.7.4; T340): it makes the one-hour link and drops the right in the
/// same step. The right is taken only when the item is downloadable, a file is there to give and the counter is below the allowance frozen at the order; the file the
/// link opens is the variant's current one, read when the transfer starts (the download endpoint, T312). Joins the caller's transaction and must run inside one
/// (09 9.1 item 3), so the right dropped and the page that shows the link are one commit.
/// </summary>
public interface IDigitalDelivery
{
    Task<DownloadLinkIssue> IssueLinkAsync(Guid orderItemId, CancellationToken ct);
}
