using Shopfolio.Infrastructure;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The in-memory cache without a database (T229 criteria 1, 2 and 4, KK2 and KK4; 09 10.4 items 1, 2 and 6; 05 7.10.1): the five areas, the second
/// read that does not load, the invalidation that drops the value and raises the generation, the old read that an invalidation overtakes and that is
/// handed to its caller but never stored, the failed load that leaves the slot as it was, and areas that do not touch each other - the last also under
/// real parallel readers and saves.
/// </summary>
public sealed class AppCacheTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    // --- The areas (criterion 2; KK2) ---

    [Fact]
    public void TheAreas_AreTheFiveOf05_MenuCategoryTreeSettingsHomepageBlocksAndSitemap()
    {
        // 05 7.10.1: menu, category tree, settings, homepage blocks and the sitemap; 09 10.4 item 1 names them in this order.
        Assert.Equal(["Menu", "CategoryTree", "Settings", "HomepageBlocks", "Sitemap"], Enum.GetNames<CacheArea>());
        Assert.Equal(Enumerable.Range(0, 5), Enum.GetValues<CacheArea>().Select(a => (int)a));
    }

    [Theory]
    [MemberData(nameof(AreaNumbers))]
    public async Task EachArea_HasItsOwnSlot_AndItsSecondReadDoesNotLoad(int areaNumber)
    {
        CacheArea area = (CacheArea)areaNumber;
        AppCache cache = new();
        int loads = 0;

        Box first = await cache.GetOrLoadAsync(area, _ => Task.FromResult(new Box(++loads)), Ct);
        Box second = await cache.GetOrLoadAsync(area, _ => Task.FromResult(new Box(++loads)), Ct);

        Assert.Equal(1, loads);
        Assert.Same(first, second);
    }

    public static TheoryData<int> AreaNumbers => [.. Enum.GetValues<CacheArea>().Select(a => (int)a)];

    [Fact]
    public async Task AnUnknownArea_IsRefused()
    {
        AppCache cache = new();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => cache.GetOrLoadAsync((CacheArea)5, _ => Task.FromResult(new Box(1)), Ct));
        Assert.Throws<ArgumentOutOfRangeException>(() => cache.Invalidate((CacheArea)(-1)));
    }

    [Fact]
    public async Task AValueOfAnotherType_IsAProgrammingError()
    {
        AppCache cache = new();
        await cache.GetOrLoadAsync(CacheArea.Menu, _ => Task.FromResult(new Box(1)), Ct);

        await Assert.ThrowsAsync<InvalidCastException>(() => cache.GetOrLoadAsync(CacheArea.Menu, _ => Task.FromResult("text"), Ct));
    }

    // --- Invalidation and the generation (criteria 3 and 4; KK3, KK4) ---

    [Fact]
    public async Task Invalidate_DropsTheValue_AndRaisesTheGeneration_SoTheNextReadLoadsTheNewValue()
    {
        AppCache cache = new();
        await cache.GetOrLoadAsync(CacheArea.Settings, _ => Task.FromResult(new Box(1)), Ct);
        long before = cache.GenerationOf(CacheArea.Settings);

        cache.Invalidate(CacheArea.Settings);

        Assert.Equal(before + 1, cache.GenerationOf(CacheArea.Settings));
        Box next = await cache.GetOrLoadAsync(CacheArea.Settings, _ => Task.FromResult(new Box(2)), Ct);
        Assert.Equal(2, next.Value);
    }

    [Fact]
    public async Task AnInvalidationDuringTheLoad_TheOldValueGoesToItsCaller_ButIsNotStored_AndTheNextReadLoadsAgain()
    {
        AppCache cache = new();
        TaskCompletionSource loading = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<Box> release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<Box> oldRead = cache.GetOrLoadAsync(
            CacheArea.Settings,
            async _ =>
            {
                loading.SetResult();
                return await release.Task;
            },
            Ct);
        await loading.Task;

        // The record is saved and committed while the old read is still loading.
        cache.Invalidate(CacheArea.Settings);
        release.SetResult(new Box(1));

        Assert.Equal(1, (await oldRead).Value);
        int loads = 0;
        Box next = await cache.GetOrLoadAsync(CacheArea.Settings, _ => Task.FromResult(new Box(10 + ++loads)), Ct);
        Assert.Equal(11, next.Value);
        Assert.Equal(1, loads);
        Assert.Same(next, await cache.GetOrLoadAsync(CacheArea.Settings, _ => Task.FromResult(new Box(99)), Ct));
    }

    [Fact]
    public async Task AReadThatStartsAfterTheInvalidation_IsStored()
    {
        AppCache cache = new();
        cache.Invalidate(CacheArea.Menu);

        Box loaded = await cache.GetOrLoadAsync(CacheArea.Menu, _ => Task.FromResult(new Box(1)), Ct);

        Assert.Same(loaded, await cache.GetOrLoadAsync(CacheArea.Menu, _ => Task.FromResult(new Box(2)), Ct));
    }

    [Fact]
    public async Task AFailedLoad_LeavesTheSlotEmpty_AndTheGenerationWhereItWas()
    {
        AppCache cache = new();
        long before = cache.GenerationOf(CacheArea.CategoryTree);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => cache.GetOrLoadAsync<Box>(CacheArea.CategoryTree, _ => throw new InvalidOperationException("the database is gone"), Ct));

        Assert.Equal(before, cache.GenerationOf(CacheArea.CategoryTree));
        Box next = await cache.GetOrLoadAsync(CacheArea.CategoryTree, _ => Task.FromResult(new Box(2)), Ct);
        Assert.Equal(2, next.Value);
    }

    [Fact]
    public async Task ALoadThatGivesNothing_IsAProgrammingError_AndStoresNothing()
    {
        AppCache cache = new();

        await Assert.ThrowsAsync<ArgumentNullException>(() => cache.GetOrLoadAsync<Box>(CacheArea.Sitemap, _ => Task.FromResult<Box>(null!), Ct));

        Box next = await cache.GetOrLoadAsync(CacheArea.Sitemap, _ => Task.FromResult(new Box(3)), Ct);
        Assert.Equal(3, next.Value);
    }

    [Fact]
    public async Task TwoAreas_DoNotTouchEachOther()
    {
        AppCache cache = new();
        Box menu = await cache.GetOrLoadAsync(CacheArea.Menu, _ => Task.FromResult(new Box(1)), Ct);
        await cache.GetOrLoadAsync(CacheArea.HomepageBlocks, _ => Task.FromResult(new Box(2)), Ct);

        cache.Invalidate(CacheArea.HomepageBlocks);

        Assert.Equal(0, cache.GenerationOf(CacheArea.Menu));
        Assert.Equal(1, cache.GenerationOf(CacheArea.HomepageBlocks));
        Assert.Same(menu, await cache.GetOrLoadAsync(CacheArea.Menu, _ => Task.FromResult(new Box(9)), Ct));
        Assert.Equal(9, (await cache.GetOrLoadAsync(CacheArea.HomepageBlocks, _ => Task.FromResult(new Box(9)), Ct)).Value);
    }

    // --- The generation race under real parallelism (criterion 4; KK4) ---

    [Fact]
    public async Task ParallelReadersAndSaves_NeverLeaveAValueOlderThanTheLastSave()
    {
        const int saves = 3000;
        AppCache cache = new();
        int saved = 0;

        // The "database" is one number. A save writes it and then invalidates, as the runner does after the commit; a reader reads it inside the
        // load and yields before handing it back, so an invalidation can come between its read and its write.
        Task<Box> Load(CancellationToken ct) => LoadAsync();

        async Task<Box> LoadAsync()
        {
            int value = Volatile.Read(ref saved);
            await Task.Yield();
            return new Box(value);
        }

        Task[] readers =
        [
            .. Enumerable.Range(0, Math.Max(4, Environment.ProcessorCount)).Select(_ => Task.Run(
                async () =>
                {
                    for (int i = 0; i < saves; i++)
                    {
                        await cache.GetOrLoadAsync(CacheArea.Settings, Load, Ct);
                    }
                },
                Ct)),
        ];
        Task writer = Task.Run(
            () =>
            {
                for (int i = 1; i <= saves; i++)
                {
                    Volatile.Write(ref saved, i);
                    cache.Invalidate(CacheArea.Settings);
                }
            },
            Ct);

        await Task.WhenAll([.. readers, writer]);

        // Whatever the slot holds now was read after the last invalidation; an empty slot loads the last saved value.
        Box last = await cache.GetOrLoadAsync(CacheArea.Settings, Load, Ct);
        Assert.Equal(saves, last.Value);
        Assert.Equal(saves, cache.GenerationOf(CacheArea.Settings));
    }

    /// <summary>An immutable cached value of the tests.</summary>
    private sealed record Box(int Value);
}
