using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Pages.Components;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The query model of the sales summary (E-51; 07 5.3.2 <c>SalesSummaryQuery</c>; UC-105): the period - one of the ready periods or <c>Custom</c> - and, for <c>Custom</c>, the two dates. Every field is text
/// so that a wrong value is never a binding error (07 2.6.6): a period outside the list is ignored and the default period is shown.
/// </summary>
public sealed class SalesSummaryQuery : IQueryModel
{
    [SpecAssignedName]
    public string? Period { get; init; }

    [SpecAssignedName]
    public string? StartDate { get; init; }

    [SpecAssignedName]
    public string? EndDate { get; init; }
}

/// <summary>One number card (04 9.22.4): the name, the value and - for the revenue - the line under it.</summary>
public sealed record SummaryNumberView(string Id, string Name, string Value, string? Note);

/// <summary>
/// One measure card (04 9.22.5): the name, the one-sentence summary of the definition, and either the percentage with its numerator and denominator or the count. A ratio whose denominator is zero shows
/// no percentage: <see cref="Value"/> is "Değerlendirilemez" and <see cref="Detail"/> says why (K-490, K-821).
/// </summary>
public sealed record SummaryMeasureView(string Id, string Name, string Value, string? Detail, string Definition);

/// <summary>One row of the best sellers (04 9.22.6): the product, the units and their amount.</summary>
public sealed record TopSellerView(string Product, string Quantity, string Amount);

/// <summary>One row of the payment method distribution (04 9.22.6): the method, the order count and its share.</summary>
public sealed record PaymentMethodView(string Method, string Count, string Share);

/// <summary>
/// E-51, the sales summary (T524; 04 9.22; 07 UC-105): the period choice and the three regions - the numbers, the four measures and the two distributions. Read only: the summary is the server's computation
/// at the request, from the records as they are, and nothing is stored (02 10.6.2; K-483). A request with no parameter is "Son 30 gün"; "Ölçüm penceresi" is offered only when sales were ever opened
/// (K-486). A custom range that is missing a date, has a future date or ends before it starts is a field message with a 200 and no summary (07 9.22.1). Rows lead to no other screen (K-821).
/// </summary>
internal sealed class SalesSummaryModel : PageModel
{
    /// <summary>The value of the custom range in the period choice (07 5.3.2).</summary>
    public const string CustomPeriod = "Custom";

    public const string PeriodField = "period";

    public const string StartField = "startDate";

    public const string EndField = "endDate";

    public const string NotAssessable = "Değerlendirilemez";

    /// <summary>The line under the revenue card (04 9.22.4; K-821).</summary>
    public const string RevenueNote = "Geri ödemeler düşülmez; iptal ve iade sayısı ayrıca gösterilir.";

    public const string NoTopSellerText = "Bu dönemde satılan ürün yok.";

    public const string NoPaymentText = "Bu dönemde ödemesi tamamlanmış sipariş yok.";

    /// <summary>The ready periods in the order of 04 9.22.3, with their labels; the custom range follows them.</summary>
    public static readonly IReadOnlyList<(SalesSummaryPreset Preset, string Label)> Presets =
    [
        (SalesSummaryPreset.Today, "Bugün"),
        (SalesSummaryPreset.Yesterday, "Dün"),
        (SalesSummaryPreset.Last7Days, "Son 7 gün"),
        (SalesSummaryPreset.Last30Days, "Son 30 gün"),
        (SalesSummaryPreset.ThisMonth, "Bu ay"),
        (SalesSummaryPreset.LastMonth, "Geçen ay"),
        (SalesSummaryPreset.MeasurementWindow, "Ölçüm penceresi"),
    ];

    public const string CustomLabel = "Özel aralık";

    private readonly ISalesSummary _summary;
    private readonly TimeProvider _time;

    public SalesSummaryModel(ISalesSummary summary, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(summary);
        ArgumentNullException.ThrowIfNull(time);
        _summary = summary;
        _time = time;
    }

    public DateOnly Today { get; private set; }

    /// <summary>The period choice as the form draws it: the offered options and the selected value.</summary>
    public IReadOnlyList<FieldOption> PeriodOptions { get; private set; } = [];

    public string SelectedPeriod { get; private set; } = nameof(SalesSummaryPreset.Last30Days);

    public string? StartDate { get; private set; }

    public string? EndDate { get; private set; }

    /// <summary>The start and end of the selected period, written beside the choice (04 9.22.3); null when a custom range was refused.</summary>
    public string? PeriodText { get; private set; }

    /// <summary>The summary of the period; null when a custom range was refused and nothing was computed.</summary>
    public SalesSummary? Summary { get; private set; }

    public IReadOnlyList<SummaryNumberView> Numbers { get; private set; } = [];

    public IReadOnlyList<SummaryMeasureView> Measures { get; private set; } = [];

    public IReadOnlyList<TopSellerView> TopSellers { get; private set; } = [];

    public IReadOnlyList<PaymentMethodView> PaymentMethods { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(SalesSummaryQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        Today = TurkeyTime.DateOf(_time.GetUtcNow());
        DateOnly? salesOpenDay = await _summary.FindSalesOpenDayAsync(ct);
        bool windowOffered = SalesSummaryPeriod.Of(SalesSummaryPreset.MeasurementWindow, Today, salesOpenDay) is not null;
        PeriodOptions =
        [
            .. Presets.Where(p => p.Preset != SalesSummaryPreset.MeasurementWindow || windowOffered).Select(p => new FieldOption(p.Preset.ToString(), p.Label)),
            new FieldOption(CustomPeriod, CustomLabel),
        ];

        SalesSummaryPeriod? period;
        if (string.Equals(query.Period, CustomPeriod, StringComparison.Ordinal))
        {
            SelectedPeriod = CustomPeriod;
            StartDate = query.StartDate;
            EndDate = query.EndDate;
            DateRangeVerdict verdict = DateFields.CheckRange(query.StartDate, query.EndDate, Today, DateRangePolicy.NoFuture, startRequired: true, endRequired: true);
            if (!verdict.IsAccepted)
            {
                // A refused range is a field message; the summary is not computed and the answer is 200 (07 9.22.1; 2.6.6).
                DateFields.AddRefusal(ModelState, StartField, EndField, verdict);
                return Page();
            }

            period = SalesSummaryPeriod.Custom(verdict.Start, verdict.End, Today).Period;
        }
        else
        {
            // A value outside the offered list - an unknown word, a number, the window when sales never opened - is ignored: the default period (07 9.22.1; 2.6.6).
            SalesSummaryPreset preset = ReadPreset(query.Period, windowOffered) ?? SalesSummaryPreset.Last30Days;
            SelectedPeriod = preset.ToString();
            period = SalesSummaryPeriod.Of(preset, Today, salesOpenDay);
        }

        if (period is null)
        {
            return Page();
        }

        PeriodText = DisplayFormat.DateRange(period.StartDay, period.EndDay);
        SalesSummary summary = await _summary.GetAsync(period, ct);
        Summary = summary;
        Numbers =
        [
            new("summary-order-count", "Sipariş sayısı", DisplayFormat.Number(summary.OrderCount), null),
            new("summary-revenue", "Ciro", DisplayFormat.Amount(summary.Revenue), RevenueNote),
            new("summary-cancellation-count", "İptal ve iade sayısı", DisplayFormat.Number(summary.CancellationAndReturnCount), null),
        ];
        Measures =
        [
            Ratio("summary-payment-completion", "Ödeme tamamlama oranı", summary.PaymentCompletion, "sipariş onayına gelen sepet", "Sipariş onayına gelen sepetlerden ödemesi tamamlananların payı."),
            new("summary-contact-requests", "İletişim talebi sayısı", DisplayFormat.Number(summary.ContactRequestCount), null, "Dönemde gelen iletişim talepleri; \"KVKK talebi\" ve \"Sipariş hakkında\" tipi dışındakiler."),
            Ratio("summary-shipping-promise", "Kargoya verme sözüne uyum oranı", summary.ShippingPromiseCompliance, "kargoya verme süresi dolan sipariş", "Kargoya verme süresi dolan siparişlerden süresi içinde kargoya verilenlerin payı."),
            Ratio("summary-cancellation-ratio", "İptal ve iade oranı", summary.CancellationAndReturn, "ödemesi tamamlanmış sipariş", "Dönemde ödemesi tamamlanan siparişlerden iptal ya da iade görenlerin payı."),
        ];
        TopSellers = [.. summary.TopSellers.Select(t => new TopSellerView(t.ProductName, DisplayFormat.Number(t.Quantity), DisplayFormat.Amount(t.Amount)))];
        int paid = summary.PaymentMethods.Sum(m => m.OrderCount);
        PaymentMethods = [.. summary.PaymentMethods.Where(m => m.OrderCount > 0).Select(m => new PaymentMethodView(MethodName(m.Method), DisplayFormat.Number(m.OrderCount), Share(m.OrderCount, paid)))];
        return Page();
    }

    /// <summary>The ready period a parameter names, or null when it names none of the offered ones.</summary>
    internal static SalesSummaryPreset? ReadPreset(string? value, bool windowOffered)
    {
        foreach ((SalesSummaryPreset preset, _) in Presets)
        {
            if (string.Equals(value, preset.ToString(), StringComparison.Ordinal))
            {
                return preset == SalesSummaryPreset.MeasurementWindow && !windowOffered ? null : preset;
            }
        }

        return null;
    }

    /// <summary>A ratio card (04 9.22.5): the percentage and under it the numerator and the denominator; a zero denominator is "Değerlendirilemez" with the reason (K-490, K-821).</summary>
    internal static SummaryMeasureView Ratio(string id, string name, SummaryRatio ratio, string denominatorName, string definition)
    {
        ArgumentNullException.ThrowIfNull(ratio);
        if (!ratio.IsAssessable)
        {
            return new(id, name, NotAssessable, $"{NotAssessable} — bu dönemde {denominatorName} yok.", definition);
        }

        return new(id, name, Share(ratio.Numerator, ratio.Denominator), $"{DisplayFormat.Number(ratio.Numerator)} / {DisplayFormat.Number(ratio.Denominator)}", definition);
    }

    private static string Share(int part, int whole) => DisplayFormat.Percent(Math.Round(100m * part / whole, 1, MidpointRounding.AwayFromZero));

    private static string MethodName(PaymentMethod method) => method switch
    {
        PaymentMethod.Card => "Kart",
        PaymentMethod.BankTransfer => "Havale/EFT",
        _ => method.ToString(),
    };
}
