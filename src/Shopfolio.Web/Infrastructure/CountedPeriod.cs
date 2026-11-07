namespace Shopfolio.Infrastructure;

/// <summary>The periods of 02 4.1.3 that are set in days. Only the code names live here; which way each is counted lives in <c>PeriodCounting.CountingOf</c>.</summary>
public enum CountedPeriod
{
    ShippingPeriod,
    BankTransferPaymentWindow,
    ServiceFulfilmentPeriod,
    WithdrawalWindow,
    ReturnShipmentPeriod,
    RefundPeriod,
    DefectClaimPeriod,
    DataRequestAnswerPeriod,
    ImpossibilityNoticePeriod,
    LegalDeliveryLimit,
}
