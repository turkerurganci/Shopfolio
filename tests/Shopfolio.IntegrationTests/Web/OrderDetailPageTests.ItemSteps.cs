using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Panel;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// E-37's item steps through the page (T528 düzeltme 1; 04 9.9.19, 9.9.20, 7.2.10; 07 UC-129, UC-130; K-798, K-1547, K-1563): the legal interest warning in the firm's cancellation form
/// once Z-10 ran out and in the termination record's form, the termination's listed items and its IBAN field with the sent values kept on a refusal, the withdrawal record on many items
/// with their units, and the warnings' fixed texts in their order with the box's text.
/// </summary>
public sealed partial class OrderDetailPageTests
{
    private const string WrongChecksumIban = "TR000006100519786457841326";

    [Fact]
    public void TheItemStepsWarnings_AreTheFixedTextsOfK798_LetterForLetter_T528_KK1_KK5()
    {
        // K-1514 (5), 12 12.13 U2, U3a, U3b, U3c, U6a: the letter of the text is the test's.
        Assert.Equal("gecikme feshi — kanuni faiz yükümlülüğü", OrderDetailModel.LegalInterestWarningText);
        Assert.Equal(
            "Girilen tarih cayma süresinin dışında. Kayıt, süresinde gönderilmiş ama geç ulaşmış bir bildirimde ya da eksik bilgilendirmede yapılır; değerlendirme firmanındır.",
            OrderDetailModel.OutsideWindowWarningText);
        Assert.Equal("Bildirimin bir yıllık süre dolmadan gönderilmiş olması gerekir; gönderimin ispatı tüketicidedir.", OrderDetailModel.BeyondOneYearWarningText);
        Assert.Equal("Bu ürün cayma hakkının istisnası olarak işaretli. Kayıt eksik bilgilendirmede yapılır; değerlendirme firmanındır.", OrderDetailModel.AbsoluteExemptionWarningText);
        Assert.Equal("Uyarıyı değerlendirdim, kaydı yapıyorum.", OrderDetailModel.ExceptionAcknowledgementLabel);
        Assert.Equal(
            "Talep siparişin e-posta adresinden gelmediyse, işlemden önce siparişin e-posta adresine yazarak ya da teslimat telefonunu arayarak siparişin sahibiyle teyit edin. Sipariş numarası tek başına teyit değildir.",
            OrderDetailModel.OwnerCheckReminderText);
    }

    [Fact]
    public async Task FirmCancellation_AfterTheShippingPromiseRanOut_ShowsTheLegalInterestWarningInTheForm_AlsoWithTheWindow_AndBeforeIt_Not_T528_KK1()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid late, Guid[] lateItems) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        (Guid early, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        (Guid service, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Service, false));
        await ApprovedAsync(late, DateTimeOffset.UtcNow.AddDays(-30));
        await ApprovedAsync(early, DateTimeOffset.UtcNow);
        await ApprovedAsync(service, DateTimeOffset.UtcNow.AddDays(-30)); // Z-39: the service item's five-day fulfilment period ran out (D-1591)
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode lateStatus, string lateHtml) = await GetAsync(client, $"{OrdersPath}/{late:D}", key);
        (HttpStatusCode earlyStatus, string earlyHtml) = await GetAsync(client, $"{OrdersPath}/{early:D}", key);
        (HttpStatusCode serviceStatus, string serviceHtml) = await GetAsync(client, $"{OrdersPath}/{service:D}", key);
        (HttpStatusCode ask, string window) = await PostAsync(
            client,
            $"{OrdersPath}/{late:D}?handler=CancelOrderByCompany",
            [
                ("Input.Items[0].OrderItemId", lateItems[0].ToString("D")), ("Input.Items[0].Quantity", "1"),
                ("Input.CancellationReason", nameof(CancellationReason.DamagedOrUnsellable)), ("Input.Version", "1"),
            ],
            key);

        Assert.Equal(HttpStatusCode.OK, lateStatus);
        Assert.Contains($"id=\"cancel-legal-interest-warning\">{OrderDetailModel.LegalInterestWarningText}</p>", FormOf(lateHtml, "cancel-order-by-company-form"), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, earlyStatus);
        Assert.Contains("id=\"cancel-order-by-company-form\"", earlyHtml, StringComparison.Ordinal);
        Assert.DoesNotContain(OrderDetailModel.LegalInterestWarningText, earlyHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, serviceStatus);
        Assert.Contains(OrderDetailModel.LegalInterestWarningText, FormOf(serviceHtml, "cancel-order-by-company-form"), StringComparison.Ordinal);

        // K-1547 (3): the warning stays in the form while the window is open.
        Assert.Equal(HttpStatusCode.OK, ask);
        Assert.Contains(OrderDetailModel.LegalInterestWarningText, FormOf(window, "cancel-order-by-company-form"), StringComparison.Ordinal);
        await using AppDbContext reader = NewContext();
        Assert.Equal(0, await reader.Cancellations.CountAsync(c => c.OrderItemId == lateItems[0], Ct));
    }

    [Fact]
    public async Task LateTerminationRecord_ListsTheItemsWithTheirOpenUnits_ShowsTheLegalInterestWarning_HasTheIbanField_AndKeepsTheSentValuesOnARefusal_T528_KK1_KK5()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, Guid[] items) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        await using (AppDbContext db = NewContext())
        {
            await db.Orders.Where(o => o.Id == orderId).ExecuteUpdateAsync(s => s.SetProperty(o => o.CreatedAt, DateTimeOffset.UtcNow.AddDays(-40)), Ct);
        }

        string yesterday = DateText(TurkeyTime.DateOf(DateTimeOffset.UtcNow).AddDays(-1));
        string path = $"{OrdersPath}/{orderId:D}?handler=RecordLateDeliveryTermination";
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);
        // A date in the future is the refusal; the IBAN that fails OB-14 is none (K-1631 (5); D-1599): it is kept in the form and, with a good date, the confirmation window opens.
        string tomorrow = DateText(TurkeyTime.DateOf(DateTimeOffset.UtcNow).AddDays(1));
        (HttpStatusCode wrong, string wrongHtml) = await PostAsync(
            client, path, [("Input.DeclarationDate", tomorrow), ("Input.IsFromContactEmail", "true"), ("Input.Iban", WrongChecksumIban), ("Input.Version", "1")], key);
        (HttpStatusCode ibanAsk, string ibanWindow) = await PostAsync(
            client, path, [("Input.DeclarationDate", yesterday), ("Input.IsFromContactEmail", "true"), ("Input.Iban", WrongChecksumIban), ("Input.Version", "1")], key);
        (HttpStatusCode ask, string window) = await PostAsync(client, path, [("Input.DeclarationDate", yesterday), ("Input.Version", "1")], key);

        Assert.Equal(HttpStatusCode.OK, status);
        string form = FormOf(html, "record-late-termination-form");
        Assert.Contains("<li>Ürün — 1 adet</li>", form, StringComparison.Ordinal);
        Assert.Contains($"id=\"termination-legal-interest-warning\">{OrderDetailModel.LegalInterestWarningText}</p>", form, StringComparison.Ordinal);
        Assert.Contains("id=\"termination-iban\"", form, StringComparison.Ordinal);
        Assert.True(
            form.IndexOf("name=\"Input.IsFromContactEmail\"", StringComparison.Ordinal) < form.IndexOf("id=\"termination-iban\"", StringComparison.Ordinal),
            "The IBAN field follows the contact e-mail option (04 7.2.10.4).");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, wrong);
        Assert.Contains(OrderDetailModel.NoticeDateInvalidText, wrongHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("sağlama basamağı tutmuyor", wrongHtml, StringComparison.Ordinal);
        string kept = FormOf(wrongHtml, "record-late-termination-form");
        Assert.Contains($"value=\"{tomorrow}\"", kept, StringComparison.Ordinal);
        Assert.Contains($"value=\"{WrongChecksumIban}\"", kept, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.IsFromContactEmail\" value=\"true\" checked=\"checked\"", kept, StringComparison.Ordinal);

        Assert.Equal(HttpStatusCode.OK, ibanAsk);
        Assert.Contains(OrderDetailModel.LegalInterestWarningText, FormOf(ibanWindow, "record-late-termination-form"), StringComparison.Ordinal);

        Assert.Equal(HttpStatusCode.OK, ask);
        Assert.Contains(OrderDetailModel.LegalInterestWarningText, FormOf(window, "record-late-termination-form"), StringComparison.Ordinal);
        await using AppDbContext reader = NewContext();
        Assert.Equal(0, await reader.LateDeliveryTerminations.CountAsync(t => t.OrderItemId == items[0], Ct));
    }

    [Fact]
    public async Task ItemRecords_OfACancellationAfterThePromise_AndOfATermination_CarryTheLegalInterestWarning_AndACancellationWithinThePromise_Not_T528_KK1_VAL_A228()
    {
        // 04 9.9.6, K-798 (2), VAL-B135, VAL-B136, VAL-B140: the warning stands on the record's row of the item.
        string onTheRow = $"<span class=\"item-records__warning\" role=\"alert\">{OrderDetailModel.LegalInterestWarningText}</span>";
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid late, Guid[] lateItems) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        (Guid early, Guid[] earlyItems) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        (Guid terminated, Guid[] terminatedItems) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        await ApprovedAsync(late, DateTimeOffset.UtcNow.AddDays(-30));
        await ApprovedAsync(early, DateTimeOffset.UtcNow);
        await using (AppDbContext db = NewContext())
        {
            await db.Orders.Where(o => o.Id == terminated).ExecuteUpdateAsync(s => s.SetProperty(o => o.CreatedAt, DateTimeOffset.UtcNow.AddDays(-40)), Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        foreach ((Guid orderId, Guid itemId) in new[] { (late, lateItems[0]), (early, earlyItems[0]) })
        {
            (HttpStatusCode cancelled, _) = await PostForLocationAsync(
                client,
                $"{OrdersPath}/{orderId:D}?handler=CancelOrderByCompany",
                [
                    ("Input.Items[0].OrderItemId", itemId.ToString("D")), ("Input.Items[0].Quantity", "1"),
                    ("Input.CancellationReason", nameof(CancellationReason.DamagedOrUnsellable)), ("Input.Version", "1"), ("Input.Confirmed", "true"),
                ],
                key);
            Assert.Equal(HttpStatusCode.SeeOther, cancelled);
        }

        (HttpStatusCode recorded, _) = await PostForLocationAsync(
            client,
            $"{OrdersPath}/{terminated:D}?handler=RecordLateDeliveryTermination",
            [("Input.DeclarationDate", DateText(TurkeyTime.DateOf(DateTimeOffset.UtcNow).AddDays(-1))), ("Input.Version", "1"), ("Input.Confirmed", "true")],
            key);
        Assert.Equal(HttpStatusCode.SeeOther, recorded);

        (HttpStatusCode lateStatus, string lateHtml) = await GetAsync(client, $"{OrdersPath}/{late:D}", key);
        (HttpStatusCode earlyStatus, string earlyHtml) = await GetAsync(client, $"{OrdersPath}/{early:D}", key);
        (HttpStatusCode terminatedStatus, string terminatedHtml) = await GetAsync(client, $"{OrdersPath}/{terminated:D}", key);

        Assert.Equal(HttpStatusCode.OK, lateStatus);
        Assert.Matches($"İptal kaydı</span>[^<]*(<span class=\"item-records__(moment|detail)\">[^<]*</span>\\s*)*{Regex.Escape(onTheRow)}", lateHtml);
        Assert.Equal(HttpStatusCode.OK, earlyStatus);
        Assert.Contains("İptal kaydı</span>", earlyHtml, StringComparison.Ordinal);
        Assert.DoesNotContain(OrderDetailModel.LegalInterestWarningText, earlyHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, terminatedStatus);
        Assert.Matches($"Gecikme feshi</span>[^<]*(<span class=\"item-records__(moment|detail)\">[^<]*</span>\\s*)*{Regex.Escape(onTheRow)}", terminatedHtml);
    }

    [Fact]
    public async Task WithdrawalRecord_TakesManyItemsWithTheirUnits_AsksOnce_ThenWritesOneDeclarationPerItem_T528_KK4()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, Guid[] items) = await StepOrderAsync(OrderStatus.Delivered, PaymentStatus.Paid, (ProductType.Physical, true), (ProductType.Physical, true));
        DateOnly today = TurkeyTime.DateOf(DateTimeOffset.UtcNow);
        await DeliveredOnAsync(items, today.AddDays(-5), null);
        string path = $"{OrdersPath}/{orderId:D}?handler=RecordWithdrawal";
        (string Name, string Value)[] fields =
        [
            ("Input.Items[0].OrderItemId", items[0].ToString("D")), ("Input.Items[0].Quantity", "1"),
            ("Input.Items[1].OrderItemId", items[1].ToString("D")), ("Input.Items[1].Quantity", "1"),
            ("Input.DeclarationDate", DateText(today.AddDays(-1))), ("Input.Version", "1"),
        ];
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);
        (HttpStatusCode ask, _) = await PostAsync(client, path, fields, key);
        int askedWithdrawals;
        await using (AppDbContext db = NewContext())
        {
            askedWithdrawals = await db.Withdrawals.CountAsync(w => items.Contains(w.OrderItemId), Ct);
        }

        (HttpStatusCode done, _) = await PostForLocationAsync(client, path, [.. fields, ("Input.Confirmed", "true")], key);

        Assert.Equal(HttpStatusCode.OK, status);
        string form = FormOf(html, "record-withdrawal-form");
        // The two items share their creation time, so their rows' order (and index) is the key's: each item has a row, whichever index it takes.
        Assert.Equal("01", string.Concat(items.Select(i => Regex.Match(form, $"name=\"Input\\.Items\\[(\\d)\\]\\.OrderItemId\" value=\"{i:D}\"").Groups[1].Value).Order()));
        Assert.Contains(OrderDetailModel.OwnerCheckReminderText, html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, ask);
        Assert.Equal(0, askedWithdrawals);
        Assert.Equal(HttpStatusCode.SeeOther, done);
        await using AppDbContext reader = NewContext();
        List<(Guid, int)> written = [.. (await reader.Withdrawals.Where(w => items.Contains(w.OrderItemId)).ToListAsync(Ct)).Select(w => (w.OrderItemId, w.Quantity)).OrderBy(w => w.OrderItemId)];
        Assert.Equal(items.Order().Select(i => (i, 1)), written);
        Assert.Equal(2, await reader.AuditLogs.CountAsync(a => a.TargetRecordId == orderId && a.Operation == nameof(AuditOperation.RecordWithdrawalFromOtherChannel), Ct));
    }

    [Fact]
    public async Task WithdrawalRecord_BeyondTheYear_OnAnExemptItem_ShowsTheOutsideWindowTextWithTheYearSentence_ThenTheExemptionText_AndTheBoxText_AndKeepsTheTick_T528_KK5()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, Guid[] items) = await StepOrderAsync(OrderStatus.Delivered, PaymentStatus.Paid, (ProductType.Physical, true));
        DateOnly today = TurkeyTime.DateOf(DateTimeOffset.UtcNow);
        await DeliveredOnAsync(items, today.AddYears(-2), WithdrawalExemptionReason.MadeToOrder);
        string path = $"{OrdersPath}/{orderId:D}?handler=RecordWithdrawal";
        (string Name, string Value)[] fields =
        [
            ("Input.Items[0].OrderItemId", items[0].ToString("D")), ("Input.Items[0].Quantity", "1"), ("Input.DeclarationDate", DateText(today.AddDays(-1))), ("Input.Version", "1"),
        ];
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode unticked, string untickedHtml) = await PostAsync(client, path, fields, key);
        (HttpStatusCode ticked, string tickedHtml) = await PostAsync(client, path, [.. fields, ("Input.IsExceptionAcknowledged", "true")], key);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, unticked);
        Assert.Contains(OrderDetailModel.ExceptionUnacknowledgedText, untickedHtml, StringComparison.Ordinal);
        string form = FormOf(untickedHtml, "record-withdrawal-form");
        int outside = form.IndexOf(OrderDetailModel.OutsideWindowWarningText + " " + OrderDetailModel.BeyondOneYearWarningText, StringComparison.Ordinal);
        int exemption = form.IndexOf(OrderDetailModel.AbsoluteExemptionWarningText, StringComparison.Ordinal);
        Assert.True(outside >= 0 && exemption > outside, "K-1547: both warnings, the outside window's first.");
        Assert.Contains($"name=\"Input.IsExceptionAcknowledged\" value=\"true\" /> {OrderDetailModel.ExceptionAcknowledgementLabel}</label>", form, StringComparison.Ordinal);
        Assert.Single(form.Split("name=\"Input.IsExceptionAcknowledged\"", StringSplitOptions.None).Skip(1));

        Assert.Equal(HttpStatusCode.OK, ticked);
        string kept = FormOf(tickedHtml, "record-withdrawal-form");
        Assert.Contains("name=\"Input.IsExceptionAcknowledged\" value=\"true\" checked=\"checked\"", kept, StringComparison.Ordinal);
        Assert.Contains(OrderDetailModel.AbsoluteExemptionWarningText, kept, StringComparison.Ordinal);
        await using AppDbContext reader = NewContext();
        Assert.Equal(0, await reader.Withdrawals.CountAsync(w => w.OrderItemId == items[0], Ct));
    }

    [Fact]
    public async Task WithdrawalRecord_KeepsTheOutsideWindowAndExemptionWarnings_AndTheSentValues_OnTheUnitsRefusals_T528_KK5_VAL_B159_D1599()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, Guid[] items) = await StepOrderAsync(OrderStatus.Delivered, PaymentStatus.Paid, (ProductType.Physical, true));
        DateOnly today = TurkeyTime.DateOf(DateTimeOffset.UtcNow);
        await DeliveredOnAsync(items, today.AddDays(-60), WithdrawalExemptionReason.MadeToOrder);
        string path = $"{OrdersPath}/{orderId:D}?handler=RecordWithdrawal";
        string date = DateText(today.AddDays(-1));
        (string Name, string Value)[] Fields(string quantity) =>
            [("Input.Items[0].OrderItemId", items[0].ToString("D")), ("Input.Items[0].Quantity", quantity), ("Input.DeclarationDate", date), ("Input.Version", "1")];
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        // The page's own units refusal and the record's units refusal (over the open units): neither reaches the warnings' rule in the record. An IBAN that fails OB-14 is no refusal
        // (K-1631 (5); D-1599): the send reaches the warnings' rule and is refused there, with the IBAN kept.
        (HttpStatusCode unreadable, string unreadableHtml) = await PostAsync(client, path, [.. Fields("bir"), ("Input.IsExceptionAcknowledged", "true")], key);
        (HttpStatusCode exceeding, string exceedingHtml) = await PostAsync(client, path, Fields("5"), key);
        (HttpStatusCode iban, string ibanHtml) = await PostAsync(client, path, [.. Fields("1"), ("Input.IsFromContactEmail", "true"), ("Input.Iban", "TR00 0000")], key);

        foreach ((HttpStatusCode status, string html, string message) in new[]
        {
            (unreadable, unreadableHtml, OrderDetailModel.QuantityInvalidText),
            (exceeding, exceedingHtml, OrderDetailModel.QuantityExceedsOpenText),
            (iban, ibanHtml, OrderDetailModel.ExceptionUnacknowledgedText),
        })
        {
            Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
            Assert.Contains(message, html, StringComparison.Ordinal);
            string form = FormOf(html, "record-withdrawal-form");
            int ownerCheck = form.IndexOf(OrderDetailModel.OwnerCheckReminderText, StringComparison.Ordinal);
            int outside = form.IndexOf(OrderDetailModel.OutsideWindowWarningText, StringComparison.Ordinal);
            int exemption = form.IndexOf(OrderDetailModel.AbsoluteExemptionWarningText, StringComparison.Ordinal);
            Assert.True(ownerCheck >= 0 && outside > ownerCheck && exemption > outside, "VAL-B159: U6a at the head, U3a, then U3b stand on the refusal.");
            Assert.DoesNotContain(OrderDetailModel.BeyondOneYearWarningText, form, StringComparison.Ordinal);
            Assert.Single(form.Split("name=\"Input.IsExceptionAcknowledged\"", StringSplitOptions.None).Skip(1));
            Assert.Contains($"name=\"Input.DeclarationDate\" value=\"{date}\"", form, StringComparison.Ordinal);
        }

        Assert.Contains("name=\"Input.IsExceptionAcknowledged\" value=\"true\" checked=\"checked\"", FormOf(unreadableHtml, "record-withdrawal-form"), StringComparison.Ordinal);
        Assert.Contains("value=\"TR00 0000\"", FormOf(ibanHtml, "record-withdrawal-form"), StringComparison.Ordinal);
        await using AppDbContext reader = NewContext();
        Assert.Equal(0, await reader.Withdrawals.CountAsync(w => w.OrderItemId == items[0], Ct));
    }

    [Fact]
    public async Task AnOrderNotHandedToTheCarrier_SaysAtTheWithdrawalRecordsPlaceThatTheWayIsTheAgreedCancellation_AndAShippedOrderDoesNotSayIt_D1604()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid unshipped, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        (Guid shipped, _) = await StepOrderAsync(OrderStatus.Shipped, PaymentStatus.Paid, (ProductType.Physical, false));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode unshippedStatus, string unshippedHtml) = await GetAsync(client, $"{OrdersPath}/{unshipped:D}", key);
        (HttpStatusCode shippedStatus, string shippedHtml) = await GetAsync(client, $"{OrdersPath}/{shipped:D}", key);

        Assert.Equal(HttpStatusCode.OK, unshippedStatus);
        Assert.Contains($"id=\"withdrawal-not-shipped-note\">{OrderDetailModel.NotShippedItemText}</p>", System.Net.WebUtility.HtmlDecode(unshippedHtml), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, shippedStatus);
        Assert.DoesNotContain("withdrawal-not-shipped-note", shippedHtml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task FirmCancellation_TheReasonsWarning_StandsInTheFormBeforeAndWithTheWindow_U1ForOutOfStock_U6aForAgreed_AndTheWindowRepeatsNone_T528_KK1()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, Guid[] items) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        await ApprovedAsync(orderId, DateTimeOffset.UtcNow.AddDays(-30)); // Z-10 ran out: U2 stands beside U1 (12 VAL-B133)
        string path = $"{OrdersPath}/{orderId:D}?handler=CancelOrderByCompany";
        (string Name, string Value)[] Fields(CancellationReason reason) =>
            [("Input.Items[0].OrderItemId", items[0].ToString("D")), ("Input.Items[0].Quantity", "1"), ("Input.CancellationReason", reason.ToString()), ("Input.Version", "1")];
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);
        (HttpStatusCode stockAsk, string stockHtml) = await PostAsync(client, path, Fields(CancellationReason.OutOfStock), key);
        (HttpStatusCode agreedAsk, string agreedHtml) = await PostAsync(client, path, Fields(CancellationReason.AgreedWithBuyer), key);
        (HttpStatusCode otherAsk, string otherHtml) = await PostAsync(client, path, Fields(CancellationReason.DamagedOrUnsellable), key);

        // Before a reason is chosen both stand hidden in the form (admin.css opens the chosen reason's one); U2 already shows.
        Assert.Equal(HttpStatusCode.OK, status);
        string form = FormOf(html, "cancel-order-by-company-form");
        Assert.Contains($"id=\"cancel-out-of-stock-warning\" hidden=\"hidden\">{OrderDetailModel.OutOfStockLegalNoticeText}</p>", form, StringComparison.Ordinal);
        Assert.Contains($"id=\"cancel-owner-check-reminder\" hidden=\"hidden\">{OrderDetailModel.OwnerCheckReminderText}</p>", form, StringComparison.Ordinal);
        Assert.Contains(OrderDetailModel.LegalInterestWarningText, form, StringComparison.Ordinal);

        // "Stokta bulunamadı": the window is open; U1 and U2 stand in the form, the reason kept; the window says the scope, the line and the result, none of U1, U2, U6a.
        Assert.Equal(HttpStatusCode.OK, stockAsk);
        string stockForm = FormOf(stockHtml, "cancel-order-by-company-form");
        Assert.Contains($"id=\"cancel-out-of-stock-warning\">{OrderDetailModel.OutOfStockLegalNoticeText}</p>", stockForm, StringComparison.Ordinal);
        Assert.Contains("id=\"cancel-owner-check-reminder\" hidden=\"hidden\">", stockForm, StringComparison.Ordinal);
        Assert.Contains(OrderDetailModel.LegalInterestWarningText, stockForm, StringComparison.Ordinal);
        Assert.Contains($"<option value=\"{nameof(CancellationReason.OutOfStock)}\" selected=\"selected\">", stockForm, StringComparison.Ordinal);
        string stockWindow = WindowOf(stockHtml);
        Assert.Contains("1 ürün iptal edilecek", stockWindow, StringComparison.Ordinal);
        AssertRepeatsNoWarning(stockWindow);

        // "Müşteriyle anlaşıldı (müşteri talebi)": U6a stands in the form, U1 does not (12 VAL-B154); the window does not repeat it.
        Assert.Equal(HttpStatusCode.OK, agreedAsk);
        string agreedForm = FormOf(agreedHtml, "cancel-order-by-company-form");
        Assert.Contains($"id=\"cancel-owner-check-reminder\">{OrderDetailModel.OwnerCheckReminderText}</p>", agreedForm, StringComparison.Ordinal);
        Assert.Contains("id=\"cancel-out-of-stock-warning\" hidden=\"hidden\">", agreedForm, StringComparison.Ordinal);
        AssertRepeatsNoWarning(WindowOf(agreedHtml));

        // Another reason: neither (12 VAL-B132).
        Assert.Equal(HttpStatusCode.OK, otherAsk);
        string otherForm = FormOf(otherHtml, "cancel-order-by-company-form");
        Assert.Contains("id=\"cancel-out-of-stock-warning\" hidden=\"hidden\">", otherForm, StringComparison.Ordinal);
        Assert.Contains("id=\"cancel-owner-check-reminder\" hidden=\"hidden\">", otherForm, StringComparison.Ordinal);

        await using AppDbContext reader = NewContext();
        Assert.Equal(0, await reader.Cancellations.CountAsync(c => c.OrderItemId == items[0], Ct));
        Assert.Equal("Stok yokluğu, siparişi yerine getirmenin yasal olarak imkânsızlaşması sayılmaz; bu iptalin sonucu firmadadır.", OrderDetailModel.OutOfStockLegalNoticeText);

        static void AssertRepeatsNoWarning(string window)
        {
            Assert.DoesNotContain(OrderDetailModel.OutOfStockLegalNoticeText, window, StringComparison.Ordinal);
            Assert.DoesNotContain(OrderDetailModel.OwnerCheckReminderText, window, StringComparison.Ordinal);
            Assert.DoesNotContain(OrderDetailModel.LegalInterestWarningText, window, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task NoticeRecords_OpenTheIbanFieldWithTheContactEmailOption_InOneMarkup_AndCarryTheOwnerCheckAtTheirHead_T528_KK5()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _) = await StepOrderAsync(OrderStatus.Shipped, PaymentStatus.Paid, (ProductType.Physical, false));
        await using (AppDbContext db = NewContext())
        {
            await db.Orders.Where(o => o.Id == orderId).ExecuteUpdateAsync(s => s.SetProperty(o => o.CreatedAt, DateTimeOffset.UtcNow.AddDays(-40)), Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);

        // A shipped, undelivered item after Z-11: both records are open (12 VAL-B137, VAL-B153, VAL-B156).
        Assert.Equal(HttpStatusCode.OK, status);
        foreach (string prefix in new[] { "withdrawal", "termination" })
        {
            string form = FormOf(html, prefix == "withdrawal" ? "record-withdrawal-form" : "record-late-termination-form");
            int reminder = form.IndexOf($"id=\"{prefix}-owner-check-reminder\">{OrderDetailModel.OwnerCheckReminderText}</p>", StringComparison.Ordinal);
            Assert.True(reminder >= 0 && reminder < form.IndexOf("<label", StringComparison.Ordinal), $"U6a heads the {prefix} form (K-798 (6)).");
            // 04 7.2.10.4: the option, then the IBAN field in the block admin.css closes while the option is not ticked.
            int block = form.IndexOf("<div class=\"order-detail__contact-iban\">", StringComparison.Ordinal);
            int option = form.IndexOf("name=\"Input.IsFromContactEmail\" value=\"true\" /> Bildirim siparişin iletişim e-postasından geldi</label>", StringComparison.Ordinal);
            int field = form.IndexOf($"<div class=\"order-detail__notice-iban\" id=\"{prefix}-iban-field\">", StringComparison.Ordinal);
            int iban = form.IndexOf($"id=\"{prefix}-iban\" name=\"Input.Iban\"", StringComparison.Ordinal);
            Assert.True(block >= 0 && option > block && field > option && iban > field, $"The {prefix} form opens the IBAN field with the option.");
        }

        Assert.DoesNotContain("<p class=\"order-detail__reminder\">", html, StringComparison.Ordinal);
        string css = File.ReadAllText(System.IO.Path.Combine(RepositoryRoot.Find(), "src", "Shopfolio.Web", "wwwroot", "css", "admin.css"));
        Assert.Contains(
            ".order-detail__contact-iban:has(input[type=\"checkbox\"]:not(:checked)) > .order-detail__notice-iban {\n    display: none;",
            css.ReplaceLineEndings("\n"),
            StringComparison.Ordinal);
    }

    // The confirmation window of the page's 200 (_Confirm: the administrator's dialog).
    [Fact]
    public async Task CardRemovalWindow_SaysTheAmountTheRecordPaysBack_WithTheShippingFeeOnlyWhenTheRemovalTakesTheLastPhysicalUnit_AndWritesNothing_D1598()
    {
        // K-797: "{n} ürün siparişten çıkarılacak ve {tutar} müşterinin kartına geri ödenecek." - {tutar} the system's 02 7.1.7 and 7.2.9 amount (the item's price, no coupon; the fee
        // when the removal meets the trigger). Before T604 the window said "tutarı" with no amount.
        const decimal fee = 30m;
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid whole, Guid[] wholeItems) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        (Guid part, Guid[] partItems) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false), (ProductType.Physical, false));
        decimal price;
        await using (AppDbContext db = NewContext())
        {
            await db.Database.ExecuteSqlAsync(
                $"UPDATE orders SET payment_method = 'Card', bank_transfer_iban = NULL, shipping_fee = {fee}, shipping_fee_vat_breakdown = '[]' WHERE id = {whole} OR id = {part}", Ct);
            price = (await db.OrderItems.SingleAsync(i => i.Id == wholeItems[0], Ct)).UnitPrice;
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (HttpStatusCode wholeStatus, string wholeHtml) = await PostAsync(
            client, $"{OrdersPath}/{whole:D}?handler=RemoveOrderItem", [("Input.OrderItemId", wholeItems[0].ToString("D")), ("Input.Quantity", "1"), ("Input.Version", "1")], key);
        (HttpStatusCode partStatus, string partHtml) = await PostAsync(
            client, $"{OrdersPath}/{part:D}?handler=RemoveOrderItem", [("Input.OrderItemId", partItems[0].ToString("D")), ("Input.Quantity", "1"), ("Input.Version", "1")], key);

        Assert.Equal(HttpStatusCode.OK, wholeStatus);
        Assert.Contains(IrreversibleConfirmations.CardRefundingRemoval(1, price + fee).Sentence, WindowOf(wholeHtml), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, partStatus);
        Assert.Contains(IrreversibleConfirmations.CardRefundingRemoval(1, price).Sentence, WindowOf(partHtml), StringComparison.Ordinal);
        await using AppDbContext reader = NewContext();
        Assert.Equal(0, await reader.ItemRemovals.CountAsync(r => r.OrderItemId == wholeItems[0] || r.OrderItemId == partItems[0], Ct));
    }

    private static string WindowOf(string html)
    {
        int start = html.IndexOf("<dialog", StringComparison.Ordinal);
        Assert.True(start >= 0, "The screen has no confirmation window.");
        return html[start..html.IndexOf("</dialog>", start, StringComparison.Ordinal)];
    }

    // The payment's approval (the Ö1 record) at the given moment, and the order's frozen shipping promise in business days (03 4.1.10).
    private async Task ApprovedAsync(Guid orderId, DateTimeOffset at, int shippingPeriod = 2)
    {
        await using AppDbContext db = NewContext();
        db.TransitionRecords.Add(TransitionRecord.Create(orderId, TransitionCode.O1, ActorKind.System, null, null, null, null, null, null, null, at));
        await db.SaveChangesAsync(Ct);
        await db.Orders.Where(o => o.Id == orderId).ExecuteUpdateAsync(s => s.SetProperty(o => o.ShippingPeriod, shippingPeriod), Ct);
    }

    // The items delivered on the given day, with the frozen exemption reason given.
    private async Task DeliveredOnAsync(Guid[] items, DateOnly day, WithdrawalExemptionReason? exemption)
    {
        DateTimeOffset marked = new(day.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);
        await using AppDbContext db = NewContext();
        await db.OrderItems.Where(i => items.Contains(i.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.DeliveryDate, day).SetProperty(i => i.DeliveryMarkedAt, marked).SetProperty(i => i.WithdrawalExemptionReason, exemption), Ct);
    }
}
