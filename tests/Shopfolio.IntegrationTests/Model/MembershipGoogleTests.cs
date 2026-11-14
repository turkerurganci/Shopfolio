using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Membership.Contracts.Results;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>
/// The Google sign-in's account rules and the e-mail change with its revert link through the real Membership module on a real PostgreSQL and a fake clock (T343 criteria 1-9;
/// KK1-KK9). The Google application is a conditional line the test opens and closes; the account e-mail writer, the link layer, the attempt counter and the transaction runner
/// are the real ones, and every call runs in the runner's transaction the way a page handler does, so a refusal rolls back and a born account is saved by the runner.
/// </summary>
public sealed partial class MembershipTests
{
    private static readonly GoogleIdentity AliAtGmail = new("ali@gmail.com", true, "Ali Veli", null);

    // ---- KK2: no application, no button, no flow ----

    [Fact]
    public async Task GoogleSignIn_WithTheLineClosed_IsUnavailable_AndReadsAndWritesNothing_KK2()
    {
        Assert.False(ReadOpen());

        GoogleSignInResult result = await RunAsync(m => m.SignInWithGoogleAsync(AliAtGmail, Ip, Ct));

        Assert.Equal(GoogleSignInOutcome.Unavailable, result.Outcome);
        Assert.True(result.IsRejected);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM customers"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM login_records"));
    }

    [Fact]
    public void GoogleSignIn_IsOpenOnlyWhenTheClientIdAndTheSecretAreBothPresent_KK2()
    {
        _lines.Add(new ConditionalLine(ConditionalLine.GoogleSignIn, [GoogleOptions.ClientSecretKey]));
        Assert.False(ReadOpen());

        _lines.Clear();
        _lines.Add(new ConditionalLine(ConditionalLine.CardPayment, []));
        Assert.False(ReadOpen());

        _lines.Add(new ConditionalLine(ConditionalLine.GoogleSignIn, []));
        Assert.True(ReadOpen());
    }

    // ---- criterion 3, KK3: one account, two ways in; the pending registration goes ----

    [Fact]
    public async Task GoogleSignIn_OfANewAddress_OpensAPasswordlessVerifiedLinkedAccount_NamedByGoogle_AndCountsNoAttempt()
    {
        OpenGoogle();

        GoogleSignInResult result = await RunAsync(m => m.SignInWithGoogleAsync(AliAtGmail, Ip, Ct));

        Assert.True(result.IsSignedIn);
        Assert.True(result.AccountCreated);
        Assert.False(result.IsRejected);
        await using AppDbContext db = NewContext();
        Customer customer = await db.Customers.AsNoTracking().SingleAsync(Ct);
        Assert.Equal(result.CustomerId, customer.Id);
        Assert.Equal("Ali Veli", customer.Name);
        Assert.Null(customer.PasswordHash);
        Assert.Equal(Start, customer.EmailVerifiedAt);
        Assert.Equal(Start, customer.GoogleLinkedAt);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM attempt_records"));
        Assert.Equal([LoginOutcome.Succeeded], await OutcomesAsync());
    }

    [Fact]
    public async Task GoogleSignIn_WithoutAGoogleName_TakesThePartBeforeTheAtSign()
    {
        OpenGoogle();

        await RunAsync(m => m.SignInWithGoogleAsync(new GoogleIdentity("ali.veli@gmail.com", true, "  ", null), Ip, Ct));

        Assert.Equal("ali.veli", await ScalarAsync<string>("SELECT name FROM customers"));
    }

    [Fact]
    public async Task GoogleSignIn_OfAVerifiedAccountsAddress_KeepsOneAccount_WithTwoWaysIn_AndLinksAtOnceWhereGoogleIsTheAuthority_KK3()
    {
        OpenGoogle();
        Guid id = await SeedCustomerAsync("ali@gmail.com", verified: true);

        GoogleSignInResult first = await RunAsync(m => m.SignInWithGoogleAsync(AliAtGmail, Ip, Ct));
        GoogleSignInResult second = await RunAsync(m => m.SignInWithGoogleAsync(AliAtGmail, Ip, Ct));

        Assert.True(first.IsSignedIn);
        Assert.Equal(id, first.CustomerId);
        Assert.True(first.LinkMade);
        Assert.False(first.AccountCreated);
        Assert.True(second.IsSignedIn);
        Assert.False(second.LinkMade);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM customers"));
        Assert.NotNull(await ScalarAsync<string>("SELECT password_hash FROM customers"));
        Assert.NotNull(await ScalarAsync<DateTimeOffset?>("SELECT google_linked_at FROM customers"));
    }

    [Fact]
    public async Task GoogleSignIn_OfAWorkspaceAddress_WithTheHostedDomain_IsAuthoritative_AndWithoutItIsNot_KK4()
    {
        OpenGoogle();
        Guid withDomain = await SeedCustomerAsync("ali@firma.com", verified: true);
        Guid without = await SeedCustomerAsync("veli@firma.com", verified: true);

        GoogleSignInResult linked = await RunAsync(m => m.SignInWithGoogleAsync(new GoogleIdentity("ali@firma.com", true, "Ali", "firma.com"), Ip, Ct));
        GoogleSignInResult needsConfirmation = await RunAsync(m => m.SignInWithGoogleAsync(new GoogleIdentity("veli@firma.com", true, "Veli", null), Ip, Ct));

        Assert.True(linked.IsSignedIn);
        Assert.Equal(withDomain, linked.CustomerId);
        Assert.Equal(GoogleSignInOutcome.ConfirmationRequired, needsConfirmation.Outcome);
        Assert.Equal(without, needsConfirmation.CustomerId);
    }

    [Theory]
    [InlineData("ali@evil-gmail.com")]
    [InlineData("ali@sub.gmail.com")]
    [InlineData("ali@gmail.com.tr")]
    [InlineData("ali@notgmail.com")]
    public async Task GoogleSignIn_WithAnAddressThatOnlyLooksLikeGmail_IsNotTheAuthority_KK4(string email)
    {
        OpenGoogle();
        await SeedCustomerAsync(email, verified: true);

        GoogleSignInResult result = await RunAsync(m => m.SignInWithGoogleAsync(new GoogleIdentity(email, true, "Ali", null), Ip, Ct));

        Assert.Equal(GoogleSignInOutcome.ConfirmationRequired, result.Outcome);
        Assert.False(result.IsSignedIn);
        Assert.Null(await ScalarAsync<DateTimeOffset?>("SELECT google_linked_at FROM customers"));
    }

    [Fact]
    public async Task GoogleSignIn_ReplacesAnUnverifiedPendingRegistration_DeletingItWithTheCount_KK3()
    {
        OpenGoogle();
        Guid pending = await SeedCustomerAsync("ali@gmail.com", verified: false);

        GoogleSignInResult result = await RunAsync(m => m.SignInWithGoogleAsync(AliAtGmail, Ip, Ct));

        Assert.True(result.IsSignedIn);
        Assert.True(result.AccountCreated);
        Assert.True(result.PendingRegistrationReplaced);
        Assert.NotEqual(pending, result.CustomerId);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM customers"));
        await using AppDbContext db = NewContext();
        DisposalRecord record = await db.DisposalRecords.AsNoTracking().SingleAsync(Ct);
        Assert.Equal(DisposalRecordType.PendingRegistration, record.DisposalRecordType);
    }

    [Fact]
    public async Task GoogleSignIn_OfANewAddress_WithTheGateClosed_OpensNothing_AndLeavesThePendingRegistration_KK3()
    {
        OpenGoogle();
        Guid pending = await SeedCustomerAsync("ali@gmail.com", verified: false);
        _gate.Gate = new SalesGate([SalesGateCondition.PrivacyNotice], []);

        GoogleSignInResult closed = await RunAsync(m => m.SignInWithGoogleAsync(AliAtGmail, Ip, Ct));
        GoogleSignInResult fresh = await RunAsync(m => m.SignInWithGoogleAsync(new GoogleIdentity("yeni@gmail.com", true, "Yeni", null), Ip, Ct));

        Assert.Equal(GoogleSignInOutcome.GateClosed, closed.Outcome);
        Assert.Equal(GoogleSignInOutcome.GateClosed, fresh.Outcome);
        Assert.True(closed.IsRejected);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM customers"));
        Assert.Equal(pending, await ScalarAsync<Guid>("SELECT id FROM customers"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM disposal_records"));
    }

    [Fact]
    public async Task GoogleSignIn_OfAnExistingAccount_IsNotBehindTheClosedGate_AndAnUnverifiedGoogleAddressBindsNothing()
    {
        OpenGoogle();
        await SeedCustomerAsync("ali@gmail.com", verified: true);
        _gate.Gate = new SalesGate([SalesGateCondition.PrivacyNotice], []);

        GoogleSignInResult existing = await RunAsync(m => m.SignInWithGoogleAsync(AliAtGmail, Ip, Ct));
        GoogleSignInResult notVerified = await RunAsync(m => m.SignInWithGoogleAsync(new GoogleIdentity("veli@gmail.com", false, "Veli", null), Ip, Ct));

        Assert.True(existing.IsSignedIn);
        Assert.Equal(GoogleSignInOutcome.EmailNotVerified, notVerified.Outcome);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM customers"));
    }

    [Theory]
    [InlineData("", ErrorCode.FieldRequired)]
    [InlineData("not-an-address", ErrorCode.InvalidEmail)]
    public async Task GoogleSignIn_WithAnUnusableAddress_IsRefusedAndNamed(string email, ErrorCode expected)
    {
        OpenGoogle();

        GoogleSignInResult result = await RunAsync(m => m.SignInWithGoogleAsync(new GoogleIdentity(email, true, "Ali", null), Ip, Ct));

        Assert.Equal(GoogleSignInOutcome.Refused, result.Outcome);
        Assert.Equal(expected, result.Code);
    }

    // ---- criterion 4, KK4: no authority, no link, no session; the confirmation link ----

    [Fact]
    public async Task GoogleSignIn_WhereGoogleIsNotTheAuthority_MakesNoLinkAndNoSession_AndSendsOneConfirmationToTheAccountsAddress_KK4()
    {
        OpenGoogle();
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        GoogleIdentity identity = new("ali@example.com", true, "Ali", null);

        GoogleSignInResult first = await RunAsync(m => m.SignInWithGoogleAsync(identity, Ip, Ct));
        GoogleSignInResult second = await RunAsync(m => m.SignInWithGoogleAsync(identity, Ip, Ct));

        Assert.Equal(GoogleSignInOutcome.ConfirmationRequired, first.Outcome);
        Assert.False(first.IsSignedIn);
        Assert.False(first.IsRejected);
        Assert.True(first.ConfirmationQueued);
        Assert.Equal(id, first.CustomerId);
        Assert.False(second.ConfirmationQueued);
        Assert.Null(await ScalarAsync<DateTimeOffset?>("SELECT google_linked_at FROM customers"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM sessions"));
        await using AppDbContext db = NewContext();
        LinkRecord link = await db.LinkRecords.AsNoTracking().SingleAsync(l => l.LinkType == LinkType.GoogleLinkConfirmation, Ct);
        Assert.Equal(id, link.CustomerId);
        Assert.Equal(Start.AddMinutes(ProductConstants.PasswordResetLinkMinutes), link.ExpiresAt);
        DispatchRecord row = await db.DispatchRecords.AsNoTracking().SingleAsync(Ct);
        Assert.Equal(DispatchType.GoogleLinkConfirmationLink, row.DispatchType);
        Assert.Equal("ali@example.com", row.RecipientEmail);
        Assert.Equal(DispatchState.Queued, row.DispatchState);
    }

    [Fact]
    public async Task TheConfirmationButton_LinksTheAccount_OpensNoSession_AndTheLinkIsSingleUse_KK4()
    {
        OpenGoogle();
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        string key = await ConfirmationKeyAsync("ali@example.com");

        EmailLinkReading read = await ReadAsync(m => m.ReadEmailLinkAsync(key, Ct));
        GoogleLinkConfirmationResult confirmed = await RunAsync(m => m.ConfirmGoogleLinkAsync(key, Ct));
        GoogleLinkConfirmationResult again = await RunAsync(m => m.ConfirmGoogleLinkAsync(key, Ct));

        Assert.Equal(EmailLinkKind.GoogleConfirmation, read.Kind);
        Assert.Equal(VerificationLinkStatus.Valid, read.Status);
        Assert.Equal(id, read.CustomerId);
        Assert.True(confirmed.IsConfirmed);
        Assert.Equal(id, confirmed.CustomerId);
        Assert.Equal(Start, await ScalarAsync<DateTimeOffset?>("SELECT google_linked_at FROM customers"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM sessions"));
        Assert.True(again.IsRejected);
        Assert.Equal(VerificationLinkStatus.Used, again.Status);
    }

    [Fact]
    public async Task OpeningTheConfirmationLink_ChangesNothing_AndAnotherTypesKeyIsMissing_KK4()
    {
        OpenGoogle();
        await SeedCustomerAsync("ali@example.com", verified: true);
        string key = await ConfirmationKeyAsync("ali@example.com");
        string before = await StateAsync();

        EmailLinkReading read = await ReadAsync(m => m.ReadEmailLinkAsync(key, Ct));
        EmailLinkReading unknown = await ReadAsync(m => m.ReadEmailLinkAsync("nope", Ct));
        GoogleLinkConfirmationResult wrongType = await RunAsync(m => m.ConfirmGoogleLinkAsync("nope", Ct));

        Assert.Equal(VerificationLinkStatus.Valid, read.Status);
        Assert.Null(unknown.Kind);
        Assert.Equal(VerificationLinkStatus.Missing, unknown.Status);
        Assert.Equal(VerificationLinkStatus.Missing, wrongType.Status);
        Assert.Equal(before, await StateAsync());
    }

    [Fact]
    public async Task TheConfirmationLink_EndsWithItsLife_AndASignInAfterwardsMakesTheNextOne_KK4()
    {
        OpenGoogle();
        await SeedCustomerAsync("ali@example.com", verified: true);
        string key = await ConfirmationKeyAsync("ali@example.com");
        _time.Advance(TimeSpan.FromMinutes(ProductConstants.PasswordResetLinkMinutes));

        GoogleLinkConfirmationResult late = await RunAsync(m => m.ConfirmGoogleLinkAsync(key, Ct));
        GoogleSignInResult next = await RunAsync(m => m.SignInWithGoogleAsync(new GoogleIdentity("ali@example.com", true, "Ali", null), Ip, Ct));

        Assert.Equal(VerificationLinkStatus.Expired, late.Status);
        Assert.True(next.ConfirmationQueued);
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM link_records WHERE link_type = 'GoogleLinkConfirmation'"));
    }

    [Fact]
    public async Task TheConfirmationLink_IsInvalidAfterTheAccountsAddressChanges_KK4()
    {
        OpenGoogle();
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        string key = await ConfirmationKeyAsync("ali@example.com");
        EmailChangeRequestResult requested = await RequestEmailChangeAsync(id, "yeni@example.com");
        Assert.True((await RunAsync(m => m.VerifyNewEmailAsync(_keys.Derive(requested.LinkId!.Value), Ct))).IsVerified);

        GoogleLinkConfirmationResult result = await RunAsync(m => m.ConfirmGoogleLinkAsync(key, Ct));

        Assert.True(result.IsRejected);
        Assert.Equal(VerificationLinkStatus.Superseded, result.Status);
        Assert.Null(await ScalarAsync<DateTimeOffset?>("SELECT google_linked_at FROM customers"));
    }

    [Fact]
    public async Task TwoGoogleSignIns_OfOneAccountAtOnce_MakeOneLinkAndOneEmail_D234()
    {
        OpenGoogle();
        await SeedCustomerAsync("ali@example.com", verified: true);
        GoogleIdentity identity = new("ali@example.com", true, "Ali", null);

        GoogleSignInResult[] results = await Task.WhenAll(
            RunAsync(m => m.SignInWithGoogleAsync(identity, Ip, Ct)),
            RunAsync(m => m.SignInWithGoogleAsync(identity, Ip, Ct)));

        Assert.Equal(1, results.Count(r => r.ConfirmationQueued));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM link_records WHERE link_type = 'GoogleLinkConfirmation'"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records"));
    }

    // ---- criterion 5, KK5: a Google account starts without a password; the reset sets one ----

    [Fact]
    public async Task TheResetFlow_OnAPasswordlessGoogleAccount_SetsThePassword_KK5()
    {
        OpenGoogle();
        await RunAsync(m => m.SignInWithGoogleAsync(AliAtGmail, Ip, Ct));
        Assert.Null(await ScalarAsync<string>("SELECT password_hash FROM customers"));

        ResetRequested requested = await RunAsync(m => m.RequestPasswordResetAsync("ali@gmail.com", Ip, Ct));
        ResetOutcome reset = await RunAsync(m => m.ResetPasswordAsync(_keys.Derive(requested.LinkId!.Value), "a-brand-new-password-1", null, Ct));

        Assert.NotNull(requested.LinkId);
        Assert.True(reset.IsDone);
        Assert.NotNull(await ScalarAsync<string>("SELECT password_hash FROM customers"));
    }

    // ---- criteria 6 to 8, KK6 to KK8: the e-mail change ----

    [Fact]
    public async Task EmailChange_Request_WritesAPendingChange_AndSendsTheLinkToTheNewAddress_AndTheAddressStaysUntilTheButton_KK6()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);

        EmailChangeRequestResult requested = await RequestEmailChangeAsync(id, " Yeni@Example.com ");

        Assert.True(requested.IsRequested);
        Assert.False(requested.Replaced);
        Assert.Equal(Start.AddHours(ProductConstants.EmailVerificationLinkHours), requested.ExpiresAt);
        Assert.Equal("ali@example.com", await ScalarAsync<string>("SELECT email FROM customers"));
        await using AppDbContext db = NewContext();
        DispatchRecord row = await db.DispatchRecords.AsNoTracking().SingleAsync(Ct);
        Assert.Equal(DispatchType.NewEmailVerificationLink, row.DispatchType);
        Assert.Equal("Yeni@Example.com", row.RecipientEmail);
        Assert.Equal(1L, await db.LinkRecords.CountAsync(l => l.LinkType == LinkType.NewEmailVerification, Ct));
        string key = _keys.Derive(requested.LinkId!.Value);
        EmailLinkReading read = await ReadAsync(m => m.ReadEmailLinkAsync(key, Ct));
        Assert.Equal(EmailLinkKind.NewAddress, read.Kind);
        Assert.Equal("ali@example.com", await ScalarAsync<string>("SELECT email FROM customers"));
    }

    [Fact]
    public async Task EmailChange_Verify_ChangesTheAddress_DropsTheGoogleLink_MakesTheRevertLink_AndNoticesTheOldAddress_KK6_KK7()
    {
        OpenGoogle();
        Guid id = await SeedCustomerAsync("ali@gmail.com", verified: true);
        await RunAsync(m => m.SignInWithGoogleAsync(AliAtGmail, Ip, Ct));
        Assert.NotNull(await ScalarAsync<DateTimeOffset?>("SELECT google_linked_at FROM customers"));
        EmailChangeRequestResult requested = await RequestEmailChangeAsync(id, "yeni@example.com");

        NewEmailVerificationResult verified = await RunAsync(m => m.VerifyNewEmailAsync(_keys.Derive(requested.LinkId!.Value), Ct));

        Assert.True(verified.IsVerified);
        Assert.Equal(Start.AddDays(ProductConstants.EmailChangeRevertLinkDays), verified.RevertLinkEndsAt);
        Assert.Equal("yeni@example.com", await ScalarAsync<string>("SELECT email FROM customers"));
        Assert.Equal("yeni@example.com", await ScalarAsync<string>("SELECT normalized_email FROM customers"));
        Assert.Null(await ScalarAsync<DateTimeOffset?>("SELECT google_linked_at FROM customers"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM pending_email_changes"));
        await using AppDbContext db = NewContext();
        LinkRecord revert = await db.LinkRecords.AsNoTracking().SingleAsync(l => l.LinkType == LinkType.EmailChangeRevert, Ct);
        Assert.Equal(verified.RevertLinkId, revert.Id);
        Assert.Equal("ali@gmail.com", revert.PreviousEmail);
        DispatchRecord notice = await db.DispatchRecords.AsNoTracking().SingleAsync(d => d.DispatchType == DispatchType.EmailChangeNotice, Ct);
        Assert.Equal("ali@gmail.com", notice.RecipientEmail);
        Assert.Equal(Start, await ScalarAsync<DateTimeOffset?>("SELECT used_at FROM link_records WHERE id = '" + requested.LinkId + "'"));
    }

    [Fact]
    public async Task EmailChange_Verify_LinkedGuestOrdersOfTheNewAddress_AndKeepsTheOldAddressesOrders_KK7()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        Guid owned = await SeedGuestOrderAsync("ali@example.com", id);
        Guid guestNew = await SeedGuestOrderAsync("yeni@example.com");
        EmailChangeRequestResult requested = await RequestEmailChangeAsync(id, "yeni@example.com");

        NewEmailVerificationResult verified = await RunAsync(m => m.VerifyNewEmailAsync(_keys.Derive(requested.LinkId!.Value), Ct));

        Assert.True(verified.IsVerified);
        Assert.Equal(1, verified.LinkedOrders);
        Assert.Equal(id, await OwnerOfAsync(owned));
        Assert.Equal(id, await OwnerOfAsync(guestNew));
    }

    [Fact]
    public async Task EmailChange_NewAddressLink_IsSingleUse_AndEndsWithThePendingChangesLife_KK6()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        EmailChangeRequestResult used = await RequestEmailChangeAsync(id, "yeni@example.com");
        string usedKey = _keys.Derive(used.LinkId!.Value);
        Assert.True((await RunAsync(m => m.VerifyNewEmailAsync(usedKey, Ct))).IsVerified);

        NewEmailVerificationResult again = await RunAsync(m => m.VerifyNewEmailAsync(usedKey, Ct));

        Assert.Equal(VerificationLinkStatus.Used, again.Status);
        Assert.Equal(VerificationLinkStatus.Missing, (await RunAsync(m => m.VerifyNewEmailAsync("nope", Ct))).Status);
    }

    [Fact]
    public async Task EmailChange_AfterTheLife_LeavesTheAddressAsItWas_KK6()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        EmailChangeRequestResult requested = await RequestEmailChangeAsync(id, "yeni@example.com");
        _time.Advance(TimeSpan.FromHours(ProductConstants.EmailVerificationLinkHours));

        NewEmailVerificationResult late = await RunAsync(m => m.VerifyNewEmailAsync(_keys.Derive(requested.LinkId!.Value), Ct));
        EmailChangeResendResult resend = await RunAsync(m => m.ResendEmailChangeLinkAsync(id, Ip, Ct));

        Assert.True(late.IsRejected);
        Assert.Equal("ali@example.com", await ScalarAsync<string>("SELECT email FROM customers"));
        Assert.True(resend.NothingPending);
    }

    [Fact]
    public async Task EmailChange_DeletesAnUnverifiedPendingRegistrationOfTheNewAddress_WithTheCount_AndRefusesAVerifiedAccountsAddress()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        await SeedCustomerAsync("yeni@example.com", verified: false);
        await SeedCustomerAsync("tutulu@example.com", verified: true);

        EmailChangeRequestResult taken = await RequestEmailChangeAsync(id, "Tutulu@Example.com");
        EmailChangeRequestResult requested = await RequestEmailChangeAsync(id, "yeni@example.com");
        NewEmailVerificationResult verified = await RunAsync(m => m.VerifyNewEmailAsync(_keys.Derive(requested.LinkId!.Value), Ct));

        Assert.Equal(ErrorCode.EmailTakenByOtherAccount, taken.Code);
        Assert.True(requested.IsRequested);
        Assert.True(verified.IsVerified);
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM customers"));
        await using AppDbContext db = NewContext();
        Assert.Equal(DisposalRecordType.PendingRegistration, (await db.DisposalRecords.AsNoTracking().SingleAsync(Ct)).DisposalRecordType);
    }

    [Fact]
    public async Task EmailChange_Verify_IsRefusedAndRolledBack_WhenTheAddressBecameAVerifiedAccountsMeanwhile()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        EmailChangeRequestResult requested = await RequestEmailChangeAsync(id, "yeni@example.com");
        await SeedCustomerAsync("yeni@example.com", verified: true);

        NewEmailVerificationResult result = await RunAsync(m => m.VerifyNewEmailAsync(_keys.Derive(requested.LinkId!.Value), Ct));

        Assert.True(result.AddressTaken);
        Assert.True(result.IsRejected);
        Assert.Equal("ali@example.com", await ScalarAsync<string>("SELECT email FROM customers WHERE id = '" + id + "'"));
        Assert.Null(await ScalarAsync<DateTimeOffset?>("SELECT used_at FROM link_records WHERE id = '" + requested.LinkId + "'"));
    }

    [Theory]
    [InlineData("", ErrorCode.FieldRequired)]
    [InlineData("not-an-address", ErrorCode.InvalidEmail)]
    [InlineData("ali@example.com", ErrorCode.EmailTakenByOtherAccount)]
    [InlineData("  ALI@example.com", ErrorCode.EmailTakenByOtherAccount)]
    public async Task EmailChange_Request_RefusesAnEmptyWrongOrOwnAddress_AndWritesNothing(string entered, ErrorCode expected)
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);

        EmailChangeRequestResult result = await RequestEmailChangeAsync(id, entered);

        Assert.True(result.IsRejected);
        Assert.Equal(expected, result.Code);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM link_records"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records"));
    }

    [Fact]
    public async Task EmailChange_Request_IsClosedWhileTheRevertLinkIsOpen_AndOpensAgainWhenItEnds_Z45()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        EmailChangeRequestResult first = await RequestEmailChangeAsync(id, "yeni@example.com");
        await RunAsync(m => m.VerifyNewEmailAsync(_keys.Derive(first.LinkId!.Value), Ct));

        EmailChangeRequestResult blocked = await RequestEmailChangeAsync(id, "baska@example.com");
        _time.Advance(TimeSpan.FromDays(ProductConstants.EmailChangeRevertLinkDays));
        EmailChangeRequestResult open = await RequestEmailChangeAsync(id, "baska@example.com");

        Assert.Equal(ErrorCode.EmailChangeRevertPending, blocked.Code);
        Assert.Equal(Start.AddDays(ProductConstants.EmailChangeRevertLinkDays), blocked.RevertLinkEndsAt);
        Assert.True(open.IsRequested);
    }

    [Fact]
    public async Task TheRevertLinksOldAddress_IsEmptiedWhenItsLifeEnds_AndTheRowStays_KK8()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        EmailChangeRequestResult requested = await RequestEmailChangeAsync(id, "yeni@example.com");
        NewEmailVerificationResult verified = await RunAsync(m => m.VerifyNewEmailAsync(_keys.Derive(requested.LinkId!.Value), Ct));
        string sql = "SELECT previous_email FROM link_records WHERE id = '" + verified.RevertLinkId + "'";
        Assert.Equal("ali@example.com", await ScalarAsync<string>(sql));

        await RunRevertDisposalAsync();
        Assert.Equal("ali@example.com", await ScalarAsync<string>(sql));
        _time.Advance(TimeSpan.FromDays(ProductConstants.EmailChangeRevertLinkDays) + TimeSpan.FromSeconds(1));
        await RunRevertDisposalAsync();

        Assert.Null(await ScalarAsync<string>(sql));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM link_records WHERE id = '" + verified.RevertLinkId + "'"));
    }

    // ---- criterion 9, KK9: L-9 counts the asking again and the replacing, never the Google sign-in ----

    [Fact]
    public async Task EmailChange_Resend_SupersedesTheEarlierLink_KeepsTheEnd_AndIsCountedToL9_KK9()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        EmailChangeRequestResult requested = await RequestEmailChangeAsync(id, "yeni@example.com");
        _time.Advance(TimeSpan.FromHours(1));

        EmailChangeResendResult resent = await RunAsync(m => m.ResendEmailChangeLinkAsync(id, Ip, Ct));

        Assert.True(resent.IsIssued);
        Assert.Equal(requested.ExpiresAt, resent.ExpiresAt);
        Assert.Equal(VerificationLinkStatus.Superseded, (await RunAsync(m => m.VerifyNewEmailAsync(_keys.Derive(requested.LinkId!.Value), Ct))).Status);
        Assert.True((await RunAsync(m => m.VerifyNewEmailAsync(_keys.Derive(resent.LinkId!.Value), Ct))).IsVerified);
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM attempt_records WHERE attempt_limit = 'L9'"));
    }

    [Fact]
    public async Task EmailChange_Resend_OverTheLimit_IsRefused_AndTheEarlierLinkStaysValid_KK9()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        EmailChangeRequestResult requested = await RequestEmailChangeAsync(id, "yeni@example.com");
        EmailChangeResendResult last = await RunAsync(m => m.ResendEmailChangeLinkAsync(id, Ip, Ct));
        for (int i = 0; i < 20 && last.IsIssued; i++)
        {
            last = await RunAsync(m => m.ResendEmailChangeLinkAsync(id, Ip, Ct));
        }

        Assert.Equal(ErrorCode.VerificationLinkLimitExceeded, last.Code);
        EmailLinkReading latest = await ReadAsync(m => m.ReadEmailLinkAsync(_keys.Derive(requested.LinkId!.Value), Ct));
        Assert.NotEqual(VerificationLinkStatus.Missing, latest.Status);
    }

    [Fact]
    public async Task EmailChange_Request_ThatReplacesAPendingOne_IsCountedToL9_AndTheFirstRequestIsNot_KK9()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);

        EmailChangeRequestResult first = await RequestEmailChangeAsync(id, "yeni@example.com");
        long afterFirst = await ScalarAsync<long>("SELECT count(*) FROM attempt_records WHERE attempt_limit = 'L9'");
        EmailChangeRequestResult replacing = await RequestEmailChangeAsync(id, "baska@example.com");
        long afterSecond = await ScalarAsync<long>("SELECT count(*) FROM attempt_records WHERE attempt_limit = 'L9'");

        Assert.False(first.Replaced);
        Assert.True(replacing.Replaced);
        Assert.Equal(0L, afterFirst);
        Assert.Equal(2L, afterSecond);
        Assert.Equal(VerificationLinkStatus.Superseded, (await RunAsync(m => m.VerifyNewEmailAsync(_keys.Derive(first.LinkId!.Value), Ct))).Status);
        Assert.Equal("baska@example.com", await ScalarAsync<string>("SELECT new_email FROM pending_email_changes"));
    }

    [Fact]
    public async Task GoogleSignIn_IsNeverCountedToAnyLimit_KK9()
    {
        OpenGoogle();
        await SeedCustomerAsync("ali@example.com", verified: true);
        await RunAsync(m => m.SignInWithGoogleAsync(new GoogleIdentity("ali@example.com", true, "Ali", null), Ip, Ct));
        await RunAsync(m => m.SignInWithGoogleAsync(AliAtGmail, Ip, Ct));
        await RunAsync(m => m.SignInWithGoogleAsync(AliAtGmail, Ip, Ct));

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM attempt_records"));
    }

    // ---- helpers of the Google and e-mail change tests ----

    private void OpenGoogle()
    {
        _lines.Clear();
        _lines.Add(new ConditionalLine(ConditionalLine.GoogleSignIn, []));
    }

    private bool ReadOpen()
    {
        using IServiceScope scope = _provider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IMembershipModule>().IsGoogleSignInOpen;
    }

    private async Task<string> ConfirmationKeyAsync(string email)
    {
        GoogleSignInResult result = await RunAsync(m => m.SignInWithGoogleAsync(new GoogleIdentity(email, true, "Ali", null), Ip, Ct));
        Assert.True(result.ConfirmationQueued);
        Guid linkId = await ScalarAsync<Guid>("SELECT id FROM link_records WHERE link_type = 'GoogleLinkConfirmation' ORDER BY created_at DESC LIMIT 1");
        return _keys.Derive(linkId);
    }

    private async Task RunRevertDisposalAsync()
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        await new RevertLinkAddressDisposalPart().RunAsync(new DisposalRun(scope.ServiceProvider, _time.GetUtcNow()), Ct);
    }
}
