using System.Globalization;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the content record and brand tests (T114): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(ContentRecordAndBrandingTablesDefinition.Name)]
public sealed class ContentRecordAndBrandingTablesDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "ContentRecordAndBrandingTables";
}

/// <summary>
/// The content record and brand tables on a real PostgreSQL (T114 criteria 1-9; 06 4.46-4.49, 4.51, 4.52, 4.54, 4.55, 4.72, 2.3.2, 2.9.2,
/// 2.10.1; 09 10.6): the migration made the eight tables; the database itself refuses a second row of the three one-row tables, a third
/// publish status, an announcement that ends before it starts, a second link of a platform, a repeated or malformed page address and a
/// short brand colour; the manual order repeats freely; the long text columns are text; the brand's logo and icon links are restrictive;
/// the context refuses the deletion of the about page and the brand row.
/// </summary>
[Collection(ContentRecordAndBrandingTablesDefinition.Name)]
public sealed class ContentRecordAndBrandingTablesTests : IAsyncLifetime
{
    private const string CheckViolation = PostgresErrorCodes.CheckViolation;
    private const string UniqueViolation = PostgresErrorCodes.UniqueViolation;
    private const string ForeignKeyViolation = PostgresErrorCodes.ForeignKeyViolation;

    private static readonly string[] EightTables =
        ["about_pages", "announcements", "brandings", "custom_pages", "faq_items", "offerings", "portfolio_items", "social_links"];

    private static readonly string[] SingleRowTableNames = ["about_pages", "announcements", "brandings"];

    private static readonly DateTimeOffset Now = new(2026, 11, 4, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    public ContentRecordAndBrandingTablesTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    /// <summary>One insert statement per table that carries a publish status; the status is the parameter <c>@status</c>.</summary>
    public static TheoryData<string, string> StatusInserts => new()
    {
        { "about_pages", "INSERT INTO about_pages (id, created_at, publish_status) VALUES (gen_random_uuid(), now(), @status)" },
        { "announcements", "INSERT INTO announcements (id, created_at, publish_status) VALUES (gen_random_uuid(), now(), @status)" },
        { "offerings", "INSERT INTO offerings (id, created_at, name, sort_order, slug, publish_status) VALUES (gen_random_uuid(), now(), 'a', 1, 'a', @status)" },
        { "portfolio_items", "INSERT INTO portfolio_items (id, created_at, title, sort_order, slug, publish_status) VALUES (gen_random_uuid(), now(), 'a', 1, 'a', @status)" },
        { "faq_items", "INSERT INTO faq_items (id, created_at, question, sort_order, publish_status) VALUES (gen_random_uuid(), now(), 'a', 1, @status)" },
        { "custom_pages", "INSERT INTO custom_pages (id, created_at, title, sort_order, slug, publish_status) VALUES (gen_random_uuid(), now(), 'a', 1, 'a', @status)" },
    };

    /// <summary>The three tables with a page address and the name of their title column.</summary>
    public static TheoryData<string, string> SlugTables => new()
    {
        { "offerings", "name" },
        { "portfolio_items", "title" },
        { "custom_pages", "title" },
    };

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE about_pages, announcements, brandings, custom_pages, faq_items, offerings, portfolio_items, social_links, images CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the migration ----

    [Fact]
    public async Task Migration_MakesTheEightTables()
    {
        List<string> tables = await QueryAsync(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name = ANY(@names)",
            ("names", EightTables));

        Assert.Equal(EightTables, tables.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Columns_HaveTheTypesAndNullabilityOfTheFieldTables()
    {
        // 06 4.46-4.49, 4.51, 4.52, 4.54, 4.55 (the common Id and CreatedAt included); the lengths are P-27..P-30 of 06 11.2; the long text
        // columns are text (06 2.10.1); the brand name and every address are text (06 2.5.1).
        string[] expected =
        [
            "about_pages.created_at timestamp with time zone not null",
            "about_pages.id uuid not null",
            "about_pages.long_text text",
            "about_pages.publish_status text not null",
            "about_pages.short_introduction character varying(400)",
            "about_pages.version integer not null",
            "announcements.created_at timestamp with time zone not null",
            "announcements.end_date date",
            "announcements.id uuid not null",
            "announcements.link_url text",
            "announcements.publish_status text not null",
            "announcements.start_date date",
            "announcements.text character varying(120)",
            "announcements.version integer not null",
            "brandings.brand_color text",
            "brandings.brand_name text",
            "brandings.created_at timestamp with time zone not null",
            "brandings.favicon_id uuid",
            "brandings.id uuid not null",
            "brandings.is_platform_signature_enabled boolean not null",
            "brandings.logo_id uuid",
            "brandings.version integer not null",
            "custom_pages.created_at timestamp with time zone not null",
            "custom_pages.id uuid not null",
            "custom_pages.is_shown_in_menu boolean not null",
            "custom_pages.publish_status text not null",
            "custom_pages.slug text not null",
            "custom_pages.slug_fixed_at timestamp with time zone",
            "custom_pages.sort_order integer not null",
            "custom_pages.text text",
            "custom_pages.title character varying(150) not null",
            "custom_pages.version integer not null",
            "faq_items.answer text",
            "faq_items.created_at timestamp with time zone not null",
            "faq_items.id uuid not null",
            "faq_items.publish_status text not null",
            "faq_items.question character varying(200) not null",
            "faq_items.sort_order integer not null",
            "faq_items.version integer not null",
            "offerings.created_at timestamp with time zone not null",
            "offerings.id uuid not null",
            "offerings.is_shown_on_homepage boolean not null",
            "offerings.name character varying(150) not null",
            "offerings.publish_status text not null",
            "offerings.short_description character varying(200)",
            "offerings.slug text not null",
            "offerings.slug_fixed_at timestamp with time zone",
            "offerings.sort_order integer not null",
            "offerings.text text",
            "offerings.version integer not null",
            "portfolio_items.created_at timestamp with time zone not null",
            "portfolio_items.id uuid not null",
            "portfolio_items.is_shown_on_homepage boolean not null",
            "portfolio_items.publish_status text not null",
            "portfolio_items.short_description character varying(200)",
            "portfolio_items.slug text not null",
            "portfolio_items.slug_fixed_at timestamp with time zone",
            "portfolio_items.sort_order integer not null",
            "portfolio_items.text text",
            "portfolio_items.title character varying(150) not null",
            "portfolio_items.version integer not null",
            "social_links.created_at timestamp with time zone not null",
            "social_links.id uuid not null",
            "social_links.social_platform text not null",
            "social_links.url text not null",
        ];

        List<string> columns = await QueryAsync(
            "SELECT table_name || '.' || column_name || ' ' || data_type"
            + " || CASE WHEN character_maximum_length IS NOT NULL THEN '(' || character_maximum_length || ')' ELSE '' END"
            + " || CASE WHEN is_nullable = 'NO' THEN ' not null' ELSE '' END"
            + " FROM information_schema.columns WHERE table_schema = 'public' AND table_name = ANY(@names)",
            ("names", EightTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), columns.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Columns_HaveTheDatabaseDefaultsOfTheDefaultColumn()
    {
        // The "Varsayılan" column of 06 4.46-4.54 is a database default, not only a default of the entity: the publish status is Draft, the
        // version 1, the two display switches false and the platform signature true. No other column has a default.
        string[] expected =
        [
            "about_pages.publish_status 'Draft'::text",
            "about_pages.version 1",
            "announcements.publish_status 'Draft'::text",
            "announcements.version 1",
            "brandings.is_platform_signature_enabled true",
            "brandings.version 1",
            "custom_pages.is_shown_in_menu false",
            "custom_pages.publish_status 'Draft'::text",
            "custom_pages.version 1",
            "faq_items.publish_status 'Draft'::text",
            "faq_items.version 1",
            "offerings.is_shown_on_homepage false",
            "offerings.publish_status 'Draft'::text",
            "offerings.version 1",
            "portfolio_items.is_shown_on_homepage false",
            "portfolio_items.publish_status 'Draft'::text",
            "portfolio_items.version 1",
        ];

        List<string> defaults = await QueryAsync(
            "SELECT table_name || '.' || column_name || ' ' || column_default FROM information_schema.columns"
            + " WHERE table_schema = 'public' AND table_name = ANY(@names) AND column_default IS NOT NULL",
            ("names", EightTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), defaults.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Indexes_AreTheDocumentedOnes()
    {
        // 06 4.46-4.55 and 4.72.5: the single-row unique indexes of the three one-row tables (no foreign-key index on the brand: one row,
        // 06 7.5.2), the unique page addresses, the status-and-order list indexes, the filtered homepage and menu indexes and the unique platform.
        string[] expected =
        [
            "about_pages.ix_about_pages_single_row | CREATE UNIQUE INDEX ix_about_pages_single_row ON public.about_pages USING btree ((true))",
            "about_pages.pk_about_pages | CREATE UNIQUE INDEX pk_about_pages ON public.about_pages USING btree (id)",
            "announcements.ix_announcements_single_row | CREATE UNIQUE INDEX ix_announcements_single_row ON public.announcements USING btree ((true))",
            "announcements.pk_announcements | CREATE UNIQUE INDEX pk_announcements ON public.announcements USING btree (id)",
            "brandings.ix_brandings_single_row | CREATE UNIQUE INDEX ix_brandings_single_row ON public.brandings USING btree ((true))",
            "brandings.pk_brandings | CREATE UNIQUE INDEX pk_brandings ON public.brandings USING btree (id)",
            "custom_pages.ix_custom_pages_is_shown_in_menu_sort_order | CREATE INDEX ix_custom_pages_is_shown_in_menu_sort_order ON public.custom_pages USING btree (is_shown_in_menu, sort_order) WHERE (is_shown_in_menu AND (publish_status = 'Published'::text))",
            "custom_pages.ix_custom_pages_publish_status_sort_order | CREATE INDEX ix_custom_pages_publish_status_sort_order ON public.custom_pages USING btree (publish_status, sort_order)",
            "custom_pages.ix_custom_pages_slug | CREATE UNIQUE INDEX ix_custom_pages_slug ON public.custom_pages USING btree (slug)",
            "custom_pages.pk_custom_pages | CREATE UNIQUE INDEX pk_custom_pages ON public.custom_pages USING btree (id)",
            "faq_items.ix_faq_items_publish_status_sort_order | CREATE INDEX ix_faq_items_publish_status_sort_order ON public.faq_items USING btree (publish_status, sort_order)",
            "faq_items.pk_faq_items | CREATE UNIQUE INDEX pk_faq_items ON public.faq_items USING btree (id)",
            "offerings.ix_offerings_is_shown_on_homepage_sort_order | CREATE INDEX ix_offerings_is_shown_on_homepage_sort_order ON public.offerings USING btree (is_shown_on_homepage, sort_order) WHERE (is_shown_on_homepage AND (publish_status = 'Published'::text))",
            "offerings.ix_offerings_publish_status_sort_order | CREATE INDEX ix_offerings_publish_status_sort_order ON public.offerings USING btree (publish_status, sort_order)",
            "offerings.ix_offerings_slug | CREATE UNIQUE INDEX ix_offerings_slug ON public.offerings USING btree (slug)",
            "offerings.pk_offerings | CREATE UNIQUE INDEX pk_offerings ON public.offerings USING btree (id)",
            "portfolio_items.ix_portfolio_items_is_shown_on_homepage_sort_order | CREATE INDEX ix_portfolio_items_is_shown_on_homepage_sort_order ON public.portfolio_items USING btree (is_shown_on_homepage, sort_order) WHERE (is_shown_on_homepage AND (publish_status = 'Published'::text))",
            "portfolio_items.ix_portfolio_items_publish_status_sort_order | CREATE INDEX ix_portfolio_items_publish_status_sort_order ON public.portfolio_items USING btree (publish_status, sort_order)",
            "portfolio_items.ix_portfolio_items_slug | CREATE UNIQUE INDEX ix_portfolio_items_slug ON public.portfolio_items USING btree (slug)",
            "portfolio_items.pk_portfolio_items | CREATE UNIQUE INDEX pk_portfolio_items ON public.portfolio_items USING btree (id)",
            "social_links.ix_social_links_social_platform | CREATE UNIQUE INDEX ix_social_links_social_platform ON public.social_links USING btree (social_platform)",
            "social_links.pk_social_links | CREATE UNIQUE INDEX pk_social_links ON public.social_links USING btree (id)",
        ];

        List<string> indexes = await QueryAsync(
            "SELECT tablename || '.' || indexname || ' | ' || indexdef FROM pg_indexes WHERE schemaname = 'public' AND tablename = ANY(@names)",
            ("names", EightTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), indexes.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ForeignKeys_AreTheTwoRestrictiveImageLinksOfTheBrand()
    {
        // 06 4.54 and 7.2: the logo and the icon link to Image restrictively (a); no other table of the eight links anywhere.
        string[] expected =
        [
            "brandings fk_brandings_images_favicon_id a",
            "brandings fk_brandings_images_logo_id a",
        ];

        List<string> links = await QueryAsync(
            "SELECT conrelid::regclass::text || ' ' || conname || ' ' || confdeltype::text FROM pg_constraint WHERE contype = 'f' AND conrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x))",
            ("oids", EightTables.Select(t => "public." + t).ToArray()));

        Assert.Equal(expected.Order(StringComparer.Ordinal), links.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task CheckConstraints_AreTheDocumentedOnes()
    {
        // 06 2.9.2 (the two-valued status and the platform list), 4.52 (the dates), 4.54 (the colour), 4.72.4 (the address).
        const string StatusList = "ARRAY['Draft'::text, 'Published'::text]";
        string[] expected =
        [
            $"about_pages.ck_about_pages_publish_status | CHECK ((publish_status = ANY ({StatusList})))",
            "announcements.ck_announcements_end_not_before_start | CHECK (((start_date IS NULL) OR (end_date IS NULL) OR (end_date >= start_date)))",
            $"announcements.ck_announcements_publish_status | CHECK ((publish_status = ANY ({StatusList})))",
            "brandings.ck_brandings_brand_color_format | CHECK ((brand_color ~ '^#[0-9A-F]{6}$'::text))",
            $"custom_pages.ck_custom_pages_publish_status | CHECK ((publish_status = ANY ({StatusList})))",
            "custom_pages.ck_custom_pages_slug_format | CHECK ((slug ~ '^[a-z0-9]+(-[a-z0-9]+)*$'::text))",
            $"faq_items.ck_faq_items_publish_status | CHECK ((publish_status = ANY ({StatusList})))",
            $"offerings.ck_offerings_publish_status | CHECK ((publish_status = ANY ({StatusList})))",
            "offerings.ck_offerings_slug_format | CHECK ((slug ~ '^[a-z0-9]+(-[a-z0-9]+)*$'::text))",
            $"portfolio_items.ck_portfolio_items_publish_status | CHECK ((publish_status = ANY ({StatusList})))",
            "portfolio_items.ck_portfolio_items_slug_format | CHECK ((slug ~ '^[a-z0-9]+(-[a-z0-9]+)*$'::text))",
            "social_links.ck_social_links_social_platform | CHECK ((social_platform = ANY (ARRAY['Instagram'::text, 'Facebook'::text, 'X'::text, 'LinkedIn'::text, 'YouTube'::text, 'TikTok'::text])))",
        ];

        List<string> checks = await QueryAsync(
            "SELECT c.conrelid::regclass::text || '.' || c.conname || ' | ' || pg_get_constraintdef(c.oid) FROM pg_constraint c"
            + " WHERE c.contype = 'c' AND c.conrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x))",
            ("oids", EightTables.Select(t => "public." + t).ToArray()));

        Assert.Equal(expected.Order(StringComparer.Ordinal), checks.Order(StringComparer.Ordinal));
    }

    // ---- KK2: the three one-row tables ----

    [Fact]
    public async Task SecondAboutPageRow_IsRefusedByTheSingleRowIndex()
    {
        await SaveAsync(c => c.AboutPages.Add(AboutPage.Create(Now)));

        Assert.Equal((UniqueViolation, "ix_about_pages_single_row"), await ViolationOfAsync(c => c.AboutPages.Add(AboutPage.Create(Now))));
    }

    [Fact]
    public async Task SecondAnnouncementRow_IsRefusedByTheSingleRowIndex()
    {
        await SaveAsync(c => c.Announcements.Add(Announcement.Create(Now)));

        Assert.Equal((UniqueViolation, "ix_announcements_single_row"), await ViolationOfAsync(c => c.Announcements.Add(Announcement.Create(Now))));
    }

    [Fact]
    public async Task SecondBrandingRow_IsRefusedByTheSingleRowIndex()
    {
        await SaveAsync(c => c.Brandings.Add(Branding.Create(Now)));

        Assert.Equal((UniqueViolation, "ix_brandings_single_row"), await ViolationOfAsync(c => c.Brandings.Add(Branding.Create(Now))));
    }

    [Fact]
    public async Task SingleRowTables_CarryOnlyTheSingleRowIndexAndTheKey()
    {
        List<string> indexes = await QueryAsync(
            "SELECT indexname FROM pg_indexes WHERE schemaname = 'public' AND tablename = ANY(@names) ORDER BY 1",
            ("names", SingleRowTableNames));

        Assert.Equal(
            ["ix_about_pages_single_row", "ix_announcements_single_row", "ix_brandings_single_row", "pk_about_pages", "pk_announcements", "pk_brandings"],
            indexes);
    }

    [Fact]
    public async Task AnnouncementDeleted_LetsANewRowBeOpened()
    {
        // 06 4.52: the announcement may be deleted; a new row may then be opened.
        Announcement first = Announcement.Create(Now);
        await SaveAsync(c => c.Announcements.Add(first));

        await SaveAsync(c =>
        {
            c.Announcements.Attach(first);
            c.Announcements.Remove(first);
        });
        await SaveAsync(c => c.Announcements.Add(Announcement.Create(Now)));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM announcements"));
    }

    // ---- KK3: the two-valued publish status ----

    [Theory]
    [MemberData(nameof(StatusInserts))]
    public async Task PublishStatus_DraftAndPublished_AreAccepted(string table, string insert)
    {
        await ExecuteAsync(insert, ("status", "Draft"));
        await ExecuteAsync($"TRUNCATE {table} CASCADE");
        await ExecuteAsync(insert, ("status", "Published"));

        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM {table}"));
    }

    [Theory]
    [MemberData(nameof(StatusInserts))]
    public async Task PublishStatus_ThirdValueOfTheEnum_IsRefused(string table, string insert)
    {
        // Archived is a value of PublishStatus (06 5.2.3) but not of the content records (06 2.9.2).
        Assert.Equal((CheckViolation, $"ck_{table}_publish_status"), await ViolationOfSqlAsync(insert, ("status", "Archived")));
    }

    [Theory]
    [MemberData(nameof(StatusInserts))]
    public async Task PublishStatus_UnknownText_IsRefused(string table, string insert)
    {
        Assert.Equal((CheckViolation, $"ck_{table}_publish_status"), await ViolationOfSqlAsync(insert, ("status", "draft")));
    }

    [Fact]
    public async Task PublishStatus_OfARowWithoutIt_IsDraftAndTheVersionOne()
    {
        // The database default (06 4.46...): an insert that names neither column gets Draft and 1.
        await ExecuteAsync("INSERT INTO faq_items (id, created_at, question, sort_order) VALUES (gen_random_uuid(), now(), 'Soru?', 1)");

        Assert.Equal("Draft|1", await ScalarAsync<string>("SELECT publish_status || '|' || version FROM faq_items"));
    }

    [Fact]
    public async Task Entities_RoundTripTheirStatusAndSwitches()
    {
        // The entity default is Draft; a published row and a false platform signature are written as they are (a database default of true
        // must not swallow an explicit false).
        Offering offering = Offering.Create("Hizmet", "hizmet", 1, Now);
        offering.PublishStatus = PublishStatus.Published;
        offering.IsShownOnHomepage = true;
        Branding branding = Branding.Create(Now);
        branding.IsPlatformSignatureEnabled = false;
        await SaveAsync(c =>
        {
            c.Offerings.Add(offering);
            c.Brandings.Add(branding);
        });

        Assert.Equal("Published|true", await ScalarAsync<string>("SELECT publish_status || '|' || is_shown_on_homepage FROM offerings"));
        Assert.False(await ScalarAsync<bool>("SELECT is_platform_signature_enabled FROM brandings"));
    }

    [Fact]
    public async Task BrandingCreated_HasThePlatformSignatureOn()
    {
        await SaveAsync(c => c.Brandings.Add(Branding.Create(Now)));

        Assert.True(await ScalarAsync<bool>("SELECT is_platform_signature_enabled FROM brandings"));
    }

    // ---- KK4: the announcement dates ----

    [Fact]
    public async Task Announcement_EndBeforeStart_IsRefused()
    {
        Announcement announcement = Announcement.Create(Now);
        announcement.StartDate = new DateOnly(2026, 11, 10);
        announcement.EndDate = new DateOnly(2026, 11, 9);

        Assert.Equal((CheckViolation, "ck_announcements_end_not_before_start"), await ViolationOfAsync(c => c.Announcements.Add(announcement)));
    }

    [Theory]
    [InlineData("2026-11-10", "2026-11-10")]
    [InlineData("2026-11-10", "2026-11-11")]
    [InlineData("2026-11-10", null)]
    [InlineData(null, "2026-11-09")]
    [InlineData(null, null)]
    public async Task Announcement_EqualOrLaterEndOrAnOpenSide_IsAccepted(string? start, string? end)
    {
        Announcement announcement = Announcement.Create(Now);
        announcement.StartDate = start is null ? null : DateOnly.Parse(start, CultureInfo.InvariantCulture);
        announcement.EndDate = end is null ? null : DateOnly.Parse(end, CultureInfo.InvariantCulture);

        await SaveAsync(c => c.Announcements.Add(announcement));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM announcements"));
    }

    // ---- KK5: the social platform and the page addresses ----

    [Fact]
    public async Task SocialLink_SecondLinkOfThePlatform_IsRefused()
    {
        await SaveAsync(c => c.SocialLinks.Add(SocialLink.Create(SocialPlatform.Instagram, "https://instagram.com/a", Now)));

        Assert.Equal(
            (UniqueViolation, "ix_social_links_social_platform"),
            await ViolationOfAsync(c => c.SocialLinks.Add(SocialLink.Create(SocialPlatform.Instagram, "https://instagram.com/b", Now))));
    }

    [Fact]
    public async Task SocialLink_EveryPlatformOnce_IsAccepted()
    {
        await SaveAsync(c =>
        {
            foreach (SocialPlatform platform in Enum.GetValues<SocialPlatform>())
            {
                c.SocialLinks.Add(SocialLink.Create(platform, "https://example.test/" + platform, Now));
            }
        });

        Assert.Equal((long)Enum.GetValues<SocialPlatform>().Length, await ScalarAsync<long>("SELECT count(*) FROM social_links"));
    }

    [Fact]
    public async Task Offering_SecondOfferingWithTheSameSlug_IsRefused()
    {
        await SaveAsync(c => c.Offerings.Add(Offering.Create("Çelik kapı", "celik-kapi", 1, Now)));

        Assert.Equal(
            (UniqueViolation, "ix_offerings_slug"),
            await ViolationOfAsync(c => c.Offerings.Add(Offering.Create("Çelik kapı 2", "celik-kapi", 2, Now))));
    }

    [Fact]
    public async Task PortfolioItem_SecondItemWithTheSameSlug_IsRefused()
    {
        await SaveAsync(c => c.PortfolioItems.Add(PortfolioItem.Create("İş", "is", 1, Now)));

        Assert.Equal(
            (UniqueViolation, "ix_portfolio_items_slug"),
            await ViolationOfAsync(c => c.PortfolioItems.Add(PortfolioItem.Create("İş 2", "is", 2, Now))));
    }

    [Fact]
    public async Task CustomPage_SecondPageWithTheSameSlug_IsRefused()
    {
        await SaveAsync(c => c.CustomPages.Add(CustomPage.Create("Sayfa", "sayfa", 1, Now)));

        Assert.Equal(
            (UniqueViolation, "ix_custom_pages_slug"),
            await ViolationOfAsync(c => c.CustomPages.Add(CustomPage.Create("Sayfa 2", "sayfa", 2, Now))));
    }

    [Fact]
    public async Task Slug_OfADraftRow_IsStillTaken()
    {
        // 06 4.72.1: the index has no filter; a draft row's address counts as taken.
        Offering draft = Offering.Create("Taslak", "taslak", 1, Now);
        Assert.Equal(PublishStatus.Draft, draft.PublishStatus);
        await SaveAsync(c => c.Offerings.Add(draft));

        Assert.Equal((UniqueViolation, "ix_offerings_slug"), await ViolationOfAsync(c => c.Offerings.Add(Offering.Create("Başka", "taslak", 2, Now))));
    }

    [Fact]
    public async Task Slug_SameValueInTwoTables_IsAccepted()
    {
        // 06 4.72.1: uniqueness is within a record type; the prefix separates the types.
        await SaveAsync(c =>
        {
            c.Offerings.Add(Offering.Create("A", "ortak", 1, Now));
            c.PortfolioItems.Add(PortfolioItem.Create("A", "ortak", 1, Now));
            c.CustomPages.Add(CustomPage.Create("A", "ortak", 1, Now));
        });

        Assert.Equal(3L, await ScalarAsync<long>("SELECT (SELECT count(*) FROM offerings) + (SELECT count(*) FROM portfolio_items) + (SELECT count(*) FROM custom_pages)"));
    }

    [Theory]
    [MemberData(nameof(SlugTables))]
    public async Task Slug_Uppercase_IsRefused(string table, string titleColumn)
    {
        Assert.Equal((CheckViolation, $"ck_{table}_slug_format"), await ViolationOfSqlAsync(SlugInsert(table, titleColumn), ("slug", "Celik-Kapi")));
    }

    [Theory]
    [MemberData(nameof(SlugTables))]
    public async Task Slug_OtherMalformedValues_AreRefused(string table, string titleColumn)
    {
        // 06 4.72.4: lowercase ASCII letters and digits with single hyphens, not at the ends; the empty text is not an address.
        foreach (string slug in new[] { "-a", "a-", "a--b", "a b", "çelik", "a_b", "a/b", "a.b", string.Empty, "-" })
        {
            Assert.Equal((CheckViolation, $"ck_{table}_slug_format"), await ViolationOfSqlAsync(SlugInsert(table, titleColumn), ("slug", slug)));
        }
    }

    [Theory]
    [MemberData(nameof(SlugTables))]
    public async Task Slug_WellFormedValues_AreAccepted(string table, string titleColumn)
    {
        string[] slugs = ["a", "celik-kapi", "celik-kapi-2", "2026", "a-1-b"];
        foreach (string slug in slugs)
        {
            await ExecuteAsync(SlugInsert(table, titleColumn), ("slug", slug));
        }

        Assert.Equal((long)slugs.Length, await ScalarAsync<long>($"SELECT count(*) FROM {table}"));
    }

    // ---- KK6: the brand colour ----

    [Theory]
    [InlineData("#FFF")]
    [InlineData("#1a6b3c")]
    [InlineData("1A6B3C")]
    [InlineData("#1A6B3")]
    [InlineData("#1A6B3CF")]
    [InlineData("#GGGGGG")]
    [InlineData(" #1A6B3C")]
    [InlineData("#1A6B3C ")]
    [InlineData("")]
    public async Task BrandColor_OtherThanHashAndSixCapitalHexDigits_IsRefused(string color)
    {
        // The empty text is stored as NULL by the save (06 2.8.5), so the statement goes to the database directly.
        (string, string?) violation = await ViolationOfSqlAsync("INSERT INTO brandings (id, created_at, brand_color) VALUES (gen_random_uuid(), now(), @color)", ("color", color));

        Assert.Equal((CheckViolation, "ck_brandings_brand_color_format"), violation);
    }

    [Theory]
    [InlineData("#1A6B3C")]
    [InlineData("#000000")]
    [InlineData("#FFFFFF")]
    [InlineData("#ABCDEF")]
    [InlineData(null)]
    public async Task BrandColor_HashAndSixCapitalHexDigitsOrEmpty_IsAccepted(string? color)
    {
        Branding branding = Branding.Create(Now);
        branding.BrandColor = color;

        await SaveAsync(c => c.Brandings.Add(branding));

        Assert.Equal(color, await ScalarAsync<string?>("SELECT brand_color FROM brandings"));
    }

    // ---- KK7: the manual order ----

    [Fact]
    public async Task SortOrder_SameValueInTwoRows_IsAccepted()
    {
        // 06 4.47, 4.49, 4.51: the order is not unique; reordering changes rows.
        await SaveAsync(c =>
        {
            c.Offerings.Add(Offering.Create("A", "a", 1, Now));
            c.Offerings.Add(Offering.Create("B", "b", 1, Now));
            c.PortfolioItems.Add(PortfolioItem.Create("A", "a", 1, Now));
            c.PortfolioItems.Add(PortfolioItem.Create("B", "b", 1, Now));
            c.FaqItems.Add(FaqItem.Create("A?", 1, Now));
            c.FaqItems.Add(FaqItem.Create("B?", 1, Now));
            c.CustomPages.Add(CustomPage.Create("A", "a", 1, Now));
            c.CustomPages.Add(CustomPage.Create("B", "b", 1, Now));
        });

        Assert.Equal(
            8L,
            await ScalarAsync<long>(
                "SELECT (SELECT count(*) FROM offerings WHERE sort_order = 1) + (SELECT count(*) FROM portfolio_items WHERE sort_order = 1)"
                + " + (SELECT count(*) FROM faq_items WHERE sort_order = 1) + (SELECT count(*) FROM custom_pages WHERE sort_order = 1)"));
    }

    // ---- KK8: the long text and the image links ----

    [Fact]
    public async Task LongTextColumns_AreTextAndTakeALongValue()
    {
        string[] textColumns =
        [
            "about_pages.long_text",
            "offerings.text",
            "portfolio_items.text",
            "faq_items.answer",
            "custom_pages.text",
        ];

        List<string> types = await QueryAsync(
            "SELECT table_name || '.' || column_name || ' ' || data_type FROM information_schema.columns WHERE table_schema = 'public'"
            + " AND table_name || '.' || column_name = ANY(@columns)",
            ("columns", textColumns));
        Assert.Equal(textColumns.Select(c => c + " text").Order(StringComparer.Ordinal), types.Order(StringComparer.Ordinal));

        string longText = "<p>" + new string('a', 100_000) + "</p>";
        AboutPage page = AboutPage.Create(Now);
        page.SetLongText(longText);
        Offering offering = Offering.Create("Hizmet", "hizmet", 1, Now);
        offering.SetText(longText);
        await SaveAsync(c =>
        {
            c.AboutPages.Add(page);
            c.Offerings.Add(offering);
        });

        Assert.Equal(longText.Length, await ScalarAsync<int>("SELECT length(long_text) FROM about_pages"));
        Assert.Equal(longText.Length, await ScalarAsync<int>("SELECT length(text) FROM offerings"));
    }

    [Fact]
    public async Task ShortTextColumns_RefuseAValueOverTheirLength()
    {
        // 06 11.2 P-27..P-30: 150, 200, 400 and 120 characters.
        Assert.Equal(
            ("22001", null),
            await ViolationOfAsync(c => c.Offerings.Add(Offering.Create(new string('a', 151), "a", 1, Now))));
        Assert.Equal(
            ("22001", null),
            await ViolationOfAsync(c => c.FaqItems.Add(FaqItem.Create(new string('a', 201), 1, Now))));

        AboutPage page = AboutPage.Create(Now);
        page.ShortIntroduction = new string('a', 401);
        Assert.Equal(("22001", null), await ViolationOfAsync(c => c.AboutPages.Add(page)));

        Announcement announcement = Announcement.Create(Now);
        announcement.Text = new string('a', 121);
        Assert.Equal(("22001", null), await ViolationOfAsync(c => c.Announcements.Add(announcement)));
    }

    [Fact]
    public async Task ShortTextColumns_TakeAValueOfExactlyTheirLength()
    {
        AboutPage page = AboutPage.Create(Now);
        page.ShortIntroduction = new string('a', 400);
        Announcement announcement = Announcement.Create(Now);
        announcement.Text = new string('a', 120);
        await SaveAsync(c =>
        {
            c.Offerings.Add(Offering.Create(new string('a', 150), "a", 1, Now));
            c.FaqItems.Add(FaqItem.Create(new string('a', 200), 1, Now));
            c.AboutPages.Add(page);
            c.Announcements.Add(announcement);
        });

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM about_pages"));
    }

    [Fact]
    public async Task DeletingTheImageOfTheLogo_IsRefused()
    {
        Image logo = Image.Create("logo-key", ImageFormat.Png, 100, 100, Now);
        await SaveAsync(c => c.Images.Add(logo));
        Branding branding = Branding.Create(Now);
        branding.LogoId = logo.Id;
        await SaveAsync(c => c.Brandings.Add(branding));

        Assert.Equal(ForeignKeyViolation, (await ViolationOfSqlAsync("DELETE FROM images WHERE id = @id", ("id", logo.Id))).SqlState);
    }

    [Fact]
    public async Task DeletingTheImageOfTheFavicon_IsRefused()
    {
        Image favicon = Image.Create("favicon-key", ImageFormat.Png, 32, 32, Now);
        await SaveAsync(c => c.Images.Add(favicon));
        Branding branding = Branding.Create(Now);
        branding.FaviconId = favicon.Id;
        await SaveAsync(c => c.Brandings.Add(branding));

        Assert.Equal(ForeignKeyViolation, (await ViolationOfSqlAsync("DELETE FROM images WHERE id = @id", ("id", favicon.Id))).SqlState);
    }

    [Fact]
    public async Task ReplacedLogo_LeavesTheOldImageUnlinkedAndDeletable()
    {
        // 06 4.54: a replaced or removed image is left without a link and the disposal deletes it.
        Image oldLogo = Image.Create("old", ImageFormat.Png, 100, 100, Now);
        Image newLogo = Image.Create("new", ImageFormat.Png, 100, 100, Now);
        await SaveAsync(c =>
        {
            c.Images.Add(oldLogo);
            c.Images.Add(newLogo);
        });
        Branding branding = Branding.Create(Now);
        branding.LogoId = oldLogo.Id;
        await SaveAsync(c => c.Brandings.Add(branding));

        await ExecuteAsync("UPDATE brandings SET logo_id = @id", ("id", newLogo.Id));
        await ExecuteAsync("DELETE FROM images WHERE id = @id", ("id", oldLogo.Id));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM images"));
    }

    [Fact]
    public async Task BrandingWithAnUnknownImage_IsRefused()
    {
        Branding branding = Branding.Create(Now);
        branding.LogoId = Guid.NewGuid();

        Assert.Equal((ForeignKeyViolation, "fk_brandings_images_logo_id"), await ViolationOfAsync(c => c.Brandings.Add(branding)));
    }

    // ---- The permanent rows ----

    [Fact]
    public async Task ContextRefusesToDeleteTheAboutPage()
    {
        AboutPage page = AboutPage.Create(Now);
        await SaveAsync(c => c.AboutPages.Add(page));
        await using AppDbContext context = NewContext();
        context.AboutPages.Attach(page);
        context.AboutPages.Remove(page);

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM about_pages"));
    }

    [Fact]
    public async Task ContextRefusesToDeleteTheBranding()
    {
        Branding branding = Branding.Create(Now);
        await SaveAsync(c => c.Brandings.Add(branding));
        await using AppDbContext context = NewContext();
        context.Brandings.Attach(branding);
        context.Brandings.Remove(branding);

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM brandings"));
    }

    [Fact]
    public async Task ContextDeletesTheRecordsOfTheirOwnEvents()
    {
        // 06 4.47-4.49, 4.51, 4.55: these are deleted by an event (the firm's delete, an emptied form field).
        Offering offering = Offering.Create("A", "a", 1, Now);
        PortfolioItem item = PortfolioItem.Create("A", "a", 1, Now);
        FaqItem faq = FaqItem.Create("A?", 1, Now);
        CustomPage page = CustomPage.Create("A", "a", 1, Now);
        SocialLink link = SocialLink.Create(SocialPlatform.X, "https://x.test/a", Now);
        await SaveAsync(c =>
        {
            c.Offerings.Add(offering);
            c.PortfolioItems.Add(item);
            c.FaqItems.Add(faq);
            c.CustomPages.Add(page);
            c.SocialLinks.Add(link);
        });

        await SaveAsync(c =>
        {
            c.Offerings.Attach(offering);
            c.Offerings.Remove(offering);
            c.PortfolioItems.Attach(item);
            c.PortfolioItems.Remove(item);
            c.FaqItems.Attach(faq);
            c.FaqItems.Remove(faq);
            c.CustomPages.Attach(page);
            c.CustomPages.Remove(page);
            c.SocialLinks.Attach(link);
            c.SocialLinks.Remove(link);
        });

        Assert.Equal(
            0L,
            await ScalarAsync<long>(
                "SELECT (SELECT count(*) FROM offerings) + (SELECT count(*) FROM portfolio_items) + (SELECT count(*) FROM faq_items)"
                + " + (SELECT count(*) FROM custom_pages) + (SELECT count(*) FROM social_links)"));
    }

    private static string SlugInsert(string table, string titleColumn) =>
        $"INSERT INTO {table} (id, created_at, {titleColumn}, sort_order, slug) VALUES (gen_random_uuid(), now(), 'Başlık', 1, @slug)";

    private AppDbContext NewContext()
    {
        Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext> builder = new();
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

    /// <summary>Runs the statement and returns the SQLSTATE and the constraint name of the database error it raises.</summary>
    private async Task<(string SqlState, string? Constraint)> ViolationOfSqlAsync(string sql, params (string Name, object Value)[] parameters)
    {
        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(sql, parameters));
        return (error.SqlState, error.ConstraintName);
    }

    private async Task ExecuteAsync(string sql, params (string Name, object Value)[] parameters)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        foreach ((string name, object value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

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
