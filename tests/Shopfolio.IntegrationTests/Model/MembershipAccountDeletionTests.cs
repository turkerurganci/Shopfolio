using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Membership.Contracts.Results;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;

using Xunit;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>
/// T345: the deletion of an account and the personal data request (02 3.15; 03 9.4; 07 UC-68, UC-157, UC-95). The module's own rules are sampled here on the real database: what goes with
/// the account, what stays with the order, the one e-mail, the revert link's gate, the reauthentication and the administrator's trail row.
/// </summary>
public sealed partial class MembershipTests
{
    private async Task<Guid> SeedAdminActorAsync()
    {
        Admin admin = Admin.Create("Yonetici", "yonetici@example.com", SeedDigest.Value, Start);
        await SaveAsync(c => c.Admins.Add(admin));
        ActAs(admin.Id, admin.Name, admin.Email);
        return admin.Id;
    }

    private async Task<(Guid Id, SessionOpening Session)> SeedMemberWithEverythingAsync(string email)
    {
        Guid id = await SeedCustomerAsync(email, verified: true);
        SessionOpening session = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(id, true, null, Ct));
        await SaveAsync(c => c.AddressBooks.Add(AddressBook.Create(id, "Ev", "Ali Veli", _provinceId, "Çankaya", "Atatürk Bulvarı 1", "5321234567", Start)));
        await SaveAsync(c => c.Carts.Add(Cart.ForCustomer(id, Start)));
        await RunAsync(m => m.RequestEmailChangeAsync(id, "yeni-" + email, Ip, Ct));
        return (id, session);
    }

    private async Task<string> GrantDeletionAsync(Guid id, Guid sessionId)
    {
        ReauthenticationResult granted = await RunAsync(m => m.ReauthenticateAsync(id, sessionId, ReauthenticationOperation.AccountDeletion, Entered, Ip, null, Ct));
        Assert.True(granted.Succeeded);
        return granted.Value!;
    }

    private async Task SeedOpenRevertLinkAsync(Guid id, DateTimeOffset ends)
    {
        LinkRecord link = LinkRecord.ForCustomer(_keys, LinkType.EmailChangeRevert, id, ends, Start);
        await SaveAsync(c => c.LinkRecords.Add(link));
    }

    // ---- KK1: the account and what hangs on it go; the disposal record counts them by type ----

    [Fact]
    public async Task DeleteOwn_RemovesTheAccountAndWhatHangsOnIt_AndTheDisposalRecordCountsThem_KK1()
    {
        (Guid id, SessionOpening session) = await SeedMemberWithEverythingAsync("ali@example.com");
        Guid other = await SeedCustomerAsync("veli@example.com", verified: true);
        string value = await GrantDeletionAsync(id, session.SessionId);

        AccountDeletionResult result = await RunAsync(m => m.DeleteAccountAsync(id, session.SessionId, value, Ct));

        Assert.True(result.IsDeleted);
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM customers WHERE id = '{id}'"));
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM customers WHERE id = '{other}'"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM sessions"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM recognized_browser_marks"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM address_books"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM carts"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM pending_email_changes"));
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM link_records WHERE customer_id = '{id}'"));
        await using AppDbContext db = NewContext();
        Dictionary<DisposalRecordType, int?> records = await db.DisposalRecords.AsNoTracking().ToDictionaryAsync(r => r.DisposalRecordType, r => r.Count, Ct);
        Assert.Equal(1, records[DisposalRecordType.Customer]);
        Assert.Equal(1, records[DisposalRecordType.Session]);
        Assert.Equal(1, records[DisposalRecordType.RecognizedBrowserMark]);
        Assert.Equal(1, records[DisposalRecordType.Cart]);
        Assert.Equal(1, records[DisposalRecordType.PendingEmailChange]);
        Assert.True(records[DisposalRecordType.LinkRecord] >= 1);
        Assert.DoesNotContain(DisposalRecordType.Order, records.Keys);
    }

    // ---- KK2, KK4: the orders keep their own record; an order that is still open does not stop the deletion ----

    [Fact]
    public async Task DeleteOwn_LeavesTheOrdersWithTheirOwnRecord_MarkedDeleted_WithoutRaisingTheVersion_KK2_KK4()
    {
        (Guid id, SessionOpening session) = await SeedMemberWithEverythingAsync("ali@example.com");
        Guid order = await SeedGuestOrderAsync("ali@example.com", id);
        int versionBefore = await ScalarAsync<int>($"SELECT version FROM orders WHERE id = '{order}'");
        string value = await GrantDeletionAsync(id, session.SessionId);

        AccountDeletionResult result = await RunAsync(m => m.DeleteAccountAsync(id, session.SessionId, value, Ct));

        Assert.True(result.IsDeleted);
        Assert.Equal(1, result.OrdersKept);
        await using AppDbContext db = NewContext();
        Order kept = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == order, Ct);
        Assert.Null(kept.CustomerId);
        Assert.True(kept.IsAccountDeleted);
        Assert.Equal(versionBefore, kept.Version);
        Assert.Equal("ali@example.com", kept.ContactEmail);
        Assert.Equal("Ali Veli", kept.BillingRecipientName);
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM order_items WHERE order_id = '{order}'"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM disposal_records WHERE disposal_record_type = 'Order'"));
    }

    // ---- KK3: one e-mail to the account's address, whoever deletes ----

    [Fact]
    public async Task DeleteOwn_QueuesTheNoticeOnce_ToTheAccountsAddress_KK3()
    {
        (Guid id, SessionOpening session) = await SeedMemberWithEverythingAsync("ali@example.com");
        string value = await GrantDeletionAsync(id, session.SessionId);

        await RunAsync(m => m.DeleteAccountAsync(id, session.SessionId, value, Ct));

        await using AppDbContext db = NewContext();
        DispatchRecord notice = await db.DispatchRecords.AsNoTracking().SingleAsync(d => d.DispatchType == DispatchType.AccountDeletedNotice, Ct);
        Assert.Equal("ali@example.com", notice.RecipientEmail);
    }

    // ---- KK5: a deleted account's order is not linked again to a new account of the same address ----

    [Fact]
    public async Task ANewAccountOfTheSameAddress_DoesNotTakeTheDeletedAccountsOrder_KK5()
    {
        (Guid id, SessionOpening session) = await SeedMemberWithEverythingAsync("ali@example.com");
        Guid order = await SeedGuestOrderAsync("ali@example.com", id);
        string value = await GrantDeletionAsync(id, session.SessionId);
        await RunAsync(m => m.DeleteAccountAsync(id, session.SessionId, value, Ct));
        RegistrationResult again = await RegisterAsync("Ali", "ali@example.com");
        Assert.True(again.IsRegistered);

        EmailVerificationResult verified = await RunAsync(m => m.VerifyEmailAsync(_keys.Derive(again.LinkId!.Value), Ct));

        Assert.True(verified.IsVerified);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM customers WHERE email_verified_at IS NOT NULL"));
        Assert.Null(await OwnerOfAsync(order));
        Assert.True(await ScalarAsync<bool>($"SELECT is_account_deleted FROM orders WHERE id = '{order}'"));
    }

    // The race of 05 6.1.2 / 09 15.1 item 3 on two real sessions: the order's insert takes FOR KEY SHARE on its customer row, the deletion takes FOR UPDATE on it. Whichever comes first, the order
    // ends marked (the insert committed first and the deletion found it) or never exists (the deletion committed first and the insert failed on the removed account).

    [Fact]
    public async Task AnOrderInsertedWhileTheAccountIsBeingDeleted_IsWaitedFor_AndEndsMarked_KK5()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        await SeedAdminActorAsync();
        (Order order, OrderItem item) = BuildOrder("ali@example.com", id, accountDeleted: false);
        await using AppDbContext confirming = NewContext();
        await using IDbContextTransaction open = await confirming.Database.BeginTransactionAsync(Ct);
        confirming.Orders.Add(order);
        confirming.OrderItems.Add(item);
        await confirming.SaveChangesAsync(Ct);

        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.OpenConnectionAsync(Ct);
        int pid = await BackendPidAsync(db);
        Task<AccountDeletionResult> deleting = scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            _ => scope.ServiceProvider.GetRequiredService<IMembershipModule>().DeleteAccountOnRequestAsync(id, Ct),
            Ct);
        await WaitUntilBlockedAsync(pid);
        Assert.False(deleting.IsCompleted);
        await open.CommitAsync(Ct);

        AccountDeletionResult result = await deleting.WaitAsync(TimeSpan.FromSeconds(30), Ct);

        Assert.True(result.IsDeleted);
        Assert.Equal(1, result.OrdersKept);
        Assert.Null(await OwnerOfAsync(order.Id));
        Assert.True(await ScalarAsync<bool>($"SELECT is_account_deleted FROM orders WHERE id = '{order.Id}'"));
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM customers WHERE id = '{id}'"));
    }

    [Fact]
    public async Task AnOrderInsertedAfterTheDeletionHoldsTheAccount_WaitsAndFails_SoNoUnmarkedOrderExists_KK5()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        await SeedAdminActorAsync();
        (Order order, OrderItem item) = BuildOrder("ali@example.com", id, accountDeleted: false);
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using IDbContextTransaction open = await db.Database.BeginTransactionAsync(Ct);
        AccountDeletionResult result = await scope.ServiceProvider.GetRequiredService<IMembershipModule>().DeleteAccountOnRequestAsync(id, Ct);
        Assert.True(result.IsDeleted);
        await db.SaveChangesAsync(Ct);

        await using AppDbContext confirming = NewContext();
        await confirming.Database.OpenConnectionAsync(Ct);
        int pid = await BackendPidAsync(confirming);
        confirming.Orders.Add(order);
        confirming.OrderItems.Add(item);
        Task inserting = confirming.SaveChangesAsync(Ct);
        await WaitUntilBlockedAsync(pid);
        Assert.False(inserting.IsCompleted);
        await open.CommitAsync(Ct);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => inserting.WaitAsync(TimeSpan.FromSeconds(30), Ct));
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM orders WHERE id = '{order.Id}'"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM orders WHERE customer_id IS NULL AND is_account_deleted = false"));
    }

    private static async Task<int> BackendPidAsync(AppDbContext context)
    {
        return (await context.Database.SqlQuery<int>($"SELECT pg_backend_pid() AS \"Value\"").ToListAsync(Ct)).Single();
    }

    /// <summary>Waits until the backend waits on a lock (pg_stat_activity), at most about ten seconds.</summary>
    private async Task WaitUntilBlockedAsync(int pid)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        for (int attempt = 0; attempt < 400; attempt++)
        {
            await using NpgsqlCommand command = new("SELECT wait_event_type FROM pg_stat_activity WHERE pid = @pid", connection);
            command.Parameters.AddWithValue("pid", pid);
            if (await command.ExecuteScalarAsync(Ct) is string type && type == "Lock")
            {
                return;
            }

            await Task.Delay(25, Ct);
        }

        throw new Xunit.Sdk.XunitException($"Backend {pid} never waited on a lock.");
    }

    // ---- KK9: an open revert link of an e-mail change closes the deletion, the member's and the firm's ----

    [Fact]
    public async Task AnOpenRevertLink_ClosesBothDeletions_NothingIsDeleted_AndTheReauthenticationValueIsNotSpent_KK9()
    {
        await SeedAdminActorAsync();
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        SessionOpening session = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(id, false, null, Ct));
        DateTimeOffset ends = Start.AddDays(ProductConstants.EmailChangeRevertLinkDays);
        await SeedOpenRevertLinkAsync(id, ends);
        string value = await GrantDeletionAsync(id, session.SessionId);

        AccountDeletionResult own = await RunAsync(m => m.DeleteAccountAsync(id, session.SessionId, value, Ct));
        AccountDeletionResult byAdmin = await RunAsync(m => m.DeleteAccountOnRequestAsync(id, Ct));
        DateTimeOffset? blocked = await ReadAsync(m => m.GetDeletionBlockedUntilAsync(id, Ct));

        Assert.Equal(ErrorCode.EmailChangeRevertPending, own.Code);
        Assert.Equal(ends, own.RevertLinkEndsAt);
        Assert.Equal(ErrorCode.EmailChangeRevertPending, byAdmin.Code);
        Assert.Equal(ends, byAdmin.RevertLinkEndsAt);
        Assert.Equal(ends, blocked);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM customers"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM sessions"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM disposal_records"));

        _time.Advance(TimeSpan.FromDays(ProductConstants.EmailChangeRevertLinkDays) + TimeSpan.FromSeconds(1));
        AccountDeletionResult after = await RunAsync(m => m.DeleteAccountAsync(id, session.SessionId, value, Ct));

        Assert.True(after.IsDeleted);
        Assert.Null(await ReadAsync(m => m.GetDeletionBlockedUntilAsync(id, Ct)));
    }

    // ---- reauthentication ----

    [Fact]
    public async Task DeleteOwn_WithoutTheReauthenticationValue_IsRefused_AndNothingIsDeleted()
    {
        (Guid id, SessionOpening session) = await SeedMemberWithEverythingAsync("ali@example.com");

        AccountDeletionResult none = await RunAsync(m => m.DeleteAccountAsync(id, session.SessionId, null, Ct));
        AccountDeletionResult wrong = await RunAsync(m => m.DeleteAccountAsync(id, session.SessionId, "not-the-value", Ct));

        Assert.Equal(ErrorCode.ReauthenticationRequired, none.Code);
        Assert.Equal(ErrorCode.ReauthenticationRequired, wrong.Code);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM customers"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM disposal_records"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records WHERE dispatch_type = 'AccountDeletedNotice'"));
    }

    [Fact]
    public async Task ASecondDeletion_IsNotFound_AndTheValueOfAnotherOperationDoesNotOpenIt()
    {
        (Guid id, SessionOpening session) = await SeedMemberWithEverythingAsync("ali@example.com");
        ReauthenticationResult forEmail = await RunAsync(m => m.ReauthenticateAsync(id, session.SessionId, ReauthenticationOperation.EmailChange, Entered, Ip, null, Ct));
        string value = await GrantDeletionAsync(id, session.SessionId);

        AccountDeletionResult wrongOperation = await RunAsync(m => m.DeleteAccountAsync(id, session.SessionId, forEmail.Value, Ct));
        AccountDeletionResult first = await RunAsync(m => m.DeleteAccountAsync(id, session.SessionId, value, Ct));
        AccountDeletionResult second = await RunAsync(m => m.DeleteAccountAsync(id, session.SessionId, value, Ct));

        Assert.Equal(ErrorCode.ReauthenticationRequired, wrongOperation.Code);
        Assert.True(first.IsDeleted);
        Assert.Equal(ErrorCode.NotFound, second.Code);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records WHERE dispatch_type = 'AccountDeletedNotice'"));
    }

    // ---- KK8: the administrator's deletion on the data subject's request ----

    [Fact]
    public async Task DeleteOnRequest_GivesTheSameResult_AndWritesATrailRowThatNamesNothingOfTheAccount_KK8()
    {
        Guid adminId = await SeedAdminActorAsync();
        (Guid id, _) = await SeedMemberWithEverythingAsync("ali@example.com");
        await SeedGuestOrderAsync("ali@example.com", id);

        AccountDeletionResult result = await RunAsync(m => m.DeleteAccountOnRequestAsync(id, Ct));

        Assert.True(result.IsDeleted);
        Assert.Equal(1, result.OrdersKept);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM customers"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records WHERE dispatch_type = 'AccountDeletedNotice'"));
        await using AppDbContext db = NewContext();
        AuditLog row = await db.AuditLogs.AsNoTracking().SingleAsync(Ct);
        Assert.Equal(nameof(AuditOperation.DeleteCustomerAccount), row.Operation);
        Assert.Equal(adminId, row.AdminId);
        Assert.Equal("Customer", row.TargetRecordType);
        Assert.Equal(id, row.TargetRecordId);
        Assert.NotNull(row.Details);
        Assert.Null(row.Details!.DisplayName);
        string text = System.Text.Json.JsonSerializer.Serialize(row.Details);
        Assert.DoesNotContain("ali@example.com", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Ali", text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeleteOnRequest_WithoutAnActor_Throws_AndAnUnverifiedRegistrationIsNotFound()
    {
        Guid verified = await SeedCustomerAsync("ali@example.com", verified: true);
        Guid pending = await SeedCustomerAsync("veli@example.com", verified: false);

        AccountDeletionResult unverified = await RunAsync(m => m.DeleteAccountOnRequestAsync(pending, Ct));
        await Assert.ThrowsAsync<InvalidOperationException>(() => RunAsync(m => m.DeleteAccountOnRequestAsync(verified, Ct)));

        Assert.Equal(ErrorCode.NotFound, unverified.Code);
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM customers"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    // ---- the member record view (UC-95) ----

    [Fact]
    public async Task TheMemberRecordView_ReadsTheAccount_AndWritesNothing_KK7()
    {
        (Guid id, _) = await SeedMemberWithEverythingAsync("ali@example.com");
        await SeedGuestOrderAsync("ali@example.com", id);
        await SeedCustomerAsync("bekleyen@example.com", verified: false);
        string before = await StateAsync();

        MemberRecord account = await ReadAsync(m => m.FindMemberRecordAsync("  ALI@example.com ", Ct));
        MemberRecord pending = await ReadAsync(m => m.FindMemberRecordAsync("bekleyen@example.com", Ct));
        MemberRecord none = await ReadAsync(m => m.FindMemberRecordAsync("yok@example.com", Ct));

        Assert.True(account.Exists && account.IsAccount);
        Assert.Equal(id, account.CustomerId);
        Assert.Equal("Ali", account.Name);
        Assert.Single(account.Addresses);
        Assert.True(account.HasPassword);
        Assert.False(account.IsGoogleLinked);
        Assert.True(pending.Exists && !pending.IsAccount);
        Assert.Null(pending.Name);
        Assert.Empty(pending.Addresses);
        Assert.False(none.Exists || none.IsAccount);
        Assert.Equal(before, await StateAsync());
    }

    // ---- KK6, KK7: no table, no state machine, no counter of its own; no export or download surface ----

    [Fact]
    public void ThePersonalDataRequest_HasNoTableStateMachineOrExportOfItsOwn_KK6_KK7()
    {
        Assert.Contains(ContactTopic.PersonalDataRequest, Enum.GetValues<ContactTopic>());
        Assert.Equal(2, Enum.GetValues<ContactRequestStatus>().Length);
        Assert.DoesNotContain(typeof(AppDbContext).GetProperties(), p => p.Name.Contains("Kvkk", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("PersonalData", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(typeof(AppDbContext).Assembly.GetTypes(), t => t.Name.Contains("Kvkk", StringComparison.OrdinalIgnoreCase));
        string[] forbidden = ["Export", "Download", "Archive", "Backup"];
        Assert.DoesNotContain(typeof(IMembershipModule).GetMethods(), m => forbidden.Any(f => m.Name.Contains(f, StringComparison.OrdinalIgnoreCase)));
        Assert.All(typeof(MemberRecord).GetProperties(), p => Assert.True(p.SetMethod is null || p.SetMethod.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(System.Runtime.CompilerServices.IsExternalInit))));
    }
}
