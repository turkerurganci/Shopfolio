using System.Reflection;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Settings;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Jobs;

/// <summary>
/// The register of the moments that run by themselves and the outage's rules without a database (T327 criteria 1, 2, 6, 8; KK1, KK2, KK6, KK8; 03 4.2;
/// 02 3.34.7, 4.3; 10 KD-60): the register has the twelve moments of 03 4.2 in order and every action it names exists; there is no maintenance mode and
/// no maintenance window; the only change the return from an outage makes is the transfer window's deferral, so no legal period is frozen or moved; the
/// legal periods are constants and not settings; the refund IBAN's deletion day deletes only when no refund still uses the IBAN.
/// </summary>
public sealed partial class AutomaticMomentsTests
{
    private const BindingFlags AnyMember = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    // ---- KK1: the twelve moments, each with a system action ----

    [Fact]
    public void Register_HasTheTwelveMomentsOf03_4_2_InTheirOrder_KK1()
    {
        string[] documented = [.. File.ReadAllLines(Path.Combine(RepositoryRoot.Find(), "Docs", "03_USER_FLOWS.md"))
            .Select(line => MomentRow().Match(line))
            .Where(match => match.Success)
            .Select(match => match.Groups[1].Value)];

        Assert.Equal(12, documented.Length);
        Assert.Equal(documented, AutomaticMoments.All.Select(m => m.Number));
    }

    [Fact]
    public void EveryMoment_HasAnAction_AndEveryActionIsAMemberOfTheProduct_KK1()
    {
        Assembly product = typeof(AutomaticMoments).Assembly;
        Assert.All(AutomaticMoments.All, moment =>
        {
            Assert.NotEmpty(moment.Actions);
            Assert.All(moment.Actions, action =>
            {
                int dot = action.LastIndexOf('.');
                Type? type = product.GetType(action[..dot]);
                Assert.True(type is not null, $"{moment.Number}: the type of '{action}' is not in the product.");
                Assert.True(type!.GetMember(action[(dot + 1)..], AnyMember).Length > 0, $"{moment.Number}: '{action}' names no member.");
            });
        });
    }

    // ---- KK2: no maintenance mode, no maintenance window ----

    [Fact]
    public void Product_HasNoMaintenanceModeAndNoMaintenanceWindow_KK2()
    {
        string source = Path.Combine(RepositoryRoot.Find(), "src");
        string[] hits = [.. Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".cs", StringComparison.Ordinal) || path.EndsWith(".json", StringComparison.Ordinal) || path.EndsWith(".cshtml", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => MaintenanceSwitch().IsMatch(File.ReadAllText(path)))];

        Assert.Empty(hits);
        Assert.DoesNotContain(typeof(CompanySettings).GetProperties(), p => p.Name.Contains("Maintenance", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void MaintenanceScan_CatchesEveryShapeOfTheSwitch_KK2()
    {
        string[] caught = ["MaintenanceMode", "maintenance_window", "IsInMaintenance", "\"Maintenance\": {", "planned maintenance", "bakım modu", "Bakım penceresi"];
        string[] passed = ["DatabaseSessionProfile.Maintenance", "MaintenanceConnectionString(provider)", "the maintenance session times"];

        Assert.All(caught, text => Assert.Matches(MaintenanceSwitch(), text));
        Assert.All(passed, text => Assert.DoesNotMatch(MaintenanceSwitch(), text));
    }

    // ---- KK2, KK8: the return changes only the transfer window; legal periods are never frozen or set ----

    [Fact]
    public void OnlyTheTransferWindowDeferral_IsAnOutageEffect_KK2_KK8()
    {
        Type[] effects = [.. typeof(AutomaticMoments).Assembly.GetTypes().Where(t => !t.IsAbstract && typeof(IOutageEffect).IsAssignableFrom(t))];

        Assert.Equal([typeof(TransferWindowDeferral)], effects);
    }

    [Fact]
    public void LegalPeriods_AreConstants_AndNoSettingCarriesThem_KK8()
    {
        FieldInfo[] legal =
        [
            typeof(AfterSalesPeriodRules).GetField(nameof(AfterSalesPeriodRules.WithdrawalWindowDays))!,
            typeof(AfterSalesPeriodRules).GetField(nameof(AfterSalesPeriodRules.RefundPeriodDays))!,
            typeof(AfterSalesPeriodRules).GetField(nameof(AfterSalesPeriodRules.ReturnShipmentPeriodDays))!,
            typeof(AfterSalesPeriodRules).GetField(nameof(AfterSalesPeriodRules.DefectClaimYears))!,
            typeof(DeliveryPeriodRules).GetField(nameof(DeliveryPeriodRules.LegalDeliveryLimitDays))!,
        ];
        Assert.All(legal, field => Assert.True(field.IsLiteral, $"{field.Name} is a constant (03 4; 02 4.3)."));

        string[] words = ["Withdrawal", "Refund", "ReturnShipment", "DefectClaim", "LegalDelivery", "Freeze", "Frozen", "Outage"];
        Assert.DoesNotContain(
            typeof(CompanySettings).GetProperties(),
            p => words.Any(w => p.Name.Contains(w, StringComparison.OrdinalIgnoreCase)));
    }

    // ---- KK6: the refund IBAN's deletion day ----

    [Fact]
    public void DeletionDay_DeletesTheIban_WhenItsOnlyWithdrawalsGoodsDidNotArrive_KK6()
    {
        RefundIbanUse use = new(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UnixEpoch, []);

        Assert.True(RefundIbanDeletionRule.ShouldDelete(use, new HashSet<Guid>()));
    }

    [Fact]
    public void DeletionDay_KeepsTheIban_WhileAnotherRefundStillUsesIt_AndDeletesItOnceThatRefundIsSettled_KK6()
    {
        Guid pending = Guid.NewGuid();
        Guid settled = Guid.NewGuid();
        RefundIbanUse use = new(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UnixEpoch, [settled, pending]);

        Assert.False(RefundIbanDeletionRule.ShouldDelete(use, new HashSet<Guid> { settled }));
        Assert.True(RefundIbanDeletionRule.ShouldDelete(use, new HashSet<Guid> { settled, pending }));
    }

    [GeneratedRegex(@"^\| (4\.2\.\d+) \|", RegexOptions.CultureInvariant)]
    private static partial Regex MomentRow();

    // The switch in any shape: a mode, a window, a flag, a configuration section, a planned maintenance, and the Turkish names (02 3.34.7; 10 KD-60).
    [GeneratedRegex(@"(?i)maintenance[\s_]*(mode|window|page|banner|\x22\s*:)|in[\s_]*maintenance|planned[\s_]*maintenance|bakım[\s_]*(modu|penceresi)", RegexOptions.CultureInvariant)]
    private static partial Regex MaintenanceSwitch();
}
