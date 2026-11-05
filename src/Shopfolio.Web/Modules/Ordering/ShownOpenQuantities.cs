namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The check of the customer's item operations (05 7.3.2; MK-21; K-1202): an item cancellation or a withdrawal carries, per item, the open
/// quantity the screen showed; under the order's row lock the open quantity as it is now is compared and, if any item differs, the request is
/// not applied at all - a second send of a partial choice is not handled as a second operation. The customer's operations carry no version; the
/// open quantity is derived (06 2.7.2) by the operation that calls this check, after the lock.
/// </summary>
internal static class ShownOpenQuantities
{
    /// <summary>True when every shown item has an open quantity now and it equals the shown one; an empty request matches nothing.</summary>
    public static bool Match(IReadOnlyDictionary<Guid, int> shown, IReadOnlyDictionary<Guid, int> current)
    {
        ArgumentNullException.ThrowIfNull(shown);
        ArgumentNullException.ThrowIfNull(current);
        return shown.Count > 0 && shown.All(item => current.TryGetValue(item.Key, out int now) && now == item.Value);
    }
}
