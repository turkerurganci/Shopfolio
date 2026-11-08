using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts.Lists;

namespace Shopfolio.UnitTests.Catalog;

/// <summary>
/// The category tree's level and move rules as pure functions (T333; 02 3.4.1, 3.4.3, 3.4.6; 06 4.9; 09 9.4 item 4; 11 T333 criteria 1, 3, 6; KK1, KK3, KK6): a category's level is its
/// parent's plus one and 1 at the root; the depth comes from the product constant; a move shifts the whole branch by one difference; a move past the depth and a move under the
/// category's own branch are refused with no level changed; and a branch's scope for a list holds the category and everything below it.
/// </summary>
public sealed class CategoryLevelsTests
{
    private static readonly Guid Electric = Id(1);

    private static readonly Guid Cable = Id(2);

    private static readonly Guid Nya = Id(3);

    private static readonly Guid Garden = Id(4);

    private static readonly Guid Tools = Id(5);

    private static List<CategoryPlace> Tree() =>
    [
        new(Electric, null, 1),
        new(Cable, Electric, 2),
        new(Nya, Cable, 3),
        new(Garden, null, 1),
        new(Tools, Garden, 2),
    ];

    [Fact]
    public void ALevel_IsTheParentsPlusOne_AndOneAtTheRoot()
    {
        Assert.Equal(1, CategoryLevels.LevelUnder(null));
        Assert.All(Enumerable.Range(1, ProductConstants.CategoryLevelMax), parent => Assert.Equal(parent + 1, CategoryLevels.LevelUnder(parent)));
    }

    [Fact]
    public void TheDepth_ComesFromTheProductConstant_AndAFourthLevelDoesNotFit()
    {
        int max = ProductConstants.CategoryLevelMax;

        Assert.False(CategoryLevels.FitsDepth(0));
        Assert.All(Enumerable.Range(1, max), level => Assert.True(CategoryLevels.FitsDepth(level)));
        Assert.False(CategoryLevels.FitsDepth(max + 1));
        Assert.False(CategoryLevels.FitsDepth(CategoryLevels.LevelUnder(max)));
    }

    [Fact]
    public void TheSubtree_IsTheCategoryFirstThenEveryCategoryBelowIt_AndEmptyForAnUnknownCategory()
    {
        Assert.Equal([Electric, Cable, Nya], CategoryLevels.SubtreeOf(Electric, Tree()));
        Assert.Equal([Nya], CategoryLevels.SubtreeOf(Nya, Tree()));
        Assert.Empty(CategoryLevels.SubtreeOf(Id(99), Tree()));
    }

    [Fact]
    public void AMoveToTheRoot_ShiftsTheWholeBranchByTheSameDifference()
    {
        CategoryMoveVerdict verdict = CategoryLevels.CheckMove(Cable, null, Tree());

        Assert.Equal(CategoryMoveKind.Allowed, verdict.Kind);
        Assert.Equal(2, verdict.NewLevels.Count);
        Assert.Equal(1, verdict.NewLevels[Cable]);
        Assert.Equal(2, verdict.NewLevels[Nya]);
    }

    [Fact]
    public void AMoveUnderAnotherParent_TakesTheParentsLevelPlusOneForTheCategoryAndKeepsTheBranchShape()
    {
        CategoryMoveVerdict verdict = CategoryLevels.CheckMove(Tools, Electric, Tree());

        Assert.Equal(CategoryMoveKind.Allowed, verdict.Kind);
        Assert.Equal(2, verdict.NewLevels[Tools]);
    }

    [Fact]
    public void AMoveThatPutsAnyCategoryOfTheBranchPastTheDepth_IsRefused_WithNoLevelChanged()
    {
        // Garden has one level below it; Nya is already at the deepest level, so nothing can go under it.
        CategoryMoveVerdict beneathTheDeepest = CategoryLevels.CheckMove(Garden, Nya, Tree());
        // Electric's branch is three levels deep; under Garden (level 1) it would reach level 4.
        CategoryMoveVerdict wholeBranchPastTheDepth = CategoryLevels.CheckMove(Electric, Garden, Tree());

        Assert.Equal(CategoryMoveKind.DepthExceeded, beneathTheDeepest.Kind);
        Assert.Equal(CategoryMoveKind.DepthExceeded, wholeBranchPastTheDepth.Kind);
        Assert.Empty(beneathTheDeepest.NewLevels);
        Assert.Empty(wholeBranchPastTheDepth.NewLevels);
    }

    [Fact]
    public void ABranchThatJustFits_IsMoved_AndOneMoreLevelIsNot()
    {
        List<CategoryPlace> tree = Tree();
        // Cable (level 2) with Nya (level 3) under Garden (level 1) would be levels 2 and 3: it fits.
        Assert.Equal(CategoryMoveKind.Allowed, CategoryLevels.CheckMove(Cable, Garden, tree).Kind);
        // Under Tools (level 2) the same branch would be levels 3 and 4: it does not.
        Assert.Equal(CategoryMoveKind.DepthExceeded, CategoryLevels.CheckMove(Cable, Tools, tree).Kind);
    }

    [Fact]
    public void ACategoryMovedUnderItselfOrItsOwnBranch_IsRefused()
    {
        Assert.Equal(CategoryMoveKind.IntoOwnSubtree, CategoryLevels.CheckMove(Electric, Electric, Tree()).Kind);
        Assert.Equal(CategoryMoveKind.IntoOwnSubtree, CategoryLevels.CheckMove(Electric, Nya, Tree()).Kind);
        Assert.Equal(CategoryMoveKind.IntoOwnSubtree, CategoryLevels.CheckMove(Cable, Nya, Tree()).Kind);
    }

    [Fact]
    public void AMissingCategoryOrParent_IsNamedAndWritesNothing()
    {
        Assert.Equal(CategoryMoveKind.CategoryNotFound, CategoryLevels.CheckMove(Id(99), null, Tree()).Kind);
        CategoryMoveVerdict noParent = CategoryLevels.CheckMove(Cable, Id(99), Tree());

        Assert.Equal(CategoryMoveKind.ParentNotFound, noParent.Kind);
        Assert.Empty(noParent.NewLevels);
    }

    [Fact]
    public void AMoveUnderTheParentTheCategoryAlreadyHas_IsAllowed_AndKeepsEveryLevel()
    {
        CategoryMoveVerdict verdict = CategoryLevels.CheckMove(Cable, Electric, Tree());

        Assert.Equal(CategoryMoveKind.Allowed, verdict.Kind);
        Assert.Equal(2, verdict.NewLevels[Cable]);
        Assert.Equal(3, verdict.NewLevels[Nya]);
    }

    [Fact]
    public void EveryAllowedMove_LeavesEachCategoryOfTheBranchAtItsParentsLevelPlusOne()
    {
        List<CategoryPlace> tree = Tree();
        foreach (CategoryPlace node in tree)
        {
            foreach (Guid? target in new Guid?[] { null, Electric, Cable, Nya, Garden, Tools })
            {
                CategoryMoveVerdict verdict = CategoryLevels.CheckMove(node.Id, target, tree);
                if (verdict.Kind != CategoryMoveKind.Allowed)
                {
                    continue;
                }

                Dictionary<Guid, Guid?> parents = tree.ToDictionary(c => c.Id, c => c.Id == node.Id ? target : c.ParentId);
                Dictionary<Guid, int> levels = tree.ToDictionary(c => c.Id, c => verdict.NewLevels.TryGetValue(c.Id, out int moved) ? moved : c.Level);
                Assert.All(tree, c => Assert.Equal(parents[c.Id] is Guid p ? levels[p] + 1 : 1, levels[c.Id]));
                Assert.All(levels.Values, level => Assert.True(CategoryLevels.FitsDepth(level)));
            }
        }
    }

    [Fact]
    public void AnInvertedPriceRange_IsVoid_AndAnyOtherRangeIsNot()
    {
        Assert.True(new ProductListFilter(MinPrice: 200m, MaxPrice: 100m).IsPriceRangeInverted);
        Assert.False(new ProductListFilter(MinPrice: 100m, MaxPrice: 100m).IsPriceRangeInverted);
        Assert.False(new ProductListFilter(MinPrice: 100m).IsPriceRangeInverted);
        Assert.False(new ProductListFilter(MaxPrice: 100m).IsPriceRangeInverted);
        Assert.False(ProductListFilter.None.IsPriceRangeInverted);
    }

    private static Guid Id(int n) => Guid.Parse($"00000000-0000-7000-8000-{n:x12}");
}
