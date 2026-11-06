using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.IntegrationTests.Pages.Panel;

/// <summary>
/// The test-only sample of a value written against the shown value (T208; 07 2.4.3; 05 7.3.3): the stock field of a variant. The form carries the
/// value the screen showed in <c>OriginalStockQuantity</c>. Lowering the stock needs a confirmation step; the comparison is made on the first send
/// too (before the confirmation window), and the last step carries the first form's value, never the page's current one (K-1212 (1)).
/// </summary>
[IgnoreAntiforgeryToken]
public sealed class VersionedStockModel : PageModel
{
    public VersionedStockInput Input { get; set; } = new();

    public Guid RecordId { get; private set; }

    /// <summary>The step the page shows: the form, the confirmation of a lowering, or the notice that the shown value changed.</summary>
    public string Step { get; private set; } = "form";

    public int? CurrentStock { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        RecordId = id;
        int? stock = await ReadStockAsync(id, ct);
        Input = new VersionedStockInput { OriginalStockQuantity = Text(stock), StockQuantity = Text(stock) };
        CurrentStock = stock;
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(Guid id, VersionedStockInput input, CancellationToken ct)
    {
        Input = input;
        IServiceProvider services = HttpContext.RequestServices;
        RecordId = id;
        int? current = await ReadStockAsync(id, ct);
        if (!ShownValue.TryReadNumber(Input.OriginalStockQuantity, out int? shown) || !ShownValue.TryReadNumber(Input.StockQuantity, out int? value))
        {
            return Changed(current);
        }

        // The first send is compared too: a lowering is not even confirmed against a value that is no longer there.
        if (!ShownValue.Matches(Input.OriginalStockQuantity, current))
        {
            return Changed(current);
        }

        if (!Input.Confirmed && value is not null && (shown is null || value < shown))
        {
            Step = "confirm";
            CurrentStock = current;
            return Page();
        }

        StockQuantityWriteResult result = await services.GetRequiredService<ITransactionRunner>().RunAsync(
            cancel => services.GetRequiredService<ICatalogModule>().SaveStockQuantityAsync(id, shown, value, cancel),
            ct);
        if (result.IsShownValueChanged)
        {
            return Changed(await ReadStockAsync(id, ct));
        }

        if (result.IsRejected)
        {
            CurrentStock = await ReadStockAsync(id, ct);
            return FormResults.Redisplay(this);
        }

        return FormResults.SeeOther("/panel/surumlu-stok/" + id);
    }

    private IActionResult Changed(int? current)
    {
        Step = "changed";
        CurrentStock = current;
        Input.OriginalStockQuantity = Text(current);
        Input.Confirmed = false;
        return FormResults.Conflict(this);
    }

    private async Task<int?> ReadStockAsync(Guid id, CancellationToken ct)
    {
        AppDbContext db = HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        return await db.Variants.AsNoTracking().Where(v => v.Id == id).Select(v => v.StockQuantity).SingleAsync(ct);
    }

    private static string Text(int? stock) => stock?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
}

/// <summary>The input of the sample stock form.</summary>
public sealed class VersionedStockInput : IFormInput
{
    public string? OriginalStockQuantity { get; set; }

    public string? StockQuantity { get; set; }

    public bool Confirmed { get; set; }
}
