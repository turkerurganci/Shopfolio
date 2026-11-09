using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>One link row of a content gallery as the readers need it: the owner, the image, the mark, the place and the firm's text.</summary>
internal sealed record ContentGalleryRow(Guid OwnerId, Guid ImageId, bool IsMain, int SortOrder, DateTimeOffset CreatedAt, string? AltText);

/// <summary>The one place that says which image a content record shows on a card (02 3.11.2): the home page blocks and the list pages read it here.</summary>
internal static class ContentMainImages
{
    /// <summary>The image each record shows: the one marked main, or - a gallery always has one, so only when the mark is missing - the first of its order.</summary>
    public static Dictionary<Guid, ContentImage> Of(IEnumerable<ContentGalleryRow> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        return rows
            .GroupBy(r => r.OwnerId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    ContentGalleryRow main = g.OrderByDescending(r => r.IsMain).ThenBy(r => r.SortOrder).ThenBy(r => r.CreatedAt).ThenBy(r => r.ImageId).First();
                    return new ContentImage(main.ImageId, main.AltText);
                });
    }
}
