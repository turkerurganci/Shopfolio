using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The order page's download button as a module operation (02 3.12.1, 3.12.5; 05 7.7.4): the item and its payment are read through the Ordering module's open face, the
/// file through <see cref="DigitalFileReader"/>, and only when both are there the Ordering module takes the download - the one conditional update - and the link is made.
/// The order is the reverse of the file's: a download that would find no file never spends the buyer's right.
/// </summary>
internal sealed class DigitalDelivery : IDigitalDelivery
{
    private readonly IOrderingModule _ordering;
    private readonly DigitalFileReader _files;
    private readonly DownloadLinks _links;
    private readonly TimeProvider _time;

    public DigitalDelivery(IOrderingModule ordering, DigitalFileReader files, DownloadLinks links, TimeProvider time)
    {
        _ordering = ordering;
        _files = files;
        _links = links;
        _time = time;
    }

    public async Task<DownloadLinkIssue> IssueLinkAsync(Guid orderItemId, CancellationToken ct)
    {
        DownloadableItem? item = await _ordering.FindDownloadableItemAsync(orderItemId, ct);
        if (item is null)
        {
            return DownloadLinkIssue.NotAvailable;
        }

        if (await _files.FindAsync(item.VariantId, item.DigitalFileId, ct) is null)
        {
            return DownloadLinkIssue.NoFile;
        }

        DownloadTakeResult taken = await _ordering.TakeDownloadAsync(orderItemId, ct);
        if (taken.IsTaken)
        {
            return DownloadLinkIssue.Issued(_links.Create(orderItemId, _time));
        }

        return ReferenceEquals(taken, DownloadTakeResult.AllowanceUsed) ? DownloadLinkIssue.AllowanceUsed : DownloadLinkIssue.NotAvailable;
    }
}
