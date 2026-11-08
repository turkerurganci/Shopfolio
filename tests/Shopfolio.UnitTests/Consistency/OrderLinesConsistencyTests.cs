using System.Globalization;
using System.Text.RegularExpressions;

using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the lines and the manual step budget of the code equal to 03 1.5 and 8.5 (T318 criteria 9-10, KK9-KK10; 09 21.4 item 1): the seven lines'
/// numbers, their ordinary sequences - the codes and who takes each step - and the mandatory manual step counts on the card and on the transfer are
/// read from the document and compared with <see cref="OrderLines.Rows"/> and <see cref="OrderLines.StepsOf"/>; 8.5.5's mixed order paid by transfer
/// and 8.5's ceiling likewise. The expected numbers are read from the document, never typed in the test.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class OrderLinesConsistencyTests
{
    private static readonly Dictionary<string, int> NumberWords = new(StringComparer.Ordinal)
    {
        ["bir"] = 1,
        ["iki"] = 2,
        ["üç"] = 3,
        ["dört"] = 4,
        ["beş"] = 5,
    };

    private static string[] Lines() =>
        File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "03_USER_FLOWS.md")).ReplaceLineEndings("\n").Split('\n');

    private static string[] Cells(string prefix)
    {
        string line = Assert.Single(Lines(), l => l.StartsWith($"| {prefix} |", StringComparison.Ordinal));
        return [.. line.Trim('|').Split('|').Select(c => c.Trim())];
    }

    // "2", "0 · havalede 1", "**1** — havalede 2": the card count first, the transfer count after "havalede" (the card count when absent).
    private static (int Card, int Transfer) Counts(string cell)
    {
        Match match = CountCell().Match(cell);
        Assert.True(match.Success, $"The count cell '{cell}' is not read.");
        int card = int.Parse(match.Groups["card"].Value, CultureInfo.InvariantCulture);
        int transfer = match.Groups["transfer"].Success ? int.Parse(match.Groups["transfer"].Value, CultureInfo.InvariantCulture) : card;
        return (card, transfer);
    }

    private static int Count(IEnumerable<ProductType> types, PaymentMethod method) => OrderLines.StepsOf([.. types], method).Count;

    private static OrderLineRow Row(string number) => Assert.Single(OrderLines.Rows, r => r.Number == number);

    // "*(sistem: Ö1, S1)*", "*(yönetici: son "tamamlandı", S2)*", "*(Ö1)*": the codes of a step and its actor (the administrator when named).
    private static List<string> DocumentedSequence(string cell)
    {
        List<string> steps = [];
        foreach (Match step in SequenceStep().Matches(cell))
        {
            string body = step.Groups["body"].Value;
            string actor = body.StartsWith("yönetici:", StringComparison.Ordinal) ? nameof(ActorKind.Admin) : nameof(ActorKind.System);
            string codes = string.Join(",", Code().Matches(body).Select(m => m.Value.Replace('Ö', 'O')));
            steps.Add($"{codes}/{actor}");
        }

        return steps;
    }

    private static List<string> CodeSequence(OrderLineRow row) =>
        [.. row.Sequence.Select(s => $"{string.Join(",", s.Codes)}/{s.Actor}")];

    [Fact]
    public void TheSevenLines_AreTheDocumentsRows()
    {
        List<string> documented = [.. Lines().Where(l => LineRow().IsMatch(l)).Select(l => l.Split('|')[1].Trim())];

        Assert.Empty(SetComparison.Compare("03 1.5", documented, OrderLines.Rows.Select(r => r.Number)));
        Assert.Equal(7, documented.Count);
    }

    [Theory]
    [InlineData("1.5.1")]
    [InlineData("1.5.3")]
    [InlineData("1.5.4")]
    [InlineData("1.5.5")]
    public void TheOrdinarySequence_EqualsTheDocument_KK9(string number)
    {
        Assert.Equal(DocumentedSequence(Cells(number)[2]), CodeSequence(Row(number)));
    }

    [Fact]
    public void TheTransferAndTheMixedPhysicalLines_FollowTheCardLine_AndTheTransfersO1IsTheAdministrators()
    {
        Assert.StartsWith("Kart hattı", Cells("1.5.2")[2], StringComparison.Ordinal);
        Assert.StartsWith("Fiziksel hat", Cells("1.5.6")[2], StringComparison.Ordinal);
        Assert.Equal(Row("1.5.1").Sequence.Select(s => s.Codes), Row("1.5.2").Sequence.Select(s => s.Codes));
        Assert.Equal(Row("1.5.1").Sequence.Select(s => s.Codes), Row("1.5.6").Sequence.Select(s => s.Codes));
        Assert.All(Row("1.5.2").Sequence, s => Assert.Equal(ActorKind.Admin, s.Actor));
    }

    [Theory]
    [InlineData("1.5.1", PaymentMethod.Card)]
    [InlineData("1.5.2", PaymentMethod.BankTransfer)]
    public void ThePhysicalLines_ManualSteps_EqualTheDocument_KK9(string number, PaymentMethod method)
    {
        OrderLineRow row = Row(number);
        (int card, _) = Counts(Cells(number)[3]);

        Assert.Equal(method, row.Method);
        Assert.Equal(card, Count(row.Types, method));
        Assert.Equal(card, row.Sequence.Count(s => s.Actor == ActorKind.Admin));
    }

    [Theory]
    [InlineData("1.5.3")]
    [InlineData("1.5.4")]
    [InlineData("1.5.5")]
    public void TheLinesWithoutAPhysicalItem_ManualSteps_EqualTheDocument_OnBothMethods_KK9(string number)
    {
        OrderLineRow row = Row(number);
        (int card, int transfer) = Counts(Cells(number)[3]);

        Assert.Null(row.Method);
        Assert.Equal(card, Count(row.Types, PaymentMethod.Card));
        Assert.Equal(transfer, Count(row.Types, PaymentMethod.BankTransfer));
        Assert.Equal(card, row.Sequence.Count(s => s.Actor == ActorKind.Admin));
    }

    [Theory]
    [InlineData("8.5.1", new[] { ProductType.Physical }, PaymentMethod.Card)]
    [InlineData("8.5.2", new[] { ProductType.Physical }, PaymentMethod.BankTransfer)]
    [InlineData("8.5.3", new[] { ProductType.Service }, null)]
    [InlineData("8.5.4", new[] { ProductType.Digital }, null)]
    public void TheBudgetRows_EqualStepsOf_KK9(string number, ProductType[] types, PaymentMethod? method)
    {
        (int card, int transfer) = Counts(Cells(number)[2]);

        if (method is { } only)
        {
            Assert.Equal(card, Count(types, only));
        }
        else
        {
            Assert.Equal(card, Count(types, PaymentMethod.Card));
            Assert.Equal(transfer, Count(types, PaymentMethod.BankTransfer));
        }
    }

    [Fact]
    public void AMixedOrder_AddsTheLines_AndCountsThePaidMarkOnce_PhysicalAndServiceByTransferIsFour_KK9()
    {
        Match match = MixedExample().Match(Cells("8.5.5")[2]);
        Assert.True(match.Success, "8.5.5's example is not read.");
        int documented = NumberWords[match.Groups["n"].Value];

        IReadOnlyList<ManualStep> steps = OrderLines.StepsOf([ProductType.Physical, ProductType.Service], PaymentMethod.BankTransfer);

        Assert.Equal(documented, steps.Count);
        Assert.Single(steps, s => s == ManualStep.TransferPaidMark);
        Assert.Equal(
            Count([ProductType.Physical], PaymentMethod.BankTransfer) + Count([ProductType.Service], PaymentMethod.BankTransfer) - 1,
            steps.Count);
    }

    [Fact]
    public void ASingleTypeLine_NeverExceedsTheCeiling_AndTheHeaviestIsThePhysicalOrderByTransfer_KK10()
    {
        string budget = Assert.Single(Lines(), l => l.StartsWith("Sipariş başına sistem içi zorunlu elle adımlar", StringComparison.Ordinal));
        Match match = Ceiling().Match(budget);
        Assert.True(match.Success, "8.5's ceiling is not read.");
        int ceiling = NumberWords[match.Groups["n"].Value];

        List<(ProductType Type, PaymentMethod Method, int Steps)> all =
            [.. from type in Enum.GetValues<ProductType>() from method in Enum.GetValues<PaymentMethod>() select (type, method, Count([type], method))];

        Assert.All(all, line => Assert.True(line.Steps <= ceiling, $"{line.Type}/{line.Method}: {line.Steps} steps."));
        (ProductType heaviestType, PaymentMethod heaviestMethod, int heaviest) = all.MaxBy(l => l.Steps);
        Assert.Equal(ceiling, heaviest);
        Assert.Equal((ProductType.Physical, PaymentMethod.BankTransfer), (heaviestType, heaviestMethod));
        Assert.Single(all, l => l.Steps == ceiling);
    }

    [Theory]
    [InlineData(new[] { ProductType.Physical }, PaymentMethod.Card, false, nameof(OrderLine.PhysicalCard))]
    [InlineData(new[] { ProductType.Physical }, PaymentMethod.BankTransfer, false, nameof(OrderLine.PhysicalTransfer))]
    [InlineData(new[] { ProductType.Digital, ProductType.Digital }, PaymentMethod.BankTransfer, false, nameof(OrderLine.DigitalOnly))]
    [InlineData(new[] { ProductType.Service }, PaymentMethod.Card, false, nameof(OrderLine.ServiceOnly))]
    [InlineData(new[] { ProductType.Service, ProductType.Digital }, PaymentMethod.Card, false, nameof(OrderLine.DigitalAndService))]
    [InlineData(new[] { ProductType.Physical, ProductType.Digital }, PaymentMethod.Card, false, nameof(OrderLine.MixedWithPhysical))]
    [InlineData(new[] { ProductType.Physical, ProductType.Digital }, PaymentMethod.BankTransfer, true, nameof(OrderLine.MixedPhysicalClosed))]
    public void LineOf_TheTypesAndTheMethod(ProductType[] types, PaymentMethod method, bool physicalClosed, string expected)
    {
        Assert.Equal(expected, OrderLines.LineOf(types, method, physicalClosed).ToString());
    }

    [Fact]
    public void LineOf_AnOrderWithoutItems_IsRefused()
    {
        Assert.Throws<ArgumentException>(() => OrderLines.LineOf([], PaymentMethod.Card, false));
    }

    [GeneratedRegex(@"^\|\s1\.5\.\d+\s\|")]
    private static partial Regex LineRow();

    [GeneratedRegex(@"^\**(?<card>\d+)\**(?:\s*[·—]\s*havalede\s+(?<transfer>\d+))?")]
    private static partial Regex CountCell();

    [GeneratedRegex(@"\*\((?<body>[^)]*)\)\*")]
    private static partial Regex SequenceStep();

    [GeneratedRegex(@"[ÖS]\d+")]
    private static partial Regex Code();

    [GeneratedRegex(@"fiziksel \+ hizmet siparişi (?<n>\p{L}+) adımdır")]
    private static partial Regex MixedExample();

    [GeneratedRegex(@"zorunlu elle adım (?<n>\p{L}+?)ü? geçmez")]
    private static partial Regex Ceiling();
}
