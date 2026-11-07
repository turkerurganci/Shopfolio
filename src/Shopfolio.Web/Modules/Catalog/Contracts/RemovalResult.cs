using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;

namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// Where a refused move that would break a published product's gate sends the administrator (02 3.7.8; 03 1.8.2): first the product itself, to draft or to the archive. One
/// list for the status move and for the deletion, so the panel says the same thing in both.
/// </summary>
public static class LastVariantGuidance
{
    /// <summary>The product statuses the panel offers when the last Published variant cannot go: Draft and Archived.</summary>
    public static IReadOnlyList<PublishStatus> ProductMoves { get; } = [PublishStatus.Draft, PublishStatus.Archived];
}

/// <summary>
/// The outcome of a permanent deletion or of a digital file's removal (02 3.7.4, 3.7.7, 3.7.8; 07 6.2): done; unchanged because the file was already removed; or refused. A refusal
/// carries the code of the error registry and nothing else is written. <c>LastPublishedVariant</c> is the refusal of the last Published variant of a published product (it carries
/// <see cref="LastVariantGuidance.ProductMoves"/>); <c>DigitalFileRequired</c> is the refusal of a file removal that would leave a Published variant of a published digital
/// product without a file (it carries those variants, so the panel shows them). The transaction runner rolls a refused operation back.
/// </summary>
public sealed class RemovalResult : IOperationResult
{
    private static readonly IReadOnlyList<Guid> NoVariants = [];

    private RemovalResult(bool isDone, ErrorCode? code, IReadOnlyList<Guid> variantsWithoutFile)
    {
        IsDone = isDone;
        Code = code;
        VariantsWithoutFile = variantsWithoutFile;
    }

    /// <summary>The record was deleted or the file removed.</summary>
    public static RemovalResult Done { get; } = new(true, null, NoVariants);

    /// <summary>The file was already removed; nothing was written.</summary>
    public static RemovalResult Unchanged { get; } = new(false, null, NoVariants);

    /// <summary>The record was not found (deleted meanwhile).</summary>
    public static RemovalResult NotFound { get; } = new(false, ErrorCode.NotFound, NoVariants);

    /// <summary>The form's version is no longer the record's (07 6.2: <c>RecordVersionConflict</c>).</summary>
    public static RemovalResult VersionConflict { get; } = new(false, ErrorCode.RecordVersionConflict, NoVariants);

    /// <summary>The last Published variant of a published product cannot be deleted (07 6.2: <c>LastPublishedVariant</c>).</summary>
    public static RemovalResult LastPublishedVariant { get; } = new(false, ErrorCode.LastPublishedVariant, NoVariants);

    public bool IsDone { get; }

    /// <summary>The refusal's code; empty when the operation was done or changed nothing.</summary>
    public ErrorCode? Code { get; }

    /// <summary>Filled only with <see cref="ErrorCode.DigitalFileRequired"/>: the Published variants the removal would leave without a file.</summary>
    public IReadOnlyList<Guid> VariantsWithoutFile { get; }

    public bool IsRejected => Code is not null;

    /// <summary>Where the panel sends the administrator after a <c>LastPublishedVariant</c> refusal: the product's draft or archive; empty otherwise.</summary>
    public IReadOnlyList<PublishStatus> SuggestedProductMoves => Code == ErrorCode.LastPublishedVariant ? LastVariantGuidance.ProductMoves : [];

    /// <summary>The file removal would leave Published variants of a published digital product without a file (07 6.2: <c>DigitalFileRequired</c>).</summary>
    public static RemovalResult FileRequired(IReadOnlyList<Guid> variantsWithoutFile)
    {
        ArgumentNullException.ThrowIfNull(variantsWithoutFile);
        return new RemovalResult(false, ErrorCode.DigitalFileRequired, variantsWithoutFile);
    }
}
