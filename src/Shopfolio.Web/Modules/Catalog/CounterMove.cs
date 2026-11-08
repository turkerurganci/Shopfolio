namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The system movements of the counters (06 4.14): freeing, reserving, the final decrease, the return, and the two corrections' movements - the
/// retake of a wrongly cancelled paid order's units (path 6) and the restore of a corrected transfer mark's reservation (path 5). Not stored; within
/// one variant the order of the members is the order of the writes, so a confirmation frees the earlier order's units before it reserves its own.
/// </summary>
internal enum CounterMove
{
    Release,
    Reserve,
    Commit,
    Return,
    Retake,
    Restore,
}
