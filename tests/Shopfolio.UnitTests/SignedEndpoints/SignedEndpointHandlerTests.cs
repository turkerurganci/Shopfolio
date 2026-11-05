using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.SignedEndpoints;

namespace Shopfolio.UnitTests.SignedEndpoints;

/// <summary>
/// The order and the answers of the signed endpoint pipeline (T203 criteria 1, 3, 5, 6, 7, 9; KK1, KK3, KK5, KK6, KK7, KK9; 08 3.2; 07 4.30.1,
/// 2.8.7). Every size comes from <see cref="SignedEndpointLimits.BodyMaxBytes"/>. The database side of the same guarantees is in the
/// integration tests (<c>SignedEndpointDatabaseTests</c>).
/// </summary>
public sealed class SignedEndpointHandlerTests : IDisposable
{
    private readonly SignedEndpointHarness _harness = new();

    public void Dispose() => _harness.Dispose();

    [Fact]
    public async Task ValidSignature_IsVerifiedFirst_ThenRecordedInATransaction_AndReceivedOnlyAfterTheCommit()
    {
        (int status, string body, string? contentType) = await _harness.SendAsync(SignedEndpointHarness.Request(TestSignedEndpoint.Body()));

        Assert.Equal(StatusCodes.Status200OK, status);
        Assert.Equal(_harness.Endpoint.Acknowledgement, body);
        Assert.StartsWith("text/plain", contentType, StringComparison.Ordinal);
        Assert.Equal(["Verify", "Begin", "RecordAsync", "Commit"], _harness.Endpoint.Calls);
        Assert.Empty(_harness.Lines);
    }

    [Fact]
    public async Task SameNoticeAgain_IsReceivedWithTheSameAnswer()
    {
        (int firstStatus, string firstBody, _) = await _harness.SendAsync(SignedEndpointHarness.Request(TestSignedEndpoint.Body()));
        _harness.Endpoint.Outcome = new InboundRecordOutcome.AlreadyRecorded();

        (int status, string body, _) = await _harness.SendAsync(SignedEndpointHarness.Request(TestSignedEndpoint.Body()));

        Assert.Equal(firstStatus, status);
        Assert.Equal(firstBody, body);
    }

    [Theory]
    [InlineData("wrong")]
    [InlineData("")]
    public async Task BrokenOrEmptySignature_Gets400WithAnEmptyBody_AndNothingIsRecorded(string signature)
    {
        (int status, string body, _) = await _harness.SendAsync(SignedEndpointHarness.Request(TestSignedEndpoint.Body(signature: signature)));

        Assert.Equal(StatusCodes.Status400BadRequest, status);
        Assert.Empty(body);
        Assert.Equal(["Verify"], _harness.Endpoint.Calls);
    }

    [Fact]
    public async Task MissingSignature_Gets400WithAnEmptyBody_AndNothingIsRecorded()
    {
        string unsigned = $"{TestSignedEndpoint.ReferenceField}=order-1&{TestSignedEndpoint.OutcomeField}=success";

        (int status, string body, _) = await _harness.SendAsync(SignedEndpointHarness.Request(unsigned));

        Assert.Equal(StatusCodes.Status400BadRequest, status);
        Assert.Empty(body);
        Assert.Equal(["Verify"], _harness.Endpoint.Calls);
    }

    [Theory]
    [InlineData("application/json")]
    [InlineData("text/plain")]
    public async Task BodyThatIsNoForm_Gets400_WithoutTheSignatureCheck(string contentType)
    {
        (int status, string body, _) = await _harness.SendAsync(SignedEndpointHarness.Request(TestSignedEndpoint.Body(), contentType));

        Assert.Equal(StatusCodes.Status400BadRequest, status);
        Assert.Empty(body);
        Assert.Empty(_harness.Endpoint.Calls);
        Assert.Equal(nameof(SignatureFailureReason.BodyUnreadable), Line(0).GetProperty("reason").GetString());
    }

    [Fact]
    public async Task FieldGivenTwice_Gets400_WithoutTheSignatureCheck()
    {
        string twice = TestSignedEndpoint.Body(extra: $"&{TestSignedEndpoint.OutcomeField}=failed");

        (int status, _, _) = await _harness.SendAsync(SignedEndpointHarness.Request(twice));

        Assert.Equal(StatusCodes.Status400BadRequest, status);
        Assert.Empty(_harness.Endpoint.Calls);
    }

    [Fact]
    public async Task RejectedRequest_WritesOneThinnedSecurityLine_WithTheReasonAndNoBodyOrSignature()
    {
        string signature = TestSignedEndpoint.Sign("order-1", "failed");

        await _harness.SendAsync(SignedEndpointHarness.Request(TestSignedEndpoint.Body(signature: signature)));

        string line = Assert.Single(_harness.Lines);
        Assert.Equal(LogEvents.SignatureFailureSampled, Line(0).GetProperty("event").GetString());
        Assert.Equal(nameof(SignatureFailureReason.SignatureInvalid), Line(0).GetProperty("reason").GetString());
        Assert.DoesNotContain("order-1", line, StringComparison.Ordinal);
        Assert.DoesNotContain(signature, line, StringComparison.Ordinal);
        Assert.DoesNotContain(Uri.EscapeDataString(signature), line, StringComparison.Ordinal);
        Assert.DoesNotContain("192.0.2.10", line, StringComparison.Ordinal);
    }

    // 09 8.1 items 3, 6, 7: the pipeline does not catch a database error - it goes to the single catching layer (T212), which answers 500 and
    // writes the line. The response is left untouched: no status, no body, no "received", no line of the pipeline's own.
    [Fact]
    public async Task DatabaseErrorOnTheWrite_IsNotCaught_AndNothingIsWrittenToTheResponse()
    {
        _harness.Endpoint.RecordFailure = new TestDatabaseException();
        DefaultHttpContext context = SignedEndpointHarness.Request(TestSignedEndpoint.Body());

        await Assert.ThrowsAsync<TestDatabaseException>(() => _harness.Handler.HandleAsync(context, _harness.Endpoint, _harness.Transactions));

        AssertResponseUntouched(context);
        Assert.Equal(["Verify", "Begin", "RecordAsync"], _harness.Endpoint.Calls);
    }

    [Fact]
    public async Task DatabaseErrorOnTheCommit_IsNotCaught_AndNothingIsWrittenToTheResponse()
    {
        _harness.Transactions.FailCommit = true;
        DefaultHttpContext context = SignedEndpointHarness.Request(TestSignedEndpoint.Body());

        await Assert.ThrowsAsync<TestDatabaseException>(() => _harness.Handler.HandleAsync(context, _harness.Endpoint, _harness.Transactions));

        AssertResponseUntouched(context);
        Assert.Equal(["Verify", "Begin", "RecordAsync", "CommitFailed"], _harness.Endpoint.Calls);
    }

    private void AssertResponseUntouched(DefaultHttpContext context)
    {
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.False(context.Response.HasStarted);
        Assert.Equal(0, context.Response.Body.Length);
        Assert.Null(context.Response.ContentType);
        Assert.Empty(_harness.Lines);
    }

    [Fact]
    public async Task DeclaredLengthAboveTheLimit_Gets413WithoutReadingTheBody_OrCheckingTheSignature()
    {
        DefaultHttpContext context = SignedEndpointHarness.Request(string.Empty);
        context.Request.Body = new UnreadableStream();
        context.Request.ContentLength = SignedEndpointLimits.BodyMaxBytes + 1;

        (int status, string body, _) = await _harness.SendAsync(context);

        Assert.Equal(StatusCodes.Status413PayloadTooLarge, status);
        Assert.Empty(body);
        Assert.Empty(_harness.Endpoint.Calls);
        Assert.Empty(_harness.Lines);
    }

    [Fact]
    public async Task UndeclaredBodyAboveTheLimit_Gets413_BeforeTheSignatureCheck()
    {
        string signed = TestSignedEndpoint.Body();
        string over = signed + "&pad=" + new string('a', SignedEndpointLimits.BodyMaxBytes + 1 - signed.Length - "&pad=".Length);
        Assert.Equal(SignedEndpointLimits.BodyMaxBytes + 1, Encoding.UTF8.GetByteCount(over));

        (int status, string body, _) = await _harness.SendAsync(SignedEndpointHarness.Request(over, declareLength: false));

        Assert.Equal(StatusCodes.Status413PayloadTooLarge, status);
        Assert.Empty(body);
        Assert.Empty(_harness.Endpoint.Calls);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task BodyExactlyAtTheLimit_IsReadAndReceived(bool declareLength)
    {
        string signed = TestSignedEndpoint.Body();
        string atLimit = signed + "&pad=" + new string('a', SignedEndpointLimits.BodyMaxBytes - signed.Length - "&pad=".Length);
        Assert.Equal(SignedEndpointLimits.BodyMaxBytes, Encoding.UTF8.GetByteCount(atLimit));

        (int status, string body, _) = await _harness.SendAsync(SignedEndpointHarness.Request(atLimit, declareLength: declareLength));

        Assert.Equal(StatusCodes.Status200OK, status);
        Assert.Equal(_harness.Endpoint.Acknowledgement, body);
    }

    [Fact]
    public void MappedEndpoint_IsPostOnly_AndOutsideTheForgeryProtection()
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        using WebApplication app = builder.Build();

        IEndpointConventionBuilder mapped = app.MapSignedEndpoint<TestSignedEndpoint, TestNotice>("/test/signed");
        Assert.NotNull(mapped);

        RouteEndpoint endpoint = Assert.Single(((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints).OfType<RouteEndpoint>());
        Assert.Equal("/test/signed", endpoint.RoutePattern.RawText);
        Assert.Equal([HttpMethods.Post], endpoint.Metadata.GetRequiredMetadata<HttpMethodMetadata>().HttpMethods);
        Assert.False(endpoint.Metadata.GetRequiredMetadata<IAntiforgeryMetadata>().RequiresValidation);
    }

    private JsonElement Line(int index) => JsonDocument.Parse(_harness.Lines[index]).RootElement;
}
