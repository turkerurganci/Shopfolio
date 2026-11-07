using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Settings.Contracts;

using FieldRejection = Shopfolio.Modules.Settings.Contracts.FieldRejection;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// The one writer of the company settings row (09 10.3 items 1-4): the row is read without tracking and written only by the section methods of this
/// class, each by one conditional update of its own section version (06 2.6.3) - never by <c>SaveChanges</c>, so a section's save does not depend on
/// the other sections' versions. The order of a section's save is fixed: the row lock, then the check against the row as it is, then the update
/// (and, in the tasks that own them, the section's sub-records). The row lock is also the lock that the identity save and the legal text writer take
/// (05 7.3.5; K-1088): they are serialized on the one row. The sections arrive with the tasks that own them; this task owns the return address.
/// </summary>
internal sealed class CompanySettingsWriter
{
    private readonly AppDbContext _db;
    private readonly CacheInvalidations _invalidations;

    public CompanySettingsWriter(AppDbContext db, CacheInvalidations invalidations)
    {
        _db = db;
        _invalidations = invalidations;
    }

    /// <summary>
    /// Locks the one settings row until the end of the transaction (05 7.3.5; 09 10.3 item 2 (a)). Taken alone: a transaction that holds it takes
    /// no order, variant or coupon lock (<see cref="LockSequence"/>).
    /// </summary>
    public async Task LockAsync(CancellationToken ct)
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The company settings row lock is taken only inside a transaction (09 10.3 item 2).");
        }

        LockSequence.Enter(_db, LockKind.CompanySettings, null);
        await _db.Database.SqlQuery<Guid>($"SELECT id AS \"Value\" FROM company_settings FOR UPDATE").ToListAsync(ct);
    }

    /// <summary>
    /// Writes the "close sales temporarily" switch (02 3.1.6; 09 10.3 item 3): the switch carries no version and takes no row lock, so the write is one update conditional on the
    /// value the screen showed (<paramref name="original"/>). <see langword="false"/> means the switch is no longer that value - another administrator changed it - and nothing
    /// was written. The caller writes the trail row in the same transaction.
    /// </summary>
    public async Task<bool> WriteSalesSwitchAsync(bool original, bool target, CancellationToken ct)
    {
        int written = await _db.CompanySettings
            .Where(s => s.IsSalesTemporarilyClosed == original)
            .ExecuteUpdateAsync(set => set.SetProperty(s => s.IsSalesTemporarilyClosed, target), ct);
        if (written == 0)
        {
            return false;
        }

        _invalidations.Add(CacheArea.Settings);
        return true;
    }

    /// <summary>The return address section as stored: the address when all four required parts are there, and the section's version.</summary>
    public async Task<ReturnAddressSection> ReadReturnAddressAsync(CancellationToken ct)
    {
        var row = await _db.CompanySettings.AsNoTracking()
            .Select(s => new { s.ReturnRecipientName, s.ReturnProvinceId, s.ReturnDistrict, s.ReturnAddressLine, s.ReturnPhone, s.ReturnAddressVersion })
            .SingleAsync(ct);
        ReturnAddressSnapshot? address = row.ReturnRecipientName is not null && row.ReturnProvinceId is Guid province
            && row.ReturnDistrict is not null && row.ReturnAddressLine is not null
            ? new ReturnAddressSnapshot(row.ReturnRecipientName, province, row.ReturnDistrict, row.ReturnAddressLine, row.ReturnPhone)
            : null;
        return new ReturnAddressSection(address, row.ReturnAddressVersion);
    }

    /// <summary>
    /// The return address section's save (02 3.1.7; 04 7.2.21.6, 7.2.21.7): in the caller's transaction, row lock first. The four required parts come
    /// together; once they are filled none can be emptied; the telephone is optional and, like the address, is not accepted alone. A version that is
    /// not the section's now writes nothing and is a result (09 10.3 item 2).
    /// </summary>
    public async Task<SettingsSaveResult> SaveReturnAddressAsync(ReturnAddressInput input, int formVersion, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        await LockAsync(ct);

        string? recipient = CompanyIdentityRules.Text(input.RecipientName);
        Guid? province = input.ProvinceId == Guid.Empty ? null : input.ProvinceId;
        string? district = CompanyIdentityRules.Text(input.District);
        string? addressLine = CompanyIdentityRules.Text(input.AddressLine);
        string? phone = CompanyIdentityRules.Phone(input.Phone);
        bool[] filled = [recipient is not null, province is not null, district is not null, addressLine is not null];

        ReturnAddressSection current = await ReadReturnAddressAsync(ct);
        List<FieldRejection> rejections = [];
        if (phone is not null && new InvalidPhoneAttribute().GetValidationResult(phone, new System.ComponentModel.DataAnnotations.ValidationContext(phone)) is not null)
        {
            rejections.Add(new FieldRejection(nameof(ReturnAddressInput.Phone), FieldCodes.InvalidPhone));
        }

        if (province is not null && !await _db.Provinces.AnyAsync(p => p.Id == province, ct))
        {
            rejections.Add(new FieldRejection(nameof(ReturnAddressInput.ProvinceId), FieldCodes.InvalidProvince));
        }

        // The four parts come together; a telephone alone is no address (the declaration copies it only with one). An address that was complete stays so.
        if (filled.Any(f => f) || phone is not null || current.IsComplete)
        {
            string missingCode = current.IsComplete ? FieldCodes.FilledFieldCleared : FieldCodes.FieldRequired;
            string[] names =
            [
                nameof(ReturnAddressInput.RecipientName), nameof(ReturnAddressInput.ProvinceId), nameof(ReturnAddressInput.District), nameof(ReturnAddressInput.AddressLine),
            ];
            for (int i = 0; i < filled.Length; i++)
            {
                if (!filled[i])
                {
                    rejections.Add(new FieldRejection(names[i], missingCode));
                }
            }
        }

        if (rejections.Count > 0)
        {
            return SettingsSaveResult.Refused(rejections);
        }

        int written = await _db.CompanySettings
            .Where(s => s.ReturnAddressVersion == formVersion)
            .ExecuteUpdateAsync(
                set => set
                    .SetProperty(s => s.ReturnRecipientName, recipient)
                    .SetProperty(s => s.ReturnProvinceId, province)
                    .SetProperty(s => s.ReturnDistrict, district)
                    .SetProperty(s => s.ReturnAddressLine, addressLine)
                    .SetProperty(s => s.ReturnPhone, phone)
                    .SetProperty(s => s.ReturnAddressVersion, s => s.ReturnAddressVersion + 1),
                ct);
        if (written == 0)
        {
            return SettingsSaveResult.VersionConflict;
        }

        _invalidations.Add(CacheArea.Settings);
        return SettingsSaveResult.Saved;
    }
}
