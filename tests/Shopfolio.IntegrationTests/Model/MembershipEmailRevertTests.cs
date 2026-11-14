using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Membership.Contracts.Results;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>
/// The revert link's button through the real Membership module on a real PostgreSQL (T519 KK3-KK8; D-578): the link's reading, the revert's writes, the refusal that writes nothing, and the unique-index race
/// of the bulk update - the account that takes the address between the check and the write - for the new address's button (D-578) and for the revert's.
/// </summary>
public sealed partial class MembershipTests
{
    private const string Rival = "Rakip";

    // ---- the reading ----

    [Fact]
    public async Task RevertLink_IsReadByItsKind_AndAnotherTypesKeyAndAnUnknownKeyAreMissing_T519_KK2_KK3()
    {
        (Guid id, string key) = await ChangedAccountAsync();
        EmailRevertLinkReading valid = await ReadAsync(m => m.ReadEmailRestoreLinkAsync(key, Ct));
        Guid verifyLinkId = await ScalarAsync<Guid>("SELECT id FROM link_records WHERE link_type = 'NewEmailVerification' LIMIT 1");

        EmailRevertLinkReading wrongType = await ReadAsync(m => m.ReadEmailRestoreLinkAsync(_keys.Derive(verifyLinkId), Ct));
        EmailRevertLinkReading unknown = await ReadAsync(m => m.ReadEmailRestoreLinkAsync("nope", Ct));

        Assert.Equal(VerificationLinkStatus.Valid, valid.Status);
        Assert.Equal(VerificationLinkStatus.Missing, wrongType.Status);
        Assert.Equal(VerificationLinkStatus.Missing, unknown.Status);
        Assert.Equal("yeni@example.com", await ScalarAsync<string>("SELECT email FROM customers WHERE id = '" + id + "'"));
    }

    // ---- the revert ----

    [Fact]
    public async Task Revert_GoesBackToTheEarlierAddress_ConsumesTheLinkOnce_AndEndsTheLinksThatWereAskedForDuringTheChange_T519_KK4_KK5()
    {
        (Guid id, string key) = await ChangedAccountAsync();
        OpenGoogle();
        await RunAsync(m => m.SignInWithGoogleAsync(new GoogleIdentity("yeni@example.com", true, "Ali", null), Ip, Ct));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM link_records WHERE link_type = 'GoogleLinkConfirmation' AND used_at IS NULL AND superseded_at IS NULL"));

        EmailRevertResult reverted = await RunAsync(m => m.RestorePreviousEmailAsync(key, Ct));
        EmailRevertResult again = await RunAsync(m => m.RestorePreviousEmailAsync(key, Ct));

        Assert.True(reverted.IsReverted);
        Assert.Equal(id, reverted.CustomerId);
        Assert.True(reverted.ResetLinkSent);
        Assert.False(again.IsReverted);
        Assert.Equal(VerificationLinkStatus.Used, again.Status);
        Assert.Equal("ali@example.com", await ScalarAsync<string>("SELECT email FROM customers WHERE id = '" + id + "'"));

        // The used link no longer holds the earlier address (06 4.42; VAL-A809): emptied in the transaction that used it; the row stays with its use time.
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM link_records WHERE link_type = 'EmailChangeRevert' AND previous_email IS NOT NULL"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM link_records WHERE link_type = 'EmailChangeRevert' AND used_at IS NOT NULL"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM customers WHERE id = '" + id + "' AND password_hash IS NOT NULL"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM link_records WHERE link_type IN ('NewEmailVerification', 'GoogleLinkConfirmation') AND used_at IS NULL AND superseded_at IS NULL"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM link_records WHERE link_type = 'PasswordReset' AND used_at IS NULL AND superseded_at IS NULL"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records WHERE dispatch_type = 'PasswordResetLink' AND recipient_email = 'ali@example.com'"));
    }

    [Fact]
    public async Task Revert_WhenTheEarlierAddressBecameAnotherVerifiedAccounts_IsRefused_AndWritesNothing_T519_KK7()
    {
        (Guid id, string key) = await ChangedAccountAsync();
        await SeedCustomerAsync("ali@example.com", verified: true);

        EmailRevertResult result = await RunAsync(m => m.RestorePreviousEmailAsync(key, Ct));

        Assert.True(result.AddressTaken);
        Assert.False(result.IsReverted);
        Assert.Equal("yeni@example.com", await ScalarAsync<string>("SELECT email FROM customers WHERE id = '" + id + "'"));
        Assert.Equal(VerificationLinkStatus.Valid, (await ReadAsync(m => m.ReadEmailRestoreLinkAsync(key, Ct))).Status);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records WHERE dispatch_type = 'PasswordResetLink'"));
    }

    [Fact]
    public async Task Revert_WhenAnotherAccountTakesTheAddressBetweenTheCheckAndTheWrite_IsRefusedAsTaken_AndTheConsumptionIsTakenBack_T519_KK7()
    {
        (Guid id, string key) = await ChangedAccountAsync();

        EmailRevertResult result = await RaceAsync("ali@example.com", () => RunAsync(m => m.RestorePreviousEmailAsync(key, Ct)));

        Assert.True(result.AddressTaken);
        Assert.Equal("yeni@example.com", await ScalarAsync<string>("SELECT email FROM customers WHERE id = '" + id + "'"));
        Assert.Equal(VerificationLinkStatus.Valid, (await ReadAsync(m => m.ReadEmailRestoreLinkAsync(key, Ct))).Status);
        Assert.Equal("ali@example.com", await ScalarAsync<string>("SELECT previous_email FROM link_records WHERE link_type = 'EmailChangeRevert'"));
    }

    [Fact]
    public async Task Revert_WhenTheLinkIsSpentWhileTheCallWaitsForTheAccountsRowLock_FindsItUsed_AndWritesNothing_T519_KK8()
    {
        (Guid id, string key) = await ChangedAccountAsync();
        await using AppDbContext rival = NewContext();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await rival.Database.BeginTransactionAsync(Ct);
        await rival.Database.ExecuteSqlAsync($"SELECT id FROM customers WHERE id = {id} FOR UPDATE", Ct);

        // The call reads the link valid, then waits on the account's row; the rival uses the link meanwhile (the same conditional update the call would make) and commits.
        Task<EmailRevertResult> running = Task.Run(() => RunAsync(m => m.RestorePreviousEmailAsync(key, Ct)), Ct);
        await Task.Delay(TimeSpan.FromSeconds(2), Ct);
        Assert.False(running.IsCompleted, "The call must wait for the account's row lock.");
        await rival.Database.ExecuteSqlRawAsync("UPDATE link_records SET used_at = now(), previous_email = NULL WHERE link_type = 'EmailChangeRevert'", Ct);
        await transaction.CommitAsync(Ct);
        EmailRevertResult result = await running;

        Assert.False(result.IsReverted);
        Assert.False(result.AddressTaken);
        Assert.Equal(VerificationLinkStatus.Used, result.Status);
        Assert.Equal("yeni@example.com", await ScalarAsync<string>("SELECT email FROM customers WHERE id = '" + id + "'"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records WHERE dispatch_type = 'PasswordResetLink'"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM link_records WHERE link_type = 'PasswordReset'"));
    }

    [Fact]
    public async Task Revert_OnALinkPastItsLifetime_IsExpired_AndWritesNothing_T519_VALC029()
    {
        (Guid id, string key) = await ChangedAccountAsync();
        _time.Advance(TimeSpan.FromDays(ProductConstants.EmailChangeRevertLinkDays) + TimeSpan.FromSeconds(1));

        EmailRevertResult result = await RunAsync(m => m.RestorePreviousEmailAsync(key, Ct));

        Assert.False(result.IsReverted);
        Assert.Equal(VerificationLinkStatus.Expired, result.Status);
        Assert.Equal("yeni@example.com", await ScalarAsync<string>("SELECT email FROM customers WHERE id = '" + id + "'"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM link_records WHERE link_type = 'EmailChangeRevert' AND used_at IS NOT NULL"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records WHERE dispatch_type = 'PasswordResetLink'"));

        // The earlier address stays until the nightly cleanup empties it (06 4.42; K-1065): a refused use does not.
        Assert.Equal("ali@example.com", await ScalarAsync<string>("SELECT previous_email FROM link_records WHERE link_type = 'EmailChangeRevert'"));
    }

    // ---- D-578: the bulk update's provider exception is the same refusal as the tracked save's ----

    [Fact]
    public async Task EmailChange_Verify_WhenAnotherAccountTakesTheAddressBetweenTheCheckAndTheWrite_IsRefusedAsTaken_D578()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        EmailChangeRequestResult requested = await RequestEmailChangeAsync(id, "yeni@example.com");

        NewEmailVerificationResult result = await RaceAsync("yeni@example.com", () => RunAsync(m => m.VerifyNewEmailAsync(_keys.Derive(requested.LinkId!.Value), Ct)));

        Assert.True(result.AddressTaken);
        Assert.Equal("ali@example.com", await ScalarAsync<string>("SELECT email FROM customers WHERE id = '" + id + "'"));
        Assert.Equal(VerificationLinkStatus.Valid, (await ReadAsync(m => m.ReadEmailLinkAsync(_keys.Derive(requested.LinkId!.Value), Ct))).Status);
    }

    // ---- helpers of the revert tests ----

    /// <summary>An account that changed its address from <c>ali@example.com</c> to <c>yeni@example.com</c>: the revert link of the earlier address and its raw key.</summary>
    private async Task<(Guid CustomerId, string RevertKey)> ChangedAccountAsync()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        EmailChangeRequestResult requested = await RequestEmailChangeAsync(id, "yeni@example.com");
        NewEmailVerificationResult verified = await RunAsync(m => m.VerifyNewEmailAsync(_keys.Derive(requested.LinkId!.Value), Ct));
        Assert.True(verified.IsVerified);
        return (id, _keys.Derive(verified.RevertLinkId!.Value));
    }

    /// <summary>
    /// Runs <paramref name="call"/> while a rival's transaction holds an uncommitted verified account of <paramref name="address"/>: the call's check does not see the rival, its bulk update waits on the unique
    /// index, and the rival's commit turns the wait into the index's violation - the race between the check and the write, made deterministic.
    /// </summary>
    private async Task<T> RaceAsync<T>(string address, Func<Task<T>> call)
    {
        await using AppDbContext rival = NewContext();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await rival.Database.BeginTransactionAsync(Ct);
        rival.Customers.Add(Customer.Create(Rival, address, null, Start, null, Start));
        await rival.SaveChangesAsync(Ct);

        Task<T> running = Task.Run(call, Ct);
        await Task.Delay(TimeSpan.FromSeconds(2), Ct);
        Assert.False(running.IsCompleted, "The call must wait for the rival's uncommitted row on the unique index.");
        await transaction.CommitAsync(Ct);
        return await running;
    }
}
