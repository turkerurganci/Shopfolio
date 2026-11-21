using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

using Shopfolio.Infrastructure.Security;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// T604 D-1843 (07 2.5.4; 04 2.7.3.1): the forgery check's 400 on the member record shows the record of the e-mail the form carried through the one hook (<see cref="IReadsOnForgeryRefusal"/>) - the
/// read the 422 makes - and deletes nothing.
/// </summary>
public sealed partial class MemberRecordPageTests
{
    [Fact]
    public async Task TheForgeryRefusal_OfTheDeletion_ShowsTheSearchedRecord_AndDeletesNothing_D1843()
    {
        Guid customer = await SeedCustomerAsync("d1843@example.com", "Deniz Uye", verified: true);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await ForgedPosts.SendAsync(
            client,
            DeletePath,
            [("CustomerId", customer.ToString()), ("SearchEmail", "d1843@example.com"), ("Confirmed", "true")]);

        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.Contains("Deniz Uye", html, StringComparison.Ordinal);
        Assert.Contains("value=\"d1843@example.com\"", html, StringComparison.Ordinal);
        Assert.Contains(ForgeryProtectionFilter.Message, html, StringComparison.Ordinal);
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM customers WHERE id = '{customer}'"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs WHERE operation = 'DeleteCustomerAccount'"));
    }
}
