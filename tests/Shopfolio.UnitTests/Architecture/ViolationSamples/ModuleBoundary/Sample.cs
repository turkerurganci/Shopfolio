// Deliberate violation sample for the architecture rules of 09 6 (item 2 and 4; 19.2 item 3).
// This file is compiled into its own assembly (ModuleBoundaryViolationSample.dll) by the UnitTests project,
// never into the test assembly itself. It carries no reference to the application.
// Every rule has its own violation here and the allowed neighbours that must stay quiet:
//   module boundary    - Ordering.ForbiddenCaller (internal type of Catalog), Ordering.ForbiddenContextReader (Catalog's entity through the context)
//   public surface     - Catalog.PublicLeak (public type outside Contracts), Catalog.Contracts.LeakyContract (Contracts depends on an internal type)
//   page and endpoint  - Pages.ForbiddenPage (the context), Pages.ForbiddenPageEntity (an internal type of a module), AspNetCoreGeneratedDocument.Pages_Forbidden_View (the compiled .cshtml; D-73)
//   skeleton           - Ordering has no Contracts interface IOrderingModule (Catalog has ICatalogModule)
//   tenant / operator  - OperatorPanel namespace, Accounts.TenantResolver type, Accounts.Account.TenantId property; Accounts.Account itself is allowed
//   infrastructure     - Infrastructure.ForbiddenInfrastructure (a module type); Infrastructure.AppDbContext is the one allowed exception

using Microsoft.EntityFrameworkCore;

namespace Shopfolio.Modules.Catalog
{
    // Internal part of the Catalog module: not under Contracts.
    internal sealed class ProductEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    // Violation: a public type outside Contracts.
    public sealed class PublicLeak
    {
    }
}

namespace Shopfolio.Modules.Catalog.Contracts
{
    public interface ICatalog
    {
        string GetName();
    }

    // Allowed: Catalog's open interface of the skeleton rule; Ordering has none (violation of the Contracts-per-module rule).
    public interface ICatalogModule
    {
    }

    // Violation: a Contracts type that reaches the module's internal entity.
    public sealed class LeakyContract
    {
        internal string Read(ProductEntity product)
        {
            return product.Name;
        }
    }
}

namespace Shopfolio.Modules.Ordering
{
    // Allowed: Ordering calls Catalog only through its Contracts namespace.
    internal sealed class AllowedCaller
    {
        internal string Read(Catalog.Contracts.ICatalog catalog)
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

    // Violation: Ordering reaches Catalog's entity through the database context.
    internal sealed class ForbiddenContextReader
    {
        internal int Count(Infrastructure.AppDbContext context)
        {
            return context.Products.Local.Count;
        }
    }
}

namespace Shopfolio.Infrastructure
{
    // The one allowed exception: the context carries the set of every entity.
    internal sealed class AppDbContext : DbContext
    {
        public DbSet<Modules.Catalog.ProductEntity> Products => Set<Modules.Catalog.ProductEntity>();
    }

    // Allowed: infrastructure that knows no module type.
    internal sealed class AllowedInfrastructure
    {
        internal string Read()
        {
            return "clock";
        }
    }

    // Violation: infrastructure other than the context reaches a module type.
    internal sealed class ForbiddenInfrastructure
    {
        internal string Read(Modules.Catalog.ProductEntity product)
        {
            return product.Name;
        }
    }
}

namespace Shopfolio.OperatorPanel
{
    // Violation: a namespace named after the operator panel (05 3.3.1; 10 KD-21).
    internal sealed class Dashboard
    {
    }
}

namespace Shopfolio.Accounts
{
    // Violation: a type named after a tenant.
    internal sealed class TenantResolver
    {
    }

    // Allowed neighbour: nothing tenant-like or operator-like in its name; one property is a violation.
    internal sealed class Account
    {
        public string Name { get; set; } = string.Empty;

        // Violation: a property named after a tenant.
        public string TenantId { get; set; } = string.Empty;
    }
}

namespace Shopfolio.Pages
{
    // Allowed: a page calls a module's open surface.
    internal sealed class AllowedPage
    {
        internal string Read(Modules.Catalog.Contracts.ICatalog catalog)
        {
            return catalog.GetName();
        }
    }

    // Violation: a page touches the database context.
    internal sealed class ForbiddenPage
    {
        internal int Count(Infrastructure.AppDbContext context)
        {
            return context.Products.Local.Count;
        }
    }

    // Violation: a page touches a module's internal type.
    internal sealed class ForbiddenPageEntity
    {
        internal string Read(Modules.Catalog.ProductEntity product)
        {
            return product.Name;
        }
    }
}

namespace AspNetCoreGeneratedDocument
{
    // Allowed: the compiled class of a view calls a module's open surface (T206, D-73).
    internal sealed class Pages_Allowed_View
    {
        internal string Read(Shopfolio.Modules.Catalog.Contracts.ICatalog catalog)
        {
            return catalog.GetName();
        }
    }

    // Violation: the compiled class of a view (a .cshtml with @inject AppDbContext) touches the database context (T206, D-73).
    internal sealed class Pages_Forbidden_View
    {
        internal int Count(Shopfolio.Infrastructure.AppDbContext context)
        {
            return context.Products.Local.Count;
        }
    }
}
