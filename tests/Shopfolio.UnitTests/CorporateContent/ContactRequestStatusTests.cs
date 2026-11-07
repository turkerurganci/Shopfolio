using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.UnitTests.CorporateContent;

/// <summary>
/// The status and the retention start of the contact request (T322 criteria 7, 9; KK7, KK9; 02 5.13; 03 1.9.2; Z-33): two values, born open, and the
/// retention counted from the last closing - or from the opening of a request that was never closed.
/// </summary>
public sealed class ContactRequestStatusTests
{
    private static readonly DateTimeOffset Created = new(2026, 11, 1, 9, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset FirstClosing = Created.AddDays(2);

    private static readonly DateTimeOffset SecondClosing = Created.AddDays(10);

    [Fact]
    public void TheStatus_HasTwoValues_AndNoOther_KK7()
    {
        Assert.Equal([ContactRequestStatus.Open, ContactRequestStatus.Closed], Enum.GetValues<ContactRequestStatus>());
    }

    [Fact]
    public void ARequestIsBornOpen_WithNoClosingMoment_KK7()
    {
        ContactRequest request = ContactRequest.Create("Ayşe", "ayse@example.com", ContactTopic.GeneralQuestion, "Merhaba", Created);

        Assert.Equal(ContactRequestStatus.Open, request.ContactRequestStatus);
        Assert.Null(request.LastClosedAt);
        Assert.Null(request.Phone);
    }

    [Fact]
    public void ANeverClosedRequest_CountsItsRetentionFromTheOpening_KK9()
    {
        ContactRequest request = ContactRequest.Create("Ayşe", "ayse@example.com", ContactTopic.GeneralQuestion, "Merhaba", Created);

        Assert.Equal(Created, request.RetentionStart);
    }

    [Fact]
    public void AClosedRequest_CountsItsRetentionFromTheLastClosing_KK9()
    {
        ContactRequest request = Closed(FirstClosing);
        Assert.Equal(FirstClosing, request.RetentionStart);

        request.LastClosedAt = SecondClosing;
        Assert.Equal(SecondClosing, request.RetentionStart);
    }

    [Fact]
    public void AReopenedRequest_KeepsCountingFromTheOldClosing_UntilItIsClosedAgain_KK9()
    {
        ContactRequest request = Closed(FirstClosing);

        request.ContactRequestStatus = ContactRequestStatus.Open;

        Assert.Equal(FirstClosing, request.RetentionStart);
    }

    private static ContactRequest Closed(DateTimeOffset closedAt)
    {
        ContactRequest request = ContactRequest.Create("Ayşe", "ayse@example.com", ContactTopic.GeneralQuestion, "Merhaba", Created);
        request.ContactRequestStatus = ContactRequestStatus.Closed;
        request.LastClosedAt = closedAt;
        return request;
    }
}
