namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>One change line (02 3.17.2): the item's name and the confirmed and the current value, so the page can say "Lambanın fiyatı 1.000 TL'den 1.200 TL'ye değişti".</summary>
public sealed record SummaryChange(SummaryChangeKind Kind, string? ProductName, decimal? Before, decimal? After);
