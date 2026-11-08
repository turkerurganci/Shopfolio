using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;

namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// The outcome of a product's or a variant's publish status change (03 1.8.1, 1.8.2; 02 3.7.1): changed; unchanged because the record already held the
/// asked status (a repeat or a stale tab: nothing is written, no trail row); or refused. A refusal carries the code of the error registry (07 6.2) and,
/// for the publish gate, the unmet conditions by name - the panel names them and sends the administrator to take the product to draft or archive first
/// (03 1.8.2). The transaction runner rolls a refused operation back.
/// </summary>
public sealed class PublishChangeResult : IOperationResult
{
    private static readonly IReadOnlyList<string> NoConditions = [];

    private PublishChangeResult(bool isChanged, ErrorCode? code, IReadOnlyList<string> unmetConditions)
    {
        IsChanged = isChanged;
        Code = code;
        UnmetConditions = unmetConditions;
    }

    /// <summary>The status was written and the trail row added.</summary>
    public static PublishChangeResult Changed { get; } = new(true, null, NoConditions);

    /// <summary>The record already held the asked status; nothing was written.</summary>
    public static PublishChangeResult Unchanged { get; } = new(false, null, NoConditions);

    /// <summary>The record was not found (deleted meanwhile).</summary>
    public static PublishChangeResult NotFound { get; } = new(false, ErrorCode.NotFound, NoConditions);

    /// <summary>The form's version is no longer the record's (07 6.2: <c>RecordVersionConflict</c>).</summary>
    public static PublishChangeResult VersionConflict { get; } = new(false, ErrorCode.RecordVersionConflict, NoConditions);

    /// <summary>The last Published variant of a published product cannot leave Published (07 6.2: <c>LastPublishedVariant</c>).</summary>
    public static PublishChangeResult LastPublishedVariant { get; } = new(false, ErrorCode.LastPublishedVariant, NoConditions);

    public bool IsChanged { get; }

    /// <summary>The refusal's code; empty when the operation succeeded or changed nothing.</summary>
    public ErrorCode? Code { get; }

    /// <summary>The gate's unmet conditions by name; filled only with <see cref="ErrorCode.PublishGateIncomplete"/>.</summary>
    public IReadOnlyList<string> UnmetConditions { get; }

    public bool IsRejected => Code is not null;

    /// <summary>Where the panel sends the administrator after a <c>LastPublishedVariant</c> refusal (03 1.8.2; 02 3.7.8): the product's draft or archive; empty otherwise.</summary>
    public IReadOnlyList<PublishStatus> SuggestedProductMoves => Code == ErrorCode.LastPublishedVariant ? LastVariantGuidance.ProductMoves : [];

    /// <summary>The publish gate refused (07 6.2: <c>PublishGateIncomplete</c>); the conditions are named.</summary>
    public static PublishChangeResult GateIncomplete(IReadOnlyList<string> unmetConditions)
    {
        ArgumentNullException.ThrowIfNull(unmetConditions);
        return new PublishChangeResult(false, ErrorCode.PublishGateIncomplete, unmetConditions);
    }
}
