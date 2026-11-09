using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// One line of the confirmation summary (T356; 02 3.24.3; 04 5.13.9 (b)): what the customer reads about an item before the order exists and what the order's frozen item says after it - both
/// are rendered from this one shape, so the form's summary and the confirmation step's summary cannot differ. <paramref name="UnitPrice"/> is the price that applies (the discounted one while
/// the discount runs); <paramref name="ListPrice"/> is the price before the discount and is empty without one; <paramref name="Payable"/> is the line's amount after the coupon's share,
/// VAT included.
/// </summary>
public sealed record ConfirmationSummaryLine(
    string ProductName,
    string? FirstOptionValue,
    string? SecondOptionValue,
    ProductType ProductType,
    int Quantity,
    decimal UnitPrice,
    decimal? ListPrice,
    decimal? DiscountPercentage,
    decimal CouponShare,
    decimal VatRate,
    decimal Vat,
    decimal Payable,
    WithdrawalExemptionReason? WithdrawalExemptionReason,
    int? FulfilmentPeriodDays);

/// <summary>
/// The whole confirmation summary (T356): the lines that enter the order, the shipping fee (zero for none or free), the total to pay and the payment method. <paramref name="ShippingPeriod"/>
/// is the firm's shipping period in business days and is empty when no line is physical. <paramref name="ContactEmail"/> is the guest's e-mail the summary shows and the customer may correct
/// before confirming (02 3.24.5); it is not sent a code. <paramref name="Shipping"/> and <paramref name="Billing"/> are the addresses the customer wrote at the payment step (they live in the
/// payment step's encrypted cookie, 05 MK-56, not on the server); empty values stay as tokens in the form's text and are filled the way an order's are.
/// </summary>
public sealed record ConfirmationSummary(
    IReadOnlyList<ConfirmationSummaryLine> Lines,
    decimal ShippingFee,
    decimal Total,
    PaymentMethod PaymentMethod,
    int? ShippingPeriod,
    string? ContactEmail,
    PartyAddress? Shipping,
    PartyAddress? Billing);

/// <summary>The four boxes of the confirmation step (02 3.24.1, 3.24.2): the first two come with every order, the digital one when a digital item is in the order, the service one when a service is.</summary>
public enum ConfirmationBox
{
    PreInformationFormRead,
    DistanceSalesContractAccepted,
    DigitalWithdrawalWaiver,
    ServiceFulfilmentConsent,
}

/// <summary>A box offered to the customer: its text as shown and, for the digital and service boxes, the names of the items it covers (04 2.12.7.6; K-771).</summary>
public sealed record ConfirmationBoxOffer(ConfirmationBox Box, string Text, IReadOnlyList<string> CoveredItemNames);

/// <summary>The boxes the customer ticked. A box that was not offered is simply false; a ticked box that was not offered is ignored (the rule reads what the order needs, not what the client sent).</summary>
public sealed record TickedBoxes(bool PreInformationFormRead, bool DistanceSalesContractAccepted, bool DigitalWithdrawalWaiver, bool ServiceFulfilmentConsent);

/// <summary>
/// What the confirmation step shows (T356; 04 5.13.9): the form and the contract as full text (the form is the summary followed by the generated text), the ids of the versions shown - the
/// confirmation sends them back and a risen version stops the order -, the fixed "how the order is placed" text, the short withdrawal summary above the boxes, the boxes with their texts, the
/// guest's e-mail line and the button with the payment obligation. There is no consent box, no age box and no note field in it (02 3.24.1, 3.24.4, 3.24.7).
/// </summary>
public sealed record ConfirmationStepView(
    string SummaryText,
    string PreInformationFormText,
    string DistanceSalesContractText,
    Guid ShownPreInformationFormId,
    Guid ShownDistanceSalesContractId,
    string HowTheOrderIsPlacedText,
    string WithdrawalSummaryText,
    IReadOnlyList<ConfirmationBoxOffer> Boxes,
    string? ContactEmailLine,
    string PaymentObligationNotice,
    string ConfirmButtonText);

/// <summary>What the confirmation sends back: the version ids shown, the boxes ticked, and the lines' types and names so the rule works out which boxes the order needs.</summary>
public sealed record ConfirmationStepSubmission(
    Guid ShownPreInformationFormId,
    Guid ShownDistanceSalesContractId,
    TickedBoxes Ticked,
    IReadOnlyList<ConfirmationSummaryLine> Lines,
    PaymentMethod PaymentMethod);

/// <summary>The outcomes of the step's check, in the order they are looked at: a risen legal text version first (the customer confirms the current text), then the boxes.</summary>
public enum ConfirmationStepOutcome
{
    /// <summary>Every needed box is ticked and the shown texts are the current ones.</summary>
    Ready,

    /// <summary>The form or the contract was published again since it was shown; the order is not created and the boxes' ticks are cleared (04 2.12.7.3).</summary>
    LegalTextVersionRaised,

    /// <summary>A box the order needs is not ticked; the order is not created and the first missing box is marked (04 5.13.18).</summary>
    BoxesMissing,
}

/// <summary>The step's verdict: the outcome and, for <see cref="ConfirmationStepOutcome.BoxesMissing"/>, the needed boxes still unticked in the order they stand on the screen.</summary>
public sealed record ConfirmationStepCheck(ConfirmationStepOutcome Outcome, IReadOnlyList<ConfirmationBox> MissingBoxes)
{
    public bool IsReady => Outcome == ConfirmationStepOutcome.Ready;
}
