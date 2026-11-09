namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>
/// The panel home's data (02 10.6.1, 10.8.2; 11 T382; KP-50, KP-65): the six pending work counters, the two warnings and the setup checklist. It is computed from the records at every
/// call and never stored; no new screen and no new data (K-405, K-467). The page that shows it is E-31's (T52x).
/// </summary>
public interface IPanelHome
{
    /// <summary>The home's data now; nothing is written.</summary>
    Task<PanelHome> GetAsync(CancellationToken ct);
}

/// <summary>
/// The six counters that wait for the firm, the two read warnings, and the setup checklist - null once every item is complete (02 10.8.2).
/// </summary>
/// <param name="Counters">Exactly six, one per <see cref="PanelCounterKind"/>, in the enum's order; a seventh counter is never opened (02 10.6.1; K-535, K-467).</param>
/// <param name="ChannelWarningOpen">The e-mail channel warning (05 10.2.3).</param>
/// <param name="FirmNoticesWarningOpen">The firm notices warning (05 10.2.4).</param>
/// <param name="SetupChecklist">The five items of 02 10.8.2 with the completed ones marked; null when all are complete.</param>
public sealed record PanelHome(
    IReadOnlyList<PanelCounter> Counters,
    bool ChannelWarningOpen,
    bool FirmNoticesWarningOpen,
    IReadOnlyList<SetupChecklistItem>? SetupChecklist);

/// <summary>One counter: its name, the count and the list it leads to. The unit is the order, except the open request, whose unit is the request.</summary>
public sealed record PanelCounter(PanelCounterKind Kind, int Count, PanelListTarget ListTarget);

/// <summary>One item: done, or waiting - and for the identity, the missing field names.</summary>
public sealed record SetupChecklistItem(SetupChecklistItemKind Kind, bool IsDone, IReadOnlyList<string> MissingFields);
