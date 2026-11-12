using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts.Results;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Storefront;

/// <summary>
/// The contact page and the form's send (E-10; T506; 07 UC-14, UC-15; 04 5.10): the GET composes the legal identity block, the branches and the form - open or closed by the gate of the data-collecting
/// entries - and, for a signed-in member, starts the form with the account's name and e-mail, which the member may change (02 3.32.1). The POST hands the form to the module in one transaction and
/// maps its result: a sent request or a send the trap field dropped both end in 303 to <c>/iletisim</c> with the <c>ContactRequestSent</c> code, so the two are the same to the visitor; a closed gate
/// is the closed page in 422; the refused fields are shown together in 422 with the sent values; the limit L-4 is 429 with the neutral message beside the button and the sent values kept. The page
/// puts no validation attribute on the input: the module owns the order of gate, trap, fields and limit, and an attribute would answer a closed gate or a filled trap differently.
/// </summary>
public sealed class ContactModel : PageModel
{
    /// <summary>The form's fields in the order the page draws them: the summary lists the wrong ones in this order (04 2.3.2).</summary>
    public static IReadOnlyList<string> FormOrder { get; } =
    [
        FieldName(nameof(SendContactRequestInput.Name)),
        FieldName(nameof(SendContactRequestInput.Email)),
        FieldName(nameof(SendContactRequestInput.Phone)),
        FieldName(nameof(SendContactRequestInput.ContactTopic)),
        FieldName(nameof(SendContactRequestInput.Message)),
    ];

    private readonly ICorporateContentModule _content;
    private readonly IMembershipModule _membership;

    public ContactModel(ICorporateContentModule content, IMembershipModule membership)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(membership);
        _content = content;
        _membership = membership;
    }

    /// <summary>The values of the form: the member's starting values on the first read, the sent values after a refused send.</summary>
    public SendContactRequestInput Input { get; private set; } = new();

    /// <summary>The identity block and the branches; the administrator's preview adds the draft branches.</summary>
    public ContactPageView PageContent { get; private set; } = new(new LegalIdentity(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null), []);

    /// <summary>The form's state: open or closed by the gate.</summary>
    public ContactFormView Form { get; private set; } = new(false, null, [], string.Empty, string.Empty, LegalLinkScreen.ContactForm);

    /// <summary>True when the address carries <c>ContactRequestSent</c>: the form's place shows the state after the send (04 5.10.8).</summary>
    public bool ShowsSentState { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        ShowsSentState = NoticeParameter.Read(Request, NoticeKind.Result) == NoticeCode.ContactRequestSent;
        await LoadAsync(null, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostSendContactRequestAsync(SendContactRequestInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);

        ITransactionRunner transactions = HttpContext.RequestServices.GetRequiredService<ITransactionRunner>();
        ContactFormInput form = new(input.Name, input.Email, input.Phone, input.ContactTopic, input.Message, input.Website);
        System.Net.IPAddress? address = ClientAddress.Of(HttpContext);
        ContactFormResult result = await transactions.RunAsync(token => _content.SendContactRequestAsync(form, address, token), ct);

        if (result.ShowsSentState)
        {
            return FormResults.SeeOther(FramePaths.Contact, NoticeCode.ContactRequestSent);
        }

        await LoadAsync(input, ct);
        switch (result.Outcome)
        {
            case ContactFormOutcome.FieldsRefused:
                foreach (ContactFormFieldError field in result.Fields)
                {
                    ModelState.AddFieldError(FieldName(field.Field), field.Code.ToString());
                }

                return FormResults.Redisplay(this);
            case ContactFormOutcome.LimitExceeded:
                return FormResults.AttemptLimitExceeded(this, ErrorCode.ContactRequestLimitExceeded);
            default:
                // The gate is closed: the page answers with its closed state (04 5.10.12) and nothing of the form; the one rejection line keeps the gate's code (09 8.2 item 3; K-1198).
                HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(result.Code ?? ErrorCode.DataCollectionClosed);
                return FormResults.Redisplay(this);
        }
    }

    /// <summary>The name of a form field as the page sends and the summary lists it: <c>Input.Name</c>.</summary>
    public static string FieldName(string property) => nameof(Input) + "." + property;

    private async Task LoadAsync(SendContactRequestInput? sent, CancellationToken ct)
    {
        bool asAdministrator = await HttpContext.HasAdminSessionAsync();
        PageContent = await _content.GetContactPageAsync(asAdministrator, ct);

        MemberContact? member = null;
        if (HttpContext.User.Identity is { IsAuthenticated: true, AuthenticationType: SessionSchemes.CustomerScheme } && HttpContext.User.AccountId() is Guid customerId)
        {
            member = await _membership.GetMemberContactAsync(customerId, ct);
        }

        Form = await _content.GetContactFormAsync(member?.Name, member?.Email, ct);
        Input = sent ?? new SendContactRequestInput { Name = Form.Name, Email = Form.Email };
    }
}

/// <summary>The form of the <c>SendContactRequest</c> handler (07 5.2.1; 02 3.32.2): name, e-mail, telephone, topic, message and the invisible trap field.</summary>
public sealed class SendContactRequestInput : IFormInput
{
    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? ContactTopic { get; set; }

    public string? Message { get; set; }

    /// <summary>The trap field (02 3.32.7): invisible to a visitor and filled by a bot; the module drops a send that fills it.</summary>
    [SpecAssignedName]
    public string? Website { get; set; }
}
