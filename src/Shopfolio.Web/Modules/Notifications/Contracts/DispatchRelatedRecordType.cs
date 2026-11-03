namespace Shopfolio.Modules.Notifications.Contracts;

/// <summary>The closed values of DispatchRelatedRecordType (06 5.6.9). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum DispatchRelatedRecordType
{
    Order,
    Withdrawal,
    LateDeliveryTermination,
    DefectClaim,
    Refund,
    ContactRequest,
    Customer,
    Admin,
    AdminInvitation,
}
