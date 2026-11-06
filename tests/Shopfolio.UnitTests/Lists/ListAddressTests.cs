using System.Reflection;

using Microsoft.AspNetCore.Http;

using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Lists;

namespace Shopfolio.UnitTests.Lists;

/// <summary>
/// The names and the addresses of a list (T210 criteria 1, 2, 8, 9; KK1, KK2, KK8, KK9; 07 2.6.1, 2.6.2, 2.6.7, 2.6.8; 09 5.3 item 2): the camelCase of
/// the model's fields is the parameter name, the first page is parameterless, the address a record returns to is the list's own address, and the mark of a
/// closed list follows the parameters the request carries.
/// </summary>
public sealed class ListAddressTests
{
    private const string Root = "/panel/siparisler";

    [Theory]
    [InlineData("Search", "search")]
    [InlineData("MinPrice", "minPrice")]
    [InlineData("InStock", "inStock")]
    [InlineData("Page", "page")]
    [InlineData("OrderStatus", "orderStatus")]
    [InlineData("A", "a")]
    public void Of_IsTheCamelCaseOfTheFieldName(string field, string expected)
    {
        Assert.Equal(expected, QueryParameterNames.Of(field));
    }

    [Fact]
    public void FieldsOf_AQueryModel_AreItsReadableAndSettableProperties_WithCamelCaseNames()
    {
        string[] names = [.. QueryParameterNames.FieldsOf(typeof(SampleQuery)).Select(QueryParameterNames.Of).Order(StringComparer.Ordinal)];

        Assert.Equal(["minPrice", "orderStatus", "page", "search"], names);
    }

    [Fact]
    public void PagedQuery_HasTheTextPageField_ThatTheModelNameRulesKnow()
    {
        PropertyInfo page = typeof(PagedQuery).GetProperty(nameof(PagedQuery.Page))!;

        Assert.Equal(typeof(string), page.PropertyType);
        Assert.True(page.IsDefined(typeof(SpecAssignedNameAttribute), inherit: false));
        Assert.True(typeof(IQueryModel).IsAssignableFrom(typeof(PagedQuery)));
    }

    [Fact]
    public void Of_OnTheFirstPageWithNoValue_IsTheRootAlone()
    {
        Assert.Equal(Root, ListAddress.Of(Root, new SampleQuery(), 1));
    }

    [Fact]
    public void Of_WritesThePageOnlyBeyondTheFirst_AndItComesLast()
    {
        SampleQuery query = new() { Search = "masa", MinPrice = "10,5" };

        Assert.Equal(Root + "?search=masa&minPrice=10%2C5", ListAddress.Of(Root, query, 1));
        Assert.Equal(Root + "?search=masa&minPrice=10%2C5&page=3", ListAddress.Of(Root, query, 3));
        Assert.Equal(Root + "?page=2", ListAddress.Of(Root, new SampleQuery(), 2));
    }

    [Fact]
    public void Of_WritesARepeatingFieldOncePerValue_AndEscapesTheValues()
    {
        SampleQuery query = new() { Search = "a&b=c d/é", OrderStatus = ["Paid", "Shipped"] };

        Assert.Equal(Root + "?search=a%26b%3Dc%20d%2F%C3%A9&orderStatus=Paid&orderStatus=Shipped", ListAddress.Of(Root, query, 1));
    }

    [Fact]
    public void Of_KeepsAnInvalidValueTheRequestCarried_SoTheReturnShowsWhatTheUserTyped()
    {
        SampleQuery query = new() { MinPrice = "abc" };

        Assert.Equal(Root + "?minPrice=abc", ListAddress.Of(Root, query, 1));
    }

    [Fact]
    public void Of_IsAlwaysAPathOfThisSite()
    {
        SampleQuery query = new() { Search = "//evil.example", MinPrice = "https://evil.example" };

        string address = ListAddress.Of(Root, query, 2);

        Assert.True(Shopfolio.Infrastructure.Security.ReturnPath.IsLocalPath(address));
        Assert.StartsWith(Root + "?", address, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("", false, false)]
    [InlineData("?page=2", false, false)]
    [InlineData("?page=abc", false, false)]
    [InlineData("?Page=2", false, false)]
    [InlineData("?minPrice=1", false, true)]
    [InlineData("?page=2&minPrice=1", false, true)]
    [InlineData("?inStock=true", false, true)]
    [InlineData("?unknown=1", false, true)]
    [InlineData("?sayfa=2", false, true)]
    [InlineData("?search=masa", true, true)]
    [InlineData("", true, true)]
    [InlineData("?page=2", true, true)]
    public void IsClosed_IsTrueForAnyParameterBeyondPage_AndForTheSearchPage(string queryString, bool searchPage, bool expected)
    {
        DefaultHttpContext context = new();
        context.Request.QueryString = new QueryString(queryString);

        Assert.Equal(expected, ListIndexing.IsClosed(context.Request.Query, searchPage));
    }

    private sealed class SampleQuery : PagedQuery
    {
        public string? Search { get; init; }

        public string? MinPrice { get; init; }

        public string[]? OrderStatus { get; init; }

        public string Computed => Search ?? "not a parameter";
    }
}
