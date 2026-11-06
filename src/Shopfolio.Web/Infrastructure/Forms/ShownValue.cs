using System.Globalization;

namespace Shopfolio.Infrastructure.Forms;

/// <summary>
/// The comparison of a value the screen showed with the value the record holds now (07 2.4.3, 2.4.5; 05 7.3.1, 7.3.3): a stock or capacity field, the
/// internal note, the temporary-closure switch, the manual order of two records. The value comes back in a hidden <c>Original...</c> field
/// (<see cref="VersionFields.Original(string, string?, string)"/>), so a field that is missing, empty where a number is expected or not a number is
/// not a match: the write that depends on it is not made. The comparison is made on every send - the first one without the confirmation mark too -
/// and the last step carries the first form's value, not the page's current value (K-1212 (1)): a form opened in two tabs keeps its protection.
/// The write itself is the one conditional update of the module that owns the record (09 10.2 item 4); this class decides only whether the shown
/// value is still the value.
/// </summary>
internal static class ShownValue
{
    /// <summary>
    /// Reads the posted shown number: an empty field is the empty value (<see langword="null"/>, an unlimited stock) and a whole number is the number.
    /// Anything else - a decimal, a word, a number beyond the integer range - is not a shown value and the caller treats it as a mismatch.
    /// </summary>
    public static bool TryReadNumber(string? posted, out int? value)
    {
        if (string.IsNullOrEmpty(posted))
        {
            value = null;
            return true;
        }

        if (int.TryParse(posted, NumberStyles.None, CultureInfo.InvariantCulture, out int number))
        {
            value = number;
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>
    /// True when the posted shown number is a number (or the empty value) and equals the value the record holds now. The empty value matches only
    /// the empty value: an unlimited stock that became 0 is a different value.
    /// </summary>
    public static bool Matches(string? posted, int? current) => TryReadNumber(posted, out int? shown) && shown == current;

    /// <summary>
    /// True when the posted shown text equals the text the record holds now. A browser sends a line break as CR LF whatever the stored text uses, so the
    /// comparison is made on LF; a missing field is the empty text. The case and the spaces are the text's own and are not folded.
    /// </summary>
    public static bool Matches(string? posted, string? current) => string.Equals(Normalize(posted), Normalize(current), StringComparison.Ordinal);

    /// <summary>True when the posted shown switch (<c>true</c> or <c>false</c>) is the switch as it is now; anything else is not a shown value.</summary>
    public static bool Matches(string? posted, bool current) => bool.TryParse(posted, out bool shown) && shown == current;

    private static string Normalize(string? text) => (text ?? string.Empty).Replace("\r\n", "\n", StringComparison.Ordinal);
}
