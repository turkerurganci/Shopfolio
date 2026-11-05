using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Forms;

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

/// <summary>The input model of <c>SaveNote</c> (07 5.1.5: the handler's name and the kind); <c>SearchNotes</c> sends the same form.</summary>
public sealed class SaveNoteInput : IFormInput
{
    [Required]
    [StringLength(20)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [AllowedValues("Damaged", "Wrong")]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>The input model of <c>ReturnItems</c>: a list of rows, one row per item.</summary>
public sealed class ReturnItemsInput : IFormInput
{
    [Required]
    [MinLength(1)]
    public List<ReturnRow> Items { get; set; } = [];
}

/// <summary>A row of <c>Items</c>: the item and the quantity (the names of 06's order item).</summary>
public sealed class ReturnRow
{
    public Guid OrderItemId { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; }
}

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

    public void AddReturn(ReturnRecord record)
    {
        lock (_gate)
        {
            _returns.Add(record);
        }
    }
}
