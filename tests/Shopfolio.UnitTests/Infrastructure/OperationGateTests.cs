using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Operations;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The pure half of the available-operations pattern (T217 criteria 3, 4, 5, 6, 8, KK3, KK4, KK6, KK8; 07 8.1; 09 8.3): the availability value, the gate's verdict
/// and the codes a refusal carries. A rule is a function of a plain value, so the table of its verdicts is a table of values.
/// </summary>
public sealed class OperationGateTests
{
    [Fact]
    public void TheDefaultAvailability_IsTheClosedOne_AndCarriesNoValue()
    {
        OperationAvailability closed = default;

        Assert.False(closed.IsOpen);
        Assert.Equal(OperationAvailability.Closed, closed);
        Assert.Equal(0, closed.OpenQuantity);
        Assert.Equal(0, closed.DefaultQuantity);
        Assert.Equal(0, closed.MaxQuantity);
        Assert.Null(closed.LastDay);
        Assert.False(closed.HasQuantity);
    }

    [Fact]
    public void AnOpenOperationWithoutAQuantity_CarriesTheLastDayOnly()
    {
        DateOnly last = new(2026, 11, 19);

        OperationAvailability open = OperationAvailability.Open(last);

        Assert.True(open.IsOpen);
        Assert.False(open.HasQuantity);
        Assert.Equal(last, open.LastDay);
        Assert.Null(OperationAvailability.Open().LastDay);
    }

    [Fact]
    public void AnOpenOperationWithAQuantity_CarriesTheOpenQuantityTheDefaultTheUpperLimitAndTheLastDay_KK6()
    {
        DateOnly last = new(2026, 11, 19);

        OperationAvailability open = OperationAvailability.OpenForQuantity(5, 1, 4, last);

        Assert.True(open.IsOpen);
        Assert.True(open.HasQuantity);
        Assert.Equal(5, open.OpenQuantity);
        Assert.Equal(1, open.DefaultQuantity);
        Assert.Equal(4, open.MaxQuantity);
        Assert.Equal(last, open.LastDay);
    }

    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(3, 0, 1)]
    [InlineData(3, 1, 0)]
    [InlineData(3, 1, 4)]
    [InlineData(3, 3, 2)]
    [InlineData(-1, 1, 1)]
    public void AnOpenQuantityOperation_WhoseValuesDoNotNest_CannotBeBuilt(int open, int defaultQuantity, int max)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => OperationAvailability.OpenForQuantity(open, defaultQuantity, max));
    }

    [Fact]
    public void TheGate_AClosedOperation_IsClosedWhateverIsRequested_AndCarriesNoValue_KK3_KK4()
    {
        FixedRule rule = new(OperationAvailability.Closed);

        foreach (int? requested in new int?[] { null, 1, 5, 1000 })
        {
            OperationVerdict verdict = OperationGate.Decide(rule, "state", requested);

            Assert.Equal(OperationDecision.Closed, verdict.Decision);
            Assert.False(verdict.IsAllowed);
            Assert.False(verdict.Availability.IsOpen);
        }
    }

    [Theory]
    [InlineData(1, OperationDecision.Allowed)]
    [InlineData(2, OperationDecision.Allowed)]
    [InlineData(3, OperationDecision.Allowed)]
    [InlineData(4, OperationDecision.QuantityExceedsOpen)]
    [InlineData(1000000, OperationDecision.QuantityExceedsOpen)]
    [InlineData(int.MaxValue, OperationDecision.QuantityExceedsOpen)]
    public void TheGate_AQuantityUpToTheUpperLimitIsAllowed_AboveItIsRefusedWhole_KK3(int requested, OperationDecision expected)
    {
        FixedRule rule = new(OperationAvailability.OpenForQuantity(3, 3, 3));

        OperationVerdict verdict = OperationGate.Decide(rule, "state", requested);

        Assert.Equal(expected, verdict.Decision);
        Assert.Equal(expected == OperationDecision.Allowed, verdict.IsAllowed);
        Assert.Equal(3, verdict.Availability.MaxQuantity);
    }

    [Fact]
    public void TheGate_TheUpperLimitIsTheRulesNotTheOpenQuantity_AClaimOpenForFewerThanTheOpenQuantityRefusesTheRest()
    {
        FixedRule rule = new(OperationAvailability.OpenForQuantity(5, 1, 2));

        Assert.Equal(OperationDecision.Allowed, OperationGate.Decide(rule, "s", 2).Decision);
        Assert.Equal(OperationDecision.QuantityExceedsOpen, OperationGate.Decide(rule, "s", 3).Decision);
    }

    [Fact]
    public void TheGate_AnOperationWithoutAQuantity_IsAllowedWithoutOne()
    {
        FixedRule rule = new(OperationAvailability.Open());

        Assert.Equal(OperationDecision.Allowed, OperationGate.Decide(rule, "s", null).Decision);
    }

    [Fact]
    public void TheGate_ARequestBelowOne_OrWithoutTheQuantityOfAQuantityOperation_IsAProgrammingError_TheFieldRuleCameFirst()
    {
        FixedRule rule = new(OperationAvailability.OpenForQuantity(3, 3, 3));

        Assert.Throws<ArgumentOutOfRangeException>(() => OperationGate.Decide(rule, "s", 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => OperationGate.Decide(rule, "s", -1));
        Assert.Throws<ArgumentException>(() => OperationGate.Decide(rule, "s", null));
    }

    [Fact]
    public void TheGate_AClosedOperationIsClosedBeforeTheQuantityIsRead_ABadQuantityDoesNotMaskIt()
    {
        FixedRule rule = new(OperationAvailability.Closed);

        Assert.Equal(OperationDecision.Closed, OperationGate.Decide(rule, "s", 0).Decision);
    }

    [Fact]
    public void TheGate_AsksTheRuleOnceWithTheContextItWasGiven_KK8()
    {
        FixedRule rule = new(OperationAvailability.Open());

        OperationGate.Decide(rule, "the state", null);

        Assert.Equal(["the state"], rule.Contexts);
    }

    [Fact]
    public void EveryRefusedDecision_HasAnErrorCodeAndANoticeCode_AndTheAllowedOneHasNone()
    {
        foreach (OperationDecision decision in Enum.GetValues<OperationDecision>().Where(d => d != OperationDecision.Allowed))
        {
            ErrorCode code = OperationRefusals.CodeOf(decision);
            NoticeCode notice = OperationRefusals.NoticeOf(decision);

            Assert.Contains(code, new[] { ErrorCode.OperationNotAvailable, ErrorCode.QuantityExceedsOpen });
            Assert.Contains(notice, new[] { NoticeCode.OperationClosed, NoticeCode.QuantityExceedsOpen });
        }

        Assert.Throws<ArgumentOutOfRangeException>(() => OperationRefusals.CodeOf(OperationDecision.Allowed));
        Assert.Throws<ArgumentOutOfRangeException>(() => OperationRefusals.NoticeOf(OperationDecision.Allowed));
    }

    [Fact]
    public void TheCodesOfARefusal_AreTheOnesOf07_Closed422_Quantity422_AndTheNoticesOf63()
    {
        Assert.Equal(ErrorCode.OperationNotAvailable, OperationRefusals.CodeOf(OperationDecision.Closed));
        Assert.Equal(ErrorCode.QuantityExceedsOpen, OperationRefusals.CodeOf(OperationDecision.QuantityExceedsOpen));
        Assert.Equal(NoticeCode.OperationClosed, OperationRefusals.NoticeOf(OperationDecision.Closed));
        Assert.Equal(NoticeCode.QuantityExceedsOpen, OperationRefusals.NoticeOf(OperationDecision.QuantityExceedsOpen));
        Assert.Equal(422, ErrorStatuses.Of(ErrorCode.OperationNotAvailable));
        Assert.Equal(422, ErrorStatuses.Of(ErrorCode.QuantityExceedsOpen));
    }

    private sealed class FixedRule : IOperationRule<string>
    {
        private readonly OperationAvailability _availability;

        public FixedRule(OperationAvailability availability)
        {
            _availability = availability;
        }

        public List<string> Contexts { get; } = [];

        public OperationAvailability Evaluate(string context)
        {
            Contexts.Add(context);
            return _availability;
        }
    }
}
