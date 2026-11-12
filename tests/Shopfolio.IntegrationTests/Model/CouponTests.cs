using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Coupons;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the coupon tests (T338): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(CouponTestsDefinition.Name)]
public sealed class CouponTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "Coupon";
}

/// <summary>
/// The coupon through the real Catalog code on a real PostgreSQL (T338 criteria 1, 2, 4-8; KK1, KK2, KK4, KK6-KK8; 02 3.10.1-3.10.6, 5.11.3, 8.2 L-6; P-36): both definitions are
/// written, a code that differs only in letter case - "I", "İ", "ı", "i" included - is not, and it matches at the payment step; the payment step's field closes after the
/// cart's invalid attempts pass L-6 while an accepted code does not count; a coupon outside its dates or with its rights taken is refused; two confirmations of the last right
/// reserve it once, and the right comes back only when the whole order closes; the share the order item froze stays when the coupon changes. The share arithmetic, the
/// refusals that need no table (KK3, KK5) and the threshold base (KK9) are pure and are the unit tests' (CouponRulesTests).
/// </summary>
[Collection(CouponTestsDefinition.Name)]
public sealed class CouponTests : IAsyncLifetime
{
    // 15 November 2026, 12:00 in Turkey; the coupons run from 1 to 30 November.
    private static readonly DateTimeOffset Now = new(2026, 11, 15, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Start = new(2026, 11, 1);
    private static readonly DateOnly End = new(2026, 11, 30);
    private static readonly CouponLine[] Basket = [new CouponLine(100m, 2, 20m), new CouponLine(60m, 1, 10m)];

    private readonly CollectionDatabaseFixture _collection;

    private readonly ManualTimeProvider _time = new(Now);

    private int _counter;

    public CouponTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE stock_reservations, order_items, orders, coupons, attempt_records, legal_text_versions, provinces CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK2: both definitions are written ----

    [Fact]
    public async Task APercentageCoupon_AndAFixedAmountCoupon_AreWritten_WithTheValueOfTheirKindOnly()
    {
        CouponWriteResult percent = await CreateAsync(new CouponDefinition("YAZ10", CouponKind.Percentage, 10m, null, Start, End, 100));
        CouponWriteResult amount = await CreateAsync(new CouponDefinition("KIS50", CouponKind.Amount, 50m, 300m, Start, End, 1));

        Assert.False(percent.IsRejected);
        Assert.False(amount.IsRejected);
        await using AppDbContext reader = NewContext();
        Coupon p = await reader.Coupons.SingleAsync(c => c.Id == percent.CouponId, Ct);
        Coupon a = await reader.Coupons.SingleAsync(c => c.Id == amount.CouponId, Ct);
        Assert.Equal((10m, (decimal?)null, (decimal?)null, 100, 0, 0), (p.Percentage!.Value, p.Amount, p.MinimumBasketAmount, p.TotalUsageLimit, p.UsedCount, p.ReservedCount));
        Assert.Equal(((decimal?)null, 50m, 300m, 1), (a.Percentage, a.Amount!.Value, a.MinimumBasketAmount!.Value, a.TotalUsageLimit));
        Assert.Equal(("YAZ10", "yaz10"), (p.Code, p.NormalizedCode));
    }

    [Fact]
    public async Task ADefinitionWithFieldProblems_IsRefused_WithEveryProblem_AndWritesNothing()
    {
        CouponWriteResult result = await CreateAsync(new CouponDefinition("KIS50", CouponKind.Amount, 50m, 50m, End, Start, 10));

        Assert.True(result.IsRejected);
        Assert.Equal(
            [new CouponProblem("MinimumBasketAmount", ErrorCode.CouponValueInvalid), new CouponProblem("EndDate", ErrorCode.InvalidDateRange)],
            result.Problems);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM coupons"));
    }

    // ---- KK7: one code without letter case ----

    [Theory]
    [InlineData("YAZ10", "yaz10")]
    [InlineData("İNDİRİM", "indirim")]
    [InlineData("indirim", "INDIRIM")]
    [InlineData("KIŞ", "kış")]
    public async Task ACodeThatDiffersOnlyInLetterCase_IsNotWritten(string first, string second)
    {
        Assert.False((await CreateAsync(new CouponDefinition(first, CouponKind.Percentage, 10m, null, Start, End, 5))).IsRejected);

        CouponWriteResult again = await CreateAsync(new CouponDefinition(second, CouponKind.Percentage, 20m, null, Start, End, 5));

        Assert.Equal([new CouponProblem("Code", ErrorCode.DuplicateCouponCode)], again.Problems);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM coupons"));
    }

    [Fact]
    public async Task TwoSavesThatRaceForOneCode_MeetTheUniqueIndex()
    {
        await SaveAsync(c => c.Coupons.Add(Coupon.CreatePercentage("İNDİRİM", 10m, Start, End, 5, Now)));

        DbUpdateException error = await Assert.ThrowsAsync<DbUpdateException>(() => SaveAsync(c => c.Coupons.Add(Coupon.CreatePercentage("ındırım", 10m, Start, End, 5, Now))));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, Assert.IsType<PostgresException>(error.InnerException).SqlState);
    }

    [Theory]
    [InlineData("yaz10")]
    [InlineData("  Yaz10 ")]
    [InlineData("YAZ10")]
    public async Task TheCodeTyped_MatchesWithoutLetterCase_AndTheCodeAsDefinedComesBack(string typed)
    {
        Coupon coupon = await SeedPercentAsync("YAZ10", 10m);

        CouponApplication application = await EvaluateAsync(typed, Basket);

        Assert.Equal((CouponOutcome.Accepted, coupon.Id, "YAZ10"), (application.Outcome, application.CouponId, application.Code));
    }

    [Fact]
    public async Task TheTurkishDottedAndDotlessI_AreOneLetter_WhenTheCodeIsMatched()
    {
        await SeedPercentAsync("İNDİRİM", 10m);

        Assert.Equal(CouponOutcome.Accepted, (await EvaluateAsync("indirim", Basket)).Outcome);
        Assert.Equal(CouponOutcome.Accepted, (await EvaluateAsync("INDIRIM", Basket)).Outcome);
        Assert.Equal(CouponOutcome.Accepted, (await EvaluateAsync("ındırım", Basket)).Outcome);
    }

    // ---- KK1 and KK7: the payment step's field and the attempt limit ----

    [Fact]
    public async Task ThePaymentStep_AppliesTheCode_WithTheSharesOfEveryItem()
    {
        await SeedPercentAsync("YAZ10", 10m);

        CouponApplication application = await ApplyAsync(Guid.NewGuid(), "yaz10", Basket);

        // 100 x 2 = 200 and 60 x 1 = 60: 10% gives 20,00 and 6,00; the VAT after the share: 180 x 20 / 120 = 30,00 and 54 x 10 / 110 = 4,91.
        Assert.Equal(CouponOutcome.Accepted, application.Outcome);
        Assert.Equal([new ItemAmounts(200m, 20m, 180m, 30m, 150m), new ItemAmounts(60m, 6m, 54m, 4.91m, 49.09m)], application.Items);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM attempt_records"));
    }

    [Fact]
    public async Task InvalidCodesPastTheLimit_CloseTheCartsField_EvenForAValidCode_AndAnotherCartIsNotTouched()
    {
        await SeedPercentAsync("YAZ10", 10m);
        Guid cart = Guid.NewGuid();
        int limit = ProductConstants.InvalidCouponAttemptsPerCart;

        for (int i = 0; i < limit; i++)
        {
            Assert.Equal(CouponOutcome.Rejected, (await ApplyAsync(cart, $"YANLIS{i}", Basket)).Outcome);
        }

        Assert.Equal(CouponOutcome.AttemptLimitExceeded, (await ApplyAsync(cart, "YAZ10", Basket)).Outcome);
        Assert.Equal(CouponOutcome.AttemptLimitExceeded, (await ApplyAsync(cart, "YANLIS", Basket)).Outcome);
        Assert.Equal(CouponOutcome.Accepted, (await ApplyAsync(Guid.NewGuid(), "YAZ10", Basket)).Outcome);

        // Only the invalid attempts stay on the cart; a refused attempt past the limit and the accepted one leave no row.
        Assert.Equal((long)limit, await ScalarAsync<long>($"SELECT count(*) FROM attempt_records WHERE normalized_value = '{cart:D}'"));
    }

    [Fact]
    public async Task AnAcceptedCode_AndAValidCodeUnderItsMinimum_DoNotCountTowardTheLimit()
    {
        await SeedPercentAsync("YAZ10", 10m);
        await SaveAsync(c => c.Coupons.Add(Coupon.CreateAmount("KIS50", 50m, 1000m, Start, End, 5, Now)));
        Guid cart = Guid.NewGuid();
        int limit = ProductConstants.InvalidCouponAttemptsPerCart;

        for (int i = 0; i < limit - 1; i++)
        {
            Assert.Equal(CouponOutcome.Rejected, (await ApplyAsync(cart, $"YANLIS{i}", Basket)).Outcome);
        }

        CouponApplication under = await ApplyAsync(cart, "kis50", Basket);
        Assert.Equal((CouponOutcome.BelowMinimumBasket, 1000m, 260m), (under.Outcome, under.MinimumBasketAmount, under.BasketTotal));
        Assert.Equal(CouponOutcome.Accepted, (await ApplyAsync(cart, "YAZ10", Basket)).Outcome);

        // The limit's own invalid attempt is still let through - and is the last.
        Assert.Equal(CouponOutcome.Rejected, (await ApplyAsync(cart, "YANLIS", Basket)).Outcome);
        Assert.Equal(CouponOutcome.AttemptLimitExceeded, (await ApplyAsync(cart, "YAZ10", Basket)).Outcome);
    }

    [Fact]
    public async Task ACouponDoesNotLowerTheBasis_OfTheThresholdAndTheMinimumOrderAmount_T349_KK2()
    {
        await SeedPercentAsync("YARI50", 50m);
        CouponLine[] basket = [new(250m, 2, 20m)];

        CouponApplication applied = await EvaluateAsync("YARI50", basket);

        // The customer pays 250 after the coupon; the basis stays the 500 before it (02 3.10.7, 3.18.2, 3.19.4), so shipping is free and the minimum of 400 is met.
        Assert.Equal(CouponOutcome.Accepted, applied.Outcome);
        Assert.Equal(250m, applied.Items.Sum(i => i.Payable));
        ShippingAssessment assessment = ShippingCharge.Assess(new ShippingFeeSection(50m, 500m, 400m, 1), applied.BasketTotal, hasPhysicalItem: true);
        Assert.Equal(500m, assessment.Basis);
        Assert.Equal(0m, assessment.Fee);
        Assert.Equal("Kargo ücretsiz", assessment.FreeShippingMessage);
        Assert.Null(assessment.Refusal);
    }

    // ---- KK6: dates and rights ----

    [Fact]
    public async Task ACodeOutsideItsDates_OrWithItsRightsTaken_IsRefused()
    {
        await SaveAsync(c => c.Coupons.Add(Coupon.CreatePercentage("EKIM", 10m, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31), 5, Now)));
        await SaveAsync(c => c.Coupons.Add(Coupon.CreatePercentage("ARALIK", 10m, new DateOnly(2026, 12, 1), new DateOnly(2026, 12, 31), 5, Now)));
        Coupon personal = await SeedPercentAsync("KISIYE", 10m, limit: 1);

        Assert.Equal(CouponOutcome.Rejected, (await EvaluateAsync("ekim", Basket)).Outcome);
        Assert.Equal(CouponOutcome.Rejected, (await EvaluateAsync("aralik", Basket)).Outcome);
        Assert.Equal(CouponOutcome.Accepted, (await EvaluateAsync("kisiye", Basket)).Outcome);

        Assert.True(await ReserveAsync(personal.Id));
        Assert.Equal(CouponOutcome.Rejected, (await EvaluateAsync("kisiye", Basket)).Outcome);
    }

    [Fact]
    public async Task AfterTheEndDay_ANewOrderIsRefused_AndTheRightsOfConfirmedOrdersStayWhereTheyWere_T326_KK3()
    {
        Coupon coupon = await SeedPercentAsync("SONGUN", 10m, limit: 5);
        StockClaim paid = new([], coupon.Id);
        Assert.True(await ReserveAsync(coupon.Id));
        await RunAsync(async (catalog, token) =>
        {
            await catalog.CommitAsync(paid, token);
            return Done.Value;
        });
        Assert.True(await ReserveAsync(coupon.Id));
        await AssertRightsAsync(coupon.Id, used: 1, reserved: 1);

        // The last minute of the end day still accepts the code; the moment after it does not - no step in between touches the coupon.
        DateTimeOffset gone = TurkeyTime.EndOfRange(End);
        _time.SetUtcNow(gone.AddTicks(-1));
        Assert.Equal(CouponOutcome.Accepted, (await EvaluateAsync("songun", Basket)).Outcome);
        _time.SetUtcNow(gone);
        Assert.Equal(CouponOutcome.Rejected, (await EvaluateAsync("songun", Basket)).Outcome);
        _time.SetUtcNow(gone.AddDays(40));
        Assert.Equal(CouponOutcome.Rejected, (await EvaluateAsync("songun", Basket)).Outcome);

        // The paid order's used right and the waiting order's reserved right are where they were; the row is not changed (version 1).
        await AssertRightsAsync(coupon.Id, used: 1, reserved: 1);

        // The waiting order's right still goes through its own transitions after the end: it is released like any other, not by the date.
        await RunAsync(async (catalog, token) =>
        {
            await catalog.ReleaseAsync(new StockClaim([], coupon.Id), token);
            return Done.Value;
        });
        await AssertRightsAsync(coupon.Id, used: 1, reserved: 0);
    }

    // ---- KK8: the last right, and its return ----

    [Fact]
    public async Task TwoConfirmationsOfTheLastRight_ReserveItOnce_TheSecondWaitsAndIsRefused()
    {
        Coupon coupon = await SeedPercentAsync("SON1", 10m, limit: 1);
        StockClaim claim = new([], coupon.Id);

        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope first = provider.CreateAsyncScope();
        await using AsyncServiceScope second = provider.CreateAsyncScope();
        AppDbContext firstDb = first.ServiceProvider.GetRequiredService<AppDbContext>();
        AppDbContext secondDb = second.ServiceProvider.GetRequiredService<AppDbContext>();

        await using IDbContextTransaction firstTransaction = await firstDb.Database.BeginTransactionAsync(Ct);
        Assert.True(await first.ServiceProvider.GetRequiredService<ICatalogModule>().ReserveAsync(claim, null, Ct));
        await using IDbContextTransaction secondTransaction = await secondDb.Database.BeginTransactionAsync(Ct);
        int pid = (await secondDb.Database.SqlQuery<int>($"SELECT pg_backend_pid() AS \"Value\"").ToListAsync(Ct)).Single();
        Task<bool> waiting = second.ServiceProvider.GetRequiredService<ICatalogModule>().ReserveAsync(claim, null, Ct);
        await WaitUntilBlockedAsync(pid);

        await firstTransaction.CommitAsync(Ct);
        bool secondReserved = await waiting.WaitAsync(TimeSpan.FromSeconds(30), Ct);
        await secondTransaction.CommitAsync(Ct);

        Assert.False(secondReserved);
        await AssertRightsAsync(coupon.Id, used: 0, reserved: 1);
    }

    [Fact]
    public async Task TheRight_IsReservedAtConfirmation_UsedAtPayment_AndGivenBackOnlyWhenTheWholeOrderCloses()
    {
        Coupon coupon = await SeedPercentAsync("TAM", 10m, limit: 1);
        StockClaim claim = new([], coupon.Id);
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        ICatalogModule rules = scope.ServiceProvider.GetRequiredService<ICatalogModule>();

        Assert.True(await ReserveAsync(coupon.Id));
        await AssertRightsAsync(coupon.Id, used: 0, reserved: 1);
        await RunAsync(async (catalog, token) =>
        {
            await catalog.CommitAsync(claim, token);
            return Done.Value;
        });
        await AssertRightsAsync(coupon.Id, used: 1, reserved: 0);

        // A partial cancellation or return keeps the right used; the order's last units closing give it back (02 5.11.3, 7.2.6, 7.4.8).
        ItemClosure[] partial = [new ItemClosure(2, 2), new ItemClosure(1, 0)];
        ItemClosure[] whole = [new ItemClosure(2, 2), new ItemClosure(1, 1)];
        Assert.False(rules.ReturnsCouponRight(partial));
        Assert.True(rules.ReturnsCouponRight(whole));
        await RunAsync(async (catalog, token) =>
        {
            await catalog.ReturnAsync(claim, token);
            return Done.Value;
        });
        await AssertRightsAsync(coupon.Id, used: 0, reserved: 0);
        Assert.Equal(CouponOutcome.Accepted, (await EvaluateAsync("tam", Basket)).Outcome);
    }

    [Fact]
    public async Task AnUnpaidOrdersRight_ComesBack_WhenItIsReleased()
    {
        Coupon coupon = await SeedPercentAsync("BEKLE", 10m, limit: 1);

        Assert.True(await ReserveAsync(coupon.Id));
        await RunAsync(async (catalog, token) =>
        {
            await catalog.ReleaseAsync(new StockClaim([], coupon.Id), token);
            return Done.Value;
        });

        await AssertRightsAsync(coupon.Id, used: 0, reserved: 0);
    }

    // ---- KK4: the share freezes on the item ----

    [Fact]
    public async Task TheShareTheOrderItemFroze_StaysWhenTheCouponChanges_AndANewBasketGetsTheNewShare()
    {
        Coupon coupon = await SeedPercentAsync("DON", 10m);
        CouponLine[] basket = [new CouponLine(100m, 1, 20m)];
        CouponApplication applied = await EvaluateAsync("don", basket);
        Guid itemId = await SeedOrderItemAsync(applied);

        await ExecuteAsync($"UPDATE coupons SET percentage = 50, version = version + 1 WHERE id = '{coupon.Id}'");

        Assert.Equal(("10.00", "15.00"), await ItemColumnsAsync(itemId));
        Assert.Equal(50m, (await EvaluateAsync("don", basket)).Items.Single().CouponShare);
    }

    // ---- T526: the panel's change, list and read (07 UC-90, UC-122; KK8-KK10) ----

    [Fact]
    public async Task ChangingAMissingCoupon_IsNotFound_T526_KK10()
    {
        CouponWriteResult result = await UpdateAsync(EntityIds.New(), Definition("YOK"), 1);

        Assert.True(result.IsNotFound);
        Assert.Empty(result.Problems);
    }

    [Fact]
    public async Task AStaleVersion_IsAConflict_BeforeTheFieldRules_AndWritesNothing_T526_KK10()
    {
        Coupon coupon = await SeedPercentAsync("SURUM", 10m);

        CouponWriteResult result = await UpdateAsync(coupon.Id, new CouponDefinition(" ", CouponKind.Percentage, 100m, null, End, Start, -1), 2);

        Assert.True(result.IsVersionConflict);
        Assert.Empty(result.Problems);
        Coupon stored = await ReadCouponAsync(coupon.Id);
        Assert.Equal(("SURUM", 10m, 1), (stored.Code, stored.Percentage!.Value, stored.Version));
    }

    [Fact]
    public async Task EveryFieldIsWritten_TheVersionRises_TheRightsStay_AndNoTrailRowIsWritten_T526_KK9()
    {
        Coupon coupon = await SeedPercentAsync("ESKI", 10m, limit: 5);
        Assert.True(await ReserveAsync(coupon.Id));
        long trailBefore = await ScalarAsync<long>("SELECT count(*) FROM audit_logs");
        DateOnly newStart = new(2026, 11, 5);
        DateOnly newEnd = new(2026, 11, 20);

        CouponWriteResult result = await UpdateAsync(coupon.Id, new CouponDefinition("YENI", CouponKind.Amount, 50m, 200m, newStart, newEnd, 7), 1);

        Assert.False(result.IsRejected);
        Coupon stored = await ReadCouponAsync(coupon.Id);
        Assert.Equal("YENI", stored.Code);
        Assert.Null(stored.Percentage);
        Assert.Equal(50m, stored.Amount);
        Assert.Equal(200m, stored.MinimumBasketAmount);
        Assert.Equal((newStart, newEnd, 7), (stored.StartDate, stored.EndDate, stored.TotalUsageLimit));
        Assert.Equal((0, 1, 2), (stored.UsedCount, stored.ReservedCount, stored.Version));
        Assert.Equal(trailBefore, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
        Assert.Equal(CouponOutcome.Accepted, (await EvaluateAsync("yeni", Basket)).Outcome);
        Assert.Equal(CouponOutcome.Rejected, (await EvaluateAsync("eski", Basket)).Outcome);
    }

    [Fact]
    public async Task TheUsageCount_MayNotGoUnderTheUsedAndReservedRights_ButMayEqualThem_T526_KK10()
    {
        Coupon coupon = await SeedPercentAsync("TABAN", 10m, limit: 5);
        Assert.True(await ReserveAsync(coupon.Id));
        await RunAsync(async (catalog, token) =>
        {
            await catalog.CommitAsync(new StockClaim([], coupon.Id), token);
            return Done.Value;
        });
        Assert.True(await ReserveAsync(coupon.Id));

        CouponWriteResult below = await UpdateAsync(coupon.Id, Definition("TABAN", limit: 1), 1);
        CouponWriteResult equal = await UpdateAsync(coupon.Id, Definition("TABAN", limit: 2), 1);

        Assert.Equal([new CouponProblem(nameof(CouponDefinition.TotalUsageLimit), ErrorCode.CouponUsageBelowUsed)], below.Problems);
        Assert.Equal(2, below.UsedAndReserved);
        Assert.False(equal.IsRejected);
        Assert.Equal((1, 1, 2, 2), await RightsAndLimitAsync(coupon.Id));
    }

    [Fact]
    public async Task TheCode_IsUniqueAgainstOtherCoupons_ButTheCouponItselfIsNotAnother_T526_KK10()
    {
        Coupon own = await SeedPercentAsync("KENDI", 10m);
        await SeedPercentAsync("BASKA", 10m);

        CouponWriteResult same = await UpdateAsync(own.Id, Definition("kendi"), 1);
        CouponWriteResult taken = await UpdateAsync(own.Id, Definition("baska"), 2);

        Assert.False(same.IsRejected);
        Assert.Equal([new CouponProblem(nameof(CouponDefinition.Code), ErrorCode.DuplicateCouponCode)], taken.Problems);
        Assert.Equal("kendi", (await ReadCouponAsync(own.Id)).Code);
    }

    [Fact]
    public async Task MovingTheEndDayEarlier_RefusesANewOrder_AndTheReservedRightsStay_T526_KK9()
    {
        Coupon coupon = await SeedPercentAsync("ERKEN", 10m, limit: 5);
        Assert.True(await ReserveAsync(coupon.Id));
        Assert.Equal(CouponOutcome.Accepted, (await EvaluateAsync("erken", Basket)).Outcome);

        CouponWriteResult result = await UpdateAsync(coupon.Id, Definition("ERKEN", end: new DateOnly(2026, 11, 10), limit: 5), 1);

        Assert.False(result.IsRejected);
        Assert.Equal(CouponOutcome.Rejected, (await EvaluateAsync("erken", Basket)).Outcome);
        Assert.Equal((0, 1, 5, 2), await RightsAndLimitAsync(coupon.Id));
    }

    [Fact]
    public async Task AChangedValue_ReachesOnlyANewBasket_TheOrderItemKeepsItsShare_T526_KK9()
    {
        Coupon coupon = await SeedPercentAsync("DEGIS", 10m);
        CouponLine[] basket = [new CouponLine(100m, 1, 20m)];
        Guid itemId = await SeedOrderItemAsync(await EvaluateAsync("degis", basket));

        CouponWriteResult result = await UpdateAsync(coupon.Id, Definition("DEGIS", value: 50m), 1);

        Assert.False(result.IsRejected);
        Assert.Equal(("10.00", "15.00"), await ItemColumnsAsync(itemId));
        Assert.Equal(50m, (await EvaluateAsync("degis", basket)).Items.Single().CouponShare);
    }

    [Fact]
    public async Task AChangeThatRacesAReservation_WaitsForIt_AndMeasuresTheFloorAfterIt_T526_KK10()
    {
        Coupon coupon = await SeedPercentAsync("YARIS", 10m, limit: 1);

        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope first = provider.CreateAsyncScope();
        await using AsyncServiceScope second = provider.CreateAsyncScope();
        AppDbContext firstDb = first.ServiceProvider.GetRequiredService<AppDbContext>();
        AppDbContext secondDb = second.ServiceProvider.GetRequiredService<AppDbContext>();

        await using IDbContextTransaction firstTransaction = await firstDb.Database.BeginTransactionAsync(Ct);
        Assert.True(await first.ServiceProvider.GetRequiredService<ICatalogModule>().ReserveAsync(new StockClaim([], coupon.Id), null, Ct));
        await using IDbContextTransaction secondTransaction = await secondDb.Database.BeginTransactionAsync(Ct);
        int pid = (await secondDb.Database.SqlQuery<int>($"SELECT pg_backend_pid() AS \"Value\"").ToListAsync(Ct)).Single();
        Task<CouponWriteResult> waiting = second.ServiceProvider.GetRequiredService<ICatalogModule>().UpdateCouponAsync(coupon.Id, Definition("YARIS", limit: 0), 1, Ct);
        await WaitUntilBlockedAsync(pid);

        await firstTransaction.CommitAsync(Ct);
        CouponWriteResult result = await waiting.WaitAsync(TimeSpan.FromSeconds(30), Ct);
        await secondTransaction.RollbackAsync(Ct);

        Assert.Equal([new CouponProblem(nameof(CouponDefinition.TotalUsageLimit), ErrorCode.CouponUsageBelowUsed)], result.Problems);
        Assert.Equal(1, result.UsedAndReserved);
        await AssertRightsAsync(coupon.Id, used: 0, reserved: 1);
    }

    [Fact]
    public async Task TheList_IsNewestFirst_ThenByInnerKey_AndAPagePastTheLastShowsTheLast_T526_KK8()
    {
        for (int i = 0; i < PageSizes.Panel + 1; i++)
        {
            await SeedPercentAsync($"AYNI{i:D2}", 10m);
        }

        await SaveAsync(c => c.Coupons.Add(Coupon.CreatePercentage("ENYENI", 10m, Start, End, 5, Now.AddHours(1))));
        List<Guid> sameTime;
        await using (AppDbContext reader = NewContext())
        {
            sameTime = await reader.Coupons.Where(c => c.Code != "ENYENI").OrderByDescending(c => c.Id).Select(c => c.Id).ToListAsync(Ct);
        }

        CouponListPage firstPage = await ReadAsync(catalog => catalog.ListCouponsAsync(1, Ct));
        CouponListPage pastTheLast = await ReadAsync(catalog => catalog.ListCouponsAsync(int.MaxValue, Ct));

        Assert.Equal((1, 2, PageSizes.Panel), (firstPage.PageNumber, firstPage.PageCount, firstPage.Items.Count));
        Assert.Equal("ENYENI", firstPage.Items[0].Code);
        Assert.Equal(sameTime.Take(PageSizes.Panel - 1), firstPage.Items.Skip(1).Select(r => r.Id));
        Assert.Equal(2, pastTheLast.PageNumber);
        Assert.Equal(sameTime.Skip(PageSizes.Panel - 1), pastTheLast.Items.Select(r => r.Id));
    }

    [Fact]
    public async Task EachRowsValidity_IsReadFromItsDatesAndItsRightsLeft_AndAForeignKeyReadsNothing_T526_KK8()
    {
        await SaveAsync(c => c.Coupons.Add(Coupon.CreatePercentage("GELECEK", 10m, new DateOnly(2026, 12, 1), new DateOnly(2026, 12, 31), 5, Now)));
        await SaveAsync(c => c.Coupons.Add(Coupon.CreatePercentage("GECMIS", 10m, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31), 5, Now)));
        Coupon usedUp = await SeedPercentAsync("BITTI", 10m, limit: 1);
        Assert.True(await ReserveAsync(usedUp.Id));
        Coupon valid = await SeedPercentAsync("GECERLI", 10m, limit: 3);

        CouponListPage page = await ReadAsync(catalog => catalog.ListCouponsAsync(1, Ct));
        CouponRecord? read = await ReadAsync(catalog => catalog.GetCouponAsync(valid.Id, Ct));
        CouponRecord? foreign = await ReadAsync(catalog => catalog.GetCouponAsync(EntityIds.New(), Ct));

        Dictionary<string, CouponValidity> validity = page.Items.ToDictionary(r => r.Code, r => r.Validity);
        Assert.Equal(CouponValidity.NotStarted, validity["GELECEK"]);
        Assert.Equal(CouponValidity.Expired, validity["GECMIS"]);
        Assert.Equal(CouponValidity.RightsUsedUp, validity["BITTI"]);
        Assert.Equal(CouponValidity.Valid, validity["GECERLI"]);
        Assert.NotNull(read);
        Assert.Equal((CouponKind.Percentage, 10m, 3, 0, 0, 1), (read.Kind, read.Value, read.TotalUsageLimit, read.UsedCount, read.ReservedCount, read.Version));
        Assert.Null(foreign);
    }

    private static CouponDefinition Definition(string code, decimal value = 10m, DateOnly? end = null, int limit = 100)
        => new(code, CouponKind.Percentage, value, null, Start, end ?? End, limit);

    private async Task<CouponWriteResult> UpdateAsync(Guid couponId, CouponDefinition definition, int formVersion)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        ICatalogModule catalog = scope.ServiceProvider.GetRequiredService<ICatalogModule>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => catalog.UpdateCouponAsync(couponId, definition, formVersion, token), Ct);
    }

    private async Task<T> ReadAsync<T>(Func<ICatalogModule, Task<T>> read)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await read(scope.ServiceProvider.GetRequiredService<ICatalogModule>());
    }

    private async Task<Coupon> ReadCouponAsync(Guid couponId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Coupons.AsNoTracking().SingleAsync(c => c.Id == couponId, Ct);
    }

    private async Task<(int Used, int Reserved, int Limit, int Version)> RightsAndLimitAsync(Guid couponId)
    {
        Coupon coupon = await ReadCouponAsync(couponId);
        return (coupon.UsedCount, coupon.ReservedCount, coupon.TotalUsageLimit, coupon.Version);
    }

    private async Task<Guid> SeedOrderItemAsync(CouponApplication applied)
    {
        int n = Interlocked.Increment(ref _counter);
        ItemAmounts amounts = applied.Items.Single();
        Province province = Province.Create($"{n:D2}", $"İl {n}", Now);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Now, "metin", Now);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Now, "metin", Now);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Now, "metin", Now).WithHosting();
        Order order = Order.Create(
            $"2026-{n:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", PaymentMethod.Card, true, "Ali@Example.com", applied.PayableTotal, 0m, Now.AddHours(1), Now);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.CouponId = applied.CouponId;
        order.CouponCode = applied.Code;
        order.BillingRecipientName = "Ali Veli";
        order.BillingProvinceId = province.Id;
        order.BillingDistrict = "Çankaya";
        order.BillingAddressLine = "Atatürk Bulvarı 1";
        order.CompanyType = CompanyType.Tradesman;
        order.CompanyLegalName = "Örnek Ticaret";
        order.CompanyTaxIdNumber = "1234567890";
        order.CompanyProvinceId = province.Id;
        order.CompanyDistrict = "Çankaya";
        order.CompanyAddressLine = "Kızılay 2";
        order.CompanyPhone = "3121234567";
        order.CompanyEmail = "firma@example.com";
        order.CompanyKepAddress = "firma@hs01.kep.tr";
        order.PreInformationFormVersionId = form.Id;
        order.DistanceSalesContractVersionId = contract.Id;
        order.PrivacyNoticeVersionId = notice.Id;
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        OrderItem item = OrderItem.Create(order.Id, null, null, ProductType.Physical, "Ürün", 1, 100m, 20m, amounts.Vat, Now);
        item.CouponShare = amounts.CouponShare;
        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.LegalTextVersions.AddRange(form, contract, notice);
            c.Orders.Add(order);
            c.OrderItems.Add(item);
        });
        return item.Id;
    }

    private async Task<(string Share, string Vat)> ItemColumnsAsync(Guid itemId)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new($"SELECT coupon_share::text, vat_amount::text FROM order_items WHERE id = '{itemId}'", connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct);
        Assert.True(await reader.ReadAsync(Ct));
        return (reader.GetString(0), reader.GetString(1));
    }

    private async Task<Coupon> SeedPercentAsync(string code, decimal percentage, int limit = 100)
    {
        Coupon coupon = Coupon.CreatePercentage(code, percentage, Start, End, limit, Now);
        await SaveAsync(c => c.Coupons.Add(coupon));
        return coupon;
    }

    private async Task<CouponWriteResult> CreateAsync(CouponDefinition definition)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        ICatalogModule catalog = scope.ServiceProvider.GetRequiredService<ICatalogModule>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => catalog.CreateCouponAsync(definition, token), Ct);
    }

    private async Task<CouponApplication> EvaluateAsync(string code, IReadOnlyList<CouponLine> lines)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ICatalogModule>().EvaluateCouponAsync(code, lines, Ct);
    }

    private async Task<CouponApplication> ApplyAsync(Guid cartId, string code, IReadOnlyList<CouponLine> lines)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ICatalogModule>().ApplyCouponAsync(cartId, code, lines, Ct);
    }

    private async Task<bool> ReserveAsync(Guid couponId)
    {
        bool reserved = false;
        await RunAsync(async (catalog, token) =>
        {
            reserved = await catalog.ReserveAsync(new StockClaim([], couponId), null, token);
            return Done.Value;
        });
        return reserved;
    }

    private async Task RunAsync(Func<ICatalogModule, CancellationToken, Task<Done>> work)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        ICatalogModule catalog = scope.ServiceProvider.GetRequiredService<ICatalogModule>();
        await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(catalog, token), Ct);
    }

    private async Task AssertRightsAsync(Guid couponId, int used, int reserved)
    {
        await using AppDbContext reader = NewContext();
        Coupon coupon = await reader.Coupons.SingleAsync(c => c.Id == couponId, Ct);
        Assert.Equal((used, reserved, 1), (coupon.UsedCount, coupon.ReservedCount, coupon.Version));
    }

    /// <summary>Waits until the backend waits on a lock (pg_stat_activity), at most about ten seconds.</summary>
    private async Task WaitUntilBlockedAsync(int pid)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        for (int attempt = 0; attempt < 400; attempt++)
        {
            await using NpgsqlCommand command = new("SELECT wait_event_type FROM pg_stat_activity WHERE pid = @pid", connection);
            command.Parameters.AddWithValue("pid", pid);
            if (await command.ExecuteScalarAsync(Ct) is string type && type == "Lock")
            {
                return;
            }

            await Task.Delay(25, Ct);
        }

        throw new Xunit.Sdk.XunitException($"Backend {pid} never waited on a lock.");
    }

    private ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<AdvisoryLocks>();
        services.AddSingleton<TimeProvider>(_time);
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddFileStore();
        services.AddOptions<Shopfolio.Infrastructure.InstallationSettings.StatusThresholdOptions>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.IDiskUsageReader, Shopfolio.Infrastructure.Status.DriveDiskUsageReader>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.DiskUsageGuard>();
        services.AddSingleton<ImageProcessor>();
        services.AddCatalogModule();
        services.AddPaymentModule();
        services.AddOrderingModule();
        return services.BuildServiceProvider();
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

    /// <summary>A written result of a test transaction.</summary>
    private sealed class Done : IOperationResult
    {
        public static readonly Done Value = new();

        public bool IsRejected => false;
    }
}
