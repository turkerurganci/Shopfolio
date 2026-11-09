using System.Reflection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Ordering.Documents;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// The confirmation step's rules and texts that need no database (T356 KK1-KK9; 11 T356 criteria 1-9; 02 3.24.1-3.24.8): the boxes an order needs and the unticked ones, the box texts, the
/// button, the withdrawal summary, the summary written from the product's source and from the frozen item, the closed set of the step's fields, the form's added content and the process guide.
/// </summary>
public sealed class ConfirmationStepTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 12, 0, 0, TimeSpan.Zero);

    private static OrderItemSource Source(
        ProductType type = ProductType.Physical, string name = "Lamba", decimal price = 1200m, DiscountLine? discount = null, WithdrawalExemptionReason? exemption = null, int? fulfilment = null) =>
        new(Guid.NewGuid(), Guid.NewGuid(), type, name, "Kırmızı", null, null, price, 20m, discount, exemption, fulfilment, null);

    private static ItemAmounts Amounts(OrderItemSource source, int quantity, decimal couponShare)
    {
        decimal amount = (source.Discount?.DiscountedPrice ?? source.UnitPrice) * quantity;
        decimal payable = amount - couponShare;
        decimal vat = Math.Round(payable * source.VatRate / (100m + source.VatRate), 2, MidpointRounding.AwayFromZero);
        return new ItemAmounts(amount, couponShare, payable, vat, payable - vat);
    }

    private static ConfirmationSummaryLine Line(ProductType type, string name = "Ürün") =>
        ConfirmationSummaryText.LineOf(Source(type, name, 100m, null, null, type == ProductType.Service ? 3 : null), 1, Amounts(Source(type, name, 100m), 1, 0m));

    // ---- KK1, KK2: the boxes ----

    [Fact]
    public void NeededBoxes_AreTheFormAndTheContractAlways_TheDigitalBoxWithADigitalLine_TheServiceBoxWithAService()
    {
        Assert.Equal([ConfirmationBox.PreInformationFormRead, ConfirmationBox.DistanceSalesContractAccepted], ConfirmationStepRules.NeededBoxes([Line(ProductType.Physical)]));
        Assert.Equal(
            [ConfirmationBox.PreInformationFormRead, ConfirmationBox.DistanceSalesContractAccepted, ConfirmationBox.DigitalWithdrawalWaiver],
            ConfirmationStepRules.NeededBoxes([Line(ProductType.Physical), Line(ProductType.Digital)]));
        Assert.Equal(
            [ConfirmationBox.PreInformationFormRead, ConfirmationBox.DistanceSalesContractAccepted, ConfirmationBox.ServiceFulfilmentConsent],
            ConfirmationStepRules.NeededBoxes([Line(ProductType.Service)]));
        Assert.Equal(ConfirmationBox.All, ConfirmationStepRules.NeededBoxes([Line(ProductType.Digital), Line(ProductType.Service)]));
        Assert.Throws<ArgumentException>(() => ConfirmationStepRules.NeededBoxes([]));
    }

    [Fact]
    public void MissingBoxes_OneBoxAloneIsNotEnough_ForEveryOfTheTwoAlwaysBoxes()
    {
        IReadOnlyList<ConfirmationSummaryLine> lines = [Line(ProductType.Physical)];

        Assert.Equal([ConfirmationBox.DistanceSalesContractAccepted], ConfirmationStepRules.MissingBoxes(lines, new TickedBoxes(true, false, false, false)));
        Assert.Equal([ConfirmationBox.PreInformationFormRead], ConfirmationStepRules.MissingBoxes(lines, new TickedBoxes(false, true, false, false)));
        Assert.Equal([ConfirmationBox.PreInformationFormRead, ConfirmationBox.DistanceSalesContractAccepted], ConfirmationStepRules.MissingBoxes(lines, new TickedBoxes(false, false, false, false)));
        Assert.Empty(ConfirmationStepRules.MissingBoxes(lines, new TickedBoxes(true, true, false, false)));
    }

    [Fact]
    public void MissingBoxes_TheDigitalAndTheServiceBoxAreRequired_WhenTheCartCallsForThem_AndATickOfAnotherIsIgnored()
    {
        IReadOnlyList<ConfirmationSummaryLine> mixed = [Line(ProductType.Digital), Line(ProductType.Service)];

        Assert.Equal([ConfirmationBox.DigitalWithdrawalWaiver, ConfirmationBox.ServiceFulfilmentConsent], ConfirmationStepRules.MissingBoxes(mixed, new TickedBoxes(true, true, false, false)));
        Assert.Equal([ConfirmationBox.ServiceFulfilmentConsent], ConfirmationStepRules.MissingBoxes(mixed, new TickedBoxes(true, true, true, false)));
        Assert.Equal([ConfirmationBox.DigitalWithdrawalWaiver], ConfirmationStepRules.MissingBoxes(mixed, new TickedBoxes(true, true, false, true)));
        Assert.Empty(ConfirmationStepRules.MissingBoxes(mixed, new TickedBoxes(true, true, true, true)));

        // A physical-only cart does not need the digital box, and ticking it changes nothing.
        Assert.Empty(ConfirmationStepRules.MissingBoxes([Line(ProductType.Physical)], new TickedBoxes(true, true, true, true)));
    }

    [Fact]
    public void TheButton_SaysThePaymentObligation_ForTheCardAndTheBankTransferAlike()
    {
        Assert.Equal("Siparişi onayla — ödeme yükümlülüğü doğar", ConfirmationStepRules.ConfirmButtonText);
        Assert.Contains("ödeme yükümlülüğü doğurur", ConfirmationStepRules.PaymentObligationNotice, StringComparison.Ordinal);

        // The button and the notice are constants of the rule: no method is an input to them.
        Assert.All(Enum.GetValues<PaymentMethod>(), method =>
            Assert.All(ConfirmationStepRules.Offers([Line(ProductType.Physical)], method), offer => Assert.DoesNotContain("ödeme yükümlülüğü", offer.Text, StringComparison.Ordinal)));
    }

    // ---- KK2, KK5, KK8: the closed set of the step (no consent box, no age box, no note field) ----

    [Fact]
    public void TheBoxSet_IsClosedToTheFourContractBoxes_AndNoStepRecordHasAConsentAgeBirthOrNoteField()
    {
        Assert.Equal(4, ConfirmationBox.All.Count);
        Assert.Equal(4, typeof(TickedBoxes).GetProperties().Length);

        string[] forbidden = ["Kvkk", "Privacy", "Marketing", "Age", "Birth", "Note", "Instruction", "Comment", "Membership"];
        foreach (Type type in new[] { typeof(TickedBoxes), typeof(ConfirmationStepView), typeof(ConfirmationStepSubmission), typeof(ConfirmationSummary), typeof(ConfirmationBoxOffer) })
        {
            Assert.All(type.GetProperties().Select(p => p.Name), name => Assert.DoesNotContain(forbidden, f => name.Contains(f, StringComparison.OrdinalIgnoreCase)));
        }

        Assert.All(ConfirmationBox.All.Select(b => b.Name), name => Assert.DoesNotContain(forbidden, f => name.Contains(f, StringComparison.OrdinalIgnoreCase)));
    }

    // ---- K-771: the box texts ----

    [Fact]
    public void TheDigitalBox_NamesTheItemsItCovers_InThePlural_AndTheBankTransferTextDoesNotSayImmediately()
    {
        ConfirmationSummaryLine one = Line(ProductType.Digital, "Şablon paketi");
        ConfirmationSummaryLine two = Line(ProductType.Digital, "Yazı tipi");

        ConfirmationBoxOffer card = ConfirmationStepRules.Offers([one], PaymentMethod.Card).Single(o => o.Box == ConfirmationBox.DigitalWithdrawalWaiver);
        ConfirmationBoxOffer transfer = ConfirmationStepRules.Offers([one], PaymentMethod.BankTransfer).Single(o => o.Box == ConfirmationBox.DigitalWithdrawalWaiver);
        ConfirmationBoxOffer plural = ConfirmationStepRules.Offers([one, two], PaymentMethod.Card).Single(o => o.Box == ConfirmationBox.DigitalWithdrawalWaiver);

        Assert.Equal("İndirme, ödemem onaylandığı anda açılacak; \"Şablon paketi (Kırmızı)\" ürününde cayma hakkımın düşeceğini kabul ediyorum.", card.Text);
        Assert.Contains("firma ödememi onayladığında", transfer.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("hemen", transfer.Text, StringComparison.Ordinal);
        Assert.Equal(["Şablon paketi (Kırmızı)"], card.CoveredItemNames);
        Assert.Equal(["Şablon paketi (Kırmızı)", "Yazı tipi (Kırmızı)"], plural.CoveredItemNames);
        Assert.Contains("ürünlerinde cayma haklarımın düşeceğini", plural.Text, StringComparison.Ordinal);
        Assert.Contains("\"Yazı tipi (Kırmızı)\"", plural.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheServiceBox_NamesTheServices_AndSaysThatTheRightEndsWhenTheFulfilmentIsDone()
    {
        ConfirmationBoxOffer single = ConfirmationStepRules.Offers([Line(ProductType.Service, "Montaj")], PaymentMethod.Card).Single(o => o.Box == ConfirmationBox.ServiceFulfilmentConsent);
        ConfirmationBoxOffer plural = ConfirmationStepRules.Offers([Line(ProductType.Service, "Montaj"), Line(ProductType.Service, "Bakım")], PaymentMethod.BankTransfer)
            .Single(o => o.Box == ConfirmationBox.ServiceFulfilmentConsent);

        Assert.Contains("cayma süresi dolmadan ifasına başlanmasını onaylıyorum; ifa tamamlandığında cayma hakkım düşer.", single.Text, StringComparison.Ordinal);
        Assert.Contains("\"Montaj (Kırmızı)\" hizmetinin", single.Text, StringComparison.Ordinal);
        Assert.Contains("hizmetlerinin", plural.Text, StringComparison.Ordinal);
        Assert.Contains("cayma haklarım düşer", plural.Text, StringComparison.Ordinal);
        Assert.Equal(2, plural.CoveredItemNames.Count);
    }

    // ---- KK3: the withdrawal summary ----

    [Fact]
    public void TheWithdrawalSummary_SaysTheTermsThePeriodTheWayAndTheCarrier()
    {
        string text = ConfirmationStepRules.WithdrawalSummary([Line(ProductType.Physical)]);

        Assert.Contains("on dört gün", text, StringComparison.Ordinal);
        Assert.Contains("sipariş sayfanızdan cayma beyanında", text, StringComparison.Ordinal);
        Assert.Contains("taşıyıcı belirlenmemiştir", text, StringComparison.Ordinal);
        Assert.Contains("(ödemeli)", text, StringComparison.Ordinal);
        Assert.Contains("İade kargo bedeli satıcıya aittir", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Hizmette", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Dijital", text, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(WithdrawalExemptionReason.MadeToOrder, false)]
    [InlineData(WithdrawalExemptionReason.Perishable, false)]
    [InlineData(WithdrawalExemptionReason.ExpiryDated, false)]
    [InlineData(WithdrawalExemptionReason.HygieneSensitive, true)]
    [InlineData(WithdrawalExemptionReason.SealedMedia, true)]
    public void TheWithdrawalSummary_SaysTheExemptionAndItsReason_AndTheConditionOnlyOnAConditionalOne(WithdrawalExemptionReason reason, bool conditional)
    {
        OrderItemSource source = Source(exemption: reason, name: "Mamul");
        ConfirmationSummaryLine line = ConfirmationSummaryText.LineOf(source, 1, Amounts(source, 1, 0m));

        string text = ConfirmationStepRules.WithdrawalSummary([line]);

        Assert.Contains(ConfirmationSummaryText.ReasonName(reason), text, StringComparison.Ordinal);
        Assert.Equal(conditional, text.Contains("koruyucu ambalajı açılmamışsa cayabilirsiniz", StringComparison.Ordinal));
        Assert.Equal(!conditional, text.Contains("Cayma hakkı yoktur", StringComparison.Ordinal));
    }

    [Fact]
    public void TheWithdrawalSummary_AddsTheServiceAndTheDigitalTerms_WhenTheCartHasThem()
    {
        string text = ConfirmationStepRules.WithdrawalSummary([Line(ProductType.Service), Line(ProductType.Digital)]);

        Assert.Contains("ifasının tamamlanmasıyla sona erer", text, StringComparison.Ordinal);
        Assert.Contains("indirme başladıktan sonra cayma hakkı kullanılamaz", text, StringComparison.Ordinal);
    }

    // ---- the summary: one shape for the step and the frozen item (KK3: the form equals the summary) ----

    [Fact]
    public void TheLine_OfTheSource_EqualsTheLineOfTheFrozenItem_ForEveryKindOfItem()
    {
        DiscountLine discount = new(1000m, 1200m, 16.67m, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));
        (OrderItemSource Source, int Quantity, decimal Coupon, int Allowance)[] cases =
        [
            (Source(), 1, 0m, 0),
            (Source(discount: discount), 2, 50m, 0),
            (Source(exemption: WithdrawalExemptionReason.HygieneSensitive), 3, 10.50m, 0),
            (Source(ProductType.Service, "Montaj", 500m, null, null, 4), 1, 0m, 0),
            (Source(ProductType.Digital, "Şablon", 80m), 1, 5m, 3),
        ];

        foreach ((OrderItemSource source, int quantity, decimal coupon, int allowance) in cases)
        {
            ItemAmounts amounts = Amounts(source, quantity, coupon);
            OrderItem frozen = OrderFreeze.FreezeItem(Guid.NewGuid(), source, quantity, amounts, allowance, Now);

            Assert.Equal(ConfirmationSummaryText.LineOf(source, quantity, amounts), ConfirmationSummaryText.LineOf(frozen));
        }
    }

    [Fact]
    public void Render_OfTheSourceLines_EqualsRender_OfTheFrozenItems()
    {
        DiscountLine discount = new(1000m, 1200m, 16.67m, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));
        OrderItemSource a = Source(discount: discount, exemption: WithdrawalExemptionReason.Perishable);
        OrderItemSource b = Source(ProductType.Service, "Montaj", 500m, null, null, 4);
        ItemAmounts amountsA = Amounts(a, 2, 50m);
        ItemAmounts amountsB = Amounts(b, 1, 0m);
        OrderItem frozenA = OrderFreeze.FreezeItem(Guid.NewGuid(), a, 2, amountsA, 0, Now);
        OrderItem frozenB = OrderFreeze.FreezeItem(Guid.NewGuid(), b, 1, amountsB, 0, Now);

        ConfirmationSummary before = new([ConfirmationSummaryText.LineOf(a, 2, amountsA), ConfirmationSummaryText.LineOf(b, 1, amountsB)], 49.90m, 2549.90m, PaymentMethod.Card, 3, "a@b.example", null, null);
        ConfirmationSummary after = new([ConfirmationSummaryText.LineOf(frozenA), ConfirmationSummaryText.LineOf(frozenB)], 49.90m, 2549.90m, PaymentMethod.Card, 3, null, null, null);

        Assert.Equal(ConfirmationSummaryText.Render(before), ConfirmationSummaryText.Render(after));
    }

    [Fact]
    public void Render_WritesTheLinesTheDiscountTheCouponShareTheVatTheExemptionTheFulfilmentTheShippingAndTheTotal_AndNoPersonalData()
    {
        DiscountLine discount = new(1000m, 1200m, 16.67m, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));
        OrderItemSource lamp = Source(discount: discount, exemption: WithdrawalExemptionReason.MadeToOrder);
        OrderItemSource fitting = Source(ProductType.Service, "Montaj", 500m, null, null, 4);
        ConfirmationSummary summary = new(
            [ConfirmationSummaryText.LineOf(lamp, 2, Amounts(lamp, 2, 50m)), ConfirmationSummaryText.LineOf(fitting, 1, Amounts(fitting, 1, 0m))],
            49.90m, 2499.90m, PaymentMethod.BankTransfer, 3, "musteri@example.test", new PartyAddress("Ali Veli", "5551112233", Guid.NewGuid(), "Kadıköy", "Gül Sokak 1"), null);

        string text = ConfirmationSummaryText.Render(summary);

        Assert.StartsWith("SİPARİŞ ÖZETİ", text, StringComparison.Ordinal);
        Assert.Contains("Lamba (Kırmızı)", text, StringComparison.Ordinal);
        Assert.Contains("Adet: 2", text, StringComparison.Ordinal);
        Assert.Contains("Birim fiyat: 1.000 TL (indirimden önceki fiyat 1.200 TL, indirim %16,67)", text, StringComparison.Ordinal);
        Assert.Contains("Kupon payı: -50 TL", text, StringComparison.Ordinal);
        Assert.Contains("KDV (%20 dahil): 325 TL", text, StringComparison.Ordinal);
        Assert.Contains("Satır tutarı: 1.950 TL", text, StringComparison.Ordinal);
        Assert.Contains("Cayma hakkı yoktur: tüketicinin istekleri", text, StringComparison.Ordinal);
        Assert.Contains("İfa süresi: 4 gün (ödemenin onaylandığı andan itibaren)", text, StringComparison.Ordinal);
        Assert.Contains("Ara toplam: 2.500 TL", text, StringComparison.Ordinal);
        Assert.Contains("Kupon indirimi: -50 TL", text, StringComparison.Ordinal);
        Assert.Contains("Kargo ücreti: 49,90 TL", text, StringComparison.Ordinal);
        Assert.Contains("Ödenecek toplam (KDV dahil): 2.499,90 TL", text, StringComparison.Ordinal);
        Assert.DoesNotContain("musteri@example.test", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Ali Veli", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Kadıköy", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Render_SaysFreeShippingWhenTheOrderShipsForFree_AndWritesNoShippingLineWithoutAPhysicalItem()
    {
        ConfirmationSummary free = new([Line(ProductType.Physical)], 0m, 100m, PaymentMethod.Card, 2, null, null, null);
        ConfirmationSummary digital = new([Line(ProductType.Digital)], 0m, 100m, PaymentMethod.Card, null, null, null, null);

        Assert.Contains("Kargo ücretsiz", ConfirmationSummaryText.Render(free), StringComparison.Ordinal);
        Assert.DoesNotContain("Kargo", ConfirmationSummaryText.Render(digital), StringComparison.Ordinal);
    }

    [Fact]
    public void Render_MakesAProductNameWithADoubleBraceInert_SoTheOrderStageCannotExpandIt()
    {
        OrderItemSource source = Source(name: "{{ShippingPhone}} lamba");
        ConfirmationSummary summary = new([ConfirmationSummaryText.LineOf(source, 1, Amounts(source, 1, 0m))], 0m, 1200m, PaymentMethod.Card, 2, null, null, null);

        string text = ConfirmationSummaryText.Render(summary);

        Assert.Empty(PlaceholderFiller.TokenNames(text));
        Assert.Contains("{ {ShippingPhone}} lamba", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheLine_RefusesAmountsThatAreNotOfThePriceThatApplies()
    {
        OrderItemSource source = Source();

        Assert.Throws<ArgumentException>(() => ConfirmationSummaryText.LineOf(source, 2, Amounts(source, 1, 0m)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ConfirmationSummaryText.LineOf(source, 0, Amounts(source, 1, 0m)));
    }

    [Fact]
    public void TheEmailLine_ShowsTheGuestsAddress()
    {
        Assert.Equal("Siparişiniz şu adrese gönderilecek: misafir@example.test", ConfirmationSummaryText.EmailLine("misafir@example.test"));
        Assert.Throws<ArgumentException>(() => ConfirmationSummaryText.EmailLine(" "));
    }

    // ---- KK3: the form's added content (the shipped template) ----

    [Fact]
    public void TheForm_AddsTheFourItemsTheFixedReturnTermsTheExemptionTheFulfilmentTheChamberAndTheHowTheOrderIsPlacedText()
    {
        string form = LegalTextTemplates.Get(LegalTextType.PreInformationForm).Text;

        Assert.Contains("yalnızca satıcının teslimat yaptığı illere", form, StringComparison.Ordinal);
        Assert.Contains("{{ShippingPeriod}} iş günü", form, StringComparison.Ordinal);
        Assert.Contains("cayma hakkı", form, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("on dört gün", form, StringComparison.Ordinal);
        Assert.Contains("{{CompanyLegalName}}", form, StringComparison.Ordinal);
        Assert.Contains("{{CompanyChamberName}}", form, StringComparison.Ordinal);
        Assert.Contains("taşıyıcı belirlenmemiştir", form, StringComparison.Ordinal);
        Assert.Contains("İade kargo bedeli satıcıya aittir", form, StringComparison.Ordinal);
        Assert.Contains("Cayma hakkının istisnaları", form, StringComparison.Ordinal);
        Assert.Contains("koruyucu ambalajı açılmamışsa cayabilirsiniz", form, StringComparison.Ordinal);
        Assert.Contains("ifa", form, StringComparison.Ordinal);
        Assert.Contains("SİPARİŞ NASIL KURULUR", form, StringComparison.Ordinal);
        Assert.Contains("{{HowTheOrderIsPlaced}}", form, StringComparison.Ordinal);
        Assert.Contains("bu formun başındaki sipariş özetinde", form, StringComparison.Ordinal);
    }

    [Fact]
    public void TheForm_RefundStartsFromTheDeclarationBeforeDeliveryAndForServices_AndFromTheReturnedGoodsAfterIt_InTheFormAndTheContractAlike()
    {
        foreach (LegalTextType type in LegalTextTemplates.GeneratedTypes)
        {
            string text = LegalTextTemplates.Get(type).Text;

            Assert.Contains("teslim tarihinden önce yapıldıysa ya da kalem hizmetse", text, StringComparison.Ordinal);
            Assert.Contains("iade edilen malın satıcıya ulaşmasından itibaren", text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TheCompanyTokens_AreAClosedSetThatNamesTheChamberAndTheGuideText()
    {
        Assert.Contains("CompanyChamberName", LegalTextTemplates.CompanyTokens);
        Assert.Contains("HowTheOrderIsPlaced", LegalTextTemplates.CompanyTokens);
        Assert.Equal(LegalTextTemplates.CompanyTokens.Count, LegalTextTemplates.CompanyTokens.Distinct(StringComparer.Ordinal).Count());
    }

    // ---- the order's documents: the copy holds no personal data and the second stage fills it ----

    private static Dictionary<string, string?> CompanyValues() =>
        LegalTextTemplates.CompanyTokens.ToDictionary(t => t, t => (string?)(t == "HowTheOrderIsPlaced" ? ProcessGuide.Text : $"<{t}>"), StringComparer.Ordinal);

    [Fact]
    public void Produce_WritesTheSummaryAndTheFrozenValues_AndKeepsEveryPersonalTokenStanding()
    {
        string formVersion = PlaceholderFiller.Fill(LegalTextTemplates.Get(LegalTextType.PreInformationForm).Text, CompanyValues(), requireAll: false);
        string contractVersion = PlaceholderFiller.Fill(LegalTextTemplates.Get(LegalTextType.DistanceSalesContract).Text, CompanyValues(), requireAll: false);
        OrderItemSource source = Source();
        ItemAmounts amounts = Amounts(source, 1, 0m);
        Order order = Order.Create("SF-100", "key", "ck", PaymentMethod.Card, true, "ali@example.test", 1249.90m, 49.90m, Now.AddHours(1), Now);
        order.FreezeShippingPromise(3);
        OrderItem item = OrderFreeze.FreezeItem(order.Id, source, 1, amounts, 0, Now);

        (string form, string contract) = OrderDocuments.Produce(order, [item], formVersion, contractVersion);

        Assert.StartsWith("SİPARİŞ ÖZETİ", form, StringComparison.Ordinal);
        Assert.Contains("Sipariş numarası: SF-100", form, StringComparison.Ordinal);
        Assert.Contains("Vergiler dahil toplam bedel: 1.249,90 TL", form, StringComparison.Ordinal);
        Assert.Contains("Kargo bedeli: 49,90 TL", form, StringComparison.Ordinal);
        Assert.Contains("en geç 3 iş günü", form, StringComparison.Ordinal);
        Assert.Contains("1. SATICI\nUnvan: <CompanyLegalName>", form, StringComparison.Ordinal);
        Assert.Contains("SİPARİŞ NASIL KURULUR", form, StringComparison.Ordinal);
        Assert.Contains(ProcessGuide.Sections[0].Title, form, StringComparison.Ordinal);
        Assert.Contains("Sipariş numarası", contract, StringComparison.Ordinal);

        // Nothing of the customer is in the copy: the ten personal tokens stand and no frozen token is left.
        string[] expected = [.. OrderDocuments.PersonalValues(null, null, null, new Dictionary<Guid, string>()).Keys];
        foreach (string text in new[] { form, contract })
        {
            Assert.DoesNotContain("ali@example.test", text, StringComparison.Ordinal);
            string[] left = [.. PlaceholderFiller.TokenNames(text).Distinct(StringComparer.Ordinal)];
            Assert.All(left, t => Assert.Contains(t, expected));
            Assert.DoesNotContain("OrderNumber", left);
            Assert.DoesNotContain("Total", left);
            Assert.DoesNotContain("ShippingFee", left);
            Assert.DoesNotContain("ShippingPeriod", left);
        }

        Assert.Contains("ShippingRecipientName", PlaceholderFiller.TokenNames(form));
        Assert.Contains("ContactEmail", PlaceholderFiller.TokenNames(form));
    }

    [Fact]
    public void TheSecondStage_FillsThePersonalTokens_AndAnErasedFieldReadsAsEmpty()
    {
        string template = "A: {{ShippingRecipientName}} / {{ShippingPhone}} / {{ShippingProvince}} / {{BillingRecipientName}} / {{BillingProvince}} / {{ContactEmail}}";
        Guid province = Guid.NewGuid();
        Dictionary<Guid, string> provinces = new() { [province] = "İstanbul" };

        string filled = PlaceholderFiller.Fill(
            template,
            OrderDocuments.PersonalValues("ali@example.test", new PartyAddress("Ali Veli", "5551112233", province, "Kadıköy", "Gül Sokak 1"), new PartyAddress("Veli Ali", null, province, "Üsküdar", "Lale Sokak 2"), provinces),
            requireAll: true);
        string erased = PlaceholderFiller.Fill(template, OrderDocuments.PersonalValues(null, null, null, provinces), requireAll: true);

        Assert.Equal("A: Ali Veli / 5551112233 / İstanbul / Veli Ali / İstanbul / ali@example.test", filled);
        Assert.Equal("A:  /  /  /  /  / ", erased);
    }

    [Fact]
    public void ThePersonalValues_AreExactlyTheTenTokensOfTheDocumentRule_AndTheBillingAddressHasNoPhone()
    {
        string[] names = [.. OrderDocuments.PersonalValues(null, null, null, new Dictionary<Guid, string>()).Keys.Order(StringComparer.Ordinal)];

        Assert.Equal(
            [
                "BillingAddressLine", "BillingDistrict", "BillingProvince", "BillingRecipientName", "ContactEmail", "ShippingAddressLine", "ShippingDistrict", "ShippingPhone", "ShippingProvince",
                "ShippingRecipientName",
            ],
            names);
        Assert.DoesNotContain("BillingPhone", names);
    }

    [Fact]
    public void Inert_PutsASpaceBetweenADoubleOpeningBrace_AndTouchesNothingElse()
    {
        Assert.Equal("{ {ShippingPhone}}", PlaceholderFiller.Inert("{{ShippingPhone}}"));
        Assert.Equal("Tek { süslü } ve 100% değer", PlaceholderFiller.Inert("Tek { süslü } ve 100% değer"));
        Assert.Null(PlaceholderFiller.Inert(null));

        // The version's text fills a company value once and the order stage fills the order's tokens over the whole text: the inert value survives the second pass unchanged.
        string firstPass = PlaceholderFiller.Fill("Unvan: {{CompanyLegalName}} / {{ShippingPhone}}", new Dictionary<string, string?> { ["CompanyLegalName"] = PlaceholderFiller.Inert("{{ShippingPhone}} Ltd.") }, requireAll: false);
        string secondPass = PlaceholderFiller.Fill(firstPass, new Dictionary<string, string?> { ["ShippingPhone"] = "5551112233" }, requireAll: false);

        Assert.Equal("Unvan: { {ShippingPhone}} Ltd. / 5551112233", secondPass);
    }

    // ---- KK7: the box record carries the box, its text and the covered items ----

    [Fact]
    public void TheRecords_CarryTheDigitalAndTheServiceBoxWithTheirTextsAndTheirItems_AndNoRecordForTheTwoTextBoxes()
    {
        OrderItemSource physical = Source();
        OrderItemSource digital1 = Source(ProductType.Digital, "Şablon", 80m);
        OrderItemSource digital2 = Source(ProductType.Digital, "Yazı tipi", 40m);
        OrderItemSource service = Source(ProductType.Service, "Montaj", 500m, null, null, 4);
        Guid orderId = Guid.NewGuid();
        OrderItem[] items =
        [
            OrderFreeze.FreezeItem(orderId, physical, 1, Amounts(physical, 1, 0m), 0, Now),
            OrderFreeze.FreezeItem(orderId, digital1, 1, Amounts(digital1, 1, 0m), 3, Now),
            OrderFreeze.FreezeItem(orderId, digital2, 1, Amounts(digital2, 1, 0m), 3, Now),
            OrderFreeze.FreezeItem(orderId, service, 1, Amounts(service, 1, 0m), 0, Now),
        ];
        IReadOnlyList<ConfirmationBoxOffer> offers = ConfirmationStepRules.Offers([.. items.Select(ConfirmationSummaryText.LineOf)], PaymentMethod.Card);

        var records = ConfirmationAcceptances.Records(orderId, offers, items, Now);

        Assert.Equal(2, records.Count);
        var digital = records.Single(r => r.Acceptance.CheckboxType == CheckboxType.Digital);
        var serviceRecord = records.Single(r => r.Acceptance.CheckboxType == CheckboxType.Service);
        Assert.Equal(offers.Single(o => o.Box == ConfirmationBox.DigitalWithdrawalWaiver).Text, digital.Acceptance.ShownText);
        Assert.Equal(offers.Single(o => o.Box == ConfirmationBox.ServiceFulfilmentConsent).Text, serviceRecord.Acceptance.ShownText);
        Assert.Equal([items[1].Id, items[2].Id], digital.Items.Select(l => l.OrderItemId).Order());
        Assert.Equal([items[3].Id], serviceRecord.Items.Select(l => l.OrderItemId));
        Assert.All(records, r => Assert.Equal(orderId, r.Acceptance.OrderId));
        Assert.All(digital.Items, l => Assert.Equal(digital.Acceptance.Id, l.CheckboxAcceptanceId));
    }

    [Fact]
    public void TheRecords_ABoxForATypeTheOrderHasNoItemOfIsAProgrammingError()
    {
        OrderItemSource physical = Source();
        OrderItem item = OrderFreeze.FreezeItem(Guid.NewGuid(), physical, 1, Amounts(physical, 1, 0m), 0, Now);
        ConfirmationBoxOffer offer = new(ConfirmationBox.DigitalWithdrawalWaiver, "metin", ["x"]);

        Assert.Throws<ArgumentException>(() => ConfirmationAcceptances.Records(item.OrderId, [offer], [item], Now));
    }

    // ---- KK9: the process guide ----

    [Fact]
    public void TheProcessGuide_HasTheFourInformationsOfTheSpec_NoTokenAndNoCompanySetting()
    {
        Assert.Equal("İşlem rehberi", ProcessGuide.Title);
        Assert.Equal(4, ProcessGuide.Sections.Count);
        Assert.Contains("izlenecek adımlar", ProcessGuide.Sections[0].Title, StringComparison.Ordinal);
        Assert.Contains("saklanması", ProcessGuide.Sections[1].Title, StringComparison.Ordinal);
        Assert.Contains("düzeltilmesi", ProcessGuide.Sections[2].Title, StringComparison.Ordinal);
        Assert.Contains("uyuşmazlık", ProcessGuide.Sections[3].Title, StringComparison.Ordinal);
        Assert.All(ProcessGuide.Sections, s => Assert.Equal(s, ProcessGuide.Sections.Single(x => x.Title == s.Title)));
        Assert.Contains("aydınlatma", ProcessGuide.Sections[3].Text, StringComparison.Ordinal);
        Assert.Empty(PlaceholderFiller.TokenNames(ProcessGuide.Text));
        Assert.All(ProcessGuide.Sections, s => Assert.Contains(s.Title, ProcessGuide.Text, StringComparison.Ordinal));
    }

    [Fact]
    public void TheProcessGuidePage_IsLinkedInTheFooterOfEveryPage_ShowsWhileSalesAreClosed_AndIsAFixedPageAddress()
    {
        Assert.True(ProcessGuide.IsInFooterOfEveryPage);
        Assert.True(ProcessGuide.ShowsWhileSalesAreClosed);
        Assert.Contains(ProcessGuide.PageSegment, AddressRules.FixedPages);
        Assert.Equal("islem-rehberi", ProcessGuide.PageSegment);
    }

    [Fact]
    public void TheStepsFixedText_IsTheGuideText_TheFormCarriesAsItsOwnPart()
    {
        string form = PlaceholderFiller.Fill(LegalTextTemplates.Get(LegalTextType.PreInformationForm).Text, CompanyValues(), requireAll: false);

        Assert.Contains(ProcessGuide.Text, form, StringComparison.Ordinal);
        Assert.All(ProcessGuide.Sections, s => Assert.Contains(s.Text, form, StringComparison.Ordinal));
        Assert.Equal(typeof(IConfirmationStep).GetMethods(BindingFlags.Public | BindingFlags.Instance).Select(m => m.Name).Order(), ["CheckAsync", "GetViewAsync"]);
    }
}
