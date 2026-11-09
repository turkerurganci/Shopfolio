using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// The seven corporate content records (02 5.2): each carries its own publish status of two values, <c>Draft</c> and <c>Published</c>. There is
/// no archive, no publication on a later date and no version history (03 1.8.4; 10 KD-38).
/// </summary>
internal enum ContentRecordKind
{
    AboutPage,
    Offering,
    PortfolioItem,
    FaqItem,
    Branch,
    CustomPage,
    Announcement,
}

/// <summary>A field of the publish gate (02 3.27.13): what a record must hold to be published and to stay published.</summary>
internal enum ContentGateField
{
    Name,
    Title,
    Question,
    Answer,
    Province,
    District,
    AddressLine,
    ShortIntroduction,
    Text,
}

/// <summary>
/// A corporate content record seen by the publish rules (T322): its kind, its publish status and the gate fields it lacks. The status is written by
/// <see cref="ContentPublishing"/> (the Draft to Published gate, the way back to draft), which also saves the edit of a record
/// (<see cref="ContentPublishing.SaveEditAsync{TRecord}"/>: an edit that breaks the gate of a published record is not saved).
/// </summary>
internal interface IContentRecord : IVersionedRecord
{
    Guid Id { get; }

    ContentRecordKind Kind { get; }

    PublishStatus PublishStatus { get; set; }

    /// <summary>The gate fields of <see cref="Kind"/> that are empty now, in the order of <see cref="ContentGate.RequiredFields"/>.</summary>
    IReadOnlyList<ContentGateField> MissingGateFields();
}

/// <summary>
/// A content record that has a page address of its own (06 4.72; 02 3.30.2): the service introduction, the reference work and the general page. The address follows the name
/// until the first publication (<see cref="ContentAuthoring"/> makes it again on every save) and is fixed at that moment (<see cref="ContentPublishing"/> writes
/// <see cref="SlugFixedAt"/> in the same save as the status); from then on it never changes, whatever the name becomes and wherever the record goes after.
/// </summary>
internal interface IAddressedContentRecord : IContentRecord
{
    /// <summary>The record's part of the page address.</summary>
    string Slug { get; set; }

    /// <summary>The moment of the first publication; filled means the address no longer changes.</summary>
    DateTimeOffset? SlugFixedAt { get; set; }
}

/// <summary>
/// The publish gate of the corporate content (02 3.27.13, 5.2; 03 1.8.4): the required fields of each record kind, in one table. A field that holds
/// only white space is empty. The about page and the announcement have no name or title; their gate is the content itself (the short introduction,
/// the text). The picture is required in no kind.
/// </summary>
internal static class ContentGate
{
    /// <summary>The fields the kind needs to be published (02 3.27.13; 02 5.2 "Taslak → Yayında").</summary>
    public static IReadOnlyList<ContentGateField> RequiredFields(ContentRecordKind kind)
    {
        return kind switch
        {
            ContentRecordKind.AboutPage => [ContentGateField.ShortIntroduction],
            ContentRecordKind.Offering => [ContentGateField.Name],
            ContentRecordKind.PortfolioItem => [ContentGateField.Title],
            ContentRecordKind.FaqItem => [ContentGateField.Question, ContentGateField.Answer],
            ContentRecordKind.Branch => [ContentGateField.Name, ContentGateField.Province, ContentGateField.District, ContentGateField.AddressLine],
            ContentRecordKind.CustomPage => [ContentGateField.Title],
            ContentRecordKind.Announcement => [ContentGateField.Text],
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown content record kind."),
        };
    }

    /// <summary>The required fields of <paramref name="kind"/> that <paramref name="isFilled"/> says are empty.</summary>
    public static IReadOnlyList<ContentGateField> Missing(ContentRecordKind kind, Func<ContentGateField, bool> isFilled)
    {
        ArgumentNullException.ThrowIfNull(isFilled);
        return [.. RequiredFields(kind).Where(field => !isFilled(field))];
    }

    /// <summary>True when the text holds something other than white space.</summary>
    public static bool IsFilled(string? text) => !string.IsNullOrWhiteSpace(text);

    /// <summary>A field the kind does not have: the record's switch refuses it so a table row without a reader fails loudly.</summary>
    public static bool Unreadable(ContentRecordKind kind, ContentGateField field) =>
        throw new InvalidOperationException($"{kind} has no gate field {field}.");
}
