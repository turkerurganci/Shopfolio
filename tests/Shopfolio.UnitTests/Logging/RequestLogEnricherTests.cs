using System.Security.Claims;
using System.Text.Json;

using Microsoft.AspNetCore.Http;

using Serilog.Events;

using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Values;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Logging;

/// <summary>The request fields of a line (T230; 09 18.2 item 1): they come from the enricher, from the current request, masked; outside a request there are none.</summary>
public sealed class RequestLogEnricherTests
{
    private static readonly string[] RequestFields = ["requestId", "method", "path", "accountType", "accountId"];

    private sealed class FixedAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }

    [Fact]
    public void Enrich_OutsideARequest_AddsNoRequestField()
    {
        RequestLogEnricher enricher = new(new FixedAccessor());

        using JsonDocument json = LogCapture.Json(LogCapture.One(logger => logger.Warning("text"), enricher));

        Assert.All(RequestFields, field => Assert.False(json.RootElement.TryGetProperty(field, out _), field));
    }

    [Fact]
    public void Enrich_InARequest_AddsTheRequestIdTheMethodAndTheMaskedPath()
    {
        DefaultHttpContext context = new();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/siparis/erisim/ABC123/teyit";
        context.Request.QueryString = new QueryString("?email=a@b.com");
        context.Request.Headers[RequestLogEnricher.RequestIdHeader] = "req-42.a_b";
        RequestLogEnricher enricher = new(new FixedAccessor { HttpContext = context });

        string line = LogCapture.Line(LogCapture.One(logger => logger.Warning("text"), enricher));

        using JsonDocument json = JsonDocument.Parse(line);
        Assert.Equal("req-42.a_b", json.RootElement.GetProperty("requestId").GetString());
        Assert.Equal(HttpMethods.Post, json.RootElement.GetProperty("method").GetString());
        Assert.Equal("/siparis/erisim/{key}/teyit?email=*", json.RootElement.GetProperty("path").GetString());
        Assert.DoesNotContain("ABC123", line, StringComparison.Ordinal);
        Assert.DoesNotContain("a@b.com", line, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("has space")]
    [InlineData("a@b.com")]
    [InlineData("")]
    public void Enrich_ARequestIdThatIsNotAnIdentifier_IsNotWritten(string header)
    {
        DefaultHttpContext context = new();
        context.Request.Headers[RequestLogEnricher.RequestIdHeader] = header;
        RequestLogEnricher enricher = new(new FixedAccessor { HttpContext = context });

        using JsonDocument json = LogCapture.Json(LogCapture.One(logger => logger.Warning("text"), enricher));

        Assert.False(json.RootElement.TryGetProperty("requestId", out _));
    }

    [Theory]
    [InlineData(AccountType.Admin)]
    [InlineData(AccountType.Customer)]
    public void Enrich_ASignedInAccount_AddsTheTypeAndTheInternalId(AccountType type)
    {
        Guid accountId = Guid.NewGuid();
        DefaultHttpContext context = new()
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(RequestLogEnricher.AccountTypeClaim, type.ToString()),
                    new Claim(RequestLogEnricher.AccountIdClaim, accountId.ToString()),
                ],
                "test")),
        };
        RequestLogEnricher enricher = new(new FixedAccessor { HttpContext = context });

        using JsonDocument json = LogCapture.Json(LogCapture.One(logger => logger.Warning("text"), enricher));

        Assert.Equal(type.ToString(), json.RootElement.GetProperty("accountType").GetString());
        Assert.Equal(accountId.ToString(), json.RootElement.GetProperty("accountId").GetString());
    }

    [Fact]
    public void Enrich_AnAccountIdThatIsAnEmailAddress_IsNotWritten()
    {
        const string email = "someone@example.invalid";
        DefaultHttpContext context = new()
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(RequestLogEnricher.AccountIdClaim, email), new Claim(RequestLogEnricher.AccountTypeClaim, email)], "test")),
        };
        RequestLogEnricher enricher = new(new FixedAccessor { HttpContext = context });

        string line = LogCapture.Line(LogCapture.One(logger => logger.Warning("text"), enricher));

        Assert.DoesNotContain(email, line, StringComparison.Ordinal);
    }

    [Fact]
    public void Enrich_APropertyTheCallerSetAlready_IsNotReplaced()
    {
        DefaultHttpContext context = new();
        context.Request.Method = HttpMethods.Get;
        RequestLogEnricher enricher = new(new FixedAccessor { HttpContext = context });

        LogEvent logEvent = LogCapture.One(logger => logger.ForContext("method", "FromCaller").Warning("text"), enricher);

        using JsonDocument json = LogCapture.Json(logEvent);
        Assert.Equal("FromCaller", json.RootElement.GetProperty("method").GetString());
    }
}
