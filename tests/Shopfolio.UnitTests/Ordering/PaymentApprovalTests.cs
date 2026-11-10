using System.Text.RegularExpressions;

using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// T412 KK9 (D-847, D-896): the "payment completed" set has one definition (<see cref="PaymentApproval"/>); the test fixes the set, and a source scan finds no
/// second copy of it - neither the three statuses written side by side (array, list or "is ... or ..." pattern) nor a private field or method that names it -
/// outside that one file. The allowed-pairs table of the two axes (03 1.4.1, <c>TransitionTable</c>) is a different rule: its rows list the payment statuses a
/// state allows (Placed also Pending, Cancelled also Failed), so its sets are not this one.
/// </summary>
public sealed partial class PaymentApprovalTests
{
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

            string[] lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                bool copy = TheThreeStatuses().IsMatch(lines[i]) && !(string.Equals(file, table, StringComparison.OrdinalIgnoreCase) && AllowedPairsRow().IsMatch(lines[i]));
                if (copy)
                {
                    findings.Add($"{Path.GetRelativePath(root, file)}:{i + 1}");
                }
            }
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

    [Fact]
    public void TheTablesException_CoversOnlyItsAllowedPairsRows_KK9()
    {
        Assert.Matches(AllowedPairsRow(), "        [OrderStatus.Preparing] = new HashSet<PaymentStatus> { PaymentStatus.Paid, PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded },");
        Assert.DoesNotMatch(AllowedPairsRow(), "    private static readonly PaymentStatus[] Approved = [PaymentStatus.Paid, PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded];");
    }

    // The three statuses on one line, in any order.
    [GeneratedRegex(@"^(?=.*\bPaymentStatus\.Paid\b)(?=.*\bPaymentStatus\.PartiallyRefunded\b)(?=.*\bPaymentStatus\.Refunded\b)", RegexOptions.CultureInvariant)]
    private static partial Regex TheThreeStatuses();

    // A row of the allowed-pairs table: "[OrderStatus.X] = new HashSet<PaymentStatus> { ... },".
    [GeneratedRegex(@"^\s*\[OrderStatus\.\w+\]\s*=\s*new HashSet<PaymentStatus>\s*\{[^}]*\},?\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex AllowedPairsRow();
}
