using Shopfolio.Infrastructure.Integrations;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The class each result family gives a call without a classified answer (T202 criteria 5, 6 and 8, KK5, KK6, KK8; 08 3.1 rules 3 and 4, the
/// paragraph under the table, 2.1.6, 2.1.8, 2.1.9, 2.2.2, 2.2.3, 2.3; 09 11.1 item 3): one row per family and failure kind.
/// </summary>
public sealed class ResultFamilyTests
{
    private const string Reason = "reason-code";

    public static TheoryData<ExternalCallFailureKind> Kinds => [.. Enum.GetValues<ExternalCallFailureKind>()];

    private static ExternalCallFailure Failure(ExternalCallFailureKind kind) => new(kind, Reason);

    [Theory]
    [MemberData(nameof(Kinds))]
    public void PaymentStart_WithoutAClassifiedAnswer_IsAConnectionError(ExternalCallFailureKind kind)
    {
        Assert.Equal(new PaymentStartResult.ConnectionFailed(Reason), PaymentStartResult.FromFailure(Failure(kind)));
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public void PaymentQuery_WithoutAClassifiedAnswer_IsNoResponse_TheResultIsUnknown(ExternalCallFailureKind kind)
    {
        Assert.Equal(new PaymentQueryResult.NoResponse(Reason), PaymentQueryResult.FromFailure(Failure(kind)));
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public void RefundRequest_WithoutAClassifiedAnswer_IsUnknown_NotAFailure(ExternalCallFailureKind kind)
    {
        Assert.Equal(new RefundRequestResult.Unknown(Reason), RefundRequestResult.FromFailure(Failure(kind)));
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public void RefundQuery_WithoutAClassifiedAnswer_IsNoResult(ExternalCallFailureKind kind)
    {
        Assert.Equal(new RefundQueryResult.NoResult(), RefundQueryResult.FromFailure(Failure(kind)));
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public void EmailSend_WithoutAClassifiedAnswer_IsAChannelError_NeverARecipientError(ExternalCallFailureKind kind)
    {
        EmailSendResult result = EmailSendResult.FromFailure(Failure(kind));

        Assert.Equal(new EmailSendResult.ChannelError(Reason), result);
        Assert.IsNotType<EmailSendResult.RecipientError>(result);
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public void BounceQuery_WithoutAClassifiedAnswer_IsAQueryError(ExternalCallFailureKind kind)
    {
        Assert.Equal(new BounceQueryResult.QueryFailed(Reason), BounceQueryResult.FromFailure(Failure(kind)));
    }

    [Theory]
    [InlineData(ExternalCallFailureKind.TimedOut)]
    [InlineData(ExternalCallFailureKind.Unreachable)]
    public void Identity_TimedOutOrUnreachable_IsUnavailable(ExternalCallFailureKind kind)
    {
        Assert.Equal(new GoogleIdentityResult.Unavailable(Reason), GoogleIdentityResult.FromFailure(Failure(kind)));
    }

    [Fact]
    public void Identity_AnAnswerNotInTheTable_IsAFailure()
    {
        Assert.Equal(new GoogleIdentityResult.Failed(Reason), GoogleIdentityResult.FromFailure(Failure(ExternalCallFailureKind.Unrecognized)));
    }

    [Fact]
    public void Families_KeepTheConnectionErrorApartFromTheSecondTokenRefusal_AndTheRecipientErrorApartFromTheChannelError()
    {
        // The layer tells the classes apart; what each means for the order or the dispatch row is the core's rule (08 3.1 rule 4).
        Assert.NotEqual<PaymentStartResult>(new PaymentStartResult.ConnectionFailed(Reason), new PaymentStartResult.SecondTokenRejected(Reason));
        Assert.NotEqual<EmailSendResult>(new EmailSendResult.RecipientError(Reason), new EmailSendResult.ChannelError(Reason));
        Assert.NotEqual<RefundRequestResult>(new RefundRequestResult.NotCompleted(Reason), new RefundRequestResult.Unknown(Reason));
    }

    [Fact]
    public void TimedOut_CarriesTheTimeoutReason()
    {
        Assert.Equal(new ExternalCallFailure(ExternalCallFailureKind.TimedOut, ExternalCallFailure.TimeoutReason), ExternalCallFailure.TimedOut());
    }
}
