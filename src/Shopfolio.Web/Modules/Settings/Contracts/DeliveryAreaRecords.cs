using System.Globalization;

namespace Shopfolio.Modules.Settings.Contracts;

/// <summary>
/// The delivery provinces section as stored (06 4.2; 02 3.20.1; 07 UC-169): the ids of the selected provinces in plate-code order and the section's own version. No selection at all is a
/// valid state - the section then says nothing is delivered (K-817, K-828).
/// </summary>
public sealed record DeliveryProvincesSection(IReadOnlyList<Guid> ProvinceIds, int Version);

/// <summary>
/// The delivery area of the company (02 3.20.1): which of the 81 provinces it delivers to. The storefront reads it before the address step (04 5.4.5), the address check reads it at the
/// payment step and the order confirmation reads it again (02 6.2.3). A plain value read from the database on every call.
/// </summary>
public sealed class DeliveryArea
{
    /// <summary>The most provinces a restriction line names before it shortens to the first five and a count (04 5.4.5; K-773).</summary>
    public const int LineLimit = 5;

    private readonly HashSet<Guid> _servedIds;

    public DeliveryArea(IReadOnlyList<ProvinceInfo> served, IReadOnlyList<ProvinceInfo> notServed)
    {
        ArgumentNullException.ThrowIfNull(served);
        ArgumentNullException.ThrowIfNull(notServed);
        Served = served;
        NotServed = notServed;
        _servedIds = [.. served.Select(p => p.Id)];
    }

    /// <summary>The provinces the company delivers to, in plate-code order.</summary>
    public IReadOnlyList<ProvinceInfo> Served { get; }

    /// <summary>The provinces the company does not deliver to, in plate-code order.</summary>
    public IReadOnlyList<ProvinceInfo> NotServed { get; }

    /// <summary>True when at least one province is not delivered to: only then does the site show a restriction line (02 3.20.1).</summary>
    public bool IsRestricted => NotServed.Count > 0;

    /// <summary>True when the company delivers to no province at all (04 5.4.5; K-828).</summary>
    public bool DeliversNowhere => Served.Count == 0;

    /// <summary>True when the company delivers to the province. A province that is not on the list of 81 is not delivered to.</summary>
    public bool IsServed(Guid provinceId) => _servedIds.Contains(provinceId);

    /// <summary>The refusal text of a province that is not delivered to (04 5.13.23; K-772): it says why, naming the province.</summary>
    public static string NotServedMessage(string provinceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provinceName);
        return string.Create(CultureInfo.InvariantCulture, $"Bu firma {provinceName} iline teslimat yapmıyor.");
    }

    /// <summary>
    /// The restriction line of the storefront (04 5.4.5; K-773, K-828): empty when every province is served; "Şu an hiçbir ile teslimat yapılmıyor." when none is; otherwise the shorter of
    /// the two lists, the served one on a tie. A list longer than five names shows the first five and the rest as a count; <see cref="DeliveryRestrictionLine.FullList"/> holds all names for
    /// the part the visitor opens without leaving the page.
    /// </summary>
    public DeliveryRestrictionLine? RestrictionLine()
    {
        if (!IsRestricted)
        {
            return null;
        }

        if (DeliversNowhere)
        {
            return new DeliveryRestrictionLine("Şu an hiçbir ile teslimat yapılmıyor.", null);
        }

        bool listServed = Served.Count <= NotServed.Count;
        IReadOnlyList<ProvinceInfo> shown = listServed ? Served : NotServed;
        string heading = listServed ? "Teslimat yapılan iller" : "Teslimat yapılmayan iller";
        string full = $"{heading}: {string.Join(", ", shown.Select(p => p.Name))}";
        if (shown.Count <= LineLimit)
        {
            return new DeliveryRestrictionLine(full, null);
        }

        string first = string.Join(", ", shown.Take(LineLimit).Select(p => p.Name));
        return new DeliveryRestrictionLine(string.Create(CultureInfo.InvariantCulture, $"{heading}: {first} ve {shown.Count - LineLimit} il daha"), full);
    }
}

/// <summary>The restriction line and, when the line is shortened, the whole list that opens inside it without a page change (04 5.4.5).</summary>
public sealed record DeliveryRestrictionLine(string Text, string? FullList);
