using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Account;

/// <summary>
/// The address book (E-26; T518; 07 UC-70 to UC-73; 04 5.26; 02 3.14): inside the account folder, so the customer's session is required. The GET lists the member's entries, oldest first, with the add button
/// first; <c>addressBookId</c> opens the edit form with the entry's values and <c>create=true</c> the add form, both inside the screen (07 4.26.1; K-1186 (3)); an entry that is not this account's, a text that is
/// no id and an id that is gone are the one 404 (07 2.3.3). The three POSTs are the module's one-transaction writes: a saved entry is 303 to the book with the <c>Saved</c> notice (UC-71, UC-72), a deletion is
/// 303 to the book with no notice and no confirmation (UC-73; 04 5.26.6), an entry that is not the member's or is gone is 404 and writes nothing, and refused fields are 422 with the form open and the values
/// sent (04 5.26.7). The page puts no validation attribute on the input: the one rule home is <c>AddressFieldRules</c> in the module, and the refusals come back as field codes that the page filter turns into the
/// catalog's messages. A repeat of a create is a second entry (07 4.26.2; K-1179 (1)); the button's lock is the form's <c>submit-once</c> and the redirect after the post is the rest. The response is personal and not stored.
/// </summary>
[NoStore]
internal sealed class AddressesModel : PageModel
{
    /// <summary>The prefix of the form's field names: the handler's parameter is <c>input</c>.</summary>
    public const string FieldPrefix = "Input.";

    // The id a province text that is no id stands for: no province has it, so the module answers "not on the list" (InvalidProvince).
    private static readonly Guid NotAnId = new("ffffffff-ffff-ffff-ffff-ffffffffffff");

    private readonly IMembershipModule _membership;
    private readonly ISettingsModule _settings;
    private readonly ITransactionRunner _transactions;

    public AddressesModel(IMembershipModule membership, ISettingsModule settings, ITransactionRunner transactions)
    {
        ArgumentNullException.ThrowIfNull(membership);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(transactions);
        _membership = membership;
        _settings = settings;
        _transactions = transactions;
    }

    /// <summary>The member's entries, oldest first.</summary>
    public IReadOnlyList<AddressBookEntry> Entries { get; private set; } = [];

    /// <summary>The form that stands open inside the screen; null when none is open (04 5.26.4).</summary>
    public AddressFormView? Form { get; private set; }

    /// <summary>The entry the open form edits; null for the add form and when no form is open.</summary>
    public Guid? EditingId { get; private set; }

    /// <summary>The short-lived "kaydedildi" notice (04 5.26.6); null when the address carries none.</summary>
    public MessageView? Notice { get; private set; }

    public async Task<IActionResult> OnGetAsync(AddressBookQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (CustomerId() is not Guid customerId)
        {
            return NotFoundResult();
        }

        if (NoticeParameter.Read(Request, NoticeKind.ShortLived) == NoticeCode.Saved)
        {
            Notice = Messages.ShortLivedSuccess(NoticeTexts.Saved);
        }

        Entries = await _membership.GetAddressBookAsync(customerId, ct);
        IReadOnlyList<ProvinceChoice> provinces = await ProvincesAsync(ct);
        if (!string.IsNullOrEmpty(query.AddressBookId))
        {
            // The edit form: an entry of this account only (the list is the account's own), the answer is the same for a stranger's, a gone one and a text that is no id.
            AddressBookEntry? entry = Guid.TryParse(query.AddressBookId, out Guid id) ? Entries.FirstOrDefault(e => e.Id == id) : null;
            if (entry is null)
            {
                return NotFoundResult();
            }

            EditingId = entry.Id;
            Form = AddressForms.Create(
                AddressVariant.Book,
                FieldPrefix,
                provinces,
                new AddressValuesView(entry.Name, entry.RecipientName, entry.ProvinceId.ToString("D"), entry.District, entry.AddressLine, entry.Phone));
        }
        else if (string.Equals(query.Create, "true", StringComparison.OrdinalIgnoreCase))
        {
            Form = AddressForms.Create(AddressVariant.Book, FieldPrefix, provinces);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCreateAddressBookAsync(CreateAddressBookInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (CustomerId() is not Guid customerId)
        {
            return NotFoundResult();
        }

        AddressBookInput value = ValueOf(input);
        AddressBookSaveResult result = await _transactions.RunAsync(cancel => _membership.CreateAddressBookEntryAsync(customerId, value, cancel), ct);
        if (result.IsSaved)
        {
            return FormResults.SeeOther(FramePaths.AccountAddresses, NoticeCode.Saved);
        }

        // The account is gone: the one 404; no field is refused then.
        if (result.IsNotFound)
        {
            return NotFoundResult();
        }

        return await RedisplayAsync(customerId, result, input, null, ct);
    }

    public async Task<IActionResult> OnPostUpdateAddressBookAsync(UpdateAddressBookInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (CustomerId() is not Guid customerId || !Guid.TryParse(input.AddressBookId, out Guid entryId))
        {
            return NotFoundResult();
        }

        AddressBookInput value = ValueOf(input);
        AddressBookSaveResult result = await _transactions.RunAsync(cancel => _membership.UpdateAddressBookEntryAsync(customerId, entryId, value, cancel), ct);
        if (result.IsSaved)
        {
            return FormResults.SeeOther(FramePaths.AccountAddresses, NoticeCode.Saved);
        }

        if (result.IsNotFound)
        {
            return NotFoundResult();
        }

        return await RedisplayAsync(customerId, result, input, entryId, ct);
    }

    public async Task<IActionResult> OnPostDeleteAddressBookAsync(DeleteAddressBookInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (CustomerId() is not Guid customerId || !Guid.TryParse(input.AddressBookId, out Guid entryId))
        {
            return NotFoundResult();
        }

        // The deletion asks no confirmation and has no notice of its own; a repeat finds nothing, writes nothing and is the 404 (07 UC-73; K-1179 (2)).
        AddressBookDeleteResult result = await _transactions.RunAsync(cancel => _membership.DeleteAddressBookEntryAsync(customerId, entryId, cancel), ct);
        return result.IsDeleted ? FormResults.SeeOther(FramePaths.AccountAddresses) : NotFoundResult();
    }

    // The 422 of refused fields: the form stays open with the values sent, the list behind it; the refusals go through the page filter's one path (09 3.4 item 2; 04 5.26.7).
    private async Task<IActionResult> RedisplayAsync(Guid customerId, AddressBookSaveResult result, CreateAddressBookInput input, Guid? editing, CancellationToken ct)
    {
        foreach (MembershipFieldError refusal in result.Fields)
        {
            ModelState.AddFieldError(FieldPrefix + refusal.Field, refusal.Code.ToString());
        }

        Entries = await _membership.GetAddressBookAsync(customerId, ct);
        EditingId = editing;
        Form = AddressForms.Create(
            AddressVariant.Book,
            FieldPrefix,
            await ProvincesAsync(ct),
            new AddressValuesView(input.Name, input.RecipientName, input.ProvinceId, input.District, input.AddressLine, input.Phone));
        return FormResults.Redisplay(this);
    }

    // The text of the province field becomes the id the module checks against the closed list; a text that is no id is an id that is not on the list (the empty one is "required").
    private static AddressBookInput ValueOf(CreateAddressBookInput input) =>
        new(input.Name, input.RecipientName, ProvinceOf(input.ProvinceId), input.District, input.AddressLine, input.Phone);

    private static Guid? ProvinceOf(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return Guid.TryParse(text.Trim(), out Guid id) ? id : NotAnId;
    }

    private async Task<IReadOnlyList<ProvinceChoice>> ProvincesAsync(CancellationToken ct) =>
        [.. (await _settings.GetProvincesAsync(ct)).Select(p => new ProvinceChoice(p.Id, p.Name))];

    private Guid? CustomerId() => User.AccountId();

    // Every denial is the one 404 and leaves its one rejection line (07 2.3.3; 09 8.2 item 3).
    private IActionResult NotFoundResult()
    {
        HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(ErrorCode.NotFound);
        return AccessDenial.NotFound();
    }
}

/// <summary>The query of UC-70 (07 5.3.2): <c>addressBookId</c> opens the edit form of an entry, <c>create=true</c> the add form.</summary>
public sealed class AddressBookQuery : IQueryModel
{
    /// <summary>The entry's inner key as text: a wrong value is never a binding error (07 2.6.6); the page answers 404 for what is not this account's entry.</summary>
    public string? AddressBookId { get; init; }

    [SpecAssignedName]
    public string? Create { get; init; }
}

/// <summary>
/// The form of UC-71 (07 5.3; 04 7.1.16): the six values of an entry. The province is text so that a value that is not an id is a field message and not a binding error; the module owns the rules, so no rule
/// attribute stands here - the labels are the form's (<see cref="AddressForms.Labels"/>), which the page filter writes the refusals with.
/// </summary>
public class CreateAddressBookInput : IFormInput
{
    [Display(Name = AddressForms.Labels.Name)]
    public string? Name { get; set; }

    [Display(Name = AddressForms.Labels.RecipientName)]
    public string? RecipientName { get; set; }

    [Display(Name = AddressForms.Labels.Province)]
    public string? ProvinceId { get; set; }

    [Display(Name = AddressForms.Labels.District)]
    public string? District { get; set; }

    [Display(Name = AddressForms.Labels.AddressLine)]
    public string? AddressLine { get; set; }

    [Display(Name = AddressForms.Labels.Phone)]
    public string? Phone { get; set; }
}

/// <summary>The form of UC-72 (07 5.3): the entry's inner key (text, like the query's) and the fields of <see cref="CreateAddressBookInput"/>.</summary>
public sealed class UpdateAddressBookInput : CreateAddressBookInput
{
    public string? AddressBookId { get; set; }
}

/// <summary>The form of UC-73 (07 5.3): the entry to delete.</summary>
public sealed class DeleteAddressBookInput : IFormInput
{
    public string? AddressBookId { get; set; }
}
