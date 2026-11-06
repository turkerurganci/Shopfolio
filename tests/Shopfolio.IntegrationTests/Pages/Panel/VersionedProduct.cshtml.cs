using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Modules.Catalog;

namespace Shopfolio.IntegrationTests.Pages.Panel;

/// <summary>
/// The test-only sample of the record variant (T208; 07 2.4.1, 2.4.5; 09 10.2 item 2): the edit form of a product. The GET writes the version the
/// record has into a hidden field; the POST is the one <see cref="RecordSave"/> sequence inside the transaction runner. A refused save is shown again
/// with 409, the values that were sent, the record's current version in the form and "Yine de kaydet" as the one button.
/// </summary>
public sealed class VersionedProductModel : PageModel
{
    public VersionedProductInput Input { get; set; } = new();

    public Guid RecordId { get; private set; }

    public bool Conflicted { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        AppDbContext db = HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        Product? product = await db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
        {
            return NotFound();
        }

        RecordId = id;
        Input = new VersionedProductInput { Version = product.Version, Description = product.Description ?? string.Empty };
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(Guid id, VersionedProductInput input, CancellationToken ct)
    {
        Input = input;
        IServiceProvider services = HttpContext.RequestServices;
        AppDbContext db = services.GetRequiredService<AppDbContext>();
        RecordId = id;
        string description = Input.Description;
        RecordSaveResult result = await services.GetRequiredService<ITransactionRunner>().RunAsync(
            async token =>
            {
                Product product = await db.Products.SingleAsync(p => p.Id == id, token);
                return await RecordSave.SaveAsync(db, product, Input.Version, p => p.Description = description, token);
            },
            ct);
        if (result.IsSaved)
        {
            return FormResults.SeeOther("/panel/surumlu-urun/" + id);
        }

        // The form carries the record's current version, so "Yine de kaydet" is a save like any other (07 2.4.5).
        Input.Version = await db.Products.AsNoTracking().Where(p => p.Id == id).Select(p => p.Version).SingleAsync(ct);
        Conflicted = true;
        return FormResults.Conflict(this);
    }
}

/// <summary>The input of the sample product form: the version the form was opened with and the field that is written.</summary>
public sealed class VersionedProductInput : IFormInput
{
    public int Version { get; set; }

    public string Description { get; set; } = string.Empty;
}
