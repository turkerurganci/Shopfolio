using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Validation;

namespace Shopfolio.IntegrationTests.Pages;

/// <summary>
/// The values the two display samples show (T216 KK1, KK2; 04 8.2): the storefront page and the panel page write the same values through
/// <see cref="DisplayFormat"/>, so the same value is the same text on both. The two instants sit on both sides of a Turkey midnight.
/// </summary>
public static class FormatSampleValues
{
    public static readonly DateOnly Day = new(2026, 10, 4);

    public static readonly DateOnly RangeEnd = new(2026, 10, 31);

    /// <summary>11:30 UTC: 14:30 Turkey time on 4 October.</summary>
    public static readonly DateTimeOffset Instant = new(2026, 10, 4, 11, 30, 0, TimeSpan.Zero);

    /// <summary>The last second of 4 October in Turkey time (20:59:59 UTC).</summary>
    public static readonly DateTimeOffset BeforeMidnight = new(2026, 10, 4, 20, 59, 59, TimeSpan.Zero);

    /// <summary>The Turkey midnight between 4 and 5 October (21:00 UTC): the instant is on 5 October.</summary>
    public static readonly DateTimeOffset AtMidnight = TurkeyTime.StartOf(new DateOnly(2026, 10, 5));

    public const long Count = 1250;

    public const decimal Fraction = 0.5m;

    public const decimal Rate = 12.5m;

    public const decimal AmountWithKurus = 266.67m;

    public const decimal WholeAmount = 1200m;

    public const decimal Deduction = -50m;

    public const int Days = 3;

    public const int BusinessDays = 2;

    public const string Phone = "5321234567";

    public const string Iban = "TR330006100519786457841326";

    /// <summary>The texts both pages must show, in the order they show them: the display format of each value.</summary>
    public static IReadOnlyList<(string Name, string Text)> Texts() =>
    [
        ("date", DisplayFormat.Date(Day)),
        ("range", DisplayFormat.DateRange(Day, RangeEnd)),
        ("time", DisplayFormat.Time(Instant)),
        ("date-time", DisplayFormat.DateAndTime(Instant)),
        ("before-midnight", DisplayFormat.DateAndTime(BeforeMidnight)),
        ("at-midnight", DisplayFormat.DateAndTime(AtMidnight)),
        ("count", DisplayFormat.Number(Count)),
        ("fraction", DisplayFormat.Number(Fraction)),
        ("rate", DisplayFormat.Percent(Rate)),
        ("amount", DisplayFormat.Amount(AmountWithKurus)),
        ("whole-amount", DisplayFormat.Amount(WholeAmount)),
        ("deduction", DisplayFormat.Amount(Deduction)),
        ("days", DisplayFormat.Days(Days)),
        ("business-days", DisplayFormat.BusinessDays(BusinessDays)),
        ("phone", DisplayFormat.Phone(Phone)),
        ("iban", DisplayFormat.Iban(Iban)),
    ];
}

/// <summary>The storefront display sample (<c>/bicim-ornegi</c>); the markup writes every value with <see cref="DisplayFormat"/>.</summary>
public sealed class StorefrontFormatSampleModel : PageModel
{
    public static void OnGet()
    {
    }
}

/// <summary>The panel display sample (<c>/panel/bicim-ornegi</c>); the same values, the same calls.</summary>
public sealed class PanelFormatSampleModel : PageModel
{
    public static void OnGet()
    {
    }
}

/// <summary>The limits of the format form's fields, written once (09 3.4 items 1, 6).</summary>
public static class FormatFormLimits
{
    public const double PriceMax = 100000;

    public const int QuantityMax = 1000;
}

/// <summary>
/// The test-only form of the request formats (T216 KK3-KK6; 07 2.9.3-2.9.5, 2.9.10; 09 7.2 items 2-4): an amount, a percentage, two dates, a quantity,
/// an IBAN and a required text, posted to <c>SavePrice</c>; and a query with an amount and a date read by the same binder on a GET (07 2.6.6).
/// </summary>
public sealed class FormatFormModel : PageModel
{
    private readonly FormatRecordStore _store;

    public FormatFormModel(FormatRecordStore store)
    {
        _store = store;
    }

    public string Applied { get; private set; } = string.Empty;

    public IReadOnlyList<SavedPrice> Saved => _store.All();

    public IActionResult OnGetFind(PriceQuery query)
    {
        Applied = ModelState.IsValid
            ? $"{(query.MinPrice is { } min ? RequestFormat.FormatDecimal(min) : "-")}|{(query.From is { } from ? RequestFormat.FormatDate(from) : "-")}"
            : "none";
        return Page();
    }

    public IActionResult OnPostSavePrice(SavePriceInput input)
    {
        if (!ModelState.IsValid)
        {
            return FormResults.Redisplay(this);
        }

        _store.Add(new SavedPrice(input.Price, input.Rate, input.StartsOn, input.EndsOn, input.Quantity, RequestFormat.CompactIban(input.Iban), input.Note, input.Deposit));
        return FormResults.SeeOther("/panel/bicim-formu");
    }
}

/// <summary>The input model of <c>SavePrice</c>.</summary>
public sealed class SavePriceInput : IFormInput
{
    [Display(Name = "Fiyat")]
    [InvalidAmount(Max = FormatFormLimits.PriceMax)]
    public decimal Price { get; set; }

    [Display(Name = "İndirim oranı")]
    [InvalidPercentage]
    public decimal? Rate { get; set; }

    [Display(Name = "Başlangıç")]
    [InvalidDate]
    public DateOnly? StartsOn { get; set; }

    /// <summary>A date the type cannot leave empty: an empty value is "boş olamaz" (the binder's empty path).</summary>
    [Display(Name = "Bitiş")]
    [InvalidDate]
    public DateOnly EndsOn { get; set; }

    [Display(Name = "Adet")]
    [InvalidQuantity(FormatFormLimits.QuantityMax)]
    public int Quantity { get; set; }

    [Display(Name = "IBAN")]
    [InvalidIban]
    public string Iban { get; set; } = string.Empty;

    [Display(Name = "Not")]
    [FieldRequired]
    public string Note { get; set; } = string.Empty;

    /// <summary>A decimal without its rule - the model rule refuses it in the application (<c>DecimalWithoutRule</c>); here it shows the binder's fallback code.</summary>
    [Display(Name = "Kapora")]
    public decimal? Deposit { get; set; }
}

/// <summary>The query model of the <c>Find</c> GET handler: an amount and a date in the query (07 2.9.3, 2.9.4).</summary>
public sealed class PriceQuery : IQueryModel
{
    [Display(Name = "En düşük fiyat")]
    [InvalidAmount(AllowZero = true)]
    public decimal? MinPrice { get; set; }

    [Display(Name = "Başlangıç")]
    [InvalidDate]
    public DateOnly? From { get; set; }
}

/// <summary>A saved price as the binder gave its values.</summary>
public sealed record SavedPrice(decimal Price, decimal? Rate, DateOnly? StartsOn, DateOnly EndsOn, int Quantity, string Iban, string Note, decimal? Deposit);

/// <summary>The saved prices of the sample form, in memory; a singleton of the test host.</summary>
public sealed class FormatRecordStore
{
    private readonly List<SavedPrice> _saved = [];
    private readonly Lock _gate = new();

    public IReadOnlyList<SavedPrice> All()
    {
        lock (_gate)
        {
            return [.. _saved];
        }
    }

    public void Add(SavedPrice price)
    {
        lock (_gate)
        {
            _saved.Add(price);
        }
    }
}
