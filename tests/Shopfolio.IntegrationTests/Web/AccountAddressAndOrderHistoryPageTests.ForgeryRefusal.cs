using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Pages;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// T604 D-1843 (07 2.5.4; 04 2.7.3.1; K-1627 (2)): the address book the forgery check's 400 shows again reads the member's own entries in the page's own wrapper and keeps the sent form open with the
/// sent values - the edit form only for an entry of this account - and nothing is written.
/// </summary>
public sealed partial class AccountAddressAndOrderHistoryPageTests
{
    [Fact]
    public async Task TheForgeryRefusal_ShowsTheOwnEntries_AndTheSentFormWithItsValues_AndWritesNothing_D1843()
    {
        (string key, Guid member) = await TestSessions.CustomerAsync(_database.Dsn);
        Guid entry = await SeedEntryAsync(member, "Ev", "Ali Veli", "Çankaya", "Atatürk Bulvarı 1", "5321234567", Now);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string before = await EntriesFingerprintAsync();

        (HttpStatusCode update, string updateHtml) = await PostWithoutTokenAsync(client, "UpdateAddressBook", UpdateFields(entry, "Yazlık", "Ali Veli", _izmir.ToString(), "Konak", "Kordon 5", ""), key);
        (HttpStatusCode foreign, string foreignHtml) = await PostWithoutTokenAsync(client, "UpdateAddressBook", UpdateFields(Guid.NewGuid(), "Yazlık", "Ali Veli", _izmir.ToString(), "Konak", "Kordon 5", ""), key);
        (HttpStatusCode delete, string deleteHtml) = await PostWithoutTokenAsync(client, "DeleteAddressBook", [("Input.AddressBookId", entry.ToString())], key);

        Assert.Equal(HttpStatusCode.BadRequest, update);
        Assert.Contains("Atatürk Bulvarı 1", updateHtml, StringComparison.Ordinal);
        Assert.Contains("Kordon 5", updateHtml, StringComparison.Ordinal);
        Assert.Contains($"name=\"Input.AddressBookId\" value=\"{entry}\"", updateHtml, StringComparison.Ordinal);
        Assert.Contains(ForgeryProtectionFilter.Message, updateHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.BadRequest, foreign);
        Assert.Contains("Atatürk Bulvarı 1", foreignHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("Kordon 5", foreignHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.BadRequest, delete);
        Assert.Contains("Atatürk Bulvarı 1", deleteHtml, StringComparison.Ordinal);
        Assert.Equal(before, await EntriesFingerprintAsync());
    }

    private static async Task<(HttpStatusCode Status, string Html)> PostWithoutTokenAsync(HttpClient client, string handler, IEnumerable<(string Name, string Value)> fields, string sessionKey)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, $"{BookPath}?handler={handler}");
        request.Headers.Add(ForgeryTokenHandler.SkipHeader, "1");
        request.Headers.TryAddWithoutValidation("Cookie", $"{CookieNames.Session}={sessionKey}");
        request.Content = new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value)));
        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }
}
