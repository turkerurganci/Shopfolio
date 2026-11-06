using System.Globalization;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the attempt limit table of the code equal to the rows of the product constants table in <c>02</c> (T215 criterion 8, KK8; 09 21.4 item 1, 3.5
/// item 1; D-103): P-31 to P-36 and P-49 are read as "threshold / window" per axis and compared with <see cref="AttemptLimits"/>, and the window of
/// every limit is the one the clean-up of the attempt records deletes by. L-7 and L-8 (P-37, P-46) are counted from the orders in T377; their constants
/// are compared with their rows here and the counter refuses them. The expected values are read from the document, never typed in the test.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class AttemptLimitsConsistencyTests
{
    private static readonly (AttemptLimit Limit, string Row)[] Counted =
    [
        (AttemptLimit.L1, "P-31"),
        (AttemptLimit.L2, "P-32"),
        (AttemptLimit.L3, "P-33"),
        (AttemptLimit.L4, "P-34"),
        (AttemptLimit.L5, "P-35"),
        (AttemptLimit.L6, "P-36"),
        (AttemptLimit.L9, "P-49"),
    ];

    private static string Row(string id)
    {
        string[] candidates = Directory.GetFiles(Path.Combine(RepositoryRoot.Find(), "Docs"), "02_*.md");
        string document = File.ReadAllText(Assert.Single(candidates)).ReplaceLineEndings("\n");
        return document.Split('\n').Single(line => line.StartsWith($"| {id} |", StringComparison.Ordinal));
    }

    private static AttemptAxis AxisOf(string label) => label switch
    {
        "E-posta" => AttemptAxis.Email,
        "IP" => AttemptAxis.Ip,
        "Sepet" => AttemptAxis.Cart,
        _ => throw new InvalidOperationException("Unknown axis label in the 02 row: " + label),
    };

    private static TimeSpan WindowOf(string amount, string unit)
    {
        int number = int.Parse(amount, CultureInfo.InvariantCulture);
        return unit == "saat" ? TimeSpan.FromHours(number) : TimeSpan.FromMinutes(number);
    }

    // The "E-posta başına **5 / 15 dakika**" and "IP başına **20 / 15 dakika**" parts of a row.
    private static List<(AttemptAxis Axis, int Threshold, TimeSpan Window)> Parts(string row)
    {
        List<(AttemptAxis, int, TimeSpan)> parts = [];
        foreach (Match match in AxisPart().Matches(row))
        {
            parts.Add((
                AxisOf(match.Groups["axis"].Value),
                int.Parse(match.Groups["n"].Value, CultureInfo.InvariantCulture),
                WindowOf(match.Groups["w"].Value, match.Groups["unit"].Value)));
        }

        return parts;
    }

    [Fact]
    public void EveryRowOfTheDocument_IsInTheCodesTable_WithTheSameThresholdAndWindow_AndNothingElseIsInIt()
    {
        List<(AttemptLimit Limit, AttemptAxis Axis)> documented = [];
        foreach ((AttemptLimit limit, string id) in Counted)
        {
            List<(AttemptAxis Axis, int Threshold, TimeSpan Window)> parts = Parts(Row(id));
            Assert.NotEmpty(parts);
            foreach ((AttemptAxis axis, int threshold, TimeSpan window) in parts)
            {
                AttemptRule rule = AttemptLimits.Of(limit, axis);
                Assert.True(threshold == rule.Threshold, $"{id} {axis} threshold: document {threshold}, code {rule.Threshold}");
                Assert.True(window == rule.Window, $"{id} {axis} window: document {window}, code {rule.Window}");
                documented.Add((limit, axis));
            }
        }

        // L-1's recognized browser counter carries the e-mail axis' number (02 8.2.6) and is not a separate row.
        Assert.Equal(
            AttemptLimits.Counted.Where(c => c.Axis != AttemptAxis.RecognizedBrowser).Order().ToList(),
            documented.Order().ToList());
        Assert.Equal(AttemptLimits.Of(AttemptLimit.L1, AttemptAxis.Email), AttemptLimits.Of(AttemptLimit.L1, AttemptAxis.RecognizedBrowser));
    }

    [Fact]
    public void TheWindowOfEveryLimit_IsTheWindowTheDisposalDeletesBy()
    {
        foreach ((AttemptLimit limit, AttemptAxis axis) in AttemptLimits.Counted)
        {
            Assert.True(AttemptRecordDisposalPart.WindowMinutes.ContainsKey(limit), $"{limit} has no disposal window");
            Assert.Equal(TimeSpan.FromMinutes(AttemptRecordDisposalPart.WindowMinutes[limit]), AttemptLimits.Of(limit, axis).Window);
        }

        Assert.Equal(
            AttemptLimits.Counted.Select(c => c.Limit).Distinct().Order(),
            AttemptRecordDisposalPart.WindowMinutes.Keys.Order());
    }

    [Fact]
    public void L7AndL8Constants_AreTheNumbersOfTheirRows()
    {
        string p37 = Row("P-37");
        Match l7 = Cancellations().Match(p37);
        Assert.True(l7.Success, "The P-37 row has changed shape; update this check with it.");
        Assert.Equal(ProductConstants.UnpaidOrderCancellationsMax, int.Parse(l7.Groups["n"].Value, CultureInfo.InvariantCulture));
        Assert.Equal(WindowOf(l7.Groups["w"].Value, l7.Groups["unit"].Value), TimeSpan.FromHours(ProductConstants.UnpaidOrderCancellationWindowHours));

        Match l8 = OpenOrders().Match(Row("P-46"));
        Assert.True(l8.Success, "The P-46 row has changed shape; update this check with it.");
        Assert.Equal(ProductConstants.OpenUnpaidOrdersMax, int.Parse(l8.Groups["n"].Value, CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData(AttemptLimit.L7)]
    [InlineData(AttemptLimit.L8)]
    public void L7AndL8_AreNotCountedByTheAttemptRecords_TheTableRefusesThem(AttemptLimit limit)
    {
        // The e-mail key of L-7 and L-8 is the signed-in account's, counted from the orders (T377); the attempt record has no row for them.
        Assert.Throws<ArgumentException>(() => AttemptLimits.Of(limit, AttemptAxis.Email));
        Assert.Throws<ArgumentException>(() => AttemptLimits.Of(limit, AttemptAxis.Ip));
        Assert.DoesNotContain(AttemptLimits.Counted, c => c.Limit == limit);
    }

    [Fact]
    public void ALimitOnAnAxisItDoesNotCount_IsAnArgumentError()
    {
        Assert.Throws<ArgumentException>(() => AttemptLimits.Of(AttemptLimit.L4, AttemptAxis.Email));
        Assert.Throws<ArgumentException>(() => AttemptLimits.Of(AttemptLimit.L6, AttemptAxis.Ip));
        Assert.Throws<ArgumentException>(() => AttemptLimits.Of(AttemptLimit.L1, AttemptAxis.Cart));
    }

    [GeneratedRegex(@"(?<axis>E-posta|IP|Sepet) başına \*\*(?<n>[0-9]+) / (?<w>[0-9]+) (?<unit>dakika|saat)\*\*", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex AxisPart();

    [GeneratedRegex(@"\*\*(?<n>[0-9]+) / (?<w>[0-9]+) (?<unit>saat)\*\*", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Cancellations();

    [GeneratedRegex(@"\*\*(?<n>[0-9]+)\*\* açık sipariş", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex OpenOrders();
}
