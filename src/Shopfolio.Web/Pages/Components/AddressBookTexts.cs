using System.Globalization;

namespace Shopfolio.Pages.Components;

/// <summary>
/// The address book screen's texts (T518; 04 E-26 5.26; 07 UC-70 to UC-73; K-768). Forms and account screens speak "siz" (04 8.2.9). Written here once; the page and its tests read them from here. The
/// sources fix no wording for these sentences except the empty line's meaning and the sentence under the form (5.26.5, 5.26.9); they are written in the voice of the other account screens.
/// </summary>
public static class AddressBookTexts
{
    /// <summary>04 5.26.2: the screen's heading and tab title; the name of the inner navigation's link.</summary>
    public const string ScreenName = "Adres defteri";

    /// <summary>04 5.26.3 (2): the button that opens the new entry's form; the empty line's way out is the same button (04 5.26.9).</summary>
    public const string AddButton = "Adres ekle";

    /// <summary>04 5.26.9: the empty address book says there is no saved address.</summary>
    public const string EmptySentence = "Kayıtlı adresiniz yok.";

    public const string NewHeading = "Yeni adres";

    public const string EditHeading = "Adresi düzenle";

    public const string SaveButton = "Kaydet";

    public const string CancelLink = "Vazgeç";

    public const string EditLink = "Düzenle";

    public const string DeleteButton = "Sil";

    /// <summary>04 5.26.5: what a change or a deletion in the book does not do - said in one sentence under the form (02 3.14.5).</summary>
    public const string OrdersAreFrozen = "Defterdeki değişiklik ve silme geçmiş siparişlerinize dokunmaz; siparişleriniz adreslerini sipariş anında dondurmuştur.";

    /// <summary>The accessible name of an entry's edit link: the visible text is the same for every entry, the name says which one.</summary>
    public static string EditLabel(string entryName) => $"{entryName} adresini düzenle";

    /// <summary>The accessible name of an entry's delete button.</summary>
    public static string DeleteLabel(string entryName) => $"{entryName} adresini sil";

    /// <summary>The place line of an entry: the district and the province (04 5.26.3).</summary>
    public static string PlaceLine(string district, string provinceName) => string.Create(CultureInfo.InvariantCulture, $"{district}, {provinceName}");
}

/// <summary>
/// The order history screen's texts (T518; 04 E-27 5.27; 07 UC-74). Written here once; the page and its tests read them from here.
/// </summary>
public static class OrderHistoryTexts
{
    /// <summary>04 5.27.2: the screen's heading and tab title; the name of the inner navigation's link ("Siparişlerim", 04 3.1.7).</summary>
    public const string ScreenName = "Siparişlerim";

    /// <summary>The caption of the list (04 2.13.4): the table is named for assistive technology.</summary>
    public const string Caption = "Siparişleriniz";

    public const string NumberColumn = "Sipariş no";

    public const string DateColumn = "Tarih";

    public const string ItemsColumn = "Ürünler";

    public const string TotalColumn = "Toplam";

    public const string StatusColumn = "Durum";

    /// <summary>04 5.27.10: the history of a member without an order says there is no order yet and gives the way back to the products (04 2.7.1.2).</summary>
    public const string EmptySentence = "Henüz siparişiniz yok.";

    public const string EmptyExitLabel = "Ürünlere dön";

    /// <summary>04 5.27.4: the first item's name and the count of the other items.</summary>
    public static string ItemsLine(string? firstItemName, int otherItemCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(otherItemCount);
        string first = firstItemName ?? string.Empty;
        return otherItemCount == 0 ? first : string.Create(CultureInfo.InvariantCulture, $"{first} ve {otherItemCount} kalem daha");
    }
}
