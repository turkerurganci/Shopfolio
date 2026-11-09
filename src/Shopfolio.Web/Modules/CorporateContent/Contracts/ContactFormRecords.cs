using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Modules.CorporateContent.Contracts.Results;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.CorporateContent.Contracts;

/// <summary>
/// The contact form's values as the visitor sent them (02 3.32.2; 07 UC-15 <c>SendContactRequestInput</c>): the name, the e-mail, the optional phone, the topic by its code name, the message, and the
/// invisible trap field (<see cref="ContactFormFields.TrapFieldName"/>). There is no file, no order number field and no marketing consent here (02 3.32.2). The topic is text so that a value
/// outside the closed list is refused by the rule and not lost in the binding (04 5.10.10).
/// </summary>
public sealed record ContactFormInput(string? Name, string? Email, string? Phone, string? ContactTopic, string? Message, string? Website);

/// <summary>The names of the contact form's fields (07 5.2 <c>SendContactRequestInput</c>), so the page, the rule and the tests spell them one way.</summary>
public static class ContactFormFields
{
    public const string Name = nameof(ContactFormInput.Name);

    public const string Email = nameof(ContactFormInput.Email);

    public const string Phone = nameof(ContactFormInput.Phone);

    public const string ContactTopic = nameof(ContactFormInput.ContactTopic);

    public const string Message = nameof(ContactFormInput.Message);

    /// <summary>The invisible trap field (02 3.32.7): a person never sees it, a bot fills it, and a send with it filled is dropped without a word.</summary>
    public const string TrapFieldName = nameof(ContactFormInput.Website);
}

/// <summary>One refused field of the contact form: the field's name as the input's and the code of 07 6.2.2.</summary>
public sealed record ContactFormFieldError(string Field, ErrorCode Code);

/// <summary>
/// The result of a send of the contact form (02 3.32; 07 UC-15). The visitor is told the same thing for <see cref="ContactFormOutcome.Sent"/> and
/// <see cref="ContactFormOutcome.DroppedSilently"/> (<see cref="ShowsSentState"/>); a refusal is a return value (09 9.1 item 3).
/// </summary>
public sealed class ContactFormResult : IOperationResult
{
    private static readonly ContactFormFieldError[] NoFields = [];

    private ContactFormResult(ContactFormOutcome outcome, ErrorCode? code, IReadOnlyList<ContactFormFieldError> fields, GateRefusal? gate)
    {
        Outcome = outcome;
        Code = code;
        Fields = fields;
        Gate = gate;
    }

    /// <summary>What became of the send. The result carries no request number: the visitor is never shown one (02 3.32.6).</summary>
    public ContactFormOutcome Outcome { get; }

    /// <summary>The refusal's code; empty for a send that shows the sent state.</summary>
    public ErrorCode? Code { get; }

    /// <summary>The refused fields; empty unless the outcome is <see cref="ContactFormOutcome.FieldsRefused"/>.</summary>
    public IReadOnlyList<ContactFormFieldError> Fields { get; }

    /// <summary>The gate's refusal with the conditions that wait; filled only for <see cref="ContactFormOutcome.GateClosed"/>.</summary>
    public GateRefusal? Gate { get; }

    /// <summary>True when the page shows the state of the sent form: the request was written, or the trap dropped it (the two are the same to the visitor).</summary>
    public bool ShowsSentState => Outcome is ContactFormOutcome.Sent or ContactFormOutcome.DroppedSilently;

    public bool IsRejected => !ShowsSentState;

    internal static ContactFormResult Sent() => new(ContactFormOutcome.Sent, null, NoFields, null);

    internal static ContactFormResult Dropped() => new(ContactFormOutcome.DroppedSilently, null, NoFields, null);

    internal static ContactFormResult GateClosed(GateRefusal gate) => new(ContactFormOutcome.GateClosed, gate.Code, NoFields, gate);

    internal static ContactFormResult FieldsRefused(IReadOnlyList<ContactFormFieldError> fields) => new(ContactFormOutcome.FieldsRefused, fields[0].Code, fields, null);

    internal static ContactFormResult LimitExceeded() => new(ContactFormOutcome.LimitExceeded, ErrorCode.ContactRequestLimitExceeded, NoFields, null);
}

/// <summary>
/// The contact form as the page shows it (02 3.32.1, 3.32.3, 3.32.8): whether it is open, the gate's refusal when it is not, the five topics in the order of the list, and the values
/// the form starts with - the signed-in member's name and e-mail, which the member may change; empty for a visitor. The privacy notice link shows on the open form
/// (<see cref="PrivacyNoticeScreen"/>). The form has no file field and no order number field.
/// </summary>
public sealed record ContactFormView(bool IsOpen, GateRefusal? Refusal, IReadOnlyList<ContactTopic> Topics, string Name, string Email, LegalLinkScreen PrivacyNoticeScreen);
