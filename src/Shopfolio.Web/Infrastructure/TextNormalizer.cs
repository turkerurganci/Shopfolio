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
}
