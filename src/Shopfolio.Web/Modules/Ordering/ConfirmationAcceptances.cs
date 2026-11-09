using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The frozen record of the boxes ticked at the confirmation (T356; 02 3.24.6; 06 4.21): for each digital or service box the order carries one record with the box's type, the text that was
/// shown and the items it covers - every item of the box's product type. The form's and the contract's boxes are not recorded here - their proof is the frozen text versions (T355). T348
/// calls this in the confirmation's transaction with the order's frozen items and adds the rows; the acceptance's moment is the order's creation, so the rows have no stamp of their own.
/// Pure: no database.
/// </summary>
internal static class ConfirmationAcceptances
{
    /// <summary>
    /// The records for the boxes of <paramref name="offers"/> that cover items (the digital and the service box): one record per box and, with it, one link per covered item. A box offered
    /// for a type the order has no item of is a programming error. The caller passes the offers of a <c>Ready</c> <see cref="ConfirmationStepCheck"/>: a box offered was ticked.
    /// </summary>
    public static IReadOnlyList<(CheckboxAcceptance Acceptance, IReadOnlyList<CheckboxAcceptanceOrderItem> Items)> Records(
        Guid orderId, IReadOnlyList<ConfirmationBoxOffer> offers, IReadOnlyList<OrderItem> items, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(offers);
        ArgumentNullException.ThrowIfNull(items);
        List<(CheckboxAcceptance, IReadOnlyList<CheckboxAcceptanceOrderItem>)> records = [];
        foreach (ConfirmationBoxOffer offer in offers)
        {
            (CheckboxType Type, ProductType Product)? covers = offer.Box == ConfirmationBox.DigitalWithdrawalWaiver
                ? (CheckboxType.Digital, ProductType.Digital)
                : offer.Box == ConfirmationBox.ServiceFulfilmentConsent ? (CheckboxType.Service, ProductType.Service) : null;
            if (covers is null)
            {
                continue;
            }

            OrderItem[] covered = [.. items.Where(i => i.ProductType == covers.Value.Product)];
            if (covered.Length == 0)
            {
                throw new ArgumentException($"The {offer.Box} box was offered but the order has no {covers.Value.Product} item.", nameof(offers));
            }

            CheckboxAcceptance acceptance = CheckboxAcceptance.Create(orderId, covers.Value.Type, offer.Text, now);
            records.Add((acceptance, [.. covered.Select(i => CheckboxAcceptanceOrderItem.Create(acceptance.Id, i.Id, now))]));
        }

        return records;
    }
}
