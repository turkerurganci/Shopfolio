using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Lists;

namespace Shopfolio.Pages.Storefront;

/// <summary>
/// The FAQ page as the screen draws it (E-09; T506; 04 5.9): the page's name, the questions in the firm's order in one list - no category, no sub-heading, no search - and, when no question is
/// published, the empty-state line with the way home (04 5.9.8; 2.7.1.2). A draft question is in the list only for the administrator's preview, marked in its place (04 5.9.7).
/// </summary>
public sealed record FaqPage(string Title, IReadOnlyList<FaqQuestionView> Questions, EmptyState? Empty)
{
    /// <summary>The name the page carries when the menu names none (the product's default of the menu item, 02 3.28.4).</summary>
    public const string DefaultTitle = "Sık sorulan sorular";

    /// <summary>The sentence of the page opened directly while no question is published (04 5.9.8).</summary>
    public const string EmptyText = "Henüz yayında soru yok.";

    /// <summary>True when the list holds a draft question, which only the administrator's preview does: the page is then closed to the search engines and carries no structured data (05 3.7.2).</summary>
    public bool HasDraft => Questions.Any(q => q.IsDraft);

    /// <summary>The page's name is the menu's name of the FAQ item (04 5.9.3), or the default while the frame has none.</summary>
    public static string TitleOf(StorefrontFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        string? name = frame.Menu?.Items.FirstOrDefault(i => i.Kind == MenuItemKind.FaqItems)?.Name;
        return string.IsNullOrWhiteSpace(name) ? DefaultTitle : name.Trim();
    }

    /// <summary>The page of a title and the module's questions.</summary>
    public static FaqPage Of(string title, IReadOnlyList<FaqQuestionView> questions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(questions);
        return new FaqPage(title, questions, questions.Count == 0 ? new EmptyState(EmptyText, "Ana sayfaya dön", FramePaths.Home) : null);
    }
}
