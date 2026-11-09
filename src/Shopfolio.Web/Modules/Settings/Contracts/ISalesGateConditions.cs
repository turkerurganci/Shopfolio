namespace Shopfolio.Modules.Settings.Contracts;

/// <summary>
/// The card line of the payment method condition (02 3.1.5 condition 2; 10 ÖK-9): true when the payment provider's keys are defined at installation. The payment task
/// (T352) binds it: the Payment module registers <c>CardPaymentCondition</c>, which reads the conditional line worked out at start-up (05 8.4.6; D-81), in place of the default.
/// </summary>
public interface ICardPaymentCondition
{
    /// <summary>True when the card payment line is open.</summary>
    Task<bool> IsOpenAsync(CancellationToken ct);
}

/// <summary>
/// The bank transfer line of the payment method condition (02 3.1.5 condition 2; 02 3.21.5): true when the transfer is switched on and the IBAN is filled. The transfer task
/// (T353) binds it by registering its implementation in place of the default; until then the default says "not open" and the condition is not met (11 T331 criterion 8).
/// </summary>
public interface IBankTransferCondition
{
    /// <summary>True when the bank transfer is open with its IBAN.</summary>
    Task<bool> IsOpenAsync(CancellationToken ct);
}

/// <summary>
/// The privacy notice part of the legal text condition (02 3.1.5 condition 4, 3.33.2, 3.33.9): true when a version of the privacy notice has been published with its hosting
/// location section filled. The legal text task (T366) binds it (<c>PrivacyNoticeCondition</c>: the last published version carries its hosting location). The
/// identity fields of the same definition are read by the gate itself from the identity, so the notice is "complete" only when both hold.
/// </summary>
public interface IPrivacyNoticeCondition
{
    /// <summary>True when the privacy notice is published with the hosting location filled.</summary>
    Task<bool> IsPublishedWithHostingLocationAsync(CancellationToken ct);
}
