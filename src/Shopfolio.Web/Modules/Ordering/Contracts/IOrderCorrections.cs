using Shopfolio.Infrastructure;
using Shopfolio.Modules.Membership.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The administrator's corrections of an order's contact data (T375; 02 7.6.6, 10.4.1, 10.4.2, 10.4.11, 8.3.6, 8.3.7): the address and the guest order's e-mail. Both are forward records
/// that touch no amount, no item, no status and no period; each runs in the caller's transaction, after the order's row lock, and raises the order's version once (09 9.1). The panel
/// calls them; the customer has no way to (02 8.3.6, 8.3.7). A refusal writes nothing.
/// </summary>
public interface IOrderCorrections
{
    /// <summary>
    /// The correction of the shipping or the billing address (02 10.4.2, 8.3.7; 03 1.11.23): open while the operation decision says so - a shipping address until "Kargoya verildi", the billing
    /// address of an order with a physical item the same, the billing address of an order with none until "Teslim edildi" - and only when the request says how the firm knows it came from
    /// the order's owner (the order's e-mail, or the firm's own return to the order's e-mail or shipping telephone; the number the request came from is no confirmation). The fields are the
    /// address rules' (<c>IMembershipModule.ResolveOrderAddressesAsync</c>: four required fields, the closed province list, the delivery area for a shipping province, the telephone). The old and the
    /// new value stay on the order's own record (<c>OrderAddressHistory</c>), B-10 goes to the order's e-mail, the trail gets a row without values.
    /// </summary>
    Task<OrderCorrectionResult> CorrectAddressAsync(CorrectAddressRequest request, CancellationToken ct);

    /// <summary>
    /// The correction of a guest order's contact e-mail (02 10.4.11, 8.3.6; 03 1.11.35): only a guest order whose personal data is not disposed, and only with the firm's confirmation of the
    /// buyer by the name, the shipping telephone of an order with a physical item, the items and the amount - the order number alone is no confirmation. The new address becomes the contact
    /// e-mail, the old one stays in <c>OrderEmailHistory</c>, the access key is renewed (the old links open nothing), the account link follows the new address (a verified account of it takes the
    /// order, an account deleted earlier never does), unsent notices go to the new address, B-1 goes to the new address and B-15 to the old one; the trail gets a row without values.
    /// </summary>
    Task<OrderCorrectionResult> CorrectGuestEmailAsync(CorrectGuestEmailRequest request, CancellationToken ct);
}

/// <summary>How the firm knows an address correction was asked by the order's owner (02 10.4.2, 8.3.7). The product does not check it; the proof is the firm's (02 12.5).</summary>
public enum AddressRequestConfirmation
{
    /// <summary>Nothing says the request is the owner's: a request from another channel that the firm has not confirmed through the order's own channels.</summary>
    None,

    /// <summary>The request came from the order's contact e-mail.</summary>
    FromOrderEmail,

    /// <summary>The request came from another channel; the firm wrote to the order's contact e-mail and the owner confirmed.</summary>
    ConfirmedByEmailToOrder,

    /// <summary>The request came from another channel; the firm called the order's shipping telephone and the owner confirmed.</summary>
    ConfirmedByCallToOrderPhone,
}

/// <summary>The address correction's input: the four address fields and, on a shipping address, the telephone (the billing address has none).</summary>
public sealed record CorrectAddressRequest(
    Guid OrderId,
    TransitionAdmin? Admin,
    AddressType AddressType,
    string? RecipientName,
    Guid? ProvinceId,
    string? District,
    string? AddressLine,
    string? Phone,
    AddressRequestConfirmation Confirmation,
    int? ExpectedVersion);

/// <summary>What the firm checked of the buyer before it corrects a guest order's e-mail (02 8.3.6): the name, the shipping telephone (asked only when the order has a physical item), the items and the amount.</summary>
public sealed record GuestIdentityConfirmation(bool Name, bool ShippingPhone, bool Items, bool Amount);

/// <summary>The e-mail correction's input: the new address and the firm's confirmation.</summary>
public sealed record CorrectGuestEmailRequest(Guid OrderId, TransitionAdmin? Admin, string? NewEmail, GuestIdentityConfirmation? Confirmation, int? ExpectedVersion);

/// <summary>
/// The outcome of a correction: applied, or refused with a reason (<see cref="TransitionRefusal"/>; the address rules' refused fields too).
/// <see cref="TransitionRefusal.ConfirmationRequired"/> is the missing confirmation, <see cref="TransitionRefusal.ConditionNotMet"/> a closed operation, <see cref="TransitionRefusal.InvalidInput"/> a value.
/// </summary>
public sealed record OrderCorrectionResult(TransitionRefusal? Refusal, IReadOnlyList<MembershipFieldError> Fields, Guid? CorrectionRecordId) : IOperationResult
{
    public bool IsRejected => Refusal is not null;

    /// <summary>The account the order fell to by the e-mail correction; empty when it stays a guest order.</summary>
    public Guid? LinkedCustomerId { get; init; }
}
