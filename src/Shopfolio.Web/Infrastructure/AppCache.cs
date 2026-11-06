namespace Shopfolio.Infrastructure;

/// <summary>
/// The application's one cache (09 10.4 item 2; 05 7.10.1, 3.1.2; MK-34): in the process's memory, one slot per <see cref="CacheArea"/>, no
/// separate cache server and no cache library. One instance runs, so the invalidation is within this process. A slot holds the value, its
/// generation counter and its lock. A reader takes the generation before it loads; an invalidation raises the generation and drops the value;
/// the reader writes its value only when the generation has not moved - an old read that an invalidation overtook is handed to its caller but
/// never stored. The generation is read, compared and the value written under the slot's lock, and there is no <c>await</c> inside the lock.
/// There is no expiry and no warm-up: a value falls only on an invalidation or a restart, and the first request loads it. Two requests that find
/// the slot empty may both load; both read a valid value and one overwrites the other (no single-flight lock: 09 10.4 item 2).
/// The only caller of <see cref="Invalidate"/> is the transaction runner, after the commit (09 10.4 item 3; the source scan holds it there).
/// </summary>
internal sealed class AppCache
{
    private readonly Slot[] _slots;

    public AppCache()
    {
        CacheArea[] areas = Enum.GetValues<CacheArea>();
        _slots = new Slot[areas.Length];
        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i] = new Slot();
        }
    }

    /// <summary>
    /// The value of <paramref name="area"/>, loaded by <paramref name="load"/> when the slot is empty. The loaded value is an immutable read
    /// record (an <c>AsNoTracking</c> projection; 09 10.4 item 1). A load that fails leaves the slot empty and the generation where it was; the
    /// exception goes to the caller. Each area's value type is fixed; a type that does not match is a programming error (<see cref="InvalidCastException"/>).
    /// </summary>
    public async Task<T> GetOrLoadAsync<T>(CacheArea area, Func<CancellationToken, Task<T>> load, CancellationToken ct)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(load);
        Slot slot = SlotOf(area);
        long generation;
        lock (slot.Gate)
        {
            if (slot.Value is not null)
            {
                return (T)slot.Value;
            }

            // The generation is taken BEFORE the database read.
            generation = slot.Generation;
        }

        // An old read may be running here while the record is saved and the slot invalidated.
        T value = await load(ct);
        ArgumentNullException.ThrowIfNull(value, nameof(load));
        lock (slot.Gate)
        {
            // An invalidation came in between: the value is not written, the next call loads again.
            if (slot.Generation == generation)
            {
                slot.Value = value;
            }
        }

        // The caller uses the value it read either way.
        return value;
    }

    /// <summary>Drops the value of <paramref name="area"/> and raises its generation, so a read that started before cannot write its old value.</summary>
    public void Invalidate(CacheArea area)
    {
        Slot slot = SlotOf(area);
        lock (slot.Gate)
        {
            slot.Generation++;
            slot.Value = null;
        }
    }

    /// <summary>The generation of <paramref name="area"/>'s slot, read under its lock; it rises by one with each invalidation.</summary>
    public long GenerationOf(CacheArea area)
    {
        Slot slot = SlotOf(area);
        lock (slot.Gate)
        {
            return slot.Generation;
        }
    }

    private Slot SlotOf(CacheArea area)
    {
        if (!Enum.IsDefined(area))
        {
            throw new ArgumentOutOfRangeException(nameof(area), area, "Unknown cache area.");
        }

        return _slots[(int)area];
    }

    private sealed class Slot
    {
        public readonly Lock Gate = new();

        public object? Value;

        public long Generation;
    }
}

/// <summary>The registration of the cache (09 10.4 items 2, 3): the one cache of the process and the scope's list of areas to drop after the commit.</summary>
internal static class AppCacheRegistration
{
    public static IServiceCollection AddAppCache(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<AppCache>();
        services.AddScoped<CacheInvalidations>();
        return services;
    }
}
