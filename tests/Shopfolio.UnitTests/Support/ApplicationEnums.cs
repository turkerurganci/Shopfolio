namespace Shopfolio.UnitTests.Support;

/// <summary>The enums of the application assembly, found by reflection for the enum tests (T102).</summary>
internal static class ApplicationEnums
{
    private const string ModulesPrefix = "Shopfolio.Modules.";

    private const string ContractsSuffix = ".Contracts";

    /// <summary>The namespace of the stored enums that belong to shared infrastructure (06 5.4, 5.5).</summary>
    public const string ValuesNamespace = "Shopfolio.Infrastructure.Values";

    /// <summary>All enums of the application assembly, by simple name; a name used twice is a failure of the naming rule (09 5.1).</summary>
    public static Dictionary<string, Type> ByName()
    {
        return typeof(Shopfolio.Infrastructure.AppDbContext).Assembly.GetTypes()
            .Where(t => t.IsEnum && t.Namespace is not null && (t.Namespace.StartsWith(ModulesPrefix, StringComparison.Ordinal) || t.Namespace.StartsWith("Shopfolio.Infrastructure", StringComparison.Ordinal)))
            .ToDictionary(t => t.Name, StringComparer.Ordinal);
    }

    /// <summary>The enums whose value is stored: the ones in a module's <c>Contracts</c> namespace and in the values namespace of the infrastructure.</summary>
    public static IReadOnlyList<Type> Persistent()
    {
        return typeof(Shopfolio.Infrastructure.AppDbContext).Assembly.GetTypes()
            .Where(t => t.IsEnum && IsPersistentNamespace(t.Namespace))
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .ToList();
    }

    public static bool IsPersistentNamespace(string? ns)
    {
        return ns is not null
            && (ns == ValuesNamespace
                || (ns.StartsWith(ModulesPrefix, StringComparison.Ordinal) && ns.EndsWith(ContractsSuffix, StringComparison.Ordinal)));
    }
}
