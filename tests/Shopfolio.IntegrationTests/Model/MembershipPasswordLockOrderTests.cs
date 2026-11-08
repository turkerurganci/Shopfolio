using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Membership.Contracts.Results;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>
/// T342 validation round 2, finding 1 (KK2; 09 15.1 item 3; 09 25 "concurrency"): every password flow takes the account's row lock FIRST and the session rows after it, so two flows of
/// one account never lock in opposite orders (no 40P01). The change and the reset, and the change and the sign-in, run on two real connections.
/// </summary>
public sealed partial class MembershipTests
{
    [Fact]
    public async Task TheChange_TakesTheAccountLockBeforeTheSessionRow_SoItWaitsWithoutHoldingTheSession_KK2()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        SessionOpening mine = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(id, false, null, Ct));
        string value = await GrantAsync(id, mine.SessionId);

        await using AppDbContext holder = NewContext();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction held = await holder.Database.BeginTransactionAsync(Ct);
        await holder.Database.SqlQuery<Guid>($"SELECT id AS \"Value\" FROM customers WHERE id = {id} FOR UPDATE").ToListAsync(Ct);

        Task<ChangeOutcome> changing = RunScopeAsync(scope => scope.GetRequiredService<IMembershipModule>().ChangePasswordAsync(id, mine.SessionId, Replacement, value, null, Ct));
        await WaitForABlockedStatementAsync();
        Assert.False(changing.IsCompleted);

        // The change waits for the account row and has not touched its session row yet: a no-wait lock on the session row succeeds.
        await using (AppDbContext probe = NewContext())
        await using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction probing = await probe.Database.BeginTransactionAsync(Ct))
        {
            List<Guid> free = await probe.Database.SqlQuery<Guid>($"SELECT id AS \"Value\" FROM sessions WHERE id = {mine.SessionId} FOR UPDATE NOWAIT").ToListAsync(Ct);
            Assert.Single(free);
        }

        await held.CommitAsync(Ct);
        Assert.True((await changing).IsDone);
    }

    [Fact]
    public async Task TheChangeAndTheReset_OfOneAccount_FinishOneAfterTheOther_WithNoDeadlock_KK2()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        SessionOpening mine = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(id, false, null, Ct));
        string value = await GrantAsync(id, mine.SessionId);
        ResetRequested requested = await RunAsync(m => m.RequestPasswordResetAsync("ali@example.com", Ip, Ct));
        string key = _keys.Derive(requested.LinkId!.Value);
        const string FromTheReset = "the-reset-wins-the-last-word-42";

        TaskCompletionSource written = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<ChangeOutcome> changing = RunScopeAsync(async scope =>
        {
            ChangeOutcome outcome = await scope.GetRequiredService<IMembershipModule>().ChangePasswordAsync(id, mine.SessionId, Replacement, value, null, Ct);
            written.SetResult();
            await release.Task;
            return outcome;
        });
        await written.Task.WaitAsync(TimeSpan.FromSeconds(30), Ct);

        Task<ResetOutcome> resetting = RunScopeAsync(scope => scope.GetRequiredService<IMembershipModule>().ResetPasswordAsync(key, FromTheReset, null, Ct));
        await WaitForABlockedStatementAsync();
        Assert.False(resetting.IsCompleted);
        release.SetResult();

        ChangeOutcome changed = await changing.WaitAsync(TimeSpan.FromSeconds(60), Ct);
        ResetOutcome reset = await resetting.WaitAsync(TimeSpan.FromSeconds(60), Ct);

        Assert.True(changed.IsDone);
        Assert.True(reset.IsDone);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Sessions.CountAsync(s => s.CustomerId == id, Ct));
        Assert.True((await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", FromTheReset, Ip, null, Ct))).Succeeded);
        Assert.False((await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Replacement, Ip, null, Ct))).Succeeded);
    }

    [Fact]
    public async Task ASignInThatWaitsForTheChange_ChecksTheNewDigest_AndLeavesNoSessionOfTheOldPassword_KK2()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        SessionOpening mine = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(id, false, null, Ct));
        string value = await GrantAsync(id, mine.SessionId);

        TaskCompletionSource written = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<ChangeOutcome> changing = RunScopeAsync(async scope =>
        {
            ChangeOutcome outcome = await scope.GetRequiredService<IMembershipModule>().ChangePasswordAsync(id, mine.SessionId, Replacement, value, null, Ct);
            written.SetResult();
            await release.Task;
            return outcome;
        });
        await written.Task.WaitAsync(TimeSpan.FromSeconds(30), Ct);

        Task<Held<bool>> signing = RunScopeAsync(async scope => new Held<bool>((await scope.GetRequiredService<IMembershipModule>().CheckCredentialsAsync("ali@example.com", Entered, Ip, null, Ct)).Succeeded));
        await WaitForABlockedStatementAsync();
        Assert.False(signing.IsCompleted);
        release.SetResult();

        Assert.True((await changing.WaitAsync(TimeSpan.FromSeconds(60), Ct)).IsDone);
        Assert.False((await signing.WaitAsync(TimeSpan.FromSeconds(60), Ct)).Value);
        await using AppDbContext db = NewContext();
        Assert.Equal([mine.SessionId], await db.Sessions.Where(s => s.CustomerId == id).Select(s => s.Id).ToListAsync(Ct));
    }

    [Fact]
    public async Task AChangeThatWaitsForASignIn_EndsTheSessionTheSignInOpened_KK2()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        SessionOpening mine = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(id, false, null, Ct));
        string value = await GrantAsync(id, mine.SessionId);

        TaskCompletionSource checkedOnce = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<Held<Guid>> signing = RunScopeAsync(async scope =>
        {
            bool matched = (await scope.GetRequiredService<IMembershipModule>().CheckCredentialsAsync("ali@example.com", Entered, Ip, null, Ct)).Succeeded;
            checkedOnce.SetResult();
            await release.Task;
            SessionOpening opened = await scope.GetRequiredService<ISessionService>().SignInAsync(id, false, null, Ct);
            Assert.True(matched);
            return new Held<Guid>(opened.SessionId);
        });
        await checkedOnce.Task.WaitAsync(TimeSpan.FromSeconds(30), Ct);

        Task<ChangeOutcome> changing = RunScopeAsync(scope => scope.GetRequiredService<IMembershipModule>().ChangePasswordAsync(id, mine.SessionId, Replacement, value, null, Ct));
        await WaitForABlockedStatementAsync();
        Assert.False(changing.IsCompleted);
        release.SetResult();

        Guid opened = (await signing.WaitAsync(TimeSpan.FromSeconds(60), Ct)).Value;
        Assert.True((await changing.WaitAsync(TimeSpan.FromSeconds(60), Ct)).IsDone);
        await using AppDbContext db = NewContext();
        List<Guid> left = await db.Sessions.Where(s => s.CustomerId == id).Select(s => s.Id).ToListAsync(Ct);
        Assert.Equal([mine.SessionId], left);
        Assert.DoesNotContain(opened, left);
    }

    private async Task<string> GrantAsync(Guid customerId, Guid sessionId)
    {
        ReauthenticationResult granted = await RunAsync(m => m.ReauthenticateAsync(customerId, sessionId, ReauthenticationOperation.PasswordChange, Entered, Ip, null, Ct));
        return granted.Value!;
    }
}
