using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Lists;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Coupons;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Lists;

namespace Shopfolio.Pages.Panel;

/// <summary>The query of the coupon list (07 UC-90): the page only - there is no search and no filter (04 9.7.5; K-737).</summary>
public sealed class CouponListQuery : PagedQuery
{
}

/// <summary>
/// The coupon form of UC-122 (07 9.7.2; 04 7.2.6; 06 4.13): the code, the kind and its value - a percentage (<see cref="Percentage"/>) or an amount (<see cref="Amount"/>), only the
/// one the kind names is read -, the minimum basket amount, the date range and the total usage count; an existing coupon's form carries its <see cref="Version"/>. The field
/// rules are the project's attributes (09 3.4 item 1); the rules that need the coupon's rights or the other coupons' codes are the module's (07 UC-122).
/// </summary>
public sealed class SaveCouponInput : IFormInput, IValidatableObject
{
    [Display(Name = "Kod")]
    [FieldRequired]
    public string? Code { get; set; }

    /// <summary>The coupon's kind (04 7.2.6.2): <c>Percentage</c> or <c>Amount</c>. 06 keeps no kind; the filled value tells it (06 4.13).</summary>
    [SpecAssignedName]
    [Display(Name = "Tür")]
    [FieldRequired]
    [InvalidListValue(typeof(CouponKind))]
    public string? CouponType { get; set; }

    [Display(Name = "Yüzde")]
    [InvalidPercentage]
    public decimal? Percentage { get; set; }

    [Display(Name = "Tutar")]
    [InvalidAmount(AllowZero = true)]
    public decimal? Amount { get; set; }

    [Display(Name = "Asgari sepet tutarı")]
    [InvalidAmount(AllowZero = true)]
    public decimal? MinimumBasketAmount { get; set; }

    [Display(Name = "Başlangıç")]
    [FieldRequired]
    [InvalidDate]
    public DateOnly? StartDate { get; set; }

    [Display(Name = "Bitiş")]
    [FieldRequired]
    [InvalidDate]
    public DateOnly? EndDate { get; set; }

    [Display(Name = "Toplam kullanım adedi")]
    [FieldRequired]
    [InvalidQuantity(int.MaxValue)]
    public int? TotalUsageLimit { get; set; }

    public int Version { get; set; }

    /// <summary>The kind as a value; null when the field is empty or no kind.</summary>
    public CouponKind? ParsedKind() => Enum.TryParse(CouponType, ignoreCase: false, out CouponKind kind) && Enum.IsDefined(kind) ? kind : null;

    /// <summary>
    /// The fields the kind makes required (04 7.2.6.3, 7.2.6.4; 09 3.4 item 2): the value of the chosen kind, and on a fixed amount the minimum basket amount. The order of the
    /// dates is the module's rule (<c>InvalidDateRange</c>), shown with the module's other problems.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ParsedKind() == CouponKind.Percentage && Percentage is null)
        {
            yield return FieldError.Result(FieldCodes.FieldRequired, nameof(Percentage));
        }

        if (ParsedKind() == CouponKind.Amount && Amount is null)
        {
            yield return FieldError.Result(FieldCodes.FieldRequired, nameof(Amount));
        }

        if (ParsedKind() == CouponKind.Amount && MinimumBasketAmount is null)
        {
            yield return FieldError.Result(FieldCodes.FieldRequired, nameof(MinimumBasketAmount));
        }
    }
}

/// <summary>
/// E-35, the coupons screen (T526; 04 9.7; 07 UC-90, UC-122): the list, newest first, and the coupon's form on the same screen - "Yeni kupon" (<c>/panel/kuponlar/yeni</c>) or a row
/// (<c>/panel/kuponlar/{iç anahtar}</c>). The GET writes nothing. The save is one transaction of the module's write (09 9.1); a change reaches only new orders (K-806). A coupon is
/// never deleted: the campaign is stopped by moving the end earlier (04 9.7.5). When both a version conflict and a field problem arise, the conflict answers first and the
/// field rules are not looked at (07 2.4.8): the version is compared before the page filter runs.
/// </summary>
internal sealed class CouponsModel : PageModel
{
    public const string NewKey = "yeni";

    public const string NewCouponLabel = "Yeni kupon";

    public const string EmptySentence = "Henüz kupon yok.";

    public const string NotStartedText = "Başlamadı";

    public const string ValidText = "Geçerli";

    public const string ExpiredText = "Süresi doldu";

    public const string RightsUsedUpText = "Hakkı doldu";

    public const string PercentageKindText = "Yüzde";

    public const string AmountKindText = "Sabit tutar";

    public const string BackToListLabel = "Listeye dön";

    public const string StopNote = "Kupon silinmez. Kampanyayı durdurmak için bitiş tarihini öne çekin: yeni siparişte kupon kabul edilmez, ayrılmış haklar onaylanmış siparişlerde kalır.";

    public const string ChangeNote = "Değişiklik yalnız yeni siparişlere işler; onaylanmış siparişteki kupon payı değişmez.";

    /// <summary>K-799's texts (04 7.2.6).</summary>
    public const string DuplicateCodeText = "Bu kod başka bir kuponda kullanılıyor.";

    public const string MinimumNotAboveAmountText = "Asgari sepet tutarı kupon tutarından büyük olmalı.";

    public const string PercentageReachesHundredText = "Yüzdesel kupon %100'e ulaşamaz.";

    public const string AmountNotAboveZeroText = "Tutar sıfırdan büyük olmalı.";

    private static readonly string[] FormOrder =
    [
        "Input.Code", "Input.CouponType", "Input.Percentage", "Input.Amount", "Input.MinimumBasketAmount", "Input.StartDate", "Input.EndDate", "Input.TotalUsageLimit",
    ];

    private static readonly InvalidAmountAttribute AmountRule = new() { AllowZero = true };

    private static readonly InvalidPercentageAttribute PercentageRule = new();

    private const decimal Hundred = 100m;

    private const string PercentageField = "Input." + nameof(SaveCouponInput.Percentage);

    /// <summary>The cell of a coupon without a minimum basket amount (a percentage coupon may have none; 02 3.10.4).</summary>
    public const string NoMinimumText = "Yok";

    private static readonly PanelTableColumn[] Columns =
    [
        new("Kod", true),
        new("Tür ve değer", true),
        new("Asgari sepet tutarı", false),
        new("Geçerlilik aralığı", false),
        new("Kullanım", true),
        new("Hâl", true),
    ];

    private readonly ICatalogModule _catalog;
    private readonly ITransactionRunner _transactions;
    private readonly RejectionLog _rejections;

    private bool _posted;
    private bool _loaded;
    private CouponRecord? _formCoupon;
    private (FormErrorSummary Summary, IReadOnlyList<FieldView> Fields)? _built;

    public CouponsModel(ICatalogModule catalog, ITransactionRunner transactions, RejectionLog rejections)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentNullException.ThrowIfNull(rejections);
        _catalog = catalog;
        _transactions = transactions;
        _rejections = rejections;
    }

    /// <summary>The list (04 9.7.3; 2.13.4): a table on the wide class and a card list on the narrow one; null when there is no coupon - the empty row stands instead.</summary>
    public PanelTable? Table { get; private set; }

    public Pager Pager { get; private set; } = new(null, null, []);

    public EmptyStateView? Empty { get; private set; }

    /// <summary>True when the screen shows the form: "Yeni kupon" or a coupon.</summary>
    public bool ShowsForm { get; private set; }

    /// <summary>The form's address: <c>/panel/kuponlar/yeni</c> or the coupon's own.</summary>
    public string FormPath { get; private set; } = PanelFramePaths.Coupons;

    public string FormHeading { get; private set; } = NewCouponLabel;

    /// <summary>The version the form carries: the coupon's current one on a conflict, the one it was opened with on a 422 (07 2.4.5).</summary>
    public int FormVersion { get; private set; }

    public bool IsExisting { get; private set; }

    public bool IsConflicted { get; private set; }

    /// <summary>The used and the reserved rights, read only (04 9.7.4; K-654); null on a new coupon.</summary>
    public string? RightsText { get; private set; }

    // The views carry the model state's messages, and the page filter turns a handler's rule codes into the catalog texts only after the handler returns; so they are drawn on the first read.
    public IReadOnlyList<FieldView> FormFields => Built().Fields;

    public FormErrorSummary Summary => Built().Summary;

    public MessageView? Notice { get; private set; }

    /// <summary>
    /// The coupon save's version is compared before the page filter checks the fields (07 2.4.8): a coupon that is gone is 404 and a changed version is 409 with the current
    /// version in the form, whatever the fields hold. The module compares again under its save (09 10.2 item 2); this is only the order of the answers. The page model's own
    /// filter runs before the project's page filter (the framework orders it first), so a 422 of the field rules reaches here after the filter answered without the handler:
    /// the screen is loaded then, and the percentage that reaches one hundred gets K-799's text in place of the catalog's (04 7.2.6.3).
    /// </summary>
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        if (!string.Equals(context.HandlerMethod?.Name, "SaveCoupon", StringComparison.Ordinal))
        {
            await next();
            return;
        }

        CancellationToken ct = HttpContext.RequestAborted;
        string? key = RouteKey();
        if (key != NewKey && !Guid.TryParse(key, out _))
        {
            context.Result = NotFound();
            return;
        }

        _posted = true;
        SaveCouponInput? input = context.HandlerArguments.Values.OfType<SaveCouponInput>().FirstOrDefault();
        if (Guid.TryParse(key, out Guid couponId))
        {
            CouponRecord? current = await _catalog.GetCouponAsync(couponId, ct);
            if (current is null)
            {
                context.Result = NotFound();
                return;
            }

            // A changed version is answered before the field rules (07 2.4.8): the field problems are dropped so the validation filter lets the request through, and the
            // handler answers 409. The request is not cut here - the inner filters (the panel frame among them) still run.
            if (current.Version != (input?.Version ?? 0))
            {
                ModelState.Clear();
                IsConflicted = true;
            }
        }

        PageHandlerExecutedContext executed = await next();
        if (executed.Exception is null && executed.Result is PageResult { StatusCode: StatusCodes.Status422UnprocessableEntity })
        {
            if (input?.ParsedKind() == CouponKind.Percentage && input.Percentage >= Hundred && ModelState.TryGetValue(PercentageField, out ModelStateEntry? entry) && entry.Errors.Count > 0)
            {
                entry.Errors.Clear();
                ModelState.AddModelError(PercentageField, PercentageReachesHundredText);
            }

            if (!_loaded)
            {
                await LoadAsync(ct);
            }
        }
    }

    public async Task<IActionResult> OnGetAsync(CouponListQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        string? key = RouteKey();
        if (key is not null && key != NewKey && !Guid.TryParse(key, out _))
        {
            return NotFound();
        }

        bool found = await LoadAsync(ct, query.RequestedPage());
        return found ? Page() : NotFound();
    }

    /// <summary>UC-122: saves a new coupon or changes one. 303 to the coupon's own address with the short-lived notice; 422 with every field's message; 409 on the version; 404 when gone.</summary>
    public async Task<IActionResult> OnPostSaveCouponAsync(SaveCouponInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (IsConflicted)
        {
            _rejections.Rejected(ErrorCode.RecordVersionConflict);
            await LoadAsync(ct);
            return FormResults.Conflict(this);
        }

        string? key = RouteKey();
        CouponKind kind = input.ParsedKind() ?? CouponKind.Percentage; // non-null: the field's rules refuse an empty or unknown kind before the handler
        CouponDefinition definition = new(
            input.Code,
            kind,
            kind == CouponKind.Percentage ? input.Percentage : input.Amount,
            input.MinimumBasketAmount,
            input.StartDate ?? default, // non-null: FieldRequired refuses an empty date before the handler
            input.EndDate ?? default,
            input.TotalUsageLimit ?? 0); // non-null: FieldRequired refuses an empty count before the handler

        CouponWriteResult result;
        if (Guid.TryParse(key, out Guid couponId))
        {
            result = await _transactions.RunAsync(token => _catalog.UpdateCouponAsync(couponId, definition, input.Version, token), ct);
        }
        else
        {
            // The filter let only "yeni" and an inner key through.
            result = await _transactions.RunAsync(token => _catalog.CreateCouponAsync(definition, token), ct);
        }

        if (result.IsNotFound)
        {
            return NotFound();
        }

        if (result.CouponId is { } written && !result.IsRejected)
        {
            return FormResults.SeeOther(PanelFramePaths.Coupons + "/" + written.ToString("D"), NoticeCode.Saved);
        }

        if (result.IsVersionConflict)
        {
            _rejections.Rejected(ErrorCode.RecordVersionConflict);
            IsConflicted = true;
            await LoadAsync(ct);
            return FormResults.Conflict(this);
        }

        foreach (CouponProblem problem in result.Problems)
        {
            AddProblem(problem, kind, result.UsedAndReserved);
        }

        await LoadAsync(ct);
        return FormResults.Redisplay(this);
    }

    /// <summary>
    /// One problem of the module on its field: a code of the field rules (07 6.2.2) goes through the page filter's catalog with the rule's limit; a code of the coupon's own rules
    /// (07 6.2.6) carries K-799's text and its rejection line here (09 8.2 item 3).
    /// </summary>
    private void AddProblem(CouponProblem problem, CouponKind kind, int usedAndReserved)
    {
        string field = problem.Field == nameof(CouponDefinition.Value)
            ? "Input." + (kind == CouponKind.Percentage ? nameof(SaveCouponInput.Percentage) : nameof(SaveCouponInput.Amount))
            : "Input." + problem.Field;
        switch (problem.Code)
        {
            case ErrorCode.InvalidPercentage:
                ModelState.AddFieldError(field, FieldCodes.InvalidPercentage, PercentageRule.ParameterText);
                break;
            case ErrorCode.InvalidAmount:
                ModelState.AddFieldError(field, FieldCodes.InvalidAmount, AmountRule.ParameterText);
                break;
            case ErrorCode.CouponValueInvalid:
                _rejections.Rejected(problem.Code);
                ModelState.AddModelError(field, problem.Field == nameof(CouponDefinition.Value) ? AmountNotAboveZeroText : MinimumNotAboveAmountText);
                break;
            case ErrorCode.DuplicateCouponCode:
                _rejections.Rejected(problem.Code);
                ModelState.AddModelError(field, DuplicateCodeText);
                break;
            case ErrorCode.CouponUsageBelowUsed:
                _rejections.Rejected(problem.Code);
                ModelState.AddModelError(field, UsageBelowRightsText(usedAndReserved));
                break;
            default:
                ModelState.AddFieldError(field, problem.Code.ToString());
                break;
        }
    }

    private string? RouteKey() => RouteData.Values.TryGetValue("key", out object? value) ? value as string : null;

    /// <summary>Loads the list and, when the address names one, the form; false when the address names a coupon that does not exist.</summary>
    private async Task<bool> LoadAsync(CancellationToken ct, int page = PageNumber.First)
    {
        _loaded = true;
        CouponListPage list = await _catalog.ListCouponsAsync(page, ct);
        Table = list.Items.Count == 0 ? null : new PanelTable("Kuponlar", Columns, [.. list.Items.Select(RowOf)]);
        Pager = Pager.Of(PanelFramePaths.Coupons, new CouponListQuery(), list.PageNumber, list.PageCount);
        Empty = list.Items.Count == 0
            ? EmptyStates.WithFirstRecordButton(EmptySentence, NewCouponLabel, PanelFramePaths.Coupons + "/" + NewKey)
            : null;
        NoticeCode? notice = NoticeParameter.Read(Request, NoticeKind.ShortLived);
        Notice = notice is { } code && NoticeTexts.TryShortLived(code, out string? text) ? Messages.ShortLivedSuccess(text) : null;

        string? key = RouteKey();
        if (key is null)
        {
            return true;
        }

        CouponRecord? coupon = null;
        if (key != NewKey)
        {
            coupon = Guid.TryParse(key, out Guid couponId) ? await _catalog.GetCouponAsync(couponId, ct) : null;
            if (coupon is null)
            {
                return false;
            }
        }

        ShowsForm = true;
        IsExisting = coupon is not null;
        FormPath = PanelFramePaths.Coupons + "/" + key;
        FormHeading = coupon is null ? NewCouponLabel : coupon.Code;
        FormVersion = coupon is null ? 0 : IsConflicted || !_posted ? coupon.Version : PostedVersion(coupon.Version);
        RightsText = coupon is null ? null : $"Kullanılmış: {DisplayFormat.Number(coupon.UsedCount)} · Ayrılmış: {DisplayFormat.Number(coupon.ReservedCount)}";
        _formCoupon = coupon;
        _built = null;
        return true;
    }

    private (FormErrorSummary Summary, IReadOnlyList<FieldView> Fields) Built()
    {
        if (_built is { } built)
        {
            return built;
        }

        FormErrorSummary summary = FormErrorSummary.From(ModelState, FormOrder);
        _built = (summary, ShowsForm ? Fields.BindAll(FieldsOf(_formCoupon), summary) : []);
        return _built.Value;
    }

    /// <summary>K-799's text of a usage count under the used and reserved rights (04 7.2.6.6, 9.7.6): the floor is the server's number.</summary>
    public static string UsageBelowRightsText(int usedAndReserved) =>
        $"Kullanım adedi, kullanılmış ve ayrılmış {DisplayFormat.Number(usedAndReserved)} hakkın altına indirilemez. Kampanyayı durdurmak için bitiş tarihini öne çekin.";

    private int PostedVersion(int current) => int.TryParse(Request.Form["Input.Version"], out int sent) ? sent : current;

    /// <summary>The form's fields: the sent texts when the form is shown again, the coupon's values when it is opened, empty for a new coupon (07 2.2.2).</summary>
    private List<FieldView> FieldsOf(CouponRecord? coupon)
    {
        string? Value(string name, string? stored) => _posted ? Request.Form["Input." + name].ToString() : stored;
        string? kind = Value(nameof(SaveCouponInput.CouponType), coupon?.Kind.ToString());
        List<FieldOption> kinds = [new(nameof(CouponKind.Percentage), PercentageKindText), new(nameof(CouponKind.Amount), AmountKindText)];
        return
        [
            Fields.Text("Input.Code", "Kod", FieldRequirement.Required, Value(nameof(SaveCouponInput.Code), coupon?.Code)),
            Fields.ClosedList("Input.CouponType", "Tür", FieldRequirement.Required, kinds, kind),
            Fields.Text("Input.Percentage", "Yüzde — yüzdesel kuponda", FieldRequirement.Optional, Value(nameof(SaveCouponInput.Percentage), coupon?.Kind == CouponKind.Percentage ? RequestFormat.FormatDecimal(coupon.Value) : null)),
            Fields.Text("Input.Amount", "Tutar — sabit tutarlı kuponda", FieldRequirement.Optional, Value(nameof(SaveCouponInput.Amount), coupon?.Kind == CouponKind.Amount ? RequestFormat.FormatDecimal(coupon.Value) : null)),
            Fields.Text(
                "Input.MinimumBasketAmount",
                "Asgari sepet tutarı — sabit tutarlı kuponda zorunlu",
                FieldRequirement.Optional,
                Value(nameof(SaveCouponInput.MinimumBasketAmount), coupon?.MinimumBasketAmount is { } minimum ? RequestFormat.FormatDecimal(minimum) : null)),
            Fields.Date("Input.StartDate", "Başlangıç", FieldRequirement.Required, Value(nameof(SaveCouponInput.StartDate), coupon is null ? null : RequestFormat.FormatDate(coupon.StartDate))),
            Fields.Date("Input.EndDate", "Bitiş", FieldRequirement.Required, Value(nameof(SaveCouponInput.EndDate), coupon is null ? null : RequestFormat.FormatDate(coupon.EndDate))),
            Fields.Number("Input.TotalUsageLimit", "Toplam kullanım adedi", FieldRequirement.Required, Value(nameof(SaveCouponInput.TotalUsageLimit), coupon is null ? null : coupon.TotalUsageLimit.ToString(System.Globalization.CultureInfo.InvariantCulture))),
        ];
    }

    /// <summary>One row of the list (04 9.7.3): code, kind and value, minimum basket amount, date range, use - used, reserved and total - and the validity line.</summary>
    public static PanelTableRow RowOf(CouponRecord coupon) => new(
        [
        coupon.Code,
        coupon.Kind == CouponKind.Percentage ? $"{PercentageKindText} · {DisplayFormat.Percent(coupon.Value)}" : $"{AmountKindText} · {DisplayFormat.Amount(coupon.Value)}",
        coupon.MinimumBasketAmount is { } minimum ? DisplayFormat.Amount(minimum) : NoMinimumText,
        DisplayFormat.DateRange(coupon.StartDate, coupon.EndDate),
        $"{DisplayFormat.Number(coupon.UsedCount)} kullanıldı · {DisplayFormat.Number(coupon.ReservedCount)} ayrıldı · {DisplayFormat.Number(coupon.TotalUsageLimit)} toplam",
        ValidityText(coupon.Validity),
        ],
        PanelFramePaths.Coupons + "/" + coupon.Id.ToString("D"));

    /// <summary>The validity line (04 9.7.3): text, not a badge - a coupon has no state machine (03 1.10.2).</summary>
    public static string ValidityText(CouponValidity validity) => validity switch
    {
        CouponValidity.NotStarted => NotStartedText,
        CouponValidity.Valid => ValidText,
        CouponValidity.Expired => ExpiredText,
        CouponValidity.RightsUsedUp => RightsUsedUpText,
        _ => throw new ArgumentOutOfRangeException(nameof(validity), validity, "A coupon's validity is one of four lines (04 9.7.3)."),
    };
}
