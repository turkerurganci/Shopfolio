using System.ComponentModel.DataAnnotations;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Membership.Contracts;

namespace Shopfolio.Modules.Membership;

/// <summary>The four fields every address has (02 3.14.6), as stored: trimmed texts and the province's id.</summary>
internal sealed record AddressValues(string RecipientName, Guid ProvinceId, string District, string AddressLine);

/// <summary>
/// The one home of the address rules (02 3.14.1-3.14.6; 04 7.3.2.4-7.3.2.6): the book entry's form, the order's shipping address and its billing address all go through here, so the
/// four required fields, the closed province list and the telephone are one rule - not three. The telephone is the project's <see cref="InvalidPhoneAttribute"/> and the required field
/// is <see cref="FieldRequiredAttribute"/>; the province is checked against the list the Settings module gives. An address has no postal code and no national identity number.
/// </summary>
internal static class AddressFieldRules
{
    /// <summary>The form's field-name prefix of an order's shipping address (07 5.3 <c>PlaceOrderInput</c>).</summary>
    public const string ShippingPrefix = "Shipping";

    /// <summary>The form's field-name prefix of an order's billing address.</summary>
    public const string BillingPrefix = "Billing";

    /// <summary>The address book form has no prefix: its fields are named as <see cref="AddressBookInput"/>'s.</summary>
    public const string BookPrefix = "";

    private static readonly FieldRequiredAttribute RequiredRule = new();
    private static readonly InvalidPhoneAttribute PhoneRule = new();

    /// <summary>
    /// What an order asks for by the types of its items (02 3.14.4): one physical item is enough to ask the shipping address, and then the billing address may be the same; a basket
    /// of digital and service items only asks the billing address alone. A basket with no item has no order.
    /// </summary>
    public static AddressRequirement RequirementOf(IReadOnlyCollection<ProductType> itemTypes)
    {
        ArgumentNullException.ThrowIfNull(itemTypes);
        if (itemTypes.Count == 0)
        {
            throw new ArgumentException("An order has at least one item.", nameof(itemTypes));
        }

        foreach (ProductType type in itemTypes)
        {
            if (!Enum.IsDefined(type))
            {
                throw new ArgumentOutOfRangeException(nameof(itemTypes), type, "Not a product type.");
            }
        }

        bool physical = itemTypes.Contains(ProductType.Physical);
        return new AddressRequirement(AsksShipping: physical, AsksBilling: true, OffersBillingSameAsShipping: physical);
    }

    /// <summary>A text as stored (06 2.8.5): white space around it is dropped and an empty text is nothing.</summary>
    public static string? Text(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>
    /// Checks the four fields and returns them as stored, or <see langword="null"/> after adding every refused field to <paramref name="errors"/>: all four are required
    /// (<c>FieldRequired</c>), and the province must be one of <paramref name="provinceIds"/> (<c>InvalidProvince</c>) - the district is free text.
    /// </summary>
    public static AddressValues? CheckAddress(
        string prefix,
        string? recipientName,
        Guid? provinceId,
        string? district,
        string? addressLine,
        IReadOnlySet<Guid> provinceIds,
        List<MembershipFieldError> errors)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        ArgumentNullException.ThrowIfNull(provinceIds);
        ArgumentNullException.ThrowIfNull(errors);
        int before = errors.Count;
        string? recipient = RequiredText(prefix + nameof(AddressBookInput.RecipientName), recipientName, errors);
        if (provinceId is not { } province || province == Guid.Empty)
        {
            errors.Add(new MembershipFieldError(prefix + nameof(AddressBookInput.ProvinceId), ErrorCode.FieldRequired));
        }
        else if (!provinceIds.Contains(province))
        {
            errors.Add(new MembershipFieldError(prefix + nameof(AddressBookInput.ProvinceId), ErrorCode.InvalidProvince));
        }

        string? districtText = RequiredText(prefix + nameof(AddressBookInput.District), district, errors);
        string? line = RequiredText(prefix + nameof(AddressBookInput.AddressLine), addressLine, errors);
        if (errors.Count > before || recipient is null || districtText is null || line is null)
        {
            return null;
        }

        return new AddressValues(recipient, provinceId!.Value, districtText, line); // non-null: a missing province adds an error, refused by the errors.Count check above
    }

    /// <summary>
    /// The telephone as stored: the ten digits, with a leading "0" or "+90" and the separators dropped (04 7.3.2.4; 06 2.5.1). An empty value is <see langword="null"/> when
    /// <paramref name="required"/> is false and a <c>FieldRequired</c> refusal when it is true; a value that is no Turkish number is <c>InvalidPhone</c>. Returns false after
    /// adding a refusal.
    /// </summary>
    public static bool TryStorePhone(string field, string? raw, bool required, List<MembershipFieldError> errors, out string? stored)
    {
        ArgumentNullException.ThrowIfNull(field);
        ArgumentNullException.ThrowIfNull(errors);
        stored = null;
        string? text = Text(raw);
        if (text is null)
        {
            if (required)
            {
                errors.Add(new MembershipFieldError(field, ErrorCode.FieldRequired));
                return false;
            }

            return true;
        }

        if (PhoneRule.GetValidationResult(text, new ValidationContext(text)) is not null)
        {
            errors.Add(new MembershipFieldError(field, ErrorCode.InvalidPhone));
            return false;
        }

        stored = RequestFormat.CompactPhone(text);
        return true;
    }

    private static string? RequiredText(string field, string? value, List<MembershipFieldError> errors)
    {
        string text = value ?? string.Empty;
        if (RequiredRule.GetValidationResult(text, new ValidationContext(text)) is not null)
        {
            errors.Add(new MembershipFieldError(field, ErrorCode.FieldRequired));
            return null;
        }

        return text.Trim();
    }
}
