using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Administration.Contracts.Results;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>
/// T379 on the real Administration module, the real link layer, the real notification writers and the real database: the invitation and the account opened from it (k1, k2; KK1, KK2),
/// the removal (k3, k4; KK3, KK4), the one role (k5; KK5), the e-mail and the name (k6; KK6), the two account types (k7; KK7), the withdrawal and the address rules (k8; KK8). Every
/// call runs in the runner's transaction the way a page handler does, so a refusal rolls back. The acting administrator is the one the test names (<see cref="SwitchableAuditActor"/>).
/// </summary>
public sealed partial class MembershipTests
{
    private async Task<(Guid Id, string Name, string Email)> StartAdminWorldAsync(string email, string name = "Yönetici")
    {
        await ExecuteAsync("TRUNCATE audit_logs, admin_invitations, companies, sessions CASCADE");
        Admin admin = Admin.Create(name, email, SeedDigest.Value, Start);
        await SaveAsync(c => c.Admins.Add(admin));
        ActAs(admin.Id, name, email);
        return (admin.Id, name, email);
    }

    private void ActAs(Guid id, string name, string email) => _provider.GetRequiredService<SwitchableAuditActor>().Actor = new AuditActor(id, name, email);

    private void ActAsNobody() => _provider.GetRequiredService<SwitchableAuditActor>().Actor = null;

    private async Task<AdminInvitationSendResult> SendInvitationAsync(string email) => await RunAdminAsync(a => a.SendInvitationAsync(email, Ct));

    private async Task<AdminInvitationReading> ReadInvitationAsync(Guid linkId)
    {
        string key = _keys.Derive(linkId);
        return await InTransactionAsync(s => s.GetRequiredService<IAdministrationModule>().ReadInvitationLinkAsync(key, Ct));
    }

    private Task<AdminInvitationAcceptance> AcceptAsync(Guid linkId, string name, string password)
    {
        string key = _keys.Derive(linkId);
        return RunAdminAsync(a => a.AcceptInvitationAsync(key, name, password, Ct));
    }

    private async Task<List<DispatchRecord>> DispatchesAsync(DispatchType type)
    {
        await using AppDbContext db = NewContext();
        return await db.DispatchRecords.AsNoTracking().Where(d => d.DispatchType == type).ToListAsync(Ct);
    }

    // ---- k1, k2, KK1, KK2: the invitation is the only way in; the link is single use; the name is required; the password policy does not consume it ----

    [Fact]
    public async Task Invitation_IsSentWithItsLinkEmailTrailRowAndF6_AndOpensTheAccountOnce_KK1_KK2()
    {
        var first = await StartAdminWorldAsync("ilk@example.com");
        await SaveAsync(c => c.Admins.Add(Admin.Create("İkinci", "ikinci@example.com", SeedDigest.Value, Start)));

        AdminInvitationSendResult sent = await SendInvitationAsync("  Yeni@Example.com ");

        Assert.True(sent.IsSent);
        Assert.Equal(Start.AddHours(ProductConstants.AdminInvitationLinkHours), sent.ExpiresAt);
        await using (AppDbContext db = NewContext())
        {
            AdminInvitation invitation = await db.AdminInvitations.AsNoTracking().SingleAsync(Ct);
            Assert.Equal("Yeni@Example.com", invitation.Email);
            Assert.Equal(first.Id, invitation.SenderAdminId);
            Assert.Equal(first.Name, invitation.SenderAdminName);
        }

        DispatchRecord mail = Assert.Single(await DispatchesAsync(DispatchType.AdminInvitation));
        Assert.Equal("Yeni@Example.com", mail.RecipientEmail);
        List<DispatchRecord> notices = await DispatchesAsync(DispatchType.F6);
        Assert.Equal(["ikinci@example.com", "ilk@example.com"], notices.Select(n => n.RecipientEmail!).Order(StringComparer.Ordinal).ToArray());
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs WHERE operation = 'SendAdminInvitation'"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM attempt_records"));

        AdminInvitationReading reading = await ReadInvitationAsync(sent.LinkId!.Value);
        Assert.Equal(AccountLinkStatus.Valid, reading.Status);
        Assert.Equal("Yeni@Example.com", reading.Email);

        // The name is required and the password policy is the one policy: neither consumes the link.
        AdminInvitationAcceptance noName = await AcceptAsync(sent.LinkId!.Value, "   ", Entered);
        AdminInvitationAcceptance weak = await AcceptAsync(sent.LinkId!.Value, "Yeni Yönetici", ShortEntry);
        Assert.Equal(ErrorCode.FieldRequired, noName.Code);
        Assert.Equal(AdminAccountFields.Name, noName.Field);
        Assert.Equal(ErrorCode.PasswordTooWeak, weak.Code);
        Assert.Equal(AdminAccountFields.Password, weak.Field);
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM admins"));
        Assert.Equal(AccountLinkStatus.Valid, (await ReadInvitationAsync(sent.LinkId!.Value)).Status);

        AdminInvitationAcceptance created = await AcceptAsync(sent.LinkId!.Value, " Yeni Yönetici ", Entered);
        AdminInvitationAcceptance again = await AcceptAsync(sent.LinkId!.Value, "Yeni Yönetici", Entered);

        Assert.True(created.IsCreated);
        Assert.Equal(AccountLinkStatus.Used, again.LinkStatus);
        await using (AppDbContext db = NewContext())
        {
            Admin admin = await db.Admins.AsNoTracking().SingleAsync(a => a.Id == created.AdminId, Ct);
            Assert.Equal("Yeni Yönetici", admin.Name);
            Assert.Equal("Yeni@Example.com", admin.Email);
            Assert.NotNull(admin.PasswordHash);
            Assert.Equal(0, await db.Sessions.CountAsync(s => s.AdminId == admin.Id, Ct));
            Assert.NotNull((await db.AdminInvitations.AsNoTracking().SingleAsync(Ct)).AcceptedAt);
        }

        // The new administrator signs in with the password they set.
        AdminCredentialCheckResult signIn = await InTransactionAsync(s => s.GetRequiredService<IAdministrationModule>().CheckCredentialsAsync("yeni@example.com", Entered, Ip, null, Ct));
        Assert.True(signIn.Succeeded);
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM audit_logs WHERE operation = 'CreateAdminAccount' AND admin_id = '{created.AdminId}'"));
        Assert.Equal(2, (await DispatchesAsync(DispatchType.F6)).Count);
    }

    [Fact]
    public async Task Invitation_RunsOutAfterItsLinkLife_AndNoOneIsNotified_KK2_KK8()
    {
        await StartAdminWorldAsync("ilk@example.com");
        AdminInvitationSendResult sent = await SendInvitationAsync("yeni@example.com");
        int noticesBefore = (await DispatchesAsync(DispatchType.F6)).Count;

        _time.Advance(TimeSpan.FromHours(ProductConstants.AdminInvitationLinkHours) + TimeSpan.FromSeconds(1));
        AdminInvitationAcceptance late = await AcceptAsync(sent.LinkId!.Value, "Yeni", Entered);

        Assert.Equal(AccountLinkStatus.Expired, late.LinkStatus);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM admins"));
        Assert.Equal(noticesBefore, (await DispatchesAsync(DispatchType.F6)).Count);
    }

    // ---- k8, KK8: the address rules ----

    [Fact]
    public async Task Send_RefusesAnAdministratorsAddressInAnyLetterCase_AndAMalformedOne_ButNotACustomersAddress_KK7_KK8()
    {
        await StartAdminWorldAsync("ilk@example.com");
        await SeedCustomerAsync("musteri@example.com", verified: true);

        AdminInvitationSendResult taken = await SendInvitationAsync("ILK@EXAMPLE.COM");
        AdminInvitationSendResult malformed = await SendInvitationAsync("not-an-address");
        AdminInvitationSendResult empty = await SendInvitationAsync("  ");
        AdminInvitationSendResult toACustomer = await SendInvitationAsync("Musteri@Example.com");

        Assert.Equal(ErrorCode.AdminEmailTaken, taken.Code);
        Assert.Equal(AdminAccountFields.Email, taken.Field);
        Assert.Equal(ErrorCode.InvalidEmail, malformed.Code);
        Assert.Equal(ErrorCode.FieldRequired, empty.Code);
        Assert.True(toACustomer.IsSent);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM admin_invitations"));
        Assert.Contains(await DispatchesAsync(DispatchType.AdminInvitation), d => d.RecipientEmail == "Musteri@Example.com");

        // The invited address opens an administrator account beside the customer account: two account types, each unique in its own type.
        AdminInvitationAcceptance created = await AcceptAsync(toACustomer.LinkId!.Value, "Müşteri Yönetici", Entered);
        Assert.True(created.IsCreated);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM customers WHERE normalized_email = 'musteri@example.com'"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM admins WHERE normalized_email = 'musteri@example.com'"));
    }

    [Fact]
    public async Task ANewInvitationToTheSameAddress_MakesTheEarlierOneInvalid_AndOnlyTheSendingsWriteF6_KK8()
    {
        await StartAdminWorldAsync("ilk@example.com");

        AdminInvitationSendResult first = await SendInvitationAsync("yeni@example.com");
        AdminInvitationSendResult second = await SendInvitationAsync("YENI@example.com");

        Assert.True(second.IsSent);
        Assert.Equal(1, second.InvalidatedEarlierInvitations);
        Assert.Equal(AccountLinkStatus.Expired, (await ReadInvitationAsync(first.LinkId!.Value)).Status);
        Assert.Equal(AccountLinkStatus.Valid, (await ReadInvitationAsync(second.LinkId!.Value)).Status);
        AdminInvitationAcceptance old = await AcceptAsync(first.LinkId!.Value, "Eski", Entered);
        Assert.Equal(AccountLinkStatus.Expired, old.LinkStatus);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM admins"));
        Assert.Equal(2, (await DispatchesAsync(DispatchType.F6)).Count);

        // A run-out invitation still holds its address: a new one made after the life ended makes it invalid too, so the address has one valid invitation.
        _time.Advance(TimeSpan.FromHours(ProductConstants.AdminInvitationLinkHours) + TimeSpan.FromMinutes(1));
        AdminInvitationSendResult third = await SendInvitationAsync("yeni@example.com");
        Assert.True(third.IsSent);
        Assert.Equal(1, third.InvalidatedEarlierInvitations);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM admin_invitations WHERE invalidated_at IS NULL"));
    }

    [Fact]
    public async Task Withdrawal_EndsTheLinkAtOnce_WritesTheTrailRow_AndMakesNoNotice_KK8()
    {
        await StartAdminWorldAsync("ilk@example.com");
        AdminInvitationSendResult sent = await SendInvitationAsync("yeni@example.com");
        int mails = (await DispatchesAsync(DispatchType.AdminInvitation)).Count;
        int notices = (await DispatchesAsync(DispatchType.F6)).Count;

        AdminInvitationWithdrawalResult withdrawn = await RunAdminAsync(a => a.WithdrawInvitationAsync(sent.InvitationId!.Value, Ct));
        AdminInvitationWithdrawalResult second = await RunAdminAsync(a => a.WithdrawInvitationAsync(sent.InvitationId!.Value, Ct));
        AdminInvitationWithdrawalResult missing = await RunAdminAsync(a => a.WithdrawInvitationAsync(Guid.NewGuid(), Ct));
        AdminInvitationAcceptance late = await AcceptAsync(sent.LinkId!.Value, "Yeni", Entered);

        Assert.True(withdrawn.IsWithdrawn);
        Assert.Equal(AdminInvitationWithdrawalOutcome.NoLongerActive, second.Outcome);
        Assert.Equal(ErrorCode.InvitationNoLongerActive, second.Code);
        Assert.Equal(AdminInvitationWithdrawalOutcome.NotFound, missing.Outcome);
        Assert.Equal(AccountLinkStatus.Expired, late.LinkStatus);
        Assert.Equal(Start, await ScalarAsync<DateTimeOffset>("SELECT withdrawn_at FROM admin_invitations"));
        Assert.Equal(Start, await ScalarAsync<DateTimeOffset>($"SELECT expires_at FROM link_records WHERE id = '{sent.LinkId}'"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs WHERE operation = 'WithdrawAdminInvitation'"));
        Assert.Equal(mails, (await DispatchesAsync(DispatchType.AdminInvitation)).Count);
        Assert.Equal(notices, (await DispatchesAsync(DispatchType.F6)).Count);

        // A withdrawn address can be invited again at once.
        Assert.True((await SendInvitationAsync("yeni@example.com")).IsSent);
    }

    [Fact]
    public async Task Acceptance_WhenTheAddressBecameAnAdministratorsMeanwhile_OpensNoAccount_AndMakesTheInvitationInvalid_K1085_KK8()
    {
        await StartAdminWorldAsync("ilk@example.com");
        AdminInvitationSendResult sent = await SendInvitationAsync("yeni@example.com");
        await SaveAsync(c => c.Admins.Add(Admin.Create("Komut", "YENI@example.com", SeedDigest.Value, Start)));

        AdminInvitationReading reading = await ReadInvitationAsync(sent.LinkId!.Value);
        AdminInvitationAcceptance result = await AcceptAsync(sent.LinkId!.Value, "Yeni", Entered);
        AdminInvitationAcceptance again = await AcceptAsync(sent.LinkId!.Value, "Yeni", Entered);

        Assert.Equal(AccountLinkStatus.Expired, reading.Status);
        Assert.Null(reading.Email);
        Assert.Equal(AdminInvitationAcceptanceOutcome.LinkInvalid, result.Outcome);
        Assert.Equal(AccountLinkStatus.Expired, result.LinkStatus);
        Assert.Equal(AccountLinkStatus.Expired, again.LinkStatus);
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM admins"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM admin_invitations WHERE invalidated_at IS NOT NULL AND accepted_at IS NULL"));
    }

    [Fact]
    public async Task Send_AsSomeoneWhoIsNoAdministrator_WritesNothing_AndATrailRowNeedsTheActor()
    {
        await StartAdminWorldAsync("ilk@example.com");
        ActAsNobody();

        AdminInvitationSendResult sent = await SendInvitationAsync("yeni@example.com");

        Assert.Equal(AdminInvitationSendOutcome.ActorNotAdministrator, sent.Outcome);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM admin_invitations"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records"));
    }

    [Fact]
    public async Task TheInvitationsButton_AndTheManagementCommand_OnOneAddressAtOnce_OpenOneAccount_K1085()
    {
        await StartAdminWorldAsync("ilk@example.com");
        AdminInvitationSendResult sent = await SendInvitationAsync("yeni@example.com");

        Task<AdminInvitationAcceptance> accept = AcceptAsync(sent.LinkId!.Value, "Davetli", Entered);
        Task<AdminCreationResult> command = RunAdminAsync(a => a.CreateAdminAsync(Guid.NewGuid(), "Komut", "yeni@example.com", Entered, Ct));
        await Task.WhenAll(accept, command);

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM admins WHERE normalized_email = 'yeni@example.com'"));
        Assert.True((await accept).IsCreated != (await command).IsCreated);
    }

    // ---- k3, k4, KK3, KK4: the removal ----

    [Fact]
    public async Task Removal_RefusesTheActingAdministratorThemselves_AndTheLastOne_KK3()
    {
        var only = await StartAdminWorldAsync("ilk@example.com");

        AdminRemovalResult self = await RunAdminAsync(a => a.RemoveAdminAsync(only.Id, true, Ct));
        AdminRemovalResult missing = await RunAdminAsync(a => a.RemoveAdminAsync(Guid.NewGuid(), true, Ct));

        Assert.Equal(AdminRemovalOutcome.Refused, self.Outcome);
        Assert.Equal(ErrorCode.LastAdminRemoval, self.Code);
        Assert.Equal(AdminRemovalOutcome.NotFound, missing.Outcome);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM admins"));

        // With two administrators the acting one still cannot remove themselves.
        Guid other = await SeedAdminAsync("ikinci@example.com");
        AdminRemovalResult selfAgain = await RunAdminAsync(a => a.RemoveAdminAsync(only.Id, true, Ct));
        Assert.Equal(AdminRemovalOutcome.Refused, selfAgain.Outcome);
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM admins"));
        Assert.NotEqual(Guid.Empty, other);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs WHERE operation = 'RemoveAdminAccount'"));
    }

    [Fact]
    public async Task Removal_AsksForTheConfirmationWithTheCount_ThenEndsSessionsAndInvitations_KeepsTheTrail_AndTellsEveryAdministrator_KK4()
    {
        var acting = await StartAdminWorldAsync("ilk@example.com", "Ayşe");
        Guid targetId = await SeedAdminAsync("hedef@example.com");
        Guid thirdId = await SeedAdminAsync("ucuncu@example.com");
        SessionOpening one = await InTransactionAsync(s => s.GetRequiredService<IAdministrationModule>().SignInAsync(targetId, false, null, Ct));
        SessionOpening two = await InTransactionAsync(s => s.GetRequiredService<IAdministrationModule>().SignInAsync(targetId, true, null, Ct));
        Assert.NotEqual(one.SessionId, two.SessionId);

        // The target sends two invitations and the acting administrator sends one: the removal counts only the target's.
        ActAs(targetId, "Yönetici", "hedef@example.com");
        AdminInvitationSendResult sentA = await SendInvitationAsync("a@example.com");
        AdminInvitationSendResult sentB = await SendInvitationAsync("b@example.com");
        ActAs(acting.Id, acting.Name, acting.Email);
        AdminInvitationSendResult mine = await SendInvitationAsync("c@example.com");
        int noticesBefore = (await DispatchesAsync(DispatchType.F6)).Count;

        AdminRemovalResult asked = await RunAdminAsync(a => a.RemoveAdminAsync(targetId, false, Ct));

        Assert.Equal(AdminRemovalOutcome.ConfirmationNeeded, asked.Outcome);
        Assert.Equal("Yönetici", asked.RemovedName);
        Assert.Equal("hedef@example.com", asked.RemovedEmail);
        Assert.Equal(2, asked.InvalidatedInvitations);
        Assert.Equal(3L, await ScalarAsync<long>("SELECT count(*) FROM admins"));
        Assert.Equal(noticesBefore, (await DispatchesAsync(DispatchType.F6)).Count);

        AdminRemovalResult removed = await RunAdminAsync(a => a.RemoveAdminAsync(targetId, true, Ct));

        Assert.True(removed.IsRemoved);
        Assert.Equal(2, removed.InvalidatedInvitations);
        Assert.Equal(2, removed.EndedSessions);
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM admins"));
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM sessions WHERE admin_id = '{targetId}'"));
        Assert.Equal(AccountLinkStatus.Expired, (await ReadInvitationAsync(sentA.LinkId!.Value)).Status);
        Assert.Equal(AccountLinkStatus.Expired, (await ReadInvitationAsync(sentB.LinkId!.Value)).Status);
        Assert.Equal(AccountLinkStatus.Valid, (await ReadInvitationAsync(mine.LinkId!.Value)).Status);
        AdminInvitationAcceptance late = await AcceptAsync(sentA.LinkId!.Value, "A", Entered);
        Assert.Equal(AccountLinkStatus.Expired, late.LinkStatus);

        // The trail rows of the removed one stay with their name and e-mail, and the removal's own row is the acting administrator's with the counted invitations.
        Assert.Equal(2L, await ScalarAsync<long>($"SELECT count(*) FROM audit_logs WHERE admin_id = '{targetId}' AND admin_email = 'hedef@example.com' AND admin_name = 'Yönetici'"));
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM audit_logs WHERE operation = 'RemoveAdminAccount' AND admin_id = '{acting.Id}' AND target_record_id = '{targetId}'"));
        string details = await ScalarAsync<string>("SELECT details::text FROM audit_logs WHERE operation = 'RemoveAdminAccount'");
        Assert.Contains("hedef@example.com", details);
        Assert.Contains("InvalidatedInvitations", details);

        // F-6 reaches every administrator, the removed one included.
        List<DispatchRecord> notices = (await DispatchesAsync(DispatchType.F6)).Skip(noticesBefore).ToList();
        Assert.Equal(3, notices.Count);
        Assert.Equal(["hedef@example.com", "ilk@example.com", "ucuncu@example.com"], notices.Select(n => n.RecipientEmail!).Order(StringComparer.Ordinal).ToArray());
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM disposal_records WHERE disposal_record_type = 'Admin'"));
        Assert.NotEqual(Guid.Empty, thirdId);

        // The removed administrator cannot sign in any more, and the removal of one already gone is a not-found.
        AdminCredentialCheckResult signIn = await InTransactionAsync(s => s.GetRequiredService<IAdministrationModule>().CheckCredentialsAsync("hedef@example.com", Entered, Ip, null, Ct));
        Assert.False(signIn.Succeeded);
        AdminRemovalResult gone = await RunAdminAsync(a => a.RemoveAdminAsync(targetId, true, Ct));
        Assert.Equal(AdminRemovalOutcome.NotFound, gone.Outcome);
    }

    [Fact]
    public async Task Send_ByAnAdministratorWhoseRowIsGone_WritesNothing_KK3()
    {
        var acting = await StartAdminWorldAsync("ilk@example.com");
        await SeedAdminAsync("hedef@example.com");
        await ExecuteAsync($"DELETE FROM admins WHERE id = '{acting.Id}'");

        AdminInvitationSendResult result = await SendInvitationAsync("yeni@example.com");

        Assert.Equal(AdminInvitationSendOutcome.ActorNotAdministrator, result.Outcome);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM admin_invitations"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records"));
    }

    // ---- k5, KK5: one role ----

    [Fact]
    public void TheAccountAndTheInvitation_HaveNoRoleOrPermissionField_KK5()
    {
        string[] forbidden = ["Role", "Permission", "Privilege", "Authority", "Level", "IsOwner", "IsSuper"];
        foreach (Type type in new[] { typeof(Admin), typeof(AdminInvitation) })
        {
            IEnumerable<string> names = type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).Select(p => p.Name);
            Assert.DoesNotContain(names, n => forbidden.Any(f => n.Contains(f, StringComparison.OrdinalIgnoreCase)));
        }

        IEnumerable<string> contract = typeof(IAdministrationModule).GetMethods().Select(m => m.Name);
        Assert.DoesNotContain(contract, n => forbidden.Any(f => n.Contains(f, StringComparison.OrdinalIgnoreCase)));
    }

    // ---- k6, KK6: the e-mail on the customer's regime, the name without reauthentication ----

    [Fact]
    public async Task TheName_ChangesWithoutReauthentication_AndEmptyIsRefused_KK6()
    {
        var me = await StartAdminWorldAsync("ilk@example.com");

        AdminNameChangeResult empty = await RunAdminAsync(a => a.ChangeNameAsync(me.Id, "   ", Ct));
        AdminNameChangeResult done = await RunAdminAsync(a => a.ChangeNameAsync(me.Id, "  Yeni Ad ", Ct));

        Assert.Equal(ErrorCode.FieldRequired, empty.Code);
        Assert.Equal(AdminAccountFields.Name, empty.Field);
        Assert.True(done.IsDone);
        Assert.Equal("Yeni Ad", await ScalarAsync<string>($"SELECT name FROM admins WHERE id = '{me.Id}'"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM link_records"));
    }

    [Fact]
    public async Task TheEmail_ChangesAfterTheReauthentication_WithTheNewAddressLink_TheRevertNotice_TheTrailRow_AndTheFirmContactUntouched_KK6()
    {
        var me = await StartAdminWorldAsync("ilk@example.com");
        await ExecuteAsync("INSERT INTO companies (id, created_at, email) VALUES (gen_random_uuid(), now(), 'iletisim@firma.example')");
        await SeedCustomerAsync("yeni@example.com", verified: true);
        SessionOpening session = await InTransactionAsync(s => s.GetRequiredService<IAdministrationModule>().SignInAsync(me.Id, false, null, Ct));

        AdminEmailChangeRequestResult without = await RunAdminAsync(a => a.RequestEmailChangeAsync(me.Id, session.SessionId, "yeni@example.com", null, Ip, Ct));
        ReauthenticationResult wrong = await RunAdminAsync(a => a.ReauthenticateAsync(me.Id, session.SessionId, ReauthenticationOperation.EmailChange, Wrong, Ip, session.MarkValue, Ct));
        ReauthenticationResult granted = await RunAdminAsync(a => a.ReauthenticateAsync(me.Id, session.SessionId, ReauthenticationOperation.EmailChange, Entered, Ip, session.MarkValue, Ct));
        AdminEmailChangeRequestResult taken = await RunAdminAsync(a => a.RequestEmailChangeAsync(me.Id, session.SessionId, "ILK@example.com", granted.Value, Ip, Ct));
        AdminEmailChangeRequestResult requested = await RunAdminAsync(a => a.RequestEmailChangeAsync(me.Id, session.SessionId, "Yeni@Example.com", granted.Value, Ip, Ct));
        AdminEmailChangeRequestResult reused = await RunAdminAsync(a => a.RequestEmailChangeAsync(me.Id, session.SessionId, "baska@example.com", granted.Value, Ip, Ct));

        Assert.Equal(AdminEmailChangeOutcome.ReauthenticationRequired, without.Outcome);
        Assert.Equal(ErrorCode.ReauthenticationFailed, wrong.Code);
        Assert.Equal(AdminEmailChangeOutcome.FieldRefused, taken.Outcome);
        Assert.Equal(ErrorCode.AdminEmailTaken, taken.Code);
        Assert.True(requested.IsRequested);
        Assert.Equal(AdminEmailChangeOutcome.ReauthenticationRequired, reused.Outcome);
        Assert.Equal("ilk@example.com", await ScalarAsync<string>($"SELECT email FROM admins WHERE id = '{me.Id}'"));
        Assert.Contains(await DispatchesAsync(DispatchType.NewEmailVerificationLink), d => d.RecipientEmail == "Yeni@Example.com");

        string key = _keys.Derive(requested.LinkId!.Value);
        AdminEmailVerificationResult verified = await InTransactionAsync(s => s.GetRequiredService<IAdministrationModule>().VerifyNewEmailAsync(key, Ct));
        AdminEmailVerificationResult again = await InTransactionAsync(s => s.GetRequiredService<IAdministrationModule>().VerifyNewEmailAsync(key, Ct));

        Assert.True(verified.IsVerified);
        Assert.Equal(AccountLinkStatus.Used, again.LinkStatus);
        Assert.Equal("Yeni@Example.com", await ScalarAsync<string>($"SELECT email FROM admins WHERE id = '{me.Id}'"));
        Assert.Equal("yeni@example.com", await ScalarAsync<string>($"SELECT normalized_email FROM admins WHERE id = '{me.Id}'"));
        Assert.Equal("iletisim@firma.example", await ScalarAsync<string>("SELECT email FROM companies"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM customers WHERE normalized_email = 'yeni@example.com'"));
        Assert.Contains(await DispatchesAsync(DispatchType.EmailChangeNotice), d => d.RecipientEmail == "ilk@example.com");
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM audit_logs WHERE operation = 'ChangeAdminEmail' AND admin_id = '{me.Id}'"));
        string details = await ScalarAsync<string>("SELECT details::text FROM audit_logs WHERE operation = 'ChangeAdminEmail'");
        Assert.Contains("ilk@example.com", details);
        Assert.Contains("Yeni@Example.com", details);

        // The revert link is open: a following change is closed until it ends (the customer's rule).
        ReauthenticationResult next = await RunAdminAsync(a => a.ReauthenticateAsync(me.Id, session.SessionId, ReauthenticationOperation.EmailChange, Entered, Ip, session.MarkValue, Ct));
        AdminEmailChangeRequestResult closed = await RunAdminAsync(a => a.RequestEmailChangeAsync(me.Id, session.SessionId, "sonraki@example.com", next.Value, Ip, Ct));
        Assert.Equal(AdminEmailChangeOutcome.RevertPending, closed.Outcome);
        Assert.Equal(ErrorCode.EmailChangeRevertPending, closed.Code);
    }

    [Fact]
    public async Task TheEmailRequestsThatReplaceAPendingOne_AreLimitedByL9_AndTheEarlierLinkStaysValid_KK6()
    {
        var me = await StartAdminWorldAsync("ilk@example.com");
        SessionOpening session = await InTransactionAsync(s => s.GetRequiredService<IAdministrationModule>().SignInAsync(me.Id, false, null, Ct));
        Guid lastLink = Guid.Empty;
        AdminEmailChangeRequestResult? limited = null;

        for (int i = 0; i < ProductConstants.VerificationResendAttemptsPerEmail + 3 && limited is null; i++)
        {
            ReauthenticationResult granted = await RunAdminAsync(a => a.ReauthenticateAsync(me.Id, session.SessionId, ReauthenticationOperation.EmailChange, Entered, Ip, session.MarkValue, Ct));
            string address = $"yeni{i}@example.com";
            AdminEmailChangeRequestResult result = await RunAdminAsync(a => a.RequestEmailChangeAsync(me.Id, session.SessionId, address, granted.Value, Ip, Ct));
            if (result.Outcome == AdminEmailChangeOutcome.Limited)
            {
                limited = result;
            }
            else
            {
                Assert.True(result.IsRequested);
                lastLink = result.LinkId!.Value;
            }
        }

        Assert.NotNull(limited);
        Assert.Equal(ErrorCode.VerificationLinkLimitExceeded, limited.Code);
        Assert.NotEqual(Guid.Empty, lastLink);
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM link_records WHERE id = '{lastLink}' AND superseded_at IS NULL AND used_at IS NULL"));
    }

    // ---- k7, KK7: no cart or order for an administrator; the two account types are separate tables and doors ----

    [Fact]
    public void AdministratorAccounts_HaveNoCartOrOrderRelation_KK7()
    {
        using AppDbContext db = NewContext();
        Microsoft.EntityFrameworkCore.Metadata.IEntityType admin = db.Model.FindEntityType(typeof(Admin))!;
        IEnumerable<string> referencing = db.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys().Where(f => f.PrincipalEntityType == admin).Select(_ => e.ClrType.Name))
            .Distinct()
            .Order(StringComparer.Ordinal)
            .ToList();

        Assert.DoesNotContain(referencing, n => n is "Cart" or "Order" or "CartItem" or "OrderItem");
    }

    [Fact]
    public async Task ACustomerAddress_DoesNotSignInToThePanel_AndAnAdministratorAddress_DoesNotSignInToTheShop_KK7()
    {
        await StartAdminWorldAsync("yonetici@example.com");
        await SeedCustomerAsync("musteri@example.com", verified: true);

        AdminCredentialCheckResult customerAtPanel = await InTransactionAsync(s => s.GetRequiredService<IAdministrationModule>().CheckCredentialsAsync("musteri@example.com", Entered, Ip, null, Ct));
        AdminCredentialCheckResult adminAtPanel = await InTransactionAsync(s => s.GetRequiredService<IAdministrationModule>().CheckCredentialsAsync("yonetici@example.com", Entered, Ip, null, Ct));

        Assert.False(customerAtPanel.Succeeded);
        Assert.True(adminAtPanel.Succeeded);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM customers WHERE normalized_email = 'yonetici@example.com'"));
    }
}
