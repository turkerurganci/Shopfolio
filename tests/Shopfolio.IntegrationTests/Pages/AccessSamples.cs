using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Ordering.Contracts;

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

[IgnoreAntiforgeryToken]
public sealed class PanelOrdersModel(AccessWriteLog log) : AccessSamplePage(log)
{
    protected override string Label => "panel-orders";
}

[IgnoreAntiforgeryToken]
public sealed class PanelResetModel(AccessWriteLog log) : AccessSamplePage(log)
{
    protected override string Label => "panel-reset";
}

[IgnoreAntiforgeryToken]
public sealed class AccountHomeModel(AccessWriteLog log) : AccessSamplePage(log)
{
    protected override string Label => "account-home";
}

[IgnoreAntiforgeryToken]
public sealed class AccountAddressesModel(AccessWriteLog log) : AccessSamplePage(log)
{
    protected override string Label => "account-addresses";
}

[IgnoreAntiforgeryToken]
public sealed class AccountDeletedModel(AccessWriteLog log) : AccessSamplePage(log)
{
    protected override string Label => "account-deleted";
}

[IgnoreAntiforgeryToken]
public sealed class ProductSampleModel(AccessWriteLog log) : AccessSamplePage(log)
{
    protected override string Label => "product";
}

/// <summary>The sample sign-in page of the storefront: it shows where the return address would lead (07 2.3.2; 05 6.5.6).</summary>
public sealed class CustomerSignInSampleModel : PageModel
{
    public IActionResult OnGet() => Content(ReturnPath.Resolve(Request.Query[ReturnPath.ParameterName], ReturnArea.Storefront));
}

/// <summary>The sample sign-in page of the panel.</summary>
public sealed class PanelSignInSampleModel : PageModel
{
    public IActionResult OnGet() => Content(ReturnPath.Resolve(Request.Query[ReturnPath.ParameterName], ReturnArea.Panel));
}

/// <summary>A sign-out page of the panel (UC-109): it writes in <c>Save</c> only when a session reached it; the challenge sends a request without one to the sign-out's redirect.</summary>
[IgnoreAntiforgeryToken]
[SignOutPage]
public sealed class PanelSignOutModel(AccessWriteLog log) : AccessSamplePage(log)
{
    protected override string Label => "panel-signout";
}

/// <summary>A sign-out page of the account folder, to try the storefront's sign-out redirect (UC-77).</summary>
[IgnoreAntiforgeryToken]
[SignOutPage]
public sealed class AccountSignOutModel(AccessWriteLog log) : AccessSamplePage(log)
{
    protected override string Label => "account-signout";
}

/// <summary>A panel page for a record that does not exist: every request gets the one 404 (07 2.3.3), and a POST writes nothing.</summary>
[IgnoreAntiforgeryToken]
public sealed class PanelRecordModel : PageModel
{
    public IActionResult OnGet(Guid id) => AccessDenial.NotFound();

    public IActionResult OnPostSave(Guid id) => AccessDenial.NotFound();
}

/// <summary>The member root of the order page (07 2.1.3, 2.3.3): the order of the signed-in customer, else the 404 of every denied access.</summary>
[IgnoreAntiforgeryToken]
public sealed class MemberOrderModel(IOrderingModule ordering, AccessWriteLog log) : PageModel
{
    public async Task<IActionResult> OnGetAsync(string orderNumber, CancellationToken ct)
    {
        Guid? orderId = await FindAsync(orderNumber, ct);
        return orderId is null ? AccessDenial.NotFound() : Content($"order:{orderId}");
    }

    public async Task<IActionResult> OnPostCancelAsync(string orderNumber, CancellationToken ct)
    {
        Guid? orderId = await FindAsync(orderNumber, ct);
        if (orderId is null)
        {
            return AccessDenial.NotFound();
        }

        log.Add($"cancel:{orderId}");
        return FormResults.SeeOther("/");
    }

    private async Task<Guid?> FindAsync(string orderNumber, CancellationToken ct)
    {
        Guid? customerId = User.AccountId();
        return customerId is null ? null : await ordering.FindMemberOrderAsync(orderNumber, customerId.Value, ct);
    }
}

/// <summary>The access-key root of the order page (07 2.1.3, 2.3.4): the order that holds the key, else the 404 of every denied access.</summary>
[IgnoreAntiforgeryToken]
public sealed class AccessOrderModel(IOrderingModule ordering, AccessWriteLog log) : PageModel
{
    public async Task<IActionResult> OnGetAsync(string accessKey, CancellationToken ct)
    {
        Guid? orderId = await ordering.FindOrderByAccessKeyAsync(accessKey, ct);
        return orderId is null ? AccessDenial.NotFound() : Content($"order:{orderId}");
    }

    public async Task<IActionResult> OnPostCancelAsync(string accessKey, CancellationToken ct)
    {
        Guid? orderId = await ordering.FindOrderByAccessKeyAsync(accessKey, ct);
        if (orderId is null)
        {
            return AccessDenial.NotFound();
        }

        log.Add($"cancel:{orderId}");
        return FormResults.SeeOther("/");
    }
}

/// <summary>A storefront page that reads the panel's session for the admin strip (07 2.2.6; 05 3.5.4): the customer identity and the panel session are two answers.</summary>
public sealed class AdminStripModel : PageModel
{
    public async Task<IActionResult> OnGetAsync() => Content($"customer:{User.Identity?.IsAuthenticated == true}|admin:{await HttpContext.HasAdminSessionAsync()}");
}
