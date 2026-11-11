using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Pages.Components;

/// <summary>
/// The cart's texts (T508; 04 5.12.3, 5.12.5, 5.12.10, 5.12.15, 5.12.16): the once-told messages of K-772, the line messages of 02 3.16.5-3.16.9 and the empty state. The cart speaks "sen" (K-772).
/// The texts are written here once; the cart page and the frame's merge message read them from here.
/// </summary>
public static class CartTexts
{
    /// <summary>02 3.16.5; 04 5.12.5: the current price differs from the first add's; the old price and the direction are not told.</summary>
    public const string PriceChanged = "Sepete eklediğinden beri fiyatı değişti";

    /// <summary>04 2.5.6, 5.12.5: a sold-out variant or a full service.</summary>
    public const string SoldOut = "Tükendi";

    /// <summary>02 3.16.8; 04 5.12.5: the stock does not hold the line's quantity; no number is told.</summary>
    public const string NoStock = "Bu adette stok yok";

    /// <summary>K-772 (K-594): no line of the cart can enter an order; said in the place of the payment step's button.</summary>
    public const string NoOrderableItem = "Sepetindeki ürünlerin hiçbiri şu an satın alınamıyor";

    /// <summary>K-772 (03 10.1.1.1; K-1200): the return to the cart because no payment method could be used or the provider could not be reached; told once.</summary>
    public const string PaymentUnavailable = "Ödeme şu an alınamıyor; sepetin korunuyor, daha sonra yeniden dene";

    /// <summary>04 5.12.16, 2.7.1.2: the empty cart's sentence.</summary>
    public const string EmptySentence = "Sepetin boş.";

    /// <summary>04 5.12.16: the empty cart's way out - back to the products, which is the home page (K-754: there is no "tüm ürünler" page).</summary>
    public const string EmptyExitLabel = "Ürünlere dön";

    /// <summary>The label of the payment step's button (04 5.12.2).</summary>
    public const string ProceedLabel = "Ödeme adımına geç";

    /// <summary>02 3.16.9: the product's per-order limit, told as a number ("Bu üründen bir siparişte en fazla 2 adet alınabilir").</summary>
    public static string OverOrderLimit(int limit) => "Bu üründen bir siparişte en fazla " + DisplayFormat.Number(limit) + " adet alınabilir";

    /// <summary>K-772: a line taken off the showcase and out of the cart; told once ("{ürün adı} artık satışta olmadığı için sepetinden çıkarıldı").</summary>
    public static string RemovedFromShowcase(string productName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        return productName + " artık satışta olmadığı için sepetinden çıkarıldı";
    }

    /// <summary>K-772 (02 3.16.4): what the sign-in merge added; told once ("Önceki ziyaretinden sepetine {n} ürün eklendi: {ürün adları}").</summary>
    public static string MergeAdded(IReadOnlyList<string> productNames)
    {
        ArgumentNullException.ThrowIfNull(productNames);
        if (productNames.Count == 0)
        {
            throw new ArgumentException("The merge message names at least one product.", nameof(productNames));
        }

        return "Önceki ziyaretinden sepetine " + DisplayFormat.Number(productNames.Count) + " ürün eklendi: " + string.Join(", ", productNames);
    }

    /// <summary>The message of a line's quantity problem (04 5.12.5); null when the quantity is held.</summary>
    public static string? QuantityProblem(CartQuantityProblem problem, int? orderLimit) => problem switch
    {
        CartQuantityProblem.None => null,
        CartQuantityProblem.NoStock => NoStock,
        CartQuantityProblem.OverOrderLimit => OverOrderLimit(orderLimit ?? throw new ArgumentNullException(nameof(orderLimit), "A limit message names the limit (02 3.16.9).")),
        _ => throw new ArgumentOutOfRangeException(nameof(problem), problem, "A cart line's quantity problem is one of the closed list."),
    };
}
