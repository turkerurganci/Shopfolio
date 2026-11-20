using Microsoft.AspNetCore.Mvc.Filters;

using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// Marks the page that shows the forms a reauthentication permission is bound to - the customer's account page (07 UC-63) and the administrator's (07 UC-104): the page decides
/// itself whether the permission stays (its GET opens the form only for the presented value and drops the permission otherwise; its POST takes the value). No other page carries it.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class ReauthenticationFormPageAttribute : Attribute
{
}

/// <summary>
/// The one layer that drops the reauthentication permission at the session's first page request that is not the display of the permission's form (05 6.1.5; K-971; D-1454,
/// D-1389): every page of the storefront for the customer's session, every page of the panel for the administrator's. The page that shows the form
/// (<see cref="ReauthenticationFormPageAttribute"/>) is left to its own rule. The permission is dropped before the handler runs, so a page whose handler grants a new
/// permission - the reauthentication screen's POST, Google's return - keeps the one it granted. The authentication handler marks a session that holds a permission
/// (<see cref="MarkHeld"/>), so a request of a session without one asks the database nothing more.
/// </summary>
internal sealed class ReauthenticationPermissionFilter : IAsyncPageFilter, IOrderedFilter
{
    /// <summary>Inside the error layer (-300) and the frames' filter (-150), outside the forgery check (-100): a page shown again by the check is a page request too.</summary>
    public const int FilterOrder = -120;

    private const string ItemKeyPrefix = "Shopfolio.ReauthenticationHeld.";

    private const string PanelFolderPrefix = SessionSchemes.PanelFolder + "/";

    private readonly SessionStore _sessions;
    private readonly ITransactionRunner _transactions;

    public ReauthenticationPermissionFilter(SessionStore sessions, ITransactionRunner transactions)
    {
        ArgumentNullException.ThrowIfNull(sessions);
        ArgumentNullException.ThrowIfNull(transactions);
        _sessions = sessions;
        _transactions = transactions;
    }

    public int Order => FilterOrder;

    /// <summary>Notes for the request that the session the scheme authenticated holds a permission (the authentication handler only).</summary>
    internal static void MarkHeld(HttpContext context, string scheme, Guid sessionId, Guid accountId)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Items[ItemKeyPrefix + scheme] = new HeldPermission(sessionId, accountId);
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        bool formPage = context.ActionDescriptor.ModelTypeInfo?.IsDefined(typeof(ReauthenticationFormPageAttribute), inherit: false) ?? false;
        if (!formPage)
        {
            bool panel = context.ActionDescriptor.ViewEnginePath.StartsWith(PanelFolderPrefix, StringComparison.OrdinalIgnoreCase);
            (string scheme, AccountType type) = panel ? (SessionSchemes.AdminScheme, AccountType.Admin) : (SessionSchemes.CustomerScheme, AccountType.Customer);

            // Only the area's own session: a storefront page does not touch the panel session's permission, nor a panel page the customer's.
            if (context.HttpContext.Items.TryGetValue(ItemKeyPrefix + scheme, out object? value) && value is HeldPermission held)
            {
                await _transactions.RunAsync(
                    async cancel =>
                    {
                        await _sessions.DropReauthenticationAsync(type, held.AccountId, held.SessionId, cancel);
                        return Dropped.Instance;
                    },
                    context.HttpContext.RequestAborted);
                context.HttpContext.Items.Remove(ItemKeyPrefix + scheme);
            }
        }

        await next();
    }

    private sealed record HeldPermission(Guid SessionId, Guid AccountId);

    private sealed record Dropped : IOperationResult
    {
        public static readonly Dropped Instance = new();

        public bool IsRejected => false;
    }
}
