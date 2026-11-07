using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.UnitTests.Notifications;

/// <summary>
/// The guard of the notification map (T328 criteria 2, 4, 6; KK2, KK6; 03 7.1; 02 9.3.1, 9.5): a dispatch row is queued only for a type that has a trigger row in the map and
/// only through the queue path of its family, and the families are the four the document names. The queues call <see cref="NotificationMap.Require"/> before any row is written.
/// </summary>
public sealed class NotificationMapTests
{
    [Fact]
    public void EveryDispatchType_HasOneQueuePath_AndIsAcceptedOnItsOwnPathOnly()
    {
        foreach (DispatchType type in Enum.GetValues<DispatchType>())
        {
            NoticeFamily own = NotificationMap.FamilyOf(type);
            NotificationMap.Require(type, own);
            foreach (NoticeFamily other in Enum.GetValues<NoticeFamily>().Where(f => f != own))
            {
                InvalidOperationException refused = Assert.Throws<InvalidOperationException>(() => NotificationMap.Require(type, other));
                Assert.Contains(type.ToString(), refused.Message, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public void TheFamilies_AreTheCustomersNoticesTheFirmsNoticesTheAdministratorsNoticesAndTheAccountEmails()
    {
        string[] Of(NoticeFamily family) => [.. Enum.GetValues<DispatchType>().Where(t => NotificationMap.FamilyOf(t) == family).Select(t => t.ToString())];

        Assert.Equal(Enumerable.Range(1, 16).Select(n => $"B{n}"), Of(NoticeFamily.Customer));
        Assert.Equal(["F1", "F2", "F3", "F4"], Of(NoticeFamily.Firm));
        Assert.Equal(["F5", "F6"], Of(NoticeFamily.Administrators));
        Assert.Equal(
            [
                nameof(DispatchType.EmailVerificationLink), nameof(DispatchType.PasswordResetLink), nameof(DispatchType.NewEmailVerificationLink), nameof(DispatchType.EmailChangeNotice),
                nameof(DispatchType.AdminInvitation), nameof(DispatchType.AccountDeletedNotice), nameof(DispatchType.GoogleLinkConfirmationLink),
            ],
            Of(NoticeFamily.Account));
    }

    [Fact]
    public void ATypeThatIsNotInTheMap_HasNoQueuePath_AndIsRefusedOnEveryPath()
    {
        DispatchType unknown = (DispatchType)int.MaxValue;

        Assert.Throws<ArgumentOutOfRangeException>(() => NotificationMap.FamilyOf(unknown));
        foreach (NoticeFamily family in Enum.GetValues<NoticeFamily>())
        {
            Assert.Throws<InvalidOperationException>(() => NotificationMap.Require(unknown, family));
        }
    }

    [Fact]
    public void B15_GoesToTheAddressBeforeTheCorrection_AndB1AfterACorrectionToTheNewOne()
    {
        Assert.Equal([NoticeAddressee.OrderContactBeforeCorrection], NotificationMap.AddresseesOf(DispatchType.B15));
        Assert.Equal([NoticeAddressee.OrderContact, NoticeAddressee.CorrectedOrderContact], NotificationMap.AddresseesOf(DispatchType.B1));
    }

    [Fact]
    public void TheFirmNotices_GoToTheFirmsContactEmail_AndF5AndF6ToEveryAdministratorsOwnAddress()
    {
        foreach (DispatchType type in new[] { DispatchType.F1, DispatchType.F2, DispatchType.F3, DispatchType.F4 })
        {
            Assert.Equal([NoticeAddressee.FirmContact], NotificationMap.AddresseesOf(type));
        }

        Assert.Equal([NoticeAddressee.EachAdministrator], NotificationMap.AddresseesOf(DispatchType.F5));
        Assert.Equal([NoticeAddressee.EachAdministrator], NotificationMap.AddresseesOf(DispatchType.F6));
    }

    [Fact]
    public void TheAccountEmails_GoToTheAddressTheirTriggerNames()
    {
        Assert.Equal([NoticeAddressee.RegisteredAddress], NotificationMap.AddresseesOf(DispatchType.EmailVerificationLink));
        Assert.Equal([NoticeAddressee.AccountAddressOrOldOnRevert], NotificationMap.AddresseesOf(DispatchType.PasswordResetLink));
        Assert.Equal([NoticeAddressee.NewAddress], NotificationMap.AddresseesOf(DispatchType.NewEmailVerificationLink));
        Assert.Equal([NoticeAddressee.OldAccountAddress], NotificationMap.AddresseesOf(DispatchType.EmailChangeNotice));
        Assert.Equal([NoticeAddressee.InviteeAddress], NotificationMap.AddresseesOf(DispatchType.AdminInvitation));
        Assert.Equal([NoticeAddressee.DeletedAccountAddress], NotificationMap.AddresseesOf(DispatchType.AccountDeletedNotice));
        Assert.Equal([NoticeAddressee.AccountAddress], NotificationMap.AddresseesOf(DispatchType.GoogleLinkConfirmationLink));
    }

    [Fact]
    public void TheResendRow_ResendsEveryCustomerNotice_ToTheOrdersCurrentContactEmail()
    {
        NoticeTrigger resend = Assert.Single(NotificationMap.Triggers, t => t.Type is null);

        Assert.Equal(NoticeAddressee.CurrentOrderContact, resend.Addressee);
    }
}
