using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// What the panel tells the firm about a withdrawal notice from another channel (02 10.4.10; K-709, K-1563, K-608): the one rule the record's screen and its POST both read.
/// It reads the item as the operation decision built it - the window's last day comes from the AfterSales period rules (Z-13, Z-14) - and the date the firm entered. Nothing
/// here refuses: the product does not judge whether the notice was sent in time or whether the information was incomplete; it only asks the firm to confirm (K-606).
/// </summary>
internal static class WithdrawalNoticeRules
{
    /// <summary>
    /// The year after the window's end within which a notice of incomplete information must have been sent (Mesafeli Sözleşmeler Yönetmeliği m.10/1; 02 4.2 Z-13;
    /// K-1563). A legal period, not a product constant: it is not in the parameter inventory (02 11 "Envantere girmeyenler"; 09 3.5 item 1) and lives with its rule.
    /// </summary>
    public const int ExtendedYears = 1;

    public static WithdrawalNoticeWarnings Warnings(OperationItemFacts item, DateOnly noticeDate)
    {
        ArgumentNullException.ThrowIfNull(item);
        DateOnly? lastDay = item.Period.WithdrawalLastDay;

        // A service item's right fell at its "tamamlandı" mark - the mark's day is the item's delivery date (02 3.20.9, 7.3.3) - whichever comes first.
        bool afterServiceMark = item.ProductType == ProductType.Service && item.IsDeliveryMarked && item.DeliveryDate is { } marked && noticeDate >= marked;
        bool outside = afterServiceMark || (lastDay is { } last && noticeDate > last);
        bool beyondYear = lastDay is { } end && noticeDate > end.AddYears(ExtendedYears);
        bool absolute = item.Period.IsExempt && !item.Period.IsConditionallyExempt;
        bool sameDay = item.ProductType == ProductType.Physical && item.DeliveryDate == noticeDate;
        return new WithdrawalNoticeWarnings(outside, beyondYear, absolute, sameDay);
    }
}
