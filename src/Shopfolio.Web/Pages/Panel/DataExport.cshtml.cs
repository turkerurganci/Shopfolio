using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Pages.Components;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The form of UC-182 (07 5.2.2 <c>ExportOrdersInput</c>; 04 7.2.25.2): the two ends of the range, the Turkey days the orders were created on. Both are text so that a wrong value is a field message of the
/// section, never a binding failure.
/// </summary>
public sealed class ExportOrdersInput : IFormInput
{
    [SpecAssignedName]
    [Display(Name = "Başlangıç")]
    public string? StartDate { get; set; }

    [SpecAssignedName]
    [Display(Name = "Bitiş")]
    public string? EndDate { get; set; }
}

/// <summary>The three sections of the screen (04 9.24.3-9.24.5); the one a POST was for carries its messages.</summary>
public enum DataExportSection
{
    None,
    Orders,
    Customers,
    ContactRequests,
}

/// <summary>
/// E-53, the export screen (T524; 04 9.24; 07 UC-107, UC-182-UC-184): the responsibility line (K-823) and the three sections - orders by a range of creation days, the member list and the contact requests, the
/// last two with no range. Each "CSV olarak indir" is a POST whose success is the file itself, 200 and an attachment, and the screen does not change (07 9.24; K-1195 (1)): the file is made by the
/// Administration module in the temporary directory, the trail row is written there after the file is made and before it is sent, and the file is deleted when the request ends (D-849). No confirmation
/// (04 9.24.7). A wrong range and an empty set are 422 with the section's message, nothing made and nothing written; a file that could not be made is the unexpected-error layer's 500 with the operation
/// message in the section and the range kept (04 9.24.10, 2.7.3.1).
/// </summary>
[NoStore]
internal sealed class DataExportModel : PageModel
{
    /// <summary>The line at the head of the screen (04 9.24.2; K-823).</summary>
    public const string ResponsibilityText = "Her dışa aktarma işlem izine yazılır. İndirdiğiniz dosyadaki kişisel verilerin korunması sizin sorumluluğunuzdadır.";

    /// <summary>The button of every section (04 9.24.3-9.24.5; K-846).</summary>
    public const string DownloadLabel = "CSV olarak indir";

    /// <summary>The one sentence of the order section that lists what the file carries (04 9.24.3; the list's home is 02 10.7.1).</summary>
    public const string OrdersFieldsText =
        "Dosya, seçilen aralıkta oluşmuş ve ödemesi tamamlanmış siparişlerin fatura verisini taşır: tutar, KDV oranı ve tutarı, kalem dökümü, indirim ve kupon payı, kargo ücreti, donmuş fatura adresi ve firma kimliği, "
        + "ödeme yöntemi ve onay tarihi, kargoya verme tarihi, kargo şirketi, sitenin alan adı, siparişin iletişim e-postası ve alıcı adı. Her satır bir sipariş kalemidir.";

    /// <summary>04 9.24.4: what the member list carries.</summary>
    public const string CustomersText = "Üyelerin adı ve e-postası. Tarih aralığı yoktur; dosya bütün üye hesaplarını taşır.";

    /// <summary>04 9.24.5: what the contact request list carries.</summary>
    public const string ContactRequestsText = "Talep sahiplerinin formdaki adı ve e-postası. Tarih aralığı yoktur; dosya saklanan bütün talepleri taşır.";

    /// <summary>The lasting message of an order range with no order in the file's set (04 9.24.7; K-823).</summary>
    public const string NoOrdersText = "Bu aralıkta sipariş yok; dosya oluşturulmadı.";

    /// <summary>The lasting message of an empty member or contact request list (04 9.24.10; K-823).</summary>
    public const string NothingToWriteText = "Dışa aktarılacak kayıt yok.";

    public const string StartField = nameof(ExportOrdersInput.StartDate);

    public const string EndField = nameof(ExportOrdersInput.EndDate);

    private readonly IDataExport _export;
    private readonly TimeProvider _time;

    public DataExportModel(IDataExport export, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(export);
        ArgumentNullException.ThrowIfNull(time);
        _export = export;
        _time = time;
    }

    /// <summary>The section the POST was for; its messages - the empty set, the unexpected error - stand in it.</summary>
    public DataExportSection Section { get; private set; }

    /// <summary>True when the POST found nothing to put in the file (422): the section says so in place.</summary>
    public bool IsEmpty { get; private set; }

    /// <summary>The range the order section shows again.</summary>
    public string? StartDate { get; private set; }

    public string? EndDate { get; private set; }

    /// <summary>The Turkey day of the request: the range fields offer no later day (04 7.2.25.2).</summary>
    public DateOnly Today => TurkeyTime.DateOf(_time.GetUtcNow());

    public IActionResult OnGet() => Page();

    /// <summary>UC-182: the orders of the range as a CSV file. A wrong range or an empty set is 422 and writes nothing.</summary>
    public async Task<IActionResult> OnPostExportOrdersAsync(ExportOrdersInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        Section = DataExportSection.Orders;
        StartDate = input.StartDate;
        EndDate = input.EndDate;

        DateRangeVerdict verdict = DateFields.CheckRange(input.StartDate, input.EndDate, Today, DateRangePolicy.NoFuture, startRequired: true, endRequired: true);
        if (!verdict.IsAccepted)
        {
            DateFields.AddRefusal(ModelState, StartField, EndField, verdict);
            return FormResults.Redisplay(this);
        }

        ExportFileResult result = await _export.ExportOrdersAsync(verdict.Start, verdict.End, ct);
        if (result.Outcome == ExportOutcome.RangeInvalid)
        {
            // The day turned between the page's check and the module's (the module's today is later): the module's refusal is the field message.
            DateFields.AddRefusal(ModelState, StartField, EndField, new DateRangeVerdict(OutcomeOf(result.RangeError), verdict.Start, verdict.End));
            return FormResults.Redisplay(this);
        }

        return Answer(result);
    }

    /// <summary>UC-183: every member account as a CSV file; none is 422 and writes nothing.</summary>
    public async Task<IActionResult> OnPostExportCustomersAsync(CancellationToken ct)
    {
        Section = DataExportSection.Customers;
        return Answer(await _export.ExportCustomersAsync(ct));
    }

    /// <summary>UC-184: every contact request still kept as a CSV file; none is 422 and writes nothing.</summary>
    public async Task<IActionResult> OnPostExportContactRequestsAsync(CancellationToken ct)
    {
        Section = DataExportSection.ContactRequests;
        return Answer(await _export.ExportContactRequestsAsync(ct));
    }

    /// <summary>The field rule's outcome for the module's refusal of a range.</summary>
    internal static DateRangeOutcome OutcomeOf(ExportRangeError error) => error switch
    {
        ExportRangeError.StartMissing => DateRangeOutcome.StartMissing,
        ExportRangeError.EndMissing => DateRangeOutcome.EndMissing,
        ExportRangeError.StartInFuture => DateRangeOutcome.StartInTheFuture,
        ExportRangeError.EndInFuture => DateRangeOutcome.EndInTheFuture,
        ExportRangeError.EndBeforeStart => DateRangeOutcome.EndBeforeStart,
        _ => throw new ArgumentOutOfRangeException(nameof(error), error, "Not a refusal of a range."),
    };

    private IActionResult Answer(ExportFileResult result)
    {
        if (result.Outcome == ExportOutcome.Done && result.File is { } file && result.FileName is { } name)
        {
            return new CsvFileResult(name, file);
        }

        // Nothing to write (07 9.24.2-9.24.4, ExportEmpty): no file stayed and no row was written; the section says so in place.
        IsEmpty = true;
        return FormResults.Redisplay(this);
    }
}
