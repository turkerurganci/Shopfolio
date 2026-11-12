using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Lists;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Lists;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The query model of the operation trail (E-52; 07 5.3.2 <c>AuditLogQuery</c>; UC-106): the two ends of the date range, the administrator and <c>page</c>. Every field is text so that a wrong value is never a
/// binding error (07 2.6.6): <see cref="AuditTrailModel"/> reads the dates through the range rule and the administrator through the closed list. There is no search, no grouping and no sort (04 9.23.7).
/// </summary>
public sealed class AuditLogQuery : PagedQuery
{
    public string? StartDate { get; init; }

    public string? EndDate { get; init; }

    public string? AdminId { get; init; }
}

/// <summary>A selected filter shown at the top of the list with the address that removes only it (04 9.23.8; 2.13.3).</summary>
public sealed record AuditFilterChip(string Label, string RemovePath);

/// <summary>
/// E-52, the operation trail (T522; 04 9.23; 07 UC-106): the rows newest first, 25 a page, with the date range and the administrator filters applied together. The GET writes nothing and the screen has no
/// operation; a row opens no other screen (K-822). A range that does not hold - a future date, an end before the start - shows its field message and applies no filter, status 200 (07 2.6.6).
/// </summary>
public sealed class AuditTrailModel : PageModel
{
    public const string NoRowsText = "Henüz işlem izi kaydı yok.";

    public const string NoMatchText = "Bu süzgece uyan kayıt yok.";

    public const string RemoveFilterLabel = "Süzgeci kaldır";

    public const string HomeLabel = "Ana sayfaya dön";

    public const string AllAdminsLabel = "Tüm yöneticiler";

    /// <summary>The suffix of a removed administrator in the filter's list (04 9.23.3).</summary>
    public const string RemovedSuffix = " (kaldırıldı)";

    public const string StartFieldName = "startDate";

    public const string EndFieldName = "endDate";

    public const string AdminFieldName = "adminId";

    private static readonly PanelTableColumn[] Columns =
    [
        new("Tarih ve saat", true),
        new("Yönetici", true),
        new("Alan", false),
        new("İşlem", true),
        new("Kayıt", false),
        new("Değerler", false),
    ];

    private readonly IAuditTrail _trail;
    private readonly TimeProvider _time;

    public AuditTrailModel(IAuditTrail trail, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(trail);
        ArgumentNullException.ThrowIfNull(time);
        _trail = trail;
        _time = time;
    }

    public PanelTable? Table { get; private set; }

    public Pager Pager { get; private set; } = new(null, null, []);

    public EmptyStateView? Empty { get; private set; }

    /// <summary>The filter's date fields, the range rule's hint and refusal included.</summary>
    public IReadOnlyList<FieldView> RangeFieldViews { get; private set; } = [];

    public FieldView AdminField { get; private set; } = Fields.ClosedList(AdminFieldName, "Yönetici", FieldRequirement.Optional, [new FieldOption(string.Empty, AllAdminsLabel)]);

    public IReadOnlyList<AuditFilterChip> Chips { get; private set; } = [];

    /// <summary>True when the request carries a filter that applies; a refused range or an unknown administrator applies nothing.</summary>
    public bool IsFilterApplied { get; private set; }

    public string ListRoot => PanelFramePaths.OperationTrail;

    public async Task<IActionResult> OnGetAsync(AuditLogQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        DateOnly today = TurkeyTime.DateOf(_time.GetUtcNow());
        IReadOnlyList<AuditAdminOption> admins = await _trail.ListAdminsAsync(ct);
        DateRangeVerdict verdict = Components.DateFields.CheckRange(query.StartDate, query.EndDate, today, DateRangePolicy.NoFuture, false, false);
        AuditAdminOption? admin = Guid.TryParse(query.AdminId, out Guid adminId) ? admins.FirstOrDefault(a => a.AdminId == adminId) : null;

        // A range that does not hold applies no filter at all (07 9.23.1): the message is on the field and the list is the unfiltered one.
        AuditQuery trailQuery;
        if (verdict.IsAccepted)
        {
            trailQuery = new AuditQuery(verdict.Start, verdict.End, admin?.AdminId, query.RequestedPage());
            IsFilterApplied = verdict.Start is not null || verdict.End is not null || admin is not null;
        }
        else
        {
            Components.DateFields.AddRefusal(ModelState, StartFieldName, EndFieldName, verdict);
            trailQuery = new AuditQuery(Page: query.RequestedPage());
            admin = null;
        }

        AuditPage page = await _trail.ListAsync(trailQuery, ct);
        FormErrorSummary summary = FormErrorSummary.From(ModelState, [StartFieldName, EndFieldName]);
        RangeFieldViews = Fields.BindAll(Components.DateFields.RangeFields(StartFieldName, EndFieldName, today, DateRangePolicy.NoFuture, false, false, query.StartDate, query.EndDate), summary);
        AdminField = Fields.ClosedList(
            AdminFieldName,
            "Yönetici",
            FieldRequirement.Optional,
            [new FieldOption(string.Empty, AllAdminsLabel), .. admins.Select(a => new FieldOption(a.AdminId.ToString("D"), LabelOf(a)))],
            admin?.AdminId.ToString("D"));
        Chips = ChipsOf(query, verdict.IsAccepted, admin, admins);

        if (page.Items.Count == 0)
        {
            Empty = IsFilterApplied
                ? EmptyStates.WithExit(NoMatchText, RemoveFilterLabel, ListRoot)
                : EmptyStates.WithExit(NoRowsText, HomeLabel, PanelFramePaths.Home);
            return Page();
        }

        Table = new PanelTable("İşlem izi", Columns, [.. page.Items.Select(RowOf)]);
        Pager = Pager.Of(ListRoot, query, page.PageNumber, page.PageCount);
        return Page();
    }

    /// <summary>The administrator filter's option text: the name written in the rows, with the suffix when the account is gone (04 9.23.3).</summary>
    public static string LabelOf(AuditAdminOption admin)
    {
        ArgumentNullException.ThrowIfNull(admin);
        return $"{admin.Name} ({admin.Email})" + (admin.IsRemoved ? RemovedSuffix : string.Empty);
    }

    /// <summary>One row (04 9.23.4): when, who with the name and address of that moment, the area, the operation, the touched record and the old-new values. No cell carries a customer's personal data (the module refuses to write it).</summary>
    public static PanelTableRow RowOf(AuditRow row)
    {
        ArgumentNullException.ThrowIfNull(row);
        return new PanelTableRow(
            [
                DisplayFormat.DateAndTime(row.At),
                $"{row.AdminName} · {row.AdminEmail}",
                AuditTexts.AreaOf(row.Area),
                AuditTexts.OperationOf(row.Operation),
                RecordOf(row),
                ValuesOf(row.Details),
            ],
            null);
    }

    private static string RecordOf(AuditRow row) => row.Details?.DisplayName is { Length: > 0 } name ? name : "—";

    private static string ValuesOf(AuditDetails? details)
    {
        if (details is null)
        {
            return string.Empty;
        }

        IEnumerable<string> changes = details.Changes.Select(c => $"{c.Field}: {c.OldValue ?? "—"} → {c.NewValue ?? "—"}");
        IEnumerable<string> values = details.Values.Select(v => $"{v.Name}: {v.Value ?? "—"}");
        return string.Join("; ", changes.Concat(values));
    }

    private List<AuditFilterChip> ChipsOf(AuditLogQuery query, bool rangeHolds, AuditAdminOption? admin, IReadOnlyList<AuditAdminOption> admins)
    {
        List<AuditFilterChip> chips = [];
        if (!rangeHolds)
        {
            return chips;
        }

        if (!string.IsNullOrWhiteSpace(query.StartDate))
        {
            chips.Add(new AuditFilterChip("Başlangıç: " + query.StartDate, ListAddress.Of(ListRoot, With(query, startDate: false), PageNumber.First)));
        }

        if (!string.IsNullOrWhiteSpace(query.EndDate))
        {
            chips.Add(new AuditFilterChip("Bitiş: " + query.EndDate, ListAddress.Of(ListRoot, With(query, endDate: false), PageNumber.First)));
        }

        if (admin is not null)
        {
            chips.Add(new AuditFilterChip("Yönetici: " + LabelOf(admins.First(a => a.AdminId == admin.AdminId)), ListAddress.Of(ListRoot, With(query, adminId: false), PageNumber.First)));
        }

        return chips;
    }

    private static AuditLogQuery With(AuditLogQuery query, bool startDate = true, bool endDate = true, bool adminId = true) =>
        new() { StartDate = startDate ? query.StartDate : null, EndDate = endDate ? query.EndDate : null, AdminId = adminId ? query.AdminId : null };
}
