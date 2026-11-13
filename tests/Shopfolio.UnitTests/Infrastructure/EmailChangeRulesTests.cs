using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The life of a pending e-mail change that the customer's and the administrator's accounts share (T523 KK2, KK9; 02 3.13.5, 3.13.14; D-617): it ends
/// at the first link's end and a request again does not lengthen it.
/// </summary>
public sealed class EmailChangeRulesTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void LinkEnd_IsTheLinkLifeAfterTheRequest()
    {
        Assert.Equal(CreatedAt.AddHours(ProductConstants.EmailVerificationLinkHours), EmailChangeRules.LinkEnd(CreatedAt));
    }

    [Fact]
    public void IsEnded_JustBeforeTheLinkEnd_IsFalse()
    {
        DateTimeOffset now = EmailChangeRules.LinkEnd(CreatedAt).AddTicks(-1);

        Assert.False(EmailChangeRules.IsEnded(CreatedAt, now));
    }

    [Fact]
    public void IsEnded_AtTheLinkEnd_IsTrue()
    {
        Assert.True(EmailChangeRules.IsEnded(CreatedAt, EmailChangeRules.LinkEnd(CreatedAt)));
    }

    [Fact]
    public void IsEnded_RightAfterTheRequest_IsFalse()
    {
        Assert.False(EmailChangeRules.IsEnded(CreatedAt, CreatedAt));
    }
}
