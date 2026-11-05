using Shopfolio.Modules.Ordering;

namespace Shopfolio.UnitTests.Concurrency;

/// <summary>
/// The customer's item cancellation and withdrawal compare the open quantity the screen showed with the one under the order's lock (T228
/// criterion 4; 05 7.3.2; K-1202): one differing item refuses the whole request.
/// </summary>
public sealed class ShownOpenQuantitiesTests
{
    private static readonly Guid First = new("10000000-0000-0000-0000-000000000001");
    private static readonly Guid Second = new("10000000-0000-0000-0000-000000000002");

    [Fact]
    public void Match_EveryShownItemEqualToNow_IsTrue()
    {
        Assert.True(ShownOpenQuantities.Match(
            new Dictionary<Guid, int> { [First] = 2, [Second] = 1 },
            new Dictionary<Guid, int> { [First] = 2, [Second] = 1 }));
    }

    [Fact]
    public void Match_AnItemNotShownMayDiffer_IsTrue()
    {
        Assert.True(ShownOpenQuantities.Match(
            new Dictionary<Guid, int> { [First] = 2 },
            new Dictionary<Guid, int> { [First] = 2, [Second] = 0 }));
    }

    [Fact]
    public void Match_OneShownItemChanged_IsFalseForTheWholeRequest()
    {
        Assert.False(ShownOpenQuantities.Match(
            new Dictionary<Guid, int> { [First] = 2, [Second] = 1 },
            new Dictionary<Guid, int> { [First] = 2, [Second] = 0 }));
    }

    [Fact]
    public void Match_AShownItemTheOrderDoesNotHave_IsFalse()
    {
        Assert.False(ShownOpenQuantities.Match(new Dictionary<Guid, int> { [First] = 1 }, new Dictionary<Guid, int> { [Second] = 1 }));
    }

    [Fact]
    public void Match_AnEmptyRequest_IsFalse()
    {
        Assert.False(ShownOpenQuantities.Match(new Dictionary<Guid, int>(), new Dictionary<Guid, int> { [First] = 1 }));
    }
}
