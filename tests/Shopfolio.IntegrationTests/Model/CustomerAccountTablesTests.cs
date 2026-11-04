using System.Globalization;
using System.Reflection;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Migrations;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Settings;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the customer account tests (T107): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(CustomerAccountTablesDefinition.Name)]
public sealed class CustomerAccountTablesDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "CustomerAccountTables";
}

/// <summary>
/// The customer account, its address book and its pending email change on a real PostgreSQL (T107 criteria 1-6; 06 4.36, 4.40, 4.45, 7.1,
/// 7.2, 7.3, 7.5; 09 10.6): the migration made the three tables; the database itself refuses what the rules refuse; the account's deletion
/// takes its address book and its pending change with it.
/// </summary>
[Collection(CustomerAccountTablesDefinition.Name)]
public sealed class CustomerAccountTablesTests : IAsyncLifetime
{
    private const string CheckViolation = PostgresErrorCodes.CheckViolation;
    private const string UniqueViolation = PostgresErrorCodes.UniqueViolation;
    private const string ForeignKeyViolation = PostgresErrorCodes.ForeignKeyViolation;

    private static readonly string[] ThreeTables = ["address_books", "customers", "pending_email_changes"];

    private static readonly DateTimeOffset Now = new(2026, 11, 4, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    public CustomerAccountTablesTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE address_books, pending_email_changes, customers, admins, provinces CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the migration ----

    [Fact]
    public async Task Migration_MakesTheThreeTables()
    {
        List<string> tables = await QueryAsync(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name = ANY(@names)",
            ("names", ThreeTables));

        Assert.Equal(ThreeTables, tables.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Columns_HaveTheTypesAndNullabilityOfTheFieldTables()
    {
        // 06 4.36, 4.40, 4.45 and 2.5.1 (the common Id and CreatedAt included); the phone is ten characters (Company's pattern).
        string[] expected =
        [
            "address_books.address_line text not null",
            "address_books.created_at timestamp with time zone not null",
            "address_books.customer_id uuid not null",
            "address_books.district text not null",
            "address_books.id uuid not null",
            "address_books.name text not null",
            "address_books.phone character varying(10)",
            "address_books.province_id uuid not null",
            "address_books.recipient_name text not null",
            "customers.created_at timestamp with time zone not null",
            "customers.email text not null",
            "customers.email_verified_at timestamp with time zone",
            "customers.google_linked_at timestamp with time zone",
            "customers.id uuid not null",
            "customers.name text not null",
            "customers.normalized_email text not null",
            "customers.password_hash text",
            "pending_email_changes.admin_id uuid",
            "pending_email_changes.created_at timestamp with time zone not null",
            "pending_email_changes.customer_id uuid",
            "pending_email_changes.id uuid not null",
            "pending_email_changes.new_email text not null",
            "pending_email_changes.normalized_new_email text not null",
        ];

        List<string> columns = await QueryAsync(
            """
            SELECT c.relname || '.' || a.attname || ' ' || format_type(a.atttypid, a.atttypmod) || CASE WHEN a.attnotnull THEN ' not null' ELSE '' END
            FROM pg_attribute a JOIN pg_class c ON c.oid = a.attrelid JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE n.nspname = 'public' AND c.relname = ANY(@names) AND a.attnum > 0 AND NOT a.attisdropped
            """,
            ("names", ThreeTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), columns.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ThreeTables_HaveNoColumnDefault_BecauseTheFieldTablesNameNone()
    {
        // 06 4.36, 4.40, 4.45: the "Default" column is "-" in every row.
        List<string> defaults = await QueryAsync(
            "SELECT table_name || '.' || column_name FROM information_schema.columns WHERE table_schema = 'public' AND table_name = ANY(@names) AND column_default IS NOT NULL",
            ("names", ThreeTables));

        Assert.Empty(defaults);
    }

    [Fact]
    public async Task Indexes_AreTheDocumentedOnes()
    {
        // 06 4.36 (unique normalized email), 4.40 (two filtered unique links; the new address is not unique), 4.45 (the account and the province),
        // 7.1, 7.5.
        string[] expected =
        [
            "address_books.ix_address_books_customer_id: CREATE INDEX ix_address_books_customer_id ON public.address_books USING btree (customer_id)",
            "address_books.ix_address_books_province_id: CREATE INDEX ix_address_books_province_id ON public.address_books USING btree (province_id)",
            "address_books.pk_address_books: CREATE UNIQUE INDEX pk_address_books ON public.address_books USING btree (id)",
            "customers.ix_customers_normalized_email: CREATE UNIQUE INDEX ix_customers_normalized_email ON public.customers USING btree (normalized_email)",
            "customers.pk_customers: CREATE UNIQUE INDEX pk_customers ON public.customers USING btree (id)",
            "pending_email_changes.ix_pending_email_changes_admin_id: CREATE UNIQUE INDEX ix_pending_email_changes_admin_id ON public.pending_email_changes USING btree (admin_id) WHERE (admin_id IS NOT NULL)",
            "pending_email_changes.ix_pending_email_changes_customer_id: CREATE UNIQUE INDEX ix_pending_email_changes_customer_id ON public.pending_email_changes USING btree (customer_id) WHERE (customer_id IS NOT NULL)",
            "pending_email_changes.pk_pending_email_changes: CREATE UNIQUE INDEX pk_pending_email_changes ON public.pending_email_changes USING btree (id)",
        ];

        List<string> indexes = await QueryAsync(
            "SELECT tablename || '.' || indexname || ': ' || indexdef FROM pg_indexes WHERE schemaname = 'public' AND tablename = ANY(@names)",
            ("names", ThreeTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), indexes.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ForeignKeys_HaveTheDeleteBehaviorOfTheLinkTable()
    {
        // 06 7.2: the account's links cascade; the province link is restrictive (NO ACTION, never RESTRICT: 06 4 introduction; K-1038).
        string[] expected =
        [
            "address_books fk_address_books_customers_customer_id c",
            "address_books fk_address_books_provinces_province_id a",
            "pending_email_changes fk_pending_email_changes_admins_admin_id c",
            "pending_email_changes fk_pending_email_changes_customers_customer_id c",
        ];

        List<string> links = await QueryAsync(
            "SELECT conrelid::regclass::text || ' ' || conname || ' ' || confdeltype::text FROM pg_constraint WHERE contype = 'f' AND conrelid = ANY(ARRAY['public.address_books'::regclass, 'public.pending_email_changes'::regclass])");

        Assert.Equal(expected.Order(StringComparer.Ordinal), links.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task CheckConstraints_AreTheDocumentedOnes()
    {
        // 06 7.3: the customer's unverified shape, the pending change's exactly-one link, the address book's phone.
        string[] expected =
        [
            "address_books.ck_address_books_phone_ten_digits",
            "customers.ck_customers_unverified_has_password_and_no_google",
            "pending_email_changes.ck_pending_email_changes_exactly_one_account",
        ];

        List<string> checks = await QueryAsync(
            "SELECT conrelid::regclass::text || '.' || conname FROM pg_constraint WHERE contype = 'c' AND conrelid = ANY(ARRAY['public.address_books'::regclass, 'public.customers'::regclass, 'public.pending_email_changes'::regclass])");

        Assert.Equal(expected.Order(StringComparer.Ordinal), checks.Order(StringComparer.Ordinal));
    }

    // ---- KK2: the customer's email ----

    [Fact]
    public async Task SecondCustomerWithTheSameEmail_InAnotherCase_IsRefused()
    {
        await SaveAsync(c => c.Customers.Add(Verified("Ali", "Ali@Example.com")));
        Customer second = Verified("Veli", "ALI@EXAMPLE.COM");

        Assert.Equal((UniqueViolation, "ix_customers_normalized_email"), await ViolationOfAsync(c => c.Customers.Add(second)));
    }

    [Fact]
    public async Task SecondCustomerWithTheSameEmail_WithTurkishDottedAndDotlessLetters_IsRefused()
    {
        await SaveAsync(c => c.Customers.Add(Verified("Ali", "ISIK@example.com")));
        Customer second = Verified("Veli", "ısık@example.com");

        Assert.Equal((UniqueViolation, "ix_customers_normalized_email"), await ViolationOfAsync(c => c.Customers.Add(second)));
    }

    [Fact]
    public async Task UnverifiedPendingRow_HoldsTheAddress_AgainstAVerifiedRow()
    {
        // 06 4.36: the pending row counts in the uniqueness; the same address is not open to a new registration until it expires.
        await SaveAsync(c => c.Customers.Add(Customer.Create("Ali", "ali@example.com", "digest-placeholder", null, null, Now)));
        Customer second = Verified("Veli", "ALI@example.com");

        Assert.Equal((UniqueViolation, "ix_customers_normalized_email"), await ViolationOfAsync(c => c.Customers.Add(second)));
    }

    [Fact]
    public async Task CustomerAndAdmin_MayShareTheSameEmail()
    {
        // 06 4.37: the two account types are separate tables; the same address may be used in both.
        await SaveAsync(c =>
        {
            c.Customers.Add(Verified("Ali", "ali@example.com"));
            c.Admins.Add(Admin.Create("Ali", "ali@example.com", "digest-placeholder", Now));
        });

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM customers"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM admins"));
    }

    // ---- KK3: the unverified shape ----

    [Fact]
    public async Task UnverifiedCustomer_WithoutPasswordDigest_IsRefused()
    {
        Customer customer = Customer.Create("Ali", "ali@example.com", null, null, null, Now);

        Assert.Equal((CheckViolation, "ck_customers_unverified_has_password_and_no_google"), await ViolationOfAsync(c => c.Customers.Add(customer)));
    }

    [Fact]
    public async Task UnverifiedCustomer_WithGoogleLink_IsRefused()
    {
        Customer customer = Customer.Create("Ali", "ali@example.com", "digest-placeholder", null, Now, Now);

        Assert.Equal((CheckViolation, "ck_customers_unverified_has_password_and_no_google"), await ViolationOfAsync(c => c.Customers.Add(customer)));
    }

    [Fact]
    public async Task UnverifiedCustomer_WithoutPasswordAndWithGoogleLink_IsRefused()
    {
        Customer customer = Customer.Create("Ali", "ali@example.com", null, null, Now, Now);

        Assert.Equal((CheckViolation, "ck_customers_unverified_has_password_and_no_google"), await ViolationOfAsync(c => c.Customers.Add(customer)));
    }

    [Fact]
    public async Task UnverifiedCustomer_WithPasswordDigestAndNoGoogleLink_IsAccepted()
    {
        await SaveAsync(c => c.Customers.Add(Customer.Create("Ali", "ali@example.com", "digest-placeholder", null, null, Now)));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM customers WHERE email_verified_at IS NULL"));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task VerifiedCustomer_IsAcceptedWithOrWithoutPasswordAndGoogleLink(bool withPassword, bool withGoogle)
    {
        Customer customer = Customer.Create("Ali", "ali@example.com", withPassword ? "digest-placeholder" : null, Now, withGoogle ? Now : null, Now);

        await SaveAsync(c => c.Customers.Add(customer));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM customers WHERE email_verified_at IS NOT NULL"));
    }

    // ---- KK4: the pending email change ----

    [Fact]
    public async Task PendingChange_LinkedToNeitherAccount_IsRefused()
    {
        PendingEmailChange change = PendingEmailChange.ForCustomer(Guid.Empty, "yeni@example.com", Now);
        change.CustomerId = null;

        Assert.Equal((CheckViolation, "ck_pending_email_changes_exactly_one_account"), await ViolationOfAsync(c => c.PendingEmailChanges.Add(change)));
    }

    [Fact]
    public async Task PendingChange_LinkedToBothAccounts_IsRefused()
    {
        Customer customer = Verified("Ali", "ali@example.com");
        Admin admin = Admin.Create("Veli", "veli@example.com", "digest-placeholder", Now);
        await SaveAsync(c =>
        {
            c.Customers.Add(customer);
            c.Admins.Add(admin);
        });
        PendingEmailChange change = PendingEmailChange.ForCustomer(customer.Id, "yeni@example.com", Now);
        change.AdminId = admin.Id;

        Assert.Equal((CheckViolation, "ck_pending_email_changes_exactly_one_account"), await ViolationOfAsync(c => c.PendingEmailChanges.Add(change)));
    }

    [Fact]
    public async Task SecondPendingChange_OfTheSameCustomer_IsRefused()
    {
        Customer customer = Verified("Ali", "ali@example.com");
        await SaveAsync(c => c.Customers.Add(customer));
        await SaveAsync(c => c.PendingEmailChanges.Add(PendingEmailChange.ForCustomer(customer.Id, "yeni@example.com", Now)));
        PendingEmailChange second = PendingEmailChange.ForCustomer(customer.Id, "baska@example.com", Now);

        Assert.Equal((UniqueViolation, "ix_pending_email_changes_customer_id"), await ViolationOfAsync(c => c.PendingEmailChanges.Add(second)));
    }

    [Fact]
    public async Task SecondPendingChange_OfTheSameAdmin_IsRefused()
    {
        Admin admin = Admin.Create("Veli", "veli@example.com", "digest-placeholder", Now);
        await SaveAsync(c => c.Admins.Add(admin));
        await SaveAsync(c => c.PendingEmailChanges.Add(PendingEmailChange.ForAdmin(admin.Id, "yeni@example.com", Now)));
        PendingEmailChange second = PendingEmailChange.ForAdmin(admin.Id, "baska@example.com", Now);

        Assert.Equal((UniqueViolation, "ix_pending_email_changes_admin_id"), await ViolationOfAsync(c => c.PendingEmailChanges.Add(second)));
    }

    [Fact]
    public async Task TwoAccounts_MayWaitForTheSameNewEmail_InAnyCase()
    {
        // 06 4.40: the new address is not unique; the account's own unique index stops the second at verification.
        Customer first = Verified("Ali", "ali@example.com");
        Customer second = Verified("Veli", "veli@example.com");
        Admin admin = Admin.Create("Ayşe", "ayse@example.com", "digest-placeholder", Now);
        await SaveAsync(c =>
        {
            c.Customers.AddRange(first, second);
            c.Admins.Add(admin);
        });

        await SaveAsync(c => c.PendingEmailChanges.AddRange(
            PendingEmailChange.ForCustomer(first.Id, "Yeni@Example.com", Now),
            PendingEmailChange.ForCustomer(second.Id, "YENI@EXAMPLE.COM", Now),
            PendingEmailChange.ForAdmin(admin.Id, "yeni@example.com", Now)));

        Assert.Equal(3L, await ScalarAsync<long>("SELECT count(*) FROM pending_email_changes WHERE normalized_new_email = 'yeni@example.com'"));
    }

    [Fact]
    public async Task CustomerAndAdmin_MayEachHaveOnePendingChange()
    {
        Customer customer = Verified("Ali", "ali@example.com");
        Admin admin = Admin.Create("Veli", "veli@example.com", "digest-placeholder", Now);
        await SaveAsync(c =>
        {
            c.Customers.Add(customer);
            c.Admins.Add(admin);
        });

        await SaveAsync(c => c.PendingEmailChanges.AddRange(
            PendingEmailChange.ForCustomer(customer.Id, "yeni@example.com", Now),
            PendingEmailChange.ForAdmin(admin.Id, "baska@example.com", Now)));

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM pending_email_changes"));
    }

    [Fact]
    public async Task PendingChange_OfAnUnknownCustomer_IsRefused()
    {
        PendingEmailChange change = PendingEmailChange.ForCustomer(EntityIds.New(), "yeni@example.com", Now);

        Assert.Equal((ForeignKeyViolation, "fk_pending_email_changes_customers_customer_id"), await ViolationOfAsync(c => c.PendingEmailChanges.Add(change)));
    }

    // ---- KK5: the address book's phone ----

    [Theory]
    [InlineData("532123456")]
    [InlineData("53212345678")]
    [InlineData("+905321234567")]
    [InlineData("53212345a7")]
    [InlineData("532 123 45 67")]
    public async Task AddressWithAPhoneOtherThanTenDigits_IsRefused(string phone)
    {
        (Customer customer, Province province) = await SeedCustomerAndProvinceAsync();
        AddressBook entry = Entry(customer, province, phone);

        // Eleven digits do not fit the column at all; the others reach the check.
        (string state, string? constraint) = await ViolationOfAsync(c => c.AddressBooks.Add(entry));

        Assert.True(
            (state == CheckViolation && constraint == "ck_address_books_phone_ten_digits") || state == PostgresErrorCodes.StringDataRightTruncation,
            $"unexpected {state} {constraint}");
    }

    [Theory]
    [InlineData("5321234567")]
    [InlineData("0000000000")]
    public async Task AddressWithATenDigitPhone_IsAccepted(string phone)
    {
        (Customer customer, Province province) = await SeedCustomerAndProvinceAsync();

        await SaveAsync(c => c.AddressBooks.Add(Entry(customer, province, phone)));

        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM address_books WHERE phone = '{phone}'"));
    }

    [Fact]
    public async Task AddressWithoutAPhone_IsAccepted_BecauseThePhoneIsOptionalInTheBook()
    {
        (Customer customer, Province province) = await SeedCustomerAndProvinceAsync();

        await SaveAsync(c => c.AddressBooks.Add(Entry(customer, province, null)));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM address_books WHERE phone IS NULL"));
    }

    [Fact]
    public async Task Address_OfAnUnknownCustomer_IsRefused()
    {
        (Customer customer, Province province) = await SeedCustomerAndProvinceAsync();
        AddressBook entry = AddressBook.Create(EntityIds.New(), "Ev", "Ali Yılmaz", province.Id, "Çankaya", "Atatürk Bulvarı 1/2", null, Now);

        Assert.Equal((ForeignKeyViolation, "fk_address_books_customers_customer_id"), await ViolationOfAsync(c => c.AddressBooks.Add(entry)));
        Assert.NotNull(customer);
    }

    [Fact]
    public async Task Address_OfAnUnknownProvince_IsRefused()
    {
        (Customer customer, _) = await SeedCustomerAndProvinceAsync();
        AddressBook entry = AddressBook.Create(customer.Id, "Ev", "Ali Yılmaz", EntityIds.New(), "Çankaya", "Atatürk Bulvarı 1/2", null, Now);

        Assert.Equal((ForeignKeyViolation, "fk_address_books_provinces_province_id"), await ViolationOfAsync(c => c.AddressBooks.Add(entry)));
    }

    [Fact]
    public async Task Province_UsedByAnAddress_CannotBeDeleted()
    {
        (Customer customer, Province province) = await SeedCustomerAndProvinceAsync();
        await SaveAsync(c => c.AddressBooks.Add(Entry(customer, province, null)));

        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM provinces WHERE id = '{province.Id}'"));

        Assert.Equal(ForeignKeyViolation, error.SqlState);
    }

    // ---- KK6: the account's deletion ----

    [Fact]
    public async Task DeletingTheCustomer_TakesTheAddressBookAndThePendingChangeWithIt()
    {
        (Customer customer, Province province) = await SeedCustomerAndProvinceAsync();
        Customer other = Verified("Veli", "veli@example.com");
        await SaveAsync(c => c.Customers.Add(other));
        await SaveAsync(c =>
        {
            c.AddressBooks.Add(Entry(customer, province, "5321234567"));
            c.AddressBooks.Add(Entry(customer, province, null));
            c.AddressBooks.Add(Entry(other, province, null));
            c.PendingEmailChanges.Add(PendingEmailChange.ForCustomer(customer.Id, "yeni@example.com", Now));
            c.PendingEmailChanges.Add(PendingEmailChange.ForCustomer(other.Id, "baska@example.com", Now));
        });

        await ExecuteAsync($"DELETE FROM customers WHERE id = '{customer.Id}'");

        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM address_books WHERE customer_id = '{customer.Id}'"));
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM pending_email_changes WHERE customer_id = '{customer.Id}'"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM address_books"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM pending_email_changes"));
    }

    [Fact]
    public async Task DeletingTheAdmin_TakesThePendingChangeWithIt_AndLeavesTheCustomers()
    {
        Admin admin = Admin.Create("Veli", "veli@example.com", "digest-placeholder", Now);
        Customer customer = Verified("Ali", "ali@example.com");
        await SaveAsync(c =>
        {
            c.Admins.Add(admin);
            c.Customers.Add(customer);
        });
        await SaveAsync(c =>
        {
            c.PendingEmailChanges.Add(PendingEmailChange.ForAdmin(admin.Id, "yeni@example.com", Now));
            c.PendingEmailChanges.Add(PendingEmailChange.ForCustomer(customer.Id, "baska@example.com", Now));
        });

        await ExecuteAsync($"DELETE FROM admins WHERE id = '{admin.Id}'");

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM pending_email_changes WHERE admin_id IS NOT NULL"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM pending_email_changes WHERE customer_id IS NOT NULL"));
    }

    // ---- KK7: one topic, no Down ----

    [Fact]
    public void Migration_HasNoDownMethod()
    {
        MethodInfo? down = typeof(CreateCustomerAddressBookAndPendingEmailChangeTables)
            .GetMethod("Down", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);

        Assert.Null(down);
    }

    // ---- helpers ----

    private static Customer Verified(string name, string email) => Customer.Create(name, email, "digest-placeholder", Now, null, Now);

    private static AddressBook Entry(Customer customer, Province province, string? phone) =>
        AddressBook.Create(customer.Id, "Ev", "Ali Yılmaz", province.Id, "Çankaya", "Atatürk Bulvarı 1/2", phone, Now);

    private async Task<(Customer Customer, Province Province)> SeedCustomerAndProvinceAsync()
    {
        Customer customer = Verified("Ali", "ali@example.com");
        Province province = Province.Create("06", "Ankara", Now);
        await SaveAsync(c =>
        {
            c.Customers.Add(customer);
            c.Provinces.Add(province);
        });
        return (customer, province);
    }

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

    /// <summary>Saves the change and returns the SQLSTATE and the constraint name of the database error it raises.</summary>
    private async Task<(string SqlState, string? Constraint)> ViolationOfAsync(Action<AppDbContext> change)
    {
        await using AppDbContext context = NewContext();
        change(context);

        DbUpdateException error = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(Ct));

        PostgresException inner = Assert.IsType<PostgresException>(error.InnerException);
        return (inner.SqlState, inner.ConstraintName);
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
        return (T)(await command.ExecuteScalarAsync(Ct))!;
    }

    private async Task<List<string>> QueryAsync(string sql, params (string Name, object Value)[] parameters)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        foreach ((string name, object value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        List<string> rows = [];
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct);
        while (await reader.ReadAsync(Ct))
        {
            rows.Add(Convert.ToString(reader.GetValue(0), CultureInfo.InvariantCulture)!);
        }

        return rows;
    }
}
