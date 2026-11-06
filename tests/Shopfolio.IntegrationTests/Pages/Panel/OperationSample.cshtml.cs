using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Ordering;

namespace Shopfolio.IntegrationTests.Pages.Panel;

/// <summary>
/// The test-only sample of the available-operations pattern (T217; 07 8.1; 09 8.3). The GET asks the two rules and draws a form only for an open operation,
/// writing the open quantity, the default, the upper limit and the last day as text; the three POST handlers re-check the same rule instances through
/// <see cref="GatedOrderOperation"/>. The customer's two operations answer a refusal with 303 and the reason; the panel's operation answers a changed
/// version with 409 and a closed operation with 422. The page has no script and decides nothing from what the browser sent.
/// </summary>
public sealed class OperationSampleModel : PageModel
{
    public const string AdminClosedMessage = "Bu işlem şu anda yapılamaz (örnek).";

    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    internal OperationAvailability Cancel { get; private set; }

    internal OperationAvailability Withdraw { get; private set; }

    public Guid RecordId { get; private set; }

    public Guid ItemId { get; private set; }

    public int Version { get; private set; }

    public string Path => "/panel/islem-ornegi/" + RecordId;

    public CancelInput CancelForm { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        await LoadAsync(id, ct);
        return Page();
    }

    [GatedOperation]
    public async Task<IActionResult> OnPostCustomerCancelAsync(Guid id, CancelInput input, CancellationToken ct)
    {
        SampleRules rules = HttpContext.RequestServices.GetRequiredService<SampleRules>();
        GatedOrderResult result = await RunAsync(id, null, rules.Cancel, input.Quantity, ct);
        return AnswerCustomer(id, result);
    }

    [GatedOperation]
    public async Task<IActionResult> OnPostCustomerWithdrawAsync(Guid id, CancelInput input, CancellationToken ct)
    {
        SampleRules rules = HttpContext.RequestServices.GetRequiredService<SampleRules>();
        GatedOrderResult result = await RunAsync(id, null, rules.Withdraw, input.Quantity, ct);
        return AnswerCustomer(id, result);
    }

    [GatedOperation]
    public async Task<IActionResult> OnPostAdminCancelAsync(Guid id, AdminInput input, CancellationToken ct)
    {
        SampleRules rules = HttpContext.RequestServices.GetRequiredService<SampleRules>();
        GatedOrderResult result = await RunAsync(id, input.Version, rules.Cancel, input.Quantity, ct);
        if (!result.IsRejected)
        {
            return FormResults.SeeOther("/panel/islem-ornegi/" + id);
        }

        await LoadAsync(id, ct);
        return result.VersionChanged
            ? FormResults.Conflict(this)
            : OperationRefusals.Redisplay(this, result.Verdict, AdminClosedMessage);
    }

    private async Task<GatedOrderResult> RunAsync(Guid id, int? shownVersion, IOperationRule<SampleItemState> rule, int quantity, CancellationToken ct)
    {
        IServiceProvider services = HttpContext.RequestServices;
        AppDbContext db = services.GetRequiredService<AppDbContext>();
        TimeProvider time = services.GetRequiredService<TimeProvider>();
        return await services.GetRequiredService<GatedOrderOperation>().RunAsync(
            id,
            shownVersion,
            readCt => ReadStateAsync(db, time, id, readCt),
            rule,
            quantity,
            async (state, availability, token) =>
            {
                Order order = await db.Orders.SingleAsync(o => o.Id == id, token);
                OrderItem item = await db.OrderItems.SingleAsync(i => i.OrderId == id, token);
                db.Cancellations.Add(Cancellation.ByBuyer(item.Id, quantity, new ItemAmounts(100m, 10m, 15m), null, Now));
                order.BumpVersion();
            },
            ct);
    }

    private IActionResult AnswerCustomer(Guid id, GatedOrderResult result)
    {
        string orderPage = "/panel/islem-ornegi/" + id;
        return result.IsRejected
            ? OperationRefusals.ToOrderPage(this, result.Verdict, orderPage)
            : FormResults.SeeOther(orderPage);
    }

    private static async Task<SampleItemState> ReadStateAsync(AppDbContext db, TimeProvider time, Guid id, CancellationToken ct)
    {
        Order order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == id, ct);
        OrderItem item = await db.OrderItems.AsNoTracking().SingleAsync(i => i.OrderId == id, ct);
        int closed = await db.Cancellations.AsNoTracking().Where(c => c.OrderItemId == item.Id).SumAsync(c => c.Quantity, ct);
        return new SampleItemState(
            order.OrderStatus,
            order.PaymentStatus,
            item.ProductType,
            item.DeliveryDate,
            item.Quantity - closed,
            TurkeyTime.DateOf(time.GetUtcNow()));
    }

    private async Task LoadAsync(Guid id, CancellationToken ct)
    {
        IServiceProvider services = HttpContext.RequestServices;
        AppDbContext db = services.GetRequiredService<AppDbContext>();
        SampleRules rules = services.GetRequiredService<SampleRules>();
        SampleItemState state = await ReadStateAsync(db, services.GetRequiredService<TimeProvider>(), id, ct);
        Order order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == id, ct);
        OrderItem item = await db.OrderItems.AsNoTracking().SingleAsync(i => i.OrderId == id, ct);
        RecordId = id;
        ItemId = item.Id;
        Version = order.Version;
        Cancel = rules.Cancel.Evaluate(state);
        Withdraw = rules.Withdraw.Evaluate(state);
    }
}

/// <summary>The quantity form of the customer's sample operations: the quantity only; nothing the server derived travels back.</summary>
public sealed class CancelInput : IFormInput
{
    public int Quantity { get; set; }
}

/// <summary>The panel's sample operation form: the order's version and the quantity.</summary>
public sealed class AdminInput : IFormInput
{
    public int Version { get; set; }

    public int Quantity { get; set; }
}
