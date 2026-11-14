using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.CorporateContent.Contracts;

/// <summary>
/// One row of the panel's content list (T534; 07 UC-96; 04 9.13.3): the record's name, title or question - the announcement's text; the about page has none - its publish status, the marks, the
/// place in the manual order and the orders of the two neighbours the "yukarı" and "aşağı" forms carry (<see langword="null"/> at the edge of the group; K-1193 (4)). An announcement also tells its dates and
/// whether it shows on the site now (02 3.27.21; Z-23), read from the status and the dates and never stored (06 4.52).
/// </summary>
public sealed record PanelContentRow(
    Guid Id,
    string? Title,
    PublishStatus PublishStatus,
    bool IsOnHomepage,
    bool IsInMenu,
    int SortOrder,
    int? PreviousSortOrder,
    int? NextSortOrder,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsShownNow);

/// <summary>One group of the list (04 9.13.3): the type, the name the firm gave its menu item where it has one (04 9.14), the records in the firm's manual order and the number of records marked for the home page.</summary>
public sealed record PanelContentGroup(PanelContentType Type, string? MenuName, IReadOnlyList<PanelContentRow> Rows, int HomepageMarkedCount);

/// <summary>The seven groups in the order of 04 9.13.3: Hakkımızda, Hizmetlerimiz, Referanslarımız, SSS, Şubeler, Genel sayfalar, Duyuru. Never paged (K-843).</summary>
public sealed record PanelContentList(IReadOnlyList<PanelContentGroup> Groups);

/// <summary>
/// The fields of every type's form in one record (07 UC-158 <c>SaveContentInput</c>): a type reads the fields it has and ignores the rest. The long texts are the raw typed ones going in and the stored,
/// cleaned ones coming out.
/// </summary>
public sealed record PanelContentValues
{
    public string? ShortIntroduction { get; init; }

    public string? LongText { get; init; }

    public string? Name { get; init; }

    public string? Title { get; init; }

    public string? ShortDescription { get; init; }

    public string? Text { get; init; }

    public bool IsShownOnHomepage { get; init; }

    public string? Question { get; init; }

    public string? Answer { get; init; }

    public Guid? ProvinceId { get; init; }

    public string? District { get; init; }

    public string? AddressLine { get; init; }

    public string? Phone { get; init; }

    public string? WorkingHours { get; init; }

    public string? MapUrl { get; init; }

    public Guid? ImageId { get; init; }

    public string? ImageAltText { get; init; }

    public bool IsShownInMenu { get; init; }

    public string? LinkUrl { get; init; }

    public DateOnly? StartDate { get; init; }

    public DateOnly? EndDate { get; init; }
}

/// <summary>
/// A record's form as the panel opens it (T534; 07 UC-96): the stored values, the gallery and the linked products in the firm's order, and what the side column says - the status, the gate fields the
/// record lacks (02 3.27.13), the page address of a type that has one and whether it is fixed already (02 3.30.2), and how many records of the type carry the home page mark (K-755). The new record's form has no
/// <see cref="Id"/> and the version 0.
/// </summary>
public sealed record PanelContentForm(
    PanelContentType Type,
    Guid? Id,
    int Version,
    PublishStatus PublishStatus,
    PanelContentValues Values,
    IReadOnlyList<GalleryEntry> Images,
    IReadOnlyList<Guid> ProductIds,
    IReadOnlyList<PanelContentField> MissingFields,
    string? PagePath,
    bool IsAddressFixed,
    int HomepageMarkedCount);

/// <summary>What the content form posts (T534; 07 UC-158): the type, the record - <see langword="null"/> for a new one -, the version the form was opened with, the values, the gallery as posted and the linked products in the chosen order.</summary>
public sealed record PanelContentSaveCommand(
    PanelContentType Type,
    Guid? Id,
    int? Version,
    PanelContentValues Values,
    IReadOnlyList<GalleryEntry> Images,
    IReadOnlyList<Guid> ProductIds);

/// <summary>One refused field of the content form: the member of the form (<c>Name</c>, <c>Images</c>, ...) and the field code (07 6.2).</summary>
public sealed record PanelContentRejection(string Field, string Code);

/// <summary>The result of the content form's save; a refusal is a value and writes nothing - the transaction runner rolls back (09 8.1 item 1).</summary>
public sealed class PanelContentSaveResult : IOperationResult
{
    private PanelContentSaveResult(
        PanelContentSaveOutcome outcome,
        Guid? id,
        IReadOnlyList<PanelContentRejection> rejections,
        IReadOnlyList<PanelContentField> missing)
    {
        Outcome = outcome;
        Id = id;
        Rejections = rejections;
        Missing = missing;
    }

    public PanelContentSaveOutcome Outcome { get; }

    /// <summary>The saved record's key; for <see cref="PanelContentSaveOutcome.AlreadyExists"/> the key of the record that is there.</summary>
    public Guid? Id { get; }

    public Guid? ExistingId => Outcome == PanelContentSaveOutcome.AlreadyExists ? Id : null;

    public IReadOnlyList<PanelContentRejection> Rejections { get; }

    public IReadOnlyList<PanelContentField> Missing { get; }

    public bool IsRejected => Outcome != PanelContentSaveOutcome.Saved;

    public static PanelContentSaveResult Saved(Guid id) => new(PanelContentSaveOutcome.Saved, id, [], []);

    public static PanelContentSaveResult Refused(PanelContentSaveOutcome outcome, Guid? id = null) => new(outcome, id, [], []);

    public static PanelContentSaveResult Rejected(IReadOnlyList<PanelContentRejection> rejections)
    {
        ArgumentNullException.ThrowIfNull(rejections);
        if (rejections.Count == 0)
        {
            throw new ArgumentException("A refusal for values names at least one field.", nameof(rejections));
        }

        return new PanelContentSaveResult(PanelContentSaveOutcome.Rejected, null, rejections, []);
    }

    public static PanelContentSaveResult GateBroken(Guid id, IReadOnlyList<PanelContentField> missing) => new(PanelContentSaveOutcome.PublishedGateBroken, id, [], missing);
}

/// <summary>The result of a publish status move.</summary>
public sealed class PanelContentChangeResult : IOperationResult
{
    private PanelContentChangeResult(PanelContentChangeOutcome outcome, IReadOnlyList<PanelContentField> missing)
    {
        Outcome = outcome;
        Missing = missing;
    }

    public PanelContentChangeOutcome Outcome { get; }

    public IReadOnlyList<PanelContentField> Missing { get; }

    public bool IsRejected => Outcome is PanelContentChangeOutcome.GateIncomplete or PanelContentChangeOutcome.NotFound;

    public static PanelContentChangeResult Of(PanelContentChangeOutcome outcome) => new(outcome, []);

    public static PanelContentChangeResult Incomplete(IReadOnlyList<PanelContentField> missing) => new(PanelContentChangeOutcome.GateIncomplete, missing);
}

/// <summary>The result of a deletion.</summary>
public sealed class PanelContentDeleteResult : IOperationResult
{
    private PanelContentDeleteResult(PanelContentDeleteOutcome outcome)
    {
        Outcome = outcome;
    }

    public PanelContentDeleteOutcome Outcome { get; }

    public bool IsRejected => Outcome != PanelContentDeleteOutcome.Deleted;

    public static PanelContentDeleteResult Of(PanelContentDeleteOutcome outcome) => new(outcome);
}

/// <summary>The result of a move in the manual order.</summary>
public sealed class PanelContentReorderResult : IOperationResult
{
    private PanelContentReorderResult(PanelContentReorderOutcome outcome)
    {
        Outcome = outcome;
    }

    public PanelContentReorderOutcome Outcome { get; }

    public bool IsRejected => Outcome != PanelContentReorderOutcome.Moved;

    public static PanelContentReorderResult Of(PanelContentReorderOutcome outcome) => new(outcome);
}
