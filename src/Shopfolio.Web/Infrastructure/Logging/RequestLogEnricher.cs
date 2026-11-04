using System.Security.Claims;

using Serilog.Core;
using Serilog.Events;

using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Logging;

/// <summary>
/// Adds the request fields to a line (09 18.2 item 1): <c>requestId</c>, <c>method</c>, <c>path</c> (masked), <c>accountType</c> and <c>accountId</c>.
/// It reads the current request through <see cref="IHttpContextAccessor"/> at the moment the line is written — no <c>LogContext</c> scope in
/// the middleware, because that scope is closed in the outer exception handler. A line outside a request (a background job, the start) gets none
/// of the fields.
/// </summary>
internal sealed class RequestLogEnricher : ILogEventEnricher
{
    /// <summary>The header the reverse proxy sets on every request; the access log carries the same value (09 18.2 item 1; K-1354).</summary>
    public const string RequestIdHeader = "X-Request-Id";

    /// <summary>The claims that carry the account on the request; the sign-in task sets them (the names are this task's).</summary>
    public const string AccountTypeClaim = "shopfolio:account-type";

    public const string AccountIdClaim = "shopfolio:account-id";

    // A value of the header that is not plausible (long, or with characters an identifier does not have) is not written.
    private const int MaxRequestIdLength = 128;

    private readonly IHttpContextAccessor _accessor;

    public RequestLogEnricher(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        ArgumentNullException.ThrowIfNull(propertyFactory);

        HttpContext? context = _accessor.HttpContext;
        if (context is null)
        {
            return;
        }

        string? requestId = context.Request.Headers[RequestIdHeader].FirstOrDefault();
        if (IsPlausibleRequestId(requestId))
        {
            Add(logEvent, propertyFactory, "requestId", requestId);
        }

        Add(logEvent, propertyFactory, "method", context.Request.Method);
        Add(logEvent, propertyFactory, "path", PathMasker.Mask(context.Request.Path.Value + context.Request.QueryString.Value));

        ClaimsPrincipal? user = context.User;
        string? accountType = user?.FindFirst(AccountTypeClaim)?.Value;
        if (accountType is not null && Enum.TryParse(accountType, ignoreCase: false, out AccountType type) && Enum.IsDefined(type))
        {
            Add(logEvent, propertyFactory, "accountType", type.ToString());
        }

        // The id is an internal id: a value that is not one (an e-mail address) is not written.
        if (Guid.TryParse(user?.FindFirst(AccountIdClaim)?.Value, out Guid accountId))
        {
            Add(logEvent, propertyFactory, "accountId", accountId);
        }
    }

    private static bool IsPlausibleRequestId(string? value) =>
        !string.IsNullOrEmpty(value)
        && value.Length <= MaxRequestIdLength
        && value.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.');

    private static void Add(LogEvent logEvent, ILogEventPropertyFactory factory, string name, object? value) =>
        logEvent.AddPropertyIfAbsent(factory.CreateProperty(name, value));
}
