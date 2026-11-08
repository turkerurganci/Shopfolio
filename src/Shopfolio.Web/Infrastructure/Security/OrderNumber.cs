using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The order number (09 15.1 item 1; 02 3.22.1; 05 6.4.2): the year, a hyphen and a random block of <see cref="ProductConstants.OrderNumberRandomBlockLength"/> (P-19)
/// characters, as in <c>2026-7K4M9P</c>. The alphabet is fixed at 32 characters without the ones that look alike (<c>0</c>/<c>O</c>, <c>1</c>/<c>I</c>/<c>l</c>), and every
/// character is picked with <see cref="RandomNumberGenerator.GetInt32(int)"/>, which is uniform. The number is a code that can be guessed only by chance and is not an
/// invoice number (02 3.22.2). The number alone opens nothing (02 3.22.5); it travels on invoices and in letters, so every way that takes it also asks for the e-mail.
/// </summary>
internal static class OrderNumber
{
    /// <summary>The 32 characters of the random block: 2-9 and the capital letters except I and O (05 6.4.2).</summary>
    public const string Alphabet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

    /// <summary>The character between the year and the random block.</summary>
    public const char Separator = '-';

    /// <summary>The number of digits of the year part.</summary>
    public const int YearLength = 4;

    /// <summary>A new number for an order that is made in <paramref name="year"/>.</summary>
    public static string Create(int year) => Create(year, ProductConstants.OrderNumberRandomBlockLength);

    /// <summary>The same with an explicit block length; the pure form the tests use.</summary>
    internal static string Create(int year, int blockLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(year, 1000);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(year, 9999);
        ArgumentOutOfRangeException.ThrowIfLessThan(blockLength, 1);

        StringBuilder number = new(YearLength + 1 + blockLength);
        number.Append(year.ToString(CultureInfo.InvariantCulture)).Append(Separator);
        for (int i = 0; i < blockLength; i++)
        {
            number.Append(Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)]);
        }

        return number.ToString();
    }
}
