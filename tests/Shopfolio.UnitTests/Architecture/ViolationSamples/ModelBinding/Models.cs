// Deliberate violation sample of the model binding rules (07 5.1; 09 5.3 items 1-2; T211). Compiled into ModelBindingViolationSample.dll by the
// UnitTests project, never into the test assembly itself. The kind markers and the name attribute are the application's own (public types).
// SampleOrderItem and SampleFile stand in for the entities of 06; the rule test hands them over as the entity set.
// The quiet neighbours (Clean*, AddNoteInput ...) must produce no violation; every other type is one violation, named in the rule test.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Validation;

namespace Shopfolio.Pages.Samples
{
    // --- the stand-in entities of 06 ---

    public sealed class SampleOrderItem
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    public sealed class SampleFile
    {
        public Guid Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public long SizeBytes { get; set; }
    }

    // Violation EntityIsModel: an entity is no model.
    public sealed class ExposedEntityInput : IFormInput
    {
        public Guid Id { get; set; }
    }

    // --- quiet neighbours ---

    public sealed class AddNoteInput : IFormInput
    {
        [FieldRequired]
        [TextTooLong(150)]
        public string Name { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public Guid SampleOrderItemId { get; set; }

        [SpecAssignedName]
        public string Website { get; set; } = string.Empty;

        // A decimal with its amount or percentage rule and a day are the formats of 07 2.9 (T216): no DecimalWithoutRule, no TimeBound.
        [InvalidAmount]
        public decimal Price { get; set; }

        [SpecAssignedName]
        [InvalidPercentage]
        public decimal? Rate { get; set; }

        [SpecAssignedName]
        [InvalidDate]
        public DateOnly? ShipOn { get; set; }
    }

    public sealed class AdminAddNoteInput : IFormInput
    {
        public string Name { get; set; } = string.Empty;
    }

    // A password field and a field kept as typed carry the project's own markers (not validation attributes): no BuiltInRule (09 3.4 items 1 and 5).
    public sealed class SignInInput : IFormInput
    {
        [SpecAssignedName]
        [FieldRequired]
        [PasswordField]
        public string Password { get; set; } = string.Empty;

        [SpecAssignedName]
        [NotTrimmed]
        public string Passphrase { get; set; } = string.Empty;
    }

    public sealed class ReturnItemsInput : IFormInput
    {
        [SpecAssignedName]
        public List<ReturnRow> Items { get; set; } = [];
    }

    public sealed class ReturnRow
    {
        public Guid SampleOrderItemId { get; set; }

        public int Quantity { get; set; }
    }

    public sealed class NoteQuery : IQueryModel
    {
        [SpecAssignedName]
        public string Search { get; set; } = string.Empty;

        [SpecAssignedName]
        public int Page { get; set; }
    }

    public sealed class StartUploadRequest : IMachineRequest
    {
        [Required]
        public string FileName { get; set; } = string.Empty;

        public long SizeBytes { get; set; }
    }

    public sealed class UploadStatusResponse : IMachineResponse
    {
        public Guid Id { get; set; }

        [SpecAssignedName]
        public long ReceivedBytes { get; set; }
    }

    public sealed class CleanNotesPage : PageModel
    {
        public Task<IActionResult> OnGetAsync(NoteQuery query, System.Threading.CancellationToken cancellationToken) => Task.FromResult<IActionResult>(Page());

        // The form's main handler and a second handler of the same form bind the same model (07 5.1.5).
        public Task<IActionResult> OnPostAddNoteAsync(AddNoteInput input) => Task.FromResult<IActionResult>(Page());

        public Task<IActionResult> OnPostSearchNotesAsync(AddNoteInput input) => Task.FromResult<IActionResult>(Page());

        public Task<IActionResult> OnPostReturnItemsAsync(ReturnItemsInput input) => Task.FromResult<IActionResult>(Page());

        public Task<IActionResult> OnPostRemoveNoteAsync(Guid id) => Task.FromResult<IActionResult>(Page());

        // A decimal parameter with its rule and a day parameter (T216).
        public Task<IActionResult> OnPostPriceNoteAsync([InvalidAmount] decimal price, DateOnly day) => Task.FromResult<IActionResult>(Page());
    }

    public sealed class CleanAdminNotesPage : PageModel
    {
        [BindProperty]
        public AdminAddNoteInput Input { get; set; } = new();

        public Task<IActionResult> OnPostAddNoteAsync() => Task.FromResult<IActionResult>(Page());
    }

    // --- violations ---

    // KindName: marked, the name does not end with the kind.
    public sealed class BadKindName : IFormInput
    {
        public string Name { get; set; } = string.Empty;
    }

    // KindName: marked as a query model, the name says input.
    public sealed class WrongKindInput : IQueryModel
    {
        public string Name { get; set; } = string.Empty;
    }

    // MissingMarker: a name that ends with a kind, under the pages, without the marker.
    public sealed class UnmarkedInput
    {
        public string Name { get; set; } = string.Empty;
    }

    // EntityInModel: an entity as a field, and inside a row.
    public sealed class WrapsEntityInput : IFormInput
    {
        [SpecAssignedName]
        public SampleOrderItem Item { get; set; } = new();

        [SpecAssignedName]
        public List<NestedRow> Rows { get; set; } = [];
    }

    public sealed class NestedRow
    {
        [SpecAssignedName]
        public SampleFile File { get; set; } = new();
    }

    // FieldName: Cost has no 06 counterpart and no mark. SpecNameShadows06: Price is the name of a 06 field, so it needs no mark.
    public sealed class WrongFieldNamesInput : IFormInput
    {
        [InvalidAmount]
        public decimal Cost { get; set; }

        [SpecAssignedName]
        [InvalidAmount]
        public decimal Price { get; set; }
    }

    // TimeBound: a request carries no time (07 2.9.2; 09 7.2 item 2) - an instant, a time of day and a DateTime, in a form, in a query row and on a page.
    public sealed class TimedInput : IFormInput
    {
        [SpecAssignedName]
        public DateTimeOffset StartsAt { get; set; }

        [SpecAssignedName]
        public TimeOnly? OpensAt { get; set; }

        [SpecAssignedName]
        public List<TimedRow> Rows { get; set; } = [];
    }

    public sealed class TimedRow
    {
        [SpecAssignedName]
        public DateTime[] Days { get; set; } = [];
    }

    // FloatBound: an amount or a decimal number is a decimal (09 7.2 item 3) - a double and a float? in a query model.
    public sealed class FloatQuery : IQueryModel
    {
        [SpecAssignedName]
        public double Rate { get; set; }

        [SpecAssignedName]
        public float? Weight { get; set; }
    }

    // OtherNumberBound and TimeBound: a short and a TimeSpan in a query model (D-152).
    public sealed class OtherNumberQuery : IQueryModel
    {
        [SpecAssignedName]
        public short Count { get; set; }

        [SpecAssignedName]
        public TimeSpan Span { get; set; }
    }

    // DecimalWithoutRule: a decimal without its amount or percentage rule - none at all, inside a row, and a rule that owns the conversion failure but
    // checks no precision (the date rule).
    public sealed class UnruledAmountInput : IFormInput
    {
        [SpecAssignedName]
        public decimal Deposit { get; set; }

        [SpecAssignedName]
        [InvalidDate]
        public decimal Portion { get; set; }

        [SpecAssignedName]
        public List<UnruledRow> Rows { get; set; } = [];
    }

    public sealed class UnruledRow
    {
        [SpecAssignedName]
        public decimal? Fee { get; set; }
    }

    // TimeBound, FloatBound and DecimalWithoutRule on a page's handler parameters and bound property.
    public sealed class TimedPage : PageModel
    {
        [BindProperty]
        public DateTimeOffset? Stamp { get; set; }

        public IActionResult OnGet(DateTime day) => Page();

        public IActionResult OnGetRate(double rate) => Page();

        public Task<IActionResult> OnPostSaveAsync(decimal price) => Task.FromResult<IActionResult>(Page());
    }

    // MachineDate: a machine body carries no date (07 2.9.6) - an instant in a response and a day in a request.
    public sealed class StampedResponse : IMachineResponse
    {
        public Guid Id { get; set; }

        [SpecAssignedName]
        public DateTimeOffset CreatedAt { get; set; }
    }

    public sealed class DatedUploadRequest : IMachineRequest
    {
        [SpecAssignedName]
        public DateOnly ExpiresOn { get; set; }
    }

    // ByteCountNotNumber: a machine body's byte count is a JSON number (07 2.9.6) - neither a text nor a floating point value.
    public sealed class TextBytesResponse : IMachineResponse
    {
        [SpecAssignedName]
        public string ReceivedBytes { get; set; } = string.Empty;

        [SpecAssignedName]
        public double? TotalBytes { get; set; }
    }

    // BuiltInRule: the framework's validation attributes on an input model, on a query model and inside a row. A machine request (StartUploadRequest) may carry one.
    public sealed class BuiltInRuleInput : IFormInput
    {
        [Required]
        [StringLength(10)]
        public string Name { get; set; } = string.Empty;
    }

    public sealed class BuiltInRuleQuery : IQueryModel
    {
        [SpecAssignedName]
        [EmailAddress]
        public string Search { get; set; } = string.Empty;
    }

    public sealed class BuiltInRuleRowsInput : IFormInput
    {
        [SpecAssignedName]
        public List<BuiltInRuleRow> Items { get; set; } = [];
    }

    public sealed class BuiltInRuleRow
    {
        [Range(1, 5)]
        public int Quantity { get; set; }
    }

    // BuiltInRule: [DataType] is the framework's validation attribute too - also the password type is no way to mark a password field.
    public sealed class DataTypePasswordInput : IFormInput
    {
        [SpecAssignedName]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    // EntityBound: a handler parameter, a collection parameter and a bound property.
    public sealed class BindsEntityPage : PageModel
    {
        [BindProperty]
        public SampleOrderItem Item { get; set; } = new();

        public Task<IActionResult> OnPostChangeAsync(SampleOrderItem item) => Task.FromResult<IActionResult>(Page());

        public Task<IActionResult> OnPostChangeAllAsync(List<SampleOrderItem> items) => Task.FromResult<IActionResult>(Page());
    }

    // UnmarkedBound: a class that is neither a simple value nor a marked model.
    public sealed class PlainDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public sealed class BindsPlainClassPage : PageModel
    {
        public Task<IActionResult> OnPostSaveAsync(PlainDto dto) => Task.FromResult<IActionResult>(Page());
    }

    // InputName: the page has no handler Anything.
    public sealed class SaveAnythingInput : IFormInput
    {
        public string Name { get; set; } = string.Empty;
    }

    public sealed class MisnamedInputPage : PageModel
    {
        public Task<IActionResult> OnPostRemoveAsync(SaveAnythingInput input) => Task.FromResult<IActionResult>(Page());
    }
}
