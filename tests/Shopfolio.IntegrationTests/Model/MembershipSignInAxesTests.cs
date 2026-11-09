using System.Globalization;
using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Membership.Contracts.Results;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>
/// T377 on the real module, the real counter and the real database (02 8.2.2, 8.2.4, 8.2.6): the sign-in limit L-1 counts the IP and the e-mail as two separate axes, the administrator
/// meets the same limits with the same values, the mark of a recognised browser is only the one the account's valid record matches, and the mark lives 90 days from the last sign-in.
/// </summary>
public sealed partial class MembershipTests
{
    // ---- KK1: the two axes are separate ----

    [Fact]
    public async Task TheIpAxisStopsAloneWithManyEmailsFromOneIp_AndTheEmailAxisIsNotFull_T377_KK1()
    {
        await SeedCustomerAsync("ali@example.com", verified: true);
        for (int i = 0; i < ProductConstants.SignInAttemptsPerIp; i++)
        {
            await RunAsync(m => m.CheckCredentialsAsync(string.Create(CultureInfo.InvariantCulture, $"kisi{i}@example.com"), Wrong, Ip, null, Ct));
        }

        CredentialCheckResult byTheSameIp = await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Entered, Ip, null, Ct));
        CredentialCheckResult byAnotherIp = await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Entered, IPAddress.Parse("198.51.100.20"), null, Ct));

        Assert.Equal(ErrorCode.SignInLimitExceeded, byTheSameIp.Code);
        Assert.True(byAnotherIp.Succeeded);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM attempt_records WHERE attempt_axis = 'Email' AND normalized_value = 'ali@example.com'"));
    }

    [Fact]
    public async Task TheEmailAxisStopsAloneWithManyIpsForOneEmail_AndTheIpAxisIsNotFull_T377_KK1()
    {
        await SeedCustomerAsync("ali@example.com", verified: true);
        await SeedCustomerAsync("veli@example.com", verified: true);
        for (int i = 0; i < ProductConstants.SignInAttemptsPerEmail; i++)
        {
            IPAddress address = IPAddress.Parse(string.Create(CultureInfo.InvariantCulture, $"198.51.100.{i + 1}"));
            await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Wrong, address, null, Ct));
        }

        CredentialCheckResult sameEmail = await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Entered, IPAddress.Parse("198.51.100.99"), null, Ct));
        CredentialCheckResult otherEmail = await RunAsync(m => m.CheckCredentialsAsync("veli@example.com", Entered, IPAddress.Parse("198.51.100.1"), null, Ct));

        Assert.Equal(ErrorCode.SignInLimitExceeded, sameEmail.Code);
        Assert.True(otherEmail.Succeeded);
    }

    // ---- KK6: the administrator is under the same limits with the same values ----

    [Fact]
    public async Task TheAdministrator_IsBlockedOnTheEmailAxisAtTheSameFive_AndOnTheIpAxisAtTheSameTwenty_T377_KK6()
    {
        await SeedAdminAsync("yonetici@example.com");
        for (int i = 0; i < ProductConstants.SignInAttemptsPerEmail; i++)
        {
            await RunAdminAsync(a => a.CheckCredentialsAsync("yonetici@example.com", Wrong, IPAddress.Parse(string.Create(CultureInfo.InvariantCulture, $"198.51.100.{i + 1}")), null, Ct));
        }

        AdminCredentialCheckResult byEmail = await RunAdminAsync(a => a.CheckCredentialsAsync("yonetici@example.com", Entered, IPAddress.Parse("198.51.100.99"), null, Ct));
        Assert.Equal(ErrorCode.SignInLimitExceeded, byEmail.Code);

        for (int i = 0; i < ProductConstants.SignInAttemptsPerIp; i++)
        {
            await RunAdminAsync(a => a.CheckCredentialsAsync(string.Create(CultureInfo.InvariantCulture, $"digeri{i}@example.com"), Wrong, Ip, null, Ct));
        }

        AdminCredentialCheckResult byIp = await RunAdminAsync(a => a.CheckCredentialsAsync("yonetici@example.com", Entered, Ip, null, Ct));
        Assert.Equal(ErrorCode.SignInLimitExceeded, byIp.Code);
    }

    // ---- KK9: a forged mark, another account's mark and an expired mark count as no mark; a marked browser keeps the IP axis ----

    [Fact]
    public async Task AForgedMark_AndAnotherAccountsMark_AreUnmarked_AndCountOnTheEmail_T377_KK9()
    {
        await SeedCustomerAsync("ali@example.com", verified: true);
        Guid other = await SeedCustomerAsync("veli@example.com", verified: true);
        SessionOpening veli = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(other, false, null, Ct));

        await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Wrong, Ip, "forged-" + Guid.NewGuid().ToString("N"), Ct));
        await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Wrong, Ip, veli.MarkValue, Ct));

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM attempt_records WHERE attempt_axis = 'Email'"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM attempt_records WHERE attempt_axis = 'RecognizedBrowser'"));
    }

    [Fact]
    public async Task AMarkedBrowser_HasItsOwnCount_ButTheIpAxisStillStopsManyMarkedBrowsersFromOneIp_T377_KK9()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        int marks = ProductConstants.SignInAttemptsPerIp / ProductConstants.SignInAttemptsPerEmail;
        for (int m = 0; m < marks; m++)
        {
            SessionOpening opening = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(id, false, null, Ct));
            for (int i = 0; i < ProductConstants.SignInAttemptsPerEmail; i++)
            {
                await RunAsync(x => x.CheckCredentialsAsync("ali@example.com", Wrong, Ip, opening.MarkValue, Ct));
            }
        }

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM attempt_records WHERE attempt_axis = 'Email'"));
        SessionOpening fresh = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(id, false, null, Ct));
        CredentialCheckResult stopped = await RunAsync(x => x.CheckCredentialsAsync("ali@example.com", Entered, Ip, fresh.MarkValue, Ct));
        CredentialCheckResult elsewhere = await RunAsync(x => x.CheckCredentialsAsync("ali@example.com", Entered, IPAddress.Parse("198.51.100.77"), fresh.MarkValue, Ct));

        Assert.Equal(ErrorCode.SignInLimitExceeded, stopped.Code);
        Assert.True(elsewhere.Succeeded);
    }

    // ---- KK10: the mark lives 90 days from the last successful sign-in ----

    [Fact]
    public async Task TheMark_IsRecognisedUntilNinetyDaysAfterTheLastSignIn_AndNotAfter_T377_KK10()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        SessionOpening opening = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(id, false, null, Ct));

        _time.Advance(TimeSpan.FromDays(ProductConstants.RecognizedBrowserMarkDays - 1));
        await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Wrong, Ip, opening.MarkValue, Ct));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM attempt_records WHERE attempt_axis = 'RecognizedBrowser'"));

        _time.Advance(TimeSpan.FromDays(2));
        await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Wrong, Ip, opening.MarkValue, Ct));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM attempt_records WHERE attempt_axis = 'Email'"));
        await using AppDbContext db = NewContext();
        Assert.False(await db.RecognizedBrowserMarks.AnyAsync(mark => mark.MarkHash == SecretKey.Hash(opening.MarkValue) && mark.ExpiresAt > _time.GetUtcNow(), Ct));
    }
}
