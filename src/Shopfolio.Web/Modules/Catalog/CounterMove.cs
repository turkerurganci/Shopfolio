namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The four system movements of the counters (06 4.14): freeing, reserving, the final decrease and the return. Not stored; within one variant the
/// order of the members is the order of the writes, so a confirmation frees the earlier order's units before it reserves its own.
/// </summary>
internal enum CounterMove
{
    Release,
    Reserve,
    Commit,
    Return,
}
