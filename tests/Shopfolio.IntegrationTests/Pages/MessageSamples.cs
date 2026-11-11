using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Pages;

/// <summary>
/// A storefront page that draws every component of T541 with the real shared parts (04 OB-03, OB-04, OB-07, OB-08): the four messages, the once-told and short-lived places, a form that is sent again with
/// its errors (422), the neutral limit message, the customer's confirmation step, the two empty states, the loading state and the closed-sales sentences. The page's own text is one marked line.
/// </summary>
public sealed class MessageSampleModel : PageModel
{
    public const string Path = "/sss/mesaj";

    public MessageSampleInput Input { get; set; } = new();

    public IReadOnlyList<string> FormOrder { get; } = ["Input.Name", "Input.Sku", "Input.Note"];

    public bool SalesClosed { get; private set; }

    public bool DataClosed { get; private set; }

    public ConfirmView CustomerStep { get; } = Confirmations.ForCustomer(
        "Hesabı sil",
        "Hesabınız ve kayıtlı adresleriniz silinir.",
        "Hesabı sil",
        Path,
        "Delete",
        "/hesabim",
        [new KeyValuePair<string, string>("Confirmed", "true")]);

    public IActionResult OnGet(bool closed = false, bool dataClosed = false)
    {
        SalesClosed = closed;
        DataClosed = dataClosed;
        return Page();
    }

    // A form with two required fields: the sent values stay and the errors are the page filter's texts of the codes.
    public IActionResult OnPostSave(MessageSampleInput input)
    {
        Input = input;
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            ModelState.AddFieldError("Input.Name", FieldCodes.FieldRequired);
        }

        if (string.IsNullOrWhiteSpace(input.Sku))
        {
            ModelState.AddFieldError("Input.Sku", FieldCodes.FieldRequired);
        }

        return ModelState.IsValid ? FormResults.SeeOther(Path) : FormResults.Redisplay(this);
    }

    // The attempt limit's refusal: the neutral sentence at the level of the action, beside the button, the values kept.
    public IActionResult OnPostLimit(MessageSampleInput input)
    {
        Input = input;
        ModelState.AddModelError(string.Empty, NeutralMessages.LimitExceeded);
        return FormResults.Redisplay(this);
    }

    public IActionResult OnPostDelete() => FormResults.SeeOther(Request.Path.ToUriComponent());
}

/// <summary>The panel twin: the administrator's confirmation window, the panel's block message and the four places of the sales state (04 2.3.3, 2.4.4, 2.8.2).</summary>
public sealed class PanelMessageSampleModel : PageModel
{
    public const string Path = "/panel/mesaj-sss";

    public PanelSalesStatus Status { get; private set; } = PanelSalesStatus.From(new SalesGate([], []));

    public ConfirmView Window { get; } = Confirmations.ForIrreversible(
        IrreversibleConfirmations.Shipment(false, false),
        "Kargoya ver",
        "Kargoya ver",
        Path,
        "Ship",
        "/panel/siparisler",
        [new KeyValuePair<string, string>("Id", "7")]);

    public IActionResult OnGet(string? unmet)
    {
        SalesGateCondition[] conditions = [.. (unmet ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Enum.Parse<SalesGateCondition>)];
        Status = PanelSalesStatus.From(new SalesGate(conditions, []));
        return Page();
    }

    public IActionResult OnPostShip() => FormResults.SeeOther(Request.Path.ToUriComponent());
}

/// <summary>The form of the message sample.</summary>
public sealed class MessageSampleInput : IFormInput
{
    public string? Name { get; set; }

    public string? Sku { get; set; }

    public string? Note { get; set; }
}
