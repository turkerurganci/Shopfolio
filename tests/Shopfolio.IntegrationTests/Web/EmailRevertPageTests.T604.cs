using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Membership.Contracts;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>T604 D-1508 (07 UC-76; 09 8.2 item 3): the revert page's two refusals write their rejection line with the code - the 422 of a taken address and the 200 of a spent link.</summary>
public sealed partial class EmailRevertPageTests
{
    [Fact]
    public async Task TheRevertsRefusals_WriteTheirRejectionLines_The422OfATakenAddress_AndThe200OfASpentLink_D1508()
    {
        CollectingLoggerProvider logs = new();
        await using WebApplicationFactory<Program> factory = Open().Collecting(logs);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        Seeded seeded = await SeedAsync(factory, google: true);
        SessionOpening mine = await SignInAsync(factory, seeded.CustomerId);
        Guid other = await SeedCustomerAsync(Earlier, verified: true);

        using HttpResponseMessage taken = await PostAsync(client, Path(seeded.Key), "RevertPendingEmailChange", $"{CookieNames.Session}={mine.SessionKey}");
        Assert.Equal(1, logs.Count(ErrorCode.EmailTakenByOtherAccount));
        Assert.Equal(0, logs.Count(ErrorCode.LinkNoLongerValid));

        await using (AppDbContext db = NewContext())
        {
            db.Customers.Remove(await db.Customers.FindAsync([other], Ct) ?? throw new InvalidOperationException("seeded"));
            await db.SaveChangesAsync(Ct);
        }

        using HttpResponseMessage first = await PostAsync(client, Path(seeded.Key), "RevertPendingEmailChange", null);
        using HttpResponseMessage spent = await PostAsync(client, Path(seeded.Key), "RevertPendingEmailChange", null);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, taken.StatusCode);
        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, spent.StatusCode);
        Assert.Equal(1, logs.Count(ErrorCode.LinkNoLongerValid));
        Assert.Equal(1, logs.Count(ErrorCode.EmailTakenByOtherAccount));
    }
}
