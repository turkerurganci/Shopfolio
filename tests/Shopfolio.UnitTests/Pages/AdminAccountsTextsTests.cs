using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Panel;

namespace Shopfolio.UnitTests.Pages;

/// <summary>The texts of the administrator accounts and operation trail screens (T522 criteria 6, 9; KK6, KK9; 04 9.20, 9.23; K-814).</summary>
public sealed class AdminAccountsTextsTests
{
    private static readonly KeyValuePair<string, string>[] Fields = [new("Input.AdminId", "7"), new("Input.Confirmed", "true")];

    [Fact]
    public void EveryAreaAndEveryOperationHasADistinctTurkishName_KK9()
    {
        string[] areas = [.. Enum.GetValues<AuditArea>().Select(AuditTexts.AreaOf)];
        string[] operations = [.. Enum.GetValues<AuditOperation>().Select(AuditTexts.OperationOf)];

        Assert.Equal(8, areas.Length);
        Assert.All(areas, a => Assert.False(string.IsNullOrWhiteSpace(a)));
        Assert.Equal(areas.Length, areas.Distinct(StringComparer.Ordinal).Count());
        Assert.All(operations, o => Assert.False(string.IsNullOrWhiteSpace(o)));
        Assert.Equal(operations.Length, operations.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void ACodeTheApplicationDoesNotKnow_IsShownAsItself_AndAKnownCodeAsItsName_KK9()
    {
        Assert.Equal(AuditTexts.OperationOf(AuditOperation.RemoveAdminAccount), AuditTexts.OperationOf("RemoveAdminAccount"));
        Assert.Contains("FutureOperation", AuditTexts.OperationOf("FutureOperation"), StringComparison.Ordinal);
    }

    [Fact]
    public void TheRemovalConfirmation_NamesTheAccount_TheSessionsTheNoticeAndTheIrreversibility_AndCountsInvitationsOnlyWhenThereAreAny_KK6()
    {
        ConfirmView none = Confirmations.ForAdminRemoval("Hedef", "hedef@example.com", 0, "/panel/yonetici-hesaplari", "RemoveAdmin", "/panel/yonetici-hesaplari", Fields);
        ConfirmView two = Confirmations.ForAdminRemoval("Hedef", "hedef@example.com", 2, "/panel/yonetici-hesaplari", "RemoveAdmin", "/panel/yonetici-hesaplari", Fields);

        Assert.Equal(ConfirmSurface.AdminWindow, none.Surface);
        Assert.True(none.IsIrreversible);
        Assert.Equal("Yöneticiyi kaldır", none.ConfirmLabel);
        Assert.Equal(Confirmations.AdminRemovalOperationName, none.OperationName);
        Assert.Equal("Hedef (hedef@example.com) yönetici olarak kaldırılacak ve açık oturumları hemen kapanacak. Kaldırma bütün yöneticilere bildirilecek.", none.Sentence);
        Assert.Contains("Gönderdiği kullanılmamış 2 davet geçersizleşecek.", two.Sentence, StringComparison.Ordinal);
        Assert.DoesNotContain("davet", none.Sentence, StringComparison.Ordinal);
        Assert.Throws<ArgumentOutOfRangeException>(() => Confirmations.ForAdminRemoval("Hedef", "hedef@example.com", -1, "/p", "RemoveAdmin", "/p", Fields));
    }

    [Fact]
    public void ATrailRow_ShowsTheNameAndAddressOfThatMoment_TheNamesAndTheValues_AndNothingElse_KK9()
    {
        AuditRow row = new(
            Guid.NewGuid(),
            new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.Zero),
            Guid.NewGuid(),
            "Eski Ad",
            "eski@example.com",
            AuditArea.AdminAccounts,
            "SendAdminInvitation",
            "AdminInvitation",
            Guid.NewGuid(),
            new AuditDetails("davetli@example.com", [new AuditValueChange("Ad", "A", null)], [new AuditValue("Neden", "deneme")]));

        IReadOnlyList<string> cells = AuditTrailModel.RowOf(row).Cells;

        Assert.Equal(6, cells.Count);
        Assert.Equal("Eski Ad · eski@example.com", cells[1]);
        Assert.Equal(AuditTexts.AreaOf(AuditArea.AdminAccounts), cells[2]);
        Assert.Equal(AuditTexts.OperationOf(AuditOperation.SendAdminInvitation), cells[3]);
        Assert.Equal("davetli@example.com", cells[4]);
        Assert.Equal("Ad: A → —; Neden: deneme", cells[5]);
    }

    [Fact]
    public void ARowWithoutDetails_ShowsADashAsTheRecord_AndNoValues_KK9()
    {
        AuditRow row = new(Guid.NewGuid(), DateTimeOffset.UtcNow, Guid.NewGuid(), "Ad", "a@example.com", AuditArea.Export, "ExportOrders", null, null, null);

        IReadOnlyList<string> cells = AuditTrailModel.RowOf(row).Cells;

        Assert.Equal("—", cells[4]);
        Assert.Equal(string.Empty, cells[5]);
    }

    [Fact]
    public void TheAdministratorOption_CarriesTheSuffixOnlyForARemovedAccount_KK9()
    {
        AuditAdminOption kept = new(Guid.NewGuid(), "Ayşe", "ayse@example.com", false);
        AuditAdminOption gone = new(Guid.NewGuid(), "Giden", "giden@example.com", true);

        Assert.Equal("Ayşe (ayse@example.com)", AuditTrailModel.LabelOf(kept));
        Assert.Equal("Giden (giden@example.com) (kaldırıldı)", AuditTrailModel.LabelOf(gone));
    }
}
