namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>One administrator of the accounts screen (T522; 04 9.20.3): the id is the internal key the removal names, the name and the address are the contact data of 02 10.7.4.</summary>
public sealed record AdminAccountRow(Guid Id, string Name, string Email);

/// <summary>
/// One valid invitation of the accounts screen (T522; 04 9.20.4): the invited address, the sending administrator's name as written on the invitation, the sending moment, the last moment the link lives
/// and the "e-posta ulaşmadı" mark of the invitation e-mail.
/// </summary>
public sealed record AdminInvitationRow(Guid Id, string Email, string SenderName, DateTimeOffset SentAt, DateTimeOffset LinkEndsAt, bool EmailNotDelivered);

/// <summary>What the accounts screen shows (T522; 07 UC-103): the administrators in the Turkish alphabetical order of their names and the valid invitations, newest first.</summary>
public sealed record AdminAccountList(IReadOnlyList<AdminAccountRow> Admins, IReadOnlyList<AdminInvitationRow> Invitations);

/// <summary>
/// The accounts screen's read side (T522; 07 UC-103; 04 9.20): the administrators and the valid invitations. A plain read: it writes nothing and joins no transaction. An invitation is valid while none of
/// its withdrawn, invalid and accepted moments is filled and a link of it lives (unused, not replaced, not run out) - the reading the removal's confirmation counts with.
/// </summary>
public interface IAdminAccountList
{
    /// <summary>The two lists; the number of reads does not depend on the number of rows (09 20 item 4): one for the administrators, one for the invitations, one for the marks.</summary>
    Task<AdminAccountList> ListAsync(CancellationToken ct);
}
