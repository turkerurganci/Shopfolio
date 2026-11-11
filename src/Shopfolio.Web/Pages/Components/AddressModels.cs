using Microsoft.AspNetCore.Mvc.ModelBinding;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Membership.Contracts;

namespace Shopfolio.Pages.Components;

/// <summary>The variants of the address form (04 2.12.2.2): the delivery address and the billing address of the order step, the book entry, and the administrator's correction of an order's address.</summary>
public enum AddressVariant
{
    /// <summary>E-13: recipient, province, district, address and the telephone; a province the company does not deliver to is refused (04 2.12.2.3).</summary>
    Delivery,

    /// <summary>E-13: the same four fields, no telephone (03 2.4.3; K-756).</summary>
    Billing,

    /// <summary>E-26: an address name besides the four fields; the telephone is optional; every listed province is kept (04 5.26.5).</summary>
    Book,

    /// <summary>E-37: the administrator corrects the shipping address and the telephone of an order (03 8.4.1; K-735).</summary>
    PanelDelivery,

    /// <summary>E-37: the administrator corrects the billing address of an order; no telephone.</summary>
    PanelBilling,
}

/// <summary>One province of the closed list; <paramref name="Served"/> is false for a province the company does not deliver to (02 3.20.1).</summary>
public sealed record ProvinceChoice(Guid Id, string Name, bool Served = true);

/// <summary>What an address form shows when it is drawn: the sent or the stored values of its fields.</summary>
/// <param name="Name">The address name (the book variant only).</param>
/// <param name="RecipientName">The recipient's name and surname.</param>
/// <param name="ProvinceId">The chosen province's id as text; empty when none.</param>
/// <param name="District">The district, free text (04 7.3.2.5).</param>
/// <param name="AddressLine">The open address, one text field.</param>
/// <param name="Phone">The telephone as typed or stored.</param>
public sealed record AddressValuesView(string? Name = null, string? RecipientName = null, string? ProvinceId = null, string? District = null, string? AddressLine = null, string? Phone = null);

/// <summary>
/// The address form (T543; 04 OB-13, 2.12.2): the fields of its variant, in the order they are drawn. Every variant has the four fields of <c>02 3.14.6</c> - the recipient's name and surname, the province
/// from a closed list, the district and the open address in one text field -; the delivery variant adds the telephone, the book variant the address name and an optional telephone. There is no postal
/// code and no national identity number field in any variant (04 2.12.2.1). The names of the fields are the members of <c>AddressBookInput</c> after the screen's prefix, so the one rule home
/// (<c>AddressFieldRules</c>) reads what this form sends.
/// </summary>
public sealed class AddressFormView
{
    internal AddressFormView(AddressVariant variant, string namePrefix, IReadOnlyList<FieldView> fields, string? sameAsShippingName, bool sameAsShippingChecked)
    {
        Variant = variant;
        NamePrefix = namePrefix;
        Fields = fields;
        SameAsShippingName = sameAsShippingName;
        SameAsShippingChecked = sameAsShippingChecked;
    }

    public AddressVariant Variant { get; }

    /// <summary>The text before every field name (<c>Input.Shipping</c> gives <c>Input.ShippingRecipientName</c>).</summary>
    public string NamePrefix { get; }

    /// <summary>The fields in the order they are drawn.</summary>
    public IReadOnlyList<FieldView> Fields { get; }

    /// <summary>The name of the "teslimat adresimle aynı" box; null where the variant has none (04 2.12.2.2: only the billing form of a basket with a physical item).</summary>
    public string? SameAsShippingName { get; }

    /// <summary>The box comes ticked (04 2.12.2.2).</summary>
    public bool SameAsShippingChecked { get; }

    /// <summary>True when the form has the box.</summary>
    public bool OffersSameAsShipping => SameAsShippingName is not null;

    /// <summary>The element id of the box.</summary>
    public string? SameAsShippingId => SameAsShippingName is null ? null : FormErrorSummary.FieldIdOf(SameAsShippingName);

    /// <summary>The label of the box.</summary>
    public const string SameAsShippingLabel = "Teslimat adresimle aynı";

    /// <summary>The names of the form's fields in the order the form draws them: the order the error summary puts the focus by (04 2.3.2).</summary>
    public IReadOnlyList<string> FieldOrder => [.. Fields.Select(f => f.Name)];

    /// <summary>The same form with the summary bound to its fields (the messages under the fields, the autofocus on the first wrong one).</summary>
    public AddressFormView Bind(FormErrorSummary? summary) => new(Variant, NamePrefix, global::Shopfolio.Pages.Components.Fields.BindAll(Fields, summary), SameAsShippingName, SameAsShippingChecked);
}

/// <summary>The only constructors of an <see cref="AddressFormView"/> and the one home of its labels (T543; 04 2.12.2).</summary>
public static class AddressForms
{
    /// <summary>The members the form's names end with; <c>AddressBookInput</c>'s members (<c>AddressFieldRules</c> reads them).</summary>
    public static class Members
    {
        public const string Name = nameof(AddressBookInput.Name);
        public const string RecipientName = nameof(AddressBookInput.RecipientName);
        public const string ProvinceId = nameof(AddressBookInput.ProvinceId);
        public const string District = nameof(AddressBookInput.District);
        public const string AddressLine = nameof(AddressBookInput.AddressLine);
        public const string Phone = nameof(AddressBookInput.Phone);
    }

    /// <summary>The visible labels of the fields.</summary>
    public static class Labels
    {
        public const string Name = "Adres adı";
        public const string RecipientName = "Alıcının adı ve soyadı";
        public const string Province = "İl";
        public const string District = "İlçe";
        public const string AddressLine = "Açık adres";
        public const string Phone = "Telefon";
    }

    /// <summary>
    /// The form of a variant (04 2.12.2.2). <paramref name="namePrefix"/> is the text before each member name; <paramref name="provinces"/> is the whole closed list - the unserved provinces stay in it, the
    /// delivery variant refuses them with their reason at the field and the book variant keeps them (04 2.12.2.3, 5.26.5).
    /// </summary>
    /// <param name="offerSameAsShipping">Only the billing variant of a basket with a physical item: the "teslimat adresimle aynı" box, ticked when it comes; a basket without a physical item has no box and shows the form at once (04 2.12.2.2, 5.13.7).</param>
    public static AddressFormView Create(
        AddressVariant variant,
        string namePrefix,
        IReadOnlyList<ProvinceChoice> provinces,
        AddressValuesView? values = null,
        bool offerSameAsShipping = false,
        string? sameAsShippingName = null,
        bool sameAsShippingChecked = true)
    {
        ArgumentNullException.ThrowIfNull(namePrefix);
        ArgumentNullException.ThrowIfNull(provinces);
        if (!Enum.IsDefined(variant))
        {
            throw new ArgumentOutOfRangeException(nameof(variant), variant, "Not an address variant.");
        }

        if (offerSameAsShipping && variant != AddressVariant.Billing)
        {
            throw new ArgumentException("Only the billing form of the order step offers \"same as the shipping address\" (04 2.12.2.2).", nameof(offerSameAsShipping));
        }

        if (offerSameAsShipping)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sameAsShippingName);
        }

        AddressValuesView shown = values ?? new AddressValuesView();
        FieldOption[] options = [.. provinces.Select(p => new FieldOption(p.Id.ToString("D"), p.Name))];
        List<FieldView> fields = [];
        if (variant == AddressVariant.Book)
        {
            fields.Add(Fields.Text(namePrefix + Members.Name, Labels.Name, FieldRequirement.Required, shown.Name));
        }

        fields.Add(Fields.Text(namePrefix + Members.RecipientName, Labels.RecipientName, FieldRequirement.Required, shown.RecipientName, autocomplete: "name"));
        fields.Add(Fields.ClosedList(namePrefix + Members.ProvinceId, Labels.Province, FieldRequirement.Required, options, shown.ProvinceId));
        fields.Add(Fields.Text(namePrefix + Members.District, Labels.District, FieldRequirement.Required, shown.District, autocomplete: "address-level2"));
        fields.Add(Fields.TextArea(namePrefix + Members.AddressLine, Labels.AddressLine, FieldRequirement.Required, shown.AddressLine, autocomplete: "street-address"));
        if (AsksPhone(variant))
        {
            fields.Add(Fields.Telephone(namePrefix + Members.Phone, Labels.Phone, PhoneRequirementOf(variant), shown.Phone));
        }

        return new AddressFormView(variant, namePrefix, fields, offerSameAsShipping ? sameAsShippingName : null, offerSameAsShipping && sameAsShippingChecked);
    }

    /// <summary>True when the variant has a telephone field: the delivery address and its panel correction, and the book entry; the billing address has none (04 2.12.2.1, 2.12.2.2).</summary>
    public static bool AsksPhone(AddressVariant variant) => variant is AddressVariant.Delivery or AddressVariant.Book or AddressVariant.PanelDelivery;

    /// <summary>The telephone is required on the delivery address and optional on the book entry (04 2.12.2.2; 02 3.14.6).</summary>
    public static FieldRequirement PhoneRequirementOf(AddressVariant variant) => variant is AddressVariant.Book ? FieldRequirement.Optional : FieldRequirement.Required;

    /// <summary>
    /// What the province rule says of the chosen province (04 2.12.2.3): the delivery variant refuses a province the company does not deliver to and says why at the field; the book keeps every listed
    /// province - the restriction works at the order step -; the billing address and the panel's corrections are never restricted by delivery (07 UC-27). An id that is not on the list is not a choice at all.
    /// </summary>
    public static ProvinceVerdict CheckProvince(AddressVariant variant, Guid? provinceId, IReadOnlyList<ProvinceChoice> provinces)
    {
        ArgumentNullException.ThrowIfNull(provinces);
        if (provinceId is not { } id || id == Guid.Empty)
        {
            return ProvinceVerdict.Missing;
        }

        ProvinceChoice? province = provinces.FirstOrDefault(p => p.Id == id);
        if (province is null)
        {
            return ProvinceVerdict.NotOnTheList;
        }

        return variant == AddressVariant.Delivery && !province.Served
            ? ProvinceVerdict.NotDelivered(AddressMessages.NotDelivered(province.Name))
            : ProvinceVerdict.Accepted;
    }

    /// <summary>
    /// Adds the field refusals of the membership module to the model state as the texts the form shows (04 2.3.2): each code is turned into its text by the message catalog with the field's label; a
    /// refused delivery province says its reason from the province's name (04 5.13.23; K-772). The names the refusals carry are the form's: <paramref name="namePrefix"/> comes before the member.
    /// </summary>
    public static void AddRefusals(ModelStateDictionary modelState, string namePrefix, IEnumerable<MembershipFieldError> refusals)
    {
        ArgumentNullException.ThrowIfNull(modelState);
        ArgumentNullException.ThrowIfNull(namePrefix);
        ArgumentNullException.ThrowIfNull(refusals);
        foreach (MembershipFieldError refusal in refusals)
        {
            string text = refusal.Code == ErrorCode.DeliveryProvinceNotServed
                ? AddressMessages.NotDelivered(refusal.Detail ?? string.Empty)
                : FieldMessageCatalog.Build(refusal.Code.ToString(), LabelOf(refusal.Field), null);
            modelState.AddModelError(namePrefix + MemberOf(refusal.Field), text);
        }
    }

    // The refusal's field is the name the module read: the prefix of the order step ("Shipping") or none, then the member.
    private static string MemberOf(string field)
    {
        foreach (string member in new[] { Members.RecipientName, Members.ProvinceId, Members.District, Members.AddressLine, Members.Phone, Members.Name })
        {
            if (field.EndsWith(member, StringComparison.Ordinal))
            {
                return member;
            }
        }

        return field;
    }

    private static string LabelOf(string field) => MemberOf(field) switch
    {
        Members.RecipientName => Labels.RecipientName,
        Members.ProvinceId => Labels.Province,
        Members.District => Labels.District,
        Members.AddressLine => Labels.AddressLine,
        Members.Phone => Labels.Phone,
        Members.Name => Labels.Name,
        _ => field,
    };
}

/// <summary>The answer of the province rule (04 2.12.2.3): taken, missing, not on the list, or refused because the company does not deliver there - with the reason to say at the field.</summary>
public sealed record ProvinceVerdict(bool IsAccepted, string? Reason, bool IsMissing = false, bool IsNotOnTheList = false)
{
    public static ProvinceVerdict Accepted { get; } = new(true, null);

    public static ProvinceVerdict Missing { get; } = new(false, null, IsMissing: true);

    public static ProvinceVerdict NotOnTheList { get; } = new(false, null, IsNotOnTheList: true);

    public static ProvinceVerdict NotDelivered(string reason) => new(false, reason);
}

/// <summary>The sentences the address form says in place (T543; 04 2.12.2.3, 5.13.23; K-772) - one home, so the refusal at the form and the line beside a book entry are one text.</summary>
public static class AddressMessages
{
    /// <summary>"Bu firma Ankara iline teslimat yapmıyor." (K-772: the source's example is the final text).</summary>
    public static string NotDelivered(string provinceName)
    {
        ArgumentNullException.ThrowIfNull(provinceName);
        return provinceName.Length == 0
            ? "Bu firma seçtiğiniz ile teslimat yapmıyor."
            : $"Bu firma {provinceName} iline teslimat yapmıyor.";
    }
}

/// <summary>
/// A book entry as the order step offers it (T543; 04 2.12.2.3, 5.13.6, 5.26.5): selectable, or shown with its reason beside it when the entry's province is one the company does not deliver to - the entry
/// stays in the book, the restriction works at the order step. An entry without a telephone is selectable and asks the telephone on the step (04 2.12.2.2).
/// </summary>
public sealed record AddressBookChoice(Guid Id, string Name, bool Selectable, string? Reason, bool AsksPhone)
{
    public static AddressBookChoice From(AddressBookEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return new AddressBookChoice(
            entry.Id,
            entry.Name,
            entry.IsDeliverable,
            entry.IsDeliverable ? null : AddressMessages.NotDelivered(entry.ProvinceName),
            string.IsNullOrEmpty(entry.Phone));
    }
}
