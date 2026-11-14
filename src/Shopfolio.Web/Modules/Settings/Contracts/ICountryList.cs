namespace Shopfolio.Modules.Settings.Contracts;

/// <summary>A country of the closed list of the place of origin (06 4.4): its id, the two-letter code ("TR") and the Turkish name.</summary>
public sealed record CountryInfo(Guid Id, string Code, string Name);

/// <summary>
/// The closed country list as other modules and the pages read it (06 4.4; D-344): the product page asks for the name and the code of a product's place of origin, so no module reads the country
/// table itself (09 6 item 2). A plain read; it opens no transaction.
/// </summary>
public interface ICountryList
{
    /// <summary>The country with this id; null when the id is not on the list.</summary>
    Task<CountryInfo?> FindAsync(Guid countryId, CancellationToken ct);

    /// <summary>The whole closed list by name - the product form's place of origin is chosen from it (04 9.5.6; K-1023). A small start-data list.</summary>
    Task<IReadOnlyList<CountryInfo>> ListAsync(CancellationToken ct);
}
