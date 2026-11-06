using System.Globalization;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The page number a list asks for (07 2.6.2; 09 10.5 item 3). <c>page</c> is bound as text and read here: an empty, non-numeric or smaller-than-one
/// value is the first page, a positive value that does not fit an <c>int</c> is <see cref="int.MaxValue"/> (the paging helper clamps it to the last
/// page). A number is ASCII digits only - no blank, sign, decimal separator or other digit system. A wrong value is never an error (07 2.6.6).
/// </summary>
internal static class PageNumber
{
    /// <summary>The first page, which an address writes without the parameter.</summary>
    public const int First = 1;

    public static int Parse(string? value)
    {
        if (string.IsNullOrEmpty(value) || !IsAsciiDigits(value))
        {
            return First;
        }

        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int page))
        {
            // Only digits and it does not fit: the range is exceeded, which is the last page (07 2.6.2).
            return int.MaxValue;
        }

        return page < First ? First : page;
    }

    private static bool IsAsciiDigits(string value)
    {
        foreach (char c in value)
        {
            if (c is < '0' or > '9')
            {
                return false;
            }
        }

        return true;
    }
}
