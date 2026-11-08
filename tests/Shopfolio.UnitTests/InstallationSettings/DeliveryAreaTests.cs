using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.UnitTests.InstallationSettings;

/// <summary>
/// The delivery area as a value (T350 criterion 1; KK1; 02 3.20.1; 04 5.4.5, 5.13.23): which provinces are served, the refusal text that names the province and the restriction line the
/// storefront shows - none when every province is served, the shorter list otherwise, five names and a count past five, and the nowhere line when nothing is served.
/// </summary>
public sealed class DeliveryAreaTests
{
    private static readonly IReadOnlyList<ProvinceInfo> All =
    [
        .. Enumerable.Range(1, 81).Select(n => new ProvinceInfo(Guid.Parse($"019a4e3a-0000-7000-8000-{n:D12}"), $"{n:D2}", $"İl {n:D2}")),
    ];

    private static DeliveryArea Serving(params int[] plates) =>
        new([.. All.Where(p => plates.Contains(int.Parse(p.Code, System.Globalization.CultureInfo.InvariantCulture)))], [.. All.Where(p => !plates.Contains(int.Parse(p.Code, System.Globalization.CultureInfo.InvariantCulture)))]);

    [Fact]
    public void AnAreaThatServesEveryProvince_IsNotRestricted_AndHasNoLine()
    {
        DeliveryArea area = Serving([.. Enumerable.Range(1, 81)]);

        Assert.False(area.IsRestricted);
        Assert.False(area.DeliversNowhere);
        Assert.Null(area.RestrictionLine());
        Assert.All(All, p => Assert.True(area.IsServed(p.Id)));
    }

    [Fact]
    public void AProvinceNotOnTheList_IsNotServed()
    {
        Assert.False(Serving(1, 2).IsServed(Guid.NewGuid()));
        Assert.True(Serving(1, 2).IsServed(All[0].Id));
        Assert.False(Serving(1, 2).IsServed(All[2].Id));
    }

    [Fact]
    public void TheRefusalText_NamesTheProvince_AndNeedsOne()
    {
        Assert.Equal("Bu firma Van iline teslimat yapmıyor.", DeliveryArea.NotServedMessage("Van"));
        Assert.Throws<ArgumentException>(() => DeliveryArea.NotServedMessage(" "));
    }

    [Fact]
    public void ANothingServedArea_SaysSo()
    {
        DeliveryArea area = Serving();

        Assert.True(area.DeliversNowhere);
        Assert.Equal("Şu an hiçbir ile teslimat yapılmıyor.", area.RestrictionLine()!.Text);
        Assert.Null(area.RestrictionLine()!.FullList);
    }

    [Fact]
    public void AShortServedList_IsNamedInFull_AndTheShorterListWins()
    {
        DeliveryRestrictionLine served = Serving(6, 34, 35).RestrictionLine()!;
        DeliveryRestrictionLine notServed = Serving([.. Enumerable.Range(1, 79)]).RestrictionLine()!;

        Assert.Equal("Teslimat yapılan iller: İl 06, İl 34, İl 35", served.Text);
        Assert.Null(served.FullList);
        Assert.Equal("Teslimat yapılmayan iller: İl 80, İl 81", notServed.Text);
        Assert.Null(notServed.FullList);
    }

    [Fact]
    public void ALongList_ShowsTheFirstFiveAndACount_AndKeepsTheWholeListForTheOpenPart()
    {
        DeliveryRestrictionLine line = Serving(1, 2, 3, 4, 5, 6, 7).RestrictionLine()!;

        Assert.Equal("Teslimat yapılan iller: İl 01, İl 02, İl 03, İl 04, İl 05 ve 2 il daha", line.Text);
        Assert.Equal("Teslimat yapılan iller: İl 01, İl 02, İl 03, İl 04, İl 05, İl 06, İl 07", line.FullList);
    }

    [Fact]
    public void OnATie_TheServedListIsShown()
    {
        DeliveryArea area = new([.. All.Take(2)], [.. All.Skip(2).Take(2)]);

        Assert.StartsWith("Teslimat yapılan iller:", area.RestrictionLine()!.Text, StringComparison.Ordinal);
    }
}
