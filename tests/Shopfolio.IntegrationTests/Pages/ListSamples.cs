using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Lists;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.IntegrationTests.Pages;

/// <summary>
/// The query model of the storefront's product lists (T210; 07 5.3.2 <c>ProductQuery</c>): every field is text so that a wrong value is never a binding error
/// (07 2.6.6), the names are the camelCase of the fields (07 2.6.1), and the price range is the model's own rule with its code (09 3.4 item 2).
/// </summary>
public sealed class ProductQuery : PagedQuery, IValidatableObject
{
    [SpecAssignedName]
    [Display(Name = "Arama")]
    public string? Search { get; init; }

    [SpecAssignedName]
    [Display(Name = "En düşük fiyat")]
    public string? MinPrice { get; init; }

    [SpecAssignedName]
    [Display(Name = "En yüksek fiyat")]
    public string? MaxPrice { get; init; }

    [SpecAssignedName]
    public string? InStock { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) => ListFilters.PriceRangeResults(MinPrice, MaxPrice, nameof(MinPrice));
}

/// <summary>The query model of the panel's product list (07 5.3.2 <c>AdminProductQuery</c>): the name search, the publish status of the closed list and the page.</summary>
public sealed class AdminProductQuery : PagedQuery
{
    [SpecAssignedName]
    public string? Search { get; init; }

    public string? PublishStatus { get; init; }
}

/// <summary>The query model of a date-range list (07 5.3.2 <c>AuditLogQuery</c>): the two dates and the page.</summary>
public sealed class DateRangeQuery : PagedQuery, IValidatableObject
{
    [SpecAssignedName]
    [Display(Name = "Başlangıç tarihi")]
    public string? StartDate { get; init; }

    [SpecAssignedName]
    [Display(Name = "Bitiş tarihi")]
    public string? EndDate { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) => ListFilters.DateRangeResults(StartDate, EndDate, nameof(EndDate));
}

/// <summary>The query model of a panel record page (07 5.3.1): the only common parameter it takes is the return address.</summary>
public sealed class ProductRecordQuery : IQueryModel
{
    [SpecAssignedName]
    public string? ReturnUrl { get; init; }
}

/// <summary>The input of the record's save form: the return address travels in a hidden field and is read where the handler reads it.</summary>
public sealed class SaveProductRecordInput : IFormInput
{
    [SpecAssignedName]
    public string? ReturnUrl { get; set; }
}

/// <summary>The products the sample lists show: the storefront's published products and the panel's products, with the paging and search the pattern prescribes (09 10.5).</summary>
public static class SampleProductLists
{
    public const string PanelListRoot = "/panel/urun-listesi";

    public const string PanelRecordRoot = "/panel/urun-kaydi";

    internal static async Task<PagedList<ProductRow>?> StorefrontAsync(AppDbContext db, ProductQuery query, bool searchPage, CancellationToken ct)
    {
        IQueryable<Product> products = db.Products.AsNoTracking().Where(p => p.PublishStatus == PublishStatus.Published);

        if (searchPage)
        {
            // 07 2.6.3: a blank word makes no result list.
            if (SearchTerm.ToLikePattern(query.Search) is not { } pattern)
            {
                return null;
            }

            products = products.Where(p => EF.Functions.Like(p.NameSearchForm, pattern, SearchTerm.EscapeCharacter));
        }

        PriceFilter price = ListFilters.ReadPrice(query.MinPrice, query.MaxPrice);
        if (price.AppliedMin is { } min)
        {
            products = products.Where(p => db.Variants.Any(v => v.ProductId == p.Id && v.Price >= min));
        }

        if (price.AppliedMax is { } max)
        {
            products = products.Where(p => db.Variants.Any(v => v.ProductId == p.Id && v.Price <= max));
        }

        if (ListFilters.ReadInStock(query.InStock))
        {
            products = products.Where(p => db.Variants.Any(v => v.ProductId == p.Id && v.StockQuantity > v.ReservedQuantity));
        }

        // The newest first, the inner key last and descending (07 2.6.5; 09 10.5 item 1).
        PagedList<Product> page = await products.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id)
            .ToPageAsync(query.RequestedPage(), PageSizes.StorefrontProducts, ct);
        return Rows(page);
    }

    private static PagedList<ProductRow> Rows(PagedList<Product> page) =>
        new([.. page.Items.Select(p => new ProductRow(p.Id, p.Name))], page.PageNumber, page.PageSize, page.TotalCount, page.PageCount);

    internal static async Task<PagedList<ProductRow>> PanelAsync(AppDbContext db, AdminProductQuery query, CancellationToken ct)
    {
        IQueryable<Product> products = db.Products.AsNoTracking();
        if (SearchTerm.ToLikePattern(query.Search) is { } pattern)
        {
            products = products.Where(p => EF.Functions.Like(p.NameSearchForm, pattern, SearchTerm.EscapeCharacter));
        }

        if (ListFilters.ReadCode<PublishStatus>(query.PublishStatus) is { } status)
        {
            products = products.Where(p => p.PublishStatus == status);
        }

        PagedList<Product> page = await products.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id)
            .ToPageAsync(query.RequestedPage(), PageSizes.Panel, ct);
        return Rows(page);
    }
}

/// <summary>A row of a sample list.</summary>
public sealed record ProductRow(Guid Id, string Name);

/// <summary>The common part of the storefront list samples: the result, the closed mark the list pattern sets, and whether the search field is asked for.</summary>
public abstract class StorefrontListSample : PageModel, IListPage
{
    public abstract bool IsSearchPage { get; }

    public bool ClosedToSearchEngines { get; set; }

    internal PagedList<ProductRow>? Result { get; private set; }

    public bool NoResultList => Result is null;

    public async Task<IActionResult> OnGetAsync(ProductQuery query, CancellationToken ct)
    {
        AppDbContext db = HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        Result = await SampleProductLists.StorefrontAsync(db, query, IsSearchPage, ct);
        return Page();
    }
}

public sealed class CategoryListSampleModel : StorefrontListSample
{
    public override bool IsSearchPage => false;
}

public sealed class SearchListSampleModel : StorefrontListSample
{
    public override bool IsSearchPage => true;
}

/// <summary>The sample panel list: the rows link to the record with the list's address in <c>returnUrl</c> (07 2.6.8).</summary>
public sealed class PanelProductListSampleModel : PageModel
{
    internal PagedList<ProductRow> Result { get; private set; } = new([], 1, PageSizes.Panel, 0, 1);

    public string ListAddress { get; private set; } = SampleProductLists.PanelListRoot;

    public async Task<IActionResult> OnGetAsync(AdminProductQuery query, CancellationToken ct)
    {
        AppDbContext db = HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        Result = await SampleProductLists.PanelAsync(db, query, ct);
        ListAddress = Shopfolio.Infrastructure.Lists.ListAddress.Of(SampleProductLists.PanelListRoot, query, Result.PageNumber);
        return Page();
    }

    public string RecordLink(Guid id) => ListReturn.LinkTo($"{SampleProductLists.PanelRecordRoot}/{id}", ListAddress);
}

/// <summary>The sample panel record: "back to the list" and the redirect after the save use the return address, read through the one check (07 2.6.8).</summary>
public sealed class PanelProductRecordSampleModel : PageModel
{
    public string BackToList { get; private set; } = SampleProductLists.PanelListRoot;

    public string ReturnUrlField { get; private set; } = string.Empty;

    public IActionResult OnGet(Guid id, ProductRecordQuery query)
    {
        BackToList = ListReturn.Target(query.ReturnUrl, SampleProductLists.PanelListRoot);
        ReturnUrlField = query.ReturnUrl ?? string.Empty;
        return Page();
    }

#pragma warning disable CA1822 // A page handler is an instance member by the framework's convention.
    public IActionResult OnPostSaveProductRecord(Guid id, SaveProductRecordInput input)
#pragma warning restore CA1822
    {
        return FormResults.SeeOther(ListReturn.Target(input.ReturnUrl, SampleProductLists.PanelListRoot));
    }
}

/// <summary>The sample date-range list: it shows the range the pattern applies, and the field message of the end date.</summary>
public sealed class DateRangeListSampleModel : PageModel
{
    public string Applied { get; private set; } = string.Empty;

    public IActionResult OnGet(DateRangeQuery query)
    {
        DateFilter dates = ListFilters.ReadDates(query.StartDate, query.EndDate);
        Applied = $"{dates.AppliedStart:yyyy-MM-dd}|{dates.AppliedEnd:yyyy-MM-dd}";
        return Page();
    }
}
