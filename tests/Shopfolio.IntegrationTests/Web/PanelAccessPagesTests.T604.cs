using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

using Shopfolio.Infrastructure.Errors;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>T604 D-1317 (07 UC-112, UC-113; 09 8.2 item 3): the panel's reset and invitation pages answer a link that does not hold with 200 and write its rejection line.</summary>
public sealed partial class PanelAccessPagesTests
{
    [Fact]
    public async Task TheResetAndTheInvitation_OnALinkThatDoesNotHold_Are200_AndWriteTheLinkNoLongerValidLine_D1317()
    {
        await SeedAdminAsync();
        CollectingLoggerProvider logs = new();
        await using WebApplicationFactory<Program> factory = Open().Collecting(logs);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode reset, _, _) = await PostWithBodyAsync(client, $"{ResetPath}/unknown-key?handler=ResetPassword", [("Input.NewPassword", Replacement)]);
        Assert.Equal(1, logs.Count(ErrorCode.LinkNoLongerValid));
        (HttpStatusCode invite, _, _) = await PostWithBodyAsync(client, $"{InvitePath}/unknown-key?handler=AcceptAdminInvitation", [("Input.Name", "Yeni Yönetici"), ("Input.Password", Strong)]);

        Assert.Equal(HttpStatusCode.OK, reset);
        Assert.Equal(HttpStatusCode.OK, invite);
        Assert.Equal(2, logs.Count(ErrorCode.LinkNoLongerValid));
    }
}
