using System.Net;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Coupons;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Ordering.Documents;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// What the customer confirmed at the payment step (02 3.17.1, 3.17.2): the cart, the summary the customer saw - the lines, the shipping fee, the total and the payment method -, the
/// legal text versions shown with the ticked boxes, the coupon code, the contact e-mail, the addresses and the client address.
/// </summary>
internal sealed record OrderConfirmationRequest(
    CartOwner Owner,
    ConfirmationSummary ConfirmedSummary,
    Guid ShownPreInformationFormId,
    Guid ShownDistanceSalesContractId,
    TickedBoxes Ticked,
    string? CouponCode,
    string ContactEmail,
    PartyAddress? Shipping,
    PartyAddress Billing,
    IPAddress? IpAddress)
{
    /// <summary>The values the summary is made of: the same ones <see cref="OrderConfirmation.SummarizeAsync"/> made the confirmed summary from.</summary>
    public OrderSummaryInput SummaryInput => new(Owner, ConfirmedSummary.PaymentMethod, CouponCode, ContactEmail, Shipping, Billing);
}

/// <summary>The values the payment step's summary is made of (02 3.24.3): the cart's owner, the payment method, the coupon code, the contact e-mail and the two addresses.</summary>
internal sealed record OrderSummaryInput(
    CartOwner Owner,
    PaymentMethod PaymentMethod,
    string? CouponCode,
    string ContactEmail,
    PartyAddress? Shipping,
    PartyAddress Billing);

/// <summary>Why the confirmation did not make an order (02 3.17.2; T346 k7). A result code, never stored.</summary>
internal enum OrderConfirmationRefusal
{
    /// <summary>No line of the cart enters an order any more: no order, number, payment or reservation is born (11 T348 KK2).</summary>
    NoOrderableItem,

    /// <summary>The summary as the cart is now differs from the confirmed one, or an item could not be reserved: the customer sees the current summary and confirms again.</summary>
    SummaryChanged,

    /// <summary>The shown legal texts are no longer current or a needed box is unticked (02 3.17.2 last paragraph, 3.24.1).</summary>
    ConfirmationStepNotReady,
}

/// <summary>
/// The confirmation's answer: the order born, or a refusal with the current summary and the change lines that say what changed. A refusal is a value; the transaction runner
/// rolls the whole work back on it (09 9.1 item 3), so a cancelled earlier order and a reservation that partly fitted are undone with it.
/// </summary>
internal sealed record OrderConfirmationResult(
    OrderConfirmationRefusal? Refusal,
    Guid? OrderId,
    string? OrderNumber,
    ConfirmationSummary? CurrentSummary,
    IReadOnlyList<SummaryChange> Changes,
    ConfirmationStepCheck? StepCheck) : IOperationResult
{
    public bool IsRejected => Refusal is not null;

    public static OrderConfirmationResult Placed(Guid orderId, string orderNumber) => new(null, orderId, orderNumber, null, [], null);

    public static OrderConfirmationResult Refused(OrderConfirmationRefusal refusal, ConfirmationSummary? current = null, IReadOnlyList<SummaryChange>? changes = null, ConfirmationStepCheck? check = null) =>
        new(refusal, null, null, current, changes ?? [], check);
}

/// <summary>
/// The confirmation (T348; 02 3.17.1-3.17.7; 03 4.2.1): the one operation that makes an order. In the caller's transaction (09 9.1 item 3) it evaluates the cart again, compares the
/// result with the summary the customer confirmed - any difference of the total, its breakdown or the items stops it (02 3.17.2) -, checks the confirmation step, cancels the cart's
/// earlier unpaid order (02 3.17.7), and then in one go issues the number, freezes the items, the addresses, the identity, the versions and the documents, starts the payment window,
/// reserves the stock, the capacity and the coupon right (02 3.17.3) and queues B-1 and F-1 (02 9.2). Nothing calls an outside service here: the card's redirect follows the commit.
/// </summary>
internal sealed class OrderConfirmation
{
    private readonly AppDbContext _db;
    private readonly Carts _carts;
    private readonly ICatalogModule _catalog;
    private readonly ISettingsModule _settings;
    private readonly IConfirmationStep _step;
    private readonly INotificationsModule _notifications;
    private readonly OrderIdentifiers _identifiers;
    private readonly PaymentWindows _windows;
    private readonly OrderCounters _counters;
    private readonly OrderTransitions _transitions;
    private readonly TimeProvider _time;

    public OrderConfirmation(
        AppDbContext db,
        Carts carts,
        ICatalogModule catalog,
        ISettingsModule settings,
        IConfirmationStep step,
        INotificationsModule notifications,
        OrderIdentifiers identifiers,
        PaymentWindows windows,
        OrderCounters counters,
        OrderTransitions transitions,
        TimeProvider time)
    {
        _db = db;
        _carts = carts;
        _catalog = catalog;
        _settings = settings;
        _step = step;
        _notifications = notifications;
        _identifiers = identifiers;
        _windows = windows;
        _counters = counters;
        _transitions = transitions;
        _time = time;
    }

    public async Task<OrderConfirmationResult> ConfirmAsync(OrderConfirmationRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        PaymentMethod method = request.ConfirmedSummary.PaymentMethod;

        // 02 3.17.2: the cart is evaluated again, under the cart's lock (Carts.OpenAsync takes it first). 02 3.17.7: between the lock and the read the cart's earlier
        // unpaid order is cancelled and its reservation freed, so the customer is not stopped by the stock or the coupon right it held; a refusal below rolls the
        // cancellation back with the rest. A merged guest cart's order carries the deleted guest cart's id and is not found (the exception of 3.17.7).
        ShippingFeeSection shippingTerms = await _settings.GetShippingFeeAsync(ct);
        CartView cart = await _carts.OpenAsync(request.Owner, shippingTerms, CancelEarlierUnpaidOrdersAsync, ct);
        List<CartLine> lines = [.. cart.OrderingLines];
        if (cart.CartId is not Guid cartId || lines.Count == 0)
        {
            return OrderConfirmationResult.Refused(OrderConfirmationRefusal.NoOrderableItem);
        }

        Evaluation current = await EvaluateAsync(lines, cart.Shipping, request.SummaryInput, ct);
        List<SummaryChange> changes = [.. SummaryComparison.Compare(request.ConfirmedSummary, current.Summary)];
        if (cart.PaymentBlock != CartPaymentBlock.None || current.CouponRefused)
        {
            changes.Add(new SummaryChange(SummaryChangeKind.Total, null, null, null));
        }

        if (changes.Count > 0)
        {
            return OrderConfirmationResult.Refused(OrderConfirmationRefusal.SummaryChanged, current.Summary, changes);
        }

        ConfirmationStepCheck check = await _step.CheckAsync(
            new ConfirmationStepSubmission(request.ShownPreInformationFormId, request.ShownDistanceSalesContractId, request.Ticked, current.Summary.Lines, method), ct);
        if (!check.IsReady)
        {
            return OrderConfirmationResult.Refused(OrderConfirmationRefusal.ConfirmationStepNotReady, current.Summary, [], check);
        }

        DateTimeOffset now = _time.GetUtcNow();
        NewOrderIdentifiers identifiers = await _identifiers.IssueAsync(now, ct);
        PaymentWindow window = await _windows.StartAsync(method, now, ct);
        Order order = Order.Create(
            identifiers.OrderNumber,
            identifiers.OrderAccessKey,
            SecretKey.Create(),
            method,
            request.Owner.CustomerId is null,
            request.ContactEmail,
            current.Summary.Total,
            current.Summary.ShippingFee,
            window.EndsAt,
            now);
        order.CustomerId = request.Owner.CustomerId;
        order.CartId = cartId;
        order.IpAddress = request.IpAddress;
        order.PaymentReminderAt = window.ReminderAt;
        order.CouponId = current.CouponId;
        order.CouponCode = current.CouponId is null ? null : current.CouponCode;
        order.FreezeAddresses(
            current.HasPhysicalItem && request.Shipping is { } shipping
                ? (shipping.RecipientName, shipping.Phone ?? string.Empty, shipping.ProvinceId, shipping.District, shipping.AddressLine)
                : null,
            (request.Billing.RecipientName, request.Billing.ProvinceId, request.Billing.District, request.Billing.AddressLine));

        LegalTextVersionIds versions = await _settings.GetCurrentLegalTextVersionsAsync(ct);
        string? iban = method == PaymentMethod.BankTransfer ? await _settings.GetBankTransferIbanAsync(ct) : null;
        OrderFreeze.FreezeLegalIdentity(order, await _settings.GetCompanyIdentityAsync(ct), versions, iban);

        int downloadAllowance = (await _settings.GetProductDefaultsAsync(ct)).DownloadAllowance;
        List<OrderItem> items = [];
        for (int i = 0; i < current.Sources.Count; i++)
        {
            items.Add(OrderFreeze.FreezeItem(order.Id, current.Sources[i], lines[i].Quantity, current.Amounts[i], downloadAllowance, now));
        }

        order.FreezeShippingFeeVatBreakdown(_catalog.SplitShippingFee(order.ShippingFee, [.. items.Select(BasisLineOf)]));
        order.FreezeShippingPromise(current.ShippingPromise);
        string formText = (await _settings.GetLegalTextVersionAsync(versions.PreInformationForm!.Value, ct))!.Text;
        string contractText = (await _settings.GetLegalTextVersionAsync(versions.DistanceSalesContract!.Value, ct))!.Text;
        (string form, string contract) = OrderDocuments.Produce(order, items, formText, contractText);
        order.FreezeDocuments(form, contract);

        _db.Orders.Add(order);
        _db.OrderItems.AddRange(items);

        // 02 3.17.3: the stock, the capacity and the coupon right are reserved together; one that does not fit stops the order and its quantity is not told (02 3.17.2).
        if (!await _counters.ReserveAsync(order, items, ct))
        {
            return OrderConfirmationResult.Refused(
                OrderConfirmationRefusal.SummaryChanged, current.Summary, [new SummaryChange(SummaryChangeKind.ItemNotReserved, null, null, null)]);
        }

        // B-1 to the contact e-mail and F-1 to the firm, in the same transaction (02 3.17.1, 9.2; 08 2.2.6, 2.2.7).
        await _notifications.QueueCustomerNoticeAsync(new OrderReceivedNotice(order.Id), request.ContactEmail, ct);
        await _notifications.QueueCompanyNoticeAsync(new NewOrderNotice(order.Id, order.OrderNumber, method, order.Total, now), ct);
        return OrderConfirmationResult.Placed(order.Id, order.OrderNumber);
    }

    private async Task CancelEarlierUnpaidOrdersAsync(Guid cartId, CancellationToken ct)
    {
        List<Guid> earlier = await _db.Orders
            .Where(o => o.CartId == cartId && o.OrderStatus == OrderStatus.Placed && o.PaymentStatus == PaymentStatus.Pending)
            .OrderBy(o => o.Id)
            .Select(o => o.Id)
            .ToListAsync(ct);
        foreach (Guid orderId in earlier)
        {
            await _transitions.CancelOrderAsync(CancelOrderRequest.BySystem(orderId, TransitionReason.SameCartReorder), ct);
        }
    }

    /// <summary>
    /// The payment step's summary of the cart as it is now (02 3.24.3): the one evaluation the confirmation repeats, so the summary shown and the summary compared are made the same way.
    /// Null when no line enters an order. A read under the cart's lock in the caller's transaction; it writes nothing but the cart's read marks.
    /// </summary>
    public async Task<ConfirmationSummary?> SummarizeAsync(OrderSummaryInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        CartView cart = await _carts.OpenAsync(input.Owner, await _settings.GetShippingFeeAsync(ct), ct);
        List<CartLine> lines = [.. cart.OrderingLines];
        return lines.Count == 0 ? null : (await EvaluateAsync(lines, cart.Shipping, input, ct)).Summary;
    }

    private async Task<Evaluation> EvaluateAsync(List<CartLine> lines, ShippingAssessment shipping, OrderSummaryInput request, CancellationToken ct)
    {
        PaymentMethod method = request.PaymentMethod;
        Dictionary<Guid, OrderItemSource> byVariant = (await _catalog.ListOrderItemSourcesAsync([.. lines.Select(l => l.VariantId)], ct)).ToDictionary(s => s.VariantId);
        List<OrderItemSource> sources = [.. lines.Select(l => byVariant[l.VariantId])];
        List<CouponLine> couponLines = [.. lines.Select((l, i) => new CouponLine(AppliedPrice(sources[i]), l.Quantity, sources[i].VatRate))];

        List<ItemAmounts> amounts;
        Guid? couponId = null;
        bool couponRefused = false;
        if (string.IsNullOrWhiteSpace(request.CouponCode))
        {
            amounts = [.. couponLines.Select(c => _catalog.PriceItem(new ItemPriceInput(c.UnitPrice, c.Quantity, 0m, c.VatRate)))];
        }
        else
        {
            CouponApplication coupon = await _catalog.EvaluateCouponAsync(request.CouponCode, couponLines, ct);
            couponRefused = coupon.IsRejected;
            couponId = coupon.IsRejected ? null : coupon.CouponId;
            amounts = coupon.IsRejected
                ? [.. couponLines.Select(c => _catalog.PriceItem(new ItemPriceInput(c.UnitPrice, c.Quantity, 0m, c.VatRate)))]
                : [.. coupon.Items];
        }

        Dictionary<Guid, int?> periods = (await _catalog.GetShippingPeriodsAsync([.. lines.Select(l => l.VariantId)], ct)).ToDictionary(p => p.VariantId, p => p.ProductPeriod);
        int companyDefault = (await _settings.GetPeriodsAsync(ct)).DefaultShippingPeriod;
        int? promise = ShippingPromise.OfOrder(sources.Select(s => ShippingPromise.Effective(s.ProductType, periods.GetValueOrDefault(s.VariantId), companyDefault)));

        List<ConfirmationSummaryLine> summaryLines = [.. sources.Select((s, i) => ConfirmationSummaryText.LineOf(s, lines[i].Quantity, amounts[i]))];
        decimal total = _catalog.SumOrderTotal(amounts, shipping.Fee);
        ConfirmationSummary summary = new(summaryLines, shipping.Fee, total, method, promise, request.ContactEmail, request.Shipping, request.Billing);
        return new Evaluation(summary, sources, amounts, couponId, request.CouponCode, couponRefused, shipping.HasPhysicalItem, promise);
    }

    private static decimal AppliedPrice(OrderItemSource source) => source.Discount?.DiscountedPrice ?? source.UnitPrice;

    private static ShippingBasisLine BasisLineOf(OrderItem item) =>
        new(item.ProductType, item.VatRate, (item.DiscountedUnitPrice ?? item.UnitPrice) * item.Quantity);

    private sealed record Evaluation(
        ConfirmationSummary Summary,
        IReadOnlyList<OrderItemSource> Sources,
        IReadOnlyList<ItemAmounts> Amounts,
        Guid? CouponId,
        string? CouponCode,
        bool CouponRefused,
        bool HasPhysicalItem,
        int? ShippingPromise);
}
