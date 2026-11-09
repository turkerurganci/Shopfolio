using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The withdrawal declaration of the transition unit (T372; 03 1.7.1.5, 1.11.5, 1.11.6, 1.11.34; 02 7.3.5-7.3.7, 10.4.10, 8.3.8). Both ways - the customer's from the order
/// page and the administrator's record of a notice by another channel - work in the unit's order (09 9.1 item 4): the order's row lock, the screen's version (the panel's),
/// the operation decision's verdict on the locked order (09 8.3 item 3; the same rule the button is drawn from), the AfterSales writer (the record, its amounts, the return
/// address and the IBAN are that module's), the counters of 1.4.4.6, the closure rule at the end, the order's version raised once, then the notices. The declaration changes
/// no axis by itself (02 5.6.6, 7.3.6): on an item without its delivery mark the withdrawn units are closed by the record, and when no open item is left the closure rule
/// ends the line - the one way, never the cancellation (D-306); the payment axis changes with the refund. A physical item not shipped yet has no withdrawal: its way is
/// the cancellation, and the firm processes another channel's notice with the "müşteriyle anlaşıldı" cancellation (02 7.3.7). A declaration is never undone or corrected
/// (02 7.6.7): no method here or elsewhere changes a written one.
/// </summary>
internal sealed partial class OrderTransitions
{
    private static readonly InvalidIbanAttribute IbanRule = new();

    public async Task<WithdrawalResult> DeclareWithdrawalAsync(DeclareWithdrawalRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        // The last step's confirmation (02 5.9; K-732): the declaration is irreversible and never done with one touch.
        if (!request.Confirmed)
        {
            return WithdrawalRefused(TransitionRefusal.ConfirmationRequired);
        }

        string? enteredIban = null;
        if (!string.IsNullOrWhiteSpace(request.Iban))
        {
            if (!IbanRule.IsValid(request.Iban))
            {
                return WithdrawalRefused(TransitionRefusal.InvalidInput);
            }

            enteredIban = RequestFormat.CompactIban(request.Iban);
        }

        (Order? order, OrderItem? item, TransitionRefusal? locked) = await LockItemAsync(request.OrderId, request.OrderItemId, null, ct);
        if (locked is { } lockRefusal)
        {
            return WithdrawalRefused(lockRefusal);
        }

        // The decision of 1.11.5 / 1.11.6 on the locked order: shipped physical item or paid service item, the window open (Z-13, Z-14 - D-513), no absolute exemption,
        // the units at most the open ones. A digital item has no withdrawal (02 7.3.2).
        if (OrderOperations.WithdrawalOperation(item!.ProductType) is not { } operation)
        {
            return WithdrawalRefused(TransitionRefusal.ConditionNotMet);
        }

        (OperationVerdict verdict, _) = await _operations.CheckWithFactsAsync(order!.Id, operation, item.Id, request.Quantity, ct);
        if (VerdictRefusal(verdict) is { } closed)
        {
            return WithdrawalRefused(closed);
        }

        // The refund IBAN of the transfer line is entered with the declaration, or the one on file is used (02 7.3.5, 7.4.5); the card line takes none.
        WithdrawalIban iban;
        if (order.PaymentMethod == PaymentMethod.BankTransfer)
        {
            iban = enteredIban is not null ? new WithdrawalIban(enteredIban, false, false) : new WithdrawalIban(null, true, false);
        }
        else if (enteredIban is null)
        {
            iban = WithdrawalIban.None;
        }
        else
        {
            return WithdrawalRefused(TransitionRefusal.InvalidInput);
        }

        // The date stamp is the moment the declaration is sent (02 7.3.5; K-1565): nothing else is asked - no form, e-mail or notary (K-207).
        DateTimeOffset now = _time.GetUtcNow();
        WithdrawalDeclaration declaration = new(Subject(order, item), request.Quantity, null, TurkeyTime.DateOf(now), null, Frozen(item), iban, now);
        return await WriteWithdrawalAsync(order, item, declaration, TransitionActor.Buyer, WithdrawalNoticeWarnings.None, ct);
    }

    public async Task<WithdrawalResult> RecordWithdrawalAsync(RecordWithdrawalRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryAdminActor(request.Admin, out TransitionActor? actor))
        {
            return WithdrawalRefused(TransitionRefusal.ActorNotAllowed);
        }

        // The record is irreversible and asks the panel's confirmation (02 5.9; K-715).
        if (!request.Confirmed)
        {
            return WithdrawalRefused(TransitionRefusal.ConfirmationRequired);
        }

        // The date is the day the notice reached the firm: it may be in the past, never in the future (02 10.4.10; K-709).
        DateTimeOffset now = _time.GetUtcNow();
        if (request.NoticeDate > TurkeyTime.DateOf(now))
        {
            return WithdrawalRefused(TransitionRefusal.InvalidInput);
        }

        (Order? order, OrderItem? item, TransitionRefusal? locked) = await LockItemAsync(request.OrderId, request.OrderItemId, request.ExpectedVersion, ct);
        if (locked is { } lockRefusal)
        {
            return WithdrawalRefused(lockRefusal);
        }

        // 1.11.34 on the locked order: a physical item once the order has shipped (before it the notice is a cancellation - 02 7.3.7), a paid service item; the window is
        // not a condition - a record outside it is the firm's to confirm (K-709).
        (OperationVerdict verdict, OrderOperationFacts? facts) =
            await _operations.CheckWithFactsAsync(order!.Id, OrderOperation.RecordWithdrawalFromOtherChannel, item!.Id, request.Quantity, ct);
        if (VerdictRefusal(verdict) is { } closed)
        {
            return WithdrawalRefused(closed);
        }

        OperationItemFacts itemFacts = facts!.Items.Single(i => i.ItemId == item.Id);
        WithdrawalNoticeWarnings warnings = WithdrawalNoticeRules.Warnings(itemFacts, request.NoticeDate);

        // On the item's delivery day the firm answers whether the delivery came first (K-608); on any other day the answer is not kept.
        if (warnings.AsksDeliveryOrder && request.IsDeliveredBeforeDeclaration is null)
        {
            return new WithdrawalResult(TransitionRefusal.InvalidInput, null, warnings, null);
        }

        if (warnings.NeedsConfirmation && !request.WarningsConfirmed)
        {
            return new WithdrawalResult(TransitionRefusal.ConfirmationRequired, null, warnings, null);
        }

        // The IBAN of a notice is carried over only when the notice came from the order's contact e-mail and the IBAN passes the order page's check; otherwise the record is
        // made without one, the item gets an IBAN request and B-14 goes (02 8.3.8, 10.4.10; K-607, K-581). The card line takes none.
        WithdrawalIban iban = WithdrawalIban.None;
        if (order.PaymentMethod == PaymentMethod.BankTransfer)
        {
            iban = request.CameFromOrderEmail && !string.IsNullOrWhiteSpace(request.NoticeIban) && IbanRule.IsValid(request.NoticeIban)
                ? new WithdrawalIban(RequestFormat.CompactIban(request.NoticeIban), false, false)
                : new WithdrawalIban(null, false, true);
        }

        WithdrawalDeclaration declaration = new(
            Subject(order, item),
            request.Quantity,
            new ItemRecordAdmin(request.Admin.AdminId, request.Admin.AdminName.Trim()),
            request.NoticeDate,
            warnings.AsksDeliveryOrder ? request.IsDeliveredBeforeDeclaration : null,
            Frozen(item),
            iban,
            now);
        return await WriteWithdrawalAsync(order, item, declaration, actor!, warnings, ct);
    }

    /// <summary>The common core after the checks: the writer, the counters, the closure rule, the version and the notices.</summary>
    private async Task<WithdrawalResult> WriteWithdrawalAsync(
        Order order,
        OrderItem item,
        WithdrawalDeclaration declaration,
        TransitionActor actor,
        WithdrawalNoticeWarnings warnings,
        CancellationToken ct)
    {
        ItemRecordWriteResult written = await _afterSales.AddWithdrawalAsync(declaration, ct);
        if (written.Refusal is { } refused)
        {
            return new WithdrawalResult(refused == ItemRecordRefusal.IbanMissing ? TransitionRefusal.InvalidInput : Map(refused), null, warnings, null);
        }

        // 1.4.4.6: the withdrawn service capacity goes back with the declaration; a physical item's units go back at the goods' receipt (T373). The coupon right goes back
        // when the record finishes the order (06 4.14; D-453).
        List<VariantQuantity> units = item.ProductType == ProductType.Service && item.VariantId is { } variant ? [new VariantQuantity(variant, declaration.Quantity)] : [];
        await _counters.ReturnAsync(order, units, ct);

        // The closure rule reads the record just added: an item without its delivery mark closes its withdrawn units, and the last open item ends the line (02 5.6.6).
        ClosureResult closure = await EvaluateClosureAsync(order, actor, ct);
        RaiseVersionOnce(order);

        // B-9 to the customer on both ways; F-3 to the firm only on the customer's declaration - the firm's own record is not told to it (02 9.2, 9.3.2; 03 7.3.24).
        NoticeItem withdrawn = new(item.ProductName, item.FirstOptionValue, item.SecondOptionValue, declaration.Quantity);
        NoticeAddress? address = written.ReturnAddress is { } a ? new NoticeAddress(a.RecipientName, a.AddressLine, a.District, a.ProvinceId, a.Phone) : null;
        await QueueNoticeAsync(
            order.Id,
            new DeclarationReceivedNotice(order.Id, DispatchRelatedRecordType.Withdrawal, written.RecordId!.Value, false, declaration.DeclarationDate, null, [withdrawn], address),
            ct);
        if (declaration.Admin is null)
        {
            await _notifications.QueueCompanyNoticeAsync(
                new NewClaimNotice(DispatchRelatedRecordType.Withdrawal, written.RecordId.Value, order.Id, order.OrderNumber, false, declaration.At), ct);
        }
        else if (declaration.Iban.Request)
        {
            await QueueNoticeAsync(order.Id, new RefundIbanRequestedNotice(order.Id, RefundIbanRequestReason.NoticeWithoutIban), ct);
        }

        return new WithdrawalResult(null, written.RecordId, warnings, closure.Code);
    }

    /// <summary>The operation decision's verdict as the unit's refusal (07 6.2.7): a quantity above the open units is a wrong value, a closed operation its condition.</summary>
    private static TransitionRefusal? VerdictRefusal(OperationVerdict verdict) => verdict.Decision switch
    {
        OperationDecision.Allowed => null,
        OperationDecision.QuantityExceedsOpen => TransitionRefusal.InvalidInput,
        _ => TransitionRefusal.ConditionNotMet,
    };

    /// <summary>The item's frozen money values the declaration's amounts are split from (02 7.1.7 (1)): the rounded discounted price when the item was discounted.</summary>
    private static ItemFrozenAmounts Frozen(OrderItem item) => new(item.DiscountedUnitPrice ?? item.UnitPrice, item.CouponShare, item.VatAmount);

    private static WithdrawalResult WithdrawalRefused(TransitionRefusal refusal) => new(refusal, null, WithdrawalNoticeWarnings.None, null);
}
