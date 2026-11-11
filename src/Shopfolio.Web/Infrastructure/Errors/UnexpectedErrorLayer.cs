using System.Collections.Concurrent;
using System.Net;

using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.Infrastructure.Errors;

/// <summary>
/// The count of unexpected errors the status endpoint reads (05 9.4.8): every error the layer handles is one entry. 422, 409 and 429 are no
/// unexpected errors and never reach the layer (07 2.7.5). The memory is the process's: a restart empties it (05 11.2.9).
/// </summary>
internal sealed class UnexpectedErrorCounter
{
    private static readonly TimeSpan _retention = TimeSpan.FromDays(1);

    private readonly TimeProvider _time;
    private readonly ConcurrentQueue<DateTimeOffset> _moments = new();

    public UnexpectedErrorCounter(TimeProvider time)
    {
        _time = time;
    }

    public void Record()
    {
        DateTimeOffset now = _time.GetUtcNow();
        _moments.Enqueue(now);
        while (_moments.TryPeek(out DateTimeOffset oldest) && now - oldest > _retention)
        {
            _moments.TryDequeue(out _);
        }
    }

    /// <summary>The number of errors within the last <paramref name="window"/> (the status endpoint passes its fifteen minutes).</summary>
    public int CountWithin(TimeSpan window)
    {
        DateTimeOffset from = _time.GetUtcNow() - window;
        return _moments.Count(moment => moment >= from);
    }
}

/// <summary>What the layer decided for one error: the state and, for an uncertain outcome, the screen that shows the result.</summary>
internal sealed record ErrorResolution(ErrorState State, string? Target, bool ClientGone = false, int? ClientStatus = null);

/// <summary>
/// The single catching layer (09 8.1 items 2, 3, 5-8; 05 10.3). Both entry points of the framework - the page filter around the handler and the
/// middleware around the rest of the pipeline - call <see cref="Resolve"/> and neither decides anything: the layer turns the exception into a state,
/// records it (one error line, the counter) and the entry point writes the answer of that state. The layer never commits a transaction: an
/// uncommitted operation is rolled back by the exception that left it (09 8.1 item 5). The request's abort is no error (09 8.1 item 8).
/// </summary>
internal sealed partial class UnexpectedErrorLayer
{
    private readonly ILogger<UnexpectedErrorLayer> _logger;
    private readonly UnexpectedErrorCounter _counter;

    public UnexpectedErrorLayer(ILogger<UnexpectedErrorLayer> logger, UnexpectedErrorCounter counter)
    {
        _logger = logger;
        _counter = counter;
    }

    public ErrorResolution Resolve(HttpContext context, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(exception);

        if (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
        {
            return new ErrorResolution(ErrorState.PageLevel, null, ClientGone: true);
        }

        // A client's own fault is no unexpected error (D-147): a reset connection has nobody to answer, a malformed request gets its own status.
        // Neither is logged as an error line nor counted.
        if (exception is Microsoft.AspNetCore.Connections.ConnectionResetException)
        {
            return new ErrorResolution(ErrorState.PageLevel, null, ClientGone: true);
        }

        if (exception is BadHttpRequestException badRequest)
        {
            return new ErrorResolution(ErrorState.PageLevel, null, ClientStatus: badRequest.StatusCode);
        }

        OperationMark? mark = context.Items.TryGetValue(OperationMark.ItemKey, out object? stored) ? stored as OperationMark : null;
        bool machine = context.GetEndpoint()?.Metadata.GetMetadata<MachineEndpointMetadata>() is not null;
        ErrorState state = ErrorClassifier.Classify(context.Request.Method, machine, mark is not null);

        LogUnexpected(_logger, exception, LogEvents.UnexpectedError, ErrorClassifier.CodeOf(state).ToString(), state.ToString(), state == ErrorState.OutcomeUncertain ? mark?.Operation : null);
        _counter.Record();

        string? target = state == ErrorState.OutcomeUncertain ? TargetOf(context, mark) : null;
        return new ErrorResolution(state, target);
    }

    private static string TargetOf(HttpContext context, OperationMark? mark)
    {
        string? chosen = mark?.Target?.OutcomeScreen();
        if (chosen is not null && FormResults.IsLocalPath(chosen))
        {
            return chosen;
        }

        string own = context.Request.Path.ToUriComponent();
        return FormResults.IsLocalPath(own) ? own : "/";
    }

    // The line carries types, never the message (09 8.1 item 7; 18.2 item 2c): the formatter writes the exception's fields from the exception itself.
    [LoggerMessage(Level = LogLevel.Error, Message = "{Event} {Code} {State} {Operation}")]
    private static partial void LogUnexpected(ILogger logger, Exception exception, string @event, string code, string state, string? operation);
}

/// <summary>
/// The answer of a state, written for the requests the layer's middleware catches (09 8.1 item 2b) and for the page filter's page-level error. The
/// markup is the plainest error page: the storefront's and the panel's frames come with their layouts (D-135). Nothing here writes a cookie
/// (09 15.1 item 6) and nothing carries a code, an id or a technical detail (07 2.7.3). If the page itself cannot be written the answer is the
/// framework's plainest 500 (05 10.3.6).
/// </summary>
internal static class UnexpectedErrorAnswer
{
    public const string PageMessage = "Sayfa şu anda açılamadı. Birkaç dakika sonra yeniden deneyin. Sorun sürerse İletişim sayfasından bize ulaşın.";

    public const string OperationMessage = "İşlem tamamlanamadı. Yazdıklarınız duruyor; aynı düğmeyle yeniden deneyin. Sorun sürerse İletişim sayfasından bize ulaşın.";

    public const string PanelOperationMessage = "İşlem tamamlanamadı. Yazdıklarınız duruyor; aynı düğmeyle yeniden deneyin. Sorun sürerse kurulumu yapan kişiye bildirin.";

    private static readonly PathString _panelPath = new("/panel");

    /// <summary>The title of the storefront's error page (04 2.7.3.2); the page is not "Sayfa bulunamadı" (E-06).</summary>
    public const string PageTitle = "Sayfa açılamadı";

    public const string PanelTitle = "Panel";

    /// <summary>The panel's page-level message points the administrator to the person who installed the product (04 2.7.3.4).</summary>
    public const string PanelPageMessage = "Sayfa şu anda açılamadı. Birkaç dakika sonra yeniden deneyin. Sorun sürerse kurulumu yapan kişiye bildirin.";

    /// <summary>The page-level message of the request's surface: the customer is pointed to the contact page, the administrator to the installer (04 2.7.3.4).</summary>
    public static string PageMessageFor(HttpRequest request) => IsPanel(request) ? PanelPageMessage : PageMessage;

    // The framed page is the first try; any failure of drawing it - the frame's own reads included - leaves the plain page below (05 10.3.6).
#pragma warning disable CA1031 // 09 8.1 item 6: this is inside the single catching layer, the first of the six places of the wide catch.
    private static async Task<bool> TryRenderFramedAsync(HttpContext context, IErrorPageRenderer renderer, ErrorPageText page)
    {
        try
        {
            await renderer.RenderAsync(context, page);
            return true;
        }
        catch (Exception) when (!context.Response.HasStarted)
        {
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.Headers.CacheControl = Security.SecurityHeaders.NoStore;
            return false;
        }
    }
#pragma warning restore CA1031

    public static bool IsPanel(HttpRequest request) => request.Path.StartsWithSegments(_panelPath, StringComparison.OrdinalIgnoreCase);

    /// <summary>The lasting message of an operation-level error, for the model state of the page shown again (04 2.7.3.1, 2.7.3.4).</summary>
    public static string OperationMessageFor(HttpRequest request) => IsPanel(request) ? PanelOperationMessage : OperationMessage;

    public static async Task WriteAsync(HttpContext context, ErrorResolution resolution)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(resolution);
        HttpResponse response = context.Response;
        if (resolution.ClientGone || response.HasStarted)
        {
            // Nothing more can be said to this client: a started answer cannot be replaced, a gone client does not read one.
            context.Abort();
            return;
        }

        if (resolution.ClientStatus is { } clientStatus)
        {
            response.StatusCode = clientStatus;
            response.ContentType = null;
            response.ContentLength = null;
            response.Headers.CacheControl = Security.SecurityHeaders.NoStore;
            return;
        }

        try
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.ContentType = null;
            response.ContentLength = null;
            response.Headers.CacheControl = Security.SecurityHeaders.NoStore;
            switch (resolution.State)
            {
                case ErrorState.MachineEndpoint:
                    return;
                case ErrorState.OutcomeUncertain:
                    FormResults.WriteSeeOther(response, resolution.Target ?? "/");
                    return;
                default:
                    await WritePageAsync(context, resolution.State);
                    return;
            }
        }
#pragma warning disable CA1031 // 09 8.1 item 6: the single catching layer is the first place a wide catch is allowed; its own failure is the plainest 500 (05 10.3.6).
        catch (Exception)
        {
            if (!response.HasStarted)
            {
                response.Headers.Clear();
                response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }
#pragma warning restore CA1031
    }

    private static async Task WritePageAsync(HttpContext context, ErrorState state)
    {
        bool panel = IsPanel(context.Request);
        string message = state == ErrorState.PageLevel ? PageMessageFor(context.Request) : OperationMessageFor(context.Request);
        string framedTitle = panel ? PanelTitle : PageTitle;
        if (context.RequestServices.GetService(typeof(IErrorPageRenderer)) is IErrorPageRenderer renderer && await TryRenderFramedAsync(context, renderer, new ErrorPageText(framedTitle, message)))
        {
            return;
        }

        string retry = state == ErrorState.PageLevel
            ? $"<a href=\"{WebUtility.HtmlEncode(context.Request.Path.ToUriComponent())}\">Yeniden dene</a> "
            : string.Empty;
        string home = panel ? "/panel" : "/";
        string title = framedTitle;
        string html =
            "<!DOCTYPE html><html lang=\"tr\"><head><meta charset=\"utf-8\"><title>" + title + "</title></head><body><main>" +
            "<h1>" + title + "</h1><p>" + WebUtility.HtmlEncode(message) + "</p><p>" + retry + "<a href=\"" + home + "\">Ana sayfaya dön</a></p></main></body></html>";
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(html, context.RequestAborted);
    }
}
