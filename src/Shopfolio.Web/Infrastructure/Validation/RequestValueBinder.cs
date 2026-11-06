using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Shopfolio.Infrastructure.Validation;

/// <summary>The kinds of value <see cref="RequestValueBinder"/> reads (09 7.2 items 2-4).</summary>
internal enum RequestValueKind
{
    /// <summary>An amount or a decimal number - a percentage and a net quantity included: <see cref="RequestFormat.TryParseDecimal"/>.</summary>
    Decimal,

    /// <summary>A day: <see cref="RequestFormat.TryParseDate"/>.</summary>
    Date,

    /// <summary>A quantity or a number of days held in an <see cref="int"/>: <see cref="RequestFormat.TryParseWholeNumber"/>.</summary>
    Int32,

    /// <summary>A whole number held in a <see cref="long"/>.</summary>
    Int64,
}

/// <summary>
/// The model binder of the values whose format 07 2.9 fixes (09 7.2 item 4): <see cref="decimal"/>, <see cref="DateOnly"/>, <see cref="int"/> and
/// <see cref="long"/>, nullable or not, in a form or in a query. The framework's simple type binding is not used for them: it reads with the thread's
/// culture and a wide style - in <c>tr-TR</c> "12.50" would become 1250, and a whole number would take a sign, a blank or a hexadecimal prefix
/// ("0x10" is 16). The value is trimmed (09 7.2, the introduction) and read by <see cref="RequestFormat"/> only; nothing else is accepted and nothing
/// is rounded. A value that cannot be read puts the code of the field's rule type into the model state (09 3.4 item 1): the field's rule that owns the
/// conversion failure - <see cref="InvalidAmountAttribute"/>, <see cref="InvalidPercentageAttribute"/>, <see cref="InvalidQuantityAttribute"/>,
/// <see cref="InvalidDateAttribute"/> - and without one the type's code (<c>InvalidAmount</c> for a decimal, <c>InvalidDate</c> for a date). An empty value
/// is <see langword="null"/>; for a type that cannot hold it, it is <c>FieldRequired</c>. The page filter does the rest (09 3.4 item 3): 422 on a form,
/// the message without the filter on a query (07 2.6.6). What the user typed stays in the model state for the form to show again.
/// </summary>
internal sealed class RequestValueBinder : IModelBinder
{
    private readonly RequestValueKind _kind;

    public RequestValueBinder(RequestValueKind kind)
    {
        _kind = kind;
    }

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);
        ValueProviderResult result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (result == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, result);
        string text = (result.FirstValue ?? string.Empty).Trim();
        if (text.Length == 0)
        {
            if (bindingContext.ModelMetadata.IsReferenceOrNullableType)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, FieldError.Encode(FieldCodes.FieldRequired));
            }

            return Task.CompletedTask;
        }

        if (TryRead(text, out object? value))
        {
            bindingContext.Result = ModelBindingResult.Success(value);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, FailureMessage(bindingContext.ModelMetadata));
        }

        return Task.CompletedTask;
    }

    private bool TryRead(string text, out object? value)
    {
        value = null;
        switch (_kind)
        {
            case RequestValueKind.Decimal when RequestFormat.TryParseDecimal(text, out decimal amount):
                value = amount;
                return true;
            case RequestValueKind.Date when RequestFormat.TryParseDate(text, out DateOnly day):
                value = day;
                return true;
            case RequestValueKind.Int32 when RequestFormat.TryParseWholeNumber(text, out long number) && number <= int.MaxValue:
                value = (int)number;
                return true;
            case RequestValueKind.Int64 when RequestFormat.TryParseWholeNumber(text, out long wide):
                value = wide;
                return true;
            default:
                return false;
        }
    }

    /// <summary>The message of a value that cannot be read: the code of the field's rule type, with the limit its message names.</summary>
    private string FailureMessage(ModelMetadata metadata)
    {
        if (metadata.ValidatorMetadata.OfType<FieldRuleAttribute>().FirstOrDefault(rule => rule.OwnsConversionFailure) is { } owner)
        {
            return FieldError.Encode(owner.Code, owner.ParameterText);
        }

        return _kind switch
        {
            RequestValueKind.Decimal => FieldError.Encode(FieldCodes.InvalidAmount, new InvalidAmountAttribute().ParameterText),
            RequestValueKind.Date => FieldError.Encode(FieldCodes.InvalidDate),

            // A whole number without a rule: the framework's marker, which the page filter maps to the field's rule or to a list value (09 3.4 item 3).
            _ => FieldError.ConversionFailed,
        };
    }
}

/// <summary>
/// Gives <see cref="RequestValueBinder"/> to every <see cref="decimal"/>, <see cref="DateOnly"/>, <see cref="int"/> and <see cref="long"/> (nullable or not)
/// that is bound from a form, a query or the route; it is first in the provider list (09 7.2 item 4). A body (a machine endpoint's JSON, 07 2.9.6), a
/// header and a service are left to their own binders.
/// </summary>
internal sealed class RequestValueBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        BindingSource? source = context.BindingInfo.BindingSource;
        if (source is not null && source != BindingSource.ModelBinding && source != BindingSource.Form && source != BindingSource.Query && source != BindingSource.Path)
        {
            return null;
        }

        Type type = Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType;
        return KindOf(type) is { } kind ? new RequestValueBinder(kind) : null;
    }

    /// <summary>The kind of value the binder reads for a type, or <see langword="null"/> when the type is not one of its four.</summary>
    public static RequestValueKind? KindOf(Type type) =>
        type == typeof(decimal) ? RequestValueKind.Decimal
        : type == typeof(DateOnly) ? RequestValueKind.Date
        : type == typeof(int) ? RequestValueKind.Int32
        : type == typeof(long) ? RequestValueKind.Int64
        : null;
}
