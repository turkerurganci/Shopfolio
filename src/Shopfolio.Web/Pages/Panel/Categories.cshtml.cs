using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Categories;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Panel;

/// <summary>The form of UC-118 (07 9.6.2; 04 7.2.5.1): the name and the parent - empty at the first level (06 4.9).</summary>
public sealed class CreateCategoryInput : IFormInput
{
    [Display(Name = "Ad")]
    [FieldRequired]
    [TextTooLong(ProductConstants.ShortTextMaxLength)]
    public string? Name { get; set; }

    public Guid? ParentCategoryId { get; set; }
}

/// <summary>The form of UC-119 (07 9.6.3; 04 7.2.5.1): the category's internal key and the new name.</summary>
public sealed class RenameCategoryInput : IFormInput
{
    public Guid CategoryId { get; set; }

    [Display(Name = "Ad")]
    [FieldRequired]
    [TextTooLong(ProductConstants.ShortTextMaxLength)]
    public string? Name { get; set; }
}

/// <summary>The form of UC-120 (07 9.6.4; 04 7.2.5.2): the category's internal key and the new parent - empty for the first level.</summary>
public sealed class MoveCategoryInput : IFormInput
{
    public Guid CategoryId { get; set; }

    public Guid? ParentCategoryId { get; set; }
}

/// <summary>The form of UC-121 (07 9.6.5): the category's internal key. No confirmation (04 2.4.2).</summary>
public sealed class DeleteCategoryInput : IFormInput
{
    public Guid CategoryId { get; set; }
}

/// <summary>A place the node's move can go to (04 9.6.6): the first level (no key) or another category, indented by its level in the list's text.</summary>
public sealed record CategoryMoveOption(string Value, string Text, bool IsCurrent);

/// <summary>
/// One node of the tree as the screen draws it (04 9.6.3): the name, the level, the counts line and - when no published product is in it or in a branch below it - the line that
/// says the storefront menu does not show it (02 3.28.6). Whether "Alt kategori ekle" stands is the server's: not on the third level (P-20). The node's own message is the refusal
/// of the last operation on it, said in place (04 9.6.6, 9.6.7; K-799); the name a refused rename or child sent comes back in its field.
/// </summary>
public sealed record CategoryNodeView(
    Guid Id,
    string Name,
    int Level,
    string CountsText,
    string? MenuHiddenText,
    bool CanAddChild,
    IReadOnlyList<CategoryMoveOption> MoveOptions,
    string? Message,
    string? RenameMessage,
    string? RenameValue,
    string? ChildMessage,
    string? ChildValue);

/// <summary>
/// E-34, the category tree (T526; 04 9.6; 07 UC-89, UC-118...UC-121): at most three levels, the siblings in alphabetical order (K-805), every node with its counts, its menu line
/// and its operations - add a sub-category (not on the third level), rename, move, delete. The GET writes nothing; each operation is a named POST handler of the module's write
/// methods, one transaction each (09 9.1), and the module takes the tree's advisory lock first (05 7.3.5; K-1089). No operation asks a confirmation (04 2.4.2). A refusal is
/// said in place on its node with K-799's text, 422; a key that names no category is 404.
/// </summary>
internal sealed class CategoriesModel : PageModel
{
    public const string NewCategoryLabel = "Yeni kategori";

    public const string AddChildLabel = "Alt kategori ekle";

    public const string RenameLabel = "Yeniden adlandır";

    public const string MoveLabel = "Taşı";

    public const string DeleteLabel = "Sil";

    public const string RootOptionText = "Birinci seviye";

    /// <summary>The empty state (04 9.6.10): no category yet, and a product goes on sale only with a primary category (02 3.7.3).</summary>
    public const string EmptySentence = "Henüz kategori yok. Ürün, ana kategorisi olmadan yayına alınamaz; önce bir kategori açın.";

    /// <summary>02 3.28.6's line on a node with no published product in itself or below it.</summary>
    public const string MenuHiddenText = "Yayında ürünü olmadığı için vitrin menüsünde görünmüyor.";

    /// <summary>K-799's texts (04 7.2.5.2, 9.6.6, 9.6.7).</summary>
    public const string DepthExceededText = "Bu taşıma üç seviyeyi aşan bir dal oluşturur.";

    public const string IntoOwnSubtreeText = "Kategori kendi alt dalına taşınamaz.";

    /// <summary>The refusal of a sub-category under a third-level node (07 UC-118; P-20); the screen draws no such button, so only a hand-made send reaches it.</summary>
    public const string ThirdLevelText = "Üçüncü seviyedeki kategorinin altına kategori açılamaz.";

    /// <summary>The element id of the new category form; the empty state's button opens it.</summary>
    public const string NewCategoryFormId = "yeni-kategori";

    private const string NameField = "Input.Name";

    private readonly ICatalogModule _catalog;
    private readonly ITransactionRunner _transactions;
    private readonly RejectionLog _rejections;

    private bool _loaded;
    private string? _failedHandler;
    private Guid? _failedNode;
    private string? _failedMessage;
    private string? _sentName;

    public CategoriesModel(ICatalogModule catalog, ITransactionRunner transactions, RejectionLog rejections)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentNullException.ThrowIfNull(rejections);
        _catalog = catalog;
        _transactions = transactions;
        _rejections = rejections;
    }

    /// <summary>The tree, depth first: a node, then its sub-categories, alphabetical at every level (02 3.4.1).</summary>
    public IReadOnlyList<CategoryNodeView> Nodes { get; private set; } = [];

    public EmptyStateView? Empty { get; private set; }

    public MessageView? Notice { get; private set; }

    /// <summary>The field message of the new category form after a refused send; null otherwise.</summary>
    public string? NewCategoryMessage { get; private set; }

    /// <summary>The name the new category form shows again after a refused send.</summary>
    public string? NewCategoryValue { get; private set; }

    /// <summary>
    /// A form that fails its field rules is shown again by the validation filter without the handler (09 3.4 item 3): the screen still needs the tree, and the refusal is drawn on the
    /// form that sent it - the page model's own filter runs before the project's page filter, so the 422 reaches here once the filter has answered.
    /// </summary>
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        PageHandlerExecutedContext executed = await next();
        if (!_loaded && executed.Exception is null && executed.Result is PageResult { StatusCode: StatusCodes.Status422UnprocessableEntity })
        {
            object? input = context.HandlerArguments.Values.FirstOrDefault(v => v is IFormInput);
            (string handler, Guid? node, string? name) = input switch
            {
                CreateCategoryInput create => (create.ParentCategoryId is null ? "CreateCategory" : "CreateChild", create.ParentCategoryId, create.Name),
                RenameCategoryInput rename => ("RenameCategory", rename.CategoryId, (string?)rename.Name),
                _ => (string.Empty, (Guid?)null, (string?)null),
            };
            Fail(handler, node, ModelState.TryGetValue(NameField, out Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateEntry? entry) && entry.Errors.Count > 0 ? entry.Errors[0].ErrorMessage : null, name);
            await LoadAsync(HttpContext.RequestAborted);
        }
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        await LoadAsync(ct);
        return Page();
    }

    /// <summary>UC-118: opens a category at the first level or under a node. 303 to the tree with the short-lived notice; a fourth level is 422 in place; a missing parent is 404.</summary>
    public async Task<IActionResult> OnPostCreateCategoryAsync(CreateCategoryInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        CategoryWriteResult result = await _transactions.RunAsync(token => _catalog.CreateCategoryAsync(input.Name, input.ParentCategoryId, token), ct);
        return await AnswerAsync(result, input.ParentCategoryId is null ? "CreateCategory" : "CreateChild", input.ParentCategoryId, input.Name, NoticeCode.Saved, ct);
    }

    /// <summary>UC-119: renames a category; its address stays. 303 with the short-lived notice; a blank name is 422; a missing category is 404.</summary>
    public async Task<IActionResult> OnPostRenameCategoryAsync(RenameCategoryInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        CategoryWriteResult result = await _transactions.RunAsync(token => _catalog.RenameCategoryAsync(input.CategoryId, input.Name, token), ct);
        return await AnswerAsync(result, "RenameCategory", input.CategoryId, input.Name, NoticeCode.Saved, ct);
    }

    /// <summary>UC-120: moves a category with its branch. 303, the result is in the tree; past three levels or into its own branch is 422 in place; a missing category or parent is 404.</summary>
    public async Task<IActionResult> OnPostMoveCategoryAsync(MoveCategoryInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        CategoryWriteResult result = await _transactions.RunAsync(token => _catalog.MoveCategoryAsync(input.CategoryId, input.ParentCategoryId, token), ct);
        return await AnswerAsync(result, "MoveCategory", input.CategoryId, null, null, ct);
    }

    /// <summary>UC-121: deletes an empty category without a confirmation. 303; a category with products or sub-categories is 422 with their counts; a missing one is 404.</summary>
    public async Task<IActionResult> OnPostDeleteCategoryAsync(DeleteCategoryInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        CategoryWriteResult result = await _transactions.RunAsync(token => _catalog.DeleteCategoryAsync(input.CategoryId, token), ct);
        return await AnswerAsync(result, "DeleteCategory", input.CategoryId, null, null, ct);
    }

    /// <summary>K-799's text of a blocked delete (02 3.4.5; 04 9.6.7): the blocking counts and the way out.</summary>
    public static string NotEmptyText(int products, int children) =>
        $"Kategori silinemez: içinde {DisplayFormat.Number(products)} ürün ve {DisplayFormat.Number(children)} alt kategori var. Önce onları taşıyın.";

    /// <summary>The counts line of a node (04 9.6.3): the products hung directly on it and its sub-categories.</summary>
    public static string CountsTextOf(CategoryNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return $"{DisplayFormat.Number(node.ProductCount)} ürün · {DisplayFormat.Number(node.ChildCount)} alt kategori";
    }

    private async Task<IActionResult> AnswerAsync(CategoryWriteResult result, string handler, Guid? node, string? name, NoticeCode? notice, CancellationToken ct)
    {
        switch (result.Outcome)
        {
            case CategoryWriteOutcome.Written:
                return notice is { } code ? FormResults.SeeOther(PanelFramePaths.Categories, code) : FormResults.SeeOther(PanelFramePaths.Categories);
            case CategoryWriteOutcome.NotFound:
            case CategoryWriteOutcome.ParentNotFound:
                return NotFound();
            case CategoryWriteOutcome.NameMissing:
                _rejections.Rejected(ErrorCode.FieldRequired);
                Fail(handler, node, FieldMessageCatalog.Build(FieldCodes.FieldRequired, "Ad", null), name);
                break;
            case CategoryWriteOutcome.DepthExceeded:
                _rejections.Rejected(ErrorCode.CategoryDepthExceeded);
                Fail(handler, node, handler == "MoveCategory" ? DepthExceededText : ThirdLevelText, name);
                break;
            case CategoryWriteOutcome.IntoOwnSubtree:
                _rejections.Rejected(ErrorCode.CategoryDepthExceeded);
                Fail(handler, node, IntoOwnSubtreeText, name);
                break;
            case CategoryWriteOutcome.NotEmpty:
                _rejections.Rejected(ErrorCode.CategoryNotEmpty);
                Fail(handler, node, NotEmptyText(result.BlockingProducts, result.BlockingChildren), name);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result.Outcome, "A category write has one of seven outcomes.");
        }

        await LoadAsync(ct);
        return FormResults.Redisplay(this);
    }

    private void Fail(string handler, Guid? node, string? message, string? name)
    {
        _failedHandler = handler;
        _failedNode = node;
        _failedMessage = message;
        _sentName = name;
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        _loaded = true;
        IReadOnlyList<CategoryNode> tree = await _catalog.GetCategoryTreeAsync(ct);
        CatalogMenu menu = await _catalog.GetMenuAsync(ct);
        HashSet<Guid> listed = [.. menu.Categories.Select(c => c.Id)];
        Nodes = [.. tree.Select(n => ViewOf(n, tree, listed))];
        Empty = tree.Count == 0 ? EmptyStates.WithFirstRecordButton(EmptySentence, NewCategoryLabel, PanelFramePaths.Categories + "#" + NewCategoryFormId) : null;
        if (_failedHandler == "CreateCategory")
        {
            NewCategoryMessage = _failedMessage;
            NewCategoryValue = _sentName;
        }

        NoticeCode? notice = NoticeParameter.Read(Request, NoticeKind.ShortLived);
        Notice = notice is { } code && NoticeTexts.TryShortLived(code, out string? text) ? Messages.ShortLivedSuccess(text) : null;
    }

    private CategoryNodeView ViewOf(CategoryNode node, IReadOnlyList<CategoryNode> tree, HashSet<Guid> listed)
    {
        bool failedHere = _failedNode == node.Id;
        string? Message(string handler) => failedHere && _failedHandler == handler ? _failedMessage : null;
        string? Value(string handler) => failedHere && _failedHandler == handler ? _sentName : null;
        List<CategoryMoveOption> options = [new(string.Empty, RootOptionText, node.ParentCategoryId is null)];
        options.AddRange(tree
            .Where(t => t.Id != node.Id)
            .Select(t => new CategoryMoveOption(t.Id.ToString("D"), new string(' ', (t.Level - 1) * 2) + t.Name, t.Id == node.ParentCategoryId)));
        string? message = Message("MoveCategory") ?? Message("DeleteCategory");
        return new CategoryNodeView(
            node.Id,
            node.Name,
            node.Level,
            CountsTextOf(node),
            listed.Contains(node.Id) ? null : MenuHiddenText,
            node.Level < ProductConstants.CategoryLevelMax,
            options,
            message,
            Message("RenameCategory"),
            Value("RenameCategory"),
            Message("CreateChild"),
            Value("CreateChild"));
    }
}
