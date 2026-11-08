using Shopfolio.Modules.Ordering;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the moment table of the code equal to 03 1.4.4 (T318 criterion 8, KK8; 09 21.4 item 1): the nine rows are read from the document - the
/// number, the moment, the stock and capacity cell and the coupon cell - and compared with <see cref="CounterMomentTable.Rows"/> both ways. A cell is
/// read by the opening words that name its effect; the vocabulary below is the only thing typed in the test and an unknown wording fails it.
/// </summary>
[Trait("Consistency", "true")]
public sealed class CounterMomentTableConsistencyTests
{
    private static readonly (string Opening, CounterMoment Moment)[] Moments =
    [
        ("Sipariş oluşur", CounterMoment.OrderPlaced),
        ("Ödeme onayı (Ö1)", CounterMoment.PaymentApproved),
        ("Ödenmeden iptal", CounterMoment.UnpaidCancellation),
        ("Ödenmiş kalemin iptali ve çıkarma", CounterMoment.PaidItemCancellation),
        ("Gecikme feshi", CounterMoment.LateDeliveryTermination),
        ("Cayma ve iade", CounterMoment.WithdrawalAndReturn),
        ("Yanlış iptalin düzeltilmesi", CounterMoment.CancellationCorrection),
        ("Havale işaretinin düzeltilmesi", CounterMoment.TransferMarkCorrection),
        ("Varyantın kalıcı silinmesi", CounterMoment.VariantDeletion),
    ];

    private static readonly (string Opening, StockEffect Effect)[] StockWordings =
    [
        ("Ayrılır", StockEffect.Reserved),
        ("Ayrılan kesin düşer", StockEffect.Committed),
        ("Serbest kalır", StockEffect.Released),
        ("İptal edilen ya da çıkarılan adet stoğa", StockEffect.Returned),
        ("Kargoya verilmemiş kalemde kendiliğinden döner", StockEffect.ReturnedUnlessShipped),
        ("Fiziksel mal yöneticinin teslim aldığı adet kadar", StockEffect.ReturnedOnReceipt),
        ("Yeniden ayrılır; yetmezse düzeltme yapılamaz", StockEffect.Retaken),
        ("Tutulu kalır", StockEffect.KeptReserved),
        ("Silinen varyantın ayırması düşer", StockEffect.ReservationDropped),
    ];

    private static readonly (string Opening, CouponEffect Effect)[] CouponWordings =
    [
        ("Ayrılır", CouponEffect.Reserved),
        ("Kullanılmış sayılır", CouponEffect.Used),
        ("Geri döner", CouponEffect.Released),
        ("Yalnız siparişin tamamı", CouponEffect.ReturnedWhenOrderFinal),
        ("Siparişin bütün adetleri iptal ya da iade edilmişse döner", CouponEffect.ReturnedWhenOrderFinal),
        ("Yeniden kullanılmış sayılır", CouponEffect.UsedAgain),
        ("Tutulu kalır", CouponEffect.KeptReserved),
        ("—", CouponEffect.None),
    ];

    private static T Read<T>(string cell, (string Opening, T Value)[] vocabulary, string what)
    {
        (string Opening, T Value)[] hits = [.. vocabulary.Where(v => cell.StartsWith(v.Opening, StringComparison.Ordinal))];
        Assert.True(hits.Length > 0, $"03 1.4.4: the {what} cell '{cell}' has an unknown wording.");
        return hits.OrderByDescending(h => h.Opening.Length).First().Value;
    }

    private static List<CounterMomentRow> DocumentedRows()
    {
        string[] lines = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "03_USER_FLOWS.md")).ReplaceLineEndings("\n").Split('\n');
        List<CounterMomentRow> rows = [];
        foreach (string line in lines.Where(l => l.StartsWith("| 1.4.4.", StringComparison.Ordinal)))
        {
            string[] cells = [.. line.Trim('|').Split('|').Select(c => c.Trim())];
            rows.Add(new CounterMomentRow(
                cells[0],
                Read(cells[1], Moments, "moment"),
                Read(cells[2], StockWordings, "stock"),
                Read(cells[3], CouponWordings, "coupon")));
        }

        return rows;
    }

    [Fact]
    public void TheDocument_HasNineMoments()
    {
        Assert.Equal(9, DocumentedRows().Count);
    }

    [Fact]
    public void Rows_EqualTheDocumentsTable_BothWays_KK8()
    {
        IReadOnlyList<string> problems = SetComparison.Compare(
            "03 1.4.4",
            DocumentedRows().Select(r => r.ToString()),
            CounterMomentTable.Rows.Select(r => r.ToString()));

        Assert.Empty(problems);
    }

    [Fact]
    public void EveryMoment_HasOneRow()
    {
        Assert.Equal(Enum.GetValues<CounterMoment>().Order(), CounterMomentTable.Rows.Select(r => r.Moment).Order());
    }
}
