using Shopfolio.Modules.AfterSales;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the table of the item's records equal to 03 1.7.1 row by row (T320 criteria 1-2, KK1-KK2; 09 21.4 item 1): the rows' numbers and their "Hatta sayılışı"
/// cells are read from the document and compared with <see cref="ItemRecordTable.Rows"/>; the quantity and actor columns follow 03 1.7.4's sentence (every record but
/// the delivery mark and the download counter carries the actor, the moment and the units it covers). The expected cells come from the document, not from the test.
/// </summary>
[Trait("Consistency", "true")]
public sealed class ItemRecordTableConsistencyTests
{
    private static readonly string[] Lines = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "03_USER_FLOWS.md")).ReplaceLineEndings("\n").Split('\n');

    private static List<string[]> DocumentedRows()
    {
        int start = Array.FindIndex(Lines, l => l.StartsWith("**1.7.1 Kalemin kayıtları.**", StringComparison.Ordinal));
        Assert.True(start >= 0, "03 1.7.1 is not found.");
        List<string[]> rows = [];
        for (int i = start + 1; i < Lines.Length && !Lines[i].StartsWith("**1.7.2", StringComparison.Ordinal); i++)
        {
            if (Lines[i].StartsWith("| 1.7.1.", StringComparison.Ordinal))
            {
                rows.Add([.. Lines[i].Trim('|').Split('|').Select(c => c.Trim())]);
            }
        }

        return rows;
    }

    /// <summary>The "Hatta sayılışı" cell as the effect it names: the cell's opening words decide it.</summary>
    private static LineEffect EffectOf(string cell)
    {
        if (cell.StartsWith("Teslim edilmiş", StringComparison.Ordinal))
        {
            return LineEffect.Delivered;
        }

        if (cell.StartsWith("Kapanmış", StringComparison.Ordinal))
        {
            return LineEffect.Closed;
        }

        if (cell.StartsWith("İptal edilmiş gibi", StringComparison.Ordinal))
        {
            return LineEffect.CountedAsCancelled;
        }

        if (cell.StartsWith("Teslim işaretsiz kalemde", StringComparison.Ordinal))
        {
            return LineEffect.DependsOnDeliveryMark;
        }

        Assert.True(cell == "—" || cell.StartsWith("Hattı etkilemez", StringComparison.Ordinal), $"Unknown line effect cell: {cell}");
        return LineEffect.None;
    }

    [Fact]
    public void Rows_AreTheDocumentsRows_InOrder_WithTheSameLineEffect()
    {
        List<string[]> documented = DocumentedRows();

        Assert.Equal(11, documented.Count);
        Assert.Equal(documented.Select(r => r[0]), ItemRecordTable.Rows.Select(r => r.Number));
        Assert.Equal(documented.Select(r => EffectOf(r[3])), ItemRecordTable.Rows.Select(r => r.LineEffect));
    }

    [Fact]
    public void QuantityAndActorColumns_FollowTheUnitSentence_ExceptTheDeliveryMarkAndTheDownloadCounter()
    {
        int unitRule = Array.FindIndex(Lines, l => l.StartsWith("**1.7.4 Kalemin adedi", StringComparison.Ordinal));
        Assert.True(unitRule >= 0, "03 1.7.4 is not found.");
        Assert.Contains("teslim işareti ve indirme sayacı dışında — kapsadıkları adedi taşır", Lines[unitRule], StringComparison.Ordinal);

        ItemRecordKind[] unmarked = [ItemRecordKind.DeliveryMark, ItemRecordKind.DownloadCounter];
        Assert.All(ItemRecordTable.Rows, r => Assert.Equal(!unmarked.Contains(r.Kind), r.CarriesActorAndMoment));

        // The IBAN request covers no units (03 1.7.1.9 names none); every other record with an actor carries its units (03 1.7.4).
        Assert.Equal(
            [ItemRecordKind.DeliveryMark, ItemRecordKind.RefundIbanRequest, ItemRecordKind.DownloadCounter],
            ItemRecordTable.Rows.Where(r => !r.CarriesQuantity).Select(r => r.Kind));
    }

    [Fact]
    public void TheOpenQuantity_LosesExactlyTheClosingRecords_OfTheUnitRule()
    {
        // 03 1.7.4: the open quantity is the quantity minus the units closed by the cancellation, the removal, the late delivery termination and the withdrawal.
        Assert.Equal(
            [ItemRecordKind.Cancellation, ItemRecordKind.ItemRemoval, ItemRecordKind.LateDeliveryTermination, ItemRecordKind.Withdrawal],
            ItemRecordTable.Rows.Where(r => ItemRecordTable.ClosesOpenQuantity(r.Kind)).Select(r => r.Kind));
    }
}
