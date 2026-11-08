using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Membership.Contracts;
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

/// <summary>The collection of the address tests (T344): one database copied from the template, which carries the 81 provinces.</summary>
[CollectionDefinition(AddressDefinition.Name)]
public sealed class AddressDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "Address";
}

/// <summary>
/// The address book and the address part of an order through the real Membership module on a real PostgreSQL (T344 criteria 1-9; KK1-KK9). The province list is the real one the
/// template carries; every call runs in the transaction runner's transaction the way a page handler does, so a refusal rolls back.
/// </summary>
[Collection(AddressDefinition.Name)]
public sealed class AddressTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Start = new(2026, 11, 8, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;
    private readonly ManualTimeProvider _time = new(Start);
    private readonly List<ConditionalLine> _lines = [];
    private NpgsqlDataSource _dataSource = null!;
    private ServiceProvider _provider = null!;
    private List<Guid> _provinceIds = [];
    private Dictionary<string, Guid> _byCode = [];
    private LegalTextVersion _form = null!;
    private LegalTextVersion _contract = null!;
    private LegalTextVersion _notice = null!;
    private int _orderCounter;

    public AddressTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private Guid Ankara => _byCode["06"];

    private Guid Izmir => _byCode["35"];

    public async ValueTask InitializeAsync()
    {
        // The provinces are start data of the template and are never truncated.
        await ExecuteAsync("TRUNCATE customers, orders, legal_text_versions, address_books CASCADE");
        _form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, 1, Start, "metin", Start);
        _contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, 1, Start, "metin", Start);
        _notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 1, Start, "metin", Start).WithHosting();
        await SaveAsync(c => c.LegalTextVersions.AddRange(_form, _contract, _notice));
        await using (AppDbContext db = NewContext())
        {
            List<(string Code, Guid Id)> rows = (await db.Provinces.AsNoTracking().OrderBy(p => p.Code).Select(p => new { p.Code, p.Id }).ToListAsync(Ct))
                .Select(p => (p.Code, p.Id)).ToList();
            _provinceIds = rows.Select(r => r.Id).ToList();
            _byCode = rows.ToDictionary(r => r.Code, r => r.Id);
        }

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
        services.AddSingleton(new LinkKeys(Options.Create(new LinkKeyOptions { DerivationSecret = Convert.ToBase64String(new byte[LinkKeyOptions.DerivationSecretMinBytes]) })));
        services.AddScoped<LoginRecorder>();
        services.AddSingleton<PasswordService>();
        services.AddScoped<ReauthenticationValues>();
        services.AddScoped<SessionStore>();
        services.AddScoped<PasswordRegime>();
        services.AddScoped<ProvinceList>();
        services.AddScoped<ISettingsModule, ProvinceSettings>();
        services.AddScoped<OrderLocks>();
        services.AddScoped<DownloadCounters>();
        services.AddScoped<OrderPaymentArms>();
        services.AddScoped<AdvisoryLocks>();
        services.AddScoped<IOrderingModule, OrderingModule>();
        services.AddPaymentModule();
        services.AddScoped<AccountEmailQueue>();
        services.AddScoped<INotificationsModule, AccountEmailOnlyNotifications>();
        services.AddSingleton<IReadOnlyList<ConditionalLine>>(_lines);
        services.AddScoped<Shopfolio.Modules.Administration.IAuditActorSource, Shopfolio.IntegrationTests.Ordering.NoAuditActor>();
        services.AddScoped<Shopfolio.Modules.Administration.Contracts.IAuditTrail, Shopfolio.Modules.Administration.AuditTrail>();
        services.AddMembershipModule();
        _provider = services.BuildServiceProvider();
    }

    public async ValueTask DisposeAsync()
    {
        await _provider.DisposeAsync();
        await _dataSource.DisposeAsync();
    }

    // ---- KK1: a member names and keeps more than one address; a guest has no book ----

    [Fact]
    public async Task AMember_KeepsSeveralNamedAddresses_OldestFirst_AndAGuestHasNoBook_KK1()
    {
        Guid member = await SeedCustomerAsync("ali@example.com");
        Guid other = await SeedCustomerAsync("veli@example.com");

        AddressBookSaveResult home = await RunAsync(m => m.CreateAddressBookEntryAsync(member, Book("  Ev ", Ankara), Ct));
        _time.Advance(TimeSpan.FromMinutes(1));
        AddressBookSaveResult work = await RunAsync(m => m.CreateAddressBookEntryAsync(member, Book("İş", Izmir), Ct));
        await RunAsync(m => m.CreateAddressBookEntryAsync(other, Book("Başkası", Ankara), Ct));

        Assert.True(home.IsSaved);
        Assert.True(work.IsSaved);
        IReadOnlyList<AddressBookEntry> book = await ReadAsync(m => m.GetAddressBookAsync(member, Ct));
        Assert.Equal(["Ev", "İş"], book.Select(e => e.Name));
        Assert.Equal([home.EntryId, work.EntryId], book.Select(e => (Guid?)e.Id));
        Assert.Equal("Ankara", book[0].ProvinceName);
        Assert.Equal("İzmir", book[1].ProvinceName);
        Assert.Single(await ReadAsync(m => m.GetAddressBookAsync(other, Ct)));

        // A guest: no customer id to ask with, and a made-up one has no book and no way in.
        Guid nobody = EntityIds.New();
        Assert.Empty(await ReadAsync(m => m.GetAddressBookAsync(nobody, Ct)));
        Assert.True((await RunAsync(m => m.CreateAddressBookEntryAsync(nobody, Book("Ev", Ankara), Ct))).IsNotFound);
        OrderAddressResolution guestPicks = await RunAsync(m => m.ResolveOrderAddressesAsync(Request(null, [ProductType.Physical]) with { AddressBookId = home.EntryId }, Ct));
        Assert.True(guestPicks.IsBookEntryNotFound);
        Assert.Equal(ErrorCode.NotFound, guestPicks.Code);
        Assert.Equal(2, await CountAsync("address_books WHERE customer_id = '" + member + "'"));
    }

    [Fact]
    public async Task AnEntryThatIsNotTheMembers_IsNotFound_ForReadEditDeleteAndPick()
    {
        Guid member = await SeedCustomerAsync("ali@example.com");
        Guid other = await SeedCustomerAsync("veli@example.com");
        Guid theirs = (await RunAsync(m => m.CreateAddressBookEntryAsync(other, Book("Başkası", Ankara), Ct))).EntryId!.Value;

        Assert.Null(await ReadAsync(m => m.GetAddressBookEntryAsync(member, theirs, Ct)));
        Assert.True((await RunAsync(m => m.UpdateAddressBookEntryAsync(member, theirs, Book("Benim", Ankara), Ct))).IsNotFound);
        Assert.True((await RunAsync(m => m.UpdateAddressBookEntryAsync(member, theirs, Book("", Ankara) with { RecipientName = "" }, Ct))).IsNotFound);
        Assert.True((await RunAsync(m => m.DeleteAddressBookEntryAsync(member, theirs, Ct))).IsNotFound);
        OrderAddressResolution picked = await RunAsync(m => m.ResolveOrderAddressesAsync(Request(member, [ProductType.Physical]) with { AddressBookId = theirs }, Ct));

        Assert.True(picked.IsBookEntryNotFound);
        AddressBookEntry? untouched = await ReadAsync(m => m.GetAddressBookEntryAsync(other, theirs, Ct));
        Assert.Equal("Başkası", untouched!.Name);
        Assert.True((await RunAsync(m => m.DeleteAddressBookEntryAsync(other, theirs, Ct))).IsDeleted);
        Assert.True((await RunAsync(m => m.DeleteAddressBookEntryAsync(other, theirs, Ct))).IsNotFound);
    }

    // ---- KK2: a typed address is kept in the book only on the member's choice ----

    [Fact]
    public async Task ATypedAddress_GoesToTheBookOnlyWhenTheMemberChoseIt_UnderTheGivenNameOrTheRecipients_KK2()
    {
        Guid member = await SeedCustomerAsync("ali@example.com");

        OrderAddressResolution notKept = await RunAsync(m => m.ResolveOrderAddressesAsync(Request(member, [ProductType.Physical]), Ct));
        Assert.True(notKept.IsResolved);
        Assert.Null(notKept.NewBookEntryId);
        Assert.Equal(0, await CountAsync("address_books"));

        OrderAddressResolution namedKept = await RunAsync(m => m.ResolveOrderAddressesAsync(Request(member, [ProductType.Physical]) with { SaveToAddressBook = true, SavedAddressName = " Yazlık " }, Ct));
        OrderAddressResolution unnamedKept = await RunAsync(m => m.ResolveOrderAddressesAsync(Request(member, [ProductType.Physical]) with { SaveToAddressBook = true }, Ct));

        Assert.NotNull(namedKept.NewBookEntryId);
        Assert.NotNull(unnamedKept.NewBookEntryId);
        IReadOnlyList<AddressBookEntry> book = await ReadAsync(m => m.GetAddressBookAsync(member, Ct));
        Assert.Equal(["Yazlık", "Ali Veli"], book.OrderBy(e => e.Id == unnamedKept.NewBookEntryId).Select(e => e.Name));
        Assert.All(book, e => Assert.Equal("5321234567", e.Phone));

        // A guest cannot keep anything: the choice is ignored and nothing is written.
        OrderAddressResolution guest = await RunAsync(m => m.ResolveOrderAddressesAsync(Request(null, [ProductType.Physical]) with { SaveToAddressBook = true }, Ct));
        Assert.True(guest.IsResolved);
        Assert.Null(guest.NewBookEntryId);
        Assert.Equal(2, await CountAsync("address_books"));

        // A refused address keeps nothing either, even when the member chose to keep it.
        OrderAddressResolution refused = await RunAsync(m => m.ResolveOrderAddressesAsync(Request(member, [ProductType.Physical]) with { SaveToAddressBook = true, ShippingDistrict = "" }, Ct));
        Assert.True(refused.IsRejected);
        Assert.Equal(2, await CountAsync("address_books"));
    }

    [Fact]
    public async Task APickedBookEntry_IsUsedAsIs_AndNothingIsAddedToTheBook_KK2()
    {
        Guid member = await SeedCustomerAsync("ali@example.com");
        Guid entry = (await RunAsync(m => m.CreateAddressBookEntryAsync(member, Book("Ev", Izmir), Ct))).EntryId!.Value;

        OrderAddressResolution picked = await RunAsync(m => m.ResolveOrderAddressesAsync(
            Request(member, [ProductType.Physical]) with { AddressBookId = entry, SaveToAddressBook = true, ShippingRecipientName = "Yok Sayılır", ShippingProvinceId = Ankara }, Ct));

        Assert.True(picked.IsResolved);
        Assert.Equal(new ShippingAddressSnapshot("Ali Veli", "5321234567", Izmir, "Konak", "Atatürk Cad. 1"), picked.Shipping);
        Assert.Null(picked.NewBookEntryId);
        Assert.Equal(1, await CountAsync("address_books"));
    }

    // ---- KK3: the billing address is the shipping one unless another is chosen ----

    [Fact]
    public async Task TheBillingAddress_IsTheShippingOne_UnlessAnotherIsGiven_AndHasNoPhone_KK3()
    {
        OrderAddressResolution same = await RunAsync(m => m.ResolveOrderAddressesAsync(Request(null, [ProductType.Physical]), Ct));
        OrderAddressResolution own = await RunAsync(m => m.ResolveOrderAddressesAsync(
            Request(null, [ProductType.Physical]) with
            {
                IsBillingSameAsShipping = false,
                BillingRecipientName = "Firma Adı",
                BillingProvinceId = Izmir,
                BillingDistrict = "Bornova",
                BillingAddressLine = "Fatura Sok. 2",
            },
            Ct));

        Assert.Equal(new BillingAddressSnapshot("Ali Veli", Ankara, "Çankaya", "Atatürk Bulvarı 1"), same.Billing);
        Assert.Equal(new BillingAddressSnapshot("Firma Adı", Izmir, "Bornova", "Fatura Sok. 2"), own.Billing);
        Assert.Equal(Ankara, own.Shipping!.ProvinceId);
        Assert.DoesNotContain(typeof(BillingAddressSnapshot).GetProperties(), p => p.Name.Contains("Phone", StringComparison.Ordinal));

        // The box says "different" but nothing is typed: all four billing fields are asked.
        OrderAddressResolution empty = await RunAsync(m => m.ResolveOrderAddressesAsync(Request(null, [ProductType.Physical]) with { IsBillingSameAsShipping = false }, Ct));
        Assert.Equal(
            ["BillingRecipientName", "BillingProvinceId", "BillingDistrict", "BillingAddressLine"],
            empty.Fields.Select(f => f.Field));
    }

    // ---- KK4: the province is one of the 81 ----

    [Fact]
    public async Task TheProvince_IsOneOfTheClosedListOf81_AndAnotherIsRefused_KK4()
    {
        Assert.Equal(81, _provinceIds.Count);
        Assert.Equal(81, await CountAsync("provinces"));
        Guid member = await SeedCustomerAsync("ali@example.com");

        AddressBookSaveResult unknown = await RunAsync(m => m.CreateAddressBookEntryAsync(member, Book("Ev", EntityIds.New()), Ct));
        AddressBookSaveResult none = await RunAsync(m => m.CreateAddressBookEntryAsync(member, Book("Ev", Ankara) with { ProvinceId = null }, Ct));
        OrderAddressResolution order = await RunAsync(m => m.ResolveOrderAddressesAsync(Request(null, [ProductType.Physical]) with { ShippingProvinceId = EntityIds.New() }, Ct));

        Assert.Equal([new MembershipFieldError("ProvinceId", ErrorCode.InvalidProvince)], unknown.Fields);
        Assert.Equal([new MembershipFieldError("ProvinceId", ErrorCode.FieldRequired)], none.Fields);
        Assert.Equal([new MembershipFieldError("ShippingProvinceId", ErrorCode.InvalidProvince)], order.Fields);
        Assert.Equal(0, await CountAsync("address_books"));
        foreach (Guid id in _provinceIds)
        {
            Assert.True((await RunAsync(m => m.CreateAddressBookEntryAsync(member, Book("Ev", id), Ct))).IsSaved);
        }

        Assert.Equal(81, await CountAsync("address_books"));
    }

    // ---- KK5, KK6: what the basket asks by its item types ----

    [Fact]
    public async Task AnOrderWithAPhysicalItem_AsksTwoAddresses_AndAllRefusedFieldsComeTogether_KK5()
    {
        foreach (ProductType[] types in (ProductType[][])[[ProductType.Physical], [ProductType.Physical, ProductType.Digital], [ProductType.Service, ProductType.Physical, ProductType.Digital]])
        {
            AddressRequirement need = await ReadAsync(m => Task.FromResult(m.GetAddressRequirement(types)));
            Assert.Equal(new AddressRequirement(true, true, true), need);
        }

        OrderAddressResolution blank = await RunAsync(m => m.ResolveOrderAddressesAsync(
            new OrderAddressRequest(null, [ProductType.Physical, ProductType.Digital], null, null, null, null, null, null, false, null, false, null, null, null, null), Ct));

        Assert.True(blank.IsRejected);
        Assert.Equal(
            [
                "ShippingRecipientName", "ShippingProvinceId", "ShippingDistrict", "ShippingAddressLine", "ShippingPhone",
                "BillingRecipientName", "BillingProvinceId", "BillingDistrict", "BillingAddressLine",
            ],
            blank.Fields.Select(f => f.Field));
        Assert.All(blank.Fields, f => Assert.Equal(ErrorCode.FieldRequired, f.Code));
        Assert.Null(blank.Shipping);
        Assert.Null(blank.Billing);
    }

    [Fact]
    public async Task ADigitalOrDeliveredByServiceOrder_HasNoShippingAddress_AndAsksTheBillingAddressOnly_KK6()
    {
        foreach (ProductType[] types in (ProductType[][])[[ProductType.Digital], [ProductType.Service], [ProductType.Digital, ProductType.Service]])
        {
            AddressRequirement need = await ReadAsync(m => Task.FromResult(m.GetAddressRequirement(types)));
            Assert.Equal(new AddressRequirement(false, true, false), need);
        }

        // Shipping values that are sent anyway are not read: a digital basket shows no shipping field and the order freezes none.
        OrderAddressResolution ok = await RunAsync(m => m.ResolveOrderAddressesAsync(
            Request(null, [ProductType.Digital]) with { IsBillingSameAsShipping = false, BillingRecipientName = "Ali Veli", BillingProvinceId = Ankara, BillingDistrict = "Çankaya", BillingAddressLine = "Adres 1", ShippingPhone = "kötü" },
            Ct));
        Assert.True(ok.IsResolved);
        Assert.Null(ok.Shipping);
        Assert.Equal(new BillingAddressSnapshot("Ali Veli", Ankara, "Çankaya", "Adres 1"), ok.Billing);

        // "Same as shipping" has nothing to be the same as: the billing address is typed.
        OrderAddressResolution sameBox = await RunAsync(m => m.ResolveOrderAddressesAsync(Request(null, [ProductType.Digital]) with { IsBillingSameAsShipping = true }, Ct));
        Assert.True(sameBox.IsRejected);
        Assert.All(sameBox.Fields, f => Assert.StartsWith("Billing", f.Field, StringComparison.Ordinal));

        Assert.Throws<ArgumentException>(() => ReadAsync(m => Task.FromResult(m.GetAddressRequirement([]))).GetAwaiter().GetResult());
    }

    // ---- KK7: the order keeps its own copy ----

    [Fact]
    public async Task AnEditOrADeletionOfTheBookEntry_DoesNotChangeAnOrderThatFrozeIt_AndTheOrderHasNoLinkToTheBook_KK7()
    {
        Guid member = await SeedCustomerAsync("ali@example.com");
        Guid entry = (await RunAsync(m => m.CreateAddressBookEntryAsync(member, Book("Ev", Ankara), Ct))).EntryId!.Value;
        OrderAddressResolution resolved = await RunAsync(m => m.ResolveOrderAddressesAsync(Request(member, [ProductType.Physical]) with { AddressBookId = entry }, Ct));
        Guid order = await SeedOrderAsync(resolved);
        string before = await OrderAddressesAsync(order);

        AddressBookInput edited = new("Ev (yeni)", "Veli Ali", Izmir, "Karşıyaka", "Yeni Sok. 9", "5559876543");
        Assert.True((await RunAsync(m => m.UpdateAddressBookEntryAsync(member, entry, edited, Ct))).IsSaved);
        Assert.Equal(before, await OrderAddressesAsync(order));
        Assert.True((await RunAsync(m => m.DeleteAddressBookEntryAsync(member, entry, Ct))).IsDeleted);
        Assert.Equal(before, await OrderAddressesAsync(order));

        Assert.Contains("Ali Veli", before, StringComparison.Ordinal);
        Assert.Contains("5321234567", before, StringComparison.Ordinal);
        Assert.Equal(0, await CountAsync("information_schema.columns WHERE table_name = 'orders' AND column_name LIKE '%address_book%'"));
        Assert.Equal(0, await CountAsync("information_schema.table_constraints c JOIN information_schema.constraint_column_usage u ON u.constraint_name = c.constraint_name WHERE c.table_name = 'orders' AND u.table_name = 'address_books'"));
    }

    [Fact]
    public async Task TheAddressMethods_DoNotSaveOnTheirOwn_TheTransactionOwnerDoes_KK7()
    {
        Guid member = await SeedCustomerAsync("ali@example.com");
        Guid entry = (await RunAsync(m => m.CreateAddressBookEntryAsync(member, Book("Ev", Ankara), Ct))).EntryId!.Value;
        await CommitWithoutSavingAsync(async m =>
        {
            await m.CreateAddressBookEntryAsync(member, Book("Yeni", Izmir), Ct);
            await m.UpdateAddressBookEntryAsync(member, entry, Book("Değişti", Izmir), Ct);
            await m.ResolveOrderAddressesAsync(Request(member, [ProductType.Physical]) with { SaveToAddressBook = true }, Ct);
        });

        IReadOnlyList<AddressBookEntry> after = await ReadAsync(m => m.GetAddressBookAsync(member, Ct));
        Assert.Equal(["Ev"], after.Select(e => e.Name));

        await CommitWithoutSavingAsync(async m => await m.DeleteAddressBookEntryAsync(member, entry, Ct));
        Assert.Single(await ReadAsync(m => m.GetAddressBookAsync(member, Ct)));

        // Outside a transaction the writes are refused loudly: the entry would otherwise be lost silently.
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        IMembershipModule module = scope.ServiceProvider.GetRequiredService<IMembershipModule>();
        await Assert.ThrowsAsync<InvalidOperationException>(() => module.CreateAddressBookEntryAsync(member, Book("Ev", Ankara), Ct));
        await Assert.ThrowsAsync<InvalidOperationException>(() => module.UpdateAddressBookEntryAsync(member, entry, Book("Ev", Ankara), Ct));
        await Assert.ThrowsAsync<InvalidOperationException>(() => module.DeleteAddressBookEntryAsync(member, entry, Ct));
        await Assert.ThrowsAsync<InvalidOperationException>(() => module.ResolveOrderAddressesAsync(Request(member, [ProductType.Physical]), Ct));
    }

    // ---- KK8: the four fields are all required ----

    [Fact]
    public async Task WhenOneOfTheFourFieldsIsEmpty_TheEntryIsRefused_AndNoRowIsWritten_KK8()
    {
        Guid member = await SeedCustomerAsync("ali@example.com");
        AddressBookInput full = Book("Ev", Ankara);
        (string Field, AddressBookInput Input)[] cases =
        [
            ("RecipientName", full with { RecipientName = "   " }),
            ("ProvinceId", full with { ProvinceId = null }),
            ("District", full with { District = "" }),
            ("AddressLine", full with { AddressLine = null }),
            ("Name", full with { Name = " " }),
        ];

        foreach ((string field, AddressBookInput input) in cases)
        {
            AddressBookSaveResult refused = await RunAsync(m => m.CreateAddressBookEntryAsync(member, input, Ct));
            Assert.Equal([new MembershipFieldError(field, ErrorCode.FieldRequired)], refused.Fields);
        }

        Assert.Equal(0, await CountAsync("address_books"));
        Guid saved = (await RunAsync(m => m.CreateAddressBookEntryAsync(member, full, Ct))).EntryId!.Value;
        foreach ((string field, AddressBookInput input) in cases)
        {
            AddressBookSaveResult refused = await RunAsync(m => m.UpdateAddressBookEntryAsync(member, saved, input, Ct));
            Assert.Equal([new MembershipFieldError(field, ErrorCode.FieldRequired)], refused.Fields);
        }

        AddressBookEntry kept = (await ReadAsync(m => m.GetAddressBookEntryAsync(member, saved, Ct)))!;
        Assert.Equal("Konak", kept.District);
        Assert.Equal(1, await CountAsync("address_books"));
    }

    // ---- KK9: the telephone ----

    [Fact]
    public async Task TheShippingAddress_AsksATelephone_TheBookEntryDoesNot_AndAnEntryWithoutOneTakesTheOneOfTheStep_KK9()
    {
        Guid member = await SeedCustomerAsync("ali@example.com");
        Guid noPhone = (await RunAsync(m => m.CreateAddressBookEntryAsync(member, Book("Ev", Ankara) with { Phone = null }, Ct))).EntryId!.Value;
        Guid badPhone = (await RunAsync(m => m.CreateAddressBookEntryAsync(member, Book("Kötü", Ankara) with { Phone = "123" }, Ct))).EntryId ?? Guid.Empty;
        Assert.Equal(Guid.Empty, badPhone);
        Assert.Null((await ReadAsync(m => m.GetAddressBookEntryAsync(member, noPhone, Ct)))!.Phone);

        // A typed shipping address without a telephone is refused; the billing address never asks one.
        OrderAddressResolution typed = await RunAsync(m => m.ResolveOrderAddressesAsync(Request(null, [ProductType.Physical]) with { ShippingPhone = " " }, Ct));
        Assert.Equal([new MembershipFieldError("ShippingPhone", ErrorCode.FieldRequired)], typed.Fields);
        OrderAddressResolution malformed = await RunAsync(m => m.ResolveOrderAddressesAsync(Request(null, [ProductType.Physical]) with { ShippingPhone = "12" }, Ct));
        Assert.Equal([new MembershipFieldError("ShippingPhone", ErrorCode.InvalidPhone)], malformed.Fields);

        // The entry without a telephone is picked: the step asks for it, and a typed one is stored as ten digits.
        OrderAddressResolution asked = await RunAsync(m => m.ResolveOrderAddressesAsync(Request(member, [ProductType.Physical]) with { AddressBookId = noPhone, ShippingPhone = null }, Ct));
        Assert.Equal([new MembershipFieldError("ShippingPhone", ErrorCode.FieldRequired)], asked.Fields);
        OrderAddressResolution given = await RunAsync(m => m.ResolveOrderAddressesAsync(Request(member, [ProductType.Physical]) with { AddressBookId = noPhone, ShippingPhone = "+90 (532) 111 22 33" }, Ct));
        Assert.Equal("5321112233", given.Shipping!.Phone);
        Assert.Null((await ReadAsync(m => m.GetAddressBookEntryAsync(member, noPhone, Ct)))!.Phone);

        // A digital order has no shipping telephone to ask.
        OrderAddressResolution digital = await RunAsync(m => m.ResolveOrderAddressesAsync(Request(null, [ProductType.Digital]) with { IsBillingSameAsShipping = false, ShippingPhone = null, BillingRecipientName = "A B", BillingProvinceId = Ankara, BillingDistrict = "X", BillingAddressLine = "Y" }, Ct));
        Assert.True(digital.IsResolved);
    }

    [Fact]
    public async Task TheStoredAddresses_HaveNoPostalCodeAndNoNationalIdentityNumberColumn_KK9()
    {
        foreach (string table in new[] { "address_books", "orders" })
        {
            List<string> columns = await ColumnsAsync(table);
            Assert.NotEmpty(columns);
            Assert.DoesNotContain(columns, c => c.Contains("postal", StringComparison.Ordinal) || c.Contains("zip", StringComparison.Ordinal) || c.Contains("national", StringComparison.Ordinal) || c.Contains("identity", StringComparison.Ordinal));
            Assert.DoesNotContain(columns, c => c is "tckn" or "tc_no" or "tc_identity_number" or "citizen_id");
        }

        List<string> orderColumns = await ColumnsAsync("orders");
        Assert.DoesNotContain("billing_phone", orderColumns);
        Assert.Contains("shipping_phone", orderColumns);
        Assert.Contains("phone", await ColumnsAsync("address_books"));
    }

    // ---- helpers ----

    private static AddressBookInput Book(string name, Guid province) => new(name, "Ali Veli", province, "Konak", "Atatürk Cad. 1", "0532 123 45 67");

    private OrderAddressRequest Request(Guid? member, IReadOnlyCollection<ProductType> types) =>
        new(member, types, null, "Ali Veli", Ankara, "Çankaya", "Atatürk Bulvarı 1", "0532 123 45 67", false, null, true, null, null, null, null);

    private async Task<T> RunAsync<T>(Func<IMembershipModule, Task<T>> call)
        where T : IOperationResult
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        ITransactionRunner runner = scope.ServiceProvider.GetRequiredService<ITransactionRunner>();
        IMembershipModule module = scope.ServiceProvider.GetRequiredService<IMembershipModule>();
        return await runner.RunAsync(_ => call(module), Ct);
    }

    private async Task<T> ReadAsync<T>(Func<IMembershipModule, Task<T>> call)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<IMembershipModule>());
    }

    /// <summary>Opens a transaction, runs the calls and commits without any save: a method that saved itself would leave its change behind (09 6 item 3).</summary>
    private async Task CommitWithoutSavingAsync(Func<IMembershipModule, Task> calls)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(Ct);
        await calls(scope.ServiceProvider.GetRequiredService<IMembershipModule>());
        await transaction.CommitAsync(Ct);
    }

    private async Task<Guid> SeedCustomerAsync(string email)
    {
        Customer customer = Customer.Create("Ali", email, "digest-placeholder", Start, null, Start);
        await SaveAsync(c => c.Customers.Add(customer));
        return customer.Id;
    }

    private async Task<Guid> SeedOrderAsync(OrderAddressResolution resolved)
    {
        int n = Interlocked.Increment(ref _orderCounter);
        Order order = Order.Create($"2026-{n:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", PaymentMethod.Card, true, "ali@example.com", 120m, 0m, Start.AddHours(1), Start);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.FreezeAddresses(
            resolved.Shipping is { } s ? (s.RecipientName, s.Phone, s.ProvinceId, s.District, s.AddressLine) : null,
            (resolved.Billing!.RecipientName, resolved.Billing.ProvinceId, resolved.Billing.District, resolved.Billing.AddressLine));
        order.CompanyType = CompanyType.Tradesman;
        order.CompanyLegalName = "Örnek Ticaret";
        order.CompanyTaxIdNumber = "1234567890";
        order.CompanyProvinceId = Ankara;
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
        await SaveAsync(c =>
        {
            c.Orders.Add(order);
            c.OrderItems.Add(item);
        });
        return order.Id;
    }

    private Task<string> OrderAddressesAsync(Guid orderId) =>
        ScalarAsync<string>(
            $"SELECT concat_ws('|', shipping_recipient_name, shipping_phone, shipping_province_id, shipping_district, shipping_address_line, billing_recipient_name, billing_province_id, billing_district, billing_address_line) FROM orders WHERE id = '{orderId}'");

    private async Task<List<string>> ColumnsAsync(string table)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new("SELECT column_name FROM information_schema.columns WHERE table_name = @t AND table_schema = 'public'", connection);
        command.Parameters.AddWithValue("t", table);
        List<string> names = [];
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct);
        while (await reader.ReadAsync(Ct))
        {
            names.Add(reader.GetString(0));
        }

        return names;
    }

    private async Task<long> CountAsync(string fromWhere) => await ScalarAsync<long>("SELECT count(*) FROM " + fromWhere);

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
        return value is null or DBNull ? default! : (T)value;
    }

    /// <summary>The Settings module as the address code sees it: the province list is the real one, every other call is not made.</summary>
    private sealed class ProvinceSettings(ProvinceList provinces) : ISettingsModule
    {
        public Task<IReadOnlyList<ProvinceInfo>> GetProvincesAsync(CancellationToken ct) => provinces.GetAllAsync(ct);

        public Task<HomepageSection> GetHomepageAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<SalesGate> GetSalesGateAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<CompanyIdentity> GetCompanyIdentityAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<CompanyIdentityCompleteness> GetCompanyIdentityCompletenessAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<LegalIdentity> GetLegalIdentityAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<SettingsSaveResult> SaveCompanyIdentityAsync(CompanyIdentityInput input, int formVersion, CancellationToken ct) => throw new NotSupportedException();

        public Task<ReturnAddressSection> GetReturnAddressAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<SettingsSaveResult> SaveReturnAddressAsync(ReturnAddressInput input, int formVersion, CancellationToken ct) => throw new NotSupportedException();

        public Task<ShippingFeeSection> GetShippingFeeAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<SettingsSaveResult> SaveShippingFeeAsync(ShippingFeeInput input, int formVersion, CancellationToken ct) => throw new NotSupportedException();

        public Task<PeriodsSection> GetPeriodsAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<SettingsSaveResult> SavePeriodsAsync(PeriodsInput input, int formVersion, CancellationToken ct) => throw new NotSupportedException();

        public Task<ProductDefaultsSection> GetProductDefaultsAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<SettingsSaveResult> SaveProductDefaultsAsync(ProductDefaultsInput input, int formVersion, CancellationToken ct) => throw new NotSupportedException();

        public Task<SalesSwitchResult> CloseSalesTemporarilyAsync(bool shownIsClosed, CancellationToken ct) => throw new NotSupportedException();

        public Task<SalesSwitchResult> ReopenSalesAsync(bool shownIsClosed, CancellationToken ct) => throw new NotSupportedException();

        public Task<IReadOnlyList<LegalTextSummary>> GetLegalTextsAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<LegalTextDraft> GetLegalTextDraftAsync(LegalTextType type, CancellationToken ct) => throw new NotSupportedException();

        public Task<SettingsSaveResult> SaveLegalTextDraftAsync(LegalTextType type, string text, string? hostingLocation, int formVersion, CancellationToken ct) => throw new NotSupportedException();

        public Task<LegalTextPublishResult> PublishLegalTextAsync(LegalTextType type, int formVersion, CancellationToken ct) => throw new NotSupportedException();

        public Task<LegalTextVersionIds> GetCurrentLegalTextVersionsAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<bool> AreShownLegalTextVersionsCurrentAsync(Guid shownPreInformationFormId, Guid shownDistanceSalesContractId, CancellationToken ct) => throw new NotSupportedException();

        public Task<LegalTextVersionInfo?> GetLegalTextVersionAsync(Guid versionId, CancellationToken ct) => throw new NotSupportedException();

        public Task<LegalTextVersionInfo?> GetLatestLegalTextVersionAsync(LegalTextType type, CancellationToken ct) => throw new NotSupportedException();

        public Task SynchronizeGeneratedLegalTextsAsync(CancellationToken ct) => throw new NotSupportedException();
    }
}
