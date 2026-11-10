using System.Text.RegularExpressions;

using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// T412 KK9 (D-847, D-896): the "payment completed" set has one definition (<see cref="PaymentApproval"/>); the test fixes the set, and a source scan finds no
/// second copy of it outside that one file - neither the three statuses written side by side (array, list or "is ... or ..." pattern) nor the set written as its
/// complement, "not Pending and not Failed" (<c>is not (Pending or Failed)</c>, <c>is Pending or Failed</c>, <c>!= Pending &amp;&amp; != Failed</c>,
/// <c>== Pending || == Failed</c>, a <c>[Pending, Failed]</c> list), in any order, spacing or line breaks.
/// The one exception is the allowed-pairs table of the two axes (03 1.4.1, <c>TransitionTable.Combinations</c>): its rows list the payment statuses a shipment state
/// may stand with (Placed also Pending, Cancelled also Failed), a different rule whose sets only look like this one; the exception covers those rows of that file
/// and nothing else. A forbidden-pair check elsewhere reads that table (<c>TransitionTable.IsPairAllowed</c>), not a copy of it.
/// </summary>
public sealed partial class PaymentApprovalTests
{
    private const string Pending = @"PaymentStatus\.Pending\b";
    private const string Failed = @"PaymentStatus\.Failed\b";
    private const string NotPending = @"(?:!=\s*" + Pending + "|" + Pending + @"\s*!=)";
    private const string NotFailed = @"(?:!=\s*" + Failed + "|" + Failed + @"\s*!=)";
    private const string IsPending = @"(?:==\s*" + Pending + "|" + Pending + @"\s*==)";
    private const string IsFailed = @"(?:==\s*" + Failed + "|" + Failed + @"\s*==)";

    // Within one expression: no statement end or block brace between the two halves, and no operator of the other kind.
    private const string AndGap = @"(?:(?!\|\||[;{}]).)*?&&(?:(?!\|\||[;{}]).)*?";
    private const string OrGap = @"(?:(?!&&|[;{}]).)*?\|\|(?:(?!&&|[;{}]).)*?";

    private const string OrPattern = Pending + @"\s+or\s+" + Failed + "|" + Failed + @"\s+or\s+" + Pending;
    private const string AndNotPattern = @"\bnot\s+" + Pending + @"\s+and\s+not\s+" + Failed + "|" + @"\bnot\s+" + Failed + @"\s+and\s+not\s+" + Pending;
    private const string NotEqualChain = NotPending + AndGap + NotFailed + "|" + NotFailed + AndGap + NotPending;
    private const string EqualChain = IsPending + OrGap + IsFailed + "|" + IsFailed + OrGap + IsPending;
    private const string PairList = @"[\[{]\s*(?:" + Pending + @"\s*,\s*" + Failed + "|" + Failed + @"\s*,\s*" + Pending + @")\s*[\]}]";

    [Fact]
    public void TheSet_IsPaidAndTheRefundedStatusesAfterIt_KK9()
    {
        PaymentStatus[] expected = [PaymentStatus.Paid, PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded];

        Assert.Equal(expected, PaymentApproval.Statuses);
        Assert.All(expected, s => Assert.True(PaymentApproval.IsApproved(s), s.ToString()));
        Assert.All(Enum.GetValues<PaymentStatus>().Except(expected), s => Assert.False(PaymentApproval.IsApproved(s), s.ToString()));
    }

    [Fact]
    public void TheQueryArray_IsANewArrayEachCall_SoNoCallerChangesTheSet_KK9()
    {
        PaymentStatus[] first = PaymentApproval.Statuses;
        first[0] = PaymentStatus.Failed;

        Assert.Equal(PaymentStatus.Paid, PaymentApproval.Statuses[0]);
    }

    [Fact]
    public void NoSourceFile_WritesTheSetAgain_OutsideItsOneHome_KK9()
    {
        string root = RepositoryRoot.Find();
        string home = Path.Combine(root, "src", "Shopfolio.Web", "Modules", "Ordering", "Contracts", "PaymentApproval.cs");
        string table = Path.Combine(root, "src", "Shopfolio.Web", "Modules", "Ordering", "TransitionTable.cs");
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                || string.Equals(file, home, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            bool isTable = string.Equals(file, table, StringComparison.OrdinalIgnoreCase);
            string text = File.ReadAllText(file);
            string[] lines = text.Split('\n');
            HashSet<int> copies = [];
            for (int i = 0; i < lines.Length; i++)
            {
                if (TheThreeStatuses().IsMatch(lines[i]))
                {
                    copies.Add(i + 1);
                }
            }

            foreach (Match match in TheComplement().Matches(text))
            {
                copies.Add(1 + text.AsSpan(0, match.Index).Count('\n'));
            }

            findings.AddRange(copies
                .Where(line => !(isTable && AllowedPairsRow().IsMatch(lines[line - 1].TrimEnd('\r'))))
                .Order()
                .Select(line => $"{Path.GetRelativePath(root, file)}:{line}"));
        }

        Assert.Empty(findings);
    }

    [Theory]
    [InlineData("private static readonly PaymentStatus[] Approved = [PaymentStatus.Paid, PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded];")]
    [InlineData("bool x = status is PaymentStatus.Paid or PaymentStatus.PartiallyRefunded or PaymentStatus.Refunded;")]
    [InlineData("var s = new[] { PaymentStatus.Refunded, PaymentStatus.Paid, PaymentStatus.PartiallyRefunded };")]
    [InlineData("bool y = s == PaymentStatus.Paid || s == PaymentStatus.PartiallyRefunded || s == PaymentStatus.Refunded;")]
    public void TheScan_CatchesACopyInAnyOrderAndForm_KK9(string line)
    {
        Assert.Matches(TheThreeStatuses(), line);
    }

    [Theory]
    [InlineData("return payment is not (PaymentStatus.Pending or PaymentStatus.Failed);")]
    [InlineData("bool open = payment is not (PaymentStatus.Failed or PaymentStatus.Pending);")]
    [InlineData("bool open = payment is not(PaymentStatus.Pending   or\n        PaymentStatus.Failed);")]
    [InlineData("if (order.PaymentStatus is PaymentStatus.Pending or PaymentStatus.Failed)")]
    [InlineData("bool open = payment switch { PaymentStatus.Pending or PaymentStatus.Failed => false, _ => true };")]
    public void TheScan_CatchesTheComplementAsAnOrPattern_KK9(string source)
    {
        Assert.Matches(TheComplement(), source);
    }

    [Theory]
    [InlineData("bool open = payment is not PaymentStatus.Pending and not PaymentStatus.Failed;")]
    [InlineData("bool open = payment is not PaymentStatus.Failed and\n    not PaymentStatus.Pending;")]
    public void TheScan_CatchesTheComplementAsAnAndNotPattern_KK9(string source)
    {
        Assert.Matches(TheComplement(), source);
    }

    [Theory]
    [InlineData("o => o.PaymentStatus != PaymentStatus.Pending && o.PaymentStatus != PaymentStatus.Failed")]
    [InlineData("o => o.PaymentStatus != PaymentStatus.Failed && o.PaymentStatus != PaymentStatus.Pending")]
    [InlineData("o => o.PaymentStatus!=PaymentStatus.Pending&&o.PaymentStatus!=PaymentStatus.Failed")]
    [InlineData("orders.Any(o => o.Id == i.OrderId\n    && o.OrderStatus != OrderStatus.Cancelled\n    && o.PaymentStatus != PaymentStatus.Pending\n    && o.PaymentStatus != PaymentStatus.Failed)")]
    [InlineData("bool open = PaymentStatus.Pending != payment && PaymentStatus.Failed != payment;")]
    public void TheScan_CatchesTheComplementAsANotEqualChain_KK9(string source)
    {
        Assert.Matches(TheComplement(), source);
    }

    [Theory]
    [InlineData("bool unpaid = s == PaymentStatus.Pending || s == PaymentStatus.Failed;")]
    [InlineData("bool unpaid = s==PaymentStatus.Failed||\n    s==PaymentStatus.Pending;")]
    [InlineData("bool unpaid = PaymentStatus.Pending == s || PaymentStatus.Failed == s;")]
    public void TheScan_CatchesTheComplementAsAnEqualChain_KK9(string source)
    {
        Assert.Matches(TheComplement(), source);
    }

    [Theory]
    [InlineData("bool open = !new[] { PaymentStatus.Pending, PaymentStatus.Failed }.Contains(o.PaymentStatus);")]
    [InlineData("private static readonly PaymentStatus[] Unpaid = [PaymentStatus.Failed,PaymentStatus.Pending];")]
    public void TheScan_CatchesTheComplementAsAPairList_KK9(string source)
    {
        Assert.Matches(TheComplement(), source);
    }

    [Theory]
    [InlineData("o => o.PaymentStatus == PaymentStatus.Pending && o.OrderStatus == OrderStatus.Placed")]
    [InlineData("new(TransitionCode.O2, PaymentStatus.Pending, PaymentStatus.Failed),")]
    [InlineData("bool terminal = status is PaymentStatus.Failed or PaymentStatus.Refunded;")]
    [InlineData("bool a = s == PaymentStatus.Pending; bool b = c || s == PaymentStatus.Failed;")]
    [InlineData("if (s != PaymentStatus.Pending) x(); if (y && s != PaymentStatus.Failed) z();")]
    [InlineData("var a = new { b = s != PaymentStatus.Pending }; var c = new { d = y && s != PaymentStatus.Failed };")]
    [InlineData("bool any = s != PaymentStatus.Pending || t != PaymentStatus.Failed;")]
    public void TheScan_LeavesASingleStatusAndOtherRules_KK9(string source)
    {
        Assert.DoesNotMatch(TheComplement(), source);
    }

    [Fact]
    public void TheTablesException_CoversOnlyItsAllowedPairsRows_KK9()
    {
        Assert.Matches(AllowedPairsRow(), "        [OrderStatus.Preparing] = new HashSet<PaymentStatus> { PaymentStatus.Paid, PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded },");
        Assert.DoesNotMatch(AllowedPairsRow(), "    private static readonly PaymentStatus[] Approved = [PaymentStatus.Paid, PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded];");
        Assert.DoesNotMatch(AllowedPairsRow(), "    public static bool IsOpen(PaymentStatus payment) => payment is not (PaymentStatus.Pending or PaymentStatus.Failed);");
    }

    // The three statuses on one line, in any order.
    [GeneratedRegex(@"^(?=.*\bPaymentStatus\.Paid\b)(?=.*\bPaymentStatus\.PartiallyRefunded\b)(?=.*\bPaymentStatus\.Refunded\b)", RegexOptions.CultureInvariant)]
    private static partial Regex TheThreeStatuses();

    // The set as its complement - not Pending and not Failed - in any of its forms; a match may span lines.
    [GeneratedRegex(OrPattern + "|" + AndNotPattern + "|" + NotEqualChain + "|" + EqualChain + "|" + PairList, RegexOptions.CultureInvariant | RegexOptions.Singleline)]
    private static partial Regex TheComplement();

    // A row of the allowed-pairs table: "[OrderStatus.X] = new HashSet<PaymentStatus> { ... },".
    [GeneratedRegex(@"^\s*\[OrderStatus\.\w+\]\s*=\s*new HashSet<PaymentStatus>\s*\{[^}]*\},?\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex AllowedPairsRow();
}
