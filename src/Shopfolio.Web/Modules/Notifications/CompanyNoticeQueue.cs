using Shopfolio.Infrastructure;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>Where the firm notices F-1…F-4 go: the firm's contact e-mail (<c>Company.Email</c>; 02 9.3.1), read from the Settings module's open surface.</summary>
internal interface ICompanyContactEmail
{
    /// <summary>The contact e-mail as entered; null while the firm has entered none.</summary>
    Task<string?> GetAsync(CancellationToken ct);
}

/// <summary>The contact e-mail from the Settings module's open legal identity (09 6 item 1).</summary>
internal sealed class CompanyContactEmail : ICompanyContactEmail
{
    private readonly ISettingsModule _settings;

    public CompanyContactEmail(ISettingsModule settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings;
    }

    public async Task<string?> GetAsync(CancellationToken ct)
    {
        LegalIdentity identity = await _settings.GetLegalIdentityAsync(ct);
        return string.IsNullOrWhiteSpace(identity.Email) ? null : identity.Email.Trim();
    }
}

/// <summary>
/// The writer of the firm notices' dispatch rows (08 2.2.7; 06 4.58; T308): each notice is one row in the "queued" state, written in the transaction of the
/// event that made it and saved by that transaction (09 9.1 item 3; the row commits with the event or goes with it). F-1…F-4 are one row to the firm's contact
/// e-mail; F-5 and F-6 are one separate row for every administrator address. The rows carry no order link (the order is the related record of F-1 only as a
/// type and an id - a value, 06 2.2.6) and so never take the "e-mail did not arrive" mark of an order; the retry rule of <see cref="DispatchRetrySchedule"/> applies to
/// them as to every row, and no row of these six types is ever re-sent by hand (08 2.2.7). Nothing is sent here.
/// </summary>
internal sealed class CompanyNoticeQueue
{
    private readonly AppDbContext _db;
    private readonly ICompanyContactEmail _contact;
    private readonly TimeProvider _time;

    public CompanyNoticeQueue(AppDbContext db, ICompanyContactEmail contact, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(contact);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _contact = contact;
        _time = time;
    }

    /// <summary>F-1…F-4: one row to the firm's contact e-mail; zero rows while the firm has entered none.</summary>
    public async Task<int> QueueCompanyAsync(CompanyNotice notice, CancellationToken ct)
    {
        CompanyNoticeRow row = CompanyNoticeValues.ForCompany(notice);
        RequireTransaction();
        string? recipient = await _contact.GetAsync(ct);
        if (recipient is null)
        {
            return 0;
        }

        _db.DispatchRecords.Add(Make(row, recipient));
        return 1;
    }

    /// <summary>F-5 and F-6: one separate row for every address; the same address twice is one row.</summary>
    public int QueueAdmins(AdminNotice notice, IReadOnlyCollection<string> adminEmails)
    {
        ArgumentNullException.ThrowIfNull(adminEmails);
        CompanyNoticeRow row = CompanyNoticeValues.ForAdmins(notice);
        RequireTransaction();
        List<string> recipients = [];
        foreach (string email in adminEmails)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("An administrator address is empty.", nameof(adminEmails));
            }

            string trimmed = email.Trim();
            if (!recipients.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
            {
                recipients.Add(trimmed);
            }
        }

        if (recipients.Count == 0)
        {
            throw new ArgumentException("A notice of the administrators needs at least one address: the administrator who acted is one.", nameof(adminEmails));
        }

        foreach (string recipient in recipients)
        {
            _db.DispatchRecords.Add(Make(row, recipient));
        }

        return recipients.Count;
    }

    private DispatchRecord Make(CompanyNoticeRow row, string recipient)
    {
        DispatchRecord record = DispatchRecord.Queue(row.Type, recipient, null, row.RelatedRecordType, row.RelatedRecordId, _time.GetUtcNow());
        record.EventValues = row.EventValues;
        return record;
    }

    private void RequireTransaction()
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("A firm notice's row is written in the caller's transaction and never alone (09 9.1 item 3).");
        }
    }
}
