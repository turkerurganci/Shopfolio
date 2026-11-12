using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Settings;

namespace Shopfolio.UnitTests.InstallationSettings;

/// <summary>
/// The first moment sales opened (T524; 02 10.6.3 m6; 06 10.6 m6; K-486; 11 T524 criteria 1 and 3): not stored, derived by rewinding the settings row with the old values of its
/// trail changes and replaying them with the legal texts' first publications through the gate's own rule. Never open is null, and the window is then not offered.
/// </summary>
public sealed class SalesOpeningRulesTests
{
    private static readonly DateTimeOffset Start = new(2026, 9, 1, 9, 0, 0, TimeSpan.Zero);

    private static DateTimeOffset At(int days, int hours = 0) => Start.AddDays(days).AddHours(hours);

    /// <summary>The settings row with every condition met: the transfer on with its IBAN, the switch off, the return address filled.</summary>
    private static Dictionary<string, string?> OpenRow() => new()
    {
        [nameof(CompanySettings.IsBankTransferEnabled)] = "true",
        [nameof(CompanySettings.BankTransferIban)] = "TR330006100519786457841326",
        [nameof(CompanySettings.IsSalesTemporarilyClosed)] = "false",
        [nameof(CompanySettings.ReturnRecipientName)] = "Depo",
        [nameof(CompanySettings.ReturnProvinceId)] = Guid.NewGuid().ToString(),
        [nameof(CompanySettings.ReturnDistrict)] = "Çankaya",
        [nameof(CompanySettings.ReturnAddressLine)] = "Depo Sk. 4",
    };

    private static AuditFieldChangeAt Change(DateTimeOffset at, string field, string? oldValue, string? newValue) => new(at, field, oldValue, newValue);

    [Fact]
    public void NoTrail_OpenRowAndBothTexts_IsTheLaterPublication()
    {
        DateTimeOffset? opened = SalesOpeningRules.FirstOpenedAt(OpenRow(), [], cardOpen: false, privacyNoticePublishedAt: At(3), cookiePolicyPublishedAt: At(5));

        Assert.Equal(At(5), opened);
    }

    [Fact]
    public void ATextNeverPublished_NeverOpened()
    {
        Assert.Null(SalesOpeningRules.FirstOpenedAt(OpenRow(), [], cardOpen: true, privacyNoticePublishedAt: At(1), cookiePolicyPublishedAt: null));
        Assert.Null(SalesOpeningRules.FirstOpenedAt(OpenRow(), [], cardOpen: true, privacyNoticePublishedAt: null, cookiePolicyPublishedAt: At(1)));
    }

    [Fact]
    public void TransferSwitchedOnAfterTheTexts_OpensAtTheSwitch()
    {
        AuditFieldChangeAt[] changes =
        [
            Change(At(7, 2), nameof(CompanySettings.IsBankTransferEnabled), "false", "true"),
            Change(At(7, 2), nameof(CompanySettings.BankTransferIban), null, "TR330006100519786457841326"),
        ];

        DateTimeOffset? opened = SalesOpeningRules.FirstOpenedAt(OpenRow(), changes, cardOpen: false, privacyNoticePublishedAt: At(1), cookiePolicyPublishedAt: At(2));

        Assert.Equal(At(7, 2), opened);
    }

    [Fact]
    public void TheCardLine_OpensWithoutTheTransfer()
    {
        Dictionary<string, string?> row = OpenRow();
        row[nameof(CompanySettings.IsBankTransferEnabled)] = "false";

        Assert.Equal(At(2), SalesOpeningRules.FirstOpenedAt(row, [], cardOpen: true, privacyNoticePublishedAt: At(1), cookiePolicyPublishedAt: At(2)));
        Assert.Null(SalesOpeningRules.FirstOpenedAt(row, [], cardOpen: false, privacyNoticePublishedAt: At(1), cookiePolicyPublishedAt: At(2)));
    }

    [Fact]
    public void ClosedNow_ButOpenEarlier_IsTheEarlierOpening()
    {
        // Opened when the switch went off on day 4, closed again on day 9 - the first opening stays the day 4 one.
        Dictionary<string, string?> row = OpenRow();
        row[nameof(CompanySettings.IsSalesTemporarilyClosed)] = "true";
        AuditFieldChangeAt[] changes =
        [
            Change(At(4), nameof(CompanySettings.IsSalesTemporarilyClosed), "true", "false"),
            Change(At(9), nameof(CompanySettings.IsSalesTemporarilyClosed), "false", "true"),
        ];

        Assert.Equal(At(4), SalesOpeningRules.FirstOpenedAt(row, changes, cardOpen: false, privacyNoticePublishedAt: At(1), cookiePolicyPublishedAt: At(2)));
    }

    [Fact]
    public void ConditionsNeverTogether_NeverOpened()
    {
        // The switch was off only while the transfer was off: never all at once, and closed now.
        Dictionary<string, string?> row = OpenRow();
        row[nameof(CompanySettings.IsSalesTemporarilyClosed)] = "true";
        AuditFieldChangeAt[] changes =
        [
            Change(At(3), nameof(CompanySettings.IsBankTransferEnabled), "true", "false"),
            Change(At(4), nameof(CompanySettings.IsSalesTemporarilyClosed), "true", "false"),
            Change(At(5), nameof(CompanySettings.IsSalesTemporarilyClosed), "false", "true"),
            Change(At(6), nameof(CompanySettings.IsBankTransferEnabled), "false", "true"),
        ];

        Assert.Null(SalesOpeningRules.FirstOpenedAt(row, changes, cardOpen: false, privacyNoticePublishedAt: At(1), cookiePolicyPublishedAt: At(2)));
    }

    [Fact]
    public void ChangesOfOtherSettings_AreIgnored()
    {
        AuditFieldChangeAt[] changes = [Change(At(10), nameof(CompanySettings.DefaultVatRate), "20.00", "10.00")];

        Assert.Equal(At(2), SalesOpeningRules.FirstOpenedAt(OpenRow(), changes, cardOpen: false, privacyNoticePublishedAt: At(1), cookiePolicyPublishedAt: At(2)));
    }

    [Fact]
    public void ReturnAddressEnteredLast_OpensWhenItsLastPartIsFilled()
    {
        AuditFieldChangeAt[] changes =
        [
            Change(At(6), nameof(CompanySettings.ReturnRecipientName), null, "Depo"),
            Change(At(6), nameof(CompanySettings.ReturnDistrict), null, "Çankaya"),
            Change(At(8), nameof(CompanySettings.ReturnProvinceId), null, Guid.NewGuid().ToString()),
            Change(At(8), nameof(CompanySettings.ReturnAddressLine), null, "Depo Sk. 4"),
        ];

        Assert.Equal(At(8), SalesOpeningRules.FirstOpenedAt(OpenRow(), changes, cardOpen: false, privacyNoticePublishedAt: At(1), cookiePolicyPublishedAt: At(2)));
    }
}
