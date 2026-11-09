using System.Text.RegularExpressions;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the limit inventory of the code equal to the nine rows of the table of <c>02 8.2</c> (T376 criterion 1, 9, KK1, KK9; 09 21.4 item 1): the same nine ids, the
/// axes each row names, how each is counted, and the parameter row that carries each one's numbers. The expected values are read from the document, never typed in
/// the test; the numbers themselves are compared in <see cref="AttemptLimitsConsistencyTests"/>.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class AttemptLimitInventoryTests
{
    private static string Document()
    {
        string[] candidates = Directory.GetFiles(Path.Combine(RepositoryRoot.Find(), "Docs"), "02_*.md");
        return File.ReadAllText(Assert.Single(candidates)).ReplaceLineEndings("\n");
    }

    // The rows of the 8.2 table: "| L-1 | name | axis | exceeded | source |".
    private static List<(int Number, string Name, string Axis)> TableRows()
    {
        string document = Document();
        int start = document.IndexOf("### 8.2 Deneme limitleri", StringComparison.Ordinal);
        Assert.True(start >= 0, "The 8.2 heading has moved; update this check with it.");
        int end = document.IndexOf("**8.2.1", start, StringComparison.Ordinal);
        List<(int, string, string)> rows = [];
        foreach (Match match in TableRow().Matches(document[start..end]))
        {
            rows.Add((int.Parse(match.Groups["n"].Value, System.Globalization.CultureInfo.InvariantCulture), match.Groups["name"].Value, match.Groups["axis"].Value));
        }

        return rows;
    }

    private static string ParameterRow(string id)
    {
        return Document().Split('\n').Single(line => line.StartsWith($"| {id} |", StringComparison.Ordinal));
    }

    [Fact]
    public void TheTableHasNineRows_AndTheInventoryHasTheSameNineLimits_InTheSameOrder_KK1_KK9()
    {
        List<(int Number, string Name, string Axis)> rows = TableRows();

        Assert.Equal(Enumerable.Range(1, 9), rows.Select(r => r.Number));
        Assert.Equal(rows.Select(r => $"L{r.Number}"), AttemptLimitInventory.All.Select(e => e.Limit.ToString()));
        Assert.Equal(Enum.GetValues<AttemptLimit>().Order(), AttemptLimitInventory.All.Select(e => e.Limit).Order());
        Assert.Equal(AttemptLimitInventory.All.Count, AttemptLimitInventory.All.Select(e => e.Name).Distinct().Count());
    }

    [Fact]
    public void EveryLimitsAxes_AreTheOnesItsRowNames_KK1_KK9()
    {
        foreach ((int number, string _, string axis) in TableRows())
        {
            AttemptLimitEntry entry = AttemptLimitInventory.Of(Enum.Parse<AttemptLimit>($"L{number}"));
            List<AttemptAxis> expected = axis.Contains("aynı sepet", StringComparison.OrdinalIgnoreCase)
                ? [AttemptAxis.Cart]
                : axis.Contains("aynı e-posta", StringComparison.OrdinalIgnoreCase) ? [AttemptAxis.Email, AttemptAxis.Ip] : [AttemptAxis.Ip];
            if (axis.Contains("tanınan tarayıcı", StringComparison.OrdinalIgnoreCase))
            {
                expected.Add(AttemptAxis.RecognizedBrowser);
            }

            Assert.True(
                expected.Order().SequenceEqual(entry.Axes.Order()),
                $"L-{number} axes: document {string.Join(",", expected)}, code {string.Join(",", entry.Axes)}");
        }
    }

    [Fact]
    public void EveryLimitsCountingKind_IsTheOneItsRowSays_AndMatchesWhatTheCountersTableAndTheOrderRulesHold_KK1_KK9()
    {
        foreach ((int number, string name, string _) in TableRows())
        {
            AttemptLimit limit = Enum.Parse<AttemptLimit>($"L{number}");
            AttemptCounting expected = name.Contains("ödenmemiş sipariş", StringComparison.OrdinalIgnoreCase)
                ? AttemptCounting.FromOrders
                : name.StartsWith("Başarısız", StringComparison.Ordinal) || name.StartsWith("Geçersiz", StringComparison.Ordinal)
                    ? AttemptCounting.OnFailure
                    : AttemptCounting.EveryRequest;
            Assert.Equal(expected, AttemptLimitInventory.Of(limit).Counting);
        }

        // What the attempt record counts is exactly the limits not counted from the orders; those have a rule of their own.
        Assert.Equal(
            AttemptLimitInventory.All.Where(e => e.Counting != AttemptCounting.FromOrders).Select(e => e.Limit).Order(),
            AttemptLimits.Counted.Select(c => c.Limit).Distinct().Order());
        foreach (AttemptLimitEntry entry in AttemptLimitInventory.All.Where(e => e.Counting == AttemptCounting.FromOrders))
        {
            Assert.True(AttemptLimits.OrderCountedOf(entry.Limit).Max > 0);
        }

        foreach (AttemptLimitEntry entry in AttemptLimitInventory.All.Where(e => e.Counting != AttemptCounting.FromOrders))
        {
            Assert.Throws<ArgumentException>(() => AttemptLimits.OrderCountedOf(entry.Limit));
            Assert.All(entry.Axes, axis => AttemptLimits.Of(entry.Limit, axis));
        }
    }

    [Fact]
    public void EveryLimitsParameterRows_ExistAndNameTheLimit_AndNoOtherRowDoes_KK2_KK9()
    {
        foreach (AttemptLimitEntry entry in AttemptLimitInventory.All)
        {
            string label = $"L-{entry.Limit.ToString()[1..]}";
            Assert.NotEmpty(entry.ParameterIds);
            foreach (string id in entry.ParameterIds)
            {
                Assert.NotNull(ParameterInventory.Of(id));
                Assert.Matches($@"^\| {id} \| {label} ", ParameterRow(id));
            }
        }

        string[] documented = [.. Document().Split('\n').Where(l => AttemptRow().IsMatch(l)).Select(l => l.Split('|')[1].Trim())];
        Assert.Equal(
            documented.Order(StringComparer.Ordinal),
            AttemptLimitInventory.All.SelectMany(e => e.ParameterIds).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void L8HasNoWindow_AndL7TheWindowOfItsConstant_KK8()
    {
        Assert.Null(AttemptLimits.OrderCountedOf(AttemptLimit.L8).Window);
        Assert.Equal(ProductConstants.OpenUnpaidOrdersMax, AttemptLimits.OrderCountedOf(AttemptLimit.L8).Max);
        Assert.Equal(TimeSpan.FromHours(ProductConstants.UnpaidOrderCancellationWindowHours), AttemptLimits.OrderCountedOf(AttemptLimit.L7).Window);
        Assert.Equal(ProductConstants.UnpaidOrderCancellationsMax, AttemptLimits.OrderCountedOf(AttemptLimit.L7).Max);
    }

    [GeneratedRegex(@"^\| L-(?<n>[0-9]) \| (?<name>[^|]*) \| (?<axis>[^|]*) \|", RegexOptions.Multiline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex TableRow();

    // A row of the parameter table whose second cell starts with a limit id: "| P-31 | L-1 Başarısız ...".
    [GeneratedRegex(@"^\| P-[0-9]+ \| L-[0-9] ", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex AttemptRow();
}
