using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.IdentityModel.Tokens;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>What the answer of the code exchange comes to: the identity token, or the failure of the call (08 2.3 error table).</summary>
internal sealed record GoogleExchangeOutcome(string? IdToken, ExternalCallFailure? Failure);

/// <summary>
/// The pure mappings of the Google call (08 2.3; 09 11.1 item 3, 11.3): the answer of the code exchange to the identity token or the failure, the payload of a validated identity
/// token to <see cref="GoogleIdentityResult.Succeeded"/>, and the validation's failure to a short reason code. None of them calls anything. The result carries exactly the
/// fields of 08 2.3's field mapping and the nonce the core compares - the subject identifier (<c>sub</c>) is not read (K-1061; 09 11.3 item 4) -; a reason code is a short
/// code and never a message, an address, the token or a field of the answer.
/// </summary>
internal static partial class GoogleIdentityMapping
{
    public const string NotConfiguredReason = "not-configured";

    public const string NoIdTokenReason = "exchange:no-id-token";

    public const string MalformedAnswerReason = "exchange:malformed-answer";

    public const string EmailMissingReason = "email-missing";

    public const string InvalidTokenReason = "token:invalid";

    public const string ErrorReasonPrefix = "google-error:";

    public const string StatusReasonPrefix = "http:";

    public const string ConfigurationReason = "configuration:unavailable";

    /// <summary>
    /// The answer of the exchange. A success status with an identity token is the token; a success status without one, or with a body that is not the answer's shape, is an answer
    /// the table does not list (a failure); any other status is Google's error answer (unavailable), whose reason is the OAuth error code when it has the shape of one
    /// (<c>invalid_grant</c>) and the status number otherwise (08 2.3: "Google hata yanıtı verdi").
    /// </summary>
    public static GoogleExchangeOutcome ReadExchangeAnswer(HttpStatusCode status, string? body)
    {
        JsonElement root = default;
        bool isObject = false;
        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                using JsonDocument document = JsonDocument.Parse(body);
                root = document.RootElement.Clone();
                isObject = root.ValueKind == JsonValueKind.Object;
            }
            catch (JsonException)
            {
                isObject = false;
            }
        }

        if ((int)status is >= 200 and < 300)
        {
            if (!isObject)
            {
                return new GoogleExchangeOutcome(null, ExternalCallFailure.Unrecognized(MalformedAnswerReason));
            }

            return StringField(root, "id_token") is { Length: > 0 } token
                ? new GoogleExchangeOutcome(token, null)
                : new GoogleExchangeOutcome(null, ExternalCallFailure.Unrecognized(NoIdTokenReason));
        }

        string? error = isObject ? StringField(root, "error") : null;
        string reason = error is not null && OAuthErrorCode().IsMatch(error) ? ErrorReasonPrefix + error : StatusReasonPrefix + (int)status;
        return new GoogleExchangeOutcome(null, ExternalCallFailure.Unreachable(reason));
    }

    /// <summary>
    /// The result of an identity token whose signature, issuer, audience and lifetime are already verified (08 2.3 field mapping): <c>email</c> (empty or missing makes the
    /// validation fail), <c>email_verified</c> (a boolean, or the text "true"), <c>name</c> (may be empty), <c>hd</c> (read only), <c>auth_time</c> (seconds since 1970), and
    /// <c>nonce</c> (empty when the token has none, so the core's comparison fails). No other field is read.
    /// </summary>
    public static GoogleIdentityResult ReadIdentity(ReadOnlySpan<byte> payload)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(payload.ToArray());
        }
        catch (JsonException)
        {
            return new GoogleIdentityResult.Failed(InvalidTokenReason);
        }

        using (document)
        {
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return new GoogleIdentityResult.Failed(InvalidTokenReason);
            }

            string? email = StringField(root, "email");
            if (string.IsNullOrWhiteSpace(email))
            {
                return new GoogleIdentityResult.Failed(EmailMissingReason);
            }

            return new GoogleIdentityResult.Succeeded(
                Email: email,
                EmailVerified: BooleanField(root, "email_verified"),
                Name: StringField(root, "name"),
                HostedDomain: StringField(root, "hd"),
                AuthTime: SecondsField(root, "auth_time"),
                Nonce: StringField(root, "nonce") ?? string.Empty);
        }
    }

    /// <summary>The reason code of a validation that did not hold: the kind of check that failed, never the library's message (which can quote the token or the key).</summary>
    public static string ValidationReason(Exception? exception)
    {
        if (exception is null)
        {
            return InvalidTokenReason;
        }

        foreach ((Type kind, string reason) in ValidationReasons)
        {
            if (kind.IsInstanceOfType(exception))
            {
                return reason;
            }
        }

        return InvalidTokenReason;
    }

    /// <summary>The specific kinds of failed check, most specific first; any other failure is <see cref="InvalidTokenReason"/>.</summary>
    private static readonly (Type Kind, string Reason)[] ValidationReasons =
    [
        (typeof(SecurityTokenInvalidAlgorithmException), "token:alg"),
        (typeof(SecurityTokenSignatureKeyNotFoundException), "token:key-not-found"),
        (typeof(SecurityTokenInvalidSignatureException), "token:signature"),
        (typeof(SecurityTokenInvalidIssuerException), "token:issuer"),
        (typeof(SecurityTokenInvalidAudienceException), "token:audience"),
        (typeof(SecurityTokenExpiredException), "token:expired"),
        (typeof(SecurityTokenNoExpirationException), "token:no-expiration"),
        (typeof(SecurityTokenNotYetValidException), "token:not-yet-valid"),
        (typeof(SecurityTokenInvalidLifetimeException), "token:lifetime"),
        (typeof(SecurityTokenMalformedException), "token:malformed"),
    ];

    /// <summary>A validation that stopped because the key set could not be got is the outage of the service, not a bad token (08 2.3 error table, second row).</summary>
    public static bool IsConfigurationOutage(Exception? exception)
    {
        return exception is not null and not SecurityTokenException;
    }

    private static string? StringField(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    private static bool BooleanField(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out JsonElement value))
        {
            return false;
        }

        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.String => string.Equals(value.GetString(), "true", StringComparison.Ordinal),
            _ => false,
        };
    }

    private static DateTimeOffset? SecondsField(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out JsonElement value))
        {
            return null;
        }

        long? seconds = value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetInt64(out long number) => number,
            JsonValueKind.String when long.TryParse(value.GetString(), NumberStyles.None, CultureInfo.InvariantCulture, out long text) => text,
            _ => null,
        };

        // A value outside the range of a date is no time: the claim is treated as absent.
        return seconds is >= 0 and <= 253402300799 ? DateTimeOffset.FromUnixTimeSeconds(seconds.Value) : null;
    }

    [GeneratedRegex(@"^[a-z_]{1,64}$", RegexOptions.CultureInvariant)]
    private static partial Regex OAuthErrorCode();
}
