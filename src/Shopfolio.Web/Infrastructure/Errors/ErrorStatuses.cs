using System.Collections.Frozen;

namespace Shopfolio.Infrastructure.Errors;

/// <summary>
/// The one mapping of <see cref="ErrorCode"/> to its HTTP status (07 6.1.2, 2.7.1; 09 8.2 item 1): the "HTTP" column of 07 6.2, member by member.
/// A code that is answered with two statuses depending on the place (<see cref="ErrorCode.SalesClosed"/>, <see cref="ErrorCode.SingletonContentExists"/>)
/// lists both, the status of the form's own answer first. 303 is a refusal that names its reason by a redirect, 200 a state that ends without a write
/// (07 6.1.2). The page takes its status from here and from nowhere else; a consistency test keeps the table equal to the document.
/// </summary>
internal static class ErrorStatuses
{
    private static readonly FrozenDictionary<ErrorCode, int[]> _statuses = new Dictionary<ErrorCode, int[]>
    {
        [ErrorCode.AntiforgeryValidationFailed] = [400],
        [ErrorCode.NotFound] = [404],
        [ErrorCode.MethodNotAllowed] = [405],
        [ErrorCode.PayloadTooLarge] = [413],
        [ErrorCode.RangeNotSatisfiable] = [416],
        [ErrorCode.RateLimitExceeded] = [429],
        [ErrorCode.OperationNotAvailable] = [422],
        [ErrorCode.UnexpectedPageError] = [500],
        [ErrorCode.UnexpectedOperationError] = [500],
        [ErrorCode.ServiceUnhealthy] = [503],
        [ErrorCode.FieldRequired] = [422],
        [ErrorCode.FilledFieldCleared] = [422],
        [ErrorCode.InvalidEmail] = [422],
        [ErrorCode.PasswordTooWeak] = [422],
        [ErrorCode.InvalidPhone] = [422],
        [ErrorCode.InvalidProvince] = [422],
        [ErrorCode.InvalidIban] = [422],
        [ErrorCode.InvalidAmount] = [422],
        [ErrorCode.InvalidPercentage] = [422],
        [ErrorCode.InvalidQuantity] = [422],
        [ErrorCode.InvalidDate] = [422],
        [ErrorCode.InvalidDateRange] = [422],
        [ErrorCode.InvalidPriceRange] = [200],
        [ErrorCode.InvalidUrl] = [422],
        [ErrorCode.TextTooLong] = [422],
        [ErrorCode.InvalidListValue] = [422],
        [ErrorCode.InvalidColor] = [422],
        [ErrorCode.InvalidImage] = [422],
        [ErrorCode.ImageLimitExceeded] = [422],
        [ErrorCode.UploadMissing] = [422],
        [ErrorCode.CheckboxNotAccepted] = [422],
        [ErrorCode.StockInsufficient] = [422],
        [ErrorCode.MaxQuantityPerOrderExceeded] = [422],
        [ErrorCode.DigitalVariantAlreadyInCart] = [422],
        [ErrorCode.VariantUnavailable] = [422],
        [ErrorCode.SalesClosed] = [422, 303],
        [ErrorCode.NoOrderableItem] = [303],
        [ErrorCode.MinimumOrderAmountNotMet] = [303],
        [ErrorCode.CouponRejected] = [422],
        [ErrorCode.CouponBelowMinimumBasket] = [422],
        [ErrorCode.CouponAttemptLimitExceeded] = [429],
        [ErrorCode.DeliveryProvinceNotServed] = [422],
        [ErrorCode.OrderSummaryChanged] = [422],
        [ErrorCode.PaymentMethodUnavailable] = [422],
        [ErrorCode.UnpaidOrderLimitExceeded] = [429],
        [ErrorCode.PaymentNotPending] = [303],
        [ErrorCode.PaymentProviderUnavailable] = [303],
        [ErrorCode.DataCollectionClosed] = [422],
        [ErrorCode.ContactRequestLimitExceeded] = [429],
        [ErrorCode.OrderLookupFailed] = [422],
        [ErrorCode.OrderLookupLimitExceeded] = [429],
        [ErrorCode.RefundIbanNotRequested] = [422],
        [ErrorCode.DownloadNotAvailable] = [422],
        [ErrorCode.EmailAlreadyRegistered] = [422],
        [ErrorCode.RegistrationLimitExceeded] = [429],
        [ErrorCode.VerificationLinkLimitExceeded] = [429],
        [ErrorCode.InvalidCredentials] = [422],
        [ErrorCode.EmailNotVerified] = [422],
        [ErrorCode.SignInLimitExceeded] = [429],
        [ErrorCode.PasswordResetLimitExceeded] = [429],
        [ErrorCode.ReauthenticationFailed] = [422],
        [ErrorCode.ReauthenticationRequired] = [303],
        [ErrorCode.ReauthenticationOperationInvalid] = [303],
        [ErrorCode.PendingRegistrationUnresolved] = [303],
        [ErrorCode.AccountDeletedViewExpired] = [303],
        [ErrorCode.EmailTakenByOtherAccount] = [422],
        [ErrorCode.EmailChangeRevertPending] = [422],
        [ErrorCode.LinkNoLongerValid] = [200],
        [ErrorCode.AdminEmailTaken] = [422],
        [ErrorCode.RecordVersionConflict] = [409],
        [ErrorCode.DisplayedValueConflict] = [409],
        [ErrorCode.SingletonContentExists] = [409, 303],
        [ErrorCode.PublishGateIncomplete] = [422],
        [ErrorCode.PublishedRecordGateBroken] = [422],
        [ErrorCode.LastPublishedVariant] = [422],
        [ErrorCode.DiscountedPriceNotBelowReference] = [422],
        [ErrorCode.DiscountedPriceZero] = [422],
        [ErrorCode.StockBelowReserved] = [422],
        [ErrorCode.DuplicateSku] = [422],
        [ErrorCode.DigitalFileRequired] = [422],
        [ErrorCode.ValueOutOfBounds] = [422],
        [ErrorCode.CategoryNotEmpty] = [422],
        [ErrorCode.CategoryDepthExceeded] = [422],
        [ErrorCode.DuplicateCouponCode] = [422],
        [ErrorCode.CouponValueInvalid] = [422],
        [ErrorCode.CouponUsageBelowUsed] = [422],
        [ErrorCode.UntitledDraft] = [422],
        [ErrorCode.AboutPageNotDeletable] = [422],
        [ErrorCode.OrderVersionConflict] = [409],
        [ErrorCode.QuantityExceedsOpen] = [422],
        [ErrorCode.ShipmentInfoMissing] = [422],
        [ErrorCode.DeliverySequenceRequired] = [422],
        [ErrorCode.NotShippedPhysicalItem] = [422],
        [ErrorCode.DeclarationOutsideWindow] = [422],
        [ErrorCode.ExceptionAcknowledgementRequired] = [422],
        [ErrorCode.RefundIbanChanged] = [409],
        [ErrorCode.RefundIbanMissing] = [422],
        [ErrorCode.AmountBasedRefundExceeded] = [422],
        [ErrorCode.ReturnRejectionNotAllowed] = [422],
        [ErrorCode.CorrectionNotPossible] = [422],
        [ErrorCode.BankTransferIbanRequired] = [422],
        [ErrorCode.LegalTextIncomplete] = [422],
        [ErrorCode.SalesSwitchChanged] = [303],
        [ErrorCode.InvitationNoLongerActive] = [303],
        [ErrorCode.LastAdminRemoval] = [422],
        [ErrorCode.ExportEmpty] = [422],
        [ErrorCode.NotificationSignatureInvalid] = [400],
        [ErrorCode.UploadOffsetMismatch] = [409],
        [ErrorCode.UploadSizeExceeded] = [400],
        [ErrorCode.UploadStartInvalid] = [400],
        [ErrorCode.UploadContentRangeInvalid] = [400],
        [ErrorCode.UploadRejected] = [422],
        [ErrorCode.UploadNotFound] = [404],
        [ErrorCode.DownloadLinkInvalid] = [404],
        [ErrorCode.OutcomeUncertain] = [303],
    }.ToFrozenDictionary();

    /// <summary>The members of the table, to compare with the members of the enum.</summary>
    public static IReadOnlyCollection<ErrorCode> Codes => _statuses.Keys;

    /// <summary>Every status the code is answered with; the first is the status of the form's own answer.</summary>
    public static IReadOnlyList<int> AllOf(ErrorCode code)
    {
        return _statuses.TryGetValue(code, out int[]? statuses)
            ? statuses
            : throw new ArgumentOutOfRangeException(nameof(code), code, "The code has no status; 07 6.2 and this table are changed together (09 8.2 item 1).");
    }

    /// <summary>The status of the code: the first of <see cref="AllOf"/>.</summary>
    public static int Of(ErrorCode code) => AllOf(code)[0];
}
