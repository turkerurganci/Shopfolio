namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>
/// The text of an audit operation code and the one mapping from a code to its area (09 9.2 items 1, 2). The audit trail stores the code
/// name as text without a check constraint, because the trail cannot be converted by a migration; a code that this version does not
/// recognize (written by a later version) is shown as it is and never raises an error.
/// </summary>
public static class AuditOperationText
{
    private static readonly Dictionary<string, AuditOperation> ByName =
        Enum.GetValues<AuditOperation>().ToDictionary(value => value.ToString(), StringComparer.Ordinal);

    /// <summary>The text that is stored: the code name of the operation.</summary>
    public static string ToCode(AuditOperation operation)
    {
        string code = operation.ToString();
        return ByName.ContainsKey(code)
            ? code
            : throw new ArgumentOutOfRangeException(nameof(operation), operation, "The value is not an audit operation code.");
    }

    /// <summary>Reads a stored text; false when this version does not recognize the code.</summary>
    public static bool TryParse(string? code, out AuditOperation operation)
    {
        operation = default;
        return code is not null && ByName.TryGetValue(code, out operation);
    }

    /// <summary>The text the panel shows for a stored code: the code itself, recognized or not (the Turkish name comes from the message catalog).</summary>
    public static string Display(string code)
    {
        ArgumentNullException.ThrowIfNull(code);
        return code;
    }

    /// <summary>The area an operation belongs to (09 9.2 item 3). The caller never passes an area.</summary>
    public static AuditArea AreaOf(AuditOperation operation) => operation switch
    {
        AuditOperation.UpdateCompany or AuditOperation.UpdateBranding => AuditArea.CompanyAndBranding,
        AuditOperation.ChangePublishStatus => AuditArea.Publication,
        AuditOperation.MarkBankTransferPaid
            or AuditOperation.MarkShipped
            or AuditOperation.MarkDelivered
            or AuditOperation.SetDeliveryOrderAgainstWithdrawal
            or AuditOperation.MarkDeliveryFailed
            or AuditOperation.CompleteService
            or AuditOperation.CancelByCompany
            or AuditOperation.RefundManually
            or AuditOperation.ReceiveReturn
            or AuditOperation.ReceiveReturnWithIbanRequest
            or AuditOperation.OpenRefundIbanRequest
            or AuditOperation.RejectReturn
            or AuditOperation.CorrectAddress
            or AuditOperation.ReduceItemQuantity
            or AuditOperation.RemoveItem
            or AuditOperation.CorrectShippingInfo
            or AuditOperation.CorrectDeliveryDate
            or AuditOperation.CorrectReturnArrivalDate
            or AuditOperation.CorrectTransition
            or AuditOperation.SaveInternalNote
            or AuditOperation.CloseNotReturned
            or AuditOperation.RecordWithdrawalFromOtherChannel
            or AuditOperation.RecordLateDeliveryTerminationFromOtherChannel
            or AuditOperation.CorrectContactEmail
            or AuditOperation.RetryCardRefund
            or AuditOperation.RefundByBankTransfer
            or AuditOperation.MarkCardRefundNotCompleted
            or AuditOperation.ReportRefundTransferNotCompleted
            or AuditOperation.ResendEmail => AuditArea.Order,
        AuditOperation.SendAdminInvitation
            or AuditOperation.WithdrawAdminInvitation
            or AuditOperation.CreateAdminAccount
            or AuditOperation.RemoveAdminAccount
            or AuditOperation.ChangeAdminEmail => AuditArea.AdminAccounts,
        AuditOperation.PublishLegalText => AuditArea.LegalTextVersion,
        AuditOperation.UpdateSettings => AuditArea.Settings,
        AuditOperation.ExportOrders or AuditOperation.ExportCustomers or AuditOperation.ExportContactRequests => AuditArea.Export,
        AuditOperation.DeleteCustomerAccount => AuditArea.CustomerDeletion,
        _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "The audit operation has no area."),
    };
}
