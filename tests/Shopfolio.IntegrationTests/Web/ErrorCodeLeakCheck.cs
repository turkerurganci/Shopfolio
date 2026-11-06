using Shopfolio.Infrastructure.Errors;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The scan of the shown answer for the closed list of error codes (09 8.2 item 5; 07 2.7.3; T213 KK6; D-143): the body and every header of the answer -
/// the response's own headers and its content headers (<c>Content-Type</c>, <c>Content-Disposition</c> and the others live on <c>Content</c>) - name no
/// member of <see cref="ErrorCode"/>. One reader for both header sets, so no test builds the header text on its own and forgets one of them.
/// </summary>
internal static class ErrorCodeLeakCheck
{
    private static readonly string[] _codes = [.. Enum.GetNames<ErrorCode>()];

    /// <summary>The text of every header of the answer, response headers and content headers alike, one per line.</summary>
    public static string HeaderText(HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response);
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> all = response.Headers.Concat(response.Content.Headers);
        return string.Join('\n', all.Select(h => h.Key + ": " + string.Join(',', h.Value)));
    }

    /// <summary>The answer names no code: not in <paramref name="body"/>, not in a response header, not in a content header.</summary>
    public static void AssertNone(HttpResponseMessage response, string body)
    {
        ArgumentNullException.ThrowIfNull(body);
        string headers = HeaderText(response);
        foreach (string code in _codes)
        {
            Assert.DoesNotContain(code, body, StringComparison.Ordinal);
            Assert.DoesNotContain(code, headers, StringComparison.Ordinal);
        }
    }
}
