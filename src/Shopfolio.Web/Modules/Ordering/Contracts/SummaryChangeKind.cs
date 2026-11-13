namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>What changed between the confirmed summary and the current one (02 3.17.2). A result code, never stored.</summary>
public enum SummaryChangeKind
{
    /// <summary>An item of the confirmed summary no longer enters the order.</summary>
    ItemLeft,

    /// <summary>An item enters the order that the confirmed summary did not hold.</summary>
    ItemAdded,

    /// <summary>The item's quantity differs.</summary>
    Quantity,

    /// <summary>The item's unit price that applies differs - a price change or a discount that began or ended.</summary>
    UnitPrice,

    /// <summary>The item's breakdown differs: the list price, the discount rate, the coupon's share, the VAT rate or the VAT.</summary>
    Breakdown,

    /// <summary>The shipping fee differs - the fee, the free shipping threshold or the delivery provinces changed.</summary>
    ShippingFee,

    /// <summary>The total differs, or the coupon or the minimum order amount no longer holds.</summary>
    Total,

    /// <summary>An item could not be reserved; its quantity is never told (02 3.17.2; K-150).</summary>
    ItemNotReserved,

    /// <summary>The order's shipping period (the frozen promise of 02 3.24.3) differs - a product's or the firm's period changed after the summary was shown.</summary>
    ShippingPeriod,

    /// <summary>The delivery provinces changed and the shipping province is no longer delivered to, or a physical order carries no shipping address (02 3.17.2, 6.2.3).</summary>
    DeliveryArea,
}
