namespace Shopfolio.Infrastructure.Values;

/// <summary>The closed values of DisposalRecordType (06 5.5.3). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum DisposalRecordType
{
    Order,
    DispatchRecord,
    ContactRequest,
    LoginRecord,
    AuditLog,
    DisposalRecord,
    InboundNotificationRecord,
    DefectClaim,
    Image,
    DigitalFile,
    LogFile,
    ServerLogRotation,
    OrphanFile,
    Customer,
    Admin,
    PendingRegistration,
    RefundIban,
    PendingEmailChange,
    AttemptRecord,
    Session,
    RecognizedBrowserMark,
    Cart,
    AdminInvitation,
    LinkRecord,
}
