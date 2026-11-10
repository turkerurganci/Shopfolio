using System.Diagnostics;

using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Status;

namespace Shopfolio.Modules.Payment;

/// <summary>
/// The card's "şu an kullanılamıyor" state before the confirmation (08 2.1.8; 03 10.1.1.1, 3.2.1.15; 07 UC-22, UC-27): a token request whose answer is a connection
/// error keeps the moment in memory for <see cref="Duration"/> - the application is one instance and one process (05 1.2.4), and a restart empties it. While it holds,
/// the payment step shows the card as unavailable and does not let it be chosen, and a confirmation with the card is refused without a token request
/// (<see cref="PaymentMethods"/>). When it runs out the state is gone and the next card confirmation's token request finds out again. It is neither a retry nor a
/// circuit breaker (05 10.1.1, 10.1.15): no call is repeated, and the confirmed order's start (UC-29) always asks for its token.
/// </summary>
internal sealed class CardUnavailability
{
    /// <summary>08 2.1.8 — how long the card stays unavailable after a connection error of the token request; a design value (09 3.5), reviewed with the first set-up's measurement.</summary>
    public static readonly TimeSpan Duration = TimeSpan.FromMinutes(2);

    private readonly TimeProvider _time;
    private long _untilTicks;

    public CardUnavailability(TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(time);
        _time = time;
    }

    /// <summary>True while the last connection error is younger than <see cref="Duration"/>; at the end of the duration the state is gone.</summary>
    public bool IsUnavailable => _time.GetUtcNow().UtcTicks < Interlocked.Read(ref _untilTicks);

    /// <summary>A connection error of a token request: the state holds for <see cref="Duration"/> from now.</summary>
    public void Mark() => Interlocked.Exchange(ref _untilTicks, (_time.GetUtcNow() + Duration).UtcTicks);
}

/// <summary>The outcome of a payment start for the payment page (UC-29): the step of <see cref="PaymentStartRule"/> and, when the form may open, its address.</summary>
internal sealed record PaymentStartOutcome(PaymentStartStep Step, Uri? FrameAddress);

/// <summary>
/// The start of a confirmed card order's payment (08 2.1.8; UC-29): one token request through the link layer, never repeated, made outside any transaction (the page
/// cancels a <see cref="PaymentStartStep.CancelAsUnavailable"/> order in its own transaction with <see cref="PaymentStartRule.UnopenedCancellationReason"/>). The
/// answer's class goes to two in-memory records: a connection error - the table's first, second and last rows - starts the card's unavailable state
/// (<see cref="CardUnavailability"/>) and counts in the token requests' error series of the status endpoint (05 9.4.18; T314 criterion 8); a successful token ends
/// the series; the second token refusal neither counts nor ends it. The start of a confirmed order is not stopped by the unavailable state - it always asks for the token.
/// </summary>
internal sealed class PaymentStarts
{
    private readonly PaymentStartFailureSeries _series;
    private readonly CardUnavailability _unavailability;
    private readonly IPaymentGateway? _gateway;

    /// <summary>The gateway is the provider task's registration; it is optional so the module's registration holds in a host without the link layer (the command process).</summary>
    public PaymentStarts(PaymentStartFailureSeries series, CardUnavailability unavailability, IPaymentGateway? gateway = null)
    {
        ArgumentNullException.ThrowIfNull(series);
        ArgumentNullException.ThrowIfNull(unavailability);
        _series = series;
        _unavailability = unavailability;
        _gateway = gateway;
    }

    public async Task<PaymentStartOutcome> StartAsync(PaymentStartRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        IPaymentGateway gateway = _gateway ?? throw new InvalidOperationException("A payment is started only by the payment gateway.");
        PaymentStartResult answer = await gateway.StartAsync(request, ct);
        switch (answer)
        {
            case PaymentStartResult.Started:
                _series.RecordSuccess();
                break;

            case PaymentStartResult.ConnectionFailed:
                _series.RecordFailure();
                _unavailability.Mark();
                break;

            case PaymentStartResult.SecondTokenRejected:
                break;

            default:
                throw new UnreachableException();
        }

        return new PaymentStartOutcome(PaymentStartRule.Decide(answer), (answer as PaymentStartResult.Started)?.FrameAddress);
    }
}
