using System.ComponentModel.DataAnnotations;
using System.Net;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// The send of the contact form (T365; 02 3.32; 07 UC-15), in the order the contract gives it: the gate of the data-collecting entries first - a closed gate reads no field and
/// counts no attempt (02 3.32.8) -; then the invisible trap field - a filled one drops the send without a word, writes nothing and counts nothing (02 3.32.7) -; then the fields,
/// every refusal together and none of them counted (04 5.10.10); then the limit L-4 on the visitor's address, counted on every request and never forgotten (02 8.2; 09 15.4 item 5);
/// then the request is written open and the firm's notice F-2 with it (<see cref="ContactRequests.OpenAsync"/>). Nothing is sent to the sender: no number, no acknowledgement e-mail,
/// no page to follow the request on (02 3.32.6). Every send is a request of its own - a second send is a second request and counts for L-4 (07 4.10.2). The method joins the caller's
/// transaction and never opens one (09 9.1 item 3).
/// </summary>
internal sealed class ContactFormSending
{
    private static readonly InvalidEmailAttribute EmailRule = new();
    private static readonly InvalidPhoneAttribute PhoneRule = new();

    private readonly AppDbContext _db;
    private readonly ISettingsModule _settings;
    private readonly AttemptCounter _attempts;
    private readonly ContactRequests _requests;

    public ContactFormSending(AppDbContext db, ISettingsModule settings, AttemptCounter attempts, ContactRequests requests)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(attempts);
        ArgumentNullException.ThrowIfNull(requests);
        _db = db;
        _settings = settings;
        _attempts = attempts;
        _requests = requests;
    }

    /// <summary>The five topics in the order of the list (02 3.32.3): the enum is the closed list, the firm does not edit it, and the defect claim is not in it.</summary>
    public static IReadOnlyList<ContactTopic> Topics { get; } = Enum.GetValues<ContactTopic>();

    /// <summary>
    /// The form as the page shows it: open or closed by the gate, the topics, and the starting values of a signed-in member - the member's name and e-mail, which can be changed - or empty
    /// values for a visitor (02 3.32.1). A plain read; the form is open to a visitor, a guest buyer and a member alike.
    /// </summary>
    public async Task<ContactFormView> ReadAsync(string? memberName, string? memberEmail, CancellationToken ct)
    {
        SalesGate gate = await _settings.GetSalesGateAsync(ct);
        GateRefusal? refusal = gate.RefusalOf(DataCollectionEntry.ContactForm);
        return new ContactFormView(refusal is null, refusal, Topics, memberName?.Trim() ?? string.Empty, memberEmail?.Trim() ?? string.Empty, LegalLinkScreen.ContactForm);
    }

    /// <summary>Sends the form; see the class summary for the order. <paramref name="ipAddress"/> is the client address the reverse proxy forwarded (05 6.3.3).</summary>
    public async Task<ContactFormResult> SendAsync(ContactFormInput input, IPAddress? ipAddress, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The contact request is written only inside a transaction (09 6 item 3).");
        }

        SalesGate gate = await _settings.GetSalesGateAsync(ct);
        GateRefusal? refusal = gate.RefusalOf(DataCollectionEntry.ContactForm);
        if (refusal is not null)
        {
            return ContactFormResult.GateClosed(refusal);
        }

        if (!string.IsNullOrWhiteSpace(input.Website))
        {
            return ContactFormResult.Dropped();
        }

        string name = (input.Name ?? string.Empty).Trim();
        string email = (input.Email ?? string.Empty).Trim();
        string message = (input.Message ?? string.Empty).Trim();
        string? phone = RequestFormat.StoredPhone(input.Phone);
        List<ContactFormFieldError> fields = [];
        Require(fields, ContactFormFields.Name, name);
        if (email.Length == 0)
        {
            fields.Add(new ContactFormFieldError(ContactFormFields.Email, ErrorCode.FieldRequired));
        }
        else if (EmailRule.GetValidationResult(email, new ValidationContext(email)) is not null)
        {
            fields.Add(new ContactFormFieldError(ContactFormFields.Email, ErrorCode.InvalidEmail));
        }

        if (phone is not null && PhoneRule.GetValidationResult(phone, new ValidationContext(phone)) is not null)
        {
            fields.Add(new ContactFormFieldError(ContactFormFields.Phone, ErrorCode.InvalidPhone));
        }

        ContactTopic? topic = TopicOf(input.ContactTopic);
        if (string.IsNullOrWhiteSpace(input.ContactTopic))
        {
            fields.Add(new ContactFormFieldError(ContactFormFields.ContactTopic, ErrorCode.FieldRequired));
        }
        else if (topic is null)
        {
            fields.Add(new ContactFormFieldError(ContactFormFields.ContactTopic, ErrorCode.InvalidListValue));
        }

        Require(fields, ContactFormFields.Message, message);
        if (fields.Count > 0)
        {
            return ContactFormResult.FieldsRefused(fields);
        }

        // L-4 is counted on every request, before the work, on the address only (02 8.2); the row is written in a transaction of its own and stays when this one rolls back.
        AttemptSubject byIp = AttemptSubject.Ip(ipAddress);
        await _attempts.RecordAsync(AttemptLimit.L4, byIp, ct);
        if (await _attempts.IsExceededAsync(AttemptLimit.L4, byIp, ct))
        {
            return ContactFormResult.LimitExceeded();
        }

        await _requests.OpenAsync(new NewContactRequest(name, email, phone, topic!.Value, message), ct);
        return ContactFormResult.Sent();
    }

    /// <summary>The topic of a code name of the closed list, or null: a number, another spelling or free text is not a topic (04 5.10.10).</summary>
    internal static ContactTopic? TopicOf(string? code)
    {
        string? text = code?.Trim();
        foreach (ContactTopic topic in Topics)
        {
            if (string.Equals(topic.ToString(), text, StringComparison.Ordinal))
            {
                return topic;
            }
        }

        return null;
    }

    private static void Require(List<ContactFormFieldError> fields, string field, string value)
    {
        if (value.Length == 0)
        {
            fields.Add(new ContactFormFieldError(field, ErrorCode.FieldRequired));
        }
    }
}
