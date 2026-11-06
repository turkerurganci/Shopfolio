using System.Globalization;
using System.Net;
using System.Reflection;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Migrations;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Membership;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the session, link, attempt and sign-in record tests (T108): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(SessionLinkAttemptLoginTablesDefinition.Name)]
public sealed class SessionLinkAttemptLoginTablesDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "SessionLinkAttemptLoginTables";
}

/// <summary>
/// The five security record tables on a real PostgreSQL (T108 criteria 1-8; 06 4.39, 4.41-4.44, 7.1, 7.2, 7.3, 7.5; 09 10.6): the migration
/// made the tables; the database itself refuses what the rules refuse; the account's, the owner's and the invitation's deletion take the
/// rows that belong to them, and the attempt and sign-in rows stay because they carry values and no link.
/// </summary>
[Collection(SessionLinkAttemptLoginTablesDefinition.Name)]
public sealed class SessionLinkAttemptLoginTablesTests : IAsyncLifetime
{
    private const string CheckViolation = PostgresErrorCodes.CheckViolation;
    private const string UniqueViolation = PostgresErrorCodes.UniqueViolation;

    private static readonly string[] FiveTables = ["attempt_records", "link_records", "login_records", "recognized_browser_marks", "sessions"];

    private static readonly DateTimeOffset Now = new(2026, 11, 4, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    public SessionLinkAttemptLoginTablesTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE sessions, recognized_browser_marks, link_records, attempt_records, login_records, customers, admins, admin_invitations CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the migration ----

    [Fact]
    public async Task Migration_MakesTheFiveTables()
    {
        List<string> tables = await QueryAsync(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name = ANY(@names)",
            ("names", FiveTables));

        Assert.Equal(FiveTables, tables.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Columns_HaveTheTypesAndNullabilityOfTheFieldTables()
    {
        // 06 4.39, 4.41-4.44 and 2.5.1 (the common Id and CreatedAt included).
        string[] expected =
        [
            "attempt_records.attempt_axis text not null",
            "attempt_records.attempt_limit text not null",
            "attempt_records.created_at timestamp with time zone not null",
            "attempt_records.id uuid not null",
            "attempt_records.normalized_value text not null",
            "link_records.admin_id uuid",
            "link_records.admin_invitation_id uuid",
            "link_records.created_at timestamp with time zone not null",
            "link_records.customer_id uuid",
            "link_records.expires_at timestamp with time zone not null",
            "link_records.id uuid not null",
            "link_records.link_type text not null",
            "link_records.previous_email text",
            "link_records.superseded_at timestamp with time zone",
            "link_records.token_hash text not null",
            "link_records.used_at timestamp with time zone",
            "login_records.account_type text not null",
            "login_records.created_at timestamp with time zone not null",
            "login_records.id uuid not null",
            "login_records.ip_address inet not null",
            "login_records.normalized_email text not null",
            "login_records.outcome text not null",
            "recognized_browser_marks.admin_id uuid",
            "recognized_browser_marks.created_at timestamp with time zone not null",
            "recognized_browser_marks.customer_id uuid",
            "recognized_browser_marks.expires_at timestamp with time zone not null",
            "recognized_browser_marks.id uuid not null",
            "recognized_browser_marks.mark_hash text not null",
            "sessions.admin_id uuid",
            "sessions.created_at timestamp with time zone not null",
            "sessions.customer_id uuid",
            "sessions.id uuid not null",
            "sessions.is_long_lived boolean not null",
            "sessions.last_activity_at timestamp with time zone not null",
            "sessions.reauthenticated_operation text",
            "sessions.reauthentication_token_hash text",
            "sessions.token_hash text not null",
        ];

        List<string> columns = await QueryAsync(
            """
            SELECT c.relname || '.' || a.attname || ' ' || format_type(a.atttypid, a.atttypmod) || CASE WHEN a.attnotnull THEN ' not null' ELSE '' END
            FROM pg_attribute a JOIN pg_class c ON c.oid = a.attrelid JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE n.nspname = 'public' AND c.relname = ANY(@names) AND a.attnum > 0 AND NOT a.attisdropped
            """,
            ("names", FiveTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), columns.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task FiveTables_HaveNoColumnDefault_BecauseTheFieldTablesNameNone()
    {
        // 06 4.39, 4.41-4.44: the "Default" column is "-" in every row.
        List<string> defaults = await QueryAsync(
            "SELECT table_name || '.' || column_name FROM information_schema.columns WHERE table_schema = 'public' AND table_name = ANY(@names) AND column_default IS NOT NULL",
            ("names", FiveTables));

        Assert.Empty(defaults);
    }

    [Fact]
    public async Task Indexes_AreTheDocumentedOnes()
    {
        // 06 7.1 (the three unique digests), 7.5 (the cleanup, the window count, the read command's filters) and the foreign key indexes of 7.2.
        string[] expected =
        [
            "attempt_records.ix_attempt_records_attempt_limit_attempt_axis_normalized_value | CREATE INDEX ix_attempt_records_attempt_limit_attempt_axis_normalized_value ON public.attempt_records USING btree (attempt_limit, attempt_axis, normalized_value, created_at)",
            "attempt_records.ix_attempt_records_created_at | CREATE INDEX ix_attempt_records_created_at ON public.attempt_records USING btree (created_at)",
            "attempt_records.pk_attempt_records | CREATE UNIQUE INDEX pk_attempt_records ON public.attempt_records USING btree (id)",
            "link_records.ix_link_records_admin_id | CREATE INDEX ix_link_records_admin_id ON public.link_records USING btree (admin_id)",
            "link_records.ix_link_records_admin_invitation_id | CREATE INDEX ix_link_records_admin_invitation_id ON public.link_records USING btree (admin_invitation_id)",
            "link_records.ix_link_records_customer_id | CREATE INDEX ix_link_records_customer_id ON public.link_records USING btree (customer_id)",
            "link_records.ix_link_records_expires_at | CREATE INDEX ix_link_records_expires_at ON public.link_records USING btree (expires_at) WHERE (previous_email IS NOT NULL)",
            "link_records.ix_link_records_link_type_expires_at | CREATE INDEX ix_link_records_link_type_expires_at ON public.link_records USING btree (link_type, expires_at)",
            "link_records.ix_link_records_token_hash | CREATE UNIQUE INDEX ix_link_records_token_hash ON public.link_records USING btree (token_hash)",
            "link_records.pk_link_records | CREATE UNIQUE INDEX pk_link_records ON public.link_records USING btree (id)",
            "login_records.ix_login_records_created_at | CREATE INDEX ix_login_records_created_at ON public.login_records USING btree (created_at)",
            "login_records.ix_login_records_ip_address | CREATE INDEX ix_login_records_ip_address ON public.login_records USING btree (ip_address)",
            "login_records.ix_login_records_normalized_email | CREATE INDEX ix_login_records_normalized_email ON public.login_records USING btree (normalized_email)",
            "login_records.pk_login_records | CREATE UNIQUE INDEX pk_login_records ON public.login_records USING btree (id)",
            "recognized_browser_marks.ix_recognized_browser_marks_admin_id | CREATE INDEX ix_recognized_browser_marks_admin_id ON public.recognized_browser_marks USING btree (admin_id)",
            "recognized_browser_marks.ix_recognized_browser_marks_customer_id | CREATE INDEX ix_recognized_browser_marks_customer_id ON public.recognized_browser_marks USING btree (customer_id)",
            "recognized_browser_marks.ix_recognized_browser_marks_expires_at | CREATE INDEX ix_recognized_browser_marks_expires_at ON public.recognized_browser_marks USING btree (expires_at)",
            "recognized_browser_marks.ix_recognized_browser_marks_mark_hash | CREATE UNIQUE INDEX ix_recognized_browser_marks_mark_hash ON public.recognized_browser_marks USING btree (mark_hash)",
            "recognized_browser_marks.pk_recognized_browser_marks | CREATE UNIQUE INDEX pk_recognized_browser_marks ON public.recognized_browser_marks USING btree (id)",
            "sessions.ix_sessions_admin_id | CREATE INDEX ix_sessions_admin_id ON public.sessions USING btree (admin_id)",
            "sessions.ix_sessions_customer_id | CREATE INDEX ix_sessions_customer_id ON public.sessions USING btree (customer_id)",
            "sessions.ix_sessions_last_activity_at | CREATE INDEX ix_sessions_last_activity_at ON public.sessions USING btree (last_activity_at)",
            "sessions.ix_sessions_token_hash | CREATE UNIQUE INDEX ix_sessions_token_hash ON public.sessions USING btree (token_hash)",
            "sessions.pk_sessions | CREATE UNIQUE INDEX pk_sessions ON public.sessions USING btree (id)",
        ];

        List<string> indexes = await QueryAsync(
            "SELECT tablename || '.' || indexname || ' | ' || indexdef FROM pg_indexes WHERE schemaname = 'public' AND tablename = ANY(@names)",
            ("names", FiveTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), indexes.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ForeignKeys_HaveTheDeleteBehaviorOfTheLinkTable()
    {
        // 06 7.2: all eight links cascade; the attempt and sign-in tables have none (06 7.2.3).
        string[] expected =
        [
            "link_records fk_link_records_admin_invitations_admin_invitation_id c",
            "link_records fk_link_records_admins_admin_id c",
            "link_records fk_link_records_customers_customer_id c",
            "recognized_browser_marks fk_recognized_browser_marks_admins_admin_id c",
            "recognized_browser_marks fk_recognized_browser_marks_customers_customer_id c",
            "sessions fk_sessions_admins_admin_id c",
            "sessions fk_sessions_customers_customer_id c",
        ];

        List<string> links = await QueryAsync(
            "SELECT conrelid::regclass::text || ' ' || conname || ' ' || confdeltype::text FROM pg_constraint WHERE contype = 'f' AND conrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x))",
            ("oids", FiveTables.Select(t => "public." + t).ToArray()));

        Assert.Equal(expected.Order(StringComparer.Ordinal), links.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task CheckConstraints_AreTheDocumentedOnes()
    {
        // 06 7.3 and the enum columns' allowed values (06 2.9.2).
        string[] expected =
        [
            "attempt_records.ck_attempt_records_attempt_axis",
            "attempt_records.ck_attempt_records_attempt_limit",
            "attempt_records.ck_attempt_records_axis_is_the_limits_axis",
            "link_records.ck_link_records_at_most_one_of_used_and_superseded",
            "link_records.ck_link_records_customer_only_types_have_customer",
            "link_records.ck_link_records_exactly_one_owner",
            "link_records.ck_link_records_invitation_link_only_for_invitation_type",
            "link_records.ck_link_records_link_type",
            "link_records.ck_link_records_previous_email_only_for_revert_type",
            "login_records.ck_login_records_account_type",
            "login_records.ck_login_records_outcome",
            "recognized_browser_marks.ck_recognized_browser_marks_exactly_one_account",
            "sessions.ck_sessions_admin_without_account_deletion",
            "sessions.ck_sessions_exactly_one_account",
            "sessions.ck_sessions_reauthenticated_operation",
            "sessions.ck_sessions_reauthentication_fields_together",
        ];

        List<string> checks = await QueryAsync(
            "SELECT conrelid::regclass::text || '.' || conname FROM pg_constraint WHERE contype = 'c' AND conrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x))",
            ("oids", FiveTables.Select(t => "public." + t).ToArray()));

        Assert.Equal(expected.Order(StringComparer.Ordinal), checks.Order(StringComparer.Ordinal));
    }

    // ---- KK2: the session ----

    [Fact]
    public async Task Session_LinkedToNeitherAccount_IsRefused()
    {
        Session session = Session.ForCustomer(Guid.Empty, Digest("s1"), false, Now);
        session.CustomerId = null;

        Assert.Equal((CheckViolation, "ck_sessions_exactly_one_account"), await ViolationOfAsync(c => c.Sessions.Add(session)));
    }

    [Fact]
    public async Task Session_LinkedToBothAccounts_IsRefused()
    {
        (Customer customer, Admin admin) = await SeedAccountsAsync();
        Session session = Session.ForCustomer(customer.Id, Digest("s1"), false, Now);
        session.AdminId = admin.Id;

        Assert.Equal((CheckViolation, "ck_sessions_exactly_one_account"), await ViolationOfAsync(c => c.Sessions.Add(session)));
    }

    [Fact]
    public async Task Session_WithOnlyTheOperationOfThePermission_IsRefused()
    {
        (Customer customer, _) = await SeedAccountsAsync();
        Session session = Session.ForCustomer(customer.Id, Digest("s1"), false, Now);
        session.ReauthenticatedOperation = ReauthenticationOperation.PasswordChange;

        Assert.Equal((CheckViolation, "ck_sessions_reauthentication_fields_together"), await ViolationOfAsync(c => c.Sessions.Add(session)));
    }

    [Fact]
    public async Task Session_WithOnlyTheTokenOfThePermission_IsRefused()
    {
        (Customer customer, _) = await SeedAccountsAsync();
        Session session = Session.ForCustomer(customer.Id, Digest("s1"), false, Now);
        session.GrantReauthentication(ReauthenticationOperation.PasswordChange, Digest("r1"));
        session.ReauthenticatedOperation = null;

        Assert.Equal((CheckViolation, "ck_sessions_reauthentication_fields_together"), await ViolationOfAsync(c => c.Sessions.Add(session)));
    }

    [Fact]
    public async Task Session_WithBothFieldsOfThePermission_IsAccepted()
    {
        (Customer customer, Admin admin) = await SeedAccountsAsync();
        Session customerSession = Session.ForCustomer(customer.Id, Digest("s1"), true, Now);
        customerSession.GrantReauthentication(ReauthenticationOperation.AccountDeletion, Digest("r1"));
        Session adminSession = Session.ForAdmin(admin.Id, Digest("s2"), false, Now);
        adminSession.GrantReauthentication(ReauthenticationOperation.EmailChange, Digest("r2"));

        await SaveAsync(c => c.Sessions.AddRange(customerSession, adminSession));

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM sessions WHERE reauthenticated_operation IS NOT NULL"));
    }

    [Fact]
    public async Task AdminSession_WithTheAccountDeletionPermission_IsRefused()
    {
        (_, Admin admin) = await SeedAccountsAsync();
        Session session = Session.ForAdmin(admin.Id, Digest("s1"), false, Now);
        session.GrantReauthentication(ReauthenticationOperation.AccountDeletion, Digest("r1"));

        Assert.Equal((CheckViolation, "ck_sessions_admin_without_account_deletion"), await ViolationOfAsync(c => c.Sessions.Add(session)));
    }

    // ---- KK3: the unique digests and the mark's account ----

    [Fact]
    public async Task SecondSession_WithTheSameTokenDigest_IsRefused()
    {
        (Customer customer, Admin admin) = await SeedAccountsAsync();
        await SaveAsync(c => c.Sessions.Add(Session.ForCustomer(customer.Id, Digest("same"), false, Now)));

        Assert.Equal((UniqueViolation, "ix_sessions_token_hash"), await ViolationOfAsync(c => c.Sessions.Add(Session.ForAdmin(admin.Id, Digest("same"), false, Now))));
    }

    [Fact]
    public async Task SecondMark_WithTheSameDigest_IsRefused()
    {
        (Customer customer, Admin admin) = await SeedAccountsAsync();
        await SaveAsync(c => c.RecognizedBrowserMarks.Add(RecognizedBrowserMark.ForCustomer(customer.Id, Digest("same"), Now.AddDays(30), Now)));

        Assert.Equal(
            (UniqueViolation, "ix_recognized_browser_marks_mark_hash"),
            await ViolationOfAsync(c => c.RecognizedBrowserMarks.Add(RecognizedBrowserMark.ForAdmin(admin.Id, Digest("same"), Now.AddDays(30), Now))));
    }

    [Fact]
    public async Task Mark_LinkedToBothAccounts_IsRefused()
    {
        (Customer customer, Admin admin) = await SeedAccountsAsync();
        RecognizedBrowserMark mark = RecognizedBrowserMark.ForCustomer(customer.Id, Digest("m1"), Now.AddDays(30), Now);
        mark.AdminId = admin.Id;

        Assert.Equal((CheckViolation, "ck_recognized_browser_marks_exactly_one_account"), await ViolationOfAsync(c => c.RecognizedBrowserMarks.Add(mark)));
    }

    [Fact]
    public async Task Mark_LinkedToNeitherAccount_IsRefused()
    {
        RecognizedBrowserMark mark = RecognizedBrowserMark.ForCustomer(Guid.Empty, Digest("m1"), Now.AddDays(30), Now);
        mark.CustomerId = null;

        Assert.Equal((CheckViolation, "ck_recognized_browser_marks_exactly_one_account"), await ViolationOfAsync(c => c.RecognizedBrowserMarks.Add(mark)));
    }

    [Fact]
    public async Task OneMark_PerAccountKind_IsAccepted()
    {
        (Customer customer, Admin admin) = await SeedAccountsAsync();

        await SaveAsync(c => c.RecognizedBrowserMarks.AddRange(
            RecognizedBrowserMark.ForCustomer(customer.Id, Digest("m1"), Now.AddDays(30), Now),
            RecognizedBrowserMark.ForAdmin(admin.Id, Digest("m2"), Now.AddDays(30), Now)));

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM recognized_browser_marks"));
    }

    // ---- KK4: the link record ----

    [Fact]
    public async Task InvitationLink_WithoutAnInvitation_IsRefused()
    {
        (Customer customer, _) = await SeedAccountsAsync();
        LinkRecord link = LinkRecord.ForCustomer(LinkType.AdminInvitation, customer.Id, Digest("l1"), Now.AddHours(1), Now);

        Assert.Equal((CheckViolation, "ck_link_records_invitation_link_only_for_invitation_type"), await ViolationOfAsync(c => c.LinkRecords.Add(link)));
    }

    [Fact]
    public async Task NonInvitationLink_WithAnInvitation_IsRefused()
    {
        AdminInvitation invitation = await SeedInvitationAsync();
        LinkRecord link = LinkRecord.ForInvitation(invitation.Id, Digest("l1"), Now.AddHours(1), Now);
        link.LinkType = LinkType.PasswordReset;

        Assert.Equal((CheckViolation, "ck_link_records_invitation_link_only_for_invitation_type"), await ViolationOfAsync(c => c.LinkRecords.Add(link)));
    }

    [Theory]
    [InlineData(LinkType.EmailVerification)]
    [InlineData(LinkType.GoogleLinkConfirmation)]
    public async Task CustomerOnlyLinkTypes_WithAnAdminLink_AreRefused(LinkType linkType)
    {
        (_, Admin admin) = await SeedAccountsAsync();
        LinkRecord link = LinkRecord.ForAdmin(linkType, admin.Id, Digest("l1"), Now.AddHours(1), Now);

        Assert.Equal((CheckViolation, "ck_link_records_customer_only_types_have_customer"), await ViolationOfAsync(c => c.LinkRecords.Add(link)));
    }

    [Theory]
    [InlineData(LinkType.PasswordReset)]
    [InlineData(LinkType.NewEmailVerification)]
    [InlineData(LinkType.EmailChangeRevert)]
    public async Task OpenLinkTypes_AreAcceptedForAnAdminAndForACustomer(LinkType linkType)
    {
        (Customer customer, Admin admin) = await SeedAccountsAsync();

        await SaveAsync(c => c.LinkRecords.AddRange(
            LinkRecord.ForCustomer(linkType, customer.Id, Digest("l1"), Now.AddHours(1), Now),
            LinkRecord.ForAdmin(linkType, admin.Id, Digest("l2"), Now.AddHours(1), Now)));

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM link_records"));
    }

    [Fact]
    public async Task CustomerOnlyLinkTypes_WithACustomerLink_AreAccepted()
    {
        (Customer customer, _) = await SeedAccountsAsync();

        await SaveAsync(c => c.LinkRecords.AddRange(
            LinkRecord.ForCustomer(LinkType.EmailVerification, customer.Id, Digest("l1"), Now.AddHours(1), Now),
            LinkRecord.ForCustomer(LinkType.GoogleLinkConfirmation, customer.Id, Digest("l2"), Now.AddHours(1), Now)));

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM link_records"));
    }

    [Fact]
    public async Task InvitationLink_WithAnInvitation_IsAccepted()
    {
        AdminInvitation invitation = await SeedInvitationAsync();

        await SaveAsync(c => c.LinkRecords.Add(LinkRecord.ForInvitation(invitation.Id, Digest("l1"), Now.AddHours(1), Now)));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM link_records WHERE link_type = 'AdminInvitation'"));
    }

    [Fact]
    public async Task Link_WithNoOwner_IsRefused()
    {
        LinkRecord link = LinkRecord.ForCustomer(LinkType.PasswordReset, Guid.Empty, Digest("l1"), Now.AddHours(1), Now);
        link.CustomerId = null;

        Assert.Equal((CheckViolation, "ck_link_records_exactly_one_owner"), await ViolationOfAsync(c => c.LinkRecords.Add(link)));
    }

    [Fact]
    public async Task Link_WithTwoOwners_IsRefused()
    {
        (Customer customer, Admin admin) = await SeedAccountsAsync();
        LinkRecord link = LinkRecord.ForCustomer(LinkType.PasswordReset, customer.Id, Digest("l1"), Now.AddHours(1), Now);
        link.AdminId = admin.Id;

        Assert.Equal((CheckViolation, "ck_link_records_exactly_one_owner"), await ViolationOfAsync(c => c.LinkRecords.Add(link)));
    }

    [Fact]
    public async Task Link_UsedAndSupersededTogether_IsRefused()
    {
        (Customer customer, _) = await SeedAccountsAsync();
        LinkRecord link = LinkRecord.ForCustomer(LinkType.PasswordReset, customer.Id, Digest("l1"), Now.AddHours(1), Now);
        link.UsedAt = Now.AddMinutes(5);
        link.SupersededAt = Now.AddMinutes(5);

        Assert.Equal((CheckViolation, "ck_link_records_at_most_one_of_used_and_superseded"), await ViolationOfAsync(c => c.LinkRecords.Add(link)));
    }

    [Fact]
    public async Task Link_UsedOrSupersededAlone_IsAccepted()
    {
        (Customer customer, _) = await SeedAccountsAsync();
        LinkRecord used = LinkRecord.ForCustomer(LinkType.PasswordReset, customer.Id, Digest("l1"), Now.AddHours(1), Now);
        used.UsedAt = Now.AddMinutes(5);
        LinkRecord superseded = LinkRecord.ForCustomer(LinkType.PasswordReset, customer.Id, Digest("l2"), Now.AddHours(1), Now);
        superseded.SupersededAt = Now.AddMinutes(5);

        await SaveAsync(c => c.LinkRecords.AddRange(used, superseded));

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM link_records WHERE num_nonnulls(used_at, superseded_at) = 1"));
    }

    [Fact]
    public async Task PreviousEmail_OnATypeOtherThanTheRevert_IsRefused()
    {
        (Customer customer, _) = await SeedAccountsAsync();
        LinkRecord link = LinkRecord.ForCustomer(LinkType.NewEmailVerification, customer.Id, Digest("l1"), Now.AddHours(1), Now);
        link.PreviousEmail = "eski@example.com";

        Assert.Equal((CheckViolation, "ck_link_records_previous_email_only_for_revert_type"), await ViolationOfAsync(c => c.LinkRecords.Add(link)));
    }

    [Fact]
    public async Task PreviousEmail_OnTheRevertType_IsAccepted()
    {
        (Customer customer, _) = await SeedAccountsAsync();
        LinkRecord link = LinkRecord.ForCustomer(LinkType.EmailChangeRevert, customer.Id, Digest("l1"), Now.AddHours(1), Now);
        link.PreviousEmail = "eski@example.com";

        await SaveAsync(c => c.LinkRecords.Add(link));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM link_records WHERE previous_email = 'eski@example.com'"));
    }

    [Fact]
    public async Task SecondLink_WithTheSameTokenDigest_IsRefused()
    {
        (Customer customer, Admin admin) = await SeedAccountsAsync();
        await SaveAsync(c => c.LinkRecords.Add(LinkRecord.ForCustomer(LinkType.PasswordReset, customer.Id, Digest("same"), Now.AddHours(1), Now)));

        Assert.Equal(
            (UniqueViolation, "ix_link_records_token_hash"),
            await ViolationOfAsync(c => c.LinkRecords.Add(LinkRecord.ForAdmin(LinkType.PasswordReset, admin.Id, Digest("same"), Now.AddHours(1), Now))));
    }

    // ---- KK5: the attempt record's axis ----

    [Theory]
    [InlineData(AttemptLimit.L1, AttemptAxis.Email)]
    [InlineData(AttemptLimit.L1, AttemptAxis.Ip)]
    [InlineData(AttemptLimit.L1, AttemptAxis.RecognizedBrowser)]
    [InlineData(AttemptLimit.L2, AttemptAxis.Email)]
    [InlineData(AttemptLimit.L2, AttemptAxis.Ip)]
    [InlineData(AttemptLimit.L3, AttemptAxis.Email)]
    [InlineData(AttemptLimit.L3, AttemptAxis.Ip)]
    [InlineData(AttemptLimit.L4, AttemptAxis.Ip)]
    [InlineData(AttemptLimit.L5, AttemptAxis.Email)]
    [InlineData(AttemptLimit.L5, AttemptAxis.Ip)]
    [InlineData(AttemptLimit.L6, AttemptAxis.Cart)]
    [InlineData(AttemptLimit.L9, AttemptAxis.Email)]
    [InlineData(AttemptLimit.L9, AttemptAxis.Ip)]
    public async Task Attempt_OnTheLimitsAxis_IsAccepted(AttemptLimit limit, AttemptAxis axis)
    {
        await SaveAsync(c => c.AttemptRecords.Add(AttemptRecord.Create(limit, new AttemptSubject(axis, "value"), Now)));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM attempt_records"));
    }

    [Theory]
    [InlineData(AttemptLimit.L1, AttemptAxis.Cart)]
    [InlineData(AttemptLimit.L2, AttemptAxis.Cart)]
    [InlineData(AttemptLimit.L2, AttemptAxis.RecognizedBrowser)]
    [InlineData(AttemptLimit.L3, AttemptAxis.RecognizedBrowser)]
    [InlineData(AttemptLimit.L4, AttemptAxis.Email)]
    [InlineData(AttemptLimit.L4, AttemptAxis.Cart)]
    [InlineData(AttemptLimit.L4, AttemptAxis.RecognizedBrowser)]
    [InlineData(AttemptLimit.L5, AttemptAxis.Cart)]
    [InlineData(AttemptLimit.L5, AttemptAxis.RecognizedBrowser)]
    [InlineData(AttemptLimit.L6, AttemptAxis.Email)]
    [InlineData(AttemptLimit.L6, AttemptAxis.Ip)]
    [InlineData(AttemptLimit.L6, AttemptAxis.RecognizedBrowser)]
    [InlineData(AttemptLimit.L9, AttemptAxis.Cart)]
    [InlineData(AttemptLimit.L9, AttemptAxis.RecognizedBrowser)]
    public async Task Attempt_OffTheLimitsAxis_IsRefused(AttemptLimit limit, AttemptAxis axis)
    {
        AttemptRecord attempt = AttemptRecord.Create(limit, new AttemptSubject(axis, "value"), Now);

        Assert.Equal((CheckViolation, "ck_attempt_records_axis_is_the_limits_axis"), await ViolationOfAsync(c => c.AttemptRecords.Add(attempt)));
    }

    [Theory]
    [InlineData(AttemptLimit.L7)]
    [InlineData(AttemptLimit.L8)]
    public async Task Attempt_OfALimitCountedFromTheOrders_IsRefused(AttemptLimit limit)
    {
        // L7 and L8 are counted from the orders and never written here (06 4.43).
        AttemptRecord attempt = AttemptRecord.Create(limit, new AttemptSubject(AttemptAxis.Ip, "value"), Now);

        Assert.Equal((CheckViolation, "ck_attempt_records_attempt_limit"), await ViolationOfAsync(c => c.AttemptRecords.Add(attempt)));
    }

    // ---- KK6: the sign-in record is only appended ----

    [Fact]
    public async Task LoginRecords_AreAppended_SuccessfulAndFailedAlike()
    {
        await SaveAsync(c =>
        {
            c.LoginRecords.Add(LoginRecord.Create(IPAddress.Parse("203.0.113.5"), AccountType.Customer, "ali@example.com", LoginOutcome.Succeeded, Now));
            c.LoginRecords.Add(LoginRecord.Create(IPAddress.Parse("2001:db8::1"), AccountType.Admin, "veli@example.com", LoginOutcome.Failed, Now));
        });

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM login_records"));
        Assert.Equal("203.0.113.5/32", await ScalarAsync<string>("SELECT text(ip_address) FROM login_records WHERE account_type = 'Customer'"));
    }

    [Fact]
    public void LoginRecord_HasNoPublicSetter()
    {
        PropertyInfo[] writable = [.. typeof(LoginRecord).GetProperties().Where(p => p.SetMethod is { IsPublic: true })];

        Assert.Empty(writable);
    }

    [Fact]
    public async Task UpdatingALoginRecordThroughTheContext_IsRefused_AndTheRowStays()
    {
        LoginRecord row = LoginRecord.Create(IPAddress.Parse("203.0.113.5"), AccountType.Customer, "ali@example.com", LoginOutcome.Failed, Now);
        await SaveAsync(c => c.LoginRecords.Add(row));

        await using (AppDbContext context = NewContext())
        {
            LoginRecord loaded = await context.LoginRecords.SingleAsync(l => l.Id == row.Id, Ct);
            context.Entry(loaded).Property(l => l.Outcome).CurrentValue = LoginOutcome.Succeeded;
            InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));
            Assert.Contains("never updated", error.Message, StringComparison.Ordinal);
        }

        Assert.Equal("Failed", await ScalarAsync<string>($"SELECT outcome FROM login_records WHERE id = '{row.Id}'"));
    }

    // ---- KK7: deletion ----

    [Fact]
    public async Task DeletingTheCustomer_TakesItsSessionsMarksAndLinks_AndLeavesTheAttemptAndLoginRows()
    {
        (Customer customer, Admin admin) = await SeedAccountsAsync();
        await SaveAsync(c =>
        {
            c.Sessions.Add(Session.ForCustomer(customer.Id, Digest("s1"), false, Now));
            c.Sessions.Add(Session.ForAdmin(admin.Id, Digest("s2"), false, Now));
            c.RecognizedBrowserMarks.Add(RecognizedBrowserMark.ForCustomer(customer.Id, Digest("m1"), Now.AddDays(30), Now));
            c.RecognizedBrowserMarks.Add(RecognizedBrowserMark.ForAdmin(admin.Id, Digest("m2"), Now.AddDays(30), Now));
            c.LinkRecords.Add(LinkRecord.ForCustomer(LinkType.PasswordReset, customer.Id, Digest("l1"), Now.AddHours(1), Now));
            c.LinkRecords.Add(LinkRecord.ForAdmin(LinkType.PasswordReset, admin.Id, Digest("l2"), Now.AddHours(1), Now));
            c.AttemptRecords.Add(AttemptRecord.Create(AttemptLimit.L1, AttemptSubject.Email(customer.NormalizedEmail), Now));
            c.LoginRecords.Add(LoginRecord.Create(IPAddress.Parse("203.0.113.5"), AccountType.Customer, customer.NormalizedEmail, LoginOutcome.Succeeded, Now));
        });

        await ExecuteAsync($"DELETE FROM customers WHERE id = '{customer.Id}'");

        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM sessions WHERE customer_id = '{customer.Id}'"));
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM recognized_browser_marks WHERE customer_id = '{customer.Id}'"));
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM link_records WHERE customer_id = '{customer.Id}'"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM sessions"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM recognized_browser_marks"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM link_records"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM attempt_records"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM login_records"));
    }

    [Fact]
    public async Task DeletingTheAdmin_TakesItsSessionsMarksAndLinks_AndLeavesTheAttemptAndLoginRows()
    {
        (Customer customer, Admin admin) = await SeedAccountsAsync();
        await SaveAsync(c =>
        {
            c.Sessions.Add(Session.ForCustomer(customer.Id, Digest("s1"), false, Now));
            c.Sessions.Add(Session.ForAdmin(admin.Id, Digest("s2"), false, Now));
            c.RecognizedBrowserMarks.Add(RecognizedBrowserMark.ForCustomer(customer.Id, Digest("m1"), Now.AddDays(30), Now));
            c.RecognizedBrowserMarks.Add(RecognizedBrowserMark.ForAdmin(admin.Id, Digest("m2"), Now.AddDays(30), Now));
            c.LinkRecords.Add(LinkRecord.ForCustomer(LinkType.PasswordReset, customer.Id, Digest("l1"), Now.AddHours(1), Now));
            c.LinkRecords.Add(LinkRecord.ForAdmin(LinkType.PasswordReset, admin.Id, Digest("l2"), Now.AddHours(1), Now));
            c.AttemptRecords.Add(AttemptRecord.Create(AttemptLimit.L1, AttemptSubject.Email(admin.NormalizedEmail), Now));
            c.LoginRecords.Add(LoginRecord.Create(IPAddress.Parse("203.0.113.6"), AccountType.Admin, admin.NormalizedEmail, LoginOutcome.Failed, Now));
        });

        await ExecuteAsync($"DELETE FROM admins WHERE id = '{admin.Id}'");

        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM sessions WHERE admin_id = '{admin.Id}'"));
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM recognized_browser_marks WHERE admin_id = '{admin.Id}'"));
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM link_records WHERE admin_id = '{admin.Id}'"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM sessions"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM recognized_browser_marks"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM link_records"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM attempt_records"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM login_records"));
    }

    [Fact]
    public async Task DeletingTheInvitation_TakesItsLinks_AndLeavesTheOthers()
    {
        (Customer customer, _) = await SeedAccountsAsync();
        AdminInvitation invitation = await SeedInvitationAsync();
        await SaveAsync(c =>
        {
            c.LinkRecords.Add(LinkRecord.ForInvitation(invitation.Id, Digest("l1"), Now.AddHours(1), Now));
            c.LinkRecords.Add(LinkRecord.ForCustomer(LinkType.PasswordReset, customer.Id, Digest("l2"), Now.AddHours(1), Now));
        });

        await ExecuteAsync($"DELETE FROM admin_invitations WHERE id = '{invitation.Id}'");

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM link_records WHERE link_type = 'AdminInvitation'"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM link_records"));
    }

    [Fact]
    public async Task Session_OfAnUnknownAccount_IsRefused()
    {
        Session session = Session.ForCustomer(EntityIds.New(), Digest("s1"), false, Now);

        Assert.Equal((PostgresErrorCodes.ForeignKeyViolation, "fk_sessions_customers_customer_id"), await ViolationOfAsync(c => c.Sessions.Add(session)));
    }

    // ---- KK8: one topic, no Down ----

    [Fact]
    public void Migration_HasNoDownMethod()
    {
        MethodInfo? down = typeof(CreateSessionLinkAttemptAndLoginTables)
            .GetMethod("Down", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);

        Assert.Null(down);
    }

    // ---- helpers ----

    /// <summary>A placeholder digest; the tests never hold a real key or a real digest of one.</summary>
    private static string Digest(string tag) => $"digest-placeholder-{tag}";

    private async Task<(Customer Customer, Admin Admin)> SeedAccountsAsync()
    {
        Customer customer = Customer.Create("Ali", "ali@example.com", "digest-placeholder", Now, null, Now);
        Admin admin = Admin.Create("Veli", "veli@example.com", "digest-placeholder", Now);
        await SaveAsync(c =>
        {
            c.Customers.Add(customer);
            c.Admins.Add(admin);
        });
        return (customer, admin);
    }

    private async Task<AdminInvitation> SeedInvitationAsync()
    {
        AdminInvitation invitation = AdminInvitation.Create("davet@example.com", null, "Veli", Now);
        await SaveAsync(c => c.AdminInvitations.Add(invitation));
        return invitation;
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
