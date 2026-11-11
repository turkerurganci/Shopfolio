using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Lists;

namespace Shopfolio.Modules.CorporateContent.Contracts;

/// <summary>
/// One page of a content list page (T505; 04 5.7.3): <see cref="Title"/> is the type's name as the firm gave it (the menu's name), <see cref="Cards"/> the published records in the firm's manual
/// order - 24 a page, a draft never among them - and the paging figures; a type with no published record has no cards (the page shows the empty-state line, 04 5.7.8).
/// </summary>
public sealed record ContentListView(string Title, IReadOnlyList<HomepageCard> Cards, int PageNumber, int PageSize, int TotalCount, int PageCount);

/// <summary>
/// A content record's page (T505; 04 5.8.3-5.8.5): <see cref="Title"/> is the record's name or title - for Hakkımızda, which has none, the firm's menu name -; <see cref="Summary"/> the short
/// introduction or description; <see cref="BodyHtml"/> the stored cleaned text; <see cref="Images"/> the gallery in the firm's order with the text each image uses; <see cref="Related"/> the
/// linked products that are published, in the firm's order (empty for Hakkımızda and a general page, and when none is linked or none is published). <see cref="IsDraft"/> is true for the
/// administrator's preview of a draft; <see cref="RecordId"/> is the record's internal key, which only the panel's address uses. <see cref="Path"/> is the page's own address.
/// </summary>
public sealed record ContentRecordPage(
    ContentPageKind Kind,
    Guid RecordId,
    string Title,
    string? Summary,
    string? BodyHtml,
    string Path,
    bool IsDraft,
    IReadOnlyList<GalleryImage> Images,
    IReadOnlyList<ProductListCard> Related);
