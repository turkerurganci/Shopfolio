using System.ComponentModel.DataAnnotations;
using System.Text;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Forms;

namespace Shopfolio.IntegrationTests.Pages.Panel;

/// <summary>
/// The test-only sample page of the page-form pattern (T206; 07 2.2): one screen is one page, its GET shows the screen and changes nothing, and every
/// POST is a named handler that comes with the <c>handler</c> query parameter. <c>Add</c> writes: a valid form ends in 303 to the GET, an invalid one
/// is shown again with the sent values and the status 422. <c>Refuse</c> writes nothing and names its reason by a redirect (303). <c>Export</c> is the
/// one writing POST that does not redirect: 200 and a CSV. The page lives in the test host only; the anti-forgery check is the task of T209.
/// </summary>
[IgnoreAntiforgeryToken]
public sealed class PatternFormModel : PageModel
{
    private readonly PatternRecordStore _store;

    public PatternFormModel(PatternRecordStore store)
    {
        _store = store;
    }

    [BindProperty]
    public PatternInput Input { get; set; } = new();

    public IReadOnlyList<string> Records => _store.Snapshot();

    public IActionResult OnPostAdd()
    {
        if (!ModelState.IsValid)
        {
            return FormResults.Redisplay(this);
        }

        _store.Add(Input.Name);
        return FormResults.SeeOther("/panel/ornek-form");
    }

#pragma warning disable CA1822 // A page handler is an instance method the framework finds by its name; a static method is not a handler.
    public IActionResult OnPostRefuse() => FormResults.SeeOther("/panel/ornek-form?notice=refused");
#pragma warning restore CA1822

    public IActionResult OnPostExport() => new CsvFileResult("ornek-liste.csv", Encoding.UTF8.GetBytes(string.Join('\n', _store.Snapshot())));
}

/// <summary>The input model of the <c>Add</c> handler (09 5.3 item 2: the handler's name and the type of its input).</summary>
public sealed class PatternInput
{
    [Required]
    [StringLength(20)]
    public string Name { get; set; } = string.Empty;
}

/// <summary>The records of the sample page, in memory; a singleton of the test host.</summary>
public sealed class PatternRecordStore
{
    private readonly List<string> _records = [];
    private readonly Lock _gate = new();

    public IReadOnlyList<string> Snapshot()
    {
        lock (_gate)
        {
            return [.. _records];
        }
    }

    public void Add(string name)
    {
        lock (_gate)
        {
            _records.Add(name);
        }
    }
}
