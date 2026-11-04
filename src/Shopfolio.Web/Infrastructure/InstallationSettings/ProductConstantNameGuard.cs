using System.Reflection;

namespace Shopfolio.Infrastructure.InstallationSettings;

/// <summary>
/// Finds the settings whose key names a product constant (05 8.4.4, 8.4.7; 09 3.5 item 2): the settings file cannot change a product
/// constant, so a key that carries the name of one is a mistake the application does not open with. Every segment of a key is compared
/// with the names of the constants of <see cref="ProductConstants"/>, ignoring case, underscores and hyphens (so <c>CategoryLevelMax</c>,
/// <c>CATEGORY_LEVEL_MAX</c> and <c>Category-Level-Max</c> are the same name). The constants are read from the class itself, so a constant
/// added later is covered without a change here.
/// </summary>
internal static class ProductConstantNameGuard
{
    private static readonly HashSet<string> _names = ReadNames(typeof(ProductConstants));

    /// <summary>The keys of <paramref name="configuration"/> that name a product constant, each once.</summary>
    public static IReadOnlyList<string> FindKeys(IConfiguration configuration)
    {
        return FindKeys(configuration, _names);
    }

    /// <summary>The same check against the constants of any class; the tests use it with a sample class.</summary>
    public static IReadOnlyList<string> FindKeys(IConfiguration configuration, Type constantsClass)
    {
        return FindKeys(configuration, ReadNames(constantsClass));
    }

    private static List<string> FindKeys(IConfiguration configuration, HashSet<string> names)
    {
        List<string> found = [];
        foreach (string key in configuration.AsEnumerable().Select(pair => pair.Key).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (key.Split(':').Any(segment => names.Contains(Normalize(segment))))
            {
                found.Add(key);
            }
        }

        return found;
    }

    private static HashSet<string> ReadNames(Type constantsClass)
    {
        return constantsClass.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .Where(field => field.IsLiteral)
            .Select(field => Normalize(field.Name))
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string Normalize(string name)
    {
        return new string([.. name.Where(char.IsLetterOrDigit)]).ToUpperInvariant();
    }
}
