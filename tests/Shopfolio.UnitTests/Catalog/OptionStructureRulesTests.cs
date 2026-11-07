using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts.Variants;

namespace Shopfolio.UnitTests.Catalog;

/// <summary>
/// The option dimension and variant binding rules as pure functions (T332; 02 3.3.1-3.3.5; 06 4.6, 4.7; 11 T332 criteria 3-5; KK3-KK5): the dimension limit is read from the product
/// constant; a variant is bound to the first value of the option of order 1 and the second value of the option of order 2, each required where the product has that option and
/// forbidden where it has not; and the matrix of the values need not be full - a combination no variant opens is a cell with no variant.
/// </summary>
public sealed class OptionStructureRulesTests
{
    private static readonly Guid Red = Guid.Parse("00000000-0000-7000-8000-000000000001");

    private static readonly Guid Blue = Guid.Parse("00000000-0000-7000-8000-000000000002");

    private static readonly Guid SizeM = Guid.Parse("00000000-0000-7000-8000-000000000003");

    private static readonly Guid SizeL = Guid.Parse("00000000-0000-7000-8000-000000000004");

    private static readonly Guid VariantRedM = Guid.Parse("00000000-0000-7000-8000-0000000000a1");

    private static readonly Guid VariantBlueL = Guid.Parse("00000000-0000-7000-8000-0000000000a2");

    [Fact]
    public void TheNextOptionOrder_IsTheLowestFreeOrderUpToTheConstantLimit_AndNullAtTheLimit()
    {
        int limit = ProductConstants.OptionDimensionMax;

        Assert.Equal(1, OptionStructureRules.NextOptionOrder([]));
        Assert.Equal(2, OptionStructureRules.NextOptionOrder([1]));
        Assert.Equal(1, OptionStructureRules.NextOptionOrder([2]));
        Assert.Equal(1, OptionStructureRules.NextOptionOrder([limit + 1]));
        Assert.Null(OptionStructureRules.NextOptionOrder(Enumerable.Range(1, limit)));
        Assert.Null(OptionStructureRules.NextOptionOrder(Enumerable.Range(1, limit + 1)));
        Assert.All(Enumerable.Range(0, limit), taken => Assert.Equal(taken + 1, OptionStructureRules.NextOptionOrder(Enumerable.Range(1, taken))));
    }

    public static TheoryData<int[], bool, int?, bool, int?, VariantStructureCheck> Bindings => new()
    {
        { [], false, null, false, null, VariantStructureCheck.Valid },
        { [], true, 1, false, null, VariantStructureCheck.ValueForAbsentOption },
        { [], false, null, true, 2, VariantStructureCheck.ValueForAbsentOption },
        { [1], true, 1, false, null, VariantStructureCheck.Valid },
        { [1], false, null, false, null, VariantStructureCheck.ValueMissing },
        { [1], true, 1, true, 2, VariantStructureCheck.ValueForAbsentOption },
        { [1], true, null, false, null, VariantStructureCheck.ValueNotOfTheProduct },
        { [1], true, 2, false, null, VariantStructureCheck.ValueNotOfTheProduct },
        { [1, 2], true, 1, true, 2, VariantStructureCheck.Valid },
        { [1, 2], true, 1, false, null, VariantStructureCheck.ValueMissing },
        { [1, 2], false, null, true, 2, VariantStructureCheck.ValueMissing },
        { [1, 2], false, null, false, null, VariantStructureCheck.ValueMissing },
        { [1, 2], true, 2, true, 1, VariantStructureCheck.ValueNotOfTheProduct },
        { [1, 2], true, 1, true, 1, VariantStructureCheck.ValueNotOfTheProduct },
        { [1, 2], true, 1, true, null, VariantStructureCheck.ValueNotOfTheProduct },
        { [1, 2], true, null, true, 2, VariantStructureCheck.ValueNotOfTheProduct },
    };

    [Theory]
    [MemberData(nameof(Bindings))]
    public void AVariantsValues_AreTheProductsOwnAndEachRequiredWhereTheOptionExists(
        int[] optionOrders, bool firstGiven, int? firstOwner, bool secondGiven, int? secondOwner, VariantStructureCheck expected)
    {
        BoundValue first = firstGiven ? BoundValue.Of(firstOwner) : BoundValue.None;
        BoundValue second = secondGiven ? BoundValue.Of(secondOwner) : BoundValue.None;

        Assert.Equal(expected, OptionStructureRules.CheckBinding(optionOrders.ToHashSet(), first, second));
    }

    [Fact]
    public void TheMatrixOfTwoDimensions_NeedNotBeFull_AndTheMissingCombinationIsACellNoVariantOpens()
    {
        IReadOnlyList<OptionMatrixCell> cells = OptionStructureRules.BuildMatrix(
            [Red, Blue],
            [SizeM, SizeL],
            [(VariantRedM, Red, SizeM), (VariantBlueL, Blue, SizeL)]);

        Assert.Equal(
            [
                new OptionMatrixCell(Red, SizeM, VariantRedM),
                new OptionMatrixCell(Red, SizeL, null),
                new OptionMatrixCell(Blue, SizeM, null),
                new OptionMatrixCell(Blue, SizeL, VariantBlueL),
            ],
            cells);
        Assert.Equal(2, cells.Count(c => c.IsOpened));
        Assert.All(cells.Where(c => !c.IsOpened), c => Assert.Null(c.VariantId));
    }

    [Fact]
    public void TheMatrixOfOneDimension_IsACellPerValue_AndOfNoDimensionOneCellWithNoValues()
    {
        IReadOnlyList<OptionMatrixCell> oneDimension = OptionStructureRules.BuildMatrix([Red, Blue], [], [(VariantRedM, Red, null)]);
        Assert.Equal([new OptionMatrixCell(Red, null, VariantRedM), new OptionMatrixCell(Blue, null, null)], oneDimension);

        IReadOnlyList<OptionMatrixCell> none = OptionStructureRules.BuildMatrix([], [], [(VariantRedM, null, null)]);
        Assert.Equal([new OptionMatrixCell(null, null, VariantRedM)], none);

        IReadOnlyList<OptionMatrixCell> noneOpened = OptionStructureRules.BuildMatrix([], [], []);
        OptionMatrixCell cell = Assert.Single(noneOpened);
        Assert.False(cell.IsOpened);
    }

    [Fact]
    public void AVariantBoundToAPairOutsideTheValues_OpensNoCell_AndTheCellsAreFirstValueMajor()
    {
        IReadOnlyList<OptionMatrixCell> cells = OptionStructureRules.BuildMatrix([Red], [SizeM, SizeL], [(VariantBlueL, Blue, SizeL)]);

        Assert.Equal([new OptionMatrixCell(Red, SizeM, null), new OptionMatrixCell(Red, SizeL, null)], cells);
    }

    [Fact]
    public void TheCatalogRegistration_AddsTheStockCodeAndStructureReads()
    {
        ServiceCollection services = new();
        services.AddCatalogModule();

        Assert.Contains(services, d => d.ServiceType == typeof(StockCodes) && d.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, d => d.ServiceType == typeof(ProductStructureReader) && d.Lifetime == ServiceLifetime.Scoped);
    }
}
