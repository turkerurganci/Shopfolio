using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Pages.Components;

namespace Shopfolio.IntegrationTests.Pages;

/// <summary>
/// A storefront page that draws every component of T543 with the real shared parts (04 OB-12 to OB-15): the field with its label, requirement text and message, a form saved by sections, the address
/// form of the delivery variant, the IBAN field of a place, the reason selection and the limit message. The page decides nothing the components decide: the closed list, the trimming and the limits are
/// the components' and the binder's.
/// </summary>
public sealed class FormSampleModel : PageModel
{
    public const string Path = "/sss/form";

    public static readonly Guid ServedProvince = new("00000000-0000-0000-0000-0000000000a1");

    public static readonly Guid ClosedProvince = new("00000000-0000-0000-0000-0000000000a2");

    public static IReadOnlyList<ProvinceChoice> Provinces { get; } =
    [
        new ProvinceChoice(ServedProvince, "İstanbul"),
        new ProvinceChoice(ClosedProvince, "Ankara", Served: false),
    ];

    public static IReadOnlyList<FieldOption> Topics { get; } = [new FieldOption("Order", "Sipariş"), new FieldOption("Product", "Ürün")];

    public FormSampleInput Input { get; set; } = new();

    public string? SavedSection { get; private set; }

    public string? SavedName { get; private set; }

    public string? SavedTopic { get; private set; }

    public bool ReasonAccepted { get; private set; }

    public string? IbanMethod { get; private set; }

    public string? IbanOpeningName { get; private set; }

    public string? IbanPlaceName { get; private set; }

    public bool IbanProcessed { get; private set; }

    public IReadOnlyList<string> FormOrder { get; } =
    [
        "Input.Name",
        "Input.Topic",
        "Input.RecipientName",
        "Input.ProvinceId",
        "Input.District",
        "Input.AddressLine",
        "Input.Phone",
        "Input.Reason",
        "Input.Explanation",
    ];

    public IActionResult OnGet(string? method, string? opening, string? place, bool processed = false)
    {
        IbanMethod = method;
        IbanOpeningName = opening;
        IbanPlaceName = place;
        IbanProcessed = processed;
        return Page();
    }

    // A form with a free text field, a closed list and the product's character limit: the sent values stay (04 2.12.1.2) and the errors are the page filter's texts of the codes.
    public IActionResult OnPostSave(FormSampleInput input)
    {
        Input = input;
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            ModelState.AddFieldError("Input.Name", FieldCodes.FieldRequired);
        }
        else if (input.Name.Length > ProductConstants.ProductNameMaxLength)
        {
            ModelState.AddFieldError("Input.Name", FieldCodes.TextTooLong, ProductConstants.ProductNameMaxLength.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        if (!ClosedLists.Accepts(Topics, input.Topic) || string.IsNullOrWhiteSpace(input.Topic))
        {
            ModelState.AddFieldError("Input.Topic", string.IsNullOrWhiteSpace(input.Topic) ? FieldCodes.FieldRequired : FieldCodes.InvalidListValue);
        }

        if (ModelState.IsValid)
        {
            SavedName = input.Name;
            SavedTopic = input.Topic;
            return Page();
        }

        return FormResults.Redisplay(this);
    }

    public IActionResult OnPostSaveBrand(FormSampleInput input)
    {
        Input = input;
        SavedSection = "Brand";
        return Page();
    }

    public IActionResult OnPostSaveSocial(FormSampleInput input)
    {
        Input = input;
        SavedSection = "Social";
        return Page();
    }

    // The delivery address: a province the company does not deliver to is refused at the field with its reason (04 2.12.2.3).
    public IActionResult OnPostAddress(FormSampleInput input)
    {
        Input = input;
        Guid? provinceId = Guid.TryParse(input.ProvinceId, out Guid parsed) ? parsed : null;
        ProvinceVerdict verdict = AddressForms.CheckProvince(AddressVariant.Delivery, provinceId, Provinces);
        if (verdict.IsMissing)
        {
            ModelState.AddFieldError("Input.ProvinceId", FieldCodes.FieldRequired);
        }
        else if (verdict.IsNotOnTheList)
        {
            ModelState.AddFieldError("Input.ProvinceId", FieldCodes.InvalidListValue);
        }
        else if (!verdict.IsAccepted)
        {
            ModelState.AddModelError("Input.ProvinceId", verdict.Reason ?? string.Empty);
        }

        return ModelState.IsValid ? FormResults.SeeOther(Path) : FormResults.Redisplay(this);
    }

    // The firm's cancellation: no confirmation opens without a reason (04 2.12.4.1).
    public IActionResult OnPostCancel(FormSampleInput input)
    {
        Input = input;
        ReasonCheck check = Reasons.Check(ReasonKind.FirmCancellation, input.Reason, input.Explanation);
        if (!check.MayContinue)
        {
            check.AddTo(ModelState, "Input.");
            return FormResults.Redisplay(this);
        }

        ReasonAccepted = true;
        return Page();
    }

    // The attempt limit's refusal: only this action is refused, the neutral sentence beside its button, the values kept (04 7.3.3, 7.3.4).
    public IActionResult OnPostLimit(FormSampleInput input)
    {
        Input = input;
        ModelState.AddModelError(string.Empty, NeutralMessages.LimitExceeded);
        return FormResults.Redisplay(this);
    }

    /// <summary>The IBAN field of the place the query names, or none.</summary>
    public IbanFieldView? Iban()
    {
        if (!Enum.TryParse(IbanMethod, out PaymentMethod method) || !Enum.TryParse(IbanPlaceName, out IbanFieldPlace place))
        {
            return null;
        }

        _ = Enum.TryParse(IbanOpeningName, out IbanOpening opening);
        return IbanFields.Create(place, method, opening, "Input.Iban", "TR000000000000000000000000", IbanProcessed);
    }
}

/// <summary>The form of the form sample.</summary>
public sealed class FormSampleInput : IFormInput
{
    public string? Name { get; set; }

    public string? Topic { get; set; }

    public string? Brand { get; set; }

    public string? Social { get; set; }

    public string? RecipientName { get; set; }

    public string? ProvinceId { get; set; }

    public string? District { get; set; }

    public string? AddressLine { get; set; }

    public string? Phone { get; set; }

    public string? Reason { get; set; }

    public string? Explanation { get; set; }

    public string? Iban { get; set; }
}
