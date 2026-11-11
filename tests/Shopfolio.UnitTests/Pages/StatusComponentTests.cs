using System.Globalization;
using System.Text.RegularExpressions;

using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Pages;

/// <summary>
/// The badges, the item record names and the panel marks (T542 KK1-KK5; 04 2.5): the names are the source's, read here from <c>04 2.5.1</c>, <c>2.5.2</c> and <c>2.5.3</c> themselves, so a name that
/// changes in the document and not in the code - or the other way round - turns the test red (09 21.4). The comparison ignores case in Turkish, because the document writes the marks in lower case
/// inside a table and the screen writes them with a capital.
/// </summary>
public sealed partial class StatusComponentTests
{
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    private static string UiSpec() => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "04_UI_SPECS.md")).ReplaceLineEndings("\n");

    private static bool SameText(string a, string b) => string.Equals(a.ToUpper(Turkish), b.ToUpper(Turkish), StringComparison.Ordinal);

    /// <summary>The values cell of the 04 2.5.1 row <paramref name="number"/> (for example "2.5.1.1"), split at the middle dots.</summary>
    private static string[] DocumentedValues(string number)
    {
        string row = UiSpec().Split('\n').Single(l => l.StartsWith($"| {number} |", StringComparison.Ordinal));
        string[] cells = row.Split('|', StringSplitOptions.TrimEntries);
        return cells[3].Split('·', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }

    private static string[] AssertNames(string[] documented, IEnumerable<string> actual)
    {
        string[] shown = [.. actual];
        Assert.Equal(documented.Length, shown.Length);
        for (int i = 0; i < documented.Length; i++)
        {
            Assert.True(SameText(documented[i], shown[i]), $"Document says '{documented[i]}', the screen says '{shown[i]}'.");
        }

        return shown;
    }

    [Fact]
    public void TheShipmentAxisBadges_AreTheNamesOf_04_2_5_1_1_InTheEnumsOrder_KK2()
    {
        Assert.NotEmpty(AssertNames(DocumentedValues("2.5.1.1"), Enum.GetValues<OrderStatus>().Select(s => StatusBadges.For(s).Text)));
    }

    [Fact]
    public void ThePaymentAxisBadges_AreTheNamesOf_04_2_5_1_2_InTheEnumsOrder_KK2()
    {
        Assert.NotEmpty(AssertNames(DocumentedValues("2.5.1.2"), Enum.GetValues<PaymentStatus>().Select(s => StatusBadges.For(s).Text)));
    }

    [Fact]
    public void ThePublishStatusBadges_AreTheNamesOf_04_2_5_1_3_KK2()
    {
        Assert.NotEmpty(AssertNames(DocumentedValues("2.5.1.3"), Enum.GetValues<PublishStatus>().Select(s => StatusBadges.For(s).Text)));
    }

    [Fact]
    public void TheCorporateContentBadges_AreTheNamesOf_04_2_5_1_4_AndHaveNoArchive_KK2()
    {
        Assert.NotEmpty(AssertNames(DocumentedValues("2.5.1.4"), new[] { PublishStatus.Draft, PublishStatus.Published }.Select(s => StatusBadges.ForCorporateContent(s).Text)));
        Assert.Throws<ArgumentOutOfRangeException>(() => StatusBadges.ForCorporateContent(PublishStatus.Archived));
    }

    [Fact]
    public void TheDefectClaimAndTheContactRequestBadges_AreTheNamesOf_04_2_5_1_5_AndOf_2_5_1_6_KK2()
    {
        Assert.NotEmpty(AssertNames(DocumentedValues("2.5.1.5"), Enum.GetValues<DefectClaimStatus>().Select(s => StatusBadges.For(s).Text)));
        Assert.NotEmpty(AssertNames(DocumentedValues("2.5.1.6"), Enum.GetValues<ContactRequestStatus>().Select(s => StatusBadges.For(s).Text)));
    }

    [Fact]
    public void EveryBadgeCarriesText_AndNoValueHasTheSameTextOnItsOwnAxisTwice_KK1()
    {
        Badge[] shipping = [.. Enum.GetValues<OrderStatus>().Select(StatusBadges.For)];
        Badge[] payment = [.. Enum.GetValues<PaymentStatus>().Select(StatusBadges.For)];

        Assert.All([.. shipping, .. payment], b => Assert.False(string.IsNullOrWhiteSpace(b.Text)));
        Assert.Equal(shipping.Length, shipping.Select(b => b.Text).Distinct().Count());
        Assert.Equal(payment.Length, payment.Select(b => b.Text).Distinct().Count());
    }

    [Theory]
    [InlineData(OrderStatus.Placed, PaymentStatus.Pending)]
    [InlineData(OrderStatus.Shipped, PaymentStatus.Paid)]
    [InlineData(OrderStatus.Cancelled, PaymentStatus.Refunded)]
    public void AnOrder_IsShownWithTwoSeparateBadges_OneForEachAxis_KK1(OrderStatus shipping, PaymentStatus payment)
    {
        OrderBadges badges = StatusBadges.ForOrder(shipping, payment);

        Assert.Equal(StatusBadges.For(shipping), badges.Shipping);
        Assert.Equal(StatusBadges.For(payment), badges.Payment);
        Assert.NotEqual(badges.Shipping.Text, badges.Payment.Text);
    }

    [Fact]
    public void TheBadgeFamilies_HaveNoMemberForTheAccountOrTheInvitation_KK2()
    {
        // 02 5.12.1; 03 1.10.7: the account and the invitation have no state machine, so no badge function takes one of their values.
        string[] parameterTypes =
        [
            .. typeof(StatusBadges).GetMethods().Where(m => m.IsStatic && m.DeclaringType == typeof(StatusBadges)).SelectMany(m => m.GetParameters()).Select(p => p.ParameterType.Name),
        ];

        Assert.DoesNotContain(parameterTypes, n => n.Contains("Account", StringComparison.Ordinal) || n.Contains("Invitation", StringComparison.Ordinal) || n.Contains("Verification", StringComparison.Ordinal));
        Assert.Equal(
            ["ContactRequestStatus", "DefectClaimStatus", "OrderStatus", "PaymentStatus", "PublishStatus"],
            [.. parameterTypes.Where(n => n.EndsWith("Status", StringComparison.Ordinal)).Distinct().OrderBy(n => n, StringComparer.Ordinal)]);
    }

    [Fact]
    public void TheItemRecordNames_AreTheOnesOf_04_2_5_2_InItsOrder_KK3()
    {
        string text = UiSpec();
        Match sentence = ItemRecordSentence().Match(text);
        Assert.True(sentence.Success, "04 2.5.2 has no list of the records.");
        string[] documented = sentence.Groups[1].Value.Split(", ", StringSplitOptions.TrimEntries);

        Assert.NotEmpty(AssertNames(documented, ItemRecordLabels.All.Select(ItemRecordLabels.Of)));
    }

    [Fact]
    public void AnItemRecord_HasNoBadge_TheItemHasNoStatus_KK3()
    {
        Assert.Equal(10, ItemRecordLabels.All.Count);
        Assert.DoesNotContain(typeof(StatusBadges).GetMethods(), m => m.GetParameters().Any(p => p.ParameterType == typeof(ItemRecordName)));
    }

    [Fact]
    public void ThePanelMarkTexts_AreTheMarksOf_04_2_5_3_InItsOrder_KK4()
    {
        string[] documented =
        [
            .. UiSpec().Split('\n')
                .Where(l => Regex.IsMatch(l, @"^\| 2\.5\.3\.\d \|", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)))
                .Select(l => l.Split('|', StringSplitOptions.TrimEntries)[2].Trim('"')),
        ];

        Assert.Equal(7, documented.Length);
        Assert.NotEmpty(AssertNames(documented, Enum.GetValues<PanelMark>().Select(PanelMarks.TextOf)));
    }

    [Fact]
    public void AMark_ShowsWhileItsConditionHolds_AndGoesWhenTheConditionGoes_KK4()
    {
        PanelMarkConditions held = new(EmailNotDelivered: true, RefundNotCompleted: true);
        Assert.Equal([PanelMark.EmailNotDelivered, PanelMark.RefundNotCompleted], PanelMarks.Of(held, PanelMarkPlace.OrderDetail));

        PanelMarkConditions gone = held with { EmailNotDelivered = false };
        Assert.Equal([PanelMark.RefundNotCompleted], PanelMarks.Of(gone, PanelMarkPlace.OrderDetail));
        Assert.Empty(PanelMarks.Of(new PanelMarkConditions(), PanelMarkPlace.OrderDetail));
        Assert.Empty(PanelMarks.Of(new PanelMarkConditions(), PanelMarkPlace.OrderList));
    }

    [Fact]
    public void TheOrderList_HasNoIbanAndNoReturnGoodsMark_ButTheOrderPageHasAllSeven_KK4()
    {
        PanelMarkConditions all = new(true, true, true, true, true, true, true);

        Assert.Equal(7, PanelMarks.Of(all, PanelMarkPlace.OrderDetail).Count);
        Assert.Equal(
            [PanelMark.EmailNotDelivered, PanelMark.PaymentResultMissing, PanelMark.RefundNotCompleted, PanelMark.EmailCorrected, PanelMark.RefundResultPending],
            PanelMarks.Of(all, PanelMarkPlace.OrderList));
    }

    [Fact]
    public void TwoMarksThatHoldTogether_AreTwoMarks_TheEmailCorrectedOneDoesNotHideTheOther_KK4()
    {
        IReadOnlyList<PanelMark> marks = PanelMarks.Of(new PanelMarkConditions(EmailNotDelivered: true, EmailCorrected: true), PanelMarkPlace.OrderList);

        Assert.Equal([PanelMark.EmailNotDelivered, PanelMark.EmailCorrected], marks);
    }

    [Fact]
    public void TheUndeliveredEmailBlock_OffersResendAllOnlyWhenMoreThanOneRowCanBeResent_KK5()
    {
        DateTimeOffset at = new(2026, 11, 3, 9, 0, 0, TimeSpan.FromHours(3));
        UndeliveredEmailRow one = new("a", "Sipariş e-postası", at, "x@example.test", true);
        UndeliveredEmailRow two = new("b", "Kargoya verildi", at, "x@example.test", true);
        UndeliveredEmailRow invitation = new("c", "Yönetici daveti", at, "y@example.test", false);

        Assert.False(new UndeliveredEmailList("/panel/siparisler/1", "ResendEmail", "ResendAllEmails", [one], false).ShowsResendAll);
        Assert.False(new UndeliveredEmailList("/panel/siparisler/1", "ResendEmail", "ResendAllEmails", [one, invitation], false).ShowsResendAll);
        Assert.True(new UndeliveredEmailList("/panel/siparisler/1", "ResendEmail", "ResendAllEmails", [one, two], false).ShowsResendAll);
    }

    [GeneratedRegex(@"\*\*2\.5\.2 Kalem kayıtları\*\* sipariş kaleminin satırında kaydın adıyla görünür — (.+?); liste", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ItemRecordSentence();
}
