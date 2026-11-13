using System.Net;

using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Membership.Contracts.Results;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The payment step's open surface (T509; 07 UC-22…UC-27): the read of the step and the confirmation, over <see cref="OrderConfirmation"/>. It joins the caller's transaction and opens none
/// (09 6 item 3). The member's e-mail is the account's verified one; the account and the addresses are the membership module's to resolve - its address part writes a typed address to the book
/// only when the member chose it (K-680), and the confirmation's refusal rolls that write back with the rest.
/// </summary>
internal sealed class Checkout : ICheckout
{
    private readonly OrderConfirmation _confirmation;
    private readonly IMembershipModule _membership;
    private readonly Carts _carts;
    private readonly OrderPlacementLimits _limits;

    public Checkout(OrderConfirmation confirmation, IMembershipModule membership, Carts carts, OrderPlacementLimits limits)
    {
        _confirmation = confirmation;
        _membership = membership;
        _carts = carts;
        _limits = limits;
    }

    public Task MarkPaymentUnavailableAsync(CartOwner owner, CancellationToken ct) => _carts.MarkPaymentUnavailableAsync(owner, ct);

    public async Task<bool> ClosesBankTransferAsync(CartOwner owner, IPAddress? address, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(owner);
        Guid? cartId = await _carts.FindCartIdAsync(owner, ct);
        return await _limits.ClosesBankTransferAsync(address, owner.CustomerId, cartId, ct);
    }

    public async Task<CheckoutStep> ReadAsync(CheckoutInput input, bool appliesCoupon, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        string email = await ContactEmailOfAsync(input.Owner, input.ContactEmail, ct) ?? string.Empty;
        CheckoutStepRead read = await _confirmation.ReadStepAsync(
            new OrderSummaryInput(input.Owner, input.PaymentMethod, input.CouponCode, email, input.Shipping, input.Billing), appliesCoupon, ct);
        bool hasPhysical = read.Cart.OrderingLines.Any(l => l.ProductType == ProductType.Physical);
        return new CheckoutStep(read.Cart.PaymentBlock, read.Summary, read.Coupon, read.EarlierUnpaidOrderNumber, hasPhysical, read.Cart.Shipping);
    }

    public async Task<PlaceOrderResult> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 07 4.13.6: the account and the fields come before the order's work (422/404); the order's transaction is the confirmation's own sequence (05 4.2.2).
        if (await ContactEmailOfAsync(request.Owner, request.GuestEmail, ct) is not { Length: > 0 } email)
        {
            return PlaceOrderResult.Without(PlaceOrderOutcome.AccountNotAccepted);
        }

        OrderAccountResolution account = await _membership.ResolveOrderAccountAsync(request.Owner.CustomerId, email, ct);
        if (!account.IsAccepted)
        {
            return PlaceOrderResult.Without(PlaceOrderOutcome.AccountNotAccepted);
        }

        OrderAddressResolution addresses = await _membership.ResolveOrderAddressesAsync(request.Addresses with { CustomerId = request.Owner.CustomerId }, ct);
        if (addresses.IsBookEntryNotFound)
        {
            return PlaceOrderResult.Without(PlaceOrderOutcome.AddressBookEntryNotFound);
        }

        if (!addresses.IsResolved || addresses.Billing is not { } billing)
        {
            return PlaceOrderResult.Without(PlaceOrderOutcome.AddressRefused) with { AddressFields = addresses.Fields };
        }

        PartyAddress? shipping = addresses.Shipping is { } s ? new PartyAddress(s.RecipientName, s.Phone, s.ProvinceId, s.District, s.AddressLine) : null;
        OrderConfirmationResult result = await _confirmation.ConfirmAsync(
            new OrderConfirmationRequest(
                request.Owner,
                request.ConfirmedSummary,
                request.ShownPreInformationFormId,
                request.ShownDistanceSalesContractId,
                request.Ticked,
                request.CouponCode,
                email,
                shipping,
                new PartyAddress(billing.RecipientName, null, billing.ProvinceId, billing.District, billing.AddressLine),
                request.IpAddress,
                request.ConfirmationKey,
                account.Kind == OrderAccountKind.GuestBoundToAccount ? account.CustomerId : null),
            ct);
        return ResultOf(result);
    }

    private static PlaceOrderResult ResultOf(OrderConfirmationResult result) => result.Refusal switch
    {
        null => new PlaceOrderResult(PlaceOrderOutcome.Placed, result.OrderNumber, result.OrderAccessKey, result.IsGuestOrder),
        OrderConfirmationRefusal.AlreadyPlaced => new PlaceOrderResult(PlaceOrderOutcome.AlreadyPlaced, result.OrderNumber, result.OrderAccessKey, result.IsGuestOrder),
        OrderConfirmationRefusal.NoOrderableItem => PlaceOrderResult.Without(PlaceOrderOutcome.NoOrderableItem),
        OrderConfirmationRefusal.MinimumOrderAmountNotMet => PlaceOrderResult.Without(PlaceOrderOutcome.MinimumOrderAmountNotMet),
        OrderConfirmationRefusal.SalesClosed => PlaceOrderResult.Without(PlaceOrderOutcome.SalesClosed),
        OrderConfirmationRefusal.PaymentMethodUnavailable => PlaceOrderResult.Without(PlaceOrderOutcome.PaymentMethodUnavailable),
        OrderConfirmationRefusal.AttemptLimitExceeded => PlaceOrderResult.Without(PlaceOrderOutcome.AttemptLimitExceeded) with { ExceededLimit = result.ExceededLimit },
        OrderConfirmationRefusal.ConfirmationStepNotReady => PlaceOrderResult.Without(PlaceOrderOutcome.ConfirmationStepNotReady) with { CurrentSummary = result.CurrentSummary, StepCheck = result.StepCheck },
        _ => PlaceOrderResult.Without(PlaceOrderOutcome.SummaryChanged) with { CurrentSummary = result.CurrentSummary, Changes = result.Changes },
    };

    // A member's contact e-mail is the account's verified one (04 5.13.5); a guest's is the one written.
    private async Task<string?> ContactEmailOfAsync(CartOwner owner, string? written, CancellationToken ct) =>
        owner.CustomerId is Guid customerId ? await _membership.GetVerifiedEmailAsync(customerId, ct) : written?.Trim();
}
