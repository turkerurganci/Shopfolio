using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Membership.Contracts.Results;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>
/// T342 validation round 1, finding 1 (KK2; 09 15.1 item 3; 09 25 "concurrency"): the sign-in and the password reset of the same account on two real connections. The sign-in locks the
/// account's row before it reads the digest and keeps the lock until its session row is committed, the reset writes the digest under the same lock, so the two are one after the other
/// in either order - no session survives a reset that committed after the sign-in checked the old password, and a sign-in that waited for the reset checks the new digest.
/// </summary>
public sealed partial class MembershipTests
{
    [Fact]
    public async Task ASignInCheckedBeforeTheReset_HasItsSessionEndedByTheReset_Customer_KK2()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        ResetRequested requested = await RunAsync(m => m.RequestPasswordResetAsync("ali@example.com", Ip, Ct));
        string key = _keys.Derive(requested.LinkId!.Value);

        (bool signedIn, ResetOutcome reset) = await SignInThenResetAsync(
            async scope => (await scope.GetRequiredService<IMembershipModule>().CheckCredentialsAsync("ali@example.com", Entered, Ip, null, Ct)).Succeeded,
            scope => scope.GetRequiredService<ISessionService>().SignInAsync(id, false, null, Ct),
            scope => scope.GetRequiredService<IMembershipModule>().ResetPasswordAsync(key, Replacement, null, Ct));

        Assert.True(signedIn);
        Assert.True(reset.IsDone);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Sessions.CountAsync(s => s.CustomerId == id, Ct));
        Assert.False((await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Entered, Ip, null, Ct))).Succeeded);
    }

    [Fact]
    public async Task ASignInThatWaitsForTheReset_ChecksTheNewDigest_AndOpensNoSession_Customer_KK2()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        ResetRequested requested = await RunAsync(m => m.RequestPasswordResetAsync("ali@example.com", Ip, Ct));
        string key = _keys.Derive(requested.LinkId!.Value);

        (ResetOutcome reset, bool signedIn) = await ResetThenSignInAsync(
            scope => scope.GetRequiredService<IMembershipModule>().ResetPasswordAsync(key, Replacement, null, Ct),
            async scope => (await scope.GetRequiredService<IMembershipModule>().CheckCredentialsAsync("ali@example.com", Entered, Ip, null, Ct)).Succeeded);

        Assert.True(reset.IsDone);
        Assert.False(signedIn);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Sessions.CountAsync(s => s.CustomerId == id, Ct));
        Assert.True((await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Replacement, Ip, null, Ct))).Succeeded);
    }

    [Fact]
    public async Task ASignInCheckedBeforeTheReset_HasItsSessionEndedByTheReset_Administrator_KK2_KK8()
    {
        Guid id = await SeedAdminAsync("yonetici@example.com");
        ResetRequested requested = await RunAdminAsync(a => a.RequestPasswordResetAsync("yonetici@example.com", Ip, Ct));
        string key = _keys.Derive(requested.LinkId!.Value);

        (bool signedIn, ResetOutcome reset) = await SignInThenResetAsync(
            async scope => (await scope.GetRequiredService<IAdministrationModule>().CheckCredentialsAsync("yonetici@example.com", Entered, Ip, null, Ct)).Succeeded,
            scope => scope.GetRequiredService<IAdministrationModule>().SignInAsync(id, false, null, Ct),
            scope => scope.GetRequiredService<IAdministrationModule>().ResetPasswordAsync(key, Replacement, null, Ct));

        Assert.True(signedIn);
        Assert.True(reset.IsDone);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Sessions.CountAsync(s => s.AdminId == id, Ct));
        Assert.False((await RunAdminAsync(a => a.CheckCredentialsAsync("yonetici@example.com", Entered, Ip, null, Ct))).Succeeded);
    }

    [Fact]
    public async Task ASignInThatWaitsForTheReset_ChecksTheNewDigest_AndOpensNoSession_Administrator_KK2_KK8()
    {
        Guid id = await SeedAdminAsync("yonetici@example.com");
        ResetRequested requested = await RunAdminAsync(a => a.RequestPasswordResetAsync("yonetici@example.com", Ip, Ct));
        string key = _keys.Derive(requested.LinkId!.Value);

        (ResetOutcome reset, bool signedIn) = await ResetThenSignInAsync(
            scope => scope.GetRequiredService<IAdministrationModule>().ResetPasswordAsync(key, Replacement, null, Ct),
            async scope => (await scope.GetRequiredService<IAdministrationModule>().CheckCredentialsAsync("yonetici@example.com", Entered, Ip, null, Ct)).Succeeded);

        Assert.True(reset.IsDone);
        Assert.False(signedIn);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Sessions.CountAsync(s => s.AdminId == id, Ct));
        Assert.True((await RunAdminAsync(a => a.CheckCredentialsAsync("yonetici@example.com", Replacement, Ip, null, Ct))).Succeeded);
    }

    /// <summary>
    /// One transaction checks the old password and holds its session back; the reset starts on a second connection and must wait for the account's row lock; the sign-in then writes its
    /// session and commits, and only after that does the reset write the digest and delete the account's sessions.
    /// </summary>
    private async Task<(bool SignedIn, ResetOutcome Reset)> SignInThenResetAsync(
        Func<IServiceProvider, Task<bool>> check,
        Func<IServiceProvider, Task<SessionOpening>> open,
        Func<IServiceProvider, Task<ResetOutcome>> reset)
    {
        TaskCompletionSource checkedOnce = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<Held<bool>> signIn = RunScopeAsync(async scope =>
        {
            bool matched = await check(scope);
            checkedOnce.SetResult();
            await release.Task;
            if (matched)
            {
                await open(scope);
            }

            return new Held<bool>(matched);
        });
        await checkedOnce.Task.WaitAsync(TimeSpan.FromSeconds(30), Ct);

        Task<ResetOutcome> resetting = RunScopeAsync(reset);
        await WaitForABlockedStatementAsync();
        Assert.False(resetting.IsCompleted);
        release.SetResult();
        Held<bool> signed = await signIn;
        return (signed.Value, await resetting);
    }

    /// <summary>
    /// One transaction writes the new digest and holds the row; a sign-in with the old password starts on a second connection and must wait for the account's row lock; when the reset
    /// commits, the sign-in reads the new digest.
    /// </summary>
    private async Task<(ResetOutcome Reset, bool SignedIn)> ResetThenSignInAsync(Func<IServiceProvider, Task<ResetOutcome>> reset, Func<IServiceProvider, Task<bool>> signIn)
    {
        TaskCompletionSource written = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<ResetOutcome> resetting = RunScopeAsync(async scope =>
        {
            ResetOutcome outcome = await reset(scope);
            written.SetResult();
            await release.Task;
            return outcome;
        });
        await written.Task.WaitAsync(TimeSpan.FromSeconds(30), Ct);

        Task<Held<bool>> signing = RunScopeAsync(async scope => new Held<bool>(await signIn(scope)));
        await WaitForABlockedStatementAsync();
        Assert.False(signing.IsCompleted);
        release.SetResult();
        ResetOutcome done = await resetting;
        return (done, (await signing).Value);
    }

    private async Task WaitForABlockedStatementAsync()
    {
        for (int i = 0; i < 100; i++)
        {
            long blocked = await ScalarAsync<long>("SELECT count(*) FROM pg_stat_activity WHERE datname = current_database() AND wait_event_type = 'Lock'");
            if (blocked > 0)
            {
                return;
            }

            await Task.Delay(100, Ct);
        }

        Assert.Fail("No statement waited for the account's row lock.");
    }
}
