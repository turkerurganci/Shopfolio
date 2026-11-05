using System.Diagnostics;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The time limit of each job of the link layer, from the "Zaman aşımı" column of the 08 3.1 table (05 10.1.1; MK-61): the calls a user waits
/// for take <see cref="ExternalCallTimeouts.UserWaiting"/>, the calls that leave the job queue take <see cref="ExternalCallTimeouts.Job"/>. This is
/// the only place that pairs a job with its limit.
/// </summary>
internal static class IntegrationOperations
{
    public static ExternalCallKind KindOf(IntegrationOperation operation)
    {
        return operation switch
        {
            IntegrationOperation.PaymentStart => ExternalCallKind.UserWaiting,
            IntegrationOperation.IdentityVerify => ExternalCallKind.UserWaiting,
            IntegrationOperation.PaymentQuery => ExternalCallKind.Job,
            IntegrationOperation.RefundRequest => ExternalCallKind.Job,
            IntegrationOperation.RefundQuery => ExternalCallKind.Job,
            IntegrationOperation.EmailSend => ExternalCallKind.Job,
            IntegrationOperation.BounceQuery => ExternalCallKind.Job,
            _ => throw new UnreachableException(),
        };
    }

    public static TimeSpan LimitOf(IntegrationOperation operation) => ExternalCallTimeouts.For(KindOf(operation));
}
