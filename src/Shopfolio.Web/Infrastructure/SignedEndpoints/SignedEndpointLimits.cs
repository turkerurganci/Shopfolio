namespace Shopfolio.Infrastructure.SignedEndpoints;

/// <summary>
/// The three values of the signed machine endpoints (08 3.2 principles 2 and 5; 07 2.8.1, 2.8.2; 05 4.5.2). Their one written place is the 09 3.5
/// table's row; no other code or test writes the numbers.
/// </summary>
internal static class SignedEndpointLimits
{
    /// <summary>
    /// The body limit of the payment provider's notification: 64 KB (07 2.8.1; 08 3.2 principle 5; K-1169). It is applied before the body is read
    /// and before the signature is checked; a longer body is answered 413.
    /// </summary>
    public const int BodyMaxBytes = 64 * 1024;

    /// <summary>
    /// The window of the signature failure's thinned log line: the sliding window of the general rate limit, 60 seconds (07 2.8.2; 05 4.5.2;
    /// K-1570 - one kind of window).
    /// </summary>
    public static readonly TimeSpan FailureLogWindow = TimeSpan.FromSeconds(60);

    /// <summary>
    /// The ceiling of the windows the thinned line keeps in memory: 10 000 addresses (05 4.5.2; D-108; K-1582). When it is full, the oldest window with a count writes
    /// its count at once in a line and is dropped - the count is not lost, only written before its address comes back.
    /// </summary>
    public const int FailureLogMaxAddresses = 10_000;
}
