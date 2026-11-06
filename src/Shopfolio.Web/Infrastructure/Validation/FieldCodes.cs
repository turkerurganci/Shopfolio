using System.Reflection;

namespace Shopfolio.Infrastructure.Validation;

/// <summary>
/// The field validation codes of 07 6.2.2 (09 3.4 item 1): one constant per code, the same spelling. A code is the error message a rule puts
/// into the model state; the page filter turns it into the text (09 3.4 item 3) and the code itself is never shown or sent (07 2.7.3). A new
/// code is added to 07 6.2.2 first and here second; the catalog test keeps the two lists equal.
/// </summary>
public static class FieldCodes
{
    /// <summary>A required field is empty - a reason, the explanation of "other" and an item selection included.</summary>
    public const string FieldRequired = "FieldRequired";

    /// <summary>A filled required field, or a value entered once, is emptied.</summary>
    public const string FilledFieldCleared = "FilledFieldCleared";

    /// <summary>The form of an e-mail address (04 7.3.2.1).</summary>
    public const string InvalidEmail = "InvalidEmail";

    /// <summary>A password under P-17 or on the list of very common passwords (04 7.3.2.2).</summary>
    public const string PasswordTooWeak = "PasswordTooWeak";

    /// <summary>The form of a telephone number (04 7.3.2.4).</summary>
    public const string InvalidPhone = "InvalidPhone";

    /// <summary>A province outside the closed list (04 7.3.2.6).</summary>
    public const string InvalidProvince = "InvalidProvince";

    /// <summary>The form or the check digits of an IBAN (04 7.3.2.7).</summary>
    public const string InvalidIban = "InvalidIban";

    /// <summary>An amount that is negative, has more than two decimals, is zero where it must not be, is over the limit or cannot be read (04 7.3.2.8).</summary>
    public const string InvalidAmount = "InvalidAmount";

    /// <summary>The form or the range of a percentage (04 7.3.2.9).</summary>
    public const string InvalidPercentage = "InvalidPercentage";

    /// <summary>A quantity under one or not a whole number (04 7.3.2.10).</summary>
    public const string InvalidQuantity = "InvalidQuantity";

    /// <summary>A date that is in the future, before its lower limit or malformed (04 7.3.2.12).</summary>
    public const string InvalidDate = "InvalidDate";

    /// <summary>An end date before the start date (04 7.3.2.12).</summary>
    public const string InvalidDateRange = "InvalidDateRange";

    /// <summary>The lower value of the price filter is above the upper one (04 7.1.1.1).</summary>
    public const string InvalidPriceRange = "InvalidPriceRange";

    /// <summary>The form of a link (04 7.3.2.13).</summary>
    public const string InvalidUrl = "InvalidUrl";

    /// <summary>A text over its character limit (04 7.3.2.14).</summary>
    public const string TextTooLong = "TextTooLong";

    /// <summary>A value outside the closed list the form offered (04 7.3.2.16).</summary>
    public const string InvalidListValue = "InvalidListValue";

    /// <summary>A colour that is not <c>#</c> and six hexadecimal digits (06 4.54).</summary>
    public const string InvalidColor = "InvalidColor";

    /// <summary>An image of a type or size the product does not accept (04 7.3.2.18).</summary>
    public const string InvalidImage = "InvalidImage";

    /// <summary>The image count limit of a gallery or a single-image field (04 7.3.2.18).</summary>
    public const string ImageLimitExceeded = "ImageLimitExceeded";

    /// <summary>The upload a form refers to is gone or not complete (07 6.4.1).</summary>
    public const string UploadMissing = "UploadMissing";

    /// <summary>A required confirmation box is not ticked (04 7.1.5.9).</summary>
    public const string CheckboxNotAccepted = "CheckboxNotAccepted";

    /// <summary>Every code of the list, read from the constants above so the list and its members cannot drift apart.</summary>
    public static IReadOnlyList<string> All { get; } = typeof(FieldCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.IsLiteral && field.FieldType == typeof(string))
        .Select(field => (string)field.GetRawConstantValue()!)
        .ToArray();

    public static bool IsKnown(string? code) => code is not null && All.Contains(code, StringComparer.Ordinal);
}
