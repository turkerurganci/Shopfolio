namespace Shopfolio.Modules.Settings.Contracts;

/// <summary>
/// The closed list of the 81 provinces as other modules read it (06 4.3; 04 7.3.2.6): a form that takes a province - a branch of the corporate content (02 3.27.7) -
/// asks here whether the chosen one is on the list, and a page that shows it asks for its name, so no module reads the province table itself. Plain reads; they open no transaction.
/// </summary>
public interface IProvinceList
{
    /// <summary>True when a province with this id is on the list.</summary>
    Task<bool> ExistsAsync(Guid provinceId, CancellationToken ct);

    /// <summary>The names of the provinces with these ids; an id that is not on the list is left out.</summary>
    Task<IReadOnlyDictionary<Guid, string>> GetNamesAsync(IReadOnlyCollection<Guid> provinceIds, CancellationToken ct);
}
