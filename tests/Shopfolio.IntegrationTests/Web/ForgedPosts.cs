using System.Net;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>A POST without the anti-forgery value (T604 D-1843): the forgery check's 400 and the page it shows again (07 2.5.4).</summary>
internal static class ForgedPosts
{
    public static async Task<(HttpStatusCode Status, string Html)> SendAsync(HttpClient client, string path, IEnumerable<(string Name, string Value)> fields, params string[] cookies)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(fields);
        using HttpRequestMessage request = new(HttpMethod.Post, path) { Content = new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value))) };
        request.Headers.Add(ForgeryTokenHandler.SkipHeader, "1");
        if (cookies.Length > 0)
        {
            request.Headers.Add("Cookie", string.Join("; ", cookies));
        }

        CancellationToken ct = TestContext.Current.CancellationToken;
        using HttpResponseMessage response = await client.SendAsync(request, ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(ct)));
    }
}
