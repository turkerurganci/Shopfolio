using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;
using NpgsqlTypes;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.IntegrationTests.Ordering;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

using ItemAmounts = Shopfolio.Modules.Catalog.Contracts.Pricing.ItemAmounts;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The defect claim half of the requests list on the opened application (T532 criteria 1-4, KK2-KK4; 07 UC-93): real orders and items, the claim rows written as the application writes them, the dispatch
/// record of the notice B-9, the two kinds in one list, the counter equality, the page boundary and the number of commands a page sends.
/// </summary>
public sealed partial class RequestsPageTests
{
    private static readonly DateTimeOffset LongAgo = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static int _versionNumber = 9200;

    private int _seedCounter;
    private Guid _province;
    private CompanyIdentity? _identity;

    // ---- KK4: the defect claim's row ----

    [Fact]
    public async Task ADefectClaimRow_NamesTheItemWithItsOptions_LeadsToTheOrderSection_AndCarriesTheMarkOnlyWhenTheNoticeDidNotArrive_KK4()
    {
        await ResetOrdersAsync();
        (Guid marked, Guid markedOrder) = await SeedClaimAsync(3, 2, "Mavi", "M", DateTimeOffset.UtcNow.AddDays(-2));
        (Guid delivered, Guid deliveredOrder) = await SeedClaimAsync(1, 1, null, null, DateTimeOffset.UtcNow.AddDays(-1));
        await QueueNoticeAsync(marked, markedOrder, DispatchState.Undelivered);
        await QueueNoticeAsync(delivered, deliveredOrder, DispatchState.Sent);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, ListPath);

        Assert.Equal(HttpStatusCode.OK, status);
        string markedRow = RowOf(html, $"#ayip-talebi-{marked:D}");
        string deliveredRow = RowOf(html, $"#ayip-talebi-{delivered:D}");
        string expectedLink = $"/panel/siparisler/{markedOrder:D}?returnUrl={Uri.EscapeDataString(ListPath)}#ayip-talebi-{marked:D}";
        Assert.Contains(expectedLink, markedRow, StringComparison.Ordinal);
        Assert.Contains($"/panel/siparisler/{deliveredOrder:D}?returnUrl=", deliveredRow, StringComparison.Ordinal);
        Assert.Contains("Ayıp talebi", markedRow, StringComparison.Ordinal);
        Assert.Contains("(Mavi, M)", markedRow, StringComparison.Ordinal);
        Assert.Contains("2 adet ayıplı", markedRow, StringComparison.Ordinal);
        Assert.Contains("E-posta ulaşmadı", markedRow, StringComparison.Ordinal);
        Assert.Contains("Pano Ürün", deliveredRow, StringComparison.Ordinal);
        Assert.DoesNotContain("(Mavi", deliveredRow, StringComparison.Ordinal);
        Assert.Contains("1 adet ayıplı", deliveredRow, StringComparison.Ordinal);
        Assert.DoesNotContain("E-posta ulaşmadı", deliveredRow, StringComparison.Ordinal);
    }

    // ---- KK2, KK3: the two kinds together, the two orders, the counter ----

    [Fact]
    public async Task TheTwoKinds_StandInOneListByTheLastOpening_AReopenedClaimWithItsReopening_AndTheOpenRowsEqualTheHomeCounter_KK1_KK2_KK3()
    {
        await ResetOrdersAsync();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        ContactRequest contact = await AddAsync("Ayşe", "ayse@example.com", ContactTopic.Other, "Mesaj", now.AddDays(-3));
        (Guid reopened, _) = await SeedClaimAsync(1, 1, null, null, now.AddDays(-10));
        (Guid fresh, _) = await SeedClaimAsync(1, 1, null, null, now.AddDays(-2));
        (Guid resolved, _) = await SeedClaimAsync(1, 1, null, null, now.AddDays(-1));
        await ExecuteAsync($"UPDATE defect_claims SET status_history = '[{{\"At\":\"{now.AddHours(-5):O}\",\"Status\":\"Open\",\"ActorKind\":\"Admin\",\"Quantity\":null}}]'::jsonb WHERE id = '{reopened}'");
        await ExecuteAsync($"UPDATE defect_claims SET defect_claim_status = 'Resolved' WHERE id = '{resolved}'");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string html) = await GetAsync(client, ListPath);
        (_, string openOnly) = await GetAsync(client, ListPath + "?requestStatus=Open");
        (_, string defectOnly) = await GetAsync(client, ListPath + "?requestType=DefectClaim");
        (_, string resolvedOnly) = await GetAsync(client, ListPath + "?requestStatus=Resolved");

        int reopenedAt = html.IndexOf($"#ayip-talebi-{reopened:D}", StringComparison.Ordinal);
        int freshAt = html.IndexOf($"#ayip-talebi-{fresh:D}", StringComparison.Ordinal);
        int resolvedAt = html.IndexOf($"#ayip-talebi-{resolved:D}", StringComparison.Ordinal);
        int contactAt = html.IndexOf(contact.Id.ToString("D"), StringComparison.Ordinal);
        Assert.True(reopenedAt >= 0 && freshAt >= 0 && resolvedAt >= 0 && contactAt >= 0, "Every row is in the list.");
        Assert.True(reopenedAt < resolvedAt && resolvedAt < freshAt && freshAt < contactAt, "The reopened claim stands by its last opening, then the newer claims, then the older contact request.");
        Assert.Contains("son açılış", RowOf(html, $"#ayip-talebi-{reopened:D}"), StringComparison.Ordinal);
        Assert.DoesNotContain("son açılış", RowOf(html, $"#ayip-talebi-{fresh:D}"), StringComparison.Ordinal);
        Assert.DoesNotContain(contact.Id.ToString("D"), defectOnly, StringComparison.Ordinal);
        Assert.DoesNotContain($"#ayip-talebi-{resolved:D}", openOnly, StringComparison.Ordinal);
        Assert.Contains($"#ayip-talebi-{resolved:D}", resolvedOnly, StringComparison.Ordinal);
        Assert.DoesNotContain(contact.Id.ToString("D"), resolvedOnly, StringComparison.Ordinal);

        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        int counter = await scope.ServiceProvider.GetRequiredService<ICorporateContentModule>().CountOpenContactRequestsAsync(Ct)
            + await scope.ServiceProvider.GetRequiredService<IAfterSalesModule>().CountOpenDefectClaimsAsync(Ct);
        Assert.Equal(3, counter);
        Assert.Equal(counter, Rows(openOnly).Count);
    }

    // ---- The page boundary and the number of commands ----

    [Fact]
    public async Task APageBoundaryInsideOneMoment_LosesAndRepeatsNoRow_TheLastPageAndAHugePageNumberAreTheLastPage_KK1()
    {
        await ResetOrdersAsync();
        DateTimeOffset moment = DateTimeOffset.UtcNow.AddDays(-1);
        int total = PageSizes.Panel + 5;
        List<Guid> ids = [];
        for (int i = 0; i < total; i++)
        {
            ids.Add((await AddAsync($"Ad {i}", $"kisi{i}@example.com", ContactTopic.Other, "Aynı an", moment)).Id);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string first) = await GetAsync(client, ListPath);
        (_, string second) = await GetAsync(client, ListPath + "?page=2");
        (_, string huge) = await GetAsync(client, ListPath + "?page=99999999999");

        List<string> firstLinks = RecordLinks(first);
        List<string> secondLinks = RecordLinks(second);
        Assert.Equal(PageSizes.Panel, firstLinks.Count);
        Assert.Equal(5, secondLinks.Count);
        Assert.Empty(firstLinks.Intersect(secondLinks));
        Assert.Equal(total, firstLinks.Concat(secondLinks).Select(l => l[..(l.IndexOf('?', StringComparison.Ordinal))]).Distinct().Count());
        Assert.Equal(secondLinks, RecordLinks(huge));
        Assert.All(ids, id => Assert.True(first.Contains(id.ToString("D"), StringComparison.Ordinal) ^ second.Contains(id.ToString("D"), StringComparison.Ordinal)));
    }

    [Fact]
    public async Task TwoPages_SendTheSameNumberOfCommands_WhateverTheNumberOfDefectClaimsOnThem_KK1()
    {
        await ResetOrdersAsync();
        DateTimeOffset now = DateTimeOffset.UtcNow.AddDays(-1);
        for (int i = 0; i < 15; i++)
        {
            await AddAsync($"Ad {i}", $"kisi{i}@example.com", ContactTopic.Other, "Mesaj", now.AddMinutes(-2 * i));
            await InsertClaimRowAsync(now.AddMinutes((-2 * i) - 1));
        }

        StorefrontSearchAndProductTests.CommandCounter counter = new();
        await using WebApplicationFactory<Program> factory = new ShopfolioWebApplicationFactory(_database.Dsn, withJobRunner: false)
            .WithWebHostBuilder(b => b.ConfigureServices(s =>
            {
                s.AddRazorPages().AddApplicationPart(typeof(ForgeryTokenSampleModel).Assembly);
                s.ConfigureDbContext<AppDbContext>(options => options.AddInterceptors(counter));
            }));
        using HttpClient client = ClientOf(factory);
        await GetAsync(client, ListPath);

        int before = counter.Count;
        (_, string first) = await GetAsync(client, ListPath);
        int firstCommands = counter.Count - before;
        before = counter.Count;
        (_, string second) = await GetAsync(client, ListPath + "?page=2");
        int secondCommands = counter.Count - before;

        Assert.Equal(PageSizes.Panel, Rows(first).Count);
        Assert.Equal(30 - PageSizes.Panel, Rows(second).Count);
        Assert.Contains("Ayıp talebi", first, StringComparison.Ordinal);
        Assert.Contains("Ayıp talebi", second, StringComparison.Ordinal);
        Assert.Equal(firstCommands, secondCommands);
    }

    // ---- Seeding ----

    private static string RowOf(string html, string marker)
    {
        int at = html.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(at >= 0, $"The list has no row with {marker}.");
        int start = html.LastIndexOf("<tr", at, StringComparison.Ordinal);
        int end = html.IndexOf("</tr>", at, StringComparison.Ordinal);
        return html[start..end];
    }

    private static List<string> Rows(string html)
    {
        List<string> rows = [];
        int at = 0;
        while ((at = html.IndexOf("<tbody", at, StringComparison.Ordinal)) >= 0)
        {
            int end = html.IndexOf("</tbody>", at, StringComparison.Ordinal);
            string body = html[at..end];
            foreach (System.Text.RegularExpressions.Match m in RowStart().Matches(body))
            {
                rows.Add(m.Value);
            }

            at = end;
        }

        return rows;
    }

    private async Task ResetOrdersAsync()
    {
        await ExecuteAsync(
            "TRUNCATE defect_claims, dispatch_records, order_items, orders, legal_text_versions, price_histories, variants, option_values, options, products CASCADE");
        _province = await ProvinceAsync();
        _identity ??= await SetCompanyAsync();
    }

    /// <summary>An order with one item bought <paramref name="itemQuantity"/> times and one claim of <paramref name="claimQuantity"/> on it; the claim opened at <paramref name="at"/>.</summary>
    private async Task<(Guid ClaimId, Guid OrderId)> SeedClaimAsync(int itemQuantity, int claimQuantity, string? first, string? second, DateTimeOffset at)
    {
        Guid variant = await SeedVariantAsync(300m);
        Guid orderId = await SaveOrderAsync(variant, itemQuantity);
        Guid itemId;
        await using (AppDbContext db = NewContext())
        {
            itemId = await db.OrderItems.AsNoTracking().Where(i => i.OrderId == orderId).Select(i => i.Id).SingleAsync(Ct);
        }

        string firstSql = first is null ? "NULL" : $"'{first}'";
        string secondSql = second is null ? "NULL" : $"'{second}'";
        await ExecuteAsync($"UPDATE order_items SET first_option_value = {firstSql}, second_option_value = {secondSql} WHERE id = '{itemId}'");
        DefectClaim claim = DefectClaim.Create(itemId, claimQuantity, "Kusurlu", null, at);
        await using AppDbContext context = NewContext();
        context.DefectClaims.Add(claim);
        await context.SaveChangesAsync(Ct);
        return (claim.Id, orderId);
    }

    private async Task InsertClaimRowAsync(DateTimeOffset createdAt)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using (NpgsqlCommand role = new("SET session_replication_role = replica", connection))
        {
            await role.ExecuteNonQueryAsync(Ct);
        }

        await using NpgsqlCommand command = new(
            "INSERT INTO defect_claims (id, created_at, order_item_id, quantity, description, defect_claim_status, status_history) VALUES (@id, @created, @item, 1, 'Kusurlu', 'Open', @history)",
            connection);
        command.Parameters.AddWithValue("id", Guid.NewGuid());
        command.Parameters.AddWithValue("created", createdAt);
        command.Parameters.AddWithValue("item", Guid.NewGuid());
        command.Parameters.Add(new NpgsqlParameter("history", NpgsqlDbType.Jsonb) { Value = "[]" });
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task QueueNoticeAsync(Guid claimId, Guid orderId, DispatchState state)
    {
        DispatchRecord row = DispatchRecord.Queue(DispatchType.B9, "ali@example.com", orderId, DispatchRelatedRecordType.DefectClaim, claimId, DateTimeOffset.UtcNow.AddHours(-1));
        row.DispatchState = state;
        if (state == DispatchState.Sent)
        {
            row.SentAt = row.CreatedAt;
            row.LastOutcome = DispatchOutcome.Sent;
            row.AttemptCount = 1;
        }
        else
        {
            row.LastOutcome = DispatchOutcome.RecipientError;
            row.AttemptCount = 3;
        }

        await using AppDbContext context = NewContext();
        context.DispatchRecords.Add(row);
        await context.SaveChangesAsync(Ct);
    }

    private async Task<Guid> SaveOrderAsync(Guid variantId, int quantity)
    {
        int n = Interlocked.Increment(ref _seedCounter);
        DateTimeOffset createdAt = LongAgo.AddMinutes(n);
        OrderItemSource source = await SourceAsync(variantId);
        ItemAmounts amounts = ItemPricing.Price(new ItemPriceInput(source.UnitPrice, quantity, 0m, source.VatRate));
        Order order = Order.Create(
            $"2026-R{n:D5}{Guid.NewGuid().ToString("N")[..4]}", $"access-{Guid.NewGuid():N}", $"confirm-{Guid.NewGuid():N}", PaymentMethod.Card, true, "ali@example.com",
            ItemPricing.SumOrderTotal([amounts], 0m), 0m, createdAt.AddHours(1), createdAt);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = Guid.NewGuid();
        order.FreezeAddresses(("Ali Veli", "5321234567", _province, "Çankaya", "Atatürk Bulvarı 1"), ("Ali Veli", _province, "Çankaya", "Atatürk Bulvarı 1"));
        OrderFreeze.FreezeLegalIdentity(order, _identity!, new LegalTextVersionIds(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), null);
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        order.FreezeShippingFeeVatBreakdown([]);
        order.FreezeShippingPromise(2);
        OrderItem item = OrderFreeze.FreezeItem(order.Id, source, quantity, amounts, 3, order.CreatedAt);
        int version = Interlocked.Increment(ref _versionNumber);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, version, LongAgo, "metin", LongAgo);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, version, LongAgo, "metin", LongAgo);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, version, LongAgo, "metin", LongAgo).WithHosting();
        order.PreInformationFormVersionId = form.Id;
        order.DistanceSalesContractVersionId = contract.Id;
        order.PrivacyNoticeVersionId = notice.Id;
        await using AppDbContext context = NewContext();
        context.LegalTextVersions.AddRange(form, contract, notice);
        context.Orders.Add(order);
        context.OrderItems.Add(item);
        await context.SaveChangesAsync(Ct);
        return order.Id;
    }

    private async Task<Guid> SeedVariantAsync(decimal price)
    {
        int n = Interlocked.Increment(ref _seedCounter);
        Product product = Product.Create($"Pano Ürün {n}", $"pano-urun-{n}", ProductType.Physical, 20m, LongAgo);
        product.WithSeededOrigin();
        product.PublishStatus = PublishStatus.Published;
        Variant variant = Variant.Create(product.Id, $"PNO-{n}", price, LongAgo, 100);
        variant.PublishStatus = PublishStatus.Published;
        variant.FirstOnSaleAt = LongAgo;
        await using AppDbContext context = NewContext();
        context.Products.Add(product);
        context.Variants.Add(variant);
        context.PriceHistories.Add(PriceHistory.Create(variant.Id, price, LongAgo, LongAgo));
        await context.SaveChangesAsync(Ct);
        return variant.Id;
    }

    private async Task<OrderItemSource> SourceAsync(Guid variantId)
    {
        await using AppDbContext db = NewContext();
        TimeProvider time = new SeedTime(LongAgo.AddDays(1));
        return Assert.Single(await new OrderItemSourceReader(db, time, new ProductGallery(db, time), new DigitalFileReader(db)).ListAsync([variantId], Ct));
    }

    private async Task<Guid> ProvinceAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.Provinces.AsNoTracking().OrderBy(p => p.Code).Select(p => p.Id).FirstAsync(Ct);
    }

    private async Task<CompanyIdentity> SetCompanyAsync()
    {
        await ExecuteAsync(
            $"UPDATE companies SET company_type = 'Tradesman', legal_name = 'Örnek Ticaret', tax_id_number = '1111111111', province_id = '{_province}', district = 'Çankaya', "
            + "address_line = 'Kızılay 2', phone = '3121234567', email = 'firma@example.com', kep_address = 'firma@hs01.kep.tr', chamber_name = 'Ankara Esnaf Odası', professional_conduct_rules = 'Meslek kuralları'");
        await using ServiceProvider provider = OrderingTestServices.Build(_database.Dsn);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ISettingsModule>().GetCompanyIdentityAsync(Ct);
    }

    private sealed class SeedTime(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
