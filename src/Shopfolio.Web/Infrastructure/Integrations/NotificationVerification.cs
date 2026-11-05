namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The result of verifying an incoming payment notification (08 3.1 table, 3.2; 09 11.1 items 2 and 5). It is a local computation and makes
/// no call, so it has no failure class: a notification whose signature is missing or does not match is <see cref="SignatureInvalid"/>.
/// </summary>
internal abstract record NotificationVerification
{
    private NotificationVerification()
    {
    }

    /// <summary>The signature matches; the notice carries only the fields the field mapping reads (08 2.1.5).</summary>
    public sealed record SignatureValid(PaymentNotice Notice) : NotificationVerification;

    /// <summary>The signature is missing or does not match: the request is not a notification (08 3.2 principle 2).</summary>
    public sealed record SignatureInvalid : NotificationVerification;
}
