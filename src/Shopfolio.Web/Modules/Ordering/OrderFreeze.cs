using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The values an order freezes at the moment it is born (T355; 02 3.23): the one place that copies the live product, the firm's identity and the legal text versions into the order's
/// own columns. The moment of freezing is the moment the order is created (02 3.23.4): the confirmation calls these before the order's first save, inside its transaction, and
/// nothing writes the frozen values again. A later change of the product, the firm settings or the identity reaches only new orders; the order's delivery, cancellation and
/// withdrawal operations read the item's frozen columns, never the product (K-598). The brand settings are not frozen: the order has no brand column, and its page shows the
/// current logo, name and colour (02 3.23.5; K-265). Pure: no database, no clock.
/// </summary>
internal static class OrderFreeze
{
    /// <summary>
    /// Freezes one item (02 3.23.1, 3.23.2): every value of <paramref name="source"/> is copied flat into the item - the price as stored, the product's VAT rate, the running discount's
    /// rate, rounded discounted price and declared reference price, the coupon's share and the item's VAT from <paramref name="amounts"/>, the name, the option values and the image; the
    /// type, the physical item's exemption, the service's fulfilment period (Z-39) and, on a digital item, the download allowance of P-8 that applies now and the variant's current file.
    /// <paramref name="amounts"/> is <c>ICatalogModule.PriceItem</c>'s answer for the price that applies - the discounted one while the discount runs - and the quantity; a mismatch is a
    /// programming error.
    /// </summary>
    public static OrderItem FreezeItem(Guid orderId, OrderItemSource source, int quantity, ItemAmounts amounts, int downloadAllowance, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(amounts);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        decimal applied = source.Discount?.DiscountedPrice ?? source.UnitPrice;
        if (amounts.Amount != applied * quantity)
        {
            throw new ArgumentException("The amounts are not of the price that applies and the quantity (02 3.8.3).", nameof(amounts));
        }

        bool isDigital = source.ProductType == ProductType.Digital;
        if (isDigital && downloadAllowance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(downloadAllowance), "A digital item freezes a download allowance above zero (P-8).");
        }

        OrderItem item = OrderItem.Create(
            orderId, source.ProductId, source.VariantId, source.ProductType, source.ProductName, quantity, source.UnitPrice, source.VatRate, amounts.Vat, now);
        item.FirstOptionValue = source.FirstOptionValue;
        item.SecondOptionValue = source.SecondOptionValue;
        item.ImageId = source.ImageId;
        item.CouponShare = amounts.CouponShare;
        item.DiscountPercentage = source.Discount?.Percentage;
        item.DiscountedUnitPrice = source.Discount?.DiscountedPrice;
        item.ReferencePrice = source.Discount?.ReferencePrice;
        item.WithdrawalExemptionReason = source.ProductType == ProductType.Physical ? source.WithdrawalExemptionReason : null;
        item.FulfilmentPeriod = source.ProductType == ProductType.Service ? source.FulfilmentPeriod : null;
        item.DownloadAllowance = isDigital ? downloadAllowance : null;
        item.DigitalFileId = isDigital ? source.DigitalFileId : null;
        return item;
    }

    /// <summary>
    /// Freezes the firm's legal identity of the day, the legal text versions and the bank transfer IBAN into the order (02 3.23.3, 3.24.6; 06 4.17): the type, the identity fields of the
    /// type - the tax number of a tradesman, the MERSIS and trade registry numbers of the two merchant types (06 4.17) -, the address and the contact details; the pre-information
    /// form, distance sales contract and privacy notice versions of that moment; and, on a bank transfer order, the firm's IBAN of that moment (02 3.21.5). The sales gate keeps the
    /// identity and the texts complete at confirmation (02 3.1.5); an empty required value here is a programming error, as is an IBAN on a card order or none on a bank transfer.
    /// </summary>
    public static void FreezeLegalIdentity(Order order, CompanyIdentity identity, LegalTextVersionIds versions, string? bankTransferIban)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(versions);
        CompanyType type = identity.CompanyType ?? throw Incomplete(nameof(identity.CompanyType));
        bool isTradesman = type == CompanyType.Tradesman;
        if ((order.PaymentMethod == PaymentMethod.BankTransfer) != (bankTransferIban is not null))
        {
            throw new ArgumentException("The IBAN is frozen exactly on a bank transfer order (02 3.21.5).", nameof(bankTransferIban));
        }

        order.CompanyType = type;
        order.CompanyLegalName = identity.LegalName ?? throw Incomplete(nameof(identity.LegalName));
        order.CompanyTaxIdNumber = isTradesman ? identity.TaxIdNumber ?? throw Incomplete(nameof(identity.TaxIdNumber)) : null;
        order.CompanyMersisNumber = isTradesman ? null : identity.MersisNumber ?? throw Incomplete(nameof(identity.MersisNumber));
        order.CompanyTradeRegistryNumber = isTradesman ? null : identity.TradeRegistryNumber ?? throw Incomplete(nameof(identity.TradeRegistryNumber));
        order.CompanyProvinceId = identity.ProvinceId ?? throw Incomplete(nameof(identity.ProvinceId));
        order.CompanyDistrict = identity.District ?? throw Incomplete(nameof(identity.District));
        order.CompanyAddressLine = identity.AddressLine ?? throw Incomplete(nameof(identity.AddressLine));
        order.CompanyPhone = identity.Phone ?? throw Incomplete(nameof(identity.Phone));
        order.CompanyEmail = identity.Email ?? throw Incomplete(nameof(identity.Email));
        order.CompanyKepAddress = identity.KepAddress ?? throw Incomplete(nameof(identity.KepAddress));
        order.PreInformationFormVersionId = versions.PreInformationForm ?? throw Incomplete(nameof(versions.PreInformationForm));
        order.DistanceSalesContractVersionId = versions.DistanceSalesContract ?? throw Incomplete(nameof(versions.DistanceSalesContract));
        order.PrivacyNoticeVersionId = versions.PrivacyNotice ?? throw Incomplete(nameof(versions.PrivacyNotice));
        order.BankTransferIban = bankTransferIban;
    }

    /// <summary>
    /// The customer's name, addresses and e-mail at confirmation (02 3.23.3; K-1027 (4), (5)): the order's two exceptions to freezing are the firm's corrections, and each keeps the
    /// earlier value in its own history (T375). A corrected value is the old value of the history's first row - the earliest correction -, an uncorrected one is the order's own.
    /// </summary>
    public static ConfirmationParties PartiesAtConfirmation(Order order, IReadOnlyList<OrderAddressHistory> addressHistory, IReadOnlyList<OrderEmailHistory> emailHistory)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(addressHistory);
        ArgumentNullException.ThrowIfNull(emailHistory);
        OrderEmailHistory? firstEmail = emailHistory.OrderBy(h => h.CreatedAt).ThenBy(h => h.Id).FirstOrDefault();
        return new ConfirmationParties(
            firstEmail?.OldEmail ?? order.ContactEmail,
            AddressAtConfirmation(AddressType.ShippingAddress, addressHistory, Current(order.ShippingRecipientName, order.ShippingPhone, order.ShippingProvinceId, order.ShippingDistrict, order.ShippingAddressLine)),
            AddressAtConfirmation(AddressType.BillingAddress, addressHistory, Current(order.BillingRecipientName, null, order.BillingProvinceId, order.BillingDistrict, order.BillingAddressLine)));
    }

    private static PartyAddress? AddressAtConfirmation(AddressType type, IReadOnlyList<OrderAddressHistory> history, PartyAddress? current)
    {
        OrderAddressHistory? first = history.Where(h => h.AddressType == type).OrderBy(h => h.CreatedAt).ThenBy(h => h.Id).FirstOrDefault();
        return first is null || current is null
            ? current
            : new PartyAddress(first.OldRecipientName, first.OldPhone, first.OldProvinceId, first.OldDistrict, first.OldAddressLine);
    }

    private static PartyAddress? Current(string? recipientName, string? phone, Guid? provinceId, string? district, string? addressLine)
    {
        return recipientName is null || provinceId is null || district is null || addressLine is null
            ? null
            : new PartyAddress(recipientName, phone, provinceId.Value, district, addressLine);
    }

    private static InvalidOperationException Incomplete(string field)
        => new($"{field} is empty at confirmation; the sales gate keeps it filled (02 3.1.5, 3.23.3).");
}
