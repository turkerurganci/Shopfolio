using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Membership.Contracts.Results;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>
/// T342 on the real module, the real counter, the real session store and the real database: the password policy at the registration (KK7), the session's two lifetimes (KK4), the reset
/// (KK1, KK2, KK3, KK9), the reauthentication and the change (KK2, KK5, KK6), the sign-in limit L-1 (KK9) and the administrator's account on the same code (KK8). Every call runs
/// in the runner's transaction the way a page handler does.
/// </summary>
public sealed partial class MembershipTests
{
    private const string Replacement = "another-long-phrase-42";
    private const string Wrong = "definitely-not-it";
    private const string ShortEntry = "short-one";
    private const string CommonEntry = "1234567890";

    // ---- KK7: length and the common list, no character-type dictation ----

    [Fact]
    public async Task Register_RefusesAShortAndACommonPassword_AndDoesNotDictateCharacterTypes_AndCountsNoAttempt_KK7()
    {
        RegistrationResult shortOne = await RunAsync(m => m.RegisterAsync(new RegisterInput("Ali", "ali@example.com", ShortEntry), Ip, Ct));
        RegistrationResult common = await RunAsync(m => m.RegisterAsync(new RegisterInput("Ali", "ali@example.com", CommonEntry), Ip, Ct));
        RegistrationResult plain = await RunAsync(m => m.RegisterAsync(new RegisterInput("Ali", "ali@example.com", "onlylowercaseletters"), Ip, Ct));

        Assert.Contains(new MembershipFieldError(nameof(RegisterInput.Password), ErrorCode.PasswordTooWeak), shortOne.Fields);
        Assert.Contains(new MembershipFieldError(nameof(RegisterInput.Password), ErrorCode.PasswordTooWeak), common.Fields);
        Assert.True(plain.IsRegistered);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM attempt_records"));
    }

    // ---- KK4: "remember me" is the long session, no choice the short one ----

    [Fact]
    public async Task SignIn_OpensTheLongSessionWithRememberMe_AndTheShortOneWithout_KK4()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);

        SessionOpening longOne = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(id, true, null, Ct));
        SessionOpening shortOne = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(id, false, null, Ct));

        Assert.Equal(TimeSpan.FromDays(ProductConstants.LongSessionDays), longOne.CookieLifetime);
        Assert.Null(shortOne.CookieLifetime);
        Assert.Equal(TimeSpan.FromDays(ProductConstants.RecognizedBrowserMarkDays), longOne.MarkLifetime);
        await using AppDbContext db = NewContext();
        Assert.True(await db.Sessions.AnyAsync(s => s.Id == longOne.SessionId && s.IsLongLived && s.CustomerId == id, Ct));
        Assert.True(await db.Sessions.AnyAsync(s => s.Id == shortOne.SessionId && !s.IsLongLived && s.CustomerId == id, Ct));
        Assert.Equal(SecretKey.Hash(longOne.SessionKey), await db.Sessions.Where(s => s.Id == longOne.SessionId).Select(s => s.TokenHash).SingleAsync(Ct));
    }

    [Fact]
    public async Task SignIn_KeepsTheValidMarkOfTheBrowser_AndGivesANewOneForAnUnknownValue()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        Guid other = await SeedCustomerAsync("veli@example.com", verified: true);
        SessionOpening first = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(id, false, null, Ct));

        _time.Advance(TimeSpan.FromDays(10));
        SessionOpening again = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(id, false, first.MarkValue, Ct));
        SessionOpening foreign = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(other, false, first.MarkValue, Ct));

        Assert.Equal(first.MarkValue, again.MarkValue);
        Assert.NotEqual(first.MarkValue, foreign.MarkValue);
        await using AppDbContext db = NewContext();
        DateTimeOffset renewed = await db.RecognizedBrowserMarks.Where(m => m.MarkHash == SecretKey.Hash(first.MarkValue)).Select(m => m.ExpiresAt).SingleAsync(Ct);
        Assert.Equal(_time.GetUtcNow().AddDays(ProductConstants.RecognizedBrowserMarkDays), renewed);
        Assert.Equal(3, await db.RecognizedBrowserMarks.CountAsync(Ct));
    }

    // ---- KK9 and L-1: the sign-in limit blocks, never locks ----

    [Fact]
    public async Task SignIn_IsBlockedAfterFiveFailures_EvenWithTheRightPassword_AndTheAccountIsNotLocked_KK9()
    {
        await SeedCustomerAsync("ali@example.com", verified: true);
        for (int i = 0; i < ProductConstants.SignInAttemptsPerEmail; i++)
        {
            CredentialCheckResult failed = await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Wrong, Ip, null, Ct));
            Assert.Equal(ErrorCode.InvalidCredentials, failed.Code);
        }

        CredentialCheckResult blocked = await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Entered, Ip, null, Ct));
        CredentialCheckResult stillBlocked = await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Entered, Ip, null, Ct));

        Assert.Equal(ErrorCode.SignInLimitExceeded, blocked.Code);
        Assert.Equal(ErrorCode.SignInLimitExceeded, stillBlocked.Code);
        Assert.Equal(ProductConstants.SignInAttemptsPerEmail, await ScalarAsync<long>("SELECT count(*) FROM attempt_records WHERE attempt_axis = 'Email'"));
        Assert.All(await OutcomesAsync(), o => Assert.Equal(LoginOutcome.Failed, o));

        // The window ends: the same account signs in, nothing was locked, and the success forgets the rows.
        _time.Advance(TimeSpan.FromMinutes(ProductConstants.SignInAttemptWindowMinutes + 1));
        CredentialCheckResult granted = await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Entered, Ip, null, Ct));
        Assert.True(granted.Succeeded);
    }

    [Fact]
    public async Task ARecognizedBrowser_IsCountedOnItsMark_NotOnTheEmail()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        SessionOpening opening = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(id, false, null, Ct));

        for (int i = 0; i < ProductConstants.SignInAttemptsPerEmail; i++)
        {
            await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Wrong, Ip, opening.MarkValue, Ct));
        }

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM attempt_records WHERE attempt_axis = 'Email'"));
        Assert.Equal(ProductConstants.SignInAttemptsPerEmail, await ScalarAsync<long>("SELECT count(*) FROM attempt_records WHERE attempt_axis = 'RecognizedBrowser'"));

        // A browser without the mark has its own count on the e-mail and is not blocked by the marked browser's failures.
        CredentialCheckResult otherBrowser = await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Entered, IPAddress.Parse("198.51.100.9"), null, Ct));
        CredentialCheckResult markedBlocked = await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Entered, Ip, opening.MarkValue, Ct));
        Assert.True(otherBrowser.Succeeded);
        Assert.Equal(ErrorCode.SignInLimitExceeded, markedBlocked.Code);
    }

    [Fact]
    public async Task TheSignInCheck_DoesTheHashingWorkOnceForANobodyAndOnceForAnAccount_D364()
    {
        await SeedCustomerAsync("ali@example.com", verified: true);
        CountingHasher counter = _provider.GetRequiredService<CountingHasher>();
        counter.Reset();

        await RunAsync(m => m.CheckCredentialsAsync("yok@example.com", Entered, Ip, null, Ct));
        Assert.Equal((1, 0), (counter.NobodyCalls, counter.VerifyCalls));

        await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Wrong, Ip, null, Ct));
        Assert.Equal((1, 1), (counter.NobodyCalls, counter.VerifyCalls));
    }

    // ---- KK1, KK2, KK3, KK9: the reset ----

    [Fact]
    public async Task TheResetRequest_IsNeutral_ForAKnownAndAnUnknownAddress_AndTheLinkLivesP13Minutes_KK3()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        Guid pending = await SeedCustomerAsync("bekleyen@example.com", verified: false);

        ResetRequested known = await RunAsync(m => m.RequestPasswordResetAsync("Ali@Example.com", Ip, Ct));
        ResetRequested unknown = await RunAsync(m => m.RequestPasswordResetAsync("yok@example.com", Ip, Ct));
        ResetRequested unverified = await RunAsync(m => m.RequestPasswordResetAsync("bekleyen@example.com", Ip, Ct));

        Assert.False(known.IsRejected);
        Assert.False(unknown.IsRejected);
        Assert.False(unverified.IsRejected);
        Assert.True(known.IsIssued);
        Assert.False(unknown.IsIssued);
        Assert.False(unverified.IsIssued);
        Assert.Null(unknown.Code);
        Assert.Equal(Start.AddMinutes(ProductConstants.PasswordResetLinkMinutes), known.ExpiresAt);
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM link_records WHERE link_type = 'PasswordReset' AND customer_id = '{id}'"));
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM link_records WHERE customer_id = '{pending}'"));
    }

    [Fact]
    public async Task TheResetRequest_OverL2_IsRefusedWithoutLockingTheAccount_AndSaysNoNumber_KK9()
    {
        await SeedCustomerAsync("ali@example.com", verified: true);
        for (int i = 0; i < ProductConstants.PasswordResetAttemptsPerEmail; i++)
        {
            Assert.False((await RunAsync(m => m.RequestPasswordResetAsync("ali@example.com", Ip, Ct))).IsRejected);
        }

        ResetRequested over = await RunAsync(m => m.RequestPasswordResetAsync("ali@example.com", Ip, Ct));
        CredentialCheckResult signIn = await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Entered, Ip, null, Ct));

        Assert.True(over.IsRejected);
        Assert.Equal(ErrorCode.PasswordResetLimitExceeded, over.Code);
        Assert.False(over.IsIssued);
        Assert.True(signIn.Succeeded);
    }

    [Fact]
    public async Task TheReset_TakesTheLinkOnce_EndsEverySession_EndsTheOtherMarks_AndTheNewPasswordWorks_KK1_KK2()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        SessionOpening a = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(id, true, null, Ct));
        SessionOpening b = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(id, false, null, Ct));
        ResetRequested requested = await RunAsync(m => m.RequestPasswordResetAsync("ali@example.com", Ip, Ct));
        string key = _keys.Derive(requested.LinkId!.Value);

        AccountLinkReading reading = await ReadAsync(m => m.ReadPasswordResetLinkAsync(key, Ct));
        ResetOutcome done = await RunAsync(m => m.ResetPasswordAsync(key, Replacement, a.MarkValue, Ct));
        ResetOutcome second = await RunAsync(m => m.ResetPasswordAsync(key, "a-third-long-phrase-77", a.MarkValue, Ct));

        Assert.Equal(AccountLinkStatus.Valid, reading.Status);
        Assert.True(done.IsDone);
        Assert.Equal(a.MarkValue, done.MarkValue);
        Assert.True(second.IsRejected);
        Assert.Equal(AccountLinkStatus.Used, second.LinkStatus);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Sessions.CountAsync(s => s.CustomerId == id, Ct));
        DateTimeOffset now = _time.GetUtcNow();
        Assert.True(await db.RecognizedBrowserMarks.AnyAsync(m => m.MarkHash == SecretKey.Hash(a.MarkValue) && m.ExpiresAt > now, Ct));
        Assert.False(await db.RecognizedBrowserMarks.AnyAsync(m => m.MarkHash == SecretKey.Hash(b.MarkValue) && m.ExpiresAt > now, Ct));
        Assert.Equal(ErrorCode.InvalidCredentials, (await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Entered, Ip, null, Ct))).Code);
        Assert.True((await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Replacement, Ip, null, Ct))).Succeeded);
    }

    [Fact]
    public async Task TheReset_RefusesAWeakNewPasswordWithoutTakingTheLink_AndAnExpiredOrAlienLinkIsInvalid()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        ResetRequested requested = await RunAsync(m => m.RequestPasswordResetAsync("ali@example.com", Ip, Ct));
        string key = _keys.Derive(requested.LinkId!.Value);

        ResetOutcome weak = await RunAsync(m => m.ResetPasswordAsync(key, ShortEntry, null, Ct));
        ResetOutcome common = await RunAsync(m => m.ResetPasswordAsync(key, CommonEntry, null, Ct));
        ResetOutcome unknown = await RunAsync(m => m.ResetPasswordAsync("not-a-key", Replacement, null, Ct));

        Assert.Equal(ErrorCode.PasswordTooWeak, weak.Code);
        Assert.Equal(ErrorCode.PasswordTooWeak, common.Code);
        Assert.Equal(AccountLinkStatus.Missing, unknown.LinkStatus);
        Assert.Equal(AccountLinkStatus.Valid, (await ReadAsync(m => m.ReadPasswordResetLinkAsync(key, Ct))).Status);

        _time.Advance(TimeSpan.FromMinutes(ProductConstants.PasswordResetLinkMinutes));
        ResetOutcome expired = await RunAsync(m => m.ResetPasswordAsync(key, Replacement, null, Ct));
        Assert.Equal(AccountLinkStatus.Expired, expired.LinkStatus);

        // A verification link is not a reset link, and a customer's reset link is no administrator's.
        _time.Advance(TimeSpan.FromMinutes(-ProductConstants.PasswordResetLinkMinutes));
        LinkRecord verification = LinkRecord.ForCustomer(_keys, LinkType.EmailVerification, id, Start.AddHours(24), Start);
        await SaveAsync(c => c.LinkRecords.Add(verification));
        Assert.Equal(AccountLinkStatus.Missing, (await ReadAsync(m => m.ReadPasswordResetLinkAsync(_keys.Derive(verification.Id), Ct))).Status);
        AccountLinkReading adminReading = await InTransactionAsync(s => s.GetRequiredService<IAdministrationModule>().ReadPasswordResetLinkAsync(key, Ct));
        Assert.Equal(AccountLinkStatus.Missing, adminReading.Status);
    }

    [Fact]
    public async Task AskingAgain_MakesTheEarlierResetLinkInvalid()
    {
        await SeedCustomerAsync("ali@example.com", verified: true);
        ResetRequested first = await RunAsync(m => m.RequestPasswordResetAsync("ali@example.com", Ip, Ct));
        ResetRequested second = await RunAsync(m => m.RequestPasswordResetAsync("ali@example.com", Ip, Ct));

        Assert.Equal(AccountLinkStatus.Superseded, (await ReadAsync(m => m.ReadPasswordResetLinkAsync(_keys.Derive(first.LinkId!.Value), Ct))).Status);
        Assert.Equal(AccountLinkStatus.Valid, (await ReadAsync(m => m.ReadPasswordResetLinkAsync(_keys.Derive(second.LinkId!.Value), Ct))).Status);
    }

    // ---- KK2, KK5, KK6: the reauthentication and the change at the account ----

    [Fact]
    public async Task TheChange_AsksForTheReauthenticationValue_TakesItOnce_EndsTheOtherSessionsAndMarks_AndTheChangerStays_KK2_KK5()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        SessionOpening mine = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(id, false, null, Ct));
        SessionOpening other = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(id, true, null, Ct));

        ChangeOutcome without = await RunAsync(m => m.ChangePasswordAsync(id, mine.SessionId, Replacement, null, mine.MarkValue, Ct));
        ReauthenticationResult wrong = await RunAsync(m => m.ReauthenticateAsync(id, mine.SessionId, ReauthenticationOperation.PasswordChange, Wrong, Ip, mine.MarkValue, Ct));
        ReauthenticationResult granted = await RunAsync(m => m.ReauthenticateAsync(id, mine.SessionId, ReauthenticationOperation.PasswordChange, Entered, Ip, mine.MarkValue, Ct));
        ChangeOutcome weak = await RunAsync(m => m.ChangePasswordAsync(id, mine.SessionId, ShortEntry, granted.Value, mine.MarkValue, Ct));
        ChangeOutcome changed = await RunAsync(m => m.ChangePasswordAsync(id, mine.SessionId, Replacement, granted.Value, mine.MarkValue, Ct));
        ChangeOutcome again = await RunAsync(m => m.ChangePasswordAsync(id, mine.SessionId, "a-third-long-phrase-77", granted.Value, mine.MarkValue, Ct));

        Assert.Equal(ErrorCode.ReauthenticationRequired, without.Code);
        Assert.Equal(ErrorCode.ReauthenticationFailed, wrong.Code);
        Assert.True(granted.Succeeded);
        Assert.Equal(ErrorCode.PasswordTooWeak, weak.Code);
        Assert.True(changed.IsDone);
        Assert.Equal(ErrorCode.ReauthenticationRequired, again.Code);
        await using AppDbContext db = NewContext();
        Assert.Equal([mine.SessionId], await db.Sessions.Where(s => s.CustomerId == id).Select(s => s.Id).ToListAsync(Ct));
        DateTimeOffset now = _time.GetUtcNow();
        Assert.True(await db.RecognizedBrowserMarks.AnyAsync(m => m.MarkHash == SecretKey.Hash(mine.MarkValue) && m.ExpiresAt > now, Ct));
        Assert.False(await db.RecognizedBrowserMarks.AnyAsync(m => m.MarkHash == SecretKey.Hash(other.MarkValue) && m.ExpiresAt > now, Ct));
        Assert.True((await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Replacement, Ip, null, Ct))).Succeeded);
    }

    [Fact]
    public async Task TheReauthenticationValue_IsForOneOperationAndOneOwnSession()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        Guid other = await SeedCustomerAsync("veli@example.com", verified: true);
        SessionOpening mine = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(id, false, null, Ct));
        SessionOpening theirs = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(other, false, null, Ct));

        ReauthenticationResult foreign = await RunAsync(m => m.ReauthenticateAsync(id, theirs.SessionId, ReauthenticationOperation.PasswordChange, Entered, Ip, null, Ct));
        ReauthenticationResult forEmail = await RunAsync(m => m.ReauthenticateAsync(id, mine.SessionId, ReauthenticationOperation.EmailChange, Entered, Ip, null, Ct));
        ChangeOutcome wrongOperation = await RunAsync(m => m.ChangePasswordAsync(id, mine.SessionId, Replacement, forEmail.Value, null, Ct));
        ChangeOutcome foreignSession = await RunAsync(m => m.ChangePasswordAsync(id, theirs.SessionId, Replacement, forEmail.Value, null, Ct));

        Assert.Equal(ErrorCode.ReauthenticationFailed, foreign.Code);
        Assert.True(forEmail.Succeeded);
        Assert.Equal(ErrorCode.ReauthenticationRequired, wrongOperation.Code);
        Assert.Equal(ErrorCode.ReauthenticationRequired, foreignSession.Code);
    }

    [Fact]
    public async Task AWrongPasswordAtTheReauthentication_CountsToL1_AndTheSixthIsBlocked_KK6()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        SessionOpening mine = await InTransactionAsync(s => s.GetRequiredService<ISessionService>().SignInAsync(id, false, null, Ct));

        for (int i = 0; i < ProductConstants.SignInAttemptsPerEmail; i++)
        {
            ReauthenticationResult failed = await RunAsync(m => m.ReauthenticateAsync(id, mine.SessionId, ReauthenticationOperation.PasswordChange, Wrong, Ip, null, Ct));
            Assert.Equal(ErrorCode.ReauthenticationFailed, failed.Code);
        }

        ReauthenticationResult blocked = await RunAsync(m => m.ReauthenticateAsync(id, mine.SessionId, ReauthenticationOperation.PasswordChange, Entered, Ip, null, Ct));
        Assert.Equal(ErrorCode.SignInLimitExceeded, blocked.Code);
        Assert.Equal(ProductConstants.SignInAttemptsPerEmail, await ScalarAsync<long>("SELECT count(*) FROM attempt_records WHERE attempt_axis = 'Email'"));
    }

    // ---- KK8: the administrator on the same code ----

    [Fact]
    public async Task TheAdministrator_GetsTheSameSessionLifetimes_PolicyAndLimit_AndNoAccountDeletion_KK8()
    {
        Guid id = await SeedAdminAsync("yonetici@example.com");

        AdminCredentialCheckResult granted = await RunAdminAsync(a => a.CheckCredentialsAsync("Yonetici@Example.com", Entered, Ip, null, Ct));
        SessionOpening longOne = await RunAdminSessionAsync(a => a.SignInAsync(id, true, null, Ct));
        SessionOpening shortOne = await RunAdminSessionAsync(a => a.SignInAsync(id, false, null, Ct));
        ReauthenticationResult deletion = await RunAdminAsync(a => a.ReauthenticateAsync(id, shortOne.SessionId, ReauthenticationOperation.AccountDeletion, Entered, Ip, null, Ct));
        ReauthenticationResult change = await RunAdminAsync(a => a.ReauthenticateAsync(id, shortOne.SessionId, ReauthenticationOperation.PasswordChange, Entered, Ip, shortOne.MarkValue, Ct));
        ChangeOutcome weak = await RunAdminAsync(a => a.ChangePasswordAsync(id, shortOne.SessionId, CommonEntry, change.Value, shortOne.MarkValue, Ct));
        ChangeOutcome changed = await RunAdminAsync(a => a.ChangePasswordAsync(id, shortOne.SessionId, Replacement, change.Value, shortOne.MarkValue, Ct));

        Assert.True(granted.Succeeded);
        Assert.Equal(TimeSpan.FromDays(ProductConstants.LongSessionDays), longOne.CookieLifetime);
        Assert.Null(shortOne.CookieLifetime);
        Assert.Equal(ErrorCode.ReauthenticationOperationInvalid, deletion.Code);
        Assert.Equal(ErrorCode.PasswordTooWeak, weak.Code);
        Assert.True(changed.IsDone);
        await using AppDbContext db = NewContext();
        Assert.Equal([shortOne.SessionId], await db.Sessions.Where(s => s.AdminId == id).Select(s => s.Id).ToListAsync(Ct));
        Assert.True((await RunAdminAsync(a => a.CheckCredentialsAsync("yonetici@example.com", Replacement, Ip, null, Ct))).Succeeded);
        Assert.Equal(2, await db.LoginRecords.CountAsync(l => l.AccountType == AccountType.Admin, Ct));

        for (int i = 0; i < ProductConstants.SignInAttemptsPerEmail; i++)
        {
            await RunAdminAsync(a => a.CheckCredentialsAsync("yonetici@example.com", Wrong, Ip, null, Ct));
        }

        Assert.Equal(ErrorCode.SignInLimitExceeded, (await RunAdminAsync(a => a.CheckCredentialsAsync("yonetici@example.com", Replacement, Ip, null, Ct))).Code);
    }

    [Fact]
    public async Task TheAdministratorsReset_IsTheSameOneUseLink_AndEndsEverySession_KK8()
    {
        Guid id = await SeedAdminAsync("yonetici@example.com");
        SessionOpening session = await RunAdminSessionAsync(a => a.SignInAsync(id, false, null, Ct));
        ResetRequested requested = await RunAdminAsync(a => a.RequestPasswordResetAsync("yonetici@example.com", Ip, Ct));
        ResetRequested unknown = await RunAdminAsync(a => a.RequestPasswordResetAsync("yok@example.com", Ip, Ct));
        string key = _keys.Derive(requested.LinkId!.Value);

        ResetOutcome done = await RunAdminAsync(a => a.ResetPasswordAsync(key, Replacement, session.MarkValue, Ct));
        ResetOutcome second = await RunAdminAsync(a => a.ResetPasswordAsync(key, Replacement, session.MarkValue, Ct));

        Assert.False(unknown.IsIssued);
        Assert.True(done.IsDone);
        Assert.Equal(AccountLinkStatus.Used, second.LinkStatus);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Sessions.CountAsync(s => s.AdminId == id, Ct));
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM link_records WHERE link_type = 'PasswordReset' AND admin_id = '{id}'"));
    }

    // ---- helpers of T342 ----

    private async Task<Guid> SeedAdminAsync(string email)
    {
        Admin admin = Admin.Create("Yönetici", email, SeedDigest.Value, Start);
        await SaveAsync(c => c.Admins.Add(admin));
        return admin.Id;
    }

    private Task<T> RunAdminAsync<T>(Func<IAdministrationModule, Task<T>> call)
        where T : IOperationResult
        => RunScopeAsync(scope => call(scope.GetRequiredService<IAdministrationModule>()));

    private async Task<T> RunAdminSessionAsync<T>(Func<IAdministrationModule, Task<T>> call)
        => (await InTransactionAsync(scope => call(scope.GetRequiredService<IAdministrationModule>())));

    private async Task<T> RunScopeAsync<T>(Func<IServiceProvider, Task<T>> call)
        where T : IOperationResult
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        ITransactionRunner runner = scope.ServiceProvider.GetRequiredService<ITransactionRunner>();
        return await runner.RunAsync(_ => call(scope.ServiceProvider), Ct);
    }

    /// <summary>Runs a call that returns a value that is not an operation result inside the runner's transaction, so what it adds is saved.</summary>
    private async Task<T> InTransactionAsync<T>(Func<IServiceProvider, Task<T>> call)
    {
        Held<T> held = await RunScopeAsync(async scope => new Held<T>(await call(scope)));
        return held.Value;
    }

    private sealed record Held<T>(T Value) : IOperationResult
    {
        public bool IsRejected => false;
    }

    /// <summary>The password service that counts its two checks: the sign-in must do the same work whether or not the address has an account (D-364).</summary>
    private sealed class CountingHasher : PasswordService
    {
        public int VerifyCalls { get; private set; }

        public int NobodyCalls { get; private set; }

        public void Reset()
        {
            VerifyCalls = 0;
            NobodyCalls = 0;
        }

        public override HashCheck Verify(string digest, string entered)
        {
            VerifyCalls++;
            return base.Verify(digest, entered);
        }

        public override HashCheck VerifyNobody(string entered)
        {
            NobodyCalls++;
            return base.VerifyNobody(entered);
        }
    }
}
