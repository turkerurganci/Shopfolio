using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.SearchEngine;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Lists;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Lists;

namespace Shopfolio.Pages.Content;

/// <summary>
/// The common part of the four content record pages (T505; 07 UC-09...UC-12; 04 5.8): Hakkımızda at its fixed address, a service introduction, a reference work and a general page by
/// <c>{Slug}</c>. The page shows the name or title, the gallery, the text and the type's own region; a service and a reference carry "İlgili ürünler" - the linked products that are published,
/// the heading only when there is one - and a service the "Bize ulaşın" link to the contact page. A draft opens for a signed-in administrator with the "Taslak" band and is closed to the search
/// engines; a visitor, an unknown slug and another type's slug all answer 404 alike (04 5.8.9). The page writes nothing: the video link lives in the text and "Paylaş" is the shared button.
/// </summary>
public abstract class ContentRecordPageModel : PageModel
{
    private readonly ICorporateContentModule _content;

    protected ContentRecordPageModel(ICorporateContentModule content)
    {
        ArgumentNullException.ThrowIfNull(content);
        _content = content;
    }

    public string Title { get; private set; } = string.Empty;

    public IHtmlContent? Body { get; private set; }

    public IReadOnlyList<GalleryImage> Images { get; private set; } = [];

    public IReadOnlyList<ProductCard> LinkedProducts { get; private set; } = [];

    /// <summary>The "Bize ulaşın" link of a service introduction, else null (07 UC-10; D-1209 keeps the form's anchor for the contact page's task).</summary>
    public string? ContactPath => Kind == ContentPageKind.Offering ? FramePaths.Contact : null;

    internal IHtmlContent? Head { get; private set; }

    protected abstract ContentPageKind Kind { get; }

    /// <summary>The type segment of the panel's record address (<c>/panel/kurumsal-icerik/{tip}/{iç anahtar}</c>).</summary>
    protected abstract string PanelType { get; }

    /// <summary>The address prefix of the record's page; null for Hakkımızda.</summary>
    protected abstract string? Prefix { get; }

    public async Task<IActionResult> OnGetAsync(string? slug, CancellationToken ct)
    {
        bool asAdministrator = await HttpContext.HasAdminSessionAsync();
        ContentRecordPage? page = await _content.GetContentPageAsync(Kind, slug, asAdministrator, ct);
        if (page is null)
        {
            return NotFound();
        }

        BrandIdentity brand = await _content.GetBrandIdentityAsync(ct);
        string? brandName = brand.HasBrandName ? brand.DisplayName : null;
        string? logoPath = brand.LogoId is { } logo ? FrameStyle.ImagePath(logo) : null;
        GalleryImage? first = page.Images.FirstOrDefault(i => i.IsMain) ?? (page.Images.Count > 0 ? page.Images[0] : null);
        string? imagePath = first is null ? null : FrameStyle.ImagePath(first.ImageId);

        if (page.IsDraft)
        {
            HttpContext.SetDraftPreview($"{PanelFramePaths.CorporateContent}/{PanelType}/{page.RecordId:D}");
        }

        Title = page.Title;
        Body = RichText.Render(page.BodyHtml);
        Images = page.Images;
        LinkedProducts = ProductCards.Of(
            [.. page.Related.Select(r => r.Item)],
            item =>
            {
                ProductListCard card = page.Related.Single(r => r.Item.Id == item.Id);
                return card.MainImageId is { } id ? ProductCards.ImageOf(id, card.ImageAltText ?? item.Name) : null;
            });

        PageSearchData data = Prefix is null
            ? RecordPages.FixedContent(page.Path, page.Title, page.Summary, page.BodyHtml, imagePath, logoPath, brandName, !page.IsDraft)
            : RecordPages.Content(Prefix, page.Title, page.Summary, page.BodyHtml, AddressRules.SlugForLookup(slug ?? string.Empty), imagePath, logoPath, brandName, !page.IsDraft, []);
        Head = HttpContext.RequestServices.GetRequiredService<SearchEngineData>().Head(HttpContext, data);
        return Page();
    }
}

/// <summary>Hakkımızda (T505; 07 UC-09): one record at a fixed address.</summary>
public sealed class AboutPageModel : ContentRecordPageModel
{
    public AboutPageModel(ICorporateContentModule content)
        : base(content)
    {
    }

    protected override ContentPageKind Kind => ContentPageKind.AboutPage;

    protected override string PanelType => "hakkimizda";

    protected override string? Prefix => null;
}

/// <summary>A service introduction's page (T505; 07 UC-10).</summary>
public sealed class ServicePageModel : ContentRecordPageModel
{
    public ServicePageModel(ICorporateContentModule content)
        : base(content)
    {
    }

    protected override ContentPageKind Kind => ContentPageKind.Offering;

    protected override string PanelType => "hizmet";

    protected override string? Prefix => AddressRules.ServicePrefix;
}

/// <summary>A reference work's page (T505; 07 UC-11).</summary>
public sealed class ReferencePageModel : ContentRecordPageModel
{
    public ReferencePageModel(ICorporateContentModule content)
        : base(content)
    {
    }

    protected override ContentPageKind Kind => ContentPageKind.PortfolioItem;

    protected override string PanelType => "referans";

    protected override string? Prefix => AddressRules.ReferencePrefix;
}

/// <summary>A general page (T505; 07 UC-12): text and gallery only.</summary>
public sealed class GeneralPageModel : ContentRecordPageModel
{
    public GeneralPageModel(ICorporateContentModule content)
        : base(content)
    {
    }

    protected override ContentPageKind Kind => ContentPageKind.CustomPage;

    protected override string PanelType => "sayfa";

    protected override string? Prefix => AddressRules.GeneralPagePrefix;
}
