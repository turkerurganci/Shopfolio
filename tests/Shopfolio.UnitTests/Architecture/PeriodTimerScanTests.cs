using System.Text.RegularExpressions;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The delivery, withdrawal, refund and defect claim periods have no timer (T325 criteria 1, 6, 7, 9, KK1, KK6, KK9; 03 4.1.10-4.1.22): no job type
/// names one of them, and no scheduled source, outage effect, nightly step or job handler of the application reads their rules, their counted
/// periods or the download counter (Z-19 is a count, not a period). Every such type is found by reflection, so a new one is scanned without a list
/// to keep; its source file is found by its class declaration. Z-47 is neither kept nor shown: its counted period is named only where the periods
/// are listed and counted. The card refund job (Z-17's card line starts the refund at the cancellation) is a refund, not a period, and is not a timer.
/// </summary>
public sealed partial class PeriodTimerScanTests
{
    private static readonly Type[] TimerContracts =
        [typeof(IDueRecordSource), typeof(IOutageEffect), typeof(INightlyStep), typeof(IJobHandler), typeof(ITransactionalJobHandler)];

    private static readonly string[] PeriodWords =
        ["Shipping", "Delivery", "Fulfilment", "Withdrawal", "ReturnShipment", "Defect", "Download", "Interest", "Impossib", "Deadline", "Period"];

    [Fact]
    public void NoJobType_NamesADeliveryWithdrawalRefundOrDefectPeriod_KK6_KK9()
    {
        Assert.DoesNotContain(
            Enum.GetNames<JobType>(),
            name => PeriodWords.Any(word => name.Contains(word, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void NoScheduledSource_OutageEffect_NightlyStep_OrJobHandler_ReadsThePeriods_KK1_KK6_KK7_KK9()
    {
        List<Type> timers = typeof(ProductConstants).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && TimerContracts.Any(c => c.IsAssignableFrom(t)))
            .ToList();
        Assert.Contains(typeof(Shopfolio.Modules.Ordering.TransferPaymentExpiry), timers);
        Assert.Contains(typeof(Shopfolio.Infrastructure.Disposal.DisposalStep), timers);

        string root = RepositoryRoot.Find();
        Dictionary<string, string> texts = SourceFiles(root).ToDictionary(f => f, File.ReadAllText, StringComparer.Ordinal);

        List<string> findings = [];
        foreach (Type timer in timers.SelectMany(Hierarchy).Distinct())
        {
            Regex declaration = new($@"\bclass\s+{Regex.Escape(timer.Name.Split('`')[0])}\b", RegexOptions.CultureInvariant);
            string[] files = texts.Where(t => declaration.IsMatch(t.Value)).Select(t => t.Key).ToArray();
            Assert.True(files.Length > 0, $"The source of {timer.FullName} was not found.");
            foreach (string file in files)
            {
                if (PeriodUse().IsMatch(texts[file]))
                {
                    findings.Add($"{Path.GetRelativePath(root, file)}: {timer.Name}");
                }
            }
        }

        Assert.Empty(findings);
    }

    [Fact]
    public void ImpossibilityNoticePeriod_IsNamedOnlyWhereThePeriodsAreListedAndCounted_Z47()
    {
        string root = RepositoryRoot.Find();
        string[] allowed =
        [
            Path.Combine(root, "src", "Shopfolio.Web", "Infrastructure", "CountedPeriod.cs"),
            Path.Combine(root, "src", "Shopfolio.Web", "Infrastructure", "PeriodCounting.cs"),
        ];

        string[] users = SourceFiles(root)
            .Where(f => File.ReadAllText(f).Contains(nameof(CountedPeriod.ImpossibilityNoticePeriod), StringComparison.Ordinal))
            .ToArray();

        Assert.Equal(allowed.Order(StringComparer.Ordinal), users.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void PeriodPattern_SeesTheRulesThePeriodsAndTheDownloadCounter_AndNotAPlainWord()
    {
        Assert.Matches(PeriodUse(), "if (DeliveryPeriodRules.IsLateDeliveryTerminationOpen(order.OrderStatus, ...))");
        Assert.Matches(PeriodUse(), "bool open = AfterSalesPeriodRules.IsNotReturnedClosureOpen(true, 1, day, now);");
        Assert.Matches(PeriodUse(), "PeriodStanding? standing = await periods.ShippingStandingAsync(id, now, ct);");
        Assert.Matches(PeriodUse(), "OrderPeriods periods = scope.ServiceProvider.GetRequiredService<OrderPeriods>();");
        Assert.Matches(PeriodUse(), "PeriodCounting.EndOf(CountedPeriod.RefundPeriod, start, 14, calendar)");
        Assert.Matches(PeriodUse(), "PeriodCounting.EndOf(CountedPeriod.ImpossibilityNoticePeriod, start, 3, calendar)");
        Assert.Matches(PeriodUse(), "db.OrderItems.Where(i => i.DownloadCount >= i.DownloadAllowance)");
        Assert.Matches(PeriodUse(), "o.ShippingPeriod != null && o.CreatedAt < limit");
        Assert.DoesNotMatch(PeriodUse(), "PeriodCounting.EndOf(CountedPeriod.BankTransferPaymentWindow, start, days, calendar)");
        Assert.DoesNotMatch(PeriodUse(), "// the shipping period and the refund period are not timers");
    }

    private static IEnumerable<string> SourceFiles(string root)
    {
        return Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal));
    }

    private static IEnumerable<Type> Hierarchy(Type type)
    {
        for (Type? current = type; current is not null && current.Assembly == type.Assembly; current = current.BaseType)
        {
            yield return current;
        }
    }

    [GeneratedRegex(
        @"\b(?:DeliveryPeriodRules|AfterSalesPeriodRules|OrderPeriods|PeriodStanding)\b"
            + @"|\bCountedPeriod\.(?:ShippingPeriod|ServiceFulfilmentPeriod|LegalDeliveryLimit|WithdrawalWindow|ReturnShipmentPeriod|RefundPeriod|DefectClaimPeriod|ImpossibilityNoticePeriod)\b"
            + @"|\.(?:ShippingPeriod|FulfilmentPeriod|DeliveryDate|DeliveryMarkedAt|DeclarationDate|ArrivalDate|DownloadCount|DownloadAllowance)\b",
        RegexOptions.CultureInvariant)]
    private static partial Regex PeriodUse();
}
