using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the state by role page tests (T547): one database copied from the template.</summary>
[CollectionDefinition(StateRolePageTestsDefinition.Name)]
public sealed class StateRolePageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "StateRolePageTests";
}

/// <summary>
/// The order page's state by role rows on the opened application (T547 criterion 2, KK2; 04 6.1.4, 6.2.16.1-10, 6.3.9.1-10; 09 19.2; K-1356): the shipping and payment pairs are read from the state cell of the
/// document's rows, an order is seeded for each pair, and the customer's page and the panel's detail are fetched. What the page offers is exactly what <see cref="IOrderOperations"/> says is open
/// (the table of 03 1.11); an operation outside the range has no control on the page at all - not a disabled one (04 6.1.4).
/// </summary>
[Collection(StateRolePageTestsDefinition.Name)]
public sealed partial class StateRolePageTests
{
    private const string PanelOrdersPath = "/panel/siparisler";

    /// <summary>The customer page's operation link of each operation: the segment under the order's root that the button opens (04 5.16.6).</summary>
    private static readonly Dictionary<OrderOperation, string> CustomerLinks = new()
    {
        [OrderOperation.CancelOrder] = "/iptal",
        [OrderOperation.CancelPhysicalItem] = "/iptal",
        [OrderOperation.CancelServiceItem] = "/iptal",
        [OrderOperation.CancelDigitalItem] = "/iptal",
        [OrderOperation.TerminateForLateDelivery] = "/gecikme-feshi",
        [OrderOperation.WithdrawPhysicalItem] = "/cayma",
        [OrderOperation.WithdrawServiceItem] = "/cayma",
        [OrderOperation.WithdrawDigitalItem] = "/cayma",
        [OrderOperation.OpenDefectClaim] = "/ayip-talebi",
        [OrderOperation.ReopenDefectClaimByCustomer] = "/ayip-talebi",
    };

    /// <summary>The panel detail's form of each operation it draws; an operation that opens one of several forms lists them all (the page draws the form for the part that exists).</summary>
    private static readonly Dictionary<OrderOperation, string[]> PanelForms = new()
    {
        [OrderOperation.MarkTransferPaid] = ["mark-paid-form"],
        [OrderOperation.ShipOrder] = ["mark-shipped-form"],
        [OrderOperation.MarkDelivered] = ["mark-delivered-form"],
        [OrderOperation.MarkDeliveryFailed] = ["mark-delivery-failed-form"],
        [OrderOperation.ResendOrder] = ["mark-reshipped-form"],
        [OrderOperation.CloseOrder] = ["close-delivery-failed-form"],
        [OrderOperation.CancelByFirm] = ["cancel-order-by-company-form"],
        [OrderOperation.RecordWithdrawalFromOtherChannel] = ["record-withdrawal-form"],
        [OrderOperation.RecordLateTerminationFromOtherChannel] = ["record-late-termination-form"],
        [OrderOperation.RemoveOrReduceItem] = ["remove-item-form-"],
        [OrderOperation.ReceiveReturnedGoods] = ["receive-return-form-"],
        [OrderOperation.RejectReturnedGoods] = ["receive-return-form-"],
        [OrderOperation.CloseAsNotReturned] = ["close-not-returned-form-"],
        [OrderOperation.MarkServiceCompleted] = ["complete-service-"],
        [OrderOperation.RenewDownloadAllowance] = ["reset-download-"],
        [OrderOperation.WriteInternalNote] = ["internal-note-form"],
        [OrderOperation.CorrectAddress] = [OrderDetailModel.ShippingAddressFormId, OrderDetailModel.BillingAddressFormId],
        [OrderOperation.CorrectCarrier] = [OrderDetailModel.CarrierFormId],
        [OrderOperation.CorrectDeliveryOrArrivalDate] = [OrderDetailModel.DeliveryDateFormId, "correct-arrival-date-form"],
        [OrderOperation.CorrectTransition] = [OrderDetailModel.TransitionFormId],
        [OrderOperation.CorrectGuestEmail] = [OrderDetailModel.OrderEmailFormId],
    };

    private readonly CollectionDatabaseFixture _database;

    public StateRolePageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    /// <summary>One state row of the document: the row id, the pairs its state cell names and how the order's items are seeded.</summary>
    private sealed record StateCase(string Row, OrderStatus Shipping, PaymentStatus Payment, bool Card, ItemScenario Items);

    private enum ItemScenario
    {
        OpenPhysical,
        ServiceOnly,
        ClosedPhysical,
    }

    [Fact]
    public async Task TheCustomersPage_OffersExactlyTheOpenOperations_InEveryPairOfTheRows_KK2()
    {
        StateCase[] cases = Cases("6.2.16");
        Assert.NotEmpty(cases);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        List<string> problems = [];
        int offered = 0;

        foreach (StateCase state in cases)
        {
            (Guid orderId, string accessKey) = await SeedAsync(state);
            IReadOnlyList<OrderOperationEntry> open = await OpenAsync(factory, orderId, ActorKind.Buyer);
            using HttpResponseMessage response = await client.GetAsync($"/siparis/erisim/{accessKey}", Ct);
            string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));
            if (response.StatusCode != HttpStatusCode.OK)
            {
                problems.Add($"{state.Row} {state.Shipping}+{state.Payment}: the page answered {(int)response.StatusCode}.");
                continue;
            }

            string area = OperationsArea().Match(html) is { Success: true } found ? found.Groups[1].Value : string.Empty;
            string[] links = [.. OperationLink().Matches(area).Select(m => m.Groups[1].Value)];
            string[] expected = [.. open.Where(e => CustomerLinks.ContainsKey(e.Operation)).Select(e => CustomerLinks[e.Operation]).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];
            string[] shown = [.. CustomerLinks.Values.Distinct(StringComparer.Ordinal).Where(s => links.Any(l => l.EndsWith(s, StringComparison.Ordinal))).Order(StringComparer.Ordinal)];
            offered += shown.Length;
            if (!expected.SequenceEqual(shown))
            {
                problems.Add($"{state.Row} {state.Shipping}+{state.Payment}: the page links [{string.Join(", ", shown)}], the open operations ask for [{string.Join(", ", expected)}].");
            }

            if (area.Contains("disabled", StringComparison.OrdinalIgnoreCase) || area.Contains("aria-disabled", StringComparison.OrdinalIgnoreCase))
            {
                problems.Add($"{state.Row} {state.Shipping}+{state.Payment}: the operations area has a passive control (04 6.1.4).");
            }

            bool transferOpen = open.Any(e => e.Operation == OrderOperation.ViewTransferDetails);
            if (transferOpen != html.Contains("TR00 0000 0000 0000 0000 0000 00", StringComparison.Ordinal) && !state.Card)
            {
                problems.Add($"{state.Row} {state.Shipping}+{state.Payment}: the transfer details are {(transferOpen ? "open but not shown" : "shown but not open")}.");
            }
        }

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
        Assert.True(offered > 0, "No control was drawn in any pair: the test would pass on an empty page.");
    }

    [Fact]
    public async Task ThePanelsDetail_DrawsExactlyTheFormsOfTheOpenOperations_InEveryPairOfTheRows_KK2()
    {
        StateCase[] cases = Cases("6.3.9");
        Assert.NotEmpty(cases);
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        List<string> problems = [];
        int offered = 0;

        foreach (StateCase state in cases)
        {
            (Guid orderId, _) = await SeedAsync(state);
            IReadOnlyList<OrderOperationEntry> open = await OpenAsync(factory, orderId, ActorKind.Admin);
            using HttpRequestMessage request = new(HttpMethod.Get, $"{PanelOrdersPath}/{orderId:D}");
            request.Headers.Add("Cookie", $"{CookieNames.AdminSession}={key}");
            using HttpResponseMessage response = await client.SendAsync(request, Ct);
            string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));
            if (response.StatusCode != HttpStatusCode.OK)
            {
                problems.Add($"{state.Row} {state.Shipping}+{state.Payment}: the page answered {(int)response.StatusCode}.");
                continue;
            }

            foreach ((OrderOperation operation, string[] forms) in PanelForms)
            {
                bool isOpen = open.Any(e => e.Operation == operation);
                bool drawn = forms.Any(f => html.Contains($"id=\"{f}", StringComparison.Ordinal));
                offered += drawn ? 1 : 0;
                if (!isOpen && drawn)
                {
                    problems.Add($"{state.Row} {state.Shipping}+{state.Payment}: {operation} is outside its range but the page draws [{string.Join(", ", forms)}].");
                }

                if (isOpen && !drawn && Always.Contains(operation))
                {
                    problems.Add($"{state.Row} {state.Shipping}+{state.Payment}: {operation} is open but the page draws none of [{string.Join(", ", forms)}].");
                }
            }

            if (Disabled().IsMatch(html))
            {
                problems.Add($"{state.Row} {state.Shipping}+{state.Payment}: the page has a passive control (04 6.1.4).");
            }
        }

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
        Assert.True(offered > 0, "No control was drawn in any pair: the test would pass on an empty page.");
    }

    /// <summary>
    /// The operations the page draws whenever the table opens them, in a seed of one item and no record: the ones that need a record (a delivery to correct, a transition to undo, a part of the order to
    /// correct) are drawn only when that part exists, so for them the test asserts only the other direction - outside the range nothing is drawn.
    /// </summary>
    private static readonly HashSet<OrderOperation> Always =
    [
        OrderOperation.MarkTransferPaid,
        OrderOperation.ShipOrder,
        OrderOperation.MarkDelivered,
        OrderOperation.MarkDeliveryFailed,
        OrderOperation.ResendOrder,
        OrderOperation.CloseOrder,
        OrderOperation.CancelByFirm,
        OrderOperation.RecordWithdrawalFromOtherChannel,
        OrderOperation.WriteInternalNote,
    ];

    /// <summary>The cases of a block: each row's state cell gives its pairs; an item scenario follows the words of the cell.</summary>
    private static StateCase[] Cases(string block)
    {
        string document = File.ReadAllText(Path.Combine(RepositoryRoot(), "Docs", "04_UI_SPECS.md"));
        List<StateCase> cases = [];
        foreach (Match line in StateRow().Matches(document).Where(m => m.Groups[1].Value.StartsWith(block + ".", StringComparison.Ordinal) && int.Parse(m.Groups[1].Value[(block.Length + 1)..], System.Globalization.CultureInfo.InvariantCulture) <= 10))
        {
            string row = line.Groups[1].Value;
            string state = line.Groups[2].Value;
            string pairs = state.Contains(" — ", StringComparison.Ordinal) ? state[..state.IndexOf(" — ", StringComparison.Ordinal)] : state;
            ItemScenario items = state.Contains("fiziksel açık kalemi olmayan", StringComparison.Ordinal) ? ItemScenario.ServiceOnly
                : state.Contains("açık kalem yok", StringComparison.Ordinal) ? ItemScenario.ClosedPhysical
                : ItemScenario.OpenPhysical;
            string? shipping = null;
            foreach (string token in pairs.Split(" · ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                string paymentName = token;
                int plus = token.IndexOf(" + ", StringComparison.Ordinal);
                if (plus >= 0)
                {
                    shipping = token[..plus];
                    paymentName = token[(plus + 3)..];
                }

                Assert.NotNull(shipping);
                OrderStatus order = Enum.GetValues<OrderStatus>().Single(s => StatusBadges.For(s).Text == shipping);
                PaymentStatus payment = Enum.GetValues<PaymentStatus>().Single(s => StatusBadges.For(s).Text == paymentName);
                cases.Add(new StateCase(row, order, payment, state.Contains("kart", StringComparison.Ordinal) && payment == PaymentStatus.Pending, items));
            }
        }

        return [.. cases];
    }

    private async Task<(Guid OrderId, string AccessKey)> SeedAsync(StateCase state)
    {
        (Guid orderId, _, string accessKey) = await TestSessions.OrderAsync(_database.Dsn, null);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        await using AppDbContext db = NewContext();
        OrderItem item = state.Items == ItemScenario.ServiceOnly
            ? OrderItem.Create(orderId, null, null, ProductType.Service, "Danışmanlık", 1, 100m, 20m, 16.67m, now)
            : OrderItem.Create(orderId, null, null, ProductType.Physical, "Kapı", 2, 50m, 20m, 16.67m, now);
        if (state.Items == ItemScenario.ServiceOnly)
        {
            item.FulfilmentPeriod = 5;
        }

        db.OrderItems.Add(item);
        if (state.Items == ItemScenario.ClosedPhysical)
        {
            db.Cancellations.Add(Cancellation.ByBuyer(item.Id, 2, new ItemAmounts(100m, 0m, 16.67m), null, now));
        }

        await db.SaveChangesAsync(Ct);
        string shipping = state.Shipping.ToString();
        string payment = state.Payment.ToString();
        await db.Database.ExecuteSqlAsync($"UPDATE orders SET order_status = {shipping}, payment_status = {payment} WHERE id = {orderId}", Ct);
        if (state.Card)
        {
            await db.Database.ExecuteSqlAsync($"UPDATE orders SET payment_method = 'Card', bank_transfer_iban = NULL WHERE id = {orderId}", Ct);
        }

        return (orderId, accessKey);
    }

    private static async Task<IReadOnlyList<OrderOperationEntry>> OpenAsync(WebApplicationFactory<Program> factory, Guid orderId, ActorKind actor)
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        OrderOperationsView? view = await scope.ServiceProvider.GetRequiredService<IOrderOperations>().ReadAsync(orderId, actor, Ct);
        Assert.NotNull(view);
        return view.Open;
    }

    private static string RepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Docs", "04_UI_SPECS.md")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("The repository root (Docs/04_UI_SPECS.md) was not found above the test's folder.");
    }

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn, withJobRunner: true).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(OrdersModel).Assembly)));

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }

    [GeneratedRegex(@"^\| (6\.[23]\.\d+\.\d+) \| [^|]+ \| ([^|]+) \|", RegexOptions.CultureInvariant | RegexOptions.Multiline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex StateRow();

    [GeneratedRegex(@"<ul class=""order-operations"">(.*?)</ul>", RegexOptions.CultureInvariant | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex OperationsArea();

    [GeneratedRegex(@"<a [^>]*href=""[^""]*(/iptal|/gecikme-feshi|/cayma|/ayip-talebi)""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex OperationLink();

    [GeneratedRegex(@"<(button|input|a)\b[^>]*\bdisabled\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Disabled();
}
