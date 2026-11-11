using System.Globalization;

using Microsoft.AspNetCore.Mvc.ModelBinding;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Validation;

namespace Shopfolio.Pages.Components;

/// <summary>What the event date rule says about a sent date (04 2.12.5.1).</summary>
public enum EventDateOutcome
{
    Accepted,

    /// <summary>Nothing was sent; the caller decides whether the field is required.</summary>
    Missing,

    /// <summary>The text is not a calendar date in the form the date input sends.</summary>
    Malformed,

    /// <summary>A date after today is refused: an event that has not happened has no date yet.</summary>
    InTheFuture,

    /// <summary>A date before the lower limit the record gives (the shipping day, the delivery date) is refused with the limit as its reason.</summary>
    BelowLowerBound,
}

/// <summary>The lower limit of an event date and its name for the message: "kargoya verildiği gün" with its day (03 8.2.6, 8.3.2.1, 8.4.4). A date with no limit has none (03 8.3.2.1: no delivery date, no lower limit).</summary>
/// <param name="Day">The earliest day that is accepted.</param>
/// <param name="Name">What the day is, in words that finish "Tarih, … olan …" (the message says it).</param>
public sealed record EventDateLimit(DateOnly Day, string Name);

/// <summary>The answer of the event date rule: the outcome, the date when it is a date, and the reason to say at the field when the date is refused for its limit.</summary>
public sealed record EventDateVerdict(EventDateOutcome Outcome, DateOnly? Date, string? Reason)
{
    public bool IsAccepted => Outcome == EventDateOutcome.Accepted;
}

/// <summary>How a date range is bounded (04 2.12.5.3, 7.3.2.12): a discount, a coupon and an announcement run in the future too; the report and the log filters refuse a future date.</summary>
public enum DateRangePolicy
{
    /// <summary>The discount's, the coupon's and the announcement's range: a future date is free (04 2.12.5.3).</summary>
    FutureAllowed,

    /// <summary>The report and the log filters' range: a future date is refused (04 7.3.2.12; 9.22.9, 9.23.9, 9.24.8).</summary>
    NoFuture,
}

/// <summary>What the date range rule says (04 2.12.5.3, 7.3.2.12): which of the two dates the refusal belongs to.</summary>
public enum DateRangeOutcome
{
    Accepted,
    StartMissing,
    EndMissing,
    StartMalformed,
    EndMalformed,
    StartInTheFuture,
    EndInTheFuture,

    /// <summary>The end is before the start (<c>InvalidDateRange</c>).</summary>
    EndBeforeStart,
}

/// <summary>The answer of the range rule: the outcome and, for an accepted range, the two days (a date that was left out of an optional pair is null).</summary>
public sealed record DateRangeVerdict(DateRangeOutcome Outcome, DateOnly? Start, DateOnly? End)
{
    public bool IsAccepted => Outcome == DateRangeOutcome.Accepted;
}

/// <summary>The instants a date range runs between (02 4.1.4; T378 k7): the 00:00 of the start day and the 00:00 of the day after the end day - the whole end day is in.</summary>
public sealed record DateRangeInstants(DateTimeOffset? From, DateTimeOffset? Until);

/// <summary>The order the panel asks when the entered date is the day of a withdrawal declaration (04 2.12.5.2; 03 2.8.1.3).</summary>
public enum SequenceOutcome
{
    /// <summary>The date does not coincide with a declaration day: no question.</summary>
    NotAsked,

    /// <summary>The question is asked and has no answer.</summary>
    Missing,

    /// <summary>The answer is not one of the two choices.</summary>
    NotOnTheList,

    Answered,
}

/// <summary>The answer of the sequence rule: whether the question is asked, the answer and - when answered - whether the delivery was before the declaration.</summary>
public sealed record SequenceVerdict(SequenceOutcome Outcome, bool? DeliveredBeforeDeclaration)
{
    /// <summary>True when the form may go on: no question, or the question is answered.</summary>
    public bool MayContinue => Outcome is SequenceOutcome.NotAsked or SequenceOutcome.Answered;
}

/// <summary>
/// The date fields (T544; 04 OB-16, 2.12.5): the event date, the sequence question and the date range. The rules are pure: today is a day the caller gives (from the one time source and
/// <see cref="TurkeyTime"/>, 09 7.1), never read here, and every range instant goes through <see cref="TurkeyTime"/> (T378 k7) - this class has no second Turkey time.
/// </summary>
public static class DateFields
{
    /// <summary>The value the date input sends and the form keeps.</summary>
    public const string InputFormat = "yyyy-MM-dd";

    /// <summary>The choice that says the delivery was before the declaration.</summary>
    public const string BeforeDeclaration = "BeforeDeclaration";

    /// <summary>The choice that says the delivery was after the declaration.</summary>
    public const string AfterDeclaration = "AfterDeclaration";

    /// <summary>The label of the sequence question.</summary>
    public const string SequenceLabel = "Teslim, cayma beyanından önce mi sonra mı oldu?";

    /// <summary>The refusal of a date after today.</summary>
    public const string FutureMessage = "Tarih bugünden sonra olamaz.";

    /// <summary>The refusal of a range date after today where the range takes none (the report and the log filters).</summary>
    public const string RangeFutureMessage = "İleri tarih seçilemez.";

    /// <summary>The date a text names, or null: the strict <c>yyyy-MM-dd</c> of the date input, the surrounding white space dropped; anything else is not a date.</summary>
    public static DateOnly? Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return DateOnly.TryParseExact(text.Trim(), InputFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly day) ? day : null;
    }

    /// <summary>The text of a day as the date input takes and sends it.</summary>
    public static string Format(DateOnly day) => day.ToString(InputFormat, CultureInfo.InvariantCulture);

    /// <summary>
    /// The event date rule (04 2.12.5.1): a date in the past is entered freely; a date after <paramref name="today"/> is refused, and so is a date before the lower limit - the refusal says why. A limit
    /// of null means none. The limit's own day is accepted: only the earlier days are refused (03 8.2.6).
    /// </summary>
    public static EventDateVerdict CheckEvent(string? text, DateOnly today, EventDateLimit? lowerBound = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new EventDateVerdict(EventDateOutcome.Missing, null, null);
        }

        if (Parse(text) is not { } day)
        {
            return new EventDateVerdict(EventDateOutcome.Malformed, null, null);
        }

        if (day > today)
        {
            return new EventDateVerdict(EventDateOutcome.InTheFuture, day, FutureMessage);
        }

        if (lowerBound is not null && day < lowerBound.Day)
        {
            return new EventDateVerdict(EventDateOutcome.BelowLowerBound, day, BelowLimitMessage(lowerBound));
        }

        return new EventDateVerdict(EventDateOutcome.Accepted, day, null);
    }

    /// <summary>The refusal of a date before the lower limit: the limit and its day are in the message.</summary>
    public static string BelowLimitMessage(EventDateLimit limit)
    {
        ArgumentNullException.ThrowIfNull(limit);
        return $"Tarih, {limit.Name} olan {DisplayFormat.Date(limit.Day)} tarihinden önce olamaz.";
    }

    /// <summary>The event date field: the date input offers nothing after today and nothing before the limit (hints; the rule above decides).</summary>
    public static FieldView EventDate(string name, string label, FieldRequirement requirement, DateOnly today, string? value = null, EventDateLimit? lowerBound = null) =>
        Fields.Date(name, label, requirement, value, min: lowerBound?.Day, max: today);

    /// <summary>
    /// Puts the refusal of an event date into the model state as the text the field shows (04 2.3.2): the missing date and the malformed one are the catalog's codes with the field's label, a refused
    /// date says its reason. An accepted date adds nothing; a missing date of an optional field is not asked here - the caller does not call this for it.
    /// </summary>
    public static void AddRefusal(ModelStateDictionary modelState, string name, string label, EventDateVerdict verdict)
    {
        ArgumentNullException.ThrowIfNull(modelState);
        ArgumentNullException.ThrowIfNull(verdict);
        switch (verdict.Outcome)
        {
            case EventDateOutcome.Accepted:
                break;
            case EventDateOutcome.Missing:
                modelState.AddFieldError(name, FieldCodes.FieldRequired);
                break;
            case EventDateOutcome.Malformed:
                modelState.AddFieldError(name, FieldCodes.InvalidDate);
                break;
            default:
                modelState.AddModelError(name, verdict.Reason ?? string.Empty);
                break;
        }
    }

    /// <summary>True when the sequence question is asked: the entered day is one of the declaration days (04 2.12.5.2; 03 2.8.1.3, 8.2.6, 8.4.4).</summary>
    public static bool IsSequenceAsked(DateOnly entered, IReadOnlyCollection<DateOnly> declarationDays)
    {
        ArgumentNullException.ThrowIfNull(declarationDays);
        return declarationDays.Contains(entered);
    }

    /// <summary>The sequence question as a field of the form, or null when it is not asked (04 2.12.5.2): a required closed list - the question is never a window.</summary>
    public static FieldView? SequenceQuestion(string name, DateOnly? entered, IReadOnlyCollection<DateOnly> declarationDays, string? answer = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (entered is not { } day || !IsSequenceAsked(day, declarationDays))
        {
            return null;
        }

        return Fields.ClosedList(
            name,
            SequenceLabel,
            FieldRequirement.Required,
            [new FieldOption(BeforeDeclaration, "Beyandan önce"), new FieldOption(AfterDeclaration, "Beyandan sonra")],
            answer);
    }

    /// <summary>
    /// The sequence rule: no question when the date is no declaration day; otherwise the answer is required and is one of the two choices. The answer says whether the delivery was before the declaration,
    /// which sets the start of the refund period (03 2.8.1.3).
    /// </summary>
    public static SequenceVerdict CheckSequence(DateOnly entered, IReadOnlyCollection<DateOnly> declarationDays, string? answer)
    {
        if (!IsSequenceAsked(entered, declarationDays))
        {
            return new SequenceVerdict(SequenceOutcome.NotAsked, null);
        }

        return answer switch
        {
            null or "" => new SequenceVerdict(SequenceOutcome.Missing, null),
            BeforeDeclaration => new SequenceVerdict(SequenceOutcome.Answered, true),
            AfterDeclaration => new SequenceVerdict(SequenceOutcome.Answered, false),
            _ => new SequenceVerdict(SequenceOutcome.NotOnTheList, null),
        };
    }

    /// <summary>
    /// The range rule (04 2.12.5.3, 7.3.2.12): the end is never before the start; a future date is free in a discount's, a coupon's and an announcement's range and refused in the report and log filters.
    /// An announcement's two dates are optional (02 3.27.21): with both flags false a missing date is no refusal and a range with one date is a range.
    /// </summary>
    public static DateRangeVerdict CheckRange(string? startText, string? endText, DateOnly today, DateRangePolicy policy, bool startRequired, bool endRequired)
    {
        bool startGiven = !string.IsNullOrWhiteSpace(startText);
        bool endGiven = !string.IsNullOrWhiteSpace(endText);
        if (!startGiven && startRequired)
        {
            return new DateRangeVerdict(DateRangeOutcome.StartMissing, null, null);
        }

        if (!endGiven && endRequired)
        {
            return new DateRangeVerdict(DateRangeOutcome.EndMissing, null, null);
        }

        DateOnly? start = Parse(startText);
        DateOnly? end = Parse(endText);
        if (startGiven && start is null)
        {
            return new DateRangeVerdict(DateRangeOutcome.StartMalformed, null, null);
        }

        if (endGiven && end is null)
        {
            return new DateRangeVerdict(DateRangeOutcome.EndMalformed, null, null);
        }

        if (policy == DateRangePolicy.NoFuture)
        {
            if (start > today)
            {
                return new DateRangeVerdict(DateRangeOutcome.StartInTheFuture, start, end);
            }

            if (end > today)
            {
                return new DateRangeVerdict(DateRangeOutcome.EndInTheFuture, start, end);
            }
        }

        if (start is { } from && end is { } to && to < from)
        {
            return new DateRangeVerdict(DateRangeOutcome.EndBeforeStart, start, end);
        }

        return new DateRangeVerdict(DateRangeOutcome.Accepted, start, end);
    }

    /// <summary>
    /// The instants of an accepted range (02 4.1.4; T378 k7): the range begins at the 00:00 of the start day and ends at the 00:00 of the day after the end day - the end day is in as a whole, to 23.59. A date that was
    /// left out gives no instant. The instants are <see cref="TurkeyTime"/>'s.
    /// </summary>
    public static DateRangeInstants InstantsOf(DateRangeVerdict verdict)
    {
        ArgumentNullException.ThrowIfNull(verdict);
        if (!verdict.IsAccepted)
        {
            throw new ArgumentException("Only an accepted range has instants.", nameof(verdict));
        }

        return new DateRangeInstants(
            verdict.Start is { } start ? TurkeyTime.StartOf(start) : null,
            verdict.End is { } end ? TurkeyTime.EndOfRange(end) : null);
    }

    /// <summary>The two fields of a range; the end's browser hint starts at the start day. A future date is offered where the policy takes one.</summary>
    public static IReadOnlyList<FieldView> RangeFields(string startName, string endName, DateOnly today, DateRangePolicy policy, bool startRequired, bool endRequired, string? start = null, string? end = null)
    {
        DateOnly? max = policy == DateRangePolicy.NoFuture ? today : null;
        FieldView first = Fields.Date(startName, "Başlangıç", startRequired ? FieldRequirement.Required : FieldRequirement.Optional, start, max: max);
        FieldView second = Fields.Date(endName, "Bitiş", endRequired ? FieldRequirement.Required : FieldRequirement.Optional, end, min: Parse(start), max: max);
        return [first, second];
    }

    /// <summary>
    /// Puts the refusal of a range into the model state (04 2.3.2): a missing or malformed date is the catalog's code on its field, the end before the start is the catalog's <c>InvalidDateRange</c> on the end and a future date
    /// refused by the policy says so on its field.
    /// </summary>
    public static void AddRefusal(ModelStateDictionary modelState, string startName, string endName, DateRangeVerdict verdict)
    {
        ArgumentNullException.ThrowIfNull(modelState);
        ArgumentNullException.ThrowIfNull(verdict);
        switch (verdict.Outcome)
        {
            case DateRangeOutcome.Accepted:
                break;
            case DateRangeOutcome.StartMissing:
                modelState.AddFieldError(startName, FieldCodes.FieldRequired);
                break;
            case DateRangeOutcome.EndMissing:
                modelState.AddFieldError(endName, FieldCodes.FieldRequired);
                break;
            case DateRangeOutcome.StartMalformed:
                modelState.AddFieldError(startName, FieldCodes.InvalidDate);
                break;
            case DateRangeOutcome.EndMalformed:
                modelState.AddFieldError(endName, FieldCodes.InvalidDate);
                break;
            case DateRangeOutcome.StartInTheFuture:
                modelState.AddModelError(startName, RangeFutureMessage);
                break;
            case DateRangeOutcome.EndInTheFuture:
                modelState.AddModelError(endName, RangeFutureMessage);
                break;
            default:
                modelState.AddFieldError(endName, FieldCodes.InvalidDateRange);
                break;
        }
    }
}
