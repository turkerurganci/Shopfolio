using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.IntegrationTests.Pages;

/// <summary>The writes the sample pages of the access tests make, in memory; a singleton of the test host. A POST that is refused writes nothing here (T207; 07 2.3.2, 2.3.3).</summary>
public sealed class AccessWriteLog
{
    private readonly List<string> _writes = [];
    private readonly Lock _gate = new();

    public IReadOnlyList<string> Snapshot()
    {
        lock (_gate)
        {
            return [.. _writes];
        }
    }

    public void Add(string write)
    {
        lock (_gate)
        {
            _writes.Add(write);
        }
    }
}

/// <summary>The common shape of the test-only pages of the access tests: a GET shows a plain text, <c>Save</c> is the one writing POST (T207).</summary>
public abstract class AccessSamplePage : PageModel
{
    private readonly AccessWriteLog _log;

    protected AccessSamplePage(AccessWriteLog log)
    {
        _log = log;
    }

    protected abstract string Label { get; }

    public IActionResult OnGet() => Content(Label);

    public IActionResult OnPostSave()
    {
        _log.Add(Label);
        return FormResults.SeeOther("/");
    }
}

public sealed class PanelOrdersModel(AccessWriteLog log) : AccessSamplePage(log)
{
    protected override string Label => "panel-orders";
}

public sealed class AccountHomeModel(AccessWriteLog log) : AccessSamplePage(log)
{
    protected override string Label => "account-home";
}

public sealed class AccountSampleModel(AccessWriteLog log) : AccessSamplePage(log)
{
    protected override string Label => "account-sample";
}

public sealed class ProductSampleModel(AccessWriteLog log) : AccessSamplePage(log)
{
    protected override string Label => "product";
}

/// <summary>A sign-out page of the account folder, to try the storefront's sign-out redirect (UC-77).</summary>
[SignOutPage]
public sealed class AccountSignOutModel(AccessWriteLog log) : AccessSamplePage(log)
{
    protected override string Label => "account-signout";
}

/// <summary>A panel page for a record that does not exist: every request gets the one 404 (07 2.3.3), and a POST writes nothing.</summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822", Justification = "Razor Pages ignores static handler methods.")]
public sealed class PanelRecordModel : PageModel
{
    public IActionResult OnGet(Guid id) => AccessDenial.NotFound();

    public IActionResult OnPostSave(Guid id) => AccessDenial.NotFound();
}

/// <summary>A storefront page that reads the panel's session for the admin strip (07 2.2.6; 05 3.5.4): the customer identity and the panel session are two answers.</summary>
public sealed class AdminStripModel : PageModel
{
    public async Task<IActionResult> OnGetAsync() => Content($"customer:{User.Identity?.IsAuthenticated == true}|admin:{await HttpContext.HasAdminSessionAsync()}");
}
