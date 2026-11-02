// Deliberate violation sample for the module boundary rule (09 6 item 4, 19.2 item 3).
// This file is compiled into its own assembly (ModuleBoundaryViolationSample.dll) by the UnitTests project,
// never into the test assembly itself. It carries no reference to the application.

namespace Shopfolio.Modules.Catalog
{
    // Internal part of the Catalog module: not under Contracts.
    internal sealed class ProductEntity
    {
        public string Name { get; set; } = string.Empty;
    }
}

namespace Shopfolio.Modules.Catalog.Contracts
{
    public interface ICatalog
    {
        string GetName();
    }
}

namespace Shopfolio.Modules.Ordering
{
    // Allowed: Ordering calls Catalog only through its Contracts namespace.
    public sealed class AllowedCaller
    {
        public string Read(Catalog.Contracts.ICatalog catalog)
        {
            return catalog.GetName();
        }
    }

    // Violation: Ordering reaches into a Catalog internal type.
    internal sealed class ForbiddenCaller
    {
        internal string Read(Catalog.ProductEntity product)
        {
            return product.Name;
        }
    }
}
