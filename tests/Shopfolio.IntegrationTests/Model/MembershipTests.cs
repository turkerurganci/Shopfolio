using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Ordering;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Membership.Contracts.Results;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the membership tests (T341): one database copied from the template.</summary>
[CollectionDefinition(MembershipDefinition.Name)]
public sealed class MembershipDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "Membership";
}

/// <summary>
/// The registration, the e-mail verification gate, the guest order's account, the account name and the sign-in record through the real Membership module on a real PostgreSQL
/// and a fake clock (T341 criteria 1-9; KK1-KK9). The gate of the data-collecting entries is a stub the test opens and closes; the Ordering module, the attempt counter, the link
/// layer, the sign-in recorder and the transaction runner are the real ones, and every call runs in the runner's transaction the way a page handler does, so a refusal rolls back.
/// </summary>
[Collection(MembershipDefinition.Name)]
public sealed partial class MembershipTests : IAsyncLifetime
{
    private const string Entered = "a-long-enough-password";

    private static readonly DateTimeOffset Start = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    private static readonly IPAddress Ip = IPAddress.Parse("203.0.113.7");

    // One digest for every seeded account: hashing is slow on purpose.
    private static readonly Lazy<string> SeedDigest = new(() => new PasswordService().Hash(Entered));

    private readonly CollectionDatabaseFixture _collection;
    private readonly ManualTimeProvider _time = new(Start);
    private readonly GateSwitch _gate = new();
    private readonly List<ConditionalLine> _lines = [];
    private readonly LinkKeys _keys = new(Options.Create(new LinkKeyOptions { DerivationSecret = Convert.ToBase64String(new byte[LinkKeyOptions.DerivationSecretMinBytes]) }));
    private NpgsqlDataSource _dataSource = null!;
    private ServiceProvider _provider = null!;
    private Guid _provinceId;
    private LegalTextVersion _form = null!;
    private LegalTextVersion _contract = null!;
    private LegalTextVersion _notice = null!;
    private int _orderCounter;

    public MembershipTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE customers, admins, audit_logs, carts, orders, legal_text_versions, provinces, link_records, attempt_records, login_records, disposal_records, dispatch_records CASCADE");
        Province province = Province.Create("06", "Ankara", Start);
        _form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, 1, Start, "metin", Start);
        _contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, 1, Start, "metin", Start);
        _notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 1, Start, "metin", Start).WithHosting();
        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.LegalTextVersions.AddRange(_form, _contract, _notice);
        });
        _provinceId = province.Id;
        _gate.Provinces = [new ProvinceInfo(province.Id, province.Code, province.Name)];

        _dataSource = NpgsqlDataSource.Create(_collection.Dsn);
        ServiceCollection services = new();
        services.AddSingleton<TimeProvider>(_time);
        services.AddSingleton(_dataSource);
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.None));
        services.AddScoped<AttemptCounter>();
        services.AddScoped<LinkRecords>();
        services.AddSingleton(_keys);
        services.AddScoped<LoginRecorder>();
        services.AddSingleton<CountingHasher>();
        services.AddSingleton<PasswordService>(p => p.GetRequiredService<CountingHasher>());
        services.AddScoped<ReauthenticationValues>();
        services.AddScoped<SessionStore>();
        services.AddScoped<PasswordRegime>();
        services.AddSingleton(_gate);
        services.AddScoped<ISettingsModule, GateOnlySettings>();
        services.AddScoped<OrderLocks>();
        services.AddScoped<DownloadCounters>();
        services.AddScoped<OrderPaymentArms>();
        services.AddScoped<OrderAccess>();
        services.AddScoped<AdvisoryLocks>();
        services.AddCartServices();
        services.AddScoped<IOrderingModule, OrderingModule>();
        services.AddScoped<OrderExport>();
        services.AddPaymentModule();
        services.AddScoped<AccountEmailQueue>();
        services.AddScoped<INotificationsModule, AccountEmailOnlyNotifications>();
        services.AddSingleton<IReadOnlyList<ConditionalLine>>(_lines);
        services.AddScoped<IAuditTrail, AuditTrail>();
        services.AddMembershipModule();
        services.AddScoped<AdminPasswords>();
        services.AddScoped<IAdministrationModule, AdministrationModule>();
        services.AddScoped<AdminAccounts>();
        services.AddScoped<AdminEmailChanges>();
        services.AddScoped<AuditTrail>();
        services.AddSingleton<SwitchableAuditActor>();
        services.AddSingleton<IAuditActorSource>(p => p.GetRequiredService<SwitchableAuditActor>());
        _provider = services.BuildServiceProvider();
    }

    public async ValueTask DisposeAsync()
    {
        await _provider.DisposeAsync();
        await _dataSource.DisposeAsync();
    }

    // ---- KK7, KK8: the form asks the name, the e-mail and the password; no marketing field anywhere ----

    [Fact]
    public async Task Register_MakesAnUnverifiedAccount_WithTheDigestAndAFirstLinkOfP12Hours()
    {
        RegistrationResult result = await RegisterAsync("  Ali Veli ", "Ali@Example.com");

        Assert.True(result.IsRegistered);
        await using AppDbContext db = NewContext();
        Customer customer = await db.Customers.AsNoTracking().SingleAsync(Ct);
        Assert.Equal(result.CustomerId, customer.Id);
        Assert.Equal("Ali Veli", customer.Name);
        Assert.Equal("ali@example.com", customer.NormalizedEmail);
        Assert.Null(customer.EmailVerifiedAt);
        Assert.Null(customer.GoogleLinkedAt);
        Assert.NotNull(customer.PasswordHash);
        Assert.DoesNotContain(Entered, customer.PasswordHash, StringComparison.Ordinal);
        LinkRecord link = await db.LinkRecords.AsNoTracking().SingleAsync(Ct);
        Assert.Equal(result.LinkId, link.Id);
        Assert.Equal(LinkType.EmailVerification, link.LinkType);
        Assert.Equal(customer.Id, link.CustomerId);
        Assert.Equal(Start.AddHours(ProductConstants.EmailVerificationLinkHours), link.ExpiresAt);
        Assert.Equal(link.ExpiresAt, result.LinkExpiresAt);
        Assert.Equal(_keys.DigestFor(link.Id), link.TokenHash);
    }

    [Fact]
    public async Task Register_WithoutAName_IsRefused_AndNothingIsWritten()
    {
        RegistrationResult empty = await RegisterAsync("", "ali@example.com");
        RegistrationResult blank = await RegisterAsync("   ", "ali@example.com");

        foreach (RegistrationResult result in new[] { empty, blank })
        {
            Assert.True(result.IsRejected);
            Assert.Equal(ErrorCode.FieldRequired, result.Code);
            Assert.Contains(new MembershipFieldError(nameof(RegisterInput.Name), ErrorCode.FieldRequired), result.Fields);
        }

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM customers"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM link_records"));
    }

    [Theory]
    [InlineData("", ErrorCode.FieldRequired)]
    [InlineData("not-an-address", ErrorCode.InvalidEmail)]
    [InlineData("Ali <ali@example.com>", ErrorCode.InvalidEmail)]
    [InlineData("ali@localhost", ErrorCode.InvalidEmail)]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa@example.com", ErrorCode.InvalidEmail)]
    [InlineData("ali@exa_mple.com", ErrorCode.InvalidEmail)]
    [InlineData("ali@-example.com", ErrorCode.InvalidEmail)]
    [InlineData("\"a b\"@example.com", ErrorCode.InvalidEmail)]
    [InlineData("a..b@example.com", ErrorCode.InvalidEmail)]
    [InlineData(".ali@example.com", ErrorCode.InvalidEmail)]
    [InlineData("ali.@example.com", ErrorCode.InvalidEmail)]
    public async Task Register_WithAWrongAddress_IsRefusedAndNamed(string email, ErrorCode expected)
    {
        RegistrationResult result = await RegisterAsync("Ali", email);

        Assert.Equal(expected, result.Code);
        Assert.Contains(new MembershipFieldError(nameof(RegisterInput.Email), expected), result.Fields);
    }

    [Fact]
    public async Task Register_TakesTheAddressTheProjectsOneEmailRuleAccepts()
    {
        // 04 7.3.2.1 accepts this address (the guest order form does too); a second, stricter rule here would keep its guest orders from ever binding.
        // The local part's dots are the one rule's own (D-379); the refusals are in Register_WithAWrongAddress_IsRefusedAndNamed.
        const string address = "a.b+etiket@example.com";
        Assert.Null(new InvalidEmailAttribute().GetValidationResult(address, new ValidationContext(address)));

        RegistrationResult result = await RegisterAsync("Ali", address);

        Assert.True(result.IsRegistered);
    }

    [Fact]
    public async Task Register_WithoutAPassword_IsRefused()
    {
        RegistrationResult result = await RunAsync(m => m.RegisterAsync(new RegisterInput("Ali", "ali@example.com", ""), Ip, Ct));

        Assert.Contains(new MembershipFieldError(nameof(RegisterInput.Password), ErrorCode.FieldRequired), result.Fields);
    }

    [Fact]
    public void TheFormAndTheAccount_HaveNoMarketingConsentField()
    {
        string[] banned = ["marketing", "consent", "newsletter", "promotion", "campaign", "optin", "subscri"];
        IEnumerable<string> names = typeof(RegisterInput).GetProperties().Select(p => p.Name)
            .Concat(typeof(Customer).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name));

        Assert.Equal(["Name", "Email", "Password"], typeof(RegisterInput).GetProperties().Select(p => p.Name));
        Assert.DoesNotContain(names, n => banned.Any(b => n.Contains(b, StringComparison.OrdinalIgnoreCase)));
    }

    // ---- KK9: the closed gate refuses the registration; the sign-in is not behind it ----

    [Fact]
    public async Task Register_WithTheGateClosed_IsRefused_NamingTheWaitingConditions_AndCountsNoAttempt()
    {
        _gate.Gate = new SalesGate([SalesGateCondition.PrivacyNotice, SalesGateCondition.CookiePolicy], []);

        RegistrationResult result = await RegisterAsync("Ali", "ali@example.com");

        Assert.Equal(ErrorCode.DataCollectionClosed, result.Code);
        Assert.Equal([SalesGateCondition.PrivacyNotice, SalesGateCondition.CookiePolicy], result.Gate!.UnmetConditions);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM customers"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM attempt_records"));
    }

    [Fact]
    public async Task SignIn_OfAnExistingAccount_IsNotBehindTheClosedGate()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        _gate.Gate = new SalesGate([SalesGateCondition.PrivacyNotice], []);

        CredentialCheckResult result = await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Entered, Ip, null, Ct));

        Assert.True(result.Succeeded);
        Assert.Equal(id, result.CustomerId);
    }

    // ---- the address: taken, free again after Z-1, limited by L-3 ----

    [Fact]
    public async Task Register_WithAKnownAddress_IsRefused_ForAVerifiedAccountAndForALivePendingOne()
    {
        await SeedCustomerAsync("ali@example.com", verified: true);
        await RegisterAsync("Veli", "veli@example.com");

        RegistrationResult verified = await RegisterAsync("Başka", "ALI@example.com");
        RegistrationResult pending = await RegisterAsync("Başka", "Veli@Example.com");

        Assert.Equal(ErrorCode.EmailAlreadyRegistered, verified.Code);
        Assert.Equal(ErrorCode.EmailAlreadyRegistered, pending.Code);
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM customers"));
    }

    [Fact]
    public async Task Register_AfterThePendingRegistrationEnded_FreesTheAddress_DeletesTheOldRowAndCountsItInTheDisposalRecord()
    {
        RegistrationResult first = await RegisterAsync("Eski", "ali@example.com");
        _time.Advance(TimeSpan.FromHours(ProductConstants.EmailVerificationLinkHours));
        _time.Advance(TimeSpan.FromHours(2));

        RegistrationResult again = await RegisterAsync("Yeni", "ali@example.com", ip: IPAddress.Parse("203.0.113.99"));

        Assert.True(again.IsRegistered);
        await using AppDbContext db = NewContext();
        Customer only = await db.Customers.AsNoTracking().SingleAsync(Ct);
        Assert.Equal(again.CustomerId, only.Id);
        Assert.NotEqual(first.CustomerId, only.Id);
        Assert.Equal("Yeni", only.Name);
        Assert.Equal(1, await db.LinkRecords.AsNoTracking().CountAsync(Ct));
        DisposalRecord record = await db.DisposalRecords.AsNoTracking().SingleAsync(Ct);
        Assert.Equal(DisposalRecordType.PendingRegistration, record.DisposalRecordType);
        Assert.Equal(1, record.Count);
    }

    [Fact]
    public async Task Register_PastTheLimitL3_IsRefused_OnTheEmailAndOnTheIp()
    {
        for (int i = 0; i < ProductConstants.RegistrationAttemptsPerEmail; i++)
        {
            RegistrationResult counted = await RegisterAsync("Ali", "ali@example.com", ip: IPAddress.Parse($"198.51.100.{i + 1}"));
            Assert.NotEqual(ErrorCode.RegistrationLimitExceeded, counted.Code);
        }

        RegistrationResult byEmail = await RegisterAsync("Ali", "ALI@example.com", ip: IPAddress.Parse("198.51.100.50"));
        Assert.Equal(ErrorCode.RegistrationLimitExceeded, byEmail.Code);

        IPAddress sameIp = IPAddress.Parse("192.0.2.1");
        for (int i = 0; i < ProductConstants.RegistrationAttemptsPerIp; i++)
        {
            RegistrationResult counted = await RegisterAsync("Ali", $"kisi{i}@example.com", ip: sameIp);
            Assert.True(counted.IsRegistered);
        }

        RegistrationResult byIp = await RegisterAsync("Ali", "son@example.com", ip: sameIp);
        Assert.Equal(ErrorCode.RegistrationLimitExceeded, byIp.Code);
    }

    // ---- KK6: the link opens the screen, the button verifies ----

    [Fact]
    public async Task OpeningTheLink_ChangesNothing_AndTheButtonVerifies()
    {
        RegistrationResult registered = await RegisterAsync("Ali", "ali@example.com");
        string key = _keys.Derive(registered.LinkId!.Value);
        string before = await StateAsync();

        VerificationLinkReading reading = await ReadAsync(m => m.ReadVerificationLinkAsync(key, Ct));

        Assert.Equal(VerificationLinkStatus.Valid, reading.Status);
        Assert.Equal(registered.CustomerId, reading.CustomerId);
        Assert.Equal(before, await StateAsync());
        Assert.Null(await ScalarAsync<DateTimeOffset?>("SELECT email_verified_at FROM customers"));

        EmailVerificationResult verified = await RunAsync(m => m.VerifyEmailAsync(key, Ct));

        Assert.True(verified.IsVerified);
        Assert.Equal(registered.CustomerId, verified.CustomerId);
        Assert.Equal(Start, await ScalarAsync<DateTimeOffset?>("SELECT email_verified_at FROM customers"));
        Assert.NotNull(await ScalarAsync<DateTimeOffset?>("SELECT used_at FROM link_records"));
    }

    [Fact]
    public async Task AnUnknownKey_AnEmptyKey_AndAKeyOfAnotherLinkType_AreMissing_AndVerifyNothing()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: false);
        LinkRecord reset = LinkRecord.ForCustomer(_keys, LinkType.PasswordReset, id, Start.AddHours(1), Start);
        await SaveAsync(c => c.LinkRecords.Add(reset));

        foreach (string key in new[] { "nope", "", _keys.Derive(reset.Id) })
        {
            Assert.Equal(VerificationLinkStatus.Missing, (await ReadAsync(m => m.ReadVerificationLinkAsync(key, Ct))).Status);
            EmailVerificationResult result = await RunAsync(m => m.VerifyEmailAsync(key, Ct));
            Assert.True(result.IsRejected);
            Assert.Equal(VerificationLinkStatus.Missing, result.Status);
        }

        Assert.Null(await ScalarAsync<DateTimeOffset?>("SELECT email_verified_at FROM customers"));
    }

    [Fact]
    public async Task AnExpiredLink_IsInvalid_AndVerifiesNothing_AndASecondUseIsRefused()
    {
        RegistrationResult registered = await RegisterAsync("Ali", "ali@example.com");
        string key = _keys.Derive(registered.LinkId!.Value);
        _time.Advance(TimeSpan.FromHours(ProductConstants.EmailVerificationLinkHours));

        EmailVerificationResult expired = await RunAsync(m => m.VerifyEmailAsync(key, Ct));

        Assert.Equal(VerificationLinkStatus.Expired, expired.Status);
        Assert.Equal(VerificationLinkStatus.Expired, (await ReadAsync(m => m.ReadVerificationLinkAsync(key, Ct))).Status);
        Assert.Null(await ScalarAsync<DateTimeOffset?>("SELECT email_verified_at FROM customers"));

        _time.SetUtcNow(Start.AddHours(1));
        Assert.True((await RunAsync(m => m.VerifyEmailAsync(key, Ct))).IsVerified);
        EmailVerificationResult second = await RunAsync(m => m.VerifyEmailAsync(key, Ct));
        Assert.Equal(VerificationLinkStatus.Used, second.Status);
    }

    // ---- KK2, KK1: the verified address takes the past guest orders, and only those that may be taken ----

    [Fact]
    public async Task Verifying_BringsTheGuestOrdersOfTheAddressToTheAccount_AndOnlyThose()
    {
        Guid other = await SeedCustomerAsync("baska@example.com", verified: true);
        Guid mine = await SeedGuestOrderAsync("Ali@Example.com");
        Guid mineAgain = await SeedGuestOrderAsync("ali@example.com ");
        Guid someoneElses = await SeedGuestOrderAsync("veli@example.com");
        Guid alreadyOwned = await SeedGuestOrderAsync("ali@example.com", customerId: other);
        Guid ofADeletedAccount = await SeedGuestOrderAsync("ali@example.com", accountDeleted: true);
        RegistrationResult registered = await RegisterAsync("Ali", "ali@example.com");

        EmailVerificationResult result = await RunAsync(m => m.VerifyEmailAsync(_keys.Derive(registered.LinkId!.Value), Ct));

        Assert.Equal(2, result.LinkedOrders);
        Assert.Equal(registered.CustomerId, await OwnerOfAsync(mine));
        Assert.Equal(registered.CustomerId, await OwnerOfAsync(mineAgain));
        Assert.Null(await OwnerOfAsync(someoneElses));
        Assert.Equal(other, await OwnerOfAsync(alreadyOwned));
        Assert.Null(await OwnerOfAsync(ofADeletedAccount));
        Assert.Equal(1, await ScalarAsync<int>($"SELECT version FROM orders WHERE id = '{mine}'"));
    }

    [Fact]
    public async Task APendingRegistration_TakesNoOrder_UntilItIsVerified()
    {
        Guid guestOrder = await SeedGuestOrderAsync("ali@example.com");

        await RegisterAsync("Ali", "ali@example.com");

        Assert.Null(await OwnerOfAsync(guestOrder));
    }

    // ---- KK1, KK3, KK4: the guest, the verified account's guest order, the unverified account ----

    [Fact]
    public async Task AGuestOrder_NeedsNoVerification_AndAnAddressWithNoAccountStaysAGuest()
    {
        OrderAccountResolution guest = await ResolveAsync(null, "yeni@example.com");

        Assert.Equal(OrderAccountKind.Guest, guest.Kind);
        Assert.True(guest.IsAccepted);
        Assert.True(guest.IsGuestOrder);
        Assert.Null(guest.CustomerId);
    }

    [Fact]
    public async Task AGuestOrderWithAVerifiedAccountsAddress_IsBoundToTheAccountAtOnce_AndAPendingRegistrationIsNotBoundTo()
    {
        Guid verified = await SeedCustomerAsync("ali@example.com", verified: true);
        await SeedCustomerAsync("veli@example.com", verified: false);

        OrderAccountResolution bound = await ResolveAsync(null, " ALI@example.com");
        OrderAccountResolution pending = await ResolveAsync(null, "veli@example.com");

        Assert.Equal(OrderAccountKind.GuestBoundToAccount, bound.Kind);
        Assert.Equal(verified, bound.CustomerId);
        Assert.True(bound.IsGuestOrder);
        Assert.Equal(OrderAccountKind.Guest, pending.Kind);
        Assert.Null(pending.CustomerId);
    }

    [Fact]
    public async Task ASignedInOrder_IsTheAccountsOwn_AndAnUnverifiedAccountCannotOrder()
    {
        Guid verified = await SeedCustomerAsync("ali@example.com", verified: true);
        Guid pending = await SeedCustomerAsync("veli@example.com", verified: false);

        OrderAccountResolution signedIn = await ResolveAsync(verified, "baska@example.com");
        OrderAccountResolution refused = await ResolveAsync(pending, "veli@example.com");

        Assert.Equal(new OrderAccountResolution(OrderAccountKind.SignedIn, verified), signedIn);
        Assert.False(signedIn.IsGuestOrder);
        Assert.Equal(OrderAccountKind.RefusedUnverified, refused.Kind);
        Assert.False(refused.IsAccepted);
    }

    [Fact]
    public async Task AnUnverifiedAccount_IsRefusedAtTheSignIn_EvenWithTheRightPassword_AndTheAttemptIsAFailedOne()
    {
        await SeedCustomerAsync("ali@example.com", verified: false);

        CredentialCheckResult result = await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Entered, Ip, null, Ct));

        Assert.Equal(ErrorCode.EmailNotVerified, result.Code);
        Assert.Equal([LoginOutcome.Failed], await OutcomesAsync());
    }

    // ---- KK9: every attempt is a row with four values ----

    [Fact]
    public async Task EveryAttempt_IsOneRow_WithTheMomentTheIpTheNormalizedEmailAndTheOutcome_AndARefusalDoesNotTakeItBack()
    {
        await SeedCustomerAsync("ali@example.com", verified: true);
        IPAddress ip = IPAddress.Parse("203.0.113.55");

        CredentialCheckResult good = await RunAsync(m => m.CheckCredentialsAsync("Ali@Example.com", Entered, ip, null, Ct));
        _time.Advance(TimeSpan.FromMinutes(1));
        CredentialCheckResult wrong = await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", "yanlis", ip, null, Ct));
        _time.Advance(TimeSpan.FromMinutes(1));
        CredentialCheckResult nobody = await RunAsync(m => m.CheckCredentialsAsync(" Yok@Example.com", Entered, null, null, Ct));

        Assert.True(good.Succeeded);
        Assert.Equal(ErrorCode.InvalidCredentials, wrong.Code);
        Assert.Equal(ErrorCode.InvalidCredentials, nobody.Code);
        await using AppDbContext db = NewContext();
        List<LoginRecord> rows = await db.LoginRecords.AsNoTracking().OrderBy(l => l.CreatedAt).ToListAsync(Ct);
        Assert.Equal(3, rows.Count);
        Assert.Equal([Start, Start.AddMinutes(1), Start.AddMinutes(2)], rows.Select(r => r.CreatedAt));
        Assert.Equal([ip, ip, IPAddress.None], rows.Select(r => r.IpAddress));
        Assert.Equal(["ali@example.com", "ali@example.com", "yok@example.com"], rows.Select(r => r.NormalizedEmail));
        Assert.Equal([LoginOutcome.Succeeded, LoginOutcome.Failed, LoginOutcome.Failed], rows.Select(r => r.Outcome));
        Assert.All(rows, r => Assert.Equal(AccountType.Customer, r.AccountType));
    }

    [Fact]
    public void TheSignInRecord_NormalizesTheEnteredAddressItself()
    {
        LoginRecord row = LoginRecord.Create(Ip, AccountType.Customer, "  ALİ@Example.COM ", LoginOutcome.Failed, Start);

        Assert.Equal("ali@example.com", row.NormalizedEmail);
    }

    [Fact]
    public async Task ASuccessfulSignIn_WithADigestOfOlderSettings_RenewsTheDigest()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        string old = new Microsoft.AspNetCore.Identity.PasswordHasher<object>(Options.Create(new Microsoft.AspNetCore.Identity.PasswordHasherOptions { IterationCount = 10_000 })).HashPassword(new object(), Entered);
        await ExecuteAsync($"UPDATE customers SET password_hash = '{old}'");

        CredentialCheckResult result = await RunAsync(m => m.CheckCredentialsAsync("ali@example.com", Entered, Ip, null, Ct));

        Assert.True(result.Succeeded);
        string renewed = await ScalarAsync<string>($"SELECT password_hash FROM customers WHERE id = '{id}'");
        Assert.NotEqual(old, renewed);
        Assert.False(_provider.GetRequiredService<PasswordService>().Verify(renewed, Entered).NeedsRehash);
        Assert.True(_provider.GetRequiredService<PasswordService>().Verify(renewed, Entered).Matched);
    }

    // ---- the verification link asked for again ----

    [Fact]
    public async Task AskingForANewLink_SupersedesTheEarlierOnes_AndEndsWhereTheFirstEnds()
    {
        RegistrationResult registered = await RegisterAsync("Ali", "ali@example.com");
        string firstKey = _keys.Derive(registered.LinkId!.Value);
        _time.Advance(TimeSpan.FromHours(3));

        VerificationResendResult resent = await RunAsync(m => m.ResendVerificationLinkAsync(registered.CustomerId!.Value, Ip, Ct));

        Assert.True(resent.IsIssued);
        Assert.Equal(registered.LinkExpiresAt, resent.ExpiresAt);
        Assert.NotEqual(registered.LinkId, resent.LinkId);
        Assert.Equal(VerificationLinkStatus.Superseded, (await ReadAsync(m => m.ReadVerificationLinkAsync(firstKey, Ct))).Status);
        Assert.Equal(VerificationLinkStatus.Valid, (await ReadAsync(m => m.ReadVerificationLinkAsync(_keys.Derive(resent.LinkId!.Value), Ct))).Status);
        Assert.True((await RunAsync(m => m.VerifyEmailAsync(_keys.Derive(resent.LinkId!.Value), Ct))).IsVerified);
    }

    [Fact]
    public async Task AskingForANewLink_IsLimitedByL9_AndRefusedForAVerifiedAccountAndAnEndedRegistration()
    {
        RegistrationResult registered = await RegisterAsync("Ali", "ali@example.com");
        Guid id = registered.CustomerId!.Value;

        for (int i = 0; i < ProductConstants.VerificationResendAttemptsPerEmail; i++)
        {
            Assert.True((await RunAsync(m => m.ResendVerificationLinkAsync(id, IPAddress.Parse($"198.51.100.{i + 1}"), Ct))).IsIssued);
        }

        VerificationResendResult limited = await RunAsync(m => m.ResendVerificationLinkAsync(id, IPAddress.Parse("198.51.100.77"), Ct));
        Assert.Equal(ErrorCode.VerificationLinkLimitExceeded, limited.Code);

        _time.Advance(TimeSpan.FromHours(ProductConstants.EmailVerificationLinkHours + 2));
        VerificationResendResult ended = await RunAsync(m => m.ResendVerificationLinkAsync(id, IPAddress.Parse("198.51.100.78"), Ct));
        Assert.Equal(ErrorCode.PendingRegistrationUnresolved, ended.Code);

        Guid verified = await SeedCustomerAsync("veli@example.com", verified: true);
        Assert.Equal(ErrorCode.PendingRegistrationUnresolved, (await RunAsync(m => m.ResendVerificationLinkAsync(verified, Ip, Ct))).Code);
        Assert.Equal(ErrorCode.PendingRegistrationUnresolved, (await RunAsync(m => m.ResendVerificationLinkAsync(EntityIds.New(), Ip, Ct))).Code);
    }

    [Fact]
    public async Task SupersedingWithNoOwner_IsRefused()
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        LinkRecords links = scope.ServiceProvider.GetRequiredService<LinkRecords>();

        await Assert.ThrowsAsync<ArgumentException>(() => links.SupersedeEarlierAsync(LinkType.EmailVerification, null, null, EntityIds.New(), Ct));
    }

    // ---- KK7: the name changes without a new verification and without touching an order ----

    [Fact]
    public async Task TheName_ChangesWithoutANewVerification_AndLeavesTheOrdersRecipientNameAlone()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", verified: true);
        Guid order = await SeedGuestOrderAsync("ali@example.com", customerId: id);

        NameChangeResult changed = await RunAsync(m => m.ChangeNameAsync(id, "  Yeni Ad ", Ct));
        NameChangeResult empty = await RunAsync(m => m.ChangeNameAsync(id, "  ", Ct));
        NameChangeResult missing = await RunAsync(m => m.ChangeNameAsync(EntityIds.New(), "Ad", Ct));

        Assert.True(changed.IsChanged);
        Assert.Equal("Yeni Ad", await ScalarAsync<string>($"SELECT name FROM customers WHERE id = '{id}'"));
        Assert.NotNull(await ScalarAsync<DateTimeOffset?>($"SELECT email_verified_at FROM customers WHERE id = '{id}'"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM link_records"));
        Assert.Equal("Ali Veli", await ScalarAsync<string>($"SELECT billing_recipient_name FROM orders WHERE id = '{order}'"));
        Assert.Contains(new MembershipFieldError(nameof(RegisterInput.Name), ErrorCode.FieldRequired), empty.Fields);
        Assert.Equal("Yeni Ad", await ScalarAsync<string>($"SELECT name FROM customers WHERE id = '{id}'"));
        Assert.True(missing.IsAccountMissing);
    }

    // ---- helpers ----

    private Task<RegistrationResult> RegisterAsync(string name, string email, IPAddress? ip = null) =>
        RunAsync(m => m.RegisterAsync(new RegisterInput(name, email, Entered), ip ?? Ip, Ct));

    /// <summary>Calls the module the way a page handler does: in the transaction runner of one scope, so a refused result rolls everything back.</summary>
    private async Task<T> RunAsync<T>(Func<IMembershipModule, Task<T>> call)
        where T : IOperationResult
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        ITransactionRunner runner = scope.ServiceProvider.GetRequiredService<ITransactionRunner>();
        IMembershipModule module = scope.ServiceProvider.GetRequiredService<IMembershipModule>();
        return await runner.RunAsync(_ => call(module), Ct);
    }

    /// <summary>A read the module answers without a transaction of its own.</summary>
    private async Task<T> ReadAsync<T>(Func<IMembershipModule, Task<T>> call)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<IMembershipModule>());
    }

    private Task<OrderAccountResolution> ResolveAsync(Guid? signedIn, string email) => ReadAsync(m => m.ResolveOrderAccountAsync(signedIn, email, Ct));

    private async Task<Guid> SeedCustomerAsync(string email, bool verified)
    {
        string digest = SeedDigest.Value;
        Customer customer = Customer.Create("Ali", email, digest, verified ? Start : null, null, Start);
        await SaveAsync(c => c.Customers.Add(customer));
        return customer.Id;
    }

    private async Task<Guid> SeedGuestOrderAsync(string email, Guid? customerId = null, bool accountDeleted = false)
    {
        (Order order, OrderItem item) = BuildOrder(email, customerId, accountDeleted);
        await SaveAsync(c =>
        {
            c.Orders.Add(order);
            c.OrderItems.Add(item);
        });
        return order.Id;
    }

    private (Order Order, OrderItem Item) BuildOrder(string email, Guid? customerId, bool accountDeleted)
    {
        int n = Interlocked.Increment(ref _orderCounter);
        Order order = Order.Create($"2026-{n:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", PaymentMethod.Card, true, email, 120m, 0m, Start.AddHours(1), Start);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.CustomerId = customerId;
        order.IsAccountDeleted = accountDeleted;
        order.BillingRecipientName = "Ali Veli";
        order.BillingProvinceId = _provinceId;
        order.BillingDistrict = "Çankaya";
        order.BillingAddressLine = "Atatürk Bulvarı 1";
        order.CompanyType = CompanyType.Tradesman;
        order.CompanyLegalName = "Örnek Ticaret";
        order.CompanyTaxIdNumber = "1234567890";
        order.CompanyProvinceId = _provinceId;
        order.CompanyDistrict = "Çankaya";
        order.CompanyAddressLine = "Kızılay 2";
        order.CompanyPhone = "3121234567";
        order.CompanyEmail = "firma@example.com";
        order.CompanyKepAddress = "firma@hs01.kep.tr";
        order.PreInformationFormVersionId = _form.Id;
        order.DistanceSalesContractVersionId = _contract.Id;
        order.PrivacyNoticeVersionId = _notice.Id;
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        OrderItem item = OrderItem.Create(order.Id, null, null, ProductType.Physical, "Kapı", 3, 100m, 20m, 50m, Start);
        return (order, item);
    }

    private async Task<Guid?> OwnerOfAsync(Guid orderId)
    {
        await using AppDbContext db = NewContext();
        return await db.Orders.AsNoTracking().Where(o => o.Id == orderId).Select(o => o.CustomerId).SingleAsync(Ct);
    }

    private async Task<List<LoginOutcome>> OutcomesAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.LoginRecords.AsNoTracking().OrderBy(l => l.CreatedAt).Select(l => l.Outcome).ToListAsync(Ct);
    }

    private Task<string> StateAsync() =>
        ScalarAsync<string>("SELECT coalesce((SELECT string_agg(to_jsonb(c)::text, '|') FROM customers c), '') || '#' || coalesce((SELECT string_agg(to_jsonb(l)::text, '|') FROM link_records l), '')");

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task SaveAsync(Action<AppDbContext> change)
    {
        await using AppDbContext context = NewContext();
        change(context);
        await context.SaveChangesAsync(Ct);
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<T> ScalarAsync<T>(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        object? value = await command.ExecuteScalarAsync(Ct);
        if (value is DateTime moment && (typeof(T) == typeof(DateTimeOffset?) || typeof(T) == typeof(DateTimeOffset)))
        {
            return (T)(object)new DateTimeOffset(DateTime.SpecifyKind(moment, DateTimeKind.Utc));
        }

        return value is null or DBNull ? default! : (T)value;
    }

    /// <summary>The gate the test opens or closes.</summary>
    private sealed class GateSwitch
    {
        public SalesGate Gate { get; set; } = new([], []);

        public IReadOnlyList<ProvinceInfo> Provinces { get; set; } = [];
    }

    private sealed class GateOnlySettings(GateSwitch gate) : ISettingsModule
    {
        public Task<SalesGate> GetSalesGateAsync(CancellationToken ct) => Task.FromResult(gate.Gate);

        public Task<DateTimeOffset?> FindSalesFirstOpenedAtAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<string?> GetWhatsAppNumberAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<DeliveryProvincesSection> GetDeliveryProvincesAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<SettingsSaveResult> SaveDeliveryProvincesAsync(IReadOnlyCollection<Guid> provinceIds, int formVersion, CancellationToken ct) => throw new NotSupportedException();

        public Task<DeliveryArea> GetDeliveryAreaAsync(CancellationToken ct) => Task.FromResult(new DeliveryArea(gate.Provinces, []));

        public Task<CompanyIdentity> GetCompanyIdentityAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<CompanyIdentityCompleteness> GetCompanyIdentityCompletenessAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<LegalIdentity> GetLegalIdentityAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<SettingsSaveResult> SaveCompanyIdentityAsync(CompanyIdentityInput input, int formVersion, CancellationToken ct) => throw new NotSupportedException();

        public Task<ReturnAddressSection> GetReturnAddressAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<SettingsSaveResult> SaveReturnAddressAsync(ReturnAddressInput input, int formVersion, CancellationToken ct) => throw new NotSupportedException();

        public Task<ShippingFeeSection> GetShippingFeeAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<SettingsSaveResult> SaveShippingFeeAsync(ShippingFeeInput input, int formVersion, CancellationToken ct) => throw new NotSupportedException();

        public Task<PeriodsSection> GetPeriodsAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<string?> GetBankTransferIbanAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<SettingsSaveResult> SavePeriodsAsync(PeriodsInput input, int formVersion, CancellationToken ct) => throw new NotSupportedException();

        public Task<ProductDefaultsSection> GetProductDefaultsAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<SettingsSaveResult> SaveProductDefaultsAsync(ProductDefaultsInput input, int formVersion, CancellationToken ct) => throw new NotSupportedException();

        public Task<SalesSwitchResult> CloseSalesTemporarilyAsync(bool shownIsClosed, CancellationToken ct) => throw new NotSupportedException();

        public Task<SalesSwitchResult> ReopenSalesAsync(bool shownIsClosed, CancellationToken ct) => throw new NotSupportedException();

        public Task<IReadOnlyList<LegalTextSummary>> GetLegalTextsAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<LegalTextDraft> GetLegalTextDraftAsync(LegalTextType type, CancellationToken ct) => throw new NotSupportedException();
        public Task<LegalTextDraftPreview> GetLegalTextDraftPreviewAsync(LegalTextType type, CancellationToken ct) => throw new NotSupportedException();

        public Task<SettingsSaveResult> SaveLegalTextDraftAsync(LegalTextType type, string text, string? hostingLocation, int formVersion, CancellationToken ct) => throw new NotSupportedException();

        public Task<LegalTextPublishResult> PublishLegalTextAsync(LegalTextType type, int formVersion, CancellationToken ct) => throw new NotSupportedException();

        public Task<LegalTextVersionIds> GetCurrentLegalTextVersionsAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<bool> AreShownLegalTextVersionsCurrentAsync(Guid shownPreInformationFormId, Guid shownDistanceSalesContractId, CancellationToken ct) => throw new NotSupportedException();

        public Task<LegalTextVersionInfo?> GetLegalTextVersionAsync(Guid versionId, CancellationToken ct) => throw new NotSupportedException();

        public Task<LegalTextVersionInfo?> GetLatestLegalTextVersionAsync(LegalTextType type, CancellationToken ct) => throw new NotSupportedException();

        public Task SynchronizeGeneratedLegalTextsAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<IReadOnlyList<ProvinceInfo>> GetProvincesAsync(CancellationToken ct) => Task.FromResult(gate.Provinces);

        public Task<HomepageSection> GetHomepageAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<SettingsSaveResult> SaveHomepageAsync(HomepageInput input, int formVersion, CancellationToken ct) => throw new NotSupportedException();
    }
}
