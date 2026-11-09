using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

using Shopfolio.Infrastructure.Disposal;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The one place a cookie is written, read and deleted (09 15.1 item 6; 05 6.7). Every write takes its <see cref="CookieOptions"/> from
/// <see cref="CookieDefaults.Create"/> — so Secure, HttpOnly, SameSite=Lax, the path <c>/</c> and no Domain, the conditions of the <c>__Host-</c>
/// prefix, hold for each cookie — and goes through the framework's chunking cookie manager, which splits a value that is too long for one cookie and
/// joins it again on reading. A name outside <see cref="CookieNames"/> is a programming error. The cookies that carry a protected value (the checkout
/// step's, the flow cookie of the sign-in with Google) are written encrypted and signed with the purpose of <see cref="DataProtectionPurposes"/>;
/// a cookie never carries an e-mail address, a name or a password in clear (05 6.7.1).
/// </summary>
internal sealed class BrowserCookies
{
    /// <summary>The guest cart cookie lives as long as the longest life the browsers allow, 400 days, and is renewed at every visit (05 6.7.5, 6.7.8).</summary>
    public static readonly TimeSpan GuestCartLifetime = TimeSpan.FromDays(DisposalTimings.GuestCartIdleDays);

    /// <summary>The flow cookie of the sign-in with Google lasts ten minutes (09 3.5 item 4; 05 6.7.12).</summary>
    public static readonly TimeSpan ExternalLoginFlowLifetime = TimeSpan.FromMinutes(10);

    private static readonly ChunkingCookieManager _manager = new();

    private readonly ProtectedValues _protectedValues;

    public BrowserCookies(ProtectedValues protectedValues)
    {
        ArgumentNullException.ThrowIfNull(protectedValues);
        _protectedValues = protectedValues;
    }

    /// <summary>Writes a cookie of the inventory; <paramref name="lifetime"/> empty makes it last as long as the browser session.</summary>
    public static void Write(HttpContext context, string name, string value, TimeSpan? lifetime = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(value);
        CookieOptions options = CookieDefaults.Create(name, lifetime).Build(context);
        _manager.AppendResponseCookie(context, name, value, options);
    }

    /// <summary>The value of a cookie of the inventory as the request brought it; null when it did not come.</summary>
    public static string? Read(HttpContext context, string name)
    {
        ArgumentNullException.ThrowIfNull(context);
        _ = CookieDefaults.Create(name);
        return _manager.GetRequestCookie(context, name);
    }

    /// <summary>Deletes a cookie of the inventory with the attributes it was written with, its chunks included; the prefix needs them on a deletion as well.</summary>
    public static void Delete(HttpContext context, string name)
    {
        ArgumentNullException.ThrowIfNull(context);
        CookieOptions options = CookieDefaults.Create(name).Build(context);
        _manager.DeleteCookie(context, name, options);
    }

    /// <summary>Writes the guest cart's key again with the full life; the cart module calls it at every visit that brings the cookie (05 6.7.5).</summary>
    public static void WriteGuestCart(HttpContext context, string rawCartKey) => Write(context, CookieNames.Cart, rawCartKey, GuestCartLifetime);

    /// <summary>
    /// Writes the recognized browser mark after a successful sign-in or a password renewed by a reset link (02 8.2.6; 05 6.7.6): the random value the session store gave
    /// (<see cref="SessionOpening.MarkValue"/>, <c>ResetOutcome.MarkValue</c>) for the lifetime it gave (P-48), required, first party. The value is random and carries no e-mail address,
    /// no password and no account identifier; only the server side knows which account it is the mark of. It is written again at every successful sign-in, which renews its life.
    /// </summary>
    public static void WriteKnownDevice(HttpContext context, string markValue, TimeSpan lifetime)
    {
        ArgumentException.ThrowIfNullOrEmpty(markValue);
        Write(context, CookieNames.KnownDevice, markValue, lifetime);
    }

    /// <summary>The mark the browser brought, as the sign-in and the reset hand it to the store (<c>presentedMark</c>); null when the cookie did not come. The store decides whether it is valid for the account.</summary>
    public static string? ReadKnownDevice(HttpContext context) => Read(context, CookieNames.KnownDevice);

    /// <summary>Writes what the checkout step holds, encrypted and signed, for the browser session (05 6.7.9).</summary>
    public void WriteCheckout(HttpContext context, string payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        Write(context, CookieNames.Checkout, _protectedValues.Protect(DataProtectionPurposes.CheckoutCookie, payload));
    }

    /// <summary>Reads the checkout step's cookie; a missing, damaged or foreign value is invalid and the step starts empty.</summary>
    public ProtectedValueResult ReadCheckout(HttpContext context) =>
        _protectedValues.Unprotect(DataProtectionPurposes.CheckoutCookie, Read(context, CookieNames.Checkout));

    /// <summary>Deletes the checkout step's cookie: when the order is confirmed and when the cart empties (05 6.7.7).</summary>
    public static void DeleteCheckout(HttpContext context) => Delete(context, CookieNames.Checkout);

    /// <summary>Writes the flow cookie of the sign-in with Google, encrypted and signed, for ten minutes (05 6.7.12; 09 15.1 item 7).</summary>
    public void WriteExternalLoginFlow(HttpContext context, string payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        Write(context, CookieNames.ExternalLogin, _protectedValues.Protect(DataProtectionPurposes.ExternalLoginFlow, payload), ExternalLoginFlowLifetime);
    }

    /// <summary>Reads the flow cookie and deletes it in the same response: the values in it are used once (09 15.1 item 7).</summary>
    public ProtectedValueResult ConsumeExternalLoginFlow(HttpContext context)
    {
        ProtectedValueResult result = _protectedValues.Unprotect(DataProtectionPurposes.ExternalLoginFlow, Read(context, CookieNames.ExternalLogin));
        Delete(context, CookieNames.ExternalLogin);
        return result;
    }
}
