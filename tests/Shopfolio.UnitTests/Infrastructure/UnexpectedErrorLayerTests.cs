using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

using Shopfolio.Infrastructure.Errors;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The pure parts of the single catching layer (T212; 09 8.1 item 9): the classification table row by row, the client's abort that is no error, the
/// uncertain outcome's target and the counter's window. The answers of the states are the integration tests'.
/// </summary>
public sealed class UnexpectedErrorLayerTests
{
    [Theory]
    [InlineData("GET", false, false, "PageLevel")]
    [InlineData("HEAD", false, false, "PageLevel")]
    [InlineData("GET", false, true, "PageLevel")]
    [InlineData("POST", false, false, "OperationLevel")]
    [InlineData("PUT", false, false, "OperationLevel")]
    [InlineData("DELETE", false, false, "OperationLevel")]
    [InlineData("POST", false, true, "OutcomeUncertain")]
    [InlineData("POST", true, false, "MachineEndpoint")]
    [InlineData("POST", true, true, "MachineEndpoint")]
    [InlineData("GET", true, false, "MachineEndpoint")]
    public void Classify_FollowsTheTableOfTheGuideline(string method, bool machine, bool marked, string expected)
    {
        Assert.Equal(expected, ErrorClassifier.Classify(method, machine, marked).ToString());
    }

    [Theory]
    [InlineData("PageLevel", "UnexpectedPageError")]
    [InlineData("OperationLevel", "UnexpectedOperationError")]
    [InlineData("MachineEndpoint", "UnexpectedOperationError")]
    [InlineData("OutcomeUncertain", "OutcomeUncertain")]
    public void TheCodeOfEachState_IsTheOneTheTableRecords(string state, string expected)
    {
        Assert.Equal(expected, UnexpectedErrorCodes.For(Enum.Parse<ErrorState>(state)));
    }

    [Fact]
    public void Resolve_WhenTheClientWentAway_IsNoError_NoCounterEntryAndNoAnswer()
    {
        UnexpectedErrorCounter counter = new(TimeProvider.System);
        UnexpectedErrorLayer layer = new(NullLogger<UnexpectedErrorLayer>.Instance, counter);
        using CancellationTokenSource aborted = new();
        DefaultHttpContext context = new() { RequestAborted = aborted.Token };
        aborted.Cancel();

        ErrorResolution resolution = layer.Resolve(context, new OperationCanceledException(aborted.Token));

        Assert.True(resolution.ClientGone);
        Assert.Equal(0, counter.CountWithin(TimeSpan.FromMinutes(15)));
    }

    [Fact]
    public void Resolve_ACancellationWhileTheClientStays_IsAnUnexpectedError()
    {
        UnexpectedErrorCounter counter = new(TimeProvider.System);
        UnexpectedErrorLayer layer = new(NullLogger<UnexpectedErrorLayer>.Instance, counter);
        DefaultHttpContext context = new();
        context.Request.Method = "GET";

        ErrorResolution resolution = layer.Resolve(context, new OperationCanceledException("a timeout of ours"));

        Assert.False(resolution.ClientGone);
        Assert.Equal(ErrorState.PageLevel, resolution.State);
        Assert.Equal(1, counter.CountWithin(TimeSpan.FromMinutes(15)));
    }

    [Fact]
    public void Resolve_OfAMarkedOperation_ChoosesTheTargetOfThePageModel_ElseTheRequestsOwnPath_ElseTheRoot()
    {
        UnexpectedErrorLayer layer = new(NullLogger<UnexpectedErrorLayer>.Instance, new UnexpectedErrorCounter(TimeProvider.System));

        Assert.Equal("/hesap/siparisler", Resolve(layer, new OperationMark("UC-27", new Target("/hesap/siparisler"))).Target);
        Assert.Equal("/own", Resolve(layer, new OperationMark("UC-27", null)).Target);
        Assert.Equal("/own", Resolve(layer, new OperationMark("UC-27", new Target("//evil.example"))).Target);
        Assert.Equal("/own", Resolve(layer, new OperationMark("UC-27", new Target("https://evil.example/"))).Target);
        Assert.Equal("/own", Resolve(layer, new OperationMark("UC-27", new Target("/\\evil.example"))).Target);
    }

    [Fact]
    public void Counter_CountsOnlyTheEntriesInsideTheWindow_AndForgetsTheOldOnes()
    {
        MovingTime time = new();
        UnexpectedErrorCounter counter = new(time);

        counter.Record();
        time.Advance(TimeSpan.FromMinutes(20));
        counter.Record();
        counter.Record();

        Assert.Equal(2, counter.CountWithin(TimeSpan.FromMinutes(15)));
        Assert.Equal(3, counter.CountWithin(TimeSpan.FromMinutes(30)));

        time.Advance(TimeSpan.FromDays(2));
        counter.Record();

        Assert.Equal(1, counter.CountWithin(TimeSpan.FromDays(3)));
    }

    private static ErrorResolution Resolve(UnexpectedErrorLayer layer, OperationMark mark)
    {
        DefaultHttpContext context = new();
        context.Request.Method = "POST";
        context.Request.Path = "/own";
        context.Items[OperationMark.ItemKey] = mark;
        return layer.Resolve(context, new InvalidOperationException("t212"));
    }

    private sealed class Target : IOutcomeTarget
    {
        private readonly string _screen;

        public Target(string screen)
        {
            _screen = screen;
        }

        public string OutcomeScreen() => _screen;
    }

    private sealed class MovingTime : TimeProvider
    {
        private DateTimeOffset _now = new(2026, 11, 6, 12, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }
}
