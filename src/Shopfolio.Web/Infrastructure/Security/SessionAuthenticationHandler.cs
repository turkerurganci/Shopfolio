using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using Shopfolio.Infrastructure.Notices;

namespace Shopfolio.Infrastructure.Security;

/// <summary>The settings of one session scheme (09 15.1 item 2): the cookie it reads, the sign-in page it sends a session-less request to and the area it belongs to.</summary>
internal sealed class SessionAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>The name of the session cookie (a constant of <see cref="CookieNames"/>).</summary>
    public string CookieName { get; set; } = string.Empty;

    /// <summary>True for the panel's scheme: it knows administrator accounts only. False: customer accounts only.</summary>
    public bool IsAdmin { get; set; }

    /// <summary>The sign-in page of the area (07 2.3.2).</summary>
    public string SignInPath { get; set; } = string.Empty;

    /// <summary>The area (the return address of a sign-out request).</summary>
    public ReturnArea Area { get; set; }
}

/// <summary>
/// Marks a page that signs the account out (UC-77 <c>/cikis</c>, UC-109): a request without a session reaches it, writes nothing and gets the redirect of
/// the sign-out, not the redirect to the sign-in page with a <c>returnUrl</c> (07 2.3.2; 09 15.1 item 2). The target is the panel's sign-in page for the
/// panel and <see cref="ReturnPath.AfterSignOut"/> for the storefront (07 4.34.1). The attribute is endpoint metadata that the challenge reads.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
internal sealed class SignOutPageAttribute : Attribute
{
}

/// <summary>
/// The session as an authentication handler, not the framework's cookie authentication (09 15.1 item 2; 05 6.1.1, 6.1.6). Every request: no cookie, no
/// result; the cookie's digest is looked up in <c>Session.TokenHash</c>; a missing row, a row of the other account type or a row past its time (P-15 for
/// a short session, P-16 for a long one, counted from the last activity) is no result and the request is session-less. The identity carries the account's
/// internal key and the session row's id and nothing else. The last activity is written at most once a minute with a conditional update, and a long
/// session's cookie is written again with the same lifetime (05 6.1.1, 6.7.2). A request that needs a session and has none is sent to the area's sign-in page:
/// a GET by 302 with the address in <c>returnUrl</c>, a POST by 303 with the GET address of the form (07 2.3.2); 401 is never written.
/// </summary>
internal sealed class SessionAuthenticationHandler : AuthenticationHandler<SessionAuthenticationOptions>
{
    /// <summary>The longest cookie value that is looked up; a real key is 43 characters (09 15.1 item 1).</summary>
    private const int CookieValueLimit = 256;

    /// <summary>The interval the last activity is written at most once in (05 6.1.1).</summary>
    private static readonly TimeSpan _activityInterval = TimeSpan.FromMinutes(1);

    private static readonly TimeSpan _longLifetime = TimeSpan.FromDays(ProductConstants.LongSessionDays);

    private static readonly TimeSpan _shortLifetime = TimeSpan.FromHours(ProductConstants.ShortSessionHours);

    private readonly TimeProvider _time;

    public SessionAuthenticationHandler(IOptionsMonitor<SessionAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, TimeProvider time)
        : base(options, logger, encoder)
    {
        _time = time;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? key = BrowserCookies.Read(Context, Options.CookieName);
        if (string.IsNullOrEmpty(key) || key.Length > CookieValueLimit)
        {
            return AuthenticateResult.NoResult();
        }

        AppDbContext db = Context.RequestServices.GetRequiredService<AppDbContext>();
        string digest = SecretKey.Hash(key);
        var row = await db.Sessions.AsNoTracking()
            .Where(s => s.TokenHash == digest)
            .Select(s => new { s.Id, s.CustomerId, s.AdminId, s.IsLongLived, s.LastActivityAt })
            .SingleOrDefaultAsync(Context.RequestAborted);
        if (row is null)
        {
            return AuthenticateResult.NoResult();
        }

        // The row's account link must match the scheme: a customer scheme never knows an administrator's session and the other way round (05 6.1.3).
        Guid? accountId = Options.IsAdmin ? (row.CustomerId is null ? row.AdminId : null) : (row.AdminId is null ? row.CustomerId : null);
        if (accountId is null)
        {
            return AuthenticateResult.NoResult();
        }

        DateTimeOffset now = _time.GetUtcNow();
        if (now - row.LastActivityAt > (row.IsLongLived ? _longLifetime : _shortLifetime))
        {
            return AuthenticateResult.NoResult();
        }

        if (now - row.LastActivityAt >= _activityInterval)
        {
            DateTimeOffset threshold = now - _activityInterval;
            int written = await db.Sessions
                .Where(s => s.Id == row.Id && s.LastActivityAt < threshold)
                .ExecuteUpdateAsync(set => set.SetProperty(s => s.LastActivityAt, now), Context.RequestAborted);
            if (written > 0 && row.IsLongLived)
            {
                BrowserCookies.Write(Context, Options.CookieName, key, _longLifetime);
            }
        }

        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, accountId.Value.ToString("D")), new Claim(SessionSchemes.SessionIdClaim, row.Id.ToString("D"))],
            Scheme.Name);
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name));
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        if (Context.GetEndpoint()?.Metadata.GetMetadata<SignOutPageAttribute>() is not null)
        {
            // The sign-out of a request without a session writes nothing and gets the sign-out's own redirect (07 2.3.2).
            string target = Options.Area == ReturnArea.Panel
                ? SessionSchemes.AdminSignInPath
                : ReturnPath.AfterSignOut(await RequestedReturnUrlAsync());
            Redirect(StatusCodes.Status303SeeOther, target);
            return;
        }

        bool isRead = HttpMethods.IsGet(Request.Method) || HttpMethods.IsHead(Request.Method);
        string address = isRead ? EncodedPathAndQuery(Request.Query, keepHandler: true) : EncodedPathAndQuery(Request.Query, keepHandler: false);
        string location = ReturnPath.IsLocalPath(address)
            ? $"{Options.SignInPath}?{ReturnPath.ParameterName}={Uri.EscapeDataString(address)}"
            : Options.SignInPath;
        if (!isRead)
        {
            // A POST of a session that ended says so on the sign-in page: the changes were not saved (07 2.3.2, 6.3.2 SessionExpired; D-121). The GET does not.
            Redirect(StatusCodes.Status303SeeOther, NoticeParameter.Address(location, NoticeCode.SessionExpired));
            return;
        }

        Redirect(StatusCodes.Status302Found, location);
    }

    private async Task<string?> RequestedReturnUrlAsync()
    {
        if (Request.Query.TryGetValue(ReturnPath.ParameterName, out StringValues fromQuery) && fromQuery.Count > 0)
        {
            return fromQuery[0];
        }

        if (Request.HasFormContentType)
        {
            IFormCollection form = await Request.ReadFormAsync(Context.RequestAborted);
            if (form.TryGetValue(ReturnPath.ParameterName, out StringValues fromForm) && fromForm.Count > 0)
            {
                return fromForm[0];
            }
        }

        return null;
    }

    /// <summary>
    /// The request's own address. A POST's is the address of the form's GET (07 2.3.2): the <c>handler</c> parameter belongs to the POST and is dropped,
    /// the other parameters stay.
    /// </summary>
    private string EncodedPathAndQuery(IQueryCollection query, bool keepHandler)
    {
        QueryString queryString = Request.QueryString;
        if (!keepHandler)
        {
            queryString = QueryString.Create(query.Where(q => !q.Key.Equals("handler", StringComparison.OrdinalIgnoreCase)));
        }

        return Request.PathBase.Add(Request.Path).ToUriComponent() + queryString.ToUriComponent();
    }

    private void Redirect(int status, string location)
    {
        Response.StatusCode = status;
        Response.Headers.Location = location;
    }
}
