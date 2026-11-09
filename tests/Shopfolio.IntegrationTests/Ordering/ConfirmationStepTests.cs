using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Ordering.Documents;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>The collection of the confirmation step's tests (T356): one database copied from the template.</summary>
[CollectionDefinition(ConfirmationStepDefinition.Name)]
public sealed class ConfirmationStepDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "ConfirmationStep";
}

/// <summary>
/// The confirmation step and the order's documents on a real PostgreSQL (T356 KK1-KK8; 11 T356 criteria; 09 9.3 items 4, 5, 7): the step reads the last published form and contract, the form equals the
/// order's frozen copy, a risen version stops the order before the boxes are looked at, the shown text reads the first old value of a corrected address or e-mail and does not fail on a disposed
/// order, the copy holds no personal data, and the schema has no age, birth date, note or consent column. The pure rules are the unit tests' (ConfirmationStepTests).
/// </summary>
[Collection(ConfirmationStepDefinition.Name)]
public sealed class ConfirmationStepIntegrationTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 20, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    private int _counter;

    public ConfirmationStepIntegrationTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE checkbox_acceptance_order_items, checkbox_acceptances, order_email_histories, order_address_histories, order_items, orders, admins, legal_text_versions, provinces CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static Dictionary<string, string?> CompanyValues() =>
        LegalTextTemplates.CompanyTokens.ToDictionary(t => t, t => (string?)(t == "HowTheOrderIsPlaced" ? ProcessGuide.Text : $"<{t}>"), StringComparer.Ordinal);

    private static string FormText() => PlaceholderFiller.Fill(LegalTextTemplates.Get(LegalTextType.PreInformationForm).Text, CompanyValues(), requireAll: false);

    private static string ContractText() => PlaceholderFiller.Fill(LegalTextTemplates.Get(LegalTextType.DistanceSalesContract).Text, CompanyValues(), requireAll: false);

    private static OrderItemSource Source(ProductType type, string name, decimal price, int? fulfilment = null) =>
        new(Guid.NewGuid(), Guid.NewGuid(), type, name, null, null, null, price, 20m, null, null, fulfilment, null);

    private static ItemAmounts Amounts(OrderItemSource source, int quantity)
    {
        decimal amount = source.UnitPrice * quantity;
        decimal vat = Math.Round(amount * source.VatRate / (100m + source.VatRate), 2, MidpointRounding.AwayFromZero);
        return new ItemAmounts(amount, 0m, amount, vat, amount - vat);
    }

    private sealed record Published(Guid FormId, Guid ContractId, Guid ProvinceId);

    private async Task<Published> PublishAsync(string? formText = null, string? contractText = null)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n:D2}", $"İl {n}", Now);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Now, formText ?? FormText(), Now);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Now, contractText ?? ContractText(), Now);
        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.LegalTextVersions.AddRange(form, contract);
        });
        return new Published(form.Id, contract.Id, province.Id);
    }

    private async Task<Guid> PublishFormAgainAsync()
    {
        int n = Interlocked.Increment(ref _counter);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Now.AddMinutes(5), FormText() + " ", Now.AddMinutes(5));
        await SaveAsync(c => c.LegalTextVersions.Add(form));
        return form.Id;
    }

    private ServiceProvider Build() => OrderingTestServices.Build(_collection.Dsn);

    // ---- KK1, KK2, KK4: the check ----

    [Fact]
    public async Task TheCheck_StopsAnOrderWithOnlyOneOfTheTwoBoxes_AndWithAnUntickedDigitalOrServiceBox_AndPassesWhenEveryNeededBoxIsTicked_KK1_KK2()
    {
        Published shown = await PublishAsync();
        ConfirmationSummaryLine physical = ConfirmationSummaryText.LineOf(Source(ProductType.Physical, "Lamba", 100m), 1, Amounts(Source(ProductType.Physical, "Lamba", 100m), 1));
        OrderItemSource digitalSource = Source(ProductType.Digital, "Şablon", 80m);
        ConfirmationSummaryLine digital = ConfirmationSummaryText.LineOf(digitalSource, 1, Amounts(digitalSource, 1));
        OrderItemSource serviceSource = Source(ProductType.Service, "Montaj", 200m, 3);
        ConfirmationSummaryLine service = ConfirmationSummaryText.LineOf(serviceSource, 1, Amounts(serviceSource, 1));

        await using ServiceProvider provider = Build();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IConfirmationStep step = scope.ServiceProvider.GetRequiredService<IConfirmationStep>();

        ConfirmationStepCheck one = await step.CheckAsync(Submission(shown, new TickedBoxes(true, false, false, false), physical), Ct);
        ConfirmationStepCheck other = await step.CheckAsync(Submission(shown, new TickedBoxes(false, true, false, false), physical), Ct);
        ConfirmationStepCheck digitalMissing = await step.CheckAsync(Submission(shown, new TickedBoxes(true, true, false, false), physical, digital), Ct);
        ConfirmationStepCheck serviceMissing = await step.CheckAsync(Submission(shown, new TickedBoxes(true, true, true, false), physical, digital, service), Ct);
        ConfirmationStepCheck ready = await step.CheckAsync(Submission(shown, new TickedBoxes(true, true, true, true), physical, digital, service), Ct);

        AssertMissing(one, ConfirmationBox.DistanceSalesContractAccepted);
        AssertMissing(other, ConfirmationBox.PreInformationFormRead);
        AssertMissing(digitalMissing, ConfirmationBox.DigitalWithdrawalWaiver);
        AssertMissing(serviceMissing, ConfirmationBox.ServiceFulfilmentConsent);
        Assert.True(ready.IsReady);
        await Assert.ThrowsAsync<ArgumentException>(() => step.CheckAsync(Submission(shown, new TickedBoxes(true, true, true, true)), Ct));
    }

    [Fact]
    public async Task ARisenFormVersion_StopsTheOrderBeforeTheBoxesAreLookedAt_EvenWhenEveryBoxIsTicked_KK4()
    {
        Published shown = await PublishAsync();
        ConfirmationSummaryLine line = ConfirmationSummaryText.LineOf(Source(ProductType.Physical, "Lamba", 100m), 1, Amounts(Source(ProductType.Physical, "Lamba", 100m), 1));
        await using ServiceProvider provider = Build();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IConfirmationStep step = scope.ServiceProvider.GetRequiredService<IConfirmationStep>();
        Assert.True((await step.CheckAsync(Submission(shown, new TickedBoxes(true, true, false, false), line), Ct)).IsReady);

        await PublishFormAgainAsync();

        ConfirmationStepCheck risen = await step.CheckAsync(Submission(shown, new TickedBoxes(true, true, false, false), line), Ct);
        ConfirmationStepCheck risenAndUnticked = await step.CheckAsync(Submission(shown, new TickedBoxes(false, false, false, false), line), Ct);
        Assert.True(risen.LegalTextVersionRaised);
        Assert.Empty(risen.MissingBoxes);
        Assert.True(risenAndUnticked.LegalTextVersionRaised);
        Assert.False(risenAndUnticked.IsReady);

        // The customer is shown the current text next: the view carries the new form's id and the check passes with it.
        ConfirmationStepView view = await step.GetViewAsync(new ConfirmationSummary([line], 0m, 100m, PaymentMethod.Card, 2, null, null, null), Ct);
        Assert.NotEqual(shown.FormId, view.ShownPreInformationFormId);
        Assert.Equal(shown.ContractId, view.ShownDistanceSalesContractId);
        Assert.True((await step.CheckAsync(new ConfirmationStepSubmission(view.ShownPreInformationFormId, view.ShownDistanceSalesContractId, new TickedBoxes(true, true, false, false), [line], PaymentMethod.Card), Ct)).IsReady);
    }

    private static void AssertMissing(ConfirmationStepCheck check, params ConfirmationBox[] expected)
    {
        Assert.False(check.LegalTextVersionRaised);
        Assert.Equal(expected, check.MissingBoxes);
        Assert.False(check.IsReady);
    }

    private static ConfirmationStepSubmission Submission(Published shown, TickedBoxes ticked, params ConfirmationSummaryLine[] lines) =>
        new(shown.FormId, shown.ContractId, ticked, lines, PaymentMethod.Card);

    // ---- KK1, KK3, KK6: the view ----

    [Fact]
    public async Task TheView_CarriesTheButtonWithThePaymentObligation_ForBothMethods_TheBoxesTheCayma_AndTheGuestsEmail_AndHasNoConsentAgeOrNoteField_KK1_KK5_KK6_KK8()
    {
        await PublishAsync();
        OrderItemSource digitalSource = Source(ProductType.Digital, "Şablon", 80m);
        OrderItemSource physicalSource = Source(ProductType.Physical, "Lamba", 100m);
        ConfirmationSummaryLine[] lines = [ConfirmationSummaryText.LineOf(physicalSource, 1, Amounts(physicalSource, 1)), ConfirmationSummaryText.LineOf(digitalSource, 1, Amounts(digitalSource, 1))];
        await using ServiceProvider provider = Build();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IConfirmationStep step = scope.ServiceProvider.GetRequiredService<IConfirmationStep>();

        ConfirmationStepView card = await step.GetViewAsync(new ConfirmationSummary(lines, 49.90m, 229.90m, PaymentMethod.Card, 2, "misafir@example.test", null, null), Ct);
        ConfirmationStepView transfer = await step.GetViewAsync(new ConfirmationSummary(lines, 49.90m, 229.90m, PaymentMethod.BankTransfer, 2, null, null, null), Ct);

        Assert.Equal("Siparişi onayla — ödeme yükümlülüğü doğar", card.ConfirmButtonText);
        Assert.Equal(card.ConfirmButtonText, transfer.ConfirmButtonText);
        Assert.Equal(card.PaymentObligationNotice, transfer.PaymentObligationNotice);
        Assert.Contains("ödeme yükümlülüğü doğurur", transfer.PaymentObligationNotice, StringComparison.Ordinal);
        Assert.Equal([ConfirmationBox.PreInformationFormRead, ConfirmationBox.DistanceSalesContractAccepted, ConfirmationBox.DigitalWithdrawalWaiver], card.Boxes.Select(b => b.Box));
        Assert.DoesNotContain("firma ödememi", card.Boxes[2].Text, StringComparison.Ordinal);
        Assert.Contains("firma ödememi onayladığında", transfer.Boxes[2].Text, StringComparison.Ordinal);
        Assert.Equal("Siparişiniz şu adrese gönderilecek: misafir@example.test", card.ContactEmailLine);
        Assert.Null(transfer.ContactEmailLine);
        Assert.Equal(ProcessGuide.Text, card.HowTheOrderIsPlacedText);
        Assert.Contains("on dört gün", card.WithdrawalSummaryText, StringComparison.Ordinal);
        Assert.All(card.Boxes, b => Assert.DoesNotContain("kişisel verilerimin", b.Text, StringComparison.OrdinalIgnoreCase));
        Assert.All(card.Boxes, b => Assert.DoesNotContain("yaşını", b.Text, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task TheFormInTheView_IsTheSummaryFollowedByTheGeneratedText_AndEqualsTheOrdersFrozenCopyOnceTheOrderIsBorn_KK3()
    {
        Published published = await PublishAsync();
        OrderItemSource physicalSource = Source(ProductType.Physical, "Lamba", 100m);
        OrderItemSource serviceSource = Source(ProductType.Service, "Montaj", 200m, 4);
        ConfirmationSummaryLine[] lines =
        [
            ConfirmationSummaryText.LineOf(physicalSource, 2, Amounts(physicalSource, 2)),
            ConfirmationSummaryText.LineOf(serviceSource, 1, Amounts(serviceSource, 1)),
        ];
        PartyAddress shipping = new("Ali Veli", "5321234567", published.ProvinceId, "Çankaya", "Atatürk Bulvarı 1");
        PartyAddress billing = new("Ali Veli", null, published.ProvinceId, "Çankaya", "Atatürk Bulvarı 1");
        ConfirmationSummary summary = new(lines, 49.90m, 449.90m, PaymentMethod.Card, 2, "Ali@Example.com", shipping, billing);
        await using ServiceProvider provider = Build();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        ConfirmationStepView view = await scope.ServiceProvider.GetRequiredService<IConfirmationStep>().GetViewAsync(summary, Ct);

        // The form is the summary, word for word, and then the generated text with the customer's own values.
        Assert.StartsWith(view.SummaryText + "\n\n", view.PreInformationFormText, StringComparison.Ordinal);
        Assert.Equal(ConfirmationSummaryText.Render(summary), view.SummaryText);
        Assert.Contains("Teslimat bilgileri: Ali Veli, 5321234567, Atatürk Bulvarı 1, Çankaya / İl 1", view.PreInformationFormText, StringComparison.Ordinal);
        Assert.Contains("Sipariş numarası: sipariş onayında verilir", view.PreInformationFormText, StringComparison.Ordinal);
        Assert.Contains("yalnızca satıcının teslimat yaptığı illere", view.PreInformationFormText, StringComparison.Ordinal);
        Assert.Contains("Meslek odası: <CompanyChamberName>", view.PreInformationFormText, StringComparison.Ordinal);
        Assert.Contains("İfa süresi: 4 gün", view.PreInformationFormText, StringComparison.Ordinal);
        Assert.Contains(ProcessGuide.Text, view.PreInformationFormText, StringComparison.Ordinal);
        Assert.Empty(PlaceholderFiller.TokenNames(view.PreInformationFormText));
        Assert.Empty(PlaceholderFiller.TokenNames(view.DistanceSalesContractText));
        Assert.Equal(published.FormId, view.ShownPreInformationFormId);

        // The order is born with the same texts: the frozen copy shown through the one function equals the step's text but for the order's number.
        Guid orderId = await SaveOrderAsync(published, summary, physicalSource, serviceSource);
        string shownForm = await GetTextAsync(orderId, LegalTextType.PreInformationForm);
        string shownContract = await GetTextAsync(orderId, LegalTextType.DistanceSalesContract);
        Assert.Equal(view.PreInformationFormText.Replace(OrderDocuments.OrderNumberBeforeConfirmation, "2026-000001", StringComparison.Ordinal), shownForm);
        Assert.Equal(view.DistanceSalesContractText.Replace(OrderDocuments.OrderNumberBeforeConfirmation, "2026-000001", StringComparison.Ordinal), shownContract);

        // The copy in the table holds no personal data: the tokens stand.
        await using AppDbContext db = NewContext();
        Order stored = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        foreach (string text in new[] { stored.PreInformationFormText, stored.DistanceSalesContractText })
        {
            Assert.DoesNotContain("Ali Veli", text, StringComparison.Ordinal);
            Assert.DoesNotContain("5321234567", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Ali@Example.com", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Atatürk Bulvarı", text, StringComparison.Ordinal);
        }

        Assert.Contains("{{ShippingRecipientName}}", stored.PreInformationFormText, StringComparison.Ordinal);
        Assert.Contains("2026-000001", stored.PreInformationFormText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheView_OfASummaryWithNoPublishedVersion_IsAnInvalidOperation()
    {
        ConfirmationSummaryLine line = ConfirmationSummaryText.LineOf(Source(ProductType.Physical, "Lamba", 100m), 1, Amounts(Source(ProductType.Physical, "Lamba", 100m), 1));
        await using ServiceProvider provider = Build();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();

        await Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<IConfirmationStep>().GetViewAsync(new ConfirmationSummary([line], 0m, 100m, PaymentMethod.Card, 2, null, null, null), Ct));
    }

    // ---- 09 9.3 items 5 and 7: the shown text reads the confirmation's parties ----

    [Fact]
    public async Task TheShownText_ReadsTheFirstOldValueOfACorrectedAddressAndEmail_TheCurrentOneWithoutACorrection_AndDoesNotFailAfterTheDisposal()
    {
        Published published = await PublishAsync("Ad: {{ShippingRecipientName}}, {{ShippingDistrict}} / {{ShippingProvince}}, {{ShippingAddressLine}}, {{ShippingPhone}}; Fatura: {{BillingRecipientName}}, {{BillingDistrict}} / {{BillingProvince}}; E-posta: {{ContactEmail}}; Toplam {{Total}}", "Sözleşme {{OrderNumber}} {{ContactEmail}}");
        OrderItemSource source = Source(ProductType.Physical, "Lamba", 100m);
        PartyAddress shipping = new("Ali Veli", "5321234567", published.ProvinceId, "Çankaya", "Atatürk Bulvarı 1");
        ConfirmationSummary summary = new([ConfirmationSummaryText.LineOf(source, 1, Amounts(source, 1))], 0m, 100m, PaymentMethod.Card, 2, "Ali@Example.com", shipping, new PartyAddress("Ali Veli", null, published.ProvinceId, "Çankaya", "Atatürk Bulvarı 1"));
        Guid orderId = await SaveOrderAsync(published, summary, source);
        string expected = "Ad: Ali Veli, Çankaya / İl 1, Atatürk Bulvarı 1, 5321234567; Fatura: Ali Veli, Çankaya / İl 1; E-posta: Ali@Example.com; Toplam 100 TL";

        // No correction: the order's own values.
        Assert.EndsWith(expected, await GetTextAsync(orderId, LegalTextType.PreInformationForm), StringComparison.Ordinal);

        // Two corrections of the address and one of the e-mail: the first old value wins (K-1027).
        Admin admin = Admin.Create("Veli", $"veli-{EntityIds.New():N}@example.com", "digest-placeholder", Now);
        Guid province = published.ProvinceId;
        await SaveAsync(c =>
        {
            c.Admins.Add(admin);
            c.OrderAddressHistories.Add(OrderAddressHistory.Create(
                orderId, AddressType.ShippingAddress, ("Ali Veli", "5321234567", province, "Çankaya", "Atatürk Bulvarı 1"), ("Ali Veli", "5329999999", province, "Keçiören", "Yeni Sokak 2"), admin.Id, admin.Name, Now.AddHours(1)));
            c.OrderAddressHistories.Add(OrderAddressHistory.Create(
                orderId, AddressType.ShippingAddress, ("Ali Veli", "5329999999", province, "Keçiören", "Yeni Sokak 2"), ("Ali Veli", "5328888888", province, "Mamak", "Son Sokak 3"), admin.Id, admin.Name, Now.AddHours(2)));
            c.OrderEmailHistories.Add(OrderEmailHistory.Create(orderId, "Ali@Example.com", "yeni@example.com", admin.Id, admin.Name, Now.AddHours(1)));
        });
        await ExecuteAsync(
            $"UPDATE orders SET shipping_phone = '5328888888', shipping_district = 'Mamak', shipping_address_line = 'Son Sokak 3', contact_email = 'yeni@example.com', normalized_contact_email = 'yeni@example.com' WHERE id = '{orderId}'");

        string corrected = await GetTextAsync(orderId, LegalTextType.PreInformationForm);
        Assert.EndsWith(expected, corrected, StringComparison.Ordinal);
        Assert.DoesNotContain("Mamak", corrected, StringComparison.Ordinal);
        Assert.DoesNotContain("yeni@example.com", corrected, StringComparison.Ordinal);
        Assert.Equal("Sözleşme 2026-000001 Ali@Example.com", await GetTextAsync(orderId, LegalTextType.DistanceSalesContract));

        // The disposal empties the order's fields and deletes the histories: the text is still produced, the personal values are empty.
        await ExecuteAsync("DELETE FROM order_address_histories; DELETE FROM order_email_histories");
        await using (AppDbContext db = NewContext())
        {
            Order order = await db.Orders.SingleAsync(o => o.Id == orderId, Ct);
            order.ErasePersonalData();
            await db.SaveChangesAsync(Ct);
        }

        string disposed = await GetTextAsync(orderId, LegalTextType.PreInformationForm);
        Assert.EndsWith("Ad: ,  / , , ; Fatura: ,  / ; E-posta: ; Toplam 100 TL", disposed, StringComparison.Ordinal);
        Assert.Equal("Sözleşme 2026-000001 ", await GetTextAsync(orderId, LegalTextType.DistanceSalesContract));
    }

    [Fact]
    public async Task TheFourCallersOfTheDocument_AreOneFunction_AndAnotherTypeOrAMissingOrderIsRefused()
    {
        Published published = await PublishAsync("Ad: {{ShippingRecipientName}}", "Sözleşme");
        OrderItemSource source = Source(ProductType.Physical, "Lamba", 100m);
        ConfirmationSummary summary = new([ConfirmationSummaryText.LineOf(source, 1, Amounts(source, 1))], 0m, 100m, PaymentMethod.Card, 2, "a@example.test", new PartyAddress("Ali Veli", "5321234567", published.ProvinceId, "Çankaya", "Atatürk Bulvarı 1"), null);
        Guid orderId = await SaveOrderAsync(published, summary, source);

        // The order page, the panel, the e-mail's body and the resend all take the one registered face.
        string[] texts = new string[4];
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i] = await GetTextAsync(orderId, LegalTextType.PreInformationForm);
        }

        Assert.Single(texts.Distinct(StringComparer.Ordinal));
        await using ServiceProvider provider = Build();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IOrderDocuments documents = scope.ServiceProvider.GetRequiredService<IOrderDocuments>();
        Assert.IsType<OrderDocuments>(documents);
        await Assert.ThrowsAsync<ArgumentException>(() => documents.GetTextAsync(orderId, LegalTextType.PrivacyNotice, Ct));
        await Assert.ThrowsAsync<ArgumentException>(() => documents.GetTextAsync(orderId, LegalTextType.CookiePolicy, Ct));
        await Assert.ThrowsAsync<InvalidOperationException>(() => documents.GetTextAsync(Guid.NewGuid(), LegalTextType.PreInformationForm, Ct));
    }

    // ---- KK7: the box record carries the box, its text and its items ----

    [Fact]
    public async Task TheBoxRecords_ProducedByTheStep_AreWrittenWithTheirTextAndItems_AndKeepTheirTextWhenTheWordingChangesLater_KK7()
    {
        Published published = await PublishAsync();
        OrderItemSource physical = Source(ProductType.Physical, "Lamba", 100m);
        OrderItemSource digital = Source(ProductType.Digital, "Şablon", 80m);
        ConfirmationSummary summary = new(
            [ConfirmationSummaryText.LineOf(physical, 1, Amounts(physical, 1)), ConfirmationSummaryText.LineOf(digital, 1, Amounts(digital, 1))], 0m, 180m, PaymentMethod.Card, 2, null, null, null);
        Guid orderId = await SaveOrderAsync(published, summary, physical, digital);
        await using ServiceProvider provider = Build();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        ConfirmationStepView view = await scope.ServiceProvider.GetRequiredService<IConfirmationStep>().GetViewAsync(summary, Ct);
        await using AppDbContext db = NewContext();
        List<OrderItem> items = await db.OrderItems.AsNoTracking().Where(i => i.OrderId == orderId).ToListAsync(Ct);

        var records = ConfirmationAcceptances.Records(orderId, view.Boxes, items, Now);
        await SaveAsync(c =>
        {
            foreach (var (acceptance, links) in records)
            {
                c.CheckboxAcceptances.Add(acceptance);
                c.CheckboxAcceptanceOrderItems.AddRange(links);
            }
        });

        await using AppDbContext read = NewContext();
        CheckboxAcceptance saved = await read.CheckboxAcceptances.AsNoTracking().SingleAsync(a => a.OrderId == orderId, Ct);
        Assert.Equal(CheckboxType.Digital, saved.CheckboxType);
        Assert.Equal(view.Boxes.Single(b => b.Box == ConfirmationBox.DigitalWithdrawalWaiver).Text, saved.ShownText);
        Guid digitalItem = items.Single(i => i.ProductType == ProductType.Digital).Id;
        Assert.Equal(digitalItem, (await read.CheckboxAcceptanceOrderItems.AsNoTracking().SingleAsync(l => l.CheckboxAcceptanceId == saved.Id, Ct)).OrderItemId);
    }

    // ---- KK5, KK8: the schema ----

    [Fact]
    public async Task TheOrderTheCartAndTheCustomer_HaveNoAgeBirthDateNoteInstructionOrConsentColumn_AndThereIsNoMembershipAgreementTable_KK5_KK8()
    {
        await using AppDbContext db = NewContext();
        string[] forbidden = ["age", "birth", "birthdate", "dob", "note", "notes", "instruction", "instructions", "comment", "kvkk", "consent", "agreement"];
        Type[] types = [typeof(Order), typeof(OrderItem), typeof(CheckboxAcceptance)];
        foreach (Type type in types)
        {
            Assert.All(
                db.Model.FindEntityType(type)!.GetProperties().Select(p => p.GetColumnName()),
                column => Assert.DoesNotContain(column.Split('_'), segment => forbidden.Contains(segment, StringComparer.Ordinal)));
        }

        // The cart and the customer are read from the real database's catalog, whatever their entity types are called.
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(
            "SELECT table_name || '.' || column_name FROM information_schema.columns WHERE table_schema = 'public' AND table_name IN ('orders', 'carts', 'cart_items', 'customers', 'order_items')", connection);
        List<string> columns = [];
        await using (NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct))
        {
            while (await reader.ReadAsync(Ct))
            {
                columns.Add(reader.GetString(0));
            }
        }

        Assert.Contains("carts.id", columns);
        Assert.Contains("customers.id", columns);
        Assert.All(columns, c => Assert.DoesNotContain(c.Split('.')[1].Split('_'), segment => forbidden.Contains(segment, StringComparer.Ordinal)));

        await using NpgsqlCommand tables = new("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'", connection);
        List<string> names = [];
        await using (NpgsqlDataReader reader = await tables.ExecuteReaderAsync(Ct))
        {
            while (await reader.ReadAsync(Ct))
            {
                names.Add(reader.GetString(0));
            }
        }

        Assert.DoesNotContain(names, n => n.Contains("agreement", StringComparison.OrdinalIgnoreCase) || n.Contains("consent", StringComparison.OrdinalIgnoreCase));
    }

    // ---- helpers ----

    private async Task<string> GetTextAsync(Guid orderId, LegalTextType type)
    {
        await using ServiceProvider provider = Build();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IOrderDocuments>().GetTextAsync(orderId, type, Ct);
    }

    // The confirmation's birth as T348 will call it: the items frozen, the documents produced from the last published texts, the order saved once.
    private async Task<Guid> SaveOrderAsync(Published published, ConfirmationSummary summary, params OrderItemSource[] sources)
    {
        int n = Interlocked.Increment(ref _counter);
        Order order = Order.Create($"2026-{1:D6}", $"access-{n}-{Guid.NewGuid():N}", $"confirm-{n}-{Guid.NewGuid():N}", summary.PaymentMethod, true, summary.ContactEmail ?? "x@example.test", summary.Total, summary.ShippingFee, Now.AddHours(1), Now);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        bool hasPhysical = sources.Any(s => s.ProductType == ProductType.Physical);
        PartyAddress? shipping = summary.Shipping;
        PartyAddress? billing = summary.Billing ?? new PartyAddress("Fatura", null, published.ProvinceId, "Çankaya", "Atatürk Bulvarı 1");
        order.FreezeAddresses(
            hasPhysical && shipping is not null ? (shipping.RecipientName, shipping.Phone ?? "5320000000", shipping.ProvinceId, shipping.District, shipping.AddressLine) : null,
            (billing.RecipientName, billing.ProvinceId, billing.District, billing.AddressLine));
        CompanyIdentity identity = new(
            1, CompanyType.Tradesman, "Örnek Ticaret", "1234567890", "0123456789012345", null, published.ProvinceId, "Çankaya", "Kızılay 2", "3121234567", "firma@example.com",
            "firma@hs01.kep.tr", "Ankara Esnaf Odası", null, "Meslek kuralları", null);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Now, "metin", Now).WithHosting();
        string? iban = summary.PaymentMethod == PaymentMethod.BankTransfer ? "TR330006100519786457841326" : null;
        OrderFreeze.FreezeLegalIdentity(order, identity, new LegalTextVersionIds(published.FormId, published.ContractId, notice.Id), iban);
        order.FreezeShippingFeeVatBreakdown(summary.ShippingFee > 0 ? [new ShippingVatPart(20m, summary.ShippingFee, Math.Round(summary.ShippingFee * 20m / 120m, 2, MidpointRounding.AwayFromZero))] : []);
        order.FreezeShippingPromise(summary.ShippingPeriod);

        List<OrderItem> items = [];
        for (int i = 0; i < sources.Length; i++)
        {
            ConfirmationSummaryLine line = summary.Lines[i];
            OrderItem item = OrderFreeze.FreezeItem(order.Id, sources[i], line.Quantity, Amounts(sources[i], line.Quantity), 3, Now);

            // The items of these tests have no product rows: the frozen item stands without the product links.
            item.ProductId = null;
            item.VariantId = null;
            items.Add(item);
        }

        await using AppDbContext read = NewContext();
        string formVersion = (await read.LegalTextVersions.AsNoTracking().SingleAsync(v => v.Id == published.FormId, Ct)).Text;
        string contractVersion = (await read.LegalTextVersions.AsNoTracking().SingleAsync(v => v.Id == published.ContractId, Ct)).Text;
        (string form, string contract) = OrderDocuments.Produce(order, items, formVersion, contractVersion);
        order.FreezeDocuments(form, contract);
        await SaveAsync(c =>
        {
            c.LegalTextVersions.Add(notice);
            c.Orders.Add(order);
            c.OrderItems.AddRange(items);
        });
        return order.Id;
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
}
