using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Shopfolio.Infrastructure.Validation;

/// <summary>
/// The error message a rule writes into the model state (09 3.4 items 1-3): the code of 07 6.2.2, and when the text needs the value of a limit,
/// the value after a colon - <c>TextTooLong:150</c>. The page filter reads it, logs the field and the code, and replaces it with the catalog text; the
/// encoded form never reaches a page or a response. A message that is not in this form is not a rule's: the filter leaves it alone.
/// </summary>
public static class FieldError
{
    private const char Separator = ':';

    /// <summary>The message the framework's own conversion failures get (the text is not a number, not a date, not a known value); the filter maps it to a code.</summary>
    internal const string ConversionFailed = "ValueNotParsed";

    public static string Encode(string code, string? parameter = null)
    {
        ArgumentNullException.ThrowIfNull(code);
        if (!FieldCodes.IsKnown(code))
        {
            throw new ArgumentException($"'{code}' is not a field code of 07 6.2.2.", nameof(code));
        }

        if (parameter is null && FieldMessageCatalog.NeedsParameter(code))
        {
            throw new ArgumentException($"The message of '{code}' names a limit; the rule must give its value.", nameof(parameter));
        }

        return parameter is null ? code : code + Separator + parameter;
    }

    public static string Encode(string code, long parameter) => Encode(code, parameter.ToString(CultureInfo.InvariantCulture));

    /// <summary>Reads the code and the parameter of a rule's message; <see langword="false"/> when the message is not one.</summary>
    internal static bool TryParse(string? message, out string code, out string? parameter)
    {
        code = string.Empty;
        parameter = null;
        if (string.IsNullOrEmpty(message))
        {
            return false;
        }

        int cut = message.IndexOf(Separator, StringComparison.Ordinal);
        string head = cut < 0 ? message : message[..cut];
        if (!FieldCodes.IsKnown(head))
        {
            return false;
        }

        code = head;
        parameter = cut < 0 ? null : message[(cut + 1)..];
        return true;
    }

    /// <summary>
    /// The result of a field rule of a model (<c>IValidatableObject</c>: two fields' relation, a date order) with its code; the member is the field the message belongs to.
    /// </summary>
    public static ValidationResult Result(string code, string member, string? parameter = null) =>
        new(Encode(code, parameter), [member]);

    /// <summary>
    /// Adds a field error with its code to the model state: the rule a handler decides after the database answered (a stock code's uniqueness). The
    /// page filter handles it by the same path as an attribute's error (09 3.4 item 2).
    /// </summary>
    public static void AddFieldError(this ModelStateDictionary modelState, string field, string code, string? parameter = null)
    {
        ArgumentNullException.ThrowIfNull(modelState);
        ArgumentException.ThrowIfNullOrEmpty(field);
        modelState.AddModelError(field, Encode(code, parameter));
    }
}
