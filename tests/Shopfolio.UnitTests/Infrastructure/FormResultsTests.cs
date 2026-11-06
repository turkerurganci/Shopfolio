using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Net.Http.Headers;

using Shopfolio.Infrastructure.Forms;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The results of a page's POST handler (T206 criterion 7; KK7; 07 2.2.2, 2.7.1): a writing POST ends in 303 to a GET, a POST that writes nothing is
/// shown again with 422, and the export is 200 and a CSV. The end-to-end behaviour on a page is in <c>PageFormPatternTests</c>.
/// </summary>
public sealed class FormResultsTests
{
    private static ActionContext Context()
    {
        DefaultHttpContext http = new();
        http.Response.Body = new MemoryStream();
        return new ActionContext { HttpContext = http };
    }

    [Fact]
    public async Task SeeOther_Answers303WithTheLocation()
    {
        ActionContext context = Context();

        await FormResults.SeeOther("/sepet").ExecuteResultAsync(context);

        Assert.Equal(StatusCodes.Status303SeeOther, context.HttpContext.Response.StatusCode);
        Assert.Equal("/sepet", context.HttpContext.Response.Headers.Location.ToString());
    }

    [Fact]
    public async Task SeeOther_KeepsTheQueryOfTheTarget()
    {
        ActionContext context = Context();

        await FormResults.SeeOther("/sepet?notice=added").ExecuteResultAsync(context);

        Assert.Equal("/sepet?notice=added", context.HttpContext.Response.Headers.Location.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("sepet")]
    [InlineData("//example.com")]
    [InlineData("/\\example.com")]
    [InlineData("https://example.com/sepet")]
    [InlineData("/sepet\r\nSet-Cookie: x=1")]
    [InlineData("/sepet\nX: y")]
    [InlineData("/\t/evil.example")]
    [InlineData("/\u0001x")]
    [InlineData("/\u007Fx")]
    [InlineData("/a\tb")]
    public void SeeOther_OnAnythingButASitePath_Throws(string target)
    {
        Assert.Throws<ArgumentException>(() => FormResults.SeeOther(target));
    }

    [Fact]
    public void Statuses_AreTheOnesOf07()
    {
        Assert.Equal(303, FormResults.SeeOtherStatus);
        Assert.Equal(422, FormResults.UnprocessableStatus);
    }

    [Fact]
    public void Redisplay_IsThePageWithStatus422()
    {
        PageResult result = Assert.IsType<PageResult>(FormResults.Redisplay(new BarePage()));

        Assert.Equal(422, result.StatusCode);
    }

    [Fact]
    public void Conflict_IsThePageWithStatus409_AndNotTheRedisplayOfAValidationError()
    {
        PageResult result = Assert.IsType<PageResult>(FormResults.Conflict(new BarePage()));

        Assert.Equal(409, result.StatusCode);
        Assert.Equal(StatusCodes.Status409Conflict, FormResults.ConflictStatus);
        Assert.NotEqual(FormResults.UnprocessableStatus, result.StatusCode);
    }

    [Fact]
    public async Task CsvFileResult_Answers200WithTheFileAsAnAttachmentThatIsNotStoredOrIndexed()
    {
        ActionContext context = Context();
        byte[] content = "a;b\n1;2\n"u8.ToArray();

        await new CsvFileResult("siparisler-2026-11-01-2026-11-05.csv", content).ExecuteResultAsync(context);

        HttpResponse response = context.HttpContext.Response;
        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.Equal("text/csv; charset=utf-8", response.ContentType);
        ContentDispositionHeaderValue disposition = ContentDispositionHeaderValue.Parse(response.Headers.ContentDisposition.ToString());
        Assert.Equal("attachment", disposition.DispositionType.Value);
        Assert.Equal("siparisler-2026-11-01-2026-11-05.csv", disposition.FileName.Value);
        Assert.Equal("no-store", response.Headers.CacheControl.ToString());
        Assert.Equal("noindex", response.Headers["X-Robots-Tag"].ToString());
        Assert.Equal(content.Length, response.ContentLength);
        Assert.Null(response.Headers.Location.FirstOrDefault());
        response.Body.Position = 0;
        using MemoryStream copy = new();
        await response.Body.CopyToAsync(copy, TestContext.Current.CancellationToken);
        Assert.Equal(content, copy.ToArray());
    }

    [Theory]
    [InlineData("siparisler.txt")]
    [InlineData("siparisler")]
    [InlineData("Siparisler.csv")]
    [InlineData("siparişler.csv")]
    [InlineData("siparisler 2026.csv")]
    [InlineData("../siparisler.csv")]
    [InlineData(".csv")]
    public void CsvFileResult_OnAFileNameThatIsNotATurkishAsciiSegmentWithCsv_Throws(string fileName)
    {
        Assert.Throws<ArgumentException>(() => new CsvFileResult(fileName, []));
    }

    private sealed class BarePage : PageModel
    {
    }
}
