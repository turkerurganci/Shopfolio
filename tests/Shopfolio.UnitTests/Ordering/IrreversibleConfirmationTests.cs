using System.Reflection;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// The irreversible operations and the correction's open face (T319 criteria 1, 2, 9; KK1, KK2, KK9; 03 1.6.1, 1.6.2; 02 5.9, 10.4.6; K-797, K-798): the
/// catalog has one window per row of 03 1.6.1 - the rows are read from the document - every window says its result and ends with "Bu işlem geri alınamaz.",
/// the transfer mark of an order with a digital item warns first that it can not be corrected, the system's own card refund asks no confirmation, and no
/// module surface offers an undo: the one way back is the correction of a transition, and nothing corrects a withdrawal.
/// </summary>
public sealed partial class IrreversibleConfirmationTests
{
    /// <summary>
    /// An undo word as a whole word of a PascalCase name: it starts with its capital and the next letter is not lower case, so "RefundOutcome" (whose
    /// letters hold "undo") is not an undo while "UndoShipmentAsync" and "RevertPaidMark" are.
    /// </summary>
    [GeneratedRegex("(Undo|Revert|Rollback|Reverse|GeriAl)(?![a-z])", RegexOptions.CultureInvariant)]
    private static partial Regex UndoWord();

    /// <summary>One window per operation, with the screen's values; the amounts, the counts and the dates are the display format's.</summary>
    private static IReadOnlyList<IrreversibleConfirmation> Samples() =>
    [
        IrreversibleConfirmations.TransferPaidMark(hasDigitalItem: false),
        IrreversibleConfirmations.TransferPaidMark(hasDigitalItem: true),
        IrreversibleConfirmations.Shipment(isResending: false, isOwnVehicleDelivery: false),
        IrreversibleConfirmations.Shipment(isResending: true, isOwnVehicleDelivery: false),
        IrreversibleConfirmations.Shipment(isResending: false, isOwnVehicleDelivery: true),
        IrreversibleConfirmations.FirmCancellation(isPaid: false, units: 1, isCardLine: true, isCustomerCausedDeliveryFailure: false),
        IrreversibleConfirmations.FirmCancellation(isPaid: true, units: 3, isCardLine: true, isCustomerCausedDeliveryFailure: false),
        IrreversibleConfirmations.FirmCancellation(isPaid: true, units: 2, isCardLine: false, isCustomerCausedDeliveryFailure: true),
        IrreversibleConfirmations.FirmCancellation(isPaid: true, units: 1, isCardLine: true, isCustomerCausedDeliveryFailure: false, isOutOfStock: true),
        IrreversibleConfirmations.RefundProcessing(1250.5m, null),
        IrreversibleConfirmations.RefundProcessing(99m, "TR330006100519786457841326"),
        IrreversibleConfirmations.CardRefundingRemoval(2, 300m),
        IrreversibleConfirmations.ReturnRejection(1),
        IrreversibleConfirmations.OtherChannelWithdrawal(2, new DateOnly(2026, 11, 3)),
        IrreversibleConfirmations.OtherChannelTermination(1, new DateOnly(2026, 11, 4)),
        IrreversibleConfirmations.AccountDeletion(),
    ];

    private static List<string> DocumentedRowNumbers()
    {
        string[] lines = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "03_USER_FLOWS.md")).ReplaceLineEndings("\n").Split('\n');
        return [.. lines
            .Where(l => l.StartsWith("| 1.6.1.", StringComparison.Ordinal))
            .Select(l => l.Trim('|').Split('|')[0].Trim())];
    }

    [Fact]
    public void TheCatalog_HasOneOperationPerRowOf03_1_6_1_InTheTablesOrder_KK1()
    {
        List<string> rows = DocumentedRowNumbers();
        IrreversibleAction[] actions = Enum.GetValues<IrreversibleAction>();

        Assert.Equal(rows.Count, actions.Length);
        Assert.Equal([.. Enumerable.Range(1, actions.Length).Select(n => $"1.6.1.{n}")], rows);
    }

    [Fact]
    public void EveryOperation_HasAWindow_ThatSaysItsResult_AndEndsWithTheIrreversibleSentence_KK1()
    {
        IReadOnlyList<IrreversibleConfirmation> samples = Samples();

        Assert.Equal(Enum.GetValues<IrreversibleAction>().Order(), samples.Select(s => s.Action).Distinct().Order());
        Assert.All(samples, s =>
        {
            Assert.False(string.IsNullOrWhiteSpace(s.Sentence));
            Assert.Equal(IrreversibleConfirmations.IrreversibleSentence, s.Lines[^1]);
            Assert.Equal(s.Sentence, s.Lines[^2]);
            Assert.NotEqual(IrreversibleConfirmations.IrreversibleSentence, s.Sentence);
        });
    }

    [Fact]
    public void TheWindows_WriteTheScreensValues_InTheDisplayFormat_KK1()
    {
        Assert.StartsWith(DisplayFormat.Number(3) + " ürün iptal edilecek", IrreversibleConfirmations.FirmCancellation(true, 3, true, false).Sentence, StringComparison.Ordinal);
        Assert.Contains(DisplayFormat.Amount(1250.5m), IrreversibleConfirmations.RefundProcessing(1250.5m, null).Sentence, StringComparison.Ordinal);
        Assert.Contains(DisplayFormat.Iban("TR330006100519786457841326"), IrreversibleConfirmations.RefundProcessing(99m, "TR330006100519786457841326").Sentence, StringComparison.Ordinal);
        Assert.Contains(DisplayFormat.Date(new DateOnly(2026, 11, 3)), IrreversibleConfirmations.OtherChannelWithdrawal(2, new DateOnly(2026, 11, 3)).Sentence, StringComparison.Ordinal);
        Assert.Contains("düzeltilemez", IrreversibleConfirmations.OtherChannelTermination(1, new DateOnly(2026, 11, 4)).Sentence, StringComparison.Ordinal);
        Assert.EndsWith("Gidiş kargo bedeli geri ödenmeyecek.", IrreversibleConfirmations.FirmCancellation(true, 2, false, true).Sentence, StringComparison.Ordinal);
        Assert.Throws<ArgumentOutOfRangeException>(() => IrreversibleConfirmations.ReturnRejection(0));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TheFirmsCancellation_ForOutOfStock_SaysBeforeTheConfirmation_ThatAMissingStockIsNoLegalImpossibility_T368_KK8(bool isPaid)
    {
        IrreversibleConfirmation outOfStock = IrreversibleConfirmations.FirmCancellation(isPaid, 1, true, false, isOutOfStock: true);
        IrreversibleConfirmation other = IrreversibleConfirmations.FirmCancellation(isPaid, 1, true, false);

        // The warning is the window's first line, before the result sentence and the irreversible sentence the confirmation closes with (02 7.2.4).
        Assert.Equal(IrreversibleConfirmations.OutOfStockLegalNotice, outOfStock.Lines[0]);
        Assert.Contains("imkânsızlaşması sayılmaz", outOfStock.Warning!, StringComparison.Ordinal);
        Assert.Null(other.Warning);
        Assert.Equal(other.Sentence, outOfStock.Sentence);
    }

    [Fact]
    public void TheTransferMark_OfAnOrderWithADigitalItem_WarnsFirstThatTheMarkCanNotBeCorrected_KK1()
    {
        IrreversibleConfirmation digital = IrreversibleConfirmations.TransferPaidMark(hasDigitalItem: true);
        IrreversibleConfirmation plain = IrreversibleConfirmations.TransferPaidMark(hasDigitalItem: false);

        Assert.Equal(
            [IrreversibleConfirmations.DigitalItemPaidMarkWarning, plain.Sentence, IrreversibleConfirmations.IrreversibleSentence],
            digital.Lines);
        Assert.Contains("düzeltilemez", IrreversibleConfirmations.DigitalItemPaidMarkWarning, StringComparison.Ordinal);
        Assert.Equal([plain.Sentence, IrreversibleConfirmations.IrreversibleSentence], plain.Lines);
    }

    [Fact]
    public void TheSystemsOwnCardRefund_AsksNoConfirmation_WhileTheFirmsIrreversibleRequestsCarryOne_KK2()
    {
        // The firm's irreversible operations carry the confirmation in their request (03 1.6.1.1, 1.6.1.2, 1.6.1.4) ...
        Assert.Contains(typeof(TransferPaidMark).GetProperties(), p => p.PropertyType == typeof(bool) && p.Name.Contains("Confirmed", StringComparison.Ordinal));
        Assert.Contains(typeof(TransferRefundProcessing).GetProperties(), p => p.PropertyType == typeof(bool) && p.Name.Contains("Confirmed", StringComparison.Ordinal));
        Assert.Contains(typeof(ShipOrderRequest).GetProperties(), p => p.PropertyType == typeof(bool) && p.Name.Contains("Confirmed", StringComparison.Ordinal));

        // ... the system's card refund - its outcome and the late payment's refund - has none, and is not a row of the catalog (03 1.6.1; 02 5.9: F-4 tells the firm).
        Assert.DoesNotContain(typeof(CardRefundOutcome).GetProperties(), p => p.PropertyType == typeof(bool));
        Assert.DoesNotContain(
            typeof(IPaymentModule).GetMethod(nameof(IPaymentModule.StartLatePaymentRefundAsync))!.GetParameters(),
            p => p.ParameterType == typeof(bool));
        Assert.DoesNotContain(Enum.GetNames<IrreversibleAction>(), n => n.Contains("System", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("UndoShipmentAsync", true)]
    [InlineData("RevertPaidMarkAsync", true)]
    [InlineData("RollbackCancellation", true)]
    [InlineData("ReverseTransitionAsync", true)]
    [InlineData("GeriAlAsync", true)]
    [InlineData("ShipmentUndo", true)]
    [InlineData("ApplyCardRefundOutcomeAsync", false)]
    [InlineData("WriteCardRefundOutcomeAsync", false)]
    [InlineData("CorrectTransitionAsync", false)]
    [InlineData("ReversedAtOf", false)]
    public void TheUndoWordDetector_FindsAWholeWord_AndNotTheLettersInsideAnother(string name, bool isUndo) =>
        Assert.Equal(isUndo, UndoWord().IsMatch(name));

    [Fact]
    public void NoModuleSurface_OffersAnUndo_TheOneWayBackIsTheCorrectionOfATransition_KK1_KK9()
    {
        MethodInfo[] methods = [.. typeof(IOrderTransitions).Assembly.GetTypes()
            .Where(t => t.IsInterface && t.IsPublic && t.Namespace is { } ns && ns.StartsWith("Shopfolio.Modules.", StringComparison.Ordinal) && ns.EndsWith(".Contracts", StringComparison.Ordinal))
            .SelectMany(t => t.GetMethods())];

        Assert.NotEmpty(methods);
        Assert.DoesNotContain(methods, m => UndoWord().IsMatch(m.Name));

        // The correction is one method of the transition unit; it takes a transition record, so a withdrawal - not a transition - is never corrected (03 1.6.2; K-688).
        // The contact-data corrections of T375 (the address, the guest e-mail) are the other "Correct" methods and touch no transition (IOrderCorrections).
        MethodInfo correction = Assert.Single(methods, m => m.Name.StartsWith("Correct", StringComparison.Ordinal) && m.DeclaringType != typeof(IOrderCorrections));
        Assert.Equal((typeof(IOrderTransitions), nameof(IOrderTransitions.CorrectTransitionAsync)), (correction.DeclaringType, correction.Name));
        Assert.DoesNotContain(methods, m => m.Name.Contains("Withdrawal", StringComparison.Ordinal) && m.Name.Contains("Correct", StringComparison.Ordinal));
        Assert.Contains(typeof(CorrectTransitionRequest).GetProperties(), p => p.Name == nameof(CorrectTransitionRequest.TransitionRecordId));
    }
}
