using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// E-37's corrections through the page (T530 KK1-KK10; 07 UC-146...UC-151): each handler's 303 and its record, the 422s with their messages, and the trail rows that carry no customer value.
/// </summary>
public sealed partial class OrderDetailPageTests
{
    private const string CorrectionAdminName = "Yönetici Düzeltme";

    // ---- KK1, KK2: the address ----

    [Fact]
    public async Task CorrectAddress_TheShippingAddressWithItsPhoneAndTheBillingAddress_Are303_WriteTheHistory_AndTheTrailHasNoAddress_KK1()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        Guid province = await WithShippingAddressAsync(orderId);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode page, string html) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);
        (HttpStatusCode shipping, string? location) = await PostForLocationAsync(client, $"{OrdersPath}/{orderId:D}?handler=CorrectAddress", AddressFields(1, AddressType.ShippingAddress, province, "Yeni Sokak 7", "05329998877"), key);
        (HttpStatusCode billing, _) = await PostForLocationAsync(client, $"{OrdersPath}/{orderId:D}?handler=CorrectAddress", AddressFields(2, AddressType.BillingAddress, province, "Fatura Caddesi 9", null), key);
        (_, string after) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);

        // VAL-D026: the page's address history shows the old and the new address with the change's date.
        string customer = SectionOf(after, "musteri");
        Assert.Contains("id=\"adres-gecmisi\"", customer, StringComparison.Ordinal);
        Assert.Contains($"{DisplayFormat.Date(DateTimeOffset.UtcNow)} · Teslimat adresi", customer, StringComparison.Ordinal);
        Assert.Contains("Eski Sokak 1", customer, StringComparison.Ordinal);
        Assert.Contains("Yeni Sokak 7", customer, StringComparison.Ordinal);
        Assert.Contains("Fatura Caddesi 9", customer, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, page);
        string corrections = SectionOf(html, "duzeltmeler");
        Assert.Contains(OrderDetailModel.AddressReminderText, corrections, StringComparison.Ordinal);
        Assert.Contains("id=\"correct-shipping-address-form\"", corrections, StringComparison.Ordinal);
        Assert.Contains("id=\"correct-billing-address-form\"", corrections, StringComparison.Ordinal);
        Assert.Equal((HttpStatusCode.SeeOther, HttpStatusCode.SeeOther), (shipping, billing));
        Assert.Equal($"{OrdersPath}/{orderId:D}", location);
        await using AppDbContext reader = NewContext();
        Order order = await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal(("Yeni Sokak 7", "5329998877", "Fatura Caddesi 9", 3), (order.ShippingAddressLine, order.ShippingPhone, order.BillingAddressLine, order.Version));
        List<OrderAddressHistory> history = await reader.OrderAddressHistories.AsNoTracking().Where(h => h.OrderId == orderId).OrderBy(h => h.CreatedAt).ToListAsync(Ct);
        Assert.Equal(2, history.Count);
        Assert.Equal((AddressType.ShippingAddress, "Eski Sokak 1", "Yeni Sokak 7", "5321234567", "5329998877"), (history[0].AddressType, history[0].OldAddressLine, history[0].NewAddressLine, history[0].OldPhone, history[0].NewPhone));
        Assert.Equal((AddressType.BillingAddress, "Ataturk Bulvari 1", "Fatura Caddesi 9"), (history[1].AddressType, history[1].OldAddressLine, history[1].NewAddressLine));
        List<AuditLog> trail = await reader.AuditLogs.AsNoTracking().Where(a => a.TargetRecordId == orderId).ToListAsync(Ct);
        Assert.Equal([nameof(AuditOperation.CorrectAddress), nameof(AuditOperation.CorrectAddress)], trail.Select(a => a.Operation));
        string details = string.Join("|", trail.Select(a => JsonSerializer.Serialize(a.Details)));
        Assert.DoesNotContain("Yeni Sokak", details, StringComparison.Ordinal);
        Assert.DoesNotContain("Eski Sokak", details, StringComparison.Ordinal);
        Assert.DoesNotContain("9998877", details, StringComparison.Ordinal);
        Assert.DoesNotContain("Fatura Caddesi", details, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CorrectAddress_AFieldItRefuses_Is422WithTheFieldsMessage_AndItIsClosedFromShipped_OrWithNoPhysicalItemFromDelivered_KK2()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid preparingId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        Guid province = await WithShippingAddressAsync(preparingId);
        (Guid shippedId, _) = await StepOrderAsync(OrderStatus.Shipped, PaymentStatus.Paid, (ProductType.Physical, false));
        await WithShippingAddressAsync(shippedId);
        (Guid digitalPreparingId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Digital, false));
        (Guid digitalDeliveredId, _) = await StepOrderAsync(OrderStatus.Delivered, PaymentStatus.Paid, (ProductType.Digital, true));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode field, string fieldHtml) = await PostAsync(client, $"{OrdersPath}/{preparingId:D}?handler=CorrectAddress", AddressFields(1, AddressType.ShippingAddress, province, string.Empty, "05329998877"), key);
        (HttpStatusCode shipped, string shippedHtml) = await PostAsync(client, $"{OrdersPath}/{shippedId:D}?handler=CorrectAddress", AddressFields(1, AddressType.BillingAddress, province, "Fatura Caddesi 9", null), key);
        (HttpStatusCode beforeDelivered, _) = await PostForLocationAsync(client, $"{OrdersPath}/{digitalPreparingId:D}?handler=CorrectAddress", AddressFields(1, AddressType.BillingAddress, province, "Fatura Caddesi 9", null), key);
        (HttpStatusCode delivered, string deliveredHtml) = await PostAsync(client, $"{OrdersPath}/{digitalDeliveredId:D}?handler=CorrectAddress", AddressFields(1, AddressType.BillingAddress, province, "Fatura Caddesi 9", null), key);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, field);

        // B1: the refused field is marked with its own message under it, the summary at the head of its form links to it, and the sent values stay; the other form is untouched.
        string shippingForm = FormOf(fieldHtml, OrderDetailModel.ShippingAddressFormId);
        Assert.Contains($"id=\"{Shopfolio.Pages.Components.FormErrorSummary.ElementId}\"", shippingForm, StringComparison.Ordinal);
        Assert.Contains("href=\"#Input_ShippingAddressLine\"", shippingForm, StringComparison.Ordinal);
        string line = FieldOf(shippingForm, "Input_ShippingAddressLine");
        Assert.Contains("aria-invalid=\"true\"", line, StringComparison.Ordinal);
        Assert.Contains("id=\"Input_ShippingAddressLine-message\"", line, StringComparison.Ordinal);
        Assert.Contains("value=\"Ayşe Kaya\"", shippingForm, StringComparison.Ordinal);
        Assert.DoesNotContain("aria-invalid=\"true\"", FieldOf(shippingForm, "Input_ShippingRecipientName"), StringComparison.Ordinal);
        Assert.DoesNotContain("aria-invalid", FormOf(fieldHtml, OrderDetailModel.BillingAddressFormId), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, shipped);

        // B2: the closed correction's message is in the corrections' place (its form is no longer drawn), not in the next step's.
        Assert.Contains(OrderDetailModel.NotOpenText, SectionOf(shippedHtml, "duzeltmeler"), StringComparison.Ordinal);
        Assert.DoesNotContain("order-step-message", shippedHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, beforeDelivered);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, delivered);
        Assert.Contains(OrderDetailModel.NotOpenText, deliveredHtml, StringComparison.Ordinal);
        await using AppDbContext reader = NewContext();
        Assert.Equal(1, await reader.OrderAddressHistories.CountAsync(h => h.OrderId == preparingId || h.OrderId == shippedId || h.OrderId == digitalPreparingId || h.OrderId == digitalDeliveredId, Ct));
    }

    [Fact]
    public async Task TheCorrections_RepeatNoElementId_AndEachAddressFormsLabelsAreItsOwnFields_B3()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        await WithShippingAddressAsync(orderId);
        TransitionRecord paid = TransitionRecord.Create(orderId, TransitionCode.O1, ActorKind.Admin, adminId, CorrectionAdminName, null, null, null, null, null, DateTimeOffset.UtcNow.AddHours(-2));
        TransitionRecord shipment = TransitionRecord.Create(orderId, TransitionCode.S5, ActorKind.Admin, adminId, CorrectionAdminName, null, null, null, null, null, DateTimeOffset.UtcNow.AddHours(-1));
        await using (AppDbContext db = NewContext())
        {
            db.TransitionRecords.AddRange(paid, shipment);
            await db.SaveChangesAsync(Ct);
            await db.Orders.Where(o => o.Id == orderId).ExecuteUpdateAsync(s => s.SetProperty(o => o.OrderStatus, OrderStatus.Shipped), Ct);
        }

        (Guid preparingId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        await WithShippingAddressAsync(preparingId);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode shipped, string shippedHtml) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);
        (HttpStatusCode preparing, string preparingHtml) = await GetAsync(client, $"{OrdersPath}/{preparingId:D}", key);

        Assert.Equal((HttpStatusCode.OK, HttpStatusCode.OK), (shipped, preparing));
        AssertNoRepeatedId(shippedHtml);
        AssertNoRepeatedId(preparingHtml);

        // Two correctable transitions share one form and one reason field; the form asks which one.
        string transition = FormOf(shippedHtml, OrderDetailModel.TransitionFormId);
        Assert.Contains($"name=\"Input.TransitionRecordId\" value=\"{paid.Id:D}\"", transition, StringComparison.Ordinal);
        Assert.Contains($"name=\"Input.TransitionRecordId\" value=\"{shipment.Id:D}\"", transition, StringComparison.Ordinal);
        Assert.Equal(1, transition.Split("name=\"Input.CorrectionReason\"", StringSplitOptions.None).Length - 1);

        // Each address form's labels are tied to its own fields.
        string shipping = FormOf(preparingHtml, OrderDetailModel.ShippingAddressFormId);
        string billing = FormOf(preparingHtml, OrderDetailModel.BillingAddressFormId);
        foreach (string member in new[] { "RecipientName", "ProvinceId", "District", "AddressLine" })
        {
            Assert.Contains($"for=\"Input_Shipping{member}\"", shipping, StringComparison.Ordinal);
            Assert.Contains($"id=\"Input_Shipping{member}\" name=\"Input.Shipping{member}\"", shipping, StringComparison.Ordinal);
            Assert.Contains($"for=\"Input_Billing{member}\"", billing, StringComparison.Ordinal);
            Assert.Contains($"id=\"Input_Billing{member}\" name=\"Input.Billing{member}\"", billing, StringComparison.Ordinal);
        }

        Assert.Contains("for=\"Input_ShippingPhone\"", shipping, StringComparison.Ordinal);
        Assert.DoesNotContain("Phone", billing, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CorrectAddress_ThatNamesNoAddress_Is422WithItsOwnMessage_AndOneWithoutTheVersion_Is409_K4()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        Guid province = await WithShippingAddressAsync(orderId);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string path = $"{OrdersPath}/{orderId:D}?handler=CorrectAddress";
        (string Name, string Value)[] fields = AddressFields(1, AddressType.ShippingAddress, province, "Yeni Sokak 7", "05329998877");

        (HttpStatusCode undefined, string undefinedHtml) = await PostAsync(client, path, [.. fields.Select(f => f.Name == "Input.AddressType" ? (f.Name, "7") : f)], key);
        (HttpStatusCode versionless, string versionlessHtml) = await PostAsync(client, path, [.. fields.Where(f => f.Name != "Input.Version")], key);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, undefined);
        Assert.Contains(OrderDetailModel.AddressTypeMissingText, SectionOf(undefinedHtml, "duzeltmeler"), StringComparison.Ordinal);
        Assert.DoesNotContain(OrderDetailModel.UnchangedText, undefinedHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.Conflict, versionless);
        Assert.Contains(ConflictControlsText, versionlessHtml, StringComparison.Ordinal);
        Assert.DoesNotContain(OrderDetailModel.UnchangedText, versionlessHtml, StringComparison.Ordinal);
        await using AppDbContext reader = NewContext();
        Assert.Equal(0, await reader.OrderAddressHistories.CountAsync(h => h.OrderId == orderId, Ct));
        Assert.Equal(1, await reader.Orders.Where(o => o.Id == orderId).Select(o => o.Version).SingleAsync(Ct));
    }

    // ---- KK3: the carrier ----

    [Fact]
    public async Task CorrectCarrier_OnAShippedOrder_Is303_SendsB12_AndWritesTheTrail_TheEmptyInfoAndAnOrderNotShipped_Are422_KK3()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid shippedId, _) = await StepOrderAsync(OrderStatus.Shipped, PaymentStatus.Paid, (ProductType.Physical, false));
        (Guid preparingId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        await using (AppDbContext db = NewContext())
        {
            // The shipment's record: the delivery section shows the carrier from the shipment on.
            db.TransitionRecords.Add(TransitionRecord.Create(shippedId, TransitionCode.S5, ActorKind.Admin, adminId, CorrectionAdminName, null, null, null, null, null, DateTimeOffset.UtcNow.AddHours(-1)));
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (string, string)[] carrier = [("Input.Version", "1"), ("Input.Carrier", "YurticiKargo"), ("Input.TrackingNumber", "TR-2002")];

        (HttpStatusCode empty, string emptyHtml) = await PostAsync(client, $"{OrdersPath}/{shippedId:D}?handler=CorrectCarrier", [("Input.Version", "1")], key);
        (HttpStatusCode notShipped, string notShippedHtml) = await PostAsync(client, $"{OrdersPath}/{preparingId:D}?handler=CorrectCarrier", carrier, key);
        (HttpStatusCode done, _) = await PostForLocationAsync(client, $"{OrdersPath}/{shippedId:D}?handler=CorrectCarrier", carrier, key);
        (_, string after) = await GetAsync(client, $"{OrdersPath}/{shippedId:D}", key);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, empty);
        Assert.Contains(OrderDetailModel.ShipmentMissingText, FormOf(emptyHtml, OrderDetailModel.CarrierFormId), StringComparison.Ordinal);

        // VAL-A240: the corrected tracking number is on the order's page.
        Assert.Contains("TR-2002", SectionOf(after, "teslimat"), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, notShipped);
        Assert.Contains(OrderDetailModel.NotOpenText, notShippedHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, done);
        await using AppDbContext reader = NewContext();
        Order order = await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == shippedId, Ct);
        Assert.Equal(("YurticiKargo", "TR-2002", 2), (order.Carrier, order.TrackingNumber, order.Version));
        Assert.True(await reader.DispatchRecords.AnyAsync(d => d.OrderId == shippedId && d.DispatchType == DispatchType.B12, Ct));
        AuditLog row = await reader.AuditLogs.AsNoTracking().SingleAsync(a => a.TargetRecordId == shippedId, Ct);
        Assert.Equal(nameof(AuditOperation.CorrectShippingInfo), row.Operation);
        Assert.DoesNotContain("TR-2002", JsonSerializer.Serialize(row.Details), StringComparison.Ordinal);
        Assert.Equal(0, await reader.AuditLogs.CountAsync(a => a.TargetRecordId == preparingId, Ct));
    }

    // ---- KK4: the delivery date ----

    [Fact]
    public async Task CorrectDeliveryDate_RunsTheWithdrawalWindowAndTheDefectPeriodFromTheNewDate_AndWritesTheTrail_KK4()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, Guid[] items) = await StepOrderAsync(OrderStatus.Delivered, PaymentStatus.Paid, (ProductType.Physical, true));
        DateOnly today = TurkeyTime.DateOf(DateTimeOffset.UtcNow);
        await using WebApplicationFactory<Program> factory = Open();
        PanelItemFacts before = await ItemFactsAsync(factory, orderId, items[0]);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode done, _) = await PostForLocationAsync(
            client, $"{OrdersPath}/{orderId:D}?handler=CorrectDeliveryDate", [("Input.Version", "1"), ("Input.DeliveryDate", DateText(today.AddDays(-3)))], key);

        Assert.Equal(HttpStatusCode.SeeOther, done);
        PanelItemFacts after = await ItemFactsAsync(factory, orderId, items[0]);
        Assert.NotNull(before.WithdrawalLastDay);
        Assert.NotNull(before.DefectLastDay);
        Assert.True(after.WithdrawalLastDay < before.WithdrawalLastDay, "The withdrawal window runs from the corrected date.");
        Assert.True(after.DefectLastDay < before.DefectLastDay, "The defect period runs from the corrected date.");
        await using AppDbContext reader = NewContext();
        Assert.Equal(today.AddDays(-3), await reader.OrderItems.Where(i => i.Id == items[0]).Select(i => i.DeliveryDate).SingleAsync(Ct));
        AuditLog row = await reader.AuditLogs.AsNoTracking().SingleAsync(a => a.TargetRecordId == orderId, Ct);
        Assert.Equal(nameof(AuditOperation.CorrectDeliveryDate), row.Operation);
        Assert.Equal(0, await reader.DispatchRecords.CountAsync(d => d.OrderId == orderId, Ct));
    }

    [Fact]
    public async Task CorrectDeliveryDate_AFutureDate_ADateBeforeTheShipment_ADateAfterAReturnsArrival_AndAnUnmarkedOrder_Are422_KK4()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateOnly today = TurkeyTime.DateOf(now);
        (Guid orderId, Guid[] items) = await StepOrderAsync(OrderStatus.Delivered, PaymentStatus.Paid, (ProductType.Physical, true));
        (Guid unmarkedId, _) = await StepOrderAsync(OrderStatus.Shipped, PaymentStatus.Paid, (ProductType.Physical, false));
        await using (AppDbContext db = NewContext())
        {
            db.TransitionRecords.Add(TransitionRecord.Create(orderId, TransitionCode.S5, ActorKind.Admin, adminId, CorrectionAdminName, null, null, null, null, null, now.AddDays(-5)));
            Withdrawal withdrawal = Withdrawal.ByBuyer(items[0], 1, today.AddDays(-2), null, StepAmounts, null, now.AddDays(-2));
            db.Withdrawals.Add(withdrawal);
            db.Returns.Add(Return.ForWithdrawal(items[0], withdrawal.Id, 1, today.AddDays(-1), adminId, CorrectionAdminName, now.AddDays(-1)));
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string path = $"{OrdersPath}/{orderId:D}?handler=CorrectDeliveryDate";

        (HttpStatusCode future, string futureHtml) = await PostAsync(client, path, [("Input.Version", "1"), ("Input.DeliveryDate", DateText(today.AddDays(1)))], key);
        (HttpStatusCode early, string earlyHtml) = await PostAsync(client, path, [("Input.Version", "1"), ("Input.DeliveryDate", DateText(today.AddDays(-9)))], key);
        (HttpStatusCode late, string lateHtml) = await PostAsync(client, path, [("Input.Version", "1"), ("Input.DeliveryDate", DateText(today))], key);
        (HttpStatusCode unmarked, string unmarkedHtml) = await PostAsync(
            client, $"{OrdersPath}/{unmarkedId:D}?handler=CorrectDeliveryDate", [("Input.Version", "1"), ("Input.DeliveryDate", DateText(today.AddDays(-1)))], key);

        Assert.Equal((HttpStatusCode.UnprocessableEntity, HttpStatusCode.UnprocessableEntity, HttpStatusCode.UnprocessableEntity, HttpStatusCode.UnprocessableEntity), (future, early, late, unmarked));
        Assert.Contains(OrderDetailModel.DeliveryDateFutureText, futureHtml, StringComparison.Ordinal);
        Assert.Contains(OrderDetailModel.DeliveryDateBeforeShipmentText, earlyHtml, StringComparison.Ordinal);
        Assert.Contains(OrderDetailModel.DeliveryDateAfterArrivalText, lateHtml, StringComparison.Ordinal);
        Assert.Contains(OrderDetailModel.NotOpenText, unmarkedHtml, StringComparison.Ordinal);
        await using AppDbContext reader = NewContext();
        Assert.Equal(today, await reader.OrderItems.Where(i => i.Id == items[0]).Select(i => i.DeliveryDate).SingleAsync(Ct));
        Assert.Equal(0, await reader.AuditLogs.CountAsync(a => a.TargetRecordId == orderId || a.TargetRecordId == unmarkedId, Ct));
    }

    [Fact]
    public async Task CorrectDeliveryDate_OnAWithdrawalDeclarationsDay_AsksWhichCameFirstInItsForm_ThenTheAnswerWritesTheDate_ShownOnThePage_K608()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateOnly today = TurkeyTime.DateOf(now);
        DateOnly declared = today.AddDays(-2);
        (Guid orderId, Guid[] items) = await StepOrderAsync(OrderStatus.Delivered, PaymentStatus.Paid, (ProductType.Physical, true));
        await using (AppDbContext db = NewContext())
        {
            db.Withdrawals.Add(Withdrawal.ByBuyer(items[0], 1, declared, null, StepAmounts, null, now.AddDays(-2)));
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string path = $"{OrdersPath}/{orderId:D}?handler=CorrectDeliveryDate";

        (HttpStatusCode asked, string askedHtml) = await PostAsync(client, path, [("Input.Version", "1"), ("Input.DeliveryDate", DateText(declared))], key);
        (HttpStatusCode done, _) = await PostForLocationAsync(client, path, [("Input.Version", "1"), ("Input.DeliveryDate", DateText(declared)), ("Input.IsDeliveredBeforeDeclaration", "true")], key);
        (_, string after) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, asked);
        string form = FormOf(askedHtml, OrderDetailModel.DeliveryDateFormId);
        Assert.Contains("id=\"correction-delivery-order-question\"", form, StringComparison.Ordinal);
        Assert.Contains("Beyanın anı: ", form, StringComparison.Ordinal);
        Assert.Contains($"value=\"{DateText(declared)}\"", form, StringComparison.Ordinal);
        Assert.Contains(OrderDetailModel.DeliveryOrderQuestionText, form, StringComparison.Ordinal);
        Assert.DoesNotContain("order-step-message", askedHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, done);
        await using AppDbContext reader = NewContext();
        Assert.Equal(declared, await reader.OrderItems.Where(i => i.Id == items[0]).Select(i => i.DeliveryDate).SingleAsync(Ct));

        // VAL-A240: the corrected delivery date is on the order's page.
        Assert.Contains($"Teslim tarihi: {DisplayFormat.Date(declared)}", SectionOf(after, "kalemler"), StringComparison.Ordinal);
    }

    // ---- KK5: the arrival date ----

    [Fact]
    public async Task CorrectArrivalDate_RunsTheRefundPeriodFromTheNewDate_AndAFutureDateOrOneBeforeTheDelivery_Is422_KK5()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateOnly today = TurkeyTime.DateOf(now);
        (Guid orderId, Guid[] items) = await StepOrderAsync(OrderStatus.Delivered, PaymentStatus.Paid, (ProductType.Physical, false));
        Return receipt;
        DateOnly declared = today.AddDays(-4);
        await using (AppDbContext db = NewContext())
        {
            await db.OrderItems.Where(i => i.Id == items[0]).ExecuteUpdateAsync(s => s.SetProperty(i => i.DeliveryMarkedAt, now.AddDays(-6)).SetProperty(i => i.DeliveryDate, today.AddDays(-6)), Ct);
            Withdrawal withdrawal = Withdrawal.ByBuyer(items[0], 1, declared, null, StepAmounts, null, now.AddDays(-4));
            db.Withdrawals.Add(withdrawal);
            receipt = Return.ForWithdrawal(items[0], withdrawal.Id, 1, today.AddDays(-1), adminId, CorrectionAdminName, now.AddDays(-1));
            db.Returns.Add(receipt);
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string path = $"{OrdersPath}/{orderId:D}?handler=CorrectArrivalDate";
        string record = receipt.Id.ToString("D");

        (HttpStatusCode page, string html) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);
        (HttpStatusCode future, string futureHtml) = await PostAsync(client, path, [("Input.Version", "1"), ("Input.ArrivalRecordId", record), ("Input.ArrivalDate", DateText(today.AddDays(1)))], key);
        (HttpStatusCode early, string earlyHtml) = await PostAsync(client, path, [("Input.Version", "1"), ("Input.ArrivalRecordId", record), ("Input.ArrivalDate", DateText(today.AddDays(-7)))], key);
        (HttpStatusCode done, _) = await PostForLocationAsync(client, path, [("Input.Version", "1"), ("Input.ArrivalRecordId", record), ("Input.ArrivalDate", DateText(today.AddDays(-3)))], key);
        (_, string after) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);

        Assert.Equal(HttpStatusCode.OK, page);
        Assert.Contains($"id=\"correct-arrival-date-form-{record}\"", SectionOf(html, "duzeltmeler"), StringComparison.Ordinal);
        Assert.Equal((HttpStatusCode.UnprocessableEntity, HttpStatusCode.UnprocessableEntity), (future, early));

        // B2: the message is in the receipt's own form and the form keeps the sent date.
        string futureForm = FormOf(futureHtml, OrderDetailModel.ArrivalDateFormId(receipt.Id));
        Assert.Contains(OrderDetailModel.ArrivalDateInvalidText, futureForm, StringComparison.Ordinal);
        Assert.Contains($"value=\"{DateText(today.AddDays(1))}\"", futureForm, StringComparison.Ordinal);
        Assert.Contains(OrderDetailModel.ArrivalDateInvalidText, FormOf(earlyHtml, OrderDetailModel.ArrivalDateFormId(receipt.Id)), StringComparison.Ordinal);
        Assert.DoesNotContain("order-step-message", futureHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, done);

        // VAL-A240: the corrected arrival date is on the order's page.
        Assert.Contains($"Ulaşma tarihi: {DisplayFormat.Date(today.AddDays(-3))}", FormOf(after, OrderDetailModel.ArrivalDateFormId(receipt.Id)), StringComparison.Ordinal);
        await using AppDbContext reader = NewContext();
        DateOnly? arrival = (await reader.Returns.AsNoTracking().SingleAsync(r => r.Id == receipt.Id, Ct)).ArrivalDate;
        Assert.Equal(today.AddDays(-3), arrival);

        // Z-16 runs from the stored arrival (K-712): the refund's start day is the corrected date.
        Assert.Equal(today.AddDays(-3), AfterSalesPeriodRules.WithdrawalRefundStartDay(true, declared, arrival));
        AuditLog row = await reader.AuditLogs.AsNoTracking().SingleAsync(a => a.TargetRecordId == orderId, Ct);
        Assert.Equal(nameof(AuditOperation.CorrectReturnArrivalDate), row.Operation);
    }

    // ---- KK6: the status correction ----

    [Fact]
    public async Task CorrectTransition_AsksAReason_AndTheNoteOfOther_ThenWritesTheCorrectionOnTheCorrectedRecord_AndTheTrail_KK6()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _) = await StepOrderAsync(OrderStatus.Shipped, PaymentStatus.Paid, (ProductType.Physical, false));
        TransitionRecord shipment = TransitionRecord.Create(orderId, TransitionCode.S5, ActorKind.Admin, adminId, CorrectionAdminName, null, null, null, null, null, DateTimeOffset.UtcNow.AddHours(-1));
        await using (AppDbContext db = NewContext())
        {
            db.TransitionRecords.Add(shipment);
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string path = $"{OrdersPath}/{orderId:D}?handler=CorrectTransition";
        string record = shipment.Id.ToString("D");

        (HttpStatusCode page, string html) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);
        (HttpStatusCode noReason, string noReasonHtml) = await PostAsync(client, path, [("Input.Version", "1"), ("Input.TransitionRecordId", record)], key);
        (HttpStatusCode noNote, string noNoteHtml) = await PostAsync(client, path, [("Input.Version", "1"), ("Input.TransitionRecordId", record), ("Input.CorrectionReason", nameof(CorrectionReason.Other))], key);
        (HttpStatusCode done, _) = await PostForLocationAsync(
            client, path, [("Input.Version", "1"), ("Input.TransitionRecordId", record), ("Input.CorrectionReason", nameof(CorrectionReason.Other)), ("Input.ReasonNote", "Yanlış sipariş seçildi")], key);

        Assert.Equal(HttpStatusCode.OK, page);
        string form = FormOf(html, OrderDetailModel.TransitionFormId);
        Assert.Contains($"name=\"Input.TransitionRecordId\" value=\"{record}\"", form, StringComparison.Ordinal);
        Assert.Contains("Düzeltilecek geçiş: Kargoya verildi işareti", form, StringComparison.Ordinal);
        Assert.Contains($"Dönülecek durum: {Shopfolio.Pages.Components.StatusBadges.For(OrderStatus.Preparing).Text}", form, StringComparison.Ordinal);
        Assert.Equal((HttpStatusCode.UnprocessableEntity, HttpStatusCode.UnprocessableEntity), (noReason, noNote));
        Assert.Contains(OrderDetailModel.ReasonMissingText, FormOf(noReasonHtml, OrderDetailModel.TransitionFormId), StringComparison.Ordinal);
        Assert.Contains(OrderDetailModel.ReasonMissingText, FormOf(noNoteHtml, OrderDetailModel.TransitionFormId), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, done);
        await using AppDbContext reader = NewContext();
        Assert.Equal(OrderStatus.Preparing, await reader.Orders.Where(o => o.Id == orderId).Select(o => o.OrderStatus).SingleAsync(Ct));
        TransitionRecord correction = await reader.TransitionRecords.AsNoTracking().SingleAsync(t => t.OrderId == orderId && t.CorrectedTransitionRecordId == shipment.Id, Ct);
        Assert.Equal((CorrectionReason.Other, "Yanlış sipariş seçildi"), (correction.CorrectionReason!.Value, correction.ReasonNote));
        AuditLog row = await reader.AuditLogs.AsNoTracking().SingleAsync(a => a.TargetRecordId == orderId, Ct);
        Assert.Equal(nameof(AuditOperation.CorrectTransition), row.Operation);
        string details = JsonSerializer.Serialize(row.Details);
        Assert.Contains("S5", details, StringComparison.Ordinal);
        Assert.DoesNotContain("Yanlış sipariş seçildi", details, StringComparison.Ordinal);
        Assert.True(await reader.DispatchRecords.AnyAsync(d => d.OrderId == orderId && d.DispatchType == DispatchType.B13, Ct));
    }

    // ---- KK7: the four obstacles, each its own test ----

    [Fact]
    public async Task CorrectTransition_OfAWrongCancellation_WhoseRefundIsUnderway_Is422_RefundPending_KK7()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        CancelledOrder cancelled = await FirmCancelledOrderAsync(adminId, stock: 5);
        await using (AppDbContext db = NewContext())
        {
            Refund refund = Refund.ByAdmin(cancelled.OrderId, RefundType.Cancellation, PaymentMethod.BankTransfer, 200m, adminId, CorrectionAdminName, DateTimeOffset.UtcNow);
            refund.CancellationId = cancelled.CancellationId;
            refund.OrderItemId = cancelled.ItemId;
            refund.Quantity = 2;
            refund.SentAt = DateTimeOffset.UtcNow;
            db.Refunds.Add(refund);
            await db.SaveChangesAsync(Ct);
        }

        (HttpStatusCode status, string html) = await PostCorrectionAsync(key, cancelled.OrderId, cancelled.TransitionRecordId);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);

        // B2: the obstacle is said in the status correction's form (04 2.3.3), which keeps the chosen reason - not in the next step's place.
        string form = FormOf(html, OrderDetailModel.TransitionFormId);
        Assert.Contains(OrderDetailModel.ObstacleTextOf(CorrectionObstacle.RefundPending), form, StringComparison.Ordinal);
        Assert.Contains($"id=\"{OrderDetailModel.CorrectionMessageId}\"", form, StringComparison.Ordinal);
        Assert.Contains($"<option value=\"{nameof(CorrectionReason.WrongOrder)}\" selected=\"selected\"", form, StringComparison.Ordinal);
        Assert.DoesNotContain("order-step-message", html, StringComparison.Ordinal);
        await AssertStillCancelledAsync(cancelled.OrderId);
    }

    [Fact]
    public async Task CorrectTransition_OfAWrongCancellation_WhileTheCardRefundsOutcomeIsAwaited_Is422_RefundPending_KK7()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        CancelledOrder cancelled = await FirmCancelledOrderAsync(adminId, stock: 5);
        await using (AppDbContext db = NewContext())
        {
            await db.Orders.Where(o => o.Id == cancelled.OrderId).ExecuteUpdateAsync(s => s.SetProperty(o => o.PaymentMethod, PaymentMethod.Card).SetProperty(o => o.BankTransferIban, (string?)null), Ct);
            Refund refund = Refund.ByAdmin(cancelled.OrderId, RefundType.Cancellation, PaymentMethod.Card, 200m, adminId, CorrectionAdminName, DateTimeOffset.UtcNow);
            refund.CancellationId = cancelled.CancellationId;
            refund.OrderItemId = cancelled.ItemId;
            refund.Quantity = 2;
            db.Refunds.Add(refund);
            RefundRecord record = RefundRecord.Sending(refund.Id, DateTimeOffset.UtcNow);
            record.State = RefundRecordState.OutcomePending;
            db.RefundRecords.Add(record);
            await db.SaveChangesAsync(Ct);
        }

        (HttpStatusCode status, string html) = await PostCorrectionAsync(key, cancelled.OrderId, cancelled.TransitionRecordId);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.Contains(OrderDetailModel.ObstacleTextOf(CorrectionObstacle.RefundPending), FormOf(html, OrderDetailModel.TransitionFormId), StringComparison.Ordinal);
        await AssertStillCancelledAsync(cancelled.OrderId);
    }

    [Fact]
    public async Task CorrectTransition_OfAWrongCancellation_WhoseUnitsAreNoLongerLeft_Is422_StockShort_KK7()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        CancelledOrder cancelled = await FirmCancelledOrderAsync(adminId, stock: 1);

        (HttpStatusCode status, string html) = await PostCorrectionAsync(key, cancelled.OrderId, cancelled.TransitionRecordId);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.Contains(OrderDetailModel.ObstacleTextOf(CorrectionObstacle.StockShort), html, StringComparison.Ordinal);
        await AssertStillCancelledAsync(cancelled.OrderId);
    }

    [Fact]
    public async Task CorrectTransition_OfTheTransfersPaidMark_OnAnOrderWithAnItemRecord_Is422_ItemRecordExists_KK7()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, Guid[] items) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false), (ProductType.Physical, false));
        TransitionRecord paid = TransitionRecord.Create(orderId, TransitionCode.O1, ActorKind.Admin, adminId, CorrectionAdminName, null, null, null, null, null, DateTimeOffset.UtcNow.AddHours(-2));
        await using (AppDbContext db = NewContext())
        {
            db.TransitionRecords.Add(paid);
            db.Cancellations.Add(Cancellation.ByBuyer(items[1], 1, StepAmounts, null, DateTimeOffset.UtcNow.AddHours(-1)));
            await db.SaveChangesAsync(Ct);
        }

        (HttpStatusCode status, string html) = await PostCorrectionAsync(key, orderId, paid.Id);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.Contains(OrderDetailModel.ObstacleTextOf(CorrectionObstacle.ItemRecordExists), html, StringComparison.Ordinal);
        await using AppDbContext reader = NewContext();
        Assert.Equal(PaymentStatus.Paid, await reader.Orders.Where(o => o.Id == orderId).Select(o => o.PaymentStatus).SingleAsync(Ct));
        Assert.Equal(0, await reader.AuditLogs.CountAsync(a => a.TargetRecordId == orderId, Ct));
    }

    [Fact]
    public async Task CorrectTransition_OfTheCardsPaymentAxis_Is422_CardPayment_KK7()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _) = await StepOrderAsync(OrderStatus.Shipped, PaymentStatus.Paid, (ProductType.Physical, false));
        TransitionRecord paid = TransitionRecord.Create(orderId, TransitionCode.O1, ActorKind.System, null, null, null, null, null, null, null, DateTimeOffset.UtcNow.AddHours(-3));
        await using (AppDbContext db = NewContext())
        {
            await db.Orders.Where(o => o.Id == orderId).ExecuteUpdateAsync(s => s.SetProperty(o => o.PaymentMethod, PaymentMethod.Card).SetProperty(o => o.BankTransferIban, (string?)null), Ct);
            db.TransitionRecords.Add(paid);
            db.TransitionRecords.Add(TransitionRecord.Create(orderId, TransitionCode.S5, ActorKind.Admin, adminId, CorrectionAdminName, null, null, null, null, null, DateTimeOffset.UtcNow.AddHours(-1)));
            await db.SaveChangesAsync(Ct);
        }

        (HttpStatusCode status, string html) = await PostCorrectionAsync(key, orderId, paid.Id);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.Contains(OrderDetailModel.ObstacleTextOf(CorrectionObstacle.CardPayment), html, StringComparison.Ordinal);
        await using AppDbContext reader = NewContext();
        Assert.Equal(OrderStatus.Shipped, await reader.Orders.Where(o => o.Id == orderId).Select(o => o.OrderStatus).SingleAsync(Ct));
    }

    // ---- KK8: the wrong cancellation's correction ----

    [Fact]
    public async Task CorrectTransition_OfAWrongCancellation_TakesTheUnitsAgain_VoidsTheRefund_ClosesTheIbanRequest_AndDeletesTheIban_KK8()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        CancelledOrder cancelled = await FirmCancelledOrderAsync(adminId, stock: 5);
        Refund refund = Refund.ByAdmin(cancelled.OrderId, RefundType.Cancellation, PaymentMethod.BankTransfer, 200m, adminId, CorrectionAdminName, DateTimeOffset.UtcNow);
        refund.CancellationId = cancelled.CancellationId;
        refund.OrderItemId = cancelled.ItemId;
        refund.Quantity = 2;
        RefundIbanRequest request = RefundIbanRequest.ForItem(cancelled.ItemId, RefundIbanRequestReason.CancellationOrRemoval, null, adminId, CorrectionAdminName, DateTimeOffset.UtcNow);
        RefundIban iban = RefundIban.Create(cancelled.OrderId, "TR330006100519786457841326", DateTimeOffset.UtcNow);
        await using (AppDbContext db = NewContext())
        {
            db.Refunds.Add(refund);
            db.RefundIbanRequests.Add(request);
            db.RefundIbans.Add(iban);
            await db.SaveChangesAsync(Ct);
        }

        (HttpStatusCode status, _) = await PostCorrectionAsync(key, cancelled.OrderId, cancelled.TransitionRecordId);

        Assert.Equal(HttpStatusCode.SeeOther, status);
        await using AppDbContext reader = NewContext();
        Order order = await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == cancelled.OrderId, Ct);
        Assert.Equal((OrderStatus.Preparing, PaymentStatus.Paid), (order.OrderStatus, order.PaymentStatus));
        Assert.Equal(3, await reader.Variants.Where(v => v.Id == cancelled.VariantId).Select(v => v.StockQuantity).SingleAsync(Ct));
        Assert.NotNull((await reader.Refunds.AsNoTracking().SingleAsync(r => r.Id == refund.Id, Ct)).VoidedAt);
        Assert.NotNull((await reader.RefundIbanRequests.AsNoTracking().SingleAsync(r => r.Id == request.Id, Ct)).ClosedAt);
        Assert.False(await reader.RefundIbans.AnyAsync(i => i.Id == iban.Id, Ct));
    }

    // ---- KK9, KK10: the guest order's e-mail ----

    [Fact]
    public async Task CorrectOrderEmail_RenewsTheKey_ReroutesTheUnsentNotices_SendsB1ToTheNewAndB15ToTheOld_AndKeepsTheHistory_KK9_KK10()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Digital, false));
        string? oldKey;
        DispatchRecord unsent = DispatchRecord.Queue(DispatchType.B4, "alici@example.com", orderId, null, null, DateTimeOffset.UtcNow);
        await using (AppDbContext db = NewContext())
        {
            oldKey = await db.Orders.Where(o => o.Id == orderId).Select(o => o.OrderAccessKey).SingleAsync(Ct);
            db.DispatchRecords.Add(unsent);
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode page, string html) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);
        (HttpStatusCode done, _) = await PostForLocationAsync(client, $"{OrdersPath}/{orderId:D}?handler=CorrectOrderEmail", [("Input.Version", "1"), ("Input.ContactEmail", "yeni.adres@example.com")], key);
        (_, string after) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);

        // VAL-D027: the page's e-mail history shows the old and the new address with the change's date.
        Assert.Contains($"{DisplayFormat.Date(DateTimeOffset.UtcNow)} · alici@example.com → yeni.adres@example.com", SectionOf(after, "e-posta-gecmisi"), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, page);
        string corrections = SectionOf(html, "duzeltmeler");
        Assert.Contains(OrderDetailModel.EmailReminderText, corrections, StringComparison.Ordinal);
        Assert.Contains("id=\"correct-order-email-form\"", corrections, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, done);
        await using AppDbContext reader = NewContext();
        Order order = await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal("yeni.adres@example.com", order.ContactEmail);
        Assert.NotEqual(oldKey, order.OrderAccessKey);
        Assert.Null(order.CustomerId);
        Assert.Equal("yeni.adres@example.com", (await reader.DispatchRecords.AsNoTracking().SingleAsync(d => d.Id == unsent.Id, Ct)).RecipientEmail);
        List<DispatchRecord> sent = await reader.DispatchRecords.AsNoTracking().Where(d => d.OrderId == orderId && d.Id != unsent.Id).ToListAsync(Ct);
        Assert.Contains(sent, d => d.DispatchType == DispatchType.B1 && d.RecipientEmail == "yeni.adres@example.com");
        Assert.Contains(sent, d => d.DispatchType == DispatchType.B15 && d.RecipientEmail == "alici@example.com");
        OrderEmailHistory history = await reader.OrderEmailHistories.AsNoTracking().SingleAsync(h => h.OrderId == orderId, Ct);
        Assert.Equal(("alici@example.com", "yeni.adres@example.com"), (history.OldEmail, history.NewEmail));
        AuditLog row = await reader.AuditLogs.AsNoTracking().SingleAsync(a => a.TargetRecordId == orderId, Ct);
        Assert.Equal(nameof(AuditOperation.CorrectContactEmail), row.Operation);
        string details = JsonSerializer.Serialize(row.Details);
        Assert.DoesNotContain("yeni.adres", details, StringComparison.Ordinal);
        Assert.DoesNotContain("alici@", details, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CorrectOrderEmail_AVerifiedAccountOfTheNewAddress_TakesTheOrder_KK9()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (_, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        string accountEmail;
        await using (AppDbContext db = NewContext())
        {
            accountEmail = await db.Customers.Where(c => c.Id == customerId).Select(c => c.Email).SingleAsync(Ct);
        }

        (Guid orderId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Digital, false));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode done, _) = await PostForLocationAsync(client, $"{OrdersPath}/{orderId:D}?handler=CorrectOrderEmail", [("Input.Version", "1"), ("Input.ContactEmail", accountEmail)], key);

        Assert.Equal(HttpStatusCode.SeeOther, done);
        await using AppDbContext reader = NewContext();
        Assert.Equal(customerId, await reader.Orders.Where(o => o.Id == orderId).Select(o => o.CustomerId).SingleAsync(Ct));
    }

    [Fact]
    public async Task CorrectOrderEmail_AnInvalidAddress_AnAccountsOrder_AndADisposedOrder_Are422_KK10()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (_, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        (Guid guestId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Digital, false));
        (Guid memberId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Digital, false));
        (Guid disposedId, _) = await StepOrderAsync(OrderStatus.Delivered, PaymentStatus.Paid, (ProductType.Digital, true));
        await using (AppDbContext db = NewContext())
        {
            await db.Orders.Where(o => o.Id == memberId).ExecuteUpdateAsync(s => s.SetProperty(o => o.CustomerId, customerId).SetProperty(o => o.IsGuestOrder, false), Ct);
            Order erased = await db.Orders.SingleAsync(o => o.Id == disposedId, Ct);
            erased.ErasePersonalData();
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (string, string)[] fields = [("Input.Version", "1"), ("Input.ContactEmail", "yeni@example.com")];

        (HttpStatusCode invalid, string invalidHtml) = await PostAsync(client, $"{OrdersPath}/{guestId:D}?handler=CorrectOrderEmail", [("Input.Version", "1"), ("Input.ContactEmail", "yeni@@example")], key);
        (HttpStatusCode member, string memberHtml) = await PostAsync(client, $"{OrdersPath}/{memberId:D}?handler=CorrectOrderEmail", fields, key);
        (HttpStatusCode disposed, string disposedHtml) = await PostAsync(client, $"{OrdersPath}/{disposedId:D}?handler=CorrectOrderEmail", fields, key);

        Assert.Equal((HttpStatusCode.UnprocessableEntity, HttpStatusCode.UnprocessableEntity, HttpStatusCode.UnprocessableEntity), (invalid, member, disposed));
        string invalidForm = FormOf(invalidHtml, OrderDetailModel.OrderEmailFormId);
        Assert.Contains(OrderDetailModel.EmailInvalidText, invalidForm, StringComparison.Ordinal);
        Assert.Contains("value=\"yeni@@example\"", invalidForm, StringComparison.Ordinal);
        Assert.Contains(OrderDetailModel.NotOpenText, memberHtml, StringComparison.Ordinal);
        Assert.Contains(OrderDetailModel.NotOpenText, disposedHtml, StringComparison.Ordinal);
        await using AppDbContext reader = NewContext();
        Assert.Equal(0, await reader.OrderEmailHistories.CountAsync(h => h.OrderId == guestId || h.OrderId == memberId || h.OrderId == disposedId, Ct));
    }

    // ---- helpers ----

    private sealed record CancelledOrder(Guid OrderId, Guid ItemId, Guid VariantId, Guid TransitionRecordId, Guid CancellationId);

    /// <summary>A paid transfer order the firm cancelled wrongly from Preparing (S4 with a reason), its two units back in a variant whose stock is now <paramref name="stock"/>.</summary>
    private async Task<CancelledOrder> FirmCancelledOrderAsync(Guid adminId, int stock)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        (Guid orderId, _, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        string suffix = Guid.NewGuid().ToString("N")[..8];
        Product product = Product.Create("Ürün", $"duzeltme-{suffix}", ProductType.Physical, 20m, now).WithSeededOrigin();
        Variant variant = Variant.Create(product.Id, $"DZ-{suffix}", 100m, now, stock);
        OrderItem item = OrderItem.Create(orderId, product.Id, variant.Id, ProductType.Physical, "Ürün", 2, 100m, 20m, 33.33m, now);
        TransitionRecord cancelled = TransitionRecord.Create(orderId, TransitionCode.S4, ActorKind.Admin, adminId, CorrectionAdminName, null, CancellationReason.OutOfStock, null, null, null, now.AddMinutes(-5));
        Cancellation cancellation = Cancellation.ByAdmin(item.Id, 2, adminId, CorrectionAdminName, CancellationReason.OutOfStock, null, new ItemAmounts(200m, 0m, 33.33m), now.AddMinutes(-5));
        cancellation.TransitionRecordId = cancelled.Id;
        await using AppDbContext db = NewContext();
        db.Products.Add(product);
        db.Variants.Add(variant);
        db.OrderItems.Add(item);
        db.TransitionRecords.Add(cancelled);
        db.Cancellations.Add(cancellation);
        await db.SaveChangesAsync(Ct);
        await db.Orders.Where(o => o.Id == orderId)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.OrderStatus, OrderStatus.Cancelled).SetProperty(o => o.PaymentStatus, PaymentStatus.Paid), Ct);
        return new CancelledOrder(orderId, item.Id, variant.Id, cancelled.Id, cancellation.Id);
    }

    private async Task<(HttpStatusCode Status, string Html)> PostCorrectionAsync(string key, Guid orderId, Guid recordId)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        return await PostAsync(
            client,
            $"{OrdersPath}/{orderId:D}?handler=CorrectTransition",
            [("Input.Version", "1"), ("Input.TransitionRecordId", recordId.ToString("D")), ("Input.CorrectionReason", nameof(CorrectionReason.WrongOrder))],
            key);
    }

    private async Task AssertStillCancelledAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        Order order = await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal((OrderStatus.Cancelled, 1), (order.OrderStatus, order.Version));
        Assert.Equal(0, await reader.AuditLogs.CountAsync(a => a.TargetRecordId == orderId, Ct));
    }

    /// <summary>Gives the order a shipping address with a telephone in the collection's province; returns the province.</summary>
    private async Task<Guid> WithShippingAddressAsync(Guid orderId)
    {
        await using AppDbContext db = NewContext();
        Guid province = await db.Orders.Where(o => o.Id == orderId).Select(o => o.BillingProvinceId!.Value).SingleAsync(Ct);
        await db.Orders.Where(o => o.Id == orderId).ExecuteUpdateAsync(
            s => s.SetProperty(o => o.ShippingRecipientName, "Ali Veli")
                .SetProperty(o => o.ShippingPhone, "5321234567")
                .SetProperty(o => o.ShippingProvinceId, province)
                .SetProperty(o => o.ShippingDistrict, "Cankaya")
                .SetProperty(o => o.ShippingAddressLine, "Eski Sokak 1"),
            Ct);
        return province;
    }

    // The fields one address form sends: its own names (07 UC-146 Shipping…/Billing…) and the address type.
    private static (string Name, string Value)[] AddressFields(int version, AddressType type, Guid province, string line, string? phone)
    {
        string prefix = type == AddressType.ShippingAddress ? OrderDetailModel.ShippingAddressPrefix : OrderDetailModel.BillingAddressPrefix;
        return
        [
            ("Input.Version", version.ToString(CultureInfo.InvariantCulture)),
            ("Input.AddressType", type.ToString()),
            (prefix + "RecipientName", "Ayşe Kaya"),
            (prefix + "ProvinceId", province.ToString("D")),
            (prefix + "District", "Yenimahalle"),
            (prefix + "AddressLine", line),
            .. phone is null ? Array.Empty<(string, string)>() : [(prefix + "Phone", phone)],
        ];
    }

    // The form with the element id, up to its closing tag (the forms do not nest).
    private static string FormOf(string html, string id)
    {
        int start = html.IndexOf($"id=\"{id}\"", StringComparison.Ordinal);
        Assert.True(start >= 0, $"The screen has no form {id}.");
        return html[start..html.IndexOf("</form>", start, StringComparison.Ordinal)];
    }

    // A field's control and what follows it up to the first closing div: its message when it has one.
    private static string FieldOf(string form, string id)
    {
        int start = form.IndexOf($"id=\"{id}\"", StringComparison.Ordinal);
        Assert.True(start >= 0, $"The form has no field {id}.");
        return form[start..form.IndexOf("</div>", start, StringComparison.Ordinal)];
    }

    // 04 2.3.1.1: a label and a summary link find one element; no element id is repeated on the page.
    private static void AssertNoRepeatedId(string html)
    {
        string[] repeated = [.. Regex.Matches(html, "\\sid=\"([^\"]+)\"").Select(m => m.Groups[1].Value).GroupBy(i => i, StringComparer.Ordinal).Where(g => g.Count() > 1).Select(g => g.Key)];
        Assert.Empty(repeated);
    }

    private static async Task<PanelItemFacts> ItemFactsAsync(WebApplicationFactory<Program> factory, Guid orderId, Guid itemId)
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        IPanelOrderReader reader = scope.ServiceProvider.GetRequiredService<IPanelOrderReader>();
        PanelOrderDetailFacts? facts = await reader.ReadDetailAsync(orderId, Ct);
        return Assert.Single(facts!.Items, i => i.OrderItemId == itemId);
    }
}
