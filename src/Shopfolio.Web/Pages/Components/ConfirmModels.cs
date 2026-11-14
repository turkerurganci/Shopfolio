using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.Pages.Components;

/// <summary>The two faces of the one confirmation (04 2.4): the administrator's window and the customer's last step (K-744).</summary>
public enum ConfirmSurface
{
    /// <summary>04 2.4.4: a window; the focus is on "Vazgeç"; full screen on the narrow class; the script-free path draws it as the page's own confirmation state (07 2.2.4).</summary>
    AdminWindow,

    /// <summary>04 2.4.5: the last step of the operation's own page - not a window - ending with a "Vazgeç" that goes back to the page the customer came from.</summary>
    CustomerStep,
}

/// <summary>
/// The anatomy of a confirmation (T541; 04 2.4.1): the name of the operation, the one sentence that says the result, "Bu işlem geri alınamaz" when the operation can not be undone, a button named
/// after the operation and "Vazgeç". The model is built by <see cref="Confirmations"/> only; it refuses "Evet" and "Tamam" as the confirm button and a sentence that is empty.
/// </summary>
/// <param name="Surface">The window of the administrator or the customer's step.</param>
/// <param name="OperationName">The name of the operation, the heading ("Kargoya ver").</param>
/// <param name="Warning">The warning that comes before the sentence when the operation has one (04 2.4.3); null otherwise.</param>
/// <param name="Sentence">The one sentence that says the result; carries the number when the operation has one.</param>
/// <param name="IsIrreversible">True adds "Bu işlem geri alınamaz." after the sentence.</param>
/// <param name="ConfirmLabel">The name of the operation on the button.</param>
/// <param name="ActionPath">The path of the page the form posts to; a local address.</param>
/// <param name="Handler">The named handler that applies the operation (07 2.2.1).</param>
/// <param name="CancelPath">Where "Vazgeç" goes: the page the operation was started from; a local address.</param>
/// <param name="Fields">The values the confirmation carries forward as hidden fields (the form's own fields and its confirmation mark).</param>
public sealed record ConfirmView(
    ConfirmSurface Surface,
    string OperationName,
    string? Warning,
    string Sentence,
    bool IsIrreversible,
    string ConfirmLabel,
    string ActionPath,
    string Handler,
    string CancelPath,
    IReadOnlyList<KeyValuePair<string, string>> Fields)
{
    /// <summary>The label of the second button (04 2.4.1).</summary>
    public const string CancelLabel = "Vazgeç";

    /// <summary>The element id of the heading the window is named by.</summary>
    public const string HeadingId = "confirm-heading";

    /// <summary>
    /// What the customer's last step cancels or terminates, each item by name and units as the server worked them out (04 5.17.6, 2.12.1.8; K-795): drawn above the sentence and kept with it
    /// (04 5.17.15); empty on a confirmation without items.
    /// </summary>
    public IReadOnlyList<ConfirmItem> Items { get; init; } = [];

    /// <summary>The lines of the body in order: the warning if any, the sentence, the irreversible sentence if it applies.</summary>
    public IReadOnlyList<string> Lines
    {
        get
        {
            List<string> lines = [];
            if (Warning is not null)
            {
                lines.Add(Warning);
            }

            lines.Add(Sentence);
            if (IsIrreversible)
            {
                lines.Add(IrreversibleConfirmations.IrreversibleSentence);
            }

            return lines;
        }
    }
}

/// <summary>One item of the last step's list (04 5.17.6): the item's name with its option values and the units the operation takes.</summary>
public sealed record ConfirmItem(string Name, int Quantity);

/// <summary>
/// The only constructors of a <see cref="ConfirmView"/> (T541; 04 2.4). The rules of the anatomy are checked here, once: a button named "Evet" or "Tamam" is refused, an irreversible operation
/// without its sentence is impossible, the cancel path is a local address. The confirmations of the eight irreversible operations (<see cref="IrreversibleConfirmations"/>) and of the customer's four
/// are written by the screens that own them.
/// </summary>
public static class Confirmations
{
    private static readonly string[] _vagueLabels = ["Evet", "Tamam", "Onayla", "Devam", "Devam et"];

    /// <summary>The name of the administrator removal's operation and of its button (04 9.20.10; K-814).</summary>
    public const string AdminRemovalOperationName = "Yöneticiyi kaldır";

    /// <summary>
    /// 04 9.20.10 (K-662, K-814): the administrator's window of the administrator removal. The operation is irreversible (the account, its sessions and its invitations end) but it is not one of the eight of
    /// <see cref="IrreversibleConfirmation"/>, whose list is closed, so its sentence is written here: the removed one's name and address, the immediate end of the open sessions, the number of unused invitations
    /// that become invalid (only when there is one) and the notice to every administrator.
    /// </summary>
    public static ConfirmView ForAdminRemoval(string name, string email, int invalidatedInvitations, string actionPath, string handler, string cancelPath, IReadOnlyList<KeyValuePair<string, string>> fields)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentOutOfRangeException.ThrowIfNegative(invalidatedInvitations);
        string invitations = invalidatedInvitations > 0
            ? $" Gönderdiği kullanılmamış {invalidatedInvitations.ToString(System.Globalization.CultureInfo.InvariantCulture)} davet geçersizleşecek."
            : string.Empty;
        string sentence = $"{name} ({email}) yönetici olarak kaldırılacak ve açık oturumları hemen kapanacak.{invitations} Kaldırma bütün yöneticilere bildirilecek.";
        return Build(ConfirmSurface.AdminWindow, AdminRemovalOperationName, null, sentence, true, AdminRemovalOperationName, actionPath, handler, cancelPath, fields);
    }

    /// <summary>04 2.4.4: the administrator's window for an irreversible operation of 03 1.6.1: the texts are those of <see cref="IrreversibleConfirmation"/> (K-797, K-798).</summary>
    public static ConfirmView ForIrreversible(
        IrreversibleConfirmation confirmation,
        string operationName,
        string confirmLabel,
        string actionPath,
        string handler,
        string cancelPath,
        IReadOnlyList<KeyValuePair<string, string>> fields)
    {
        ArgumentNullException.ThrowIfNull(confirmation);
        string? warning = confirmation.Warning;
        return Build(ConfirmSurface.AdminWindow, operationName, warning, confirmation.Sentence, true, confirmLabel, actionPath, handler, cancelPath, fields);
    }

    /// <summary>
    /// 04 2.4.2.2 (K-655, K-797): the administrator's window of a permanent deletion - a product or a variant (04 9.5.15). The deletion is irreversible but is not one of the eight of
    /// <see cref="IrreversibleConfirmation"/>, whose list is closed; the screen writes K-797's sentence and the window adds "Bu işlem geri alınamaz.".
    /// </summary>
    public static ConfirmView ForPermanentDeletion(string sentence, string confirmLabel, string actionPath, string handler, string cancelPath, IReadOnlyList<KeyValuePair<string, string>> fields) =>
        Build(ConfirmSurface.AdminWindow, confirmLabel, null, sentence, true, confirmLabel, actionPath, handler, cancelPath, fields);

    /// <summary>04 2.4.2.3: the administrator's window for a reversible setting whose result is told with a number - no "geri alınamaz" sentence.</summary>
    public static ConfirmView ForSettingChange(string operationName, string sentence, string confirmLabel, string actionPath, string handler, string cancelPath, IReadOnlyList<KeyValuePair<string, string>> fields) =>
        Build(ConfirmSurface.AdminWindow, operationName, null, sentence, false, confirmLabel, actionPath, handler, cancelPath, fields);

    /// <summary>04 2.4.5: the customer's last step: the same anatomy inside the page; the operation can not be undone, so the irreversible sentence stays.</summary>
    public static ConfirmView ForCustomer(string operationName, string sentence, string confirmLabel, string actionPath, string handler, string cancelPath, IReadOnlyList<KeyValuePair<string, string>> fields) =>
        Build(ConfirmSurface.CustomerStep, operationName, null, sentence, true, confirmLabel, actionPath, handler, cancelPath, fields);

    private static ConfirmView Build(
        ConfirmSurface surface,
        string operationName,
        string? warning,
        string sentence,
        bool irreversible,
        string confirmLabel,
        string actionPath,
        string handler,
        string cancelPath,
        IReadOnlyList<KeyValuePair<string, string>> fields)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        ArgumentException.ThrowIfNullOrWhiteSpace(sentence);
        ArgumentException.ThrowIfNullOrWhiteSpace(confirmLabel);
        ArgumentException.ThrowIfNullOrWhiteSpace(handler);
        ArgumentNullException.ThrowIfNull(fields);
        if (Array.Exists(_vagueLabels, label => string.Equals(label, confirmLabel.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("The confirm button carries the name of the operation, never \"Evet\" or \"Tamam\" (04 2.4.1).", nameof(confirmLabel));
        }

        if (!ReturnPath.IsLocalPath(actionPath) || !ReturnPath.IsLocalPath(cancelPath))
        {
            throw new ArgumentException("The form's address and the way back are paths of this site (05 6.5.6).");
        }

        return new ConfirmView(surface, operationName.Trim(), warning, sentence.Trim(), irreversible, confirmLabel.Trim(), actionPath, handler, cancelPath, fields);
    }
}
