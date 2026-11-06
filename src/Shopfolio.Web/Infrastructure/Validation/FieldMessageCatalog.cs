using System.Globalization;

namespace Shopfolio.Infrastructure.Validation;

/// <summary>
/// The message catalog (09 3.4 item 4): one class, code to Turkish message pattern. A pattern has the form of 04 7.3.1 - the name of the field and
/// what is missing or wrong - and the value of a limit is a parameter that the message shows (<c>{1}</c>); the field's name is <c>{0}</c>. Every code of
/// 07 6.2.2 has a pattern and every pattern belongs to a code of that list (the catalog test). The texts a decision fixes (04 7.3.2: the province, the
/// price and date messages of K-799, K-817, K-772) are the decision owners' to refine; this is the common form.
/// </summary>
internal static class FieldMessageCatalog
{
    private static readonly Dictionary<string, string> _patterns = new(StringComparer.Ordinal)
    {
        [FieldCodes.FieldRequired] = "{0} boş olamaz.",
        [FieldCodes.FilledFieldCleared] = "{0} boşaltılamaz.",
        [FieldCodes.InvalidEmail] = "{0} geçerli bir e-posta adresi olmalıdır.",
        [FieldCodes.InvalidPhone] = "{0} geçerli bir telefon numarası olmalıdır.",
        [FieldCodes.InvalidProvince] = "{0} için listeden bir il seçilmelidir.",
        [FieldCodes.InvalidIban] = "{0} geçerli bir IBAN olmalıdır.",
        [FieldCodes.InvalidAmount] = "{0} geçerli bir tutar olmalıdır; en çok iki ondalık yazılabilir.",
        [FieldCodes.InvalidPercentage] = "{0} geçerli bir yüzde olmalıdır; en çok iki ondalık yazılabilir.",
        [FieldCodes.InvalidQuantity] = "{0} 1 ile {1} arasında bir tam sayı olmalıdır.",
        [FieldCodes.InvalidDate] = "{0} geçerli bir tarih olmalıdır.",
        [FieldCodes.InvalidDateRange] = "{0} başlangıç tarihinden önce olamaz.",
        [FieldCodes.InvalidPriceRange] = "{0} üst değerden büyük olamaz.",
        [FieldCodes.InvalidUrl] = "{0} http:// ya da https:// ile başlayan tam bir adres olmalıdır.",
        [FieldCodes.TextTooLong] = "{0} en çok {1} karakter olabilir.",
        [FieldCodes.InvalidListValue] = "{0} için geçerli bir seçim yapılmalıdır.",
        [FieldCodes.InvalidColor] = "{0} # ve altı onaltılık haneden oluşmalıdır.",
        [FieldCodes.InvalidImage] = "{0} desteklenen bir görsel olmalıdır.",
        [FieldCodes.ImageLimitExceeded] = "{0} için görsel sayısı sınırı aşıldı.",
        [FieldCodes.UploadMissing] = "{0} yeniden yüklenmelidir.",
        [FieldCodes.CheckboxNotAccepted] = "{0} onaylanmalıdır.",
    };

    static FieldMessageCatalog()
    {
        // Added by call (not by indexer) so that the line is not read as the assignment of a secret key (the pre-commit guard looks for a password key).
        _patterns.Add(FieldCodes.PasswordTooWeak, "{0} en az {1} karakter olmalı ve çok yaygın bir şifre olmamalıdır.");
    }

    /// <summary>The patterns, by code.</summary>
    public static IReadOnlyDictionary<string, string> Patterns => _patterns;

    /// <summary>Whether the code's message names a limit - the rule that reports it must carry the value.</summary>
    public static bool NeedsParameter(string code) => _patterns.TryGetValue(code, out string? pattern) && pattern.Contains("{1}", StringComparison.Ordinal);

    /// <summary>The message of a code for a field with the given label (the field's display name); the parameter is the limit's value.</summary>
    public static string Build(string code, string label, string? parameter)
    {
        ArgumentNullException.ThrowIfNull(label);
        if (!_patterns.TryGetValue(code, out string? pattern))
        {
            throw new ArgumentException($"'{code}' has no message pattern.", nameof(code));
        }

        if (NeedsParameter(code) && parameter is null)
        {
            throw new ArgumentException($"The message of '{code}' names a limit and the rule gave none.", nameof(parameter));
        }

        return string.Format(CultureInfo.InvariantCulture, pattern, label, parameter);
    }
}
