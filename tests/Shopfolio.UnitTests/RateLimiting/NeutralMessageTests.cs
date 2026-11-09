using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.UnitTests.RateLimiting;

/// <summary>
/// The message a user sees when a limit is exceeded is neutral (T376 criterion 5, KK5; 02 8.2.1, 6.1.5, K-329): it names no number - not the attempts left, not the
/// length of the block - and no axis (the IP, the e-mail, the cart, the browser). The same one message serves every limit, so the text cannot tell which one it was.
/// </summary>
public sealed partial class NeutralMessageTests
{
    private static readonly string[] AxisAndTimeWords =
        ["ip", "adres", "e-posta", "eposta", "mail", "sepet", "tarayıcı", "hesap", "saniye", "dakika", "saat", "gün", "kalan", "hak", "deneme hakkı", "kilit", "engel", "kadar"];

    [Fact]
    public void TheMessage_HasNoDigit_AndNoNumberWord_KK5()
    {
        string message = NeutralMessages.LimitExceeded;

        Assert.DoesNotContain(message, char.IsDigit);
        Assert.False(NumberWord().IsMatch(message), "The message names a number in words.");
    }

    [Fact]
    public void TheMessage_NamesNeitherTheAxisNorTheLengthOfTheBlockNorWhatIsLeft_KK5()
    {
        string message = NeutralMessages.LimitExceeded.ToLowerInvariant();

        Assert.All(AxisAndTimeWords, word => Assert.DoesNotContain(word, message, StringComparison.Ordinal));
    }

    [Fact]
    public void TheMessage_IsTheOneEveryLimitsRefusalShows_KK5()
    {
        // A refusal's code is a code of the error list; the text a user reads is not built from the limit, so no limit's refusal can carry its own wording.
        Assert.False(string.IsNullOrWhiteSpace(NeutralMessages.LimitExceeded));
        Assert.Equal(429, ErrorStatuses.Of(ErrorCode.SignInLimitExceeded));
        Assert.All(
            new[] { ErrorCode.SignInLimitExceeded, ErrorCode.PasswordResetLimitExceeded, ErrorCode.RegistrationLimitExceeded, ErrorCode.VerificationLinkLimitExceeded },
            code => Assert.Equal(429, ErrorStatuses.Of(code)));
    }

    [GeneratedRegex(@"\b(bir|iki|üç|dört|beş|altı|yedi|sekiz|dokuz|on|yirmi|otuz)\s+(kez|defa|dakika|saat|deneme|saniye)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex NumberWord();
}
