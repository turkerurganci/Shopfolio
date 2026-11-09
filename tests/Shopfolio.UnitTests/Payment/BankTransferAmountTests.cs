using System.Reflection;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;

namespace Shopfolio.UnitTests.Payment;

/// <summary>
/// T353 criterion 6, KK6 (02 3.21.7; K-169): the firm does not enter the amount that arrived by transfer. There is no incoming amount field, no comparison with the order total and
/// no "short payment" state: the payment axis has its five states and the "paid" mark carries no amount.
/// </summary>
public sealed class BankTransferAmountTests
{
    private static readonly string[] _incomingWords = ["Incoming", "Received", "Arrived", "Underpa", "Overpa", "Shortfall", "Short", "Missing", "Partial"];

    [Fact]
    public void ThePaymentAxis_HasNoShortPaymentState_T353_KK6()
    {
        Assert.Equal(["Pending", "Paid", "Failed", "PartiallyRefunded", "Refunded"], Enum.GetNames<PaymentStatus>());
    }

    [Fact]
    public void TheTransferPaidMark_CarriesNoAmount_T353_KK6()
    {
        ParameterInfo[] parameters = typeof(TransferPaidMark).GetConstructors().Single().GetParameters();

        Assert.Equal(["OrderId", "AdminId", "AdminName", "ExpectedVersion", "IsIrreversibleConfirmed"], parameters.Select(p => p.Name));
        Assert.DoesNotContain(parameters, p => p.ParameterType == typeof(decimal) || p.ParameterType == typeof(decimal?));
    }

    [Fact]
    public void TheOrderAndThePaymentRecord_HoldNoIncomingAmountField_T353_KK6()
    {
        // Every money property of the two records the "paid" mark writes is listed by name; none of them is an amount that arrived.
        Type[] records = [typeof(Order), typeof(PaymentRecord)];
        List<string> offenders = [.. records
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?))
            .Where(p => _incomingWords.Any(w => p.Name.Contains(w, StringComparison.Ordinal)))
            .Select(p => $"{p.DeclaringType!.Name}.{p.Name}")];

        Assert.Empty(offenders);
        Assert.DoesNotContain(typeof(AppDbContext).Assembly.GetTypes(), t => _incomingWords.Any(w => t.Name.StartsWith(w, StringComparison.Ordinal) && t.Name.EndsWith("Payment", StringComparison.Ordinal)));
    }
}
