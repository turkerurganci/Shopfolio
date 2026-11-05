using System.Net;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Jobs;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Outages;

/// <summary>
/// The payment provider's outage at the final query, on the real job runner and a real PostgreSQL (T204 criteria 3, 7, 8; KK3, KK7, KK8;
/// 08 3.1 rules 3 and 4, 3.3; 05 5.3.3): the fake provider does not answer - timeout, unreachable, unlisted answer - so the result is unknown;
/// the core's rule asks again five minutes later and touches no order: the order row stays as it was, byte for byte, also after its payment
/// period ended on the fake clock - the period runs, only its cancellation waits. The outage writes no dispatch row. The handler that reads the
/// order and applies or cancels is the payment provider task's (T403); a test-local handler calls the fake and the rule as it will.
/// </summary>
[Collection(OutagesDefinition.Name)]
public sealed class FinalPaymentQueryOutageTests : JobQueueTestBase
{
    private const string OrderNumber = "2026-000204";

    private static readonly TimeSpan PaymentPeriod = TimeSpan.FromMinutes(10);

    public FinalPaymentQueryOutageTests(CollectionDatabaseFixture collection)
        : base(collection)
    {
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await ExecuteAsync("TRUNCATE payment_records, orders CASCADE");
    }

    public override async ValueTask DisposeAsync()
    {
        await ExecuteAsync("TRUNCATE jobs, dispatch_records, payment_records, orders CASCADE");
        await base.DisposeAsync();
    }

    [Theory]
    [InlineData(ExternalCallFailureKind.TimedOut)]
    [InlineData(ExternalCallFailureKind.Unreachable)]
    [InlineData(ExternalCallFailureKind.Unrecognized)]
    public async Task UnansweredFinalQuery_IsAskedAgainEveryFiveMinutes_PastThePaymentPeriod_AndTheOrderRowNeverChanges(ExternalCallFailureKind kind)
    {
        await using JobQueueHarness harness = NewHarness([JobType.FinalPaymentQuery]);
        FakePaymentGateway provider = new() { Outage = kind };
        List<FinalPaymentQueryStep> steps = [];
        harness.Script.ByType[JobType.FinalPaymentQuery] = async call =>
        {
            PaymentQueryResult answer = await provider.QueryPaymentAsync(OrderNumber, call.Ct);
            FinalPaymentQueryStep step = FinalPaymentQueryRule.Decide(answer);
            steps.Add(step);
            return step == FinalPaymentQueryStep.AskAgain ? FinalPaymentQueryRule.AskAgainOutcome() : JobOutcome.Completed;
        };
        Guid order = await SeedOrderAsync(JobQueueHarness.Start + PaymentPeriod);
        await harness.EnqueueAsync(JobType.FinalPaymentQuery, order);
        string before = await OrderTextAsync(order);

        // Four rounds: before the period's end, at it, and twice after it - the clock runs on through the outage.
        for (int round = 1; round <= 4; round++)
        {
            Assert.True(await harness.RunOneAsync(Ct));

            Job row = Assert.Single(await harness.RowsAsync());
            Assert.Equal(JobState.Pending, row.JobState);
            Assert.Equal(harness.Time.GetUtcNow() + JobTimings.FinalPaymentQueryInterval, row.NextAttemptAt);
            Assert.Equal(0, row.AttemptCount);
            Assert.Equal(before, await OrderTextAsync(order));
            Assert.False(await harness.RunOneAsync(Ct));

            harness.Time.Advance(JobTimings.FinalPaymentQueryInterval);
        }

        Assert.True(harness.Time.GetUtcNow() > JobQueueHarness.Start + PaymentPeriod);
        Assert.Equal([FinalPaymentQueryStep.AskAgain, FinalPaymentQueryStep.AskAgain, FinalPaymentQueryStep.AskAgain, FinalPaymentQueryStep.AskAgain], steps);
        Assert.Equal(4, provider.Calls.Count);

        await using AppDbContext db = NewContext();
        Order unchanged = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == order, Ct);
        Assert.Equal(OrderStatus.Placed, unchanged.OrderStatus);
        Assert.Equal(PaymentStatus.Pending, unchanged.PaymentStatus);
        Assert.Equal(1, unchanged.Version);
        Assert.Equal(JobQueueHarness.Start + PaymentPeriod, unchanged.PaymentWindowEndsAt);

        // KK8: no notice to the customer about the outage.
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records"));
    }

    // A real card order waiting for its payment (Placed + Pending), as the confirmation leaves it.
    private async Task<Guid> SeedOrderAsync(DateTimeOffset paymentWindowEndsAt)
    {
        DateTimeOffset now = JobQueueHarness.Start;
        await using AppDbContext db = NewContext();
        Province? province = await db.Provinces.SingleOrDefaultAsync(p => p.Code == "06", Ct);
        if (province is null)
        {
            province = Province.Create("06", "Ankara", now);
            db.Provinces.Add(province);
        }

        Guid VersionOf(LegalTextType type, bool hosting)
        {
            LegalTextVersion? existing = db.LegalTextVersions.SingleOrDefault(v => v.LegalTextType == type);
            if (existing is not null)
            {
                return existing.Id;
            }

            LegalTextVersion created = LegalTextVersion.CreatePublished(type, 1, now, "metin", now);
            db.LegalTextVersions.Add(created);
            return (hosting ? created.WithHosting() : created).Id;
        }

        Guid form = VersionOf(LegalTextType.PreInformationForm, false);
        Guid contract = VersionOf(LegalTextType.DistanceSalesContract, false);
        Guid notice = VersionOf(LegalTextType.PrivacyNotice, true);
        await db.SaveChangesAsync(Ct);

        Order order = Order.Create(OrderNumber, "access-placeholder-204", "confirm-placeholder-204", PaymentMethod.Card, true, "Ali@Example.com", 120m, 0m, paymentWindowEndsAt, now);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
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
        order.PreInformationFormVersionId = form;
        order.DistanceSalesContractVersionId = contract;
        order.PrivacyNoticeVersionId = notice;
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        db.Orders.Add(order);
        await db.SaveChangesAsync(Ct);
        return order.Id;
    }

    private async Task<string> OrderTextAsync(Guid id)
    {
        await using NpgsqlConnection connection = new(Collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new("SELECT row_to_json(o)::text FROM orders o WHERE o.id = @id", connection);
        command.Parameters.AddWithValue("id", id);
        return (string)(await command.ExecuteScalarAsync(Ct))!;
    }
}
