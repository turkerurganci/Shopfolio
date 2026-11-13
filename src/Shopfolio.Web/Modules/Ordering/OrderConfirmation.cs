using System.Net;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Coupons;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Ordering.Documents;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// What the customer confirmed at the payment step (02 3.17.1, 3.17.2): the cart, the summary the customer saw - the lines, the shipping fee, the total and the payment method -, the
/// legal text versions shown with the ticked boxes, the coupon code, the contact e-mail, the addresses and the client address. The client address is required: a live order always
/// holds its IP until the disposal empties it (06 4.17; the table's <c>disposal_fields_together</c> rule), so a connection without an address cannot place an order (T377; D-783).
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
    IPAddress IpAddress,
    string? ConfirmationKey = null,
    Guid? BoundCustomerId = null)
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
    PartyAddress? Billing);

/// <summary>Why the confirmation did not make an order (02 3.17.2; T346 k7). A result code, never stored.</summary>
internal enum OrderConfirmationRefusal
{
    /// <summary>No line of the cart enters an order any more: no order, number, payment or reservation is born (11 T348 KK2).</summary>
    NoOrderableItem,

    /// <summary>The summary as the cart is now differs from the confirmed one, or an item could not be reserved: the customer sees the current summary and confirms again.</summary>
    SummaryChanged,

    /// <summary>The shown legal texts are no longer current or a needed box is unticked (02 3.17.2 last paragraph, 3.24.1).</summary>
    ConfirmationStepNotReady,

    /// <summary>
    /// The chosen payment method is not open now (T352; 02 3.21.2, 3.21.4, 6.9.3; 07 4.13.6 "seçilemeyen ödeme yöntemi", <c>PaymentMethodUnavailable</c>): the card without the
    /// provider's keys, the bank transfer switched off or without its IBAN. Nothing is born.
    /// </summary>
    PaymentMethodUnavailable,

    /// <summary>
    /// A limit that counts the orders stops the new order (T377; 02 8.2, 8.2.3; 07 4.13.6): L-7 (too many orders the system cancelled in the window) for any method, L-8 (the open unpaid
    /// ceiling) for the bank transfer only. Nothing is born and the answer is the neutral one - it names no number, no time and no axis (02 6.1.5); the limit is in
    /// <see cref="OrderConfirmationResult.ExceededLimit"/> for the log line only.
    /// </summary>
    AttemptLimitExceeded,

    /// <summary>The sales gate closed while the step was open (T509; 05 4.2.2 (1); 04 5.13.27): nothing is born and the cart stays.</summary>
    SalesClosed,

    /// <summary>A physical order's basis fell under the firm's minimum order amount (T509; 02 3.18.1; K-1229): nothing is born and the customer goes back to the cart.</summary>
    MinimumOrderAmountNotMet,

    /// <summary>
    /// An order was already born with the request's confirmation key (T509; 05 4.2.2 (0), 4.5.1; 07 2.5.1; K-1199): the transaction writes nothing and the customer goes to that order's
    /// confirmation screen - <see cref="OrderConfirmationResult.OrderNumber"/> and <see cref="OrderConfirmationResult.OrderAccessKey"/> are the existing order's.
    /// </summary>
    AlreadyPlaced,
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
    ConfirmationStepCheck? StepCheck,
    AttemptLimit? ExceededLimit,
    string? OrderAccessKey,
    bool IsGuestOrder) : IOperationResult
{
    public bool IsRejected => Refusal is not null;

    /// <summary>The order born: its number, its access key and whether it is a guest's, which pick the confirmation screen's root (07 2.1.3).</summary>
    public static OrderConfirmationResult Placed(Order order) => new(null, order.Id, order.OrderNumber, null, [], null, null, order.OrderAccessKey, order.IsGuestOrder);

    /// <summary>The order an earlier submission with the same key made (05 4.2.2 (0)); a refusal for the transaction, so nothing this request did is kept.</summary>
    public static OrderConfirmationResult Existing(Guid orderId, string orderNumber, string? accessKey, bool isGuestOrder) =>
        new(OrderConfirmationRefusal.AlreadyPlaced, orderId, orderNumber, null, [], null, null, accessKey, isGuestOrder);

    public static OrderConfirmationResult Refused(OrderConfirmationRefusal refusal, ConfirmationSummary? current = null, IReadOnlyList<SummaryChange>? changes = null, ConfirmationStepCheck? check = null) =>
        new(refusal, null, null, current, changes ?? [], check, null, null, false);

    /// <summary>The refusal of a limit (T377): the limit that stopped the order, for the log line; the user sees the neutral message only.</summary>
    public static OrderConfirmationResult LimitExceeded(AttemptLimit limit) =>
        new(OrderConfirmationRefusal.AttemptLimitExceeded, null, null, null, [], null, limit, null, false);
}

/// <summary>
/// The confirmation (T348; 02 3.17.1-3.17.7; 03 4.2.1): the one operation that makes an order. In the caller's transaction (09 9.1 item 3) it evaluates the cart again, compares the
/// result with the summary the customer confirmed - any difference of the total, its breakdown, the items, the shipping period or the delivery provinces stops it (02 3.17.2) -, checks
/// the confirmation step, cancels the cart's earlier unpaid order (02 3.17.7), and then in one go issues the number, freezes the items, the addresses, the identity, the versions, the
/// documents and the digital and service box records (02 3.23.3), starts the payment window,
/// reserves the stock, the capacity and the coupon right (02 3.17.3) and queues B-1, on a bank transfer B-2, and F-1 (02 9.2; 03 4.2.1). Nothing calls an outside service here: the card's redirect follows the commit.
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
    private readonly IPaymentMethods _paymentMethods;
    private readonly OrderPlacementLimits _limits;
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
        IPaymentMethods paymentMethods,
        OrderPlacementLimits limits,
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
        _paymentMethods = paymentMethods;
        _limits = limits;
        _time = time;
    }

    public async Task<OrderConfirmationResult> ConfirmAsync(OrderConfirmationRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        PaymentMethod method = request.ConfirmedSummary.PaymentMethod;

        // 02 3.17.2: the cart is evaluated again, under the cart's lock (Carts.OpenAsync takes it first). 02 3.17.7: between the lock and the read the cart's earlier
        // unpaid order is cancelled and its reservation freed, so the customer is not stopped by the stock or the coupon right it held; a refusal below rolls the
        // cancellation back with the rest. A merged guest cart's order carries the deleted guest cart's id and is not found (the exception of 3.17.7).
        // 05 4.2.2 (0) (T509; K-970, K-1199): under the cart's lock, before anything else, the confirmation key is looked up among the orders; an order already born with it ends the
        // transaction with nothing written - the earlier unpaid order the key made is not cancelled - and the customer goes to that order's confirmation screen.
        string confirmationKey = request.ConfirmationKey ?? SecretKey.Create();
        OrderConfirmationResult? existing = null;
        bool keyLookedUp = false;
        async Task AfterLockAsync(Guid lockedCartId, CancellationToken token)
        {
            keyLookedUp = true;
            existing = await FindByConfirmationKeyAsync(confirmationKey, token);
            if (existing is null)
            {
                await CancelEarlierUnpaidOrdersAsync(lockedCartId, token);
            }
        }

        ShippingFeeSection shippingTerms = await _settings.GetShippingFeeAsync(ct);
        CartView cart = await _carts.OpenAsync(request.Owner, shippingTerms, AfterLockAsync, ct);
        if (!keyLookedUp)
        {
            existing = await FindByConfirmationKeyAsync(confirmationKey, ct);
        }

        if (existing is not null)
        {
            return existing;
        }

        // 05 4.2.2 (1) (T509; 04 5.13.27): the sales gate is asked in the transaction; a sale closed while the step was open makes no order.
        if (!(await _settings.GetSalesGateAsync(ct)).Permits(SalesGateOperation.ConfirmOrder))
        {
            return OrderConfirmationResult.Refused(OrderConfirmationRefusal.SalesClosed);
        }

        List<CartLine> lines = [.. cart.OrderingLines];
        if (cart.CartId is not Guid cartId || lines.Count == 0)
        {
            return OrderConfirmationResult.Refused(OrderConfirmationRefusal.NoOrderableItem);
        }

        // K-1229 (T509; 02 3.18.1; 07 4.13.6): a physical cart whose basis fell under the minimum order amount goes back to the cart, which tells the missing amount.
        if (cart.PaymentBlock == CartPaymentBlock.MinimumOrderAmountNotMet)
        {
            return OrderConfirmationResult.Refused(OrderConfirmationRefusal.MinimumOrderAmountNotMet);
        }

        // 02 3.21.2, 3.21.4 (T352): only an open method makes an order - the gates come before the cart's evaluation (07 4.13.6). The card without the provider's keys is not
        // offered and not taken; the refusal rolls the earlier order's cancellation back with the rest, so the cart stays as it is (02 6.1.4).
        if (!(await _paymentMethods.ListOpenAsync(ct)).Contains(method))
        {
            return OrderConfirmationResult.Refused(OrderConfirmationRefusal.PaymentMethodUnavailable);
        }

        // 02 8.2.2, 8.2.3, 8.2.4 (T377): the two limits counted from the orders. They are asked after the cart's earlier unpaid order was cancelled above - that order is not one of the
        // open orders the ceiling counts any more and its cancellation is not L-7's (the same-cart reason) - and a refusal rolls that cancellation back with the rest. A guest is
        // counted on the IP axis only; a signed-in member on both; there is no exemption for any account (02 8.2.4).
        if (await _limits.FindExceededAsync(method, request.IpAddress, request.Owner.CustomerId, ct) is { } exceeded)
        {
            return OrderConfirmationResult.LimitExceeded(exceeded);
        }

        Evaluation current = await EvaluateAsync(lines, cart.Shipping, request.SummaryInput, null, ct);
        List<SummaryChange> changes = [.. SummaryComparison.Compare(request.ConfirmedSummary, current.Summary)];
        if (cart.PaymentBlock != CartPaymentBlock.None || current.CouponRefused)
        {
            changes.Add(new SummaryChange(SummaryChangeKind.Total, null, null, null));
        }

        // 02 3.17.2, 6.2.3: the delivery provinces are read again in this transaction - the same read the address check uses (T350; D-494). A physical order whose shipping
        // province is no longer delivered to, or that carries no shipping address, is not born; the delivery area restricts the shipping province only (02 3.20.1).
        if (current.HasPhysicalItem
            && (request.Shipping is not { } shippingAddress || !(await _settings.GetDeliveryAreaAsync(ct)).IsServed(shippingAddress.ProvinceId)))
        {
            changes.Add(new SummaryChange(SummaryChangeKind.DeliveryArea, null, null, null));
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
            confirmationKey,
            method,
            request.Owner.CustomerId is null,
            request.ContactEmail,
            current.Summary.Total,
            current.Summary.ShippingFee,
            window.EndsAt,
            now);

        // 02 3.13.3 (T509): a guest order whose e-mail belongs to a verified account is bound to that account at once and stays a guest order.
        order.CustomerId = request.Owner.CustomerId ?? request.BoundCustomerId;
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
        string formText = (await _settings.GetLegalTextVersionAsync(versions.PreInformationForm!.Value, ct))!.Text; // non-null: the confirmation step's check is Ready, so both versions exist and are published
        string contractText = (await _settings.GetLegalTextVersionAsync(versions.DistanceSalesContract!.Value, ct))!.Text; // non-null: the confirmation step's check is Ready, so both versions exist and are published
        (string form, string contract) = OrderDocuments.Produce(order, items, formText, contractText);
        order.FreezeDocuments(form, contract);

        _db.Orders.Add(order);
        _db.OrderItems.AddRange(items);

        // 02 3.17.1 -> 3.23.3, 3.24.6: the digital and the service box are frozen with the order - the box's type, the text shown and the items it covers (T356; D-732). The step's
        // check above is Ready, so every offered box was ticked; the offers are made from the same lines the step showed.
        foreach ((CheckboxAcceptance acceptance, IReadOnlyList<CheckboxAcceptanceOrderItem> covered) in ConfirmationAcceptances.Records(
            order.Id, ConfirmationStepRules.Offers(current.Summary.Lines, method), items, now))
        {
            _db.CheckboxAcceptances.Add(acceptance);
            _db.CheckboxAcceptanceOrderItems.AddRange(covered);
        }

        // 02 3.17.3: the stock, the capacity and the coupon right are reserved together; one that does not fit stops the order and its quantity is not told (02 3.17.2).
        if (!await _counters.ReserveAsync(order, items, ct))
        {
            return OrderConfirmationResult.Refused(
                OrderConfirmationRefusal.SummaryChanged, current.Summary, [new SummaryChange(SummaryChangeKind.ItemNotReserved, null, null, null)]);
        }

        // B-1 to the contact e-mail and F-1 to the firm, in the same transaction (02 3.17.1, 9.2; 08 2.2.6, 2.2.7).
        await _notifications.QueueCustomerNoticeAsync(new OrderReceivedNotice(order.Id), request.ContactEmail, ct);
        await _notifications.QueueCompanyNoticeAsync(new NewOrderNotice(order.Id, order.OrderNumber, method, order.Total, now), ct);

        // B-2 on a bank transfer order (02 9.2 B-2, 3.21.6; 03 4.2.1; D-770): the frozen IBAN, the order number and the last payment day, in the same transaction.
        if (method == PaymentMethod.BankTransfer)
        {
            await _notifications.QueueCustomerNoticeAsync(new TransferChosenNotice(order.Id, order.PaymentWindowEndsAt), request.ContactEmail, ct);
        }

        return OrderConfirmationResult.Placed(order);
    }

    private async Task<OrderConfirmationResult?> FindByConfirmationKeyAsync(string confirmationKey, CancellationToken ct)
    {
        var placed = await _db.Orders.AsNoTracking()
            .Where(o => o.ConfirmationKey == confirmationKey)
            .Select(o => new { o.Id, o.OrderNumber, o.OrderAccessKey, o.IsGuestOrder })
            .SingleOrDefaultAsync(ct);
        return placed is null ? null : OrderConfirmationResult.Existing(placed.Id, placed.OrderNumber, placed.OrderAccessKey, placed.IsGuestOrder);
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
        return lines.Count == 0 ? null : (await EvaluateAsync(lines, cart.Shipping, input, null, ct)).Summary;
    }

    /// <summary>
    /// The payment step's read (T509; 07 UC-22…UC-25): the cart under its lock, the way to the step (<see cref="CartView.PaymentBlock"/>), the summary as <see cref="SummarizeAsync"/> makes it, the
    /// coupon's verdict and the cart's earlier unpaid order the confirmation would cancel (K-837). <paramref name="appliesCoupon"/> is UC-23's application: the code is judged behind L-6 and a code
    /// that is not accepted keeps its attempt (07 4.13.2) - the only write of the read besides the cart's read marks; no coupon right is reserved.
    /// </summary>
    public async Task<CheckoutStepRead> ReadStepAsync(OrderSummaryInput input, bool appliesCoupon, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        CartView cart = await _carts.OpenAsync(input.Owner, await _settings.GetShippingFeeAsync(ct), ct);
        List<CartLine> lines = [.. cart.OrderingLines];
        if (cart.CartId is not Guid cartId || lines.Count == 0)
        {
            return new CheckoutStepRead(cart, null, null, null);
        }

        Evaluation evaluation = await EvaluateAsync(lines, cart.Shipping, input, appliesCoupon ? cartId : null, ct);
        string? earlier = await _db.Orders.AsNoTracking()
            .Where(o => o.CartId == cartId && o.OrderStatus == OrderStatus.Placed && o.PaymentStatus == PaymentStatus.Pending)
            .OrderBy(o => o.Id)
            .Select(o => o.OrderNumber)
            .FirstOrDefaultAsync(ct);
        return new CheckoutStepRead(cart, evaluation.Summary, evaluation.Coupon, earlier);
    }

    private async Task<Evaluation> EvaluateAsync(List<CartLine> lines, ShippingAssessment shipping, OrderSummaryInput request, Guid? countsCouponOnCart, CancellationToken ct)
    {
        PaymentMethod method = request.PaymentMethod;
        Dictionary<Guid, OrderItemSource> byVariant = (await _catalog.ListOrderItemSourcesAsync([.. lines.Select(l => l.VariantId)], ct)).ToDictionary(s => s.VariantId);
        List<OrderItemSource> sources = [.. lines.Select(l => byVariant[l.VariantId])];
        List<CouponLine> couponLines = [.. lines.Select((l, i) => new CouponLine(AppliedPrice(sources[i]), l.Quantity, sources[i].VatRate))];

        List<ItemAmounts> amounts;
        Guid? couponId = null;
        bool couponRefused = false;
        CouponApplication? application = null;
        if (string.IsNullOrWhiteSpace(request.CouponCode))
        {
            amounts = [.. couponLines.Select(c => _catalog.PriceItem(new ItemPriceInput(c.UnitPrice, c.Quantity, 0m, c.VatRate)))];
        }
        else
        {
            CouponApplication coupon = countsCouponOnCart is Guid cartId
                ? await _catalog.ApplyCouponAsync(cartId, request.CouponCode, couponLines, ct)
                : await _catalog.EvaluateCouponAsync(request.CouponCode, couponLines, ct);
            application = coupon;
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
        return new Evaluation(summary, sources, amounts, couponId, request.CouponCode, couponRefused, shipping.HasPhysicalItem, promise, application);
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
        int? ShippingPromise,
        CouponApplication? Coupon);
}

/// <summary>
/// The payment step's read (T509): the cart as it stands, the summary - empty when no line enters an order -, the coupon's verdict - empty without a code - and the number of the cart's earlier
/// unpaid order the confirmation would cancel (K-837).
/// </summary>
internal sealed record CheckoutStepRead(CartView Cart, ConfirmationSummary? Summary, CouponApplication? Coupon, string? EarlierUnpaidOrderNumber) : IOperationResult
{
    // The read writes the cart's marks and, on UC-23, the coupon's attempt: both are kept.
    public bool IsRejected => false;
}
