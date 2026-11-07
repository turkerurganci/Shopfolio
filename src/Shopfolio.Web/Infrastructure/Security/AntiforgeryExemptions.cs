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

    /// <summary>UC-83: the start of the chunked upload (07 4.31.2); its token is in the <c>RequestVerificationToken</c> header, not outside the protection.</summary>
    public const string UploadStartPath = "/panel/yukleme";

    /// <summary>UC-83: a chunk of the chunked upload (07 4.31.2; the same pattern answers the status GET).</summary>
    public const string UploadChunkPath = "/panel/yukleme/{id}";

    /// <summary>
    /// The writing endpoints that are checked in the endpoint, with the token in the header (09 15.7 item 4; D-131): not exemptions - the check is on, done by
    /// <see cref="HeaderForgeryTokenFilter"/> - but the only writing endpoints besides the pages and the signed notification. The path of an endpoint that
    /// carries the header check is a member of this list and the other way round (the audit of the mapped endpoints and the source scan).
    /// </summary>
    public static readonly IReadOnlyList<string> HeaderTokenPaths = [UploadStartPath, UploadChunkPath];

    /// <summary>True when <paramref name="pattern"/> (a route pattern) is on the list; the letter case of the literal segments does not matter.</summary>
    public static bool Contains(string? pattern) =>
        pattern is not null && Paths.Contains(pattern, StringComparer.OrdinalIgnoreCase);

    /// <summary>True when <paramref name="pattern"/> is on the header token list (<see cref="HeaderTokenPaths"/>).</summary>
    public static bool ContainsHeaderToken(string? pattern) =>
        pattern is not null && HeaderTokenPaths.Contains(pattern, StringComparer.OrdinalIgnoreCase);
}
