namespace Shopfolio.Infrastructure.Operations;

/// <summary>
/// The outcome of applying one instant deletion again after a database restore (T388; 05 8.5.10): applied, or skipped because the record was not there already. Neither is a refusal - a
/// user's gates are not applied to a replay - so the transaction always commits; the caller counts the two.
/// </summary>
public sealed class DeletionReapplyResult : IOperationResult
{
    private DeletionReapplyResult(bool wasApplied)
    {
        WasApplied = wasApplied;
    }

    /// <summary>True when the record was there and the removal ran; false when the record was gone and the line was skipped.</summary>
    public bool WasApplied { get; }

    public bool IsRejected => false;

    /// <summary>The record was removed by the same path as the first time.</summary>
    public static DeletionReapplyResult Applied() => new(true);

    /// <summary>The record was not there already (05 8.5.10).</summary>
    public static DeletionReapplyResult Skipped() => new(false);
}
