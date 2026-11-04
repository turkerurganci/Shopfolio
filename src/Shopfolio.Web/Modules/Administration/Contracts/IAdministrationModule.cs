namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>
/// The open surface of the Administration module (09 6 item 1; 05 3.4.1): what other modules, the pages and the commands may call. The module owns the operation trail and the admin accounts (05 3.3.9).
/// The members arrive with the tasks that own the module's rules, as methods that take and give immutable records and join the caller's
/// database transaction (09 6 item 3).
/// </summary>
public interface IAdministrationModule
{
}
