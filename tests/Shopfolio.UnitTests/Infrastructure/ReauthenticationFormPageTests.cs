using System.Reflection;

using Shopfolio.Infrastructure.Security;
using Shopfolio.Pages.Account;
using Shopfolio.Pages.Panel;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The pages left outside the reauthentication permission's one dropping layer are a closed list (05 6.1.5; K-1627 (4); D-1454): the two account pages that show the forms the
/// permission is bound to and the two screens that grant it. A new page that carries the mark would keep a permission the session should lose, so the product assembly's set
/// is compared with the list both ways.
/// </summary>
public sealed class ReauthenticationFormPageTests
{
    [Fact]
    public void ThePagesLeftToTheirOwnRule_AreTheTwoFormPagesAndTheTwoScreens_AndNoOther_D1454()
    {
        Type[] expected = [typeof(ProfileModel), typeof(ReauthenticationModel), typeof(MyAccountModel), typeof(ReauthModel)];

        Type[] marked = typeof(ReauthenticationPermissionFilter).Assembly.GetTypes()
            .Where(t => t.IsDefined(typeof(ReauthenticationFormPageAttribute), inherit: false))
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expected.OrderBy(t => t.FullName, StringComparer.Ordinal), marked);
        AttributeUsageAttribute? usage = typeof(ReauthenticationFormPageAttribute).GetCustomAttribute<AttributeUsageAttribute>();
        Assert.NotNull(usage);
        Assert.False(usage.Inherited, "a page deriving from a marked page is not left out by inheritance");
    }
}
