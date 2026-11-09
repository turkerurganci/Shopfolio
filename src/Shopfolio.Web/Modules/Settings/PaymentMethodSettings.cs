using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Settings.Contracts;

using FieldRejection = Shopfolio.Modules.Settings.Contracts.FieldRejection;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// The payment methods section (02 3.21.5; T353 criteria 1, 3): the bank transfer switch and its IBAN, saved like the other sections (<see cref="FirmSettings"/>) - the row lock, the
/// check against the row as it is, the conditional update of the section's version, the trail row - and, when the IBAN is entered or changed, F-5 to every administrator in the
/// same transaction (02 9.3.3). The IBAN rule is the project's one rule (<see cref="InvalidIbanAttribute"/>) and the stored form is <see cref="RequestFormat.CompactIban"/>.
/// </summary>
internal sealed class PaymentMethodSettings : IPaymentMethodSettings
{
    /// <summary>The name of the section as the trail shows it (02 10.3.1).</summary>
    internal const string DisplayName = "Ödeme yöntemleri";

    private static readonly InvalidIbanAttribute IbanRule = new();

    private readonly AppDbContext _db;
    private readonly CompanySettingsWriter _writer;
    private readonly IAuditTrail _trail;
    private readonly IAdminNotices _adminNotices;

    public PaymentMethodSettings(AppDbContext db, CompanySettingsWriter writer, IAuditTrail trail, IAdminNotices adminNotices)
    {
        _db = db;
        _writer = writer;
        _trail = trail;
        _adminNotices = adminNotices;
    }

    public async Task<PaymentMethodsSection> GetAsync(CancellationToken ct)
    {
        return await _db.CompanySettings.AsNoTracking()
            .Select(s => new PaymentMethodsSection(s.IsBankTransferEnabled, s.BankTransferIban, s.PaymentMethodsVersion))
            .SingleAsync(ct);
    }

    public async Task<SettingsSaveResult> SaveAsync(PaymentMethodsInput input, int formVersion, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        await _writer.LockAsync(ct);
        PaymentMethodsSection current = await GetAsync(ct);

        // 02 3.21.5: the IBAN is required while the transfer is on; an IBAN written is a valid Turkish IBAN whether the switch is on or off (04 7.3.2.7).
        string? iban = string.IsNullOrWhiteSpace(input.BankTransferIban) ? null : RequestFormat.CompactIban(input.BankTransferIban);
        if (iban is null && input.IsBankTransferEnabled)
        {
            return SettingsSaveResult.Refused([new FieldRejection(nameof(PaymentMethodsInput.BankTransferIban), FieldCodes.FieldRequired)]);
        }

        if (iban is not null && !IbanRule.Allows(iban))
        {
            return SettingsSaveResult.Refused([new FieldRejection(nameof(PaymentMethodsInput.BankTransferIban), FieldCodes.InvalidIban)]);
        }

        List<AuditValueChange> changes = [];
        if (current.IsBankTransferEnabled != input.IsBankTransferEnabled)
        {
            changes.Add(new AuditValueChange(nameof(CompanySettings.IsBankTransferEnabled), Text(current.IsBankTransferEnabled), Text(input.IsBankTransferEnabled)));
        }

        bool ibanChanged = !string.Equals(current.BankTransferIban, iban, StringComparison.Ordinal);
        if (ibanChanged)
        {
            // 02 9.3.3: the trail keeps the change with its old and new value; the e-mail carries the last four digits only.
            changes.Add(new AuditValueChange(nameof(CompanySettings.BankTransferIban), current.BankTransferIban, iban));
        }

        if (changes.Count == 0)
        {
            return current.Version == formVersion ? SettingsSaveResult.Saved : SettingsSaveResult.VersionConflict;
        }

        if (!await _writer.WritePaymentMethodsAsync(input.IsBankTransferEnabled, iban, formVersion, ct))
        {
            return SettingsSaveResult.VersionConflict;
        }

        Guid rowId = await _db.CompanySettings.AsNoTracking().Select(s => s.Id).SingleAsync(ct);
        await _trail.WriteAsync(new AuditEntry(AuditOperation.UpdateSettings, nameof(CompanySettings), rowId, new AuditDetails(DisplayName, changes)), ct);

        // 02 3.21.5, 9.3.3 (K-616): entered for the first time or changed - a cleared IBAN is neither and tells no one.
        if (ibanChanged && iban is not null)
        {
            await _adminNotices.QueueIbanChangedAsync(iban, wasFirstEntry: current.BankTransferIban is null, ct);
        }

        return SettingsSaveResult.Saved;
    }

    private static string Text(bool value) => value ? "true" : "false";
}

/// <summary>
/// The bank transfer line of the sales gate's payment method condition (02 3.1.5 condition 2, 3.21.5; D-198; T353 criterion 1): open when the transfer is switched on and the IBAN
/// is filled. The same reading feeds the gate and the offered methods (<c>PaymentMethods</c>), so the step, the confirmation and the gate say the same thing.
/// </summary>
internal sealed class BankTransferCondition : IBankTransferCondition
{
    private readonly AppDbContext _db;

    public BankTransferCondition(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> IsOpenAsync(CancellationToken ct)
    {
        return await _db.CompanySettings.AsNoTracking().Select(s => s.IsBankTransferEnabled && s.BankTransferIban != null).SingleAsync(ct);
    }
}
