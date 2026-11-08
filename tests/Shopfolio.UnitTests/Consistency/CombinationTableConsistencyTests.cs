using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the combination table of the code equal to 03 1.4.1 cell by cell (T318 criteria 1-3, KK1-KK3; 09 21.4 item 1): the table is read from the
/// document - a tick is an allowed pair, a cross an unreachable or forbidden one - and compared with <see cref="TransitionTable.Combinations"/> and
/// <see cref="TransitionTable.IsPairAllowed"/> for all thirty pairs. The expected cells are read from the document, never typed in the test.
/// </summary>
[Trait("Consistency", "true")]
public sealed class CombinationTableConsistencyTests
{
    private static readonly Dictionary<string, OrderStatus> ShipmentLabels = new(StringComparer.Ordinal)
    {
        ["Alındı"] = OrderStatus.Placed,
        ["Hazırlanıyor"] = OrderStatus.Preparing,
        ["Kargoya verildi"] = OrderStatus.Shipped,
        ["Teslim edilemedi"] = OrderStatus.DeliveryFailed,
        ["Teslim edildi"] = OrderStatus.Delivered,
        ["İptal edildi"] = OrderStatus.Cancelled,
    };

    private static readonly Dictionary<string, PaymentStatus> PaymentLabels = new(StringComparer.Ordinal)
    {
        ["Bekliyor"] = PaymentStatus.Pending,
        ["Ödendi"] = PaymentStatus.Paid,
        ["Başarısız"] = PaymentStatus.Failed,
        ["Kısmen geri ödendi"] = PaymentStatus.PartiallyRefunded,
        ["Geri ödendi"] = PaymentStatus.Refunded,
    };

    private static List<string[]> TableRows()
    {
        string[] lines = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "03_USER_FLOWS.md")).ReplaceLineEndings("\n").Split('\n');
        int start = Array.FindIndex(lines, l => l.StartsWith("**1.4.1 Birleşim tablosu.**", StringComparison.Ordinal));
        Assert.True(start >= 0, "03 1.4.1 is not found.");
        List<string[]> rows = [];
        for (int i = start + 1; i < lines.Length && !lines[i].StartsWith("**1.4.2", StringComparison.Ordinal); i++)
        {
            if (lines[i].StartsWith('|') && !lines[i].StartsWith("|---", StringComparison.Ordinal))
            {
                rows.Add([.. lines[i].Trim('|').Split('|').Select(c => c.Trim())]);
            }
        }

        return rows;
    }

    private static Dictionary<(OrderStatus, PaymentStatus), bool> DocumentedCells()
    {
        List<string[]> rows = TableRows();
        PaymentStatus[] columns = [.. rows[0].Skip(1).Select(label => PaymentLabels[label])];
        Dictionary<(OrderStatus, PaymentStatus), bool> cells = [];
        foreach (string[] row in rows.Skip(1))
        {
            OrderStatus shipment = ShipmentLabels[row[0]];
            Assert.Equal(columns.Length, row.Length - 1);
            for (int c = 0; c < columns.Length; c++)
            {
                string cell = row[c + 1];
                Assert.True(cell.StartsWith('✓') || cell.StartsWith('✗'), $"03 1.4.1 {row[0]} / {columns[c]}: '{cell}' is neither allowed nor crossed.");
                cells.Add((shipment, columns[c]), cell.StartsWith('✓'));
            }
        }

        return cells;
    }

    [Fact]
    public void TheDocumentsTable_CoversEveryPairOfTheTwoAxes()
    {
        Dictionary<(OrderStatus, PaymentStatus), bool> cells = DocumentedCells();

        Assert.Equal(Enum.GetValues<OrderStatus>().Length * Enum.GetValues<PaymentStatus>().Length, cells.Count);
    }

    [Fact]
    public void Combinations_EqualTheDocumentsTable_CellByCell_KK1()
    {
        List<string> problems = [];
        foreach (((OrderStatus shipment, PaymentStatus payment), bool allowed) in DocumentedCells())
        {
            bool inCode = TransitionTable.Combinations.TryGetValue(shipment, out IReadOnlySet<PaymentStatus>? set) && set.Contains(payment);
            if (inCode != allowed || TransitionTable.IsPairAllowed(shipment, payment) != allowed)
            {
                problems.Add($"{shipment} + {payment}: document {allowed}, code {inCode}.");
            }
        }

        Assert.Empty(problems);
    }

    [Theory]
    [InlineData(PaymentStatus.Pending)]
    [InlineData(PaymentStatus.Failed)]
    public void Delivered_WithPendingOrFailed_IsForbidden_KK2(PaymentStatus payment)
    {
        Assert.False(DocumentedCells()[(OrderStatus.Delivered, payment)]);
        Assert.False(TransitionTable.IsPairAllowed(OrderStatus.Delivered, payment));
    }

    [Theory]
    [InlineData(OrderStatus.Preparing)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.DeliveryFailed)]
    public void APendingPayment_StandsOnlyWithPlaced_KK3(OrderStatus shipment)
    {
        Assert.False(TransitionTable.IsPairAllowed(shipment, PaymentStatus.Pending));
        Assert.True(TransitionTable.IsPairAllowed(OrderStatus.Placed, PaymentStatus.Pending));
    }
}
