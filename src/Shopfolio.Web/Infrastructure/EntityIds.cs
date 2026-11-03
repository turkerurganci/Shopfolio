namespace Shopfolio.Infrastructure;

/// <summary>
/// The only place where identifiers are generated (09 10.1 item 2; 06 2.3.1). The primary key is a version 7 UUID made by the
/// application; the database never makes one. The time prefix is for sort locality, not business time: no time is read from an id.
/// </summary>
internal static class EntityIds
{
    /// <summary>A new entity id, a version 7 UUID. An entity takes it in its factory method or constructor, never in a field initializer.</summary>
    public static Guid New() => Guid.CreateVersion7();

    /// <summary>The storage key of a digital file (09 6 item 6); it is not an entity id.</summary>
    public static string NewStorageKey() => Guid.CreateVersion7().ToString("N");
}
