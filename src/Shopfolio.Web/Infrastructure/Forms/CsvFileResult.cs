using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

using Shopfolio.Infrastructure.Routing;

namespace Shopfolio.Infrastructure.Forms;

/// <summary>
/// The answer of a successful export (07 2.2.2; K-1195): the one writing POST that does not redirect. The status is 200, the body is the file
/// and it comes down as an attachment; the screen does not change. The headers are the ones K-1195 (2) lists for the file itself: the type
/// <c>text/csv; charset=utf-8</c>, <c>Content-Disposition: attachment</c> with the file name, <c>Cache-Control: no-store</c> (the file carries personal data)
/// and <c>X-Robots-Tag: noindex</c>; the security headers are the common middleware's (09 15.2 item 3). The file name is Turkish ASCII in the form of a
/// path segment (<c>siparisler-2026-11-01-2026-11-05.csv</c>), so no encoding of the name is needed. The body is either bytes or an open stream (T524; D-849):
/// the stream - the export's temporary file, positioned at its start - is tied to the request's life with <c>Response.RegisterForDisposeAsync</c> before it is
/// copied, so it is closed (and the file it holds deleted) when the request ends, whether the copy finished or the client left (09 10.10 item 7).
/// </summary>
internal sealed class CsvFileResult : IActionResult
{
    public const string ContentType = "text/csv; charset=utf-8";

    private const string Extension = ".csv";

    private readonly byte[]? _content;
    private readonly Stream? _stream;

    public CsvFileResult(string fileName, byte[] content)
        : this(fileName)
    {
        ArgumentNullException.ThrowIfNull(content);
        _content = content;
    }

    /// <summary>The file as an open stream at its start; the result owns it from here on and closes it with the request (09 10.10 item 7).</summary>
    public CsvFileResult(string fileName, Stream stream)
        : this(fileName)
    {
        ArgumentNullException.ThrowIfNull(stream);
        _stream = stream;
    }

    private CsvFileResult(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        if (!fileName.EndsWith(Extension, StringComparison.Ordinal) || !AddressRules.IsValidSegment(fileName[..^Extension.Length]))
        {
            throw new ArgumentException("The file name is a Turkish ASCII name in the form of a path segment that ends in .csv (K-1195).", nameof(fileName));
        }

        FileName = fileName;
    }

    public string FileName { get; }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        HttpResponse response = context.HttpContext.Response;
        if (_stream is not null)
        {
            // The stream is the request's from now on: it is closed when the request ends, even if the copy below stops half way (09 10.10 item 7).
            response.RegisterForDisposeAsync(_stream);
        }

        response.StatusCode = StatusCodes.Status200OK;
        response.ContentType = ContentType;
        response.ContentLength = _stream is not null ? _stream.Length - _stream.Position : _content!.Length;
        response.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = FileName }.ToString();
        response.Headers.CacheControl = "no-store";
        response.Headers["X-Robots-Tag"] = "noindex";
        if (_stream is not null)
        {
            await _stream.CopyToAsync(response.Body, context.HttpContext.RequestAborted);
        }
        else
        {
            await response.Body.WriteAsync(_content, context.HttpContext.RequestAborted);
        }
    }
}
