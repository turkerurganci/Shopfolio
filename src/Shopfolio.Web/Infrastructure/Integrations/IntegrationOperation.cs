namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The jobs of the link layer that call an outside service (08 3.1 table; 09 11.1 item 1). Verifying an incoming notification is not here: it is
/// a local computation and makes no call.
/// </summary>
internal enum IntegrationOperation
{
    PaymentStart,

    PaymentQuery,

    RefundRequest,

    RefundQuery,

    EmailSend,

    BounceQuery,

    IdentityVerify,
}
