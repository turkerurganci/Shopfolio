using System.Text.RegularExpressions;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.UnitTests.Security;

/// <summary>
/// The order number (T354 KK1; 02 3.22.1; 09 15.1 item 1; P-19): the year, a hyphen and a random block of the product's length from a 32-character alphabet without the
/// characters that look alike; plus the renewal of the access key (KK4).
/// </summary>
public sealed class OrderNumberTests
{
    private const int Samples = 3000;

    [Fact]
    public void BlockLength_IsTheProductConstant_AndTheTestsBelowUseIt()
    {
        Assert.Equal(6, ProductConstants.OrderNumberRandomBlockLength);
    }

    [Fact]
    public void Alphabet_HasThirtyTwoDistinctCharacters_AndNoneThatLooksAlike()
    {
        Assert.Equal(32, OrderNumber.Alphabet.Length);
        Assert.Equal(32, OrderNumber.Alphabet.Distinct().Count());
        foreach (char confusable in "01IOl")
        {
            Assert.DoesNotContain(confusable, OrderNumber.Alphabet);
        }

        // The rule of 05 6.4.2: the digits 2-9 and the capital letters except I and O, and nothing else.
        Assert.Equal(
            [.. "23456789ABCDEFGHJKLMNPQRSTUVWXYZ".Order()],
            [.. OrderNumber.Alphabet.Order()]);
        Assert.Equal(
            ['2', '3', '4', '5', '6', '7', '8', '9'],
            OrderNumber.Alphabet.Where(char.IsAsciiDigit).Order().ToArray());
        Assert.Equal(24, OrderNumber.Alphabet.Count(char.IsAsciiLetterUpper));
    }

    [Fact]
    public void Create_GivesTheYear_AHyphen_AndABlockOfTheProductLength()
    {
        string number = OrderNumber.Create(2026);

        Assert.Equal(4 + 1 + ProductConstants.OrderNumberRandomBlockLength, number.Length);
        Assert.StartsWith("2026-", number, StringComparison.Ordinal);
        Assert.Matches(NumberShape(), number);
        Assert.DoesNotContain(number[5..], c => "01IOl".Contains(c, StringComparison.Ordinal));
    }

    [Fact]
    public void Create_UsesOnlyTheAlphabet_AndEveryCharacterOfItShowsUp()
    {
        HashSet<char> seen = [];
        for (int i = 0; i < Samples; i++)
        {
            string block = OrderNumber.Create(2026)[5..];
            foreach (char c in block)
            {
                Assert.Contains(c, OrderNumber.Alphabet);
                seen.Add(c);
            }
        }

        // 18 000 characters over 32 symbols: a symbol that never shows up has a probability below 1e-250, so a missing one is a bug.
        Assert.Equal(OrderNumber.Alphabet.Order(), seen.Order());
    }

    [Fact]
    public void Create_TwoCalls_GiveTwoNumbers()
    {
        HashSet<string> numbers = [];
        for (int i = 0; i < Samples; i++)
        {
            numbers.Add(OrderNumber.Create(2026));
        }

        // 32^6 values: a clash in 3000 draws has a probability near 0.004; the check allows one so the test is not flaky.
        Assert.True(numbers.Count >= Samples - 1, $"{Samples - numbers.Count} clashes in {Samples} numbers");
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(2026)]
    [InlineData(9999)]
    public void Create_WritesTheYearAsFourDigits(int year)
    {
        Assert.StartsWith(year + "-", OrderNumber.Create(year), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(999)]
    [InlineData(10000)]
    [InlineData(-2026)]
    public void Create_RefusesAYearThatIsNotFourDigits(int year)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => OrderNumber.Create(year));
    }

    [Fact]
    public void Create_WithAnotherBlockLength_FollowsTheLength()
    {
        Assert.Equal(4 + 1 + 9, OrderNumber.Create(2026, 9).Length);
        Assert.Throws<ArgumentOutOfRangeException>(() => OrderNumber.Create(2026, 0));
    }

    [Fact]
    public void RenewAccessKey_MakesANewKey_AndTheOldOneIsGone()
    {
        Order order = NewOrder();
        string first = order.OrderAccessKey!;

        string renewed = order.RenewAccessKey();

        Assert.Equal(renewed, order.OrderAccessKey);
        Assert.NotEqual(first, renewed);

        // Made by the one key function: the same length and shape as any key (256 bits; 05 6.4.4 asks for at least 128).
        Assert.Equal(SecretKey.Create().Length, renewed.Length);
        Assert.Matches("^[A-Za-z0-9_-]+$", renewed);
    }

    [Fact]
    public void RenewAccessKey_OnADisposedOrder_IsRefused()
    {
        Order order = NewOrder();
        order.ErasePersonalData();

        Assert.Throws<InvalidOperationException>(order.RenewAccessKey);
        Assert.Null(order.OrderAccessKey);
    }

    private static Order NewOrder()
    {
        DateTimeOffset now = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);
        return Order.Create("2026-ABCDEF", SecretKey.Create(), SecretKey.Create(), PaymentMethod.Card, true, "ali@example.com", 100m, 0m, now.AddHours(1), now);
    }

    // Built from the alphabet and the product constant, so the test follows a changed block length (P-19) and not a second copy of the number.
    private static Regex NumberShape() => new($"^[0-9]{{4}}-[{OrderNumber.Alphabet}]{{{ProductConstants.OrderNumberRandomBlockLength}}}$", RegexOptions.None, TimeSpan.FromSeconds(1));
}
