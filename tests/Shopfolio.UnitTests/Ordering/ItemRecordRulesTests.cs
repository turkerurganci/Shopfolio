using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.UnitTests.Architecture;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// The pure rules of the item records (T320; 03 1.7.4, 1.9.1; 02 Z-18, 7.4.9): the unit rule, the claim's own transitions and history, the claim's channel and the
/// conditional exemptions, and the source scan's one allowed caller of the claim's status writer (D-274).
/// </summary>
public sealed class ItemRecordRulesTests
{
    private static readonly DateTimeOffset At = new(2026, 11, 8, 9, 0, 0, TimeSpan.Zero);

    private static ItemRecordSubject Subject(ProductType type, OrderStatus status, DateOnly? deliveryDate, DateTimeOffset? markedAt = null) =>
        new(Guid.NewGuid(), Guid.NewGuid(), 3, type, status, markedAt ?? (deliveryDate is null ? null : At), deliveryDate, null);

    // ---- KK3: the unit rule ----

    [Theory]
    [InlineData(0, 2, false)]
    [InlineData(1, 2, true)]
    [InlineData(2, 2, true)]
    [InlineData(3, 2, false)]
    [InlineData(1, 0, false)]
    public void IsSelectable_IsOneToTheOpenQuantity(int selected, int open, bool expected)
    {
        Assert.Equal(expected, ItemQuantities.IsSelectable(selected, open));
    }

    [Fact]
    public void Open_AndAwaitingReceipt_NeverGoBelowZero()
    {
        Assert.Equal(1, ItemQuantities.Open(3, 2));
        Assert.Equal(0, ItemQuantities.Open(3, 4));
        Assert.Equal(1, ItemQuantities.AwaitingReceipt(4, 1, 1, 1));
        Assert.Equal(0, ItemQuantities.AwaitingReceipt(2, 2, 1, 0));
    }

    // ---- KK5: only the conditional exemptions ----

    [Theory]
    [InlineData(WithdrawalExemptionReason.HygieneSensitive, true)]
    [InlineData(WithdrawalExemptionReason.SealedMedia, true)]
    [InlineData(WithdrawalExemptionReason.MadeToOrder, false)]
    [InlineData(WithdrawalExemptionReason.Perishable, false)]
    [InlineData(WithdrawalExemptionReason.ExpiryDated, false)]
    [InlineData(null, false)]
    public void IsConditional_IsOnlyTheHygieneSensitiveAndTheSealedMedia(WithdrawalExemptionReason? reason, bool expected)
    {
        Assert.Equal(expected, WithdrawalExemptions.IsConditional(reason));
    }

    // ---- KK6, KK7: the claim's transitions and its history ----

    [Fact]
    public void Resolve_IsTheAdministratorsOnly_AndAppendsTheEntryWithTheStatus()
    {
        DefectClaim claim = DefectClaim.Create(Guid.NewGuid(), 1, "Kırık", null, At);

        Assert.Equal(ItemRecordRefusal.ActorNotAllowed, DefectClaimStatusWriter.TryChange(claim, DefectClaimStatus.Resolved, ActorKind.Buyer, null, At));
        Assert.Equal(ItemRecordRefusal.ActorNotAllowed, DefectClaimStatusWriter.TryChange(claim, DefectClaimStatus.Resolved, ActorKind.System, null, At));
        Assert.Empty(claim.StatusHistory);

        Assert.Null(DefectClaimStatusWriter.TryChange(claim, DefectClaimStatus.Resolved, ActorKind.Admin, null, At.AddDays(1)));
        Assert.Equal(DefectClaimStatus.Resolved, claim.DefectClaimStatus);
        Assert.Equal([new DefectClaimHistoryEntry(At.AddDays(1), DefectClaimStatus.Resolved, ActorKind.Admin, null)], claim.StatusHistory);

        Assert.Equal(ItemRecordRefusal.StatusNotAllowed, DefectClaimStatusWriter.TryChange(claim, DefectClaimStatus.Resolved, ActorKind.Admin, null, At));
    }

    [Fact]
    public void Reopen_StartsFromResolved_TheCustomerMayRaiseTheUnits_NeverLowerThem()
    {
        DefectClaim claim = DefectClaim.Create(Guid.NewGuid(), 2, "Kırık", null, At);
        Assert.Equal(ItemRecordRefusal.StatusNotAllowed, DefectClaimStatusWriter.TryChange(claim, DefectClaimStatus.Open, ActorKind.Buyer, null, At));
        DefectClaimStatusWriter.TryChange(claim, DefectClaimStatus.Resolved, ActorKind.Admin, null, At);

        Assert.Equal(ItemRecordRefusal.QuantityExceedsOpen, DefectClaimStatusWriter.TryChange(claim, DefectClaimStatus.Open, ActorKind.Buyer, 1, At));
        Assert.Equal(ItemRecordRefusal.ActorNotAllowed, DefectClaimStatusWriter.TryChange(claim, DefectClaimStatus.Open, ActorKind.Admin, 3, At));
        Assert.Equal(DefectClaimStatus.Resolved, claim.DefectClaimStatus);

        Assert.Null(DefectClaimStatusWriter.TryChange(claim, DefectClaimStatus.Open, ActorKind.Buyer, 3, At.AddDays(2)));
        Assert.Equal((DefectClaimStatus.Open, 3), (claim.DefectClaimStatus, claim.Quantity));
        Assert.Equal(new DefectClaimHistoryEntry(At.AddDays(2), DefectClaimStatus.Open, ActorKind.Buyer, 3), claim.StatusHistory[^1]);
        Assert.Equal(At.AddDays(2), DefectClaimRetention.LastChange(claim.CreatedAt, claim.StatusHistory));
    }

    [Fact]
    public void Reopen_ByTheAdministrator_KeepsTheUnits_AndRecordsTheKind()
    {
        DefectClaim claim = DefectClaim.Create(Guid.NewGuid(), 2, "Kırık", null, At);
        DefectClaimStatusWriter.TryChange(claim, DefectClaimStatus.Resolved, ActorKind.Admin, null, At);

        Assert.Null(DefectClaimStatusWriter.TryChange(claim, DefectClaimStatus.Open, ActorKind.Admin, null, At.AddDays(3)));
        Assert.Equal(new DefectClaimHistoryEntry(At.AddDays(3), DefectClaimStatus.Open, ActorKind.Admin, null), claim.StatusHistory[^1]);
        Assert.Equal(2, claim.Quantity);
    }

    // ---- KK7: the channel of Z-18 ----

    [Fact]
    public void Window_OnAPhysicalItem_OpensWithTheShipment_AndDoesNotRunWithoutADeliveryDate()
    {
        Assert.False(DefectClaimWindow.IsOpen(Subject(ProductType.Physical, OrderStatus.Preparing, null), At));
        Assert.True(DefectClaimWindow.IsOpen(Subject(ProductType.Physical, OrderStatus.Shipped, null), At.AddYears(5)));
        Assert.True(DefectClaimWindow.IsOpen(Subject(ProductType.Physical, OrderStatus.DeliveryFailed, null), At));
        Assert.False(DefectClaimWindow.IsOpen(Subject(ProductType.Physical, OrderStatus.Cancelled, null), At));
    }

    [Fact]
    public void Window_ClosesAfterTheLastDay_TwoYearsFromTheDeliveryDate_InTurkeyTime()
    {
        DateOnly delivered = new(2026, 11, 8);
        ItemRecordSubject subject = Subject(ProductType.Physical, OrderStatus.Delivered, delivered);

        Assert.Equal(new DateOnly(2028, 11, 8), DefectClaimWindow.LastDay(subject));
        Assert.True(DefectClaimWindow.IsOpen(subject, new DateTimeOffset(2028, 11, 8, 20, 59, 59, TimeSpan.Zero)));
        Assert.False(DefectClaimWindow.IsOpen(subject, new DateTimeOffset(2028, 11, 8, 21, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public void Window_OnADigitalOrServiceItem_OpensWithTheDeliveryMark()
    {
        Assert.False(DefectClaimWindow.IsOpen(Subject(ProductType.Service, OrderStatus.Preparing, null), At));
        Assert.False(DefectClaimWindow.IsOpen(Subject(ProductType.Digital, OrderStatus.Shipped, null), At));
        Assert.True(DefectClaimWindow.IsOpen(Subject(ProductType.Service, OrderStatus.Delivered, new DateOnly(2026, 11, 8)), At));
        Assert.False(DefectClaimWindow.IsOpen(Subject(ProductType.Digital, OrderStatus.Delivered, new DateOnly(2026, 11, 8)), At.AddYears(2).AddDays(1)));
    }

    // ---- D-274: the scan's one caller of the claim's status writer ----

    [Fact]
    public void Scan_AllowsApplyStatus_OnlyInTheWritersFile_ByItsFullPath()
    {
        const string text = "class C\n{\n    void M(DefectClaim claim) => claim.ApplyStatus(DefectClaimStatus.Open, ActorKind.Buyer, now, null);\n}\n";

        Assert.DoesNotContain(TransitionUnitSourceScan.Scan(TransitionUnitSourceScan.DefectClaimWriterFile, text), f => f.Rule == TransitionUnitSourceScan.OutsideCall);
        foreach (string path in new[] { TransitionUnitSourceScan.UnitFile, "src/Shopfolio.Web/Modules/AfterSales/AfterSalesModule.cs", "other/src/Shopfolio.Web/Modules/AfterSales/DefectClaimStatusWriter.cs" })
        {
            Assert.Contains((TransitionUnitSourceScan.OutsideCall, 3), TransitionUnitSourceScan.Scan(path, text));
        }
    }

    [Fact]
    public void Scan_AllowsTheModulesStatusChange_OnlyInTheTransitionUnitsItemRecordFile_AndTheModulesOwnFiles()
    {
        const string text = "class C\n{\n    Task M() => _afterSales.ChangeDefectClaimStatusAsync(change, ct);\n    string N() => nameof(IAfterSalesModule.ChangeDefectClaimStatusAsync);\n}\n";

        foreach (string allowed in TransitionUnitSourceScan.DefectClaimChangeFiles)
        {
            Assert.DoesNotContain(TransitionUnitSourceScan.Scan(allowed, text), f => f.Rule == TransitionUnitSourceScan.OutsideCall);
        }

        IReadOnlyList<(string Rule, int Line)> outside = TransitionUnitSourceScan.Scan("src/Shopfolio.Web/Pages/Orders/Claim.cshtml.cs", text);
        Assert.Contains((TransitionUnitSourceScan.OutsideCall, 3), outside);
        Assert.Contains((TransitionUnitSourceScan.OutsideCall, 4), outside);
    }
}
