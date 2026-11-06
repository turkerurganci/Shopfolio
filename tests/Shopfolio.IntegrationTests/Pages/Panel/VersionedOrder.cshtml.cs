using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Ordering;

namespace Shopfolio.IntegrationTests.Pages.Panel;

/// <summary>
/// The test-only sample of the order variant (T208; 07 2.4.2, 2.4.6; 05 7.3.2; 09 10.2 item 3). The administrator's form carries the order's version and
/// compares it after the row lock and the load; the customer's item cancellation carries no version, only the open quantity the screen showed. A refusal
/// of either shows the order as it is, with 409 and a message only - there is no way to overwrite.
/// </summary>
public sealed class VersionedOrderModel : PageModel
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    public VersionedOrderInput Input { get; set; } = new();

    public VersionedCustomerInput Customer { get; set; } = new();

    public Guid RecordId { get; private set; }

    public bool Refused { get; private set; }

    public string District { get; private set; } = string.Empty;

    public int OpenQuantity { get; private set; }

    public Guid ItemId { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        RecordId = id;
        await LoadAsync(id, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAdminSaveAsync(Guid id, VersionedOrderInput input, CancellationToken ct)
    {
        Input = input;
        IServiceProvider services = HttpContext.RequestServices;
        AppDbContext db = services.GetRequiredService<AppDbContext>();
        RecordId = id;
        string district = Input.District;
        SampleOutcome outcome = await services.GetRequiredService<ITransactionRunner>().RunAsync(
            async token =>
            {
                await services.GetRequiredService<OrderLocks>().LockAsync([id], token);
                Order order = await db.Orders.SingleAsync(o => o.Id == id, token);
                if (!ShownVersion.Matches(order, Input.Version))
                {
                    return SampleOutcome.Refused;
                }

                order.BillingDistrict = district;
                order.BumpVersion();
                return SampleOutcome.Done;
            },
            ct);
        return await AnswerAsync(outcome, id, ct);
    }

    public async Task<IActionResult> OnPostCustomerCancelAsync(Guid id, VersionedCustomerInput customer, CancellationToken ct)
    {
        Customer = customer;
        IServiceProvider services = HttpContext.RequestServices;
        AppDbContext db = services.GetRequiredService<AppDbContext>();
        RecordId = id;
        Guid itemId = Customer.ItemId;
        SampleOutcome outcome = await services.GetRequiredService<ITransactionRunner>().RunAsync(
            async token =>
            {
                await services.GetRequiredService<OrderLocks>().LockAsync([id], token);
                Order order = await db.Orders.SingleAsync(o => o.Id == id, token);
                OrderItem item = await db.OrderItems.SingleAsync(i => i.Id == itemId, token);
                int cancelled = await db.Cancellations.Where(c => c.OrderItemId == itemId).SumAsync(c => c.Quantity, token);
                int open = item.Quantity - cancelled;
                if (!ShownValue.TryReadNumber(Customer.OriginalOpenQuantity, out int? shown) || shown is null || shown != open || open <= 0)
                {
                    return SampleOutcome.Refused;
                }

                db.Cancellations.Add(Cancellation.ByBuyer(itemId, open, new ItemAmounts(100m, 10m, 15m), null, Now));
                order.BumpVersion();
                return SampleOutcome.Done;
            },
            ct);
        return await AnswerAsync(outcome, id, ct);
    }

    private async Task<IActionResult> AnswerAsync(SampleOutcome outcome, Guid id, CancellationToken ct)
    {
        if (!outcome.IsRejected)
        {
            return FormResults.SeeOther("/panel/surumlu-siparis/" + id);
        }

        Refused = true;
        await LoadAsync(id, ct);
        return FormResults.Conflict(this);
    }

    // The order as it is now: the forms carry the current version and the current open quantity.
    private async Task LoadAsync(Guid id, CancellationToken ct)
    {
        AppDbContext db = HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        Order order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == id, ct);
        OrderItem item = await db.OrderItems.AsNoTracking().SingleAsync(i => i.OrderId == id, ct);
        int cancelled = await db.Cancellations.AsNoTracking().Where(c => c.OrderItemId == item.Id).SumAsync(c => c.Quantity, ct);
        District = order.BillingDistrict ?? string.Empty;
        Input = new VersionedOrderInput { Version = order.Version, District = District };
        ItemId = item.Id;
        OpenQuantity = item.Quantity - cancelled;
    }
}

/// <summary>The administrator's form of the sample order page.</summary>
public sealed class VersionedOrderInput : IFormInput
{
    public int Version { get; set; }

    public string District { get; set; } = string.Empty;
}

/// <summary>The customer's item cancellation of the sample order page: no version, the shown open quantity only.</summary>
public sealed class VersionedCustomerInput : IFormInput
{
    public Guid ItemId { get; set; }

    public string? OriginalOpenQuantity { get; set; }
}

/// <summary>The result of a sample order operation: done or refused.</summary>
internal sealed class SampleOutcome : IOperationResult
{
    private SampleOutcome(bool isRejected)
    {
        IsRejected = isRejected;
    }

    public static SampleOutcome Done { get; } = new(false);

    public static SampleOutcome Refused { get; } = new(true);

    public bool IsRejected { get; }
}
