using System.Formats.Asn1;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Shopfolio.Infrastructure.Status;

/// <summary>
/// The in-memory record of the payment start's failure series (05 9.4.18, 9.4.10; 07 UC-82): consecutive token requests that came back as a
/// connection failure, and the moment of the first. The memory is the process's: a restart empties it (05 11.2.9). This task builds the record;
/// the task that makes the token request (T404, F4) calls <see cref="RecordFailure"/> for a connection-failure result and <see cref="RecordSuccess"/>
/// for the first successful token (<c>Modules.Payment.PaymentStarts</c>): the connection-failure class holds the table's unreachable provider, its 15 second limit and
/// server errors, its <c>failed</c> answer and its unlisted answer (08 2.1.8); the second token refusal is neither and is not passed here.
/// </summary>
internal sealed class PaymentStartFailureSeries
{
    private readonly TimeProvider _time;
    private readonly object _gate = new();
    private int _count;
    private DateTimeOffset? _firstAt;

    public PaymentStartFailureSeries(TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(time);
        _time = time;
    }

    /// <summary>One more connection failure; the first one of a series stamps the series' start.</summary>
    public void RecordFailure()
    {
        lock (_gate)
        {
            if (_count == 0)
            {
                _firstAt = _time.GetUtcNow();
            }

            _count++;
        }
    }

    /// <summary>The first successful token request ends the series (05 9.4.18).</summary>
    public void RecordSuccess()
    {
        lock (_gate)
        {
            _count = 0;
            _firstAt = null;
        }
    }

    public (int Count, DateTimeOffset? FirstAt) Snapshot()
    {
        lock (_gate)
        {
            return (_count, _firstAt);
        }
    }
}

/// <summary>
/// The end of the site's certificate as the heartbeat's request saw it (05 9.4.9: "read in the heartbeat request"). The heartbeat's HTTP client
/// calls <see cref="Observe"/> in the TLS handshake; the status endpoint reads <see cref="NotAfter"/>. Nothing here opens a connection of its own.
/// </summary>
internal sealed class CertificateExpiryMemory
{
    private long _notAfterTicks;

    /// <summary>The end of the certificate the last handshake saw, or empty when no handshake has been seen.</summary>
    public DateTimeOffset? NotAfter
    {
        get
        {
            long ticks = Interlocked.Read(ref _notAfterTicks);
            return ticks == 0 ? null : new DateTimeOffset(ticks, TimeSpan.Zero);
        }
    }

    public void Record(DateTimeOffset notAfter) => Interlocked.Exchange(ref _notAfterTicks, notAfter.UtcTicks);

    /// <summary>
    /// The validation callback of the heartbeat's handler: remembers the end of the server certificate and leaves the verdict to the platform's
    /// own validation (only a clean chain is accepted). A missing certificate is remembered as nothing.
    /// </summary>
    public bool Observe(X509Certificate? certificate, SslPolicyErrors errors)
    {
        if (certificate is not null)
        {
            try
            {
                Record(NotAfterOf(certificate.GetRawCertData()));
            }
            catch (AsnContentException)
            {
                // A certificate whose encoding cannot be read teaches nothing; the platform's validation below still decides the handshake.
            }
        }

        return errors == SslPolicyErrors.None;
    }

    /// <summary>
    /// The end of the validity period of a DER certificate, read from the encoding itself: the framework's own property is a local-time value, and
    /// the application reads no moment through the server's time zone (09 7.1).
    /// </summary>
    public static DateTimeOffset NotAfterOf(byte[] der)
    {
        ArgumentNullException.ThrowIfNull(der);
        AsnReader certificate = new AsnReader(der, AsnEncodingRules.DER).ReadSequence();
        AsnReader toBeSigned = certificate.ReadSequence();
        if (toBeSigned.PeekTag().HasSameClassAndValue(new Asn1Tag(TagClass.ContextSpecific, 0)))
        {
            toBeSigned.ReadEncodedValue(); // the version
        }

        toBeSigned.ReadEncodedValue(); // the serial number
        toBeSigned.ReadEncodedValue(); // the signature algorithm
        toBeSigned.ReadEncodedValue(); // the issuer
        AsnReader validity = toBeSigned.ReadSequence();
        validity.ReadEncodedValue(); // not before
        return validity.PeekTag().HasSameClassAndValue(Asn1Tag.UtcTime) ? validity.ReadUtcTime() : validity.ReadGeneralizedTime();
    }
}
