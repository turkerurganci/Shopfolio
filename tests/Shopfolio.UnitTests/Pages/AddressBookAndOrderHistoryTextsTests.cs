using Shopfolio.Pages.Components;

namespace Shopfolio.UnitTests.Pages;

/// <summary>The address book and order history screens' texts (T518; 04 5.26, 5.27): the inner navigation names the screens by the same words the screens' headings use.</summary>
public sealed class AddressBookAndOrderHistoryTextsTests
{
    [Fact]
    public void TheInnerNavigation_NamesTheTwoScreensByTheirHeadings()
    {
        Assert.Equal(AddressBookTexts.ScreenName, AccountTexts.NavAddresses);
        Assert.Equal(OrderHistoryTexts.ScreenName, AccountTexts.NavOrders);
    }

    [Theory]
    [InlineData("Ev", "Ev adresini düzenle")]
    [InlineData("İş", "İş adresini düzenle")]
    public void EditLabel_NamesTheEntry(string name, string expected)
    {
        Assert.Equal(expected, AddressBookTexts.EditLabel(name));
    }

    [Fact]
    public void DeleteLabel_NamesTheEntry()
    {
        Assert.Equal("Ev adresini sil", AddressBookTexts.DeleteLabel("Ev"));
    }

    [Fact]
    public void PlaceLine_JoinsTheDistrictAndTheProvince()
    {
        Assert.Equal("Çankaya, Ankara", AddressBookTexts.PlaceLine("Çankaya", "Ankara"));
    }

    [Theory]
    [InlineData("Kitap", 0, "Kitap")]
    [InlineData("Kitap", 1, "Kitap ve 1 kalem daha")]
    [InlineData("Kitap", 12, "Kitap ve 12 kalem daha")]
    [InlineData(null, 0, "")]
    public void ItemsLine_NamesTheFirstItemAndTheCountOfTheOthers(string? first, int others, string expected)
    {
        Assert.Equal(expected, OrderHistoryTexts.ItemsLine(first, others));
    }

    [Fact]
    public void ItemsLine_ANegativeCount_IsRefused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => OrderHistoryTexts.ItemsLine("Kitap", -1));
    }

    [Fact]
    public void TheHistoryQuery_CarriesOnlyThePage_AndTheBookQuery_OnlyTheEntryAndTheCreateSwitch()
    {
        Assert.Equal(["Page"], typeof(Shopfolio.Pages.Account.OrderHistoryQuery).GetProperties().Select(p => p.Name).ToArray());
        Assert.Equal(["AddressBookId", "Create"], typeof(Shopfolio.Pages.Account.AddressBookQuery).GetProperties().Select(p => p.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray());
    }
}
