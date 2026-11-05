using Shopfolio.Infrastructure;

namespace Shopfolio.UnitTests.Concurrency;

/// <summary>
/// The lock order of 05 7.3.5 as <see cref="LockSequence"/> holds it (T228 criteria 7, 8; KK7): the cart's advisory lock first and alone at
/// the head, then the order rows by id, the variant rows by id and the coupon last; the category tree's and the administrator set's locks are
/// taken alone. A reversed request is refused before any statement is sent; the integration tests show the same through the real lock takers.
/// </summary>
public sealed class LockSequenceTests
{
    private static readonly Guid Low = new("10000000-0000-0000-0000-000000000000");
    private static readonly Guid High = new("f0000000-0000-0000-0000-000000000000");

    public static TheoryData<string, string> AllowedPairs => new()
    {
        { "Cart", "Order" },
        { "Cart", "Variant" },
        { "Cart", "Coupon" },
        { "Order", "Variant" },
        { "Order", "Coupon" },
        { "Variant", "Coupon" },
    };

    public static TheoryData<string, string> RefusedPairs => new()
    {
        { "Order", "Cart" },
        { "Variant", "Cart" },
        { "Coupon", "Cart" },
        { "Variant", "Order" },
        { "Coupon", "Order" },
        { "Coupon", "Variant" },
        { "CategoryTree", "Order" },
        { "CategoryTree", "Variant" },
        { "CategoryTree", "Coupon" },
        { "AdminSet", "Order" },
        { "AdminSet", "Variant" },
        { "AdminSet", "Coupon" },
        { "Order", "CategoryTree" },
        { "Variant", "AdminSet" },
        { "Cart", "CategoryTree" },
        { "CategoryTree", "AdminSet" },
    };

    [Theory]
    [MemberData(nameof(AllowedPairs))]
    public void Check_LaterKindAfterEarlierKind_IsAllowed(string firstName, string secondName)
    {
        (LockKind first, LockKind second) = (Enum.Parse<LockKind>(firstName), Enum.Parse<LockKind>(secondName));

        Assert.Null(Record.Exception(() => LockSequence.Check([(first, KeyOf(first, High))], second, KeyOf(second, Low))));
    }

    [Theory]
    [MemberData(nameof(RefusedPairs))]
    public void Check_KindOutOfOrderOrMixedWithASeparateFamily_Throws(string firstName, string secondName)
    {
        (LockKind first, LockKind second) = (Enum.Parse<LockKind>(firstName), Enum.Parse<LockKind>(secondName));

        Assert.Throws<InvalidOperationException>(() => LockSequence.Check([(first, KeyOf(first, Low))], second, KeyOf(second, High)));
    }

    [Theory]
    [InlineData("Order")]
    [InlineData("Variant")]
    [InlineData("Coupon")]
    public void Check_SameKindByAscendingId_IsAllowed_AndDescendingIdThrows(string kindName)
    {
        LockKind kind = Enum.Parse<LockKind>(kindName);

        Assert.Null(Record.Exception(() => LockSequence.Check([(kind, Low)], kind, High)));
        Assert.Null(Record.Exception(() => LockSequence.Check([(kind, Low)], kind, Low)));

        Assert.Throws<InvalidOperationException>(() => LockSequence.Check([(kind, High)], kind, Low));
    }

    [Fact]
    public void Check_TheConfirmationsOrder_CartThenEarlierOrderThenVariantsThenCoupon_IsAllowed()
    {
        List<(LockKind Kind, Guid? Key)> taken = [];
        (LockKind, Guid?)[] steps =
        [
            (LockKind.Cart, Low), (LockKind.Order, High), (LockKind.Variant, Low), (LockKind.Variant, High), (LockKind.Coupon, Low),
        ];
        foreach ((LockKind kind, Guid? key) in steps)
        {
            Assert.Null(Record.Exception(() => LockSequence.Check(taken, kind, key)));
            taken.Add((kind, key));
        }

        Assert.Equal(steps.Length, taken.Count);
    }

    [Fact]
    public void Check_CartAfterAnyOtherLockOrASecondCart_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => LockSequence.Check([(LockKind.Cart, Low)], LockKind.Cart, High));
        Assert.Throws<InvalidOperationException>(() => LockSequence.Check([(LockKind.Cart, Low), (LockKind.Order, Low)], LockKind.Cart, Low));
    }

    [Theory]
    [InlineData("CategoryTree")]
    [InlineData("AdminSet")]
    [InlineData("Cart")]
    public void Check_SameAdvisoryLockAgainRightAfterItself_IsAllowed(string kindName)
    {
        LockKind kind = Enum.Parse<LockKind>(kindName);
        Guid? key = KeyOf(kind, Low);

        Assert.Null(Record.Exception(() => LockSequence.Check([(kind, key)], kind, key)));
    }

    [Fact]
    public void Check_FirstLockOfATransaction_IsAlwaysAllowed()
    {
        foreach (LockKind kind in Enum.GetValues<LockKind>())
        {
            Assert.Null(Record.Exception(() => LockSequence.Check([], kind, KeyOf(kind, High))));
        }
    }

    [Fact]
    public void CompareKeys_IsTheOrderOfTheLowerCaseText()
    {
        Guid[] keys = [High, Low, new("0fffffff-ffff-ffff-ffff-ffffffffffff"), new("a0000000-0000-0000-0000-000000000001")];

        Guid[] sorted = [.. keys.Order(Comparer<Guid>.Create(LockSequence.CompareKeys))];

        Assert.Equal(keys.OrderBy(k => k.ToString("D"), StringComparer.Ordinal), sorted);
    }

    private static Guid? KeyOf(LockKind kind, Guid key)
    {
        return kind is LockKind.CategoryTree or LockKind.AdminSet ? null : key;
    }
}
