namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>
/// The notices that every administrator is told about and that another module's operation causes (02 9.3.3; 08 2.2.7 F-5). The Administration module owns the accounts, so it
/// names the acting administrator and the addresses; the caller hands in only the event's values. Written in the caller's open transaction, which it joins and never opens.
/// </summary>
public interface IAdminNotices
{
    /// <summary>
    /// F-5 (02 3.21.5, 9.3.3; K-616): the bank transfer IBAN was entered (<paramref name="wasFirstEntry"/>) or changed. One row for every administrator account, the acting one
    /// included; the row keeps the last four digits of <paramref name="newIban"/> only (K-1252). Returns the number of rows written; zero when no administrator acts (nothing is
    /// queued: the save itself needs a signed-in administrator for its trail row).
    /// </summary>
    Task<int> QueueIbanChangedAsync(string newIban, bool wasFirstEntry, CancellationToken ct);
}
