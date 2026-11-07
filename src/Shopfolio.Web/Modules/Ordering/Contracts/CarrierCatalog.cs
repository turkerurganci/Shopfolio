namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// One company of the carrier list (08 2.4). <see cref="Code"/> is the permanent code name stored in <c>Order.Carrier</c>; <see cref="TrackingUrlPattern"/> is the
/// company's own tracking page with the single <see cref="CarrierCatalog.Placeholder"/> where the tracking number goes.
/// </summary>
public sealed record CarrierEntry(string Code, string Name, string TrackingUrlPattern);

/// <summary>A company that left the list or changed its name: it resolves by name on an old order and never has a link (08 2.4 item 2).</summary>
public sealed record RetiredCarrierEntry(string Code, string Name);

/// <summary>The tracking link as the order page renders it: opens in a new tab and carries <c>noreferrer</c> (08 2.4 item 1).</summary>
public sealed record TrackingLink(string Href)
{
    /// <summary>The <c>target</c> attribute of the link.</summary>
    public const string Target = "_blank";

    /// <summary>The <c>rel</c> attribute of the link: the order page address is an access key and must not leak through Referer (05 6.5.4).</summary>
    public const string Rel = "noreferrer";
}

/// <summary>
/// What the delivery area of an order shows (04 5.16.8): the company name and the tracking number, the "Takip et" link only for a listed company with a
/// number, or the own-vehicle declaration alone.
/// </summary>
public sealed record ShippingDisplay(string? CarrierName, string? TrackingNumber, bool IsOwnVehicleDeclaration, TrackingLink? Link);

/// <summary>
/// The carrier list and its tracking link patterns (08 2.4; 06 5.3.12, 8.4.1). The list lives in code: no setting, no table, no panel screen - changing it
/// is a maintenance release (K-142). The product never calls a carrier from the server; the link is the customer's own browser visit.
/// </summary>
public sealed class CarrierCatalog
{
    /// <summary>The single place holder of a pattern where the percent-encoded tracking number goes.</summary>
    public const string Placeholder = "{takipNumarasi}";

    /// <summary>
    /// The product list: the four companies of 08 2.4. The browser try with a real tracking number (DV-32) is done at T601 (K-1576); a company whose pattern
    /// fails it leaves the list and is "Diğer". MNG Kargo, PTT Kargo and HepsiJet were not readable (DV-33) and stay "Diğer".
    /// </summary>
    public static CarrierCatalog Default { get; } = new(
        [
            new CarrierEntry("YurticiKargo", "Yurtiçi Kargo", "https://www.yurticikargo.com/tr/online-servisler/gonderi-sorgula?code={takipNumarasi}"),
            new CarrierEntry("ArasKargo", "Aras Kargo", "https://kargotakip.araskargo.com.tr/mainpage.aspx?code={takipNumarasi}"),
            new CarrierEntry("SuratKargo", "Sürat Kargo", "https://www.suratkargo.com.tr/KargoTakip/?kargotakipno={takipNumarasi}"),
            new CarrierEntry("Ups", "UPS", "https://www.ups.com/track?loc=tr_TR&tracknum={takipNumarasi}"),
        ],
        []);

    private readonly Dictionary<string, CarrierEntry> _active;
    private readonly Dictionary<string, RetiredCarrierEntry> _retired;

    /// <summary>Builds a catalog and checks the rules of 08 2.4: ASCII single-word code starting with a capital, codes never shared between companies, https pattern with one place holder.</summary>
    /// <exception cref="ArgumentException">An entry breaks a rule.</exception>
    public CarrierCatalog(IReadOnlyList<CarrierEntry> active, IReadOnlyList<RetiredCarrierEntry> retired)
    {
        ArgumentNullException.ThrowIfNull(active);
        ArgumentNullException.ThrowIfNull(retired);

        HashSet<string> codes = new(StringComparer.Ordinal);
        _active = [];
        _retired = [];
        foreach (CarrierEntry entry in active)
        {
            CheckCode(entry.Code, codes);
            CheckName(entry.Name);
            CheckPattern(entry.Code, entry.TrackingUrlPattern);
            _active.Add(entry.Code, entry);
        }

        foreach (RetiredCarrierEntry entry in retired)
        {
            CheckCode(entry.Code, codes);
            CheckName(entry.Name);
            _retired.Add(entry.Code, entry);
        }

        Active = active;
        Retired = retired;
    }

    /// <summary>The companies a new shipment can be marked with, in list order.</summary>
    public IReadOnlyList<CarrierEntry> Active { get; }

    /// <summary>The companies that left the list; their code stays reserved.</summary>
    public IReadOnlyList<RetiredCarrierEntry> Retired { get; }

    /// <summary>True when <paramref name="code"/> can be chosen on a new shipment: a retired or unknown code cannot (08 2.4 item 2).</summary>
    public bool IsSelectable(string? code) => code is not null && _active.ContainsKey(code);

    /// <summary>The display name of a code - listed or retired - or <c>null</c> when the code is not known.</summary>
    public string? FindName(string? code)
    {
        if (code is null)
        {
            return null;
        }

        if (_active.TryGetValue(code, out CarrierEntry? entry))
        {
            return entry.Name;
        }

        return _retired.TryGetValue(code, out RetiredCarrierEntry? retired) ? retired.Name : null;
    }

    /// <summary>The tracking link of a listed company and a typed number; <c>null</c> for a retired, unknown or absent company or an empty number (08 2.4).</summary>
    public TrackingLink? BuildLink(string? code, string? trackingNumber)
    {
        if (code is null || string.IsNullOrWhiteSpace(trackingNumber) || !_active.TryGetValue(code, out CarrierEntry? entry))
        {
            return null;
        }

        // Only the number is put into the address, percent-encoded; nothing else of the address is derived from it.
        return new TrackingLink(entry.TrackingUrlPattern.Replace(Placeholder, Uri.EscapeDataString(trackingNumber), StringComparison.Ordinal));
    }

    /// <summary>
    /// The delivery area of an order (04 5.16.8; 08 2.4): own-vehicle delivery shows the declaration alone; "Diğer" and a retired company show name and number
    /// without a link; a listed company with a number shows the link.
    /// </summary>
    public ShippingDisplay Describe(string? carrier, string? otherCarrierName, string? trackingNumber, bool isOwnVehicleDelivery)
    {
        if (isOwnVehicleDelivery)
        {
            return new ShippingDisplay(null, null, true, null);
        }

        string? number = string.IsNullOrWhiteSpace(trackingNumber) ? null : trackingNumber;
        if (string.IsNullOrEmpty(carrier))
        {
            return new ShippingDisplay(string.IsNullOrWhiteSpace(otherCarrierName) ? null : otherCarrierName, number, false, null);
        }

        // A code the list does not know is shown by its code name: the name stays in code, so this only happens on a damaged value.
        return new ShippingDisplay(FindName(carrier) ?? carrier, number, false, BuildLink(carrier, trackingNumber));
    }

    private static void CheckCode(string code, HashSet<string> seen)
    {
        if (string.IsNullOrEmpty(code) || !char.IsAsciiLetterUpper(code[0]) || !code.All(char.IsAsciiLetterOrDigit))
        {
            throw new ArgumentException($"Carrier code '{code}' must be one ASCII word starting with a capital letter.", nameof(code));
        }

        if (!seen.Add(code))
        {
            throw new ArgumentException($"Carrier code '{code}' is already given to a company and is permanent.", nameof(code));
        }
    }

    private static void CheckName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Carrier name must not be empty.", nameof(name));
        }
    }

    private static void CheckPattern(string code, string pattern)
    {
        int first = pattern.IndexOf(Placeholder, StringComparison.Ordinal);
        bool single = first >= 0 && pattern.IndexOf(Placeholder, first + Placeholder.Length, StringComparison.Ordinal) < 0;
        if (!single || !pattern.StartsWith("https://", StringComparison.Ordinal))
        {
            throw new ArgumentException($"Tracking pattern of '{code}' must be an https address with exactly one {Placeholder}.", nameof(pattern));
        }
    }
}
