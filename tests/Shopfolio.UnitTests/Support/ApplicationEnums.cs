namespace Shopfolio.UnitTests.Support;

/// <summary>The enums of the application assembly, found by reflection for the enum tests (T102).</summary>
internal static class ApplicationEnums
{
    private const string ModulesPrefix = "Shopfolio.Modules.";

    private const string ContractsSuffix = ".Contracts";

    /// <summary>The namespace of the stored enums that belong to shared infrastructure (06 5.4, 5.5).</summary>
    public const string ValuesNamespace = "Shopfolio.Infrastructure.Values";

    /// <summary>
    /// The enums of a module's <c>Contracts</c> that are named, not stored: the value is a result code an operation returns and no column ever holds it
    /// (T316: the refusal of the transition unit's open face; T335: what a product address answers; T319: why a correction can not be made). Each is listed
    /// by its full name; a stored enum is never added here.
    /// </summary>
    private static readonly HashSet<string> NotStored = new(StringComparer.Ordinal)
    {
        "Shopfolio.Modules.Ordering.Contracts.TransitionRefusal",
        "Shopfolio.Modules.AfterSales.Contracts.ItemRecordRefusal",
        // T361: the storefront's read shapes - which menu item or home page block a record is, and why a menu item is hidden; computed per read, never a column.
        "Shopfolio.Modules.CorporateContent.Contracts.MenuItemKind",
        "Shopfolio.Modules.CorporateContent.Contracts.MenuHiddenReason",
        "Shopfolio.Modules.CorporateContent.Contracts.HomepageBlockKind",
        "Shopfolio.Modules.Ordering.Contracts.CorrectionObstacle",
        "Shopfolio.Modules.Catalog.Contracts.ProductPageKind",
        // T323: the identity of an operation of 03 1.11 - the request names it and the decision table answers; the document's table is its definition, no column holds it.
        "Shopfolio.Modules.Ordering.Contracts.OrderOperation",
        "Shopfolio.Modules.Ordering.Contracts.CartAddOutcome",
        "Shopfolio.Modules.Ordering.Contracts.CartPaymentBlock",
        // T351: why a delivery date is refused - the result code of the delivery mark, never a column.
        "Shopfolio.Modules.Ordering.Contracts.DeliveryDateObstacle",
        // T347: the verdict on a cart line's quantity - computed per read from the stock and the per-order limit, never a column.
        "Shopfolio.Modules.Catalog.Contracts.CartQuantityProblem",
        // T375: how the firm knows an address correction was asked by the order's owner - the request carries it, the trail's confirmation is not a column.
        "Shopfolio.Modules.Ordering.Contracts.AddressRequestConfirmation",
        // T384: how an export request ended and what is wrong with the order export's range - result codes of the data export, never a column.
        "Shopfolio.Modules.Administration.Contracts.ExportOutcome",
        "Shopfolio.Modules.Administration.Contracts.ExportRangeError",
    };

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
            .Where(t => t.IsEnum && IsPersistentNamespace(t.Namespace) && !NotStored.Contains(t.FullName!))
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
