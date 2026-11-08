using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Membership;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The lives of the Z-1, Z-2 and Z-3 links (T324 criterion 1, KK1; 03 4.1.1-4.1.3; P-12, P-13, P-14): a link drawn with its product constant is valid
/// strictly before its end and expired at it, a newer link asked for supersedes it, and the pending address change ends with the life of its first link.
/// The lives are read from <see cref="ProductConstants"/>, never typed here.
/// </summary>
public sealed class LinkLifetimeTests
{
    private static readonly DateTimeOffset Drawn = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

    public static TheoryData<LinkType, int> Lives => new()
    {
        { LinkType.EmailVerification, ProductConstants.EmailVerificationLinkHours * 60 },
        { LinkType.NewEmailVerification, ProductConstants.EmailVerificationLinkHours * 60 },
        { LinkType.PasswordReset, ProductConstants.PasswordResetLinkMinutes },
        { LinkType.AdminInvitation, ProductConstants.AdminInvitationLinkHours * 60 },
    };

    private static LinkRecord Draw(LinkType linkType, int lifeMinutes) =>
        linkType == LinkType.AdminInvitation
            ? LinkRecord.ForInvitation(Guid.CreateVersion7(), "digest", Drawn.AddMinutes(lifeMinutes), Drawn)
            : LinkRecord.ForCustomer(linkType, Guid.CreateVersion7(), "digest", Drawn.AddMinutes(lifeMinutes), Drawn);

    [Theory]
    [MemberData(nameof(Lives))]
    public void Link_IsValidUntilItsLifeEnds_AndExpiredAtTheEnd(LinkType linkType, int lifeMinutes)
    {
        LinkRecord link = Draw(linkType, lifeMinutes);
        DateTimeOffset end = Drawn.AddMinutes(lifeMinutes);

        Assert.Equal(LinkState.Valid, LinkRecords.StateOf(link, Drawn));
        Assert.Equal(LinkState.Valid, LinkRecords.StateOf(link, end.AddTicks(-1)));
        Assert.Equal(LinkState.Expired, LinkRecords.StateOf(link, end));
    }

    [Theory]
    [MemberData(nameof(Lives))]
    public void Link_ForWhichANewerOneWasAsked_IsSuperseded_WithinItsLife(LinkType linkType, int lifeMinutes)
    {
        LinkRecord link = Draw(linkType, lifeMinutes);
        link.SupersededAt = Drawn.AddMinutes(1);

        Assert.Equal(LinkState.Superseded, LinkRecords.StateOf(link, Drawn.AddMinutes(2)));
    }

    [Fact]
    public void PendingEmailChange_EndsWithTheLifeOfItsFirstLink_KK1()
    {
        PendingEmailChange change = PendingEmailChange.ForCustomer(Guid.CreateVersion7(), "new@example.com", Drawn);
        DateTimeOffset end = Drawn.AddHours(ProductConstants.EmailVerificationLinkHours);

        Assert.False(PendingRecordExpiry.EndedEmailChange(end.AddTicks(-1)).Compile()(change));
        Assert.True(PendingRecordExpiry.EndedEmailChange(end).Compile()(change));
    }
}
