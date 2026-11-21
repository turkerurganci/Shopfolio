using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// T604 D-1317 (07 UC-49, UC-53; 09 8.2 item 3): the landing page's 422 of a taken address and 200 of a link that does not hold, and the registration page's 303 of a value that names
/// nothing pending, each write their rejection line with the code.
/// </summary>
public sealed partial class RegistrationPageTests
{
    [Fact]
    public async Task TheLinkPagesRefusals_WriteTheirRejectionLines_TakenAddress422_LinkNoLongerValid200_AndPendingUnresolved303_D1317()
    {
        CollectingLoggerProvider logs = new();
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true).Collecting(logs);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        Guid customerId = await SeedCustomerAsync("eski@example.com", verified: true);
        string key = await RequestEmailChangeAsync(factory, customerId, "yeni@example.com");
        await SeedCustomerAsync("yeni@example.com", verified: true);
        (_, string used) = await SeedLinkedCustomerAsync(factory, "a2@example.com", LinkType.EmailVerification, verified: false, used: true);

        using HttpResponseMessage taken = await PostAsync(client, $"{FramePaths.Verification}/{key}", "VerifyEmail", []);
        Assert.Equal(1, logs.Count(ErrorCode.EmailTakenByOtherAccount));
        using HttpResponseMessage spent = await PostAsync(client, $"{FramePaths.Verification}/{used}", "VerifyEmail", []);
        using HttpResponseMessage gone = await PostAsync(client, FramePaths.Register, "ResendVerificationLink", [("PendingRegistration", "garbage")]);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, taken.StatusCode);
        Assert.Equal(HttpStatusCode.OK, spent.StatusCode);
        Assert.Equal(HttpStatusCode.SeeOther, gone.StatusCode);
        Assert.Equal(FramePaths.Register, gone.Headers.Location!.OriginalString);
        Assert.Equal(1, logs.Count(ErrorCode.EmailTakenByOtherAccount));
        Assert.Equal(1, logs.Count(ErrorCode.LinkNoLongerValid));
        Assert.Equal(1, logs.Count(ErrorCode.PendingRegistrationUnresolved));
    }
}
