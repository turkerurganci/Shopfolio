using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.UnitTests.RateLimiting;

/// <summary>
/// The result of a POST an attempt limit refused (T376 criterion 5, KK5; D-145; 09 8.2 item 3, 15.4 item 5): 429, the neutral message at the level of the action,
/// and exactly one rejection line with the limit's own code - the user is told nothing of the limit, the log keeps which one it was. A code that is not an attempt
/// limit's is refused, so the line cannot carry another refusal's code.
/// </summary>
public sealed class AttemptLimitResultTests
{
    [Theory]
    [InlineData(ErrorCode.SignInLimitExceeded)]
    [InlineData(ErrorCode.PasswordResetLimitExceeded)]
    [InlineData(ErrorCode.RegistrationLimitExceeded)]
    [InlineData(ErrorCode.ContactRequestLimitExceeded)]
    [InlineData(ErrorCode.OrderLookupLimitExceeded)]
    [InlineData(ErrorCode.CouponAttemptLimitExceeded)]
    [InlineData(ErrorCode.UnpaidOrderLimitExceeded)]
    [InlineData(ErrorCode.VerificationLinkLimitExceeded)]
    public void TheResult_Is429_WithTheNeutralMessage_AndOneLineWithTheLimitsCode_KK5(ErrorCode code)
    {
        CapturedLines lines = new();
        PageModel page = PageOf(lines);

        IActionResult result = FormResults.AttemptLimitExceeded(page, code);

        Assert.Equal(StatusCodes.Status429TooManyRequests, Assert.IsType<PageResult>(result).StatusCode);
        Assert.Equal(NeutralMessages.LimitExceeded, Assert.Single(page.ModelState[string.Empty]!.Errors).ErrorMessage);
        string line = Assert.Single(lines.Messages);
        Assert.Contains(code.ToString(), line, StringComparison.Ordinal);
        Assert.Equal(typeof(RejectionLog).FullName, Assert.Single(lines.Categories));
    }

    [Theory]
    [InlineData(ErrorCode.InvalidCredentials)]
    [InlineData(ErrorCode.RateLimitExceeded)]
    [InlineData(ErrorCode.UnexpectedPageError)]
    public void ACodeThatIsNoAttemptLimitsCode_IsRefused_AndNoLineIsWritten_KK5(ErrorCode code)
    {
        CapturedLines lines = new();
        PageModel page = PageOf(lines);

        Assert.Throws<ArgumentOutOfRangeException>(() => FormResults.AttemptLimitExceeded(page, code));
        Assert.Empty(lines.Messages);
        Assert.Empty(page.ModelState);
    }

    private static PageModel PageOf(CapturedLines lines)
    {
        ServiceProvider services = new ServiceCollection()
            .AddLogging(builder => builder.AddProvider(new CapturingProvider(lines)))
            .AddSingleton<RejectionLog>()
            .BuildServiceProvider();
        DefaultHttpContext context = new() { RequestServices = services };
        return new StubPage { PageContext = new PageContext(new Microsoft.AspNetCore.Mvc.ActionContext(context, new Microsoft.AspNetCore.Routing.RouteData(), new PageActionDescriptor())) };
    }

    private sealed class StubPage : PageModel;

    private sealed class CapturedLines
    {
        public List<string> Messages { get; } = [];

        public List<string> Categories { get; } = [];
    }

    private sealed class CapturingProvider(CapturedLines lines) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new CapturingLogger(lines, categoryName);

        public void Dispose()
        {
        }
    }

    private sealed class CapturingLogger(CapturedLines lines, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            lines.Messages.Add(formatter(state, exception));
            lines.Categories.Add(category);
        }
    }
}
