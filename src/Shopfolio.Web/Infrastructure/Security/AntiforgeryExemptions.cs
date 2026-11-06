namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The closed list of the endpoints outside the protection against cross-site forgery (07 2.5.5; 05 6.5.1; 09 15.2 item 2): today only the payment
/// provider's signed notification (UC-80), whose signature is checked in the link layer (09 11.1). The provider's return (UC-30) is a GET and is not
/// on the list. An endpoint enters the list by a decision record. <see cref="SignedEndpoints.SignedEndpointRegistration.MapSignedEndpoint{TEndpoint, TNotice}"/>
/// is the one place that switches the protection off, and a source scan (<c>ForgeryProtectionSourceScan</c>) keeps its path argument a member of this
/// class.
/// </summary>
internal static class AntiforgeryExemptions
{
    /// <summary>UC-80: the payment provider's signed notification (07 2.1.9).</summary>
    public const string PaymentNotificationPath = "/sistem/odeme-bildirimi";

    /// <summary>The paths outside the protection, in the order of 07 2.5.5.</summary>
    public static readonly IReadOnlyList<string> Paths = [PaymentNotificationPath];

    /// <summary>True when <paramref name="pattern"/> (a route pattern) is on the list; the letter case of the literal segments does not matter.</summary>
    public static bool Contains(string? pattern) =>
        pattern is not null && Paths.Contains(pattern, StringComparer.OrdinalIgnoreCase);
}
