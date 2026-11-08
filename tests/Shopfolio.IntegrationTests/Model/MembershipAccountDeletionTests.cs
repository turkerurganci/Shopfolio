using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

/// <summary>The administrator the test's audit trail names; empty until a test sets it (an operation that needs an actor then throws).</summary>
internal sealed class AuditActorHolder
{
    public AuditActor? Current { get; set; }
}

internal sealed class HeldAuditActorSource(AuditActorHolder holder) : IAuditActorSource
{
    public Task<AuditActor?> CurrentAsync(CancellationToken ct) => Task.FromResult(holder.Current);
}

/// <summary>
/// T345: the deletion of an account and the personal data request (02 3.15; 03 9.4; 07 UC-68, UC-157, UC-95). The module's own rules are sampled here on the real database: what goes with
/// the account, what stays with the order, the one e-mail, the revert link's gate, the reauthentication and the administrator's trail row.
/// </summary>
public sealed partial class MembershipTests
{
    private readonly AuditActorHolder _actor = new();

    private async Task<Guid> SeedAdminActorAsync()
    {
        Admin admin = Admin.Create("Yonetici", "yonetici@example.com", SeedDigest.Value, Start);
        await SaveAsync(c => c.Admins.Add(admin));
        _actor.Current = new AuditActor(admin.Id, admin.Name, admin.Email);
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
