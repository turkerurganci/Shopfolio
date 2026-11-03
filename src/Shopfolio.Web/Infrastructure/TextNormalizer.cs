using System.Globalization;
using System.Text;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The one text normalization used to compare identity values (09 9.4 item 1; 06 2.8.1). The Turkish letter constants of the
/// application live only in this file (09 9.4 item 5).
/// </summary>
internal static class TextNormalizer
{
    /// <summary>
    /// Trims the value, folds "I", "İ" and "ı" to "i" and lowercases the rest without culture. E-mail addresses, coupon codes and stock
    /// codes are compared in this form, in a separate column with its own unique index. A culture-dependent lowercase is never used.
    /// </summary>
    public static string NormalizeIdentity(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Trim().Replace('I', 'i').Replace('İ', 'i').Replace('ı', 'i').ToLowerInvariant();
    }

    /// <summary>
    /// The searched form of a name (09 9.4 item 2; 06 4.5 <c>NameSearchForm</c>): "I", "İ" and "ı" become "i", the Turkish letters become their
    /// ASCII counterparts (upper and lower case), the other accented Latin letters lose their combining mark, and the rest is lowercased
    /// without culture. Nothing else is touched. The search expression passes through the same function.
    /// </summary>
    public static string Simplify(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        StringBuilder folded = new(value.Length);
        foreach (char c in value)
        {
            folded.Append(c switch
            {
                'I' or 'İ' or 'ı' => 'i',
                'Ç' or 'ç' => 'c',
                'Ğ' or 'ğ' => 'g',
                'Ö' or 'ö' => 'o',
                'Ş' or 'ş' => 's',
                'Ü' or 'ü' => 'u',
                _ => c,
            });
        }

        // A combining mark is dropped only when it follows a Latin base letter (09 9.4 item 2); the marks of other scripts stay.
        StringBuilder result = new(value.Length);
        bool afterLatinBase = false;
        foreach (char c in folded.ToString().Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
            {
                if (afterLatinBase)
                {
                    continue;
                }

                result.Append(c);
                continue;
            }

            result.Append(c);
            afterLatinBase = IsLatinLetter(c);
        }

        return result.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }

    // Basic Latin, Latin-1 Supplement letters and Latin Extended-A/B/Additional: the ranges an accented Latin base letter comes from.
    private static bool IsLatinLetter(char c)
    {
        return c is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z')
            or (>= 'À' and <= 'ɏ' and not '×' and not '÷')
            or (>= 'Ḁ' and <= 'ỿ');
    }
}
