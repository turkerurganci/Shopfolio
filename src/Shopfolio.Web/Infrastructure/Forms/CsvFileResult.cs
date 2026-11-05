using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

using Shopfolio.Infrastructure.Routing;

namespace Shopfolio.Infrastructure.Forms;

/// <summary>
/// The answer of a successful export (07 2.2.2; K-1195): the one writing POST that does not redirect. The status is 200, the body is the file
/// and it comes down as an attachment; the screen does not change. The headers are the ones K-1195 (2) lists for the file itself: the type
/// <c>text/csv; charset=utf-8</c>, <c>Content-Disposition: attachment</c> with the file name, <c>Cache-Control: no-store</c> (the file carries personal data)
/// and <c>X-Robots-Tag: noindex</c>; the security headers are the common middleware's (09 15.2 item 3). The file name is Turkish ASCII in the form of a
/// path segment (<c>siparisler-2026-11-01-2026-11-05.csv</c>), so no encoding of the name is needed. The inner format of the CSV and the order of
/// the trace row and the file belong to the export task.
/// </summary>
internal sealed class CsvFileResult : IActionResult
{
    public const string ContentType = "text/csv; charset=utf-8";

    private const string Extension = ".csv";

    private readonly byte[] _content;

    public CsvFileResult(string fileName, byte[] content)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(content);
        if (!fileName.EndsWith(Extension, StringComparison.Ordinal) || !AddressRules.IsValidSegment(fileName[..^Extension.Length]))
        {
            throw new ArgumentException("The file name is a Turkish ASCII name in the form of a path segment that ends in .csv (K-1195).", nameof(fileName));
        }

        FileName = fileName;
        _content = content;
    }

    public string FileName { get; }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        HttpResponse response = context.HttpContext.Response;
        response.StatusCode = StatusCodes.Status200OK;
        response.ContentType = ContentType;
        response.ContentLength = _content.Length;
        response.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = FileName }.ToString();
        response.Headers.CacheControl = "no-store";
        response.Headers["X-Robots-Tag"] = "noindex";
        await response.Body.WriteAsync(_content, context.HttpContext.RequestAborted);
    }
}
