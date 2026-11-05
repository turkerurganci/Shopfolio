using System.Text.RegularExpressions;

namespace Shopfolio.Infrastructure.Binding;

/// <summary>
/// The names of the request models and of the handlers they belong to (07 5.1.5; 09 5.3 items 1-2): the model of a form is named after the form's main
/// handler and its kind - <c>AddCartItemInput</c> for <c>OnPostAddCartItemAsync</c> - and a panel model whose storefront namesake carries other fields
/// takes the <c>Admin</c> prefix. One place holds the suffixes and the handler pattern, so the rule and the code read the same.
/// </summary>
internal static partial class ModelNaming
{
    /// <summary>The suffix of a form input model.</summary>
    public const string InputSuffix = "Input";

    /// <summary>The suffix of a query model.</summary>
    public const string QuerySuffix = "Query";

    /// <summary>The suffix of a machine endpoint's request body.</summary>
    public const string RequestSuffix = "Request";

    /// <summary>The suffix of a machine endpoint's response body.</summary>
    public const string ResponseSuffix = "Response";

    /// <summary>The prefix of a panel model whose storefront namesake has other fields (07 5.1.5).</summary>
    public const string AdminPrefix = "Admin";

    /// <summary>The marker interface of the kind that a model name's suffix says, or <see langword="null"/> when the name ends with no kind.</summary>
    public static Type? MarkerOfName(string typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);
        if (typeName.EndsWith(InputSuffix, StringComparison.Ordinal))
        {
            return typeof(IFormInput);
        }

        if (typeName.EndsWith(QuerySuffix, StringComparison.Ordinal))
        {
            return typeof(IQueryModel);
        }

        if (typeName.EndsWith(RequestSuffix, StringComparison.Ordinal))
        {
            return typeof(IMachineRequest);
        }

        return typeName.EndsWith(ResponseSuffix, StringComparison.Ordinal) ? typeof(IMachineResponse) : null;
    }

    /// <summary>The kind markers a model type carries.</summary>
    public static IEnumerable<Type> MarkersOf(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        Type[] markers = [typeof(IFormInput), typeof(IQueryModel), typeof(IMachineRequest), typeof(IMachineResponse)];
        return markers.Where(m => m.IsAssignableFrom(type));
    }

    /// <summary>
    /// The handler name inside a page-model method name - <c>Add</c> in <c>OnPostAddAsync</c> - or <see langword="null"/> when the method is no named
    /// GET or POST handler (the unnamed <c>OnGet</c> and <c>OnPost</c> have no name).
    /// </summary>
    public static string? HandlerNameOf(string methodName)
    {
        ArgumentNullException.ThrowIfNull(methodName);
        if (methodName is "OnGet" or "OnGetAsync" or "OnPost" or "OnPostAsync")
        {
            return null;
        }

        Match match = HandlerPattern().Match(methodName);
        return match.Success ? match.Groups["name"].Value : null;
    }

    /// <summary>Whether a form input model's name is the one of the handler: <c>Add</c> gives <c>AddInput</c> or <c>AdminAddInput</c> (07 5.1.5).</summary>
    public static bool IsInputNameOf(string handlerName, string inputTypeName)
    {
        ArgumentNullException.ThrowIfNull(handlerName);
        ArgumentNullException.ThrowIfNull(inputTypeName);
        return inputTypeName == handlerName + InputSuffix || inputTypeName == AdminPrefix + handlerName + InputSuffix;
    }

    [GeneratedRegex(@"^On(Get|Post)(?<name>[A-Z].*?)(Async)?$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex HandlerPattern();
}
