using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Membership.Contracts;

/// <summary>
/// The address book form's six values (07 5.3; 04 7.1.16; 02 3.14.6): the name of the entry and the four fields of an address - the recipient's name, the province from the closed
/// list, the district as free text and the open address - and the telephone, which is optional here. There is no postal code and no national identity number field (02 3.14.6).
/// </summary>
public sealed record AddressBookInput(string? Name, string? RecipientName, Guid? ProvinceId, string? District, string? AddressLine, string? Phone);

/// <summary>An entry of a member's address book as the book screen and the payment step show it; the phone is the ten stored digits or empty.</summary>
public sealed record AddressBookEntry(
    Guid Id,
    string Name,
    string RecipientName,
    Guid ProvinceId,
    string ProvinceName,
    string District,
    string AddressLine,
    string? Phone,
    DateTimeOffset CreatedAt);

/// <summary>
/// The outcome of an address book entry's save (07 UC-71, UC-72): saved with the entry's id, refused for fields (with the field errors), or no such entry - the entry is not this
/// member's or is gone, and the answer is the same (07 2.3.3).
/// </summary>
public sealed class AddressBookSaveResult : IOperationResult
{
    private static readonly MembershipFieldError[] NoFields = [];

    private AddressBookSaveResult(Guid? entryId, bool isNotFound, IReadOnlyList<MembershipFieldError> fields)
    {
        EntryId = entryId;
        IsNotFound = isNotFound;
        Fields = fields;
    }

    public bool IsSaved => EntryId is not null;

    /// <summary>The id of the saved entry; empty unless saved.</summary>
    public Guid? EntryId { get; }

    /// <summary>The entry is not the member's, or is gone (404).</summary>
    public bool IsNotFound { get; }

    /// <summary>The refused fields; empty unless the fields were refused.</summary>
    public IReadOnlyList<MembershipFieldError> Fields { get; }

    public bool IsRejected => !IsSaved;

    internal static AddressBookSaveResult Saved(Guid entryId) => new(entryId, false, NoFields);

    internal static AddressBookSaveResult NotFound() => new(null, true, NoFields);

    internal static AddressBookSaveResult FieldsRefused(IReadOnlyList<MembershipFieldError> fields) => new(null, false, fields);
}

/// <summary>The outcome of the member's deletion of an entry (07 UC-73): deleted, or not found - the entry is not this member's or is gone (404; a repeat finds nothing and writes nothing).</summary>
public sealed class AddressBookDeleteResult : IOperationResult
{
    private AddressBookDeleteResult(bool isDeleted)
    {
        IsDeleted = isDeleted;
    }

    public bool IsDeleted { get; }

    public bool IsNotFound => !IsDeleted;

    public bool IsRejected => !IsDeleted;

    internal static AddressBookDeleteResult Deleted() => new(true);

    internal static AddressBookDeleteResult NotFound() => new(false);
}

/// <summary>
/// The addresses a basket asks for (02 3.14.4): by the types of its items. An order with at least one physical item asks for a shipping and a billing address, and the billing
/// address may be the shipping one; an order with only digital and service items shows no shipping field at all and asks for the billing address.
/// </summary>
public sealed record AddressRequirement(bool AsksShipping, bool AsksBilling, bool OffersBillingSameAsShipping);

/// <summary>
/// What the payment step's address part sends (07 5.3 <c>PlaceOrderInput</c>; 04 7.1.5.2-7.1.5.6): the signed-in member's id (empty for a guest), the types of the basket's items, the
/// shipping address - a book entry's id or the four fields typed - with the shipping telephone, the member's choice to keep a typed address in the book (and the name to keep it
/// under), and the billing address or the "same as shipping" box. Field names are the form's, so a refusal names the field the form shows.
/// </summary>
public sealed record OrderAddressRequest(
    Guid? CustomerId,
    IReadOnlyCollection<ProductType> ItemTypes,
    Guid? AddressBookId,
    string? ShippingRecipientName,
    Guid? ShippingProvinceId,
    string? ShippingDistrict,
    string? ShippingAddressLine,
    string? ShippingPhone,
    bool SaveToAddressBook,
    string? SavedAddressName,
    bool IsBillingSameAsShipping,
    string? BillingRecipientName,
    Guid? BillingProvinceId,
    string? BillingDistrict,
    string? BillingAddressLine);

/// <summary>An order's frozen shipping address (02 3.14.5, 3.14.6): the four fields and the telephone, which a shipping address always carries; ten digits, no leading zero.</summary>
public sealed record ShippingAddressSnapshot(string RecipientName, string Phone, Guid ProvinceId, string District, string AddressLine);

/// <summary>An order's frozen billing address (02 3.14.6): the four fields only - a billing address carries no telephone.</summary>
public sealed record BillingAddressSnapshot(string RecipientName, Guid ProvinceId, string District, string AddressLine);

/// <summary>
/// The outcome of the address part of an order (02 3.14.1-3.14.6; 07 UC-27): resolved with the values to freeze - the shipping snapshot is empty when the basket has no physical item - and,
/// when the member chose to keep a typed address, the id of the book entry written in the caller's transaction; or refused for fields (all of them together), or because the chosen
/// book entry is not this member's (404). A refusal writes nothing.
/// </summary>
public sealed class OrderAddressResolution : IOperationResult
{
    private static readonly MembershipFieldError[] NoFields = [];

    private OrderAddressResolution(
        bool isResolved,
        ShippingAddressSnapshot? shipping,
        BillingAddressSnapshot? billing,
        Guid? newBookEntryId,
        bool isBookEntryNotFound,
        IReadOnlyList<MembershipFieldError> fields)
    {
        IsResolved = isResolved;
        Shipping = shipping;
        Billing = billing;
        NewBookEntryId = newBookEntryId;
        IsBookEntryNotFound = isBookEntryNotFound;
        Fields = fields;
    }

    public bool IsResolved { get; }

    /// <summary>The shipping address to freeze; empty when the basket has no physical item (or when refused).</summary>
    public ShippingAddressSnapshot? Shipping { get; }

    /// <summary>The billing address to freeze; empty only when refused.</summary>
    public BillingAddressSnapshot? Billing { get; }

    /// <summary>The id of the entry written to the book because the member chose to keep the typed address; empty otherwise.</summary>
    public Guid? NewBookEntryId { get; }

    /// <summary>The chosen book entry is not the member's, the member has no book (a guest), or the entry is gone: 404 (07 2.3.3).</summary>
    public bool IsBookEntryNotFound { get; }

    /// <summary>The refused fields, named as the form's; empty unless the fields were refused.</summary>
    public IReadOnlyList<MembershipFieldError> Fields { get; }

    /// <summary>The refusal's first code; empty when resolved.</summary>
    public ErrorCode? Code => IsResolved ? null : IsBookEntryNotFound ? ErrorCode.NotFound : Fields[0].Code;

    public bool IsRejected => !IsResolved;

    internal static OrderAddressResolution Resolved(ShippingAddressSnapshot? shipping, BillingAddressSnapshot billing, Guid? newBookEntryId) =>
        new(true, shipping, billing, newBookEntryId, false, NoFields);

    internal static OrderAddressResolution BookEntryNotFound() => new(false, null, null, null, true, NoFields);

    internal static OrderAddressResolution FieldsRefused(IReadOnlyList<MembershipFieldError> fields) => new(false, null, null, null, false, fields);
}
