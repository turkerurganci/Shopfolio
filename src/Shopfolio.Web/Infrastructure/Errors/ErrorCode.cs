namespace Shopfolio.Infrastructure.Errors;

/// <summary>
/// The error code registry (07 6.1.1, 6.2, 6.4.2; 09 8.2 item 1): one member for every code of the tables of 07 6.2.1-6.2.9, spelled as the document
/// spells it, and <see cref="OutcomeUncertain"/>, which only the log line carries (07 6.4.2). A code is an English PascalCase name, is unique in the
/// application and does not change once given; the number of a member means nothing - codes enter the log and every comparison by name, so a member
/// is never renumbered and never removed (07 2.7.3). A new code is written to 07 6.2 first and added here second; the lock file test and the
/// consistency test keep the three lists equal. The code is never shown or sent: not in a body, a header, a cookie or an address (07 2.7.3; 09 8.2 item 4).
/// The HTTP status of a code is the table of <c>ErrorStatuses</c>. The codes of the short-lived notices are another enum (<c>NoticeCode</c>, T214).
/// </summary>
public enum ErrorCode
{
    // 07 6.2.1
    AntiforgeryValidationFailed,
    NotFound,
    MethodNotAllowed,
    PayloadTooLarge,
    RangeNotSatisfiable,
    RateLimitExceeded,
    OperationNotAvailable,
    UnexpectedPageError,
    UnexpectedOperationError,
    ServiceUnhealthy,

    // 07 6.2.2
    FieldRequired,
    FilledFieldCleared,
    InvalidEmail,
    PasswordTooWeak,
    InvalidPhone,
    InvalidProvince,
    InvalidIban,
    InvalidAmount,
    InvalidPercentage,
    InvalidQuantity,
    InvalidDate,
    InvalidDateRange,
    InvalidPriceRange,
    InvalidUrl,
    TextTooLong,
    InvalidListValue,
    InvalidColor,
    InvalidImage,
    ImageLimitExceeded,
    UploadMissing,
    CheckboxNotAccepted,

    // 07 6.2.3
    StockInsufficient,
    MaxQuantityPerOrderExceeded,
    DigitalVariantAlreadyInCart,
    VariantUnavailable,
    SalesClosed,
    NoOrderableItem,
    MinimumOrderAmountNotMet,
    CouponRejected,
    CouponBelowMinimumBasket,
    CouponAttemptLimitExceeded,
    DeliveryProvinceNotServed,
    OrderSummaryChanged,
    PaymentMethodUnavailable,
    UnpaidOrderLimitExceeded,
    PaymentNotPending,
    PaymentProviderUnavailable,
    DataCollectionClosed,
    ContactRequestLimitExceeded,

    // 07 6.2.4
    OrderLookupFailed,
    OrderLookupLimitExceeded,
    RefundIbanNotRequested,
    DownloadNotAvailable,

    // 07 6.2.5
    EmailAlreadyRegistered,
    RegistrationLimitExceeded,
    VerificationLinkLimitExceeded,
    InvalidCredentials,
    EmailNotVerified,
    SignInLimitExceeded,
    PasswordResetLimitExceeded,
    ReauthenticationFailed,
    ReauthenticationRequired,
    ReauthenticationOperationInvalid,
    PendingRegistrationUnresolved,
    AccountDeletedViewExpired,
    EmailTakenByOtherAccount,
    EmailChangeRevertPending,
    LinkNoLongerValid,
    AdminEmailTaken,

    // 07 6.2.6
    RecordVersionConflict,
    DisplayedValueConflict,
    SingletonContentExists,
    PublishGateIncomplete,
    PublishedRecordGateBroken,
    LastPublishedVariant,
    DiscountedPriceNotBelowReference,
    DiscountedPriceZero,
    StockBelowReserved,
    DuplicateSku,
    DigitalFileRequired,
    ValueOutOfBounds,
    CategoryNotEmpty,
    CategoryDepthExceeded,
    DuplicateCouponCode,
    CouponValueInvalid,
    CouponUsageBelowUsed,
    UntitledDraft,
    AboutPageNotDeletable,

    // 07 6.2.7
    OrderVersionConflict,
    QuantityExceedsOpen,
    ShipmentInfoMissing,
    DeliverySequenceRequired,
    NotShippedPhysicalItem,
    DeclarationOutsideWindow,
    ExceptionAcknowledgementRequired,
    RefundIbanChanged,
    RefundIbanMissing,
    AmountBasedRefundExceeded,
    ReturnRejectionNotAllowed,
    CorrectionNotPossible,

    // 07 6.2.8
    BankTransferIbanRequired,
    LegalTextIncomplete,
    SalesSwitchChanged,
    InvitationNoLongerActive,
    LastAdminRemoval,
    ExportEmpty,

    // 07 6.2.9
    NotificationSignatureInvalid,
    UploadOffsetMismatch,
    UploadSizeExceeded,
    UploadStartInvalid,
    UploadContentRangeInvalid,
    UploadRejected,
    UploadNotFound,
    DownloadLinkInvalid,

    // 07 6.4.2: only the log line carries it
    OutcomeUncertain,
}
