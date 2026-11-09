using System.Text.RegularExpressions;

using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings;

namespace Shopfolio.UnitTests.AfterSales;

/// <summary>
/// What the return does not carry (T373 KK1, KK2, KK9; 02 7.4.2-7.4.4, 7.4.10): the return's shipping is the firm's without a setting, the customer picks the carrier
/// and no label or pickup is produced, and a used or damaged good is taken back without a deduction or a loss of value. The records, the writer's entries and the
/// transition requests of the return have no such field, and no setting of the shop names the return.
/// </summary>
public sealed class ReturnShapeTests
{
    private static readonly Type[] ReturnTypes =
    [
        typeof(Return),
        typeof(ReturnRejection),
        typeof(NotReturnedClosure),
        typeof(Withdrawal),
        typeof(WithdrawalDeclaration),
        typeof(ReturnReceiptEntry),
        typeof(NotReturnedClosureEntry),
        typeof(ReturnStockEntry),
        typeof(ReceiveReturnRequest),
        typeof(RejectReturnRequest),
        typeof(CloseAsNotReturnedRequest),
        typeof(AddReturnedStockRequest),
        typeof(ItemAmounts),
    ];

    // ---- KK1: the return's shipping cost is the firm's, with no setting and nothing paid in advance ----

    [Fact]
    public void NoSettingOfTheShop_NamesTheReturn_AndNoReturnRecordCarriesAShippingCost_KK1()
    {
        Regex cost = new("(Shipping|Cargo|Fee|Cost|Postage|Freight|Prepaid|PaidBy|CashOnDelivery|Carrier|Label|Pickup)", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

        // The return's only setting is the return address (02 7.4.4; the "iade adresi" fields and their gate fact); none prices or routes the return's shipment.
        Regex returnName = new("(Return|Iade)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
        IEnumerable<Type> settings = typeof(CompanySettings).Assembly.GetTypes()
            .Where(t => t.Namespace is { } ns && ns.StartsWith("Shopfolio.Modules.Settings", StringComparison.Ordinal));
        Assert.All(settings, t => Assert.DoesNotContain(t.GetProperties(), p => returnName.IsMatch(p.Name) && cost.IsMatch(p.Name)));
        Assert.All(
            typeof(CompanySettings).GetProperties().Where(p => returnName.IsMatch(p.Name)),
            p => Assert.Matches("^Return(RecipientName|ProvinceId|District|AddressLine|Phone|PostalCode|Address\\w*)$", p.Name));

        Assert.All(ReturnTypes, t => Assert.DoesNotContain(t.GetProperties(), p => cost.IsMatch(p.Name)));
    }

    // ---- KK2: the customer picks the carrier; no label, no pickup offered ----

    [Fact]
    public void NoReturnRecordCarriesALabel_ACarrier_OrAPickup_KK2()
    {
        Regex carrier = new("(Label|Carrier|Courier|Pickup|Collection|Tracking|Barcode|ShipmentCode)", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        Assert.All(ReturnTypes, t => Assert.DoesNotContain(t.GetProperties(), p => carrier.IsMatch(p.Name)));
    }

    // ---- KK9: no deduction and no loss of value; a used good is not refused (the conditional exemption's rejection aside) ----

    [Fact]
    public void NoReturnRecordCarriesADeduction_OrALossOfValue_OrAGoodsCondition_KK9()
    {
        Regex deduction = new("(Deduct|Loss|Depreciat|Damage|Used|Wear|Condition|Penalty|Kesinti|DegerKaybi)", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        Assert.All(ReturnTypes, t => Assert.DoesNotContain(t.GetProperties(), p => deduction.IsMatch(p.Name)));

        // The only refusal of a return is the conditional exemption's opened packaging (02 7.4.9): there is no refusal for a used or damaged good.
        Regex goodsState = new("(Deduct|Loss|Depreciat|Damage|Used|Wear|Penalty)", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        Assert.DoesNotContain(Enum.GetNames<ItemRecordRefusal>(), n => goodsState.IsMatch(n));
        Assert.DoesNotContain(Enum.GetNames<TransitionRefusal>(), n => goodsState.IsMatch(n));
    }
}
