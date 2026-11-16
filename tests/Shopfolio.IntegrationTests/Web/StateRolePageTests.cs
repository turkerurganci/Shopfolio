using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
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
/// (the table of 03 1.11); an operation outside the range has no control on the page at all - not a disabled one (04 6.1.4). Each pair is seeded three times (<see cref="Records"/>): with no record, with the records that open the refund,
/// defect claim, undelivered e-mail and download steps, and with an open request for the refund IBAN, so that the operations that need a record are compared too.
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
        [OrderOperation.ProcessRefund] = ["amount-based-refund-form"],
        [OrderOperation.RequestRefundIban] = ["request-refund-iban-form"],
        [OrderOperation.ReportTransferRefundFailed] = [Handler + "MarkBankTransferRefundFailed"],
        [OrderOperation.RetryOrRedirectCardRefund] = [Handler + "RetryCardRefund", Handler + "MarkCardRefundFailed"],
        [OrderOperation.ResendEmail] = [Handler + "ResendNotification"],
        [OrderOperation.ResolveOrReopenDefectClaim] = ["defect-claim-form-"],
    };

    /// <summary>A marker that starts with this names the handler a form posts to (the form has no id of its own); any other marker is a form id prefix.</summary>
    private const string Handler = "handler=";

    /// <summary>The records a pair's order carries besides its items: none; the ones that open the order-level and claim steps; an open request for the customer's IBAN.</summary>
    private enum Records
    {
        None,
        Full,
        IbanRequest,

        /// <summary>The records that open the delivery, return and transition steps: a delivered item with a conditional exemption and a withdrawal whose goods are late, the shipment's own transition record.</summary>
        Returns,

        /// <summary>An order old enough for the late delivery termination (Z-11) with its physical item not delivered (03 1.11.8, 1.11.41).</summary>
        Aged,
    }

    private readonly CollectionDatabaseFixture _database;

    public StateRolePageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    /// <summary>One state row of the document: the row id, the pairs its state cell names and how the order's items are seeded.</summary>
    private sealed record StateCase(string Row, OrderStatus Shipping, PaymentStatus Payment, bool Card, ItemScenario Items, Records Records = Records.None);

    private enum ItemScenario
    {
        OpenPhysical,
        ServiceOnly,
        ClosedPhysical,
    }

    [Fact]
    public async Task TheCustomersPage_OffersExactlyTheOpenOperations_InEveryPairOfTheRows_KK2()
    {
        StateCase[] cases = WithRecords(Cases("6.2.16"));
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

            // The forms that stand on the page's body, not in the operations list: the refund IBAN's form (the customer enters or corrects it) and the digital file's download button.
            bool ibanOpen = open.Any(e => e.Operation is OrderOperation.EnterRefundIban or OrderOperation.CorrectRefundIban);
            if (ibanOpen != Drawn(html, Handler + "SaveRefundIban"))
            {
                problems.Add($"{state.Row} {state.Shipping}+{state.Payment}: the refund IBAN's form is {(ibanOpen ? "open but not drawn" : "drawn but not open")}.");
            }

            bool downloadOpen = open.Any(e => e.Operation == OrderOperation.DownloadDigitalFile);
            if (downloadOpen != Drawn(html, Handler + "DownloadDigitalFile"))
            {
                problems.Add($"{state.Row} {state.Shipping}+{state.Payment}: the download button is {(downloadOpen ? "open but not drawn" : "drawn but not open")}.");
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
        StateCase[] cases = WithRecords(Cases("6.3.9"));
        Assert.NotEmpty(cases);
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        List<string> problems = [];
        int offered = 0;
        Dictionary<OrderOperation, int> opened = [];
        Dictionary<OrderOperation, int> drawnWhenOpen = [];

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
                // Two operations may share one form (receiving and rejecting returned goods are chosen in the same form): it is in its range when either is open.
                bool isOpen = PanelForms.Where(p => p.Value.Intersect(forms, StringComparer.Ordinal).Any()).Any(p => open.Any(e => e.Operation == p.Key));
                bool drawn = forms.Any(f => Drawn(html, f));
                offered += drawn ? 1 : 0;
                if (!isOpen && drawn)
                {
                    problems.Add($"{state.Row} {state.Shipping}+{state.Payment}: {operation} is outside its range but the page draws [{string.Join(", ", forms)}].");
                }

                // Both directions for every operation the panel draws (04 6.3.9.11-26): open in the pair and not drawn is as wrong as drawn outside the range.
                if (isOpen && !drawn)
                {
                    problems.Add($"{state.Row} {state.Shipping}+{state.Payment}: {operation} is open but the page draws none of [{string.Join(", ", forms)}].");
                }

                if (open.Any(e => e.Operation == operation))
                {
                    opened[operation] = opened.GetValueOrDefault(operation) + 1;
                    drawnWhenOpen[operation] = drawnWhenOpen.GetValueOrDefault(operation) + (drawn ? 1 : 0);
                }
            }

            if (Disabled().IsMatch(html))
            {
                problems.Add($"{state.Row} {state.Shipping}+{state.Payment}: the page has a passive control (04 6.1.4).");
            }
        }

        // No silent pass: an operation the seeds never open, or never see drawn when open, is a seed that cannot reach it - the test says so instead of passing (T547 validation B5).
        foreach (OrderOperation operation in PanelForms.Keys)
        {
            if (opened.GetValueOrDefault(operation) == 0)
            {
                problems.Add($"{operation} was never open in any pair of the rows: the seeds cannot reach it, so its drawing is not checked.");
            }
            else if (drawnWhenOpen.GetValueOrDefault(operation) == 0)
            {
                problems.Add($"{operation} was open in {opened[operation]} cases and never drawn.");
            }
        }

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
        Assert.True(offered > 0, "No control was drawn in any pair: the test would pass on an empty page.");
    }

    /// <summary>Each pair three times: with no record, with the records that open the order-level steps, and with an open request for the customer's IBAN.</summary>
    private static StateCase[] WithRecords(StateCase[] cases) =>
        [.. cases.SelectMany(c => Enum.GetValues<Records>().Select(r => c with { Row = $"{c.Row} [{r}]", Records = r }))];

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

    private static bool Drawn(string html, string marker) =>
        marker.StartsWith(Handler, StringComparison.Ordinal)
            ? html.Contains($"?{marker}\"", StringComparison.Ordinal)
            : html.Contains($"id=\"{marker}", StringComparison.Ordinal);

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

        if (state.Records == Records.Full)
        {
            // The records that open the order-level steps: the IBAN on file (the transfer's refund step, the card refund turned to the transfer), an open defect claim with its remedy refund, an e-mail
            // that did not arrive, a digital item with its download allowance - and on the card line a card refund whose request failed (the retry or the switch to the transfer).
            if (!state.Card)
            {
                db.RefundIbans.Add(RefundIban.Create(orderId, "TR330006100519786457841326", now));
            }

            DefectClaim claim = DefectClaim.Create(item.Id, 1, "Kusurlu", null, now);
            Refund remedy = Refund.BySystem(orderId, RefundType.DefectClaimRescission, state.Card ? PaymentMethod.Card : PaymentMethod.BankTransfer, 50m, now);
            remedy.DefectClaimId = claim.Id;
            remedy.OrderItemId = item.Id;
            remedy.Quantity = 1;
            remedy.CouponShare = 0m;
            remedy.VatAmount = 8.33m;
            db.DefectClaims.Add(claim);
            db.Refunds.Add(remedy);
            if (state.Card)
            {
                RefundRecord record = RefundRecord.Sending(remedy.Id, now);
                record.State = RefundRecordState.Failed;
                record.OutcomeAt = now;
                db.RefundRecords.Add(record);
            }

            OrderItem digital = OrderItem.Create(orderId, null, null, ProductType.Digital, "Kitap", 1, 30m, 20m, 5m, now);
            digital.DownloadAllowance = 3;
            db.OrderItems.Add(digital);
            DispatchRecord mail = DispatchRecord.Queue(DispatchType.B4, "alici@example.com", orderId, null, null, now.AddHours(-2));
            mail.DispatchState = DispatchState.Undelivered;
            mail.LastOutcome = DispatchOutcome.RecipientError;
            mail.AttemptCount = 3;
            db.DispatchRecords.Add(mail);
        }

        if (state.Records == Records.IbanRequest)
        {
            (_, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
            Guid? refundId = null;
            if (state.Card)
            {
                // On the card line the request exists only for a card refund that did not come through (04 2.12.3.1): it names that refund.
                DefectClaim claim = DefectClaim.Create(item.Id, 1, "Kusurlu", null, now);
                Refund failed = Refund.BySystem(orderId, RefundType.DefectClaimRescission, PaymentMethod.Card, 50m, now);
                failed.DefectClaimId = claim.Id;
                failed.OrderItemId = item.Id;
                failed.Quantity = 1;
                failed.CouponShare = 0m;
                failed.VatAmount = 8.33m;
                db.DefectClaims.Add(claim);
                db.Refunds.Add(failed);
                refundId = failed.Id;
            }

            db.RefundIbanRequests.Add(RefundIbanRequest.ForItem(item.Id, state.Card ? RefundIbanRequestReason.CardRefundFailed : RefundIbanRequestReason.AdminRequest, refundId, adminId, "Yönetici", now));
        }

        if (state.Records == Records.Returns)
        {
            // Opens the date and return steps (03 1.11.25, 1.11.28-1.11.30): the item was delivered 40 days ago, its exemption holds while the packaging is unopened (reject), a withdrawal of one unit was
            // declared 35 days ago and nothing came back (receive, reject, close as not returned), and the shipment is on file.
            if (state.Items == ItemScenario.OpenPhysical)
            {
                DateOnly today = DateOnly.FromDateTime(now.UtcDateTime);
                item.DeliveryDate = today.AddDays(-40);
                item.DeliveryMarkedAt = now.AddDays(-40);
                item.WithdrawalExemptionReason = WithdrawalExemptionReason.HygieneSensitive;
                db.Withdrawals.Add(Withdrawal.ByBuyer(item.Id, 1, today.AddDays(-35), null, new ItemAmounts(50m, 0m, 8.33m), null, now.AddDays(-35)));
            }

            (_, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
            TransitionCode[] shipmentCodes = [TransitionCode.S3, TransitionCode.S4, TransitionCode.S5, TransitionCode.S6, TransitionCode.S7, TransitionCode.S8, TransitionCode.S9];
            foreach (TransitionCode code in shipmentCodes.Where(c => TransitionTable.FindShipment(c)?.To == state.Shipping).Take(1))
            {
                db.TransitionRecords.Add(TransitionRecord.Create(orderId, code, ActorKind.Admin, adminId, "Yönetici", null, null, null, null, null, now));
            }
        }

        await db.SaveChangesAsync(Ct);
        if (state.Records == Records.Aged)
        {
            await db.Database.ExecuteSqlAsync($"UPDATE orders SET created_at = {now.AddDays(-120)} WHERE id = {orderId}", Ct);
        }

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
