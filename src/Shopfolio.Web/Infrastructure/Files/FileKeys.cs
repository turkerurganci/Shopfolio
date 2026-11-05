using System.Text.RegularExpressions;

namespace Shopfolio.Infrastructure.Files;

/// <summary>
/// The shape of a file key per section (09 6 item 6). An image key is the SHA-256 fingerprint of the content (64 lowercase hexadecimal digits)
/// or, for a derivative, <c>{fingerprint}-{width}</c>; a digital file key is <see cref="EntityIds.NewStorageKey"/> (32 lowercase hexadecimal
/// digits). A key is the only part of the path the application makes, so nothing a user typed can reach it.
/// </summary>
internal static partial class FileKeys
{
    [GeneratedRegex("^[0-9a-f]{64}(-[1-9][0-9]{0,4})?\\z", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ImageKeyShape();

    [GeneratedRegex("^[0-9a-f]{32}\\z", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex DigitalFileKeyShape();

    public static bool IsValid(FileArea area, string? key)
    {
        if (key is null)
        {
            return false;
        }

        return area switch
        {
            FileArea.Image => ImageKeyShape().IsMatch(key),
            FileArea.DigitalFile => DigitalFileKeyShape().IsMatch(key),
            _ => false,
        };
    }

    /// <summary>Refuses a key that does not fit the section, a path separator and a parent directory included.</summary>
    public static void Require(FileArea area, string? key)
    {
        if (!IsValid(area, key))
        {
            throw new ArgumentException($"The key does not fit the {area} section's shape.", nameof(key));
        }
    }

    /// <summary>The key of an image derivative: the source copy's key and the width.</summary>
    public static string DerivativeKey(string sourceKey, int width)
    {
        Require(FileArea.Image, sourceKey);
        if (sourceKey.Length != ImageFingerprintLength)
        {
            throw new ArgumentException("A derivative is made from a source copy's key.", nameof(sourceKey));
        }

        return string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{sourceKey}-{width}");
    }

    /// <summary>The source copy's key of an image file key (the key itself, or the part before the width).</summary>
    public static string SourceKeyOf(string imageKey)
    {
        Require(FileArea.Image, imageKey);
        return imageKey[..ImageFingerprintLength];
    }

    public const int ImageFingerprintLength = 64;
}
