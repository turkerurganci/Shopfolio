using System.ComponentModel.DataAnnotations;

namespace Shopfolio.Infrastructure.Validation;

/// <summary>
/// The base of the project's rule attributes (09 3.4 item 1): a rule type is an attribute class that carries its code of 07 6.2.2. The built-in
/// <c>Required</c>, <c>StringLength</c>, <c>EmailAddress</c>, <c>Range</c> and the like are not used on an input or query model - their codes are not
/// the codes of 04 7 (an architecture rule keeps them out). A rule accepts an empty value unless it is <see cref="FieldRequiredAttribute"/>: whether a
/// field is optional is the required rule's business alone (04 7.3.1). A limit is the attribute's argument and is read from a product constant (09 3.5),
/// never written as a number on the attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public abstract class FieldRuleAttribute : ValidationAttribute
{
    /// <summary>The code of 07 6.2.2 the rule reports.</summary>
    public abstract string Code { get; }

    /// <summary>The value of the limit the message names (a character count, a maximum quantity), or <see langword="null"/>.</summary>
    public virtual string? ParameterText => null;

    /// <summary>
    /// The rule's own pattern of its code's message, when the code's catalog text does not fit the rule (a rule that shares a code of 07 6.2.2 with another limit); the catalog keeps the
    /// pattern (<see cref="FieldMessageCatalog"/>). <see langword="null"/>: the code's pattern.
    /// </summary>
    public virtual string? MessagePattern => null;

    /// <summary>
    /// <see langword="true"/> when a posted text that cannot be converted to the property's type (letters in a quantity) is this rule's failure: the
    /// filter reports the rule's code for it instead of the framework's text.
    /// </summary>
    public virtual bool OwnsConversionFailure => false;

    /// <summary>Whether the value passes; called for every value, empty ones included.</summary>
    protected abstract bool Accepts(object? value);

    /// <summary>
    /// Whether the value passes the rule, for code that saves or calculates outside a form (a module asking the same rule its form field asks, so the two
    /// cannot part: 09 3.4 item 1).
    /// </summary>
    public bool Allows(object? value) => Accepts(value);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        ArgumentNullException.ThrowIfNull(validationContext);
        if (Accepts(value))
        {
            return ValidationResult.Success;
        }

        string message = FieldError.Encode(Code, ParameterText);
        return validationContext.MemberName is null ? new ValidationResult(message) : new ValidationResult(message, [validationContext.MemberName]);
    }

    /// <summary>Whether a number has at most two decimals (04 7.3.2.8, 7.3.2.9); trailing zeros do not count, and nothing is rounded.</summary>
    protected static bool HasAtMostTwoDecimals(decimal value) => HasAtMostDecimals(value, 2);

    /// <summary>
    /// Whether a number has at most <paramref name="decimals"/> decimals; trailing zeros do not count, and nothing is rounded. The one decimal count of the
    /// project: the form rules and the save-time rules of a price or a net quantity (02 3.8.4, 3.8.5) both ask it.
    /// </summary>
    public static bool HasAtMostDecimals(decimal value, int decimals)
    {
        const int ScaleShift = 16;
        const int ScaleMask = 0xFF;

        // Dividing by one with 28 decimals drops the trailing zeros of the scale (10.500 becomes 10.5).
        decimal stripped = value / 1.0000000000000000000000000000m;
        return ((decimal.GetBits(stripped)[3] >> ScaleShift) & ScaleMask) <= decimals;
    }

    /// <summary>An empty value: null, or a text of nothing but white space.</summary>
    protected static bool IsEmpty(object? value) => value is null || (value is string text && string.IsNullOrWhiteSpace(text));
}
