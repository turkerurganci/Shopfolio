using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.UnitTests.Jobs;

/// <summary>
/// What the card refund's handler does with the row it is given (T220 criterion 7, KK7; 05 4.3.3, 4.3.4; 09 13.1 item 5 table): no record of the
/// row means the request has not gone; "sending" or "outcome pending" means a request went or may have gone, so the handler asks and does not send
/// again; a result already written completes the row. The table runs over every state of the enum, so a new state fails here until it is placed.
/// </summary>
public sealed class CardRefundJobRuleTests
{
    private static readonly (RefundRecordState State, CardRefundStep Step)[] _table =
    [
        (RefundRecordState.Sending, CardRefundStep.AskOutcome),
        (RefundRecordState.OutcomePending, CardRefundStep.AskOutcome),
        (RefundRecordState.Succeeded, CardRefundStep.Complete),
        (RefundRecordState.Failed, CardRefundStep.Complete),
    ];

    [Fact]
    public void Decide_NoRecordOfTheRow_SendsTheRequest()
    {
        Assert.Equal(CardRefundStep.SendRequest, CardRefundJobRule.Decide(null));
    }

    [Fact]
    public void Decide_FollowsTheStateOfTheRowsLatestRecord()
    {
        foreach ((RefundRecordState state, CardRefundStep step) in _table)
        {
            Assert.Equal(step, CardRefundJobRule.Decide(state));
        }
    }

    [Fact]
    public void Decide_TheTableNamesEveryStateOfTheRefundRecord_AndNoRecordedStateSendsARequest()
    {
        Assert.Equal(Enum.GetValues<RefundRecordState>().Order(), _table.Select(row => row.State).Order());
        foreach (RefundRecordState state in Enum.GetValues<RefundRecordState>())
        {
            Assert.NotEqual(CardRefundStep.SendRequest, CardRefundJobRule.Decide(state));
        }
    }

    [Fact]
    public void Decide_AnUnknownState_IsRefused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CardRefundJobRule.Decide((RefundRecordState)999));
    }
}
