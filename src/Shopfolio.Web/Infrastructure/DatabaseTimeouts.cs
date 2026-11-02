namespace Shopfolio.Infrastructure;

/// <summary>
/// Database session times (09 10.7 item 1). Only the client command time exists so far: the liveness setting is checked
/// against it (09 10.7 item 6). The server-side times and the maintenance values arrive with the session profiles (T233).
/// </summary>
internal static class DatabaseTimeouts
{
    /// <summary>The client command time of the web profile (09 3.5 table); it is the backup of the server-side times.</summary>
    public static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(20);
}
