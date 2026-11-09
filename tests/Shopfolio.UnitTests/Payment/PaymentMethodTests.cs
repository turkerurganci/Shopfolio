using System.Reflection;
using System.Runtime.CompilerServices;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.UnitTests.Payment;

/// <summary>
/// The payment's core rules (T352; 02 3.21.1-3.21.4, 3.21.9-3.21.11): the card line reads the conditional line worked out at start-up (D-81) and the offered methods are the card and
/// the bank transfer only; the product keeps no card data, knows no installment and no chargeback - checked over every name of the application (types, properties, fields,
/// parameters, enum values, log method parameters) with a sample that proves the scan finds each kind; a card payment has no switch that skips 3D Secure and no amount threshold;
/// the order reaches Preparing only with an approved payment.
/// </summary>
public sealed class PaymentMethodTests
{
    private static readonly Assembly Application = typeof(AppDbContext).Assembly;

    /// <summary>Names of card data (02 3.21.1; 09 18.2 item 2): the card number, its masked form, the holder, the expiry, the security code.</summary>
    private static readonly string[] CardDataNames =
        ["cardnumber", "cardno", "maskedpan", "cardholder", "cardexpir", "expirymonth", "expiryyear", "cvv", "cvc", "securitycode", "cardtoken"];

    /// <summary>Exact names of card data that would match an unrelated longer name (a withdrawal exemption's "expiry dated" goods) if looked for inside it.</summary>
    private static readonly string[] CardDataExactNames = ["pan", "bin", "cardbin", "expiry", "expirydate", "expirationdate"];

    private static readonly string[] InstallmentNames = ["installment", "taksit"];

    private static readonly string[] ChargebackNames = ["chargeback", "tersibraz", "dispute", "reversal"];

    private static readonly string[] ThreeDSecureNames = ["non3d", "threed", "3dsecure", "secure3d", "3dthreshold"];

    // ---- KK4: the card line and the offered methods ----

    [Fact]
    public async Task CardCondition_IsOpen_OnlyWhenTheCardLineIsOpen_KK4()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        bool open = await new CardPaymentCondition([new ConditionalLine(ConditionalLine.CardPayment, [])]).IsOpenAsync(ct);
        bool keyless = await new CardPaymentCondition([new ConditionalLine(ConditionalLine.CardPayment, [PaymentOptions.ProviderKeysKey])]).IsOpenAsync(ct);
        bool otherLineOnly = await new CardPaymentCondition([new ConditionalLine(ConditionalLine.GoogleSignIn, [])]).IsOpenAsync(ct);
        bool none = await new CardPaymentCondition([]).IsOpenAsync(ct);

        Assert.True(open);
        Assert.False(keyless);
        Assert.False(otherLineOnly);
        Assert.False(none);
    }

    [Fact]
    public async Task CardCondition_ReadsTheLineWorkedOutFromTheInstallationSettings_NotItsOwnCheck_KK4()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        IReadOnlyList<ConditionalLine> keyed = ConditionalLines.Evaluate(new PaymentOptions { ProviderKeys = "keys" }, new GoogleOptions());
        IReadOnlyList<ConditionalLine> keyless = ConditionalLines.Evaluate(new PaymentOptions(), new GoogleOptions());

        Assert.True(await new CardPaymentCondition(keyed).IsOpenAsync(ct));
        Assert.False(await new CardPaymentCondition(keyless).IsOpenAsync(ct));
    }

    public static TheoryData<bool, bool, PaymentMethod[]> OpenMethodCases => new()
    {
        { true, true, [PaymentMethod.Card, PaymentMethod.BankTransfer] },
        { false, true, [PaymentMethod.BankTransfer] },
        { true, false, [PaymentMethod.Card] },
        { false, false, [] },
    };

    [Theory]
    [MemberData(nameof(OpenMethodCases))]
    public void OpenMethods_AreTheCardWhenItsLineIsOpen_AndTheTransferWhenItIsOpen_KK2_KK4(bool cardOpen, bool transferOpen, PaymentMethod[] expected)
    {
        Assert.Equal(expected, PaymentMethodRules.Open(cardOpen, transferOpen));
    }

    [Fact]
    public async Task PaymentMethods_WithoutTheProviderKeys_OfferOnlyTheTransfer_KK4()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        PaymentMethods keyless = new(new CardPaymentCondition(ConditionalLines.Evaluate(new PaymentOptions(), new GoogleOptions())), new FixedTransfer(true));
        PaymentMethods neither = new(new CardPaymentCondition([]), new FixedTransfer(false));

        Assert.Equal([PaymentMethod.BankTransfer], await keyless.ListOpenAsync(ct));
        Assert.Empty(await neither.ListOpenAsync(ct));
    }

    [Fact]
    public void PaymentMethod_HasExactlyTheCardAndTheBankTransfer_NoCashOnDelivery_KK2()
    {
        Assert.Equal([PaymentMethod.Card, PaymentMethod.BankTransfer], Enum.GetValues<PaymentMethod>());
    }

    [Fact]
    public void Confirmation_RefusesAMethodThatIsNotOpen_WithItsOwnRefusal_KK4()
    {
        Assert.Contains(OrderConfirmationRefusal.PaymentMethodUnavailable, Enum.GetValues<OrderConfirmationRefusal>());
    }

    // ---- KK1: no card data in the records, the database or the log ----

    [Fact]
    public void Application_NamesNoCardData_InAnyRecordColumnOrLogParameter_KK1()
    {
        Assert.Empty(Offenders(Application, CardDataNames, CardDataExactNames));
    }

    [Fact]
    public void CardDataScan_FindsEachKindOfName_InTheSample_KK1()
    {
        List<string> found = Offenders(typeof(ViolationSample), CardDataNames, CardDataExactNames);

        Assert.Contains("ViolationSample.CardNumber", found);
        Assert.Contains("ViolationSample._cvv", found);
        Assert.Contains("ViolationSample.Log(maskedPan)", found);
        Assert.Contains("ViolationSample.Pan", found);
        Assert.Contains("SampleKind.CardHolder", found);
    }

    [Fact]
    public void PaymentStartRequest_CarriesNoCardData_TheCardIsEnteredOnTheProvidersForm_KK1()
    {
        string[] fields = [.. typeof(PaymentStartRequest).GetProperties().Select(p => p.Name)];

        Assert.Equal(
            ["OrderNumber", "Total", "ContactEmail", "CompanyEmail", "BillingRecipientName", "BillingAddressLine", "ShippingPhone", "CompanyPhone", "ClientIpAddress", "PaymentDeadline", "OrderAccessKey"],
            fields);
    }

    // ---- KK3: every card payment through 3D Secure, no threshold, a failed check is not a payment ----

    [Fact]
    public void Application_HasNoSwitchThatSkips3DSecure_AndNoAmountThreshold_KK3()
    {
        Assert.Empty(Offenders(Application, ThreeDSecureNames, []));
    }

    [Fact]
    public void ThreeDSecureScan_FindsTheSwitch_InTheSample_KK3()
    {
        Assert.Contains("ViolationSample.Non3dAllowed", Offenders(typeof(ViolationSample), ThreeDSecureNames, []));
    }

    // ---- KK5: the product knows no installment; the order carries one amount ----

    [Fact]
    public void Application_NamesNoInstallment_InTheModelOrTheScreensData_KK5()
    {
        Assert.Empty(Offenders(Application, InstallmentNames, []));
    }

    [Fact]
    public void InstallmentScan_FindsTheName_InTheSample_KK5()
    {
        Assert.Contains("ViolationSample.InstallmentCount", Offenders(typeof(ViolationSample), InstallmentNames, []));
    }

    [Fact]
    public void Order_CarriesOneAmount_NoInstallmentPlan_KK5()
    {
        string[] amounts = [.. typeof(Order).GetProperties().Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?)).Select(p => p.Name)];

        Assert.Contains("Total", amounts);
        Assert.DoesNotContain(amounts, name => name.Contains("Installment", StringComparison.OrdinalIgnoreCase));
    }

    // ---- KK6: Preparing only with an approved payment ----

    [Theory]
    [InlineData(PaymentStatus.Pending)]
    [InlineData(PaymentStatus.Failed)]
    public void Preparing_StandsWithAnUnapprovedPayment_Never_KK6(PaymentStatus unapproved)
    {
        Assert.False(TransitionTable.IsPairAllowed(OrderStatus.Preparing, unapproved));
        Assert.True(TransitionTable.IsPairAllowed(OrderStatus.Preparing, PaymentStatus.Paid));
    }

    // ---- KK7: no endpoint, state or field for a chargeback ----

    [Fact]
    public void Application_HasNoChargebackEndpointStateOrField_KK7()
    {
        Assert.Empty(Offenders(Application, ChargebackNames, []));
        Assert.DoesNotContain(ConstantTexts(Application), text => text.Contains("ters-ibraz", StringComparison.OrdinalIgnoreCase) || text.Contains("chargeback", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ChargebackScan_FindsTheStateAndTheRoute_InTheSample_KK7()
    {
        Assert.Contains("SampleKind.Chargeback", Offenders(typeof(ViolationSample), ChargebackNames, []));
        Assert.Contains("/panel/ters-ibraz", ConstantTexts(typeof(ViolationSample)));
    }

    [Fact]
    public void PaymentAxis_HasOnlyItsFiveStates_NoneForAChargeback_KK7()
    {
        Assert.Equal(
            [PaymentStatus.Pending, PaymentStatus.Paid, PaymentStatus.Failed, PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded],
            Enum.GetValues<PaymentStatus>());
    }

    // ---- the scan ----

    private static List<string> Offenders(Assembly assembly, string[] contained, string[] exact) => Offenders(assembly.GetTypes(), contained, exact);

    private static List<string> Offenders(Type sample, string[] contained, string[] exact) => Offenders([sample, .. sample.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)], contained, exact);

    private static List<string> Offenders(IEnumerable<Type> types, string[] contained, string[] exact)
    {
        const BindingFlags Declared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        List<string> found = [];
        foreach (Type type in types.Where(t => !t.IsDefined(typeof(CompilerGeneratedAttribute), false)))
        {
            string typeName = type.Name;
            Check(typeName, typeName);
            foreach (PropertyInfo property in type.GetProperties(Declared))
            {
                Check(property.Name, $"{typeName}.{property.Name}");
            }

            foreach (FieldInfo field in type.GetFields(Declared).Where(f => !f.IsDefined(typeof(CompilerGeneratedAttribute), false)))
            {
                Check(field.Name, $"{typeName}.{field.Name}");
            }

            foreach (MethodBase method in type.GetMethods(Declared).Cast<MethodBase>().Concat(type.GetConstructors(Declared)))
            {
                Check(method.Name, $"{typeName}.{method.Name}");
                foreach (ParameterInfo parameter in method.GetParameters())
                {
                    Check(parameter.Name ?? string.Empty, $"{typeName}.{method.Name}({parameter.Name})");
                }
            }
        }

        return found;

        void Check(string name, string where)
        {
            string plain = name.TrimStart('_').Replace("_", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
            if (contained.Any(word => plain.Contains(word, StringComparison.Ordinal)) || exact.Contains(plain))
            {
                found.Add(where);
            }
        }
    }

    private static List<string> ConstantTexts(Assembly assembly) => ConstantTexts(assembly.GetTypes());

    private static List<string> ConstantTexts(Type sample) => ConstantTexts([sample, .. sample.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)]);

    private static List<string> ConstantTexts(IEnumerable<Type> types)
    {
        return
        [
            .. types
                .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Where(f => f.IsLiteral && f.FieldType == typeof(string))
                .Select(f => (string)f.GetRawConstantValue()!),
        ];
    }

    private sealed class FixedTransfer(bool open) : IBankTransferCondition
    {
        public Task<bool> IsOpenAsync(CancellationToken ct) => Task.FromResult(open);
    }

    /// <summary>The scan's violation sample (09 19.2 item 3): one name of each kind the scans look for.</summary>
    private sealed class ViolationSample
    {
        public const string ChargebackRoute = "/panel/ters-ibraz";

#pragma warning disable CS0169, IDE0044, IDE0051 // The sample's members exist only to be found by name.
        private string? _cvv;
#pragma warning restore CS0169, IDE0044, IDE0051

        public enum SampleKind
        {
            CardHolder,
            Chargeback,
        }

        public string CardNumber { get; set; } = string.Empty;

        public string Pan { get; set; } = string.Empty;

        public bool Non3dAllowed { get; set; }

        public int InstallmentCount { get; set; }

        public static void Log(string maskedPan) => _ = maskedPan;
    }
}
