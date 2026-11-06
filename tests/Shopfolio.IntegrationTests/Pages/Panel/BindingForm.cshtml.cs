using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Validation;

namespace Shopfolio.IntegrationTests.Pages.Panel;

/// <summary>
/// The test-only sample page of the model binding pattern (T211; 07 5.1): each handler binds its own input model by a parameter, two handlers of one form
/// bind the same model (<c>SaveNote</c> and <c>SearchNotes</c> take <see cref="SaveNoteInput"/>), the model has no field for the price the server writes,
/// and the <c>Items</c> list of <see cref="ReturnItemsInput"/> is one row per item. The page lives in the test host only; the anti-forgery check is T209's.
/// </summary>
[IgnoreAntiforgeryToken]
public sealed class BindingFormModel : PageModel
{
    private readonly BindingRecordStore _store;

    public BindingFormModel(BindingRecordStore store)
    {
        _store = store;
    }

    public IReadOnlyList<NoteRecord> Notes => _store.Notes();

    public IReadOnlyList<ReturnRecord> Returns => _store.Returns();

    public string SearchResult { get; private set; } = string.Empty;

    public IReadOnlyList<ContactRecord> Contacts => _store.Contacts();

    /// <summary>Read-only models the template renders with the tag helper, to see what the framework writes into the HTML (09 3.4 item 6).</summary>
    public SaveNoteInput Preview { get; } = new();

    public ReturnRow PreviewRow { get; } = new();

    public IActionResult OnGetFind(NoteQuery query)
    {
        SearchResult = query.Search;
        return Page();
    }

    public Task<IActionResult> OnPostContactAsync(ContactInput input)
    {
        // A rule the handler decides itself (09 3.4 item 2): the address is taken. The code goes through the same filter after the handler returns.
        if (input.Email == "taken@example.com")
        {
            ModelState.AddFieldError(nameof(ContactInput.Email), FieldCodes.InvalidEmail);
        }

        if (!ModelState.IsValid)
        {
            return Task.FromResult(FormResults.Redisplay(this));
        }

        _store.AddContact(new ContactRecord(input.Email, input.Phone, input.Password, input.Remark, input.Amount, input.Nickname));
        return Task.FromResult(FormResults.SeeOther("/panel/model-baglama"));
    }

    public Task<IActionResult> OnPostSaveNoteAsync(SaveNoteInput input)
    {
        if (!ModelState.IsValid)
        {
            return Task.FromResult(FormResults.Redisplay(this));
        }

        // The price is the server's: it is not a field of the model, and nothing the request carries reaches it.
        _store.AddNote(new NoteRecord(input.Name, input.Reason, BindingRecordStore.ServerPrice));
        return Task.FromResult(FormResults.SeeOther("/panel/model-baglama"));
    }

    public Task<IActionResult> OnPostSearchNotesAsync(SaveNoteInput input)
    {
        if (!ModelState.IsValid)
        {
            return Task.FromResult(FormResults.Redisplay(this));
        }

        SearchResult = string.Join(',', _store.Notes().Where(n => n.Name == input.Name).Select(n => n.Name));
        return Task.FromResult<IActionResult>(Page());
    }

    public Task<IActionResult> OnPostReturnItemsAsync(ReturnItemsInput input)
    {
        // One row per item: a repeated item is an invalid selection and no row is processed (07 5.1.5).
        ItemRows.RejectRepeated(ModelState, nameof(ReturnItemsInput.Items), input.Items.Select(r => r.OrderItemId));
        if (!ModelState.IsValid)
        {
            return Task.FromResult(FormResults.Redisplay(this));
        }

        foreach (ReturnRow row in input.Items)
        {
            _store.AddReturn(new ReturnRecord(row.OrderItemId, row.Quantity));
        }

        return Task.FromResult(FormResults.SeeOther("/panel/model-baglama"));
    }
}

/// <summary>The limits of the sample page's fields; a number is written once, here, and the rule attributes and the HTML attributes read it (09 3.4 items 1 and 6).</summary>
public static class BindingLimits
{
    public const int NameMaxLength = 20;

    public const int QuantityMax = 1000;

    public const int SearchMaxLength = 10;
}

/// <summary>The input model of <c>SaveNote</c> (07 5.1.5: the handler's name and the kind); <c>SearchNotes</c> sends the same form.</summary>
public sealed class SaveNoteInput : IFormInput
{
    [Display(Name = "Ad")]
    [FieldRequired]
    [TextTooLong(BindingLimits.NameMaxLength)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Sebep")]
    [FieldRequired]
    [InvalidListValue("Damaged", "Wrong")]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>The input model of <c>ReturnItems</c>: a list of rows, one row per item.</summary>
public sealed class ReturnItemsInput : IFormInput
{
    [Display(Name = "Kalemler")]
    [FieldRequired]
    public List<ReturnRow> Items { get; set; } = [];
}

/// <summary>A row of <c>Items</c>: the item and the quantity (the names of 06's order item).</summary>
public sealed class ReturnRow
{
    [Display(Name = "Kalem")]
    public Guid OrderItemId { get; set; }

    [Display(Name = "Adet")]
    [InvalidQuantity(BindingLimits.QuantityMax)]
    public int Quantity { get; set; }
}

/// <summary>The input model of <c>Contact</c>: the rule kinds the sample page shows together - required, e-mail, phone, amount, password, a field kept as typed.</summary>
public sealed class ContactInput : IFormInput
{
    [Display(Name = "E-posta")]
    [FieldRequired]
    [InvalidEmail]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Telefon")]
    [InvalidPhone]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "Şifre")]
    [PasswordField]
    [FieldRequired]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Not")]
    [NotTrimmed]
    public string Remark { get; set; } = string.Empty;

    [Display(Name = "Tutar")]
    [InvalidAmount(AllowZero = true, Max = 1000)]
    public decimal Amount { get; set; }

    /// <summary>A non-nullable text with no rule and no initial value: absent from the request it stays null and is not an error (the framework's implicit "required" is off).</summary>
    [Display(Name = "Takma ad")]
    public string Nickname { get; set; } = null!;
}

/// <summary>The query model of the <c>Find</c> GET handler: a rejected query on a GET shows the message and keeps status 200 (07 2.6.6).</summary>
public sealed class NoteQuery : IQueryModel
{
    [Display(Name = "Arama")]
    [TextTooLong(BindingLimits.SearchMaxLength)]
    public string Search { get; set; } = string.Empty;
}

/// <summary>The contact as the sample page stores it, the values as the binder gave them.</summary>
public sealed record ContactRecord(string Email, string Phone, string Password, string Remark, decimal Amount, string? Nickname);

/// <summary>A note as the server writes it, with the price the server fixed.</summary>
public sealed record NoteRecord(string Name, string Reason, decimal Price);

/// <summary>A processed return row.</summary>
public sealed record ReturnRecord(Guid OrderItemId, int Quantity);

/// <summary>The records of the sample page, in memory; a singleton of the test host.</summary>
public sealed class BindingRecordStore
{
    /// <summary>The value the server writes into every note; no request field can change it.</summary>
    public const decimal ServerPrice = 10.00m;

    private readonly List<NoteRecord> _notes = [];
    private readonly List<ReturnRecord> _returns = [];
    private readonly List<ContactRecord> _contacts = [];
    private readonly Lock _gate = new();

    public IReadOnlyList<NoteRecord> Notes()
    {
        lock (_gate)
        {
            return [.. _notes];
        }
    }

    public IReadOnlyList<ReturnRecord> Returns()
    {
        lock (_gate)
        {
            return [.. _returns];
        }
    }

    public void AddNote(NoteRecord note)
    {
        lock (_gate)
        {
            _notes.Add(note);
        }
    }

    public IReadOnlyList<ContactRecord> Contacts()
    {
        lock (_gate)
        {
            return [.. _contacts];
        }
    }

    public void AddContact(ContactRecord record)
    {
        lock (_gate)
        {
            _contacts.Add(record);
        }
    }

    public void AddReturn(ReturnRecord record)
    {
        lock (_gate)
        {
            _returns.Add(record);
        }
    }
}
