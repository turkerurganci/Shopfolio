using Shopfolio.Infrastructure.Migrations;
using Shopfolio.Modules.Catalog;

namespace Shopfolio.IntegrationTests.Database;

/// <summary>
/// The countries every test database carries from the start-up data migration (06 8.1.1): a physical product needs its place of origin (02 3.8.5; T336),
/// so a test that saves one points it at the seeded row. The id is the migration's own fixed id, derived from its list - not a second copy of it.
/// </summary>
internal static class SeededCountry
{
    /// <summary>The id of the seeded "TR" row (table number 2 of the migration, the row's position in its list).</summary>
    public static Guid Turkey { get; } = IdOf("TR");

    /// <summary>Gives <paramref name="product"/> the seeded "TR" row as its place of origin and returns it, for a test that saves a physical product.</summary>
    public static Product WithSeededOrigin(this Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        product.OriginCountryId = Turkey;
        return product;
    }

    /// <summary>The id of a seeded country by its code.</summary>
    public static Guid IdOf(string code)
    {
        int index = Array.FindIndex(InsertStartupData.Countries, c => c.Code == code);
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(code), code, "The start-up data has no such country.");
        }

        return Guid.Parse(InsertStartupData.Id(2, index + 1));
    }
}
