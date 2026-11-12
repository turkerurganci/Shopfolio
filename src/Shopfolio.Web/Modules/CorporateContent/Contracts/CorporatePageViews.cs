using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.CorporateContent.Contracts;

/// <summary>
/// A question as the one FAQ page lists it (T506; 02 3.27.6; 04 5.9.3): the question and its answer, which is cleaned HTML of the closed text format set. <paramref name="IsDraft"/> is true only for the
/// signed-in administrator's preview, where the draft question stands in its place marked "Taslak" (04 5.9.7); a visitor's list holds no draft.
/// </summary>
public sealed record FaqQuestionView(Guid Id, string Question, string? AnswerHtml, bool IsDraft);

/// <summary>
/// A branch of the contact page (T506; 02 3.27.7; 04 5.10.4): the name, the address parts, the optional telephone (ten digits) and working hours, the image with its alternative text and the
/// map link with the fixed text. <paramref name="IsDraft"/> is true only for the administrator's preview (04 5.10.13). The map is a link only; nothing is embedded (02 8.3.3).
/// </summary>
public sealed record ContactBranchView(
    Guid Id,
    string Name,
    string? ProvinceName,
    string? District,
    string? AddressLine,
    string? Phone,
    string? WorkingHours,
    Guid? ImageId,
    string? ImageAltText,
    string? MapUrl,
    string? MapLinkText,
    bool IsDraft);

/// <summary>
/// The contact page's content (T506; 02 3.27.8; 04 5.10.3, 5.10.4): not a record but a composition of the company's legal identity - the same set the footer shows - and the published branches in the
/// firm's order (the administrator's preview adds the drafts). The firm enters no text of its own for it.
/// </summary>
public sealed record ContactPageView(LegalIdentity Identity, IReadOnlyList<ContactBranchView> Branches);
